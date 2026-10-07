// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.GeneratedImage.ImageQuantization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.UserRequest;

    internal class ImageHandlerInternal
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ImageHandlerInternal));
        private static TimeSpan defaultClientCacheExpiration = new TimeSpan(0, 10, 0);

        private readonly IEventLogger eventLogger;
        private TimeSpan clientCacheExpiration = defaultClientCacheExpiration;
        private IImageStore imageStore;
        private DateTime? now;

        /// <summary>Initializes a new instance of the <see cref="ImageHandlerInternal"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        public ImageHandlerInternal(IEventLogger eventLogger)
        {
			Logger.LogError(">>> Entering ImageHandlerInternal(eventLogger={EventLogger})", eventLogger);
            this.eventLogger = eventLogger;
			Logger.LogDebug("ImageHandlerInternal(eventLogger={EventLogger}): this.eventLogger → {Arg1}", eventLogger, this.eventLogger);

            this.ContentType = ImageFormat.Jpeg;
			Logger.LogDebug("ImageHandlerInternal(eventLogger={EventLogger}): this.ContentType → {Arg1}", eventLogger, this.ContentType);
            this.ImageCompression = 95;
            this.ImageTransforms = new List<ImageTransform>();
			Logger.LogDebug("ImageHandlerInternal(eventLogger={EventLogger}): this.ImageCompression → {Arg1}", eventLogger, this.ImageCompression);
        Logger.LogDebug("ImageHandlerInternal(eventLogger={EventLogger},imageStore={ImageStore},now={Now}): now → {Now1}", eventLogger, imageStore, now, now);
        Logger.LogWarning("<<< Exiting ImageHandlerInternal(eventLogger={EventLogger})", eventLogger);
            this.AllowStandalone = false;
        }

        /// <summary>Initializes a new instance of the <see cref="ImageHandlerInternal"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="imageStore">The image store.</param>
        /// <param name="now">The current <see cref="DateTime"/>.</param>
        internal ImageHandlerInternal(IEventLogger eventLogger, IImageStore imageStore, DateTime now)
            : this(eventLogger)
        {
            this.imageStore = imageStore;
			Logger.LogDebug("ImageHandlerInternal(eventLogger={EventLogger},imageStore={ImageStore},now={Now}): this.imageStore → {Arg3}", eventLogger, imageStore, now, this.imageStore);
            this.now = now;
        }

        public TimeSpan ClientCacheExpiration
        {
            get
            {
                return this.clientCacheExpiration;
            }

            set
            {
                if (value.Ticks < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "ClientCacheExpiration must be positive");
                }

                this.clientCacheExpiration = value;
                this.EnableClientCache = true;
            }
        }

        public ImageFormat ContentType { get; set; }

        public long ImageCompression { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int IPCountMax
        {
            get { return IPCount.MaxCount; }
            set { IPCount.MaxCount = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public TimeSpan IpCountPurgeInterval
        {
            get { return IPCount.PurgeInterval; }
            set { IPCount.PurgeInterval = value; }
        }

        public bool EnableClientCache { get; set; }

        public bool EnableServerCache { get; set; }

        public bool EnableIPCount { get; set; }

        public bool AllowStandalone { get; set; }

        public string[] AllowedDomains { get; set; }

        public bool LogSecurity { get; set; }

        public List<ImageTransform> ImageTransforms
        {
            get;
            private set;
        }

        private DateTime DateTime_Now
        {
            get
            {
                return this.now ?? DateTime.Now;
            }
        }

        private IImageStore ImageStore
        {
            get
            {
                return this.imageStore ?? DiskImageStore.Instance;
            }
        }

        public void HandleImageRequest(HttpContextBase context, Func<NameValueCollection, ImageInfo> imageGenCallback, string uniqueIdStringSeed)
        {
			Logger.LogError(">>> Entering HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed})", context, imageGenCallback, uniqueIdStringSeed);
            context.Response.Clear();

            string ipAddress = UserRequestIPAddressController.Instance.GetUserRequestIPAddress(context.Request);

            // Check if allowed standalone
            if (!this.AllowStandalone && context.Request.UrlReferrer == null && !context.Request.IsLocal)
            {
                const string Message = "Not allowed to use standalone";
                if (this.LogSecurity)
                {
                    ILogInfo logInfo = new LogInfo
                    {
                        LogTypeKey = nameof(EventLogType.ADMIN_ALERT),
                    };
                    logInfo.LogUserId = PortalSettings.Current.UserId;
					Logger.LogDebug("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): logInfo.LogUserId → {Arg3}", context, imageGenCallback, uniqueIdStringSeed, logInfo.LogUserId);
                    logInfo.LogPortalId = PortalSettings.Current.PortalId;
					Logger.LogDebug("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): logInfo.LogPortalId → {Arg3}", context, imageGenCallback, uniqueIdStringSeed, logInfo.LogPortalId);
                    logInfo.AddProperty("DnnImageHandler", Message);
                    logInfo.AddProperty("IP", ipAddress);
                    this.eventLogger.AddLog(logInfo);
                }

                context.Response.StatusCode = 403;
                context.Response.StatusDescription = Message;
                context.Response.End();
                return;
            }

            // Check if domain is allowed to embed image
            if (!string.IsNullOrEmpty(this.AllowedDomains[0]) &&
                context.Request.UrlReferrer != null &&
                !context.Request.UrlReferrer.Host.Equals(context.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
            {
                bool allowed = false;
				Logger.LogError("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): allowed → {Allowed}", context, imageGenCallback, uniqueIdStringSeed, allowed);
                string allowedDomains = string.Empty;
                foreach (string allowedDomain in this.AllowedDomains)
                {
                    if (!string.IsNullOrEmpty(allowedDomain))
                    {
                        allowedDomains += allowedDomain + ",";
                        if (context.Request.UrlReferrer.Host.Contains(allowedDomain, StringComparison.OrdinalIgnoreCase))
                        {
                            allowed = true;
                        }
                    }
                }

                if (!allowed)
                {
                    string message = $"Not allowed to use from referrer '{context.Request.UrlReferrer.Host}'";
                    if (this.LogSecurity)
                    {
                        ILogInfo logInfo = new LogInfo
                        {
                            LogTypeKey = nameof(EventLogType.ADMIN_ALERT),
                        };
                        logInfo.LogUserId = PortalSettings.Current.UserId;
                        logInfo.LogPortalId = PortalSettings.Current.PortalId;
                        logInfo.AddProperty("DnnImageHandler", message);
			Logger.LogWarning("<<< Exiting HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed})", context, imageGenCallback, uniqueIdStringSeed);
                        logInfo.AddProperty("IP", ipAddress);

#pragma warning disable CA1507 // Use nameof in place of string
                        logInfo.AddProperty("AllowedDomains", allowedDomains);
#pragma warning restore CA1507

                        this.eventLogger.AddLog(logInfo);
                    }

                    context.Response.StatusCode = 403;
                    context.Response.StatusDescription = "Forbidden";
                    context.Response.End();
                    return;
                }
            }

            // Generate Image
            var imageMethodData = imageGenCallback(context.Request.QueryString);
			Logger.LogError("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): imageMethodData → {ImageMethodData}", context, imageGenCallback, uniqueIdStringSeed, imageMethodData);

            context.Response.ContentType = GetImageMimeType(this.ContentType);
            if (imageMethodData == null)
            {
				Logger.LogError("Throwing InvalidOperationException from HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed})", context, imageGenCallback, uniqueIdStringSeed);
                throw new InvalidOperationException("The DnnImageHandler cannot return null.");
            }

            if (imageMethodData.IsEmptyImage)
            {
                using var imageOutputBuffer = new MemoryStream();
				Logger.LogError("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): imageOutputBuffer → {ImageOutputBuffer}", context, imageGenCallback, uniqueIdStringSeed, imageOutputBuffer);
                this.RenderImage(imageMethodData.Image, imageOutputBuffer);
                var buffer = imageOutputBuffer.GetBuffer();
				Logger.LogError("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): buffer → {Buffer}", context, imageGenCallback, uniqueIdStringSeed, buffer);
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.End();
                return;
            }

            string cacheId = this.GetUniqueIDString(context, uniqueIdStringSeed);

            var userId = -1;
			Logger.LogError("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): userId → {UserId}", context, imageGenCallback, uniqueIdStringSeed, userId);
            var cacheCleared = false;
			Logger.LogError("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): cacheCleared → {CacheCleared}", context, imageGenCallback, uniqueIdStringSeed, cacheCleared);
            var isProfilePic = "profilepic".Equals(context.Request.QueryString["mode"], StringComparison.OrdinalIgnoreCase);
			Logger.LogError("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): isProfilePic → {IsProfilePic}", context, imageGenCallback, uniqueIdStringSeed, isProfilePic);
            if (isProfilePic)
            {
                if (int.TryParse(context.Request.QueryString["userId"], out userId))
                {
                    cacheCleared = this.ClearDiskImageCacheIfNecessary(userId, PortalSettings.Current.PortalId, cacheId);
                }
            }

            // Handle client cache
            var cachePolicy = context.Response.Cache;
            cachePolicy.SetValidUntilExpires(true);
            if (this.EnableClientCache)
            {
                if (!string.IsNullOrEmpty(context.Request.Headers["If-Modified-Since"]) && !string.IsNullOrEmpty(context.Request.Headers["If-None-Match"]) && !cacheCleared)
                {
                    var provider = CultureInfo.InvariantCulture;
                    var lastMod = DateTime.ParseExact(context.Request.Headers["If-Modified-Since"], "r", provider).ToLocalTime();
					Logger.LogError("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): lastMod → {LastMod}", context, imageGenCallback, uniqueIdStringSeed, lastMod);
                    var etag = context.Request.Headers["If-None-Match"];
					Logger.LogError("HandleImageRequest(context={Context},imageGenCallback={ImageGenCallback},uniqueIdStringSeed={UniqueIdStringSeed}): etag → {Etag}", context, imageGenCallback, uniqueIdStringSeed, etag);
                    if (lastMod + this.ClientCacheExpiration > this.DateTime_Now && etag == cacheId)
                    {
                        context.Response.StatusCode = 304;
                        context.Response.StatusDescription = "Not Modified";
                        context.Response.End();
                        return;
                    }
                }

                cachePolicy.SetCacheability(HttpCacheability.Public);
                cachePolicy.SetLastModified(this.DateTime_Now);
                cachePolicy.SetExpires(this.DateTime_Now + this.ClientCacheExpiration);
                cachePolicy.SetETag(cacheId);
            }

            // Handle Server cache
            if (this.EnableServerCache)
            {
                var isAnonymousUser = userId <= 0 ? true : false;
                if (isProfilePic && !isAnonymousUser && !IsPicVisibleToCurrentUser(userId))
                {
                    const string message = "Not allowed to see profile picture";

                    if (this.LogSecurity)
                    {
                        ILogInfo logInfo = new LogInfo
                        {
                            LogTypeKey = nameof(EventLogType.ADMIN_ALERT),
                        };
                        logInfo.LogUserId = PortalSettings.Current.UserId;
                        logInfo.LogPortalId = PortalSettings.Current.PortalId;
                        logInfo.AddProperty("DnnImageHandler", message);
                        logInfo.AddProperty("IP", ipAddress);
                        this.eventLogger.AddLog(logInfo);
                    }

                    context.Response.StatusCode = 403;
                    context.Response.StatusDescription = "Forbidden";
                    context.Response.End();
                    return;
                }

                if (this.ImageStore.TryTransmitIfContains(cacheId, context.Response))
                {
                    context.Response.Flush();
                    return;
                }
            }

            // Check IP Cout boundaries
            if (this.EnableIPCount)
            {
                if (!IPCount.CheckIp(ipAddress))
                {
                    const string Message = "Too many requests";

                    if (this.LogSecurity)
                    {
                        ILogInfo logInfo = new LogInfo
                        {
                            LogTypeKey = nameof(EventLogType.ADMIN_ALERT),
                        };
                        logInfo.LogUserId = PortalSettings.Current.UserId;
                        logInfo.LogPortalId = PortalSettings.Current.PortalId;
                        logInfo.AddProperty("DnnImageHandler", Message);
                        logInfo.AddProperty("IP", ipAddress);
                        this.eventLogger.AddLog(logInfo);
                    }

                    context.Response.StatusCode = 429;
                    context.Response.StatusDescription = Message;
                    context.Response.End();
                    return;
                }
            }

            if (imageMethodData.HttpStatusCode != null)
            {
                context.Response.StatusCode = (int)imageMethodData.HttpStatusCode;
                context.Response.End();
                return;
            }

            using (var imageOutputBuffer = new MemoryStream())
            {
                Debug.Assert(!(imageMethodData.Image == null && imageMethodData.ImageByteBuffer == null), "Image or ImageByteByffer must have a value");
                if (imageMethodData.Image != null)
                {
                    this.RenderImage(this.GetImageThroughTransforms(imageMethodData.Image), imageOutputBuffer);
                }
                else if (imageMethodData.ImageByteBuffer != null)
                {
                    this.RenderImage(this.GetImageThroughTransforms(imageMethodData.ImageByteBuffer), imageOutputBuffer);
                }

                byte[] buffer = imageOutputBuffer.GetBuffer();

                context.Response.OutputStream.Write(buffer, 0, buffer.Length);

                if (this.EnableServerCache)
                {
                    this.ImageStore.Add(cacheId, buffer);
                }

                context.Response.End();
            }
        }

        internal static string GetImageMimeType(ImageFormat format)
        {
			Logger.LogError(">>> Entering GetImageMimeType(format={Format})", format);
            string mimeType = "image/x-unknown";

			Logger.LogError("GetImageMimeType(format={Format}): mimeType → {MimeType}", format, mimeType);
            if (format.Equals(ImageFormat.Gif))
            {
                mimeType = "image/gif";
            }
            else if (format.Equals(ImageFormat.Jpeg))
            {
                mimeType = "image/jpeg";
            }
            else if (format.Equals(ImageFormat.Png))
            {
                mimeType = "image/png";
            }
            else if (format.Equals(ImageFormat.Bmp) || format.Equals(ImageFormat.MemoryBmp))
            {
                mimeType = "image/bmp";
            }
            else if (format.Equals(ImageFormat.Tiff))
            {
                mimeType = "image/tiff";
            }
            else if (format.Equals(ImageFormat.Icon))
            {
                mimeType = "image/x-icon";
            }

			Logger.LogWarning("<<< Exiting GetImageMimeType(format={Format})", format);
            return mimeType;
        }

        private static string GetIDFromBytes(byte[] buffer)
        {
			Logger.LogError(">>> Entering GetIDFromBytes(buffer={Buffer})", buffer);
            using var hasher = CryptographyUtils.CreateSHA256();
            byte[] result = hasher.ComputeHash(buffer);
			Logger.LogError("GetIDFromBytes(buffer={Buffer}): hasher → {Hasher}", buffer, hasher);
            var sb = new StringBuilder();
			Logger.LogError("GetIDFromBytes(buffer={Buffer}): sb → {Sb}", buffer, sb);
            foreach (var b in result)
            {
                sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }

			Logger.LogWarning("<<< Exiting GetIDFromBytes(buffer={Buffer})", buffer);
            return sb.ToString();
        }

        /// <summary>Returns the encoder for the specified mime type.</summary>
        /// <param name="mimeType">The mime type of the content.</param>
        /// <returns>ImageCodecInfo.</returns>
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
			Logger.LogError(">>> Entering GetEncoderInfo(mimeType={MimeType})", mimeType);
            var encoders = ImageCodecInfo.GetImageEncoders();
            var e = encoders.FirstOrDefault(x => x.MimeType == mimeType);
			Logger.LogError("GetEncoderInfo(mimeType={MimeType}): e → {E}", mimeType, e);
			Logger.LogError("GetEncoderInfo(mimeType={MimeType}): encoders → {Encoders}", mimeType, encoders);
			Logger.LogWarning("<<< Exiting GetEncoderInfo(mimeType={MimeType})", mimeType);
            return e;
        }

        private static bool IsPicVisibleToCurrentUser(int profileUserId)
        {
			Logger.LogError(">>> Entering IsPicVisibleToCurrentUser(profileUserId={ProfileUserId})", profileUserId);
            var settings = PortalController.Instance.GetCurrentSettings();
            var profileUser = UserController.Instance.GetUser(settings.PortalId, profileUserId);
			Logger.LogError("IsPicVisibleToCurrentUser(profileUserId={ProfileUserId}): profileUser → {ProfileUser}", profileUserId, profileUser);
			Logger.LogError("IsPicVisibleToCurrentUser(profileUserId={ProfileUserId}): settings → {Settings}", profileUserId, settings);

            var photoProperty = profileUser?.Profile.GetProperty("Photo");
			Logger.LogError("IsPicVisibleToCurrentUser(profileUserId={ProfileUserId}): photoProperty → {PhotoProperty}", profileUserId, photoProperty);
            if (photoProperty == null)
            {
                return false;
            }

            var currentUser = UserController.Instance.GetCurrentUserInfo();
			Logger.LogError("IsPicVisibleToCurrentUser(profileUserId={ProfileUserId}): currentUser → {CurrentUser}", profileUserId, currentUser);
			Logger.LogWarning("<<< Exiting IsPicVisibleToCurrentUser(profileUserId={ProfileUserId})", profileUserId);
            return ProfilePropertyAccess.CheckAccessLevel(settings, photoProperty, currentUser, profileUser);
        }

        private string GetUniqueIDString(HttpContextBase context, string uniqueIdStringSeed)
        {
			Logger.LogError(">>> Entering GetUniqueIDString(context={Context},uniqueIdStringSeed={UniqueIdStringSeed})", context, uniqueIdStringSeed);
            var builder = new StringBuilder();
            builder.Append(uniqueIdStringSeed);
			Logger.LogError("GetUniqueIDString(context={Context},uniqueIdStringSeed={UniqueIdStringSeed}): builder → {Builder}", context, uniqueIdStringSeed, builder);
            foreach (var key in context.Request.QueryString.AllKeys.OrderBy(k => k))
            {
                builder.Append(key);
                builder.Append(context.Request.QueryString.Get(key));
            }

            foreach (var tran in this.ImageTransforms)
            {
                builder.Append(tran.UniqueString);
            }

			Logger.LogWarning("<<< Exiting GetUniqueIDString(context={Context},uniqueIdStringSeed={UniqueIdStringSeed})", context, uniqueIdStringSeed);
            return GetIDFromBytes(Encoding.ASCII.GetBytes(builder.ToString()));
        }

        private Image GetImageThroughTransforms(Image image)
        {
			Logger.LogError(">>> Entering GetImageThroughTransforms(image={Image})", image);
            try
            {
                Image temp = image;

                foreach (var tran in this.ImageTransforms)
                {
                    temp = tran.ProcessImage(temp);
                }

				Logger.LogWarning("<<< Exiting GetImageThroughTransforms(image={Image})", image);
                return temp;
            }
            finally
            {
                image?.Dispose();
			Logger.LogWarning("<<< Exiting GetImageThroughTransforms(buffer={Buffer})", buffer);
            }
        }

        // Clear the user image disk cache if userid is found in clear list and is within ClientCacheExpiration time.
        private bool ClearDiskImageCacheIfNecessary(int userId, int portalId, string cacheId)
        {
			Logger.LogError(">>> Entering ClearDiskImageCacheIfNecessary(userId={UserId},portalId={PortalId},cacheId={CacheId})", userId, portalId, cacheId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.UserIdListToClearDiskImageCacheKey, portalId);
            Dictionary<int, DateTime> userIds;
			Logger.LogError("ClearDiskImageCacheIfNecessary(userId={UserId},portalId={PortalId},cacheId={CacheId}): cacheKey → {CacheKey}", userId, portalId, cacheId, cacheKey);
            if ((userIds = DataCache.GetCache<Dictionary<int, DateTime>>(cacheKey)) == null || !userIds.ContainsKey(userId))
            {
                return false;
            }

            this.ImageStore.ForcePurgeFromServerCache(cacheId);
            DateTime expiry;

            // The clear mechanism is performed for ClientCacheExpiration timespan so that all active clients clears the cache and don't see old data.
            if (!userIds.TryGetValue(userId, out expiry) || DateTime.UtcNow <= expiry.Add(this.ClientCacheExpiration))
            {
                return true;
            }

            // Remove the userId from the clear list when timespan is > ClientCacheExpiration.
            userIds.Remove(userId);
            DataCache.SetCache(cacheKey, userIds);
			Logger.LogWarning("<<< Exiting ClearDiskImageCacheIfNecessary(userId={UserId},portalId={PortalId},cacheId={CacheId})", userId, portalId, cacheId);
            return true;
        }

        private Image GetImageThroughTransforms(byte[] buffer)
        {
            using (var memoryStream = new MemoryStream(buffer))
            {
                return this.GetImageThroughTransforms(Image.FromStream(memoryStream));
            }
        }

        private void RenderImage(Image image, Stream outStream)
        {
			Logger.LogError(">>> Entering RenderImage(image={Image},outStream={OutStream})", image, outStream);
            try
            {
                if (this.ContentType == ImageFormat.Gif)
                {
                    var quantizer = new OctreeQuantizer(255, 8);
					Logger.LogError("RenderImage(image={Image},outStream={OutStream}): quantizer → {Quantizer}", image, outStream, quantizer);
                    using (var quantized = quantizer.Quantize(image))
                    {
                        quantized.Save(outStream, ImageFormat.Gif);
                    }
                }
                else
                {
                    var eps = new EncoderParameters(1)
                    {
                        Param = { [0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, this.ImageCompression) },
                    };
                    var ici = GetEncoderInfo(GetImageMimeType(this.ContentType));
					Logger.LogError("RenderImage(image={Image},outStream={OutStream}): ici → {Ici}", image, outStream, ici);
                    image?.Save(outStream, ici, eps);
                }
            }
            finally
            {
                image?.Dispose();
            }
			Logger.LogWarning("<<< Exiting RenderImage(image={Image},outStream={OutStream})", image, outStream);
        }
    }
}
