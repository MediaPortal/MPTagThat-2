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
using System.IO;
using System.Xml;
using Microsoft.Practices.ServiceLocation;
using MPTagThat.Services.Logging;

#endregion

namespace MPTagThat.Services.Settings
{
  public class XmlSettingsProvider
  {
    #region Variables

    private readonly XmlDocument _document;
    private readonly string _filename;
    private bool _modified;

    #endregion

    public XmlSettingsProvider(string xmlfilename)
    {
      _filename = xmlfilename;
      var options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;
      var fullFileName = $@"{options.ConfigDir}\{xmlfilename}";
      
      _document = new XmlDocument();
      if (File.Exists(fullFileName))
      {
        _document.Load(fullFileName);
        if (_document.DocumentElement == null) _document = null;
      }
      else if (File.Exists(fullFileName + ".bak"))
      {
        _document.Load(fullFileName + ".bak");
        if (_document.DocumentElement == null) _document = null;
      }
      if (_document == null)
        _document = new XmlDocument();
    }

    public string FileName => _filename;

    public string GetValue(string section, string entry, SettingScope scope)
    {
      var root = _document?.DocumentElement;
      if (root == null) return null;
      XmlNode entryNode;
      if (scope == SettingScope.User)
        entryNode =
          root.SelectSingleNode(GetSectionPath(section) + "/" + GetScopePath(scope.ToString()) + "/" +
                                GetUserPath() + "/" + GetEntryPath(entry));
      else
        entryNode =
          root.SelectSingleNode(GetSectionPath(section) + "/" + GetScopePath(scope.ToString()) + "/" +
                                GetEntryPath(entry));
      return entryNode?.InnerText;
    }

    public void Save()
    {
      var options = (ServiceLocator.Current.GetInstance(typeof(ISettingsManager)) as ISettingsManager).GetOptions;
      var log = (ServiceLocator.Current.GetInstance(typeof(ILogger)) as ILogger).GetLogger;
      log.Trace($"Saving({_filename},{_modified})");
      if (!_modified) return;
      if (!Directory.Exists(options.ConfigDir))
        Directory.CreateDirectory(options.ConfigDir);

      var fullFilename = $@"{options.ConfigDir}\{_filename}";
      log.Trace($"Saving {fullFilename}");
      if (_document?.DocumentElement == null) return;
      if (_document.ChildNodes.Count == 0) return;
      try
      {
        if (File.Exists(fullFilename + ".bak")) File.Delete(fullFilename + ".bak");
        if (File.Exists(fullFilename)) File.Move(fullFilename, fullFilename + ".bak");
      }

      catch (Exception)
      {
        // ignored
      }

      using (StreamWriter stream = new StreamWriter(fullFilename, false))
      {
        _document.Save(stream);
        stream.Flush();
        stream.Close();
      }
      _modified = false;
    }

    public void SetValue(string section, string entry, string value, SettingScope scope)
    {
      // If the value is null, remove the entry
      if (value == null)
      {
        RemoveEntry(section, entry);
        return;
      }

      string valueString = value;

      if (_document.DocumentElement == null)
      {
        var node = _document.CreateElement("Configuration");
        _document.AppendChild(node);
      }
      var root = _document.DocumentElement;
      // Get the section element and add it if it's not there
      if (root != null)
      {
        var sectionNode = root.SelectSingleNode("Section[@name=\"" + section + "\"]");
        if (sectionNode == null)
        {
          var element = _document.CreateElement("Section");
          var attribute = _document.CreateAttribute("name");
          attribute.Value = section;
          element.Attributes.Append(attribute);
          sectionNode = root.AppendChild(element);
        }
        // Get the section element and add it if it's not there
        var scopeSectionNode = sectionNode.SelectSingleNode("Scope[@value=\"" + scope + "\"]");
        if (scopeSectionNode == null)
        {
          var element = _document.CreateElement("Scope");
          var attribute = _document.CreateAttribute("value");
          attribute.Value = scope.ToString();
          element.Attributes.Append(attribute);
          scopeSectionNode = sectionNode.AppendChild(element);
        }
        if (scope == SettingScope.User)
        {
          var userNode = scopeSectionNode.SelectSingleNode("User[@name=\"" + Environment.UserName + "\"]");
          if (userNode == null)
          {
            var element = _document.CreateElement("User");
            var attribute = _document.CreateAttribute("name");
            attribute.Value = Environment.UserName;
            element.Attributes.Append(attribute);
            userNode = scopeSectionNode.AppendChild(element);
          }
        }
        // Get the entry element and add it if it's not there
        XmlNode entryNode = null;
        if (scope == SettingScope.User)
        {
          var userNode = scopeSectionNode.SelectSingleNode("User[@name=\"" + Environment.UserName + "\"]");
          if (userNode != null) entryNode = userNode.SelectSingleNode("Setting[@name=\"" + entry + "\"]");
        }
        else entryNode = scopeSectionNode.SelectSingleNode("Setting[@name=\"" + entry + "\"]");

        if (entryNode == null)
        {
          XmlElement element = _document.CreateElement("Setting");
          XmlAttribute attribute = _document.CreateAttribute("name");
          attribute.Value = entry;
          element.Attributes.Append(attribute);
          if (scope == SettingScope.Global) entryNode = scopeSectionNode.AppendChild(element);
          else
          {
            var userNode = scopeSectionNode.SelectSingleNode("User[@name=\"" + Environment.UserName + "\"]");
            if (userNode != null) entryNode = userNode.AppendChild(element);
          }
        }
        if (entryNode != null) entryNode.InnerText = valueString;
      }
      _modified = true;
    }

    public void RemoveEntry(string section, string entry)
    {
      //todo
    }

    private string GetSectionPath(string section)
    {
      return "Section[@name=\"" + section + "\"]";
    }

    private string GetEntryPath(string entry)
    {
      return "Setting[@name=\"" + entry + "\"]";
    }

    private string GetScopePath(string scope)
    {
      return "Scope[@value=\"" + scope + "\"]";
    }

    private string GetUserPath()
    {
      return "User[@name=\"" + Environment.UserName + "\"]";
    }
  }

}
