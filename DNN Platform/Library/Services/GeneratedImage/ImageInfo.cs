// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Drawing;
    using System.Net;

    /// <summary>Image info class.</summary>
    public class ImageInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ImageInfo));
        /// <summary>Initializes a new instance of the <see cref="ImageInfo"/> class.</summary>
        /// <param name="statusCode">The HTTP status code.</param>
        public ImageInfo(HttpStatusCode statusCode)
        {
			Logger.LogDebug(">>> Entering ImageInfo(statusCode={StatusCode})", statusCode);
            this.HttpStatusCode = statusCode;
			Logger.LogDebug("ImageInfo(statusCode={StatusCode}): this.HttpStatusCode → {Arg1}", statusCode, this.HttpStatusCode);
			Logger.LogDebug("<<< Exiting ImageInfo(statusCode={StatusCode})", statusCode);
        }

        /// <summary>Initializes a new instance of the <see cref="ImageInfo"/> class.</summary>
        /// <param name="image">The image.</param>
        public ImageInfo(Image image)
        {
			Logger.LogDebug(">>> Entering ImageInfo(image={Image})", image);
            if (image == null)
            {
				Logger.LogWarning("Throwing ArgumentNullException from ImageInfo(image={Image})", image);
                throw new ArgumentNullException(nameof(image));
            }

            this.Image = image;
			Logger.LogDebug("<<< Exiting ImageInfo(image={Image})", image);
        }

        /// <summary>Initializes a new instance of the <see cref="ImageInfo"/> class.</summary>
        /// <param name="imageBuffer">An array of bytes representing the contents of the image.</param>
        public ImageInfo(byte[] imageBuffer)
        {
			Logger.LogDebug(">>> Entering ImageInfo(imageBuffer={ImageBuffer})", imageBuffer);
            if (imageBuffer == null)
            {
				Logger.LogWarning("Throwing ArgumentNullException from ImageInfo(imageBuffer={ImageBuffer})", imageBuffer);
                throw new ArgumentNullException(nameof(imageBuffer));
            }

            this.ImageByteBuffer = imageBuffer;
			Logger.LogDebug("<<< Exiting ImageInfo(imageBuffer={ImageBuffer})", imageBuffer);
        }

        /// <summary>Gets image.</summary>
        public Image Image { get; private set; }

        /// <summary>Gets image byte buffer.</summary>
        public byte[] ImageByteBuffer { get; private set; }

        /// <summary>Gets http status code.</summary>
        public HttpStatusCode? HttpStatusCode { get; private set; }

        public bool IsEmptyImage { get; set; }
    }
}
