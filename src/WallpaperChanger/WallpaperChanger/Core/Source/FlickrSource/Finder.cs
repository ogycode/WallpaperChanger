using FlickrNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperChanger.Core.Source.FlickrSource
{
    public static class Finder
    {
        static PhotoCollection photos;
        static double width, height;
        static string last;
        static List<string> ids;
        static string UsedListPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\Data\usedflikr.json";

        public static Task<Tuple<string, string, string>> FindAsync(double w, double h, string l, string tags)
        {
            return Task.Factory.StartNew(() =>
            {
                width = w;
                height = h;
                last = l;

                if (ids == null)
                    LoadUsed();

                var options = new PhotoSearchOptions
                {
                    Tags = tags,
                    TagMode = TagMode.AnyTag,
                    PerPage = 20,
                    Page = 1,
                    SafeSearch = SafetyLevel.None,
                    Extras = PhotoSearchExtras.AllUrls | PhotoSearchExtras.Description,
                    SortOrder = PhotoSearchSortOrder.DatePostedDescending
                };

                return getPhotoCollection(new Flickr(ApiKeys.FlikrAPIKey), options);
            });
        }
        public static void SaveUsed()
        {
            if (File.Exists(UsedListPath))
                File.Delete(UsedListPath);

            using (StreamWriter sw = File.CreateText(UsedListPath))
                sw.Write(JsonConvert.SerializeObject(ids));
        }

        static Tuple<string, string, string> getPhotoCollection(Flickr f, PhotoSearchOptions ops)
        {
            try
            {
                photos = f.PhotosSearch(ops);
            }
            catch (Exception e)
            {
                throw;
            }



            foreach (var item in photos)
            {
                if (!item.DoesLargeExist || ids.Contains(item.PhotoId))
                    continue;

                if (width <= 1600)
                {
                    if (item.Large1600Height >= height && item.Large1600Width >= width && item.Large1600Url != last)
                    {
                        ids.Add(item.PhotoId);
                        return new Tuple<string, string, string>(item.Large1600Url, item.SmallUrl, item.Description);
                    }
                }
                else
                {
                    if (item.Large2048Height >= height && item.Large2048Width >= width && item.Large2048Url != last)
                    {
                        ids.Add(item.PhotoId);
                        return new Tuple<string, string, string>(item.Large2048Url, item.SmallUrl, item.Description);
                    }
                }
            }

            ops.Page = ops.Page + 1;
            return getPhotoCollection(f, ops);
        }
        static void LoadUsed()
        {
            if (File.Exists(UsedListPath))
                using (StreamReader sr = File.OpenText(UsedListPath))
                    ids = JsonConvert.DeserializeObject<List<string>>(sr.ReadToEnd());
            else
                ids = new List<string>();
        }
    }
}
