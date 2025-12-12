using System.Windows.Media;

namespace AoShinhoServ_Monitor
{
    public class rAthena
    {
        public enum Type
        {
            Map,
            Login,
            Char,
            Web,
            DevConsole,
            WSproxy,
            ROBrowser
        };

        public class Error
        {
            public string Header;
            public string Body;
        }

        public class ProcessesInfo
        {
            public int pID;
            public Type type;
        }

        public struct Data
        {
            public string Header;
            public string Body;
            public Brush Paint;
        }
    }
}