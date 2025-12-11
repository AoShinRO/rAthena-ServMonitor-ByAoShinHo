using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
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

        #region Btn_Related

        public string OpenPathDialogBox(rAthena.Type type)
        {
            OpenFileDialog box;
            switch (type)
            {
                case rAthena.Type.Login:
                    box = new OpenFileDialog { Filter = @"login-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                case rAthena.Type.Char:
                    box = new OpenFileDialog { Filter = @"char-server (*.exe)|*.exe|All files (*.*)|*.*" };
                    break;

                case rAthena.Type.Web:
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
                case rAthena.Type.Login: return Configuration.LoginPath;
                case rAthena.Type.Char: return Configuration.CharPath;
                case rAthena.Type.Web: return Configuration.WebPath;
                default: return Configuration.MapPath;
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

        private void MapExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(MapPath, rAthena.Type.Map);

        private void WebExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(WebPath, rAthena.Type.Web);

        private void LoginExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(LoginPath, rAthena.Type.Login);

        private void CharExePath_Click(object sender, RoutedEventArgs e) => SaverAthenaFilePath(CharPath, rAthena.Type.Char);

        private void SaverAthenaFilePath(TextBox Box,rAthena.Type Types)
        {
            Box.Text = OpenPathDialogBox(Types);
            switch (Types)
            {
                case rAthena.Type.Login:
                    Configuration.LoginPath = Box.Text;
                    break;

                case rAthena.Type.Char:
                    Configuration.CharPath = Box.Text;
                    break;

                case rAthena.Type.Web:
                    Configuration.WebPath = Box.Text;
                    break;

                default:
                    Configuration.MapPath = Box.Text;
                    break;
                    
            }
        }


        #endregion Btn_Related

        private void DevMode_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void DevMode_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void CmakeMode_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CmakeMode_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void PreRenewalMode_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void PreRenewalMode_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}