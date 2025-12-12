namespace AoShinhoServ_Monitor
{
    public static class Configuration
    {
        public static string LoginPath = Properties.Settings.Default.LoginPath;  
        public static string CharPath = Properties.Settings.Default.CharPath;  
        public static string MapPath = Properties.Settings.Default.MapPath;  
        public static string WebPath = Properties.Settings.Default.WebPath;
        public static string RobPath = Properties.Settings.Default.ROBPath;

        public static void Save()
        {
            Properties.Settings.Default.LoginPath = LoginPath;
            Properties.Settings.Default.CharPath = CharPath;
            Properties.Settings.Default.MapPath = MapPath;
            Properties.Settings.Default.WebPath = WebPath;
            Properties.Settings.Default.ROBPath = RobPath;
            Properties.Settings.Default.Save();
        }
    }  
}
