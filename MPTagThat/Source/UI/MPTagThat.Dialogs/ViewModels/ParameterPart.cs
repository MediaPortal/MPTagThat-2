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
  public class ParameterPart
  {
    private readonly List<string> parms = new List<string>();

    #region Properties

    public string[] Delimiters { get; }

    public List<string> Parameters => parms;

    #endregion

    public ParameterPart(string parm)
    {
      var str = parm;
      str = str.Replace("%artist%", "\x0001");    
      str = str.Replace("%title%", "\x0001"); 
      str = str.Replace("%album%", "\x0001"); 
      str = str.Replace("%genre%", "\x0001"); 
      str = str.Replace("%comment%", "\x0001");
      str = str.Replace("%year%", "\x0001"); 
      str = str.Replace("%x%", "\x0001"); 
      str = str.Replace("%albumartist%", "\x0001"); 
      str = str.Replace("%disc%", "\x0001"); 
      str = str.Replace("%disctotal%", "\x0001"); 
      str = str.Replace("%track%", "\x0001"); 
      str = str.Replace("%tracktotal%", "\x0001"); 
      str = str.Replace("%conductor%", "\x0001"); 
      str = str.Replace("%composer%", "\x0001"); 
      str = str.Replace("%group%", "\x0001"); 
      str = str.Replace("%subtitle%", "\x0001"); 
      str = str.Replace("%remixed%", "\x0001"); 
      str = str.Replace("%bpm%", "\x0001"); 

      Delimiters = str.Split(new[] {'\x0001'});
      str = parm;

      var upperBound = Delimiters.GetUpperBound(0);
      for (var i = 0; i <= upperBound; i++)
      {
        if ((i == upperBound) | (Delimiters[i] != ""))
        {
          var parameterStart = str.IndexOf("%", StringComparison.Ordinal);
          var parameterEnd = str.IndexOf("%", parameterStart + 1, StringComparison.Ordinal);
          parms.Add(str.Substring(parameterStart, parameterEnd - parameterStart + 1));
          str = str.Substring(str.IndexOf("%", parameterStart + 1, StringComparison.Ordinal) + 1);
        }
      }
    }
  }
}
