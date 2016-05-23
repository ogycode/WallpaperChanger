using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WallpaperChanger
{
    public class Bing
    {
        /// <summary>
        /// Get daily bing picture
        /// </summary>
        /// <returns>Url of the picture</returns>
        static async Task<string> GetUrl()
        {
            HttpClient client = new HttpClient();
            string url = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1";
            HttpResponseMessage response = await client.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
        /// <summary>
        /// Get daily bing picture
        /// </summary>
        /// <returns>Path of the picture</returns>
        public static async Task<string> GetImageUri()
        {
            string jsonText = await GetUrl();
            int pos = jsonText.IndexOf("\"url\":\"");
            string url1 = "http://bing.com/", url2 = "";
            pos += 6;
            while (jsonText[++pos] != '"') url2 += jsonText[pos];

            var request = WebRequest.Create(url1 + url2);

            return url1 + url2;
        }
    }
}
