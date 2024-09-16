using Microsoft.Win32;
using System.IO;
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

        public string OpenPathDialogBox(rAthenaTypes type)
        {
            OpenFileDialog box;
            switch (type)
            {
                case rAthenaTypes.LoginSv:
                    box = new OpenFileDialog { Filter = @"login-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                case rAthenaTypes.CharSv:
                    box = new OpenFileDialog { Filter = @"char-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                case rAthenaTypes.WebSv:
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
                case rAthenaTypes.LoginSv: return Properties.Settings.Default.LoginPath;
                case rAthenaTypes.CharSv: return Properties.Settings.Default.CharPath;
                case rAthenaTypes.WebSv: return Properties.Settings.Default.WebPath;
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

        private void MapExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(MapPath, rAthenaTypes.MapSv);

        private void WebExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(WebPath, rAthenaTypes.WebSv);

        private void LoginExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(LoginPath, rAthenaTypes.LoginSv);

        private void CharExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(CharPath, rAthenaTypes.CharSv);

        private void SaverAthenaFilePath(TextBox Box,rAthenaTypes Types)
        {
            Box.Text = OpenPathDialogBox(Types);
            switch (Types)
            {
                case rAthenaTypes.LoginSv:
                    Properties.Settings.Default.LoginPath = Box.Text;
                    break;

                case rAthenaTypes.CharSv:
                    Properties.Settings.Default.CharPath = Box.Text;
                    break;

                case rAthenaTypes.WebSv:
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