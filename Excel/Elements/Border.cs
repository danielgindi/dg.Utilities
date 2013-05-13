using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace dg.Utilities
{
    public partial class Excel
    {
        public class Border
        {
            public BorderPosition Position;
            public Color Color;
            public BorderLineStyle LineStyle;
            public double Weight;
            
            public Border(BorderPosition Position)
            {
                this.Position = Position;
            }

            public Border(BorderPosition Position, Color Color)
            {
                this.Position = Position;
                this.Color = Color;
            }

            public Border(BorderPosition Position, Color Color, BorderLineStyle LineStyle)
            {
                this.Position = Position;
                this.Color = Color;
                this.LineStyle = LineStyle;
            }

            public Border(BorderPosition Position, Color Color, BorderLineStyle LineStyle, double Weight)
            {
                this.Position = Position;
                this.Color = Color;
                this.LineStyle = LineStyle;
                this.Weight = Weight;
            }
        }
    }
}