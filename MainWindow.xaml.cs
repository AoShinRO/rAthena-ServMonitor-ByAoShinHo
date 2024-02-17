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
using ContextMenu = System.Windows.Forms.ContextMenu;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using MenuItem = System.Windows.Forms.MenuItem;
using Application = System.Windows.Application;

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
            InitializeNotifyIcon();
        }

        #region structing vars

        private NotifyIcon _notifyIcon;
        private ContextMenu contextMenu = new ContextMenu();
        public int errormsgcount;
        public int sqlmsgcount;
        public int warningmsgcount;
        public int debugmsgcount;
        public int onlinecount;
        public Point p;
        public bool IsDragging;
        public string LastErrorLog;

        public struct ErrorLog
        {
            public string Type;
            public string Content;
        }

        private List<ErrorLog> errorLogs = new List<ErrorLog>();

        public enum MonitorType
        {
            LOGIN,
            CHAR,
            MAP,
            WEB
        };

        public OptionsWnd OptWin = new OptionsWnd();
        public Logs LogWin = new Logs();

        private struct ProcessData
        {
            public string type;
            public string info;
            public Brush Color;
        }

        #endregion structing vars

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
            KillAll(Procnamecfg(Properties.Settings.Default.LoginPath));
            KillAll(Procnamecfg(Properties.Settings.Default.CharPath));
            KillAll(Procnamecfg(Properties.Settings.Default.WebPath));
            KillAll(Procnamecfg(Properties.Settings.Default.MapPath));
        }

        private async void Do_Run_All_Async() => await Task.Run(() => Do_Run_All());

        public async Task Do_Run_All()
        {
            try
            {
                await RunWithRedirectAsync(Properties.Settings.Default.MapPath);
                await RunWithRedirectAsync(Properties.Settings.Default.LoginPath);
                await RunWithRedirectAsync(Properties.Settings.Default.CharPath);
                await RunWithRedirectAsync(Properties.Settings.Default.WebPath);
            }
            catch
            {
            }
        }

        private Task RunWithRedirectAsync(string cmdPath)
        {
            return Task.Run(() =>
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
            });
        }

        public string Procnamecfg(string cfgname) => Path.GetFileNameWithoutExtension(cfgname);

        public MonitorType Get_process_num(string Processname)
        {
            MonitorType type = MonitorType.MAP;
            Dictionary<string, Action> actions = new Dictionary<string, Action>();

            #region filldictionary

            actions.Add(Procnamecfg(Properties.Settings.Default.LoginPath).ToLowerInvariant(), () =>
            {
                type = MonitorType.LOGIN;
            });

            actions.Add(Procnamecfg(Properties.Settings.Default.CharPath).ToLowerInvariant(), () =>
            {
                type = MonitorType.CHAR;
            });

            actions.Add(Procnamecfg(Properties.Settings.Default.WebPath).ToLowerInvariant(), () =>
            {
                type = MonitorType.WEB;
            });

            actions.Add(Procnamecfg(Properties.Settings.Default.MapPath).ToLowerInvariant(), () =>
            {
                type = MonitorType.MAP;
            });

            #endregion filldictionary

            if (actions.ContainsKey(Processname))
                actions[Processname].Invoke();

            return type;
        }

        private void RTB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => ((RichTextBox)sender).ScrollToEnd();

        #endregion CoreFunctions

        #region color

        public Paragraph AppendColoredText(string type, string info, Brush typeColor)
        {
            Paragraph paragraph = new Paragraph();

            Run typeRun = new Run(type);
            typeRun.Foreground = typeColor;
            paragraph.Inlines.Add(typeRun);

            Run infoRun = new Run(info);
            infoRun.Foreground = Brushes.White;
            paragraph.Inlines.Add(infoRun);
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

                case "[Status]":
                    return Brushes.Green;

                case "[Users]":
                    return Brushes.Green;

                default:
                    return Brushes.White;
            }
        }

        #endregion color

        #region OptionWinRelated

        private void OptionWin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OptWin.Okaylbl.MouseDown += OptionWin_Okay;
            OptWin.Cancellbl.MouseDown += OptionWin_Cancel;
            OptWin.Show();
        }

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

        private void Add_ErrorLog(string type, string content) => errorLogs.Add(new ErrorLog { Type = type, Content = content });

        #endregion LogWinRelated

        #region tray

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.Main_Icon, // Substitua "SeuIcone" pelo nome do seu ícone nos recursos
                Visible = true,
                Text = "Seu programa está na bandeja do sistema."
            };
            // Manipulador de evento para restaurar a janela ao dar um clique duplo no ícone
            _notifyIcon.MouseDoubleClick += (sender, e) =>
            {
                Show();
                _notifyIcon.Visible = false;
                WindowState = WindowState.Normal;
            };

            // Adicione um menu de contexto para permitir opções adicionais ao clicar com o botão direito do mouse no ícone
            contextMenu.MenuItems.Add($"Online: {onlinecount}");
            contextMenu.MenuItems.Add($"Erro: {errormsgcount}");
            contextMenu.MenuItems.Add($"SQL: {sqlmsgcount}");
            contextMenu.MenuItems.Add($"Warning: {warningmsgcount}");
            contextMenu.MenuItems.Add($"Debug: {debugmsgcount}");

            contextMenu.MenuItems.Add("Restore", (sender, e) =>
            {
                Show();
                _notifyIcon.Visible = false;
                WindowState = WindowState.Normal;
            });
            contextMenu.MenuItems.Add("Close", (sender, e) =>
            {
                Close();
            });
            _notifyIcon.Visible = false;
            _notifyIcon.ContextMenu = contextMenu;
        }

        private void UpdateContextMenu()
        {
            foreach (MenuItem menuItem in contextMenu.MenuItems)
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

            _notifyIcon.ContextMenu = contextMenu;
        }

        #endregion tray

        #region ProcesingInfo

        public void Proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            #region preprocessinginfo

            ProcessData thisdata = new ProcessData();

            int endIndex = e.Data.IndexOf("]");

            if (endIndex != -1)
            {
                thisdata.type = e.Data.Substring(0, endIndex + 1);
                thisdata.info = e.Data.Substring(endIndex + 1);
                if (thisdata.info.Contains("set users"))
                    thisdata.type = "[Users]";
                LastErrorLog = thisdata.type;
            }
            else
            {
                thisdata.type = "";
                thisdata.info = e.Data;
                if (LastErrorLog == "[Error]")
                    Add_ErrorLog(thisdata.type, thisdata.info);
            }

            #endregion preprocessinginfo

            #region SwitchProcess

            switch (Get_process_num(((Process)sender).ProcessName.ToLowerInvariant()))
            {
                case MonitorType.LOGIN:

                    #region Login

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (thisdata.type)
                        {
                            case "[Error]":
                                errormsgcount++;
                                lb_error.Text = "Error: " + errormsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[Debug]":
                                debugmsgcount++;
                                lb_debug.Text = "Debug: " + debugmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[SQL]":
                                sqlmsgcount++;
                                lb_sql.Text = "SQL: " + sqlmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[Warning]":
                                warningmsgcount++;
                                lb_warning.Text = "Warning: " + warningmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[Users]":
                                {
                                    string[] playercount = e.Data.Split(new Char[] { ':' });
                                    lb_online.Text = playercount[2];
                                    onlinecount = int.Parse(lb_online.Text);
                                }
                                break;

                            default:
                                break;
                        }
                        thisdata.Color = GetMessageTypeColor(thisdata.type);
                        Login.Document.Blocks.Add(AppendColoredText($"{thisdata.type} ", $"{thisdata.info}", thisdata.Color));
                    });

                    #endregion Login

                    break;

                case MonitorType.CHAR:

                    #region Char

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (thisdata.type)
                        {
                            case "[Error]":
                                errormsgcount++;
                                lb_error.Text = "Error: " + errormsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[Debug]":
                                debugmsgcount++;
                                lb_debug.Text = "Debug: " + debugmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[SQL]":
                                sqlmsgcount++;
                                lb_sql.Text = "SQL: " + sqlmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[Warning]":
                                warningmsgcount++;
                                lb_warning.Text = "Warning: " + warningmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            default:
                                break;
                        }
                        thisdata.Color = GetMessageTypeColor(thisdata.type);
                        Char.Document.Blocks.Add(AppendColoredText($"{thisdata.type} ", $"{thisdata.info}", thisdata.Color));
                    });

                    #endregion Char

                    break;

                case MonitorType.WEB:

                    #region Web

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (thisdata.type)
                        {
                            case "[Error]":
                                errormsgcount++;
                                lb_error.Text = "Error: " + errormsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[Debug]":
                                debugmsgcount++;
                                lb_debug.Text = "Debug: " + debugmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[SQL]":
                                sqlmsgcount++;
                                lb_sql.Text = "SQL: " + sqlmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[Warning]":
                                warningmsgcount++;
                                lb_warning.Text = "Warning: " + warningmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            default:
                                break;
                        }
                        thisdata.Color = GetMessageTypeColor(thisdata.type);
                        Web.Document.Blocks.Add(AppendColoredText($"{thisdata.type} ", $"{thisdata.info}", thisdata.Color));
                    });

                    #endregion Web

                    break;

                default:

                    #region Map

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (thisdata.type)
                        {
                            case "[Error]":
                                errormsgcount++;
                                lb_error.Text = "Error: " + errormsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[Debug]":
                                debugmsgcount++;
                                lb_debug.Text = "Debug: " + debugmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[SQL]":
                                sqlmsgcount++;
                                lb_sql.Text = "SQL: " + sqlmsgcount;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                break;

                            case "[Warning]":
                                warningmsgcount++;
                                Add_ErrorLog(thisdata.type, thisdata.info);
                                lb_warning.Text = "Warning: " + warningmsgcount;

                                break;

                            default:
                                break;
                        }
                        thisdata.Color = GetMessageTypeColor(thisdata.type);
                        Map.Document.Blocks.Add(AppendColoredText($"{thisdata.type} ", $"{thisdata.info}", thisdata.Color));
                    });

                    #endregion Map

                    break;
            }

            #endregion SwitchProcess

            UpdateContextMenu();
        }

        public void Proc_HasExited(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (Get_process_num(((Process)sender).ProcessName.ToLower()))
                {
                    case MonitorType.LOGIN:
                        Login.AppendText(Environment.NewLine + ">>Login Server - stopped<<");
                        break;

                    case MonitorType.CHAR:
                        Char.AppendText(Environment.NewLine + ">>Char Server - stopped<<");
                        break;

                    case MonitorType.WEB:
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
            if (!File.Exists(Properties.Settings.Default.LoginPath) || Properties.Settings.Default.LoginPath == String.Empty)
            {
                MessageBox.Show($"File \"login -server.exe\" at \"{Properties.Settings.Default.LoginPath}\" is missing");
                return false;
            }
            if (!File.Exists(Properties.Settings.Default.CharPath) || Properties.Settings.Default.CharPath == String.Empty)
            {
                MessageBox.Show($"File \"char-server.exe\" at \"{Properties.Settings.Default.CharPath}\" is missing");
                return false;
            }
            if (!File.Exists(Properties.Settings.Default.MapPath) || Properties.Settings.Default.MapPath == String.Empty)
            {
                MessageBox.Show($"File \"map-server.exe\" at \"{Properties.Settings.Default.MapPath}\" is missing");
                return false;
            }
            if (!File.Exists(Properties.Settings.Default.WebPath) || Properties.Settings.Default.WebPath == String.Empty)
            {
                MessageBox.Show($"File \"web-server.exe\" at \"{Properties.Settings.Default.WebPath}\" is missing");
                return false;
            }
            return true;
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
                    try
                    {
                        Do_Kill_All();
                    }
                    catch
                    {
                    }
                    Do_Clear_All();
                    Do_Run_All_Async();
                }
                catch { }
                finally
                {
                    StartGrid.Visibility = Visibility.Collapsed;
                    RestartGrid.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void Program_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            if (!IsDragging)
                return;
            Point currentMousePoint = e.GetPosition(this);
            double offsetX = currentMousePoint.X - p.X;
            double offsetY = currentMousePoint.Y - p.Y;

            Left += offsetX;
            Top += offsetY;
        }

        private void StopBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Do_Kill_All();
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
            LogWin.CloseLog.Click += LogWin_Cancel;
            LogWin.Save.Click += LogWin_Save;
            LogWin.Show();
        }

        private void BG_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            IsDragging = true;
            p = e.GetPosition(this);
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

        private void xBtn_MouseDown(object sender, MouseButtonEventArgs e) => Close();

        private void MinimizeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Hide();
            _notifyIcon.Visible = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Do_End();

        #endregion Btn_related
    }
}