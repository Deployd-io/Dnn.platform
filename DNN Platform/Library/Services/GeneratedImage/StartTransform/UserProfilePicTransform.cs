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
    using System.Globalization;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;

    /// <summary>User Profile Picture ImageTransform class.</summary>
    public class UserProfilePicTransform : ImageTransform
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JPEG", ".ICO", };

        /// <summary>Initializes a new instance of the <see cref="UserProfilePicTransform"/> class.</summary>
        public UserProfilePicTransform()
        {
			Logger.LogDebug(">>> Entering UserProfilePicTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
			Logger.LogDebug("UserProfilePicTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
			Logger.LogDebug("UserProfilePicTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
			Logger.LogDebug("<<< Exiting UserProfilePicTransform()");
			Logger.LogDebug("UserProfilePicTransform(): this.PixelOffsetMode → {Arg0}", this.PixelOffsetMode);
			Logger.LogDebug("UserProfilePicTransform(): this.CompositingQuality → {Arg0}", this.CompositingQuality);
        }

        /// <summary>Gets provides a Unique String for the image transformation.</summary>
        public override string UniqueString => base.UniqueString + this.UserID;

        /// <summary>Gets a value indicating whether is reusable.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool IsReusable => false;

        /// <summary>Gets or sets the UserID of the profile pic.</summary>
        public int UserID { get; set; }

        /// <inheritdoc />
        public override Image ProcessImage(Image image)
        {
			Logger.LogDebug(">>> Entering ProcessImage(image={Image})", image);
            if (this.TryGetPhotoFile(out var photoFile))
            {
                if (!IsImageExtension(photoFile.Extension))
                {
                    return this.GetNoAvatarImage();
                }

                using var content = FileManager.Instance.GetFileContent(photoFile);
                return this.CopyImage(content);
            }

			Logger.LogDebug("<<< Exiting ProcessImage(image={Image})", image);
            return this.GetNoAvatarImage();
        }

        /// <summary>Get the Bitmap of the No Avatar Image.</summary>
        /// <returns>A <see cref="Bitmap"/> of the No Avatar image.</returns>
        public Bitmap GetNoAvatarImage()
        {
			Logger.LogDebug(">>> Entering GetNoAvatarImage()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var avatarAbsolutePath = $@"{Globals.ApplicationMapPath}\images\no_avatar.gif";
            using (var content = File.OpenRead(avatarAbsolutePath))
            {
			Logger.LogDebug("GetNoAvatarImage(): avatarAbsolutePath → {AvatarAbsolutePath}", avatarAbsolutePath);
				Logger.LogInformation("GetNoAvatarImage(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting GetNoAvatarImage()");
                return this.CopyImage(content);
            }
        }

        /// <summary>whether current user has permission to view target user's photo.</summary>
        /// <param name="photoFile">The user's profile photo file, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the file was able to be retrieved, otherwise <see langword="false"/>.</returns>
        public bool TryGetPhotoFile(out IFileInfo photoFile)
        {
			Logger.LogDebug(">>> Entering TryGetPhotoFile()");
            photoFile = null;

			Logger.LogDebug("TryGetPhotoFile(): photoFile → {PhotoFile}", photoFile);
            var settings = PortalController.Instance.GetCurrentSettings();
            var targetUser = UserController.Instance.GetUser(settings.PortalId, this.UserID);
			Logger.LogDebug("TryGetPhotoFile(): settings → {Settings}", settings);
            if (targetUser == null)
			Logger.LogDebug("TryGetPhotoFile(): targetUser → {TargetUser}", targetUser);
            {
                return false;
            }

            var photoProperty = targetUser.Profile.GetProperty("Photo");
            if (photoProperty == null)
			Logger.LogDebug("TryGetPhotoFile(): photoProperty → {PhotoProperty}", photoProperty);
            {
                return false;
            }

            var user = UserController.Instance.GetCurrentUserInfo();
            var isVisible = ProfilePropertyAccess.CheckAccessLevel(settings, photoProperty, user, targetUser);
			Logger.LogDebug("TryGetPhotoFile(): user → {User}", user);
			Logger.LogDebug("TryGetPhotoFile(): isVisible → {IsVisible}", isVisible);

            if (!string.IsNullOrEmpty(photoProperty.PropertyValue) && isVisible)
            {
                photoFile = FileManager.Instance.GetFile(int.Parse(photoProperty.PropertyValue, CultureInfo.InvariantCulture));
                if (photoFile == null)
                {
                    isVisible = false;
                }
            }
            else
            {
                isVisible = false;
            }

			Logger.LogDebug("<<< Exiting TryGetPhotoFile()");
            return isVisible;
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
    }
}
