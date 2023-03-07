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
using LiteDB;
using Prism.Mvvm;
using System.ComponentModel;

#endregion

namespace MPTagThat.Core.Common.Song
{

  public enum ItemStatus
  {
    Ignored = -1,
    NoMatch = 0,
    FullMatch = 1,
    FullMatchChanged = 2,
    PartialMatch = 3,
    Applied = 4,
  }

  public class TagCheckerData : BindableBase
  {
    /// <summary>
    /// Unique ID of the Item
    /// To be used in the Database
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Current Status of Track, as indicated in Column 0 of grid
    /// Based on the ItemStatus Enum
    /// </summary>
    private ItemStatus _status = ItemStatus.NoMatch;

    public ItemStatus Status
    {
      get => _status;
      set
      {
        SetProperty(ref _status, value);
      }
    }

    /// <summary>
    /// The Item has been Changed
    /// </summary>
    private bool _changed = false;
    public bool Changed
    {
      get => _changed;
      set => SetProperty(ref _changed, value);
    }

    /// <summary>
    /// Indicates if the Changed status should bet set
    /// </summary>
    [BsonIgnore]
    public bool UpdateChangedProperty { get; set; } = false;

    /// <summary>
    /// The Item has been Changed
    /// </summary>
    private bool _foundInMusicBrainz = false;
    public bool FoundInMusicBrainz
    {
      get => _foundInMusicBrainz;
      set => SetProperty(ref _foundInMusicBrainz, value);
    }

    /// <summary>
    /// The original Item as returned from the database
    /// </summary>
    private string _originalItem = "";
    public string OriginalItem
    {
      get => _originalItem;
      set => SetProperty(ref _originalItem, value);
    }

    /// <summary>
    /// The changed item as returned by the Musicbrainz query or entered by the user
    /// </summary>
    private string _changedItem = "";
    public string ChangedItem
    {
      get => _changedItem;
      set => SetProperty(ref _changedItem, value);
    }

    #region overrides

    protected override void OnPropertyChanged(PropertyChangedEventArgs args)
    {
      if (UpdateChangedProperty && args.PropertyName == "ChangedItem")
      {
        Changed = true;
      }
      base.OnPropertyChanged(args);
    }

    #endregion
  }
}
