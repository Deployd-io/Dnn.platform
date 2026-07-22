// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;

    /// <summary>Secure File ImageTransform class.</summary>
    public class SecureFileTransform : ImageTransform
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JPEG", ".ICO", };

        /// <summary>Initializes a new instance of the <see cref="SecureFileTransform"/> class.</summary>
        public SecureFileTransform()
        {
			Logger.LogDebug(">>> Entering SecureFileTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
			Logger.LogDebug("SecureFileTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
			Logger.LogDebug("SecureFileTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
			Logger.LogDebug("<<< Exiting SecureFileTransform()");
			Logger.LogDebug("SecureFileTransform(): this.PixelOffsetMode → {Arg0}", this.PixelOffsetMode);
			Logger.LogDebug("SecureFileTransform(): this.CompositingQuality → {Arg0}", this.CompositingQuality);
        }

        /// <summary>Gets provides an Unique String for the image transformation.</summary>
        public override string UniqueString => base.UniqueString + this.SecureFile.FileId;

        /// <summary>Gets or sets set IFileInfo object of given FileId.</summary>
        public IFileInfo SecureFile { get; set; }

        /// <summary>Gets or sets the Image to return if no image or error.</summary>
        public Image EmptyImage { get; set; }

        /// <summary>Processes an input image returing a secure file image.</summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        /// <remarks>
        /// If the secure file is not an image, it returns an image representing the file extension.
        /// </remarks>
        public override Image ProcessImage(Image image)
        {
			Logger.LogDebug(">>> Entering ProcessImage(image={Image})", image);
            // if SecureFile is no ImageFile return FileType-Image instead
            if (!IsImageExtension(this.SecureFile.Extension))
            {
                return this.GetSecureFileExtensionIconImage();
            }

            using var content = FileManager.Instance.GetFileContent(this.SecureFile);
			Logger.LogDebug("ProcessImage(image={Image}): content → {Content}", image, content);
			Logger.LogDebug("<<< Exiting ProcessImage(image={Image})", image);
            return this.CopyImage(content);
        }

        /// <summary>Checks if the current user have READ permission on a given folder.</summary>
        /// <param name="folder">Folder info object.</param>
        /// <returns>True if the user has READ permission, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool DoesHaveReadFolderPermission(IFolderInfo folder)
        {
			Logger.LogDebug(">>> Entering DoesHaveReadFolderPermission(folder={Folder})", folder);
			Logger.LogDebug("<<< Exiting DoesHaveReadFolderPermission(folder={Folder})", folder);
            return FolderPermissionController.HasFolderPermission(folder.FolderPermissions, "Read");
        }

        private static bool IsImageExtension(string extension)
        {
			Logger.LogDebug(">>> Entering IsImageExtension(extension={Extension})", extension);
            if (!extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = $".{extension}";
            }
			Logger.LogDebug("IsImageExtension(extension={Extension}): extension → {Extension1}", extension, extension);

			Logger.LogDebug("<<< Exiting IsImageExtension(extension={Extension})", extension);
            return ImageExtensions.Contains(extension);
        }

        private Image GetSecureFileExtensionIconImage()
        {
			Logger.LogDebug(">>> Entering GetSecureFileExtensionIconImage()");
            var iconLocation = PortalSettings.Current.DefaultIconLocation.Replace("/", @"\");
            var extensionImageAbsolutePath = $@"{Globals.ApplicationMapPath}\{iconLocation}\Ext{this.SecureFile.Extension}_32x32_Standard.png";
			Logger.LogDebug("GetSecureFileExtensionIconImage(): iconLocation → {IconLocation}", iconLocation);
			Logger.LogDebug("GetSecureFileExtensionIconImage(): extensionImageAbsolutePath → {ExtensionImageAbsolutePath}", extensionImageAbsolutePath);

            if (!File.Exists(extensionImageAbsolutePath))
            {
                return this.EmptyImage;
            }

            using var stream = new FileStream(extensionImageAbsolutePath, FileMode.Open);
			Logger.LogDebug("GetSecureFileExtensionIconImage(): stream → {Stream}", stream);
			Logger.LogDebug("<<< Exiting GetSecureFileExtensionIconImage()");
            return this.CopyImage(stream);
        }
    }
}
