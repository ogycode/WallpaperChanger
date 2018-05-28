using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperChanger.Core.Source.Unsplash
{
    public static class Unsplash
    {
        public static Tuple<string,string>Get(double w, double h)
        {
            string url = $"http://source.unsplash.com/random/{w}x{h}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string originalUrl = response.ResponseUri.OriginalString;
            string thumb = originalUrl.Replace($"&w={w}", "&w=400").Replace($"&h={h}", "&h=240");

            response.Close();

            return new Tuple<string, string>(originalUrl, thumb);
        }
    }
}
