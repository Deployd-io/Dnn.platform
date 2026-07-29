// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>Brightness ImageTransform class.</summary>
    public class ImageBrightnessTransform : ImageTransform
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ImageBrightnessTransform"/> class.</summary>
        public ImageBrightnessTransform()
        {
			Logger.LogDebug(">>> Entering ImageBrightnessTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
			Logger.LogDebug("ImageBrightnessTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
			Logger.LogDebug("ImageBrightnessTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
            this.Brightness = 0;
			Logger.LogDebug("<<< Exiting ImageBrightnessTransform()");
			Logger.LogDebug("ImageBrightnessTransform(): this.PixelOffsetMode → {Arg0}", this.PixelOffsetMode);
			Logger.LogDebug("ImageBrightnessTransform(): this.CompositingQuality → {Arg0}", this.CompositingQuality);
			Logger.LogDebug("ImageBrightnessTransform(): this.Brightness → {Arg0}", this.Brightness);
        }

        /// <summary>Gets provides an Unique String for this class.</summary>
        public override string UniqueString => base.UniqueString + "-" + this.Brightness;

        /// <summary>Gets or sets the brightness value. Defaultvalue is 0. Range is -255 .. 255.</summary>
        public int Brightness { get; set; }

        /// <summary>Processes an input image applying a brightness image transformation.</summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
			Logger.LogDebug(">>> Entering ProcessImage(image={Image})", image);
            var temp = (Bitmap)image;
            var bmap = (Bitmap)temp.Clone();
            if (this.Brightness < -255)
			Logger.LogDebug("ProcessImage(image={Image}): bmap → {Bmap}", image, bmap);
            {
                this.Brightness = -255;
				Logger.LogDebug("ProcessImage(image={Image}): this.Brightness → {Arg1}", image, this.Brightness);
            }

            if (this.Brightness > 255)
            {
                this.Brightness = 255;
            }

            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R + this.Brightness;
					Logger.LogDebug("ProcessImage(image={Image}): c → {C}", image, c);
					Logger.LogDebug("ProcessImage(image={Image}): cR → {CR}", image, cR);
                    int cG = c.G + this.Brightness;
                    int cB = c.B + this.Brightness;
					Logger.LogDebug("ProcessImage(image={Image}): cG → {CG}", image, cG);
					Logger.LogDebug("ProcessImage(image={Image}): cB → {CB}", image, cB);

                    if (cR < 0)
                    {
                        cR = 1;
                    }

                    if (cR > 255)
                    {
                        cR = 255;
                    }

                    if (cG < 0)
                    {
                        cG = 1;
                    }

                    if (cG > 255)
                    {
                        cG = 255;
                    }

                    if (cB < 0)
                    {
                        cB = 1;
                    }

                    if (cB > 255)
                    {
                        cB = 255;
                    }

                    bmap.SetPixel(i, j, Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
                }
            }

			Logger.LogDebug("<<< Exiting ProcessImage(image={Image})", image);
            return (Bitmap)bmap.Clone();
        }
    }
}
