using System.Windows.Documents;
using System.Windows.Media;

namespace AoShinhoServ_Monitor
{
    public class IText
    {
        public static Run RunColoredText(string text, Brush typeColor)
        {
            Run typeRun = new Run(text);
            typeRun.Foreground = typeColor;
            return typeRun;
        }

        public static Paragraph AppendColoredText(rAthena.Data Data)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(RunColoredText(Data.Header, Data.Paint));
            paragraph.Inlines.Add(RunColoredText(Data.Body, GetWhiteModeColor()));
            return paragraph;
        }

        public static Brush GetMessageTypeColor(rAthena.Data Data)
        {
            switch (Data.Header)
            {
                case "[Error]":
                    return Brushes.Red;

                case "[Debug]":
                    return Brushes.Aqua;

                case "[SQL]":
                    return Brushes.BlueViolet;

                case "[Warning]":
                    return Brushes.Orange;

                case "[Users]":
                case "[Status]":
                    return Brushes.Green;

                default: return GetWhiteModeColor();
            }
        }

        public static Brush GetWhiteModeColor(bool is_background = false) => (Properties.Settings.Default.WhiteMode && !is_background || !Properties.Settings.Default.WhiteMode && is_background) ? Brushes.Black : Brushes.White;

        public static void Do_Starting_Message(
            System.Windows.Controls.RichTextBox Char,
            System.Windows.Controls.RichTextBox Login,
            System.Windows.Controls.RichTextBox Map,
            System.Windows.Controls.RichTextBox Web)
        {
            Brush color = GetWhiteModeColor();

            rAthena.Data Data = new rAthena.Data
            {
                Header = "[Info]: ",
                Paint = color,
                Body = "Login Server is Waiting..."
            };
            Starting_Message_sub(Login, Data);

            Data.Body = "Char Server is Waiting...";
            Starting_Message_sub(Char, Data);

            Data.Body = "Web Server is Waiting...";
            Starting_Message_sub(Web, Data);

            Data.Body = "Map Server is Waiting...";
            Starting_Message_sub(Map, Data);
        }

        private static void Starting_Message_sub(System.Windows.Controls.RichTextBox Box, rAthena.Data Data) => Box.Document.Blocks.Add(AppendColoredText(Data));

    }
}