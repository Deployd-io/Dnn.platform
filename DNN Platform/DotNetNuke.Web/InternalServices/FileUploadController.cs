// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Abstractions.Security;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;

using Microsoft.Extensions.DependencyInjection;

using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

/// <summary>A web API for uploading files.</summary>
[DnnAuthorize]
public class FileUploadController : DnnApiController
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileUploadController));
    private static readonly Regex UserFolderEx = new Regex(@"users/\d+/\d+/(\d+)/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly List<string> ImageExtensions = Globals.ImageFileTypes.Split(',').ToList();

    private readonly IHostSettings hostSettings;
    private readonly ICryptographyProvider cryptographyProvider;
    private readonly IPortalController portalController;
    private readonly IApplicationStatusInfo appStatus;
    private readonly IPortalGroupController portalGroupController;

    /// <summary>Initializes a new instance of the <see cref="FileUploadController"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
    public FileUploadController()
        : this(null, null, null, null, null)
    {
	Logger.LogDebug(">>> Entering FileUploadController()");
    }

	Logger.LogDebug("<<< Exiting FileUploadController()");
    /// <summary>Initializes a new instance of the <see cref="FileUploadController"/> class.</summary>
    /// <param name="hostSettings">The host settings.</param>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
    public FileUploadController(IHostSettings hostSettings)
        : this(hostSettings, null, null, null, null)
    {
	Logger.LogDebug(">>> Entering FileUploadController(hostSettings={HostSettings})", hostSettings);
	Logger.LogDebug("<<< Exiting FileUploadController(hostSettings={HostSettings})", hostSettings);
    }

    /// <summary>Initializes a new instance of the <see cref="FileUploadController"/> class.</summary>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="cryptographyProvider">The cryptography provider.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="portalGroupController">The portal group controller.</param>
    public FileUploadController(IHostSettings hostSettings, ICryptographyProvider cryptographyProvider, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
    {
        this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        this.cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
		Logger.LogDebug("FileUploadController(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): cryptographyProvider → {CryptographyProvider1}", hostSettings, cryptographyProvider, portalController, appStatus, portalGroupController, cryptographyProvider);
		Logger.LogDebug("FileUploadController(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): appStatus → {AppStatus1}", hostSettings, cryptographyProvider, portalController, appStatus, portalGroupController, appStatus);
		Logger.LogDebug("FileUploadController(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalGroupController → {PortalGroupController1}", hostSettings, cryptographyProvider, portalController, appStatus, portalGroupController, portalGroupController);
        this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
		Logger.LogDebug("FileUploadController(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalController → {PortalController1}", hostSettings, cryptographyProvider, portalController, appStatus, portalGroupController, portalController);
        this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        this.portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
    }

    /// <summary>Gets the URL for a file.</summary>
    /// <param name="fileId">The ID of the file.</param>
    /// <returns>The URL.</returns>
    public static string GetUrl(int fileId)
    {
		Logger.LogDebug(">>> Entering GetUrl(fileId={FileId})", fileId);
        var file = FileManager.Instance.GetFile(fileId, true);
		Logger.LogDebug("GetUrl(fileId={FileId}): file → {File}", fileId, file);
		Logger.LogDebug("<<< Exiting GetUrl(fileId={FileId})", fileId);
        return FileManager.Instance.GetUrl(file);
    }

    /// <summary>Gets the files in the folder.</summary>
    /// <param name="folderItem">Information about the folder.</param>
    /// <returns>A response with a list of <see cref="FileItem"/> objects.</returns>
    [HttpPost]
    public HttpResponseMessage LoadFiles(FolderItemDTO folderItem)
    {
		Logger.LogDebug(">>> Entering LoadFiles(folderItem={FolderItem})", folderItem);
        var effectivePortalId = this.PortalSettings.PortalId;

		Logger.LogDebug("LoadFiles(folderItem={FolderItem}): effectivePortalId → {EffectivePortalId}", folderItem, effectivePortalId);
        if (folderItem.FolderId <= 0)
        {
            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        var folder = FolderManager.Instance.GetFolder(folderItem.FolderId);
		Logger.LogDebug("LoadFiles(folderItem={FolderItem}): folder → {Folder}", folderItem, folder);
        if (folder == null)
        {
            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        if (IsUserFolder(folder.FolderPath, out var userId))
        {
            var user = UserController.GetUserById(this.hostSettings, effectivePortalId, userId);
			Logger.LogDebug("LoadFiles(folderItem={FolderItem}): user → {User}", folderItem, user);
            if (user is { IsSuperUser: true, })
            {
                effectivePortalId = Null.NullInteger;
            }
            else
            {
                effectivePortalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, effectivePortalId);
            }
        }

        var list = Globals.GetFileList(effectivePortalId, folderItem.FileFilter, !folderItem.Required, folder.FolderPath);
		Logger.LogDebug("LoadFiles(folderItem={FolderItem}): list → {List}", folderItem, list);
        var fileItems = list.OfType<FileItem>().ToList();
		Logger.LogDebug("LoadFiles(folderItem={FolderItem}): fileItems → {FileItems}", folderItem, fileItems);

		Logger.LogDebug("<<< Exiting LoadFiles(folderItem={FolderItem})", folderItem);
        return this.Request.CreateResponse(HttpStatusCode.OK, fileItems);
    }

    /// <summary>Gets the URL of an image file.</summary>
    /// <param name="fileId">The file ID.</param>
    /// <returns>A response with either <c>null</c> or the image URL.</returns>
    [HttpGet]
    public HttpResponseMessage LoadImage(string fileId)
    {
		Logger.LogDebug(">>> Entering LoadImage(fileId={FileId})", fileId);
        if (string.IsNullOrEmpty(fileId) || !int.TryParse(fileId, out var file))
        {
            return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        var imageUrl = ShowImage(file);
		Logger.LogDebug("LoadImage(fileId={FileId}): imageUrl → {ImageUrl}", fileId, imageUrl);
		Logger.LogDebug("<<< Exiting LoadImage(fileId={FileId})", fileId);
        return imageUrl == null
            ? this.Request.CreateResponse(HttpStatusCode.NotFound)
            : this.Request.CreateResponse(HttpStatusCode.OK, imageUrl);
    }

    /// <summary>Uploads an image.</summary>
    /// <returns>A response with an <see cref="SavedFileDTO"/> object.</returns>
    /// <exception cref="HttpResponseException">If the request is not using a multipart MIME content type.</exception>
    [HttpPost]
    [IFrameSupportedValidateAntiForgeryToken]
    public Task<HttpResponseMessage> PostFile()
    {
		Logger.LogDebug(">>> Entering PostFile()");
        var request = this.Request;
        if (!request.Content.IsMimeMultipartContent())
        {
			Logger.LogWarning("Throwing HttpResponseException from PostFile()");
            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        var provider = new MultipartMemoryStreamProvider();
		Logger.LogDebug("PostFile(): provider → {Provider}", provider);

        // local references for use in closure
        IPortalSettings portalSettings = this.PortalSettings;
        var currentSynchronizationContext = SynchronizationContext.Current;
        var userInfo = this.UserInfo;
        var task = request.Content.ReadAsMultipartAsync(provider)
            .ContinueWith(_ =>
            {
                var folder = string.Empty;
				Logger.LogDebug("PostFile(): folder → {Folder}", folder);
                var filter = string.Empty;
				Logger.LogDebug("PostFile(): filter → {Filter}", filter);
                var fileName = string.Empty;
				Logger.LogDebug("PostFile(): fileName → {FileName}", fileName);
                var overwrite = false;
				Logger.LogDebug("PostFile(): overwrite → {Overwrite}", overwrite);
                var isHostMenu = false;
				Logger.LogDebug("PostFile(): isHostMenu → {IsHostMenu}", isHostMenu);
                var extract = false;
				Logger.LogDebug("PostFile(): extract → {Extract}", extract);
                Stream stream = null;
				Logger.LogDebug("PostFile(): stream → {Stream}", stream);
                var returnFileDto = new SavedFileDTO();
				Logger.LogDebug("PostFile(): returnFileDto → {ReturnFileDto}", returnFileDto);

                foreach (var item in provider.Contents)
                {
                    var name = item.Headers.ContentDisposition.Name;
					Logger.LogDebug("PostFile(): name → {Name}", name);
                    switch (name.ToUpperInvariant())
                    {
                        case "\"FOLDER\"":
                            folder = item.ReadAsStringAsync().Result ?? string.Empty;
                            break;

                        case "\"FILTER\"":
                            filter = item.ReadAsStringAsync().Result ?? string.Empty;
                            break;

                        case "\"OVERWRITE\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out overwrite))
                            {
                                overwrite = false;
                            }

                            break;

                        case "\"ISHOSTMENU\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out isHostMenu))
                            {
                                isHostMenu = false;
                            }

                            break;

                        case "\"EXTRACT\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out extract))
                            {
                                extract = false;
                            }

                            break;

                        case "\"POSTFILE\"":
                            fileName = item.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                            if (fileName.IndexOf(@"\", StringComparison.Ordinal) != -1)
                            {
                                fileName = Path.GetFileName(fileName);
                            }

                            stream = item.ReadAsStreamAsync().Result;
                            break;
                    }
                }

                var errorMessage = string.Empty;
                var alreadyExists = false;
                if (!string.IsNullOrEmpty(fileName) && stream != null)
                {
                    // Everything ready

                    // The SynchronizationContext keeps the main thread context. Send method is synchronous
                    currentSynchronizationContext.Send(
                        _ =>
                        {
                            returnFileDto = SaveFile(stream, this.portalController, this.appStatus, this.portalGroupController, this.hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, out alreadyExists, out errorMessage);
                        },
                        null);
                }

                /* Response Content Type cannot be application/json
                 * because IE9 with iframe-transport manages the response
                 * as a file download
                 */
                var mediaTypeFormatter = new JsonMediaTypeFormatter();
				Logger.LogDebug("PostFile(): mediaTypeFormatter → {MediaTypeFormatter}", mediaTypeFormatter);
                mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return this.Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new
                        {
                            AlreadyExists = alreadyExists,
                            Message = string.Format(
                                CultureInfo.CurrentCulture,
                                GetLocalizedString("ErrorMessage"),
                                fileName,
                                errorMessage),
                        },
                        mediaTypeFormatter,
                        "text/plain");
						Logger.LogDebug("PostFile(): AlreadyExists → {AlreadyExists}", AlreadyExists);
						Logger.LogDebug("PostFile(): Message → {Message}", Message);
                }

		Logger.LogDebug("<<< Exiting PostFile()");
                return this.Request.CreateResponse(HttpStatusCode.OK, returnFileDto, mediaTypeFormatter, "text/plain");
            });

        return task;
    }

    /// <summary>Uploads a file to the current portal.</summary>
    /// <returns>A response with a <see cref="FileUploadDto"/> object.</returns>
    [HttpPost]
    [IFrameSupportedValidateAntiForgeryToken]
    [AllowAnonymous]
    public Task<HttpResponseMessage> UploadFromLocal()
    {
		Logger.LogDebug(">>> Entering UploadFromLocal()");
		Logger.LogDebug("<<< Exiting UploadFromLocal()");
        return this.UploadFromLocal(this.PortalSettings.PortalId);
    }

    /// <summary>Uploads a file.</summary>
    /// <param name="portalId">The ID of the portal to which to upload it.</param>
    /// <returns>A response with a <see cref="FileUploadDto"/> object.</returns>
    /// <exception cref="HttpResponseException">If the request is not using a multipart MIME content type.</exception>
    [HttpPost]
    [IFrameSupportedValidateAntiForgeryToken]
    [AllowAnonymous]
    public Task<HttpResponseMessage> UploadFromLocal(int portalId)
    {
        var request = this.Request;
        FileUploadDto result = null;
        if (!request.Content.IsMimeMultipartContent())
        {
            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        if (portalId > -1)
        {
            if (!this.IsPortalIdValid(portalId))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
        }
        else
        {
            portalId = this.PortalSettings.PortalId;
        }

        var provider = new MultipartMemoryStreamProvider();

        // local references for use in closure
        var currentSynchronizationContext = SynchronizationContext.Current;
        var userInfo = this.UserInfo;
        var task = request.Content.ReadAsMultipartAsync(provider)
            .ContinueWith(_ =>
            {
                var folder = string.Empty;
                var filter = string.Empty;
                var fileName = string.Empty;
                var validationCode = string.Empty;
                var overwrite = false;
                var isHostPortal = false;
                var extract = false;
                Stream stream = null;

                foreach (var item in provider.Contents)
                {
                    var name = item.Headers.ContentDisposition.Name;
                    switch (name.ToUpperInvariant())
                    {
                        case "\"FOLDER\"":
                            folder = item.ReadAsStringAsync().Result ?? string.Empty;
                            break;

                        case "\"FILTER\"":
                            filter = item.ReadAsStringAsync().Result ?? string.Empty;
                            break;

                        case "\"OVERWRITE\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out overwrite))
                            {
                                overwrite = false;
                            }

                            break;

                        case "\"ISHOSTPORTAL\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out isHostPortal))
                            {
                                isHostPortal = false;
                            }

                            break;

                        case "\"EXTRACT\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out extract))
                            {
                                extract = false;
                            }

                            break;

                        case "\"PORTALID\"":
                            if (userInfo.IsSuperUser)
                            {
                                var originalPortalId = portalId;
                                if (!int.TryParse(item.ReadAsStringAsync().Result, out portalId))
                                {
                                    portalId = originalPortalId;
                                }
                            }

                            break;
                        case "\"VALIDATIONCODE\"":
                            validationCode = item.ReadAsStringAsync().Result ?? string.Empty;
                            break;
                        case "\"POSTFILE\"":
                            fileName = item.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                            if (fileName.IndexOf(@"\", StringComparison.Ordinal) != -1)
                            {
                                fileName = Path.GetFileName(fileName);
                            }

                            if (!Globals.FileEscapingRegex.IsMatch(fileName))
                            {
                                stream = item.ReadAsStreamAsync().Result;
                            }

                            break;
                    }
                }

                if (!string.IsNullOrEmpty(fileName) && stream != null)
                {
                    // The SynchronizationContext keeps the main thread context. Send method is synchronous
                    currentSynchronizationContext.Send(
                        _ =>
                        {
                            result = UploadFile(this.cryptographyProvider, this.hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode);
                        },
                        null);
                }

                var mediaTypeFormatter = new JsonMediaTypeFormatter();
                mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

                /* Response Content Type cannot be application/json
                 * because IE9 with iframe-transport manages the response
                 * as a file download
                 */
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    result,
                    mediaTypeFormatter,
                    "text/plain");
            });

        return task;
    }

    private static SavedFileDTO SaveFile(
        Stream stream,
        IPortalController portalController,
        IApplicationStatusInfo appStatus,
        IPortalGroupController portalGroupController,
        IHostSettings hostSettings,
        IPortalSettings portalSettings,
        UserInfo userInfo,
        string folder,
        string filter,
        string fileName,
        bool overwrite,
        bool isHostMenu,
        bool extract,
        out bool alreadyExists,
        out string errorMessage)
    {
		Logger.LogDebug(">>> Entering SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract})", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract);
        alreadyExists = false;
        var savedFileDto = new SavedFileDTO();
		Logger.LogDebug("SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract}): savedFileDto → {SavedFileDto}", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, savedFileDto);
		Logger.LogDebug("SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract}): alreadyExists → {AlreadyExists}", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, alreadyExists);
        try
        {
            var extension = Path.GetExtension(fileName).ValueOrEmpty().Replace(".", string.Empty);
			Logger.LogDebug("SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract}): extension → {Extension}", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, extension);
            if (!string.IsNullOrEmpty(filter) && !filter.ToLowerInvariant().Contains(extension.ToLowerInvariant()))
            {
                errorMessage = GetLocalizedString("ExtensionNotAllowed");
				Logger.LogDebug("SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract}): errorMessage → {ErrorMessage}", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, errorMessage);
                return savedFileDto;
            }

            var folderManager = FolderManager.Instance;

            // Check if this is a User Folder
            var effectivePortalId = isHostMenu ? Null.NullInteger : PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, portalSettings.PortalId);
			Logger.LogDebug("SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract}): effectivePortalId → {EffectivePortalId}", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, effectivePortalId);
            var folderInfo = folderManager.GetFolder(effectivePortalId, folder);
			Logger.LogDebug("SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract}): folderInfo → {FolderInfo}", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, folderInfo);
            if (IsUserFolder(folder, out var userId))
            {
                var user = UserController.GetUserById(hostSettings, effectivePortalId, userId);
				Logger.LogDebug("SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract}): user → {User}", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, user);
                if (user != null)
                {
                    folderInfo = folderManager.GetUserFolder(user);
                }
            }

            if (!PortalSecurity.IsInRoles(userInfo, portalSettings, folderInfo.FolderPermissions.ToString("WRITE"))
                && !PortalSecurity.IsInRoles(userInfo, portalSettings, folderInfo.FolderPermissions.ToString("ADD")))
            {
                errorMessage = GetLocalizedString("NoPermission");
                return savedFileDto;
            }

            const bool AlreadyCheckedPermissions = true;
            if (!overwrite && FileManager.Instance.FileExists(folderInfo, fileName, true))
            {
                errorMessage = GetLocalizedString("AlreadyExists");
                alreadyExists = true;
                savedFileDto.FilePath = Path.Combine(folderInfo.PhysicalPath, fileName);
                return savedFileDto;
            }

            var contentType = FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName));
			Logger.LogDebug("SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract}): contentType → {ContentType}", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, contentType);
            var file = FileManager.Instance.AddFile(folderInfo, fileName, stream, true, !AlreadyCheckedPermissions, contentType, userInfo.UserID);
			Logger.LogDebug("SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract}): file → {File}", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, file);

            if (extract && extension.Equals("zip", StringComparison.OrdinalIgnoreCase))
            {
                FileManager.Instance.UnzipFile(file);
                FileManager.Instance.DeleteFile(file);
            }

            errorMessage = string.Empty;
            savedFileDto.FileId = file.FileId.ToString(CultureInfo.InvariantCulture);
            savedFileDto.FilePath = FileManager.Instance.GetUrl(file);
            return savedFileDto;
        }
        catch (InvalidFileExtensionException)
        {
            errorMessage = GetLocalizedString("ExtensionNotAllowed");
            return savedFileDto;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            errorMessage = ex.Message;
		Logger.LogDebug("<<< Exiting SaveFile(stream={Stream},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},hostSettings={HostSettings},portalSettings={PortalSettings},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostMenu={IsHostMenu},extract={Extract})", stream, portalController, appStatus, portalGroupController, hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract);
            return savedFileDto;
        }
    }

    private static string GetLocalizedString(string key)
    {
		Logger.LogDebug(">>> Entering GetLocalizedString(key={Key})", key);
		Logger.LogDebug("<<< Exiting GetLocalizedString(key={Key})", key);
        return Localization.GetString(key, resourceFileRoot: "/App_GlobalResources/FileUpload.resx");
    }

    private static bool IsUserFolder(string folderPath, out int userId)
    {
		Logger.LogDebug(">>> Entering IsUserFolder(folderPath={FolderPath})", folderPath);
        var match = UserFolderEx.Match(folderPath);
        userId = match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : Null.NullInteger;

		Logger.LogDebug("IsUserFolder(folderPath={FolderPath}): userId → {UserId}", folderPath, userId);
		Logger.LogDebug("IsUserFolder(folderPath={FolderPath}): match → {Match}", folderPath, match);
		Logger.LogDebug("<<< Exiting IsUserFolder(folderPath={FolderPath})", folderPath);
        return match.Success;
    }

    private static string ShowImage(int fileId)
    {
		Logger.LogDebug(">>> Entering ShowImage(fileId={FileId})", fileId);
        var image = (FileInfo)FileManager.Instance.GetFile(fileId);

		Logger.LogDebug("ShowImage(fileId={FileId}): image → {Image}", fileId, image);
        if (image != null && IsImageExtension(image.Extension))
        {
            var folder = FolderManager.Instance.GetFolder(image.FolderId);
			Logger.LogDebug("ShowImage(fileId={FileId}): folder → {Folder}", fileId, folder);
            if (folder == null || !FolderPermissionController.CanViewFolder((FolderInfo)folder))
            {
                return null;
            }

            var imageUrl = FileManager.Instance.GetUrl(image);
			Logger.LogDebug("ShowImage(fileId={FileId}): imageUrl → {ImageUrl}", fileId, imageUrl);
			Logger.LogDebug(">>> Entering IsImageExtension(extension={Extension})", extension);
			Logger.LogDebug("<<< Exiting IsImageExtension(extension={Extension})", extension);
		Logger.LogDebug("<<< Exiting ShowImage(fileId={FileId})", fileId);
            return imageUrl;
        }

        return null;
    }

    private static bool IsImageExtension(string extension)
    {
		Logger.LogDebug(">>> Entering IsImageExtension(extension={Extension})", extension);
		Logger.LogDebug("<<< Exiting IsImageExtension(extension={Extension})", extension);
        return ImageExtensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsImage(string fileName)
    {
		Logger.LogDebug(">>> Entering IsImage(fileName={FileName})", fileName);
		Logger.LogDebug("<<< Exiting IsImage(fileName={FileName})", fileName);
        return ImageExtensions.Any(extension => fileName.EndsWith("." + extension, StringComparison.OrdinalIgnoreCase));
    }

    private static FileUploadDto UploadFile(
        ICryptographyProvider cryptographyProvider,
        IHostSettings hostSettings,
        Stream stream,
        int portalId,
        UserInfo userInfo,
        string folder,
        string filter,
        string fileName,
        bool overwrite,
        bool isHostPortal,
        bool extract,
        string validationCode)
    {
		Logger.LogDebug(">>> Entering UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode})", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode);
        var result = new FileUploadDto();
		Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): result → {Result}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, result);
        BinaryReader reader = null;
        Stream fileContent = null;
		Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): fileContent → {FileContent}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, fileContent);
        try
        {
            var extensionList = new List<string>();
			Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): extensionList → {ExtensionList}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, extensionList);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                extensionList = filter.Split(',').Select(i => i.Trim()).ToList();
            }

            var validateParams = new List<object> { extensionList, userInfo.UserID };
            if (!userInfo.IsSuperUser)
            {
                validateParams.Add(portalId);
            }

            if (!ValidationUtils.ValidationCodeMatched(cryptographyProvider, hostSettings, validateParams, validationCode))
            {
				Logger.LogWarning("Throwing InvalidOperationException from UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode})", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode);
                throw new InvalidOperationException("Bad Request");
            }

            var extension = Path.GetExtension(fileName).ValueOrEmpty().Replace(".", string.Empty);
			Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): extension → {Extension}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, extension);
            result.FileIconUrl = IconController.GetFileIconUrl(extension);

            if (!string.IsNullOrEmpty(filter) && !filter.ToLowerInvariant().Contains(extension.ToLowerInvariant()))
            {
                result.Message = GetLocalizedString("ExtensionNotAllowed");
                return result;
            }

            var folderManager = FolderManager.Instance;
            var effectivePortalId = isHostPortal ? Null.NullInteger : portalId;
            var folderInfo = folderManager.GetFolder(effectivePortalId, folder);
			Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): folderInfo → {FolderInfo}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, folderInfo);

            if (folderInfo == null && IsUserFolder(folder, out var userId))
            {
                var user = UserController.GetUserById(hostSettings, effectivePortalId, userId);
				Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): user → {User}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, user);
                if (user != null)
                {
                    folderInfo = folderManager.GetUserFolder(user);
                }
            }

            if (!FolderPermissionController.HasFolderPermission(portalId, folder, "WRITE")
                && !FolderPermissionController.HasFolderPermission(portalId, folder, "ADD"))
            {
                result.Message = GetLocalizedString("NoPermission");
                return result;
            }

            const bool AlreadyCheckedPermissions = true;
            IFileInfo file;
            if (!overwrite && FileManager.Instance.FileExists(folderInfo, fileName, true))
            {
                result.Message = GetLocalizedString("AlreadyExists");
                result.AlreadyExists = true;
				Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): result.AlreadyExists → {Arg12}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, result.AlreadyExists);
                file = FileManager.Instance.GetFile(folderInfo, fileName, true);
                result.FileId = file.FileId;
				Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): file → {File}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, file);
				Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): result.FileId → {Arg12}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, result.FileId);
            }
            else
            {
                file = FileManager.Instance.AddFile(folderInfo, fileName, stream, true, !AlreadyCheckedPermissions, FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName)), userInfo.UserID);
                if (extract && extension.Equals("zip", StringComparison.OrdinalIgnoreCase))
                {
                    var destinationFolder = FolderManager.Instance.GetFolder(file.FolderId);
					Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): destinationFolder → {DestinationFolder}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, destinationFolder);
                    var invalidFiles = new List<string>();
					Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): invalidFiles → {InvalidFiles}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, invalidFiles);
                    var filesCount = FileManager.Instance.UnzipFile(file, destinationFolder, invalidFiles);
					Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): filesCount → {FilesCount}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, filesCount);

                    var invalidFilesJson = invalidFiles.Count > 0
                        ? string.Join(",", invalidFiles.Select(invalidFile => HttpUtility.JavaScriptStringEncode(invalidFile, addDoubleQuotes: true)))
                        : string.Empty;
                    result.Prompt = $"{{\"invalidFiles\":[{invalidFilesJson}], \"totalCount\": {filesCount}}}";
					Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): result.Prompt → {Arg12}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, result.Prompt);
                }

                result.FileId = file.FileId;
            }

            fileContent = FileManager.Instance.GetFileContent(file);

            var path = GetUrl(result.FileId);
			Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): path → {Path}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, path);
            using (reader = new BinaryReader(fileContent))
            {
                Size size;
                if (IsImage(fileName))
                {
                    try
                    {
                        size = ImageHeader.GetDimensions(reader);
                    }
                    catch (ArgumentException exc)
                    {
                        Logger.Warn("Unable to get image dimensions for image file", exc);
                        size = new Size(32, 32);
                    }
                }
                else
                {
                    size = new Size(32, 32);
                }

                result.Orientation = size.Orientation();
            }

            result.Path = result.FileId > 0 ? path : string.Empty;
			Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): result.Path → {Arg12}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, result.Path);
			Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): result.FileName → {Arg12}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, result.FileName);
            result.FileName = fileName;

            if (extract && extension.Equals("zip", StringComparison.OrdinalIgnoreCase))
            {
                FileManager.Instance.DeleteFile(file);
            }

            return result;
        }
        catch (InvalidFileExtensionException)
        {
            result.Message = GetLocalizedString("ExtensionNotAllowed");
            return result;
        }
        catch (Exception exe)
        {
            Logger.Error(exe);
            result.Message = exe.Message;
			Logger.LogDebug("UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode}): result.Message → {Arg12}", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode, result.Message);
            return result;
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }

            if (fileContent != null)
            {
                fileContent.Close();
                fileContent.Dispose();
            }
        }
		Logger.LogDebug("<<< Exiting UploadFile(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},stream={Stream},portalId={PortalId},userInfo={UserInfo},folder={Folder},filter={Filter},fileName={FileName},overwrite={Overwrite},isHostPortal={IsHostPortal},extract={Extract},validationCode={ValidationCode})", cryptographyProvider, hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode);
    }

    private static IPortalInfo[] GetMyPortalGroup()
    {
		Logger.LogDebug(">>> Entering GetMyPortalGroup()");
		Logger.LogDebug("<<< Exiting GetMyPortalGroup()");
        return (
                from @group in PortalGroupController.Instance.GetPortalGroups().ToArray()
                select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId) into portals
                where portals.Any((IPortalInfo x) => x.PortalId == PortalSettings.Current.PortalId)
                select portals.Cast<IPortalInfo>().ToArray())
            .FirstOrDefault();
    }

    private bool IsPortalIdValid(int portalId)
    {
		Logger.LogDebug(">>> Entering IsPortalIdValid(portalId={PortalId})", portalId);
        if (this.UserInfo.IsSuperUser)
        {
            return true;
        }

        if (this.PortalSettings.PortalId == portalId)
        {
            return true;
        }

        var isAdminUser = PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
		Logger.LogDebug("IsPortalIdValid(portalId={PortalId}): isAdminUser → {IsAdminUser}", portalId, isAdminUser);
        if (!isAdminUser)
        {
            return false;
        }

        var myGroup = GetMyPortalGroup();
		Logger.LogDebug("IsPortalIdValid(portalId={PortalId}): myGroup → {MyGroup}", portalId, myGroup);
		Logger.LogDebug("<<< Exiting IsPortalIdValid(portalId={PortalId})", portalId);
        return myGroup != null && myGroup.Any(p => p.PortalId == portalId);
    }

    /// <summary>A data transfer object with information about a folder.</summary>
    public class FolderItemDTO
    {
        /// <summary>Gets or sets the folder ID.</summary>
        public int FolderId { get; set; }

        /// <summary>Gets or sets the file filter.</summary>
        public string FileFilter { get; set; }

        /// <summary>Gets or sets a value indicating whether to include an entry for an unspecified file.</summary>
        public bool Required { get; set; }
    }

    /// <summary>A data transfer object with information about a file that has been saved.</summary>
    public class SavedFileDTO
    {
        /// <summary>Gets or sets the ID of the file.</summary>
        public string FileId { get; set; }

        /// <summary>Gets or sets the path of the file.</summary>
        public string FilePath { get; set; }
    }

    /// <summary>A data transfer object with information about an upload by URL request.</summary>
    public class UploadByUrlDto
    {
        /// <summary>Gets or sets the URL.</summary>
        public string Url { get; set; }

        /// <summary>Gets or sets the destination folder.</summary>
        public string Folder { get; set; }

        /// <summary>Gets or sets a value indicating whether to overwrite an existing file.</summary>
        public bool Overwrite { get; set; }

        /// <summary>Gets or sets a value indicating whether to unzip the resulting file.</summary>
        public bool Unzip { get; set; }

        /// <summary>Gets or sets the filter.</summary>
        public string Filter { get; set; }

        /// <summary>Gets or sets a value indicating whether the request is from the host menu.</summary>
        public bool IsHostMenu { get; set; }

        /// <summary>Gets or sets the portal ID.</summary>
        public int PortalId { get; set; } = -1;

        /// <summary>Gets or sets the validation code.</summary>
        public string ValidationCode { get; set; }
    }

    /// <summary>A data transfer object with information about a file upload.</summary>
    [DataContract]
    public class FileUploadDto
    {
        /// <summary>Gets or sets the file path.</summary>
        [DataMember(Name = "path")]
        public string Path { get; set; }

        /// <summary>Gets or sets the image orientation.</summary>
        [DataMember(Name = "orientation")]
        public Orientation Orientation { get; set; }

        /// <summary>Gets or sets a value indicating whether the file already exists.</summary>
        [DataMember(Name = "alreadyExists")]
        public bool AlreadyExists { get; set; }

        /// <summary>Gets or sets an error message.</summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        /// <summary>Gets or sets the URL of the file type icon.</summary>
        [DataMember(Name = "fileIconUrl")]
        public string FileIconUrl { get; set; }

        /// <summary>Gets or sets the ID of the file.</summary>
        [DataMember(Name = "fileId")]
        public int FileId { get; set; }

        /// <summary>Gets or sets the name of the file.</summary>
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        /// <summary>Gets or sets a JSON string with <c>invalidFiles</c> and <c>totalCount</c> fields.</summary>
        [DataMember(Name = "prompt")]
        public string Prompt { get; set; }
    }
}
