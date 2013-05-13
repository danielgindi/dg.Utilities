using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace dg.Utilities.Imaging.Filters
{
    public class Sharpen : ConvolutionMatrix
    {
        public new ImageFilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            Matrix3x3 kernel = new Matrix3x3(
                0, -1, 0,
                -1, 5, -1,
                0, -1, 0,
                1, 0, true);
            Channel channels = Channel.None;

            foreach (object arg in args)
            {
                if (arg is Channel)
                {
                    channels |= (Channel)arg;
                }
            }

            return base.ProcessImage(bmp, kernel, channels);
        }
    }
}
