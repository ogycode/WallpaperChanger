using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace WallpaperChanger
{
    public partial class App : Application
    {
        private const string MutexName = "Wallpaper Changer 2";
        private readonly Mutex mutex;
        bool CreatedNew;

        public App()
        {
            mutex = new Mutex(true, MutexName, out CreatedNew);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Count() == 0)
            {
                if (!CreatedNew) Current.Shutdown(0);
                else new MainWindow().Show();
            }
            else if (e.Args[0] == "-sl") new MainWindow();
        }
    }
}
