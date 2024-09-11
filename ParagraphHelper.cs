using System.Windows.Documents;
using System.Windows.Media;

namespace AoShinhoServ_Monitor
{
    public static class ParagraphHelper
    {
        public static Run ColoredText(string text, Brush typeColor)
        {
            Run typeRun = new Run(text);
            typeRun.Foreground = typeColor;
            return typeRun;
        }

        public static Paragraph AppendColoredText(string type, string info, Brush typeColor)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(ColoredText(type, typeColor));
            paragraph.Inlines.Add(ColoredText(info, GetMessageTypeColor(info)));
            return paragraph;
        }

        public static Brush GetMessageTypeColor(string messageType)
        {
            switch (messageType)
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

                default: return WhiteModeColor();
            }
        }

        public static Brush WhiteModeColor(bool reverse = false) => (Properties.Settings.Default.WhiteMode && !reverse || !Properties.Settings.Default.WhiteMode && reverse) ? Brushes.Black : Brushes.White;
    }
}