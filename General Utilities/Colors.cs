using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities
{
    public static class Colors
    {
        public static string GetCss(System.Drawing.Color Color, bool NumberSign = true)
        {
            if (Color == System.Drawing.Color.Transparent) return @"transparent";
            if (Color == System.Drawing.Color.Empty) return @"";
            if (NumberSign) return string.Format(@"#{0:x2}{1:x2}{2:x2}", Color.R, Color.G, Color.B);
            else return string.Format(@"{0:x2}{1:x2}{2:x2}", Color.R, Color.G, Color.B);
        }
        public static string GetCssRgb(System.Drawing.Color Color)
        {
            if (Color == System.Drawing.Color.Transparent) return @"transparent";
            if (Color == System.Drawing.Color.Empty) return @"";
            return string.Format(@"rgb({0},{1},{2})", Color.R, Color.G, Color.B);
        }
        public static string GetCssRgba(System.Drawing.Color Color)
        {
            if (Color == System.Drawing.Color.Transparent) return @"transparent";
            if (Color == System.Drawing.Color.Empty) return @"";
            return string.Format(@"rgba({0},{1},{2},{3:0.##})", Color.R, Color.G, Color.B, Color.A / 255.0);
        }
        public static System.Drawing.Color FromCss(string Css)
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
                        return System.Drawing.Color.FromArgb(
                            (int)(Convert.ToDecimal(rgba[3].Trim()) * 255),
                            Convert.ToInt32(rgba[0].Trim()),
                            Convert.ToInt32(rgba[1].Trim()),
                            Convert.ToInt32(rgba[2].Trim())
                            );
                    }
                    catch { }
                }
                return System.Drawing.Color.Empty;
            }
            else if (Css.StartsWith(@"rgb("))
            {
                Css = Css.Trim('r', 'g', 'b', 'a', '(', ')', ' ', ';');
                string[] rgba = Css.Split(',');
                if (rgba.Length == 3)
                {
                    try
                    {
                        return System.Drawing.Color.FromArgb(
                              Convert.ToInt32(rgba[0].Trim()),
                              Convert.ToInt32(rgba[1].Trim()),
                              Convert.ToInt32(rgba[2].Trim())
                              );
                    }
                    catch { }
                }
                return System.Drawing.Color.Empty;
            }
            else if (Css.Equals(@"transparent"))
            {
                return System.Drawing.Color.Transparent;
            }
            else
            {
                Css = Css.Trim('#', ' ', ';');
                if (Css.Length == 3) Css = "" + (char)Css[0] + (char)Css[0] + (char)Css[1] + (char)Css[1] + (char)Css[2] + (char)Css[2];
                if (Css.Length != 6) return System.Drawing.Color.Empty;
                try
                {
                    return System.Drawing.Color.FromArgb(
                        Int32.Parse(Css.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                        Int32.Parse(Css.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                        Int32.Parse(Css.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)
                        );
                }
                catch
                {
                    return System.Drawing.Color.Empty;
                }
            }
        }
    }
}
