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
            {
                ErrorHandler.ShowError($"Failed to find Servers", "Missing File");
                return false;
            }

            return true;
        }

        public static bool CheckMissingFile(string file, string mes)
        {
            if (!File.Exists(file) || file == string.Empty)
            {
                return true;
            }

            return false;
        }

        #endregion ValidatePathConfig

        public static rAthena.Type GetProcessType(Process rAthenaProcess)
        {               
            try
            {
                switch (rAthenaProcess.ProcessName.ToLowerInvariant())
                {
                    case var n when n == GetFileName(Configuration.LoginPath).ToLowerInvariant():
                        return rAthena.Type.Login;
                    case var n when n == GetFileName(Configuration.CharPath).ToLowerInvariant():
                        return rAthena.Type.Char;
                    case var n when n == GetFileName(Configuration.WebPath).ToLowerInvariant():
                        return rAthena.Type.Web;
                    case var n when n == GetFileName(Configuration.MapPath).ToLowerInvariant():
                        return rAthena.Type.Map;
                    default:
                        return rAthena.Type.DevConsole;
                }
            }
            catch(Exception)
            {
                return rAthena.Type.DevConsole;
            }
        }
    }
}