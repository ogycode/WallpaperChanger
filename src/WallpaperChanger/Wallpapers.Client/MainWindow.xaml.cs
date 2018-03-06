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
using Wallpapers.Client.Core;

namespace Wallpapers.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Window Events
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch { }
        }
        /*private void btnCloseClick()
        {

        }
        private void btnMinimazeClick()
        {

        }
        private void btnHelpClick()
        {

        }*/
        #endregion

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            SetupService.Initialize();


            frame.Navigate(new Uri("Views/SelectSource.xaml", UriKind.Relative));
        }
        private void btnCloseWinodwClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
