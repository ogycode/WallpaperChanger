using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace WallpaperChangerSilentRun
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = $@"{Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName}\WallpaperChanger\WallpaperChanger.exe",
                    Arguments = "-sl"
                };
                Process.Start(startInfo);
                Current.Shutdown(0);
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.Message, ee.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown(1);
            }
        }
    }
}
