using System;
using System.Globalization;
using System.Drawing;

namespace dg.Utilities
{
    public static class ColorHelper
    {
        private static CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        public static string GetCss(Color color)
        {
            if (color == Color.Transparent) return @"transparent";
            if (color == Color.Empty) return @"";
            return string.Format(InvariantCulture, @"#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        public static string GetCss(Color color, bool hash)
        {
            if (color == Color.Transparent) return @"transparent";
            if (color == Color.Empty) return @"";
            if (hash) return string.Format(InvariantCulture, @"#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
            else return string.Format(InvariantCulture, @"{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        public static string GetCssRgb(Color color)
        {
            if (color == Color.Transparent) return @"transparent";
            if (color == Color.Empty) return @"";
            return string.Format(InvariantCulture, @"rgb({0},{1},{2})", color.R, color.G, color.B);
        }

        public static string GetCssRgba(Color color)
        {
            if (color == Color.Transparent) return @"transparent";
            if (color == Color.Empty) return @"";
            return string.Format(InvariantCulture, @"rgba({0},{1},{2},{3:0.##})", color.R, color.G, color.B, color.A / 255.0);
        }

        public static Color FromCss(string css)
        {
            css = css.Trim();

            if (css.StartsWith(@"rgba("))
            {
                css = css.Trim('r', 'g', 'b', 'a', '(', ')', ' ', ';');
                string[] rgba = css.Split(',');
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
            else if (css.StartsWith(@"argb("))
            {
                css = css.Trim('r', 'g', 'b', 'a', '(', ')', ' ', ';');
                string[] rgba = css.Split(',');
                if (rgba.Length == 4)
                {
                    try
                    {
                        return Color.FromArgb(
                            (int)(Convert.ToDecimal(rgba[0].Trim(), InvariantCulture) * 255),
                            Convert.ToInt32(rgba[1].Trim(), InvariantCulture),
                            Convert.ToInt32(rgba[2].Trim(), InvariantCulture),
                            Convert.ToInt32(rgba[3].Trim(), InvariantCulture)
                            );
                    }
                    catch { }
                }
                return Color.Empty;
            }
            else if (css.StartsWith(@"rgb("))
            {
                css = css.Trim('r', 'g', 'b', 'a', '(', ')', ' ', ';');
                string[] rgba = css.Split(',');
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
            else if (css.Equals(@"transparent"))
            {
                return Color.Transparent;
            }
            else
            {
                css = css.Trim('#', ' ', ';');
                if (css.Length == 3) css = "" + (char)css[0] + (char)css[0] + (char)css[1] + (char)css[1] + (char)css[2] + (char)css[2];
                if (css.Length == 6)
                {
                    try
                    {
                        return Color.FromArgb(
                            Int32.Parse(css.Substring(0, 2), NumberStyles.HexNumber, InvariantCulture),
                            Int32.Parse(css.Substring(2, 2), NumberStyles.HexNumber, InvariantCulture),
                            Int32.Parse(css.Substring(4, 2), NumberStyles.HexNumber, InvariantCulture)
                            );
                    }
                    catch
                    {
                        return Color.Empty;
                    }
                }
                else if (css.Length == 8)
                {
                    try
                    {
                        return Color.FromArgb(
                            Int32.Parse(css.Substring(6, 2), NumberStyles.HexNumber, InvariantCulture),
                            Int32.Parse(css.Substring(0, 2), NumberStyles.HexNumber, InvariantCulture),
                            Int32.Parse(css.Substring(2, 2), NumberStyles.HexNumber, InvariantCulture),
                            Int32.Parse(css.Substring(4, 2), NumberStyles.HexNumber, InvariantCulture)
                            );
                    }
                    catch
                    {
                        return Color.Empty;
                    }
                }

                return Color.Empty;
            }
        }

        public static void ToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color FromHSV(double hue, double saturation, double value)
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
