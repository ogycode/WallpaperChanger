using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WallpaperChanger
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool rz = false;
            if (args.Length != 0)
                rz = args[0].Equals("-s");

            Application.Run(new Form1(rz));
        }
    }
}
