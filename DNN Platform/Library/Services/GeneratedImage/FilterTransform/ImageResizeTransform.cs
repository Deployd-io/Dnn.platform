// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>Resize ImageTransform class.</summary>
    public class ImageResizeTransform : ImageTransform
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private int width;
        private int height;
        private int border;
        private int maxWidth;
        private int maxHeight;

        /// <summary>Initializes a new instance of the <see cref="ImageResizeTransform"/> class.</summary>
        public ImageResizeTransform()
        {
			Logger.LogDebug(">>> Entering ImageResizeTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
			Logger.LogDebug("ImageResizeTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
			Logger.LogDebug("ImageResizeTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
            this.Mode = ImageResizeMode.Fit;
			Logger.LogDebug("<<< Exiting ImageResizeTransform()");
			Logger.LogDebug("ImageResizeTransform(): this.PixelOffsetMode → {Arg0}", this.PixelOffsetMode);
			Logger.LogDebug("ImageResizeTransform(): this.CompositingQuality → {Arg0}", this.CompositingQuality);
			Logger.LogDebug("ImageResizeTransform(): this.Mode → {Arg0}", this.Mode);
        }

        /// <summary>Gets provides an Unique String for this transformation.</summary>
        [Browsable(false)]
        public override string UniqueString => base.UniqueString + this.Width + this.InterpolationMode + this.Height + this.Mode;

        /// <summary>Gets or sets the resize mode. The default value is Fit.</summary>
        public ImageResizeMode Mode { get; set; }

        /// <summary>Gets or sets the width of the resulting image.</summary>
        public int Width
        {
            get
            {
                return this.width;
            }

            set
            {
                CheckValue(value);
                this.width = value;
            }
        }

        /// <summary>Gets or sets the Max width of the resulting image.</summary>
        public int MaxWidth
        {
            get
            {
                return this.maxWidth;
            }

            set
            {
                CheckValue(value);
                this.maxWidth = value;
            }
        }

        /// <summary>Gets or sets the height of the resulting image.</summary>
        public int Height
        {
            get
            {
                return this.height;
            }

            set
            {
                CheckValue(value);
                this.height = value;
            }
        }

        /// <summary>Gets or sets the max height of the resulting image.</summary>
        public int MaxHeight
        {
            get
            {
                return this.maxHeight;
            }

            set
            {
                CheckValue(value);
                this.maxHeight = value;
            }
        }

        /// <summary>Gets or sets the border width of the resulting image.</summary>
        public int Border
        {
            get
            {
                return this.border;
            }

            set
            {
                CheckValue(value);
                this.border = value;
            }
        }

        /// <summary>Gets or sets the Backcolor.</summary>
        public Color BackColor { get; set; } = Color.White;

        /// <summary>Processes an input image applying a resize image transformation.</summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
			Logger.LogDebug(">>> Entering ProcessImage(image={Image})", image);
            if (image == null)
            {
                return null;
            }

            if (this.MaxWidth > 0)
            {
                this.Width = image.Width > this.MaxWidth ? this.MaxWidth : image.Width;
				Logger.LogDebug("ProcessImage(image={Image}): this.Width → {Arg1}", image, this.Width);
            }

            if (this.MaxHeight > 0)
            {
                this.Height = image.Height > this.MaxHeight ? this.MaxHeight : image.Height;
				Logger.LogDebug("ProcessImage(image={Image}): this.Height → {Arg1}", image, this.Height);
            }

            int scaledHeight = (int)(image.Height * ((float)this.Width / (float)image.Width));
            int scaledWidth = (int)(image.Width * ((float)this.Height / (float)image.Height));
			Logger.LogDebug("ProcessImage(image={Image}): scaledHeight → {ScaledHeight}", image, scaledHeight);
			Logger.LogDebug("ProcessImage(image={Image}): scaledWidth → {ScaledWidth}", image, scaledWidth);

            Image procImage;
            switch (this.Mode)
            {
                case ImageResizeMode.Fit:
                    procImage = this.FitImage(image, scaledHeight, scaledWidth);
					Logger.LogDebug("ProcessImage(image={Image}): procImage → {ProcImage}", image, procImage);
                    break;
                case ImageResizeMode.Crop:
                    procImage = this.CropImage(image, scaledHeight, scaledWidth);
                    break;
                case ImageResizeMode.FitSquare:
                    procImage = this.FitSquareImage(image);
                    break;
                case ImageResizeMode.Fill:
                    procImage = this.FillImage(image);
                    break;
                default:
                    Debug.Fail("Should not reach this");
                    return null;
            }

			Logger.LogDebug("<<< Exiting ProcessImage(image={Image})", image);
            return procImage;
        }

        /// <inheritdoc />
        public override string ToString()
        {
			Logger.LogDebug(">>> Entering ToString()");
			Logger.LogDebug("<<< Exiting ToString()");
            return "ImageResizeTransform";
        }

        private static void CheckValue(int value)
        {
			Logger.LogDebug(">>> Entering CheckValue(value={Value})", value);
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
			Logger.LogDebug("<<< Exiting CheckValue(value={Value})", value);
        }

        private Bitmap FitImage(Image img, int scaledHeight, int scaledWidth)
        {
            int resizeWidth;
            int resizeHeight;
            if (this.Height == 0)
            {
                resizeWidth = this.Width;
                resizeHeight = scaledHeight;
            }
            else if (this.Width == 0)
            {
                resizeWidth = scaledWidth;
                resizeHeight = this.Height;
            }
            else
            {
                if ((float)this.Width / (float)img.Width < this.Height / (float)img.Height)
                {
                    resizeWidth = this.Width;
                    resizeHeight = scaledHeight;
                }
                else
                {
                    resizeWidth = scaledWidth;
                    resizeHeight = this.Height;
                }
            }

            var newimage = new Bitmap(resizeWidth + (2 * this.border), resizeHeight + (2 * this.border));
            var graphics = Graphics.FromImage(newimage);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = this.CompositingQuality;
            graphics.InterpolationMode = this.InterpolationMode;
            graphics.SmoothingMode = this.SmoothingMode;

            graphics.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(0, 0, resizeWidth + (2 * this.border), resizeHeight + (2 * this.border)));
            graphics.DrawImage(img, this.border, this.border, resizeWidth, resizeHeight);

            return newimage;
        }

        private Bitmap FitSquareImage(Image img)
        {
			Logger.LogDebug(">>> Entering FitSquareImage(img={Img})", img);
            int resizeWidth;
            int resizeHeight;

            int newDim = this.Width > 0 ? this.Width : this.Height;

            if (img.Height > img.Width)
            {
                resizeWidth = Convert.ToInt32((float)img.Width / (float)img.Height * newDim);
				Logger.LogDebug("FitSquareImage(img={Img}): resizeWidth → {ResizeWidth}", img, resizeWidth);
                resizeHeight = newDim;
            }
			Logger.LogDebug("FitSquareImage(img={Img}): resizeHeight → {ResizeHeight}", img, resizeHeight);
            else
            {
                resizeWidth = newDim;
                resizeHeight = Convert.ToInt32((float)img.Height / (float)img.Width * newDim);
            }

            var newImage = new Bitmap(newDim + (2 * this.border), newDim + (2 * this.border));
            var graphics = Graphics.FromImage(newImage);
			Logger.LogDebug("FitSquareImage(img={Img}): newImage → {NewImage}", img, newImage);
			Logger.LogDebug("FitSquareImage(img={Img}): graphics → {Graphics}", img, graphics);
			Logger.LogDebug("FitSquareImage(img={Img}): graphics.CompositingMode → {Arg1}", img, graphics.CompositingMode);
			Logger.LogDebug("FitSquareImage(img={Img}): graphics.CompositingQuality → {Arg1}", img, graphics.CompositingQuality);
			Logger.LogDebug("FitSquareImage(img={Img}): graphics.InterpolationMode → {Arg1}", img, graphics.InterpolationMode);
			Logger.LogDebug("FitSquareImage(img={Img}): graphics.SmoothingMode → {Arg1}", img, graphics.SmoothingMode);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = this.CompositingQuality;
            graphics.InterpolationMode = this.InterpolationMode;
            graphics.SmoothingMode = this.SmoothingMode;

            graphics.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(0, 0, newDim + (2 * this.border), newDim + (2 * this.border)));
            graphics.DrawImage(img, ((newDim - resizeWidth) / 2) + this.border, ((newDim - resizeHeight) / 2) + this.border, resizeWidth, resizeHeight);
			Logger.LogDebug("<<< Exiting FitSquareImage(img={Img})", img);
            return newImage;
        }

        private Bitmap CropImage(Image img, int scaledHeight, int scaledWidth)
        {
            int resizeWidth;
            int resizeHeight;
            if ((float)this.Width / (float)img.Width > this.Height / (float)img.Height)
            {
                resizeWidth = this.Width;
                resizeHeight = scaledHeight;
            }
            else
            {
                resizeWidth = scaledWidth;
                resizeHeight = this.Height;
            }

            var newImage = new Bitmap(this.Width, this.Height);
            var graphics = Graphics.FromImage(newImage);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = this.CompositingQuality;
            graphics.InterpolationMode = this.InterpolationMode;
            graphics.SmoothingMode = this.SmoothingMode;
            graphics.PixelOffsetMode = this.PixelOffsetMode;

            graphics.DrawImage(img, (this.Width - resizeWidth) / 2, (this.Height - resizeHeight) / 2, resizeWidth, resizeHeight);
            return newImage;
        }

        private Bitmap FillImage(Image img)
        {
			Logger.LogDebug(">>> Entering FillImage(img={Img})", img);
            int resizeHeight = this.Height;
            int resizeWidth = this.Width;

            var newImage = new Bitmap(this.Width, this.Height);
            var graphics = Graphics.FromImage(newImage);
			Logger.LogDebug("FillImage(img={Img}): newImage → {NewImage}", img, newImage);
			Logger.LogDebug("FillImage(img={Img}): graphics → {Graphics}", img, graphics);
			Logger.LogDebug("FillImage(img={Img}): graphics.CompositingMode → {Arg1}", img, graphics.CompositingMode);
			Logger.LogDebug("FillImage(img={Img}): graphics.CompositingQuality → {Arg1}", img, graphics.CompositingQuality);
			Logger.LogDebug("FillImage(img={Img}): graphics.InterpolationMode → {Arg1}", img, graphics.InterpolationMode);
			Logger.LogDebug("FillImage(img={Img}): graphics.SmoothingMode → {Arg1}", img, graphics.SmoothingMode);
			Logger.LogDebug("FillImage(img={Img}): graphics.PixelOffsetMode → {Arg1}", img, graphics.PixelOffsetMode);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = this.CompositingQuality;
            graphics.InterpolationMode = this.InterpolationMode;
            graphics.SmoothingMode = this.SmoothingMode;
            graphics.PixelOffsetMode = this.PixelOffsetMode;

            graphics.DrawImage(img, (this.Width - resizeWidth) / 2, (this.Height - resizeHeight) / 2, resizeWidth, resizeHeight);
			Logger.LogDebug("<<< Exiting FillImage(img={Img})", img);
            return newImage;
        }
    }
}
