using System;
using System.IO;
using System.Diagnostics;

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
                ErrorHandler.ShowError(ex.Message, $"Failed to find Process {ProcessName}");
                return;
            }
            foreach (Process process in processes)
                process.Kill();
            
        }

        public static bool Do_Kill_All()
        {
            KillAll(GetFileName(Configuration.LoginPath));
            KillAll(GetFileName(Configuration.CharPath));
            KillAll(GetFileName(Configuration.WebPath));
            KillAll(GetFileName(Configuration.MapPath));
            return true;
        }

        public static string GetFileName(string FilePath) => System.IO.Path.GetFileNameWithoutExtension(FilePath);

        #region ValidatePathConfig

        public static bool CheckServerPath()
        {
            if (CheckMissingFile(Configuration.LoginPath, "login-server.exe") ||
               CheckMissingFile(Configuration.CharPath, "char-server.exe") ||
               CheckMissingFile(Configuration.WebPath, "web-server.exe") ||
               CheckMissingFile(Configuration.MapPath, "map-server.exe"))
                return false;

            return true;
        }

        public static bool CheckMissingFile(string file, string mes)
        {
            if (!File.Exists(file) || file == string.Empty)
            {
                ErrorHandler.ShowError($"File \"{mes}\" at \"{file}\" is missing", "Missing File");
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
                case var n when n == GetFileName(Configuration.LoginPath).ToLowerInvariant():
                    type = rAthena.Type.Login;
                    break;
                case var n when n == GetFileName(Configuration.CharPath).ToLowerInvariant():
                    type = rAthena.Type.Char;
                    break;
                case var n when n == GetFileName(Configuration.WebPath).ToLowerInvariant():
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