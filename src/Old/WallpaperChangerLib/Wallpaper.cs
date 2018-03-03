using System;
using System.Threading.Tasks;

namespace WallpaperChangerLib
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
            return Task.Factory.StartNew(()=>
            {
                
                return string.Empty;
            });
        }
    }
}
