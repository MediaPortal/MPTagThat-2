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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CommonServiceLocator;
using MPTagThat.Core.Common;
using MPTagThat.Core.Services.Logging;
using MPTagThat.Core.Services.Settings;
using MPTagThat.Core.Services.Settings.Setting;
using Syncfusion.UI.Xaml.Grid;
using TagLib;
using File = TagLib.File;
using Tag = TagLib.Id3v2.Tag;

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

    #endregion

    #region ctor

    private Util()
    {
      log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger;
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
    /// The standard ID3 Frames directly supported by TagLib #
    /// </summary>
    public static string[] StandardFrames { get; } = new[]
    {
      "TPE1", "TPE2", "TALB", "TBPM", "COMM", "TCOM",
      "TPE3", "TCOP", "TPOS", "TCON", "TIT1", "USLT", "APIC",
      "POPM", "TIT2", "TRCK", "TYER", "TDRC"
    };

    /// <summary>
    /// The extended ID3 Frames 
    /// </summary>
    public static string[] ExtendedFrames { get; } = new[]
    {
      "TSOP", "TSOA", "WCOM", "WCOP", "TENC", "TPE4", "TIPL",
      "IPLS", "TMED", "TMCL", "WOAF", "WOAR", "WOAS", "WORS",
      "WPAY", "WPUB", "TOAL", "TOFN", "TOLY", "TOPE", "TOWN",
      "TDOR", "TORY", "TPUB", "TIT3", "TEXT","TSOT", "TLEN",
      "TCMP", "TSO2"
    };

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
          //((GridImageColumn)column) = new Bitmap(1, 1);   // Default empty Image
          break;

        case "process":
          column = new GridPercentColumn();
          break;

        case "check":
          column = new GridCheckBoxColumn();
          break;

        case "rating":
          //column = new DataGridViewRatingColumn();
          column = new GridTextColumn();
          break;

        default:
          column = new GridTextColumn();
          break;
      }

      column.HeaderText = setting.Title;
      column.IsReadOnly = setting.Readonly;
      column.IsHidden = !setting.Display;
      column.Width = setting.Width;
      //column.IsFrozen = setting.Frozen;
      // For columns bound to a data Source set the property
      //if (setting.Bound)
      //{
        column.MappingName = setting.Name;
      //}

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

    #endregion

    #region Tag related methods

    /// <summary>
    /// Checks, if we got a replaygain frame
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    public static bool IsReplayGain(string description)
    {
      switch (description.ToLowerInvariant())
      {
        case "replaygain_track_gain":
        case "replaygain_track_peak":
        case "replaygain_album_gain":
        case "replaygain_album_peak":
          return true;
      }
      return false;
    }

    /// <summary>
    ///   Based on the Options set, use the correct version for ID3
    ///   Eventually remove V1 or V2 tags, if set in the options
    /// </summary>
    /// <param name = "File"></param>
    public static File FormatID3Tag(File file)
    {
      if (file.MimeType == "taglib/mp3")
      {
        var options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager)?.GetOptions;
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
        case ".png":
          return true;
      }
      return false;
    }

    #endregion

    #region Folder related methods

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

    #endregion
  }
}
