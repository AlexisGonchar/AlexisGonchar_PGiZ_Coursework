using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    public enum ColorBMP
    {
        Black,
        Yellow,
        Grey,
        White
    }
    public static class ColorDetect
    {
        public static ColorBMP Detect(System.Drawing.Color c)
        {
            if (c.R == 0 && c.G == 0 && c.B == 0)
                return ColorBMP.Black;
            if (c.R == 255 && c.G == 255 && c.B == 0)
                return ColorBMP.Yellow;
            if (c.R == 96 && c.G == 96 && c.B == 128)
                return ColorBMP.Grey;
            return ColorBMP.White;
        }
    }
}
