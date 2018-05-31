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
    public partial class TagControll : UserControl
    {
        bool pressed;
        public event Action<string> Clicked;

        public string TagText
        {
            get { return (string)GetValue(TagTextProperty); }
            set { SetValue(TagTextProperty, value); }
        }
        public static readonly DependencyProperty TagTextProperty = DependencyProperty.Register("TagText", typeof(string), typeof(TagControll), null);

        public SolidColorBrush TabBackground
        {
            get { return (SolidColorBrush)GetValue(TabBackgroundProperty); }
            set { SetValue(TabBackgroundProperty, value); }
        }
        public static readonly DependencyProperty TabBackgroundProperty = DependencyProperty.Register("TabBackground", typeof(SolidColorBrush), typeof(TagControll), null);

        static List<Color> colors = new List<Color>()
        {
            Color.FromRgb(244, 67, 54),
            Color.FromRgb(233, 30, 99),
            Color.FromRgb(156, 39, 176),
            Color.FromRgb(103, 58, 183),
            Color.FromRgb(63, 81, 181),
            Color.FromRgb(33, 150, 243),
            Color.FromRgb(3, 169, 244),
            Color.FromRgb(0, 188, 212),
            Color.FromRgb(0, 150, 136),
            Color.FromRgb(76, 175, 80),
            Color.FromRgb(139, 195, 74),
            Color.FromRgb(205, 220, 57),
            Color.FromRgb(255, 87, 34),
            Color.FromRgb(121, 85, 72),
            Color.FromRgb(158, 158, 158),
            Color.FromRgb(96, 125, 139)
        };

        public TagControll()
        {
            InitializeComponent();
            DataContext = this;

            TabBackground = new SolidColorBrush(colors[new Random(DateTime.Now.Millisecond).Next(colors.Count)]);

        }

        private void gridMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => pressed = true;
        private void gridMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (pressed)
            {
                pressed = false;
                Clicked?.Invoke(TagText);
            }
        }
        private void gridMouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => pressed = false;
    }
}
