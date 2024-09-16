using System.Windows.Documents;
using System.Windows.Media;
using static AoShinhoServ_Monitor.Consts;

namespace AoShinhoServ_Monitor
{
    public static class ParagraphHelper
    {
        public static Run RunColoredText(string text, Brush typeColor)
        {
            Run typeRun = new Run(text);
            typeRun.Foreground = typeColor;
            return typeRun;
        }

        public static Paragraph AppendColoredText(rAthenaData Data)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(RunColoredText(Data.SvType, Data.SvBrush));
            paragraph.Inlines.Add(RunColoredText(Data.SvInfo, GetWhiteModeColor()));
            return paragraph;
        }

        public static Brush GetMessageTypeColor(rAthenaData Data)
        {
            switch (Data.SvType)
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
    }
}