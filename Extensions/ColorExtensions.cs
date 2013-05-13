using System;
using System.Drawing;

namespace dg.Utilities
{
    public static class ColorExtensions
    {
        public static bool IsTransparent(this Color Input)
        {
            return Input == Color.Transparent;
        }

        public static bool IsTransparentOrEmpty(this Color Input)
        {
            return Input.IsEmpty || Input == Color.Transparent;
        }

        public static string Css(this Color Input, bool NumberSign = true)
        {
            return Colors.GetCss(Input, NumberSign);
        }

        public static string CssRgb(this Color Input)
        {
            return Colors.GetCssRgb(Input);
        }

        public static string CssRgba(this Color Input)
        {
            return Colors.GetCssRgba(Input);
        }
    }
}
