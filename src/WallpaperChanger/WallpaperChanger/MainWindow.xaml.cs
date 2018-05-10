using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Verloka.HelperLib.Settings;

namespace WallpaperChanger
{
    public partial class MainWindow : Window
    {
        public bool Startup
        {
            get
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                return (string)key.GetValue("WallpaperChanger2") == null ? false : true;
            }
            set
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (!value)
                    key.DeleteValue("WallpaperChanger2", false);
                else
                    key.SetValue("WallpaperChanger2", $"{Directory.GetCurrentDirectory()}/WallpaperChanger.exe -sl");
            }
        }
        public int Source
        {
            get => rs.GetValue("Source", 0);
            set => rs.SetValue("Source", value);
        }

        RegSettings rs;
        Timer timer;
        System.Windows.Forms.NotifyIcon notifyIcon;

        double w = SystemParameters.VirtualScreenWidth;
        double h = SystemParameters.VirtualScreenHeight;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            rs = new RegSettings("WallpaperChanger2");

            SetupTimer();
            SetupNotifyIcon();
        }

        public bool GetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("https://www.google.com"))
                    return true;
            }
            catch { return false; }
        }

        void SetupTimer()
        {
            timer = new Timer(GetMilisec(1));
            timer.Elapsed += TimerElapsed;
            timer.Enabled = true;
        }
        void SetupNotifyIcon()
        {
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add("Show Sindow", (sh, eh) => Show());
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add("Exit", (sh, eh) => Close());

            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Text = "Wallpaper Changer 2",
                Icon = Properties.Resources.photo,
                Visible = true,
                ContextMenu = contextMenu
            };
        }
        double GetMilisec(int minunte)
        {
            return 60000 * minunte;
        }

        #region Window Events
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch { }
        }
        private void btnCloseWinodwClick(object sender, RoutedEventArgs e) => Close();
        private void btnInfoClick(object sender, RoutedEventArgs e)
        {
            //ShowModal(true, ModalWindowType.About)
        }
        #endregion

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

        }
        private void windowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Visible = false;
        }
        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            /*var c = await Core.Source.Bing.Bing.Get(w, h);
            //
            imgImageThumb.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { imgImageThumb.Source = new BitmapImage(new Uri(c.Item2)); });
            tbImageInfo.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { tbImageInfo.Text = c.Item3; });*/
        }
    }
}
