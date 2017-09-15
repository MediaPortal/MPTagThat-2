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

using System.Linq;
using System.Windows.Controls;
using Prism.Regions;
using Syncfusion.Windows.Tools.Controls;

#endregion

namespace MPTagThat
{
  /// <summary>
  /// SyncFusion DockingManager is not an ItemsControl, we need a region adapter to notify that regions should be mapped into Children property.
  /// This class defines the region adapter. It overrides the methods, Adapt and CreateRegion.
  /// In the Adapt method, add the regions to DockingManager.Children whenever the regions collection is changed.
  /// It will be used in the Bootstrapper for handling the redions 
  /// </summary>
  public class DockingManagerRegionAdapter : RegionAdapterBase<DockingManager>
  {
    public DockingManagerRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
     : base(regionBehaviorFactory)
    {
    }

    protected override void Adapt(IRegion region, DockingManager regionTarget)
    {
      region.Views.CollectionChanged += delegate
      {
        foreach (var child in region.Views.Cast<UserControl>())
        {
          if (!regionTarget.Children.Contains(child))
          {
            regionTarget.BeginInit();
            regionTarget.Children.Add(child);
            regionTarget.EndInit();
          }
        }
      };
    }

    protected override IRegion CreateRegion()
    {
      return new AllActiveRegion();
    }
  }
}
