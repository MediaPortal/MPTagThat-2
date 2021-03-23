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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.MusicDatabase;
using Prism.Ioc;
using Prism.Services.Dialogs;
using WPFLocalizeExtension.Engine;

namespace MPTagThat.Dialogs.ViewModels
{
  public class SwitchDatabaseViewModel : DialogViewModelBase
  {

    #region Variables

    #endregion

    #region Properties

    private string _databaseDescription;

    public string DatabaseDescription
    {
      get => _databaseDescription;
      set => SetProperty(ref _databaseDescription, value);
    }

    private ObservableCollection<Database> _databaseTitles = new ObservableCollection<Database>();

    public ObservableCollection<Database> Databases
    {
      get => _databaseTitles;
      set
      {
        _databaseTitles = value;
        RaisePropertyChanged("Databases");
      }
    }

    /// <summary>
    /// The Selected Database
    /// </summary>
    private ObservableCollection<object> _selectedDatabase = new ObservableCollection<object>();

    public ObservableCollection<object> SelectedDatabase
    {
      get => _selectedDatabase;
      set => SetProperty(ref _selectedDatabase, value);
    }

    #endregion



    #region ctor

    public SwitchDatabaseViewModel()
    {
      Title = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", "switchDatabase_Header",
        LocalizeDictionary.Instance.Culture).ToString();
      AddDatabaseCommand = new BaseCommand(AddDatabase);
      DeleteDatabaseCommand = new BaseCommand(DeleteDatabase);
      SwitchDatabaseCommand = new BaseCommand(SwitchDatabase);
    }

    #endregion

    #region Commands

    /// <summary>
    ///Add Database Button has been clicked
    /// </summary>
    public ICommand SwitchDatabaseCommand { get; }

    private void SwitchDatabase(object param)
    {
      log.Trace(">>>");

      if (SelectedDatabase.Count > 0)
      {
        ContainerLocator.Current.Resolve<IMusicDatabase>().SwitchDatabase((SelectedDatabase[0] as Database).Name);

        CloseDialog("true");
      }

      log.Trace("<<<");
    }


    /// <summary>
    ///Add Database Button has been clicked
    /// </summary>
    public ICommand AddDatabaseCommand { get; }

    private void AddDatabase(object param)
    {
      log.Trace(">>>");

      if (!string.IsNullOrEmpty(DatabaseDescription))
      {
        var databaseName = DatabaseDescription.Replace(" ", "_");
        ContainerLocator.Current.Resolve<IMusicDatabase>().SwitchDatabase(databaseName);
        var title = DatabaseDescription;
        Databases.Add(new Database {DatabaseTitle = title, Name = databaseName});  
      }

      log.Trace("<<<");
    }

    /// <summary>
    /// Delete Database Button has been clicked
    /// </summary>
    public ICommand DeleteDatabaseCommand { get; }

    private void DeleteDatabase(object param)
    {
      log.Trace(">>>");

      if (SelectedDatabase.Count > 0)
      {
        ContainerLocator.Current.Resolve<IMusicDatabase>().DeleteDatabase((SelectedDatabase[0] as Database).Name);
        Databases.Remove((Database)SelectedDatabase[0]);
      }

      log.Trace("<<<");
    }

    #endregion


    #region Private Methods

    private void GetDatabases()
    {
      log.Trace(">>>");

      var databases = Directory.GetFiles(_options.StartupSettings.DatabaseFolder, "*.db");
      foreach (var db in databases)
      {
        var dbName = Path.GetFileNameWithoutExtension(db);
        if (dbName.EndsWith("-log"))
        {
          continue;
        }
        var title = dbName.Replace('_', ' ');
        Databases.Add(new Database {DatabaseTitle = title, Name = dbName});  
      }

      log.Trace("<<<");
    }

    #endregion

    #region Overrides

    public override void OnDialogOpened(IDialogParameters parameters)
    {
      GetDatabases();
    }

    #endregion

    #region local classes

    public class Database
    {
      public string Name { get; set; }
      public string DatabaseTitle { get; set; }
    }

    #endregion
  }
}
