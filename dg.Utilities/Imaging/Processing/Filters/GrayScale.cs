using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;

namespace dg.Utilities.Imaging.Processing.Filters
{
    public class GrayScale : IImageFilter
    {
        public FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            FilterGrayScaleWeight mode = FilterGrayScaleWeight.Natural;
            foreach (object arg in args)
            {
                if (arg is FilterGrayScaleWeight)
                {
                    mode = (FilterGrayScaleWeight)arg;
                }
            }

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, mode);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, mode);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32rgba(bmp, mode);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32prgba(bmp, mode);
                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }

        public FilterError ProcessImage24rgb(DirectAccessBitmap bmp, FilterGrayScaleWeight mode)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;

            if (mode == FilterGrayScaleWeight.Accurate)
            {
                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 3;
                        data[pos2] = data[pos2 + 1] = data[pos2 + 2] =
                            (byte)((data[pos2] + data[pos2 + 1] + data[pos2 + 2]) / 3f);
                    }
                }
            }
            else
            {
                float rL = 0, gL = 0, bL = 0;

                if (mode == FilterGrayScaleWeight.NaturalNTSC)
                {
                    rL = GrayScaleMultiplier.NtscRed;
                    gL = GrayScaleMultiplier.NtscGreen;
                    bL = GrayScaleMultiplier.NtscBlue;
                }
                else if (mode == FilterGrayScaleWeight.Natural)
                {
                    rL = GrayScaleMultiplier.NaturalRed;
                    gL = GrayScaleMultiplier.NaturalGreen;
                    bL = GrayScaleMultiplier.NaturalBlue;
                }
                else
                {
                    rL = GrayScaleMultiplier.AccurateRed;
                    gL = GrayScaleMultiplier.AccurateGreen;
                    bL = GrayScaleMultiplier.AccurateBlue;
                }

                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 3;
                        data[pos2] = data[pos2 + 1] = data[pos2 + 2] =
                            (byte)((data[pos2] * bL +
                            data[pos2 + 1] * gL +
                            data[pos2 + 2] * rL));
                    }
                }
            }
            return FilterError.OK;
        }

        public FilterError ProcessImage32rgb(DirectAccessBitmap bmp, FilterGrayScaleWeight mode)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;

            if (mode == FilterGrayScaleWeight.Accurate)
            {
                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 4;
                        data[pos2] = data[pos2 + 1] = data[pos2 + 2] =
                            (byte)((data[pos2] + data[pos2 + 1] + data[pos2 + 2]) / 3f);
                    }
                }
            }
            else
            {
                float rL = 0, gL = 0, bL = 0;

                if (mode == FilterGrayScaleWeight.NaturalNTSC)
                {
                    rL = GrayScaleMultiplier.NtscRed;
                    gL = GrayScaleMultiplier.NtscGreen;
                    bL = GrayScaleMultiplier.NtscBlue;
                }
                else if (mode == FilterGrayScaleWeight.Natural)
                {
                    rL = GrayScaleMultiplier.NaturalRed;
                    gL = GrayScaleMultiplier.NaturalGreen;
                    bL = GrayScaleMultiplier.NaturalBlue;
                }
                else
                {
                    rL = GrayScaleMultiplier.AccurateRed;
                    gL = GrayScaleMultiplier.AccurateGreen;
                    bL = GrayScaleMultiplier.AccurateBlue;
                }

                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 4;
                        data[pos2] = data[pos2 + 1] = data[pos2 + 2] =
                            (byte)((data[pos2] * bL +
                            data[pos2 + 1] * gL +
                            data[pos2 + 2] * rL));
                    }
                }
            }

            return FilterError.OK;
        }

        public FilterError ProcessImage32rgba(DirectAccessBitmap bmp, FilterGrayScaleWeight mode)
        {
            return ProcessImage32rgb(bmp, mode);
        }

        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, FilterGrayScaleWeight mode)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            float preAlpha;

            if (mode == FilterGrayScaleWeight.Accurate)
            {
                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 4;
                        preAlpha = (float)data[pos2 + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        data[pos2] = data[pos2 + 1] = data[pos2 + 2] =
                            (byte)(((
                            (data[pos2] / preAlpha) +
                            (data[pos2 + 1] / preAlpha) +
                            (data[pos2 + 2] / preAlpha)
                            ) / 3f) * preAlpha);
                    }
                }
            }
            else
            {
                float rL = 0, gL = 0, bL = 0;

                if (mode == FilterGrayScaleWeight.NaturalNTSC)
                {
                    rL = GrayScaleMultiplier.NtscRed;
                    gL = GrayScaleMultiplier.NtscGreen;
                    bL = GrayScaleMultiplier.NtscBlue;
                }
                else if (mode == FilterGrayScaleWeight.Natural)
                {
                    rL = GrayScaleMultiplier.NaturalRed;
                    gL = GrayScaleMultiplier.NaturalGreen;
                    bL = GrayScaleMultiplier.NaturalBlue;
                }
                else
                {
                    rL = GrayScaleMultiplier.AccurateRed;
                    gL = GrayScaleMultiplier.AccurateGreen;
                    bL = GrayScaleMultiplier.AccurateBlue;
                }

                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 4;
                        preAlpha = (float)data[pos2 + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        data[pos2] = data[pos2 + 1] = data[pos2 + 2] =
                            (byte)(((
                            (data[pos2] / preAlpha) * bL +
                            (data[pos2 + 1] / preAlpha) * gL +
                            (data[pos2 + 2] / preAlpha) * rL
                            )) * preAlpha);

                    }
                }
            }

            return FilterError.OK;
        }
    }
}
