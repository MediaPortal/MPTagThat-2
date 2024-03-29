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

using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace MPTagThat.Core.GnuDB
{
  /// <summary>
  ///   Summary description for XMCDParser.
  /// </summary>
  public class XMCDParser
  {
    private const string Appname = "MPTagThat";
    private const string Appversion = "1.0";
    private readonly char[] _seps = { '\r', '\n' };

    // store all the data
    private string _artist;
    private CDTrackDetail[] _cdTrackDetail;
    private string _content;
    private string _discid;
    private string _extd;
    private string _genre;
    private int _length; // in seconds
    private int[] _offsets;
    private int[] _playorder;
    private string _title;
    private int[] _trackDurations;
    private int _year;

    public XMCDParser() { }

    public XMCDParser(string xmcdContent)
    {
      parse(xmcdContent);
    }

    public CDInfoDetail parse(string[] content)
    {
      StringBuilder buff = new StringBuilder(1024);
      for (int i = 0; i < content.Length; i++)
      {
        buff.Append(content[i]);
        buff.Append('\n');
      }
      return parse(buff.ToString());
    }

    public CDInfoDetail parse(string content)
    {
      _content = (string)content.Clone();
      if (_content.IndexOf("# xmcd") != 0)
        return null;

      _offsets = parseOffsets();
      _length = parseLength();
      _trackDurations = calculateDurations(_offsets, _length);
      _discid = parseDiscIDs()[0];
      _artist = parseArtist();
      _playorder = parsePlayOrder();
      _title = parseTitle();
      _year = parseYear();
      _genre = parseGenre();
      _extd = parseExtension();
      _cdTrackDetail = new CDTrackDetail[_offsets.Length];

      for (int i = 0; i < _offsets.Length; i++)
      {
        _cdTrackDetail[i] = new CDTrackDetail();
        string trackArtist = parseTrackArtist(i);
        _cdTrackDetail[i].Artist = trackArtist.Equals(_artist) ? null : trackArtist;
        _cdTrackDetail[i].Title = parseTrackTitle(i);
        _cdTrackDetail[i].Offset = _offsets[i];
        _cdTrackDetail[i].DurationInt = _trackDurations[i];
        _cdTrackDetail[i].EXTT = parseTrackExtension(i);
        _cdTrackDetail[i].Track = i + 1;
      }

      return new CDInfoDetail(_discid, _artist, _title, _genre, _year, _length,
                              _cdTrackDetail, _extd, _playorder);
    }

    public CDInfoDetail Parse2(string[] content)
    {
      ArrayList offsets = new ArrayList();
      Hashtable comments = new Hashtable();
      Hashtable fields = new Hashtable();
      char[] commentSep = { ':' };
      char[] fieldSep = { '=' };
      //string[] tokens = _content.Split(_seps);

      for (int i = 0; i < content.Length; i++)
      {
        string curLine = content[i].Trim();
        if (curLine.StartsWith("#") && curLine.Trim().Length > 1)
        {
          //string[] curcomment = curLine.Substring(1).Split(commentSep);
          string[] curcomment = null;
          int index = curLine.IndexOf(":");
          if (index > 0)
          {
            curcomment = new string[2];
            curcomment[0] = curLine.Substring(1, index - 1).Trim();
            if (index < curLine.Length)
              curcomment[1] = curLine.Substring(index + 1).Trim();
            else
              curcomment[1] = "";
          }
          else
          {
            curcomment = new string[1];
            curcomment[0] = curLine.Substring(1).Trim();
          }

          if (curcomment.Length == 2 && curcomment[0].Length > 0 && curcomment[1].Length > 0)
          {
            // for comments that are split over two lines.
            if (comments.ContainsKey(curcomment[0]))
            {
              comments[curcomment[0]] += curcomment[1];
            }
            else
            {
              comments.Add(curcomment[0], curcomment[1]);
            }
          }
          else if (curcomment.Length == 1 && curcomment[0].Length > 0)
          {
            try
            {
              int lastOne = 0;
              int thisOne = Convert.ToInt32(curcomment[0]);
              if (offsets.Count > 0)
                lastOne = Convert.ToInt32(offsets[offsets.Count - 1]);
              if (thisOne > lastOne) // just to avoid adding unexpected commented numbers as offsets
                offsets.Add(Convert.ToInt32(curcomment[0]));
            }
            catch
            {
              ;
            }
          }
        }
        else
        {
          //string[] curfield = curLine.Split(fieldSep);
          string[] curfield = null;
          int index = curLine.IndexOf("=");
          if (index > 0)
          {
            curfield = new string[2];
            curfield[0] = curLine.Substring(0, index).Trim();
            if (index < curLine.Length)
              curfield[1] = curLine.Substring(index + 1).Trim();
            else
              curfield[1] = "";
          }
          else
          {
            curfield = new string[1];
            curfield[0] = curLine.Substring(1).Trim();
          }

          if (curfield.Length == 2 && curfield[0].Length > 0)
          {
            // for fields that are split over two lines.
            if (fields.ContainsKey(curfield[0]))
            {
              fields[curfield[0]] += curfield[1];
            }
            else
            {
              fields.Add(curfield[0], curfield[1]);
            }
          }
        }
      }
      InitVariables(offsets, comments, fields);
      return new CDInfoDetail(_discid, _artist, _title, _genre, _year, _length,
                              _cdTrackDetail, _extd, _playorder);
    }

    private void InitVariables(ArrayList offsets, Hashtable comments, Hashtable fields)
    {
      // all the song offsets
      _offsets = (int[])offsets.ToArray(typeof(int));
      _cdTrackDetail = new CDTrackDetail[_offsets.Length];

      foreach (DictionaryEntry dict in comments)
      {
        string key = (string)dict.Key;
        string val = (string)dict.Value;
        switch (key)
        {
          case "Disc length":
            _length = Convert.ToInt32(val.Split(new[] { ' ' })[0].Trim());
            _trackDurations = calculateDurations(_offsets, _length);
            break;
          case "Revision":
            break;
          case "Submitted via":
            break;
          case "Processed by":
            break;
          default:
            break;
        }
      }

      foreach (DictionaryEntry dict in fields)
      {
        string key = (string)dict.Key;
        string val = (string)dict.Value;

        if (key.StartsWith("TTITLE") || key.StartsWith("EXTT")) // a track
        {
          continue;
        }
        else
        {
          switch (key)
          {
            case "DISCID":
              _discid = val;
              break;
            case "DTITLE":
              _title = val;
              break;
            case "DYEAR":
              try
              {
                _year = Convert.ToInt32(val);
              }
              catch
              {
                string year = val;
                int k = 0;
                int l = 0;
                char[] yearChar = year.ToCharArray();
                for (l = 0; l < yearChar.Length; l++)
                {
                  if (Char.IsDigit(yearChar[l]))
                    break;
                }

                for (k = l; k < yearChar.Length; k++)
                {
                  if (!Char.IsDigit(yearChar[k]))
                    break;
                }
                if (l < k)
                {
                  if (k == year.Length)
                    _year = Convert.ToInt32(year.Substring(l));
                  else
                    _year = Convert.ToInt32(year.Substring(l, k - l));
                }
              }
              break;
            case "DGENRE":
              _genre = val;
              break;
            case "EXTD":
              _extd = val;
              break;
            case "PLAYORDER":
              {
                // restricting scope...
                int[] ai;
                char[] seps = { ',', '\n', '\r', '\t' };

                string[] tokens = val.Split(seps);
                if (tokens.Length == 1 && tokens[0].Trim().Length == 0)
                  ai = new int[0];
                else
                  ai = new int[tokens.Length];
                try
                {
                  for (int i = 0; i < ai.Length; i++)
                    ai[i] = Convert.ToInt32(tokens[i]);
                }
                catch { }
                _playorder = ai;
              }
              break;
            default:
              break;
          }
        }
      }
      // find the artist from the title...
      int slash = _title.IndexOf(" / ");
      //Some FreeDB annotators use a " - " instead of the conventional " / "
      //Try to find such a delimiter if the standard one is not found beforehand
      if (slash < 0)
        slash = _title.IndexOf(" - ");
      if (slash < 0)
        _artist = null;
      else
      {
        _artist = _title.Substring(0, slash);
        _title = _title.Substring(slash + 3);
      }

      /*Tracks from compilation-like albums have their own nomenclature in FreeDB (cf. http://www.freedb.org/modules.php?name=Sections&sop=viewarticle&artid=26#2-2), track tags have to be pre-processed

      The only difficulty here is whether or not some Artist or Title names may contain legitimate " / ", regarding FreeDB nomenclature it is illegal
      We split the string even if we're not sure this CD is a real compilation: it is usually no use to try to figure out if the CD is an actual compilation by looking at its Artist tag (album Artist tag = "((Various)|(Assorted))(Artists)?") because most annotators don't tag it that way

      Extended to the detection of " - " as well, a lot of FreeDB annotators do not follow the above rule and split the Artist from the Title name this way; this workaround is a hell more tricky, a few legitimate tags may be badly cut
      We only split the string if we're sure this CD is a real compilation
    
      */
      //isALegitimateCompilation if the CD Artist is either not set or equals "Various", "Various Artists", etc...
      Regex artistTagIsSetToVarious = new Regex(@"^(([Vv]arious)|([Aa]ssorted))( [Aa]rtist[s]?)?$");
      bool isALegitimateCompilation = (_artist == string.Empty || artistTagIsSetToVarious.Match(_artist).Success)
                                        ? true
                                        : false;

      for (int i = 0; i < offsets.Count; i++)
      {
        string title = (string)fields["TTITLE" + i];
        string extt = (string)fields["EXTT" + i];
        _cdTrackDetail[i] = new CDTrackDetail();
        string trackArtist = extendedParseTrackArtist(title, isALegitimateCompilation);
        string trackTitle = extendedParseTrackTitle(title, isALegitimateCompilation);
        _cdTrackDetail[i].Artist = trackArtist.Equals(_artist) ? null : trackArtist;
        _cdTrackDetail[i].Title = trackTitle;
        _cdTrackDetail[i].Offset = _offsets[i];
        _cdTrackDetail[i].DurationInt = _trackDurations[i];
        _cdTrackDetail[i].EXTT = extt;
        _cdTrackDetail[i].Track = i + 1;
      }
    }

    private int[] calculateDurations(int[] offsets, int totalDuration)
    {
      int i = 1;
      int[] durations = new int[offsets.Length];

      for (i = 1; i < offsets.Length; i++)
        durations[i - 1] = (offsets[i] - offsets[i - 1]) / 75;
      durations[i - 1] = totalDuration - (offsets[i - 1] / 75);

      return durations;
    }

    private int[] parseOffsets()
    {
      ArrayList list = new ArrayList();

      try
      {
        int index = _content.IndexOf("Track frame offsets:");
        string[] tokens = _content.Substring(index).Split(_seps);

        for (int i = 1; i < tokens.Length; i++) // skip the track frame offsets
        {
          string offset = tokens[i].Substring(1).Trim(); // skip the # at the begining of the 
          offset = offset.Trim();
          if (offset.Length > 0)
            list.Add(Convert.ToInt32(offset));
          else
            break;
        }
      }
      catch { }

      return (int[])list.ToArray(typeof(int));
    }

    private int parseLength()
    {
      string pattern = @"\s*Disc\slength:\s*(\d*)\s*sec[s|onds]*";
      string val = parseTag(pattern);
      try
      {
        return Convert.ToInt32(val);
      }
      catch { }
      return 0;
    }

    private int parseRevision()
    {
      string pattern = @"\s*Revision:\s*(\d*)\s*\r\n";
      string val = parseTag(pattern);
      try
      {
        return Convert.ToInt32(val);
      }
      catch { }
      return 0;
    }

    private string readSubmitter()
    {
      string pattern = @"\s*Submitted\svia:\s*(.*)\s*\n";
      return parseTag(pattern);
    }

    private string parseProcessedBy()
    {
      string pattern = @"\s*Processed\sby:\s*(.*)\s*\n";
      return parseTag(pattern);
    }

    private string parseTitle()
    {
      string pattern = @"\s*DTITLE\s*=\s*(.*)\s*\n";
      return parseTag(pattern);
    }

    private string parseCDTitle()
    {
      String title = parseTitle();
      int i = title.IndexOf(" / ");
      if (i < 0)
        return title;
      else
        return title.Substring(i + 3);
    }

    private string parseArtist()
    {
      String title = parseTitle();
      int i = title.IndexOf(" / ");
      if (i < 0)
        return null;
      else
        return title.Substring(0, i);
    }

    private string parseExtension()
    {
      string pattern = @"\s*EXTD\s*=\s*(.*)\s*\n";
      return parseTag(pattern);
    }

    private int parseYear()
    {
      int retval = 0;

      try
      {
        retval = Convert.ToInt32(parseTag(@"\s*DYEAR\s*=\s*(.*)\s*\n"));
      }
      catch { }

      return retval;
    }

    private string parseGenre()
    {
      string pattern = @"\s*DGENRE\s*=\s*(.*)\s*\n";
      return parseTag(pattern);
    }

    private string parseTitle(int i)
    {
      string pattern = @"\s*TTITLE" + i + @"\s*=\s*(.*)\s*\n";
      return parseTag(pattern);
    }

    private string extendedParseTrackTitle(string title, bool isALegitimateCompilation)
    {
      int j = title.IndexOf(" / ");
      if (j > 0)
      {
        return title.Substring(j + 3);
      }
      //If we're sure that the CD is a real compilation then we can use this workaround:
      //A lot of annotators don't use the standard " / " to split the Artist name from the Title name, instead they rely on the unconventional " - " delimiter
      else if (isALegitimateCompilation)
      {
        j = title.IndexOf(" - ");
        if (j > 0)
          return title.Substring(j + 3);
      }
      return title;
    }

    private string extendedParseTrackArtist(string title, bool isALegitimateCompilation)
    {
      int j = title.IndexOf(" / ");
      if (j > 0)
      {
        return title.Substring(0, j);
      }
      //If we're sure that the CD is a real compilation then we can use this workaround:
      //A lot of annotators don't use the standard " / " to split the Artist name from the Title name, instead they rely on the unconventional " - " delimiter
      else if (isALegitimateCompilation)
      {
        j = title.IndexOf(" - ");
        if (j > 0)
          return title.Substring(0, j);
      }
      return _artist;
    }

    private string parseTrackTitle(int i)
    {
      String title = parseTitle(i);
      int j = title.IndexOf(" / ");
      if (j > 0)
        return title.Substring(j + 3);
      else
        return title;
    }

    private string parseTrackArtist(int i)
    {
      String title = parseTitle(i);
      int j = title.IndexOf(" / ");
      if (j > 0)
        return title.Substring(0, j);
      else
        return _artist;
    }

    private string parseTrackArtist(int i, string title)
    {
      int j = title.IndexOf(" / ");
      if (j > 0)
        return title.Substring(0, j);
      else
        return _artist;
    }

    private string parseTrackExtension(int i)
    {
      string pattern = @"\s*EXTT" + i + @"\s*=\s*(.*)\s*\n";
      return parseTag(pattern);
    }

    private int[] parsePlayOrder()
    {
      int[] ai;
      char[] seps = { ',', '\n', '\r', '\t' };

      string[] tokens = parseTag(@"\s*PLAYORDER\s*=\s*(.*)\s*\n").Split(seps);
      ai = new int[tokens.Length];
      try
      {
        for (int i = 0; i < ai.Length; i++)
          ai[i] = Convert.ToInt32(tokens[i]);
      }
      catch { }
      return ai;
    }


    private string[] parseDiscIDs()
    {
      string pattern = @"\s*DISCID\s*=\s*(\w*)";
      return parseMultiResultTag(pattern);
    }

    private string getTagText(String key)
    {
      String search = "\n" + key + "=";
      int i = _content.IndexOf(search);
      if (i < 0)
        return null;
      string s2 = "";
      for (; i > -1; i = _content.IndexOf(search, i + key.Length + 2))
        s2 += " " + _content.Substring(i + key.Length + 2, _content.IndexOf("\n", i + key.Length + 2)).Trim();

      return translate(s2.Trim());
    }

    private string translate(string str)
    {
      str = str.Replace("\\n", "\n");
      str = str.Replace("\\t", "\t");
      str = str.Replace("\\\\", "\\");
      return str;
    }

    private string[] parseMultiResultTag(string pattern)
    {
      ArrayList list = new ArrayList();

      Regex tag = new Regex(
        pattern,
        RegexOptions.IgnoreCase
        | RegexOptions.Multiline
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.Compiled
        );

      for (Match m = tag.Match(_content); m.Success; m = m.NextMatch())
      {
        list.Add(m.Groups[1].ToString().Trim());
      }

      return (string[])list.ToArray(typeof(string));
    }

    private string parseTag(string pattern)
    {
      Regex tag = new Regex(
        pattern,
        RegexOptions.IgnoreCase
        | RegexOptions.Multiline
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.Compiled
        );

      Match match = tag.Match(_content);
      return match.Groups[1].ToString().Trim();
    }

    private string parseTag(String beg, String end, int i)
    {
      int j = _content.IndexOf(beg);
      if (j < 1)
        return null;
      int k = _content.IndexOf(beg, j + beg.Length);
      if (k < 1)
        return null;
      else
        return _content.Substring(j + beg.Length, k).Trim();
    }

    public static string createXMCD(CDInfoDetail cdinfo)
    {
      string newline = "\n";
      int index = 0;
      StringBuilder content = new StringBuilder(400);
      StringBuilder tracks = new StringBuilder(100);
      StringBuilder extt = new StringBuilder(100);

      content.Append("# xmcd");
      content.Append(newline);
      content.Append("#");
      content.Append(newline);

      // Track frame offsets
      content.Append("# Track frame offsets:");
      foreach (CDTrackDetail track in cdinfo.Tracks)
      {
        content.Append(newline);
        content.Append("#\t");
        content.Append(track.Offset);

        // do the other info about track too...

        // track title
        tracks.Append(newline);
        tracks.Append("TTITLE");
        tracks.Append(index);
        tracks.Append("=");
        tracks.Append(track.Title);

        // track EXTT
        extt.Append(newline);
        extt.Append("EXTT");
        extt.Append(index);
        extt.Append("=");
        extt.Append(track.EXTT);

        index++;
      }

      // Disc Length
      content.Append(newline);
      content.Append("#");
      content.Append(newline);
      content.Append("# Disc length: ");
      content.Append(cdinfo.Duration);
      content.Append(" seconds");

      // Revision
      content.Append(newline);
      content.Append("#");
      content.Append(newline);
      content.Append("# Revision: 0");

      // App information
      content.Append(newline);
      content.Append("# Submitted via: ");
      content.Append(Appname);
      content.Append(" ");
      content.Append(Appversion);
      content.Append(newline);
      content.Append("#");
      content.Append(newline);

      // DISC ID
      content.Append("DISCID=");
      content.Append(cdinfo.DiscID);
      content.Append(newline);

      // Title = Artist / Title
      content.Append("DTITLE=");
      content.Append(cdinfo.Artist);
      content.Append(" / ");
      content.Append(cdinfo.Title);
      content.Append(newline);

      // Year
      content.Append("DYEAR=");
      content.Append(cdinfo.Year);
      content.Append(newline);

      // Genre
      content.Append("DGENRE=");
      content.Append(cdinfo.Genre);

      //track titles
      content.Append(tracks);

      // EXTD
      content.Append(newline);
      content.Append("EXTD=");
      content.Append(cdinfo.EXTD);

      // EXTT
      content.Append(extt);

      // EXTD
      content.Append(newline);
      content.Append("PLAYORDER=");
      int[] order = cdinfo.PlayOrder;
      for (int i = 0; i < order.Length; i++)
      {
        if (i != 0)
          content.Append(',');
        content.Append(order[i]);
      }
      content.Append(newline);

      return content.ToString();
    }
  }
}
