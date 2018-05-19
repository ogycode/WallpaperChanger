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
    public partial class LikeButton
    {
        bool pressed;
        public event Action<bool> Clicked;

        public double LikeSize
        {
            get { return (double)GetValue(LikeSizeProperty); }
            set { SetValue(LikeSizeProperty, value); }
        }
        public static readonly DependencyProperty LikeSizeProperty = DependencyProperty.Register("LikeSize", typeof(double), typeof(LikeButton), null);

        bool isSwitched;
        public bool IsSwitched
        {
            get => isSwitched;
            set
            {
                if (value)
                    tbHeart.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                else
                    tbHeart.Foreground = new SolidColorBrush(Color.FromRgb(236, 239, 241));

                isSwitched = value;
            }
        }
        public static readonly DependencyProperty IsSwitchedProperty = DependencyProperty.Register("IsSwitched", typeof(double), typeof(LikeButton), null);

        public LikeButton()
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
                IsSwitched = !IsSwitched;
                Clicked?.Invoke(IsSwitched);
            }
        }
        private void gridMouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => pressed = false;
    }
}
