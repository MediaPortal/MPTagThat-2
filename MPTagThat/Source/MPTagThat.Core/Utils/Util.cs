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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using MPTagThat.Core.Common.Converter;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Prism.Ioc;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Controls.Input;
using Syncfusion.Windows.Primitives;
using TagLib;
using Un4seen.Bass.AddOn.Cd;
using WPFLocalizeExtension.Engine;
using Binding = System.Windows.Data.Binding;
using File = TagLib.File;
using GridViewColumn = MPTagThat.Core.Common.GridViewColumn;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Drawing.Image;
using Tag = TagLib.Id3v2.Tag;
// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringLiteralTypo

#endregion

namespace MPTagThat.Core.Utils
{
  public sealed class Util
  {
    #region Enum

    public enum MP3Error : int
    {
      NoError = 0,
      Fixable = 1,
      NonFixable = 2,
      Fixed = 3
    }

    #endregion

    #region Variables

    private static Util instance = new Util();
    private static ILogger log;
    private static readonly object padlock = new object();

    private static char[] _invalidFilenameChars;
    private static char[] _invalidFoldernameChars;

    #endregion

    #region ctor

    private Util()
    {
      log = ContainerLocator.Current.Resolve<ILogger>().GetLogger;
      _invalidFilenameChars = Path.GetInvalidFileNameChars();
      _invalidFoldernameChars = Path.GetInvalidPathChars();
    }

    #endregion

    #region Properties

    /// <summary>
    /// The available ISO Languages
    /// </summary>
    public static string[] ISO_LANGUAGES { get; } =
    {
      "aar - Afar", "abk - Abkhazian", "ace - Achinese", "ach - Acoli"
      , "ada - Adangme", "ady - Adyghe; Adygei",
      "afa - Afro-Asiatic (Other)", "afh - Afrihili",
      "afr - Afrikaans", "ain - Ainu", "aka - Akan", "akk - Akkadian",
      "alb - Albanian", "ale - Aleut", "alg - Algonquian languages",
      "alt - Southern Altai", "amh - Amharic",
      "ang - English, Old (ca.450-1100)", "anp - Angika",
      "apa - Apache languages", "ara - Arabic",
      "arc - Official Aramaic (700-300 BCE)", "arg - Aragonese",
      "arm - Armenian", "arn - Mapudungun; Mapuche", "arp - Arapaho",
      "art - Artificial (Other)", "arw - Arawak", "asm - Assamese",
      "ast - Asturian; Bable; Leonese; Asturleonese",
      "ath - Athapascan languages", "aus - Australian languages",
      "ava - Avaric", "ave - Avestan", "awa - Awadhi", "aym - Aymara",
      "aze - Azerbaijani", "bad - Banda languages",
      "bai - Bamileke languages", "bak - Bashkir", "bal - Baluchi",
      "bam - Bambara", "ban - Balinese", "baq - Basque", "bas - Basa",
      "bat - Baltic (Other)", "bej - Beja; Bedawiyet",
      "bel - Belarusian", "bem - Bemba", "ben - Bengali",
      "ber - Berber (Other)", "bho - Bhojpuri", "bih - Bihari",
      "bik - Bikol", "bin - Bini; Edo", "bis - Bislama",
      "bla - Siksika", "bnt - Bantu (Other)", "bos - Bosnian",
      "bra - Braj",
      "bre - Breton", "btk - Batak languages", "bua - Buriat",
      "bug - Buginese", "bul - Bulgarian", "bur - Burmese",
      "byn - Blin; Bilin", "cad - Caddo",
      "cai - Central American Indian (Other)", "car - Galibi Carib",
      "cat - Catalan; Valencian", "cau - Caucasian (Other)",
      "ceb - Cebuano", "cel - Celtic (Other)", "cha - Chamorro",
      "chb - Chibcha",
      "che - Chechen", "chg - Chagatai", "chi - Chinese",
      "chk - Chuukese", "chm - Mari", "chn - Chinook jargon",
      "cho - Choctaw", "chp - Chipewyan; Dene Suline",
      "chr - Cherokee", "chu - Church Slavic; Old Slavonic",
      "chv - Chuvash", "chy - Cheyenne", "cmc - Chamic languages",
      "cop - Coptic", "cor - Cornish", "cos - Corsican",
      "cpe - Creoles and pidgins, English based (Other)",
      "cpf - Creoles and pidgins, French-based (Other)",
      "cpp - Creoles and pidgins, Portuguese-based (Other)",
      "cre - Cree", "crh - Crimean Tatar; Crimean Turkish",
      "crp - Creoles and pidgins (Other)", "csb - Kashubian",
      "cus - Cushitic (Other)", "cze - Czech", "dak - Dakota",
      "dan - Danish", "dar - Dargwa", "day - Land Dayak languages",
      "del - Delaware", "den - Slave (Athapascan)", "dgr - Dogrib",
      "din - Dinka", "div - Divehi; Dhivehi; Maldivian", "doi - Dogri"
      , "dra - Dravidian (Other)", "dsb - Lower Sorbian",
      "dua - Duala", "dum - Dutch, Middle (ca.1050-1350)",
      "dut - Dutch; Flemish", "dyu - Dyula", "dzo - Dzongkha",
      "efi - Efik", "egy - Egyptian (Ancient)", "eka - Ekajuk",
      "elx - Elamite", "eng - English",
      "enm - English, Middle (1100-1500)", "epo - Esperanto",
      "est - Estonian", "ewe - Ewe", "ewo - Ewondo", "fan - Fang",
      "fao - Faroese", "fat - Fanti", "fij - Fijian",
      "fil - Filipino; Pilipino", "fin - Finnish",
      "fiu - Finno-Ugrian (Other)", "fon - Fon", "fre - French",
      "frm - French, Middle (ca.1400-1600)",
      "fro - French, Old (842-ca.1400)",
      "frr - Northern Frisian", "frs - Eastern Frisian",
      "fry - Western Frisian", "ful - Fulah", "fur - Friulian",
      "gaa - Ga", "gay - Gayo", "gba - Gbaya",
      "gem - Germanic (Other)", "geo - Georgian", "ger - German",
      "gez - Geez", "gil - Gilbertese",
      "gla - Gaelic; Scottish Gaelic", "gle - Irish", "glg - Galician"
      , "glv - Manx", "gmh - German, Middle High (ca.1050-1500)",
      "goh - German, Old High (ca.750-1050)", "gon - Gondi",
      "gor - Gorontalo", "got - Gothic", "grb - Grebo",
      "grc - Greek, Ancient (to 1453)", "gre - Greek, Modern (1453-)",
      "grn - Guarani", "gsw - Swiss German; Alemannic; Alsatian",
      "guj - Gujarati", "gwi - Gwich'in", "hai - Haida",
      "hat - Haitian; Haitian Creole", "hau - Hausa", "haw - Hawaiian"
      , "heb - Hebrew", "her - Herero",
      "hil - Hiligaynon", "him - Himachali", "hin - Hindi",
      "hit - Hittite", "hmn - Hmong", "hmo - Hiri Motu",
      "hsb - Upper Sorbian", "hun - Hungarian", "hup - Hupa",
      "iba - Iban", "ibo - Igbo", "ice - Icelandic", "ido - Ido",
      "iii - Sichuan Yi; Nuosu", "ijo - Ijo languages",
      "iku - Inuktitut", "ile - Interlingue; Occidental",
      "ilo - Iloko", "ina - Interlingua", "inc - Indic (Other)",
      "ind - Indonesian", "ine - Indo-European (Other)",
      "inh - Ingush", "ipk - Inupiaq", "ira - Iranian (Other)",
      "iro - Iroquoian languages", "ita - Italian", "jav - Javanese",
      "jbo - Lojban", "jpn - Japanese", "jpr - Judeo-Persian",
      "jrb - Judeo-Arabic", "kaa - Kara-Kalpak", "kab - Kabyle",
      "kac - Kachin; Jingpho", "kal - Kalaallisut; Greenlandic",
      "kam - Kamba", "kan - Kannada", "kar - Karen languages",
      "kas - Kashmiri", "kau - Kanuri", "kaw - Kawi", "kaz - Kazakh",
      "kbd - Kabardian", "kha - Khasi", "khi - Khoisan (Other)",
      "khm - Central Khmer", "kho - Khotanese", "kik - Kikuyu; Gikuyu"
      , "kin - Kinyarwanda", "kir - Kirghiz; Kyrgyz", "kmb - Kimbundu"
      , "kok - Konkani", "kom - Komi",
      "kon - Kongo", "kor - Korean", "kos - Kosraean", "kpe - Kpelle",
      "krc - Karachay-Balkar", "krl - Karelian", "kro - Kru languages"
      , "kru - Kurukh", "kua - Kuanyama; Kwanyama", "kum - Kumyk",
      "kur - Kurdish", "kut - Kutenai", "lad - Ladino", "lah - Lahnda"
      , "lam - Lamba", "lao - Lao", "lat - Latin", "lav - Latvian",
      "lez - Lezghian", "lim - Limburgan; Limburger; Limburgish",
      "lin - Lingala", "lit - Lithuanian", "lol - Mongo", "loz - Lozi"
      , "ltz - Luxembourgish; Letzeburgesch", "lua - Luba-Lulua",
      "lub - Luba-Katanga", "lug - Ganda", "lui - Luiseno",
      "lun - Lunda", "luo - Luo (Kenya and Tanzania)", "lus - Lushai",
      "mac - Macedonian", "mad - Madurese", "mag - Magahi",
      "mah - Marshallese", "mai - Maithili",
      "mak - Makasar", "mal - Malayalam", "man - Mandingo",
      "mao - Maori", "map - Austronesian (Other)", "mar - Marathi",
      "mas - Masai", "may - Malay", "mdf - Moksha", "mdr - Mandar",
      "men - Mende", "mga - Irish, Middle (900-1200)",
      "mic - Mi'kmaq; Micmac", "min - Minangkabau",
      "mis - Uncoded languages", "mkh - Mon-Khmer (Other)",
      "mlg - Malagasy", "mlt - Maltese",
      "mnc - Manchu", "mni - Manipuri", "mno - Manobo languages",
      "moh - Mohawk", "mol - Moldavian", "mon - Mongolian",
      "mos - Mossi", "mul - Multiple languages",
      "mun - Munda languages", "mus - Creek", "mwl - Mirandese",
      "mwr - Marwari", "myn - Mayan languages", "myv - Erzya",
      "nah - Nahuatl languages", "nai - North American Indian",
      "nap - Neapolitan", "nau - Nauru",
      "nav - Navajo; Navaho", "nbl - Ndebele, South; South Ndebele",
      "nde - Ndebele, North; North Ndebele", "ndo - Ndonga",
      "nds - Low German; Low Saxon; German, Low; Saxon, Low",
      "nep - Nepali", "new - Nepal Bhasa; Newari", "nia - Nias",
      "nic - Niger-Kordofanian (Other)", "niu - Niuean",
      "nno - Norwegian Nynorsk; Nynorsk, Norwegian",
      "nob - Bokmål, Norwegian; Norwegian Bokmål", "nog - Nogai",
      "non - Norse, Old", "nor - Norwegian", "nqo - N'Ko",
      "nso - Pedi; Sepedi; Northern Sotho", "nub - Nubian languages",
      "nwc - Classical Newari; Old Newari; Classical Nepal Bhasa",
      "nya - Chichewa; Chewa; Nyanja", "nym - Nyamwezi",
      "nyn - Nyankole", "nyo - Nyoro", "nzi - Nzima",
      "oci - Occitan (post 1500); Provençal", "oji - Ojibwa",
      "ori - Oriya", "orm - Oromo", "osa - Osage",
      "oss - Ossetian; Ossetic", "ota - Turkish, Ottoman (1500-1928)",
      "oto - Otomian languages", "paa - Papuan (Other)",
      "pag - Pangasinan", "pal - Pahlavi",
      "pam - Pampanga; Kapampangan", "pan - Panjabi; Punjabi",
      "pap - Papiamento", "pau - Palauan",
      "peo - Persian, Old (ca.600-400 B.C.)", "per - Persian",
      "phi - Philippine (Other)", "phn - Phoenician", "pli - Pali",
      "pol - Polish", "pon - Pohnpeian", "por - Portuguese",
      "pra - Prakrit languages", "pro - Provençal, Old (to 1500)",
      "pus - Pushto; Pashto", "que - Quechua", "raj - Rajasthani",
      "rap - Rapanui", "rar - Rarotongan; Cook Islands Maori",
      "roa - Romance (Other)", "roh - Romansh", "rom - Romany",
      "rum - Romanian", "run - Rundi",
      "rup - Aromanian; Arumanian; Macedo-Romanian", "rus - Russian",
      "sad - Sandawe", "sag - Sango", "sah - Yakut",
      "sai - South American Indian (Other)",
      "sal - Salishan languages", "sam - Samaritan Aramaic",
      "san - Sanskrit", "sas - Sasak", "sat - Santali",
      "scc - Serbian", "scn - Sicilian", "sco - Scots",
      "scr - Croatian", "sel - Selkup", "sem - Semitic (Other)",
      "sga - Irish, Old (to 900)", "sgn - Sign Languages",
      "shn - Shan",
      "sid - Sidamo", "sin - Sinhala; Sinhalese",
      "sio - Siouan languages", "sit - Sino-Tibetan (Other)",
      "sla - Slavic (Other)", "slo - Slovak", "slv - Slovenian",
      "sma - Southern Sami", "sme - Northern Sami",
      "smi - Sami languages (Other)", "smj - Lule Sami",
      "smn - Inari Sami", "smo - Samoan", "sms - Skolt Sami",
      "sna - Shona", "snd - Sindhi", "snk - Soninke", "sog - Sogdian",
      "som - Somali", "son - Songhai languages",
      "sot - Sotho, Southern", "spa - Spanish; Castilian",
      "srd - Sardinian", "srn - Sranan Tongo", "srr - Serer",
      "ssa - Nilo-Saharan (Other)", "ssw - Swati", "suk - Sukuma",
      "sun - Sundanese", "sus - Susu", "sux - Sumerian",
      "swa - Swahili", "swe - Swedish", "syc - Classical Syriac",
      "syr - Syriac", "tah - Tahitian", "tai - Tai (Other)",
      "tam - Tamil", "tat - Tatar", "tel - Telugu", "tem - Timne",
      "ter - Tereno", "tet - Tetum",
      "tgk - Tajik", "tgl - Tagalog", "tha - Thai", "tib - Tibetan",
      "tig - Tigre", "tir - Tigrinya", "tiv - Tiv", "tkl - Tokelau",
      "tlh - Klingon; tlhIngan-Hol", "tli - Tlingit", "tmh - Tamashek"
      , "tog - Tonga (Nyasa)", "ton - Tonga (Tonga Islands)",
      "tpi - Tok Pisin", "tsi - Tsimshian", "tsn - Tswana",
      "tso - Tsonga", "tuk - Turkmen", "tum - Tumbuka",
      "tup - Tupi languages", "tur - Turkish",
      "tut - Altaic (Other)", "tvl - Tuvalu", "twi - Twi",
      "tyv - Tuvinian", "udm - Udmurt", "uga - Ugaritic",
      "uig - Uighur; Uyghur", "ukr - Ukrainian", "umb - Umbundu",
      "und - Undetermined", "urd - Urdu", "uzb - Uzbek", "vai - Vai",
      "ven - Venda", "vie - Vietnamese", "vol - Volapük",
      "vot - Votic", "wak - Wakashan languages", "wal - Walamo",
      "war - Waray",
      "was - Washo", "wel - Welsh", "wen - Sorbian languages",
      "wln - Walloon", "wol - Wolof", "xal - Kalmyk; Oirat",
      "xho - Xhosa", "yao - Yao", "yap - Yapese", "yid - Yiddish",
      "yor - Yoruba", "ypk - Yupik languages", "zap - Zapotec",
      "zbl - Blissymbols; Blissymbolics; Bliss", "zen - Zenaga",
      "zha - Zhuang; Chuang", "znd - Zande languages", "zul - Zulu",
      "zun - Zuni", "zxx - No linguistic content",
      "zza - Zaza; Dimili; Dimli; Kirdki; Kirmanjki; Zazaki"
    };

    /// <summary>
    /// Standard Genres
    /// </summary>
    public static string[] Genres { get; } =
    {
      "Pop", "Rock", "Classic Rock", "Dance",
      "Rap", "Techno", "Blues", "Country",
      "Punk Rock", "Punk", "R&B", "Metal",
      "Folk", "Folk/Rock", "A Cappella", "Acid Jazz",
      "Acid Punk", "Acid", "Acoustic", "Alternative Rock",
      "Alternative", "Ambient", "Anime", "Avantgarde",
      "Ballad", "Bass", "Beat", "Bebob",
      "Big Band", "Black Metal", "Bluegrass", "Booty Bass",
      "BritPop", "Cabaret", "Celtic", "Chamber Music",
      "Chanson", "Chorus", "Christian Gangsta Rap", "Christian Rap",
      "Christian Rock", "Classical", "Club", "Club-House",
      "Comedy", "Contemporary Christian", "Crossover", "Cult",
      "Dance Hall", "Darkwave", "Death Metal", "Disco",
      "Dream", "Drum & Bass", "Drum Solo", "Duet",
      "Easy Listening", "Electronic", "Ethnic", "Euro-House",
      "Euro-Techno", "Eurodance", "Folklore", "Freestyle",
      "Funk", "Fusion", "Fusion", "Game",
      "Gangsta", "Goa", "Gospel", "Gothic Rock",
      "Gothic", "Grunge", "Hard Rock", "Hardcore",
      "Heavy Metal", "Hip-Hop", "House", "Humour",
      "Indie", "Industrial", "Instrumental Pop", "Instrumental Rock",
      "Instrumental", "Jazz", "Jazz+Funk", "Jpop",
      "Jungle", "Latin", "Lo-Fi", "Meditative",
      "Merengue", "Musical", "National Folk", "Native American",
      "Negerpunk", "New Age", "New Wave", "Noise",
      "Oldies", "Opera", "Other", "Polka",
      "Polsk Punk", "Pop-Folk", "Pop/Funk", "Porn Groove",
      "Power Ballad", "Pranks", "Primus", "Progressive Rock",
      "Psychedelic Rock", "Psychedelic", "Rave", "Reggae",
      "Retro", "Revival", "Rhythmic Soul", "Rock & Roll",
      "Salsa", "Samba", "Satire", "Showtunes",
      "Ska", "Slow Jam", "Slow Rock", "Sonata",
      "Soul", "Sound Clip", "Soundtrack", "Southern Rock",
      "Space", "Speech", "Swing", "Symphonic Rock",
      "Symphony", "Synthpop", "Tango", "Techno-Industrial",
      "Terror", "Thrash Metal", "Top 40", "Trailer",
      "Trance", "Tribal", "Trip-Hop", "Vocal"
    };

    /// <summary>
    /// The standard ID3 Frames directly supported by TagLib #
    /// </summary>
    public static string[] StandardFrames { get; } = {
      "TPE1", "TPE2", "TALB", "TBPM", "COMM", "TCOM",
      "TPE3", "TCOP", "TPOS", "TCON", "TIT1", "USLT", "APIC",
      "POPM", "TIT2", "TRCK", "TYER", "TDRC"
    };

    /// <summary>
    /// The extended ID3 Frames 
    /// </summary>
    public static string[] ExtendedFrames { get; } = {
      "TSOP", "TSOA", "WCOM", "WCOP", "TENC", "TPE4", "TIPL",
      "IPLS", "TMED", "TMCL", "WOAF", "WOAR", "WOAS", "WORS",
      "WPAY", "WPUB", "TOAL", "TOFN", "TOLY", "TOPE", "TOWN",
      "TDOR", "TORY", "TPUB", "TIT3", "TEXT","TSOT", "TLEN",
      "TCMP", "TSO2", "UFID"
    };

    #endregion

    #region String related methods

    /// <summary>
    ///   Fast Case Sensitive Replace Method
    /// </summary>
    /// <param name = "original"></param>
    /// <param name = "pattern"></param>
    /// <param name = "replacement"></param>
    /// <returns></returns>
    public static string ReplaceEx(string original, string pattern, string replacement)
    {
      int position0, position1;
      var count = position0 = position1 = 0;
      var upperString = original.ToUpper();
      var upperPattern = pattern.ToUpper();
      var inc = (original.Length / pattern.Length) *
                (replacement.Length - pattern.Length);
      var chars = new char[original.Length + Math.Max(0, inc)];
      while ((position1 = upperString.IndexOf(upperPattern,
               position0, StringComparison.Ordinal)) != -1)
      {
        for (int i = position0; i < position1; ++i)
          chars[count++] = original[i];
        for (int i = 0; i < replacement.Length; ++i)
          chars[count++] = replacement[i];
        position0 = position1 + pattern.Length;
      }
      if (position0 == 0) return original;
      for (int i = position0; i < original.Length; ++i)
        chars[count++] = original[i];
      return new string(chars, 0, count);
    }

    /// <summary>
    ///   This function matches the Longet Common Substring of the source string found in target string
    /// </summary>
    /// <param name = "sourceString">The Source String to match</param>
    /// <param name = "targetString">The Target String to search within</param>
    /// <returns>a match ratio</returns>
    public static double LongestCommonSubstring(string sourceString, string targetString)
    {
      if (String.IsNullOrEmpty(sourceString) || String.IsNullOrEmpty(targetString))
        return 0;

      sourceString = sourceString.ToLower().Replace(",", "").Replace(" ", "").Replace(";", "").Replace("_", "");
      targetString = targetString.ToLower().Replace(",", "").Replace(" ", "").Replace(";", "").Replace("_", "");

      int[,] num = new int[sourceString.Length,targetString.Length];
      int maxlen = 0;

      for (int i = 0; i < sourceString.Length; i++)
      {
        for (int j = 0; j < targetString.Length; j++)
        {
          if (sourceString[i] != targetString[j])
            num[i, j] = 0;
          else
          {
            if ((i == 0) || (j == 0))
              num[i, j] = 1;
            else
              num[i, j] = 1 + num[i - 1, j - 1];

            if (num[i, j] > maxlen)
            {
              maxlen = num[i, j];
            }
          }
        }
      }
      return maxlen / (double)sourceString.Length;
    }

    #endregion

    #region UI Related Methods

    /// <summary>
    ///   Formats a Grid Column based on the Settings
    /// </summary>
    /// <param name = "setting"></param>
    public static GridColumn FormatGridColumn(GridViewColumn setting)
    {
      GridColumn column;
      switch (setting.Type.ToLower())
      {
        case "image":
          column = new GridImageColumn();
          break;

        case "process":
          column = new GridPercentColumn();
          break;

        case "check":
          column = new GridCheckBoxColumn();
          break;

        case "rating":
          column = new GridTemplateColumn();
          var rating = new FrameworkElementFactory(typeof(SfRating));
          rating.SetValue(SfRating.PrecisionProperty, Precision.Standard);
          rating.SetValue(SfRating.ItemsCountProperty, 5);
          rating.SetBinding(SfRating.ValueProperty, new Binding { Path = new PropertyPath("Rating"), Mode = BindingMode.TwoWay });

          var itemStyle = new Style();
          itemStyle.Setters.Add(new Setter(SfRatingItem.HeightProperty, 17d));
          itemStyle.Setters.Add(new Setter(SfRatingItem.WidthProperty, 17d));
          rating.SetValue(SfRating.ItemContainerStyleProperty, itemStyle);

          var dataTemplate = new DataTemplate();
          dataTemplate.VisualTree = rating;
          column.CellTemplate = dataTemplate;
          break;

        default:
          column = new GridTextColumn();
          break;
      }

      column.HeaderText = LocalizeDictionary.Instance.GetLocalizedObject("MPTagThat", "Strings", $"songHeader_{setting.Name}",
        LocalizeDictionary.Instance.Culture).ToString();
      column.IsReadOnly = setting.Readonly;
      column.IsHidden = !setting.Display;
      column.AllowFiltering = setting.AllowFilter;
      column.Width = setting.Width;
      column.MappingName = setting.Name;

      if (setting.Name == "Status")
      {
        // Don't need a header and also set image width
        column.HeaderText = "";
        column.Width = 25;
        column.TextAlignment = TextAlignment.Center;
        var binding = new Binding("Status") { Converter = new SongStatusToImageConverter() };
        column.ValueBinding = binding;
        column.AllowFiltering = false;

        // Bind the tooltip for the column to StatusMsg
        var styleToolTip = new Style();
        styleToolTip.Setters.Add(new Setter(ToolTipService.ToolTipProperty, new Binding("StatusMsg")));
        column.CellStyle = styleToolTip;
      }

      switch (setting.Type.ToLower())
      {
        case "text":
        case "process":
          //  column.ValueType = typeof(string);
          break;
        case "number":
        case "check":
        case "rating":
          //  column.ValueType = typeof(int);
          break;
      }

      return column;
    }

    /// <summary>
    /// Update the Current File in the Status Bar
    /// </summary>
    /// <param name="msg"></param>
    public static void StatusCurrentFile(string msg)
    {
      var evt = new StatusBarEvent { CurrentFile = msg, Type = StatusBarEvent.StatusTypes.CurrentFile };
      EventSystem.Publish(evt);
    }

    #endregion

    #region Tag related methods

    /// <summary>
    /// Checks, if we got a Spacial Userframe
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    public static bool IsSpecialUserFrame(string description)
    {
      switch (description.ToLowerInvariant())
      {
        case "replaygain_track_gain":
        case "replaygain_track_peak":
        case "replaygain_album_gain":
        case "replaygain_album_peak":
        case "musicbrainz artist id":
        case "musicbrainz release group id":
        case "musicbrainz album id":
        case "musicbrainz album artist id":
        case "musicbrainz disc id":
        case "musicbrainz album status":
        case "musicbrainz album type":
        case "musicbrainz album release country":
          return true;
      }
      return false;
    }

    /// <summary>
    ///   Based on the Options set, use the correct version for ID3
    ///   Eventually remove V1 or V2 tags, if set in the options
    /// </summary>
    /// <param name = "file"></param>
    public static File FormatID3Tag(File file)
    {
      if (file.MimeType == "taglib/mp3")
      {
        var options = ContainerLocator.Current.Resolve<ISettingsManager>()?.GetOptions;
        Tag id3v2_tag = file.GetTag(TagTypes.Id3v2) as Tag;
        if (id3v2_tag != null && options.MainSettings.ID3V2Version > 0)
          id3v2_tag.Version = (byte)options.MainSettings.ID3V2Version;

        // Remove V1 Tags, if checked or "Save V2 only checked"
        if (options.MainSettings.RemoveID3V1 || options.MainSettings.ID3Version == 2)
          file.RemoveTags(TagTypes.Id3v1);

        // Remove V2 Tags, if checked or "Save V1 only checked"
        if (options.MainSettings.RemoveID3V2 || options.MainSettings.ID3Version == 1)
          file.RemoveTags(TagTypes.Id3v2);

        // Remove V2 Tags, if Ape checked
        if (options.MainSettings.ID3V2Version == 0)
        {
          file.RemoveTags(TagTypes.Id3v2);
        }
        else
        {
          file.RemoveTags(TagTypes.Ape);
        }
      }
      return file;
    }

    #endregion

    #region Picture related methods

    /// <summary>
    ///   Save the Picture of the track as folder.jpg
    /// </summary>
    /// <param name = "pic"></param>
    /// <param name = "name"></param>
    public static void SavePicture(Common.Song.Picture pic, string name)
    {
      if (pic != null)
      {
        try
        {
          Image img = Common.Song.Picture.ImageFromData(pic.Data);
          // Need to make a copy, otherwise we have a GDI+ Error
          Bitmap bCopy = new Bitmap(img);
          bCopy.Save(name, ImageFormat.Jpeg);
        }
        catch (Exception ex)
        {
          log.Error("Exception Saving picture: {0} {1}", name, ex.Message);
        }
      }
    }

    /// <summary>
    ///   Return the folder.jpg as a Taglib.Picture
    /// </summary>
    /// <param name = "folder"></param>
    /// <returns></returns>
    public static Common.Song.Picture GetFolderThumb(string folder)
    {
      string thumb = Path.Combine(folder, "folder.jpg");
      if (!System.IO.File.Exists(thumb))
      {
        return null;
      }

      try
      {
        Common.Song.Picture pic = new Common.Song.Picture(thumb);
        pic.Description = "Front Cover";
        pic.Type = PictureType.FrontCover;
        return pic;
      }
      catch (Exception ex)
      {
        log.Error("Exception loading thumb file: {0} {1}", thumb, ex.Message);
        return null;
      }
    }

    #endregion

    #region File related methods

    /// <summary>
    ///   Is this an Audio file, which can be handled by MPTagThat
    /// </summary>
    /// <param name = "fileName"></param>
    /// <returns></returns>
    public static bool IsAudio(string fileName)
    {
      string ext = Path.GetExtension(fileName)?.ToLower();

      switch (ext)
      {
        case ".aif":
        case ".aiff":
        case ".ape":
        case ".asf":
        case ".dsf":
        case ".flac":
        case ".mp3":
        case ".ogg":
        case ".opus":
        case ".wv":
        case ".wma":
        case ".mp4":
        case ".m4a":
        case ".m4b":
        case ".m4p":
        case ".mpc":
        case ".mp+":
        case ".mpp":
        case ".wav":
          return true;
      }
      return false;
    }

    /// <summary>
    ///   Is this a Picture file, which can be shown in Listview
    /// </summary>
    /// <param name = "fileName"></param>
    /// <returns></returns>
    public static bool IsPicture(string fileName)
    {
      string ext = Path.GetExtension(fileName)?.ToLower();

      switch (ext)
      {
        case ".bmp":
        case ".gif":
        case ".jpg":
        case ".jpeg":
        case ".png":
          return true;
      }
      return false;
    }

    /// <summary>
    ///   Make a Valid Filename out of a given String
    /// </summary>
    /// <param name = "str"></param>
    /// <returns></returns>
    public static string MakeValidFileName(string str)
    {
      if (str.IndexOfAny(_invalidFilenameChars) > -1)
      {
        foreach (char c in _invalidFilenameChars)
          str = str.Replace(c, '_');
      }
      return str;
    }

    #endregion

    #region Folder related methods

    /// <summary>
    ///   Make a Valid Foldername out of a given String
    /// </summary>
    /// <param name = "str"></param>
    /// <returns></returns>
    public static string MakeValidFolderName(string str)
    {
      if (str.IndexOfAny(_invalidFoldernameChars) > -1)
      {
        foreach (char c in _invalidFoldernameChars)
          str = str.Replace(c, '_');
      }

      // In addition we don't want to see "?" and ":" in our folders
      str = str.Replace('?', '_');
      str = str.Replace(':', '_');

      return str;
    }

    /// <summary>
    /// Deletes folder, setting the attributes before to allow deletion
    /// </summary>
    /// <param name="folderName"></param>
    public static void DeleteFolder(string folderName)
    {
      try
      {
        if (Directory.Exists(folderName))
        {
          var dirInfo = new DirectoryInfo(folderName);
          dirInfo.Attributes = dirInfo.Attributes & ~FileAttributes.ReadOnly;
          Directory.Delete(folderName, true);
        }
      }
      catch (Exception ex)
      {
        log.Error("Exception deleting folder: {0} {1}", folderName, ex.Message);
      }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    /// <summary>
    /// Use to emulate the C lib function _splitpath()
    /// </summary>
    /// <param name="path">The path to split</param>
    /// <param name="rootpath">optional root if a relative path</param>
    /// <returns>the folders in the path. 
    ///     Item 0 is drive letter with ':' 
    ///     If path is UNC path then item 0 is "\\"
    /// </returns>
    /// <example>
    /// string p1 = @"c:\p1\p2\p3\p4";
    /// string[] ap1 = p1.SplitPath();
    /// // ap1 = {"c:", "p1", "p2", "p3", "p4"}
    /// string p2 = @"\\server\p2\p3\p4";
    /// string[] ap2 = p2.SplitPath();
    /// // ap2 = {@"\\", "server", "p2", "p3", "p4"}
    /// string p3 = @"..\p3\p4";
    /// string root3 = @"c:\p1\p2\";
    /// string[] ap3 = p1.SplitPath(root3);
    /// // ap3 = {"c:", "p1", "p3", "p4"}
    /// </example>
    public static string[] SplitPath(string path, string rootpath = "")
    {
      string drive;
      string[] astr;
      path = Path.GetFullPath(Path.Combine(rootpath, path));
      if (path[1] == ':')
      {
        drive = path.Substring(0, 2);
        string newpath = path.Substring(2);
        astr = newpath.Split(new[] { Path.DirectorySeparatorChar }
          , StringSplitOptions.RemoveEmptyEntries);
      }
      else
      {
        drive = @"\\";
        astr = path.Split(new[] { Path.DirectorySeparatorChar }
          , StringSplitOptions.RemoveEmptyEntries);
      }
      string[] splitPath = new string[astr.Length + 1];
      splitPath[0] = drive;
      astr.CopyTo(splitPath, 1);
      return splitPath;
    }

    #endregion

    #region Web Related Methods

    /// <summary>
    ///   Returns the requested WebPage
    /// </summary>
    /// <param name = "requestString"></param>
    /// <returns></returns>
    public static string GetWebPage(string requestString)
    {
      string responseString = null;
      try
      {
        var request = (HttpWebRequest)WebRequest.Create(requestString);
        request.Proxy.Credentials = CredentialCache.DefaultCredentials;
        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36";
        var response = (HttpWebResponse)request.GetResponse();
        using (var responseStream = response.GetResponseStream())
        {
          using (var reader = new StreamReader(responseStream))
          {
            responseString = reader.ReadToEnd();
          }
        }
      }
      catch (Exception ex)
      {
        log.Error($"Util: Error retrieving Web Page: {requestString} {ex.Message} {ex.StackTrace}");
      }

      return responseString;
    }

    /// <summary>
    ///   Reads data from a stream until the end is reached. The
    ///   data is returned as a byte array. An IOException is
    ///   thrown if any of the underlying IO calls fail.
    /// </summary>
    /// <param name = "stream">The stream to read data from</param>
    /// <param name = "initialLength">The initial buffer length</param>
    public static byte[] ReadFullStream(Stream stream, int initialLength)
    {
      // If we've been passed an unhelpful initial length, just
      // use 32K.
      if (initialLength < 1)
      {
        initialLength = 32768;
      }

      var buffer = new byte[initialLength];
      var read = 0;

      var chunk = 0;
      while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
      {
        read += chunk;

        // If we've reached the end of our buffer, check to see if there's
        // any more information
        if (read == buffer.Length)
        {
          var nextByte = stream.ReadByte();

          // End of stream? If so, we're done
          if (nextByte == -1)
          {
            return buffer;
          }

          // Nope. Resize the buffer, put in the byte we've just
          // read, and continue
          var newBuffer = new byte[buffer.Length * 2];
          Array.Copy(buffer, newBuffer, buffer.Length);
          newBuffer[read] = (byte)nextByte;
          buffer = newBuffer;
          read++;
        }
      }
      // Buffer is now too big. Shrink it.
      var ret = new byte[read];
      Array.Copy(buffer, ret, read);
      return ret;
    }

    /// <summary>
    /// Extracts selected Html fragment string from clipboard data by parsing header information 
    /// in htmlDataString
    /// </summary>
    /// <param name="htmlDataString">
    /// String representing Html clipboard data. This includes Html header
    /// </param>
    /// <returns>
    /// String containing only the Html selection part of htmlDataString, without header
    /// </returns>
    public static string ExtractHtmlFragmentFromClipboardData(string htmlDataString)
    {
      // HTML Clipboard Format
      // (https://msdn.microsoft.com/en-us/library/aa767917(v=vs.85).aspx)

      // The fragment contains valid HTML representing the area the user has selected. This 
      // includes the information required for basic pasting of an HTML fragment, as follows:
      //  - Selected text. 
      //  - Opening tags and attributes of any element that has an end tag within the selected text. 
      //  - End tags that match the included opening tags. 

      // The fragment should be preceded and followed by the HTML comments <!--StartFragment--> and 
      // <!--EndFragment--> (no space allowed between the !-- and the text) to indicate where the 
      // fragment starts and ends. So the start and end of the fragment are indicated by these 
      // comments as well as by the StartFragment and EndFragment byte counts. Though redundant, 
      // this makes it easier to find the start of the fragment (from the byte count) and mark the 
      // position of the fragment directly in the HTML tree.

      // Byte count from the beginning of the clipboard to the start of the fragment.
      int startFragmentIndex = htmlDataString.IndexOf("StartFragment:");
      if (startFragmentIndex < 0)
      {
        return "ERROR: Unrecognized html header";
      }
      // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
      // which could be wrong assumption. We need to implement more flrxible parsing here
      startFragmentIndex = Int32.Parse(htmlDataString.Substring(startFragmentIndex + "StartFragment:".Length, 10));
      if (startFragmentIndex < 0 || startFragmentIndex > htmlDataString.Length)
      {
        return "ERROR: Unrecognized html header";
      }

      // Byte count from the beginning of the clipboard to the end of the fragment.
      int endFragmentIndex = htmlDataString.IndexOf("EndFragment:");
      if (endFragmentIndex < 0)
      {
        return "ERROR: Unrecognized html header";
      }
      // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
      // which could be wrong assumption. We need to implement more flrxible parsing here
      endFragmentIndex = Int32.Parse(htmlDataString.Substring(endFragmentIndex + "EndFragment:".Length, 10));
      if (endFragmentIndex > htmlDataString.Length)
      {
        endFragmentIndex = htmlDataString.Length;
      }

      // CF_HTML is entirely text format and uses the transformation format UTF-8
      byte[] bytes = Encoding.UTF8.GetBytes(htmlDataString);
      return Encoding.UTF8.GetString(bytes, startFragmentIndex, endFragmentIndex - startFragmentIndex);
    }
    #endregion

    #region Tag And Rename Parameter related methods

    /// <summary>
    ///   Convert the Label to a Parameter
    /// </summary>
    /// <param name = "label"></param>
    /// <returns></returns>
    public static string LabelToParameter(string label)
    {
      var parameter = string.Empty;

      switch (label)
      {
        case "Artist":
          parameter = "%artist%";
          break;

        case "Title":
          parameter = "%title%";
          break;

        case "Album":
          parameter = "%album%";
          break;

        case "Year":
          parameter = "%year%";
          break;

        case "TrackNumber":
          parameter = "%track%";
          break;

        case "TrackTotal":
          parameter = "%tracktotal%";
          break;

        case "DiscNumber":
          parameter = "%disc%";
          break;

        case "DiscTotal":
          parameter = "%disctotal%";
          break;

        case "Genre":
          parameter = "%genre%";
          break;

        case "AlbumArtist":
          parameter = "%albumartist%";
          break;

        case "Comment":
          parameter = "%comment%";
          break;

        case "Conductor":
          parameter = "%conductor%";
          break;

        case "Composer":
          parameter = "%composer%";
          break;

        case "Remixed":
          parameter = "%remixed%";
          break;

        case "Bpm":
          parameter = "%bpm%";
          break;

        case "Subtitle":
          parameter = "%subtitle";
          break;

        case "Group":
          parameter = "%group%";
          break;

        case "FileName":
          parameter = "%filename%";
          break;

        case "Enumerate":
          parameter = "%#%";
          break;

        case "Bitrate":
          parameter = "%bitrate%";
          break;

        case "FirstArtist":
          parameter = "%artist:n%";
          break;

        case "FirstAlbumArtist":
          parameter = "%albumartist:n%";
          break;

        case "Unused":
          parameter = "%x%";
          break;

        case "Folder":
          parameter = @"\";
          break;
      }
      return parameter;
    }

    /// <summary>
    ///   Convert the Parameter to a Label
    /// </summary>
    /// <param name = "parameter"></param>
    /// <returns></returns>
    public static string ParameterToLabel(string parameter)
    {
      var label = string.Empty;

      switch (parameter)
      {
        case "%artist%":
          label = "Artist";
          break;

        case "%title%":
          label = "Title";
          break;

        case "%album%":
          label = "Album";
          break;

        case "%year%":
          label = "Year";
          break;

        case "%track%":
          label = "Track";
          break;

        case "%tracktotal%":
          label = "TrackCount";
          break;

        case "%disc%":
          label = "Disc";
          break;

        case "%disctotal%":
          label = "DiscCount";
          break;

        case "%genre%":
          label = "Genre";
          break;

        case "%albumartist%":
          label = "AlbumArtist";
          break;

        case "%comment%":
          label = "Comment";
          break;

        case "%conductor%":
          label = "Conductor";
          break;

        case "%composer%":
          label = "Composer";
          break;

        case "%remixed%":
          label = "Interpreter";
          break;

        case "%bpm%":
          label = "Bpm";
          break;

        case "%subtitle%":
          label = "Subtitle";
          break;

        case "%group%":
          label = "Grouping";
          break;

        case "%filename%":
          label = "Filename";
          break;

        case "%#%":
          label = "Enumerate";
          break;

        case "%bitrate%":
          label = "Bitrate";
          break;

        case "%artist:n%":
          label = "FirstArtist";
          break;

        case "%albumartist:n%":
          label = "FirstAlbumArtist";
          break;

        case "%x%":
          label = "Unused";
          break;

        case @"\":
          label = "Folder";
          break;
      }
      return label;
    }

    /// <summary>
    ///   Replace the given Parameter string with the values from the Track
    /// </summary>
    /// <param name = "parameter"></param>
    /// <param name = "song"></param>
    /// <returns></returns>
    public static string ReplaceParametersWithTrackValues(string parameter, SongData song)
    {
      string replacedString = parameter.Trim(new[] { '\\' });

      try
      {
        if (replacedString.IndexOf("%artist%") > -1)
          replacedString = replacedString.Replace("%artist%", song.Artist.Replace(';', '_').Trim());

        if (replacedString.IndexOf("%title%") > -1)
          replacedString = replacedString.Replace("%title%", song.Title.Trim());

        if (replacedString.IndexOf("%album%") > -1)
          replacedString = replacedString.Replace("%album%", song.Album.Trim());

        if (replacedString.IndexOf("%year%") > -1)
          replacedString = replacedString.Replace("%year%", song.Year.ToString().Trim());

        if (replacedString.IndexOf("%track%") > -1)
        {
          replacedString = replacedString.Replace("%track%", song.TrackNumber.ToString().PadLeft(2, '0'));
        }

        if (replacedString.IndexOf("%tracktotal%") > -1)
        {
          replacedString = replacedString.Replace("%tracktotal%", song.TrackCount.ToString().PadLeft(2, '0'));
        }

        if (replacedString.IndexOf("%disc%") > -1)
        {
          replacedString = replacedString.Replace("%disc%", song.DiscNumber.ToString().PadLeft(2, '0'));
        }

        if (replacedString.IndexOf("%disctotal%") > -1)
        {
          replacedString = replacedString.Replace("%disctotal%", song.DiscCount.ToString().PadLeft(2, '0'));
        }

        if (replacedString.IndexOf("%genre%") > -1)
        {
          var strGenre = song.Genre.Replace(';', '_');
          replacedString = replacedString.Replace("<G>", strGenre.Trim());
        }

        if (replacedString.IndexOf("%albumartist%") > -1)
          replacedString = replacedString.Replace("%albumartist%", song.AlbumArtist.Replace(';', '_').Trim());

        if (replacedString.IndexOf("%comment%") > -1)
          replacedString = replacedString.Replace("%comment%", song.Comment.Trim());

        if (replacedString.IndexOf("%group%") > -1)
          replacedString = replacedString.Replace("%group%", song.Grouping.Trim());

        if (replacedString.IndexOf("%conductor%") > -1)
          replacedString = replacedString.Replace("%conductor%", song.Conductor.Trim());

        if (replacedString.IndexOf("%composer%") > -1)
          replacedString = replacedString.Replace("%composer%", song.Composer.Replace(';', '_').Trim());

        if (replacedString.IndexOf("%subtitle%") > -1)
          replacedString = replacedString.Replace("%subtitle%", song.SubTitle.Trim());

        if (replacedString.IndexOf("%bpm%") > -1)
          replacedString = replacedString.Replace("%bpm%", song.BPM.ToString());

        if (replacedString.IndexOf("%remixed%") > -1)
          replacedString = replacedString.Replace("%remixed%", song.Interpreter.Trim());

        if (replacedString.IndexOf("%bitrate%") > -1)
          replacedString = replacedString.Replace("%bitrate%", song.BitRate);

        int index = replacedString.IndexOf("%artist:");
        if (index > -1)
        {
          replacedString = ReplaceStringWithLengthIndicator(index, replacedString, song.Artist.Replace(';', '_').Trim());
        }

        index = replacedString.IndexOf("%albumartist:");
        if (index > -1)
        {
          replacedString = ReplaceStringWithLengthIndicator(index, replacedString,
                                                            song.AlbumArtist.Replace(';', '_').Trim());
        }

        index = replacedString.IndexOf("%track:");
        if (index > -1)
        {
          replacedString = ReplaceStringWithLengthIndicator(index, replacedString, song.TrackNumber.ToString());
        }

        index = replacedString.IndexOf("%tracktotal:");
        if (index > -1)
        {
          replacedString = ReplaceStringWithLengthIndicator(index, replacedString, song.TrackCount.ToString());
        }

        index = replacedString.IndexOf("%disc:");
        if (index > -1)
        {
          replacedString = ReplaceStringWithLengthIndicator(index, replacedString, song.DiscNumber.ToString());
        }

        index = replacedString.IndexOf("%disctotal:");
        if (index > -1)
        {
          replacedString = ReplaceStringWithLengthIndicator(index, replacedString, song.DiscCount.ToString());
        }

        // Empty Values would create invalid folders
        replacedString = replacedString.Replace(@"\\", @"\_\");

        // If the directory name starts with a backslash, we've got an empty value on the beginning
        if (replacedString.IndexOf("\\") == 0)
          replacedString = "_" + replacedString;

        // We might have an empty value on the end of the path, which is indicated by a slash. 
        // replace it with underscore
        if (replacedString.LastIndexOf("\\") == replacedString.Length - 1)
          replacedString += "_";

        replacedString = MakeValidFolderName(replacedString);
      }
      catch (Exception)
      {
        return "";
      }
      return replacedString;
    }

    private static string ReplaceStringWithLengthIndicator(int startIndex, string replaceString, string replaceValue)
    {
      // Check if we have a numeric Parameter as replace value
      var numberParameters = new string[] { "%t", "%d" };
      var isNumericParm = Array.IndexOf(numberParameters, replaceString.Substring(startIndex, 2), startIndex) == 0;
      var last = replaceString.IndexOf("%", startIndex + 1);
      var s1 = replaceString.Substring(startIndex, last - startIndex + 1);
      var s2 = replaceString.Substring(replaceString.IndexOf(":") + 1, last - replaceString.IndexOf(":") - 1);

      int.TryParse(s2, out int strLength);

      if (replaceValue.Length >= strLength)
      {
        replaceValue = isNumericParm ? replaceValue.Substring(replaceValue.Length - strLength) : replaceValue.Substring(0, strLength);
      }
      else if (isNumericParm && replaceValue.Length < strLength)
      {
        // Do Pad numeric values with zeroes
        replaceValue = replaceValue.PadLeft(strLength, '0');
      }

      return replaceString.Replace(s1, replaceValue);
    }

    /// <summary>
    ///   Check the Parameter Format for validity
    /// </summary>
    /// <param name = "str"></param>
    /// <param name = "formattype"></param>
    /// <returns></returns>
    public static bool CheckParameterFormat(string str, Options.ParameterFormat formattype)
    {
      if (formattype == Options.ParameterFormat.FileNameToTag || formattype == Options.ParameterFormat.TagToFileName ||
          formattype == Options.ParameterFormat.RipFileName || formattype == Options.ParameterFormat.Organise)
      {
        str = str.Replace("%artist%", "\x0001");
        str = str.Replace("%title%", "\x0001");
        str = str.Replace("%album%", "\x0001");
        str = str.Replace("%genre%", "\x0001");
        str = str.Replace("%year%", "\x0001");
        str = str.Replace("%albumartist%", "\x0001");
        str = str.Replace("%track%", "\x0001");
      }

      if (formattype == Options.ParameterFormat.FileNameToTag || formattype == Options.ParameterFormat.TagToFileName ||
          formattype == Options.ParameterFormat.Organise)
      {
        str = str.Replace("%comment%", "\x0001");
        str = str.Replace("%disc%", "\x0001");
        str = str.Replace("%disctotal%", "\x0001");
        str = str.Replace("%tracktotal%", "\x0001");
        str = str.Replace("%group%", "\x0001");
        str = str.Replace("%conductor%", "\x0001");
        str = str.Replace("%composer%", "\x0001");
        str = str.Replace("%subtitle%", "\x0001");
        str = str.Replace("%bpm%", "\x0001");
        str = str.Replace("%remixed%", "\x0001");
      }

      if (formattype == Options.ParameterFormat.TagToFileName)
      {
        str = str.Replace("%filename%", "\x0001");
        str = str.Replace("%#%", "\x0001");

        int index = str.IndexOf("%track:");
        if (index > -1 && !CheckParmWithLengthIndicator(index, str, out str))
        {
          return false;
        }

        index = str.IndexOf("%tracktotal:");
        if (index > -1 && !CheckParmWithLengthIndicator(index, str, out str))
        {
          return false;
        }

        index = str.IndexOf("%disc:");
        if (index > -1 && !CheckParmWithLengthIndicator(index, str, out str))
        {
          return false;
        }

        index = str.IndexOf("%disctotal:");
        if (index > -1 && !CheckParmWithLengthIndicator(index, str, out str))
        {
          return false;
        }
      }

      if (formattype == Options.ParameterFormat.Organise)
      {
        str = str.Replace("%bitrate%", "\x0001");

        int index = str.IndexOf("%artist:");
        if (index > -1 && !CheckParmWithLengthIndicator(index, str, out str))
        {
          return false;
        }

        index = str.IndexOf("%albumartist:");
        if (index > -1 && !CheckParmWithLengthIndicator(index, str, out str))
        {
          return false;
        }
      }

      if (formattype == Options.ParameterFormat.FileNameToTag)
      {
        str = str.Replace("%x%", "\x0001");
      }

      if (str.IndexOf("%") >= 0 || (str.IndexOf("\x0001\x0001") >= 0 &&
                                     formattype == Options.ParameterFormat.FileNameToTag))
        return false;

      if (str.Length == 0)
      {
        return false;
      }

      return true;
    }

    /// <summary>
    ///   Check a Parameter with a given Length Indicator for correctness
    /// </summary>
    /// <param name = "startIndex"></param>
    /// <param name = "str"></param>
    /// <param name = "parmString"></param>
    /// <returns></returns>
    private static bool CheckParmWithLengthIndicator(int startIndex, string str, out string parmString)
    {
      var retVal = true;
      var last = str.IndexOf("%", startIndex + 1, StringComparison.Ordinal);
      var s1 = str.Substring(startIndex, last - startIndex + 1);
      var s2 = str.Substring(str.IndexOf(":") + 1, last - str.IndexOf(":") - 1);

      if (!int.TryParse(s2, out int n))
      {
        retVal = false;
      }

      parmString = str.Replace(s1, "\x0001");
      return retVal;
    }

    #endregion

    #region Database related methods
    
    /// <summary>
    ///   Changes the Quote to a double Quote, to have correct SQL Syntax
    /// </summary>
    /// <param name = "strText"></param>
    /// <returns></returns>
    public static string RemoveInvalidChars(string strText)
    {
      if (strText == null)
      {
        return "";
      }
      return strText.Replace("'", "''").Trim();
    }

    /// <summary>
    /// Escape Database queries to have the correct syntax
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static string EscapeDatabaseQuery(string query)
    {

      return query;

      // Seems to be not needed with LiteDB. Just leave it in, so that it can be re-activated when needed

      /*
      var literal = new StringBuilder(query.Length);
      foreach (var c in query)
      {
        switch (c)
        {
          // && || 
          case '+':
            literal.Append(@"\+");
            break;
          case '!':
            literal.Append(@"\!");
            break;
          case '(':
            literal.Append(@"\(");
            break;
          case ')':
            literal.Append(@"\)");
            break;
          case '}':
            literal.Append(@"\}");
            break;
          case '{':
            literal.Append(@"\{");
            break;
          case '[':
            literal.Append(@"\[");
            break;
          case ']':
            literal.Append(@"\]");
            break;
          case '^':
            literal.Append(@"\^");
            break;
          case '~':
            literal.Append(@"\~");
            break;
          case '*':
            literal.Append(@"\*");
            break;
          case '?':
            literal.Append(@"\?");
            break;
          case ':':
            literal.Append(@"\:");
            break;
          case '\\':
            literal.Append(@"\\");
            break;

          case '"':
            literal.Append("\"");
            break;
          default:
            if (Char.GetUnicodeCategory(c) != UnicodeCategory.Control)
            {
              literal.Append(c);
            }
            else
            {
              literal.Append(@"\u");
              literal.Append(((ushort) c).ToString("x4"));
            }
            break;
        }
      }
      return literal.ToString();
      */
    }


    #endregion

    #region CD Related Methods

    /// <summary>
    ///   Converts the given CD/DVD Drive Letter to a number suiteable for BASS
    /// </summary>
    /// <param name = "driveLetter"></param>
    /// <returns></returns>
    public static int Drive2BassID(char driveLetter)
    {
      BASS_CD_INFO cdinfo = new BASS_CD_INFO();
      for (int i = 0; i < 25; i++)
      {
        if (BassCd.BASS_CD_GetInfo(i, cdinfo))
        {
          if (cdinfo.DriveLetter == driveLetter)
            return i;
        }
      }
      return -1;
    }

    #endregion
  }
}
