// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>Invert ImageTransform class.</summary>
    public class ImageInvertTransform : ImageTransform
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ImageInvertTransform));
        /// <summary>Initializes a new instance of the <see cref="ImageInvertTransform"/> class.</summary>
        public ImageInvertTransform()
        {
			Logger.LogDebug(">>> Entering ImageInvertTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
			Logger.LogDebug("ImageInvertTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
			Logger.LogDebug("ImageInvertTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
			Logger.LogDebug("<<< Exiting ImageInvertTransform()");
        }

        /// <summary>Processes an input image applying an invert image transformation.</summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
			Logger.LogDebug(">>> Entering ProcessImage(image={Image})", image);
            var temp = (Bitmap)image;
            var bmap = (Bitmap)temp.Clone();
			Logger.LogDebug("ProcessImage(image={Image}): bmap → {Bmap}", image, bmap);
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
					Logger.LogDebug("ProcessImage(image={Image}): c → {C}", image, c);
                    bmap.SetPixel(i, j, Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B));
                }
            }

			Logger.LogDebug("<<< Exiting ProcessImage(image={Image})", image);
            return (Bitmap)bmap.Clone();
        }
    }
}
