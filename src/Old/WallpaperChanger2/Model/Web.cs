using System.Net;

namespace WallpaperChanger2.Model
{
    public static class Web
    {
        public static bool GetConnection()
        {
            try
            {
                using (WebClient client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                    return true;
            }
            catch { return false; }
        }
    }
}
