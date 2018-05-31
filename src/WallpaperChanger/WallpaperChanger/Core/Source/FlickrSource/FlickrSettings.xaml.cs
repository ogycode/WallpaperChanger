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
        public event Action FlickrSettingsClosed;

        public bool AllTags
        {
            get => (Application.Current.MainWindow as MainWindow).rs.GetValue(MainWindow.WALLPAPER_FLICKR_TAGS_ALL, false);
            set => (Application.Current.MainWindow as MainWindow).rs.SetValue(MainWindow.WALLPAPER_FLICKR_TAGS_ALL, value);
        }

        public FlickrSettings()
        {
            InitializeComponent();
            DataContext = this;
        }

        UIElement getItem(string tag)
        {
            foreach (var item in wpTags.Children)
                if ((item as TagControll).TagText == tag)
                    return item as UIElement;
            return null;
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

            cbAllTags.Content = (Application.Current.MainWindow as MainWindow).Lang["cbAllTags"];


            foreach (var tag in (Application.Current.MainWindow as MainWindow).rs.GetValue(MainWindow.WALLPAPER_FLICKR_TAGS, "nature").Split(','))
            {
                if (string.IsNullOrWhiteSpace(tag))
                    continue;

                var item = new TagControll()
                {
                    TagText = tag,
                    Margin = new Thickness(5)
                };

                item.Clicked += ItemClicked;

                wpTags.Children.Add(item);
            }

            foreach (var colors in (Application.Current.MainWindow as MainWindow).rs.GetValue(MainWindow.WALLPAPER_FLICKR_COLORS, "").Split(','))
                foreach (var item in spColorButtons.Children)
                    if (item.GetType() == typeof(ColorSwitchButton) && (item as ColorSwitchButton).Code == colors)
                        (item as ColorSwitchButton).IsSwitched = true;

            string styles = (Application.Current.MainWindow as MainWindow).rs.GetValue(MainWindow.WALLPAPER_FLICKR_STYLES, "0,0,0,0");

            csbBlacnAndWhite.IsSwitched = styles[0] == '1';
            csbdepthoffield.IsSwitched = styles[2] == '1';
            csbminimalism.IsSwitched = styles[4] == '1';
            csbpattern.IsSwitched = styles[6] == '1';
        }
        private void btnAddTagClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbAddTag.Text) || getItem(tbAddTag.Text) != null)
            {
                tbAddTag.Text = string.Empty;
                return;
            }

            var item = new TagControll()
            {
                TagText = tbAddTag.Text,
                Margin = new Thickness(5)
            };

            item.Clicked += ItemClicked;

            wpTags.Children.Add(item);

            tbAddTag.Text = string.Empty;
        }
        private void ItemClicked(string tag)
        {
            wpTags.Children.Remove(getItem(tag));
        }
        private void btnApplyClick(object sender, RoutedEventArgs e)
        {
            if (wpTags.Children.Count == 0)
                (Application.Current.MainWindow as MainWindow).rs[MainWindow.WALLPAPER_FLICKR_TAGS] = "nature";
            else
            {
                bool first = true;
                string str = "";

                foreach (var item in wpTags.Children)
                {
                    if (!first)
                        str += ",";
                    else
                        first = false;

                    str += (item as TagControll).TagText;
                }

                (Application.Current.MainWindow as MainWindow).rs[MainWindow.WALLPAPER_FLICKR_TAGS] = str;
            }

            bool firstColor = true;
            string strColor = "";

            foreach (var item in spColorButtons.Children)
            {
                if (item.GetType() == typeof(ColorSwitchButton))
                    if ((item as ColorSwitchButton).IsSwitched)
                    {
                        if (!firstColor)
                            strColor += ",";
                        else
                            firstColor = false;

                        strColor += (item as ColorSwitchButton).Code;
                    }
            }

            (Application.Current.MainWindow as MainWindow).rs[MainWindow.WALLPAPER_FLICKR_COLORS] = strColor;
            (Application.Current.MainWindow as MainWindow).rs[MainWindow.WALLPAPER_FLICKR_STYLES] = $"{(csbBlacnAndWhite.IsSwitched ? 1 : 0)},{(csbdepthoffield.IsSwitched ? 1 : 0)},{(csbminimalism.IsSwitched ? 1 : 0)},{(csbpattern.IsSwitched ? 1 : 0)}";

            FlickrSettingsClosed.Invoke();

            Close();
        }
    }
}
