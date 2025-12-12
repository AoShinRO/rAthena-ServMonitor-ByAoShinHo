using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
            ILogging.CompileMargin = CompileGrid.Margin;
            ILogging.BuildROBMargin = CompileGrid_Rob.Margin;
            ILogging.StartROBMargin = StartROBrowser.Margin;
            ILogging.StartWSMargin = StartWS_Grid.Margin;

            ILogging.OptionCancelMargin = ILogging.OptWin.CancelGrid.Margin;
            ILogging.OptionSaveMargin = ILogging.OptWin.OkayGrid.Margin;

            if (!Properties.Settings.Default.DevMode)
            {
                DevBox.Visibility = Visibility.Collapsed;
                CompileGrid.Visibility = Visibility.Collapsed;
                CompileGrid_Rob.Visibility = Visibility.Collapsed;
                
                ILogging.OptWin.CmakeMode.Visibility = Visibility.Collapsed;
                ILogging.OptWin.PreRenewalMode.Visibility = Visibility.Collapsed;
                ApplyRoundedCorners(5);
            }
            else
            {
                Height = 780;
                ApplyRoundedCorners(10);
                if (Properties.Settings.Default.ROBMode)
                    DevBox.Width = 935 + NpmBox.Width + 8;
                else
                    CompileGrid_Rob.Visibility = Visibility.Collapsed;
            }

            if (!Properties.Settings.Default.ROBMode)
            {
                StartROBrowser.Visibility = Visibility.Collapsed;
                StartWS_Grid.Visibility = Visibility.Collapsed;
                ROBGrid.Visibility = Visibility.Collapsed;
                ApplyRoundedCorners(5);
            }
            else
            {
                Width = 1600;
                ApplyRoundedCorners(10);
                if(!Properties.Settings.Default.DevMode)
                    CompileGrid_Rob.Visibility = Visibility.Collapsed;
            }
            if (Properties.Settings.Default.FontFamily != string.Empty)
            {
                ILogging.OptWin.FontSelector.SelectedItem = Properties.Settings.Default.FontFamily;
                ApplyFontToAll();
            }
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
            DevBox.Document.Blocks.Clear();
            NpmBox.Document.Blocks.Clear();
            WSBox.Document.Blocks.Clear();
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
                    string path = cmdPath.Substring(0, cmdPath.LastIndexOf('\\'));
                    string arg = IProcess.GetFileName(cmdPath);
                    Process process = new Process()
                    {
                        StartInfo =
                        {
                            FileName = cmdPath,
                            UseShellExecute = false,
                            WorkingDirectory = path,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        },
                        EnableRaisingEvents = true
                    };
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.StandardOutputEncoding = new UTF8Encoding(false);
                    process.StartInfo.StandardErrorEncoding = new UTF8Encoding(false);
                    process.ErrorDataReceived += new DataReceivedEventHandler(Proc_DataReceived);
                    process.OutputDataReceived += new DataReceivedEventHandler(Proc_DataReceived);
                    process.Exited += new EventHandler(Proc_HasExited);
                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    rAthena.ProcessesInfo info = new rAthena.ProcessesInfo();
                    info.pID = process.Id;
                    info.type = IProcess.GetProcessType(process);
                    ILogging.processesInfos.Add(info);
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(ex.StackTrace, ex.Message);
                }
            });
        }

        public async Task RunWithRedirectCmdAsync(string cmdPath, string alias)
        {
            await Task.Run(() =>
            {
                try
                {
                    string cmd = cmdPath;
                    string path = "";

                    if (alias != "wsproxy")
                        path += cmd.Substring(0, cmd.LastIndexOf('\\'));

                    string projectname = path;
                    rAthena.ProcessesInfo info = new rAthena.ProcessesInfo();

                    #region comandline
                    switch (alias)
                    {
                        case "build":
                            cmd = @"npm run build -- -O -T";
                            if (!Properties.Settings.Default.ROBH)
                                cmd += " -H";
                            path = cmdPath;
                            info.type = rAthena.Type.ROBrowser;

                            break;
                        case "robrowser":
                            if (Properties.Settings.Default.DevMode)
                                cmd = @"npm run serve";
                            else
                                cmd = @"npm run live";
                            path = cmdPath;
                            info.type = rAthena.Type.ROBrowser;
                            break;
                        case "wsproxy":
                            if (Properties.Settings.Default.wsport > 0)
                                cmd = $"wsproxy -p {Properties.Settings.Default.wsport}";
                            else
                                cmd = $"wsproxy";
                            info.type = rAthena.Type.WSproxy;
                            break;
                        default:
                            if (!IProcess.CheckMissingFile(projectname + "/rAthena.sln", "rAthena.sln"))
                                projectname = "rAthena.sln";
                            else if (!IProcess.CheckMissingFile(projectname + "/brHades.sln", "brHades.sln"))
                                projectname = "brHades.sln";
                            else if (!IProcess.CheckMissingFile(projectname + "/Hercules.sln", "Hercules.sln"))
                                projectname = "Hercules.sln";
                            else
                            {
                                ErrorHandler.ShowError("Failed to find a valid .sln", "Error");
                                return;
                            }
                            if (Properties.Settings.Default.UseCMake)
                            {
                                cmd = @"cmake -G ""Unix Makefiles"" -DINSTALL_TO_SOURCE=ON";
                                if (projectname == "brHades.sln")
                                    cmd += " -DCMAKE_CXX_STANDARD=20";

                                cmd += " -DCMAKE_BUILD_TYPE=RelWithDebInfo ..";

                                Process configProcess = CreateCMake(cmd, path);
                                configProcess.Start();
                                configProcess.BeginErrorReadLine();
                                configProcess.BeginOutputReadLine();
                                configProcess.WaitForExit();

                                cmd = @"make install";
                                Process buildProcess = CreateCMake(cmd, path);
                                buildProcess.Start();
                                buildProcess.BeginErrorReadLine();
                                buildProcess.BeginOutputReadLine();
                                info.type = rAthena.Type.DevConsole;
                                info.pID = buildProcess.Id;
                                ILogging.processesInfos.Add(info);
                                return;
                            }
                            else
                            {
                                if (Properties.Settings.Default.PreRenewal)
                                    cmd = $@"msbuild {projectname} -t:build -property:Configuration=Release /p:DefineConstants=""PRERE""";
                                else
                                    cmd = $@"msbuild {projectname} -t:build -property:Configuration=Release";

                                if (projectname == "brHades.sln")
                                    cmd += @" /p:CppLanguageStandard=stdcpp20";

                                cmd += " /warnaserror";
                            }
                            info.type = rAthena.Type.DevConsole;
                            break;

                    }
                    #endregion comandline

                    Process process = new Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/c {cmd}";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.EnableRaisingEvents = true;

                    if (alias != "wsproxy")
                        process.StartInfo.WorkingDirectory = path;
                    
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.StandardOutputEncoding = new UTF8Encoding(false);
                    process.StartInfo.StandardErrorEncoding = new UTF8Encoding(false);
                    process.ErrorDataReceived += new DataReceivedEventHandler(Proc_DataReceived);
                    process.OutputDataReceived += new DataReceivedEventHandler(Proc_DataReceived);
                    process.Exited += new EventHandler(Proc_HasExited);
                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    info.pID = process.Id;
                    ILogging.processesInfos.Add(info);
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(ex.StackTrace, ex.Message);
                }
            });
        }

        private Process CreateCMake(string cmd, string path)
        {
            Process process = new Process()
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {cmd}",
                    UseShellExecute = false,
                    WorkingDirectory = path,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = new UTF8Encoding(false);
            process.StartInfo.StandardErrorEncoding = new UTF8Encoding(false);
            process.ErrorDataReceived += new DataReceivedEventHandler(Proc_DataReceived);
            process.OutputDataReceived += new DataReceivedEventHandler(Proc_DataReceived);
            process.Exited += new EventHandler(Proc_HasExited);
            rAthena.ProcessesInfo info = new rAthena.ProcessesInfo();
            info.pID = process.Id;
            info.type = rAthena.Type.DevConsole;
            ILogging.processesInfos.Add(info);
            return process;
        }

        private static rAthena.Data ParseServerData(string rawData)
        {
            var data = new rAthena.Data();
            rawData = IText.RemoveAnsi(rawData);
            int endIndex = rawData.IndexOf("]");

            if (endIndex != -1)
            {
                data.Header = rawData.Substring(0, endIndex + 1);
                data.Body = rawData.Substring(endIndex + 1);
                if (data.Header == "[Status]")
                {
                    if (rawData.Contains("set users"))
                        data.Header = "[Users]";
                    else if ((Properties.Settings.Default.DebugMode && ( rawData.Contains("Loading") || rawData.Contains("Carregando"))) ||
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

                case rAthena.Type.DevConsole:
                    Proc_Data2Box(DevBox, Data);
                    break;
                case rAthena.Type.ROBrowser:
                    Proc_Data2Box(NpmBox, Data);
                    break;
                case rAthena.Type.WSproxy:
                    Proc_Data2Box(WSBox, Data);
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
                Process p = (Process)sender;
                rAthena.Type type = IProcess.GetProcessType(p);
                switch (type)
                {
                    case rAthena.Type.Map:
                        MapBox.AppendText(Environment.NewLine + ">>Map Server - stopped<<");
                        break;
                    case rAthena.Type.Login:
                        LoginBox.AppendText(Environment.NewLine + ">>Login Server - stopped<<");
                        break;

                    case rAthena.Type.Char:
                        CharBox.AppendText(Environment.NewLine + ">>Char Server - stopped<<");
                        break;

                    case rAthena.Type.Web:
                        WebBox.AppendText(Environment.NewLine + ">>Web Server - stopped<<");
                        break;
                    case rAthena.Type.WSproxy:
                        WSBox.AppendText(Environment.NewLine + ">>wsProxy - stopped<<");
                        break;
                    case rAthena.Type.ROBrowser:
                        NpmBox.AppendText(Environment.NewLine + ">>ROBrowser - stopped<<");
                        break;
                    default:
                        DevBox.AppendText(Environment.NewLine + ">>Dev Console - stopped<<");
                        break;
                }
                Parallel.ForEach(ILogging.processesInfos, it =>
                {
                    if (it.pID == p.Id)
                        ILogging.processesInfos.Remove(it);
                });
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
            ILogging.OptWin.DevMode.Checked += OptionWin_Do_DevMode_Mode_On;
            ILogging.OptWin.DevMode.Unchecked += OptionWin_Do_DevMode_Mode_Off;
            ILogging.OptWin.ROBrowser.Checked += OptionWin_Do_ROBrowser_Mode_On;
            ILogging.OptWin.ROBrowser.Unchecked += OptionWin_Do_ROBrowser_Mode_Off;
            ILogging.OptWin.FontSelector.SelectionChanged += FontSelector_SelectionChanged;
            ILogging.OptWin.FontSizeBox.SelectionChanged += FontSizeBox_SelectionChanged;
        }

        private void FontSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ILogging.OptWin.FontSelector.SelectedItem is string fontName)
            {
                Properties.Settings.Default.FontFamily = fontName;
                Properties.Settings.Default.Save();

                ApplyFontToAll();
            }
        }

        private void FontSizeBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ILogging.OptWin.FontSizeBox.Text, out int size))
            {
                Properties.Settings.Default.FontSize = size;
                Properties.Settings.Default.Save();

                ApplyFontToAll();
            }
        }

        private void ApplyFontToAll()
        {
            string fontName = Properties.Settings.Default.FontFamily;
            double fontSize = Properties.Settings.Default.FontSize;

            var ff = new FontFamily(fontName);

            // lista dos RichTextBoxes da UI
            var boxes = new[]
            {CharBox, LoginBox, WebBox, DevBox, NpmBox, WSBox, MapBox};

            foreach (var box in boxes)
                ApplyFontToRichTextBox(box, ff, fontSize);
        }

        private void ApplyFontToRichTextBox(System.Windows.Controls.RichTextBox rtb, FontFamily ff, double fontSize)
        {
            if (rtb == null) return;

            // aplica nas propriedades do controle (para novos inlines)
            rtb.FontFamily = ff;
            rtb.FontSize = fontSize;

            // aplica no documento (para novos elementos)
            var doc = rtb.Document;
            if (doc == null) return;

            doc.FontFamily = ff;
            doc.FontSize = fontSize;

            // aplica em todo o conteúdo já existente
            var range = new TextRange(doc.ContentStart, doc.ContentEnd);

            // define FontFamily e FontSize para todo o intervalo
            range.ApplyPropertyValue(TextElement.FontFamilyProperty, ff);
            range.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
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
            DevBox.Background =
            NpmBox.Background =
            WSBox.Background =
            WebBox.Background = Background;
            
            MapText.Foreground =
            LoginText.Foreground =
            CharText.Foreground =
            DevText.Foreground =
            NpmBox.Foreground =
            WSBox.Foreground =
            WebText.Foreground = Foreground;

            if (!ILogging.OnOff)
            {
                Do_Clear_All();
                IText.Do_Starting_Message(CharBox,LoginBox,MapBox,WebBox,DevBox,NpmBox,WSBox);
            }
        }

        private void OptionWin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Properties.Settings.Default.ROBMode)
            {
                ILogging.OptWin.Width = 550;
                ILogging.OptWin.BG.Width = Width;
                ILogging.OptWin.ROBGrid.Visibility = Visibility.Visible;
            }
            ILogging.OptWin.Show();
        }

        private void OptionWin_Okay(object sender, RoutedEventArgs e)
        {
            Configuration.Save();
            ILogging.OptWin.Hide();
        }

        private void OptionWin_Cancel(object sender, RoutedEventArgs e) => ILogging.OptWin.Hide();

        private void OptionWin_Do_DevMode_Mode_On(object sender, RoutedEventArgs e)
        {
            CompileGrid.Visibility = Visibility.Visible;
            DevBox.Visibility = Visibility.Visible;
            
            ILogging.OptWin.CmakeMode.Visibility = Visibility.Visible;
            ILogging.OptWin.PreRenewalMode.Visibility = Visibility.Visible;
            Height = 780;
            if (Properties.Settings.Default.ROBMode)
            {
                DevBox.Width = 935 + NpmBox.Width + 8;
                CompileGrid_Rob.Visibility = Visibility.Visible;
            }
            ApplyRoundedCorners(10);
        }

        private void OptionWin_Do_DevMode_Mode_Off(object sender, RoutedEventArgs e)
        {
            CompileGrid_Rob.Visibility = Visibility.Collapsed;
            CompileGrid.Visibility = Visibility.Collapsed;
            DevBox.Visibility = Visibility.Collapsed;
            ILogging.OptWin.CmakeMode.Visibility = Visibility.Collapsed;
            ILogging.OptWin.PreRenewalMode.Visibility = Visibility.Collapsed;
            Height = 600;
            if (Properties.Settings.Default.ROBMode)
            {
                DevBox.Width = 935;
                CompileGrid_Rob.Visibility = Visibility.Collapsed;
            }
            ApplyRoundedCorners(5);
        }
        private void OptionWin_Do_ROBrowser_Mode_On(object sender, RoutedEventArgs e)
        {
            StartROBrowser.Visibility = Visibility.Visible;
            StartWS_Grid.Visibility = Visibility.Visible;
            ROBGrid.Visibility = Visibility.Visible;
            Width = 1600;
            if (Properties.Settings.Default.DevMode)
            {
                DevBox.Width = 935 + NpmBox.Width + 8;
                CompileGrid_Rob.Visibility = Visibility.Visible;
            }
            ApplyRoundedCorners(10);

            ILogging.OptWin.Width = 550;
            ILogging.OptWin.BG.Width = ILogging.OptWin.Width;
            ILogging.OptWin.ROBGrid.Visibility = Visibility.Visible;
        }

        private void OptionWin_Do_ROBrowser_Mode_Off(object sender, RoutedEventArgs e)
        {
            StartROBrowser.Visibility = Visibility.Collapsed;
            StartWS_Grid.Visibility = Visibility.Collapsed;
            ROBGrid.Visibility = Visibility.Collapsed;
            Width = 1200;
            if (Properties.Settings.Default.DevMode)
            {
                DevBox.Width = 935;
                CompileGrid_Rob.Visibility = Visibility.Collapsed;
            }
            ApplyRoundedCorners(5);
            ILogging.OptWin.Width = 270;
            ILogging.OptWin.BG.Width = ILogging.OptWin.Width;
            ILogging.OptWin.ROBGrid.Visibility = Visibility.Collapsed;
        }

        #endregion OptionWinRelated

        private void ApplyRoundedCorners(int radius) => Clip = new RectangleGeometry(new Rect(0, 0, Width, Height), radius, radius);

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

        private async void CompileBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DevBox.Document.Blocks.Clear();
            IProcess.Do_Kill_All();
            await Task.Run(() => RunWithRedirectCmdAsync(Configuration.MapPath, "compiler"));
        }

        private void CompileBtn_MouseEnter(object sender, MouseEventArgs e) => IAnimation.F_Grid(CompileGrid, ILogging.CompileMargin, true);

        private void CompileBtn_MouseLeave(object sender, MouseEventArgs e) => IAnimation.F_Grid(CompileGrid, ILogging.CompileMargin);

        private void StartROBBtn_MouseEnter(object sender, MouseEventArgs e) => IAnimation.F_Grid(StartROBrowser, ILogging.StartROBMargin, true);

        private void StartROBBtn_MouseLeave(object sender, MouseEventArgs e) => IAnimation.F_Grid(StartROBrowser, ILogging.StartROBMargin);

        private void StartWSBtn_MouseEnter(object sender, MouseEventArgs e) => IAnimation.F_Grid(StartWS_Grid, ILogging.StartWSMargin, true);

        private void StartWSBtn_MouseLeave(object sender, MouseEventArgs e) => IAnimation.F_Grid(StartWS_Grid, ILogging.StartWSMargin);

        private void CompileBtn_Rob_MouseLeave(object sender, MouseEventArgs e) => IAnimation.F_Grid(CompileGrid_Rob, ILogging.BuildROBMargin);

        private void CompileBtn_Rob_MouseEnter(object sender, MouseEventArgs e) => IAnimation.F_Grid(CompileGrid_Rob, ILogging.BuildROBMargin, true);

        private async void CompileBtn_Rob_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NpmBox.Document.Blocks.Clear();
            Parallel.ForEach(ILogging.processesInfos, p =>
            {
                if (p.type == rAthena.Type.ROBrowser)
                    IProcess.KillAll(p.pID);
            });

            await Task.Run(() => RunWithRedirectCmdAsync(Configuration.RobPath, "build"));
        }
        private async void StartROBBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NpmBox.Document.Blocks.Clear();
            Parallel.ForEach(ILogging.processesInfos, p =>
            {
                if (p.type == rAthena.Type.ROBrowser)
                    IProcess.KillAll(p.pID);
            });
            await Task.Run(() => RunWithRedirectCmdAsync(Configuration.RobPath, "robrowser"));
        }

        private async void StartWSBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WSBox.Document.Blocks.Clear();
            Parallel.ForEach(ILogging.processesInfos, p =>
            {
                if (p.type == rAthena.Type.WSproxy)
                    IProcess.KillAll(p.pID);
            });
            await Task.Run(() => RunWithRedirectCmdAsync("", "wsproxy"));
        }
    }
}