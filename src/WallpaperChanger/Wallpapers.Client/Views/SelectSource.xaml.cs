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

namespace Wallpapers.Client.Views
{
    public partial class SelectSource : Page
    {
        public SelectSource()
        {
            InitializeComponent();
        }

        void Bing()
        {

        }

        private void cbSourcesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbSources.SelectedIndex)
            {
                case 0:
                    tbInfo.Text = "Get daily picture from bing.com, absolutely free. This image updates about once in 24 hours.";
                    btnNext.Content = "Setup";
                    break;
                case -1:
                default:
                    tbInfo.Text = string.Empty;
                    btnNext.Content = "Next";
                    break;
            }
        }
        private void btnNextClick(object sender, RoutedEventArgs e)
        {
            switch (cbSources.SelectedIndex)
            {
                case 0:
                    Bing();
                    break;
                case -1:
                default:
                    break;
            }
        }
    }
}
