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
    public partial class StoreButton : UserControl
    {
        bool pressed;
        public event Action Clicked;

        public SolidColorBrush BackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }
        public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register("BackgroundBrush", typeof(SolidColorBrush), typeof(StoreButton), null);

        public SolidColorBrush ForegroundBrush
        {
            get { return (SolidColorBrush)GetValue(ForegroundBrushProperty); }
            set { SetValue(ForegroundBrushProperty, value); }
        }
        public static readonly DependencyProperty ForegroundBrushProperty = DependencyProperty.Register("ForegroundBrush", typeof(SolidColorBrush), typeof(StoreButton), null);

        public ImageSource AppLogo
        {
            get { return (ImageSource)GetValue(AppLogoProperty); }
            set { SetValue(AppLogoProperty, value); }
        }
        public static readonly DependencyProperty AppLogoProperty = DependencyProperty.Register("AppLogo", typeof(ImageSource), typeof(StoreButton), null);

        public SolidColorBrush IconBrush
        {
            get { return (SolidColorBrush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(SolidColorBrush), typeof(StoreButton), null);

        public bool IsConsole
        {
            get { return ((Visibility)GetValue(IsConsoleProperty)) == Visibility.Visible; }
            set { SetValue(IsConsoleProperty, value ? Visibility.Visible : Visibility.Collapsed); }
        }
        public static readonly DependencyProperty IsConsoleProperty = DependencyProperty.Register("IsConsole", typeof(bool), typeof(StoreButton), null);

        public bool IsVR
        {
            get { return ((Visibility)GetValue(IsVRProperty)) == Visibility.Visible; }
            set { SetValue(IsVRProperty, value ? Visibility.Visible : Visibility.Collapsed); }
        }
        public static readonly DependencyProperty IsVRProperty = DependencyProperty.Register("IsVR", typeof(Visibility), typeof(StoreButton), null);

        public bool IsPC
        {
            get { return ((Visibility)GetValue(IsPCProperty)) == Visibility.Visible; }
            set { SetValue(IsPCProperty, value ? Visibility.Visible : Visibility.Collapsed); }
        }
        public static readonly DependencyProperty IsPCProperty = DependencyProperty.Register("IsPC", typeof(Visibility), typeof(StoreButton), null);

        public bool IsMobile
        {
            get { return ((Visibility)GetValue(IsMobileProperty)) == Visibility.Visible; }
            set { SetValue(IsMobileProperty, value ? Visibility.Visible : Visibility.Collapsed); }
        }
        public static readonly DependencyProperty IsMobileProperty = DependencyProperty.Register("IsMobile", typeof(Visibility), typeof(StoreButton), null);

        public SolidColorBrush BackgroundButtonBrush
        {
            get { return (SolidColorBrush)GetValue(BackgroundButtonBrushProperty); }
            set { SetValue(BackgroundButtonBrushProperty, value); }
        }
        public static readonly DependencyProperty BackgroundButtonBrushProperty = DependencyProperty.Register("BackgroundButtonBrush", typeof(SolidColorBrush), typeof(StoreButton), null);

        public SolidColorBrush ForegroundButtonBrush
        {
            get { return (SolidColorBrush)GetValue(ForegroundButtonBrushProperty); }
            set { SetValue(ForegroundButtonBrushProperty, value); }
        }
        public static readonly DependencyProperty ForegroundButtonBrushProperty = DependencyProperty.Register("ForegroundButtonBrush", typeof(SolidColorBrush), typeof(StoreButton), null);

        public string AppName
        {
            get { return (string)GetValue(AppNameProperty); }
            set { SetValue(AppNameProperty, value); }
        }
        public static readonly DependencyProperty AppNameProperty = DependencyProperty.Register("AppName", typeof(string), typeof(StoreButton), null);

        public string AppDescription
        {
            get { return (string)GetValue(AppDescriptionProperty); }
            set { SetValue(AppDescriptionProperty, value); }
        }
        public static readonly DependencyProperty AppDescriptionProperty = DependencyProperty.Register("AppDescription", typeof(string), typeof(StoreButton), null);

        public string TitleButton
        {
            get { return (string)GetValue(TitleButtonProperty); }
            set { SetValue(TitleButtonProperty, value); }
        }
        public static readonly DependencyProperty TitleButtonProperty = DependencyProperty.Register("TitleButton", typeof(string), typeof(StoreButton), null);

        public StoreButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void gridMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => pressed = true;
        private void gridMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (pressed)
            {
                pressed = false;
                Clicked?.Invoke();
            }
        }
        private void gridMouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => pressed = false;
    }
}
