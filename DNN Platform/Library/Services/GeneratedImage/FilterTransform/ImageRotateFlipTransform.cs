// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>Rotation ImageTransform class.</summary>
    public class ImageRotateFlipTransform : ImageTransform
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ImageRotateFlipTransform"/> class.</summary>
        public ImageRotateFlipTransform()
        {
			Logger.LogDebug(">>> Entering ImageRotateFlipTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
			Logger.LogDebug("ImageRotateFlipTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
			Logger.LogDebug("ImageRotateFlipTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
            this.RotateFlip = RotateFlipType.RotateNoneFlipNone;
			Logger.LogDebug("<<< Exiting ImageRotateFlipTransform()");
			Logger.LogDebug("ImageRotateFlipTransform(): this.PixelOffsetMode → {Arg0}", this.PixelOffsetMode);
			Logger.LogDebug("ImageRotateFlipTransform(): this.CompositingQuality → {Arg0}", this.CompositingQuality);
			Logger.LogDebug("ImageRotateFlipTransform(): this.RotateFlip → {Arg0}", this.RotateFlip);
        }

        /// <summary>Gets provides an Unique String for this transformation.</summary>
        public override string UniqueString => base.UniqueString + "-" + this.RotateFlip;

        /// <summary>Gets or sets the type of rotation / flip . Defaultvalue is RotateNoneFlipNone.</summary>
        public RotateFlipType RotateFlip { get; set; }

        /// <summary>Processes an input image applying a rotation image transformation.</summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
			Logger.LogDebug(">>> Entering ProcessImage(image={Image})", image);
            var temp = (Bitmap)image;
            var bmap = (Bitmap)temp.Clone();
            bmap.RotateFlip(this.RotateFlip);
			Logger.LogDebug("ProcessImage(image={Image}): bmap → {Bmap}", image, bmap);
			Logger.LogDebug("<<< Exiting ProcessImage(image={Image})", image);
            return (Bitmap)bmap.Clone();
        }
    }
}
