using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace dg.Utilities.Imaging.Processing.Filters
{
    public interface IImageFilter
    {
        FilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args);
    }
}
