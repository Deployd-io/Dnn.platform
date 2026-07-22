// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Browser
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Web;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;

    /// <summary>The process image.</summary>
    public class ProcessImage : IHttpHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Gets a value indicating whether IsReusable.</summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>The get thumb abort.</summary>
        /// <returns>
        /// Returns abort.
        /// </returns>
        public bool GetThumbAbort()
        {
			Logger.LogDebug(">>> Entering GetThumbAbort()");
			Logger.LogDebug("<<< Exiting GetThumbAbort()");
            return false;
        }

        /// <summary>The process request.</summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public void ProcessRequest(HttpContext context)
        {
			Logger.LogDebug(">>> Entering ProcessRequest(context={Context})", context);
            float imageH = float.Parse(context.Request["imageH"]);
            float imageW = float.Parse(context.Request["imageW"]);
			Logger.LogDebug("ProcessRequest(context={Context}): imageH → {ImageH}", context, imageH);
			Logger.LogDebug("ProcessRequest(context={Context}): imageW → {ImageW}", context, imageW);
            float angle = float.Parse(context.Request["imageRotate"]);
            int sourceImageId = int.Parse(context.Request["fileId"]);
			Logger.LogDebug("ProcessRequest(context={Context}): angle → {Angle}", context, angle);
			Logger.LogDebug("ProcessRequest(context={Context}): sourceImageId → {SourceImageId}", context, sourceImageId);
            float imageX = float.Parse(context.Request["imageX"]);
            float imageY = float.Parse(context.Request["imageY"]);
			Logger.LogDebug("ProcessRequest(context={Context}): imageX → {ImageX}", context, imageX);
			Logger.LogDebug("ProcessRequest(context={Context}): imageY → {ImageY}", context, imageY);
            float selectorH = float.Parse(context.Request["selectorH"]);
            float selectorW = float.Parse(context.Request["selectorW"]);
			Logger.LogDebug("ProcessRequest(context={Context}): selectorH → {SelectorH}", context, selectorH);
			Logger.LogDebug("ProcessRequest(context={Context}): selectorW → {SelectorW}", context, selectorW);
            float selectorX = float.Parse(context.Request["selectorX"]);
            float selectorY = float.Parse(context.Request["selectorY"]);
			Logger.LogDebug("ProcessRequest(context={Context}): selectorX → {SelectorX}", context, selectorX);
			Logger.LogDebug("ProcessRequest(context={Context}): selectorY → {SelectorY}", context, selectorY);
            float viewPortH = float.Parse(context.Request["viewPortH"]);
            float viewPortW = float.Parse(context.Request["viewPortW"]);
			Logger.LogDebug("ProcessRequest(context={Context}): viewPortH → {ViewPortH}", context, viewPortH);
			Logger.LogDebug("ProcessRequest(context={Context}): viewPortW → {ViewPortW}", context, viewPortW);

            bool bSaveFile;

            try
            {
                bSaveFile = bool.Parse(context.Request["saveFile"]);
				Logger.LogDebug("ProcessRequest(context={Context}): bSaveFile → {BSaveFile}", context, bSaveFile);
            }
            catch (Exception)
            {
				Logger.LogError("Exception in ProcessRequest(context={Context}): {Arg1}", context, ex.Message);
                bSaveFile = false;
            }

            string sNewFileName = null;

			Logger.LogDebug("ProcessRequest(context={Context}): sNewFileName → {SNewFileName}", context, sNewFileName);
            if (!string.IsNullOrEmpty(context.Request["newFileName"]))
            {
                sNewFileName = context.Request["newFileName"];
            }

            float pWidth = imageW;
            float pHeight = imageH;
			Logger.LogDebug("ProcessRequest(context={Context}): pWidth → {PWidth}", context, pWidth);
			Logger.LogDebug("ProcessRequest(context={Context}): pHeight → {PHeight}", context, pHeight);

            var file = FileManager.Instance.GetFile(sourceImageId);
            if (file == null)
			Logger.LogDebug("ProcessRequest(context={Context}): file → {File}", context, file);
            {
                return;
            }

            Bitmap img = (Bitmap)Image.FromStream(FileManager.Instance.GetFileContent(file));

			Logger.LogDebug("ProcessRequest(context={Context}): img → {Img}", context, img);
            // Resize
            Bitmap imageP = this.ResizeImage(img, Convert.ToInt32(pWidth), Convert.ToInt32(pHeight));

			Logger.LogDebug("ProcessRequest(context={Context}): imageP → {ImageP}", context, imageP);
            // Rotate if angle is not 0.00 or 360
            if (angle > 0.0F && angle < 360.00F)
            {
                imageP = (Bitmap)RotateImage(imageP, angle);
                pWidth = imageP.Width;
                pHeight = imageP.Height;
            }

            // Calculate Coords of the Image into the ViewPort
            float srcX;
            float dstX;
            float srcY;
            float dstY;

            if (pWidth > viewPortW)
            {
                srcX = Math.Abs(imageX - Math.Abs((imageW - pWidth) / 2));
				Logger.LogDebug("ProcessRequest(context={Context}): srcX → {SrcX}", context, srcX);
                dstX = 0;
            }
			Logger.LogDebug("ProcessRequest(context={Context}): dstX → {DstX}", context, dstX);
            else
            {
                srcX = 0;
                dstX = imageX + ((imageW - pWidth) / 2);
            }

            if (pHeight > viewPortH)
            {
                srcY = Math.Abs(imageY - Math.Abs((imageH - pHeight) / 2));
				Logger.LogDebug("ProcessRequest(context={Context}): srcY → {SrcY}", context, srcY);
                dstY = 0;
            }
			Logger.LogDebug("ProcessRequest(context={Context}): dstY → {DstY}", context, dstY);
            else
            {
                srcY = 0;
                dstY = imageY + ((imageH - pHeight) / 2);
            }

            // Get Image viewed into the ViewPort
            imageP = ImageCopy(imageP, dstX, dstY, srcX, srcY, viewPortW, viewPortH);

            // Get Selector Portion
            imageP = ImageCopy(imageP, 0, 0, selectorX, selectorY, selectorW, selectorH);

            if (bSaveFile)
            {
                context.Response.ContentType = "text/plain";

                var sourceFolder = FolderManager.Instance.GetFolder(file.FolderId);
                if (PortalSettings.Current != null
						Logger.LogDebug("ProcessRequest(context={Context}): sourceFolder → {SourceFolder}", context, sourceFolder);
                        && !this.HasWritePermission(sourceFolder.FolderPath))
                {
                    throw new SecurityException("You don't have write permission to save files under this folder.");
                }

                using var stream = new MemoryStream();
                var fileName = GenerateName(file, sNewFileName);
				Logger.LogDebug("ProcessRequest(context={Context}): stream → {Stream}", context, stream);
				Logger.LogDebug("ProcessRequest(context={Context}): fileName → {FileName}", context, fileName);
                imageP.Save(stream, img.RawFormat);

                const bool alreadyCheckedPermissions = true;
                FileManager.Instance.AddFile(sourceFolder, fileName, stream, false, !alreadyCheckedPermissions, FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName)));
            }
            else
            {
                context.Response.ContentType = "image/jpeg";
                imageP.Save(context.Response.OutputStream, ImageFormat.Jpeg);
            }

            imageP.Dispose();
            img.Dispose();
			Logger.LogDebug("<<< Exiting ProcessRequest(context={Context})", context);
        }

        /// <summary>Generates the New File Path.</summary>
        /// <param name="file">
        /// The Original Image.
        /// </param>
        /// <param name="sNewFileName">
        /// New File Name for the Image.
        /// </param>
        /// <returns>
        /// The generate name.
        /// </returns>
        private static string GenerateName(IFileInfo file, string sNewFileName)
        {
            string sNewFilePath = !string.IsNullOrEmpty(sNewFileName)
                                      ? $"{CleanName(sNewFileName)}.{file.Extension}"
                                      : $"{Path.GetFileNameWithoutExtension(sNewFileName)}_crop.{file.Extension}";

            int iCounter = 0;

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            while (FileManager.Instance.FileExists(folder, sNewFilePath))
            {
                iCounter++;

                string sFileNameNoExt = Path.GetFileNameWithoutExtension(sNewFilePath);
                sNewFilePath = $"{sFileNameNoExt}_{iCounter}.{file.Extension}";
            }

            return sNewFilePath;
        }

        /// <summary>The image copy.</summary>
        /// <param name="srcBitmap">
        /// The src bitmap.
        /// </param>
        /// <param name="dstX">
        /// The dst x.
        /// </param>
        /// <param name="dstY">
        /// The dst y.
        /// </param>
        /// <param name="srcX">
        /// The src x.
        /// </param>
        /// <param name="srcY">
        /// The src y.
        /// </param>
        /// <param name="dstWidth">
        /// The dst width.
        /// </param>
        /// <param name="dstHeight">
        /// The dst height.
        /// </param>
        /// <returns>
        /// Returns the copied Bitmap.
        /// </returns>
        private static Bitmap ImageCopy(
            Image srcBitmap, float dstX, float dstY, float srcX, float srcY, float dstWidth, float dstHeight)
        {
            // Create the new bitmap and associated graphics object
            RectangleF sourceRec = new RectangleF(srcX, srcY, dstWidth, dstHeight);
            RectangleF destRec = new RectangleF(dstX, dstY, dstWidth, dstHeight);
            Bitmap bmp = new Bitmap(Convert.ToInt32(dstWidth), Convert.ToInt32(dstHeight));
            Graphics g = Graphics.FromImage(bmp);

            g.DrawImage(srcBitmap, destRec, sourceRec, GraphicsUnit.Pixel);
            g.Dispose();

            return bmp;
        }

        /// <summary>Method to rotate an image either clockwise or counter-clockwise.</summary>
        /// <param name="img">
        /// the image to be rotated.
        /// </param>
        /// <param name="rotationAngle">
        /// the angle (in degrees).
        ///   Positive values will rotate clockwise
        ///   negative values will rotate counter-clockwise.
        /// </param>
        /// <returns>
        /// Returns the Rotated Image.
        /// </returns>
        private static Image RotateImage(Image img, double rotationAngle)
        {
            Bitmap returnBitmap = new Bitmap(img.Width, img.Height + 1);

            Graphics g = Graphics.FromImage(returnBitmap);

            g.TranslateTransform((float)img.Width / 2, (float)img.Height / 2);
            g.RotateTransform((float)rotationAngle);
            g.TranslateTransform(-(float)img.Width / 2, -(float)img.Height / 2);

            g.DrawImage(img, (img.Width / 2) - (img.Height / 2), (img.Height / 2) - (img.Width / 2), img.Height, img.Width);

            return returnBitmap;
        }

        private static string CleanName(string name)
        {
			Logger.LogDebug(">>> Entering CleanName(name={Name})", name);
            name = name.Replace(@"\", "/");
            if (name.Contains("/"))
			Logger.LogDebug("CleanName(name={Name}): name → {Name1}", name, name);
            {
                name = name.Substring(name.LastIndexOf('/') + 1);
            }

			Logger.LogDebug("<<< Exiting CleanName(name={Name})", name);
            return name;
        }

        private bool HasWritePermission(string relativePath)
        {
			Logger.LogDebug(">>> Entering HasWritePermission(relativePath={RelativePath})", relativePath);
            var portalId = PortalSettings.Current.PortalId;
            return FolderPermissionController.HasFolderPermission(portalId, relativePath, "WRITE");
        }

        /// <summary>The resize image.</summary>
        /// <param name="img">
        /// The img.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <returns>
        /// Returns the Resized Bitmap.
        /// </returns>
        private Bitmap ResizeImage(Image img, int width, int height)
        {
            Image.GetThumbnailImageAbort callback = this.GetThumbAbort;
            return (Bitmap)img.GetThumbnailImage(width, height, callback, IntPtr.Zero);
        }
    }
}
