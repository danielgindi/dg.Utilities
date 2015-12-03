using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;

namespace dg.Utilities.Imaging.Processing.Filters
{
    public class ColorFilter : IImageFilter
    {
        public class ColorFilterValue
        {
            private short _ValueR = 0;
            private short _ValueG = 0;
            private short _ValueB = 0;
            public short ValueR
            {
                set 
                {
                    if (value < -255) value = -255;
                    else if (value > 255) value = 255;
                    _ValueR = value;
                }
                get { return _ValueR; }
            }
            public short ValueG
            {
                set 
                {
                    if (value < -255) value = -255;
                    else if (value > 255) value = 255;
                    _ValueG = value;
                }
                get { return _ValueG; }
            }
            public short ValueB
            {
                set 
                {
                    if (value < -255) value = -255;
                    else if (value > 255) value = 255;
                    _ValueB = value;
                }
                get { return _ValueB; }
            }
            public ColorFilterValue(short filterR, short filterG, short filterB)
            {
                this.ValueR = filterR;
                this.ValueG = filterG;
                this.ValueB = filterB;
            }
        }

        public FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            ColorFilterValue filter = null;
            foreach (object arg in args)
            {
                if (arg is ColorFilterValue)
                {
                    filter = (ColorFilterValue)arg;
                }
            }
            if (filter == null) return FilterError.MissingArgument;

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, filter);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, filter);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32rgba(bmp, filter);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32prgba(bmp, filter);
                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }

        public FilterError ProcessImage24rgb(DirectAccessBitmap bmp, ColorFilterValue filter)
        {
            if (filter == null) return FilterError.MissingArgument;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            int value;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 3;

                    value = (int)(data[pos2] + filter.ValueB);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2] = (byte)value;
                    value = (int)(data[pos2 + 1] + filter.ValueG);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 1] = (byte)value;
                    value = (int)(data[pos2 + 2] + filter.ValueR);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 2] = (byte)value;
                }
            }
            return FilterError.OK;
        }
        public FilterError ProcessImage32rgb(DirectAccessBitmap bmp, ColorFilterValue filter)
        {
            if (filter == null) return FilterError.MissingArgument;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            int value;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    value = (int)(data[pos2] + filter.ValueB);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2] = (byte)value;
                    value = (int)(data[pos2 + 1] + filter.ValueG);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 1] = (byte)value;
                    value = (int)(data[pos2 + 2] + filter.ValueR);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 2] = (byte)value;
                }
            }
            return FilterError.OK;
        }
        public FilterError ProcessImage32rgba(DirectAccessBitmap bmp, ColorFilterValue filter)
        {
            return ProcessImage32rgb(bmp, filter);
        }
        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, ColorFilterValue filter)
        {
            if (filter == null) return FilterError.MissingArgument;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            float preAlpha;
            int value;
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    preAlpha = (float)data[pos2 + 3];
                    if (preAlpha > 0) preAlpha = preAlpha / 255f;

                    value = (int)(data[pos2] / preAlpha + filter.ValueB);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2] = (byte)(value * preAlpha);
                    value = (int)(data[pos2 + 1] / preAlpha + filter.ValueG);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 1] = (byte)(value * preAlpha);
                    value = (int)(data[pos2 + 2] / preAlpha + filter.ValueR);
                    if (value > 255) value = 255; else if (value < 0) value = 0;
                    data[pos2 + 2] = (byte)(value * preAlpha); 
                }
            }
            return FilterError.OK;
        }
    }
}
