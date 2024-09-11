using AoShinhoServ_Monitor.Forms;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows;
using static AoShinhoServ_Monitor.ProcessManager;

namespace AoShinhoServ_Monitor
{
    public class MainDefs
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
        public static rAthenaData LastErrorLog { set; get; }

        public static readonly List<rAthenaError> errorLogs = new List<rAthenaError>();

        public static OptionsWnd OptWin = new OptionsWnd();

        public static Logs LogWin = new Logs();

    }
}
