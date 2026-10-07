// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Net;

    /// <summary>Image File ImageTransform class.</summary>
    public class ImageFileTransform : ImageTransform
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ImageFileTransform));
        /// <summary>Initializes a new instance of the <see cref="ImageFileTransform"/> class.</summary>
        public ImageFileTransform()
        {
			Logger.LogError(">>> Entering ImageFileTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
			Logger.LogDebug("ImageFileTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
			Logger.LogDebug("ImageFileTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
        Logger.LogWarning("<<< Exiting ImageFileTransform()");
            this.CompositingQuality = CompositingQuality.HighQuality;
        }

        /// <summary>Gets provides an Unique String for the image transformation.</summary>
        public override string UniqueString => base.UniqueString + "-" + this.ImageFilePath + this.ImageUrl;

        /// <summary>Gets or sets file path of the image.</summary>
        public string ImageFilePath { get; set; }

        /// <summary>Gets or sets url of the image.</summary>
        public string ImageUrl { get; set; }

        /// <summary>Gets or sets the Image to return if no image or error.</summary>
        public Image EmptyImage { get; set; }

        /// <summary>
        /// Processes an input image applying a file image transformation.
        /// This will return an image after read the stream from the File Path  <see cref="ImageFilePath"/> or Url <see cref="ImageUrl"/>.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after file image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
			Logger.LogError(">>> Entering ProcessImage(image={Image})", image);
			Logger.LogWarning("<<< Exiting ProcessImage(image={Image})", image);
            return !string.IsNullOrEmpty(this.ImageUrl) ?
                this.ProcessImageFromUrl() :
                this.ProcessImageFilePath();
        }

        private Image ProcessImageFilePath()
        {
			Logger.LogError(">>> Entering ProcessImageFilePath()");
            try
            {
                using (var stream = new FileStream(this.ImageFilePath, FileMode.Open))
                {
                    return this.CopyImage(stream);
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in ProcessImageFilePath(): {Arg0}", ex.Message);
                Exceptions.Exceptions.LogException(ex);
				Logger.LogWarning("<<< Exiting ProcessImageFilePath()");
                return this.EmptyImage;
            }
        }

        private Image ProcessImageFromUrl()
        {
			Logger.LogError(">>> Entering ProcessImageFromUrl()");
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.ImageUrl);

			Logger.LogError("ProcessImageFromUrl(): httpWebRequest → {HttpWebRequest}", httpWebRequest);
            try
            {
                using (var httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var stream = httpWebReponse.GetResponseStream())
                    {
                        return this.CopyImage(stream);
                    }
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in ProcessImageFromUrl(): {Arg0}", ex.Message);
                Exceptions.Exceptions.LogException(ex);
				Logger.LogError("ProcessImageFromUrl(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
				Logger.LogWarning("<<< Exiting ProcessImageFromUrl()");
                return this.EmptyImage;
            }
        }
    }
}
