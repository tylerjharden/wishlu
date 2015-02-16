using System.Drawing;

namespace Disco.Common
{
    public static class Helpers
    {
        public static Color ContrastColor(Color color)
        {
            /*int d = 0;
            // Counting the perceptive luminance - human eye favors green color... 
            double a = 1 - (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

            if (a < 0.5)
                d = 0; // bright colors - black font
            else
                d = 255; // dark colors - white font

            return Color.FromArgb(d, d, d);*/

            return Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
        }
    }
}