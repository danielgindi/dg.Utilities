﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;

namespace dg.Utilities.Imaging.Filters
{
    public class Brightness : IImageFilter
    {
        public class Amount
        {
            public float Value = 0;
            public Amount(float amount)
            {
                this.Value = amount;
            }
        }

        public ImageFilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            Amount amount = null;
            foreach (object arg in args)
            {
                if (arg is Amount)
                {
                    amount = (Amount)arg;
                }
            }
            if (amount == null) return ImageFilterError.MissingArgument;

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, amount);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, amount);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32argb(bmp, amount);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32pargb(bmp, amount);
                default:
                    return ImageFilterError.IncompatiblePixelFormat;
            }
        }
        public ImageFilterError ProcessImage24rgb(DirectAccessBitmap bmp, Amount amount)
        {
            if (amount == null) return ImageFilterError.MissingArgument;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            int endXb = endX * (bmp.PixelFormatSize / 8);
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1;
            int xb, y;
            float factor = amount.Value;
            if (factor < 0f) factor = 0f;
            int value;
            int cxbl;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                cxbl = pos1 + endXb;
                for (xb = pos1 + bmp.StartX * 3; xb < cxbl; xb++)
                {
                    value = (int)(factor * data[xb]);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[xb] = (byte)value;
                }
            }
            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32rgb(DirectAccessBitmap bmp, Amount amount)
        {
            if (amount == null) return ImageFilterError.MissingArgument;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            float factor = amount.Value;
            if (factor < 0f) factor = 0f;
            int value;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    value = (int)(factor * data[pos2]);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2] = (byte)value;
                    value = (int)(factor * data[pos2 + 1]);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 1] = (byte)value;
                    value = (int)(factor * data[pos2 + 2]);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 2] = (byte)value;
                }
            }
            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32argb(DirectAccessBitmap bmp, Amount amount)
        {
            return ProcessImage32rgb(bmp, amount);
        }
        public ImageFilterError ProcessImage32pargb(DirectAccessBitmap bmp, Amount amount)
        {
            if (amount == null) return ImageFilterError.MissingArgument;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            float preAlpha;
            float factor = amount.Value;
            if (factor < 0f) factor = 0f;
            int value;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    preAlpha = (float)data[pos2 + 3];
                    if (preAlpha > 0) preAlpha = preAlpha / 255f;

                    value = (int)(factor * (data[pos2] / preAlpha));
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2] = (byte)(value * preAlpha);
                    value = (int)(factor * (data[pos2 + 1] / preAlpha));
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 1] = (byte)(value * preAlpha);
                    value = (int)(factor * (data[pos2 + 2] / preAlpha));
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 2] = (byte)(value * preAlpha);
                }
            }
            return ImageFilterError.OK;
        }
    }
}
