// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.ImageQuantization
{
    using System;
    using System.Collections;
    using System.Drawing;

    /// <summary>A <see cref="PaletteQuantizer"/> for grayscale.</summary>
    [CLSCompliant(false)]
    public class GrayscaleQuantizer : PaletteQuantizer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GrayscaleQuantizer));
        /// <summary>Initializes a new instance of the <see cref="GrayscaleQuantizer"/> class.</summary>
        /// <remarks>Palette quantization only requires a single quantization step.</remarks>
        public GrayscaleQuantizer()
            : base(new ArrayList())
        {
			Logger.LogDebug(">>> Entering GrayscaleQuantizer()");
            this._colors = new Color[256];

            int nColors = 256;

            // Initialize a new color table with entries that are determined
            // by some optimal palette-finding algorithm; for demonstration
            // purposes, use a grayscale.
            for (uint i = 0; i < nColors; i++)
            {
                uint alpha = 0xFF;                      // Colors are opaque.
                uint intensity = Convert.ToUInt32(i * 0xFF / (nColors - 1));    // Even distribution.

                // The GIF encoder makes the first entry in the palette
                // that has a ZERO alpha the transparent color in the GIF.
                // Pick the first one arbitrarily, for demonstration purposes.

                // Create a gray scale for demonstration purposes.
                // Otherwise, use your favorite color reduction algorithm
                // and an optimum palette for that algorithm generated here.
                // For example, a color histogram, or a median cut palette.
                this._colors[i] = Color.FromArgb(
                    (int)alpha,
                    (int)intensity,
                    (int)intensity,
                    (int)intensity);
            }
			Logger.LogDebug("<<< Exiting GrayscaleQuantizer()");
        }

        /// <summary>Override this to process the pixel in the second pass of the algorithm.</summary>
        /// <param name="pixel">The pixel to quantize.</param>
        /// <returns>The quantized value.</returns>
        protected override byte QuantizePixel(Color32 pixel)
        {
			Logger.LogDebug(">>> Entering QuantizePixel(pixel={Pixel})", pixel);
            double luminance = (pixel.Red * 0.299) + (pixel.Green * 0.587) + (pixel.Blue * 0.114);

			Logger.LogDebug("QuantizePixel(pixel={Pixel}): luminance → {Luminance}", pixel, luminance);
            // Gray scale is an intensity map from black to white.
            // Compute the index to the grayscale entry that
            // approximates the luminance, and then round the index.
            // Also, constrain the index choices by the number of
            // colors to do, and then set that pixel's index to the
            // byte value.
            var colorIndex = (byte)(luminance + 0.5);
			Logger.LogDebug("QuantizePixel(pixel={Pixel}): colorIndex → {ColorIndex}", pixel, colorIndex);

			Logger.LogDebug("<<< Exiting QuantizePixel(pixel={Pixel})", pixel);
            return colorIndex;
        }
    }
}
