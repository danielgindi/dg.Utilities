using System;
using dg.Utilities.Imaging.Quantizers.Helpers;
using dg.Utilities.Imaging.Quantizers.PathProviders;
using dg.Utilities.Imaging.Quantizers;

namespace dg.Utilities.Imaging.Quantizers.Ditherers
{
    /// <summary>
    /// Provided by SmartK8 on CodeProject. http://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    /// </summary>
    public interface IColorDitherer : IPathProvider
    {
        /// <summary>
        /// Gets a value indicating whether this ditherer uses only actually process pixel.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this ditherer is inplace; otherwise, <c>false</c>.
        /// </value>
        Boolean IsInplace { get; }

        /// <summary>
        /// Prepares this instance.
        /// </summary>
        void Prepare(IColorQuantizer quantizer, Int32 colorCount, ImageBuffer sourceBuffer, ImageBuffer targetBuffer);

        /// <summary>
        /// Processes the specified buffer.
        /// </summary>
        Boolean ProcessPixel(Pixel sourcePixel, Pixel targetPixel);

        /// <summary>
        /// Finishes this instance.
        /// </summary>
        void Finish();
    }
}
