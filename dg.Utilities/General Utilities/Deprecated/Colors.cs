using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Drawing;

namespace dg.Utilities
{
    public static class Colors
    {
        [Obsolete]
        public static string GetCss(Color color)
        {
            return ColorHelper.GetCss(color);
        }

        [Obsolete]
        public static string GetCss(Color color, bool numberSign)
        {
            return ColorHelper.GetCss(color, numberSign);
        }

        [Obsolete]
        public static string GetCssRgb(Color color)
        {
            return ColorHelper.GetCssRgb(color);
        }

        [Obsolete]
        public static string GetCssRgba(Color color)
        {
            return ColorHelper.GetCssRgba(color);
        }

        [Obsolete]
        public static Color FromCss(string css)
        {
            return ColorHelper.FromCss(css);
        }

        [Obsolete]
        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            ColorHelper.ToHSV(color, out hue, out saturation, out value);
        }

        [Obsolete]
        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            return ColorHelper.FromHSV(hue, saturation, value);
        }
    }
}
