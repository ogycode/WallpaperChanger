using System;
using System.Threading.Tasks;

namespace WallpaperChanger2.Model
{
    public class Wallpaper
    {
        public event Action Completed;

        protected void completed()
        {
            Completed?.Invoke();
        }

        public virtual Task<string> WallpaperPath()
        {
            return Task.Factory.StartNew(() =>
            {
                return string.Empty;
            });
        }
    }
}
