using System.Windows;

namespace AoShinhoServ_Monitor
{
    public static class ErrorHandler
    {  
        public static void ShowError(string message, string context = "")
        {  
            var fullMessage = string.IsNullOrEmpty(context)
                ? message
                : $"{context}: {message}";  
            MessageBox.Show(fullMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);  
        }
    }
}
