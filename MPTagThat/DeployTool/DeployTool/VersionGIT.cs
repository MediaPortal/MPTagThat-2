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

    private Process RunGitCommand(string arguments)
    {
      string programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? Environment.GetEnvironmentVariable("ProgramFiles");

      FileInfo file = new FileInfo(programFiles + @"\Git\bin\git.exe");
      ProcessStartInfo procInfo = new ProcessStartInfo();
      procInfo.RedirectStandardOutput = true;
      procInfo.UseShellExecute = false;
      procInfo.Arguments = arguments;
      procInfo.FileName = file.FullName;

      Console.WriteLine("Running : {0} {1}", file.FullName, arguments);

      if (file.Exists)
      {
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

      Console.WriteLine("Running : {0} {1}", file.FullName, arguments);

      if (file.Exists)
      {
        return Process.Start(procInfo);
      }

      Console.WriteLine("git.exe not found!");
      return null;
    }

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

    public string GetCurrentBuild(string gitDir)
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

    public string GetBuild()
    {
      return _build;
    }
  }
}
