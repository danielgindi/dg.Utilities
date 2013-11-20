using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;

namespace dg.Utilities.Imaging.Filters
{
    public class GrayScale : IImageFilter
    {
        public enum Mode
        {
            Natural = 0,
            NaturalNTSC = 1,
            Accurate = 2
        }

        public ImageFilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            Mode mode = Mode.Natural;
            foreach (object arg in args)
            {
                if (arg is Mode)
                {
                    mode = (Mode)arg;
                }
            }

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, mode);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, mode);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32argb(bmp, mode);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32pargb(bmp, mode);
                default:
                    return ImageFilterError.IncompatiblePixelFormat;
            }
        }
        public ImageFilterError ProcessImage24rgb(DirectAccessBitmap bmp, Mode mode)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;

            if (mode == Mode.Accurate)
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
                if (mode == Mode.NaturalNTSC)
                {
                    rL = ImagingUtility.RedLuminosityNTSC;
                    gL = ImagingUtility.GreenLuminosityNTSC;
                    bL = ImagingUtility.BlueLuminosityNTSC;
                }
                else
                {
                    rL = ImagingUtility.RedLuminosity;
                    gL = ImagingUtility.GreenLuminosity;
                    bL = ImagingUtility.BlueLuminosity;
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
            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32rgb(DirectAccessBitmap bmp, Mode mode)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;

            if (mode == Mode.Accurate)
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
                if (mode == Mode.NaturalNTSC)
                {
                    rL = ImagingUtility.RedLuminosityNTSC;
                    gL = ImagingUtility.GreenLuminosityNTSC;
                    bL = ImagingUtility.BlueLuminosityNTSC;
                }
                else
                {
                    rL = ImagingUtility.RedLuminosity;
                    gL = ImagingUtility.GreenLuminosity;
                    bL = ImagingUtility.BlueLuminosity;
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

            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32argb(DirectAccessBitmap bmp, Mode mode)
        {
            return ProcessImage32rgb(bmp, mode);
        }
        public ImageFilterError ProcessImage32pargb(DirectAccessBitmap bmp, Mode mode)
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

            if (mode == Mode.Accurate)
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
                if (mode == Mode.NaturalNTSC)
                {
                    rL = ImagingUtility.RedLuminosityNTSC;
                    gL = ImagingUtility.GreenLuminosityNTSC;
                    bL = ImagingUtility.BlueLuminosityNTSC;
                }
                else
                {
                    rL = ImagingUtility.RedLuminosity;
                    gL = ImagingUtility.GreenLuminosity;
                    bL = ImagingUtility.BlueLuminosity;
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

            return ImageFilterError.OK;
        }
    }
}
