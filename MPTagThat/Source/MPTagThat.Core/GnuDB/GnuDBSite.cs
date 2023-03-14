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

namespace MPTagThat.Core.GnuDB
{
  /// <summary>
  ///   Contains Information about a GnuDB Site
  /// </summary>
  public class GnuDBSite
  {
    #region GnuDBProtocol enum

    public enum GnuDBProtocol
    {
      CDDB,
      HTTP
    };

    #endregion

    public GnuDBSite() { }

    public GnuDBSite(string host, GnuDBProtocol proto, int port, string uri,
                      string latitude, string longitude, string location)
    {
      Host = host;
      Protocol = proto;
      Port = port;
      URI = uri;
      Latitude = latitude;
      Longitude = longitude;
      Location = location;
    }

    public string Host { get; set; }

    public GnuDBProtocol Protocol { get; set; }

    public int Port { get; set; }

    public string URI { get; set; }

    public string Latitude { get; set; }

    public string Longitude { get; set; }

    public string Location { get; set; }
  }
}
