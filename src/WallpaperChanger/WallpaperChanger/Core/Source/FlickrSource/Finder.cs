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
        public static bool IsSaving = true;
        public static bool IsLoading = true;

        static PhotoCollection photos;
        static double width, height;
        static string last;
        static List<string> ids;
        static string FLICKR_FILE_NAME = "flickr.json";

        public static Task<Tuple<string, string, string>> FindAsync(double w, double h, string l, string tags, TagMode mode = TagMode.None, List<string> colors = null, List<FlickrNet.Style> styles = null)
        {
            return Task.Factory.StartNew(() =>
            {
                width = w;
                height = h;
                last = l;

                if (ids == null)
                    LoadUsed();

                while (IsLoading)
                    Task.Delay(500).Wait();

                var options = new PhotoSearchOptions
                {
                    Tags = tags,
                    TagMode = mode,
                    ColorCodes = colors,
                    Styles = styles,
                    PerPage = 20,
                    Page = 1,
                    SafeSearch = SafetyLevel.None,
                    Extras = PhotoSearchExtras.AllUrls | PhotoSearchExtras.Description,
                    SortOrder = PhotoSearchSortOrder.DatePostedDescending
                };

                return getPhotoCollection(new Flickr(ApiKeys.FlikrAPIKey), options);
            });
        }
        public static async Task SaveUsed()
        {
            Task.Factory.StartNew(async() =>
            {
                try
                {
                    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    Windows.Storage.StorageFile flickrFile = await localFolder.CreateFileAsync(FLICKR_FILE_NAME, Windows.Storage.CreationCollisionOption.ReplaceExisting);

                    await Windows.Storage.FileIO.WriteTextAsync(flickrFile, JsonConvert.SerializeObject(ids));
                }
                finally
                {
                    IsSaving = false;
                }
            });
        }

        static Tuple<string, string, string> getPhotoCollection(Flickr f, PhotoSearchOptions ops)
        {
            try
            {
                photos = f.PhotosSearch(ops);
            }
            catch
            {
                return new Tuple<string, string, string>("www.bing.com/az/hprichbg/rb/OchaBatake_ROW10481280883_1366x768.jpg", "www.bing.com/az/hprichbg/rb/OchaBatake_ROW10481280883_400x240.jpg", "Error");
            }

            if (ids == null)
                ids = new List<string>();

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
        async static void LoadUsed()
        {
            try
            {
                Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile favoriteFile = await localFolder.TryGetItemAsync(FLICKR_FILE_NAME) as Windows.Storage.StorageFile;

                ids = favoriteFile != null ? JsonConvert.DeserializeObject<List<string>>(await Windows.Storage.FileIO.ReadTextAsync(favoriteFile)) : new List<string>();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
