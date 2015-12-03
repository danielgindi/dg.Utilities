using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;

namespace dg.Utilities.Imaging.Processing.Filters
{
    public class ReduceTransparency : IImageFilter
    {
        public FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            float transparencyMultiplier = 0.7f;

            foreach (object arg in args)
            {
                if (arg is Single || 
                    arg is Double)
                {
                    transparencyMultiplier = Convert.ToSingle(arg);
                }
            }

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, transparencyMultiplier);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, transparencyMultiplier);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32rgba(bmp, transparencyMultiplier);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32prgba(bmp, transparencyMultiplier);
                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }

        public FilterError ProcessImage24rgb(DirectAccessBitmap bmp, float transparencyMultiplier)
        {
            return FilterError.OK;
        }

        public FilterError ProcessImage32rgb(DirectAccessBitmap bmp, float transparencyMultiplier)
        {
            return FilterError.OK;
        }

        public FilterError ProcessImage32rgba(DirectAccessBitmap bmp, float transparencyMultiplier)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            int alphaByte;

            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;

                    alphaByte = data[pos2 + 3];
                    if (alphaByte != 0 && alphaByte != 255)
                    {
                        alphaByte = 255 - (int)Math.Round((255 - alphaByte) * transparencyMultiplier);
                        if (alphaByte > 255)
                        {
                            alphaByte = 255;
                        }
                        else if (alphaByte < 0)
                        {
                            alphaByte = 0;
                        }
                        data[pos2 + 3] = (byte)alphaByte;
                    }
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, float transparencyMultiplier)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            float preAlpha, postAlpha;

            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    preAlpha = (float)data[pos2 + 3];
                    if (preAlpha != 0.0f & preAlpha != 1.0f)
                    {
                        if (preAlpha > 0)
                        {
                            preAlpha = preAlpha / 255f;
                        }

                        postAlpha = 1.0f - ((1.0f - preAlpha) * transparencyMultiplier);
                        if (postAlpha > 1.0f)
                        {
                            postAlpha = 1.0f;
                        }
                        else if (postAlpha < 0.0f)
                        {
                            postAlpha = 0.0f;
                        }

                        data[pos2] = (byte)((data[pos2] / preAlpha) * postAlpha);
                        data[pos2 + 1] = (byte)((data[pos2 + 1] / preAlpha) * postAlpha);
                        data[pos2 + 2] = (byte)((data[pos2 + 2] / preAlpha) * postAlpha);
                        data[pos2 + 3] = (byte)(postAlpha * 255.0f);
                    }
                }
            }

            return FilterError.OK;
        }
    }
}
