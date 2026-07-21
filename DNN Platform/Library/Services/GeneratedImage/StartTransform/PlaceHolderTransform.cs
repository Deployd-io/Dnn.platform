// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;

    /// <summary>Placeholder ImageTransform class.</summary>
    public class PlaceholderTransform : ImageTransform
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="PlaceholderTransform"/> class.</summary>
        public PlaceholderTransform()
        {
			Logger.LogDebug(">>> Entering PlaceholderTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
			Logger.LogDebug("PlaceholderTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
			Logger.LogDebug("PlaceholderTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
            this.BackColor = Color.LightGray;
			Logger.LogDebug("PlaceholderTransform(): this.PixelOffsetMode → {Arg0}", this.PixelOffsetMode);
			Logger.LogDebug("PlaceholderTransform(): this.CompositingQuality → {Arg0}", this.CompositingQuality);
			Logger.LogDebug("PlaceholderTransform(): this.BackColor → {Arg0}", this.BackColor);
            this.Color = Color.LightSlateGray;
            this.Width = 0;
            this.Height = 0;
            this.Text = string.Empty;
			Logger.LogDebug("<<< Exiting PlaceholderTransform()");
			Logger.LogDebug("PlaceholderTransform(): this.Color → {Arg0}", this.Color);
			Logger.LogDebug("PlaceholderTransform(): this.Width → {Arg0}", this.Width);
			Logger.LogDebug("PlaceholderTransform(): this.Height → {Arg0}", this.Height);
			Logger.LogDebug("PlaceholderTransform(): this.Text → {Arg0}", this.Text);
        }

        /// <summary>Gets provides an Unique String for the image transformation.</summary>
        public override string UniqueString => base.UniqueString + this.Width + "-" + this.Height + "-" + this.Color + "-" + this.BackColor + "-" + this.Text;

        /// <summary>Gets or sets the width of the placeholder image.</summary>
        public int Width { get; set; }

        /// <summary>Gets or sets the Height of the placeholder image.</summary>
        public int Height { get; set; }

        /// <summary>Gets or sets the Color of the border and text element.</summary>
        public Color Color { get; set; }

        /// <summary>Gets or sets the backcolor of the placeholder element.</summary>
        public Color BackColor { get; set; }

        /// <summary>Gets or sets the text of the placeholder image. if blank dimension will be used.</summary>
        public string Text { get; set; }

        /// <summary>Processes an input image returning a placeholder image.</summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
			Logger.LogDebug(">>> Entering ProcessImage(image={Image})", image);
            // Check dimensions
            if (this.Width == 0 && this.Height > 0)
            {
                this.Width = this.Height;
				Logger.LogDebug("ProcessImage(image={Image}): this.Width → {Arg1}", image, this.Width);
            }

            if (this.Width > 0 && this.Height == 0)
            {
                this.Height = this.Width;
				Logger.LogDebug("ProcessImage(image={Image}): this.Height → {Arg1}", image, this.Height);
            }

            var bitmap = new Bitmap(this.Width, this.Height);
            Brush backColorBrush = new SolidBrush(this.BackColor);
			Logger.LogDebug("ProcessImage(image={Image}): bitmap → {Bitmap}", image, bitmap);
			Logger.LogDebug("ProcessImage(image={Image}): backColorBrush → {BackColorBrush}", image, backColorBrush);
            Brush colorBrush = new SolidBrush(this.Color);
            var colorPen = new Pen(this.Color, 2);
			Logger.LogDebug("ProcessImage(image={Image}): colorBrush → {ColorBrush}", image, colorBrush);
			Logger.LogDebug("ProcessImage(image={Image}): colorPen → {ColorPen}", image, colorPen);
            var text = string.IsNullOrEmpty(this.Text) ? $"{this.Width}x{this.Height}" : this.Text;

			Logger.LogDebug("ProcessImage(image={Image}): text → {Text}", image, text);
            using (var objGraphics = Graphics.FromImage(bitmap))
            {
                // Initialize graphics
                objGraphics.Clear(Color.White);
                objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
				Logger.LogDebug("ProcessImage(image={Image}): objGraphics.SmoothingMode → {Arg1}", image, objGraphics.SmoothingMode);
				Logger.LogDebug("ProcessImage(image={Image}): objGraphics.TextRenderingHint → {Arg1}", image, objGraphics.TextRenderingHint);
                objGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                // Fill bitmap with backcolor
                objGraphics.FillRectangle(backColorBrush, 0, 0, this.Width, this.Height);

                // Draw border
                objGraphics.DrawRectangle(colorPen, 1, 1, this.Width - 3, this.Height - 3);

                // Determine fontsize
                var fontSize = 13;
                if (this.Width < 101)
				Logger.LogDebug("ProcessImage(image={Image}): fontSize → {FontSize}", image, fontSize);
                {
                    fontSize = 8;
                }
                else if (this.Width < 151)
                {
                    fontSize = 10;
                }
                else if (this.Width < 201)
                {
                    fontSize = 12;
                }
                else if (this.Width < 301)
                {
                    fontSize = 14;
                }
                else
                {
                    fontSize = 24;
                }

                // Draw text on image
                // Use rectangle for text and align text to center of rectangle
                var font = new Font("Arial", fontSize, FontStyle.Bold);
                var stringFormat = new StringFormat
				Logger.LogDebug("ProcessImage(image={Image}): font → {Font}", image, font);
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };

                var rectangle = new Rectangle(5, 5, this.Width - 10, this.Height - 10);
                objGraphics.DrawString(text, font, colorBrush, rectangle, stringFormat);
				Logger.LogDebug("ProcessImage(image={Image}): rectangle → {Rectangle}", image, rectangle);

                // Save indicator to file
                objGraphics.Flush();
            }

			Logger.LogDebug("<<< Exiting ProcessImage(image={Image})", image);
            return bitmap;
        }
    }
}
