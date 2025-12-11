using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Application = System.Windows.Application;
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

        private void GetButtonPosition()
        {
            ILogging.StartMargin = StartGrid.Margin;
            ILogging.StopMargin = StopGrid.Margin;
            ILogging.OptionMargin = OptGrid.Margin;
            ILogging.RestartMargin = RestartGrid.Margin;
            ILogging.OptionCancelMargin = ILogging.OptWin.CancelGrid.Margin;
            ILogging.OptionSaveMargin = ILogging.OptWin.OkayGrid.Margin;
        }

        #region CoreFunctions

        public void Do_Clear_All()
        {
            ILogging.CounterDebug = 0;
            ILogging.CounterSql = 0;
            ILogging.CounterError = 0;
            ILogging.CounterWarning = 0;
            lb_online.Text = "0";
            lb_debug.Text = "Debug: 0";
            lb_sql.Text = "SQL: 0";
            lb_error.Text = "Error: 0";
            lb_warning.Text = "Warning: 0";
            LoginBox.Document.Blocks.Clear();
            CharBox.Document.Blocks.Clear();
            MapBox.Document.Blocks.Clear();
            WebBox.Document.Blocks.Clear();
            ILogging.errorLogs.Clear();
            ILogging.LogWin.LogsRTB.Document.Blocks.Clear();
        }

        #region ProcesingInfo
        public async Task<bool> Do_Run_All()
        {
            // Execute todos os processos em paralelo  
            var tasks = new[]
            {
                RunWithRedirectAsync(Configuration.LoginPath),
                RunWithRedirectAsync(Configuration.CharPath),
                RunWithRedirectAsync(Configuration.WebPath),
                RunWithRedirectAsync(Configuration.MapPath)
            };

            await Task.WhenAll(tasks);
            return true;
        }

        public async Task RunWithRedirectAsync(string cmdPath)
        {
            await Task.Run(() =>
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
                    throw new Exception($"{ex.Message} {ex.StackTrace}");
                }
            });
        }

        private static rAthena.Data ParseServerData(string rawData)
        {
            var data = new rAthena.Data();
            int endIndex = rawData.IndexOf("]");

            if (endIndex != -1)
            {
                data.Header = rawData.Substring(0, endIndex + 1);
                data.Body = rawData.Substring(endIndex + 1);
                if (data.Header == "[Status]")
                {
                    if (rawData.Contains("set users"))
                        data.Header = "[Users]";
                    else if ((Properties.Settings.Default.DebugMode && rawData.Contains("Loading")) ||
                             ILogging.LastErrorLog.Header == "[Status]" && rawData.Contains("Loading maps"))
                    {
                        data.Body = "";
                        return data;
                    }
                }
            }
            else
            {
                data.Header = "";
                data.Body = rawData;
            }

            data.Paint = IText.GetMessageTypeColor(data);
            return data;
        }

        public void Proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            var Data = ParseServerData(e.Data);
            if (Data.Body == "") return;
            
            ILogging.LastErrorLog = Data;

            #region SwitchProcess

            switch (IProcess.GetProcessType((Process)sender))
            {

                case rAthena.Type.Login:
                    Proc_Data2Box(LoginBox, Data);
                    break;

                case rAthena.Type.Char:
                    Proc_Data2Box(CharBox, Data);
                    break;

                case rAthena.Type.Web:
                    Proc_Data2Box(WebBox, Data);
                    break;

                default:
                    Proc_Data2Box(MapBox, Data);
                    break;
            }

            #endregion SwitchProcess
        }

        public void Proc_Data2Box(System.Windows.Controls.RichTextBox ThisBox, rAthena.Data Data)
        {
            Application.Current.Dispatcher?.BeginInvoke(
                DispatcherPriority.Background,
                (Action)(() => 
                {
                    switch (Data.Header)
                    {
                        case "[Error]":
                            ILogging.CounterError++;
                            lb_error.Text = "Error: " + ILogging.CounterError;
                            ILogging.Add_ErrorLog(Data);
                            break;

                        case "[Debug]":
                            ILogging.CounterDebug++;
                            lb_debug.Text = "Debug: " + ILogging.CounterDebug;
                            ILogging.Add_ErrorLog(Data);
                            break;

                        case "[SQL]":
                            ILogging.CounterSql++;
                            lb_sql.Text = "SQL: " + ILogging.CounterSql;
                            ILogging.Add_ErrorLog(Data);
                            break;

                        case "[Warning]":
                            ILogging.CounterWarning++;
                            lb_warning.Text = "Warning: " + ILogging.CounterWarning;
                            ILogging.Add_ErrorLog(Data);
                            break;

                        case "[Users]":
                            string[] playercount = Data.Body.Split(new Char[] { ':' });
                            lb_online.Text = playercount[2];
                            ILogging.CounterOnline = short.Parse(lb_online.Text);
                            ILogging.UpdateContextMenu();
                            break;

                        default:
                            break;
                    }
                    ThisBox.Document.Blocks.Add(IText.AppendColoredText(Data));
                }));
        }

        public void Proc_HasExited(object sender, EventArgs e)
        {
            Application.Current.Dispatcher?.InvokeAsync(() =>
            {
                switch (IProcess.GetProcessType((Process)sender))
                {
                    case rAthena.Type.Login:
                        LoginBox.AppendText(Environment.NewLine + ">>Login Server - stopped<<");
                        break;

                    case rAthena.Type.Char:
                        CharBox.AppendText(Environment.NewLine + ">>Char Server - stopped<<");
                        break;

                    case rAthena.Type.Web:
                        WebBox.AppendText(Environment.NewLine + ">>Web Server - stopped<<");
                        break;

                    default:
                        MapBox.AppendText(Environment.NewLine + ">>Map Server - stopped<<");
                        break;
                }
            });
        }

        #endregion ProcesingInfo

        // auto scrool RTB to end
        private void RTB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => ((RichTextBox)sender).ScrollToEnd();

        #endregion CoreFunctions

        #region OptionWinRelated

        private void InitializeSubWinComponent()
        {
            ILogging.OptWin.Okaylbl.MouseDown += OptionWin_Okay;
            ILogging.OptWin.Okaylbl.MouseEnter += OptionWin_Enter;
            ILogging.OptWin.Okaylbl.MouseLeave += OptionWin_Leave;
            ILogging.OptWin.Cancellbl.MouseDown += OptionWin_Cancel;
            ILogging.OptWin.Cancellbl.MouseEnter += OptionWin_Cancel_Enter;
            ILogging.OptWin.Cancellbl.MouseLeave += OptionWin_Cancel_Leave;
            ILogging.OptWin.WhiteMode.Checked += OptionWin_Do_White_Mode;
            ILogging.OptWin.WhiteMode.Unchecked += OptionWin_Do_White_Mode;
        }
        private void InitializeNotifyIcon()
        {
            ILogging._notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.Main_Icon,
                Visible = true,
                Text = "rAthena Server Monitor by AoShinHo."
            };

            ILogging._notifyIcon.MouseDoubleClick += (sender, e) =>
            {
                Show();
                ILogging._notifyIcon.Visible = false;
                WindowState = WindowState.Normal;
            };

            ILogging.trayMenu.MenuItems.Add($"Online: {ILogging.CounterOnline}");
            ILogging.trayMenu.MenuItems.Add($"Error: {ILogging.CounterError}");
            ILogging.trayMenu.MenuItems.Add($"SQL: {ILogging.CounterSql}");
            ILogging.trayMenu.MenuItems.Add($"Warning: {ILogging.CounterWarning}");
            ILogging.trayMenu.MenuItems.Add($"Debug: {ILogging.CounterDebug}");

            ILogging.trayMenu.MenuItems.Add("Restore", (sender, e) =>
            {
                Show();
                ILogging._notifyIcon.Visible = false;
                WindowState = WindowState.Normal;
            });

            ILogging.trayMenu.MenuItems.Add("Close", (sender, e) =>
            {
                Close();
            });

            ILogging._notifyIcon.Visible = false;
            ILogging._notifyIcon.ContextMenu = ILogging.trayMenu;
        }

        private void OptionWin_Do_White_Mode(object sender, RoutedEventArgs e) => Do_White_Mode();

        public void Do_White_Mode()
        {
            Brush Foreground = IText.GetWhiteModeColor();
            Brush Background = IText.GetWhiteModeColor(true);

            MapBox.Background =
            CharBox.Background =
            LoginBox.Background =
            WebBox.Background = Background;

            MapText.Foreground =
            LoginText.Foreground =
            CharText.Foreground =
            WebText.Foreground = Foreground;

            if (!ILogging.OnOff)
            {
                Do_Clear_All();
                IText.Do_Starting_Message(CharBox,LoginBox,MapBox,WebBox);
            }
        }

        private void OptionWin_MouseDown(object sender, MouseButtonEventArgs e) => ILogging.OptWin.Show();

        private void OptionWin_Okay(object sender, RoutedEventArgs e)
        {
            Configuration.Save();
            ILogging.OptWin.Hide();
        }

        private void OptionWin_Cancel(object sender, RoutedEventArgs e) => ILogging.OptWin.Hide();

        #endregion OptionWinRelated

        #region Btn_related

        private async void StartBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IProcess.CheckServerPath())
            {
                ILogging.OnOff = false;
                StartGrid.Visibility = Visibility.Visible;
                RestartGrid.Visibility = Visibility.Collapsed;
                return;
            }
            IProcess.Do_Kill_All();
            Do_Clear_All();
            if (await Do_Run_All())
            {
                ILogging.OnOff = true;
                StartGrid.Visibility = Visibility.Collapsed;
                RestartGrid.Visibility = Visibility.Visible;
            }    
        }

        private void Program_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            if (!ILogging.IsDragging)
                return;
            Point currentMousePoint = e.GetPosition(this);
            double offsetX = currentMousePoint.X - ILogging.MousePosition.X;
            double offsetY = currentMousePoint.Y - ILogging.MousePosition.Y;

            Left += offsetX;
            Top += offsetY;
        }

        private void StopBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IProcess.Do_Kill_All())
            {
                ILogging.OnOff = false;
                StartGrid.Visibility = Visibility.Visible;
                RestartGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowLogsBtn_Click(object sender, RoutedEventArgs e)
        {
            ILogging.LogWin.LogsRTB.Document.Blocks.Clear();
            foreach (var log in ILogging.errorLogs)
            {
                ILogging.LogWin.LogsRTB.AppendText(Environment.NewLine + $"{log.Header} {log.Body}");
            }
            ILogging.LogWin.Show();
        }

        private void BG_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            ILogging.IsDragging = true;
            ILogging.MousePosition = e.GetPosition(this);
        }

        private void BG_MouseUp(object sender, MouseButtonEventArgs e) => ILogging.IsDragging = false;

        private void Do_End()
        {
            IProcess.Do_Kill_All();
            Configuration.Save();
            ILogging.LogWin.Close();
            ILogging.OptWin.Close();
        }

        private void XBtn_MouseDown(object sender, MouseButtonEventArgs e) => Close();

        private void MinimizeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Hide();
            ILogging._notifyIcon.Visible = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Do_End();

        #region btn_animation

        private void OptionWin_Enter(object sender, RoutedEventArgs e) => IAnimation.F_Grid(ILogging.OptWin.OkayGrid, ILogging.OptionSaveMargin, true);

        private void OptionWin_Leave(object sender, RoutedEventArgs e) => IAnimation.F_Grid(ILogging.OptWin.OkayGrid, ILogging.OptionSaveMargin);

        private void OptionWin_Cancel_Enter(object sender, RoutedEventArgs e) => IAnimation.F_Grid(ILogging.OptWin.CancelGrid, ILogging.OptionCancelMargin, true);

        private void OptionWin_Cancel_Leave(object sender, RoutedEventArgs e) => IAnimation.F_Grid(ILogging.OptWin.CancelGrid, ILogging.OptionCancelMargin);

        private void StartBtn_MouseEnter(object sender, MouseEventArgs e) => IAnimation.F_Grid(StartGrid, ILogging.StartMargin, true);

        private void StartBtn_MouseLeave(object sender, MouseEventArgs e) => IAnimation.F_Grid(StartGrid, ILogging.StartMargin);

        private void OptionWin_MouseEnter(object sender, MouseEventArgs e) => IAnimation.F_Grid(OptGrid, ILogging.OptionMargin, true);

        private void OptionWin_MouseLeave(object sender, MouseEventArgs e) => IAnimation.F_Grid(OptGrid, ILogging.OptionMargin);

        private void StopBtn_MouseEnter(object sender, MouseEventArgs e) => IAnimation.F_Grid(StopGrid, ILogging.StopMargin, true);

        private void StopBtn_MouseLeave(object sender, MouseEventArgs e) => IAnimation.F_Grid(StopGrid, ILogging.StopMargin);

        private void RestartBtn_MouseEnter(object sender, MouseEventArgs e) => IAnimation.F_Grid(RestartGrid, ILogging.RestartMargin, true);

        private void RestartBtn_MouseLeave(object sender, MouseEventArgs e) => IAnimation.F_Grid(RestartGrid, ILogging.RestartMargin);

        #endregion btn_animation

        #endregion Btn_related
    }
}