using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WallpaperChangerLib;
using System.Timers;

namespace WallpaperChanger
{
    public partial class MainWindow : Window
    {
        Wallpaper wallpaper;
        System.Windows.Forms.NotifyIcon notifyIcon;
        Timer timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        async void ApplyWallpaper()
        {
            if (Web.GetConnection())
            {
                Worker.Set(new Uri(await wallpaper.WallpaperPath()), (WallpaperChangerLib.Style)cbStyle.SelectedIndex);
                Properties.Settings.Default.DayUpdate = DateTime.Now.Day;
                Properties.Settings.Default.Save();
            }
            else
            {
                if (Properties.Settings.Default.ShowNoInetMessage)
                    MessageBox.Show("Nope internet! Update the wallpaper can not be", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        void CreateIcon()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "Wallpaper Changer";
            notifyIcon.Icon = Properties.Resources.icon;
            notifyIcon.DoubleClick += NotifyIconDoubleClick;

            notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Show window").Click += MainWindowNotifyIconOpenClick;
            notifyIcon.ContextMenuStrip.Items.Add("Update wallpaper").Click += MainWindowUpdateWidgetClick; ;
            notifyIcon.ContextMenuStrip.Items.Add("-");
            notifyIcon.ContextMenuStrip.Items.Add("About").Click += MainWindowInfoClick;
            notifyIcon.ContextMenuStrip.Items.Add("-");
            notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += MainWindowNotifyIconExitClick;

            notifyIcon.Visible = true;
        }
        double GetMilisec(int minunte)
        {
            return 60000 * minunte;
        }

        //common
        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            cbSource.SelectedIndex = Properties.Settings.Default.WallpaperSource;
            cbSource.SelectionChanged += CbSourceSelectionChanged;

            cbStyle.SelectedIndex = Properties.Settings.Default.WallpaperStyle;
            cbStyle.SelectionChanged += CbStyleSelectionChanged;

            cbTime.SelectedIndex = Properties.Settings.Default.TimeUpdate;
            cbTime.SelectionChanged += CbTimeSelectionChanged;

            cbShowNoItem.IsChecked = Properties.Settings.Default.ShowNoInetMessage;
            cbShowNoItem.Click += CbShowNoItemClick;

            cbUpdateStart.IsChecked = Properties.Settings.Default.UpdateWithStart;
            cbUpdateStart.Click += CbUpdateStartClick;

            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if ((string)key.GetValue("Wallpaper Changer") == null)
                cbStartup.IsChecked = false;
            else
                cbStartup.IsChecked = true;
            cbStartup.Click += CbStartupClick;

            switch (cbSource.SelectedIndex)
            {
                default:
                case 0:
                    wallpaper = new Bing();
                    break;
            }
            wallpaper.Completed += WallpaperCompleted;

            Application.Current.Exit += CurrentExit;
            timer = new Timer(GetMilisec(60));
            timer.Elapsed += TimerElapsed;
            timer.Enabled = true;
            CreateIcon();

            if (Properties.Settings.Default.UpdateWithStart)
                ApplyWallpaper();
        }
        private void CurrentExit(object sender, ExitEventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            notifyIcon = null;
            timer.Dispose();
            timer = null;
        }
        private void windowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        private void WallpaperCompleted()
        {
            btnUpdateWallpaper.IsEnabled = true;
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            DateTime dt = DateTime.Now;

            if (dt.Hour >= cbTime.SelectedIndex && dt.Day != Properties.Settings.Default.DayUpdate)
                ApplyWallpaper();
        }
        //settings
        private void CbSourceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.WallpaperSource = cbSource.SelectedIndex;
                Properties.Settings.Default.Save();
            }
            catch { }
        }
        private void CbStyleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.WallpaperStyle = cbStyle.SelectedIndex;
                Properties.Settings.Default.Save();
            }
            catch { }
        }
        private void CbTimeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.TimeUpdate = cbTime.SelectedIndex;
                Properties.Settings.Default.Save();
            }
            catch { }
        }
        private void CbShowNoItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.ShowNoInetMessage = cbShowNoItem.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
            catch { }
        }
        private void CbUpdateStartClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.UpdateWithStart = cbUpdateStart.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
            catch { }
        }
        private void CbStartupClick(object sender, RoutedEventArgs e)
        {
            string cmd = cbUpdateStart.IsChecked.Value ? "-silent -update" : "-silent";

            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (!cbStartup.IsChecked.Value)
                key.DeleteValue("Wallpaper Changer", false);
            else
                key.SetValue("Wallpaper Changer", $"\"{Assembly.GetExecutingAssembly().Location}\" " + cmd);
        }
        //buttons
        private void btnUpdateWallpaperClick(object sender, RoutedEventArgs e)
        {
            ApplyWallpaper();
            btnUpdateWallpaper.IsEnabled = false;
        }
        //notify icon
        private void MainWindowUpdateWidgetClick(object sender, EventArgs e)
        {
            ApplyWallpaper();
        }
        private void MainWindowNotifyIconOpenClick(object sender, EventArgs e)
        {
            Show();
        }
        private void MainWindowInfoClick(object sender, EventArgs e)
        {
            new About().ShowDialog();
        }
        private void MainWindowNotifyIconExitClick(object sender, EventArgs e)
        {
            Application.Current.Shutdown(0);
        }
        private void NotifyIconDoubleClick(object sender, EventArgs e)
        {
            Show();
        }
    }
}
