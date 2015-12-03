using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace dg.Utilities.Imaging
{
    [Flags]
    public enum CropAnchor
    {
        None = 0,
        Top = 0x01,
        Right = 0x02,
        Bottom = 0x04,
        Left = 0x08,
        Center = 0x10,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        TopCenter = Top | Center,
        BottomRight = Bottom | Right,
        BottomLeft = Bottom | Left,
        BottomCenter = Bottom | Center,
        CenterLeft = Center | Left,
        CenterRight = Center | Right,
    }

    [Flags]
    public enum Corner
    {
        None = 0,
        TopLeft = 0x01,
        TopRight = 0x02,
        BottomRight = 0x04,
        BottomLeft = 0x08,
        AllCorners = TopLeft | TopRight | BottomRight | BottomLeft
    }
}
