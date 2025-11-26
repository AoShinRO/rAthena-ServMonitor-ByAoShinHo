using AoShinhoServ_Monitor.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MenuItem = System.Windows.Forms.MenuItem;

namespace AoShinhoServ_Monitor
{
    internal class ILogging
    {
        public static NotifyIcon _notifyIcon;

        public static readonly ContextMenu trayMenu = new ContextMenu();
        public static short CounterError { set; get; }
        public static short CounterSql { set; get; }
        public static short CounterWarning { set; get; }
        public static short CounterDebug { set; get; }
        public static short CounterOnline { set; get; }
        public static bool OnOff { set; get; }
        public static bool IsDragging { set; get; }
        public static Point MousePosition { set; get; }
        public static Thickness StartMargin { set; get; }
        public static Thickness StopMargin { set; get; }
        public static Thickness OptionMargin { set; get; }
        public static Thickness RestartMargin { set; get; }
        public static Thickness OptionSaveMargin { set; get; }
        public static Thickness OptionCancelMargin { set; get; }
        public static rAthena.Data LastErrorLog { set; get; }

        public static readonly List<rAthena.Error> errorLogs = new List<rAthena.Error>();

        public static OptionsWnd OptWin = new OptionsWnd();

        public static Logs LogWin = new Logs();

        #region LogWinRelated

        public static void Add_ErrorLog(rAthena.Data Data)
        {
            errorLogs.Add(new rAthena.Error { Header = Data.Header, Body = Data.Body });
            Task.Run(() => UpdateContextMenu());
        }

        #endregion LogWinRelated

        #region tray

        public static void UpdateContextMenu()
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
    }
}
