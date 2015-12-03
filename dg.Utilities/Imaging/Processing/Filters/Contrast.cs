using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;

namespace dg.Utilities.Imaging.Processing.Filters
{
    public class Contrast : IImageFilter
    {
        public class Amount
        {
            public float Value = 0;
            public Amount(float amount)
            {
                this.Value = amount;
            }
        }

        public FilterError ProcessImage(
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
            if (amount == null) return FilterError.MissingArgument;

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, amount);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, amount);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32rgba(bmp, amount);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32prgba(bmp, amount);
                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }
        public FilterError ProcessImage24rgb(DirectAccessBitmap bmp, Amount amount)
        {
            if (amount == null) return FilterError.MissingArgument;
            if (amount.Value == 1f) return FilterError.OK;

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
            float value;
            int cxbl;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                cxbl = pos1 + endXb;
                for (xb = pos1 + bmp.StartX * 3; xb < cxbl; xb++)
                {
                    value = data[xb];
                    value -= 127.5f;
                    value *= factor;
                    value += 127.5f;
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[xb] = (byte)value;
                }
            }
            return FilterError.OK;
        }
        public FilterError ProcessImage32rgb(DirectAccessBitmap bmp, Amount amount)
        {
            if (amount == null) return FilterError.MissingArgument;
            if (amount.Value == 1f) return FilterError.OK;

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
            float value;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    value = data[pos2];
                    value -= 127.5f;
                    value *= factor;
                    value += 127.5f;
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2] = (byte)value;
                    value = data[pos2 + 1];
                    value -= 127.5f;
                    value *= factor;
                    value += 127.5f;
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 1] = (byte)value;
                    value = data[pos2 + 2];
                    value -= 127.5f;
                    value *= factor;
                    value += 127.5f;
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 2] = (byte)value;
                }
            }
            return FilterError.OK;
        }
        public FilterError ProcessImage32rgba(DirectAccessBitmap bmp, Amount amount)
        {
            return ProcessImage32rgb(bmp, amount);
        }
        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, Amount amount)
        {
            if (amount == null) return FilterError.MissingArgument;
            if (amount.Value == 1f) return FilterError.OK;

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
            float value;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    preAlpha = (float)data[pos2 + 3];
                    if (preAlpha > 0) preAlpha = preAlpha / 255f;

                    value = data[pos2];
                    value /= preAlpha;
                    value -= 127.5f;
                    value *= factor;
                    value += 127.5f;
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2] = (byte)(value * preAlpha);
                    value = data[pos2 + 1];
                    value /= preAlpha;
                    value -= 127.5f;
                    value *= factor;
                    value += 127.5f;
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 1] = (byte)(value * preAlpha);
                    value = data[pos2 + 2];
                    value /= preAlpha;
                    value -= 127.5f;
                    value *= factor;
                    value += 127.5f;
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 2] = (byte)(value * preAlpha);
                }
            }
            return FilterError.OK;
        }
    }
}
