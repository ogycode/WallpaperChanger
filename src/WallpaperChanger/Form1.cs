using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace WallpaperChanger
{
    public partial class Form1 : Form
    {
        bool firstLang = true, fisrtSource = true, fisrtPeriod = true, fisrtWallpaper = true, fisrtTray = true, fisrtAuto = true, exit = false;

        public int Year { get { return Properties.Settings.Default.DateToUpdateY; } }
        public int Month { get { return Properties.Settings.Default.DateToUpdateM; } }
        public int Day { get { return Properties.Settings.Default.DateToUpdateD; } }
        public int Hour { get { return Properties.Settings.Default.DateToUpdateH; } }
        public int Minute { get { return Properties.Settings.Default.DateToUpdateMin; } }

        public int CurrentYear { get { return DateTime.Now.Year; } }
        public int CurrentMonth { get { return DateTime.Now.Month; } }
        public int CurrentDay { get { return DateTime.Now.Day; } }
        public int CurrentHour { get { return DateTime.Now.Hour; } }
        public int CurrentMinute { get { return DateTime.Now.Minute; } }

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Swithc language
        /// </summary>
        /// <param name="resources">ComponentResourceManager</param>
        /// <param name="ctls">ControlCollection</param>
        void ApplyResources(ComponentResourceManager resources, Control.ControlCollection ctls)
        {
            foreach (Control ctl in ctls)
            {
                resources.ApplyResources(ctl, ctl.Name);
                ApplyResources(resources, ctl.Controls);
            }
        }
        /// <summary>
        /// Apply background
        /// </summary>
        async void ApplyBackground()
        {
            if (IsTime())
            {
                Wallpaper.Set(new Uri(await Bing.GetImageUri()), (Wallpaper.Style)Properties.Settings.Default.cbWallpaperSetIndex);
                SetPeriod();
                timerPeriod.Enabled = true;
            }
        }
        /// <summary>
        /// Set startup
        /// </summary>
        /// <param name="add">True - add key, False - remove key</param>
        void Startup(bool add = false)
        {
            if (add)
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    key.SetValue("WallpaperChanger", "\"" + Application.ExecutablePath + "\" -s");
            else
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    key.DeleteValue("WallpaperChanger", false);
        }
        /// <summary>
        /// Set period of update wallpapper
        /// </summary>
        void SetPeriod()
        {
            int y, d, m, h, min;

            y = DateTime.Now.Year;
            m = DateTime.Now.Month;
            d = DateTime.Now.Day;
            h = DateTime.Now.Hour;
            min = DateTime.Now.Minute;


            d += cbPeriod.SelectedIndex + 1;

            if (d > DateTime.DaysInMonth(y, m))
            {
                d = 1;
                m++;
            }

            if (m > 12)
                y++;

            Properties.Settings.Default.DateToUpdateY = y;
            Properties.Settings.Default.DateToUpdateM = m;
            Properties.Settings.Default.DateToUpdateD = d;
            Properties.Settings.Default.DateToUpdateH = h;
            Properties.Settings.Default.DateToUpdateMin = min;

            Properties.Settings.Default.Save();
        }
        /// <summary>
        /// Check time of update
        /// </summary>
        /// <returns>True - need update, false - dont need</returns>
        bool IsTime()
        {
            if (Year <= CurrentYear)
                if (Month <= CurrentMonth)
                    if (Day <= CurrentDay)
                        if (Hour <= CurrentHour)
                            if (Minute <= CurrentMinute)
                                return true;
            return false;
        }

        private async void formLoad(object sender, EventArgs e)
        {
            cbSource.SelectedIndex = Properties.Settings.Default.cbSourceIndex;
            cbPeriod.SelectedIndex = Properties.Settings.Default.cbPeriodIndex;
            cbWallpaperSet.SelectedIndex = Properties.Settings.Default.cbWallpaperSetIndex;
            cbLang.SelectedIndex = Properties.Settings.Default.cbLangIndex;

            cbHideTray.Checked = Properties.Settings.Default.cbHideTrayChecked;
            cbAutorun.Checked = Properties.Settings.Default.cbAutorunChecked;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Properties.Settings.Default.CurrentLang);
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
            resources.ApplyResources(this, "$this");
            ApplyResources(resources, Controls);

            if (Properties.Settings.Default.cbHideTrayChecked)
            {
                Hide();
                Wallpaper.Set(new Uri(await Bing.GetImageUri()), (Wallpaper.Style)Properties.Settings.Default.cbWallpaperSetIndex);
                timerPeriod.Enabled = true;
            }
        }
        private async void btnApply_Click(object sender, EventArgs e)
        {
            SetPeriod();
            Wallpaper.Set(new Uri(await Bing.GetImageUri()), (Wallpaper.Style)Properties.Settings.Default.cbWallpaperSetIndex);
        }
        private void cbLangSelectedChanged(object sender, EventArgs e)
        {
            if (firstLang)
            {
                firstLang = false;
                return;
            }

            switch (cbLang.SelectedIndex)
            {
                case 0:
                default:
                    Properties.Settings.Default.CurrentLang = "en-US";
                    break;
                case 1:
                    Properties.Settings.Default.CurrentLang = "ru-RU";
                    break;
            }
            Properties.Settings.Default.cbLangIndex = cbLang.SelectedIndex;
            Properties.Settings.Default.Save();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Properties.Settings.Default.CurrentLang);
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
            resources.ApplyResources(this, "$this");
            ApplyResources(resources, Controls);
        }
        private void formClosing(object sender, FormClosingEventArgs e)
        {
            if (cbHideTray.Checked && !exit)
            {
                e.Cancel = true;
                Hide();
            }
        }
        private void timerPeriodTick(object sender, EventArgs e)
        {
            ApplyBackground();
        }
        private void notifyIconDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            SetPeriod();
            timerPeriod.Enabled = true;
            Hide();
        }
        private void ncpStartClick(object sender, EventArgs e)
        {
            SetPeriod();
            timerPeriod.Enabled = true;
            Hide();
        }
        private async void ncpApplyNowClick(object sender, EventArgs e)
        {
            SetPeriod();
            Wallpaper.Set(new Uri(await Bing.GetImageUri()), (Wallpaper.Style)Properties.Settings.Default.cbWallpaperSetIndex);
        }
        private void ncpExitClick(object sender, EventArgs e)
        {
            exit = true;
            Application.Exit();
        }
        private void cbAutorunClick(object sender, EventArgs e)
        {
            if (fisrtAuto)
            {
                fisrtAuto = false;
                return;
            }

            Properties.Settings.Default.cbAutorunChecked = cbAutorun.Checked;
            Properties.Settings.Default.Save();

            Startup(Properties.Settings.Default.cbAutorunChecked);
        }
        private void cbHideTrayClick(object sender, EventArgs e)
        {
            if (fisrtTray)
            {
                fisrtTray = false;
                return;
            }

            Properties.Settings.Default.cbHideTrayChecked = cbHideTray.Checked;
            Properties.Settings.Default.Save();
        }
        private void cbWallpaperSetSelectedChanged(object sender, EventArgs e)
        {
            if (fisrtWallpaper)
            {
                fisrtWallpaper = false;
                return;
            }

            Properties.Settings.Default.cbWallpaperSetIndex = cbWallpaperSet.SelectedIndex;
            Properties.Settings.Default.Save();
        }
        private void cbPeriodSelectedChanged(object sender, EventArgs e)
        {
            if (fisrtSource)
            {
                fisrtSource = false;
                return;
            }

            Properties.Settings.Default.cbPeriodIndex = cbPeriod.SelectedIndex;
            Properties.Settings.Default.Save();
        }
        private void cbSourceSelectedChanged(object sender, EventArgs e)
        {
            if (fisrtPeriod)
            {
                fisrtPeriod = false;
                return;
            }

            Properties.Settings.Default.cbSourceIndex = cbSource.SelectedIndex;
            Properties.Settings.Default.Save();
        }
    }
}
