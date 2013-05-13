﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;

namespace dg.Utilities.Imaging.Filters
{
    public class Invert : IImageFilter
    {
        public ImageFilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32argb(bmp);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32pargb(bmp);
                default:
                    return ImageFilterError.IncompatiblePixelFormat;
            }
        }
        public ImageFilterError ProcessImage24rgb(DirectAccessBitmap bmp)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 3;
                    data[pos2] = (byte)(255 - data[pos2]);
                    data[pos2 + 1] = (byte)(255 - data[pos2 + 1]);
                    data[pos2 + 2] = (byte)(255 - data[pos2 + 2]);
                }
            }
            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32rgb(DirectAccessBitmap bmp)
        {
            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    data[pos2] = (byte)(255 - data[pos2]);
                    data[pos2 + 1] = (byte)(255 - data[pos2 + 1]);
                    data[pos2 + 2] = (byte)(255 - data[pos2 + 2]);
                }
            }
            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32argb(DirectAccessBitmap bmp)
        {
            return ProcessImage32rgb(bmp);
        }
        public ImageFilterError ProcessImage32pargb(DirectAccessBitmap bmp)
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
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    preAlpha = (float)data[pos2 + 3];
                    if (preAlpha > 0) preAlpha = preAlpha / 255f;
                    data[pos2] = (byte)((255f - (data[pos2] / preAlpha)) * preAlpha);
                    data[pos2 + 1] = (byte)((255f - (data[pos2 + 1] / preAlpha)) * preAlpha);
                    data[pos2 + 2] = (byte)((255f - (data[pos2 + 2] / preAlpha)) * preAlpha);
                }
            }
            return ImageFilterError.OK;
        }
    }
}
