using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MPTagThat.Dialogs.Views
{
    /// <summary>
    /// Interaction logic for OrganiseFilesView.xaml
    /// </summary>
    public partial class OrganiseFilesView : UserControl
    {
        public OrganiseFilesView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Ignore characters, which would result in invalid filenames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
          switch (e.Text)
          {
            case "|":
            case "\"":
            case "/":
            case "*":
            case "?":
            case ":":
              e.Handled = true;
              break;
          }
        }
    }
}
