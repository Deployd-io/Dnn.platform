// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Browser;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

using DNNConnect.CKEditorProvider.Constants;
using DNNConnect.CKEditorProvider.Objects;
using DNNConnect.CKEditorProvider.Utilities;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Extensions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;

using Microsoft.Extensions.DependencyInjection;

/// <summary>The File Upload Handler.</summary>
public class FileUploader : IHttpHandler
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <summary>The JavaScript Serializer.</summary>
    private readonly JavaScriptSerializer js = new JavaScriptSerializer();

    private readonly IHostSettings hostSettings;

    private PortalSettings portalSettings;

    /// <summary>Initializes a new instance of the <see cref="FileUploader"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public FileUploader()
        : this(null)
    {
	Logger.LogDebug(">>> Entering FileUploader()");
    }
	Logger.LogDebug("<<< Exiting FileUploader()");

    /// <summary>Initializes a new instance of the <see cref="FileUploader"/> class.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public FileUploader(IHostSettings hostSettings)
    {
        this.hostSettings = hostSettings ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettings>();
		Logger.LogDebug("FileUploader(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
    }

    /// <summary>Gets a value indicating whether another request can use the <see cref="IHttpHandler" /> instance.</summary>
    public bool IsReusable => false;

    /// <summary>Gets a value indicating whether to override files.</summary>
    private static bool OverrideFiles =>
        HttpContext.Current.Request["overrideFiles"].Equals("1")
        || HttpContext.Current.Request["overrideFiles"].Equals("true", StringComparison.InvariantCultureIgnoreCase);

    /// <summary>Gets the storage folder.</summary>
    /// <value>The storage folder.</value>
    private static IFolderInfo StorageFolder =>
        FolderManager.Instance.GetFolder(Convert.ToInt32(HttpContext.Current.Request["storageFolderID"]));

    /// <summary>Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="IHttpHandler" /> interface.</summary>
    /// <param name="context">An <see cref="HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
    public void ProcessRequest(HttpContext context)
    {
		Logger.LogDebug(">>> Entering ProcessRequest(context={Context})", context);
        context.Response.AddHeader("Pragma", "no-cache");
        context.Response.AddHeader("Cache-Control", "private, no-cache");

        this.HandleMethod(context);
		Logger.LogDebug("<<< Exiting ProcessRequest(context={Context})", context);
    }

    /// <summary>Returns the options.</summary>
    /// <param name="context">The context.</param>
    private static void ReturnOptions(HttpContext context)
    {
		Logger.LogDebug(">>> Entering ReturnOptions(context={Context})", context);
        context.Response.AddHeader("Allow", "DELETE,GET,HEAD,POST,PUT,OPTIONS");
        context.Response.StatusCode = 200;
		Logger.LogDebug("<<< Exiting ReturnOptions(context={Context})", context);
    }

    /// <summary>Gets the codec for the given <paramref name="format"/>.</summary>
    /// <param name="format">The image format.</param>
    /// <returns>The codec, or <see langword="null"/>.</returns>
    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
		Logger.LogDebug(">>> Entering GetEncoder(format={Format})", format);
        var codecs = ImageCodecInfo.GetImageDecoders();

		Logger.LogDebug("GetEncoder(format={Format}): codecs → {Codecs}", format, codecs);
		Logger.LogDebug("<<< Exiting GetEncoder(format={Format})", format);
        return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
    }

    private static (int NewWidth, int NewHeight) DetermineNewDimensions(Image uplImage, int maxWidth, int maxHeight)
    {
		Logger.LogDebug(">>> Entering DetermineNewDimensions(NewWidth={NewWidth},NewHeight={NewHeight})", NewWidth, NewHeight);
        // it's too big: we need to resize
        int newWidth, newHeight;

        // which determines the max: height or width?
        var ratioWidth = (double)maxWidth / (double)uplImage.Width;
        var ratioHeight = (double)maxHeight / (double)uplImage.Height;
		Logger.LogDebug("DetermineNewDimensions(NewWidth={NewWidth},NewHeight={NewHeight}): ratioWidth → {RatioWidth}", NewWidth, NewHeight, ratioWidth);
		Logger.LogDebug("DetermineNewDimensions(NewWidth={NewWidth},NewHeight={NewHeight}): ratioHeight → {RatioHeight}", NewWidth, NewHeight, ratioHeight);
        if (ratioWidth < ratioHeight)
        {
            // max width needs to be used
            newWidth = maxWidth;
			Logger.LogDebug("DetermineNewDimensions(NewWidth={NewWidth},NewHeight={NewHeight}): newWidth → {NewWidth1}", NewWidth, NewHeight, newWidth);
            newHeight = (int)Math.Round(uplImage.Height * ratioWidth);
        }
		Logger.LogDebug("DetermineNewDimensions(NewWidth={NewWidth},NewHeight={NewHeight}): newHeight → {NewHeight1}", NewWidth, NewHeight, newHeight);
        else
        {
            // max height needs to be used
            newHeight = maxHeight;
            newWidth = (int)Math.Round(uplImage.Width * ratioHeight);
        }

		Logger.LogDebug("<<< Exiting DetermineNewDimensions(NewWidth={NewWidth},NewHeight={NewHeight})", NewWidth, NewHeight);
        return (newWidth, newHeight);
    }

    private static Stream ResizeImage(Image uplImage, int newWidth, int newHeight)
    {
		Logger.LogDebug(">>> Entering ResizeImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight})", uplImage, newWidth, newHeight);
        // Add Compression to Jpeg Images
        if (uplImage.RawFormat.Equals(ImageFormat.Jpeg))
        {
            return ResizeJpegImage(uplImage, newWidth, newHeight);
        }

        using Image newImage = uplImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);
        var imageFormat = uplImage.RawFormat;
		Logger.LogDebug("ResizeImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight}): newImage → {NewImage}", uplImage, newWidth, newHeight, newImage);

        var fileContents = new MemoryStream();
        newImage.Save(fileContents, imageFormat);
		Logger.LogDebug("ResizeImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight}): fileContents → {FileContents}", uplImage, newWidth, newHeight, fileContents);
		Logger.LogDebug("<<< Exiting ResizeImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight})", uplImage, newWidth, newHeight);
        return fileContents;
    }

    private static Stream ResizeJpegImage(Image uplImage, int newWidth, int newHeight)
    {
		Logger.LogDebug(">>> Entering ResizeJpegImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight})", uplImage, newWidth, newHeight);
        var jpgEncoder = GetEncoder(uplImage.RawFormat);

		Logger.LogDebug("ResizeJpegImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight}): jpgEncoder → {JpgEncoder}", uplImage, newWidth, newHeight, jpgEncoder);
        var myEncoder = Encoder.Quality;
        var encodeParams = new EncoderParameters(1);
        var encodeParam = new EncoderParameter(myEncoder, 80L);
		Logger.LogDebug("ResizeJpegImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight}): encodeParams → {EncodeParams}", uplImage, newWidth, newHeight, encodeParams);
		Logger.LogDebug("ResizeJpegImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight}): encodeParam → {EncodeParam}", uplImage, newWidth, newHeight, encodeParam);
        encodeParams.Param[0] = encodeParam;

        using var destination = new Bitmap(newWidth, newHeight);
        using (var graphic = Graphics.FromImage(destination))
        {
		Logger.LogDebug("ResizeJpegImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight}): destination → {Destination}", uplImage, newWidth, newHeight, destination);
            graphic.SmoothingMode = SmoothingMode.AntiAlias;
			Logger.LogDebug("ResizeJpegImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight}): graphic.SmoothingMode → {Arg3}", uplImage, newWidth, newHeight, graphic.SmoothingMode);
			Logger.LogDebug("ResizeJpegImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight}): graphic.InterpolationMode → {Arg3}", uplImage, newWidth, newHeight, graphic.InterpolationMode);
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.DrawImage(uplImage, 0, 0, destination.Width, destination.Height);
        }

        var fileContents = new MemoryStream();
        destination.Save(fileContents, jpgEncoder, encodeParams);
		Logger.LogDebug("ResizeJpegImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight}): fileContents → {FileContents}", uplImage, newWidth, newHeight, fileContents);
		Logger.LogDebug("<<< Exiting ResizeJpegImage(uplImage={UplImage},newWidth={NewWidth},newHeight={NewHeight})", uplImage, newWidth, newHeight);
        return fileContents;
    }

    /// <summary>Handle request based on method.</summary>
    /// <param name="context">The context.</param>
    private void HandleMethod(HttpContext context)
    {
		Logger.LogDebug(">>> Entering HandleMethod(context={Context})", context);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        switch (context.Request.HttpMethod)
        {
            case "HEAD":
            case "GET":
                /*if (GivenFilename(context))
                {
                    this.DeliverFile(context);
                }
                else
                {
                    ListCurrentFiles(context);
                }*/

                break;

            case "POST":
            case "PUT":
                this.UploadFile(context);
                break;

            case "OPTIONS":
                ReturnOptions(context);
                break;

            default:
                context.Response.ClearHeaders();
                context.Response.StatusCode = 405;
                break;
        }
		Logger.LogDebug("<<< Exiting HandleMethod(context={Context})", context);
		Logger.LogInformation("HandleMethod(context)={Context}: external service call took {Arg1} ms", context, __logStart.ElapsedMilliseconds);
    }

    /// <summary>Uploads the file.</summary>
    /// <param name="context">The context.</param>
    private void UploadFile(HttpContext context)
    {
		Logger.LogDebug(">>> Entering UploadFile(context={Context})", context);
        var statuses = new List<FilesUploadStatus>();

		Logger.LogDebug("UploadFile(context={Context}): statuses → {Statuses}", context, statuses);
        this.UploadWholeFile(context, statuses);

        this.WriteJsonIframeSafe(context, statuses);
		Logger.LogDebug("<<< Exiting UploadFile(context={Context})", context);
    }

    private EditorProviderSettings GetCurrentSettings(HttpContext context)
    {
		Logger.LogDebug(">>> Entering GetCurrentSettings(context={Context})", context);
        var request = context.Request;
        int.TryParse(request.QueryString["PortalID"], out var portalId);
        this.portalSettings = new PortalSettings(portalId);
		Logger.LogDebug("GetCurrentSettings(context={Context}): portalSettings → {PortalSettings}", context, portalSettings);

        if (!Enum.TryParse<SettingsMode>(request.QueryString["mode"], true, out var settingMode))
        {
            settingMode = SettingsMode.Default;
			Logger.LogDebug("GetCurrentSettings(context={Context}): settingMode → {SettingMode}", context, settingMode);
        }

        var providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
        var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];
		Logger.LogDebug("GetCurrentSettings(context={Context}): providerConfiguration → {ProviderConfiguration}", context, providerConfiguration);
		Logger.LogDebug("GetCurrentSettings(context={Context}): objProvider → {ObjProvider}", context, objProvider);
        var settingsDictionary = EditorController.GetEditorHostSettings(this.hostSettings);
        var portalRoles = RoleController.Instance.GetRoles(this.portalSettings.PortalId);
		Logger.LogDebug("GetCurrentSettings(context={Context}): settingsDictionary → {SettingsDictionary}", context, settingsDictionary);
		Logger.LogDebug("GetCurrentSettings(context={Context}): portalRoles → {PortalRoles}", context, portalRoles);

		Logger.LogDebug("GetCurrentSettings(context={Context}): SettingsMode.Default → {Arg1}", context, SettingsMode.Default);
		Logger.LogDebug("GetCurrentSettings(context={Context}): SettingsMode.Host → {Arg1}", context, SettingsMode.Host);
		Logger.LogDebug("GetCurrentSettings(context={Context}): SettingsMode.Portal → {Arg1}", context, SettingsMode.Portal);
		Logger.LogDebug("GetCurrentSettings(context={Context}): SettingsMode.Page → {Arg1}", context, SettingsMode.Page);
		Logger.LogDebug("GetCurrentSettings(context={Context}): SettingsMode.ModuleInstance → {Arg1}", context, SettingsMode.ModuleInstance);
		Logger.LogDebug("<<< Exiting GetCurrentSettings(context={Context})", context);
        return settingMode switch
        {
            SettingsMode.Default =>
                SettingsUtil.GetDefaultSettings(
                    this.portalSettings,
                    this.hostSettings,
                    this.portalSettings.HomeDirectoryMapPath,
                    objProvider.Attributes["ck_configFolder"],
                    portalRoles),
            SettingsMode.Host =>
                SettingsUtil.LoadEditorSettingsByKey(
                    this.portalSettings,
                    new EditorProviderSettings(),
                    settingsDictionary,
                    SettingConstants.HostKey,
                    portalRoles),
            SettingsMode.Portal =>
                SettingsUtil.LoadEditorSettingsByKey(
                    this.portalSettings,
                    new EditorProviderSettings(),
                    settingsDictionary,
                    SettingConstants.PortalKey(portalId),
                    portalRoles),
            SettingsMode.Page =>
                SettingsUtil.LoadEditorSettingsByKey(
                    this.portalSettings,
                    new EditorProviderSettings(),
                    settingsDictionary,
                    $"DNNCKT#{request.QueryString["tabid"]}#",
                    portalRoles),
            SettingsMode.ModuleInstance =>
                SettingsUtil.LoadModuleSettings(
                    this.portalSettings,
                    new EditorProviderSettings(),
                    $"DNNCKMI#{request.QueryString["mid"]}#INS#{request.QueryString["ckId"]}#",
                    int.Parse(request.QueryString["mid"]),
                    portalRoles),
            _ => new EditorProviderSettings(),
        };
    }

    /// <summary>Uploads the whole file.</summary>
    /// <param name="context">The context.</param>
    /// <param name="statuses">The statuses.</param>
    private void UploadWholeFile(HttpContext context, List<FilesUploadStatus> statuses)
    {
        for (var i = 0; i < context.Request.Files.Count; i++)
        {
            var file = context.Request.Files[i];
            if (file is null)
            {
                continue;
            }

            var fileName = Path.GetFileName(file.FileName);

            if (!string.IsNullOrEmpty(fileName))
            {
                // Convert Unicode Chars
                fileName = Utility.ConvertUnicodeChars(fileName);

                // Replace dots in the name with underscores (only one dot can be there... security issue).
                fileName = Regex.Replace(fileName, @"\.(?![^.]*$)", "_", RegexOptions.None);

                // Check for Illegal Chars
                if (Utility.ValidateFileName(fileName))
                {
                    fileName = Utility.CleanFileName(fileName);
                }
            }
            else
            {
                throw new HttpRequestValidationException("File does not have a name");
            }

            if (fileName.Length > 220)
            {
                fileName = fileName.Substring(fileName.Length - 220);
            }

            // file names starting with '\\' may be used for manipulating the filepath and explore vulnerabilities
            fileName = Regex.Replace(fileName, @"^\\+", string.Empty);

            var fileNameNoExtenstion = Path.GetFileNameWithoutExtension(fileName);

            // Rename File if Exists
            if (!OverrideFiles)
            {
                var counter = 0;

                while (File.Exists(Path.Combine(StorageFolder.PhysicalPath, fileName)))
                {
                    counter++;
                    fileName = string.Format(
                        "{0}_{1}{2}",
                        fileNameNoExtenstion,
                        counter,
                        Path.GetExtension(file.FileName));
                }
            }

            var contentType = FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName));
            var userId = UserController.Instance.GetCurrentUserInfo().UserID;

            var disposeStream = false;
            Stream fileContents = null;
            try
            {
                if (!contentType.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                {
                    fileContents = file.InputStream;
                }
                else
                {
                    // it's an image, so we might need to resize
                    var currentSettings = this.GetCurrentSettings(context);

                    int maxWidth = currentSettings.ResizeWidthUpload;
                    int maxHeight = currentSettings.ResizeHeightUpload;
                    if (maxWidth <= 0 && maxHeight <= 0)
                    {
                        fileContents = file.InputStream;
                    }
                    else
                    {
                        // check if the size of the image is within boundaries
                        using var uplImage = Image.FromStream(file.InputStream);
                        if (uplImage.Width <= maxWidth && uplImage.Height <= maxHeight)
                        {
                            // fits within configured maximum dimensions
                            fileContents = file.InputStream;
                        }
                        else
                        {
                            var (newWidth, newHeight) = DetermineNewDimensions(uplImage, maxWidth, maxHeight);

                            disposeStream = true;
                            fileContents = ResizeImage(uplImage, newWidth, newHeight);
                        }
                    }
                }

                FileManager.Instance.AddFile(
                    StorageFolder,
                    fileName,
                    fileContents,
                    OverrideFiles,
                    true,
                    contentType,
                    userId);
            }
            finally
            {
                if (disposeStream)
                {
                    fileContents?.Dispose();
                }
            }

            var fullName = Path.GetFileName(fileName);
            statuses.Add(new FilesUploadStatus(fullName, file.ContentLength));
        }
    }

    /// <summary>Writes the JSON iFrame safe.</summary>
    /// <param name="context">The context.</param>
    /// <param name="statuses">The statuses.</param>
    private void WriteJsonIframeSafe(HttpContext context, List<FilesUploadStatus> statuses)
    {
        context.Response.AddHeader("Vary", "Accept");
        try
        {
            context.Response.ContentType =
                context.Request["HTTP_ACCEPT"].Contains("application/json")
                    ? "application/json"
                    : "text/plain";
        }
        catch
        {
            context.Response.ContentType = "text/plain";
        }

        var jsonObj = this.js.Serialize(statuses.ToArray());
        context.Response.Write(jsonObj);
    }
}
