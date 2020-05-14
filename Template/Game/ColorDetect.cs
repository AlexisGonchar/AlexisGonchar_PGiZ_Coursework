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
        Green,
        White,
        Red,
        Blue
    }
    public static class ColorDetect
    {
        public static ColorBMP Detect(System.Drawing.Color c)
        {
            if (c.R == 0 && c.G == 0 && c.B == 0)
                return ColorBMP.Black;
            else if (c.R == 255 && c.G == 255 && c.B == 0)
                return ColorBMP.Yellow;
            else if (c.R == 96 && c.G == 96 && c.B == 128)
                return ColorBMP.Grey;
            else if (c.R == 0 && c.G == 255 && c.B == 0)
                return ColorBMP.Green;
            else if (c.R == 255 && c.G == 0 && c.B == 0)
                return ColorBMP.Red;
            else if (c.R == 0 && c.G == 0 && c.B == 255)
                return ColorBMP.Blue;
            return ColorBMP.White;
        }
    }
}
