using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTagThat.Core.Settings
{
  public sealed class Options
  {
    public static string ConfigDir
    {
      get { return $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\MPTagThat\Config"; }
    }
  }
}
