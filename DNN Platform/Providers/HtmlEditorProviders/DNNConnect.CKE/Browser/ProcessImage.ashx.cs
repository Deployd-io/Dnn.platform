// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Browser;

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
    /// <summary>Gets a value indicating whether IsReusable.</summary>
    public bool IsReusable => false;

    /// <summary>The get thumb abort.</summary>
    /// <returns>Returns abort.</returns>
    public bool GetThumbAbort() => false;

    /// <summary>The process request.</summary>
    /// <param name="context">The context.</param>
    public void ProcessRequest(HttpContext context)
    {
		eventLogger.LogDebug(">>> Entering ProcessRequest(context={Context})", context);
        var imageH = float.Parse(context.Request["imageH"]);
        var imageW = float.Parse(context.Request["imageW"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): imageW → {ImageW}", context, imageW);
        var angle = float.Parse(context.Request["imageRotate"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): angle → {Angle}", context, angle);
        var sourceImageId = int.Parse(context.Request["fileId"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): sourceImageId → {SourceImageId}", context, sourceImageId);
        var imageX = float.Parse(context.Request["imageX"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): imageX → {ImageX}", context, imageX);
        var imageY = float.Parse(context.Request["imageY"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): imageY → {ImageY}", context, imageY);
        var selectorH = float.Parse(context.Request["selectorH"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): selectorH → {SelectorH}", context, selectorH);
        var selectorW = float.Parse(context.Request["selectorW"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): selectorW → {SelectorW}", context, selectorW);
        var selectorX = float.Parse(context.Request["selectorX"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): selectorX → {SelectorX}", context, selectorX);
        var selectorY = float.Parse(context.Request["selectorY"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): selectorY → {SelectorY}", context, selectorY);
        var viewPortH = float.Parse(context.Request["viewPortH"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): viewPortH → {ViewPortH}", context, viewPortH);
        var viewPortW = float.Parse(context.Request["viewPortW"]);
		eventLogger.LogDebug("ProcessRequest(context={Context}): viewPortW → {ViewPortW}", context, viewPortW);
		eventLogger.LogDebug("ProcessRequest(context={Context}): imageH → {ImageH}", context, imageH);

        bool bSaveFile;
        try
        {
            bSaveFile = bool.Parse(context.Request["saveFile"]);
        }
        catch (Exception)
        {
			eventLogger.LogError("Exception in ProcessRequest(context={Context}): {Arg1}", context, Exception.Message);
            bSaveFile = false;
        }

        string sNewFileName = null;
		eventLogger.LogDebug("ProcessRequest(context={Context}): sNewFileName → {SNewFileName}", context, sNewFileName);
        if (!string.IsNullOrEmpty(context.Request["newFileName"]))
        {
            sNewFileName = context.Request["newFileName"];
        }

        var pWidth = imageW;
		eventLogger.LogDebug("ProcessRequest(context={Context}): pWidth → {PWidth}", context, pWidth);
        var pHeight = imageH;
		eventLogger.LogDebug("ProcessRequest(context={Context}): pHeight → {PHeight}", context, pHeight);

        var file = FileManager.Instance.GetFile(sourceImageId);
		eventLogger.LogDebug("ProcessRequest(context={Context}): file → {File}", context, file);
        if (file == null)
        {
            return;
        }

        var sourceFolder = FolderManager.Instance.GetFolder(file.FolderId);
		eventLogger.LogDebug("ProcessRequest(context={Context}): sourceFolder → {SourceFolder}", context, sourceFolder);
        if (sourceFolder == null)
        {
            return;
        }

        if (PortalSettings.Current != null
            && !HasReadPermission(sourceFolder))
        {
			eventLogger.LogWarning("Throwing SecurityException from ProcessRequest(context={Context})", context);
            throw new SecurityException("You don't have read permission to view files under this folder.");
        }

        var img = (Bitmap)Image.FromStream(FileManager.Instance.GetFileContent(file));
		eventLogger.LogDebug("ProcessRequest(context={Context}): img → {Img}", context, img);

        // Resize
        var imageP = this.ResizeImage(img, Convert.ToInt32(pWidth), Convert.ToInt32(pHeight));
		eventLogger.LogDebug("ProcessRequest(context={Context}): imageP → {ImageP}", context, imageP);

        // Rotate if angle is not 0.00 or 360
        if (angle is > 0.0F and < 360.00F)
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
            dstX = 0;
        }
        else
		eventLogger.LogDebug("ProcessRequest(context={Context}): dstX → {DstX}", context, dstX);
        {
            srcX = 0;
            dstX = imageX + ((imageW - pWidth) / 2);
        }

        if (pHeight > viewPortH)
        {
            srcY = Math.Abs(imageY - Math.Abs((imageH - pHeight) / 2));
            dstY = 0;
        }
        else
		eventLogger.LogDebug("ProcessRequest(context={Context}): dstY → {DstY}", context, dstY);
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

            if (PortalSettings.Current != null
                && !HasWritePermission(sourceFolder))
            {
                throw new SecurityException("You don't have write permission to save files under this folder.");
            }

            using var stream = new MemoryStream();
			eventLogger.LogDebug("ProcessRequest(context={Context}): stream → {Stream}", context, stream);
            var fileName = GenerateName(file, sNewFileName);
			eventLogger.LogDebug("ProcessRequest(context={Context}): fileName → {FileName}", context, fileName);
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
		eventLogger.LogDebug("<<< Exiting ProcessRequest(context={Context})", context);
    }

    /// <summary>Generates the New File Path.</summary>
    /// <param name="file">The Original Image.</param>
    /// <param name="sNewFileName">New File Name for the Image.</param>
    /// <returns>The generated name.</returns>
    private static string GenerateName(IFileInfo file, string sNewFileName)
    {
		eventLogger.LogDebug(">>> Entering GenerateName(file={File},sNewFileName={SNewFileName})", file, sNewFileName);
        var sNewFilePath = !string.IsNullOrEmpty(sNewFileName)
            ? $"{CleanName(sNewFileName)}.{file.Extension}"
            : $"{Path.GetFileNameWithoutExtension(sNewFileName)}_crop.{file.Extension}";
			eventLogger.LogDebug("GenerateName(file={File},sNewFileName={SNewFileName}): sNewFilePath → {SNewFilePath}", file, sNewFileName, sNewFilePath);

        var iCounter = 0;

        var folder = FolderManager.Instance.GetFolder(file.FolderId);
		eventLogger.LogDebug("GenerateName(file={File},sNewFileName={SNewFileName}): folder → {Folder}", file, sNewFileName, folder);
        while (FileManager.Instance.FileExists(folder, sNewFilePath))
        {
            iCounter++;

            var sFileNameNoExt = Path.GetFileNameWithoutExtension(sNewFilePath);
			eventLogger.LogDebug("GenerateName(file={File},sNewFileName={SNewFileName}): sFileNameNoExt → {SFileNameNoExt}", file, sNewFileName, sFileNameNoExt);
            sNewFilePath = $"{sFileNameNoExt}_{iCounter}.{file.Extension}";
        }

		eventLogger.LogDebug("<<< Exiting GenerateName(file={File},sNewFileName={SNewFileName})", file, sNewFileName);
        return sNewFilePath;
    }

    /// <summary>The image copy.</summary>
    /// <param name="srcBitmap">The src bitmap.</param>
    /// <param name="dstX">The dst x.</param>
    /// <param name="dstY">The dst y.</param>
    /// <param name="srcX">The src x.</param>
    /// <param name="srcY">The src y.</param>
    /// <param name="dstWidth">The dst width.</param>
    /// <param name="dstHeight">The dst height.</param>
    /// <returns>Returns the copied Bitmap.</returns>
    private static Bitmap ImageCopy(
        Image srcBitmap, float dstX, float dstY, float srcX, float srcY, float dstWidth, float dstHeight)
    {
		eventLogger.LogDebug(">>> Entering ImageCopy(srcBitmap={SrcBitmap},dstX={DstX},dstY={DstY},srcX={SrcX},srcY={SrcY},dstWidth={DstWidth},dstHeight={DstHeight})", srcBitmap, dstX, dstY, srcX, srcY, dstWidth, dstHeight);
        // Create the new bitmap and associated graphics object
        var sourceRec = new RectangleF(srcX, srcY, dstWidth, dstHeight);
		eventLogger.LogDebug("ImageCopy(srcBitmap={SrcBitmap},dstX={DstX},dstY={DstY},srcX={SrcX},srcY={SrcY},dstWidth={DstWidth},dstHeight={DstHeight}): sourceRec → {SourceRec}", srcBitmap, dstX, dstY, srcX, srcY, dstWidth, dstHeight, sourceRec);
        var destRec = new RectangleF(dstX, dstY, dstWidth, dstHeight);
		eventLogger.LogDebug("ImageCopy(srcBitmap={SrcBitmap},dstX={DstX},dstY={DstY},srcX={SrcX},srcY={SrcY},dstWidth={DstWidth},dstHeight={DstHeight}): destRec → {DestRec}", srcBitmap, dstX, dstY, srcX, srcY, dstWidth, dstHeight, destRec);
        var bmp = new Bitmap(Convert.ToInt32(dstWidth), Convert.ToInt32(dstHeight));
		eventLogger.LogDebug("ImageCopy(srcBitmap={SrcBitmap},dstX={DstX},dstY={DstY},srcX={SrcX},srcY={SrcY},dstWidth={DstWidth},dstHeight={DstHeight}): bmp → {Bmp}", srcBitmap, dstX, dstY, srcX, srcY, dstWidth, dstHeight, bmp);
        var g = Graphics.FromImage(bmp);
		eventLogger.LogDebug("ImageCopy(srcBitmap={SrcBitmap},dstX={DstX},dstY={DstY},srcX={SrcX},srcY={SrcY},dstWidth={DstWidth},dstHeight={DstHeight}): g → {G}", srcBitmap, dstX, dstY, srcX, srcY, dstWidth, dstHeight, g);

        g.DrawImage(srcBitmap, destRec, sourceRec, GraphicsUnit.Pixel);
        g.Dispose();

		eventLogger.LogDebug("<<< Exiting ImageCopy(srcBitmap={SrcBitmap},dstX={DstX},dstY={DstY},srcX={SrcX},srcY={SrcY},dstWidth={DstWidth},dstHeight={DstHeight})", srcBitmap, dstX, dstY, srcX, srcY, dstWidth, dstHeight);
        return bmp;
    }

    /// <summary>Method to rotate an image either clockwise or counter-clockwise.</summary>
    /// <param name="img">the image to be rotated.</param>
    /// <param name="rotationAngle">
    /// the angle (in degrees).
    ///   Positive values will rotate clockwise
    ///   negative values will rotate counter-clockwise.
    /// </param>
    /// <returns>Returns the Rotated Image.</returns>
    private static Image RotateImage(Image img, double rotationAngle)
    {
		eventLogger.LogDebug(">>> Entering RotateImage(img={Img},rotationAngle={RotationAngle})", img, rotationAngle);
        var returnBitmap = new Bitmap(img.Width, img.Height + 1);

		eventLogger.LogDebug("RotateImage(img={Img},rotationAngle={RotationAngle}): returnBitmap → {ReturnBitmap}", img, rotationAngle, returnBitmap);
        var g = Graphics.FromImage(returnBitmap);
		eventLogger.LogDebug("RotateImage(img={Img},rotationAngle={RotationAngle}): g → {G}", img, rotationAngle, g);

        g.TranslateTransform((float)img.Width / 2, (float)img.Height / 2);
        g.RotateTransform((float)rotationAngle);
        g.TranslateTransform(-(float)img.Width / 2, -(float)img.Height / 2);

        g.DrawImage(img, (img.Width / 2) - (img.Height / 2), (img.Height / 2) - (img.Width / 2), img.Height, img.Width);

		eventLogger.LogDebug("<<< Exiting RotateImage(img={Img},rotationAngle={RotationAngle})", img, rotationAngle);
        return returnBitmap;
    }

    private static string CleanName(string name)
    {
		eventLogger.LogDebug(">>> Entering CleanName(name={Name})", name);
        name = name.Replace(@"\", "/");
        if (name.Contains("/"))
        {
			eventLogger.LogDebug("CleanName(name={Name}): name → {Name1}", name, name);
            name = name.Substring(name.LastIndexOf('/') + 1);
        }

		eventLogger.LogDebug("<<< Exiting CleanName(name={Name})", name);
        return name;
    }

    private static bool HasReadPermission(IFolderInfo folder)
    {
		eventLogger.LogDebug(">>> Entering HasReadPermission(folder={Folder})", folder);
		eventLogger.LogDebug("<<< Exiting HasReadPermission(folder={Folder})", folder);
        return folder != null && FolderPermissionController.HasFolderPermission(folder.FolderPermissions, "READ");
    }

    private static bool HasWritePermission(IFolderInfo folder)
    {
		eventLogger.LogDebug(">>> Entering HasWritePermission(folder={Folder})", folder);
		eventLogger.LogDebug("<<< Exiting HasWritePermission(folder={Folder})", folder);
        return folder != null && FolderPermissionController.HasFolderPermission(folder.FolderPermissions, "WRITE");
    }

    /// <summary>The resize image.</summary>
    /// <param name="img">The img.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <returns>Returns the Resized Bitmap.</returns>
    private Bitmap ResizeImage(Image img, int width, int height)
    {
		eventLogger.LogDebug(">>> Entering ResizeImage(img={Img},width={Width},height={Height})", img, width, height);
        Image.GetThumbnailImageAbort callback = this.GetThumbAbort;
		eventLogger.LogDebug("<<< Exiting ResizeImage(img={Img},width={Width},height={Height})", img, width, height);
        return (Bitmap)img.GetThumbnailImage(width, height, callback, IntPtr.Zero);
    }
}
