using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace dg.Utilities.Imaging.Filters
{
    public class ExactColorReplace : IImageFilter
    {
        public class SourceColor : ColorArgument
        {
            public SourceColor(short A, short R, short G, short B, bool includeAlpha) :
                base(A, R, G, B, includeAlpha) { }
            public SourceColor(short A, short R, short G, short B)
                : base(A, R, G, B, false)
            {
            }
            public SourceColor(Color color, bool includeAlpha) :
                base(color, includeAlpha) { }
            public SourceColor(Color color) :
                base(color) { }
        }
        public class DestColor : ColorArgument
        {
            public DestColor(short A, short R, short G, short B, bool includeAlpha) :
                base(A, R, G, B, includeAlpha) { }
            public DestColor(short A, short R, short G, short B)
                : base(A, R, G, B, false)
            {
            }
            public DestColor(Color color, bool includeAlpha) :
                base(color, includeAlpha) { }
            public DestColor(Color color) :
                base(color) { }
        }
        public ImageFilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            SourceColor clrSource = null;
            DestColor clrDest = null;
            foreach (object arg in args)
            {
                if (arg is SourceColor) clrSource = arg as SourceColor;
                else if (arg is DestColor) clrDest = arg as DestColor;
            }
            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, clrSource, clrDest);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, clrSource, clrDest);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32argb(bmp, clrSource, clrDest);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32pargb(bmp, clrSource, clrDest);
                default:
                    return ImageFilterError.IncompatiblePixelFormat;
            }
        }
        public ImageFilterError ProcessImage24rgb(DirectAccessBitmap bmp, SourceColor clrSource, DestColor clrDest)
        {
            if (clrSource == null || clrDest == null)
                return ImageFilterError.MissingArgument;
            if (clrSource.Is64Bit || clrDest.Is64Bit)
                return ImageFilterError.InvalidArgument;

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
                    if (clrSource.R == data[pos2 + 2] &&
                        clrSource.G == data[pos2 + 1] &&
                        clrSource.B == data[pos2])
                    {
                        data[pos2 + 2] = (byte)clrDest.R;
                        data[pos2 + 1] = (byte)clrDest.G;
                        data[pos2] = (byte)clrDest.B;
                    }
                }
            }
            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32rgb(DirectAccessBitmap bmp, SourceColor clrSource, DestColor clrDest)
        {
            if (clrSource == null || clrDest == null)
                return ImageFilterError.MissingArgument;
            if (clrSource.Is64Bit || clrDest.Is64Bit)
                return ImageFilterError.InvalidArgument;

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
                    if (clrSource.R == data[pos2 + 2] &&
                        clrSource.G == data[pos2 + 1] &&
                        clrSource.B == data[pos2])
                    {
                        data[pos2 + 2] = (byte)clrDest.R;
                        data[pos2 + 1] = (byte)clrDest.G;
                        data[pos2] = (byte)clrDest.B;
                    }
                }
            }
            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32argb(DirectAccessBitmap bmp, SourceColor clrSource, DestColor clrDest)
        {
            if (clrSource == null || clrDest == null)
                return ImageFilterError.MissingArgument;
            if (clrSource.Is64Bit || clrDest.Is64Bit)
                return ImageFilterError.InvalidArgument;

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
                    if ((!clrSource.IncludeAlpha || clrSource.A == data[pos2 + 3]) &&
                        clrSource.R == data[pos2 + 2] &&
                        clrSource.G == data[pos2 + 1] &&
                        clrSource.B == data[pos2])
                    {
                        if (clrDest.IncludeAlpha) data[pos2 + 3] = (byte)clrDest.A;
                        data[pos2 + 2] = (byte)clrDest.R;
                        data[pos2 + 1] = (byte)clrDest.G;
                        data[pos2] = (byte)clrDest.B;
                    }
                }
            }
            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32pargb(DirectAccessBitmap bmp, SourceColor clrSource, DestColor clrDest)
        {
            if (clrSource == null || clrDest == null)
                return ImageFilterError.MissingArgument;
            if (clrSource.Is64Bit || clrDest.Is64Bit)
                return ImageFilterError.InvalidArgument;

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
                    if ((!clrSource.IncludeAlpha || clrSource.A == data[pos2 + 3]) &&
                        clrSource.R == (data[pos2 + 2] / preAlpha) &&
                        clrSource.G == (data[pos2 + 1] / preAlpha) &&
                        clrSource.B == (data[pos2] / preAlpha))
                    {
                        if (clrDest.IncludeAlpha) data[pos2 + 3] = (byte)clrDest.A;
                        data[pos2 + 2] = (byte)(clrDest.R * preAlpha);
                        data[pos2 + 1] = (byte)(clrDest.G * preAlpha);
                        data[pos2] = (byte)(clrDest.B * preAlpha);
                    }
                }
            }
            return ImageFilterError.OK;
        }
    }
}
