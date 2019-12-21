﻿#region Copyright (C) 2020 Team MediaPortal
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

using MPTagThat.Core.AlbumCoverSearch;
using MPTagThat.Core.Utils;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TagLib;

#endregion

namespace MPTagThat.Core.AlbumSearch
{
  /// <summary>
  /// The Album as returned by an AlbumSearch
  /// </summary>
  public class Album
  {
    #region ctor

    public Album()
    {
      CoverHeight = "";
      CoverWidth = "";
      Discs = new List<List<AlbumSong>>();
    }

    #endregion

    #region Properties

    public string MusicBrainzId { get; set; }

    public string Asin { get; set; }

    public string Title { get; set; }

    public string Artist { get; set; }

    public string Binding { get; set; }

    public string Label { get; set; }

    public string Year { get; set; }

    public int DiscCount { get; set; }

    public List<List<AlbumSong>> Discs { get; set; }

    public string SmallImageUrl { get; set; }

    public string MediumImageUrl { get; set; }

    public string LargeImageUrl { get; set; }

    public string CoverWidth { get; set; }

    public string CoverHeight { get; set; }

    public ByteVector AlbumImage
    {
      get
      {
        var vector = new ByteVector();
        var sUrl = LargeImageUrl ?? MediumImageUrl ?? SmallImageUrl;

        if (sUrl == null)
        {
          return null;
        }

        try
        {
          var webReq = WebRequest.Create(sUrl);

          // For Discogs, we need a special User Agent
          if (sUrl.Contains("discogs.com"))
          {
            (webReq as HttpWebRequest).UserAgent = "MPTagThat/3.2 +http://www.team-mediaportal.com";
          }
          WebResponse webResp = webReq.GetResponse();
          Stream stream = webResp.GetResponseStream();

          byte[] data = Util.ReadFullStream(stream, 32768);
          if (data.Length > 0)
            vector.Add(data);
        }
        catch { }
        return vector;
      }
    }
    #endregion
  }
}
