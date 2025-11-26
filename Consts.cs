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
            Web
        };

        public class Error
        {
            public string Header;
            public string Body;
        }

        public struct Data
        {
            public string Header;
            public string Body;
            public Brush Paint;
        }
    }
}