// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>Grey Scale ImageTransform class.</summary>
    public class ImageGreyScaleTransform : ImageTransform
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ImageGreyScaleTransform"/> class.</summary>
        public ImageGreyScaleTransform()
        {
			Logger.LogDebug(">>> Entering ImageGreyScaleTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
			Logger.LogDebug("ImageGreyScaleTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
			Logger.LogDebug("ImageGreyScaleTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
			Logger.LogDebug("<<< Exiting ImageGreyScaleTransform()");
			Logger.LogDebug("ImageGreyScaleTransform(): this.PixelOffsetMode → {Arg0}", this.PixelOffsetMode);
			Logger.LogDebug("ImageGreyScaleTransform(): this.CompositingQuality → {Arg0}", this.CompositingQuality);
        }

        /// <summary>Processes an input image applying a grey scale image transformation.</summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
			Logger.LogDebug(">>> Entering ProcessImage(image={Image})", image);
            var temp = (Bitmap)image;
            var bmap = (Bitmap)temp.Clone();
            Color c;
			Logger.LogDebug("ProcessImage(image={Image}): bmap → {Bmap}", image, bmap);
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    byte gray = (byte)((.299 * c.R) + (.587 * c.G) + (.114 * c.B));
					Logger.LogDebug("ProcessImage(image={Image}): c → {C}", image, c);
					Logger.LogDebug("ProcessImage(image={Image}): gray → {Gray}", image, gray);

                    bmap.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }

			Logger.LogDebug("<<< Exiting ProcessImage(image={Image})", image);
            return (Bitmap)bmap.Clone();
        }
    }
}
