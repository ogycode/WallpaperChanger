using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WallpaperChanger2.Controls
{
    public partial class MetroButton : UserControl
    {
        public event Action Click;
        bool pressed = false;

        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MetroButton), null);

        public MetroButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void gridMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pressed = true;
        }
        private void gridMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (pressed)
            {
                Click?.Invoke();
                pressed = false;
            }
        }
        private void gridMouseLeave(object sender, MouseEventArgs e)
        {
            pressed = false;
        }
    }
}
