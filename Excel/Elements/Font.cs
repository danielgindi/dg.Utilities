using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace dg.Utilities
{
    public partial class Excel
    {
        public class Font
        {
            public bool Bold;
            public Color Color;
            public string FontName;
            public bool Italic;
            public bool Outline;
            public bool Shadow;
            public double Size = 10.0;
            public bool StrikeThrough;
            public FontUnderline Underline;
            public FontVerticalAlign VerticalAlign;
            [CLSCompliant(false)]
            public UInt32 CharSet;
            public FontFamily Family;
        }
    }
}