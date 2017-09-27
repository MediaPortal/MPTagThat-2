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

using Prism.Modularity;
using Prism.Regions;
using MPTagThat.SongGrid.Views;

#endregion

namespace MPTagThat.SongGrid
{
  /// <summary>
  /// This class is the Song Grid Module to be inserted into the MainRegion of the Shell
  /// </summary>
  public class SongGridModule : IModule
  {
    private readonly IRegionManager _regionManager;

    public SongGridModule(IRegionManager regionManager)
    {
      _regionManager = regionManager;
    }

    public void Initialize()
    {
      _regionManager.RegisterViewWithRegion("MainRegion", typeof(SongGridView));
    }
  }
}
