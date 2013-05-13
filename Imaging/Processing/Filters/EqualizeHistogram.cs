using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;

namespace dg.Utilities.Imaging.Filters
{
    /// <summary>
    /// This will equalize the histogram for an image.
    /// It will never work on the Alpha channel.
    /// 
    /// Optional arguments:
    /// Channel: sets the channels to be equalized. Default is RGB
    /// HistogramParam: passes ready histogram data, to prevent extra calculations
    /// GrayMultiplier: for gray channel only, type of luminosity multipliers
    /// </summary>
    public class EqualizeHistogram : IImageFilter
    {
        /************************************************************************/
        /*                                                                      */
        /* A little theory:                                                     */
        /* ----------------                                                     */
        /*                                                                      */
        /* Probability of a pixel value:                                        */
        /*   p(i) = n(i) / n = Histogram of i / Number of Pixels = [0..1]       */
        /*                                                                      */
        /* Cumulative distribution function:                                    */
        /*           i                                                          */
        /*  cdf(i) = Σ   p(j)                                                   */
        /*           j=0                                                        */
        /*                                                                      */
        /* Normalize to number of pixels:                                       */
        /*  cdf(i) = cdf(i) * number of pixels                                  */
        /*                                                                      */
        /* Compute cdf for all pixels in the image, and then normalize to       */
        /*  the number of color levels [0..255]:                                */
        /*                                                                      */
        /* h(v) = round( ( ( cdf(v) - cdfmin ) / ( (MxN) - cdfmin ) ) * (L-1) ) */
        /*                                                                      */
        /* h: resulting color level                                             */
        /* v: current color level value                                         */
        /* cdf: normalized table of cdf for all pixels in the image             */
        /* cdfmin: minimum value in the cdf table                               */
        /* M: width of image in pixels                                          */
        /* N: height of image in pixels                                         */
        /* L: number of color levels in the image (usually 256)                 */
        /*                                                                      */
        /************************************************************************/

        public class HistogramParam
        {
            public Channel Channel;
            public int[] Histogram;

            public HistogramParam(Channel channel, int[] histogram)
            {
                Channel = channel;
                Histogram = histogram;
            }
        }

        public ImageFilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            Channel channels = Channel.None;
            GrayMultiplier grayMultiplier = GrayMultiplier.None;
            int[] histogramR = null;
            int[] histogramG = null;
            int[] histogramB = null;
            int[] histogramGrey = null;
            foreach (object arg in args)
            {
                if (arg is HistogramParam)
                {
                    switch (((HistogramParam)arg).Channel)
                    {
                        case Channel.Red:
                            histogramR = ((HistogramParam)arg).Histogram;
                            if (histogramR != null) channels |= Channel.Red;
                            break;
                        case Channel.Green:
                            histogramG = ((HistogramParam)arg).Histogram;
                            if (histogramG != null) channels |= Channel.Green;
                            break;
                        case Channel.Blue:
                            histogramB = ((HistogramParam)arg).Histogram;
                            if (histogramB != null) channels |= Channel.Blue;
                            break;
                        case Channel.Gray:
                            histogramGrey = ((HistogramParam)arg).Histogram;
                            if (histogramGrey != null) channels = Channel.Gray;
                            break;
                    }
                }
                else if (arg is Channel)
                {
                    channels |= (Channel)arg;
                }
                else if (arg is GrayMultiplier)
                {
                    grayMultiplier |= (GrayMultiplier)arg;
                }
            }

            switch (bmp.Bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    if (channels == Channel.Gray) return ProcessImage24rgb(bmp, histogramGrey, grayMultiplier);
                    else return ProcessImage24rgb(bmp, channels, histogramR, histogramG, histogramB);
                case PixelFormat.Format32bppRgb:
                    if (channels == Channel.Gray) return ProcessImage32rgb(bmp, histogramGrey, grayMultiplier);
                    else return ProcessImage32rgb(bmp, channels, histogramR, histogramG, histogramB);
                case PixelFormat.Format32bppArgb:
                    if (channels == Channel.Gray) return ProcessImage32argb(bmp, histogramGrey, grayMultiplier);
                    else return ProcessImage32argb(bmp, channels, histogramR, histogramG, histogramB);
                case PixelFormat.Format32bppPArgb:
                    if (channels == Channel.Gray) return ProcessImage32pargb(bmp, histogramGrey, grayMultiplier);
                    else return ProcessImage32pargb(bmp, channels, histogramR, histogramG, histogramB);
                default:
                    return ImageFilterError.IncompatiblePixelFormat;
            }
        }

        private ImageFilterError CalculateHistogramsIfNeeded(DirectAccessBitmap dab, Channel channels, ref int[] R, ref int[] G, ref int [] B)
        {
            Channel channelsToRetrieve = Channel.None;
            if ((channels & Channel.Red) == Channel.Red && R == null)
                channelsToRetrieve |= Channel.Red;
            if ((channels & Channel.Green) == Channel.Green && G == null)
                channelsToRetrieve |= Channel.Green;
            if ((channels & Channel.Blue) == Channel.Blue && B == null)
                channelsToRetrieve |= Channel.Blue;
            if (channelsToRetrieve != Channel.None)
            {
                ImageFilterError err;
                if (channelsToRetrieve == Channel.Red)
                {
                    err = ImagingUtility.CalculateHistogram(dab, Channel.Red, out R);
                }
                else if (channelsToRetrieve == Channel.Green)
                {
                    err = ImagingUtility.CalculateHistogram(dab, Channel.Green, out G);
                }
                else if (channelsToRetrieve == Channel.Blue)
                {
                    err = ImagingUtility.CalculateHistogram(dab, Channel.Blue, out B);
                }
                else
                {
                    err = ImagingUtility.CalculateHistogram(dab, out R, out G, out B);
                }
                if (err != ImageFilterError.OK) return err;
            }
            return ImageFilterError.OK;
        }
        private double[] CalculateCDF(int[] histogram, int numberOfPixels)
        {
            double[] cdf = new double[histogram.Length];
            double sigma = 0d;
            double numOfPx = (double)numberOfPixels;

            for (int i = 0, l = histogram.Length; i < l; i++)
            {
                if (histogram[i] > 0)
                {
                    sigma += histogram[i] / numOfPx;
                    cdf[i] = sigma;
                }
            }

            return cdf;
        }
        private int[] NormalizeCDF(double[] cdf, int numberOfPixels)
        {
            int[] ncdf = new int[cdf.Length];

            for (int i = 0, l = cdf.Length; i < l; i++)
            {
                ncdf[i] = (int)(cdf[i] * (double)numberOfPixels);
            }

            return ncdf;
        }
        private int FindLowestExcludingZero(int[] cdf)
        {
            int lowest = int.MaxValue;
            int cur;

            for (int i = 0, l = cdf.Length; i < l; i++)
            {
                cur = cdf[i];
                if (cur < lowest && cur != 0) lowest = cur;
            }

            if (lowest == int.MaxValue) lowest = 0;
            return lowest;
        }

        public ImageFilterError ProcessImage24rgb(DirectAccessBitmap bmp, Channel channels, int[] histR, int[] histG, int[] histB)
        {
            if (channels == Channel.None) channels = Channel.RGB;
            ImageFilterError err = CalculateHistogramsIfNeeded(bmp, channels, ref histR, ref histG, ref histB);
            if (err != ImageFilterError.OK) return err;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;

            int numberOfPixels = cx * cy;
            int[] cdfR = null, cdfG = null, cdfB = null;
            int cdfminR = 0, cdfminG = 0, cdfminB = 0;
            if (histR != null)
            {
                cdfR = NormalizeCDF(CalculateCDF(histR, numberOfPixels), numberOfPixels);
                cdfminR = FindLowestExcludingZero(cdfR);
            }
            if (histG != null)
            {
                cdfG = NormalizeCDF(CalculateCDF(histG, numberOfPixels), numberOfPixels);
                cdfminG = FindLowestExcludingZero(cdfG);
            }
            if (histB != null)
            {
                cdfB = NormalizeCDF(CalculateCDF(histB, numberOfPixels), numberOfPixels);
                cdfminB = FindLowestExcludingZero(cdfB);
            }
            double numberOfPixelsR = numberOfPixels - cdfminR;
            double numberOfPixelsG = numberOfPixels - cdfminG;
            double numberOfPixelsB = numberOfPixels - cdfminB;
            int curPos;

            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 3;
                    if (histR != null)
                    {
                        curPos = pos2 + 2;
                        data[curPos] =
                            (byte)Math.Round(
                            (double)(((cdfR[data[curPos]] - cdfminR) / numberOfPixelsR) * 255));
                    }
                    if (histG != null)
                    {
                        curPos = pos2 + 1;
                        data[curPos] =
                            (byte)Math.Round(
                            (double)(((cdfG[data[curPos]] - cdfminG) / numberOfPixelsG) * 255));
                    }
                    if (histB != null)
                    {
                        curPos = pos2;
                        data[curPos] =
                            (byte)Math.Round(
                            (double)(((cdfB[data[curPos]] - cdfminB) / numberOfPixelsB) * 255));
                    }
                }
            }

            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32rgb(DirectAccessBitmap bmp, Channel channels, int[] histR, int[] histG, int[] histB)
        {
            if (channels == Channel.None) channels = Channel.RGB;
            ImageFilterError err = CalculateHistogramsIfNeeded(bmp, channels, ref histR, ref histG, ref histB);
            if (err != ImageFilterError.OK) return err;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;

            int numberOfPixels = cx*cy;
            int[] cdfR = null, cdfG = null, cdfB = null;
            int cdfminR = 0, cdfminG = 0, cdfminB = 0;
            if (histR != null)
            {
                cdfR = NormalizeCDF(CalculateCDF(histR, numberOfPixels), numberOfPixels);
                cdfminR = FindLowestExcludingZero(cdfR);
                if (cdfminR == 0) cdfR = null; // Do not work on this channel
            }
            if (histG != null)
            {
                cdfG = NormalizeCDF(CalculateCDF(histG, numberOfPixels), numberOfPixels);
                cdfminG = FindLowestExcludingZero(cdfG);
                if (cdfminG == 0) cdfG = null; // Do not work on this channel
            }
            if (histB != null)
            {
                cdfB = NormalizeCDF(CalculateCDF(histB, numberOfPixels), numberOfPixels);
                cdfminB = FindLowestExcludingZero(cdfB);
                if (cdfminB == 0) cdfB = null; // Do not work on this channel
            }
            double numberOfPixelsR = numberOfPixels - cdfminR;
            double numberOfPixelsG = numberOfPixels - cdfminG;
            double numberOfPixelsB = numberOfPixels - cdfminB;
            int curPos;

            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    if (histR != null)
                    {
                        curPos = pos2 + 2;
                        data[curPos] =
                            (byte)Math.Round(
                            (double)(((cdfR[data[curPos]] - cdfminR) / numberOfPixelsR) * 255));
                    }
                    if (histG != null)
                    {
                        curPos = pos2 + 1;
                        data[curPos] =
                            (byte)Math.Round(
                            (double)(((cdfG[data[curPos]] - cdfminG) / numberOfPixelsG) * 255));
                    }
                    if (histB != null)
                    {
                        curPos = pos2;
                        data[curPos] =
                            (byte)Math.Round(
                            (double)(((cdfB[data[curPos]] - cdfminB) / numberOfPixelsB) * 255));
                    }
                }
            }

            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32argb(DirectAccessBitmap bmp, Channel channels, int[] histR, int[] histG, int[] histB)
        {
            return ProcessImage32rgb(bmp, channels, histR, histG, histB);
        }
        public ImageFilterError ProcessImage32pargb(DirectAccessBitmap bmp, Channel channels, int[] histR, int[] histG, int[] histB)
        {
            if (channels == Channel.None) channels = Channel.RGB;
            ImageFilterError err = CalculateHistogramsIfNeeded(bmp, channels, ref histR, ref histG, ref histB);
            if (err != ImageFilterError.OK) return err;

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            float preAlpha;

            int numberOfPixels = cx * cy;
            int[] cdfR = null, cdfG = null, cdfB = null;
            int cdfminR = 0, cdfminG = 0, cdfminB = 0;
            if (histR != null)
            {
                cdfR = NormalizeCDF(CalculateCDF(histR, numberOfPixels), numberOfPixels);
                cdfminR = FindLowestExcludingZero(cdfR);
            }
            if (histG != null)
            {
                cdfG = NormalizeCDF(CalculateCDF(histG, numberOfPixels), numberOfPixels);
                cdfminG = FindLowestExcludingZero(cdfG);
            }
            if (histB != null)
            {
                cdfB = NormalizeCDF(CalculateCDF(histB, numberOfPixels), numberOfPixels);
                cdfminB = FindLowestExcludingZero(cdfB);
            }
            double numberOfPixelsR = numberOfPixels - cdfminR;
            double numberOfPixelsG = numberOfPixels - cdfminG;
            double numberOfPixelsB = numberOfPixels - cdfminB;
            int curPos;

            for (y = bmp.StartY; y < endY; y++)
            {
                pos1 = stride * y;
                for (x = bmp.StartX; x < endX; x++)
                {
                    pos2 = pos1 + x * 4;
                    if (histR != null)
                    {
                        preAlpha = (float)data[pos2 + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        curPos = pos2 + 2;
                        data[curPos] =
                            (byte)(Math.Round(
                            (double)(((cdfR[(byte)(data[curPos] / preAlpha)] - cdfminR) / numberOfPixelsR) * 255))
                            * preAlpha);
                    }
                    if (histG != null)
                    {
                        preAlpha = (float)data[pos2 + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        curPos = pos2 + 1;
                        data[curPos] =
                            (byte)(Math.Round(
                            (double)(((cdfG[(byte)(data[curPos] / preAlpha)] - cdfminG) / numberOfPixelsG) * 255))
                            * preAlpha);
                    }
                    if (histB != null)
                    {
                        preAlpha = (float)data[pos2 + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        curPos = pos2;
                        data[curPos] =
                            (byte)(Math.Round(
                            (double)(((cdfB[(byte)(data[curPos] / preAlpha)] - cdfminB) / numberOfPixelsB) * 255))
                            * preAlpha);
                    }
                }
            }

            return ImageFilterError.OK;
        }

        public ImageFilterError ProcessImage24rgb(DirectAccessBitmap bmp, int[] histGrey, GrayMultiplier grayMultiplier)
        {
            if (histGrey == null)
            {
                ImageFilterError err = ImagingUtility.CalculateHistogram(bmp, Channel.Gray, out histGrey, grayMultiplier);
                if (err != ImageFilterError.OK) return err;
            }

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;

            int numberOfPixels = cx * cy;
            int[] cdf;
            int cdfmin = 0;
            cdf = NormalizeCDF(CalculateCDF(histGrey, numberOfPixels), numberOfPixels);
            cdfmin = FindLowestExcludingZero(cdf);

            double numberOfPixels2 = numberOfPixels - cdfmin;
            int value;

            if (grayMultiplier == GrayMultiplier.None)
            {
                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 3;
                        value = data[pos2 + 2] + data[pos2 + 1] + data[pos2];
                        value = (byte)(value / 3);
                        data[pos2 + 2] = data[pos2 + 1] = data[pos2] =
                            (byte)Math.Round(
                            (double)(((cdf[value] - cdfmin) / numberOfPixels2) * 255));
                    }
                }
            }
            else
            {
                double lumR, lumG, lumB;

                if (grayMultiplier == GrayMultiplier.NaturalNTSC)
                {
                    lumR = ImagingUtility.RedLuminosityNTSC;
                    lumG = ImagingUtility.GreenLuminosityNTSC;
                    lumB = ImagingUtility.BlueLuminosityNTSC;
                }
                else
                {
                    lumR = ImagingUtility.RedLuminosity;
                    lumG = ImagingUtility.GreenLuminosity;
                    lumB = ImagingUtility.BlueLuminosity;
                }

                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 3;
                        value = (byte)(data[pos2 + 2] * lumR) + (byte)(data[pos2 + 1] * lumG) + (byte)(data[pos2] * lumB);
                        data[pos2 + 2] = data[pos2 + 1] = data[pos2] =
                            (byte)Math.Round(
                            (double)(((cdf[value] - cdfmin) / numberOfPixels2) * 255));
                    }
                }
            }

            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32rgb(DirectAccessBitmap bmp, int[] histGrey, GrayMultiplier grayMultiplier)
        {
            if (histGrey == null)
            {
                ImageFilterError err = ImagingUtility.CalculateHistogram(bmp, Channel.Gray, out histGrey, grayMultiplier);
                if (err != ImageFilterError.OK) return err;
            }

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;

            int numberOfPixels = cx * cy;
            int[] cdf;
            int cdfmin = 0;
            cdf = NormalizeCDF(CalculateCDF(histGrey, numberOfPixels), numberOfPixels);
            cdfmin = FindLowestExcludingZero(cdf);
            if (cdfmin == 0) return ImageFilterError.OK; // No cdf...

            double numberOfPixels2 = numberOfPixels - cdfmin;
            int value;

            if (grayMultiplier == GrayMultiplier.None)
            {
                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 4;
                        value = data[pos2 + 2] + data[pos2 + 1] + data[pos2];
                        value = (byte)(value / 3);
                        data[pos2 + 2] = data[pos2 + 1] = data[pos2] =
                            (byte)Math.Round(
                            (double)(((cdf[value] - cdfmin) / numberOfPixels2) * 255));
                    }
                }
            }
            else
            {
                double lumR, lumG, lumB;

                if (grayMultiplier == GrayMultiplier.NaturalNTSC)
                {
                    lumR = ImagingUtility.RedLuminosityNTSC;
                    lumG = ImagingUtility.GreenLuminosityNTSC;
                    lumB = ImagingUtility.BlueLuminosityNTSC;
                }
                else
                {
                    lumR = ImagingUtility.RedLuminosity;
                    lumG = ImagingUtility.GreenLuminosity;
                    lumB = ImagingUtility.BlueLuminosity;
                }

                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 4;
                        value = (byte)(data[pos2 + 2] * lumR) + (byte)(data[pos2 + 1] * lumG) + (byte)(data[pos2] * lumB);
                        data[pos2 + 2] = data[pos2 + 1] = data[pos2] =
                            (byte)Math.Round(
                            (double)(((cdf[value] - cdfmin) / numberOfPixels2) * 255));
                    }
                }
            }

            return ImageFilterError.OK;
        }
        public ImageFilterError ProcessImage32argb(DirectAccessBitmap bmp, int[] histGrey, GrayMultiplier grayMultiplier)
        {
            return ProcessImage32rgb(bmp, histGrey, grayMultiplier);
        }
        public ImageFilterError ProcessImage32pargb(DirectAccessBitmap bmp, int[] histGrey, GrayMultiplier grayMultiplier)
        {
            if (histGrey == null)
            {
                ImageFilterError err = ImagingUtility.CalculateHistogram(bmp, Channel.Gray, out histGrey, grayMultiplier);
                if (err != ImageFilterError.OK) return err;
            }

            int cx = bmp.Width;
            int cy = bmp.Height;
            int endX = cx + bmp.StartX;
            int endY = cy + bmp.StartY;
            byte[] data = bmp.Bits;
            int stride = bmp.Stride;
            int pos1, pos2;
            int x, y;
            float preAlpha;

            int numberOfPixels = cx * cy;
            int[] cdf;
            int cdfmin = 0;
            cdf = NormalizeCDF(CalculateCDF(histGrey, numberOfPixels), numberOfPixels);
            cdfmin = FindLowestExcludingZero(cdf);

            double numberOfPixels2 = numberOfPixels - cdfmin;
            int value;

            if (grayMultiplier == GrayMultiplier.None)
            {
                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 4;
                        preAlpha = (float)data[pos2 + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        value = (byte)(data[pos2 + 2] / preAlpha);
                        value += (byte)(data[pos2 + 1] / preAlpha);
                        value += (byte)(data[pos2] / preAlpha);
                        value = (byte)(value / 3);
                        data[pos2 + 2] = data[pos2 + 1] = data[pos2] =
                            (byte)(Math.Round(
                            (double)(((cdf[value] - cdfmin) / numberOfPixels2) * 255))
                            * preAlpha);
                    }
                }
            }
            else
            {
                double lumR, lumG, lumB;

                if (grayMultiplier == GrayMultiplier.NaturalNTSC)
                {
                    lumR = ImagingUtility.RedLuminosityNTSC;
                    lumG = ImagingUtility.GreenLuminosityNTSC;
                    lumB = ImagingUtility.BlueLuminosityNTSC;
                }
                else
                {
                    lumR = ImagingUtility.RedLuminosity;
                    lumG = ImagingUtility.GreenLuminosity;
                    lumB = ImagingUtility.BlueLuminosity;
                }

                for (y = bmp.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = bmp.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * 4;
                        preAlpha = (float)data[pos2 + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        value = (int)Math.Round((byte)(data[pos2 + 2] / preAlpha) * lumR + (byte)(data[pos2 + 1] / preAlpha) * lumG + (byte)(data[pos2] / preAlpha) * lumB);
                        data[pos2 + 2] = data[pos2 + 1] = data[pos2] =
                            (byte)(Math.Round(
                            (double)(((cdf[value] - cdfmin) / numberOfPixels2) * 255))
                            * preAlpha);
                    }
                }
            }

            return ImageFilterError.OK;
        }
    }
}
