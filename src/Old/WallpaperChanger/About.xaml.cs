using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WallpaperChanger
{
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void tbVersionLoaded(object sender, RoutedEventArgs e)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            (sender as TextBlock).Text = $"{version.Major}.{version.Minor}.{version.MajorRevision}.{version.MinorRevision}";
        }
        private void AppPageClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://ogycode.github.io/WallpaperChanger/");
            
        }
        private void AuthorPageClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://verloka.github.io/");
        }
    }
}
