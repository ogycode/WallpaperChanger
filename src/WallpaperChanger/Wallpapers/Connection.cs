using System.Net;

namespace Wallpapers
{
    public static class Connection
    {
        /// <summary>
        /// Checking internet connection
        /// </summary>
        /// <returns></returns>
        public static bool Check()
        {
            try
            {
                using (WebClient client = new WebClient())
                using (var stream = client.OpenRead("https://www.google.com"))
                    return true;
            }
            catch { return false; }
        }
    }
}
