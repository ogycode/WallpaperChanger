using System.Net.Http;
using System.Threading.Tasks;

namespace WallpaperChanger2.Model
{
    public class Bing : Wallpaper
    {
        BingImage img;

        public Bing() { }

        async Task<string> GetUrl()
        {
            HttpClient client = new HttpClient();
            string url = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1";
            HttpResponseMessage response = await client.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
        public async override Task<string> WallpaperPath()
        {
            string jsonText = await GetUrl();
            BingJson bj = Newtonsoft.Json.JsonConvert.DeserializeObject<BingJson>(jsonText);
            if (bj.Images != null && bj.Images.Count != 0)
                img = bj.Images[0];

            completed();
            return $"http://www.bing.com{img.Url}";
        }
    }
}