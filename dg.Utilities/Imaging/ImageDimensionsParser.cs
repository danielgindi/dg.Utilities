using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using dg.Utilities.General_Utilities;

//  Created by Daniel Cohen Gindi on 3/29/13.
//  Copyright (c) 2013 danielgindi@gmail.com. All rights reserved.
//
namespace dg.Utilities.Imaging
{
    public static class ImageDimensionsParser
    {
        static byte[] JPEG_HEADER = new byte[] { 0xff, 0xd8 };
        static byte[] JPEG_EXIF_HEADER = new byte[] { (byte)'E', (byte)'x', (byte)'i', (byte)'f' };
        static byte[] PNG_HEADER = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        static byte[] GIF_HEADER = new byte[] { (byte)'G', (byte)'I', (byte)'F' };
        static byte[] BMP_HEADER = new byte[] { 0x42, 0x4D };
        
        const UInt16 EXIF_BYTE       = 1;		// BYTE	    8-bit unsigned integer
        const UInt16 EXIF_SHORT      = 3;		// SHORT    16-bit unsigned integer
        const UInt16 EXIF_LONG       = 4;		// LONG     32-bit unsigned integer
        const UInt16 EXIF_SBYTE      = 6;		// SBYTE    8-bit signed integer
        const UInt16 EXIF_SSHORT     = 8;		// SSHORT   16-bit signed integer
        const UInt16 EXIF_SLONG      = 9;		// SLONG    32-bit signed integer

        private static Size GetImageSize_EXIF(FileStream stream)
        {
            byte[] buffer = new byte[4];

            long offset = stream.Position;

            // Read Byte alignment
            if (stream.Read(buffer, 0, 2) != 2) return Size.Empty;

            bool littleEndian = false;
            if (buffer[0] == 0x49 && buffer[1] == 0x49)
            {
                littleEndian = true;
            }
            else if (buffer[0] != 0x4D && buffer[1] != 0x4D) return Size.Empty;

            using (EndianSensitiveReader reader = new EndianSensitiveReader(stream))
            {
                reader.LittleEndian = littleEndian;

                // TIFF tag marker
                if (reader.ReadUInt16() != 0x002A) return Size.Empty;

                // Directory offset bytes
                UInt32 dirOffset = reader.ReadUInt32();

                ExifPropertyTag tag;
                UInt16 numberOfTags, tagType;
                UInt32 tagLength, tagValue;
                int orientation = 1, width = 0, height = 0;
                UInt32 exifIFDOffset = 0;

                while (dirOffset != 0)
                {
                    stream.Seek(offset + dirOffset, SeekOrigin.Begin);

                    numberOfTags = reader.ReadUInt16();

                    for (UInt16 i = 0; i < numberOfTags; i++)
                    {
                        tag = (ExifPropertyTag)reader.ReadUInt16();
                        tagType = reader.ReadUInt16();
                        tagLength = reader.ReadUInt32();

                        if (tag == ExifPropertyTag.PropertyTagOrientation ||
                            tag == ExifPropertyTag.PropertyTagExifPixXDim ||
                            tag == ExifPropertyTag.PropertyTagExifPixYDim ||
                            tag == ExifPropertyTag.PropertyTagImageWidth ||
                            tag == ExifPropertyTag.PropertyTagImageHeight ||
                            tag == ExifPropertyTag.PropertyTagExifIFD)
                        {
                            switch (tagType)
                            {
                                default:
                                case EXIF_BYTE:
                                case EXIF_SBYTE:
                                    tagValue = reader.ReadByte();
                                    stream.Seek(3, SeekOrigin.Current);
                                    break;
                                case EXIF_SHORT:
                                case EXIF_SSHORT:
                                    tagValue = reader.ReadUInt16();
                                    stream.Seek(2, SeekOrigin.Current);
                                    break;
                                case EXIF_LONG:
                                    tagValue = reader.ReadUInt32();
                                    break;
                                case EXIF_SLONG:
                                    tagValue = (UInt32)reader.ReadInt32();
                                    break;
                            }

                            switch (tag)
                            {
                                case ExifPropertyTag.PropertyTagOrientation:
                                    // Orientation tag
                                    orientation = (int)tagValue;
                                    break;
                                case ExifPropertyTag.PropertyTagExifPixXDim:
                                case ExifPropertyTag.PropertyTagImageWidth:
                                    // Width tag
                                    width = (int)tagValue;
                                    break;
                                case ExifPropertyTag.PropertyTagExifPixYDim:
                                case ExifPropertyTag.PropertyTagImageHeight:
                                    // Height tag
                                    height = (int)tagValue;
                                    break;
                                case ExifPropertyTag.PropertyTagExifIFD:
                                    // EXIF IFD offset tag
                                    exifIFDOffset = tagValue;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            stream.Seek(4, SeekOrigin.Current);
                        }
                    }

                    if (dirOffset == exifIFDOffset)
                    {
                        break;
                    }

                    dirOffset = reader.ReadUInt32();

                    if (dirOffset == 0)
                    {
                        dirOffset = exifIFDOffset;
                    }
                }

                if (width > 0 && height > 0)
                {
                    if (orientation >= 5 && orientation <= 8)
                    {
                        return new Size(height, width);
                    }
                    else
                    {
                        return new Size(width, height);
                    }
                }
            }

            return Size.Empty;
        }

        private static Size GetImageSize_JPEG(FileStream stream)
        {
            byte[] buffer = new byte[4];

            while (stream.Read(buffer, 0, 2) == 2 && buffer[0] == 0xFF &&
                ((buffer[1] >= 0xE0 && buffer[1] <= 0xEF) ||
                buffer[1] == 0xDB ||
                buffer[1] == 0xC4 || buffer[1] == 0xC2 ||
                buffer[1] == 0xC0))
            {
                if (buffer[1] == 0xE1)
                { // Parse APP1 EXIF
                    
                    // Marker segment length
                    if (stream.Read(buffer, 0, 2) != 2) return Size.Empty;
                    // int blockLength = ((buffer[0] << 8) | buffer[1]) - 2;

                    // Exif
                    if (stream.Read(buffer, 0, 4) != 4
                        || !CompareBytesUnsafe(buffer, JPEG_EXIF_HEADER, 4)) return Size.Empty;

                    // Read Byte alignment offset
                    if (stream.Read(buffer, 0, 2) != 2 ||
                        buffer[0] != 0x00 || buffer[1] != 0x00) return Size.Empty;

                    Size size = GetImageSize_EXIF(stream);
                    if (!size.IsEmpty)
                    {
                        return size;
                    }
                }
                else if (buffer[1] == 0xC0 || buffer[1] == 0xC2)
                { // Parse SOF0 (Start of Frame, Baseline DCT or Progressive DCT)
            
                    // Skip LF, P
                    stream.Seek(3, SeekOrigin.Current);
            
                    // Read Y,X
                    if (stream.Read(buffer, 0, 4) != 4) return Size.Empty;

                    return new Size(buffer[2] << 8 | buffer[3], buffer[0] << 8 | buffer[1]);
                }
                else
                { // Skip APPn segment
                    if (stream.Read(buffer, 0, 2) == 2)
                    { // Marker segment length
                        stream.Seek((int)((buffer[0] << 8) | buffer[1]) - 2, SeekOrigin.Current);
                    }
                    else
                    {
                        return Size.Empty;
                    }
                }
            }

            return Size.Empty;
        }

        public static Size GetImageSize(string path)
        {
            bool success = false;
            Size size = Size.Empty;

            using (FileStream fileStream = File.OpenRead(path))
            {
                byte[] buffer = new byte[4];

                try
                {
                    if (fileStream.Read(buffer, 0, 2) == 2 &&
                        CompareBytesUnsafe(buffer, JPEG_HEADER, 2))
                    {// JPEG                        
                        size = GetImageSize_JPEG(fileStream);
                        success = !size.IsEmpty;
                    }
                }
                catch { }

                try
                {
                    if (!success)
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);

                        byte[] buffer8 = new byte[8];

                        if (fileStream.Read(buffer8, 0, 8) == 8 &&
                            CompareBytesUnsafe(buffer, PNG_HEADER))
                        {
                            // PNG

                            fileStream.Seek(8, SeekOrigin.Current);

                            if (fileStream.Read(buffer, 0, 4) == 4)
                            {
                                size.Width = (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
                            }
                            if (fileStream.Read(buffer, 0, 4) == 4)
                            {
                                size.Height = (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
                                success = true;
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    if (!success)
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);

                        if (fileStream.Read(buffer, 0, 3) == 3 &&
                            CompareBytesUnsafe(buffer, GIF_HEADER, 3))
                        {
                            // GIF

                            fileStream.Seek(3, SeekOrigin.Current);// 87a / 89a

                            if (fileStream.Read(buffer, 0, 4) == 4)
                            {
                                size.Width = (buffer[1] << 8) | buffer[0];
                                size.Height = (buffer[3] << 8) | buffer[2];
                                success = true;
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    if (!success)
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);

                        if (fileStream.Read(buffer, 0, 2) == 2 &&
                            CompareBytesUnsafe(buffer, BMP_HEADER, 2))
                        {
                            // BMP

                            fileStream.Seek(16, SeekOrigin.Current);

                            if (fileStream.Read(buffer, 0, 4) == 4)
                            {
                                size.Width = (buffer[3] << 24) | (buffer[2] << 16) | (buffer[1] << 8) | buffer[0];
                            }
                            if (fileStream.Read(buffer, 0, 4) == 4)
                            {
                                size.Height = (buffer[3] << 24) | (buffer[2] << 16) | (buffer[1] << 8) | buffer[0];
                                success = true;
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    if (!success)
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);

                        // TIFF starts with just plain EXIF
                        size = GetImageSize_EXIF(fileStream);
                        if (!size.IsEmpty)
                        {
                            success = true;
                        }
                    }
                }
                catch { }
            }

            if (!success)
            {
                using (Image image = Image.FromFile(path))
                {
                    return image.Size;
                }
            }

            return size;
        }

        #region Bytes compare functions

        private static bool CompareBytesUnsafe(byte[] leftBytes, byte[] rightBytes)
        {
            if (leftBytes.Length != rightBytes.Length) return false;
            return CompareBytesUnsafe(leftBytes, rightBytes, leftBytes.Length);
        }
        private static unsafe bool CompareBytesUnsafe(byte[] leftBytes, byte[] rightBytes, int length)
        {
            if (leftBytes == null || rightBytes == null || leftBytes.Length < length || rightBytes.Length < length)
                return false;
            fixed (byte* p1 = leftBytes, p2 = rightBytes)
            {
                byte* x1 = p1, x2 = p2;
                for (int i = 0; i < length / 8; i++, x1 += 8, x2 += 8)
                {
                    if (*((long*)x1) != *((long*)x2))
                    {
                        return false;
                    }
                }
                if ((length & 4) != 0)
                {
                    if (*((int*)x1) != *((int*)x2))
                    {
                        return false;
                    }
                    x1 += 4; x2 += 4;
                }
                if ((length & 2) != 0)
                {
                    if (*((short*)x1) != *((short*)x2))
                    {
                        return false;
                    }
                    x1 += 2;
                    x2 += 2;
                }
                if ((length & 1) != 0)
                {
                    if (*((byte*)x1) != *((byte*)x2))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        #endregion
    }
}
