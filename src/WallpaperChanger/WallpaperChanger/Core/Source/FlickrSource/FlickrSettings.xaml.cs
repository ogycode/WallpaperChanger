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
using System.Windows.Shapes;
using WallpaperChanger.Controlls;

namespace WallpaperChanger.Core.Source.FlickrSource
{
    public partial class FlickrSettings : Window
    {
        public FlickrSettings()
        {
            InitializeComponent();
        }

        #region Window Events
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch { }
        }
        private void btnCloseWinodwClick(object sender, RoutedEventArgs e) => Close();
        #endregion

        private void btnSpecialClick(bool isSwitched, string code)
        {
            if (isSwitched && code == "blackandwhite")
                foreach (var item in spColorButtons.Children)
                    if (item.GetType() == typeof(ColorSwitchButton))
                        (item as ColorSwitchButton).IsSwitched = false;
        }
        private void ColorButtonClicked(bool isSwitched, string code)
        {
            if (isSwitched)
                csbBlacnAndWhite.IsSwitched = false;
        }
        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in spColorButtons.Children)
                if (item.GetType() == typeof(ColorSwitchButton))
                    (item as ColorSwitchButton).ToolTip = (Application.Current.MainWindow as MainWindow).Lang[$"code_{(item as ColorSwitchButton).Code}"];

            foreach (var item in spSpecailas.Children)
                if (item.GetType() == typeof(ColorSwitchButton))
                    (item as ColorSwitchButton).ToolTip = (Application.Current.MainWindow as MainWindow).Lang[$"code_{(item as ColorSwitchButton).Code}"];

            Title = $"{(Application.Current.MainWindow as MainWindow).Lang["tiOptions"]} {(Application.Current.MainWindow as MainWindow).Lang["cbFlickr"]}";

            tbTagInfo.Text = (Application.Current.MainWindow as MainWindow).Lang["tbTagInfo"];

            tbAddTagLabel.Text = (Application.Current.MainWindow as MainWindow).Lang["tbAddTagLabel"];
            tbColorLabel.Text = (Application.Current.MainWindow as MainWindow).Lang["tbColorLabel"];
            tbSpecialLabel.Text = (Application.Current.MainWindow as MainWindow).Lang["tbSpecialLabel"];
            tbAcceptButton.Text = (Application.Current.MainWindow as MainWindow).Lang["tbAcceptButton"]; 
        }
    }
}
