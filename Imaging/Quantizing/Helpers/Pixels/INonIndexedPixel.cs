using System;
using System.Drawing;

namespace dg.Utilities.Imaging.Quantizers.Helpers.Pixels
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    [CLSCompliant(false)]
    public interface INonIndexedPixel
    {
        // components
        Int32 Alpha { get; }
        Int32 Red { get; }
        Int32 Green { get; }
        Int32 Blue { get; }

        // higher-level values
        Int32 Argb { get; }
        [CLSCompliant(false)]
        UInt64 Value { get; set; }

        // color methods
        Color GetColor();
        void SetColor(Color color);
    }
}
