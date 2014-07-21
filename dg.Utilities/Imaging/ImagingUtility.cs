using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using dg.Utilities.Imaging.Encoding;
using dg.Utilities.Imaging.Quantizers.Helpers;
using dg.Utilities.Imaging.Quantizers.XiaolinWu;

/// <summary>
/// ImagesManager class
/// Written By Daniel Cohen Gindi (danielgindi@gmail.com)
/// Updated on 2009-05-07
/// </summary>
namespace dg.Utilities.Imaging
{
    public static partial class ImagingUtility
    {
        public const float RedLuminosityNTSC = 0.299f;
        public const float GreenLuminosityNTSC = 0.587f;
        public const float BlueLuminosityNTSC = 0.114f;
        public const float RedLuminosity = 0.3086f;
        public const float GreenLuminosity = 0.6094f;
        public const float BlueLuminosity = 0.0820f;

        public static int ComputeStride(int width, int bitsPerPixel)
        {
            int stride = width * (bitsPerPixel / 8);
            int padding = (stride % 4);
            stride += (padding == 0) ? 0 : (4 - padding);
            return stride;
        }

        public static void RgbToHsv(int red, int green, int blue, out double h, out double s, out double v)
        {
            int maxRgb = red > green ? red : green; maxRgb = maxRgb > blue ? maxRgb : blue;
            v = maxRgb;
            if (v == 0) { h = s = 0; return; }
            red = (int)((double)red / v);
            green = (int)((double)green / v);
            blue = (int)((double)blue / v);
            maxRgb = red > green ? red : green; maxRgb = maxRgb > blue ? maxRgb : blue;
            int minRgb = red < green ? red : green; minRgb = minRgb < blue ? minRgb : blue;
            s = maxRgb - minRgb;
            if (s == 0) { h = 0; return; }
            red = (red - minRgb) / (maxRgb - minRgb);
            green = (green - minRgb) / (maxRgb - minRgb);
            blue = (blue - minRgb) / (maxRgb - minRgb);
            maxRgb = red > green ? red : green; maxRgb = maxRgb > blue ? maxRgb : blue;
            minRgb = red < green ? red : green; minRgb = minRgb < blue ? minRgb : blue;
            if (maxRgb == red)
            {
                h = 0.0d + 60.0d * (green - blue);
                if (h < 0.0)
                {
                    h += 360.0d;
                }
            }
            else if (maxRgb == green) h = 120.0d + 60.0d * (blue - red);
            else h = 240.0d + 60.0d * (red - green);
        }
        public static void HsvToRgb(double h, double s, double v, out int red, out int green, out int blue)
        {
            if (s == 0)
            {
                red = green = blue = (int)v;
                return;
            }

            h /= 60.0f;
            int i = (int)(h);
            double f = h - i;
            double p = v * (1 - s);
            double q = v * (1 - s * f);
            double t = v * (1 - s * (1 - f));
            switch (i)
            {
                case 0: red = (int)v; green = (int)t; blue = (int)p; return;
                case 1: red = (int)q; green = (int)v; blue = (int)p; return;
                case 2: red = (int)p; green = (int)v; blue = (int)t; return;
                case 3: red = (int)p; green = (int)q; blue = (int)v; return;
                case 4: red = (int)t; green = (int)p; blue = (int)v; return;
                default: red = (int)v; green = (int)p; blue = (int)q; return;
            }
        }


        public static ImageFilterError CalculateHistogram(DirectAccessBitmap dab, Channel channel, out int[] values)
        {
            return CalculateHistogram(dab, channel, out values, GrayMultiplier.None);
        }
        public static ImageFilterError CalculateHistogram(DirectAccessBitmap dab, Channel channel, out int[] values, GrayMultiplier grayMultiplier)
        {
            values = null;
            if (dab == null) return ImageFilterError.InvalidArgument;
            if (dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppRgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            {
                return ImageFilterError.IncompatiblePixelFormat;
            }

            values = new int[256];

            if (channel == Channel.Alpha &&
                (dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ||
                dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                values[255] = dab.Width * dab.Height;
                return ImageFilterError.OK;
            }

            int cx = dab.Width;
            int cy = dab.Height;
            int endX = cx + dab.StartX;
            int endY = cy + dab.StartY;
            int pixelBytes = dab.PixelFormatSize / 8;
            int endXb = endX * pixelBytes;
            byte[] data = dab.Bits;
            int stride = dab.Stride;
            int pos1, pos2;
            int x, y;

            if (channel == Channel.Gray)
            {
                if (grayMultiplier == GrayMultiplier.None)
                {
                    int value;
                    if (dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                    {
                        float preAlpha;
                        for (y = dab.StartY; y < endY; y++)
                        {
                            pos1 = stride * y;
                            for (x = dab.StartX; x < endX; x++)
                            {
                                pos2 = pos1 + x * pixelBytes;
                                preAlpha = (float)data[pos2 + 3];
                                if (preAlpha > 0) preAlpha = preAlpha / 255f;
                                value = (byte)(data[pos2 + 2] / preAlpha);
                                value += (byte)(data[pos2 + 1] / preAlpha);
                                value += (byte)(data[pos2] / preAlpha);
                                values[(byte)Math.Round(value / 3d)]++;
                            }
                        }
                    }
                    else
                    {
                        for (y = dab.StartY; y < endY; y++)
                        {
                            pos1 = stride * y;
                            for (x = dab.StartX; x < endX; x++)
                            {
                                pos2 = pos1 + x * pixelBytes;
                                value = data[pos2 + 2];
                                value += data[pos2 + 1];
                                value += data[pos2];
                                values[(byte)Math.Round(value / 3d)]++;
                            }
                        }
                    }
                }
                else
                {
                    double lumR, lumG, lumB;

                    if (grayMultiplier == GrayMultiplier.NaturalNTSC)
                    {
                        lumR = RedLuminosityNTSC;
                        lumG = GreenLuminosityNTSC;
                        lumB = BlueLuminosityNTSC;
                    }
                    else
                    {
                        lumR = RedLuminosity;
                        lumG = GreenLuminosity;
                        lumB = BlueLuminosity;
                    }

                    if (dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                    {
                        float preAlpha;
                        for (y = dab.StartY; y < endY; y++)
                        {
                            pos1 = stride * y;
                            for (x = dab.StartX; x < endX; x++)
                            {
                                pos2 = pos1 + x * pixelBytes;
                                preAlpha = (float)data[pos2 + 3];
                                if (preAlpha > 0) preAlpha = preAlpha / 255f;
                                values[(byte)Math.Round((byte)(data[pos2 + 2] / preAlpha) * lumR + (byte)(data[pos2 + 1] / preAlpha) * lumG + (byte)(data[pos2] / preAlpha) * lumB)]++;
                            }
                        }
                    }
                    else
                    {
                        for (y = dab.StartY; y < endY; y++)
                        {
                            pos1 = stride * y;
                            for (x = dab.StartX; x < endX; x++)
                            {
                                pos2 = pos1 + x * pixelBytes;
                                values[(byte)Math.Round(data[pos2 + 2] * lumR + data[pos2 + 1] * lumG + data[pos2] * lumB)]++;
                            }
                        }
                    }
                }
            }
            else
            {
                int chanOffset = 0;
                if (channel == Channel.Alpha) chanOffset = 3;
                else if (channel == Channel.Red) chanOffset = 2;
                else if (channel == Channel.Green) chanOffset = 1;
                else if (channel == Channel.Blue) chanOffset = 0;
                else return ImageFilterError.InvalidArgument;

                if (dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                {
                    float preAlpha;
                    for (y = dab.StartY; y < endY; y++)
                    {
                        pos1 = stride * y;
                        for (x = dab.StartX; x < endX; x++)
                        {
                            pos2 = pos1 + x * pixelBytes + chanOffset;
                            preAlpha = (float)data[pos2 + 3];
                            if (preAlpha > 0) preAlpha = preAlpha / 255f;
                            values[(byte)(data[pos2] / preAlpha)]++;
                        }
                    }
                }
                else
                {
                    for (y = dab.StartY; y < endY; y++)
                    {
                        pos1 = stride * y;
                        for (x = dab.StartX; x < endX; x++)
                        {
                            pos2 = pos1 + x * pixelBytes + chanOffset;
                            values[data[pos2]]++;
                        }
                    }
                }
            }
            return ImageFilterError.OK;
        }
        public static ImageFilterError CalculateHistogram(DirectAccessBitmap dab, out int[] valuesR, out int[] valuesG, out int[] valuesB)
        {
            valuesR = valuesG = valuesB = null;
            if (dab == null) return ImageFilterError.InvalidArgument;
            if (dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppRgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb &&
                dab.Bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            {
                return ImageFilterError.IncompatiblePixelFormat;
            }

            valuesR = new int[256];
            valuesG = new int[256];
            valuesB = new int[256];

            int cx = dab.Width;
            int cy = dab.Height;
            int endX = cx + dab.StartX;
            int endY = cy + dab.StartY;
            int pixelBytes = dab.PixelFormatSize / 8;
            int endXb = endX * pixelBytes;
            byte[] data = dab.Bits;
            int stride = dab.Stride;
            int pos1, pos2;
            int x, y;

            if (dab.Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            {
                float preAlpha;
                for (y = dab.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = dab.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * pixelBytes;
                        preAlpha = (float)data[pos2 + 3];
                        if (preAlpha > 0) preAlpha = preAlpha / 255f;
                        valuesR[(byte)(data[pos2 + 2] / preAlpha)]++;
                        valuesG[(byte)(data[pos2 + 1] / preAlpha)]++;
                        valuesB[(byte)(data[pos2] / preAlpha)]++;
                    }
                }
            }
            else
            {
                for (y = dab.StartY; y < endY; y++)
                {
                    pos1 = stride * y;
                    for (x = dab.StartX; x < endX; x++)
                    {
                        pos2 = pos1 + x * pixelBytes;
                        valuesR[data[pos2 + 2]]++;
                        valuesG[data[pos2 + 2]]++;
                        valuesB[data[pos2]]++;
                    }
                }
            }
            return ImageFilterError.OK;
        }

        #region CalculateBox

        public static Size CalculateBox(
            Int32 Width, Int32 Height,
            Int32 BoundsX, Int32 BoundsY)
        {
            return CalculateBox(Width, Height, BoundsX, BoundsY, false, false, true);
        }

        public static Size CalculateBox(
            Int32 Width, Int32 Height,
            Int32 BoundsX, Int32 BoundsY,
            Boolean OutsideBox)
        {
            return CalculateBox(Width, Height, BoundsX, BoundsY, OutsideBox, false, true);
        }

        public static Size CalculateBox(
            Int32 Width, Int32 Height,
            Int32 BoundsX, Int32 BoundsY,
            Boolean OutsideBox, 
            Boolean AllowEnlarge)
        {
            return CalculateBox(Width, Height, BoundsX, BoundsY, OutsideBox, AllowEnlarge, true);
        }

        public static Size CalculateBox(
            Int32 Width, Int32 Height,
            Int32 BoundsX, Int32 BoundsY,
            Boolean OutsideBox,
            Boolean AllowEnlarge,
            Boolean AllowShrink)
        {
            Int32 newWidth, newHeight;
            if ((Width == BoundsX && Height == BoundsY) ||
                ((Width < BoundsX && Height < BoundsY) && !AllowEnlarge) ||
                ((Width > BoundsX && Height > BoundsY) && !AllowShrink) ||
                ((Width > BoundsX || Height > BoundsY) && !OutsideBox && !AllowShrink))
            {
                newWidth = Width;
                newHeight = Height;
            }
            else
            {
                Decimal aspectOriginal = (Height == 0) ? 1m : ((Decimal)Width / (Decimal)Height);
                Decimal aspectNew = (BoundsY == 0) ? 1m : ((Decimal)BoundsX / (Decimal)BoundsY);

                if ((aspectNew > aspectOriginal && !OutsideBox) ||
                        (aspectNew < aspectOriginal && OutsideBox))
                {
                    newHeight = BoundsY;
                    newWidth = Decimal.ToInt32(Decimal.Floor(((Decimal)BoundsY) * aspectOriginal));
                }
                else if ((aspectNew > aspectOriginal && OutsideBox) ||
                                 (aspectNew < aspectOriginal && !OutsideBox))
                {
                    newWidth = BoundsX;
                    newHeight = Decimal.ToInt32(Decimal.Floor(((Decimal)BoundsX) / aspectOriginal));
                }
                else // aspectNew==aspectOriginal
                {
                    newWidth = BoundsX;
                    newHeight = BoundsY;
                }
            }

            return new Size(newWidth, newHeight);
        }

        #endregion

        public static Size GetImageSize(String pathToImage)
        {
            if (pathToImage == null) return Size.Empty;

            return ImageDimensionsParser.GetImageSize(pathToImage);
        }

        public static void ApplyExifOrientation(Image image, bool removeExifOrientationTag)
        {
            if (image == null) return;

            try
            {
                PropertyItem item = image.GetPropertyItem((int)ExifPropertyTag.PropertyTagOrientation);
                if (item != null)
                {
                    switch (item.Value[0])
                    {
                        default:
                        case 1:
                            break;
                        case 2:
                            image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            break;
                        case 3:
                            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 4:
                            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            break;
                        case 5:
                            image.RotateFlip(RotateFlipType.Rotate270FlipX);
                            break;
                        case 6:
                            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 7:
                            image.RotateFlip(RotateFlipType.Rotate90FlipX);
                            break;
                        case 8:
                            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                    }
                    if (removeExifOrientationTag)
                    {
                        image.RemovePropertyItem(item.Id);
                    }
                }
            }
            catch { }
        }

        private static Color FixTransparentBgColor(Color Color, ImageFormat DestinationFormat)
        {
            if (DestinationFormat.Equals(ImageFormat.Png) ||
                DestinationFormat.Equals(ImageFormat.Gif) ||
                DestinationFormat.Equals(ImageFormat.Icon))
                return Color; // Supports transparency
            if (Color == Color.Transparent || Color == Color.Empty) Color = Color.White; // Avoid black backgrounds
            return Color;
        }

        public static void FillRoundRectangle(Graphics destinationGraphics, Rectangle destinationRect,
            int cornerRadiusTopLeft,
            int cornerRadiusTopRight,
            int cornerRadiusBottomRight,
            int cornerRadiusBottomLeft, 
            Brush fillBrush, Pen borderPen)
        {
            System.Drawing.Drawing2D.SmoothingMode mode = destinationGraphics.SmoothingMode;
            destinationGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            if (cornerRadiusTopLeft <= 0 && cornerRadiusTopRight <= 0 &&
                cornerRadiusBottomRight <= 0 && cornerRadiusBottomLeft <= 0)
            {
                // No corners.. Just fill it
                if (borderPen.Width > 0 && (borderPen.Color != Color.Transparent && borderPen.Color != Color.Empty))
                {
                    destinationGraphics.DrawRectangle(borderPen, destinationRect); // Draw the line first, this way we get better quality
                }
                destinationGraphics.FillRectangle(fillBrush, destinationRect);
            }
            else
            {
                GraphicsPath path = new GraphicsPath();

                int maxWRadius = destinationRect.Width / 2;
                int maxHRadius = destinationRect.Height / 2;
                                
                int cornerW, cornerH;
                if (cornerRadiusTopLeft > 0)
                {
                    cornerW = Math.Min(destinationRect.Width - Math.Min(cornerRadiusTopRight, destinationRect.Width), cornerRadiusTopLeft);
                    cornerH = Math.Min(destinationRect.Height - Math.Min(cornerRadiusBottomLeft, destinationRect.Height), cornerRadiusTopLeft);
                    path.AddArc(new Rectangle(destinationRect.Location, new Size(cornerW * 2, cornerH * 2)), 180, 90);
                }
                else
                {
                    path.AddLine(0, 0, 0, 1);
                    path.AddLine(0, 0, 1, 0);
                }
                if (cornerRadiusTopRight > 0)
                {
                    cornerW = Math.Min(destinationRect.Width - Math.Min(cornerRadiusTopLeft, destinationRect.Width), cornerRadiusTopRight);
                    cornerH = Math.Min(destinationRect.Height - Math.Min(cornerRadiusBottomRight, destinationRect.Height), cornerRadiusTopRight);
                    path.AddArc(new Rectangle(new Point(destinationRect.X + destinationRect.Width - cornerW * 2, destinationRect.Y), 
                        new Size(cornerW * 2, cornerH * 2)), 270, 90);
                }
                else
                {
                    path.AddLine(destinationRect.Right - 1, 0, destinationRect.Right, 0);
                    path.AddLine(destinationRect.Right, 0, destinationRect.Right, 1);
                }
                if (cornerRadiusBottomRight > 0)
                {
                    cornerW = Math.Min(destinationRect.Width - Math.Min(cornerRadiusBottomLeft, destinationRect.Width), cornerRadiusBottomRight);
                    cornerH = Math.Min(destinationRect.Height - Math.Min(cornerRadiusTopRight, destinationRect.Height), cornerRadiusBottomRight);
                    path.AddArc(new Rectangle(new Point(destinationRect.X + destinationRect.Width - cornerW * 2, destinationRect.Y + destinationRect.Height - cornerH * 2), 
                        new Size(cornerW * 2, cornerH * 2)), 0, 90);
                }
                else
                {
                    path.AddLine(destinationRect.Right - 1, destinationRect.Bottom, destinationRect.Right, destinationRect.Bottom);
                    path.AddLine(destinationRect.Right, destinationRect.Bottom - 1, destinationRect.Right, destinationRect.Bottom);
                }
                if (cornerRadiusBottomLeft > 0)
                {
                    cornerW = Math.Min(destinationRect.Width - Math.Min(cornerRadiusBottomRight, destinationRect.Width), cornerRadiusBottomLeft);
                    cornerH = Math.Min(destinationRect.Height - Math.Min(cornerRadiusTopLeft, destinationRect.Height), cornerRadiusBottomLeft);
                    path.AddArc(new Rectangle(new Point(destinationRect.X, destinationRect.Y + destinationRect.Height - cornerH * 2),
                        new Size(cornerW * 2, cornerH * 2)), 90, 90);
                }
                else
                {
                    path.AddLine(0, destinationRect.Bottom, 1, destinationRect.Bottom);
                    path.AddLine(0, destinationRect.Bottom - 1, 0, destinationRect.Bottom);
                }
                path.CloseFigure();

                if (borderPen.Width > 0 && (borderPen.Color != Color.Transparent && borderPen.Color != Color.Empty))
                {
                    destinationGraphics.DrawPath(borderPen, path); // Draw the line first, this way we get better quality
                }
                destinationGraphics.FillPath(fillBrush, path);
            }

            destinationGraphics.SmoothingMode = mode;
        }

        public static Boolean ProcessImage(
            String SourcePath, 
            String DestinationPath/*=null*/,
            ImageFormat DestinationFormat/*=null*/,
            Color BackgroundColor,
            Int32 BoundsX,
            Int32 BoundsY,
            Boolean MaintainAspectRatio/*=true*/,
            Boolean ShrinkToFit/*=true*/,
            Boolean EnlargeToFit/*=false*/,
            Boolean FitFromOutside/*=false*/,
            Boolean FixedFinalSize/*=false*/,
            Double ZoomFactor/*0.0d*/,
            CropAnchor CropAnchor/*=CropAnchor.NoCrop*/,
            Corner RoundedCorners,
            Int32 CornerRadius,
            Color BorderColor,
            float BorderWidth)
        {
            bool retValue = false;

            if (DestinationPath == null) DestinationPath = SourcePath;
            if (BackgroundColor == null || BackgroundColor == Color.Empty) BackgroundColor = Color.Transparent;
            if (BorderColor == null || BorderColor == Color.Empty) BorderColor = Color.Transparent;

            bool processBorderAndCorners =
                (BorderWidth > 0 && BorderColor != Color.Transparent)
                || (CornerRadius > 0 && RoundedCorners != Corner.None);

            try
            {
                if ((BoundsX <= 0 || BoundsY <= 0) && !FixedFinalSize)
                {
                    Size sz = GetImageSize(SourcePath);
                    if (BoundsX <= 0) BoundsX = sz.Width;
                    if (BoundsY <= 0) BoundsY = sz.Height;
                }
                using (System.Drawing.Image imgOriginal = System.Drawing.Image.FromFile(SourcePath))
                {
                    ApplyExifOrientation(imgOriginal, true);

                    if (DestinationFormat == null) DestinationFormat = imgOriginal.RawFormat;

                    BackgroundColor = FixTransparentBgColor(BackgroundColor, DestinationFormat);

                    Size szResizedImage = Size.Empty;

                    if (!MaintainAspectRatio)
                    {
                        if (FixedFinalSize)
                        {
                            if (BoundsX <= 0 || BoundsY <= 0)
                            {
                                szResizedImage = CalculateBox(
                                    imgOriginal.Width, imgOriginal.Height,
                                    BoundsX, BoundsY, FitFromOutside, EnlargeToFit, ShrinkToFit);
                                if (BoundsX <= 0) BoundsX = szResizedImage.Width;
                                if (BoundsY <= 0) BoundsY = szResizedImage.Height;
                            }
                        }
                        szResizedImage.Width = BoundsX;
                        szResizedImage.Height = BoundsY;
                    } 
                    else
                    {
                        szResizedImage = CalculateBox(
                            imgOriginal.Width, imgOriginal.Height,
                            BoundsX, BoundsY, FitFromOutside, EnlargeToFit, ShrinkToFit);
                        if (FixedFinalSize)
                        {
                            if (BoundsX <= 0) BoundsX = szResizedImage.Width;
                            if (BoundsY <= 0) BoundsY = szResizedImage.Height;
                        }
                    }
                    if (ZoomFactor != 0.0d && ZoomFactor != 1.0d)
                    {
                        szResizedImage.Width = (int)Math.Round((double)szResizedImage.Width * ZoomFactor);
                        szResizedImage.Height = (int)Math.Round((double)szResizedImage.Height * ZoomFactor);
                    }

                    Size finalSize = Size.Empty;
                    if (FixedFinalSize) 
                    {
                        finalSize.Width = BoundsX;
                        finalSize.Height = BoundsY;
                    }
                    else 
                    {
                        finalSize.Width = szResizedImage.Width > BoundsX ? BoundsX : szResizedImage.Width;
                        finalSize.Height = szResizedImage.Height > BoundsY ? BoundsY : szResizedImage.Height;
                    }

                    int xDrawPos = 0, yDrawPos = 0; // Default CropAnchor.None || CropAnchor.TopLeft
                    if ((CropAnchor & CropAnchor.Top) == CropAnchor.Top) yDrawPos = 0;
                    else if ((CropAnchor & CropAnchor.Bottom) == CropAnchor.Bottom) yDrawPos = -(szResizedImage.Height - finalSize.Height);
                    else if ((CropAnchor & CropAnchor.Center) == CropAnchor.Center) yDrawPos = -((szResizedImage.Height - finalSize.Height) / 2);
                    if ((CropAnchor & CropAnchor.Left) == CropAnchor.Left) xDrawPos = 0;
                    else if ((CropAnchor & CropAnchor.Right) == CropAnchor.Right) xDrawPos = -(szResizedImage.Width - finalSize.Width);
                    else if ((CropAnchor & CropAnchor.Center) == CropAnchor.Center) xDrawPos = -((szResizedImage.Width - finalSize.Width) / 2);

                    if (finalSize.Width == szResizedImage.Width) xDrawPos = 0; // Avoid 1px offset problems
                    if (finalSize.Height == szResizedImage.Height) yDrawPos = 0; // Avoid 1px offset problems

                    string tempFilePath = Files.CreateEmptyTempFile();

                    retValue = ProcessImageToFile(imgOriginal, tempFilePath ?? DestinationPath, DestinationFormat, 100L, delegate(Image frame)
                    {
                        System.Drawing.Image imgProcessed = null, imgProcessBorder = null;
                        try
                        {
                            imgProcessed = new System.Drawing.Bitmap(finalSize.Width, finalSize.Height);
                            using (Graphics gProcessed = Graphics.FromImage(imgProcessed))
                            {
                                gProcessed.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                gProcessed.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                gProcessed.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                gProcessed.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                                // Fill background if we are not intending to do that later on
                                if (!processBorderAndCorners) gProcessed.Clear(BackgroundColor);
                                else gProcessed.Clear(FixTransparentBgColor(Color.Transparent, DestinationFormat));

                                // Draw the final image in the correct position inside the final box
                                gProcessed.DrawImage(frame, xDrawPos, yDrawPos, szResizedImage.Width, szResizedImage.Height);

                                if (processBorderAndCorners)
                                {
                                    imgProcessBorder = new System.Drawing.Bitmap(finalSize.Width, finalSize.Height);
                                    using (System.Drawing.Graphics gProcessBorder = System.Drawing.Graphics.FromImage(imgProcessBorder))
                                    {
                                        gProcessBorder.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                        gProcessBorder.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                        gProcessBorder.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                        gProcessBorder.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                        gProcessBorder.Clear(BackgroundColor);
                                        using (System.Drawing.Brush brush = new System.Drawing.TextureBrush(imgProcessed))
                                        {
                                            using (System.Drawing.Pen pen = new System.Drawing.Pen(BorderColor, BorderWidth))
                                            {
                                                Int32 halfBorderWidth = (Int32)(BorderWidth / 2.0f + 0.5f);
                                                FillRoundRectangle(gProcessBorder,
                                                    new System.Drawing.Rectangle(
                                                        halfBorderWidth,
                                                        halfBorderWidth,
                                                        finalSize.Width - halfBorderWidth - halfBorderWidth,
                                                        finalSize.Height - halfBorderWidth - halfBorderWidth),
                                                        (RoundedCorners & Corner.TopLeft) == 0 ? 0 : CornerRadius,
                                                        (RoundedCorners & Corner.TopRight) == 0 ? 0 : CornerRadius,
                                                        (RoundedCorners & Corner.BottomRight) == 0 ? 0 : CornerRadius,
                                                        (RoundedCorners & Corner.BottomLeft) == 0 ? 0 : CornerRadius, 
                                                        brush, pen);
                                            }
                                        }
                                    }
                                    if (imgProcessed != null) imgProcessed.Dispose();
                                    imgProcessed = imgProcessBorder;
                                    imgProcessBorder = null;
                                }
                            }
                        }
                        catch (System.Exception ex)
                        {
                            if (imgProcessed != null) imgProcessed.Dispose();
                            if (imgProcessBorder != null) imgProcessBorder.Dispose();
                            imgProcessed = imgProcessBorder = null;
                            Console.WriteLine(@"ImagingUtility.ProcessImage - Error: {0}", ex.ToString());
                        }
                        return imgProcessed;
                    });

                    // So we can overwrite
                    imgOriginal.Dispose();

                    if (retValue && tempFilePath != null)
                    {
                        using (Files.TemporaryFileDeleter temporaryFileDeleter = new Files.TemporaryFileDeleter(tempFilePath))
                        {
                            if (System.IO.File.Exists(DestinationPath)) System.IO.File.Delete(DestinationPath);
                            System.IO.File.Move(tempFilePath, DestinationPath);
                            Files.ResetFilePermissionsToInherited(DestinationPath);
                            temporaryFileDeleter.DoNotDelete();
                        }
                    }
                }
            }
            catch (Exception) { }

            return retValue;
        }

        /// <summary>
        /// Saves an Image object to the specified local path
        /// In case of a GIF format chosen, it will use a Quantizer and save with high quality
        /// In case of a JPEG format, it will save with 100% quality
        /// 
        /// This may throw any Exception that an Image.Save(...) may throw
        /// </summary>
        /// <param name="imageData">Image object to save</param>
        /// <param name="imgPath">Destination local path</param>
        /// <param name="imgFormat">Image format to use</param>
        public static void SaveImage(Image imageData, string imgPath, ImageFormat imgFormat)
        {
            if (imgFormat.Equals(ImageFormat.Jpeg))
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

                    if (encoder != null) imageData.Save(imgPath, encoder, encoderParameters);
                    else imageData.Save(imgPath, imgFormat);
                }
            }
            else if (imgFormat.Equals(ImageFormat.Gif))
            {
                WuColorQuantizer quantizer = new WuColorQuantizer();
                using (Image quantized = ImageBuffer.QuantizeImage(imageData, quantizer, 255, 4))
                {
                    quantized.Save(imgPath, ImageFormat.Gif);
                }
            }
            else
            {
                imageData.Save(imgPath, imgFormat);
            }
        }

        public static bool SetImageRoundBorder(
            String sourcePath, 
            String destinationPath /* null for source */, 
            ImageFormat destinationFormat /* null for original format */, 
            int cornerRadius, 
            float borderWidth, 
            Color backgroundColor, 
            Color borderColor)
        {
            if (destinationPath == null) destinationPath = sourcePath;
            using (System.Drawing.Image imgOriginal = System.Drawing.Image.FromFile(sourcePath))
            {
                ApplyExifOrientation(imgOriginal, true);

                string tempFilePath = Files.CreateEmptyTempFile();

                bool retValue = ProcessImageToFile(imgOriginal, destinationPath, destinationFormat, 100L, delegate(Image frame)
                {
                    System.Drawing.Image imgProcessed = null;
                    try
                    {
                        imgProcessed = new System.Drawing.Bitmap(frame.Width, frame.Height);
                        using (Graphics gTemp = Graphics.FromImage(imgProcessed))
                        {
                            gTemp.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            gTemp.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            gTemp.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            gTemp.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            gTemp.Clear(backgroundColor);
                            using (Brush brush = new System.Drawing.TextureBrush(frame))
                            {
                                using (Pen pen = new System.Drawing.Pen(borderColor, borderWidth))
                                {
                                    Int32 halfBorderWidth = (Int32)(borderWidth / 2.0f + 0.5f);
                                    FillRoundRectangle(gTemp,
                                        new Rectangle(
                                            halfBorderWidth,
                                            halfBorderWidth,
                                            frame.Width - halfBorderWidth - halfBorderWidth,
                                            frame.Height - halfBorderWidth - halfBorderWidth),
                                            cornerRadius,
                                            cornerRadius,
                                            cornerRadius,
                                            cornerRadius, 
                                            brush, pen);

                                    return imgProcessed;
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (imgProcessed != null) imgProcessed.Dispose();
                        imgProcessed = null;
                        Console.WriteLine(@"ImagingUtility.SetImageRoundBorder - Error: {0}", ex.ToString());
                    }
                    return imgProcessed;
                });

                // So we can overwrite
                imgOriginal.Dispose();

                if (retValue && tempFilePath != null)
                {
                    using (Files.TemporaryFileDeleter temporaryFileDeleter = new Files.TemporaryFileDeleter(tempFilePath))
                    {
                        if (System.IO.File.Exists(destinationPath)) System.IO.File.Delete(destinationPath);
                        System.IO.File.Move(tempFilePath, destinationPath);
                        Files.ResetFilePermissionsToInherited(destinationPath);
                        temporaryFileDeleter.DoNotDelete();
                    }
                }

                return retValue;
            }
        }
        
        public delegate Image ProcessImageFrameDelegate(Image frame);

        /// <summary>
        /// Will trigger a function for processing each frame in image (will handle GIFs as well), and save the result to the destination file.
        /// </summary>
        /// <param name="SourceImageData">Source image</param>
        /// <param name="DestinationPath">Destination path</param>
        /// <param name="DestinationFormat">Destination file format. null for original</param>
        /// <param name="JpegQuality">1-100. Any out of range value will default to 100. 0 will mean 100.</param>
        /// <param name="Processor">A processing function that will be executed for each frame.</param>
        /// <returns>true if successful, false if there was any failure.</returns>
        public static bool ProcessImageToFile(Image SourceImageData, String DestinationPath, ImageFormat DestinationFormat, long JpegQuality, ProcessImageFrameDelegate Processor)
        {
            if (DestinationFormat == null) DestinationFormat = SourceImageData.RawFormat;

            if (SourceImageData.RawFormat.Equals(ImageFormat.Gif) && DestinationFormat.Equals(ImageFormat.Gif))
            {
                int frameCount = SourceImageData.GetFrameCount(FrameDimension.Time);
                byte[] durationBytes = SourceImageData.GetPropertyItem((int)ExifPropertyTag.PropertyTagFrameDelay).Value;
                Int16 loopCount = BitConverter.ToInt16(SourceImageData.GetPropertyItem((int)ExifPropertyTag.PropertyTagLoopCount).Value, 0);

                GifEncoder gifEncoder = new GifEncoder();
                try
                {
                    gifEncoder.Start(DestinationPath);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(@"ImagingUtility.ProcessImageToFile - Error: {0}", ex.ToString());
                    return false;
                }
                gifEncoder.SetSize(SourceImageData.Width, SourceImageData.Height);
                gifEncoder.SetRepeat(loopCount);

                WuColorQuantizer quantizer = new WuColorQuantizer();
                byte[] pallette = SourceImageData.GetPropertyItem((int)dg.Utilities.Imaging.ExifPropertyTag.PropertyTagGlobalPalette).Value;
                int transparentColorIndex = -1;
                Color transparentColor = Color.Empty;
                try
                {
                    transparentColorIndex = SourceImageData.GetPropertyItem((int)dg.Utilities.Imaging.ExifPropertyTag.PropertyTagIndexTransparent).Value[0];
                    transparentColor = Color.FromArgb(pallette[transparentColorIndex * 3], pallette[transparentColorIndex * 3 + 1], pallette[transparentColorIndex * 3 + 2]);
                }
                catch { }

                for (int frame = 0, duration; frame < frameCount; frame++)
                {
                    try
                    {
                        duration = BitConverter.ToInt32(durationBytes, 4 * frame); // In hundredth of a second
                        SourceImageData.SelectActiveFrame(FrameDimension.Time, frame);

                        if (Processor != null)
                        {
                            using (Image output = Processor(SourceImageData))
                            {
                                if (output != null)
                                {
                                    if (frame == 0)
                                    {
                                        gifEncoder.SetSize(output.Width, output.Height);
                                    }
                                    using (Image quantized = ImageBuffer.QuantizeImage(output, quantizer, 255, 4))
                                    {
                                        gifEncoder.SetNextFrameDuration(duration * 10);
                                        gifEncoder.SetNextFrameTransparentColor(transparentColor);
                                        gifEncoder.AddFrame(quantized);
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (Image quantized = ImageBuffer.QuantizeImage(SourceImageData, quantizer, 255, 4))
                            {
                                gifEncoder.SetNextFrameDuration(duration * 10);
                                gifEncoder.SetNextFrameTransparentColor(transparentColor);
                                gifEncoder.AddFrame(quantized);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(@"ImagingUtility.ProcessImageToFile - Error: {0}", ex.ToString());
                    }
                }

                try
                {
                    gifEncoder.Finish();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(@"ImagingUtility.ProcessImageToFile - Error: {0}", ex.ToString());
                    return false;
                }
            }
            else
            {
                if (Processor != null)
                {
                    using (Image output = Processor(SourceImageData))
                    {
                        if (output != null)
                        {
                            if (DestinationFormat.Equals(ImageFormat.Jpeg))
                            {
                                System.Drawing.Imaging.ImageCodecInfo encoder = null;
                                System.Drawing.Imaging.ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
                                using (EncoderParameters encoderParameters = new EncoderParameters(1))
                                {
                                    if (JpegQuality == 0L || JpegQuality > 100L) JpegQuality = 100L;
                                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, JpegQuality);
                                    foreach (System.Drawing.Imaging.ImageCodecInfo item in encoders)
                                    {
                                        if (item.MimeType == @"image/jpeg")
                                        {
                                            encoder = item;
                                            break;
                                        }
                                    }

                                    if (encoder != null) output.Save(DestinationPath, encoder, encoderParameters);
                                    else output.Save(DestinationPath, DestinationFormat);
                                }
                            }
                            else if (DestinationFormat.Equals(ImageFormat.Gif))
                            {
                                WuColorQuantizer quantizer = new WuColorQuantizer();
                                using (Image quantized = ImageBuffer.QuantizeImage(output, quantizer, 255, 4))
                                {
                                    quantized.Save(DestinationPath, ImageFormat.Gif);
                                }
                            }
                            else
                            {
                                output.Save(DestinationPath, DestinationFormat);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (DestinationFormat.Equals(ImageFormat.Jpeg))
                    {
                        System.Drawing.Imaging.ImageCodecInfo encoder = null;
                        System.Drawing.Imaging.ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
                        using (EncoderParameters encoderParameters = new EncoderParameters(1))
                        {
                            if (JpegQuality == 0L || JpegQuality > 100L) JpegQuality = 100L;
                            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, JpegQuality);
                            foreach (System.Drawing.Imaging.ImageCodecInfo item in encoders)
                            {
                                if (item.MimeType == @"image/jpeg")
                                {
                                    encoder = item;
                                    break;
                                }
                            }

                            if (encoder != null) SourceImageData.Save(DestinationPath, encoder, encoderParameters);
                            else SourceImageData.Save(DestinationPath, DestinationFormat);
                        }
                    }
                    else if (DestinationFormat.Equals(ImageFormat.Gif))
                    {
                        WuColorQuantizer quantizer = new WuColorQuantizer();
                        using (Image quantized = ImageBuffer.QuantizeImage(SourceImageData, quantizer, 255, 4))
                        {
                            quantized.Save(DestinationPath, ImageFormat.Gif);
                        }
                    }
                    else
                    {
                        SourceImageData.Save(DestinationPath, DestinationFormat);
                    }
                }
            }
            return true;
        }
    }

}
