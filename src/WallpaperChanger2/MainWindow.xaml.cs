using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;

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
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {

        }
    }
}
