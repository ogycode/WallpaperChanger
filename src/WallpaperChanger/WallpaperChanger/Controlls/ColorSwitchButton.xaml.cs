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
    public partial class ColorSwitchButton : UserControl
    {
        bool pressed;
        public event Action<bool, string> Clicked;

        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(FavoritePic), null);

        public SolidColorBrush SignColor
        {
            get { return (SolidColorBrush)GetValue(SignColorProperty); }
            set { SetValue(SignColorProperty, value); }
        }
        public static readonly DependencyProperty SignColorProperty = DependencyProperty.Register("SignColor", typeof(SolidColorBrush), typeof(FavoritePic), null);

        public bool IsSwitched
        {
            get { return (bool)GetValue(IsSwitchedProperty); }
            set
            {
                SetValue(IsSwitchedProperty, value);
                tbAccepts.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public static readonly DependencyProperty IsSwitchedProperty = DependencyProperty.Register("IsSwitched", typeof(bool), typeof(FavoritePic), null);

        public double SizeWidth
        {
            get { return (double)GetValue(SizeWidthProperty); }
            set { SetValue(SizeWidthProperty, value); }
        }
        public static readonly DependencyProperty SizeWidthProperty = DependencyProperty.Register("SizeWidth", typeof(double), typeof(FavoritePic), null);

        public double SizeHeight
        {
            get { return (double)GetValue(SizeHeightProperty); }
            set { SetValue(SizeHeightProperty, value); }
        }
        public static readonly DependencyProperty SizeHeightProperty = DependencyProperty.Register("SizeHeight", typeof(double), typeof(FavoritePic), null);

        public ImageSource SpecialImage
        {
            get { return (ImageSource)GetValue(SpecialImageProperty); }
            set { SetValue(SpecialImageProperty, value); }
        }
        public static readonly DependencyProperty SpecialImageProperty = DependencyProperty.Register("SpecialImage", typeof(ImageSource), typeof(FavoritePic), null);

        public string Code
        {
            get { return (string)GetValue(CodeProperty); }
            set { SetValue(CodeProperty, value); }
        }
        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register("Code", typeof(string), typeof(FavoritePic), null);

        public string Sign
        {
            get { return (string)GetValue(SignProperty); }
            set { SetValue(SignProperty, value); }
        }
        public static readonly DependencyProperty SignProperty = DependencyProperty.Register("Sign", typeof(string), typeof(FavoritePic), null);

        public double SignSize
        {
            get { return (double)GetValue(SignSizeProperty); }
            set { SetValue(SignSizeProperty, value); }
        }
        public static readonly DependencyProperty SignSizeProperty = DependencyProperty.Register("SignSize", typeof(double), typeof(FavoritePic), null);

        public ColorSwitchButton()
        {
            InitializeComponent();
            DataContext = this;

            IsSwitched = false;
        }

        private void gridMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => pressed = true;
        private void gridMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (pressed)
            {
                pressed = false;
                IsSwitched = !IsSwitched;
                Clicked?.Invoke(IsSwitched, Code);
            }
        }
        private void gridMouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => pressed = false;
    }
}
