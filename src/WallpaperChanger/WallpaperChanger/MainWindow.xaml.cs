using FlickrNet;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        static string ID = "WallpaperChanger2";
        static string AUTHOR = "Verloka Vadim";
        static string WEBSITE_AUTHOR = "verloka.github.io";
        static string WEBSITE_APP = "changer.pp.ua";
        static string VERSION = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Revision}";
        static string FAVORITE_FILE_NAME = "favorites.json";
        static string FAVORITE_FOLDER_NAME = "favorites";

        public const string WALLPAPER_URL = "ImageUID";
        public const string WALLPAPER_THUMBNAIL = "ImageUIDThumbnail";
        public const string WALLPAPER_COPYRIGHT = "CurrentCopy";
        public const string WALLPAPER_FAVORITE_NUMBER = "FuvNum";
        public const string WALLPAPER_FLICKR_TAGS = "FlickrTags";
        public const string WALLPAPER_FLICKR_COLORS = "FlickrColors";
        public const string WALLPAPER_FLICKR_STYLES = "FlickrStyles";
        public const string WALLPAPER_FLICKR_TAGS_ALL = "FlickrTagsAll";

        public ObservableCollection<Controlls.FavoritePic> FavoriteList { get; set; }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const int SPI_SETDESKWALLPAPER = 20;
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
        public bool ShowErrorMessages
        {
            get => rs.GetValue("ShowErrorMessages", true);
            set => rs.SetValue("ShowErrorMessages", value);
        }
        public bool ShowUpdateWallpaerToast
        {
            get => rs.GetValue("ShowUpdateWallpaerToast", true);
            set => rs.SetValue("ShowUpdateWallpaerToast", value);
        }
        public int ResolutionSource
        {
            //0 - auto
            //1 - custom
            //2 - selected
            get => rs.GetValue("ResSource", 0);
            set => rs.SetValue("ResSource", value);
        }
        public int ResolutionSourceSI
        {
            get => rs.GetValue("ResSourceSI", 0);
            set => rs.SetValue("ResSourceSI", value);
        }
        public int ResolutionW
        {
            get => rs.GetValue("ResW", 1366);
            set => rs.SetValue("ResW", value == 0 ? 1366 : value);
        }
        public int ResolutionH
        {
            get => rs.GetValue("ResH", 768);
            set => rs.SetValue("ResH", value == 0 ? 768 : value);
        }

        public int WallpaperWidth { get => ResolutionSource == 0 ? (int)SystemParameters.VirtualScreenWidth : ResolutionW; }
        public int WallpaperHeight { get => ResolutionSource == 0 ? (int)SystemParameters.VirtualScreenHeight : ResolutionH; }

        public RegSettings rs;
        public Manager Lang;

        Timer timer;
        System.Windows.Forms.NotifyIcon notifyIcon;
        Flickr flickr;
        int TimerMinutes = 30;
        bool initError = false;
        bool loadFav = true;

        public MainWindow()
        {
            InitRegSettings();
            InitLangManager();
            LoadFavorite();

            InitializeComponent();

            DataContext = this;

            SetupTimer();
            SetupNotifyIcon();
            SetupWallpaper(true);
        }

        //init
        void InitRegSettings()
        {
            try { rs = new RegSettings(ID); }
            catch (Exception e)
            {
                initError = true;
                new ErrorWindow("Error", $"Error {e.StackTrace}. Additional information:", MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 640, additionalText: e.Message).ShowDialog();
                Application.Current.Shutdown(1);
            }
        }
        void InitLangManager()
        {
            try
            {
                Lang = new Manager($@"{AppDomain.CurrentDomain.BaseDirectory}\Data\locales.ini");
                Lang.Load();
                Lang.SetCurrent(rs.GetValue("LanguageCode", "en-us"));
                Lang.LanguageChanged += LangLanguageChanged;
            }
            catch (Exception e)
            {
                initError = true;
                new ErrorWindow("Error", $"Error {e.StackTrace}. Additional information:", MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: e.Message).ShowDialog();
                Application.Current.Shutdown(2);
            }
        }
        async void LoadFavorite()
        {
            try
            {
                FavoriteList = new ObservableCollection<Controlls.FavoritePic>();

                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile favoriteFile = await localFolder.TryGetItemAsync(FAVORITE_FILE_NAME) as Windows.Storage.StorageFile;

                if (favoriteFile != null)
                    foreach (var item in JsonConvert.DeserializeObject<List<Favorite>>(await Windows.Storage.FileIO.ReadTextAsync(favoriteFile)))
                    {
                        Controlls.FavoritePic pf = new Controlls.FavoritePic()
                        {
                            Original = item.Original,
                            ThumbnailLocale = item.ThumbnailLocale,
                            Thumbnail = item.Thumbnail,
                            Copyright = item.Copyright,
                            Margin = new Thickness(7)
                        };

                        pf.ApplyEvent += PfApplyEvent;
                        pf.DeleteEvent += PfDeleteEvent;

                        FavoriteList.Add(pf);
                    }
            }
            catch (Exception e)
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
            }
            finally
            {
                loadFav = false;
                RemoveUnusedThumb();
            }
        }
        void SetupTimer()
        {
            try
            {
                timer = new Timer(GetMilisec(TimerMinutes));
                timer.Elapsed += TimerElapsed;
                timer.Enabled = true;
            }
            catch (Exception e)
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
            }
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
                contextMenu.MenuItems.Add(Lang["nfAddFav"], async (sh, eh) => LikeWallpaper());
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
            catch (Exception e)
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
            }
        }
        async void RemoveUnusedThumb()
        {
            try
            {
                var fav = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FAVORITE_FOLDER_NAME) as Windows.Storage.StorageFolder;

                if (fav == null)
                    fav = await ApplicationData.Current.LocalFolder.CreateFolderAsync(FAVORITE_FOLDER_NAME);

                foreach (var item in await fav.GetFilesAsync())
                    if (FavoriteList.FirstOrDefault((x) => x.ThumbnailLocale == item.Name) == null)
                        try { await item.DeleteAsync(); }
                        catch { }
            }
            catch (Exception e)
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
            }
        }

        //window loaded
        async Task SetupLocale()
        {
            Task.Factory.StartNew(() =>
            {
                try
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
                        cbShowError.Content = Lang["cbShowError"];
                        cbShowWallpaperTost.Content = Lang["cbShowWallpaperTost"];

                        cbBing.Content = Lang["cbBing"];
                        cbFavorite.Content = Lang["cbFavorite"];
                        cbFlickr.Content = Lang["cbFlickr"];
                        cbUnsplash.Content = Lang["cbUnsplash"];

                        tbBtnRefresh.Text = Lang["btnRefresh"];

                        tiCurrent.Header = Lang["tiCurrent"];
                        tiFavorite.Header = Lang["tiFavorite"];
                        tiOptions.Header = Lang["tiOptions"];

                        tbOtherStaff.Text = Lang["tbOtherStaff"];

                        sbJesusPassword.AppDescription = Lang["JesusPassword"];
                    }));
                }
                catch (Exception e)
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new ErrorWindow("Error", $"Error {e.StackTrace}. Additional information:", MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: e.Message).ShowDialog();
                        });
                }
            });
        }

        //startup
        async void GetStartup()
        {
            try
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
            catch (Exception e)
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
            }
        }
        async void SetStartup()
        {
            try
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
            catch (Exception e)
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
            }
        }

        //favorites
        async void SaveFavorite()
        {
            try
            {
                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile favoriteFile = await localFolder.CreateFileAsync(FAVORITE_FILE_NAME, Windows.Storage.CreationCollisionOption.ReplaceExisting);

                List<Favorite> favs = new List<Favorite>();

                foreach (var item in FavoriteList)
                    favs.Add(new Favorite()
                    {
                        Copyright = item.Copyright,
                        Original = item.Original,
                        Thumbnail = item.Thumbnail,
                        ThumbnailLocale = item.ThumbnailLocale
                    });

                await Windows.Storage.FileIO.WriteTextAsync(favoriteFile, JsonConvert.SerializeObject(favs));
            }
            catch (Exception e)
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
            }
        }
        async void LikeWallpaper()
        {
            try
            {
                var fav = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FAVORITE_FOLDER_NAME) as Windows.Storage.StorageFolder;

                if (fav == null)
                    fav = await ApplicationData.Current.LocalFolder.CreateFolderAsync(FAVORITE_FOLDER_NAME);

                var file = await fav.CreateFileAsync(Path.GetRandomFileName(), CreationCollisionOption.ReplaceExisting);

                using (WebClient wc = new WebClient())
                    wc.DownloadFile(rs.GetValue<string>(WALLPAPER_THUMBNAIL), file.Path);

                Controlls.FavoritePic pf = new Controlls.FavoritePic()
                {
                    Original = rs.GetValue<string>(WALLPAPER_URL),
                    Thumbnail = rs.GetValue<string>(WALLPAPER_THUMBNAIL),
                    Copyright = rs.GetValue<string>(WALLPAPER_COPYRIGHT),
                    ThumbnailLocale = file.Path,
                    Margin = new Thickness(7)
                };

                pf.ApplyEvent += PfApplyEvent;
                pf.DeleteEvent += PfDeleteEvent;

                FavoriteList.Add(pf);

                SaveFavorite();

                btnLike.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                {
                    btnLike.IsSwitched = true;
                });
            }
            catch (Exception e)
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
            }
        }
        void UnlikeWallpaper(string original)
        {
            try
            {
                var item = FavoriteList.FirstOrDefault((x) => x.Original == original);

                if (item != null && FavoriteList.Remove(item))
                    SaveFavorite();

                if (original.Equals(rs.GetValue<string>(WALLPAPER_URL)))
                    btnLike.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnLike.IsSwitched = false;
                    });
            }
            catch (Exception e)
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
            }
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
        void ShowToast(string title, string msg, string img = "", string via = "")
        {
            try
            {
                string imageBlock = string.IsNullOrWhiteSpace(img) ? "" :
                                                                $"<image placement='hero' src='{img}'/>";

                string viaBlock = string.IsNullOrWhiteSpace(via) ? "" :
                                                                  $"<text placement='attribution'>{via}</text>";

                string xml = string.Format(
                    @"<toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{0}</text>
                            <text>{1}</text>
                            {2}
                            {3}
                        </binding>
                    </visual>
                </toast>",
                    title,
                    msg,
                    imageBlock,
                    viaBlock);

                Windows.Data.Xml.Dom.XmlDocument doc = new Windows.Data.Xml.Dom.XmlDocument();
                doc.LoadXml(xml);

                Windows.UI.Notifications.ToastNotification toast = new Windows.UI.Notifications.ToastNotification(doc);
                Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (Exception e)
            {
                if (string.IsNullOrWhiteSpace(img) && ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
                else
                    ShowToast(title, msg, via: via);
            }
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

        //wallpaper
        async void SetupWallpaper(bool Update = false)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
            {
                btnRefresh.IsEnabled = false;
                btnLike.IsEnabled = false;
            });

            switch (Source)
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
                try
                {
                    if (!GetConnection())
                    {
                        ShowToast(Lang["AlertTitle"], Lang["NoInternetMsg"]);
                        return;
                    }

                    if (CheckDate() || Update)
                    {
                        var imgData = Core.Source.Unsplash.Unsplash.Get(WallpaperWidth, WallpaperHeight);

                        rs[WALLPAPER_URL] = imgData.Item1;
                        rs[WALLPAPER_THUMBNAIL] = imgData.Item2;
                        rs[WALLPAPER_COPYRIGHT] = Lang["UnsplashCopy"];

                        WriteDate();
                        WriteThumbCopy(imgData.Item1, Lang["UnsplashCopy"]);

                        SetupImage(new Uri(imgData.Item1, UriKind.RelativeOrAbsolute), Lang["cbUnsplash"]);
                    }
                }
                catch (Exception e)
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                        });
                }
                finally
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnRefresh.IsEnabled = true;
                        btnLike.IsEnabled = true;
                    });
                }
            });
        }
        async Task SetupallpaperFlickr(bool Update = false)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    if (!GetConnection())
                    {
                        ShowToast(Lang["AlertTitle"], Lang["NoInternetMsg"]);
                        return;
                    }

                    if (CheckDate() || Update)
                    {
                        var imgData = Core.Source.FlickrSource.Finder.FindAsync(WallpaperWidth,
                                                                                WallpaperHeight,
                                                                                rs.GetValue(WALLPAPER_URL, ""),
                                                                                rs.GetValue(WALLPAPER_FLICKR_TAGS, "nature"),
                                                                                rs.GetValue(WALLPAPER_FLICKR_TAGS_ALL, false) ? TagMode.AllTags : TagMode.AnyTag,
                                                                                GetFlickrColors(),
                                                                                GetFlickrStyles()).Result;

                        rs[WALLPAPER_URL] = imgData.Item1;
                        rs[WALLPAPER_THUMBNAIL] = imgData.Item2;
                        rs[WALLPAPER_COPYRIGHT] = imgData.Item3;

                        WriteDate();
                        WriteThumbCopy(imgData.Item2, imgData.Item3);

                        SetupImage(new Uri(imgData.Item1, UriKind.RelativeOrAbsolute), Lang["cbFlickr"]);
                    }
                }
                catch (Exception e)
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                        });
                }
                finally
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnRefresh.IsEnabled = true;
                        btnLike.IsEnabled = true;
                    });
                }
            });
        }
        async Task SetupWallpaperFavorite(bool Update)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    if (!GetConnection())
                    {
                        ShowToast(Lang["AlertTitle"], Lang["NoInternetMsg"]);
                        return;
                    }

                    while (loadFav)
                        Task.Delay(100).Wait();

                    if (FavoriteList.Count == 0)
                    {
                        cbSource.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            cbSource.SelectedIndex = 1;
                        });

                        SetupWallpaper(true);
                        return;
                    }


                    if (CheckDate() || Update)
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            int number = rs.GetValue(WALLPAPER_FAVORITE_NUMBER, -1) + 1;

                            number = (number > FavoriteList.Count - 1) ? 0 : number;

                            rs[WALLPAPER_URL] = FavoriteList[number].Original;
                            rs[WALLPAPER_THUMBNAIL] = FavoriteList[number].Thumbnail;
                            rs[WALLPAPER_COPYRIGHT] = FavoriteList[number].Copyright;
                            rs[WALLPAPER_FAVORITE_NUMBER] = number;

                            WriteDate();
                            WriteThumbCopy(rs.GetValue<string>(WALLPAPER_THUMBNAIL), rs.GetValue<string>(WALLPAPER_COPYRIGHT));

                            SetupImage(new Uri(rs.GetValue<string>(WALLPAPER_URL), UriKind.RelativeOrAbsolute), Lang["cbFavorite"]);
                        });
                    }
                }
                catch (Exception e)
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                        });
                }
                finally
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnRefresh.IsEnabled = true;
                        btnLike.IsEnabled = true;
                    });
                }
            });
        }
        async Task SetupWallpaperLeave(bool Update)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!Update)
                        return;

                    WriteDate();
                    WriteThumbCopy(rs.GetValue<string>(WALLPAPER_THUMBNAIL), rs.GetValue<string>(WALLPAPER_COPYRIGHT));

                    SetupImage(new Uri(rs.GetValue<string>(WALLPAPER_URL), UriKind.RelativeOrAbsolute), "");
                }
                catch (Exception e)
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                        });
                }
                finally
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnRefresh.IsEnabled = true;
                        btnLike.IsEnabled = true;
                    });
                }
            });
        }
        async Task SetupWallpaperBing(bool Update = false)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!GetConnection())
                    {
                        ShowToast(Lang["AlertTitle"], Lang["NoInternetMsg"]);
                        return;
                    }

                    var imgData = Core.Source.Bing.Bing.Get(WallpaperWidth, WallpaperHeight).Result;

                    if (CheckDate() || Update)
                    {
                        if (rs.GetValue(WALLPAPER_URL, "") != imgData.Item1 || Update)
                        {
                            rs[WALLPAPER_URL] = imgData.Item1;
                            rs[WALLPAPER_THUMBNAIL] = imgData.Item2;
                            rs[WALLPAPER_COPYRIGHT] = imgData.Item3;

                            WriteDate();
                            WriteThumbCopy(imgData.Item2, imgData.Item3);

                            SetupImage(new Uri(imgData.Item1, UriKind.RelativeOrAbsolute), Lang["cbBing"]);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                        });
                }
                finally
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnRefresh.IsEnabled = true;
                        btnLike.IsEnabled = true;
                    });
                }
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
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                switch (style)
                {
                    case Core.Style.Tile:
                        key.SetValue(@"WallpaperStyle", 0.ToString());
                        key.SetValue(@"TileWallpaper", 1.ToString());
                        break;
                    case Core.Style.Center:
                        key.SetValue(@"WallpaperStyle", 0.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                        break;
                    default:
                    case Core.Style.Stretch:
                        key.SetValue(@"WallpaperStyle", 2.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                        break;
                    case Core.Style.Span:
                        key.SetValue(@"WallpaperStyle", 22.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                        break;
                    case Core.Style.Fit:
                        key.SetValue(@"WallpaperStyle", 6.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                        break;
                    case Core.Style.Fill:
                        key.SetValue(@"WallpaperStyle", 10.ToString());
                        key.SetValue(@"TileWallpaper", 0.ToString());
                        break;
                }

                key.Close();
            }
        }
        void WriteThumbCopy(string thumb, string copy)
        {
            try
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                {
                    imgImageThumb.Source = new BitmapImage(new Uri(thumb));
                    tbImageInfo.Text = copy;
                });
            }
            catch
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                {
                    //imgImageThumb.Source = imgImageThumb.Source = new BitmapImage(new Uri("www.bing.com/az/hprichbg/rb/OchaBatake_ROW10481280883_400x240.jpg"));
                    tbImageInfo.Text = tbImageInfo.Text = "Error"; ;
                });
            }
        }
        async void SetupImage(Uri uri, string sourceName)
        {
            try
            {
                Stream s = new WebClient().OpenRead(uri.ToString());

                System.Drawing.Image img = System.Drawing.Image.FromStream(s);

                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wallpaper.bmp");
                img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                if (ShowUpdateWallpaerToast)
                    ShowToast(Lang["TitleWallpaperSetuped"],
                              Lang["MsgWallpaperSetuped"],
                              rs.GetValue<string>(WALLPAPER_THUMBNAIL),
                              sourceName);
            }
            catch (Exception e)
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new ErrorWindow(Lang["ErrorTitle"], Lang["ErrorCode"], MessageWindowIcon.Error, MessageWindowIconColor.Red, maxWidth: 800, additionalText: $"StackTrace:\n{e.StackTrace}\n\nMessage:\n{e.Message}").ShowDialog();
                    });
            }
            finally
            {
                btnLike.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                {
                    btnLike.IsSwitched = (from i in FavoriteList
                                          where i.Original == rs.GetValue<string>(WALLPAPER_URL)
                                          select i).Count() > 0;
                });
            }
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
            new ErrorWindow(Lang["AboutTitle"], $"{ Lang["Author"]}: \t    {AUTHOR} ({WEBSITE_AUTHOR})\n" +
                                       $"{Lang["Version"]}: \t    {VERSION}\n" +
                                       $"{Lang["AppWebsite"]}: \t    {WEBSITE_APP}\n\n" +
                                       $"{Lang["Copyright"]} © {AUTHOR}, {DateTime.Now.Year}", Core.MessageWindowIcon.Info, Core.MessageWindowIconColor.Orange, maxWidth: 640).ShowDialog();
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

            cbResolution.SelectedIndex = ResolutionSourceSI;
            cbResolution.SelectionChanged += cbResolutionSelectionChanged;

            if(cbResolution.SelectedIndex == 1)
            {
                RW.Text = ResolutionW.ToString();
                RH.Text = ResolutionH.ToString();
                gridCustomRes.Visibility = Visibility.Visible;
            }

            RW.TextChanged += ResolutionTextChanged;
            RH.TextChanged += ResolutionTextChanged;

            GetStartup();
        }
        private void CbStartupClick(object sender, RoutedEventArgs e) => SetStartup();
        private async void windowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (initError)
                return;

            notifyIcon.Visible = false;

            if (Source == 2)
            {
                await Core.Source.FlickrSource.Finder.SaveUsed();

                while (Core.Source.FlickrSource.Finder.IsSaving)
                    Task.Delay(100).Wait();
            }
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
        private async void btnLikeClick(bool IsSwitched)
        {
            if (IsSwitched)
                LikeWallpaper();
            else
                UnlikeWallpaper(rs.GetValue<string>(WALLPAPER_URL));
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
            var flickrSettings = new Core.Source.FlickrSource.FlickrSettings();
            flickrSettings.FlickrSettingsClosed += ItemFlickrSettingsClosed;

            flickrSettings.ShowDialog();
        }
        private void ItemFlickrSettingsClosed() => SetupWallpaper(true);
        private void sbJesusPasswordClicked() => Process.Start("ms-windows-store://pdp/?ProductId=9nblggh691x4");
        private void cbResolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResolutionSourceSI = cbResolution.SelectedIndex;

            switch (cbResolution.SelectedIndex)
            {
                case 0:
                    ResolutionSource = 0;
                    gridCustomRes.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    ResolutionSource = 1;
                    RW.Text = ResolutionW.ToString();
                    RH.Text = ResolutionH.ToString();
                    gridCustomRes.Visibility = Visibility.Visible;
                    break;
                default:
                    ResolutionSource = 2;
                    var selected = (sender as ComboBox).SelectedItem as ComboBoxItem;
                    var sizeStr = selected.Content.ToString().Split('×');
                    int.TryParse(sizeStr[0], out int w);
                    int.TryParse(sizeStr[1], out int h);
                    ResolutionW = w;
                    ResolutionH = h;
                    break;
            }
        }
        private void ResolutionTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;

            int.TryParse(tb.Text, out int s);

            if (tb.Tag.ToString() == "h")
                ResolutionH = s;
            else
                ResolutionW = s;
        }
    }
}