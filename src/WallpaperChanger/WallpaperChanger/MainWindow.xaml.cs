using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

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
        int TimerMinutes = 30;

        public MainWindow()
        {
            rs = new RegSettings("WallpaperChanger2");

            InitializeComponent();
            DataContext = this;

            SetupTimer();
            SetupNotifyIcon();

            SetupWallpaper(false);
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

        void SetupWallpaper(bool Update = false)
        {
            switch (rs.GetValue("Source", 0))
            {
                default:
                case 0:
                    SetupWallpaperBing(Update);
                    break;
            }
        }
        async void SetupWallpaperBing(bool Update = false)
        {
            if(!GetConnection())
            {
                //msg
                return;
            }

            var imgData = await Core.Source.Bing.Bing.Get(w, h);

            if (CheckDate() || Update)
                if (rs.GetValue("ImageUID", "") != imgData.Item1)
                {
                    WriteDate();
                    SetupImage(new Uri(imgData.Item1, UriKind.RelativeOrAbsolute), rs.GetValue<Core.Style>("WallpaperStyle"));
                    rs["ImageUID"] = "";

                    try
                    {
                        imgImageThumb.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { imgImageThumb.Source = new BitmapImage(new Uri(imgData.Item2)); });
                        tbImageInfo.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { tbImageInfo.Text = imgData.Item3; });
                    }
                    catch
                    {
                        imgImageThumb.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { imgImageThumb.Source = new BitmapImage(new Uri("www.bing.com/az/hprichbg/rb/OchaBatake_ROW10481280883_400x240.jpg")); });
                        tbImageInfo.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { tbImageInfo.Text = "Error"; });
                    }
                }
        }
        void WriteDate()
        {
            var dt = DateTime.Now.Add(GetTimeSpan(rs.GetValue<int>("Timetable")));

            rs["UpdateWallpaperYear"] = dt.Year;
            rs["UpdateWallpaperMounth"] = dt.Month;
            rs["UpdateWallpaperDay"] = dt.Day;
            rs["UpdateWallpaperHour"] = dt.Hour;
        }
        void SetupImage(Uri uri, Core.Style style)
        {
            Stream s = new WebClient().OpenRead(uri.ToString());

            System.Drawing.Image img = System.Drawing.Image.FromStream(s);
            string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            switch (style)
            {
                case Core.Style.Tiled:
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                    break;
                case Core.Style.Centered:
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
                default:
                case Core.Style.Stretched:
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        bool CheckDate()
        {
            return DateTime.Now > new DateTime(rs.GetValue("UpdateWallpaperYear", 2000),
                                       rs.GetValue("UpdateWallpaperMounth", 1),
                                       rs.GetValue("UpdateWallpaperDay", 1),
                                       rs.GetValue("UpdateWallpaperHour", 1), 0, 0);
        }
        TimeSpan GetTimeSpan(int timetable)
        {
            TimeSpan ts;

            switch (timetable)
            {
                case 0:
                    ts = new TimeSpan(0, 6, 0, 0);         //6 hour
                    break;
                case 1:
                    ts = new TimeSpan(0, 12, 0, 0);         //12 hour
                    break;
                case 2:
                default:
                    ts = new TimeSpan(1, 0, 0, 0);          //1 day
                    break;
                case 3:
                    ts = new TimeSpan(2, 0, 0, 0);          //2 days
                    break;
                case 4:
                    ts = new TimeSpan(3, 0, 0, 0);          //3 days
                    break;
                case 5:
                    ts = new TimeSpan(7, 0, 0, 0);          //7 days
                    break;
                case 6:
                    ts = new TimeSpan(14, 0, 0, 0);         //14 dasy
                    break;
                case 7:
                    ts = new TimeSpan(21, 0, 0, 0);         //21 days
                    break;
                case 8:
                    ts = new TimeSpan(30, 0, 0, 0);         //30 days
                    break;
            }

            return ts;
        }
        void SetupTimer()
        {
            timer = new Timer(GetMilisec(TimerMinutes));
            timer.Elapsed += TimerElapsed;
            timer.Enabled = true;
        }
        void SetupNotifyIcon()
        {
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add("Open window", (sh, eh) => Show());
            contextMenu.MenuItems.Add("Open favorite", (sh, eh) => Show());
            contextMenu.MenuItems.Add("Open settings", (sh, eh) => Show());
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add("Refresh wallpaper", (sh, eh) => SetupWallpaper(true));
            contextMenu.MenuItems.Add("Leave this wallpaper for [2 days]", (sh, eh) => Show());
            contextMenu.MenuItems.Add("Add to favorite", (sh, eh) => Show());
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
        private void btnCloseWinodwClick(object sender, RoutedEventArgs e) => Hide();
        private void btnInfoClick(object sender, RoutedEventArgs e)
        {
            new MessageWindow("About", "Author: Verloka Vadim\nVersion: 18.05\nverloka.github.io\n(c) 2018", Core.MessageWindowIcon.Info, Core.MessageWindowIconColor.Orange).ShowDialog();
        }
        #endregion

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

        }
        private void windowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Visible = false;
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            SetupWallpaper(false);
        }
        private void btnRefreshClick(object sender, RoutedEventArgs e)
        {
            SetupWallpaper(true);
        }
    }
}
