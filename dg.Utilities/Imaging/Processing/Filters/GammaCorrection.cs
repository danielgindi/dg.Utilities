using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;

namespace dg.Utilities.Imaging.Processing.Filters
{
    public class GammaCorrection : IImageFilter
    {
        public class GammaValue
        {
            private float _ValueR = 2.5f;
            private float _ValueG = 2.5f;
            private float _ValueB = 2.5f;
            public float ValueR
            {
                set 
                {
                    if (value < 0.2f) value = 0.2f;
                    else if (value > 5f) value = 5f;
                    _ValueR = value;
                }
                get { return _ValueR; }
            }
            public float ValueG
            {
                set 
                {
                    if (value < 0.2f) value = 0.2f;
                    else if (value > 5f) value = 5f;
                    _ValueG = value;
                }
                get { return _ValueG; }
            }
            public float ValueB
            {
                set 
                {
                    if (value < 0.2f) value = 0.2f;
                    else if (value > 5f) value = 5f;
                    _ValueB = value;
                }
                get { return _ValueB; }
            }
            public GammaValue(float gammaR, float gammaG, float gammaB)
            {
                this.ValueR = gammaR;
                this.ValueG = gammaG;
                this.ValueB = gammaB;
            }
        }

        public FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            GammaValue gamma = null;
            foreach (object arg in args)
            {
                if (arg is GammaValue)
                {
                    gamma = (GammaValue)arg;
                }
            }
            if (gamma == null) return FilterError.MissingArgument;

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return ProcessImage24rgb(bmp, gamma);
                case PixelFormat.Format32bppRgb:
                    return ProcessImage32rgb(bmp, gamma);
                case PixelFormat.Format32bppArgb:
                    return ProcessImage32rgba(bmp, gamma);
                case PixelFormat.Format32bppPArgb:
                    return ProcessImage32prgba(bmp, gamma);
                default:
                    return FilterError.IncompatiblePixelFormat;
            }
        }
        public static void BuildGammaArray(float gamma, out byte[] arrGamma)
        {
            if (gamma < 0.2f) gamma = 0.2f;
            else if (gamma > 5f) gamma = 5f;

            arrGamma = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                arrGamma[i] = (byte)Math.Min(255, (int)((255.0
                    * Math.Pow(i / 255.0, 1.0 / gamma)) + 0.5));
            }
        }

        public FilterError ProcessImage24rgb(DirectAccessBitmap bmp, GammaValue gamma)
        {
            if (gamma == null) return FilterError.MissingArgument;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;

            byte[] arrRed, arrGreen, arrBlue;
            BuildGammaArray(gamma.ValueR, out arrRed);
            BuildGammaArray(gamma.ValueG, out arrGreen);
            BuildGammaArray(gamma.ValueB, out arrBlue);
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 3;
                    data[pos2] = arrBlue[data[pos2]];
                    data[pos2 + 1] = arrGreen[data[pos2 + 1]];
                    data[pos2 + 2] = arrRed[data[pos2 + 2]];
                }
            }
            return FilterError.OK;
        }
        public FilterError ProcessImage32rgb(DirectAccessBitmap bmp, GammaValue gamma)
        {
            if (gamma == null) return FilterError.MissingArgument;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;

            byte[] arrRed, arrGreen, arrBlue;
            BuildGammaArray(gamma.ValueR, out arrRed);
            BuildGammaArray(gamma.ValueG, out arrGreen);
            BuildGammaArray(gamma.ValueB, out arrBlue);
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    data[pos2] = arrBlue[data[pos2]];
                    data[pos2 + 1] = arrGreen[data[pos2 + 1]];
                    data[pos2 + 2] = arrRed[data[pos2 + 2]];
                }
            }
            return FilterError.OK;
        }
        public FilterError ProcessImage32rgba(DirectAccessBitmap bmp, GammaValue gamma)
        {
            return ProcessImage32rgb(bmp, gamma);
        }
        public FilterError ProcessImage32prgba(DirectAccessBitmap bmp, GammaValue gamma)
        {
            if (gamma == null) return FilterError.MissingArgument;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            float preAlpha;

            byte[] arrRed, arrGreen, arrBlue;
            BuildGammaArray(gamma.ValueR, out arrRed);
            BuildGammaArray(gamma.ValueG, out arrGreen);
            BuildGammaArray(gamma.ValueB, out arrBlue);
            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    preAlpha = (float)data[pos2 + 3];
                    if (preAlpha > 0) preAlpha = preAlpha / 255f;

                    data[pos2] = (byte)(arrBlue[(byte)(data[pos2] / preAlpha)] * preAlpha);
                    data[pos2 + 1] = (byte)(arrGreen[(byte)(data[pos2 + 1] / preAlpha)] * preAlpha);
                    data[pos2 + 2] = (byte)(arrRed[(byte)(data[pos2 + 2] / preAlpha)] * preAlpha);
                }
            }
            return FilterError.OK;
        }
    }
}
