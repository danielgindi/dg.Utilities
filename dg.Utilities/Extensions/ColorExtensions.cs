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

        public static string Css(this Color Input)
        {
            return ColorHelper.GetCss(Input);
        }

        public static string Css(this Color Input, bool NumberSign)
        {
            return ColorHelper.GetCss(Input, NumberSign);
        }

        public static string CssRgb(this Color Input)
        {
            return ColorHelper.GetCssRgb(Input);
        }

        public static string CssRgba(this Color Input)
        {
            return ColorHelper.GetCssRgba(Input);
        }
    }
}
