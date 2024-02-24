using AoShinhoServ_Monitor.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
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

        #region structing vars

        public enum rAthena
        {
            Map,
            Login,
            Char,
            Web
        };

        public struct rAthenaError
        {
            public string Type;
            public string Content;
        }

        public struct rAthenaData
        {
            public string type;
            public string info;
            public Brush Color;
        }

        private NotifyIcon _notifyIcon;
        private readonly ContextMenu trayMenu = new ContextMenu();
        public short errormsgcount;
        public short sqlmsgcount;
        public short warningmsgcount;
        public short debugmsgcount;
        public short onlinecount;
        public bool OnOff;
        public bool IsDragging;
        public Point MousePosition;
        public Thickness StartMargin;
        public Thickness StopMargin;
        public Thickness OptionMargin;
        public Thickness RestartMargin;
        public Thickness OptionSaveMargin;
        public Thickness OptionCancelMargin;
        private readonly List<rAthenaError> errorLogs = new List<rAthenaError>();
        public rAthenaData LastErrorLog;
        public OptionsWnd OptWin = new OptionsWnd();
        public Logs LogWin = new Logs();

        #endregion structing vars

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

        private ThicknessAnimation F_Thickness_Animate(Thickness From, Thickness To, double Duration = 0.1)
        {
            ThicknessAnimation marginAnimation = new ThicknessAnimation();
            marginAnimation.GetAnimationBaseValue(MarginProperty);
            marginAnimation.From = From;
            marginAnimation.To = To;
            marginAnimation.Duration = TimeSpan.FromSeconds(Duration);
            return marginAnimation;
        }

        private Thickness F_Thickness_Pressed(Thickness thickness) => new Thickness(thickness.Left + 1, thickness.Top + 1, thickness.Right, thickness.Bottom);

        private void F_Grid_Animate(Grid grid, Thickness thickness, bool enter = false) => grid.BeginAnimation(MarginProperty, enter ? F_Thickness_Animate(thickness, F_Thickness_Pressed(thickness)) : F_Thickness_Animate(F_Thickness_Pressed(thickness), thickness));

        #endregion AnimationFunctions

        #region CoreFunctions

        public void Do_Clear_All()
        {
            debugmsgcount = 0;
            sqlmsgcount = 0;
            errormsgcount = 0;
            warningmsgcount = 0;
            lb_online.Text = "0";
            lb_debug.Text = "Debug: " + debugmsgcount;
            lb_sql.Text = "SQL: " + sqlmsgcount;
            lb_error.Text = "Error: " + errormsgcount;
            lb_warning.Text = "Warning: " + warningmsgcount;
            Login.Document.Blocks.Clear();
            Char.Document.Blocks.Clear();
            Map.Document.Blocks.Clear();
            Web.Document.Blocks.Clear();
            errorLogs.Clear();
            LogWin.LogsRTB.Document.Blocks.Clear();
        }

        public void KillAll(string ProcessName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(ProcessName);

                foreach (Process process in processes)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public void Do_Kill_All()
        {
            try
            {
                KillAll(Procnamecfg(Properties.Settings.Default.LoginPath));
                KillAll(Procnamecfg(Properties.Settings.Default.CharPath));
                KillAll(Procnamecfg(Properties.Settings.Default.WebPath));
                KillAll(Procnamecfg(Properties.Settings.Default.MapPath));
            }
            catch { }
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

        public string Procnamecfg(string cfgname) => Path.GetFileNameWithoutExtension(cfgname);

        public rAthena Get_process_num(string Processname)
        {
            rAthena type = rAthena.Map;
            Dictionary<string, Action> actions = new Dictionary<string, Action>();

            #region filldictionary

            actions.Add(Procnamecfg(Properties.Settings.Default.LoginPath).ToLowerInvariant(), () =>
            {
                type = rAthena.Login;
            });

            actions.Add(Procnamecfg(Properties.Settings.Default.CharPath).ToLowerInvariant(), () =>
            {
                type = rAthena.Char;
            });

            actions.Add(Procnamecfg(Properties.Settings.Default.WebPath).ToLowerInvariant(), () =>
            {
                type = rAthena.Web;
            });

            actions.Add(Procnamecfg(Properties.Settings.Default.MapPath).ToLowerInvariant(), () =>
            {
                type = rAthena.Map;
            });

            #endregion filldictionary

            actions[Processname]?.Invoke();

            return type;
        }

        private void RTB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => ((RichTextBox)sender).ScrollToEnd();

        #endregion CoreFunctions

        #region color

        private Run ColoredText(string text, Brush typeColor)
        {
            Run typeRun = new Run(text);
            typeRun.Foreground = typeColor;
            return typeRun;
        }

        public Paragraph AppendColoredText(string type, string info, Brush typeColor)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(ColoredText(type, typeColor));
            paragraph.Inlines.Add(ColoredText(info, GetMessageTypeColor(info)));
            return paragraph;
        }

        public Brush GetMessageTypeColor(string messageType)
        {
            switch (messageType)
            {
                case "[Error]":
                    return Brushes.Red;

                case "[Debug]":
                    return Brushes.Aqua;

                case "[SQL]":
                    return Brushes.BlueViolet;

                case "[Warning]":
                    return Brushes.Orange;

                case "[Users]":
                case "[Status]":
                    return Brushes.Green;

                default: return WhiteModeColor();
            }
        }

        private Brush WhiteModeColor(bool reverse = false) => (Properties.Settings.Default.WhiteMode && !reverse || !Properties.Settings.Default.WhiteMode && reverse) ? Brushes.Black : Brushes.White;

        private void Do_Starting_Message()
        {
            Brush color = WhiteModeColor();

            Do_Clear_All();
            Login.Document.Blocks.Add(AppendColoredText("[Info] ", "Login Server is Waiting...", color));
            Char.Document.Blocks.Add(AppendColoredText("[Info] ", "Char Server is Waiting...", color));
            Map.Document.Blocks.Add(AppendColoredText("[Info] ", "Map Server is Waiting...", color));
            Web.Document.Blocks.Add(AppendColoredText("[Info] ", "Web Server is Waiting...", color));
        }

        #endregion color

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
            LogWin.CloseLog.Click += LogWin_Cancel;
            LogWin.Save.Click += LogWin_Save;
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

        private void LogWin_Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Archive (*.txt)|*.txt";
            saveFileDialog.Title = "Save Logs";
            string fileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            saveFileDialog.FileName = fileName;
            if (saveFileDialog.ShowDialog() == true)
            {
                string directoryPath = Path.GetDirectoryName(saveFileDialog.FileName);
                string filePath = Path.Combine(directoryPath, fileName);

                try
                {
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        foreach (var log in errorLogs)
                        {
                            writer.WriteLine($"{log.Type} {log.Content}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fail saving logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                LogWin.Hide();
            }
        }

        private void LogWin_Cancel(object sender, RoutedEventArgs e) => LogWin.Hide();

        private void Add_ErrorLog(string type, string content)
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

            trayMenu.MenuItems.Add($"Online: {onlinecount}");
            trayMenu.MenuItems.Add($"Erro: {errormsgcount}");
            trayMenu.MenuItems.Add($"SQL: {sqlmsgcount}");
            trayMenu.MenuItems.Add($"Warning: {warningmsgcount}");
            trayMenu.MenuItems.Add($"Debug: {debugmsgcount}");

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
                        menuItem.Text = $"Erro: {errormsgcount}";
                        break;

                    case "SQL":
                        menuItem.Text = $"SQL: {sqlmsgcount}";
                        break;

                    case "Warning":
                        menuItem.Text = $"Warning: {warningmsgcount}";
                        break;

                    case "Debug":
                        menuItem.Text = $"Debug: {debugmsgcount}";
                        break;

                    case "Online":
                        menuItem.Text = $"Online: {onlinecount}";
                        break;
                }
            }

            _notifyIcon.ContextMenu = trayMenu;
        }

        #endregion tray

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
                                onlinecount = short.Parse(lb_online.Text);
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
                        errormsgcount++;
                        lb_error.Text = "Error: " + errormsgcount;
                        Add_ErrorLog(message.type, message.info);
                        break;

                    case "[Debug]":
                        debugmsgcount++;
                        lb_debug.Text = "Debug: " + debugmsgcount;
                        Add_ErrorLog(message.type, message.info);
                        break;

                    case "[SQL]":
                        sqlmsgcount++;
                        lb_sql.Text = "SQL: " + sqlmsgcount;
                        Add_ErrorLog(message.type, message.info);
                        break;

                    case "[Warning]":
                        warningmsgcount++;
                        lb_warning.Text = "Warning: " + warningmsgcount;
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

        #region ValidatePathConfig

        private static bool CheckServerPath()
        {
            if (CheckMissingFile(Properties.Settings.Default.LoginPath, "login-server.exe") ||
               CheckMissingFile(Properties.Settings.Default.CharPath, "char-server.exe") ||
               CheckMissingFile(Properties.Settings.Default.WebPath, "web-server.exe") ||
               CheckMissingFile(Properties.Settings.Default.MapPath, "map-server.exe"))
                return false;

            return true;
        }

        private static bool CheckMissingFile(string file, string mes)
        {
            if (!File.Exists(file) || file == String.Empty)
            {
                MessageBox.Show($"File \"{mes}\" at \"{file}\" is missing");
                return true;
            }

            return false;
        }

        #endregion ValidatePathConfig

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