using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WallpaperChanger.Controlls
{
    public partial class FavoritePic : UserControl
    {
        public event Action<string> ApplyEvent;
        public event Action<string> DeleteEvent;

        public string Wallpaper
        {
            get { return (string)GetValue(WallpaperProperty); }
            set { SetValue(WallpaperProperty, value); }
        }
        public static readonly DependencyProperty WallpaperProperty = DependencyProperty.Register("Wallpaper", typeof(string), typeof(FavoritePic), null);

        public FavoritePic()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnApplyClick(object sender, RoutedEventArgs e) => ApplyEvent?.Invoke(Wallpaper);
        private void btnRemoveClick(object sender, RoutedEventArgs e) => DeleteEvent?.Invoke(Wallpaper);
    }
}
