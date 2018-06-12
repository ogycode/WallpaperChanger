/*  Error codes:
 *  0001 - Init RegSettings
 *  0002 - Init Language Manager
 *  0003 - LoadFavorite
 *  0004 - SetupTimer
 *  0005 - SetupNotifyIcon
 *  0006 - SetupLocale
 *  0007 - GetStartup
 *  0008 - SetStartup
 *  0009 - SaveFavorite
 *  0010 - LoadFavoriteToPanel
 *  0011 - GetFavoritePath
 *  0012 - LikeWallpaper
 *  0013 - UnlikeWallpaper
 *  0014 - SetupWallpaperUnsplash
 *  0015 - SetupallpaperFlickr
 *  0016 - SetupWallpaperFavorite
 *  0017 - SetupWallpaperLeave
 *  0018 - SetupWallpaperBing
 *  0019 - SetupImage
 *  0020 - ShowToast
*/
using FlickrNet;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        static string FavoriteListPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\Data\favorites.json";

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

        public RegSettings rs;
        public Manager Lang;

        Timer timer;
        System.Windows.Forms.NotifyIcon notifyIcon;
        Flickr flickr;
        int TimerMinutes = 30;

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
            catch
            {
                new MessageWindow("Error", "Error code is 0001", MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
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
            catch
            {
                new MessageWindow("Error", "Error code is 0002", MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                Application.Current.Shutdown(2);
            }
        }
        void LoadFavorite()
        {
            try
            {
                if (!Directory.Exists(FavoritePath))
                    Directory.CreateDirectory(FavoritePath);

                if (File.Exists(FavoriteListPath))
                {
                    using (StreamReader sr = File.OpenText(FavoriteListPath))
                        FavoriteList = JsonConvert.DeserializeObject<List<Favorite>>(sr.ReadToEnd());

                    foreach (var item in Directory.GetFiles(FavoritePath))
                        if (FavoriteList.FirstOrDefault((x) => x.ThumbnailLocale == item) == null)
                            try { File.Delete(item); }
                            catch { }
                }
                else
                    FavoriteList = new List<Favorite>();
            }
            catch
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0003), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                    });
            }
            finally
            {
                if (FavoriteList == null)
                    FavoriteList = new List<Favorite>();
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
            catch
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0004), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
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
                contextMenu.MenuItems.Add(Lang["nfAddFav"], async (sh, eh) => await LikeWallpaper());
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
            catch
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0005), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
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
                catch
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0006), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
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
            catch
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0007), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
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
            catch
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0008), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                    });
            }
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
            catch
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0009), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                    });
            }
        }
        void LoadFavoriteToPanel()
        {
            try
            {
                wpFavorites.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
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
                }); 
            }
            catch
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0010), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                    });
            }
        }
        async Task LikeWallpaper()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!Directory.Exists(FavoritePath))
                        Directory.CreateDirectory(FavoritePath);

                    string locale = $@"{FavoritePath}\{Path.GetRandomFileName()}";

                    if (File.Exists(locale))
                        File.Delete(locale);

                    using (WebClient wc = new WebClient())
                        wc.DownloadFile(rs.GetValue<string>(WALLPAPER_THUMBNAIL), locale);

                    FavoriteList.Add(new Favorite()
                    {
                        Original = rs.GetValue<string>(WALLPAPER_URL),
                        Thumbnail = rs.GetValue<string>(WALLPAPER_THUMBNAIL),
                        Copyright = rs.GetValue<string>(WALLPAPER_COPYRIGHT),
                        ThumbnailLocale = locale
                    });

                    LoadFavoriteToPanel();

                    btnLike.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnLike.IsSwitched = true;
                    });

                }
                catch
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0012), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                        });
                }
                finally
                {
                    SaveFavorite();
                }
            });
        }
        void UnlikeWallpaper(string original)
        {
            try
            {
                var current = rs.GetValue<string>(WALLPAPER_URL);

                btnLike.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                {
                    btnLike.IsSwitched = !original.Equals(current);
                });

                if (FavoriteList.RemoveAll((x) => x.Original == original) > 0)
                    LoadFavoriteToPanel();
            }
            catch
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0013), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                    });
            }
            finally
            {
                SaveFavorite();
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
            catch
            {
                if (string.IsNullOrWhiteSpace(img) && ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["ErrorCode"], 0020), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
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
            btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
            {
                btnRefresh.IsEnabled = false;
            });

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
                try
                {
                    if (!GetConnection())
                    {
                        ShowToast(Lang["AlertTitle"], Lang["NoInternetMsg"]);
                        btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            btnRefresh.IsEnabled = false;
                        });
                        return;
                    }

                    if (CheckDate() || Update)
                    {
                        var imgData = Core.Source.Unsplash.Unsplash.Get(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);

                        rs[WALLPAPER_URL] = imgData.Item1;
                        rs[WALLPAPER_THUMBNAIL] = imgData.Item2;
                        rs[WALLPAPER_COPYRIGHT] = Lang["UnsplashCopy"];

                        WriteDate();
                        WriteThumbCopy(imgData.Item1, Lang["UnsplashCopy"]);

                        SetupImage(new Uri(imgData.Item1, UriKind.RelativeOrAbsolute), Lang["cbUnsplash"]);
                    }
                    else
                        btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            btnRefresh.IsEnabled = false;
                        });
                }
                catch
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["CannotSetupWallpaper"], 0014), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                        });

                    btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnRefresh.IsEnabled = false;
                    });
                }
            });
        }
        async Task SetupallpaperFlickr(bool Update = false)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!GetConnection())
                    {
                        ShowToast(Lang["AlertTitle"], Lang["NoInternetMsg"]);
                        btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            btnRefresh.IsEnabled = false;
                        });
                        return;
                    }

                    if (CheckDate() || Update)
                    {
                        var imgData = Core.Source.FlickrSource.Finder.FindAsync(SystemParameters.VirtualScreenWidth,
                                                                                SystemParameters.VirtualScreenHeight,
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
                    else
                        btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            btnRefresh.IsEnabled = false;
                        });
                }
                catch
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["CannotSetupWallpaper"], 0015), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                        });

                    btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnRefresh.IsEnabled = false;
                    });
                }
            });
        }
        async Task SetupWallpaperFavorite(bool Update)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!GetConnection())
                    {
                        ShowToast(Lang["AlertTitle"], Lang["NoInternetMsg"]);
                        btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            btnRefresh.IsEnabled = false;
                        });
                        return;
                    }

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
                        int number = rs.GetValue(WALLPAPER_FAVORITE_NUMBER, -1) + 1;
                        number = (number > FavoriteList.Count - 1) ? 0 : number;

                        rs[WALLPAPER_URL] = FavoriteList[number].Original;
                        rs[WALLPAPER_THUMBNAIL] = FavoriteList[number].Thumbnail;
                        rs[WALLPAPER_COPYRIGHT] = FavoriteList[number].Copyright;

                        rs[WALLPAPER_FAVORITE_NUMBER] = number;

                        WriteDate();
                        WriteThumbCopy(FavoriteList[number].Thumbnail, FavoriteList[number].Copyright);

                        SetupImage(new Uri(rs.GetValue<string>(WALLPAPER_URL), UriKind.RelativeOrAbsolute), Lang["cbFavorite"]);
                    }
                    else
                        btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            btnRefresh.IsEnabled = false;
                        });
                }
                catch
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["CannotSetupWallpaper"], 0016), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                        });

                    btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnRefresh.IsEnabled = false;
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
                    if (!GetConnection())
                    {
                        ShowToast(Lang["AlertTitle"], Lang["NoInternetMsg"]);
                        btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            btnRefresh.IsEnabled = false;
                        });
                        return;
                    }

                    if (!Update)
                        return;

                    WriteDate();
                    WriteThumbCopy(rs.GetValue<string>(WALLPAPER_THUMBNAIL), rs.GetValue<string>(WALLPAPER_COPYRIGHT));

                    SetupImage(new Uri(rs.GetValue<string>(WALLPAPER_URL), UriKind.RelativeOrAbsolute), "");
                }
                catch
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["CannotSetupWallpaper"], 0017), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                        });

                    btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnRefresh.IsEnabled = false;
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
                        btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            btnRefresh.IsEnabled = false;
                        });
                        return;
                    }

                    var imgData = Core.Source.Bing.Bing.Get(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight).Result;

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
                    else
                        btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            btnRefresh.IsEnabled = false;
                        });
                }
                catch
                {
                    if (ShowErrorMessages)
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                        {
                            new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["CannotSetupWallpaper"], 0018), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                        });

                    btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        btnRefresh.IsEnabled = false;
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
                imgImageThumb.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { imgImageThumb.Source = new BitmapImage(new Uri(thumb)); });
                tbImageInfo.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { tbImageInfo.Text = copy; });
            }
            catch
            {
                imgImageThumb.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { imgImageThumb.Source = new BitmapImage(new Uri("www.bing.com/az/hprichbg/rb/OchaBatake_ROW10481280883_400x240.jpg")); });
                tbImageInfo.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate () { tbImageInfo.Text = "Error"; });
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
            catch
            {
                if (ShowErrorMessages)
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                    {
                        new MessageWindow(Lang["ErrorTitle"], string.Format(Lang["CannotSetupWallpaper"], 0019), MessageWindowIcon.Error, MessageWindowIconColor.Red).ShowDialog();
                    });

                btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                {
                    btnRefresh.IsEnabled = false;
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

                btnRefresh.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Threading.ThreadStart)delegate ()
                {
                    btnRefresh.IsEnabled = true;
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
        private async void btnLikeClick(bool IsSwitched)
        {
            if (IsSwitched)
                await LikeWallpaper();
            else
                UnlikeWallpaper(rs.GetValue<string>(WALLPAPER_URL));

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
        private void sbJesusPasswordClicked() => Process.Start("ms-windows-store://pdp/?ProductId=9nblggh691x4");
    }
}