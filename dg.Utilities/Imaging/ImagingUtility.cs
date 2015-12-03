using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using dg.Utilities.Imaging.Encoders;
using dg.Utilities.Imaging.Quantizers.Helpers;
using dg.Utilities.Imaging.Quantizers.XiaolinWu;
using dg.Utilities.Imaging.Decoders;
using dg.Utilities.Imaging.Quantizers;

namespace dg.Utilities.Imaging
{
    public static partial class ImagingUtility
    {
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

            return ImageDimensionsDecoder.GetImageSize(pathToImage);
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

        private static Color FixTransparentBgColor(Color color, ImageFormat destinationFormat)
        {
            if (destinationFormat.Equals(ImageFormat.Png) ||
                destinationFormat.Equals(ImageFormat.Gif) ||
                destinationFormat.Equals(ImageFormat.Icon))
                return color; // Supports transparency

            if (color == Color.Transparent || color == Color.Empty)
            {
                color = Color.White; // Avoid black backgrounds
            }

            return color;
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
            String sourcePath, 
            String destinationPath/*=null*/,
            ImageFormat destinationFormat/*=null*/,
            Color backgroundColor,
            Int32 boundsX,
            Int32 boundsY,
            Boolean maintainAspectRatio/*=true*/,
            Boolean shrinkToFit/*=true*/,
            Boolean enlargeToFit/*=false*/,
            Boolean fitFromOutside/*=false*/,
            Boolean fixedFinalSize/*=false*/,
            Double zoomFactor/*0.0d*/,
            CropAnchor cropAnchor/*=CropAnchor.NoCrop*/,
            Corner roundedCorners,
            Int32 cornerRadius,
            Color borderColor,
            float borderWidth)
        {
            bool retValue = false;

            if (destinationPath == null) destinationPath = sourcePath;
            if (backgroundColor == null || backgroundColor == Color.Empty) backgroundColor = Color.Transparent;
            if (borderColor == null || borderColor == Color.Empty) borderColor = Color.Transparent;

            bool processBorderAndCorners =
                (borderWidth > 0 && borderColor != Color.Transparent)
                || (cornerRadius > 0 && roundedCorners != Corner.None);

            try
            {
                if ((boundsX <= 0 || boundsY <= 0) && !fixedFinalSize)
                {
                    Size sz = GetImageSize(sourcePath);
                    if (boundsX <= 0) boundsX = sz.Width;
                    if (boundsY <= 0) boundsY = sz.Height;
                }
                using (System.Drawing.Image imgOriginal = System.Drawing.Image.FromFile(sourcePath))
                {
                    ApplyExifOrientation(imgOriginal, true);

                    if (destinationFormat == null) destinationFormat = imgOriginal.RawFormat;

                    backgroundColor = FixTransparentBgColor(backgroundColor, destinationFormat);

                    Size szResizedImage = Size.Empty;

                    if (!maintainAspectRatio)
                    {
                        if (fixedFinalSize)
                        {
                            if (boundsX <= 0 || boundsY <= 0)
                            {
                                szResizedImage = CalculateBox(
                                    imgOriginal.Width, imgOriginal.Height,
                                    boundsX, boundsY, fitFromOutside, enlargeToFit, shrinkToFit);
                                if (boundsX <= 0) boundsX = szResizedImage.Width;
                                if (boundsY <= 0) boundsY = szResizedImage.Height;
                            }
                        }
                        szResizedImage.Width = boundsX;
                        szResizedImage.Height = boundsY;
                    } 
                    else
                    {
                        szResizedImage = CalculateBox(
                            imgOriginal.Width, imgOriginal.Height,
                            boundsX, boundsY, fitFromOutside, enlargeToFit, shrinkToFit);
                        if (fixedFinalSize)
                        {
                            if (boundsX <= 0) boundsX = szResizedImage.Width;
                            if (boundsY <= 0) boundsY = szResizedImage.Height;
                        }
                    }
                    if (zoomFactor != 0.0d && zoomFactor != 1.0d)
                    {
                        szResizedImage.Width = (int)Math.Round((double)szResizedImage.Width * zoomFactor);
                        szResizedImage.Height = (int)Math.Round((double)szResizedImage.Height * zoomFactor);
                    }

                    Size finalSize = Size.Empty;
                    if (fixedFinalSize) 
                    {
                        finalSize.Width = boundsX;
                        finalSize.Height = boundsY;
                    }
                    else 
                    {
                        finalSize.Width = szResizedImage.Width > boundsX ? boundsX : szResizedImage.Width;
                        finalSize.Height = szResizedImage.Height > boundsY ? boundsY : szResizedImage.Height;
                    }

                    int xDrawPos = 0, yDrawPos = 0; // Default CropAnchor.None || CropAnchor.TopLeft
                    if ((cropAnchor & CropAnchor.Top) == CropAnchor.Top) yDrawPos = 0;
                    else if ((cropAnchor & CropAnchor.Bottom) == CropAnchor.Bottom) yDrawPos = -(szResizedImage.Height - finalSize.Height);
                    else if ((cropAnchor & CropAnchor.Center) == CropAnchor.Center) yDrawPos = -((szResizedImage.Height - finalSize.Height) / 2);
                    if ((cropAnchor & CropAnchor.Left) == CropAnchor.Left) xDrawPos = 0;
                    else if ((cropAnchor & CropAnchor.Right) == CropAnchor.Right) xDrawPos = -(szResizedImage.Width - finalSize.Width);
                    else if ((cropAnchor & CropAnchor.Center) == CropAnchor.Center) xDrawPos = -((szResizedImage.Width - finalSize.Width) / 2);

                    if (finalSize.Width == szResizedImage.Width) xDrawPos = 0; // Avoid 1px offset problems
                    if (finalSize.Height == szResizedImage.Height) yDrawPos = 0; // Avoid 1px offset problems

                    string tempFilePath = Files.CreateEmptyTempFile();

                    retValue = ProcessingHelper.ProcessImageFramesToFile(imgOriginal, tempFilePath ?? destinationPath, destinationFormat, new ProcessingHelper.EncodingOptions(), delegate(Image frame)
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
                                if (!processBorderAndCorners) gProcessed.Clear(backgroundColor);
                                else gProcessed.Clear(FixTransparentBgColor(Color.Transparent, destinationFormat));

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
                                        gProcessBorder.Clear(backgroundColor);
                                        using (System.Drawing.Brush brush = new System.Drawing.TextureBrush(imgProcessed))
                                        {
                                            using (System.Drawing.Pen pen = new System.Drawing.Pen(borderColor, borderWidth))
                                            {
                                                Int32 halfBorderWidth = (Int32)(borderWidth / 2.0f + 0.5f);
                                                FillRoundRectangle(gProcessBorder,
                                                    new System.Drawing.Rectangle(
                                                        halfBorderWidth,
                                                        halfBorderWidth,
                                                        finalSize.Width - halfBorderWidth - halfBorderWidth,
                                                        finalSize.Height - halfBorderWidth - halfBorderWidth),
                                                        (roundedCorners & Corner.TopLeft) == 0 ? 0 : cornerRadius,
                                                        (roundedCorners & Corner.TopRight) == 0 ? 0 : cornerRadius,
                                                        (roundedCorners & Corner.BottomRight) == 0 ? 0 : cornerRadius,
                                                        (roundedCorners & Corner.BottomLeft) == 0 ? 0 : cornerRadius, 
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
                            if (System.IO.File.Exists(destinationPath)) System.IO.File.Delete(destinationPath);
                            System.IO.File.Move(tempFilePath, destinationPath);
                            Files.ResetFilePermissionsToInherited(destinationPath);
                            temporaryFileDeleter.DoNotDelete();
                        }
                    }
                }
            }
            catch (Exception) { }

            return retValue;
        }

        #region Saving

        /// <summary>
        /// Saves an Image object to the specified local path
        /// 
        /// This may throw any Exception that an Image.Save(...) may throw
        /// </summary>
        /// <param name="imageData">Image object to save</param>
        /// <param name="imagePath">Destination local path</param>
        /// <param name="imageFormat">Image format to use</param>
        /// <param name="jpegQuality">Quality to use in case of a jpeg format (0.0 - 1.0)</param>
        public static void SaveImage(Image imageData, string imagePath, ImageFormat imageFormat, ProcessingHelper.EncodingOptions encodingOptions)
        {
            if (encodingOptions.JpegQuality < 0.0f || encodingOptions.JpegQuality > 1.0f)
            {
                encodingOptions.JpegQuality = 1.0f;
            }

            if (imageFormat.Equals(ImageFormat.Jpeg))
            {
                System.Drawing.Imaging.ImageCodecInfo encoder = null;
                System.Drawing.Imaging.ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
                using (EncoderParameters encoderParameters = new EncoderParameters(1))
                {
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, Math.Max(Math.Min((long)Math.Round(encodingOptions.JpegQuality * 100), 100L), 0L));
                    foreach (System.Drawing.Imaging.ImageCodecInfo item in encoders)
                    {
                        if (item.MimeType == @"image/jpeg")
                        {
                            encoder = item;
                            break;
                        }
                    }

                    if (encoder != null) imageData.Save(imagePath, encoder, encoderParameters);
                    else imageData.Save(imagePath, imageFormat);
                }
            }
            else if (imageFormat.Equals(ImageFormat.Gif))
            {
                BaseColorQuantizer quantizer = null;
                if (encodingOptions.QuantizerSupplier != null)
                {
                    quantizer = encodingOptions.QuantizerSupplier(imageData);
                }
                if (quantizer == null)
                {
                    quantizer = new WuColorQuantizer();
                }

                using (Image quantized = ImageBuffer.QuantizeImage(imageData, quantizer, 255, 4))
                {
                    quantized.Save(imagePath, ImageFormat.Gif);
                }
            }
            else
            {
                imageData.Save(imagePath, imageFormat);
            }
        }

        /// <summary>
        /// Saves an Image object to the specified local path
        /// In case of a GIF format chosen, it will use a default high quality Quantizer 
        /// In case of a JPEG format, it will default to 1.0 (100%) quality
        /// 
        /// This may throw any Exception that an Image.Save(...) may throw
        /// </summary>
        /// <param name="imageData">Image object to save</param>
        /// <param name="imagePath">Destination local path</param>
        /// <param name="imageFormat">Image format to use</param>
        public static void SaveImage(Image imageData, string imagePath, ImageFormat imageFormat)
        {
            SaveImage(imageData, imagePath, imageFormat, new ProcessingHelper.EncodingOptions());
        }

        #endregion

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

                bool retValue = ProcessingHelper.ProcessImageFramesToFile(imgOriginal, destinationPath, destinationFormat, new ProcessingHelper.EncodingOptions(), delegate(Image frame)
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

        public static string MimeTypeForFormat(ImageFormat imageFormat)
        {
            if (imageFormat.Equals(ImageFormat.Jpeg))
            {
                return "image/jpeg";
            }
            else if (imageFormat.Equals(ImageFormat.Png))
            {
                return "image/png";
            }
            else if (imageFormat.Equals(ImageFormat.Gif))
            {
                return "image/gif";
            }
            else if (imageFormat.Equals(ImageFormat.Bmp))
            {
                return "image/bmp";
            }
            else if (imageFormat.Equals(ImageFormat.Emf))
            {
                return "image/x-emf";
            }
            else if (imageFormat.Equals(ImageFormat.Exif))
            {
                return "image/x-exif";
            }
            else if (imageFormat.Equals(ImageFormat.Icon))
            {
                return "image/x-icon";
            }
            else if (imageFormat.Equals(ImageFormat.Tiff))
            {
                return "image/tiff";
            }
            else if (imageFormat.Equals(ImageFormat.Wmf))
            {
                return "image/x-wmf";
            }

            return "image/x-unknown";
        }
    }

}
