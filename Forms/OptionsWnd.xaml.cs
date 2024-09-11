using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using static AoShinhoServ_Monitor.ProcessManager;

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

        #region Btn_Related

        public string OpenPathDialogBox(rAthena type)
        {
            OpenFileDialog box;
            switch (type)
            {
                case rAthena.Login:
                    box = new OpenFileDialog { Filter = @"login-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                case rAthena.Char:
                    box = new OpenFileDialog { Filter = @"char-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                case rAthena.Web:
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
                case rAthena.Login: return Properties.Settings.Default.LoginPath;
                case rAthena.Char: return Properties.Settings.Default.CharPath;
                case rAthena.Web: return Properties.Settings.Default.WebPath;
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
            MapPath.Text = OpenPathDialogBox(rAthena.Map);
            if (File.Exists(MapPath.Text))
                Properties.Settings.Default.MapPath = MapPath.Text;
        }

        private void WebExePath_Click(object sender, RoutedEventArgs e)
        {
            WebPath.Text = OpenPathDialogBox(rAthena.Web);
            if (File.Exists(WebPath.Text))
                Properties.Settings.Default.WebPath = WebPath.Text;
        }

        private void LoginExePath_Click(object sender, RoutedEventArgs e)
        {
            LoginPath.Text = OpenPathDialogBox(rAthena.Login);
            if (File.Exists(LoginPath.Text))
                Properties.Settings.Default.LoginPath = LoginPath.Text;
        }

        private void CharExePath_Click(object sender, RoutedEventArgs e)
        {
            CharPath.Text = OpenPathDialogBox(rAthena.Char);
            if (File.Exists(CharPath.Text))
                Properties.Settings.Default.CharPath = CharPath.Text;
        }

        #endregion Btn_Related


    }
}