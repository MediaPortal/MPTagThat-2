#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DeployTool
{
  internal class Program
  {
    #region Variables

    private static string _directory;
    private static CommandLineOptions _options;
    
    private const string StatusRegEx = @".*modified:\s*(.*)$";

    #endregion

    private static void Main(string[] args)
    {

      string build;
      int buildInt;
      string fullVersion = "";

      ICommandLineOptions argsOptions = new CommandLineOptions();

      try
      {
        CommandLine.Parse(args, ref argsOptions);
      }
      catch (ArgumentException)
      {
        argsOptions.DisplayOptions();
        Environment.Exit(0);
      }

      _options = argsOptions as CommandLineOptions;

      if (!_options.IsOption(CommandLineOptions.Option.path))
      {
        argsOptions.DisplayOptions();
        Environment.Exit(0);
      }
      _directory = _options.GetOption(CommandLineOptions.Option.path);

      // Update the Assembly Copyright Date
      if (_options.IsOption(CommandLineOptions.Option.UpdateCopyright))
      {
        string copyrightText = _options.GetOption(CommandLineOptions.Option.UpdateCopyright);
        Console.WriteLine("Writing Copyright text: " + copyrightText);

        AssemblyUpdate copyrightUpdate = new AssemblyUpdate(copyrightText, AssemblyUpdate.UpdateMode.Copyright);
        copyrightUpdate.UpdateAll(_directory);

        Environment.Exit(0);
      }

      // Update the Assembly Version and Revision
      if (_options.IsOption(CommandLineOptions.Option.UpdateVersion))
      {
        // Set the version from the command line
        string version = _options.GetOption(CommandLineOptions.Option.UpdateVersion);
        if (string.IsNullOrEmpty(version))
        {
          fullVersion = "1.0.0.0";
        }
        else
        {
          fullVersion = version + ".$build";
        }

        // Set the Git Directory
        string gitDir = GetGitDir(); 
        VersionGit git = new VersionGit();

        // Check, if we have some pending commits
        var status = git.GetStatus(gitDir);
        if (!status.Contains("working tree clean"))
        {
          Console.WriteLine("GIT has pending commits / untracked files");
          Environment.Exit(0);
        }

        // Lookup the latest build to get the Full Version
        bool readBuild = git.ReadBuild(gitDir);

        if (File.Exists("version.txt"))
        {
          File.Delete("version.txt");
        }
        if (!readBuild)
        {
          Console.WriteLine("Error getting Build number");
          Environment.Exit(0);
        }

        build = git.GetBuild();

        TextWriter write = new StreamWriter("version.txt");
        fullVersion = fullVersion.Replace("$build", build);
        
        AssemblyUpdate update = new AssemblyUpdate(fullVersion);
        update.UpdateAll(_directory);

        write.Write(fullVersion);
        write.Close();
        Int32.TryParse(build, out buildInt);
        Environment.Exit(buildInt);
      }

      // Revert back changes to AssemblyInfo.cs
      if (_options.IsOption(CommandLineOptions.Option.revert))
      {
        Console.WriteLine("Reverting changes to AssemblyInfo.cs made by Deploytool");
        var regEx = new Regex(StatusRegEx, RegexOptions.Multiline);
        // Set the Git Directory
        string gitDir = GetGitDir();
        VersionGit git = new VersionGit();

        // Check, if we have some pending commits for AssemblyInfo
        var status = git.GetStatus(gitDir);
        var matches = regEx.Matches(status);
        foreach (Match match in matches)
        {
          var file = match.Groups[1].Value;
          if (file.EndsWith("AssemblyInfo.cs"))
          {
            git.RevertChange(file);
            Console.WriteLine($"Reverted changes to {file}");
          }
        }
      }
    }

    /// <summary>
    /// ´Builds the Git Directory
    /// </summary>
    /// <returns></returns>
    private static string GetGitDir()
    {
      // Set the Git Directory
      string gitDir = null; 
      if (_options.IsOption(CommandLineOptions.Option.git))
      {
        gitDir = _options.GetOption(CommandLineOptions.Option.git);
      }
      if (string.IsNullOrEmpty(gitDir))
      {
        gitDir = _directory;
      }

      return gitDir;
    }
  }
}
