using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
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
using System.Windows.Shapes;

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
        public enum SM
        {
            Login,
            Char,
            Web,
            Map
        }

        public string OpenPathDialogBox(SM type)
        {
            OpenFileDialog box;
            switch (type){ 
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
            {
                return box.FileName;
            }

            return "";
        }

        private void Okaylbl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Cancellbl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void MapExePath_Click(object sender, RoutedEventArgs e)
        {
            MapPath.Text = OpenPathDialogBox(SM.Map);
            if(File.Exists(MapPath.Text))
                Properties.Settings.Default.MapPath = MapPath.Text;
        }

        private void WebExePath_Click(object sender, RoutedEventArgs e)
        {
            WebPath.Text = OpenPathDialogBox(SM.Web);
            if(File.Exists(WebPath.Text))
                Properties.Settings.Default.WebPath = WebPath.Text;
        }

        private void LoginExePath_Click(object sender, RoutedEventArgs e)
        {
            LoginPath.Text = OpenPathDialogBox(SM.Login);
            if(File.Exists(LoginPath.Text))
                Properties.Settings.Default.LoginPath = LoginPath.Text;
        }

        private void CharExePath_Click(object sender, RoutedEventArgs e)
        {
            CharPath.Text = OpenPathDialogBox(SM.Char);
            if(File.Exists(CharPath.Text))
                Properties.Settings.Default.CharPath = CharPath.Text;
        }
    }
}