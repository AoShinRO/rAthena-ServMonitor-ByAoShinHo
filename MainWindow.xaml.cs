using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static AoShinhoServ_Monitor.AnimationHelper;
using static AoShinhoServ_Monitor.Consts;
using static AoShinhoServ_Monitor.ParagraphHelper;
using static AoShinhoServ_Monitor.ProcessManager;
using Application = System.Windows.Application;
using MenuItem = System.Windows.Forms.MenuItem;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

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
            lb_debug.Text = "Debug: 0";
            lb_sql.Text = "SQL: 0";
            lb_error.Text = "Error: 0";
            lb_warning.Text = "Warning: 0";
            LoginBox.Document.Blocks.Clear();
            CharBox.Document.Blocks.Clear();
            MapBox.Document.Blocks.Clear();
            WebBox.Document.Blocks.Clear();
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

            rAthenaData Data = new rAthenaData();

            int endIndex = e.Data.IndexOf("]");

            if (endIndex != -1)
            {
                Data.Header = e.Data.Substring(0, endIndex + 1);
                Data.Body = e.Data.Substring(endIndex + 1);
                if (Data.Header == "[Status]")
                {
                    if (e.Data.Contains("set users"))
                        Data.Header = "[Users]";
                    else if (Properties.Settings.Default.DebugMode && e.Data.Contains("Loading"))
                        return;
                }
            }
            else
            {
                Data.Header = "";
                Data.Body = e.Data;
                if (LastErrorLog.Header == "[Error]")
                    Add_ErrorLog(Data);
            }

            Data.Paint = GetMessageTypeColor(Data);

            LastErrorLog = Data;

            #endregion preprocessinginfo

            #region SwitchProcess

            switch (GetProcessType((Process)sender))
            {
                case rAthena.LoginSv:
                    Proc_Data2Box(LoginBox, Data);
                    break;

                case rAthena.CharSv:
                    Proc_Data2Box(CharBox, Data);
                    break;

                case rAthena.WebSv:
                    Proc_Data2Box(WebBox, Data);
                    break;

                default:
                    Proc_Data2Box(MapBox, Data);
                    break;
            }

            #endregion SwitchProcess
        }

        public void Proc_Data2Box(RichTextBox ThisBox, rAthenaData Data)
        {
            Application.Current.Dispatcher?.InvokeAsync(() =>
            {
                switch (Data.Header)
                {
                    case "[Error]":
                        CounterError++;
                        lb_error.Text = "Error: " + CounterError;
                        Add_ErrorLog(Data);
                        break;

                    case "[Debug]":
                        CounterDebug++;
                        lb_debug.Text = "Debug: " + CounterDebug;
                        Add_ErrorLog(Data);
                        break;

                    case "[SQL]":
                        CounterSql++;
                        lb_sql.Text = "SQL: " + CounterSql;
                        Add_ErrorLog(Data);
                        break;

                    case "[Warning]":
                        CounterWarning++;
                        lb_warning.Text = "Warning: " + CounterWarning;
                        Add_ErrorLog(Data);
                        break;

                    case "[Users]":
                        string[] playercount = Data.Body.Split(new Char[] { ':' });
                        lb_online.Text = playercount[2];
                        CounterOnline = short.Parse(lb_online.Text);
                        Task.Run(() => UpdateContextMenu());
                        break;

                    default:
                        break;
                }
                ThisBox.Document.Blocks.Add(AppendColoredText(Data));
            });
        }

        public void Proc_HasExited(object sender, EventArgs e)
        {
            Application.Current.Dispatcher?.InvokeAsync(() =>
            {
                switch (GetProcessType((Process)sender))
                {
                    case rAthena.LoginSv:
                        LoginBox.AppendText(Environment.NewLine + ">>Login Server - stopped<<");
                        break;

                    case rAthena.CharSv:
                        CharBox.AppendText(Environment.NewLine + ">>Char Server - stopped<<");
                        break;

                    case rAthena.WebSv:
                        WebBox.AppendText(Environment.NewLine + ">>Web Server - stopped<<");
                        break;

                    default:
                        MapBox.AppendText(Environment.NewLine + ">>Map Server - stopped<<");
                        break;
                }
            });
        }

        #endregion ProcesingInfo

        private void RTB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => ((RichTextBox)sender).ScrollToEnd();

        #endregion CoreFunctions

        private void Do_Starting_Message()
        {
            Brush color = GetWhiteModeColor();

            Do_Clear_All();

            rAthenaData Data = new rAthenaData
            {
                Header = "[Info]: ",
                Paint = color,
                Body = "Login Server is Waiting..."
            };
            Starting_Message_sub(LoginBox, Data);

            Data.Body = "Char Server is Waiting...";
            Starting_Message_sub(CharBox, Data);

            Data.Body = "Web Server is Waiting...";
            Starting_Message_sub(WebBox, Data);

            Data.Body = "Map Server is Waiting...";
            Starting_Message_sub(MapBox, Data);
        }

        private void Starting_Message_sub(System.Windows.Controls.RichTextBox Box, rAthenaData Data) => Box.Document.Blocks.Add(AppendColoredText(Data));

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
            Brush Foreground = GetWhiteModeColor();
            Brush Background = GetWhiteModeColor(true);

            MapBox.Background =
            CharBox.Background =
            LoginBox.Background =
            WebBox.Background = Background;

            MapText.Foreground =
            LoginText.Foreground =
            CharText.Foreground =
            WebText.Foreground = Foreground;

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

        public void Add_ErrorLog(rAthenaData Data)
        {
            errorLogs.Add(new rAthenaError { Header = Data.Header, Body = Data.Body });
            Task.Run(() => UpdateContextMenu());
        }

        #endregion LogWinRelated

        #region tray

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.Main_Icon,
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
                    case "Error":
                        menuItem.Text = $"Error: {CounterError}";
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
            if (!CheckServerPath())
            {
                OnOff = false;
                StartGrid.Visibility = Visibility.Visible;
                RestartGrid.Visibility = Visibility.Collapsed;
                return;
            }
            try
            {
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
                LogWin.LogsRTB.AppendText(Environment.NewLine + $"{log.Header} {log.Body}");
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