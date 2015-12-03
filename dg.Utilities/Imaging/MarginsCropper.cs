using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using dg.Utilities.Imaging.Quantizers.Helpers;
using dg.Utilities.Imaging.Quantizers.XiaolinWu;

namespace dg.Utilities.Imaging
{
    public static class MarginsCropper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="color"></param>
        /// <returns>Image after cropping, or null if there's no work to do.
        /// If result is zero width or zero height, returns null
        /// </returns>
        public static Image CropMargins(Bitmap image, Color color, double colorDistanceAllowed)
        {
            if (image == null) return null;
            if (color == null || color == System.Drawing.Color.Empty) color = image.GetPixel(0, 0);

            colorDistanceAllowed = Math.Abs(colorDistanceAllowed);
            bool bCheckDistance = colorDistanceAllowed != 0;

            bool bGoOn;
            int iLinesTop = 0, iLinesBottom = 0;
            int iLinesLeft = 0, iLinesRight = 0;

            bGoOn = true;
            for (int y = 0, x; y < image.Height; y++)
            {
                for (x = 0; x < image.Width; x++)
                {
                    if (image.GetPixel(x, y) != color &&
                        (!bCheckDistance ||
                            GetColorDistance(image.GetPixel(x, y), color) > colorDistanceAllowed)
                        )
                    {
                        bGoOn = false; 
                        break;
                    }
                }
                if (bGoOn)
                {
                    iLinesTop++;
                }
                else break;
            }

            bGoOn = true;
            for (int y = image.Height - 1, x; y >= 0; y--)
            {
                for (x = 0; x < image.Width; x++)
                {
                    if (image.GetPixel(x, y) != color &&
                        (!bCheckDistance ||
                            GetColorDistance(image.GetPixel(x, y), color) > colorDistanceAllowed)
                        ) 
                    { 
                        bGoOn = false; 
                        break;
                    }
                }
                if (bGoOn) 
                {
                    iLinesBottom++;
                } 
                else break;
            }

            bGoOn = true;
            for (int x = 0, y; x < image.Width; x++)
            {
                for (y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y) != color &&
                        (!bCheckDistance ||
                            GetColorDistance(image.GetPixel(x, y), color) > colorDistanceAllowed)
                        )
                    {
                        bGoOn = false; 
                        break; 
                    }
                }
                if (bGoOn)
                {
                    iLinesLeft++;
                }
                else break;
            }

            bGoOn = true;
            for (int x = image.Width - 1, y; x >= 0; x--)
            {
                for (y = 0; y < image.Height; y++)
                {
                    if (image.GetPixel(x, y) != color &&
                        (!bCheckDistance ||
                            GetColorDistance(image.GetPixel(x, y), color) > colorDistanceAllowed)
                        )
                    { 
                        bGoOn = false;
                        break;
                    }
                }
                if (bGoOn)
                {
                    iLinesRight++;
                }
                else break;
            }

            int iNewWidth = image.Width - iLinesLeft - iLinesRight;
            int iNewHeight = image.Height - iLinesTop - iLinesBottom;

            if (iNewWidth <= 0 || iNewHeight <= 0 || (iLinesLeft == 0 &&
                iLinesRight == 0 && iLinesTop == 0 && iLinesBottom == 0))
            {
                return null;
            }

            Image retImg = new Bitmap(iNewWidth, iNewHeight);
            using (Graphics g = Graphics.FromImage(retImg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.DrawImage(image, -iLinesLeft, -iLinesTop, image.Width, image.Height);
            }

            return retImg;
        }

        public static Boolean CropMarginsAndSave(String imgPath, String destPath, Color colorCrop, double colorDistanceAllowed)
        {
            if (imgPath == null) return false;
            if (destPath == null) destPath = imgPath;

            try
            {
                using (Image imgSrc = Image.FromFile(imgPath))
                {
                    ImageFormat destFormat = imgSrc.RawFormat;
                    Image destImg;
                    using (Bitmap srcBmp = new Bitmap(imgSrc))
                    {
                        destImg = CropMargins(srcBmp, colorCrop, colorDistanceAllowed);
                    }
                    imgSrc.Dispose();

                    if (destImg != null)
                    {
                        if (destFormat.Equals(ImageFormat.Jpeg))
                        {
                            System.Drawing.Imaging.ImageCodecInfo encoder = null;
                            System.Drawing.Imaging.ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
                            using (EncoderParameters encoderParameters = new EncoderParameters(1))
                            {
                                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                                foreach (System.Drawing.Imaging.ImageCodecInfo item in encoders)
                                {
                                    if (item.MimeType == @"image/jpeg")
                                    {
                                        encoder = item;
                                        break;
                                    }
                                }

                                if (encoder != null) destImg.Save(destPath, encoder, encoderParameters);
                                else destImg.Save(destPath, destFormat);
                            }
                        }
                        else if (destFormat.Equals(ImageFormat.Gif))
                        {
                            WuColorQuantizer quantizer = new WuColorQuantizer();
                            using (Image quantized = ImageBuffer.QuantizeImage(destImg, quantizer, 256, 4))
                            {
                                quantized.Save(destPath, ImageFormat.Gif);
                            }
                        }
                        else
                        {
                            destImg.Save(destPath, destFormat);
                        }
                        destImg.Dispose();
                    }
                    else
                    {
                        if (!imgPath.Equals(destPath))
                        {
                            try
                            {
                                System.IO.File.Copy(imgPath, destPath, true);
                            }
                            catch (System.IO.IOException)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static double GetColorDistance(Color c1, Color c2)
        {
            double a = Math.Pow(Convert.ToDouble(c1.A - c2.A), 2.0);
            double r = Math.Pow(Convert.ToDouble(c1.R - c2.R), 2.0);
            double g = Math.Pow(Convert.ToDouble(c1.G - c2.G), 2.0);
            double b = Math.Pow(Convert.ToDouble(c1.B - c2.B), 2.0);

            return Math.Sqrt(a + r + g + b);
        }
    }
}
