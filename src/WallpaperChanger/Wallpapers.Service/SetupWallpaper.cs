using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Wallpapers.Service
{
    public static class SetupWallpaper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);


        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        public static void Setup(string path) => Setup(path, WallpaperStyle.Stretched);
        public static void Setup(string path, WallpaperStyle style)
        {
            RegistryKey key = Registry.Users.OpenSubKey($@"{LogonSid.GetLogonSid()}\\Control Panel\Desktop", true);

            switch (style)
            {
                case WallpaperStyle.Tiled:
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                    break;
                case WallpaperStyle.Centered:
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
                default:
                case WallpaperStyle.Stretched:
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                    break;
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
