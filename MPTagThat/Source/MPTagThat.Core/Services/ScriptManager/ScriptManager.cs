#region Copyright (C) 2022 Team MediaPortal
// Copyright (C) 2022 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MPTagThat is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPTagThat is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPTagThat. If not, see <http://www.gnu.org/licenses/>.
#endregion

#region

using CSScriptLibrary;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Prism.Ioc;
using System;
using System.Collections;
using System.IO;
using System.Reflection;

// ReSharper disable PossibleNullReferenceException

#endregion

namespace MPTagThat.Core.Services.ScriptManager
{
  public class ScriptManager : IScriptManager
  {
    #region Variables

    private readonly NLogLogger log;
    private Options _options;

    private readonly ArrayList _organiseScripts = new ArrayList();
    private readonly string _sharedAsemblyDir;
    private readonly ArrayList _tagScripts = new ArrayList();
    private Assembly _assembly;

    #endregion

    #region ctor

    public ScriptManager()
    {
      log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;
      _options = ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions;

      _sharedAsemblyDir = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\MPTagThat2\Scripts";
      if (!Directory.Exists(_sharedAsemblyDir))
      {
        try
        {
          Directory.CreateDirectory(_sharedAsemblyDir);
        }
        catch (Exception e)
        {
          log.Error($"Error creating Scripts folder: {e.InnerException}");
          return;
        }
      }
      LoadAvailableScripts();
    }

    #endregion

    #region Properties

    public ArrayList GetScripts() => _tagScripts;

    public ArrayList GetOrganiseScripts() => _organiseScripts;

    #endregion

    #region Initialisation

    private void LoadAvailableScripts()
    {
      var dirInfo = new DirectoryInfo(_sharedAsemblyDir);
      var files = dirInfo.GetFiles();
      foreach (var file in files)
      {
        var ext = Path.GetExtension(file.Name);
        var desc = GetDescription(file.FullName);
        if (desc == null)
          continue; // we had an error reading the file

        if (desc[1] == String.Empty)
        {
          desc[1] = Path.GetFileNameWithoutExtension(file.Name); // Use the Filename as Title            
        }

        switch (ext)
        {
          case ".sct":
            _tagScripts.Add(desc);
            break;
          case ".osc":
            _organiseScripts.Add(desc);
            break;
        }
      }
    }

    /// <summary>
    /// Gets the Description of the Script
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private string[] GetDescription(string fileName)
    {
      var description = new string[3];
      try
      {
        var file = File.OpenText(fileName);
        var line1 = file.ReadLine();
        var line2 = file.ReadLine();

        description[0] = Path.GetFileName(fileName);
        if (line1.StartsWith("// Title:"))
          description[1] = line1.Substring(9).Trim();
        else
          description[1] = String.Empty;

        if (line2.StartsWith("// Description:"))
          description[2] = line2.Substring(15).Trim();
        else
          description[2] = String.Empty;

        return description;
      }
      catch (Exception)
      {
        return null;
      }
    }

    #endregion

    #region Script Compiling and Loading

    public Assembly Load(string script)
    {
      string scriptFile = Path.Combine(_sharedAsemblyDir, script);

      try
      {
        var configDir = _options.ConfigDir.Substring(0, _options.ConfigDir.LastIndexOf("\\", StringComparison.Ordinal));
        var compiledScriptsDir = Path.Combine(configDir, @"Scripts\compiled");
        if (!Directory.Exists(compiledScriptsDir))
        {
          Directory.CreateDirectory(compiledScriptsDir);
        }

        var finalName = $@"{Path.GetFileNameWithoutExtension(scriptFile)}.dll";
        finalName = Path.Combine(compiledScriptsDir, finalName);

        // The script file could have been deleted while already executing the progrsm
        if (!File.Exists(scriptFile) && !File.Exists(finalName))
        {
          log.Info($"Script does not exist {finalName}");
          return null;
        }

        var lastwriteSourceFile = File.GetLastWriteTime(scriptFile);

        // Load the compiled Assembly only, if it is newer than the source file, to get changes done on the file
        if (File.Exists(finalName) && File.GetLastWriteTime(finalName) > lastwriteSourceFile)
        {
          _assembly = Assembly.LoadFile(finalName);
        }
        else
        {
          log.Info($"Compiling script {scriptFile}");
          _assembly = CSScript.LoadFile(scriptFile, finalName, true);

          // And reload the assembly from the compiled dir, so that we may execute it
          Assembly.LoadFile(finalName);
        }
        return _assembly;
      }
      catch (Exception ex)
      {
        log.Error("Error loading script: {0} {1}", scriptFile, ex.Message);
        return null;
      }
    }

    private string GetLoadedAssemblyLocation(string name)
    {
      foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
      {
        var asmName = asm.FullName.Split(",".ToCharArray())[0];
        if (string.Compare(name, asmName, StringComparison.OrdinalIgnoreCase) == 0)
          return asm.Location;
      }
      return "";
    }

    #endregion

  }
}
