using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Wallpapers.Service
{
    public partial class Brain : ServiceBase
    {
        System.Timers.Timer timer;

        int source = -1, update = -1;
        int timeout = 20;

        public Brain()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            RegistryKey Key = Registry.LocalMachine.CreateSubKey($"SOFTWARE\\WallpaperChanger2\\Services data");

            int.TryParse(Key.GetValue("SOURCE", 0).ToString(), out source);
            int.TryParse(Key.GetValue("UPDATE", 0).ToString(), out update);

            timer = new System.Timers.Timer(1000 * 60 * timeout)
            {
                AutoReset = true
            };
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }
        protected override void OnStop()
        {
            timer.Stop();
            timer = null;
        }
        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            switch (source)
            {
                default:
                    break;
            }
        }
    }
}
