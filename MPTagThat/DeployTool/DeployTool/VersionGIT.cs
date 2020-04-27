#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;

namespace DeployTool
{
  public class VersionGit
  {
    private string _build;

    /// <summary>
    /// Execute Git with given parameters
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    private Process RunGitCommand(string arguments)
    {
      string programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? Environment.GetEnvironmentVariable("ProgramFiles");

      FileInfo file = new FileInfo(programFiles + @"\Git\bin\git.exe");
      ProcessStartInfo procInfo = new ProcessStartInfo();
      procInfo.RedirectStandardOutput = true;
      procInfo.UseShellExecute = false;
      procInfo.Arguments = arguments;
      procInfo.FileName = file.FullName;

      if (file.Exists)
      {
        Console.WriteLine("Running : {0} {1}", file.FullName, arguments);
        return Process.Start(procInfo);
      }

      // GIT V2 is installed to x64 folder
      programFiles = Environment.GetEnvironmentVariable("ProgramW6432");

      file = new FileInfo(programFiles + @"\Git\bin\git.exe");
      procInfo = new ProcessStartInfo();
      procInfo.RedirectStandardOutput = true;
      procInfo.UseShellExecute = false;
      procInfo.Arguments = arguments;
      procInfo.FileName = file.FullName;

      if (file.Exists)
      {
        Console.WriteLine("Running : {0} {1}", file.FullName, arguments);
        return Process.Start(procInfo);
      }

      Console.WriteLine("git.exe not found!");
      return null;
    }

    /// <summary>
    /// Returns the Git Directory from the given Path
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    private string GetGitDir(string directory)
    {
      while (!Directory.Exists(directory + @"\.git"))
      {
        var parent = Directory.GetParent(directory);
        if (parent == null)
        {
          Console.WriteLine("Git dir not found");
          return "./.git";
        }
        directory = parent.FullName;
      }
      Console.WriteLine("Using git dir: {0}", directory);
      return directory + @"\.git";
    }

    /// <summary>
    /// Gets the current build
    /// </summary>
    /// <param name="gitDir"></param>
    /// <returns></returns>
    private string GetCurrentBuild(string gitDir)
    {
      using (
        var proc = RunGitCommand($"--git-dir=\"{gitDir}\" rev-list HEAD --count "))
      {
        if (proc != null)
        {
          string gitOut = proc.StandardOutput.ReadToEnd();
          return gitOut.Trim(' ', '\n', '\r', '\t');
        }
      }
      return null;
    }

    /// <summary>
    /// Get the status of the Git Folder
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    public string GetStatus(string directory)
    {
      string gitDir = GetGitDir(directory);

      using (
        var proc = RunGitCommand($"--git-dir=\"{gitDir}\" status"))
      {
        if (proc != null)
        {
          string gitOut = proc.StandardOutput.ReadToEnd();
          return gitOut;
        }
      }
      return null;
    }

    /// <summary>
    /// Revert change to file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public bool RevertChange(string fileName, string directory)
    {
      string gitDir = GetGitDir(directory);

      using (
        var proc = RunGitCommand($"--git-dir=\"{gitDir}\" checkout -- {fileName}"))
      {
        if (proc != null)
        {
          string gitOut = proc.StandardOutput.ReadToEnd();
          return true;
        }
      }
      return false;
    }


    /// <summary>
    /// Read the Build Number
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    public bool ReadBuild(string directory)
    {
      string gitDir = GetGitDir(directory);

      _build = GetCurrentBuild(gitDir);
      if (_build != null)
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Return the current Build number
    /// </summary>
    /// <returns></returns>
    public string GetBuild()
    {
      return _build;
    }
  }
}
