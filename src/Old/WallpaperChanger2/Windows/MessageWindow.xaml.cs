using System.Windows;
using System.Windows.Input;

namespace WallpaperChanger2.Windows
{
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            InitializeComponent();
        }

        public bool? ShowDialog(string Title, string Message)
        {
            this.Title = Title;
            tbMessage.Text = Message;

            return ShowDialog();
        }

        #region Window Events
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch { }
        }
        private void btnCloseClick()
        {
            Close();
        }
        private void btnMinimazeClick()
        {
            WindowState = WindowState.Minimized;
        }
        #endregion
    }
}
