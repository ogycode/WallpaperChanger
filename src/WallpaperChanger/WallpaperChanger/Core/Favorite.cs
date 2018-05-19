using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperChanger.Core
{
    public class Favorite
    {
        public string Original { get; set; }
        public string Thumbnail { get; set; }
        public string ThumbnailLocale { get; set; }
        public string Copyright { get; set; }

        public override string ToString()
        {
            return Original;
        }
    }
}
