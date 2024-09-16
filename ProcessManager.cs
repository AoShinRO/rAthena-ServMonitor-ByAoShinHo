using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Windows;
using static AoShinhoServ_Monitor.Consts;

namespace AoShinhoServ_Monitor
{
    public class ProcessManager
    {
        public static void KillAll(string ProcessName)
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

        public static void Do_Kill_All()
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

        public static string Procnamecfg(string cfgname) => Path.GetFileNameWithoutExtension(cfgname);
        #region ValidatePathConfig

        public static bool CheckServerPath()
        {
            if (CheckMissingFile(Properties.Settings.Default.LoginPath, "login-server.exe") ||
               CheckMissingFile(Properties.Settings.Default.CharPath, "char-server.exe") ||
               CheckMissingFile(Properties.Settings.Default.WebPath, "web-server.exe") ||
               CheckMissingFile(Properties.Settings.Default.MapPath, "map-server.exe"))
                return false;

            return true;
        }

        public static bool CheckMissingFile(string file, string mes)
        {
            if (!File.Exists(file) || file == String.Empty)
            {
                MessageBox.Show($"File \"{mes}\" at \"{file}\" is missing");
                return true;
            }

            return false;
        }

        #endregion ValidatePathConfig

        public static rAthenaTypes Get_process_num(string Processname)
        {
            rAthenaTypes type = rAthenaTypes.MapSv;
            Dictionary<string, Action> actions = new Dictionary<string, Action>();

            #region filldictionary

            actions.Add(Procnamecfg(Properties.Settings.Default.LoginPath).ToLowerInvariant(), () =>
            {
                type = rAthenaTypes.LoginSv;
            });

            actions.Add(Procnamecfg(Properties.Settings.Default.CharPath).ToLowerInvariant(), () =>
            {
                type = rAthenaTypes.CharSv;
            });

            actions.Add(Procnamecfg(Properties.Settings.Default.WebPath).ToLowerInvariant(), () =>
            {
                type = rAthenaTypes.WebSv;
            });

            actions.Add(Procnamecfg(Properties.Settings.Default.MapPath).ToLowerInvariant(), () =>
            {
                type = rAthenaTypes.MapSv;
            });

            #endregion filldictionary

            actions[Processname]?.Invoke();

            return type;
        }
    }
}
