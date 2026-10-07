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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PlaceHolderTransform));
        /// <summary>Initializes a new instance of the <see cref="PlaceholderTransform"/> class.</summary>
        public PlaceholderTransform()
        {
			Logger.LogError(">>> Entering PlaceholderTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
			Logger.LogDebug("PlaceholderTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
			Logger.LogDebug("PlaceholderTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
			Logger.LogDebug("PlaceholderTransform(): this.PixelOffsetMode → {Arg0}", this.PixelOffsetMode);
            this.CompositingQuality = CompositingQuality.HighQuality;
            this.BackColor = Color.LightGray;
            this.Color = Color.LightSlateGray;
            this.Width = 0;
			Logger.LogDebug("PlaceholderTransform(): this.CompositingQuality → {Arg0}", this.CompositingQuality);
            this.Height = 0;
            this.Text = string.Empty;
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
			Logger.LogError(">>> Entering ProcessImage(image={Image})", image);
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
			Logger.LogError("ProcessImage(image={Image}): bitmap → {Bitmap}", image, bitmap);
            Brush backColorBrush = new SolidBrush(this.BackColor);
			Logger.LogError("ProcessImage(image={Image}): backColorBrush → {BackColorBrush}", image, backColorBrush);
            Brush colorBrush = new SolidBrush(this.Color);
			Logger.LogError("ProcessImage(image={Image}): colorBrush → {ColorBrush}", image, colorBrush);
            var colorPen = new Pen(this.Color, 2);
			Logger.LogError("ProcessImage(image={Image}): colorPen → {ColorPen}", image, colorPen);
            var text = string.IsNullOrEmpty(this.Text) ? $"{this.Width}x{this.Height}" : this.Text;
			Logger.LogError("ProcessImage(image={Image}): text → {Text}", image, text);

            using (var objGraphics = Graphics.FromImage(bitmap))
            {
                // Initialize graphics
                objGraphics.Clear(Color.White);
                objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
				Logger.LogDebug("ProcessImage(image={Image}): objGraphics.SmoothingMode → {Arg1}", image, objGraphics.SmoothingMode);
                objGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
				Logger.LogDebug("ProcessImage(image={Image}): objGraphics.TextRenderingHint → {Arg1}", image, objGraphics.TextRenderingHint);

                // Fill bitmap with backcolor
                objGraphics.FillRectangle(backColorBrush, 0, 0, this.Width, this.Height);

                // Draw border
                objGraphics.DrawRectangle(colorPen, 1, 1, this.Width - 3, this.Height - 3);

                // Determine fontsize
                var fontSize = 13;
				Logger.LogError("ProcessImage(image={Image}): fontSize → {FontSize}", image, fontSize);
                if (this.Width < 101)
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
				Logger.LogError("ProcessImage(image={Image}): font → {Font}", image, font);
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };

                var rectangle = new Rectangle(5, 5, this.Width - 10, this.Height - 10);
				Logger.LogError("ProcessImage(image={Image}): rectangle → {Rectangle}", image, rectangle);
                objGraphics.DrawString(text, font, colorBrush, rectangle, stringFormat);

                // Save indicator to file
                objGraphics.Flush();
            }

			Logger.LogWarning("<<< Exiting ProcessImage(image={Image})", image);
            return bitmap;
        }
    }
}
