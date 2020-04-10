#region Copyright (C) 2017 Team MediaPortal
// Copyright (C) 2017 Team MediaPortal
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

using Prism.Mvvm;

#endregion

namespace MPTagThat.Core.Common.Song
{
  public class Frame : BindableBase
  {
    #region Variables

    private string _id;
    private string _description;
    private string _value;

    #endregion

    #region Properties

    public string Id { get => _id; set => SetProperty(ref _id, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public string Value { get => _value; set => SetProperty(ref _value, value); }

    #endregion

    #region ctor
    public Frame() 
    {
    }
    
    public Frame(string id, string description, string value)
    {
      Id = id;
      Description = description;
      Value = value;
    }

    #endregion
  }
}
