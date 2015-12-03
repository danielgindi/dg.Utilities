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
using dg.Utilities.Imaging.Quantizers;
using dg.Utilities.Imaging.Processing;

namespace dg.Utilities.Imaging
{
    public static partial class ProcessingHelper
    {
        public delegate Image ProcessImageFrameDelegate(Image frame);
        public delegate BaseColorQuantizer QuantizerRequestDelegate(Image frame);
        
        public class EncodingOptions
        {
            public EncodingOptions()
            {
                this.JpegQuality = 1.0f;
            }

            public float JpegQuality;
            public QuantizerRequestDelegate QuantizerSupplier;
        }

        /// <summary>
        /// Will trigger a function for processing each frame in image (will handle GIFs as well), and save the result to the destination file.
        /// </summary>
        /// <param name="sourceImageData">Source image</param>
        /// <param name="destinationPath">Destination path</param>
        /// <param name="destinationFormat">Destination file format. null for original</param>
        /// <param name="jpegQuality">0.0 - 1.0 Any out of range value will default to 1.0. 0 will mean 0.0.</param>
        /// <param name="processor">A processing function that will be executed for each frame.</param>
        /// <returns>true if successful, false if there was any failure.</returns>
        public static bool ProcessImageFramesToFile(
            Image sourceImageData,
            String destinationPath,
            ImageFormat destinationFormat,
            EncodingOptions encodingOptions,
            ProcessImageFrameDelegate processor)
        {
            if (destinationFormat == null) destinationFormat = sourceImageData.RawFormat;

            if (encodingOptions.JpegQuality < 0.0f || encodingOptions.JpegQuality > 1.0f)
            {
                encodingOptions.JpegQuality = 1.0f;
            }

            if (sourceImageData.RawFormat.Equals(ImageFormat.Gif) && destinationFormat.Equals(ImageFormat.Gif))
            {
                int frameCount = sourceImageData.GetFrameCount(FrameDimension.Time);
                byte[] durationBytes = sourceImageData.GetPropertyItem((int)ExifPropertyTag.PropertyTagFrameDelay).Value;
                Int16 loopCount = BitConverter.ToInt16(sourceImageData.GetPropertyItem((int)ExifPropertyTag.PropertyTagLoopCount).Value, 0);

                GifEncoder gifEncoder = new GifEncoder();
                try
                {
                    gifEncoder.Start(destinationPath);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(@"ImagingUtility.ProcessImageFramesToFile - Error: {0}", ex.ToString());
                    return false;
                }
                gifEncoder.SetSize(sourceImageData.Width, sourceImageData.Height);
                gifEncoder.SetRepeat(loopCount);

                byte[] pallette = sourceImageData.GetPropertyItem((int)dg.Utilities.Imaging.ExifPropertyTag.PropertyTagGlobalPalette).Value;
                int transparentColorIndex = -1;
                Color transparentColor = Color.Empty;
                try
                {
                    transparentColorIndex = sourceImageData.GetPropertyItem((int)dg.Utilities.Imaging.ExifPropertyTag.PropertyTagIndexTransparent).Value[0];
                    transparentColor = Color.FromArgb(pallette[transparentColorIndex * 3], pallette[transparentColorIndex * 3 + 1], pallette[transparentColorIndex * 3 + 2]);
                }
                catch { }

                for (int frame = 0, duration; frame < frameCount; frame++)
                {
                    try
                    {
                        duration = BitConverter.ToInt32(durationBytes, 4 * frame); // In hundredth of a second
                        sourceImageData.SelectActiveFrame(FrameDimension.Time, frame);

                        if (processor != null)
                        {
                            using (Image output = processor(sourceImageData))
                            {
                                if (output != null)
                                {
                                    if (frame == 0)
                                    {
                                        gifEncoder.SetSize(output.Width, output.Height);
                                    }

                                    BaseColorQuantizer quantizer = null;
                                    if (encodingOptions.QuantizerSupplier != null)
                                    {
                                        quantizer = encodingOptions.QuantizerSupplier(output);
                                    }
                                    if (quantizer == null)
                                    {
                                        quantizer = new WuColorQuantizer();
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
                            BaseColorQuantizer quantizer = null;
                            if (encodingOptions.QuantizerSupplier != null)
                            {
                                quantizer = encodingOptions.QuantizerSupplier(sourceImageData);
                            }
                            if (quantizer == null)
                            {
                                quantizer = new WuColorQuantizer();
                            }

                            using (Image quantized = ImageBuffer.QuantizeImage(sourceImageData, quantizer, 255, 4))
                            {
                                gifEncoder.SetNextFrameDuration(duration * 10);
                                gifEncoder.SetNextFrameTransparentColor(transparentColor);
                                gifEncoder.AddFrame(quantized);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(@"ImagingUtility.ProcessImageFramesToFile - Error: {0}", ex.ToString());
                    }
                }

                try
                {
                    gifEncoder.Finish();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(@"ImagingUtility.ProcessImageFramesToFile - Error: {0}", ex.ToString());
                    return false;
                }
            }
            else
            {
                if (processor != null)
                {
                    using (Image output = processor(sourceImageData))
                    {
                        if (output != null)
                        {
                            if (destinationFormat.Equals(ImageFormat.Jpeg))
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

                                    if (encoder != null) output.Save(destinationPath, encoder, encoderParameters);
                                    else output.Save(destinationPath, destinationFormat);
                                }
                            }
                            else if (destinationFormat.Equals(ImageFormat.Gif))
                            {
                                BaseColorQuantizer quantizer = null;
                                if (encodingOptions.QuantizerSupplier != null)
                                {
                                    quantizer = encodingOptions.QuantizerSupplier(output);
                                }
                                if (quantizer == null)
                                {
                                    quantizer = new WuColorQuantizer();
                                }

                                using (Image quantized = ImageBuffer.QuantizeImage(output, quantizer, 255, 4))
                                {
                                    quantized.Save(destinationPath, ImageFormat.Gif);
                                }
                            }
                            else
                            {
                                output.Save(destinationPath, destinationFormat);
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
                    if (destinationFormat.Equals(ImageFormat.Jpeg))
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

                            if (encoder != null) sourceImageData.Save(destinationPath, encoder, encoderParameters);
                            else sourceImageData.Save(destinationPath, destinationFormat);
                        }
                    }
                    else if (destinationFormat.Equals(ImageFormat.Gif))
                    {
                        BaseColorQuantizer quantizer = null;
                        if (encodingOptions.QuantizerSupplier != null)
                        {
                            quantizer = encodingOptions.QuantizerSupplier(sourceImageData);
                        }
                        if (quantizer == null)
                        {
                            quantizer = new WuColorQuantizer();
                        }

                        using (Image quantized = ImageBuffer.QuantizeImage(sourceImageData, quantizer, 255, 4))
                        {
                            quantized.Save(destinationPath, ImageFormat.Gif);
                        }
                    }
                    else
                    {
                        sourceImageData.Save(destinationPath, destinationFormat);
                    }
                }
            }
            return true;
        }
    }

}
