using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities
{
    public partial class Excel
    {
        public class Alignment
        {
            public HorizontalAlignment Horizontal;
            public VerticalAlignment Vertical;
            [CLSCompliant(false)]
            public UInt32 Indent;
            public HorizontalReadingOrder ReadingOrder;
            public double Rotate;
            public bool ShrinkToFit;
            public bool VerticalText;
            public bool WrapText;
        }
    }
}