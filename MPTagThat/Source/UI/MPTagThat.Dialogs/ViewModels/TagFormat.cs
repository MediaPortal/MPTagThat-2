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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace MPTagThat.Dialogs.ViewModels
{
  public class TagFormat
  {
    #region Variables

    private readonly List<ParameterPart> _parameterParts = new List<ParameterPart>();

    #endregion

    #region Properties

    public List<ParameterPart> ParameterParts => _parameterParts;

    #endregion

    #region Constructor

    /// <summary>
    ///   Parses the Parameter Format to retrieve Tags from Filenames.
    /// </summary>
    /// <param name = "parameterFormat"></param>
    public TagFormat(string parameterFormat)
    {
      _parameterParts.Clear();

      // Split the given parameters to see, if folders have been specified
      var parms = parameterFormat.Split(new[] {'\\'});
      for (var i = 0; i < parms.Length; i++)
      {
        if (!parms[i].StartsWith("%"))
          parms[i] = $"%x%{parms[i]}";

        if (!parms[i].EndsWith("%"))
          parms[i] = $"{parms[i]}%x%";
      }
      for (var i = parms.Length - 1; i >= 0; i--)
      {
        var part = new ParameterPart(parms[i]);
        _parameterParts.Add(part);
      }
    }

    #endregion
  }
}
