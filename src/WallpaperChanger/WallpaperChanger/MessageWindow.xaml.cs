using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WallpaperChanger.Core;

namespace WallpaperChanger
{
    public partial class MessageWindow : Window
    {
        public MessageWindow(string title, string message, MessageWindowIcon icon, MessageWindowIconColor iconColor, double maxWidth = 640, double maxHeight = 250)
        {
            InitializeComponent();

            brd.MaxWidth = maxWidth;
            brd.MaxHeight = maxHeight;

            Title = title;
            tbMsg.Text = message;

            switch (icon)
            {
                default:
                case MessageWindowIcon.Info:
                    tbIcon.Text = Char.ConvertFromUtf32(0xE946);
                    break;
                case MessageWindowIcon.Warning:
                    tbIcon.Text = Char.ConvertFromUtf32(0xE7BA);
                    break;
                case MessageWindowIcon.Error:
                    tbIcon.Text = Char.ConvertFromUtf32(0xE783);
                    break;
            }

            switch (iconColor)
            {
                default:
                case MessageWindowIconColor.Blue:
                    tbIcon.Foreground = new SolidColorBrush(Color.FromRgb(0, 122, 204));
                    brd.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204));
                    break;
                case MessageWindowIconColor.Orange:
                    tbIcon.Foreground = new SolidColorBrush(Color.FromRgb(255, 204, 0));
                    brd.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 204, 0));
                    break;
                case MessageWindowIconColor.Red:
                    tbIcon.Foreground = new SolidColorBrush(Color.FromRgb(239, 83, 80));
                    brd.BorderBrush = new SolidColorBrush(Color.FromRgb(239, 83, 80));
                    break;
            }
        }

        #region Window Events
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch { }
        }
        private void btnCloseWinodwClick(object sender, RoutedEventArgs e) => Close();
        #endregion

        private void btnOKClick(object sender, RoutedEventArgs e) => Close();
    }
}
