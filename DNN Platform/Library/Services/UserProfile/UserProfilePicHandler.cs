// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.UserProfile
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization.Internal;

    public class UserProfilePicHandler : IHttpHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserProfilePicHandler));
        private static readonly object ResizeLocker = new object();

        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".JPG",
            ".JPE",
            ".BMP",
            ".GIF",
            ".PNG",
            ".JPEG",
            ".ICO",
        };

        /// <inheritdoc />
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void ProcessRequest(HttpContext context)
        {
			Logger.LogError(">>> Entering ProcessRequest(context={Context})", context);
            SetupCulture();

            var userId = -1;
			Logger.LogError("ProcessRequest(context={Context}): userId → {UserId}", context, userId);
            var height = 55;
			Logger.LogError("ProcessRequest(context={Context}): height → {Height}", context, height);
            try
            {
                if (!string.IsNullOrEmpty(context.Request.QueryString["userid"]))
                {
                    userId = Convert.ToInt32(context.Request.QueryString["userid"], CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(context.Request.QueryString["h"]))
                {
                    height = Convert.ToInt32(context.Request.QueryString["h"], CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in ProcessRequest(context={Context}): {Arg1}", context, Exception.Message);
                Exceptions.Exceptions.ProcessHttpException(context.Request);
            }

            if (height > 128)
            {
                height = 128;
            }

            CalculateSize(ref height, out var width, out var size);

            var settings = PortalController.Instance.GetCurrentSettings();
			Logger.LogError("ProcessRequest(context={Context}): settings → {Settings}", context, settings);
            var user = UserController.Instance.GetUser(settings.PortalId, userId);
			Logger.LogError("ProcessRequest(context={Context}): user → {User}", context, user);

            var photoLoaded = false;
			Logger.LogError("ProcessRequest(context={Context}): photoLoaded → {PhotoLoaded}", context, photoLoaded);
            if (user != null && TryGetPhotoFile(user, out var photoFile))
            {
                if (!IsImageExtension(photoFile.Extension))
                {
                    try
                    {
                        context.Response.End();
                    }
                    catch (ThreadAbortException)
                    {
						Logger.LogError("Exception in ProcessRequest(context={Context}): {Arg1}", context, ThreadAbortException.Message);
                        // if ThreadAbortException will shown, should catch it and do nothing.
                    }
                }

                var folder = FolderManager.Instance.GetFolder(photoFile.FolderId);
				Logger.LogError("ProcessRequest(context={Context}): folder → {Folder}", context, folder);
                var extension = $".{photoFile.Extension}";
				Logger.LogError("ProcessRequest(context={Context}): extension → {Extension}", context, extension);
                var sizedPhoto = photoFile.FileName.Replace(extension, $"_{size}{extension}");
				Logger.LogError("ProcessRequest(context={Context}): sizedPhoto → {SizedPhoto}", context, sizedPhoto);
                if (!FileManager.Instance.FileExists(folder, sizedPhoto))
                {
                    lock (ResizeLocker)
                    {
                        if (!FileManager.Instance.FileExists(folder, sizedPhoto))
                        {
                            using var fileContent = FileManager.Instance.GetFileContent(photoFile);
							Logger.LogError("ProcessRequest(context={Context}): fileContent → {FileContent}", context, fileContent);
                            using var sizedContent = ImageUtils.CreateImage(fileContent, height, width, extension);
							Logger.LogError("ProcessRequest(context={Context}): sizedContent → {SizedContent}", context, sizedContent);

                            const bool operationDoesNotRequirePermissionsCheck = true;
                            FileManager.Instance.AddFile(folder, sizedPhoto, sizedContent, false, !operationDoesNotRequirePermissionsCheck, FileContentTypeManager.Instance.GetContentType(Path.GetExtension(sizedPhoto)));
                        }
                    }
                }

                using var content = FileManager.Instance.GetFileContent(FileManager.Instance.GetFile(folder, sizedPhoto));
				Logger.LogError("ProcessRequest(context={Context}): content → {Content}", context, content);
                context.Response.ContentType =
                    photoFile.Extension.ToLowerInvariant() switch
                    {
                        "png" => "image/png",
                        "jpeg" or "jpg" => "image/jpeg",
                        "gif" => "image/gif",
                        _ => context.Response.ContentType,
                    };

                using (var memoryStream = new MemoryStream())
                {
                    content.CopyTo(memoryStream);
                    memoryStream.WriteTo(context.Response.OutputStream);
                }

                photoLoaded = true;
            }

            if (!photoLoaded)
            {
                context.Response.ContentType = "image/gif";
                context.Response.WriteFile(context.Request.MapPath("~/images/no_avatar.gif"));
            }

            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(1));
            context.Response.Cache.SetMaxAge(new TimeSpan(0, 1, 0));
            context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
            context.ApplicationInstance.CompleteRequest();
			Logger.LogWarning("<<< Exiting ProcessRequest(context={Context})", context);
        }

        // whether current user has permission to view target user's photo.
        private static bool TryGetPhotoFile(UserInfo targetUser, out IFileInfo photoFile)
        {
			Logger.LogError(">>> Entering TryGetPhotoFile(targetUser={TargetUser})", targetUser);
            photoFile = null;

			Logger.LogDebug("TryGetPhotoFile(targetUser={TargetUser}): photoFile → {PhotoFile}", targetUser, photoFile);
            UserInfo user = UserController.Instance.GetCurrentUserInfo();
			Logger.LogError("TryGetPhotoFile(targetUser={TargetUser}): user → {User}", targetUser, user);
            var settings = PortalController.Instance.GetCurrentSettings();
			Logger.LogError("TryGetPhotoFile(targetUser={TargetUser}): settings → {Settings}", targetUser, settings);
			Logger.LogError("TryGetPhotoFile(targetUser={TargetUser}): photoFile → {PhotoFile}", targetUser, photoFile);
            var photoProperty = targetUser.Profile.GetProperty("Photo");
            if (photoProperty == null)
            {
				Logger.LogError("TryGetPhotoFile(targetUser={TargetUser}): photoProperty → {PhotoProperty}", targetUser, photoProperty);
                return false;
            }

            var isVisible = ProfilePropertyAccess.CheckAccessLevel(settings, photoProperty, user, targetUser);
			Logger.LogError("TryGetPhotoFile(targetUser={TargetUser}): isVisible → {IsVisible}", targetUser, isVisible);
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

			Logger.LogWarning("<<< Exiting TryGetPhotoFile(targetUser={TargetUser})", targetUser);
            return isVisible;
        }

        private static void CalculateSize(ref int height, out int width, out string size)
        {
			Logger.LogError(">>> Entering CalculateSize(height={Height})", height);
            switch (height)
            {
                case > 0 and <= 32:
					Logger.LogWarning("<<< Exiting CalculateSize(height={Height})", height);
                    height = 32;
					Logger.LogError("CalculateSize(height={Height}): height → {Height1}", height, height);
                    width = 32;
					Logger.LogError("CalculateSize(height={Height}): width → {Width}", height, width);
					Logger.LogDebug("CalculateSize(height={Height}): width → {Width}", height, width);
                    size = "xs";
					Logger.LogError("CalculateSize(height={Height}): size → {Size}", height, size);
                    break;
                case > 32 and <= 50:
					Logger.LogDebug("CalculateSize(height={Height}): size → {Size}", height, size);
                    height = 50;
                    width = 50;
                    size = "s";
                    break;
                case > 50 and <= 64:
                    height = 64;
                    width = 64;
                    size = "l";
                    break;
                case > 64 and <= 128:
                    height = 128;
                    width = 128;
                    size = "xl";
                    break;
                default:
                    height = 32;
                    width = 32;
                    size = "xs";
                    break;
            }
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

        private static void SetupCulture()
        {
			Logger.LogError(">>> Entering SetupCulture()");
            PortalSettings settings = PortalSettings.Current;
            if (settings is null)
            {
                return;
            }

            CultureInfo pageLocale = TestableLocalization.Instance.GetPageLocale(settings);
			Logger.LogError("SetupCulture(): pageLocale → {PageLocale}", pageLocale);
            if (pageLocale is not null)
            {
                TestableLocalization.Instance.SetThreadCultures(pageLocale, settings);
            }
			Logger.LogWarning("<<< Exiting SetupCulture()");
        }
    }
}
