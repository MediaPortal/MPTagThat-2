#region Copyright (C) 2020 Team MediaPortal
// Copyright (C) 2020 Team MediaPortal
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

using System;
using System.Management;
using MPTagThat.Core.Services.Logging;
using Prism.Ioc;

#endregion

namespace MPTagThat.Core.Services.MediaChangeMonitor
{
  public class MediaChangeMonitor : IMediaChangeMonitor
  {
    #region Event delegates

    public event MediaInsertedEvent MediaInserted;
    public event MediaRemovedEvent MediaRemoved;

    #endregion

    #region Variables

    private readonly NLogLogger log = ContainerLocator.Current.Resolve<ILogger>()?.GetLogger;
    private ManagementEventWatcher _watcher;

    #endregion

    #region ctor / dtor

    public MediaChangeMonitor()
    {
    }

    ~MediaChangeMonitor()
    {
      _watcher?.Stop();
      _watcher = null;
    }

    #endregion

    #region IMediaChangeMonitor implementation

    /// <summary>
    /// Start listening on Media Changes
    /// </summary>
    public void StartListening()
    {
      try
      {
        var q = new WqlEventQuery
        {
          EventClassName = "__InstanceModificationEvent",
          WithinInterval = new TimeSpan(0, 0, 1),
          Condition = @"TargetInstance ISA 'Win32_LogicalDisk' and TargetInstance.DriveType = 5"
        };

        var opt = new ConnectionOptions
        {
          EnablePrivileges = true, Authority = null, Authentication = AuthenticationLevel.Default
        };
        var scope = new ManagementScope("\\root\\CIMV2", opt);

        _watcher = new ManagementEventWatcher(scope, q);
        _watcher.EventArrived += watcher_EventArrived;
        _watcher.Start();
      }
      catch (ManagementException e)
      {
        Console.WriteLine(e.Message);
      }
    }

    /// <summary>
    /// Stop listening on Media Changes
    /// </summary>
    public void StopListening()
    {
      _watcher?.Stop();
      _watcher = null;
    }

    #endregion

    #region Events

    void watcher_EventArrived(object sender, EventArrivedEventArgs e)
    {
      var wmiDevice = (ManagementBaseObject)e.NewEvent["TargetInstance"];
      var driveName = (string)wmiDevice["DeviceID"];
      if (wmiDevice.Properties["VolumeName"].Value != null)
      {
        log.Info($"MediaChangeMonitor: Media inserted in drive {driveName}");
        MediaInserted?.Invoke(driveName);
      }
      else
      {
        log.Info($"MediaChangeMonitor: Media removed from drive {driveName}");
        MediaRemoved?.Invoke(driveName);
      }
    }

    #endregion
  }
}
