// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Http;

    using Dnn.Modules.ResourceManager.Components;
    using Dnn.Modules.ResourceManager.Helpers;
    using Dnn.Modules.ResourceManager.Services.Attributes;
    using Dnn.Modules.ResourceManager.Services.Dto;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Assets;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    using CreateNewFolderRequest = Dnn.Modules.ResourceManager.Services.Dto.CreateNewFolderRequest;

    /// <summary>Expose any services via this class. You can keep services in separate classes or all together in one service class.</summary>
    [ResourceManagerExceptionFilter]
    [DnnExceptionFilter]
    [SupportedModules("ResourceManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ItemsController : DnnApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ItemsController));
        private readonly IFolderMappingController folderMappingController = FolderMappingController.Instance;
        private readonly IModuleControlPipeline modulePipeline;
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly Hashtable mappedPathsSupported = new Hashtable();
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private readonly IHostSettings hostSettings;
        private readonly RoleProvider roleProvider;
        private readonly ICryptographyProvider cryptographyProvider;
        private readonly IModuleController moduleController;

        /// <summary>Initializes a new instance of the <see cref="ItemsController"/> class.</summary>
        /// <param name="modulePipeline">An instance of an <see cref="IModuleControlPipeline"/> used to hook into the EditUrl of the webforms folders provider settings UI.</param>
        /// <param name="applicationStatusInfo">The application status info.</param>
        /// <param name="permissionDefinitionService">The permission service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="roleProvider">The role provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public ItemsController(IModuleControlPipeline modulePipeline, IApplicationStatusInfo applicationStatusInfo, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings, RoleProvider roleProvider)
            : this(modulePipeline, applicationStatusInfo, permissionDefinitionService, hostSettings, roleProvider, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ItemsController"/> class.</summary>
        /// <param name="modulePipeline">An instance of an <see cref="IModuleControlPipeline"/> used to hook into the EditUrl of the webforms folders provider settings UI.</param>
        /// <param name="applicationStatusInfo">The application status info.</param>
        /// <param name="permissionDefinitionService">The permission service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="moduleController">The module controller.</param>
        public ItemsController(
            IModuleControlPipeline modulePipeline,
            IApplicationStatusInfo applicationStatusInfo,
            IPermissionDefinitionService permissionDefinitionService,
            IHostSettings hostSettings,
            RoleProvider roleProvider,
            ICryptographyProvider cryptographyProvider,
            IModuleController moduleController)
        {
			Logger.LogError(">>> Entering ItemsController(modulePipeline={ModulePipeline},applicationStatusInfo={ApplicationStatusInfo},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},roleProvider={RoleProvider},cryptographyProvider={CryptographyProvider},moduleController={ModuleController})", modulePipeline, applicationStatusInfo, permissionDefinitionService, hostSettings, roleProvider, cryptographyProvider, moduleController);
			Logger.LogDebug("ItemsController(modulePipeline={ModulePipeline},applicationStatusInfo={ApplicationStatusInfo},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},roleProvider={RoleProvider},cryptographyProvider={CryptographyProvider},moduleController={ModuleController}): applicationStatusInfo → {ApplicationStatusInfo1}", modulePipeline, applicationStatusInfo, permissionDefinitionService, hostSettings, roleProvider, cryptographyProvider, moduleController, applicationStatusInfo);
			Logger.LogDebug("ItemsController(modulePipeline={ModulePipeline},applicationStatusInfo={ApplicationStatusInfo},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},roleProvider={RoleProvider},cryptographyProvider={CryptographyProvider},moduleController={ModuleController}): permissionDefinitionService → {PermissionDefinitionService1}", modulePipeline, applicationStatusInfo, permissionDefinitionService, hostSettings, roleProvider, cryptographyProvider, moduleController, permissionDefinitionService);
			Logger.LogDebug("ItemsController(modulePipeline={ModulePipeline},applicationStatusInfo={ApplicationStatusInfo},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},roleProvider={RoleProvider},cryptographyProvider={CryptographyProvider},moduleController={ModuleController}): hostSettings → {HostSettings1}", modulePipeline, applicationStatusInfo, permissionDefinitionService, hostSettings, roleProvider, cryptographyProvider, moduleController, hostSettings);
            this.modulePipeline = modulePipeline;
			Logger.LogWarning("<<< Exiting ItemsController(modulePipeline={ModulePipeline},applicationStatusInfo={ApplicationStatusInfo},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},roleProvider={RoleProvider},cryptographyProvider={CryptographyProvider},moduleController={ModuleController})", modulePipeline, applicationStatusInfo, permissionDefinitionService, hostSettings, roleProvider, cryptographyProvider, moduleController);
			Logger.LogDebug("ItemsController(modulePipeline={ModulePipeline},applicationStatusInfo={ApplicationStatusInfo},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},roleProvider={RoleProvider},cryptographyProvider={CryptographyProvider},moduleController={ModuleController}): this.modulePipeline → {Arg7}", modulePipeline, applicationStatusInfo, permissionDefinitionService, hostSettings, roleProvider, cryptographyProvider, moduleController, this.modulePipeline);
            this.applicationStatusInfo = applicationStatusInfo;
            this.permissionDefinitionService = permissionDefinitionService;
            this.hostSettings = hostSettings;
			Logger.LogDebug("ItemsController(modulePipeline={ModulePipeline},applicationStatusInfo={ApplicationStatusInfo},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},roleProvider={RoleProvider},cryptographyProvider={CryptographyProvider},moduleController={ModuleController}): this.applicationStatusInfo → {Arg7}", modulePipeline, applicationStatusInfo, permissionDefinitionService, hostSettings, roleProvider, cryptographyProvider, moduleController, this.applicationStatusInfo);
            this.roleProvider = roleProvider;
            this.cryptographyProvider = cryptographyProvider ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<ICryptographyProvider>();
			Logger.LogDebug("ItemsController(modulePipeline={ModulePipeline},applicationStatusInfo={ApplicationStatusInfo},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},roleProvider={RoleProvider},cryptographyProvider={CryptographyProvider},moduleController={ModuleController}): this.permissionDefinitionService → {Arg7}", modulePipeline, applicationStatusInfo, permissionDefinitionService, hostSettings, roleProvider, cryptographyProvider, moduleController, this.permissionDefinitionService);
            this.moduleController = moduleController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IModuleController>();
        }

        /// <summary>Gets the content for a specific folder.</summary>
        /// <param name="folderId">The id of the folder.</param>
        /// <param name="startIndex">The page number to get.</param>
        /// <param name="numItems">How many items to get per page.</param>
        /// <param name="sorting">How to sort the list.</param>
        /// <returns>
        /// An object containing the folder information, a list of the folder contents and the permissions relating to that folder.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetFolderContent(int folderId, int startIndex, int numItems, string sorting)
        {
			Logger.LogError(">>> Entering GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sorting={Sorting})", folderId, startIndex, numItems, sorting);
            Logger.LogWarning("<<< Exiting GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sorting={Sorting})", folderId, startIndex, numItems, sorting);
			Logger.LogWarning("<<< Exiting GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sorting={Sorting},sortingOrder={SortingOrder})", folderId, startIndex, numItems, sorting, sortingOrder);
            return this.GetFolderContent(folderId, startIndex, numItems, sorting, "Ascending");
        }

        /// <summary>Gets the content for a specific folder.</summary>
        /// <param name="folderId">The id of the folder.</param>
        /// <param name="startIndex">The page number to get.</param>
        /// <param name="numItems">How many items to get per page.</param>
        /// <param name="sorting">How to sort the list.</param>
        /// <param name="sortingOrder">The order to sort the list.</param>
        /// <returns>
        /// An object containing the folder information, a list of the folder contents and the permissions relating to that folder.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetFolderContent(int folderId, int startIndex, int numItems, string sorting, string sortingOrder)
        {
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
            var moduleMode = new SettingsManager(this.moduleController, moduleId, groupId).Mode;
            var permissionsManager = PermissionsManager.Instance;

            var p = ItemsManager.Instance.GetFolderContent(folderId, startIndex, numItems, sorting, sortingOrder, moduleMode);

            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                folder = new
                {
                    folderId = p.Folder.FolderID,
                    folderName = p.Folder.FolderName,
                    folderMappingId = p.Folder.FolderMappingID,
                    folderPath = p.Folder.FolderPath,
                    folderParentId = p.Folder.ParentID,
                },
                items = p.Items.Select(this.GetItemViewModel),
                totalCount = p.TotalCount,
                hasAddFilesPermission = permissionsManager.HasAddFilesPermission(moduleMode, folderId),
                hasAddFoldersPermission = permissionsManager.HasAddFoldersPermission(moduleMode, folderId),
                hasDeletePermission = permissionsManager.HasDeletePermission(moduleMode, folderId),
                hasManagePermission = permissionsManager.HasManagePermission(moduleMode, folderId),
            });
        }

        /// <summary>Gets the module settings.</summary>
        /// <returns><see cref="SettingsManager"/>.</returns>
        [HttpGet]
        public IHttpActionResult GetSettings()
        {
			Logger.LogError(">>> Entering GetSettings()");
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
			Logger.LogError("GetSettings(): moduleId → {ModuleId}", moduleId);
            var settings = new SettingsManager(this.moduleController, moduleId, groupId);
			Logger.LogError("GetSettings(): settings → {Settings}", settings);
			Logger.LogError("GetSettings(): groupId → {GroupId}", groupId);
			Logger.LogWarning("<<< Exiting GetSettings()");
            return this.Ok(settings);
        }

        /// <summary>Gets an item representation for a provided folder ID.</summary>
        /// <param name="folderId">The ID of the folder to get.</param>
        /// <returns>An Item viewmodel.</returns>
        [HttpGet]
        public IHttpActionResult GetFolderItem(int folderId)
        {
			Logger.LogError(">>> Entering GetFolderItem(folderId={FolderId})", folderId);
            var folder = FolderManager.Instance.GetFolder(folderId);
            var item = this.GetItemViewModel(folder);
			Logger.LogError("GetFolderItem(folderId={FolderId}): item → {Item}", folderId, item);
			Logger.LogError("GetFolderItem(folderId={FolderId}): folder → {Folder}", folderId, folder);
			Logger.LogWarning("<<< Exiting GetFolderItem(folderId={FolderId})", folderId);
            return this.Ok(item);
        }

        /// <summary>Syncs the folder content.</summary>
        /// <param name="folderId">The folder id.</param>
        /// <param name="numItems">The number of items.</param>
        /// <param name="sorting">The sorting.</param>
        /// <param name="recursive">If true sync recursively.</param>
        /// <returns>The http response message.</returns>
        [HttpGet]
        public HttpResponseMessage SyncFolderContent(int folderId, int numItems, string sorting, bool recursive)
        {
			Logger.LogError(">>> Entering SyncFolderContent(folderId={FolderId},numItems={NumItems},sorting={Sorting},recursive={Recursive})", folderId, numItems, sorting, recursive);
            var folder = FolderManager.Instance.GetFolder(folderId);
            FolderManager.Instance.Synchronize(folder.PortalID, folder.FolderPath, recursive, true);
			Logger.LogError("SyncFolderContent(folderId={FolderId},numItems={NumItems},sorting={Sorting},recursive={Recursive}): folder → {Folder}", folderId, numItems, sorting, recursive, folder);
			Logger.LogWarning("<<< Exiting SyncFolderContent(folderId={FolderId},numItems={NumItems},sorting={Sorting},recursive={Recursive})", folderId, numItems, sorting, recursive);
            return this.GetFolderContent(folderId, 0, numItems, sorting);
        }

        /// <summary>Download thumbnail.</summary>
        /// <param name="item">The thumbnail download request.</param>
        /// <returns>The http response message.</returns>
        [HttpGet]
        public HttpResponseMessage ThumbnailDownLoad([FromUri] ThumbnailDownloadRequest item)
        {
			Logger.LogError(">>> Entering ThumbnailDownLoad(item={Item})", item);
            var file = FileManager.Instance.GetFile(item.FileId);
			Logger.LogError("ThumbnailDownLoad(item={Item}): file → {File}", item, file);
            if (file == null || !PermissionsManager.Instance.HasGetFileContentPermission(file.FolderId))
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "File doesn't exist." });
            }

            var thumbnailsManager = ThumbnailsManager.Instance;
            var result = new HttpResponseMessage(HttpStatusCode.OK);
			Logger.LogError("ThumbnailDownLoad(item={Item}): result → {Result}", item, result);
			Logger.LogDebug("ThumbnailDownLoad(item={Item}): result.Content → {Arg1}", item, result.Content);
            var thumbnail = thumbnailsManager.GetThumbnailContent(file, item.Width, item.Height, true);
			Logger.LogError("ThumbnailDownLoad(item={Item}): thumbnail → {Thumbnail}", item, thumbnail);
            result.Content = thumbnail.Content;
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(thumbnail.ContentType);
			Logger.LogWarning("<<< Exiting ThumbnailDownLoad(item={Item})", item);
            return result;
        }

        /// <summary>Downloads a file.</summary>
        /// <param name="fileId">The id of the file to download.</param>
        /// <param name="forceDownload">
        /// A value indicating whether to force the download.
        /// When true, will download the file as an attachment and ensures the browser won't just render the file if supported.
        /// When false, the browser may render the file instead of downloading it for some formats like pdf or images.
        /// </param>
        /// <returns>The actual requested file.</returns>
        [HttpGet]
        public HttpResponseMessage Download(int fileId, bool forceDownload)
        {
			Logger.LogError(">>> Entering Download(fileId={FileId},forceDownload={ForceDownload})", fileId, forceDownload);
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var streamContent = ItemsManager.Instance.GetFileContent(fileId, out var fileName, out var contentType);
			Logger.LogError("Download(fileId={FileId},forceDownload={ForceDownload}): streamContent → {StreamContent}", fileId, forceDownload, streamContent);
			Logger.LogError("Download(fileId={FileId},forceDownload={ForceDownload}): result → {Result}", fileId, forceDownload, result);
            result.Content = new StreamContent(streamContent);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue(forceDownload ? "attachment" : "inline") { FileName = fileName };
            return result;
        }

        /// <summary>Gets a list of folder mappings.</summary>
        /// <returns>
        /// A list of folder mapping including information such as if the folder mapping is default or not
        /// and a url to edit the folder mapping using the provider settings UI.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetFolderMappings()
        {
			Logger.LogError(">>> Entering GetFolderMappings()");
            var isSuperTab = this.PortalSettings.ActiveTab is { IsSuperTab: true };

            var moduleContext = this.GetModuleContext();
			Logger.LogError("GetFolderMappings(): moduleContext → {ModuleContext}", moduleContext);
            var mappings = FolderMappingController.Instance.GetFolderMappings(
                isSuperTab && this.UserInfo.IsSuperUser ?
                    Null.NullInteger :
                    this.PortalSettings.PortalId);

            var r = from m in mappings
                    select new
                    {
                        m.FolderMappingID,
                        m.MappingName,
                        m.FolderProviderType,
                        IsDefault =
                        m.MappingName == "Standard" || m.MappingName == "Secure" || m.MappingName == "Database",
                        editUrl = this.UserInfo.IsAdmin ?
                            moduleContext.EditUrl(
                                "ItemID",
                                m.FolderMappingID.ToString(),
                                "EditFolderMapping",
                                "mid",
                                this.ActiveModule.ModuleID.ToString())
                            :
							Logger.LogDebug("GetFolderMappings(): editUrl → {EditUrl}", editUrl);
                            string.Empty,
                    };

			Logger.LogWarning("<<< Exiting GetFolderMappings()");
            return this.Request.CreateResponse(HttpStatusCode.OK, r);
        }

        /// <summary>Determines whether the current user has permissions to manage the folder types.</summary>
        /// <returns>Whether the current user has permissions to manage the folder types.</returns>
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage CanManageFolderTypes()
        {
			Logger.LogError(">>> Entering CanManageFolderTypes()");
            var canManage = this.UserInfo.IsSuperUser || this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName);
			Logger.LogError("CanManageFolderTypes(): canManage → {CanManage}", canManage);
			Logger.LogWarning("<<< Exiting CanManageFolderTypes()");
            return this.Request.CreateResponse(HttpStatusCode.OK, canManage);
        }

        /// <summary>Gets a url to add a new folder type.</summary>
        /// <returns>A url to the folder providers control that allows adding a new folder type.</returns>
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        public HttpResponseMessage GetAddFolderTypeUrl()
        {
			Logger.LogError(">>> Entering GetAddFolderTypeUrl()");
            var moduleContext = this.GetModuleContext();
			Logger.LogError("GetAddFolderTypeUrl(): moduleContext → {ModuleContext}", moduleContext);
			Logger.LogWarning("<<< Exiting GetAddFolderTypeUrl()");
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                moduleContext.EditUrl(
                    "ItemID",
                    "-1",
                    "EditFolderMapping",
                    "mid",
                    this.ActiveModule.ModuleID.ToString()));
        }

        /// <summary>Removes a folder type.</summary>
        /// <param name="folderMappingId">The id of an existing folder mapping.</param>
        /// <returns>The id of the recently remove folder type.</returns>
        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveFolderType([FromBody] int folderMappingId)
        {
			Logger.LogError(">>> Entering RemoveFolderType(folderMappingId=<redacted>)");
            this.folderMappingController.DeleteFolderMapping(this.PortalSettings.PortalId, folderMappingId);
			Logger.LogWarning("<<< Exiting RemoveFolderType(folderMappingId=<redacted>)");
            return this.Request.CreateResponse(HttpStatusCode.OK, folderMappingId);
        }

        /// <summary>Attempts to create a new folder.</summary>
        /// <param name="request">The request to add a new folder, <see cref="CreateNewFolderRequest"/>.</param>
        /// <returns>Information about the new folder.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CreateNewFolder(CreateNewFolderRequest request)
        {
			Logger.LogError(">>> Entering CreateNewFolder(request={Request})", request);
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
			Logger.LogError("CreateNewFolder(request={Request}): moduleId → {ModuleId}", request, moduleId);
            var moduleMode = new SettingsManager(this.moduleController, moduleId, groupId).Mode;
			Logger.LogError("CreateNewFolder(request={Request}): moduleMode → {ModuleMode}", request, moduleMode);
			Logger.LogError("CreateNewFolder(request={Request}): groupId → {GroupId}", request, groupId);

            var folder = ItemsManager.Instance.CreateNewFolder(request.FolderName, request.ParentFolderId, request.FolderMappingId, request.MappedName, moduleMode);
			Logger.LogError("CreateNewFolder(request={Request}): folder → {Folder}", request, folder);

			Logger.LogWarning("<<< Exiting CreateNewFolder(request={Request})", request);
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    folder.FolderID,
                    folder.FolderName,
                    IconUrl = GetFolderIconUrl(this.PortalSettings.PortalId, folder.FolderMappingID),
                    folder.FolderMappingID,
                });
        }

        /// <summary>Attempts to delete a folder.</summary>
        /// <param name="request">The request to delete a folder, <see cref="DeleteFolderRequest"/>.</param>
        /// <returns>Ok if succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteFolder(DeleteFolderRequest request)
        {
			Logger.LogError(">>> Entering DeleteFolder(request={Request})", request);
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
			Logger.LogError("DeleteFolder(request={Request}): moduleId → {ModuleId}", request, moduleId);
            var moduleMode = new SettingsManager(this.moduleController, moduleId, groupId).Mode;
			Logger.LogError("DeleteFolder(request={Request}): moduleMode → {ModuleMode}", request, moduleMode);
			Logger.LogError("DeleteFolder(request={Request}): groupId → {GroupId}", request, groupId);

            ItemsManager.Instance.DeleteFolder(request.FolderId, request.UnlinkAllowedStatus, moduleMode);
			Logger.LogWarning("<<< Exiting DeleteFolder(request={Request})", request);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        /// <summary>Attempts to delete a file.</summary>
        /// <param name="request">The file deletion request, <see cref="DeleteFileRequest"/>.</param>
        /// <returns>OK if succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteFile(DeleteFileRequest request)
        {
			Logger.LogError(">>> Entering DeleteFile(request={Request})", request);
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
			Logger.LogError("DeleteFile(request={Request}): moduleId → {ModuleId}", request, moduleId);
            var moduleMode = new SettingsManager(this.moduleController, moduleId, groupId).Mode;
			Logger.LogError("DeleteFile(request={Request}): moduleMode → {ModuleMode}", request, moduleMode);
			Logger.LogError("DeleteFile(request={Request}): groupId → {GroupId}", request, groupId);

            ItemsManager.Instance.DeleteFile(request.FileId, moduleMode, groupId);
			Logger.LogWarning("<<< Exiting DeleteFile(request={Request})", request);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        /// <summary>Performs file search in a folder.</summary>
        /// <param name="folderId">The id of the folder.</param>
        /// <param name="search">The search term.</param>
        /// <param name="pageIndex">The page index to get.</param>
        /// <param name="pageSize">How many items per page.</param>
        /// <param name="sorting">How to sort the results.</param>
        /// <param name="culture">The culture requested.</param>
        /// <returns>A list of the found resources together with the total count of found resources.</returns>
        [HttpGet]
        public HttpResponseMessage Search(int folderId, string search, int pageIndex, int pageSize, string sorting, string culture)
        {
			Logger.LogError(">>> Entering Search(folderId={FolderId},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},culture={Culture})", folderId, search, pageIndex, pageSize, sorting, culture);
            var folder = FolderManager.Instance.GetFolder(folderId);
			Logger.LogError("Search(folderId={FolderId},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},culture={Culture}): folder → {Folder}", folderId, search, pageIndex, pageSize, sorting, culture, folder);
            if (folder == null)
            {
				Logger.LogError("Throwing Exception from Search(folderId={FolderId},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},culture={Culture})", folderId, search, pageIndex, pageSize, sorting, culture);
                throw new Exception(LocalizationHelper.GetString("FolderDoesNotExist.Error"));
            }

            var groupId = this.FindGroupId(this.Request);
			Logger.LogError("Search(folderId={FolderId},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},culture={Culture}): groupId → {GroupId}", folderId, search, pageIndex, pageSize, sorting, culture, groupId);
            var moduleId = this.Request.FindModuleId();
			Logger.LogError("Search(folderId={FolderId},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},culture={Culture}): moduleId → {ModuleId}", folderId, search, pageIndex, pageSize, sorting, culture, moduleId);
            var moduleMode = new SettingsManager(this.moduleController, moduleId, groupId).Mode;
			Logger.LogError("Search(folderId={FolderId},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},culture={Culture}): moduleMode → {ModuleMode}", folderId, search, pageIndex, pageSize, sorting, culture, moduleMode);

            var searchResults = SearchController.Instance.SearchFolderContent(moduleId, folder, true, search, pageIndex, pageSize, sorting, moduleMode, out int totalHits);
			Logger.LogError("Search(folderId={FolderId},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},culture={Culture}): searchResults → {SearchResults}", folderId, search, pageIndex, pageSize, sorting, culture, searchResults);

            var items = (from file in searchResults
                         select this.GetItemViewModel(file)).ToArray();

			Logger.LogWarning("<<< Exiting Search(folderId={FolderId},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},culture={Culture})", folderId, search, pageIndex, pageSize, sorting, culture);
            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                items,
                totalCount = totalHits,
            });
        }

        /// <summary>Gets details about a file.</summary>
        /// <param name="fileId">The id of the file to get the details from.</param>
        /// <returns>Detailed information about the file.</returns>
        [HttpGet]
        [AllowAnonymous]

        public IHttpActionResult GetFileDetails(int fileId)
        {
			Logger.LogError(">>> Entering GetFileDetails(fileId={FileId})", fileId);
            var file = FileManager.Instance.GetFile(fileId);
			Logger.LogError("GetFileDetails(fileId={FileId}): file → {File}", fileId, file);
            if (file == null)
            {
                return this.NotFound();
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogError("GetFileDetails(fileId={FileId}): folder → {Folder}", fileId, folder);
            if (!FolderPermissionController.CanViewFolder((FolderInfo)folder))
            {
                var message = LocalizationHelper.GetString("UserHasNoPermissionToReadFileProperties.Error");
				Logger.LogError("GetFileDetails(fileId={FileId}): message → {Message}", fileId, message);
                return this.InternalServerError(new Exception(message));
            }

            var createdBy = file.CreatedByUser(this.PortalSettings.PortalId);
			Logger.LogError("GetFileDetails(fileId={FileId}): createdBy → {CreatedBy}", fileId, createdBy);
            var lastModifiedBy = file.LastModifiedByUser(this.PortalSettings.PortalId);
			Logger.LogError("GetFileDetails(fileId={FileId}): lastModifiedBy → {LastModifiedBy}", fileId, lastModifiedBy);

			Logger.LogWarning("<<< Exiting GetFileDetails(fileId={FileId})", fileId);
            return this.Ok(new
            {
                fileId = file.FileId,
                fileName = file.FileName,
                title = file.Title,
                description = file.Description ?? string.Empty,
                size = string.Format(new FileSizeFormatProvider(), "{0:fs}", file.Size),
                createdOnDate = file.CreatedOnDate.ToShortDateString(),
                createdBy = createdBy != null ? createdBy.Username : string.Empty,
                lastModifiedOnDate = file.LastModifiedOnDate.ToShortDateString(),
                lastModifiedBy = lastModifiedBy != null ? lastModifiedBy.Username : string.Empty,
                url = FileManager.Instance.GetUrl(file),
                iconUrl = GetFileIconUrl(file.Extension),
            });
        }

        /// <summary>Attempts to save new details about a file.</summary>
        /// <param name="fileDetails">The new file details, <see cref="FileDetailsRequest"/>.</param>
        /// <returns>OK if the request succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IHttpActionResult SaveFileDetails(FileDetailsRequest fileDetails)
        {
			Logger.LogError(">>> Entering SaveFileDetails(fileDetails={FileDetails})", fileDetails);
            var file = FileManager.Instance.GetFile(fileDetails.FileId);
			Logger.LogError("SaveFileDetails(fileDetails={FileDetails}): file → {File}", fileDetails, file);
            if (file == null)
            {
                return this.NotFound();
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogError("SaveFileDetails(fileDetails={FileDetails}): folder → {Folder}", fileDetails, folder);
            if (!FolderPermissionController.CanManageFolder((FolderInfo)folder))
            {
                var message = LocalizationHelper.GetString("UserHasNoPermissionToManageFileProperties.Error");
				Logger.LogError("SaveFileDetails(fileDetails={FileDetails}): message → {Message}", fileDetails, message);
                return this.InternalServerError(new Exception(message));
            }

            ItemsManager.Instance.SaveFileDetails(file, fileDetails);

			Logger.LogWarning("<<< Exiting SaveFileDetails(fileDetails={FileDetails})", fileDetails);
            return this.Ok(new { Status = 0 });
        }

        /// <summary>Gets details about a folder.</summary>
        /// <param name="folderId">The id of the folder from which to get the details.</param>
        /// <returns>Detailed information about the folder.</returns>
        [HttpGet]

        public IHttpActionResult GetFolderDetails(int folderId)
        {
			Logger.LogError(">>> Entering GetFolderDetails(folderId={FolderId})", folderId);
            var folder = FolderManager.Instance.GetFolder(folderId);

			Logger.LogError("GetFolderDetails(folderId={FolderId}): folder → {Folder}", folderId, folder);
            if (!FolderPermissionController.CanManageFolder((FolderInfo)folder))
            {
                var message = LocalizationHelper.GetString("UserHasNoPermissionToManageFolder.Error");
				Logger.LogError("GetFolderDetails(folderId={FolderId}): message → {Message}", folderId, message);
                return this.InternalServerError(new Exception(message));
            }

            var createdBy = folder.CreatedByUser(this.PortalSettings.PortalId);
			Logger.LogError("GetFolderDetails(folderId={FolderId}): createdBy → {CreatedBy}", folderId, createdBy);
            var lastModifiedBy = folder.LastModifiedByUser(this.PortalSettings.PortalId);
			Logger.LogError("GetFolderDetails(folderId={FolderId}): lastModifiedBy → {LastModifiedBy}", folderId, lastModifiedBy);

			Logger.LogWarning("<<< Exiting GetFolderDetails(folderId={FolderId})", folderId);
            return this.Ok(new
            {
                folderId = folder.FolderID,
                folderName = folder.FolderName,
                createdOnDate = folder.CreatedOnDate.ToShortDateString(),
                createdBy = createdBy != null ? createdBy.Username : string.Empty,
                lastModifiedOnDate = folder.LastModifiedOnDate.ToShortDateString(),
                lastModifiedBy = lastModifiedBy != null ? lastModifiedBy.Username : string.Empty,
                type = FolderMappingController.Instance.GetFolderMapping(folder.FolderMappingID).MappingName,
                isVersioned = folder.IsVersioned,
                permissions = this.permissionDefinitionService.GetFolderPermissions(folder.FolderPermissions),
            });
        }

        /// <summary>Attempts to save new details about a folder.</summary>
        /// <param name="folderDetails">The new folder details, <see cref="FolderDetailsRequest"/>.</param>
        /// <returns>OK if the request succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IHttpActionResult SaveFolderDetails(FolderDetailsRequest folderDetails)
        {
			Logger.LogError(">>> Entering SaveFolderDetails(folderDetails={FolderDetails})", folderDetails);
            var folder = FolderManager.Instance.GetFolder(folderDetails.FolderId);
			Logger.LogError("SaveFolderDetails(folderDetails={FolderDetails}): folder → {Folder}", folderDetails, folder);
            if (folder == null)
            {
                return this.NotFound();
            }

            if (!FolderPermissionController.CanManageFolder((FolderInfo)folder))
            {
                var message = LocalizationHelper.GetString("UserHasNoPermissionToManageFolder.Error");
				Logger.LogError("SaveFolderDetails(folderDetails={FolderDetails}): message → {Message}", folderDetails, message);
                return this.InternalServerError(new Exception(message));
            }

            ItemsManager.Instance.SaveFolderDetails(folder, folderDetails);

			Logger.LogWarning("<<< Exiting SaveFolderDetails(folderDetails={FolderDetails})", folderDetails);
            return this.Ok(new { Status = 0 });
        }

        /// <summary>Gets available sorting options.</summary>
        /// <returns>A list of values and labels to populate the search sorting dropdown.</returns>
        [HttpGet]
        public HttpResponseMessage GetSortOptions()
        {
			Logger.LogError(">>> Entering GetSortOptions()");
            var sortOptions = new[]
            {
                new { value = "LastModifiedOnDate", label = LocalizationHelper.GetString("LastModifiedOnDate") },
                new { value = "CreatedOnDate", label = LocalizationHelper.GetString("CreatedOnDate") },
                new { value = "ItemName", label= LocalizationHelper.GetString("ItemName") },
            };
			Logger.LogWarning("<<< Exiting GetSortOptions()");
            return this.Request.CreateResponse(HttpStatusCode.OK, sortOptions);
        }

        /// <summary>Attempts to move a file into a folder.</summary>
        /// <param name="moveFileRequest">The file move request, <see cref="MoveFileRequest"/>.</param>
        /// <returns>A 0 status code if succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MoveFile(MoveFileRequest moveFileRequest)
        {
			Logger.LogError(">>> Entering MoveFile(moveFileRequest={MoveFileRequest})", moveFileRequest);
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
			Logger.LogError("MoveFile(moveFileRequest={MoveFileRequest}): moduleId → {ModuleId}", moveFileRequest, moduleId);
            var moduleMode = new SettingsManager(this.moduleController, moduleId, groupId).Mode;
			Logger.LogError("MoveFile(moveFileRequest={MoveFileRequest}): moduleMode → {ModuleMode}", moveFileRequest, moduleMode);
			Logger.LogError("MoveFile(moveFileRequest={MoveFileRequest}): groupId → {GroupId}", moveFileRequest, groupId);

            ItemsManager.Instance.MoveFile(moveFileRequest.SourceFileId, moveFileRequest.DestinationFolderId, moduleMode, groupId);
			Logger.LogWarning("<<< Exiting MoveFile(moveFileRequest={MoveFileRequest})", moveFileRequest);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        /// <summary>Attempts to move a folder into another folder.</summary>
        /// <param name="moveFolderRequest">The folder move request, <see cref="MoveFolderRequest"/>.</param>
        /// <returns>A 0 status code if succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MoveFolder(MoveFolderRequest moveFolderRequest)
        {
			Logger.LogError(">>> Entering MoveFolder(moveFolderRequest={MoveFolderRequest})", moveFolderRequest);
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
			Logger.LogError("MoveFolder(moveFolderRequest={MoveFolderRequest}): moduleId → {ModuleId}", moveFolderRequest, moduleId);
            var moduleMode = new SettingsManager(this.moduleController, moduleId, groupId).Mode;
			Logger.LogError("MoveFolder(moveFolderRequest={MoveFolderRequest}): moduleMode → {ModuleMode}", moveFolderRequest, moduleMode);
			Logger.LogError("MoveFolder(moveFolderRequest={MoveFolderRequest}): groupId → {GroupId}", moveFolderRequest, groupId);

            ItemsManager.Instance.MoveFolder(moveFolderRequest.SourceFolderId, moveFolderRequest.DestinationFolderId, moduleMode, groupId);
			Logger.LogWarning("<<< Exiting MoveFolder(moveFolderRequest={MoveFolderRequest})", moveFolderRequest);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        /// <summary>Gets the relevant icon for this requested folder.</summary>
        /// <param name="folderId">The ID of the folder for which to get the image for.</param>
        /// <returns>A string representing the full url  to the folder icon.</returns>
        [HttpGet]
        [ValidateAntiForgeryToken]
        public IHttpActionResult GetFolderIconUrl(int folderId)
        {
			Logger.LogError(">>> Entering GetFolderIconUrl(folderId={FolderId})", folderId);
            var folderMappingId = FolderManager.Instance.GetFolder(folderId).FolderMappingID;
            var url = GetFolderIconUrl(this.PortalSettings.PortalId, folderMappingId);
			Logger.LogError("GetFolderIconUrl(folderId={FolderId}): url → {Url}", folderId, url);
			Logger.LogError("GetFolderIconUrl(folderId={FolderId}): folderMappingId → {FolderMappingId}", folderId, folderMappingId);
			Logger.LogWarning("<<< Exiting GetFolderIconUrl(folderId={FolderId})", folderId);
			Logger.LogWarning("<<< Exiting GetFolderIconUrl(portalId={PortalId},folderMappingId=<redacted>)", portalId);
            return this.Ok(new { url });
        }

        /// <summary>Gets a list of role groups.</summary>
        /// <returns>A collection of role groups.</returns>
        [HttpGet]
        [ValidateAntiForgeryToken]
        public IHttpActionResult GetRoleGroups()
        {
			Logger.LogError(">>> Entering GetRoleGroups()");
            if (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                return this.Unauthorized();
            }

            var groups = RoleController.GetRoleGroups(this.roleProvider, this.PortalSettings.PortalId)
                            .Cast<RoleGroupInfo>()
                            .Select(RoleGroupDto.FromRoleGroupInfo);

			Logger.LogWarning("<<< Exiting GetRoleGroups()");
            return this.Ok(groups);
        }

        /// <summary>Gets the roles for a role group.</summary>
        /// <returns>A collection of roles.</returns>
        [HttpGet]
        [ValidateAntiForgeryToken]
        public IHttpActionResult GetRoles()
        {
			Logger.LogError(">>> Entering GetRoles()");
            var matchedRoles = RoleController.Instance.GetRoles(this.PortalSettings.PortalId)
                .Where(r => r.Status == RoleStatus.Approved)
                .Select(r =>
                {
                    r.RoleName = Localization.LocalizeRole(r.RoleName);
                    return r;
                })
                .OrderBy(r => r.RoleName)
                .Select(r => new
                {
                    IsSystemRole = r.IsSystemRole,
                    RoleGroupId = r.RoleGroupID,
                    RoleId = r.RoleID,
                    RoleName = r.RoleName,
                })
                .ToList();

			Logger.LogWarning("<<< Exiting GetRoles()");
            return this.Ok(matchedRoles);
        }

        /// <summary>Gets a list of users that match the search keyword.</summary>
        /// <param name="keyword">The keyword to search for.</param>
        /// <param name="count">The amount of results to return.</param>
        /// <returns>A collection of users.</returns>
        [HttpGet]
        [ValidateAntiForgeryToken]
        public IHttpActionResult GetSuggestionUsers(string keyword, int count)
        {
			Logger.LogError(">>> Entering GetSuggestionUsers(keyword={Keyword},count={Count})", keyword, count);
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return this.Ok(new List<string>());
                }

                var displayMatch = keyword + "%";
				Logger.LogError("GetSuggestionUsers(keyword={Keyword},count={Count}): displayMatch → {DisplayMatch}", keyword, count, displayMatch);
                var totalRecords = 0;
                var totalRecords2 = 0;
                var matchedUsers = UserController.GetUsersByDisplayName(
                    this.PortalSettings.PortalId,
                    displayMatch,
                    0,
                    count,
                    ref totalRecords,
                    false,
                    false);
                matchedUsers.AddRange(
                    UserController.GetUsersByUserName(
                        this.PortalSettings.PortalId,
                        displayMatch,
                        0,
                        count,
                        ref totalRecords2,
                        false,
                        false));
                var finalUsers = matchedUsers
                    .Cast<UserInfo>()
                    .Where(x => x.Membership.Approved)
                    .Select(u => new
                    {
                        userId = u.UserID,
                        displayName = $"{u.DisplayName}",
                    });

                return this.Ok(finalUsers.ToList().GroupBy(x => x.userId).Select(group => group.First()));
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in GetSuggestionUsers(keyword={Keyword},count={Count}): {Arg2}", keyword, count, ex.Message);
				Logger.LogWarning("<<< Exiting GetSuggestionUsers(keyword={Keyword},count={Count})", keyword, count);
                return this.InternalServerError(new Exception(ex.Message));
            }
        }

        /// <summary>Gets the list of possible file extensions as well as the validation code to use for uploads.</summary>
        /// <returns>An object containing the allowed file extensions and the validation code to use for uploads.</returns>
        [HttpGet]
        [ValidateAntiForgeryToken]
        public IHttpActionResult GetAllowedFileExtensions()
        {
			Logger.LogError(">>> Entering GetAllowedFileExtensions()");
            var allowedExtensions = FileManager.Instance.WhiteList.ToStorageString();
			Logger.LogError("GetAllowedFileExtensions(): allowedExtensions → {AllowedExtensions}", allowedExtensions);
            var parameters = new List<object>
            {
                allowedExtensions.Split(',').Select(i => i.Trim()).OrderBy(a => a).ToList(),
                this.PortalSettings.UserInfo.UserID,
            };
            if (!this.UserInfo.IsSuperUser)
            {
                parameters.Add(this.PortalSettings.PortalId);
            }

            var validationCode = ValidationUtils.ComputeValidationCode(this.cryptographyProvider, this.hostSettings, parameters);
			Logger.LogError("GetAllowedFileExtensions(): validationCode → {ValidationCode}", validationCode);

            var maxUploadFileSize = Config.GetMaxUploadSize(this.applicationStatusInfo);
			Logger.LogError("GetAllowedFileExtensions(): maxUploadFileSize → {MaxUploadFileSize}", maxUploadFileSize);

			Logger.LogWarning("<<< Exiting GetAllowedFileExtensions()");
            return this.Ok(new { allowedExtensions, validationCode, maxUploadFileSize });
        }

        private static string GetFileIconUrl(string extension)
        {
			Logger.LogError(">>> Entering GetFileIconUrl(extension={Extension})", extension);
            if (!string.IsNullOrEmpty(extension) && File.Exists(HostingEnvironment.MapPath(IconController.IconURL("Ext" + extension, "32x32", "Standard"))))
            {
                return IconController.IconURL("Ext" + extension, "32x32", "Standard");
            }

			Logger.LogWarning("<<< Exiting GetFileIconUrl(extension={Extension})", extension);
            return IconController.IconURL("ExtFile", "32x32", "Standard");
        }

        private static string GetFolderIconUrl(int portalId, int folderMappingId)
        {
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folderMappingId);
            var svgPath = Constants.ModulePath + "images/icon-asset-manager-{0}-folder.svg";

            if (folderMapping is null)
            {
                return Globals.ApplicationPath + "/" + string.Format(svgPath, "standard");
            }

            var localSvg = string.Format(svgPath, folderMapping.FolderProviderType.Replace("FolderProvider", string.Empty).ToLowerInvariant());
            if (File.Exists(HostingEnvironment.MapPath($"~/{localSvg}")))
            {
                return Globals.ApplicationPath + "/" + localSvg;
            }

            if (File.Exists(HostingEnvironment.MapPath(folderMapping.ImageUrl)))
            {
                return VirtualPathUtility.ToAbsolute(folderMapping.ImageUrl);
            }

            return Globals.ApplicationPath + "/" + string.Format(svgPath, "standard");
        }

        private int FindGroupId(HttpRequestMessage request)
        {
			Logger.LogError(">>> Entering FindGroupId(request={Request})", request);
            var headers = request.Headers;
            int groupId;
            int.TryParse(headers.GetValues("GroupId").FirstOrDefault(), out groupId);
			Logger.LogWarning("<<< Exiting FindGroupId(request={Request})", request);
            return groupId;
        }

        private object GetItemViewModel(object item)
        {
			Logger.LogError(">>> Entering GetItemViewModel(item={Item})", item);
            var folder = item as IFolderInfo;
            if (folder != null)
            {
                return new
                {
                    isFolder = true,
                    itemId = folder.FolderID,
                    itemName = folder.FolderName,
                    iconUrl = GetFolderIconUrl(this.PortalSettings.PortalId, folder.FolderMappingID),
                    createdOn = folder.CreatedOnDate,
                    modifiedOn = folder.LastModifiedOnDate,
                    unlinkAllowedStatus = this.GetUnlinkAllowedStatus(folder),
                };
            }

            var thumbnailsManager = ThumbnailsManager.Instance;
            var file = item as IFileInfo;
			Logger.LogWarning("<<< Exiting GetItemViewModel(item={Item})", item);
            return new
            {
                isFolder = false,
                itemId = file.FileId,
                itemName = file.FileName,
                path = FileManager.Instance.GetUrl(file),
                iconUrl = GetFileIconUrl(file.Extension),
                thumbnailAvailable = thumbnailsManager.ThumbnailAvailable(file.FileName),
                thumbnailUrl = thumbnailsManager.ThumbnailUrl(this.ActiveModule.ModuleID, file.FileId, 110, 110),
                createdOn = file.CreatedOnDate,
                modifiedOn = file.LastModifiedOnDate,
                fileSize = file.Size,
            };
        }

        private string GetUnlinkAllowedStatus(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering GetUnlinkAllowedStatus(folder={Folder})", folder);
            if (this.AreMappedPathsSupported(folder.FolderMappingID) && folder.ParentID > 0 && FolderManager.Instance.GetFolder(folder.ParentID).FolderMappingID != folder.FolderMappingID)
            {
                return "onlyUnlink";
            }

            if (this.AreMappedPathsSupported(folder.FolderMappingID))
            {
                return "true";
            }

			Logger.LogWarning("<<< Exiting GetUnlinkAllowedStatus(folder={Folder})", folder);
            return "false";
        }

        private bool AreMappedPathsSupported(int folderMappingId)
        {
			Logger.LogError(">>> Entering AreMappedPathsSupported(folderMappingId=<redacted>)");
            if (this.mappedPathsSupported.ContainsKey(folderMappingId))
            {
                return (bool)this.mappedPathsSupported[folderMappingId];
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folderMappingId);
			Logger.LogError("AreMappedPathsSupported(folderMappingId=<redacted>): folderMapping → {FolderMapping}", folderMapping);
            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
			Logger.LogError("AreMappedPathsSupported(folderMappingId=<redacted>): folderProvider → {FolderProvider}", folderProvider);
            var result = folderProvider.SupportsMappedPaths;
			Logger.LogError("AreMappedPathsSupported(folderMappingId=<redacted>): result → {Result}", result);
            this.mappedPathsSupported[folderMappingId] = result;
			Logger.LogWarning("<<< Exiting AreMappedPathsSupported(folderMappingId=<redacted>)");
            return result;
        }

        private ModuleInstanceContext GetModuleContext()
        {
			Logger.LogError(">>> Entering GetModuleContext()");
            IModuleControl moduleControl = this.modulePipeline.CreateModuleControl(this.ActiveModule) as IModuleControl;
			Logger.LogError("GetModuleContext(): moduleControl → {ModuleControl}", moduleControl);
			Logger.LogWarning("<<< Exiting GetModuleContext()");
            return new ModuleInstanceContext(moduleControl);
        }
    }
}
