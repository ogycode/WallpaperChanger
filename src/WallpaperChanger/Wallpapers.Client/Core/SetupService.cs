using Microsoft.Win32;

namespace Wallpapers.Client.Core
{
    public static class SetupService
    {
        static RegistryKey Key;

        public static void Initialize() => Key = Registry.LocalMachine.CreateSubKey($"SOFTWARE\\WallpaperChanger2\\Services data");

        //0 - bing
        //1 - 500px
        //2 - unsplash
        public static void SetupSource(int i) => Key.SetValue("SOURCE", i);

        //0 - once day
        public static void SetupUpdateFrequency(int i) => Key.SetValue("UPDATE", i);
    }
}
