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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserProfilePicTransform));
        private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JPEG", ".ICO", };

        /// <summary>Initializes a new instance of the <see cref="UserProfilePicTransform"/> class.</summary>
        public UserProfilePicTransform()
        {
			Logger.LogError(">>> Entering UserProfilePicTransform()");
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
			Logger.LogDebug("UserProfilePicTransform(): this.InterpolationMode → {Arg0}", this.InterpolationMode);
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
			Logger.LogDebug("UserProfilePicTransform(): this.SmoothingMode → {Arg0}", this.SmoothingMode);
        Logger.LogWarning("<<< Exiting UserProfilePicTransform()");
            this.CompositingQuality = CompositingQuality.HighQuality;
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
			Logger.LogError(">>> Entering ProcessImage(image={Image})", image);
            if (this.TryGetPhotoFile(out var photoFile))
            {
                if (!IsImageExtension(photoFile.Extension))
                {
                    return this.GetNoAvatarImage();
                }

                using var content = FileManager.Instance.GetFileContent(photoFile);
				Logger.LogError("ProcessImage(image={Image}): content → {Content}", image, content);
                return this.CopyImage(content);
            }

			Logger.LogWarning("<<< Exiting ProcessImage(image={Image})", image);
            return this.GetNoAvatarImage();
        }

        /// <summary>Get the Bitmap of the No Avatar Image.</summary>
        /// <returns>A <see cref="Bitmap"/> of the No Avatar image.</returns>
        public Bitmap GetNoAvatarImage()
        {
			Logger.LogError(">>> Entering GetNoAvatarImage()");
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var avatarAbsolutePath = $@"{Globals.ApplicationMapPath}\images\no_avatar.gif";
			Logger.LogError("GetNoAvatarImage(): avatarAbsolutePath → {AvatarAbsolutePath}", avatarAbsolutePath);
            using (var content = File.OpenRead(avatarAbsolutePath))
            {
				Logger.LogError("GetNoAvatarImage(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
				Logger.LogWarning("<<< Exiting GetNoAvatarImage()");
                return this.CopyImage(content);
            }
        }

        /// <summary>whether current user has permission to view target user's photo.</summary>
        /// <param name="photoFile">The user's profile photo file, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the file was able to be retrieved, otherwise <see langword="false"/>.</returns>
        public bool TryGetPhotoFile(out IFileInfo photoFile)
        {
			Logger.LogError(">>> Entering TryGetPhotoFile()");
            photoFile = null;

			Logger.LogDebug("TryGetPhotoFile(): photoFile → {PhotoFile}", photoFile);
            var settings = PortalController.Instance.GetCurrentSettings();
			Logger.LogError("TryGetPhotoFile(): settings → {Settings}", settings);
			Logger.LogError("TryGetPhotoFile(): photoFile → {PhotoFile}", photoFile);
            var targetUser = UserController.Instance.GetUser(settings.PortalId, this.UserID);
            if (targetUser == null)
            {
				Logger.LogError("TryGetPhotoFile(): targetUser → {TargetUser}", targetUser);
                return false;
            }

            var photoProperty = targetUser.Profile.GetProperty("Photo");
			Logger.LogError("TryGetPhotoFile(): photoProperty → {PhotoProperty}", photoProperty);
            if (photoProperty == null)
            {
                return false;
            }

            var user = UserController.Instance.GetCurrentUserInfo();
			Logger.LogError("TryGetPhotoFile(): user → {User}", user);
            var isVisible = ProfilePropertyAccess.CheckAccessLevel(settings, photoProperty, user, targetUser);
			Logger.LogError("TryGetPhotoFile(): isVisible → {IsVisible}", isVisible);

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

			Logger.LogWarning("<<< Exiting TryGetPhotoFile()");
            return isVisible;
        }

        private static bool IsImageExtension(string extension)
        {
			Logger.LogError(">>> Entering IsImageExtension(extension={Extension})", extension);
            if (!extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = $".{extension}";
				Logger.LogError("IsImageExtension(extension={Extension}): extension → {Extension1}", extension, extension);
            }

			Logger.LogWarning("<<< Exiting IsImageExtension(extension={Extension})", extension);
            return ImageExtensions.Contains(extension);
        }
    }
}
