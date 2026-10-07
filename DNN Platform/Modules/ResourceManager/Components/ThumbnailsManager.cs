// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Net.Http;

    using Dnn.Modules.ResourceManager.Components.Models;

    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.FileSystem;

    /// <summary>Provides services related to thumbnails.</summary>
    public class ThumbnailsManager : ServiceLocator<IThumbnailsManager, ThumbnailsManager>, IThumbnailsManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThumbnailsManager));
        private const int DefaultMaxWidth = 320;
        private const int DefaultMaxHeight = 240;
        private readonly IFileManager fileManager;

        /// <summary>Initializes a new instance of the <see cref="ThumbnailsManager"/> class.</summary>
        public ThumbnailsManager()
        {
			Logger.LogError(">>> Entering ThumbnailsManager()");
            this.fileManager = FileManager.Instance;
			Logger.LogDebug("ThumbnailsManager(): fileManager → {FileManager}", fileManager);
        Logger.LogDebug("ThumbnailsManager(): this.fileManager → {Arg0}", this.fileManager);
        Logger.LogWarning("<<< Exiting ThumbnailsManager()");
        }

        /// <summary>Enumerates the possible thumbnail file name extensions.</summary>
        private enum ThumbnailExtensions
        {
            /// <summary>A JPEG thumbnail.</summary>
            JPEG = 0,

            /// <summary>A JPG thumbnail.</summary>
            JPG = 1,

            /// <summary>A PNG thumbnail.</summary>
            PNG = 2,

            /// <summary>A GIF thumbnail.</summary>
            GIF = 3,
        }

        /// <summary>Gets the default content mime type.</summary>
        public string DefaultContentType => "image/png";

        /// <summary>Gets the default file name extension.</summary>
        public string DefaultThumbnailExtension => "png";

        /// <inheritdoc />
        public bool ThumbnailAvailable(string fileName)
        {
			Logger.LogError(">>> Entering ThumbnailAvailable(fileName={FileName})", fileName);
            var ext = Path.GetExtension(fileName).ToUpperInvariant();
            ext = ext.StartsWith(".", StringComparison.Ordinal) ? ext.Substring(1) : ext;
			Logger.LogError("ThumbnailAvailable(fileName={FileName}): ext → {Ext}", fileName, ext);
			Logger.LogWarning("<<< Exiting ThumbnailAvailable(fileName={FileName})", fileName);
            return Enum.TryParse(ext, out ThumbnailExtensions _);
        }

        /// <inheritdoc />
        public string ThumbnailUrl(int moduleId, int fileId, int width, int height)
        {
			Logger.LogError(">>> Entering ThumbnailUrl(moduleId={ModuleId},fileId={FileId},width={Width},height={Height})", moduleId, fileId, width, height);
            Logger.LogWarning("<<< Exiting ThumbnailUrl(moduleId={ModuleId},fileId={FileId},width={Width},height={Height})", moduleId, fileId, width, height);
			Logger.LogWarning("<<< Exiting ThumbnailUrl(moduleId={ModuleId},fileId={FileId},width={Width},height={Height},timestamp={Timestamp})", moduleId, fileId, width, height, timestamp);
			Logger.LogWarning("<<< Exiting ThumbnailUrl(moduleId={ModuleId},fileId={FileId},width={Width},height={Height},version={Version})", moduleId, fileId, width, height, version);
            return this.ThumbnailUrl(moduleId, fileId, width, height, this.GetNewTimeStamp());
        }

        /// <inheritdoc />
        public string ThumbnailUrl(int moduleId, int fileId, int width, int height, string timestamp)
        {
            var tabId = TabController.CurrentPage.TabID;

            return
                $"{ServicesFramework.GetServiceFrameworkRoot()}API/ResourceManager/Items/ThumbnailDownLoad?fileId={fileId}&width={width}&height={height}&timestamp={timestamp}&moduleId={moduleId}&tabId={tabId}";
        }

        /// <inheritdoc />
        public string ThumbnailUrl(int moduleId, int fileId, int width, int height, int version)
        {
            var result = this.ThumbnailUrl(moduleId, fileId, width, height, this.GetNewTimeStamp());
			Logger.LogError("ThumbnailUrl(moduleId={ModuleId},fileId={FileId},width={Width},height={Height},version={Version}): result → {Result}", moduleId, fileId, width, height, version, result);
            return result + "&version=" + version;
        }

        /// <inheritdoc />
        public ThumbnailContent GetThumbnailContentFromImageUrl(string imageUrl, int width, int height)
        {
			Logger.LogError(">>> Entering GetThumbnailContentFromImageUrl(imageUrl={ImageUrl},width={Width},height={Height})", imageUrl, width, height);
            Image image = new Bitmap(imageUrl);
            var result = this.GetThumbnailContentFromImage(image, width, height);
			Logger.LogError("GetThumbnailContentFromImageUrl(imageUrl={ImageUrl},width={Width},height={Height}): result → {Result}", imageUrl, width, height, result);
			Logger.LogDebug("GetThumbnailContentFromImageUrl(imageUrl={ImageUrl},width={Width},height={Height}): result.ThumbnailName → {Arg3}", imageUrl, width, height, result.ThumbnailName);
            var indexOfSlash = imageUrl.LastIndexOf('/');
			Logger.LogError("GetThumbnailContentFromImageUrl(imageUrl={ImageUrl},width={Width},height={Height}): indexOfSlash → {IndexOfSlash}", imageUrl, width, height, indexOfSlash);
            var thumbnailName = (indexOfSlash != -1) ? imageUrl.Substring(indexOfSlash + 1) + "." + this.DefaultThumbnailExtension : imageUrl + "." + this.DefaultThumbnailExtension;
			Logger.LogError("GetThumbnailContentFromImageUrl(imageUrl={ImageUrl},width={Width},height={Height}): thumbnailName → {ThumbnailName}", imageUrl, width, height, thumbnailName);
            result.ThumbnailName = thumbnailName;
			Logger.LogError("GetThumbnailContentFromImageUrl(imageUrl={ImageUrl},width={Width},height={Height}): image → {Image}", imageUrl, width, height, image);
			Logger.LogWarning("<<< Exiting GetThumbnailContentFromImageUrl(imageUrl={ImageUrl},width={Width},height={Height})", imageUrl, width, height);
            return result;
        }

        /// <inheritdoc />
        public ThumbnailContent GetThumbnailContentFromImage(Image image, int width, int height, bool crop = false)
        {
			Logger.LogError(">>> Entering GetThumbnailContentFromImage(image={Image},width={Width},height={Height},crop={Crop})", image, width, height, crop);
            int thumbnailWidth;
            int thumbnailHeight;

            if (crop)
            {
                this.GetCroppedThumbnailSize(image, width, height, out thumbnailWidth, out thumbnailHeight);
            }
            else
            {
                this.GetThumbnailSize(image, width, height, out thumbnailWidth, out thumbnailHeight);
            }

            // create the actual thumbnail image
            var thumbnailImage = new Bitmap(image, new Size(thumbnailWidth, thumbnailHeight));
			Logger.LogError("GetThumbnailContentFromImage(image={Image},width={Width},height={Height},crop={Crop}): thumbnailImage → {ThumbnailImage}", image, width, height, crop, thumbnailImage);

            using (var memoryStream = new MemoryStream())
            {
                // All thumbnails images will be png
                thumbnailImage.Save(memoryStream, ImageFormat.Png);

				Logger.LogWarning("<<< Exiting GetThumbnailContentFromImage(image={Image},width={Width},height={Height},crop={Crop})", image, width, height, crop);
                return new ThumbnailContent
                {
                    Content = new ByteArrayContent(memoryStream.ToArray()),
                    Width = thumbnailWidth,
                    Height = thumbnailHeight,
                    ContentType = this.DefaultContentType,
                };
            }
        }

        /// <inheritdoc />
        public ThumbnailContent GetThumbnailContent(IFileInfo item, int width, int height, bool crop = false)
        {
			Logger.LogError(">>> Entering GetThumbnailContent(item={Item},width={Width},height={Height},crop={Crop})", item, width, height, crop);
            using (var content = this.fileManager.GetFileContent(item))
            {
                Image image = new Bitmap(content);
				Logger.LogError("GetThumbnailContent(item={Item},width={Width},height={Height},crop={Crop}): image → {Image}", item, width, height, crop, image);
                var result = this.GetThumbnailContentFromImage(image, width, height, crop);
				Logger.LogError("GetThumbnailContent(item={Item},width={Width},height={Height},crop={Crop}): result → {Result}", item, width, height, crop, result);
                result.ThumbnailName = item.FileName + "." + this.DefaultThumbnailExtension;
				Logger.LogDebug("GetThumbnailContent(item={Item},width={Width},height={Height},crop={Crop}): result.ThumbnailName → {Arg4}", item, width, height, crop, result.ThumbnailName);
				Logger.LogWarning("<<< Exiting GetThumbnailContent(item={Item},width={Width},height={Height},crop={Crop})", item, width, height, crop);
                return result;
            }
        }

        /// <inheritdoc />
        protected override Func<IThumbnailsManager> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new ThumbnailsManager();
        }

        private int GetMinSizeValue(int thumbnailMaxSize, int imageMaxSize, int imageMinSize)
        {
			Logger.LogError(">>> Entering GetMinSizeValue(thumbnailMaxSize={ThumbnailMaxSize},imageMaxSize={ImageMaxSize},imageMinSize={ImageMinSize})", thumbnailMaxSize, imageMaxSize, imageMinSize);
            if (thumbnailMaxSize == imageMaxSize)
            {
                return imageMinSize;
            }

			Logger.LogWarning("<<< Exiting GetMinSizeValue(thumbnailMaxSize={ThumbnailMaxSize},imageMaxSize={ImageMaxSize},imageMinSize={ImageMinSize})", thumbnailMaxSize, imageMaxSize, imageMinSize);
            return (int)Math.Round(imageMinSize * (double)thumbnailMaxSize / imageMaxSize);
        }

        private int GetMaxSizeValue(int size, int imageSize, int defaultMaxValue)
        {
			Logger.LogError(">>> Entering GetMaxSizeValue(size={Size},imageSize={ImageSize},defaultMaxValue={DefaultMaxValue})", size, imageSize, defaultMaxValue);
            if (size >= imageSize)
            {
                return imageSize;
            }

            var minimum = Math.Min(size, defaultMaxValue);
			Logger.LogError("GetMaxSizeValue(size={Size},imageSize={ImageSize},defaultMaxValue={DefaultMaxValue}): minimum → {Minimum}", size, imageSize, defaultMaxValue, minimum);
			Logger.LogWarning("<<< Exiting GetMaxSizeValue(size={Size},imageSize={ImageSize},defaultMaxValue={DefaultMaxValue})", size, imageSize, defaultMaxValue);
            return (minimum <= 0) ? defaultMaxValue : minimum;
        }

        private void GetThumbnailSize(Image image, int width, int height, out int thumbnailWidth, out int thumbnailHeight)
        {
			Logger.LogError(">>> Entering GetThumbnailSize(image={Image},width={Width},height={Height})", image, width, height);
            if (image.Width >= image.Height)
            {
                thumbnailWidth = this.GetMaxSizeValue(width, image.Width, DefaultMaxWidth);
				Logger.LogError("GetThumbnailSize(image={Image},width={Width},height={Height}): thumbnailWidth → {ThumbnailWidth}", image, width, height, thumbnailWidth);
                thumbnailHeight = this.GetMinSizeValue(thumbnailWidth, image.Width, image.Height);
				Logger.LogDebug("GetThumbnailSize(image={Image},width={Width},height={Height}): thumbnailHeight → {ThumbnailHeight}", image, width, height, thumbnailHeight);
            }
            else
            {
				Logger.LogDebug("GetThumbnailSize(image={Image},width={Width},height={Height}): thumbnailWidth → {ThumbnailWidth}", image, width, height, thumbnailWidth);
                thumbnailHeight = this.GetMaxSizeValue(height, image.Height, DefaultMaxHeight);
				Logger.LogError("GetThumbnailSize(image={Image},width={Width},height={Height}): thumbnailHeight → {ThumbnailHeight}", image, width, height, thumbnailHeight);
                thumbnailWidth = this.GetMinSizeValue(thumbnailHeight, image.Height, image.Width);
			Logger.LogWarning("<<< Exiting GetThumbnailSize(image={Image},width={Width},height={Height})", image, width, height);
            }
        }

        private void GetCroppedThumbnailSize(Image image, int width, int height, out int thumbnailWidth, out int thumbnailHeight)
        {
			Logger.LogError(">>> Entering GetCroppedThumbnailSize(image={Image},width={Width},height={Height})", image, width, height);
            var aspect = (double)image.Width / image.Height;
            var thumbnailAspect = (double)width / height;
			Logger.LogError("GetCroppedThumbnailSize(image={Image},width={Width},height={Height}): thumbnailAspect → {ThumbnailAspect}", image, width, height, thumbnailAspect);
			Logger.LogError("GetCroppedThumbnailSize(image={Image},width={Width},height={Height}): aspect → {Aspect}", image, width, height, aspect);

            if (aspect > thumbnailAspect)
            {
                thumbnailHeight = Math.Min(image.Height, height);
				Logger.LogError("GetCroppedThumbnailSize(image={Image},width={Width},height={Height}): thumbnailHeight → {ThumbnailHeight}", image, width, height, thumbnailHeight);
				Logger.LogDebug("GetCroppedThumbnailSize(image={Image},width={Width},height={Height}): thumbnailHeight → {ThumbnailHeight}", image, width, height, thumbnailHeight);
                thumbnailWidth = (int)Math.Round(image.Width * (double)thumbnailHeight / image.Height);
            }
            else
            {
                thumbnailWidth = Math.Min(image.Width, width);
				Logger.LogError("GetCroppedThumbnailSize(image={Image},width={Width},height={Height}): thumbnailWidth → {ThumbnailWidth}", image, width, height, thumbnailWidth);
                thumbnailHeight = (int)Math.Round(image.Height * (double)thumbnailWidth / image.Width);
			Logger.LogWarning("<<< Exiting GetCroppedThumbnailSize(image={Image},width={Width},height={Height})", image, width, height);
            }
        }

        private string GetNewTimeStamp()
        {
			Logger.LogError(">>> Entering GetNewTimeStamp()");
            Logger.LogWarning("<<< Exiting GetNewTimeStamp()");
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
}
