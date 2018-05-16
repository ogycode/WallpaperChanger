using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Verloka.HelperLib.Localization;
using Verloka.HelperLib.Settings;

namespace WallpaperChanger
{
    public partial class MainWindow : Window
    {
        private static string ID = "WallpaperChanger2";
        private static string AUTHOR = "Verloka Vadim";
        private static string WEBSITE_AUTHOR = "verloka.github.io";
        private static string WEBSITE_APP = "changer.pp.ua";
        private static string VERSION = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Revision}";

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        public bool Startup
        {
            get
            {
                try
                {
                    var startupTask = Windows.ApplicationModel.StartupTask.GetAsync(ID).GetResults();

                    switch (startupTask.State)
                    {
                        default:
                        case Windows.ApplicationModel.StartupTaskState.Disabled:
                        case Windows.ApplicationModel.StartupTaskState.DisabledByUser:
                            return false;
                        case Windows.ApplicationModel.StartupTaskState.Enabled:
                            return true;
                    }
                }
                catch
                {
                    return false;
                }

                /*RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                return (string)key.GetValue("Wallpaper Changer 2") == null ? false : true;*/
            }
            set
            {
                try
                {
                    var startupTask = Windows.ApplicationModel.StartupTask.GetAsync(ID).GetResults();

                    if (value)
                        startupTask.RequestEnableAsync().GetResults();
                    else
                        startupTask.Disable();
                }
                catch { }

                /*RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (!value)
                    key.DeleteValue("Wallpaper Changer 2", false);
                else
                    key.SetValue("Wallpaper Changer 2", $@"{"\""}{Directory.GetCurrentDirectory()}\WallpaperChanger.exe{"\""} -sl");*/
            }
        }
        public int Source
        {
            get => rs.GetValue("Source", 0);
            set => rs.SetValue("Source", value);
        }
        public int Timetable
        {
            get => rs.GetValue("Timetable", 2);
            set => rs.SetValue("Timetable", value);
        }

        RegSettings rs;
        Manager Lang;
        Timer timer;
        System.Windows.Forms.NotifyIcon notifyIcon;

        double w = SystemParameters.VirtualScreenWidth;
        double h = SystemParameters.VirtualScreenHeight;
        int TimerMinutes = 30;

        public MainWindow()
        {
            rs = new RegSettings("WallpaperChanger2");
            Lang = new Manager($@"{AppDomain.CurrentDomain.BaseDirectory}\Data\locales.ini");
            Lang.Load();
            Lang.SetCurrent(rs.GetValue("LanguageCode", "en-us"));
            Lang.LanguageChanged += LangLanguageChanged;

            InitializeComponent();
            DataContext = this;

            SetupTimer();
            SetupNotifyIcon();

            SetupWallpaper(false);
        }

        bool GetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("https://www.google.com"))
                    return true;
            }
            catch { return false; }
        }
        Task SetupLocale()
        {
            return Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    tbSourceName.Text = Lang["Source"];
                    tbLanguage.Text = Lang["tbLanguage"];
                    tbTimetable.Text = Lang["tbTimetable"];

                    cbiTimetable0.Content = Lang["cbiTimetable0"];
                    cbiTimetable1.Content = Lang["cbiTimetable1"];
                    cbiTimetable2.Content = Lang["cbiTimetable2"];
                    cbiTimetable3.Content = Lang["cbiTimetable3"];
                    cbiTimetable4.Content = Lang["cbiTimetable4"];
                    cbiTimetable5.Content = Lang["cbiTimetable5"];
                    cbiTimetable6.Content = Lang["cbiTimetable6"];
                    cbiTimetable7.Content = Lang["cbiTimetable7"];
                    cbiTimetable8.Content = Lang["cbiTimetable8"];

                    cbStartup.Content = Lang["cbStartup"];

                    cbBing.Content = Lang["cbBing"];

                    tbBtnRefresh.Text = Lang["btnRefresh"];

                    tiCurrent.Header = Lang["tiCurrent"];
                    tiFavorite.Header = Lang["tiFavorite"];
                    tiOptions.Header = Lang["tiOptions"];
                }));
            });
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
            if (!GetConnection())
            {
                //msg
                return;
            }

            var imgData = await Core.Source.Bing.Bing.Get(w, h);

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

            if (CheckDate() || Update)
                if (rs.GetValue("ImageUID", "") != imgData.Item1)
                {
                    WriteDate();
                    SetupImage(new Uri(imgData.Item1, UriKind.RelativeOrAbsolute), rs.GetValue<Core.Style>("WallpaperStyle"));
                    rs["ImageUID"] = imgData.Item1;
                    rs["ImageUIDThumbnail"] = imgData.Item2;
                }
        }
        void WriteDate()
        {
            var dt = DateTime.Now.Add(GetTimeSpan(Timetable));

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
        void SetupNotifyIcon(bool Update = false)
        {
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add(Lang["nfOpenWindow"], (sh, eh) => { Show(); tiCurrent.IsSelected = true; });
            contextMenu.MenuItems.Add(Lang["nfOpenFavorite"], (sh, eh) => { Show(); tiFavorite.IsSelected = true; });
            contextMenu.MenuItems.Add(Lang["nfOpenSettings"], (sh, eh) => { Show(); tiOptions.IsSelected = true; });
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(Lang["nfRefresh"], (sh, eh) => SetupWallpaper(true));
            contextMenu.MenuItems.Add(Lang["nfLeave"], (sh, eh) => Show());
            contextMenu.MenuItems.Add(Lang["nfAddFav"], (sh, eh) => Show());
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(Lang["nfExit"], (sh, eh) => Close());

            if (Update)
                notifyIcon.ContextMenu = contextMenu;
            else
            {
                notifyIcon = new System.Windows.Forms.NotifyIcon
                {
                    Text = "Wallpaper Changer 2",
                    Icon = Properties.Resources.photo,
                    Visible = true,
                    ContextMenu = contextMenu
                };

                notifyIcon.DoubleClick += (sh, eh) => Show();
            }
        }
        double GetMilisec(int minunte)
        {
            return 60000 * minunte;
        }
        async void GetTaskStartup()
        {
           // Windows.ApplicationModel.StartupTask startupTask = await Windows.ApplicationModel.StartupTask.GetForCurrentPackageAsync();

            //return startupTask;
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
            new MessageWindow("About", $"{Lang["Author"]}: \t    {AUTHOR} ({WEBSITE_AUTHOR})\n" +
                                       $"{Lang["Version"]}: \t    {VERSION}\n" +
                                       $"{Lang["AppWebsite"]}: \t    {WEBSITE_APP}\n\n" +
                                       $"{Lang["Copyright"]} © {AUTHOR}, {DateTime.Now.Year}", Core.MessageWindowIcon.Info, Core.MessageWindowIconColor.Orange, 840, 300).ShowDialog();
        }
        #endregion

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            await SetupLocale();

            cbLanguages.ItemsSource = Lang.AvailableLanguages;

            try
            {
                cbLanguages.SelectedItem = (from Language item in cbLanguages.Items
                                            where (item as Language).Code == rs.GetValue<string>("LanguageCode")
                                            select item).Single();
            }
            catch { }

            cbLanguages.SelectionChanged += CbLanguagesSelectionChanged;
        }

        private void windowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Visible = false;
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            SetupWallpaper(false);
        }
        private void btnRefreshClick(object sender, RoutedEventArgs e) => SetupWallpaper(true);
        private void CbLanguagesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLanguages.SelectedIndex == -1)
                return;

            rs["LanguageCode"] = (cbLanguages.SelectedItem as Language).Code;
            Lang.SetCurrent(rs.GetValue<string>("LanguageCode"));
        }
        private async void LangLanguageChanged(Manager obj)
        {
            await SetupLocale();
            SetupNotifyIcon(true);
        }
        private void btnTimetableHelpClick(object sender, RoutedEventArgs e) => new MessageWindow(Lang["MsgInfoTitle"], Lang["MsgInfoTimtable"], Core.MessageWindowIcon.Info, Core.MessageWindowIconColor.Blue).ShowDialog();
    }
}
