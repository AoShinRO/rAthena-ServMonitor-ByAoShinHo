using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AoShinhoServ_Monitor
{
    public class IProcess
    {

        public static void KillAll(string ProcessName)
        {
            Process[] processes;
            try
            {
                processes = Process.GetProcessesByName(ProcessName);
                if (processes == null || processes.Length == 0)
                    return;
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to find Process {ProcessName} {ex.Message}");
                return;
            }
            foreach (Process process in processes)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to Killing Process {ProcessName} {ex.Message}");
                }
            }

        }

        public static bool Do_Kill_All()
        {
            KillAll(GetFileName(Properties.Settings.Default.LoginPath));
            KillAll(GetFileName(Properties.Settings.Default.CharPath));
            KillAll(GetFileName(Properties.Settings.Default.WebPath));
            KillAll(GetFileName(Properties.Settings.Default.MapPath));
            return true;
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

        public static rAthena.Type GetProcessType(Process rAthenaProcess)
        {
            rAthena.Type type;
            switch (rAthenaProcess.ProcessName.ToLowerInvariant())
            {
                case var n when n == GetFileName(Properties.Settings.Default.LoginPath).ToLowerInvariant():
                    type = rAthena.Type.Login;
                    break;
                case var n when n == GetFileName(Properties.Settings.Default.CharPath).ToLowerInvariant():
                    type = rAthena.Type.Char;
                    break;
                case var n when n == GetFileName(Properties.Settings.Default.WebPath).ToLowerInvariant():
                    type = rAthena.Type.Web;
                    break;
                default:
                    type = rAthena.Type.Map;
                    break;
            }
            return type;
        }
    }
}