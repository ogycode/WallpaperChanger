using System.Windows;

namespace WallpaperChanger2
{
    public partial class App : Application
    {
        void AppStartup(object sender, StartupEventArgs e)
        {
            bool silent = false;
            for (int i = 0; i != e.Args.Length; ++i)
                if (e.Args[i] == "-silent")
                    silent = true;


            //INIT block
            /*Settings = new Verloka.HelperLib.Settings.RegSettings("Weather Widget 2");
            UpdateTheme(Settings.GetValue("Theme", 0));
            UpdateLang(Settings.GetValue("Language", "English"));*/

            MainWindow mainWindow = new MainWindow();
            mainWindow.WindowState = silent ? WindowState.Minimized : WindowState.Normal;
            mainWindow.Show();
            if (silent)
                mainWindow.Hide();
        }
    }
}
