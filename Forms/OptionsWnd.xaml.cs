using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace AoShinhoServ_Monitor.Forms
{
    /// <summary>
    /// Lógica interna para OptionsWnd.xaml
    /// </summary>
    public partial class OptionsWnd : Window
    {
        public OptionsWnd()
        {
            InitializeComponent();
        }

        #region Enum

        public enum SM
        {
            Login,
            Char,
            Web,
            Map
        }

        #endregion Enum

        #region Btn_Related

        public string OpenPathDialogBox(SM type)
        {
            OpenFileDialog box;
            switch (type)
            {
                case SM.Login:
                    box = new OpenFileDialog { Filter = @"login-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                case SM.Char:
                    box = new OpenFileDialog { Filter = @"char-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                case SM.Web:
                    box = new OpenFileDialog { Filter = @"web-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                default:
                    box = new OpenFileDialog { Filter = @"map-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;
            }
            bool? result = box.ShowDialog();

            if (result.HasValue && result.Value)
                return box.FileName;

            switch (type)
            {
                case SM.Login: return Properties.Settings.Default.LoginPath;
                case SM.Char: return Properties.Settings.Default.CharPath;
                case SM.Web: return Properties.Settings.Default.WebPath;
                default: return Properties.Settings.Default.MapPath;
            }
        }

        private void Okaylbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Cancellbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }
        private void WhiteMode_Checked(object sender, RoutedEventArgs e)
        {           
        }

        private void WhiteMode_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void MapExePath_Click(object sender, RoutedEventArgs e)
        {
            MapPath.Text = OpenPathDialogBox(SM.Map);
            if (File.Exists(MapPath.Text))
                Properties.Settings.Default.MapPath = MapPath.Text;
        }

        private void WebExePath_Click(object sender, RoutedEventArgs e)
        {
            WebPath.Text = OpenPathDialogBox(SM.Web);
            if (File.Exists(WebPath.Text))
                Properties.Settings.Default.WebPath = WebPath.Text;
        }

        private void LoginExePath_Click(object sender, RoutedEventArgs e)
        {
            LoginPath.Text = OpenPathDialogBox(SM.Login);
            if (File.Exists(LoginPath.Text))
                Properties.Settings.Default.LoginPath = LoginPath.Text;
        }

        private void CharExePath_Click(object sender, RoutedEventArgs e)
        {
            CharPath.Text = OpenPathDialogBox(SM.Char);
            if (File.Exists(CharPath.Text))
                Properties.Settings.Default.CharPath = CharPath.Text;
        }

        #endregion Btn_Related


    }
}