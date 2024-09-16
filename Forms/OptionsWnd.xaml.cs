using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static AoShinhoServ_Monitor.Consts;

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
                case rAthena.LoginSv:
                    box = new OpenFileDialog { Filter = @"login-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                case rAthena.CharSv:
                    box = new OpenFileDialog { Filter = @"char-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                case rAthena.WebSv:
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
                case rAthena.LoginSv: return Properties.Settings.Default.LoginPath;
                case rAthena.CharSv: return Properties.Settings.Default.CharPath;
                case rAthena.WebSv: return Properties.Settings.Default.WebPath;
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

        private void MapExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(MapPath, rAthena.MapSv);

        private void WebExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(WebPath, rAthena.WebSv);

        private void LoginExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(LoginPath, rAthena.LoginSv);

        private void CharExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(CharPath, rAthena.CharSv);

        private void SaverAthenaFilePath(TextBox Box,rAthena Types)
        {
            Box.Text = OpenPathDialogBox(Types);
            switch (Types)
            {
                case rAthena.LoginSv:
                    Properties.Settings.Default.LoginPath = Box.Text;
                    break;

                case rAthena.CharSv:
                    Properties.Settings.Default.CharPath = Box.Text;
                    break;

                case rAthena.WebSv:
                    Properties.Settings.Default.WebPath = Box.Text;
                    break;

                default:
                    Properties.Settings.Default.MapPath = Box.Text;
                    break;
                    
            }
        }

        #endregion Btn_Related


    }
}