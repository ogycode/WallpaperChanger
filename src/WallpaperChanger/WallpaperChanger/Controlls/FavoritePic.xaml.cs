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
        public event Action<string, string, string> ApplyEvent;
        public event Action<string, string> DeleteEvent;

        public string Thumbnail
        {
            get { return (string)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }
        public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register("Thumbnail", typeof(string), typeof(FavoritePic), null);

        public string ThumbnailLocale
        {
            get { return (string)GetValue(ThumbnailLocalProperty); }
            set { SetValue(ThumbnailLocalProperty, value); }
        }
        public static readonly DependencyProperty ThumbnailLocalProperty = DependencyProperty.Register("ThumbnailLocale", typeof(string), typeof(FavoritePic), null);

        public string Original
    {
            get { return (string)GetValue(OriginalProperty); }
            set { SetValue(OriginalProperty, value); }
        }
        public static readonly DependencyProperty OriginalProperty = DependencyProperty.Register("Original", typeof(string), typeof(FavoritePic), null);

        public string Copyright
        {
            get { return (string)GetValue(CopyrightProperty); }
            set { SetValue(CopyrightProperty, value); }
        }
        public static readonly DependencyProperty CopyrightProperty = DependencyProperty.Register("Copyright", typeof(string), typeof(FavoritePic), null);

        public FavoritePic()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnApplyClick(object sender, RoutedEventArgs e) => ApplyEvent?.Invoke(Original, ThumbnailLocale, Copyright);
        private void btnRemoveClick(object sender, RoutedEventArgs e) => DeleteEvent?.Invoke(Original, ThumbnailLocale);
    }
}
