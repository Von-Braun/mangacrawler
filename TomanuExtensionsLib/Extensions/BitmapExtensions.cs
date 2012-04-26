using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace TomanuExtensions
{
    public static class BitmapExtensions
    {
        public static void SaveJPEG(this Bitmap a_bitmap, Stream a_stream, int a_quality)
        {
            Debug.Assert(a_quality > 0);
            Debug.Assert(a_quality <= 100);

            var eps = new System.Drawing.Imaging.EncoderParameters(1);
            eps.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                System.Drawing.Imaging.Encoder.Quality, 
                (long)a_quality);
            a_bitmap.Save(a_stream, 
                          System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders().FirstOrDefault(
                              c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid), 
                          eps);
        }
    }
}
