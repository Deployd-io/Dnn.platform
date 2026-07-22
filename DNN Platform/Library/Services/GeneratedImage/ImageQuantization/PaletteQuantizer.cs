// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.ImageQuantization
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;

    /// <summary>A <see cref="Quantizer"/> for a given color palette.</summary>
    [CLSCompliant(false)]
    public class PaletteQuantizer : Quantizer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>List of all colors in the palette.</summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected Color[] _colors;

        /// <summary>Lookup table for colors.</summary>
        private readonly Hashtable colorMap;

        /// <summary>Initializes a new instance of the <see cref="PaletteQuantizer"/> class.</summary>
        /// <param name="palette">The color palette to quantize to.</param>
        /// <remarks>Palette quantization only requires a single quantization step.</remarks>
        public PaletteQuantizer(ArrayList palette)
            : base(true)
        {
			Logger.LogDebug(">>> Entering PaletteQuantizer(palette={Palette})", palette);
            this.colorMap = new Hashtable();

			Logger.LogDebug("PaletteQuantizer(palette={Palette}): colorMap → {ColorMap}", palette, colorMap);
            this._colors = new Color[palette.Count];
			Logger.LogDebug("PaletteQuantizer(palette={Palette}): this._colors → {Arg1}", palette, this._colors);
            palette.CopyTo(this._colors);
			Logger.LogDebug("<<< Exiting PaletteQuantizer(palette={Palette})", palette);
        }

        /// <summary>Override this to process the pixel in the second pass of the algorithm.</summary>
        /// <param name="pixel">The pixel to quantize.</param>
        /// <returns>The quantized value.</returns>
        protected override byte QuantizePixel(Color32 pixel)
        {
			Logger.LogDebug(">>> Entering QuantizePixel(pixel={Pixel})", pixel);
            byte colorIndex = 0;
            int colorHash = pixel.ARGB;
			Logger.LogDebug("QuantizePixel(pixel={Pixel}): colorIndex → {ColorIndex}", pixel, colorIndex);

            // Check if the color is in the lookup table
            if (this.colorMap.ContainsKey(colorHash))
            {
                colorIndex = (byte)this.colorMap[colorHash];
            }
            else
            {
                // Not found - loop through the palette and find the nearest match.
                // Firstly check the alpha value - if 0, lookup the transparent color
                if (pixel.Alpha == 0)
                {
                    // Transparent. Lookup the first color with an alpha value of 0
                    for (int index = 0; index < this._colors.Length; index++)
                    {
                        if (this._colors[index].A == 0)
                        {
                            colorIndex = (byte)index;
                            break;
                        }
                    }
                }
                else
                {
                    // Not transparent...
                    int leastDistance = int.MaxValue;
                    int red = pixel.Red;
					Logger.LogDebug("QuantizePixel(pixel={Pixel}): leastDistance → {LeastDistance}", pixel, leastDistance);
                    int green = pixel.Green;
                    int blue = pixel.Blue;

                    // Loop through the entire palette, looking for the closest color match
                    for (int index = 0; index < this._colors.Length; index++)
                    {
                        Color paletteColor = this._colors[index];

                        int redDistance = paletteColor.R - red;
                        int greenDistance = paletteColor.G - green;
                        int blueDistance = paletteColor.B - blue;

                        int distance = (redDistance * redDistance) +
                                           (greenDistance * greenDistance) +
                                           (blueDistance * blueDistance);

                        if (distance < leastDistance)
                        {
                            colorIndex = (byte)index;
                            leastDistance = distance;

                            // And if it's an exact match, exit the loop
                            if (distance == 0)
                            {
                                break;
                            }
                        }
                    }
                }

                // Now I have the color, pop it into the hashtable for next time
                this.colorMap.Add(colorHash, colorIndex);
            }

			Logger.LogDebug("<<< Exiting QuantizePixel(pixel={Pixel})", pixel);
            return colorIndex;
        }

        /// <summary>Retrieve the palette for the quantized image.</summary>
        /// <param name="palette">Any old palette, this is overwritten.</param>
        /// <returns>The new color palette.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        protected override ColorPalette GetPalette(ColorPalette palette)
        {
			Logger.LogDebug(">>> Entering GetPalette(palette={Palette})", palette);
            for (int index = 0; index < this._colors.Length; index++)
            {
                palette.Entries[index] = this._colors[index];
            }

			Logger.LogDebug("<<< Exiting GetPalette(palette={Palette})", palette);
            return palette;
        }
    }
}
