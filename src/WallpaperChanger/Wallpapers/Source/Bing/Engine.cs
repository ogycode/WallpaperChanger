using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wallpapers.Source.Bing
{
    public class Engine : BaseEngine
    {
        public Engine(int width, int height) : base(width, height, "Bing") { }

        public override string GetImageUid() => ImageUrl().Result;
        public override string GetImageUrl() => ImageUrl().Result;

        async Task<string> GetUrl()
        {
            string content = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                string url = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1";
                using (HttpResponseMessage response = await client.GetAsync(url))
                    content = await response.Content.ReadAsStringAsync();
            }
            return content;
        }
        async Task<string> ImageUrl()
        {
            string ImageUrl = string.Empty;

            try
            {
                string jsonText = await GetUrl();
                int pos = jsonText.IndexOf("\"url\":\"");
                string url1 = "http://bing.com", url2 = "";
                pos += 6;
                while (jsonText[++pos] != '"') url2 += jsonText[pos];

                string name = Regex.Match((url1 + url2), @"(?<=_)(.*)(?=jpg)").ToString();
                string resolution = Regex.Match(name, @"(?<=_)(.*)(?=.)").ToString();

                ImageUrl = (url1 + url2).Replace(resolution, $"{Math.Round((double)ScreenSize.X)}x{Math.Round((double)ScreenSize.Y)}");

                var request = WebRequest.Create(ImageUrl);
                var resp = request.GetResponse();
            }
            catch
            {
                ImageUrl = DEFAULT_IMAGE_URL;
            }
            finally
            {
                ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? DEFAULT_IMAGE_URL : ImageUrl;
            }

            return ImageUrl;
        }
    }
}
