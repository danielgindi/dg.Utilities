using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace dg.Utilities.Imaging.Processing
{
    public enum FilterError
    {
        None = 0,
        OK = 0,
        UnknownError = 1,
        IncompatiblePixelFormat = 2,
        InvalidArgument = 3,
        MissingArgument = 4
    }

    [Flags]
    public enum FilterColorChannel
    {
        None = 0,
        Alpha = 1,
        Red = 2,
        Green = 4,
        Blue = 8,
        Gray = 16,
        RGB = Red | Green | Blue,
        ARGB = Alpha | RGB
    }

    public enum FilterGrayScaleWeight
    {
        None = 0,
        Accurate = None,
        Natural = 1,
        NaturalNTSC = 2
    }
}
