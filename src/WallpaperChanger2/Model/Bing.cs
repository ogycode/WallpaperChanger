using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace WallpaperChanger2.Model
{
    public static class Bing
    {

        static async Task<string> GetUrl()
        {
            HttpClient client = new HttpClient();
            string url = "http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1";
            HttpResponseMessage response = await client.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
        public static async Task<string> ImageUrl()
        {
            string jsonText = await GetUrl();
            int pos = jsonText.IndexOf("\"url\":\"");
            string url1 = "http://bing.com/", url2 = "";
            pos += 6;
            while (jsonText[++pos] != '"') url2 += jsonText[pos];

            var request = WebRequest.Create(url1 + url2);

            double w = SystemParameters.VirtualScreenWidth;
            double h = SystemParameters.VirtualScreenHeight;

            string name = Regex.Match((url1 + url2), @"(?<=_)(.*)(?=jpg)").ToString();
            string resolution = Regex.Match(name, @"(?<=_)(.*)(?=.)").ToString();

            return (url1 + url2).Replace(resolution, $"{Math.Round(w)}x{Math.Round(h)}"); ;
        }
    }
}