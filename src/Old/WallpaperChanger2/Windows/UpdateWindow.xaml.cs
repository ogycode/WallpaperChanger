using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Verloka.HelperLib.Update;

namespace WallpaperChanger2.Windows
{
    public partial class UpdateWindow : Window
    {
        bool Update = false;
        string path = System.IO.Path.GetTempPath();

        public UpdateWindow(UpdateItem item)
        {
            InitializeComponent();

            DownloadClient dc = new DownloadClient(item.Files, path);
            dc.DownloadProgress += Dc_DownloadProgress;
            dc.DownloadCompleted += Dc_DownloadCompleted;

            dc.Start();
        }

        #region Window Events
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch { }
        }
        private void btnCloseClick()
        {
            if (Update)
                Close();
        }
        private void btnMinimazeClick()
        {
            WindowState = WindowState.Minimized;
        }
        #endregion

        private void mywindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Update)
                e.Cancel = true;
        }
        private void Dc_DownloadCompleted()
        {
            Update = true;

            Process.Start($"{path}setup.msi");
            Application.Current.Shutdown(0);
        }
        private void Dc_DownloadProgress(string name, int perc, double speed)
        {
            pbProgress.Value = perc;
        }
    }
}
