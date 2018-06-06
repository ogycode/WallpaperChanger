using FlickrNet;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using WallpaperChanger.Core;
using Windows.Storage;
using Windows.System.UserProfile;

namespace WallpaperChanger
{
    public partial class MainWindow : Window
    {
        private static string ID = "WallpaperChanger2";
        private static string AUTHOR = "Verloka Vadim";
        private static string WEBSITE_AUTHOR = "verloka.github.io";
        private static string WEBSITE_APP = "changer.pp.ua";
        private static string VERSION = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Revision}";
        private static string FavoriteListPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\Data\favorites.json";

        public const string WALLPAPER_URL = "ImageUID";
        public const string WALLPAPER_THUMBNAIL = "ImageUIDThumbnail";
        public const string WALLPAPER_COPYRIGHT = "CurrentCopy";
        public const string WALLPAPER_FAVORITE_NUMBER = "FuvNum";
        public const string WALLPAPER_FLICKR_TAGS = "FlickrTags";
        public const string WALLPAPER_FLICKR_COLORS = "FlickrColors";
        public const string WALLPAPER_FLICKR_STYLES = "FlickrStyles";
        public const string WALLPAPER_FLICKR_TAGS_ALL = "FlickrTagsAll";

        public List<Favorite> FavoriteList { get; set; }
        public string FavoritePath = $@"{AppDomain.CurrentDomain.BaseDirectory}\fav";

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const int SPI_SETDESKWALLPAPER = 0x14;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

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
        public int WallpaperStyle
        {
            get => rs.GetValue("WallpaperStyle", 0);
            set => rs.SetValue("WallpaperStyle", value);
        }

        public RegSettings rs;
        public Manager Lang;

        Timer timer;
        System.Windows.Forms.NotifyIcon notifyIcon;
        Flickr flickr;

        double w = SystemParameters.VirtualScreenWidth;
        double h = SystemParameters.VirtualScreenHeight;
        int TimerMinutes = 30;

        public MainWindow()
        {
            try { rs = new RegSettings(ID); }
            catch (Exception e) { new MessageWindow("Error", e.Message, MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog(); Application.Current.Shutdown(1); }

            try
            {
                Lang = new Manager($@"{AppDomain.CurrentDomain.BaseDirectory}\Data\locales.ini");
                Lang.Load();
                Lang.SetCurrent(rs.GetValue("LanguageCode", "en-us"));
                Lang.LanguageChanged += LangLanguageChanged;
            }
            catch (Exception e) { new MessageWindow("Error", e.Message, MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog(); Application.Current.Shutdown(1); }

            LoadFavorite();

            InitializeComponent();
            DataContext = this;

            SetupTimer();
            SetupNotifyIcon();

            SetupWallpaper(true);
        }

        //wallpaper
        async void SetupWallpaper(bool Update = false)
        {
            btnRefresh.IsEnabled = false;

            switch (rs.GetValue("Source", 1))
            {
                case 3:
                    await SetupWallpaperUnsplash(Update);
                    break;
                case 2:
                    await SetupallpaperFlickr(Update);
                    break;
                default:
                case 1:
                    await SetupWallpaperBing(Update);
                    break;
                case 0:
                    await SetupWallpaperFavorite(Update);
                    break;
                case -1:
                    SetupWallpaperLeave(Update);
                    break;
            }
        }
        async Task SetupWallpaperUnsplash(bool Update = false)
        {
            Task.Factory.StartNew(() =>
            {
                if (CheckDate() || Update)
                {
                    var imgData = Core.Source.Unsplash.Unsplash.Get(w, h);

                    rs[WALLPAPER_URL] = imgData.Item1;
                    rs[WALLPAPER_THUMBNAIL] = imgData.Item2;
                    rs[WALLPAPER_COPYRIGHT] = Lang["UnsplashCopy"];

                    WriteDate();
                    WriteThumbCopy(imgData.Item1, Lang["UnsplashCopy"]);

                    SetupImage(new Uri(imgData.Item1, UriKind.RelativeOrAbsolute));
                }
                else
                    btnRefresh.IsEnabled = true;
            });
        }
        async Task SetupallpaperFlickr(bool Update = false)
        {
            Task.Factory.StartNew(() =>
            {
                if (CheckDate() || Update)
                {
                    var imgData = Core.Source.FlickrSource.Finder.FindAsync(w,
                                                                            h,
                                                                            rs.GetValue(WALLPAPER_URL, ""),
                                                                            rs.GetValue(WALLPAPER_FLICKR_TAGS, "nature"),
                                                                            rs.GetValue(WALLPAPER_FLICKR_TAGS_ALL, false) ? TagMode.AnyTag : TagMode.AllTags,
                                                                            GetFlickrColors(),
                                                                            GetFlickrStyles()).Result;

                    rs[WALLPAPER_URL] = imgData.Item1;
                    rs[WALLPAPER_THUMBNAIL] = imgData.Item2;
                    rs[WALLPAPER_COPYRIGHT] = imgData.Item3;

                    WriteDate();
                    WriteThumbCopy(imgData.Item2, imgData.Item3);

                    SetupImage(new Uri(imgData.Item1, UriKind.RelativeOrAbsolute));
                }
                else
                    btnRefresh.IsEnabled = true;
            });
        }
        async Task SetupWallpaperFavorite(bool Update)
        {
            Task.Factory.StartNew(() =>
            {
                if (FavoriteList.Count == 0)
                {
                    cbSource.SelectedIndex = 1;
                    SetupWallpaper(true);
                    return;
                }

                if (CheckDate() || Update)
                {
                    int number = rs.GetValue(WALLPAPER_FAVORITE_NUMBER, -1) + 1;
                    number = (number > FavoriteList.Count - 1) ? 0 : number;

                    rs[WALLPAPER_URL] = FavoriteList[number].Original;
                    rs[WALLPAPER_THUMBNAIL] = FavoriteList[number].Thumbnail;
                    rs[WALLPAPER_COPYRIGHT] = FavoriteList[number].Copyright;

                    rs[WALLPAPER_FAVORITE_NUMBER] = number;

                    WriteDate();
                    WriteThumbCopy(FavoriteList[number].Thumbnail, FavoriteList[number].Copyright);

                    SetupImage(new Uri(rs.GetValue<string>(WALLPAPER_URL), UriKind.RelativeOrAbsolute));
                }
                else
                    btnRefresh.IsEnabled = true;
            });
        }
        void SetupWallpaperLeave(bool Update)
        {
            if (!Update)
                return;

            WriteDate();
            WriteThumbCopy(rs.GetValue<string>(WALLPAPER_THUMBNAIL), rs.GetValue<string>(WALLPAPER_COPYRIGHT));

            SetupImage(new Uri(rs.GetValue<string>(WALLPAPER_URL), UriKind.RelativeOrAbsolute));
        }
        async Task SetupWallpaperBing(bool Update = false)
        {
            Task.Factory.StartNew(() =>
            {
                if (!GetConnection())
                {
                    //msg
                    btnRefresh.IsEnabled = true;
                    return;
                }

                var imgData = Core.Source.Bing.Bing.Get(w, h).Result;

                if (CheckDate() || Update)
                {
                    if (rs.GetValue(WALLPAPER_URL, "") != imgData.Item1 || Update)
                    {
                        rs[WALLPAPER_URL] = imgData.Item1;
                        rs[WALLPAPER_THUMBNAIL] = imgData.Item2;
                        rs[WALLPAPER_COPYRIGHT] = imgData.Item3;

                        WriteDate();
                        WriteThumbCopy(imgData.Item2, imgData.Item3);

                        SetupImage(new Uri(imgData.Item1, UriKind.RelativeOrAbsolute));
                    }
                }
                else
                    btnRefresh.IsEnabled = true;
            });
        }
        void WriteDate()
        {
            var dt = DateTime.Now.Add(GetTimeSpan(Timetable));

            rs["UpdateWallpaperYear"] = dt.Year;
            rs["UpdateWallpaperMounth"] = dt.Month;
            rs["UpdateWallpaperDay"] = dt.Day;
            rs["UpdateWallpaperHour"] = dt.Hour;
        }
        void WriteRegistry(Core.Style style)
        {
            //use switch
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            if (style == Core.Style.Fill)
            {
                key.SetValue(@"WallpaperStyle", 10.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Core.Style.Fit)
            {
                key.SetValue(@"WallpaperStyle", 6.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Core.Style.Span)
            {
                key.SetValue(@"WallpaperStyle", 22.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Core.Style.Stretch)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Core.Style.Tile)
            {
                key.SetValue(@"WallpaperStyle", 0.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }
            if (style == Core.Style.Center)
            {
                key.SetValue(@"WallpaperStyle", 0.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            key.Close();
        }
        void WriteThumbCopy(string thumb, string copy)
        {
            try
            {
                imgImageThumb.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { imgImageThumb.Source = new BitmapImage(new Uri(thumb)); });
                tbImageInfo.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { tbImageInfo.Text = copy; });
            }
            catch
            {
                imgImageThumb.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { imgImageThumb.Source = new BitmapImage(new Uri("www.bing.com/az/hprichbg/rb/OchaBatake_ROW10481280883_400x240.jpg")); });
                tbImageInfo.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { tbImageInfo.Text = "Error"; });
            }
        }
        async void SetupImage(Uri uri)
        {
            Stream s = new WebClient().OpenRead(uri.ToString());

            System.Drawing.Image img = System.Drawing.Image.FromStream(s);
            string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

            btnLike.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
            {
                btnLike.IsSwitched = (from i in FavoriteList
                                      where i.Original == rs.GetValue<string>(WALLPAPER_URL)
                                      select i).Count() > 0;
            });

            btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
            {
                btnRefresh.IsEnabled = true;
            });
        }

        //help
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
        double GetMilisec(int minunte)
        {
            return 60000 * minunte;
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
        void ShowToast(string title, string msg)
        {
            string xml = $@"<toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{title}</text>
                            <text>{msg}</text>
                        </binding>
                    </visual>
                </toast>";

            Windows.Data.Xml.Dom.XmlDocument doc = new Windows.Data.Xml.Dom.XmlDocument();
            doc.LoadXml(xml);

            Windows.UI.Notifications.ToastNotification toast = new Windows.UI.Notifications.ToastNotification(doc);
            Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        List<string> GetFlickrColors()
        {
            if (string.IsNullOrWhiteSpace(rs.GetValue(WALLPAPER_FLICKR_COLORS, "")))
                return null;

            return rs.GetValue<string>(WALLPAPER_FLICKR_COLORS).Split(',').ToList<string>();
        }
        List<FlickrNet.Style> GetFlickrStyles()
        {
            if (rs.GetValue(WALLPAPER_FLICKR_STYLES, "0,0,0,0") == "0,0,0,0")
                return null;

            var a = new List<FlickrNet.Style>();

            if (rs.GetValue<string>(WALLPAPER_FLICKR_STYLES)[0] == '1')
                a.Add(FlickrNet.Style.BlackAndWhite);

            if (rs.GetValue<string>(WALLPAPER_FLICKR_STYLES)[2] == '1')
                a.Add(FlickrNet.Style.DepthOfField);

            if (rs.GetValue<string>(WALLPAPER_FLICKR_STYLES)[4] == '1')
                a.Add(FlickrNet.Style.Minimalism);

            if (rs.GetValue<string>(WALLPAPER_FLICKR_STYLES)[6] == '1')
                a.Add(FlickrNet.Style.Pattern);

            return a;
        }

        //favorites
        void SaveFavorite()
        {
            try
            {
                if (File.Exists(FavoriteListPath))
                    File.Delete(FavoriteListPath);

                using (StreamWriter sw = File.CreateText(FavoriteListPath))
                    sw.Write(JsonConvert.SerializeObject(FavoriteList));
            }
            catch (Exception e) { new MessageWindow(Lang["ErrorTitle"], e.Message, MessageWindowIcon.Error, MessageWindowIconColor.Red).Show(); }
        }
        void LoadFavorite()
        {
            try
            {
                if (File.Exists(FavoriteListPath))
                    using (StreamReader sr = File.OpenText(FavoriteListPath))
                        FavoriteList = JsonConvert.DeserializeObject<List<Favorite>>(sr.ReadToEnd());
                else
                    FavoriteList = new List<Favorite>();

                foreach (var item in Directory.GetFiles(FavoritePath))
                    if (FavoriteList.FirstOrDefault((x) => x.ThumbnailLocale == item) == null)
                        try { File.Delete(item); }
                        catch { }
            }
            catch (Exception e) { new MessageWindow(Lang["ErrorTitle"], e.Message, MessageWindowIcon.Error, MessageWindowIconColor.Red).Show(); }
        }
        void LoadFavoriteToPanel()
        {
            try
            {
                wpFavorites.Children.Clear();

                foreach (var item in FavoriteList)
                {
                    Controlls.FavoritePic pf = new Controlls.FavoritePic()
                    {
                        Original = item.Original,
                        Wallpaper = item.ThumbnailLocale,
                        Copyright = item.Copyright,
                        Margin = new Thickness(7)
                    };

                    pf.ApplyEvent += PfApplyEvent;
                    pf.DeleteEvent += PfDeleteEvent;

                    wpFavorites.Children.Add(pf);
                }
            }
            catch (Exception e) { new MessageWindow(Lang["ErrorTitle"], e.Message, MessageWindowIcon.Error, MessageWindowIconColor.Red).Show(); }
        }
        string GetFavoritePath(string original)
        {
            original = original.Remove(0, original.Length - 10);


            return $@"{FavoritePath}\{original.Replace(":", "").Replace("/", "")}.jpg";
        }

        void LikeWallpaper()
        {
            if (!Directory.Exists(FavoritePath))
                Directory.CreateDirectory(FavoritePath);

            try
            {
                if (!File.Exists(GetFavoritePath(rs.GetValue<string>(WALLPAPER_URL))))
                    using (WebClient wc = new WebClient())
                        wc.DownloadFile(rs.GetValue<string>(WALLPAPER_THUMBNAIL), GetFavoritePath(rs.GetValue<string>(WALLPAPER_URL)));

                FavoriteList.Add(new Favorite()
                {
                    Original = rs.GetValue<string>(WALLPAPER_URL),
                    Thumbnail = rs.GetValue<string>(WALLPAPER_THUMBNAIL),
                    Copyright = rs.GetValue<string>(WALLPAPER_COPYRIGHT),
                    ThumbnailLocale = GetFavoritePath(rs.GetValue<string>(WALLPAPER_URL))
                });

                LoadFavoriteToPanel();
                btnLike.IsSwitched = true;

            }
            catch (Exception e)
            {
                ShowToast(Lang["ErrorTitle"], e.Message);
            }
        }
        void UnlikeWallpaper(string original = "")
        {
            var a = rs.GetValue<string>(WALLPAPER_URL);
            string path = string.IsNullOrWhiteSpace(original) ? rs.GetValue<string>(WALLPAPER_URL) : original;

            if (FavoriteList.RemoveAll((x) => x.Original == path) > 0)
            {
                LoadFavoriteToPanel();

                try { File.Delete(GetFavoritePath(path)); }
                catch { }

                btnLike.IsSwitched = false;
            }
        }

        //startup
        async void GetStartup()
        {
            var startupTask = await Windows.ApplicationModel.StartupTask.GetAsync(ID);
            switch (startupTask.State)
            {
                case Windows.ApplicationModel.StartupTaskState.Disabled:
                    cbStartup.IsChecked = false;
                    break;
                case Windows.ApplicationModel.StartupTaskState.DisabledByUser:
                    cbStartup.IsChecked = false;
                    break;
                case Windows.ApplicationModel.StartupTaskState.Enabled:
                    cbStartup.IsChecked = true;
                    break;
            }

            cbStartup.Click += CbStartupClick;
        }
        async void SetStartup()
        {
            var startupTask = await Windows.ApplicationModel.StartupTask.GetAsync(ID);
            if (startupTask.State == Windows.ApplicationModel.StartupTaskState.Enabled)
            {
                startupTask.Disable();
                cbStartup.IsChecked = false;
            }
            else
            {
                var state = await startupTask.RequestEnableAsync();
                switch (state)
                {
                    case Windows.ApplicationModel.StartupTaskState.DisabledByUser:
                        cbStartup.IsChecked = false;
                        break;
                    case Windows.ApplicationModel.StartupTaskState.Enabled:
                        cbStartup.IsChecked = true;
                        break;
                }
            }
        }

        //init
        void SetupTimer()
        {
            try
            {
                timer = new Timer(GetMilisec(TimerMinutes));
                timer.Elapsed += TimerElapsed;
                timer.Enabled = true;
            }
            catch (Exception e) { new MessageWindow(Lang["ErrorTitle"], e.Message, MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog(); Application.Current.Shutdown(1); }
        }
        void SetupNotifyIcon(bool Update = false)
        {
            try
            {
                System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
                contextMenu.MenuItems.Add(Lang["nfOpenWindow"], (sh, eh) => { Show(); tiCurrent.IsSelected = true; });
                contextMenu.MenuItems.Add(Lang["nfOpenFavorite"], (sh, eh) => { Show(); tiFavorite.IsSelected = true; });
                contextMenu.MenuItems.Add(Lang["nfOpenSettings"], (sh, eh) => { Show(); tiOptions.IsSelected = true; });
                contextMenu.MenuItems.Add("-");
                contextMenu.MenuItems.Add(Lang["nfRefresh"], (sh, eh) => SetupWallpaper(true));
                contextMenu.MenuItems.Add(Lang["nfLeave"], (sh, eh) => cbSource.SelectedIndex = -1);
                contextMenu.MenuItems.Add(Lang["nfAddFav"], (sh, eh) => LikeWallpaper());
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
            catch (Exception e) { new MessageWindow(Lang["ErrorTitle"], e.Message, MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog(); Application.Current.Shutdown(1); }
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
                    tbStyle.Text = Lang["tbStyle"];

                    cbiTimetable0.Content = Lang["cbiTimetable0"];
                    cbiTimetable1.Content = Lang["cbiTimetable1"];
                    cbiTimetable2.Content = Lang["cbiTimetable2"];
                    cbiTimetable3.Content = Lang["cbiTimetable3"];
                    cbiTimetable4.Content = Lang["cbiTimetable4"];
                    cbiTimetable5.Content = Lang["cbiTimetable5"];
                    cbiTimetable6.Content = Lang["cbiTimetable6"];
                    cbiTimetable7.Content = Lang["cbiTimetable7"];
                    cbiTimetable8.Content = Lang["cbiTimetable8"];

                    cbStyle0.Content = Lang["cbStyle0"];
                    cbStyle1.Content = Lang["cbStyle1"];
                    cbStyle2.Content = Lang["cbStyle2"];
                    cbStyle3.Content = Lang["cbStyle3"];
                    cbStyle4.Content = Lang["cbStyle4"];
                    cbStyle5.Content = Lang["cbStyle5"];

                    cbStartup.Content = Lang["cbStartup"];

                    cbBing.Content = Lang["cbBing"];
                    cbFavorite.Content = Lang["cbFavorite"];
                    cbFlickr.Content = Lang["cbFlickr"];
                    cbUnsplash.Content = Lang["cbUnsplash"];

                    tbBtnRefresh.Text = Lang["btnRefresh"];

                    tiCurrent.Header = Lang["tiCurrent"];
                    tiFavorite.Header = Lang["tiFavorite"];
                    tiOptions.Header = Lang["tiOptions"];
                }));
            });
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
            new MessageWindow(Lang["AboutTitle"], $"{ Lang["Author"]}: \t    {AUTHOR} ({WEBSITE_AUTHOR})\n" +
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

            cbStyle.SelectedIndex = Source;
            cbStyle.SelectionChanged += CbStyleSelectionChanged;

            GetStartup();
            LoadFavoriteToPanel();
        }
        private void CbStartupClick(object sender, RoutedEventArgs e) => SetStartup();
        private void windowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Visible = false;

            if (Source == 2)
                Core.Source.FlickrSource.Finder.SaveUsed();
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
        private void btnTimetableHelpClick(object sender, RoutedEventArgs e) => ShowToast(Lang["MsgInfoTitle"], Lang["MsgInfoTimtable"]);
        private void btnLikeClick(bool IsSwitched)
        {
            if (IsSwitched)
                LikeWallpaper();
            else
                UnlikeWallpaper();

            SaveFavorite();
        }
        private void PfApplyEvent(string origin, string thumb, string copy)
        {
            cbSource.SelectedIndex = -1;

            rs[WALLPAPER_URL] = origin;
            rs[WALLPAPER_THUMBNAIL] = thumb;
            rs[WALLPAPER_COPYRIGHT] = copy;

            SetupWallpaper(true);
        }
        private void PfDeleteEvent(string original, string thumb)
        {
            UnlikeWallpaper(original);
            SaveFavorite();
        }
        private void CbStyleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbStyle.SelectedIndex == -1)
                return;

            WallpaperStyle = cbStyle.SelectedIndex;
            WriteRegistry((Core.Style)WallpaperStyle);

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 1, System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wallpaper.bmp"), SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        private void btnStyleHelpClick(object sender, RoutedEventArgs e) => ShowToast(Lang["MsgInfoTitle"], string.Format(Lang["MsgInfoStyle"], VERSION));
        private void btnSetupSourceClick(object sender, RoutedEventArgs e)
        {
            var item = new Core.Source.FlickrSource.FlickrSettings();
            item.FlickrSettingsClosed += ItemFlickrSettingsClosed;

            item.ShowDialog();
        }
        private void ItemFlickrSettingsClosed() => SetupWallpaper(true);
    }
}