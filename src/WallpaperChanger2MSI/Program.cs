using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using WixSharp;

namespace WallpaperChanger2MSI
{
    class Program
    {
        public const string Company = "Verloka";
        public const string Owner = "Verloka Vadim";
        public const string Product = "Wallpaper Changer 2";
        public const string RootPathThisMoment = @"C:\Projects\Windows\WallpaperChanger\src\WallpaperChanger2\bin\Release\";
        public const string ExeIconPathThisMoment = @"C:\Projects\Windows\WallpaperChanger\src\WallpaperChanger2\Icons\AppIcon.ico";
        public const string ExeNameThisMoment = "WallpaperChanger2.exe";
        public const string LicencePathThisMoment = @"C:\Projects\Windows\WallpaperChanger\src\WallpaperChanger2MSI\Lic.rtf";

        static void Main(string[] args)
        {
            ProductInfo pi = new ProductInfo();
            pi.Manufacturer = Company;
            pi.Name = Product;
            pi.Contact = Owner;
            pi.Id = Guid.NewGuid().ToString();
            pi.UrlInfoAbout = "https://ogycode.github.io/WallpaperChanger/";
            pi.UrlUpdateInfo = "https://ogycode.github.io/WallpaperChanger/";
            pi.ProductIcon = ExeIconPathThisMoment;

            Feature Conmplete = new Feature("Conmplete", "All files from project for app can work", true);

            Project prj = new Project();

            prj.Name = Product;
            prj.UI = WUI.WixUI_ProgressOnly;
            prj.ControlPanelInfo = pi;
            prj.GUID = Guid.NewGuid();
            prj.Version = new Version(2, 0, 0, 0);
            prj.LicenceFile = LicencePathThisMoment;

            Dir installDir = new Dir("%ProgramFiles%");
            Dir root = new Dir(Company);
            Dir rootProduct = getDir(RootPathThisMoment, Conmplete, "*.dll|*.exe|*.ico|*.png|*.json");
            rootProduct.Name = Product;
            root.Dirs = new Dir[] { rootProduct };
            installDir.Dirs = new Dir[] { root };

            Dir shortcuts = new Dir(@"%Desktop%",
                        new ExeFileShortcut(Product, $"[INSTALLDIR]{ExeNameThisMoment}", ""));

            prj.Dirs = new Dir[] { installDir, shortcuts };

            prj.MajorUpgradeStrategy = MajorUpgradeStrategy.Default;
            prj.MajorUpgradeStrategy.RemoveExistingProductAfter = Step.InstallInitialize;
            prj.MajorUpgradeStrategy.PreventDowngradingVersions.OnlyDetect = false;

            prj.Actions = new WixSharp.Action[]
            {
                //new InstalledFileAction(ExeNameThisMoment, "", Return.ignore, When.After, Step.InstallFinalize, Condition.NOT_Installed)
                new ManagedAction(CustonActions.RunApp, Return.ignore, When.After, Step.PreviousActionOrInstallFinalize, Condition.NOT_Installed)
            };

            Compiler.BuildMsi(prj);
        }

        static Dir getDir(string path, Feature f, string ex = "*.*")
        {
            DirectoryInfo di = new DirectoryInfo(path);
            Dir dir = new Dir(di.Name);

            List<WixSharp.File> files = new List<WixSharp.File>();
            foreach (var item in getFiles(di.FullName, ex, SearchOption.TopDirectoryOnly))
            {
                files.Add(new WixSharp.File(f, item));
            }
            dir.Files = files.ToArray();

            List<Dir> dirs = new List<Dir>();
            foreach (var item in di.GetDirectories())
            {
                dirs.Add(getDir(item.FullName, f, ex));
            }
            dir.Dirs = dirs.ToArray();

            return dir;
        }
        static string[] getFiles(string SourceFolder, string Filter, SearchOption searchOption)
        {
            ArrayList alFiles = new ArrayList();
            string[] MultipleFilters = Filter.Split('|');
            foreach (string FileFilter in MultipleFilters)
                alFiles.AddRange(Directory.GetFiles(SourceFolder, FileFilter, searchOption));
            return (string[])alFiles.ToArray(typeof(string));
        }
    }

    public static class CustonActions
    {
        [CustomAction]
        public static ActionResult RunApp(Session session)
        {
            System.Diagnostics.Process.Start($"{session["INSTALLDIR"]}\\{ Program.ExeNameThisMoment}");
            return ActionResult.Success;
        }
    }
}
