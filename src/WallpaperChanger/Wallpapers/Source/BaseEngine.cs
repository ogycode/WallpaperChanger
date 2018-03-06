using System;
using System.Drawing;

namespace Wallpapers.Source
{
    public class BaseEngine : IEngine
    {
        public const string DEFAULT_IMAGE_URL = "https://github.com/ogycode/WallpaperChanger/blob/master/merch/logo.jpg";

        /// <summary>
        /// Name of the source
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Size of screen
        /// </summary>
        public Point ScreenSize { get; }

        public BaseEngine() : this(480, 640, "NaN") { }
        public BaseEngine(int width, int height) : this(width, height, "NaN") { }
        public BaseEngine(int width, int height, string name)
        {
            ScreenSize = new Point(width, height);
            Name = name;
        }

        /// <summary>
        /// Getting image uid for compare with other image from this or other source
        /// </summary>
        /// <returns></returns>
        public virtual string GetImageUid()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Getting image url for download
        /// </summary>
        /// <returns>Url to image</returns>
        public virtual string GetImageUrl()
        {
            throw new NotImplementedException();
        }
    }
}
