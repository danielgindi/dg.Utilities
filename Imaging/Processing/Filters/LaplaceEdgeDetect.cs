using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace dg.Utilities.Imaging.Filters
{
    public class LaplaceEdgeDetect : ConvolutionMatrix
    {
        public new ImageFilterError ProcessImage(
            DirectAccessBitmap bmp,
            params object[] args)
        {
            Matrix5x5 kernel = new Matrix5x5(
                -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1,
                -1, -1, 24, -1, -1,
                -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1,
                1, 0, false);
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
