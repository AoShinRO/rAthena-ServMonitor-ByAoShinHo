using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Application = System.Windows.Application;
using MenuItem = System.Windows.Forms.MenuItem;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using static AoShinhoServ_Monitor.MainDefs;
using static AoShinhoServ_Monitor.AnimationHelper;
using static AoShinhoServ_Monitor.ProcessManager;
using static AoShinhoServ_Monitor.ParagraphHelper;

namespace AoShinhoServ_Monitor
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeSubWinComponent();
            InitializeNotifyIcon();
            GetButtonPosition();
            Do_White_Mode();
        }

        #region AnimationFunctions

        private void GetButtonPosition()
        {
            StartMargin = StartGrid.Margin;
            StopMargin = StopGrid.Margin;
            OptionMargin = OptGrid.Margin;
            RestartMargin = RestartGrid.Margin;
            OptionCancelMargin = OptWin.CancelGrid.Margin;
            OptionSaveMargin = OptWin.OkayGrid.Margin;
        }

        #endregion AnimationFunctions

        #region CoreFunctions

        public void Do_Clear_All()
        {
            CounterDebug = 0;
            CounterSql = 0;
            CounterError = 0;
            CounterWarning = 0;
            lb_online.Text = "0";
            lb_debug.Text = "Debug: " + CounterDebug;
            lb_sql.Text = "SQL: " + CounterSql;
            lb_error.Text = "Error: " + CounterError;
            lb_warning.Text = "Warning: " + CounterWarning;
            Login.Document.Blocks.Clear();
            Char.Document.Blocks.Clear();
            Map.Document.Blocks.Clear();
            Web.Document.Blocks.Clear();
            errorLogs.Clear();
            LogWin.LogsRTB.Document.Blocks.Clear();
        }

        public void Do_Run_All()
        {
            try
            {
                Task.Run(() => RunWithRedirect(Properties.Settings.Default.MapPath));
                Task.Run(() => RunWithRedirect(Properties.Settings.Default.LoginPath));
                Task.Run(() => RunWithRedirect(Properties.Settings.Default.CharPath));
                Task.Run(() => RunWithRedirect(Properties.Settings.Default.WebPath));                
            }
            catch
            {
            }
        }

        public void RunWithRedirect(string cmdPath)
        {
            try
            {
                Process process = new Process()
                {
                    StartInfo =
                        {
                            FileName = cmdPath,
                            UseShellExecute = false,
                            WorkingDirectory = cmdPath.Substring(0, cmdPath.LastIndexOf('\\')),
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        },
                    EnableRaisingEvents = true
                };

                process.StartInfo.CreateNoWindow = true;
                process.ErrorDataReceived += new DataReceivedEventHandler(Proc_DataReceived);
                process.OutputDataReceived += new DataReceivedEventHandler(Proc_DataReceived);
                process.Exited += new EventHandler(Proc_HasExited);
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }
        }
        #region ProcesingInfo

        public void Proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            #region preprocessinginfo

            rAthenaData thisdata = new rAthenaData();

            int endIndex = e.Data.IndexOf("]");

            if (endIndex != -1)
            {
                thisdata.type = e.Data.Substring(0, endIndex + 1);
                thisdata.info = e.Data.Substring(endIndex + 1);
                if (thisdata.type == "[Status]")
                {
                    if (e.Data.Contains("set users"))
                    {
                        thisdata.type = "[Users]";
                        string[] playercount = e.Data.Split(new Char[] { ':' });
                        Application.Current.Dispatcher?.InvokeAsync(() =>
                            {
                                lb_online.Text = playercount[2];
                                CounterOnline = short.Parse(lb_online.Text);
                                Task.Run(() => UpdateContextMenu());
                            });
                    }
                    else if (Properties.Settings.Default.DebugMode && e.Data.Contains("Loading"))
                        return;
                }
            }
            else
            {
                thisdata.type = "";
                thisdata.info = e.Data;
                if (LastErrorLog.type == "[Error]")
                    Add_ErrorLog(thisdata.type, thisdata.info);
            }

            thisdata.Color = GetMessageTypeColor(thisdata.type);

            LastErrorLog = thisdata;

            #endregion preprocessinginfo

            #region SwitchProcess

            switch (Get_process_num(((Process)sender).ProcessName.ToLowerInvariant()))
            {
                case rAthena.Login:
                    Proc_Data2Box(Login, thisdata);
                    break;

                case rAthena.Char:
                    Proc_Data2Box(Char, thisdata);
                    break;

                case rAthena.Web:
                    Proc_Data2Box(Web, thisdata);
                    break;

                default:
                    Proc_Data2Box(Map, thisdata);
                    break;
            }

            #endregion SwitchProcess
        }

        public void Proc_Data2Box(RichTextBox box, rAthenaData message)
        {
            Application.Current.Dispatcher?.InvokeAsync(() =>
            {
                switch (message.type)
                {
                    case "[Error]":
                        CounterError++;
                        lb_error.Text = "Error: " + CounterError;
                        Add_ErrorLog(message.type, message.info);
                        break;

                    case "[Debug]":
                        CounterDebug++;
                        lb_debug.Text = "Debug: " + CounterDebug;
                        Add_ErrorLog(message.type, message.info);
                        break;

                    case "[SQL]":
                        CounterSql++;
                        lb_sql.Text = "SQL: " + CounterSql;
                        Add_ErrorLog(message.type, message.info);
                        break;

                    case "[Warning]":
                        CounterWarning++;
                        lb_warning.Text = "Warning: " + CounterWarning;
                        Add_ErrorLog(message.type, message.info);
                        break;

                    default:
                        break;
                }
                box.Document.Blocks.Add(AppendColoredText($"{message.type} ", $"{message.info}", message.Color));
            });
        }

        public void Proc_HasExited(object sender, EventArgs e)
        {
            Application.Current.Dispatcher?.InvokeAsync(() =>
            {
                switch (Get_process_num(((Process)sender).ProcessName.ToLower()))
                {
                    case rAthena.Login:
                        Login.AppendText(Environment.NewLine + ">>Login Server - stopped<<");
                        break;

                    case rAthena.Char:
                        Char.AppendText(Environment.NewLine + ">>Char Server - stopped<<");
                        break;

                    case rAthena.Web:
                        Web.AppendText(Environment.NewLine + ">>Web Server - stopped<<");
                        break;

                    default:
                        Map.AppendText(Environment.NewLine + ">>Map Server - stopped<<");
                        break;
                }
            });
        }

        #endregion ProcesingInfo
        private void RTB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => ((RichTextBox)sender).ScrollToEnd();

        #endregion CoreFunctions

        private void Do_Starting_Message()
        {
            Brush color = WhiteModeColor();

            Do_Clear_All();
            Login.Document.Blocks.Add(AppendColoredText("[Info] ", "Login Server is Waiting...", color));
            Char.Document.Blocks.Add(AppendColoredText("[Info] ", "Char Server is Waiting...", color));
            Map.Document.Blocks.Add(AppendColoredText("[Info] ", "Map Server is Waiting...", color));
            Web.Document.Blocks.Add(AppendColoredText("[Info] ", "Web Server is Waiting...", color));
        }

        #region OptionWinRelated

        private void InitializeSubWinComponent()
        {
            OptWin.Okaylbl.MouseDown += OptionWin_Okay;
            OptWin.Okaylbl.MouseEnter += OptionWin_Enter;
            OptWin.Okaylbl.MouseLeave += OptionWin_Leave;
            OptWin.Cancellbl.MouseDown += OptionWin_Cancel;
            OptWin.Cancellbl.MouseEnter += OptionWin_Cancel_Enter;
            OptWin.Cancellbl.MouseLeave += OptionWin_Cancel_Leave;
            OptWin.WhiteMode.Checked += OptionWin_Do_White_Mode;
            OptWin.WhiteMode.Unchecked += OptionWin_Do_White_Mode;
        }

        private void OptionWin_Do_White_Mode(object sender, RoutedEventArgs e) => Do_White_Mode();

        private void Do_White_Mode()
        {
            Brush Foreground = WhiteModeColor();
            Brush Background = WhiteModeColor(true);

            Map.Background =
            Char.Background =
            Login.Background =
            Web.Background = Background;

            WebP.Foreground =
            LoginP.Foreground =
            CharP.Foreground =
            MapP.Foreground = Foreground;

            if (!OnOff)
                Do_Starting_Message();
        }

        private void OptionWin_MouseDown(object sender, MouseButtonEventArgs e) => OptWin.Show();

        private void OptionWin_Okay(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            OptWin.Hide();
        }

        private void OptionWin_Cancel(object sender, RoutedEventArgs e) => OptWin.Hide();

        #endregion OptionWinRelated

        #region LogWinRelated

        public void Add_ErrorLog(string type, string content)
        {
            errorLogs.Add(new rAthenaError { Type = type, Content = content });
            Task.Run(() => UpdateContextMenu());
        }

        #endregion LogWinRelated

        #region tray

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.Main_Icon, // Substitua "SeuIcone" pelo nome do seu ícone nos recursos
                Visible = true,
                Text = "rAthena Server Monitor by AoShinHo."
            };
            _notifyIcon.MouseDoubleClick += (sender, e) =>
            {
                Show();
                _notifyIcon.Visible = false;
                WindowState = WindowState.Normal;
            };

            trayMenu.MenuItems.Add($"Online: {CounterOnline}");
            trayMenu.MenuItems.Add($"Error: {CounterError}");
            trayMenu.MenuItems.Add($"SQL: {CounterSql}");
            trayMenu.MenuItems.Add($"Warning: {CounterWarning}");
            trayMenu.MenuItems.Add($"Debug: {CounterDebug}");

            trayMenu.MenuItems.Add("Restore", (sender, e) =>
            {
                Show();
                _notifyIcon.Visible = false;
                WindowState = WindowState.Normal;
            });
            trayMenu.MenuItems.Add("Close", (sender, e) =>
            {
                Close();
            });
            _notifyIcon.Visible = false;
            _notifyIcon.ContextMenu = trayMenu;
        }

        private void UpdateContextMenu()
        {
            foreach (MenuItem menuItem in trayMenu.MenuItems)
            {
                string[] menuItemTextParts = menuItem.Text.Split(':');
                string variableName = menuItemTextParts[0];
                switch (variableName)
                {
                    case "Erro":
                        menuItem.Text = $"Erro: {CounterError}";
                        break;

                    case "SQL":
                        menuItem.Text = $"SQL: {CounterSql}";
                        break;

                    case "Warning":
                        menuItem.Text = $"Warning: {CounterWarning}";
                        break;

                    case "Debug":
                        menuItem.Text = $"Debug: {CounterDebug}";
                        break;

                    case "Online":
                        menuItem.Text = $"Online: {CounterOnline}";
                        break;
                }
            }

            _notifyIcon.ContextMenu = trayMenu;
        }

        #endregion tray

        #region Btn_related

        private void StartBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!CheckServerPath())
                {
                    return;
                }
                try
                {
                    Do_Kill_All();
                }
                catch { }
                Do_Clear_All();
                Do_Run_All();
            }
            catch { }
            finally
            {
                OnOff = true;
                StartGrid.Visibility = Visibility.Collapsed;
                RestartGrid.Visibility = Visibility.Visible;
            }
        }

        private void Program_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            if (!IsDragging)
                return;
            Point currentMousePoint = e.GetPosition(this);
            double offsetX = currentMousePoint.X - MousePosition.X;
            double offsetY = currentMousePoint.Y - MousePosition.Y;

            Left += offsetX;
            Top += offsetY;
        }

        private void StopBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Do_Kill_All();
            }
            catch { }
            OnOff = false;
            StartGrid.Visibility = Visibility.Visible;
            RestartGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowLogsBtn_Click(object sender, RoutedEventArgs e)
        {
            LogWin.LogsRTB.Document.Blocks.Clear();
            foreach (var log in errorLogs)
            {
                LogWin.LogsRTB.AppendText(Environment.NewLine + $"{log.Type} {log.Content}");
            }
            LogWin.Show();
        }

        private void BG_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            IsDragging = true;
            MousePosition = e.GetPosition(this);
        }

        private void BG_MouseUp(object sender, MouseButtonEventArgs e) => IsDragging = false;

        private void Do_End()
        {
            try
            {
                Do_Kill_All();
            }
            catch { }
            LogWin.Close();
            OptWin.Close();
        }

        private void XBtn_MouseDown(object sender, MouseButtonEventArgs e) => Close();

        private void MinimizeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Hide();
            _notifyIcon.Visible = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Do_End();

        #region btn_animation

        private void OptionWin_Enter(object sender, RoutedEventArgs e) => F_Grid_Animate(OptWin.OkayGrid, OptionSaveMargin, true);

        private void OptionWin_Leave(object sender, RoutedEventArgs e) => F_Grid_Animate(OptWin.OkayGrid, OptionSaveMargin);

        private void OptionWin_Cancel_Enter(object sender, RoutedEventArgs e) => F_Grid_Animate(OptWin.CancelGrid, OptionCancelMargin, true);

        private void OptionWin_Cancel_Leave(object sender, RoutedEventArgs e) => F_Grid_Animate(OptWin.CancelGrid, OptionCancelMargin);

        private void StartBtn_MouseEnter(object sender, MouseEventArgs e) => F_Grid_Animate(StartGrid, StartMargin, true);

        private void StartBtn_MouseLeave(object sender, MouseEventArgs e) => F_Grid_Animate(StartGrid, StartMargin);

        private void OptionWin_MouseEnter(object sender, MouseEventArgs e) => F_Grid_Animate(OptGrid, OptionMargin, true);

        private void OptionWin_MouseLeave(object sender, MouseEventArgs e) => F_Grid_Animate(OptGrid, OptionMargin);

        private void StopBtn_MouseEnter(object sender, MouseEventArgs e) => F_Grid_Animate(StopGrid, StopMargin, true);

        private void StopBtn_MouseLeave(object sender, MouseEventArgs e) => F_Grid_Animate(StopGrid, StopMargin);

        private void RestartBtn_MouseEnter(object sender, MouseEventArgs e) => F_Grid_Animate(RestartGrid, RestartMargin, true);

        private void RestartBtn_MouseLeave(object sender, MouseEventArgs e) => F_Grid_Animate(RestartGrid, RestartMargin);

        #endregion btn_animation

        #endregion Btn_related
    }
}