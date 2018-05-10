using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WallpaperChanger.Core.Source.Bing
{
    public static class Bing
    {
        /// <summary>
        /// Get information about image
        /// </summary>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns>1. Url, 2. Thumb, 3. Copyright</returns>
        public static Task<Tuple<string, string, string>> Get(double w, double h)
        {
            return Task.Factory.StartNew(() =>
            {
                string urlImg = "";
                string urlThumb = "";
                string copyright = "";

                try
                {
                    HttpClient client = new HttpClient();
                    string url = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1";
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    var a = JsonConvert.DeserializeObject<RootObject>(response.Content.ReadAsStringAsync().Result);

                    urlImg = $"http://bing.com{a.images[0].urlbase}_{w}x{h}.jpg";
                    urlThumb = $"http://bing.com{a.images[0].urlbase}_{400}x{240}.jpg";
                    copyright = a?.images[0]?.copyright;
                }
                catch
                {
                    urlImg = "www.bing.com/az/hprichbg/rb/OchaBatake_ROW10481280883_1366x768.jpg";
                    urlImg = "www.bing.com/az/hprichbg/rb/OchaBatake_ROW10481280883_400x240.jpg";
                    copyright = "Error";
                }

                return new Tuple<string, string, string>(urlImg, urlThumb, copyright);
            });
        }
    }
}
