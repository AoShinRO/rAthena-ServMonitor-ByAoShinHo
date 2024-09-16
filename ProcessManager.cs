using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Collections.Generic;
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
                KillAll(GetFileName(Properties.Settings.Default.LoginPath));
                KillAll(GetFileName(Properties.Settings.Default.CharPath));
                KillAll(GetFileName(Properties.Settings.Default.WebPath));
                KillAll(GetFileName(Properties.Settings.Default.MapPath));
            }
            catch { }
        }

        public static string GetFileName(string FilePath) => System.IO.Path.GetFileNameWithoutExtension(FilePath);

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
            if (!File.Exists(file) || file == string.Empty)
            {
                MessageBox.Show($"File \"{mes}\" at \"{file}\" is missing");
                return true;
            }

            return false;
        }

        #endregion ValidatePathConfig

        public static rAthena GetProcessType(Process rAthenaProcess)
        {
            Dictionary<string, Action> rAthenaTypeMap = new Dictionary<string, Action>();
            
            rAthena type = rAthena.MapSv;

            rAthenaTypeMap.Add(GetFileName(Properties.Settings.Default.LoginPath).ToLowerInvariant(), () =>
            {
                type = rAthena.LoginSv;
            });

            rAthenaTypeMap.Add(GetFileName(Properties.Settings.Default.CharPath).ToLowerInvariant(), () =>
            {
                type = rAthena.CharSv;
            });

            rAthenaTypeMap.Add(GetFileName(Properties.Settings.Default.WebPath).ToLowerInvariant(), () =>
            {
                type = rAthena.WebSv;
            });

            rAthenaTypeMap.Add(GetFileName(Properties.Settings.Default.MapPath).ToLowerInvariant(), () =>
            {
                type = rAthena.MapSv;
            });

            rAthenaTypeMap[rAthenaProcess.ProcessName.ToLowerInvariant()]?.Invoke();

            return type;
        }
    }
}