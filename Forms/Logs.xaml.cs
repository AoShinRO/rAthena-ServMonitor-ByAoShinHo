using System.IO;
using System;
using System.Windows;
using Microsoft.Win32;
using static AoShinhoServ_Monitor.MainDefs;

namespace AoShinhoServ_Monitor.Forms
{
    /// <summary>
    /// Lógica interna para Logs.xaml
    /// </summary>
    public partial class Logs : Window
    {
        public Logs()
        {
            InitializeComponent();
        }
        #region ThisFunctionIsInMainWindow
        private void CloseLog_Click(object sender, RoutedEventArgs e) => Hide();

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Archive (*.txt)|*.txt";
            saveFileDialog.Title = "Save Logs";
            string fileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            saveFileDialog.FileName = fileName;
            if (saveFileDialog.ShowDialog() == true)
            {
                string directoryPath = Path.GetDirectoryName(saveFileDialog.FileName);
                string filePath = Path.Combine(directoryPath, fileName);

                try
                {
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        foreach (var log in errorLogs)
                        {
                            writer.WriteLine($"{log.Type} {log.Content}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fail saving logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Hide();
            }
        }
        #endregion
    }
}
