using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using WallpaperChanger2.Model;

namespace WallpaperChanger2
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        Timer timer;
        
        public MainWindow()
        {
            InitializeComponent();
        }
        
        public void SetupImage(Uri uri, Model.Style style)
        {
            Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

            System.Drawing.Image img = System.Drawing.Image.FromStream(s);
            string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            switch (style)
            {
                case Model.Style.Tiled:
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                    break;
                case Model.Style.Centered:
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
                default:
                case Model.Style.Stretched:
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        public async void Build()
        {
            LOG("Start build....");

            string uid = App.Settings.GetValue("ImageUID", "");

            string url = await Bing.ImageUrl();

            if(CheckDate())
            {
                LOG("CheckDate() returned TRUE");

                if (uid != url)
                {
                    LOG("ImageUID - uid != url");

                    SetupImage(new Uri(url, UriKind.RelativeOrAbsolute), Model.Style.Stretched);

                    LOG($"Setuped new image: {url}");

                    var dt = DateTime.Now.Add(new TimeSpan(24, 0, 0));

                    App.Settings["UpdateWallpaperMounth"] = dt.Month;
                    App.Settings["UpdateWallpaperDay"] = dt.Day;
                    App.Settings["UpdateWallpaperHour"] = dt.Hour;

                    App.Settings["ImageUID"] = url;
                }
                else
                {
                    LOG("ImageUID - uid == url");
                }
            }
            else
            {
                LOG("CheckDate() returned FALSE");
            }
        }
        public bool CheckDate()
        {
            int mounth = DateTime.Now.Month;
            int day = DateTime.Now.Day;
            int hour = DateTime.Now.Hour;

            LOG($"Date: Mounth - {App.Settings.GetValue("UpdateWallpaperMounth", 0)}/{mounth} Day - {App.Settings.GetValue("UpdateWallpaperDay", 0)}/{day} Hour - {App.Settings.GetValue("UpdateWallpaperHour", 0)}/{hour}");

            if (mounth >= App.Settings.GetValue("UpdateWallpaperMounth", 0))
                if (day >= App.Settings.GetValue("UpdateWallpaperDay", 0))
                    if (hour > App.Settings.GetValue("UpdateWallpaperHour", 0))
                        return true;

            return false;
        }
        double GetMilisec(int minunte)
        {
            return 60000 * minunte;
        }

        public static void LOG(string log)
        {
            string path = @"C:\Users\Верлока Вадим\Desktop\log.txt";
            
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}: {log}");
            }
        }

        #region Window Events
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch { }
        }
        private void btnCloseClick()
        {
            /*if (App.Settings.GetValue<bool>("AppExit"))
            {
                Application.Current.Shutdown(0);
            }
            else
            {
                Hide();
            }*/

            Application.Current.Shutdown(0);
        }
        private void btnMinimazeClick()
        {
            WindowState = WindowState.Minimized;
        }
        private void btnHelpClick()
        {
            new Windows.InfoWindow().ShowDialog();
        }
        #endregion

        private void mywindowLoaded(object sender, RoutedEventArgs e)
        {
            timer = new Timer(GetMilisec(60));
            timer.Elapsed += TimerElapsed;
            timer.Enabled = true;

            LOG("Application loaded");

            Build();
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Build();
        }
    }
}
