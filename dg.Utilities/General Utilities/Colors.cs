using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Drawing;

namespace dg.Utilities
{
    public static class Colors
    {
        private static CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        public static string GetCss(Color Color)
        {
            if (Color == Color.Transparent) return @"transparent";
            if (Color == Color.Empty) return @"";
            return string.Format(InvariantCulture, @"#{0:x2}{1:x2}{2:x2}", Color.R, Color.G, Color.B);
        }
        public static string GetCss(Color Color, bool NumberSign)
        {
            if (Color == Color.Transparent) return @"transparent";
            if (Color == Color.Empty) return @"";
            if (NumberSign) return string.Format(InvariantCulture, @"#{0:x2}{1:x2}{2:x2}", Color.R, Color.G, Color.B);
            else return string.Format(InvariantCulture, @"{0:x2}{1:x2}{2:x2}", Color.R, Color.G, Color.B);
        }
        public static string GetCssRgb(Color Color)
        {
            if (Color == Color.Transparent) return @"transparent";
            if (Color == Color.Empty) return @"";
            return string.Format(InvariantCulture, @"rgb({0},{1},{2})", Color.R, Color.G, Color.B);
        }
        public static string GetCssRgba(Color Color)
        {
            if (Color == Color.Transparent) return @"transparent";
            if (Color == Color.Empty) return @"";
            return string.Format(InvariantCulture, @"rgba({0},{1},{2},{3:0.##})", Color.R, Color.G, Color.B, Color.A / 255.0);
        }
        public static Color FromCss(string Css)
        {
            Css = Css.Trim();
            if (Css.StartsWith(@"rgba("))
            {
                Css = Css.Trim('r', 'g', 'b', 'a', '(', ')', ' ', ';');
                string[] rgba = Css.Split(',');
                if (rgba.Length == 4)
                {
                    try
                    {
                        return Color.FromArgb(
                            (int)(Convert.ToDecimal(rgba[3].Trim(), InvariantCulture) * 255),
                            Convert.ToInt32(rgba[0].Trim(), InvariantCulture),
                            Convert.ToInt32(rgba[1].Trim(), InvariantCulture),
                            Convert.ToInt32(rgba[2].Trim(), InvariantCulture)
                            );
                    }
                    catch { }
                }
                return Color.Empty;
            }
            else if (Css.StartsWith(@"rgb("))
            {
                Css = Css.Trim('r', 'g', 'b', 'a', '(', ')', ' ', ';');
                string[] rgba = Css.Split(',');
                if (rgba.Length == 3)
                {
                    try
                    {
                        return Color.FromArgb(
                              Convert.ToInt32(rgba[0].Trim(), InvariantCulture),
                              Convert.ToInt32(rgba[1].Trim(), InvariantCulture),
                              Convert.ToInt32(rgba[2].Trim(), InvariantCulture)
                              );
                    }
                    catch { }
                }
                return Color.Empty;
            }
            else if (Css.Equals(@"transparent"))
            {
                return Color.Transparent;
            }
            else
            {
                Css = Css.Trim('#', ' ', ';');
                if (Css.Length == 3) Css = "" + (char)Css[0] + (char)Css[0] + (char)Css[1] + (char)Css[1] + (char)Css[2] + (char)Css[2];
                if (Css.Length != 6) return Color.Empty;
                try
                {
                    return Color.FromArgb(
                        Int32.Parse(Css.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, InvariantCulture),
                        Int32.Parse(Css.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, InvariantCulture),
                        Int32.Parse(Css.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, InvariantCulture)
                        );
                }
                catch
                {
                    return Color.Empty;
                }
            }
        }

        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Math.Max(Convert.ToInt32(value), 0);
            int p = Math.Max(Convert.ToInt32(value * (1 - saturation)), 0);
            int q = Math.Max(Convert.ToInt32(value * (1 - f * saturation)), 0);
            int t = Math.Max(Convert.ToInt32(value * (1 - (1 - f) * saturation)), 0);

            if (hi == 0) return Color.FromArgb(255, v, t, p);
            else if (hi == 1) return Color.FromArgb(255, q, v, p);
            else if (hi == 2) return Color.FromArgb(255, p, v, t);
            else if (hi == 3) return Color.FromArgb(255, p, q, v);
            else if (hi == 4) return Color.FromArgb(255, t, p, v);
            else return Color.FromArgb(255, v, p, q);
        }
    }
}
