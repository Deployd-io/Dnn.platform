// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

using Microsoft.Extensions.DependencyInjection;

/// <summary>The default <see cref="IJournalController"/> implementation.</summary>
internal class JournalControllerImpl : IJournalController
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JournalControllerImpl));
    private const string AllowResizePhotosSetting = "Journal_AllowResizePhotos";
    private const string AllowPhotosSetting = "Journal_AllowPhotos";
    private const string EditorEnabledSetting = "Journal_EditorEnabled";

    private static readonly string[] InvalidSecuritySetsWithoutId = ["R", "U", "F", "P"];
    private static readonly char[] ValidSecurityDescriptors = ['E', 'C', 'R', 'U', 'F', 'P'];
    private readonly IJournalDataService dataService;
    private readonly IHostSettings hostSettings;
    private readonly IRoleController roleController;
    private readonly ISearchHelper searchHelper;
    private readonly DataProvider dataProvider;
    private readonly IFolderManager folderManager;
    private readonly IFileManager fileManager;
    private readonly IFileContentTypeManager fileContentTypeManager;
    private readonly PortalSecurity portalSecurity;

    /// <summary>Initializes a new instance of the <see cref="JournalControllerImpl"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.1.1. Please use overload with IJournalDataService. Scheduled removal in v12.0.0.")]
    public JournalControllerImpl()
        : this(null, null, null, null, null, null, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="JournalControllerImpl"/> class.</summary>
    /// <param name="journalDataService">The journal data service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="roleController">The role controller.</param>
    /// <param name="searchHelper">The search helpers.</param>
    /// <param name="dataProvider">The data provider.</param>
    /// <param name="folderManager">The folder manager.</param>
    /// <param name="fileManager">The file manager.</param>
    /// <param name="fileContentTypeManager">The file content type manager.</param>
    /// <param name="portalSecurity">The portal security instance.</param>
    public JournalControllerImpl(IJournalDataService journalDataService, IHostSettings hostSettings, IRoleController roleController, ISearchHelper searchHelper, DataProvider dataProvider, IFolderManager folderManager, IFileManager fileManager, IFileContentTypeManager fileContentTypeManager, PortalSecurity portalSecurity)
    {
		Logger.LogError(">>> Entering JournalControllerImpl(journalDataService={JournalDataService},hostSettings={HostSettings},roleController={RoleController},searchHelper={SearchHelper},dataProvider={DataProvider},folderManager={FolderManager},fileManager={FileManager},fileContentTypeManager={FileContentTypeManager},portalSecurity={PortalSecurity})", journalDataService, hostSettings, roleController, searchHelper, dataProvider, folderManager, fileManager, fileContentTypeManager, portalSecurity);
		Logger.LogDebug("JournalControllerImpl(journalDataService={JournalDataService},hostSettings={HostSettings},roleController={RoleController},searchHelper={SearchHelper},dataProvider={DataProvider},folderManager={FolderManager},fileManager={FileManager},fileContentTypeManager={FileContentTypeManager},portalSecurity={PortalSecurity}): hostSettings → {HostSettings1}", journalDataService, hostSettings, roleController, searchHelper, dataProvider, folderManager, fileManager, fileContentTypeManager, portalSecurity, hostSettings);
		Logger.LogDebug("JournalControllerImpl(journalDataService={JournalDataService},hostSettings={HostSettings},roleController={RoleController},searchHelper={SearchHelper},dataProvider={DataProvider},folderManager={FolderManager},fileManager={FileManager},fileContentTypeManager={FileContentTypeManager},portalSecurity={PortalSecurity}): roleController → {RoleController1}", journalDataService, hostSettings, roleController, searchHelper, dataProvider, folderManager, fileManager, fileContentTypeManager, portalSecurity, roleController);
		Logger.LogDebug("JournalControllerImpl(journalDataService={JournalDataService},hostSettings={HostSettings},roleController={RoleController},searchHelper={SearchHelper},dataProvider={DataProvider},folderManager={FolderManager},fileManager={FileManager},fileContentTypeManager={FileContentTypeManager},portalSecurity={PortalSecurity}): searchHelper → {SearchHelper1}", journalDataService, hostSettings, roleController, searchHelper, dataProvider, folderManager, fileManager, fileContentTypeManager, portalSecurity, searchHelper);
		Logger.LogDebug("JournalControllerImpl(journalDataService={JournalDataService},hostSettings={HostSettings},roleController={RoleController},searchHelper={SearchHelper},dataProvider={DataProvider},folderManager={FolderManager},fileManager={FileManager},fileContentTypeManager={FileContentTypeManager},portalSecurity={PortalSecurity}): dataProvider → {DataProvider1}", journalDataService, hostSettings, roleController, searchHelper, dataProvider, folderManager, fileManager, fileContentTypeManager, portalSecurity, dataProvider);
		Logger.LogDebug("JournalControllerImpl(journalDataService={JournalDataService},hostSettings={HostSettings},roleController={RoleController},searchHelper={SearchHelper},dataProvider={DataProvider},folderManager={FolderManager},fileManager={FileManager},fileContentTypeManager={FileContentTypeManager},portalSecurity={PortalSecurity}): folderManager → {FolderManager1}", journalDataService, hostSettings, roleController, searchHelper, dataProvider, folderManager, fileManager, fileContentTypeManager, portalSecurity, folderManager);
		Logger.LogDebug("JournalControllerImpl(journalDataService={JournalDataService},hostSettings={HostSettings},roleController={RoleController},searchHelper={SearchHelper},dataProvider={DataProvider},folderManager={FolderManager},fileManager={FileManager},fileContentTypeManager={FileContentTypeManager},portalSecurity={PortalSecurity}): fileManager → {FileManager1}", journalDataService, hostSettings, roleController, searchHelper, dataProvider, folderManager, fileManager, fileContentTypeManager, portalSecurity, fileManager);
		Logger.LogDebug("JournalControllerImpl(journalDataService={JournalDataService},hostSettings={HostSettings},roleController={RoleController},searchHelper={SearchHelper},dataProvider={DataProvider},folderManager={FolderManager},fileManager={FileManager},fileContentTypeManager={FileContentTypeManager},portalSecurity={PortalSecurity}): fileContentTypeManager → {FileContentTypeManager1}", journalDataService, hostSettings, roleController, searchHelper, dataProvider, folderManager, fileManager, fileContentTypeManager, portalSecurity, fileContentTypeManager);
		Logger.LogDebug("JournalControllerImpl(journalDataService={JournalDataService},hostSettings={HostSettings},roleController={RoleController},searchHelper={SearchHelper},dataProvider={DataProvider},folderManager={FolderManager},fileManager={FileManager},fileContentTypeManager={FileContentTypeManager},portalSecurity={PortalSecurity}): portalSecurity → {PortalSecurity1}", journalDataService, hostSettings, roleController, searchHelper, dataProvider, folderManager, fileManager, fileContentTypeManager, portalSecurity, portalSecurity);
        this.dataService = journalDataService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJournalDataService>();
		Logger.LogWarning("<<< Exiting JournalControllerImpl(journalDataService={JournalDataService},hostSettings={HostSettings},roleController={RoleController},searchHelper={SearchHelper},dataProvider={DataProvider},folderManager={FolderManager},fileManager={FileManager},fileContentTypeManager={FileContentTypeManager},portalSecurity={PortalSecurity})", journalDataService, hostSettings, roleController, searchHelper, dataProvider, folderManager, fileManager, fileContentTypeManager, portalSecurity);
        this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        this.roleController = roleController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>();
        this.searchHelper = searchHelper ?? Globals.GetCurrentServiceProvider().GetRequiredService<ISearchHelper>();
        this.dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
        this.folderManager = folderManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFolderManager>();
        this.fileManager = fileManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileManager>();
        this.fileContentTypeManager = fileContentTypeManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileContentTypeManager>();
        this.portalSecurity = portalSecurity ?? Globals.GetCurrentServiceProvider().GetRequiredService<PortalSecurity>();
    }

    /// <summary>Save the journal object into database.</summary>
    /// <param name="journalItem">Journal object.</param>
    /// <param name="tabId">The ID of the tab of the journal item context.</param>
    /// <param name="moduleId">The ID of the module of the journal item context.</param>
    public void SaveJournalItem(JournalItem journalItem, int tabId, int moduleId)
    {
		Logger.LogError(">>> Entering SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId})", journalItem, tabId, moduleId);
        if (journalItem.UserId < 1)
        {
			Logger.LogError("Throwing ArgumentException from SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId})", journalItem, tabId, moduleId);
            throw new ArgumentException("journalItem.UserId must be for a real user");
        }

        UserInfo currentUser = UserController.GetUserById(journalItem.PortalId, journalItem.UserId);
		Logger.LogError("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): currentUser → {CurrentUser}", journalItem, tabId, moduleId, currentUser);
        if (currentUser == null)
        {
			Logger.LogError("Throwing UserDoesNotExistException from SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId})", journalItem, tabId, moduleId);
            throw new UserDoesNotExistException("Unable to locate the current user");
        }

        string xml = null;
		Logger.LogError("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): xml → {Xml}", journalItem, tabId, moduleId, xml);
        if (!string.IsNullOrEmpty(journalItem.Title))
        {
#pragma warning disable CS0618 // Type or member is obsolete
            journalItem.Title = this.portalSecurity.InputFilter(journalItem.Title, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        if (!string.IsNullOrEmpty(journalItem.Summary))
        {
#pragma warning disable CS0618 // Type or member is obsolete
            journalItem.Summary = this.portalSecurity.InputFilter(journalItem.Summary, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        if (!string.IsNullOrEmpty(journalItem.Body))
        {
#pragma warning disable CS0618 // Type or member is obsolete
            journalItem.Body = this.portalSecurity.InputFilter(journalItem.Body, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        if (!string.IsNullOrEmpty(journalItem.Body))
        {
            var xDoc = new XmlDocument { XmlResolver = null };
            var itemsNode = xDoc.CreateElement("items");
			Logger.LogError("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): itemsNode → {ItemsNode}", journalItem, tabId, moduleId, itemsNode);
            var itemNode = xDoc.CreateElement("item");
			Logger.LogError("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): itemNode → {ItemNode}", journalItem, tabId, moduleId, itemNode);
            itemNode.AppendChild(CreateElement(xDoc, "id", "-1"));
            itemNode.AppendChild(CreateCDataElement(xDoc, "body", journalItem.Body));
            itemsNode.AppendChild(itemNode);
            xDoc.AppendChild(itemsNode);
            var xDec = xDoc.CreateXmlDeclaration("1.0", null, null);
			Logger.LogError("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): xDec → {XDec}", journalItem, tabId, moduleId, xDec);
			Logger.LogDebug("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): xDec.Standalone → {Arg3}", journalItem, tabId, moduleId, xDec.Standalone);
            xDec.Encoding = "UTF-16";
			Logger.LogDebug("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): xDec.Encoding → {Arg3}", journalItem, tabId, moduleId, xDec.Encoding);
            xDec.Standalone = "yes";
            var root = xDoc.DocumentElement;
            xDoc.InsertBefore(xDec, root);
            journalItem.JournalXML = xDoc;
			Logger.LogDebug("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): journalItem.JournalXML → {Arg3}", journalItem, tabId, moduleId, journalItem.JournalXML);
            xml = journalItem.JournalXML.OuterXml;
        }

        if (journalItem.ItemData != null)
        {
            if (!string.IsNullOrEmpty(journalItem.ItemData.Title))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                journalItem.ItemData.Title = this.portalSecurity.InputFilter(journalItem.ItemData.Title, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.Description))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                journalItem.ItemData.Description = this.portalSecurity.InputFilter(journalItem.ItemData.Description, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.Url))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                journalItem.ItemData.Url = this.portalSecurity.InputFilter(journalItem.ItemData.Url, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.ImageUrl))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                journalItem.ItemData.ImageUrl = this.portalSecurity.InputFilter(journalItem.ItemData.ImageUrl, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        var journalData = journalItem.ItemData.ToJson();
		Logger.LogError("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): journalData → {JournalData}", journalItem, tabId, moduleId, journalData);
        if (journalData == "null")
        {
            journalData = null;
        }

        this.PrepareSecuritySet(journalItem);

        journalItem.JournalId = this.dataService.Journal_Save(
            journalItem.PortalId,
            journalItem.UserId,
            journalItem.ProfileId,
            journalItem.SocialGroupId,
            journalItem.JournalId,
            journalItem.JournalTypeId,
            journalItem.Title,
            journalItem.Summary,
            journalItem.Body,
            journalData,
            xml,
            journalItem.ObjectKey,
            journalItem.AccessKey,
            journalItem.SecuritySet,
            journalItem.CommentsDisabled,
            journalItem.CommentsHidden);

        var updatedJournalItem = this.GetJournalItem(journalItem.PortalId, journalItem.UserId, journalItem.JournalId);
		Logger.LogError("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): updatedJournalItem → {UpdatedJournalItem}", journalItem, tabId, moduleId, updatedJournalItem);
		Logger.LogDebug("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): journalItem.DateUpdated → {Arg3}", journalItem, tabId, moduleId, journalItem.DateUpdated);
        journalItem.DateCreated = updatedJournalItem.DateCreated;
		Logger.LogDebug("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): journalItem.DateCreated → {Arg3}", journalItem, tabId, moduleId, journalItem.DateCreated);
        journalItem.DateUpdated = updatedJournalItem.DateUpdated;
        var cnt = new Content();
		Logger.LogError("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): cnt → {Cnt}", journalItem, tabId, moduleId, cnt);

        if (journalItem.ContentItemId > 0)
        {
            cnt.UpdateContentItem(journalItem, tabId, moduleId);
            this.dataService.Journal_UpdateContentItemId(journalItem.JournalId, journalItem.ContentItemId);
        }
        else
        {
            var ci = cnt.CreateContentItem(journalItem, tabId, moduleId);
			Logger.LogError("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): ci → {Ci}", journalItem, tabId, moduleId, ci);
            this.dataService.Journal_UpdateContentItemId(journalItem.JournalId, ci.ContentItemId);
            journalItem.ContentItemId = ci.ContentItemId;
			Logger.LogDebug("SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): journalItem.ContentItemId → {Arg3}", journalItem, tabId, moduleId, journalItem.ContentItemId);
        }

        if (journalItem.SocialGroupId > 0)
        {
            try
            {
                this.UpdateGroupStats(journalItem.PortalId, journalItem.SocialGroupId);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): {Arg3}", journalItem, tabId, moduleId, exc.Message);
                Exceptions.Exceptions.LogException(exc);
            }
        }
		Logger.LogWarning("<<< Exiting SaveJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId})", journalItem, tabId, moduleId);
    }

    /// <summary>Update the journal info in database.</summary>
    /// <param name="journalItem">Journal object.</param>
    /// <param name="tabId">The ID of the tab of the journal item context.</param>
    /// <param name="moduleId">The ID of the module of the journal item context.</param>
    public void UpdateJournalItem(JournalItem journalItem, int tabId, int moduleId)
    {
		Logger.LogError(">>> Entering UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId})", journalItem, tabId, moduleId);
        if (journalItem.UserId < 1)
        {
			Logger.LogError("Throwing ArgumentException from UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId})", journalItem, tabId, moduleId);
            throw new ArgumentException("journalItem.UserId must be for a real user");
        }

        UserInfo currentUser = UserController.GetUserById(journalItem.PortalId, journalItem.UserId);
		Logger.LogError("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): currentUser → {CurrentUser}", journalItem, tabId, moduleId, currentUser);
        if (currentUser == null)
        {
			Logger.LogError("Throwing UserDoesNotExistException from UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId})", journalItem, tabId, moduleId);
            throw new UserDoesNotExistException("Unable to locate the current user");
        }

        string xml = null;
		Logger.LogError("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): xml → {Xml}", journalItem, tabId, moduleId, xml);
        if (!string.IsNullOrEmpty(journalItem.Title))
        {
#pragma warning disable CS0618 // Type or member is obsolete
            journalItem.Title = this.portalSecurity.InputFilter(journalItem.Title, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        if (!string.IsNullOrEmpty(journalItem.Summary))
        {
#pragma warning disable CS0618 // Type or member is obsolete
            journalItem.Summary = this.portalSecurity.InputFilter(journalItem.Summary, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        if (!string.IsNullOrEmpty(journalItem.Body))
        {
#pragma warning disable CS0618 // Type or member is obsolete
            journalItem.Body = this.portalSecurity.InputFilter(journalItem.Body, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        if (!string.IsNullOrEmpty(journalItem.Body))
        {
            var xDoc = new XmlDocument { XmlResolver = null };
            var itemsNode = xDoc.CreateElement("items");
			Logger.LogError("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): itemsNode → {ItemsNode}", journalItem, tabId, moduleId, itemsNode);
            var itemNode = xDoc.CreateElement("item");
			Logger.LogError("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): itemNode → {ItemNode}", journalItem, tabId, moduleId, itemNode);
            itemNode.AppendChild(CreateElement(xDoc, "id", "-1"));
            itemNode.AppendChild(CreateCDataElement(xDoc, "body", journalItem.Body));
            itemsNode.AppendChild(itemNode);
            xDoc.AppendChild(itemsNode);
            var xDec = xDoc.CreateXmlDeclaration("1.0", null, null);
			Logger.LogError("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): xDec → {XDec}", journalItem, tabId, moduleId, xDec);
			Logger.LogDebug("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): xDec.Standalone → {Arg3}", journalItem, tabId, moduleId, xDec.Standalone);
            xDec.Encoding = "UTF-16";
			Logger.LogDebug("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): xDec.Encoding → {Arg3}", journalItem, tabId, moduleId, xDec.Encoding);
            xDec.Standalone = "yes";
            var root = xDoc.DocumentElement;
            xDoc.InsertBefore(xDec, root);
            journalItem.JournalXML = xDoc;
			Logger.LogDebug("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): journalItem.JournalXML → {Arg3}", journalItem, tabId, moduleId, journalItem.JournalXML);
            xml = journalItem.JournalXML.OuterXml;
        }

        if (journalItem.ItemData != null)
        {
            if (!string.IsNullOrEmpty(journalItem.ItemData.Title))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                journalItem.ItemData.Title = this.portalSecurity.InputFilter(journalItem.ItemData.Title, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.Description))
            {
                journalItem.ItemData.Description =
#pragma warning disable CS0618 // Type or member is obsolete
                    this.portalSecurity.InputFilter(journalItem.ItemData.Description, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.Url))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                journalItem.ItemData.Url = this.portalSecurity.InputFilter(journalItem.ItemData.Url, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.ImageUrl))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                journalItem.ItemData.ImageUrl = this.portalSecurity.InputFilter(journalItem.ItemData.ImageUrl, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        var journalData = journalItem.ItemData.ToJson();
		Logger.LogError("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): journalData → {JournalData}", journalItem, tabId, moduleId, journalData);
        if (journalData == "null")
        {
            journalData = null;
        }

        this.PrepareSecuritySet(journalItem);

        journalItem.JournalId = this.dataService.Journal_Update(
            journalItem.PortalId,
            journalItem.UserId,
            journalItem.ProfileId,
            journalItem.SocialGroupId,
            journalItem.JournalId,
            journalItem.JournalTypeId,
            journalItem.Title,
            journalItem.Summary,
            journalItem.Body,
            journalData,
            xml,
            journalItem.ObjectKey,
            journalItem.AccessKey,
            journalItem.SecuritySet,
            journalItem.CommentsDisabled,
            journalItem.CommentsHidden);

        var updatedJournalItem = this.GetJournalItem(journalItem.PortalId, journalItem.UserId, journalItem.JournalId);
		Logger.LogError("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): updatedJournalItem → {UpdatedJournalItem}", journalItem, tabId, moduleId, updatedJournalItem);
		Logger.LogDebug("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): journalItem.DateUpdated → {Arg3}", journalItem, tabId, moduleId, journalItem.DateUpdated);
        journalItem.DateCreated = updatedJournalItem.DateCreated;
		Logger.LogDebug("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): journalItem.DateCreated → {Arg3}", journalItem, tabId, moduleId, journalItem.DateCreated);
        journalItem.DateUpdated = updatedJournalItem.DateUpdated;

        var cnt = new Content();
		Logger.LogError("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): cnt → {Cnt}", journalItem, tabId, moduleId, cnt);
        if (journalItem.ContentItemId > 0)
        {
            cnt.UpdateContentItem(journalItem, tabId, moduleId);
            this.dataService.Journal_UpdateContentItemId(journalItem.JournalId, journalItem.ContentItemId);
        }
        else
        {
            var ci = cnt.CreateContentItem(journalItem, tabId, moduleId);
			Logger.LogError("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): ci → {Ci}", journalItem, tabId, moduleId, ci);
            this.dataService.Journal_UpdateContentItemId(journalItem.JournalId, ci.ContentItemId);
            journalItem.ContentItemId = ci.ContentItemId;
			Logger.LogDebug("UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): journalItem.ContentItemId → {Arg3}", journalItem, tabId, moduleId, journalItem.ContentItemId);
        }

        if (journalItem.SocialGroupId > 0)
        {
            try
            {
                this.UpdateGroupStats(journalItem.PortalId, journalItem.SocialGroupId);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId}): {Arg3}", journalItem, tabId, moduleId, exc.Message);
                Exceptions.Exceptions.LogException(exc);
            }
        }
		Logger.LogWarning("<<< Exiting UpdateJournalItem(journalItem={JournalItem},tabId={TabId},moduleId={ModuleId})", journalItem, tabId, moduleId);
    }

    /// <inheritdoc />
    public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId)
    {
		Logger.LogError(">>> Entering GetJournalItem(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId})", portalId, currentUserId, journalId);
        Logger.LogWarning("<<< Exiting GetJournalItem(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId})", portalId, currentUserId, journalId);
        return this.GetJournalItem(portalId, currentUserId, journalId, false, false);
    }

    /// <inheritdoc />
    public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId, bool includeAllItems)
    {
        return this.GetJournalItem(portalId, currentUserId, journalId, includeAllItems, false);
    }

    /// <inheritdoc />
    public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId, bool includeAllItems, bool isDeleted)
    {
        return this.GetJournalItem(portalId, currentUserId, journalId, includeAllItems, isDeleted, false);
    }

    /// <inheritdoc />
    public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId, bool includeAllItems, bool isDeleted, bool securityCheck)
    {
        return CBO.FillObject<JournalItem>(this.dataService.Journal_Get(portalId, currentUserId, journalId, includeAllItems, isDeleted, securityCheck));
    }

    /// <inheritdoc />
    public JournalItem GetJournalItemByKey(int portalId, string objectKey)
    {
		Logger.LogError(">>> Entering GetJournalItemByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
        Logger.LogWarning("<<< Exiting GetJournalItemByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
		Logger.LogWarning("<<< Exiting GetJournalItemByKey(portalId={PortalId},objectKey={ObjectKey},includeAllItems={IncludeAllItems},isDeleted={IsDeleted})", portalId, objectKey, includeAllItems, isDeleted);
        return this.GetJournalItemByKey(portalId, objectKey, false, false);
    }

    /// <inheritdoc />
    public JournalItem GetJournalItemByKey(int portalId, string objectKey, bool includeAllItems)
    {
        return this.GetJournalItemByKey(portalId, objectKey, includeAllItems, false);
    }

    /// <inheritdoc />
    public JournalItem GetJournalItemByKey(int portalId, string objectKey, bool includeAllItems, bool isDeleted)
    {
        if (string.IsNullOrEmpty(objectKey))
        {
            return null;
        }

        return CBO.FillObject<JournalItem>(this.dataService.Journal_GetByKey(portalId, objectKey, includeAllItems, isDeleted));
    }

    /// <inheritdoc />
    public IFileInfo SaveJourmalFile(ModuleInfo module, UserInfo userInfo, string fileName, Stream fileContent)
    {
		Logger.LogError(">>> Entering SaveJourmalFile(module={Module},userInfo={UserInfo},fileName={FileName},fileContent={FileContent})", module, userInfo, fileName, fileContent);
        var userFolder = this.folderManager.GetUserFolder(userInfo);

		Logger.LogError("SaveJourmalFile(module={Module},userInfo={UserInfo},fileName={FileName},fileContent={FileContent}): userFolder → {UserFolder}", module, userInfo, fileName, fileContent, userFolder);
        Stream fileContentStream = null;
		Logger.LogError("SaveJourmalFile(module={Module},userInfo={UserInfo},fileName={FileName},fileContent={FileContent}): fileContentStream → {FileContentStream}", module, userInfo, fileName, fileContent, fileContentStream);
        try
        {
            if (IsImageFile(fileName) && IsResizePhotosEnabled(module))
            {
                fileContentStream = GetJournalImageContent(fileContent);
				Logger.LogError(">>> Entering IsResizePhotosEnabled(module={Module})", module);
        Logger.LogWarning("<<< Exiting IsResizePhotosEnabled(module={Module})", module);
            }

			Logger.LogWarning("<<< Exiting SaveJourmalFile(module={Module},userInfo={UserInfo},fileName={FileName},fileContent={FileContent})", module, userInfo, fileName, fileContent);
            return this.fileManager.AddFile(userFolder, fileName, fileContentStream ?? fileContent, true, true, this.fileContentTypeManager.GetContentType(Path.GetExtension(fileName)));
        }
        finally
        {
            fileContentStream?.Dispose();
        }
    }

    /// <inheritdoc />
    public void SaveJournalItem(JournalItem journalItem, ModuleInfo module)
    {
		Logger.LogError(">>> Entering SaveJournalItem(journalItem={JournalItem},module={Module})", journalItem, module);
        var tabId = module?.TabID ?? Null.NullInteger;
        var tabModuleId = module?.TabModuleID ?? Null.NullInteger;

        this.SaveJournalItem(journalItem, tabId, tabModuleId);
		Logger.LogWarning("<<< Exiting SaveJournalItem(journalItem={JournalItem},module={Module})", journalItem, module);
    }

    /// <inheritdoc />
    public void UpdateJournalItem(JournalItem journalItem, ModuleInfo module)
    {
		Logger.LogError(">>> Entering UpdateJournalItem(journalItem={JournalItem},module={Module})", journalItem, module);
        var tabId = module?.TabID ?? Null.NullInteger;
        var tabModuleId = module?.TabModuleID ?? Null.NullInteger;

        this.UpdateJournalItem(journalItem, tabId, tabModuleId);
		Logger.LogWarning("<<< Exiting UpdateJournalItem(journalItem={JournalItem},module={Module})", journalItem, module);
    }

    /// <inheritdoc />
    public void DisableComments(int portalId, int journalId)
    {
		Logger.LogError(">>> Entering DisableComments(portalId={PortalId},journalId={JournalId})", portalId, journalId);
        this.dataService.Journal_Comments_ToggleDisable(portalId, journalId, true);
		Logger.LogWarning("<<< Exiting DisableComments(portalId={PortalId},journalId={JournalId})", portalId, journalId);
    }

    /// <inheritdoc />
    public void EnableComments(int portalId, int journalId)
    {
		Logger.LogError(">>> Entering EnableComments(portalId={PortalId},journalId={JournalId})", portalId, journalId);
        this.dataService.Journal_Comments_ToggleDisable(portalId, journalId, false);
		Logger.LogWarning("<<< Exiting EnableComments(portalId={PortalId},journalId={JournalId})", portalId, journalId);
    }

    /// <inheritdoc />
    public void HideComments(int portalId, int journalId)
    {
		Logger.LogError(">>> Entering HideComments(portalId={PortalId},journalId={JournalId})", portalId, journalId);
        this.dataService.Journal_Comments_ToggleHidden(portalId, journalId, true);
		Logger.LogWarning("<<< Exiting HideComments(portalId={PortalId},journalId={JournalId})", portalId, journalId);
    }

    /// <inheritdoc />
    public void ShowComments(int portalId, int journalId)
    {
		Logger.LogError(">>> Entering ShowComments(portalId={PortalId},journalId={JournalId})", portalId, journalId);
        this.dataService.Journal_Comments_ToggleHidden(portalId, journalId, false);
		Logger.LogWarning("<<< Exiting ShowComments(portalId={PortalId},journalId={JournalId})", portalId, journalId);
    }

    // Delete Journal Items

    /// <summary>HARD deletes journal items.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="currentUserId">The user ID.</param>
    /// <param name="journalId">The journal ID.</param>
    public void DeleteJournalItem(int portalId, int currentUserId, int journalId)
    {
		Logger.LogError(">>> Entering DeleteJournalItem(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId})", portalId, currentUserId, journalId);
        this.DeleteJournalItem(portalId, currentUserId, journalId, false);
		Logger.LogWarning("<<< Exiting DeleteJournalItem(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId})", portalId, currentUserId, journalId);
    }

    /// <summary>HARD deletes journal items based on item key.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="objectKey">The key.</param>
    public void DeleteJournalItemByKey(int portalId, string objectKey)
    {
		Logger.LogError(">>> Entering DeleteJournalItemByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
        this.dataService.Journal_DeleteByKey(portalId, objectKey);
		Logger.LogWarning("<<< Exiting DeleteJournalItemByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
    }

    /// <summary>HARD deletes journal items based on group ID.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="groupId">The group ID.</param>
    public void DeleteJournalItemByGroupId(int portalId, int groupId)
    {
		Logger.LogError(">>> Entering DeleteJournalItemByGroupId(portalId={PortalId},groupId={GroupId})", portalId, groupId);
        this.dataService.Journal_DeleteByGroupId(portalId, groupId);
		Logger.LogWarning("<<< Exiting DeleteJournalItemByGroupId(portalId={PortalId},groupId={GroupId})", portalId, groupId);
    }

    /// <summary>SOFT deletes journal items.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="currentUserId">The user ID.</param>
    /// <param name="journalId">The journal ID.</param>
    public void SoftDeleteJournalItem(int portalId, int currentUserId, int journalId)
    {
		Logger.LogError(">>> Entering SoftDeleteJournalItem(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId})", portalId, currentUserId, journalId);
        this.DeleteJournalItem(portalId, currentUserId, journalId, true);
		Logger.LogWarning("<<< Exiting SoftDeleteJournalItem(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId})", portalId, currentUserId, journalId);
    }

    /// <summary>SOFT deletes journal items based on item key.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="objectKey">The key.</param>
    public void SoftDeleteJournalItemByKey(int portalId, string objectKey)
    {
		Logger.LogError(">>> Entering SoftDeleteJournalItemByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
        this.dataService.Journal_SoftDeleteByKey(portalId, objectKey);
		Logger.LogWarning("<<< Exiting SoftDeleteJournalItemByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
    }

    /// <summary>SOFT deletes journal items based on group ID.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="groupId">The group ID.</param>
    public void SoftDeleteJournalItemByGroupId(int portalId, int groupId)
    {
		Logger.LogError(">>> Entering SoftDeleteJournalItemByGroupId(portalId={PortalId},groupId={GroupId})", portalId, groupId);
        this.dataService.Journal_SoftDeleteByGroupId(portalId, groupId);
		Logger.LogWarning("<<< Exiting SoftDeleteJournalItemByGroupId(portalId={PortalId},groupId={GroupId})", portalId, groupId);
    }

    /// <inheritdoc />
    public IList<CommentInfo> GetCommentsByJournalIds(List<int> journalIdList)
    {
		Logger.LogError(">>> Entering GetCommentsByJournalIds(journalIdList={JournalIdList})", journalIdList);
        var journalIds = journalIdList.Aggregate(string.Empty, (current, journalId) => current + journalId + ";");
		Logger.LogError("GetCommentsByJournalIds(journalIdList={JournalIdList}): journalIds → {JournalIds}", journalIdList, journalIds);
		Logger.LogWarning("<<< Exiting GetCommentsByJournalIds(journalIdList={JournalIdList})", journalIdList);
        return CBO.FillCollection<CommentInfo>(this.dataService.Journal_Comment_ListByJournalIds(journalIds));
    }

    /// <inheritdoc />
    public void LikeJournalItem(int journalId, int userId, string displayName)
    {
		Logger.LogError(">>> Entering LikeJournalItem(journalId={JournalId},userId={UserId},displayName={DisplayName})", journalId, userId, displayName);
        this.dataService.Journal_Like(journalId, userId, displayName);
		Logger.LogWarning("<<< Exiting LikeJournalItem(journalId={JournalId},userId={UserId},displayName={DisplayName})", journalId, userId, displayName);
    }

    /// <inheritdoc />
    public void SaveComment(CommentInfo comment)
    {
		Logger.LogError(">>> Entering SaveComment(comment={Comment})", comment);
        if (!string.IsNullOrEmpty(comment.Comment))
        {
		Logger.LogWarning("<<< Exiting SaveComment(comment={Comment})", comment);
            comment.Comment =
#pragma warning disable CS0618 // Type or member is obsolete
                this.portalSecurity.InputFilter(comment.Comment, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        // TODO: enable once the profanity filter is working properly.
        ////objCommentInfo.Comment = portalSecurity.Remove(objCommentInfo.Comment, DotNetNuke.Security.PortalSecurity.ConfigType.ListController, "ProfanityFilter", DotNetNuke.Security.PortalSecurity.FilterScope.PortalList);
        string xml = null;
		Logger.LogError("SaveComment(comment={Comment}): xml → {Xml}", comment, xml);
        if (comment.CommentXML != null)
        {
            xml = comment.CommentXML.OuterXml;
        }

        comment.CommentId = this.dataService.Journal_Comment_Save(comment.JournalId, comment.CommentId, comment.UserId, comment.Comment, xml, Null.NullDate);

		Logger.LogDebug("SaveComment(comment={Comment}): comment.DateCreated → {Arg1}", comment, comment.DateCreated);
		Logger.LogDebug("SaveComment(comment={Comment}): comment.DateUpdated → {Arg1}", comment, comment.DateUpdated);
        var newComment = this.GetComment(comment.CommentId);
		Logger.LogError("SaveComment(comment={Comment}): newComment → {NewComment}", comment, newComment);
        comment.DateCreated = newComment.DateCreated;
        comment.DateUpdated = newComment.DateUpdated;
    }

    /// <inheritdoc />
    public CommentInfo GetComment(int commentId)
    {
		Logger.LogError(">>> Entering GetComment(commentId={CommentId})", commentId);
        Logger.LogWarning("<<< Exiting GetComment(commentId={CommentId})", commentId);
        return CBO.FillObject<CommentInfo>(this.dataService.Journal_Comment_Get(commentId));
    }

    /// <inheritdoc />
    public void DeleteComment(int journalId, int commentId)
    {
		Logger.LogError(">>> Entering DeleteComment(journalId={JournalId},commentId={CommentId})", journalId, commentId);
        this.dataService.Journal_Comment_Delete(journalId, commentId);

        // UNDONE: update the parent journal item and content item so this comment gets removed from search index
		Logger.LogWarning("<<< Exiting DeleteComment(journalId={JournalId},commentId={CommentId})", journalId, commentId);
    }

    /// <inheritdoc />
    public void LikeComment(int journalId, int commentId, int userId, string displayName)
    {
		Logger.LogError(">>> Entering LikeComment(journalId={JournalId},commentId={CommentId},userId={UserId},displayName={DisplayName})", journalId, commentId, userId, displayName);
        this.dataService.Journal_Comment_Like(journalId, commentId, userId, displayName);
		Logger.LogWarning("<<< Exiting LikeComment(journalId={JournalId},commentId={CommentId},userId={UserId},displayName={DisplayName})", journalId, commentId, userId, displayName);
    }

    /// <inheritdoc />
    public JournalTypeInfo GetJournalType(string journalType)
    {
		Logger.LogError(">>> Entering GetJournalType(journalType={JournalType})", journalType);
        Logger.LogWarning("<<< Exiting GetJournalType(journalType={JournalType})", journalType);
        return CBO.FillObject<JournalTypeInfo>(this.dataService.Journal_Types_Get(journalType));
    }

    /// <inheritdoc />
    public JournalTypeInfo GetJournalTypeById(int journalTypeId)
    {
		Logger.LogError(">>> Entering GetJournalTypeById(journalTypeId={JournalTypeId})", journalTypeId);
        Logger.LogWarning("<<< Exiting GetJournalTypeById(journalTypeId={JournalTypeId})", journalTypeId);
        return CBO.FillObject<JournalTypeInfo>(this.dataService.Journal_Types_GetById(journalTypeId));
    }

    /// <inheritdoc />
    public IEnumerable<JournalTypeInfo> GetJournalTypes(int portalId)
    {
		Logger.LogError(">>> Entering GetJournalTypes(portalId={PortalId})", portalId);
		Logger.LogWarning("<<< Exiting GetJournalTypes(portalId={PortalId})", portalId);
        return CBO.GetCachedObject<IEnumerable<JournalTypeInfo>>(
            this.hostSettings,
            new CacheItemArgs(
                string.Format(CultureInfo.InvariantCulture, DataCache.JournalTypesCacheKey, portalId),
                DataCache.JournalTypesTimeOut,
                DataCache.JournalTypesCachePriority,
                portalId),
            _ => CBO.FillCollection<JournalTypeInfo>(this.dataService.Journal_Types_List(portalId)));
    }

    private static XmlElement CreateElement(XmlDocument xDoc, string name, string value)
    {
		Logger.LogError(">>> Entering CreateElement(xDoc={XDoc},name={Name},value={Value})", xDoc, name, value);
        var element = xDoc.CreateElement(name);
        var xtext = xDoc.CreateTextNode(value);
		Logger.LogError("CreateElement(xDoc={XDoc},name={Name},value={Value}): xtext → {Xtext}", xDoc, name, value, xtext);
		Logger.LogError("CreateElement(xDoc={XDoc},name={Name},value={Value}): element → {Element}", xDoc, name, value, element);
        element.AppendChild(xtext);
		Logger.LogWarning("<<< Exiting CreateElement(xDoc={XDoc},name={Name},value={Value})", xDoc, name, value);
        return element;
    }

    private static XmlElement CreateCDataElement(XmlDocument xDoc, string name, string value)
    {
		Logger.LogError(">>> Entering CreateCDataElement(xDoc={XDoc},name={Name},value={Value})", xDoc, name, value);
        var element = xDoc.CreateElement(name);
        var xdata = xDoc.CreateCDataSection(value);
		Logger.LogError("CreateCDataElement(xDoc={XDoc},name={Name},value={Value}): xdata → {Xdata}", xDoc, name, value, xdata);
		Logger.LogError("CreateCDataElement(xDoc={XDoc},name={Name},value={Value}): element → {Element}", xDoc, name, value, element);
        element.AppendChild(xdata);
		Logger.LogWarning("<<< Exiting CreateCDataElement(xDoc={XDoc},name={Name},value={Value})", xDoc, name, value);
        return element;
    }

    private static MemoryStream GetJournalImageContent(Stream fileContent)
    {
		Logger.LogError(">>> Entering GetJournalImageContent(fileContent={FileContent})", fileContent);
        Image image = new Bitmap(fileContent);
        var thumbnailWidth = 400;
		Logger.LogError("GetJournalImageContent(fileContent={FileContent}): image → {Image}", fileContent, image);
        var thumbnailHeight = 400;
        GetThumbnailSize(image.Width, image.Height, ref thumbnailWidth, ref thumbnailHeight);
        var thumbnail = image.GetThumbnailImage(thumbnailWidth, thumbnailHeight, () => true, IntPtr.Zero);
		Logger.LogError("GetJournalImageContent(fileContent={FileContent}): thumbnail → {Thumbnail}", fileContent, thumbnail);
        var result = new MemoryStream();
		Logger.LogError("GetJournalImageContent(fileContent={FileContent}): result → {Result}", fileContent, result);
        thumbnail.Save(result, image.RawFormat);
		Logger.LogWarning("<<< Exiting GetJournalImageContent(fileContent={FileContent})", fileContent);
        return result;
    }

    private static void GetThumbnailSize(int imageWidth, int imageHeight, ref int thumbnailWidth, ref int thumbnailHeight)
    {
		Logger.LogError(">>> Entering GetThumbnailSize(imageWidth={ImageWidth},imageHeight={ImageHeight},thumbnailWidth={ThumbnailWidth},thumbnailHeight={ThumbnailHeight})", imageWidth, imageHeight, thumbnailWidth, thumbnailHeight);
        if (imageWidth >= imageHeight)
        {
            thumbnailWidth = Math.Min(imageWidth, thumbnailWidth);
			Logger.LogError("GetThumbnailSize(imageWidth={ImageWidth},imageHeight={ImageHeight},thumbnailWidth={ThumbnailWidth},thumbnailHeight={ThumbnailHeight}): thumbnailWidth → {ThumbnailWidth1}", imageWidth, imageHeight, thumbnailWidth, thumbnailHeight, thumbnailWidth);
            thumbnailHeight = GetMinorSize(imageHeight, imageWidth, thumbnailWidth);
			Logger.LogError("GetThumbnailSize(imageWidth={ImageWidth},imageHeight={ImageHeight},thumbnailWidth={ThumbnailWidth},thumbnailHeight={ThumbnailHeight}): thumbnailHeight → {ThumbnailHeight1}", imageWidth, imageHeight, thumbnailWidth, thumbnailHeight, thumbnailHeight);
        }
        else
        {
            thumbnailHeight = Math.Min(imageHeight, thumbnailHeight);
            thumbnailWidth = GetMinorSize(imageWidth, imageHeight, thumbnailHeight);
        }
		Logger.LogWarning("<<< Exiting GetThumbnailSize(imageWidth={ImageWidth},imageHeight={ImageHeight},thumbnailWidth={ThumbnailWidth},thumbnailHeight={ThumbnailHeight})", imageWidth, imageHeight, thumbnailWidth, thumbnailHeight);
    }

    private static int GetMinorSize(int imageMinorSize, int imageMajorSize, int thumbnailMajorSize)
    {
		Logger.LogError(">>> Entering GetMinorSize(imageMinorSize={ImageMinorSize},imageMajorSize={ImageMajorSize},thumbnailMajorSize={ThumbnailMajorSize})", imageMinorSize, imageMajorSize, thumbnailMajorSize);
        if (imageMajorSize == thumbnailMajorSize)
        {
            return imageMinorSize;
        }

        double calculated = (Convert.ToDouble(imageMinorSize) * Convert.ToDouble(thumbnailMajorSize)) / Convert.ToDouble(imageMajorSize);
		Logger.LogError("GetMinorSize(imageMinorSize={ImageMinorSize},imageMajorSize={ImageMajorSize},thumbnailMajorSize={ThumbnailMajorSize}): calculated → {Calculated}", imageMinorSize, imageMajorSize, thumbnailMajorSize, calculated);
		Logger.LogWarning("<<< Exiting GetMinorSize(imageMinorSize={ImageMinorSize},imageMajorSize={ImageMajorSize},thumbnailMajorSize={ThumbnailMajorSize})", imageMinorSize, imageMajorSize, thumbnailMajorSize);
        return Convert.ToInt32(Math.Round(calculated));
    }

    private static bool IsImageFile(string fileName)
    {
		Logger.LogError(">>> Entering IsImageFile(fileName={FileName})", fileName);
        Logger.LogWarning("<<< Exiting IsImageFile(fileName={FileName})", fileName);
        return (Globals.ImageFileTypes + ",").IndexOf(Path.GetExtension(fileName).Replace(".", string.Empty) + ",", StringComparison.InvariantCultureIgnoreCase) > -1;
    }

    private static bool IsResizePhotosEnabled(ModuleInfo module)
    {
		Logger.LogError(">>> Entering IsResizePhotosEnabled(module={Module})", module);
        Logger.LogWarning("<<< Exiting IsResizePhotosEnabled(module={Module})", module);
        return GetBooleanSetting(AllowResizePhotosSetting, false, module) &&
               GetBooleanSetting(AllowPhotosSetting, true, module) &&
               GetBooleanSetting(EditorEnabledSetting, true, module);
    }

    private static bool GetBooleanSetting(string settingName, bool defaultValue, ModuleInfo module)
    {
		Logger.LogError(">>> Entering GetBooleanSetting(settingName={SettingName},defaultValue={DefaultValue},module={Module})", settingName, defaultValue, module);
        if (module.ModuleSettings.Contains(settingName))
        {
            return Convert.ToBoolean(module.ModuleSettings[settingName].ToString());
        }

		Logger.LogWarning("<<< Exiting GetBooleanSetting(settingName={SettingName},defaultValue={DefaultValue},module={Module})", settingName, defaultValue, module);
        return defaultValue;
    }

    // none of the parameters should be null; checked before calling this method
    private void PrepareSecuritySet(JournalItem journalItem)
    {
		Logger.LogError(">>> Entering PrepareSecuritySet(journalItem={JournalItem})", journalItem);
        var originalSecuritySet =
            journalItem.SecuritySet = (journalItem.SecuritySet ?? string.Empty).ToUpperInvariant();

        if (string.IsNullOrEmpty(journalItem.SecuritySet))
        {
            journalItem.SecuritySet = "E,";
			Logger.LogDebug("PrepareSecuritySet(journalItem={JournalItem}): journalItem.SecuritySet → {Arg1}", journalItem, journalItem.SecuritySet);
        }
        else if (!journalItem.SecuritySet.EndsWith(",", StringComparison.Ordinal))
        {
            journalItem.SecuritySet += ",";
            originalSecuritySet = journalItem.SecuritySet;
        }

        if (journalItem.SecuritySet == "F,")
        {
            journalItem.SecuritySet = "F" + journalItem.UserId + ",";
            if (journalItem.ProfileId > 0)
            {
                journalItem.SecuritySet += "P" + journalItem.ProfileId + ",";
            }
        }
        else if (journalItem.SecuritySet == "U,")
        {
            journalItem.SecuritySet += "U" + journalItem.UserId + ",";
        }
        else if (journalItem.SecuritySet == "R,")
        {
            if (journalItem.SocialGroupId > 0)
            {
                journalItem.SecuritySet += "R" + journalItem.SocialGroupId + ",";
            }
        }

        if (journalItem.ProfileId > 0 && journalItem.UserId != journalItem.ProfileId)
        {
            if (!journalItem.SecuritySet.Contains("P" + journalItem.ProfileId + ","))
            {
                journalItem.SecuritySet += "P" + journalItem.ProfileId + ",";
            }

            if (!journalItem.SecuritySet.Contains("U" + journalItem.UserId + ","))
            {
                journalItem.SecuritySet += "U" + journalItem.UserId + ",";
            }
        }

        if (!journalItem.SecuritySet.Contains("U" + journalItem.UserId + ","))
        {
            journalItem.SecuritySet += "U" + journalItem.UserId + ",";
        }

        // if the post is marked as private, we shouldn't make it visible to the group.
        if (journalItem.SocialGroupId > 0 && originalSecuritySet.Contains("U,"))
        {
            var item = journalItem;
            var role = this.roleController.GetRole(
                journalItem.PortalId,
                r => r.SecurityMode != SecurityMode.SecurityRole && r.RoleID == item.SocialGroupId);

            if (role != null && !role.IsPublic)
            {
                journalItem.SecuritySet = journalItem.SecuritySet.Replace("E,", string.Empty).Replace("C,", string.Empty);
            }
        }

        // clean up and remove duplicates
        var parts = journalItem.SecuritySet
            .Replace(" ", string.Empty)
            .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            .Distinct()
            .Except(InvalidSecuritySetsWithoutId)
            .Where(p => p.IndexOfAny(ValidSecurityDescriptors) >= 0);

        // TODO: validate existence and visibility/accessability of all Roles added to the set (if any)
        journalItem.SecuritySet = string.Join(",", parts);
		Logger.LogWarning("<<< Exiting PrepareSecuritySet(journalItem={JournalItem})", journalItem);
    }

    private void UpdateGroupStats(int portalId, int groupId)
    {
		Logger.LogError(">>> Entering UpdateGroupStats(portalId={PortalId},groupId={GroupId})", portalId, groupId);
        var role = this.roleController.GetRole(portalId, r => r.RoleID == groupId);
		Logger.LogError("UpdateGroupStats(portalId={PortalId},groupId={GroupId}): role → {Role}", portalId, groupId, role);
        if (role is null)
        {
            return;
        }

        for (var i = 0; i < role.Settings.Keys.Count; i++)
        {
            var key = role.Settings.Keys.ElementAt(i);
			Logger.LogError("UpdateGroupStats(portalId={PortalId},groupId={GroupId}): key → {Key}", portalId, groupId, key);
            if (key.StartsWith("stat_", StringComparison.OrdinalIgnoreCase))
            {
                role.Settings[key] = "0";
            }
        }

        using (var dr = this.dataService.Journal_GetStatsForGroup(portalId, groupId))
        {
            while (dr.Read())
            {
                var settingName = "stat_" + dr["JournalType"];
				Logger.LogError("UpdateGroupStats(portalId={PortalId},groupId={GroupId}): settingName → {SettingName}", portalId, groupId, settingName);
                if (role.Settings.ContainsKey(settingName))
                {
                    role.Settings[settingName] = dr["JournalTypeCount"].ToString();
                }
                else
                {
                    role.Settings.Add(settingName, dr["JournalTypeCount"].ToString());
                }
            }

            dr.Close();
        }

        this.roleController.UpdateRoleSettings(role, true);
		Logger.LogWarning("<<< Exiting UpdateGroupStats(portalId={PortalId},groupId={GroupId})", portalId, groupId);
    }

    private void DeleteJournalItem(int portalId, int currentUserId, int journalId, bool softDelete)
    {
		Logger.LogError(">>> Entering DeleteJournalItem(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId},softDelete={SoftDelete})", portalId, currentUserId, journalId, softDelete);
        var ji = this.GetJournalItem(portalId, currentUserId, journalId, !softDelete);
		Logger.LogError("DeleteJournalItem(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId},softDelete={SoftDelete}): ji → {Ji}", portalId, currentUserId, journalId, softDelete, ji);
        if (ji == null)
        {
            return;
        }

        var groupId = ji.SocialGroupId;

        if (softDelete)
        {
            this.dataService.Journal_SoftDelete(journalId);
        }
        else
        {
            this.dataService.Journal_Delete(journalId);
        }

        if (groupId > 0)
        {
            this.UpdateGroupStats(portalId, groupId);
        }

        // queue remove journal from search index
        var document = new SearchDocumentToDelete
        {
            PortalId = portalId,
            AuthorUserId = currentUserId,
            UniqueKey = ji.ContentItemId.ToString("D", CultureInfo.InvariantCulture),

            ////QueryString = "journalid=" + journalId,
            SearchTypeId = this.searchHelper.GetSearchTypeByName("module").SearchTypeId,
        };

        if (groupId > 0)
        {
            document.RoleId = groupId;
			Logger.LogDebug("DeleteJournalItem(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId},softDelete={SoftDelete}): document.RoleId → {Arg4}", portalId, currentUserId, journalId, softDelete, document.RoleId);
        }

        this.dataProvider.AddSearchDeletedItems(document);
		Logger.LogWarning("<<< Exiting DeleteJournalItem(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId},softDelete={SoftDelete})", portalId, currentUserId, journalId, softDelete);
    }
}
