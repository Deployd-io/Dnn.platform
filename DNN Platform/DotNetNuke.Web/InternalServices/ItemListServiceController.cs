// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using System.Web.Http;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.DataStructures;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Common;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>A web API controller for lists of items.</summary>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="dataProvider">The data provider.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="portalGroupController">The portal group controller.</param>
    /// <param name="vocabularyController">The vocabulary controller.</param>
    /// <param name="termController">The term controller.</param>
    [DnnAuthorize]
    public class ItemListServiceController(IHostSettings hostSettings, DataProvider dataProvider, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IVocabularyController vocabularyController, ITermController termController)
        : DnnApiController
    {
		Logger.LogDebug(">>> Entering ItemListServiceController(hostSettings={HostSettings},dataProvider={DataProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},vocabularyController={VocabularyController},termController={TermController})", hostSettings, dataProvider, portalController, appStatus, portalGroupController, vocabularyController, termController);
        private const string PortalPrefix = "P-";
        private const string RootKey = "Root";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ItemListServiceController));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug("ItemListServiceController(hostSettings={HostSettings},dataProvider={DataProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},vocabularyController={VocabularyController},termController={TermController}): Logger → {Logger}", hostSettings, dataProvider, portalController, appStatus, portalGroupController, vocabularyController, termController, Logger);
		Logger.LogDebug("ItemListServiceController(hostSettings={HostSettings},dataProvider={DataProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},vocabularyController={VocabularyController},termController={TermController}): hostSettings → {HostSettings1}", hostSettings, dataProvider, portalController, appStatus, portalGroupController, vocabularyController, termController, hostSettings);
        private readonly DataProvider dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
		Logger.LogDebug("ItemListServiceController(hostSettings={HostSettings},dataProvider={DataProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},vocabularyController={VocabularyController},termController={TermController}): dataProvider → {DataProvider1}", hostSettings, dataProvider, portalController, appStatus, portalGroupController, vocabularyController, termController, dataProvider);
		Logger.LogDebug("ItemListServiceController(hostSettings={HostSettings},dataProvider={DataProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},vocabularyController={VocabularyController},termController={TermController}): portalController → {PortalController1}", hostSettings, dataProvider, portalController, appStatus, portalGroupController, vocabularyController, termController, portalController);
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
		Logger.LogDebug("ItemListServiceController(hostSettings={HostSettings},dataProvider={DataProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},vocabularyController={VocabularyController},termController={TermController}): appStatus → {AppStatus1}", hostSettings, dataProvider, portalController, appStatus, portalGroupController, vocabularyController, termController, appStatus);
		Logger.LogDebug("ItemListServiceController(hostSettings={HostSettings},dataProvider={DataProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},vocabularyController={VocabularyController},termController={TermController}): portalGroupController → {PortalGroupController1}", hostSettings, dataProvider, portalController, appStatus, portalGroupController, vocabularyController, termController, portalGroupController);
        private readonly IVocabularyController vocabularyController = vocabularyController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IVocabularyController>();
        private readonly ITermController termController = termController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITermController>();
		Logger.LogDebug("ItemListServiceController(hostSettings={HostSettings},dataProvider={DataProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},vocabularyController={VocabularyController},termController={TermController}): vocabularyController → {VocabularyController1}", hostSettings, dataProvider, portalController, appStatus, portalGroupController, vocabularyController, termController, vocabularyController);
		Logger.LogDebug("ItemListServiceController(hostSettings={HostSettings},dataProvider={DataProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},vocabularyController={VocabularyController},termController={TermController}): termController → {TermController1}", hostSettings, dataProvider, portalController, appStatus, portalGroupController, vocabularyController, termController, termController);

        /// <summary>Initializes a new instance of the <see cref="ItemListServiceController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ItemListServiceController()
            : this(null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ItemListServiceController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IVocabularyController. Scheduled removal in v12.0.0.")]
        public ItemListServiceController(IHostSettings hostSettings, DataProvider dataProvider, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
            : this(hostSettings, dataProvider, portalController, appStatus, portalGroupController, null, null)
        {
		Logger.LogDebug("<<< Exiting ItemListServiceController(hostSettings={HostSettings},dataProvider={DataProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},vocabularyController={VocabularyController},termController={TermController})", hostSettings, dataProvider, portalController, appStatus, portalGroupController, vocabularyController, termController);
        }

        /// <summary>Gets a list of page descendants.</summary>
        /// <param name="parentId">The parent ID.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeAllTypes">Whether to include all page types.</param>
        /// <param name="includeActive">Whether to include active pages.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="roles">A semicolon-delimited list of role IDs.</param>
        /// <param name="disabledNotSelectable">Whether disabled pages are not selectable.</param>
        /// <returns>A response with a list of <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage GetPageDescendants(string parentId = null, int sortOrder = 0, string searchText = "", int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
			Logger.LogDebug(">>> Entering GetPageDescendants(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable})", parentId, sortOrder, searchText, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable);
            var response = new
            {
                Success = true,
                Items = this.GetPageDescendantsInternal(portalId, parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable),
            };
				Logger.LogDebug("GetPageDescendants(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): Success → {Success}", parentId, sortOrder, searchText, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, Success);
				Logger.LogDebug("GetPageDescendants(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): Items → {Items}", parentId, sortOrder, searchText, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, Items);
			Logger.LogDebug("<<< Exiting GetPageDescendants(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable})", parentId, sortOrder, searchText, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Gets the tree path for a page.</summary>
        /// <param name="itemId">The tab ID.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeAllTypes">Whether to include all page types.</param>
        /// <param name="includeActive">Whether to include active pages.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="roles">A semicolon-delimited list of role IDs.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage GetTreePathForPage(string itemId, int sortOrder = 0, int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
			Logger.LogDebug(">>> Entering GetTreePathForPage(itemId={ItemId},sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", itemId, sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            var response = new
            {
                Success = true,
                Tree = this.GetTreePathForPageInternal(portalId, itemId, sortOrder, false, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
				Logger.LogDebug("GetTreePathForPage(itemId={ItemId},sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Success → {Success}", itemId, sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Success);
				Logger.LogDebug("GetTreePathForPage(itemId={ItemId},sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Tree → {Tree}", itemId, sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Tree);
				Logger.LogDebug("GetTreePathForPage(itemId={ItemId},sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): IgnoreRoot → {IgnoreRoot}", itemId, sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, IgnoreRoot);
			Logger.LogDebug("<<< Exiting GetTreePathForPage(itemId={ItemId},sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", itemId, sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Sort the given <paramref name="treeAsJson"/>.</summary>
        /// <param name="treeAsJson">The tree as JSON.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeAllTypes">Whether to include all page types.</param>
        /// <param name="includeActive">Whether to include active pages.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="roles">A semicolon-delimited list of role IDs.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage SortPages(string treeAsJson, int sortOrder = 0, string searchText = "", int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
			Logger.LogDebug(">>> Entering SortPages(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", treeAsJson, sortOrder, searchText, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.SortPagesInternal(portalId, treeAsJson, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages)
                            : this.SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
				Logger.LogDebug("SortPages(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Success → {Success}", treeAsJson, sortOrder, searchText, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Success);
				Logger.LogDebug("SortPages(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Tree → {Tree}", treeAsJson, sortOrder, searchText, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Tree);
			Logger.LogDebug("<<< Exiting SortPages(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", treeAsJson, sortOrder, searchText, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Sort the given <paramref name="treeAsJson"/> in portal group.</summary>
        /// <param name="treeAsJson">The tree as JSON.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeAllTypes">Whether to include all page types.</param>
        /// <param name="includeActive">Whether to include active pages.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="roles">A semicolon-delimited list of role IDs.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage SortPagesInPortalGroup(string treeAsJson, int sortOrder = 0, string searchText = "", bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
			Logger.LogDebug(">>> Entering SortPagesInPortalGroup(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", treeAsJson, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.SortPagesInPortalGroupInternal(treeAsJson, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages)
                    : this.SearchPagesInPortalGroupInternal(treeAsJson, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
				Logger.LogDebug("SortPagesInPortalGroup(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Success → {Success}", treeAsJson, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Success);
				Logger.LogDebug("SortPagesInPortalGroup(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Tree → {Tree}", treeAsJson, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Tree);
			Logger.LogDebug("<<< Exiting SortPagesInPortalGroup(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", treeAsJson, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Gets pages.</summary>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeAllTypes">Whether to include all page types.</param>
        /// <param name="includeActive">Whether to include active pages.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="roles">A semicolon-delimited list of role IDs.</param>
        /// <param name="disabledNotSelectable">Whether disabled pages are not selectable.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage GetPages(int sortOrder = 0, int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
			Logger.LogDebug(">>> Entering GetPages(sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable})", sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable);
            var response = new
            {
                Success = true,
                Tree = this.GetPagesInternal(portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable),
                IgnoreRoot = true,
            };
				Logger.LogDebug("GetPages(sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): Success → {Success}", sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, Success);
				Logger.LogDebug("GetPages(sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): Tree → {Tree}", sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, Tree);
				Logger.LogDebug("GetPages(sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): IgnoreRoot → {IgnoreRoot}", sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, IgnoreRoot);
			Logger.LogDebug("<<< Exiting GetPages(sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable})", sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Gets pages in the current portal group.</summary>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage GetPagesInPortalGroup(int sortOrder = 0)
        {
			Logger.LogDebug(">>> Entering GetPagesInPortalGroup(sortOrder={SortOrder})", sortOrder);
            var response = new
            {
                Success = true,
                Tree = this.GetPagesInPortalGroupInternal(sortOrder),
                IgnoreRoot = true,
            };
				Logger.LogDebug("GetPagesInPortalGroup(sortOrder={SortOrder}): Success → {Success}", sortOrder, Success);
				Logger.LogDebug("GetPagesInPortalGroup(sortOrder={SortOrder}): Tree → {Tree}", sortOrder, Tree);
				Logger.LogDebug("GetPagesInPortalGroup(sortOrder={SortOrder}): IgnoreRoot → {IgnoreRoot}", sortOrder, IgnoreRoot);
			Logger.LogDebug("<<< Exiting GetPagesInPortalGroup(sortOrder={SortOrder})", sortOrder);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Search pages.</summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeAllTypes">Whether to include all page types.</param>
        /// <param name="includeActive">Whether to include active pages.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="roles">A semicolon-delimited list of role IDs.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage SearchPages(string searchText, int sortOrder = 0, int portalId = -1, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
			Logger.LogDebug(">>> Entering SearchPages(searchText={SearchText},sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", searchText, sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.GetPagesInternal(portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, false)
                    : this.SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
				Logger.LogDebug("SearchPages(searchText={SearchText},sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Success → {Success}", searchText, sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Success);
				Logger.LogDebug("SearchPages(searchText={SearchText},sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Tree → {Tree}", searchText, sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Tree);
			Logger.LogDebug("<<< Exiting SearchPages(searchText={SearchText},sortOrder={SortOrder},portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", searchText, sortOrder, portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Gets descendants of a page in the current portal group.</summary>
        /// <param name="parentId">The parent page ID.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeAllTypes">Whether to include all page types.</param>
        /// <param name="includeActive">Whether to include active pages.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="roles">A semicolon-delimited list of role IDs.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage GetPageDescendantsInPortalGroup(string parentId = null, int sortOrder = 0, string searchText = "", bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
			Logger.LogDebug(">>> Entering GetPageDescendantsInPortalGroup(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            var response = new
            {
                Success = true,
                Items = this.GetPageDescendantsInPortalGroupInternal(parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
            };
				Logger.LogDebug("GetPageDescendantsInPortalGroup(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Success → {Success}", parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Success);
				Logger.LogDebug("GetPageDescendantsInPortalGroup(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Items → {Items}", parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Items);
			Logger.LogDebug("<<< Exiting GetPageDescendantsInPortalGroup(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Gets the portals in the current portal group.</summary>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <returns>A response with an object that has <c>sites</c> and <c>portalId</c> fields.</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalsInGroup(int sortOrder = 0)
        {
			Logger.LogDebug(">>> Entering GetPortalsInGroup(sortOrder={SortOrder})", sortOrder);
            var sites = this.GetPortalGroup(sortOrder);
            var portalId = this.PortalSettings.PortalId;
			Logger.LogDebug("GetPortalsInGroup(sortOrder={SortOrder}): sites → {Sites}", sortOrder, sites);
			Logger.LogDebug("<<< Exiting GetPortalsInGroup(sortOrder={SortOrder})", sortOrder);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { sites, portalId });
        }

        /// <summary>Gets the tree path for a page in the current portal group.</summary>
        /// <param name="itemId">The tab ID.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeAllTypes">Whether to include all page types.</param>
        /// <param name="includeActive">Whether to include active pages.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="roles">A semicolon-delimited list of role IDs.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage GetTreePathForPageInPortalGroup(string itemId, int sortOrder = 0, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
			Logger.LogDebug(">>> Entering GetTreePathForPageInPortalGroup(itemId={ItemId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", itemId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            var response = new
            {
                Success = true,
                Tree = this.GetTreePathForPageInternal(itemId, sortOrder, true, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
				Logger.LogDebug("GetTreePathForPageInPortalGroup(itemId={ItemId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Success → {Success}", itemId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Success);
				Logger.LogDebug("GetTreePathForPageInPortalGroup(itemId={ItemId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Tree → {Tree}", itemId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Tree);
				Logger.LogDebug("GetTreePathForPageInPortalGroup(itemId={ItemId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): IgnoreRoot → {IgnoreRoot}", itemId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, IgnoreRoot);
			Logger.LogDebug("<<< Exiting GetTreePathForPageInPortalGroup(itemId={ItemId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", itemId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Search pages in the current portal group.</summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="includeDisabled">Whether to include disabled pages.</param>
        /// <param name="includeAllTypes">Whether to include all page types.</param>
        /// <param name="includeActive">Whether to include active pages.</param>
        /// <param name="includeHostPages">Whether to include host pages.</param>
        /// <param name="roles">A semicolon-delimited list of role IDs.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage SearchPagesInPortalGroup(string searchText, int sortOrder = 0, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
			Logger.LogDebug(">>> Entering SearchPagesInPortalGroup(searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.GetPagesInPortalGroupInternal(sortOrder)
                    : this.SearchPagesInPortalGroupInternal(searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles),
                IgnoreRoot = true,
            };
				Logger.LogDebug("SearchPagesInPortalGroup(searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Success → {Success}", searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Success);
				Logger.LogDebug("SearchPagesInPortalGroup(searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): Tree → {Tree}", searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, Tree);
			Logger.LogDebug("<<< Exiting SearchPagesInPortalGroup(searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Gets folder descendants.</summary>
        /// <param name="parentId">The parent ID.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="permission">The permission key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A response with an object that has an <c>Items</c> fields that's a list of <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage GetFolderDescendants(string parentId = null, int sortOrder = 0, string searchText = "", string permission = null, int portalId = -1)
        {
			Logger.LogDebug(">>> Entering GetFolderDescendants(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId})", parentId, sortOrder, searchText, permission, portalId);
            var response = new
            {
                Success = true,
                Items = this.GetFolderDescendantsInternal(portalId, parentId, sortOrder, searchText, permission),
            };
				Logger.LogDebug("GetFolderDescendants(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId}): Success → {Success}", parentId, sortOrder, searchText, permission, portalId, Success);
				Logger.LogDebug("GetFolderDescendants(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId}): Items → {Items}", parentId, sortOrder, searchText, permission, portalId, Items);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Get folders.</summary>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="permission">The permission key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="parentFolderId">The parent folder ID.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage GetFolders(int sortOrder = 0, string permission = null, int portalId = -1, int parentFolderId = -1)
        {
			Logger.LogDebug(">>> Entering GetFolders(sortOrder={SortOrder},permission={Permission},portalId={PortalId},parentFolderId={ParentFolderId})", sortOrder, permission, portalId, parentFolderId);
            var response = new
            {
                Success = true,
                Tree = this.GetFoldersInternal(portalId, sortOrder, permission, parentFolderId),
                IgnoreRoot = true,
            };
				Logger.LogDebug("GetFolders(sortOrder={SortOrder},permission={Permission},portalId={PortalId},parentFolderId={ParentFolderId}): Success → {Success}", sortOrder, permission, portalId, parentFolderId, Success);
				Logger.LogDebug("GetFolders(sortOrder={SortOrder},permission={Permission},portalId={PortalId},parentFolderId={ParentFolderId}): Tree → {Tree}", sortOrder, permission, portalId, parentFolderId, Tree);
				Logger.LogDebug("GetFolders(sortOrder={SortOrder},permission={Permission},portalId={PortalId},parentFolderId={ParentFolderId}): IgnoreRoot → {IgnoreRoot}", sortOrder, permission, portalId, parentFolderId, IgnoreRoot);
			Logger.LogDebug("<<< Exiting GetFolders(sortOrder={SortOrder},permission={Permission},portalId={PortalId},parentFolderId={ParentFolderId})", sortOrder, permission, portalId, parentFolderId);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Sorts the folder in <paramref name="treeAsJson"/>.</summary>
        /// <param name="treeAsJson">The tree as JSON.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="permission">The permission key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage SortFolders(string treeAsJson, int sortOrder = 0, string searchText = "", string permission = null, int portalId = -1)
        {
			Logger.LogDebug(">>> Entering SortFolders(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId})", treeAsJson, sortOrder, searchText, permission, portalId);
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.SortFoldersInternal(portalId, treeAsJson, sortOrder, permission) : this.SearchFoldersInternal(portalId, searchText, sortOrder, permission),
                IgnoreRoot = true,
            };
				Logger.LogDebug("SortFolders(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId}): Success → {Success}", treeAsJson, sortOrder, searchText, permission, portalId, Success);
				Logger.LogDebug("SortFolders(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId}): Tree → {Tree}", treeAsJson, sortOrder, searchText, permission, portalId, Tree);
				Logger.LogDebug("SortFolders(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId}): IgnoreRoot → {IgnoreRoot}", treeAsJson, sortOrder, searchText, permission, portalId, IgnoreRoot);
			Logger.LogDebug("<<< Exiting SortFolders(treeAsJson={TreeAsJson},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId})", treeAsJson, sortOrder, searchText, permission, portalId);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Get the tree path for a folder.</summary>
        /// <param name="itemId">The folder ID.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="permission">The permission key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage GetTreePathForFolder(string itemId, int sortOrder = 0, string permission = null, int portalId = -1)
        {
			Logger.LogDebug(">>> Entering GetTreePathForFolder(itemId={ItemId},sortOrder={SortOrder},permission={Permission},portalId={PortalId})", itemId, sortOrder, permission, portalId);
            var response = new
            {
                Success = true,
                Tree = this.GetTreePathForFolderInternal(itemId, sortOrder, permission),
                IgnoreRoot = true,
            };
				Logger.LogDebug("GetTreePathForFolder(itemId={ItemId},sortOrder={SortOrder},permission={Permission},portalId={PortalId}): Success → {Success}", itemId, sortOrder, permission, portalId, Success);
				Logger.LogDebug("GetTreePathForFolder(itemId={ItemId},sortOrder={SortOrder},permission={Permission},portalId={PortalId}): Tree → {Tree}", itemId, sortOrder, permission, portalId, Tree);
				Logger.LogDebug("GetTreePathForFolder(itemId={ItemId},sortOrder={SortOrder},permission={Permission},portalId={PortalId}): IgnoreRoot → {IgnoreRoot}", itemId, sortOrder, permission, portalId, IgnoreRoot);
			Logger.LogDebug("<<< Exiting GetTreePathForFolder(itemId={ItemId},sortOrder={SortOrder},permission={Permission},portalId={PortalId})", itemId, sortOrder, permission, portalId);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Search folders.</summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="permission">The permission key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage SearchFolders(string searchText, int sortOrder = 0, string permission = null, int portalId = -1)
        {
			Logger.LogDebug(">>> Entering SearchFolders(searchText={SearchText},sortOrder={SortOrder},permission={Permission},portalId={PortalId})", searchText, sortOrder, permission, portalId);
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.GetFoldersInternal(portalId, sortOrder, permission) : this.SearchFoldersInternal(portalId, searchText, sortOrder, permission),
                IgnoreRoot = true,
            };
				Logger.LogDebug("SearchFolders(searchText={SearchText},sortOrder={SortOrder},permission={Permission},portalId={PortalId}): Success → {Success}", searchText, sortOrder, permission, portalId, Success);
				Logger.LogDebug("SearchFolders(searchText={SearchText},sortOrder={SortOrder},permission={Permission},portalId={PortalId}): Tree → {Tree}", searchText, sortOrder, permission, portalId, Tree);
				Logger.LogDebug("SearchFolders(searchText={SearchText},sortOrder={SortOrder},permission={Permission},portalId={PortalId}): IgnoreRoot → {IgnoreRoot}", searchText, sortOrder, permission, portalId, IgnoreRoot);
			Logger.LogDebug("<<< Exiting SearchFolders(searchText={SearchText},sortOrder={SortOrder},permission={Permission},portalId={PortalId})", searchText, sortOrder, permission, portalId);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Gets the files.</summary>
        /// <param name="parentId">The parent ID.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="permission">The permission key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage GetFiles(int parentId, string filter, int sortOrder = 0, string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = this.GetFilesInternal(portalId, parentId, filter, string.Empty, sortOrder, permission),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Sort the files.</summary>
        /// <param name="parentId">The parent ID.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="permission">The permission key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage SortFiles(int parentId, string filter, int sortOrder = 0, string searchText = "", string permission = null, int portalId = -1)
        {
			Logger.LogDebug(">>> Entering SortFiles(parentId={ParentId},filter={Filter},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId})", parentId, filter, sortOrder, searchText, permission, portalId);
            var response = new
            {
                Success = true,
                Tree = string.IsNullOrEmpty(searchText) ? this.SortFilesInternal(portalId, parentId, filter, sortOrder, permission) : this.GetFilesInternal(portalId, parentId, filter, searchText, sortOrder, permission),
                IgnoreRoot = true,
            };
				Logger.LogDebug("SortFiles(parentId={ParentId},filter={Filter},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId}): Success → {Success}", parentId, filter, sortOrder, searchText, permission, portalId, Success);
				Logger.LogDebug("SortFiles(parentId={ParentId},filter={Filter},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId}): Tree → {Tree}", parentId, filter, sortOrder, searchText, permission, portalId, Tree);
				Logger.LogDebug("SortFiles(parentId={ParentId},filter={Filter},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId}): IgnoreRoot → {IgnoreRoot}", parentId, filter, sortOrder, searchText, permission, portalId, IgnoreRoot);
			Logger.LogDebug("<<< Exiting SortFiles(parentId={ParentId},filter={Filter},sortOrder={SortOrder},searchText={SearchText},permission={Permission},portalId={PortalId})", parentId, filter, sortOrder, searchText, permission, portalId);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Search files.</summary>
        /// <param name="parentId">The parent ID.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="sortOrder"><c>1</c> for A-Z, <c>2</c> for Z-A, any other value for no sorting.</param>
        /// <param name="permission">The permission key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A response with an object with a <c>Tree</c> field containing <see cref="ItemDto"/> objects.</returns>
        [HttpGet]
        public HttpResponseMessage SearchFiles(int parentId, string filter, string searchText, int sortOrder = 0, string permission = null, int portalId = -1)
        {
            var response = new
            {
                Success = true,
                Tree = this.GetFilesInternal(portalId, parentId, filter, searchText, sortOrder, permission),
                IgnoreRoot = true,
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Search users.</summary>
        /// <param name="q">The search criteria.</param>
        /// <returns>A response with a list of results (containing <c>id</c>, <c>name</c>, and <c>iconfile</c> fields) or <c>null</c> if there was no search query.</returns>
        [HttpGet]
        public HttpResponseMessage SearchUser(string q)
        {
			Logger.LogDebug(">>> Entering SearchUser(q={Q})", q);
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, this.PortalSettings.PortalId);
                const int numResults = 5;
				Logger.LogDebug("SearchUser(q={Q}): portalId → {PortalId}", q, portalId);

                // GetUsersAdvancedSearch doesn't accept a comma or a single quote in the query so we have to remove them for now. See issue 20224.
                q = q.Replace(",", string.Empty).Replace("'", string.Empty);
                if (q.Length == 0)
				Logger.LogDebug("SearchUser(q={Q}): q → {Q1}", q, q);
                {
                    return this.Request.CreateResponse<object>(HttpStatusCode.OK, null);
                }

                var results = UserController.Instance.GetUsersBasicSearch(portalId, 0, numResults, "DisplayName", true, "DisplayName", q)
                    .Select(user => new
                    {
                        id = user.UserID,
                        name = user.DisplayName,
                        iconfile = UserController.Instance.GetUserProfilePictureUrl(user.UserID, 32, 32),
                    }).ToList();
						Logger.LogDebug("SearchUser(q={Q}): name → {Name}", q, name);
						Logger.LogDebug("SearchUser(q={Q}): iconfile → {Iconfile}", q, iconfile);

                return this.Request.CreateResponse(HttpStatusCode.OK, results.OrderBy(sr => sr.name));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting SearchUser(q={Q})", q);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>Gets terms.</summary>
        /// <param name="q">The search criteria.</param>
        /// <param name="includeSystem">Whether to include terms from system vocabularies.</param>
        /// <param name="includeTags">Whether to include terms from the tags vocabulary.</param>
        /// <returns>A response with a list of objects with <c>text</c> and <c>value</c> fields.</returns>
        [HttpGet]
        public HttpResponseMessage GetTerms(string q, bool includeSystem, bool includeTags)
        {
			Logger.LogDebug(">>> Entering GetTerms(q={Q},includeSystem={IncludeSystem},includeTags={IncludeTags})", q, includeSystem, includeTags);
            var portalId = PortalSettings.Current.PortalId;

            var terms = new ArrayList();
            var vocabularies = from v in this.vocabularyController.GetVocabularies()
                               where (v.ScopeType.ScopeType == "Application"
                                      || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == portalId))
                                     && (!v.IsSystem || includeSystem)
                                     && (v.Name != "Tags" || includeTags)
                               select v;
							Logger.LogDebug("GetTerms(q={Q},includeSystem={IncludeSystem},includeTags={IncludeTags}): terms → {Terms}", q, includeSystem, includeTags, terms);

            foreach (var v in vocabularies)
            {
                terms.AddRange(new[]
                {
                    from t in this.termController.GetTermsByVocabulary(v.VocabularyId)
                    where string.IsNullOrEmpty(q) || t.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                    select new { text = t.Name, value = t.TermId },
                });
            }

			Logger.LogDebug("<<< Exiting GetTerms(q={Q},includeSystem={IncludeSystem},includeTags={IncludeTags})", q, includeSystem, includeTags);
            return this.Request.CreateResponse(HttpStatusCode.OK, terms);
        }

        private static List<ItemDto> GetChildrenOf(IEnumerable<TabInfo> tabs, int parentId, List<int> filterTabs = null)
        {
            return tabs.Where(tab => tab.ParentId == parentId).Select(tab => new ItemDto
            {
                Key = tab.TabID.ToString(CultureInfo.InvariantCulture),
                Value = tab.LocalizedTabName,
                HasChildren = tab.HasChildren,
                Selectable = filterTabs == null || filterTabs.Contains(tab.TabID),
            }).ToList();
        }

        private static List<ItemDto> GetChildrenOf(IEnumerable<TabInfo> tabs, string parentId)
        {
            int id;
            id = int.TryParse(parentId, out id) ? id : Null.NullInteger;
            return GetChildrenOf(tabs, id);
        }

        private static void SortPagesRecursively(IEnumerable<TabInfo> tabs, NTree<ItemDto> treeNode, NTree<ItemIdDto> openedNode, int sortOrder)
        {
            if (openedNode == null)
            {
                return;
            }

            var children = ApplySort(GetChildrenOf(tabs, openedNode.Data.Id), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            treeNode.Children = children;
            if (openedNode.HasChildren())
            {
                foreach (var openedNodeChild in openedNode.Children)
                {
                    var treeNodeChild = treeNode.Children.Find(child => string.Equals(child.Data.Key, openedNodeChild.Data.Id, StringComparison.OrdinalIgnoreCase));
                    if (treeNodeChild == null)
                    {
                        continue;
                    }

                    SortPagesRecursively(tabs, treeNodeChild, openedNodeChild, sortOrder);
                }
            }
			Logger.LogDebug("<<< Exiting SortPagesRecursively(tabs={Tabs},treeNode={TreeNode},openedNode={OpenedNode},sortOrder={SortOrder})", tabs, treeNode, openedNode, sortOrder);
        }

        private static IEnumerable<ItemDto> ApplySort(IEnumerable<ItemDto> items, int sortOrder)
        {
			Logger.LogDebug(">>> Entering ApplySort(items={Items},sortOrder={SortOrder})", items, sortOrder);
            switch (sortOrder)
            {
                case 1: // sort by a-z
                    return items.OrderBy(item => item.Value).ToList();
                case 2: // sort by z-a
                    return items.OrderByDescending(item => item.Value).ToList();
                default: // no sort
					Logger.LogDebug("<<< Exiting ApplySort(items={Items},sortOrder={SortOrder})", items, sortOrder);
                    return items;
            }
        }

        private static List<int> FilterTabsByRole(IList<TabInfo> tabs, string roles, bool disabledNotSelectable)
        {
            var filterTabs = new List<int>();
            if (!string.IsNullOrEmpty(roles))
            {
                var roleList = roles.Split(';').Select(int.Parse);

                filterTabs.AddRange(
                    tabs.Where(
                            t =>
                                t.TabPermissions.Cast<IPermissionInfo>()
                                    .Any(p => roleList.Contains(p.RoleId) && p.UserId == Null.NullInteger && p.PermissionKey == "VIEW" && p.AllowAccess)).ToList()
                        .Where(t => !disabledNotSelectable || !t.DisableLink)
                        .Select(t => t.TabID));
            }
            else
            {
                filterTabs.AddRange(tabs.Where(t => !disabledNotSelectable || !t.DisableLink).Select(t => t.TabID));
            }

            return filterTabs;
        }

        private static IEnumerable<IFileInfo> GetFiles(IFolderInfo parentFolder, string filter, string searchText)
        {
            Func<IFileInfo, bool> searchFunc;
            var filterList = string.IsNullOrEmpty(filter) ? null : filter.ToLowerInvariant().Split(',').ToList();
            if (string.IsNullOrEmpty(searchText))
            {
                searchFunc = f => filterList == null || filterList.Contains(f.Extension.ToLowerInvariant());
            }
            else
            {
                searchFunc = f => f.FileName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                                  && (filterList == null || filterList.Contains(f.Extension.ToLowerInvariant()));
            }

            return FolderManager.Instance.GetFiles(parentFolder).Where(f => searchFunc(f));
        }

        private NTree<ItemDto> GetPagesInPortalGroupInternal(int sortOrder)
        {
			Logger.LogDebug(">>> Entering GetPagesInPortalGroupInternal(sortOrder={SortOrder})", sortOrder);
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey, }, };
            var portals = this.GetPortalGroup(sortOrder);
            treeNode.Children = portals.Select(dto => new NTree<ItemDto> { Data = dto, }).ToList();
			Logger.LogDebug("GetPagesInPortalGroupInternal(sortOrder={SortOrder}): portals → {Portals}", sortOrder, portals);
			Logger.LogDebug("<<< Exiting GetPagesInPortalGroupInternal(sortOrder={SortOrder})", sortOrder);
            return treeNode;
        }

        private IEnumerable<ItemDto> GetPortalGroup(int sortOrder)
        {
			Logger.LogDebug(">>> Entering GetPortalGroup(sortOrder={SortOrder})", sortOrder);
            var myGroup = this.GetMyPortalGroup();
            var portals = myGroup.Select(p => new ItemDto
            {
			Logger.LogDebug("GetPortalGroup(sortOrder={SortOrder}): myGroup → {MyGroup}", sortOrder, myGroup);
                Key = PortalPrefix + p.PortalId.ToString(CultureInfo.InvariantCulture),
                Value = p.PortalName,
                HasChildren = true,
                Selectable = false,
            }).ToList();
			Logger.LogDebug("<<< Exiting GetPortalGroup(sortOrder={SortOrder})", sortOrder);
            return ApplySort(portals, sortOrder);
        }

        private IEnumerable<IPortalInfo> GetMyPortalGroup()
        {
			Logger.LogDebug(">>> Entering GetMyPortalGroup()");
            var groups = PortalGroupController.Instance.GetPortalGroups().ToArray();
            if (groups.Length != 0)
			Logger.LogDebug("GetMyPortalGroup(): groups → {Groups}", groups);
            {
                return (
                    from @group in groups
                    select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId) into portals
                    where portals.Any((IPortalInfo x) => x.PortalId == PortalSettings.Current.PortalId)
                    select portals.ToArray())
                    .FirstOrDefault();
            }

            var currentPortal = PortalController.Instance.GetPortal(this.PortalSettings.PortalId);
			Logger.LogDebug("GetMyPortalGroup(): currentPortal → {CurrentPortal}", currentPortal);
			Logger.LogDebug("<<< Exiting GetMyPortalGroup()");
            return new List<PortalInfo> { currentPortal, };
        }

        private NTree<ItemDto> GetPagesInternal(int portalId, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
			Logger.LogDebug(">>> Entering GetPagesInternal(portalId={PortalId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable})", portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable);
            if (portalId == -1)
            {
                portalId = this.GetActivePortalId();
            }
			Logger.LogDebug("GetPagesInternal(portalId={PortalId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): portalId → {PortalId1}", portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, portalId);

            var tabs = this.GetPortalPages(portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages);
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
			Logger.LogDebug("GetPagesInternal(portalId={PortalId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): tabs → {Tabs}", portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, tabs);
            if (tabs == null)
            {
                return sortedTree;
            }

            var filterTabs = FilterTabsByRole(tabs, roles, disabledNotSelectable);
            var children = ApplySort(GetChildrenOf(tabs, Null.NullInteger, filterTabs), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
			Logger.LogDebug("GetPagesInternal(portalId={PortalId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): filterTabs → {FilterTabs}", portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, filterTabs);
			Logger.LogDebug("GetPagesInternal(portalId={PortalId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): children → {Children}", portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, children);
            sortedTree.Children = children;
			Logger.LogDebug("GetPagesInternal(portalId={PortalId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): sortedTree.Children → {Arg8}", portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, sortedTree.Children);
			Logger.LogDebug("<<< Exiting GetPagesInternal(portalId={PortalId},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable})", portalId, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable);
            return sortedTree;
        }

        private IEnumerable<ItemDto> GetPageDescendantsInPortalGroupInternal(string parentId, int sortOrder, string searchText, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "")
        {
			Logger.LogDebug(">>> Entering GetPageDescendantsInPortalGroupInternal(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            if (string.IsNullOrEmpty(parentId))
            {
                return null;
            }

            int portalId;
            int parentIdAsInt;
            if (parentId.StartsWith(PortalPrefix, StringComparison.Ordinal))
            {
                parentIdAsInt = -1;
				Logger.LogDebug("GetPageDescendantsInPortalGroupInternal(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): parentIdAsInt → {ParentIdAsInt}", parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, parentIdAsInt);
                if (!int.TryParse(parentId.Replace(PortalPrefix, string.Empty), out portalId))
                {
                    portalId = -1;
					Logger.LogDebug("GetPageDescendantsInPortalGroupInternal(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): portalId → {PortalId}", parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, portalId);
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    return this.SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles).Children.Select(node => node.Data);
                }
            }
            else
            {
                portalId = -1;
                if (!int.TryParse(parentId, out parentIdAsInt))
                {
                    parentIdAsInt = -1;
                }
            }

			Logger.LogDebug("<<< Exiting GetPageDescendantsInPortalGroupInternal(parentId={ParentId},sortOrder={SortOrder},searchText={SearchText},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", parentId, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            return this.GetPageDescendantsInternal(portalId, parentIdAsInt, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
        }

        private IEnumerable<ItemDto> GetPageDescendantsInternal(int portalId, string parentId, int sortOrder, string searchText, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
            int id;
            id = int.TryParse(parentId, out id) ? id : Null.NullInteger;
            return this.GetPageDescendantsInternal(portalId, id, sortOrder, searchText, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable);
        }

        private IEnumerable<ItemDto> GetPageDescendantsInternal(int portalId, int parentId, int sortOrder, string searchText, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
            List<TabInfo> tabs;

            if (portalId == -1)
            {
                portalId = this.GetActivePortalId(parentId);
            }
            else
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return new List<ItemDto>();
                }
            }

            Func<TabInfo, bool> searchFunc;
            if (string.IsNullOrEmpty(searchText))
            {
                searchFunc = _ => true;
            }
            else
            {
                searchFunc = page => page.LocalizedTabName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }

            if (portalId > -1)
            {
                tabs = TabController.GetPortalTabs(this.hostSettings, this.appStatus, portalId, includeActive ? Null.NullInteger : this.PortalSettings.ActiveTab.TabID, false, null, true, false, includeAllTypes, true, false)
                    .Where(tab => searchFunc(tab)
                                  && tab.ParentId == parentId
                                  && (includeDisabled || !tab.DisableLink)
                                  && (includeAllTypes || tab.TabType == TabType.Normal)
                                  && !tab.IsSystem)
                    .OrderBy(tab => tab.TabOrder)
                    .ToList();

                if (this.PortalSettings.UserInfo.IsSuperUser && includeHostPages)
                {
                    tabs.AddRange(TabController.Instance.GetTabsByPortal(-1).AsList()
                        .Where(tab => searchFunc(tab) && tab.ParentId == parentId && !tab.IsDeleted && !tab.DisableLink && !tab.IsSystem)
                        .OrderBy(tab => tab.TabOrder)
                        .ToList());
                }
            }
            else
            {
                if (this.PortalSettings.UserInfo.IsSuperUser)
                {
                    tabs = TabController.Instance.GetTabsByPortal(-1).AsList()
                        .Where(tab => searchFunc(tab) && tab.ParentId == parentId && !tab.IsDeleted && !tab.DisableLink && !tab.IsSystem)
                        .OrderBy(tab => tab.TabOrder)
                        .ToList();
                }
                else
                {
                    return new List<ItemDto>();
                }
            }

            var filterTabs = FilterTabsByRole(tabs, roles, disabledNotSelectable);

            var pages = tabs.Select(tab => new ItemDto
            {
                Key = tab.TabID.ToString(CultureInfo.InvariantCulture),
                Value = tab.LocalizedTabName,
                HasChildren = tab.HasChildren,
                Selectable = filterTabs.Contains(tab.TabID),
            });

            return ApplySort(pages, sortOrder);
        }

        private NTree<ItemDto> SearchPagesInternal(int portalId, string searchText, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = true, bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
			Logger.LogDebug(">>> Entering SearchPagesInternal(portalId={PortalId},searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable})", portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable);
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            List<TabInfo> tabs;
            if (portalId == -1)
            {
                portalId = this.GetActivePortalId();
            }
			Logger.LogDebug("SearchPagesInternal(portalId={PortalId},searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): portalId → {PortalId1}", portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, portalId);
            else
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return tree;
                }
            }

            Func<TabInfo, bool> searchFunc;
            if (string.IsNullOrEmpty(searchText))
            {
                searchFunc = _ => true;
            }
            else
            {
                searchFunc = page => page.LocalizedTabName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }

            if (portalId > -1)
            {
                tabs = TabController.Instance.GetTabsByPortal(portalId).Where(tab =>
                        (includeActive || tab.Value.TabID != this.PortalSettings.ActiveTab.TabID)
                        && (includeDisabled || !tab.Value.DisableLink)
                        && (includeAllTypes || tab.Value.TabType == TabType.Normal)
                        && searchFunc(tab.Value)
                        && !tab.Value.IsSystem)
                    .OrderBy(tab => tab.Value.TabOrder)
                    .Select(tab => tab.Value)
                    .ToList();
						Logger.LogDebug("SearchPagesInternal(portalId={PortalId},searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): tabs → {Tabs}", portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, tabs);

                if (this.PortalSettings.UserInfo.IsSuperUser && includeHostPages)
                {
                    tabs.AddRange(TabController.Instance.GetTabsByPortal(-1).Where(tab => !tab.Value.DisableLink && searchFunc(tab.Value) && !tab.Value.IsSystem)
                        .OrderBy(tab => tab.Value.TabOrder)
                        .Select(tab => tab.Value)
                        .ToList());
                }
            }
            else
            {
                if (this.PortalSettings.UserInfo.IsSuperUser)
                {
                    tabs = TabController.Instance.GetTabsByPortal(-1).Where(tab => !tab.Value.DisableLink && searchFunc(tab.Value) && !tab.Value.IsSystem)
                        .OrderBy(tab => tab.Value.TabOrder)
                        .Select(tab => tab.Value)
                        .ToList();
                }
                else
                {
                    return tree;
                }
            }

            var filterTabs = FilterTabsByRole(tabs, roles, disabledNotSelectable);

			Logger.LogDebug("SearchPagesInternal(portalId={PortalId},searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable}): filterTabs → {FilterTabs}", portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable, filterTabs);
            var pages = tabs.Select(tab => new ItemDto
            {
                Key = tab.TabID.ToString(CultureInfo.InvariantCulture),
                Value = tab.LocalizedTabName,
                HasChildren = false,
                Selectable = filterTabs.Contains(tab.TabID),
            });

            tree.Children = ApplySort(pages, sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
			Logger.LogDebug("<<< Exiting SearchPagesInternal(portalId={PortalId},searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles},disabledNotSelectable={DisabledNotSelectable})", portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, disabledNotSelectable);
            return tree;
        }

        private List<TabInfo> GetPortalPages(int portalId, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false)
        {
            List<TabInfo> tabs = null;
            if (portalId == -1)
			Logger.LogDebug("GetPortalPages(portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages}): tabs → {Tabs}", portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, tabs);
            {
                portalId = this.GetActivePortalId();
            }
			Logger.LogDebug("GetPortalPages(portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages}): portalId → {PortalId1}", portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages, portalId);
            else
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return null;
                }
            }

            if (portalId > -1)
            {
                tabs = TabController.GetPortalTabs(this.hostSettings, this.appStatus, portalId, includeActive ? Null.NullInteger : this.PortalSettings.ActiveTab.TabID, false, null, true, false, includeAllTypes, true, false)
                    .Where(t => (!t.DisableLink || includeDisabled) && !t.IsSystem)
                    .ToList();

                if (this.PortalSettings.UserInfo.IsSuperUser && includeHostPages)
                {
                    tabs.AddRange(TabController.Instance.GetTabsByPortal(-1).AsList().Where(t => !t.IsDeleted && !t.DisableLink && !t.IsSystem).ToList());
                }
            }
            else
            {
                if (this.PortalSettings.UserInfo.IsSuperUser)
                {
                    tabs = TabController.Instance.GetTabsByPortal(-1).AsList().Where(t => !t.IsDeleted && !t.DisableLink && !t.IsSystem).ToList();
                }
            }

			Logger.LogDebug("<<< Exiting GetPortalPages(portalId={PortalId},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages})", portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages);
            return tabs;
        }

        private NTree<ItemDto> SortPagesInternal(int portalId, string treeAsJson, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false)
        {
            var tree = DotNetNuke.Common.Utilities.Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
            return this.SortPagesInternal(portalId, tree, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages);
        }

        private NTree<ItemDto> SortPagesInternal(int portalId, NTree<ItemIdDto> openedNodesTree, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false)
        {
            var pages = this.GetPortalPages(portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages);
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            if (pages == null)
            {
                return sortedTree;
            }

            SortPagesRecursively(pages, sortedTree, openedNodesTree, sortOrder);
            return sortedTree;
        }

        private NTree<ItemDto> SearchPagesInPortalGroupInternal(string searchText, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false, string roles = "")
        {
			Logger.LogDebug(">>> Entering SearchPagesInPortalGroupInternal(searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var portals = this.GetPortalGroup(sortOrder);
            treeNode.Children = portals.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
			Logger.LogDebug("SearchPagesInPortalGroupInternal(searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): portals → {Portals}", searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, portals);
            foreach (var child in treeNode.Children)
            {
                int portalId;
                if (int.TryParse(child.Data.Key.Replace(PortalPrefix, string.Empty), out portalId))
                {
                    var pageTree = this.SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
                    child.Children = pageTree.Children;
					Logger.LogDebug("SearchPagesInPortalGroupInternal(searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): pageTree → {PageTree}", searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, pageTree);
					Logger.LogDebug("SearchPagesInPortalGroupInternal(searchText={SearchText},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): child.Children → {Arg7}", searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, child.Children);
                }
            }

            return treeNode;
        }

        private NTree<ItemDto> SearchPagesInPortalGroupInternal(string treeAsJson, string searchText, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false, string roles = "")
        {
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var openedNode = DotNetNuke.Common.Utilities.Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
            if (openedNode == null)
            {
                return treeNode;
            }

            var portals = this.GetPortalGroup(sortOrder);
            treeNode.Children = portals.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();

            if (!openedNode.HasChildren())
            {
                return treeNode;
            }

            foreach (var openedNodeChild in openedNode.Children)
            {
                var portalIdString = openedNodeChild.Data.Id;
                var treeNodeChild = treeNode.Children.Find(child => string.Equals(child.Data.Key, portalIdString, StringComparison.OrdinalIgnoreCase));
                if (treeNodeChild == null)
                {
                    continue;
                }

                int portalId;
                if (int.TryParse(treeNodeChild.Data.Key.Replace(PortalPrefix, string.Empty), out portalId))
                {
                    var pageTree = this.SearchPagesInternal(portalId, searchText, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
                    treeNodeChild.Children = pageTree.Children;
                }
            }

            return treeNode;
        }

        private NTree<ItemDto> SortPagesInPortalGroupInternal(string treeAsJson, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false)
        {
			Logger.LogDebug(">>> Entering SortPagesInPortalGroupInternal(treeAsJson={TreeAsJson},sortOrder={SortOrder},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages})", treeAsJson, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages);
            var tree = DotNetNuke.Common.Utilities.Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
            return this.SortPagesInPortalGroupInternal(tree, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages);
        }

        private NTree<ItemDto> SortPagesInPortalGroupInternal(NTree<ItemIdDto> openedNode, int sortOrder, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false)
        {
            var treeNode = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            if (openedNode == null)
            {
                return treeNode;
            }

            var portals = this.GetPortalGroup(sortOrder);
            treeNode.Children = portals.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            if (openedNode.HasChildren())
            {
                foreach (var openedNodeChild in openedNode.Children)
                {
                    var portalIdString = openedNodeChild.Data.Id;
                    var treeNodeChild = treeNode.Children.Find(child => string.Equals(child.Data.Key, portalIdString, StringComparison.OrdinalIgnoreCase));
                    if (treeNodeChild == null)
                    {
                        continue;
                    }

                    int portalId;
                    if (!int.TryParse(portalIdString.Replace(PortalPrefix, string.Empty), out portalId))
                    {
                        portalId = -1;
                    }

                    var treeOfPages = this.SortPagesInternal(portalId, openedNodeChild, sortOrder, includeDisabled, includeAllTypes, includeActive, includeHostPages);
                    treeNodeChild.Children = treeOfPages.Children;
                }
            }

            return treeNode;
        }

        private NTree<ItemDto> GetTreePathForPageInternal(int portalId, string itemId, int sortOrder, bool includePortalTree = false, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false, string roles = "")
        {
			Logger.LogDebug(">>> Entering GetTreePathForPageInternal(portalId={PortalId},itemId={ItemId},sortOrder={SortOrder},includePortalTree={IncludePortalTree},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles})", portalId, itemId, sortOrder, includePortalTree, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
            int itemIdAsInt;
            if (string.IsNullOrEmpty(itemId) || !int.TryParse(itemId, out itemIdAsInt))
            {
                itemIdAsInt = Null.NullInteger;
				Logger.LogDebug("GetTreePathForPageInternal(portalId={PortalId},itemId={ItemId},sortOrder={SortOrder},includePortalTree={IncludePortalTree},includeDisabled={IncludeDisabled},includeAllTypes={IncludeAllTypes},includeActive={IncludeActive},includeHostPages={IncludeHostPages},roles={Roles}): itemIdAsInt → {ItemIdAsInt}", portalId, itemId, sortOrder, includePortalTree, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles, itemIdAsInt);
            }

            return this.GetTreePathForPageInternal(portalId, itemIdAsInt, sortOrder, includePortalTree, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
        }

        private NTree<ItemDto> GetTreePathForPageInternal(string itemId, int sortOrder, bool includePortalTree = false, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false, string roles = "")
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            if (string.IsNullOrEmpty(itemId) || !int.TryParse(itemId, out var itemIdAsInt))
            {
                return tree;
            }

            var portals = PortalController.GetPortalDictionary(this.hostSettings, this.dataProvider);
            int portalId;
            if (portals.TryGetValue(itemIdAsInt, out var pid))
            {
                portalId = pid;
            }
            else
            {
                return tree;
            }

            return this.GetTreePathForPageInternal(portalId, itemIdAsInt, sortOrder, includePortalTree, includeDisabled, includeAllTypes, includeActive, includeHostPages, roles);
        }

        private NTree<ItemDto> GetTreePathForPageInternal(int portalId, int selectedItemId, int sortOrder, bool includePortalTree = false, bool includeDisabled = false, bool includeAllTypes = false, bool includeActive = false, bool includeHostPages = false, string roles = "", bool disabledNotSelectable = false)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            if (selectedItemId <= 0)
            {
                return tree;
            }

            var pages = this.GetPortalPages(portalId, includeDisabled, includeAllTypes, includeActive, includeHostPages);

            if (pages == null)
            {
                return tree;
            }

            var page = pages.SingleOrDefault(pageInfo => pageInfo.TabID == selectedItemId);

            if (page == null)
            {
                return tree;
            }

            var selfTree = new NTree<ItemDto>
            {
                Data = new ItemDto
                {
                    Key = page.TabID.ToString(CultureInfo.InvariantCulture),
                    Value = page.LocalizedTabName,
                    HasChildren = page.HasChildren,
                    Selectable = true,
                },
            };

            var parentId = page.ParentId;
            var parentTab = parentId > 0 ? pages.SingleOrDefault(t => t.TabID == parentId) : null;
            var filterTabs = FilterTabsByRole(pages, roles, disabledNotSelectable);
            while (parentTab != null)
            {
                // load all siblings
                var siblingTabs = ApplySort(GetChildrenOf(pages, parentId, filterTabs), sortOrder);
                var siblingTabsTree = siblingTabs.Select(t => new NTree<ItemDto> { Data = t, }).ToList();

                // attach the tree
                if (selfTree.Children != null)
                {
                    foreach (var node in siblingTabsTree)
                    {
                        if (node.Data.Key == selfTree.Data.Key)
                        {
                            node.Children = selfTree.Children;
                            break;
                        }
                    }
                }

                selfTree = new NTree<ItemDto>
                {
                    Data = new ItemDto
                    {
                        Key = parentId.ToString(CultureInfo.InvariantCulture),
                        Value = parentTab.LocalizedTabName,
                        HasChildren = true,
                        Selectable = true,
                    },
                    Children = siblingTabsTree,
                };

                parentId = parentTab.ParentId;
                parentTab = parentId > 0 ? pages.SingleOrDefault(t => t.TabID == parentId) : null;
            }

            // retain root pages
            var rootTabs = ApplySort(GetChildrenOf(pages, Null.NullInteger, filterTabs), sortOrder);
            var rootTree = rootTabs.Select(dto => new NTree<ItemDto> { Data = dto }).ToList();

            foreach (var node in rootTree)
            {
                if (node.Data.Key == selfTree.Data.Key)
                {
                    node.Children = selfTree.Children;
                    break;
                }
            }

            if (includePortalTree)
            {
                var myGroup = this.GetMyPortalGroup();
                var portalTree = myGroup.Select(
                    portal => new NTree<ItemDto>
                    {
                        Data = new ItemDto
                        {
                            Key = PortalPrefix + portal.PortalId.ToString(CultureInfo.InvariantCulture),
                            Value = portal.PortalName,
                            HasChildren = true,
                            Selectable = false,
                        },
                    }).ToList();

                foreach (var node in portalTree)
                {
                    if (node.Data.Key == PortalPrefix + portalId.ToString(CultureInfo.InvariantCulture))
                    {
                        node.Children = rootTree;
                        break;
                    }
                }

                rootTree = portalTree;
            }

            tree.Children = rootTree;

            return tree;
        }

        private NTree<ItemDto> GetFoldersInternal(int portalId, int sortOrder, string permissions, int parentFolderId = -1)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var children = ApplySort(this.GetFolderDescendantsInternal(portalId, parentFolderId, sortOrder, string.Empty, permissions), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            tree.Children = children;
            foreach (var child in tree.Children)
            {
                children = ApplySort(this.GetFolderDescendantsInternal(portalId, child.Data.Key, sortOrder, string.Empty, permissions), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
                child.Children = children;
            }

            return tree;
        }

        private NTree<ItemDto> SortFoldersInternal(int portalId, string treeAsJson, int sortOrder, string permissions)
        {
            var tree = DotNetNuke.Common.Utilities.Json.Deserialize<NTree<ItemIdDto>>(treeAsJson);
            return this.SortFoldersInternal(portalId, tree, sortOrder, permissions);
        }

        private NTree<ItemDto> SortFoldersInternal(int portalId, NTree<ItemIdDto> openedNodesTree, int sortOrder, string permissions)
        {
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            this.SortFoldersRecursevely(portalId, sortedTree, openedNodesTree, sortOrder, permissions);
            return sortedTree;
        }

        private void SortFoldersRecursevely(int portalId, NTree<ItemDto> treeNode, NTree<ItemIdDto> openedNode, int sortOrder, string permissions)
        {
            if (openedNode == null)
            {
                return;
            }

            var children = ApplySort(this.GetFolderDescendantsInternal(portalId, openedNode.Data.Id, sortOrder, string.Empty, permissions), sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            treeNode.Children = children;
            if (openedNode.HasChildren())
            {
                foreach (var openedNodeChild in openedNode.Children)
                {
                    var treeNodeChild = treeNode.Children.Find(child => string.Equals(child.Data.Key, openedNodeChild.Data.Id, StringComparison.OrdinalIgnoreCase));
                    if (treeNodeChild == null)
                    {
                        continue;
                    }

                    this.SortFoldersRecursevely(portalId, treeNodeChild, openedNodeChild, sortOrder, permissions);
                }
            }
			Logger.LogDebug("<<< Exiting SortFoldersRecursevely(portalId={PortalId},treeNode={TreeNode},openedNode={OpenedNode},sortOrder={SortOrder},permissions={Permissions})", portalId, treeNode, openedNode, sortOrder, permissions);
        }

        private IEnumerable<ItemDto> GetFolderDescendantsInternal(int portalId, string parentId, int sortOrder, string searchText, string permission)
        {
            int id;
            id = int.TryParse(parentId, out id) ? id : Null.NullInteger;
            return this.GetFolderDescendantsInternal(portalId, id, sortOrder, searchText, permission);
        }

        private IEnumerable<ItemDto> GetFolderDescendantsInternal(int portalId, int parentId, int sortOrder, string searchText, string permission)
        {
            if (portalId > -1)
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return new List<ItemDto>();
                }
            }
            else
            {
                portalId = this.GetActivePortalId();
            }

            var parentFolder = parentId > -1 ? FolderManager.Instance.GetFolder(parentId) : FolderManager.Instance.GetFolder(portalId, string.Empty);

            if (parentFolder == null)
            {
                return new List<ItemDto>();
            }

            var hasPermission = string.IsNullOrEmpty(permission) ?
                this.HasPermission(parentFolder, "BROWSE") || this.HasPermission(parentFolder, "READ") :
                this.HasPermission(parentFolder, permission);
            if (!hasPermission)
            {
                return new List<ItemDto>();
            }

            if (parentId < 1)
            {
                return new List<ItemDto>
                {
                    new ItemDto
                    {
                        Key = parentFolder.FolderID.ToString(CultureInfo.InvariantCulture),
                        Value = portalId == -1 ? DynamicSharedConstants.HostRootFolder : DynamicSharedConstants.RootFolder,
                        HasChildren = this.HasChildren(parentFolder, permission),
                        Selectable = true,
                    },
                };
            }

            var childrenFolders = this.GetFolderDescendants(parentFolder, searchText, permission);

            var folders = childrenFolders.Select(folder => new ItemDto
            {
                Key = folder.FolderID.ToString(CultureInfo.InvariantCulture),
                Value = folder.FolderName,
                HasChildren = this.HasChildren(folder, permission),
                Selectable = true,
            });

            return ApplySort(folders, sortOrder);
        }

        private NTree<ItemDto> SearchFoldersInternal(int portalId, string searchText, int sortOrder, string permission)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            if (portalId > -1)
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return tree;
                }
            }
            else
            {
                portalId = this.GetActivePortalId();
            }
			Logger.LogDebug("SearchFoldersInternal(portalId={PortalId},searchText={SearchText},sortOrder={SortOrder},permission={Permission}): portalId → {PortalId1}", portalId, searchText, sortOrder, permission, portalId);

            var allFolders = this.GetPortalFolders(portalId, searchText, permission);
            var folders = allFolders.Select(f => new ItemDto
            {
			Logger.LogDebug("SearchFoldersInternal(portalId={PortalId},searchText={SearchText},sortOrder={SortOrder},permission={Permission}): allFolders → {AllFolders}", portalId, searchText, sortOrder, permission, allFolders);
                Key = f.FolderID.ToString(CultureInfo.InvariantCulture),
                Value = f.FolderName,
                HasChildren = false,
                Selectable = true,
            });
            tree.Children = ApplySort(folders, sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
			Logger.LogDebug("<<< Exiting SearchFoldersInternal(portalId={PortalId},searchText={SearchText},sortOrder={SortOrder},permission={Permission})", portalId, searchText, sortOrder, permission);
            return tree;
        }

        private NTree<ItemDto> GetTreePathForFolderInternal(string selectedItemId, int sortOrder, string permission)
        {
			Logger.LogDebug(">>> Entering GetTreePathForFolderInternal(selectedItemId={SelectedItemId},sortOrder={SortOrder},permission={Permission})", selectedItemId, sortOrder, permission);
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };

            int itemId;
            if (string.IsNullOrEmpty(selectedItemId) || !int.TryParse(selectedItemId, out itemId))
            {
                return tree;
            }

            if (itemId <= 0)
            {
                return tree;
            }

            var folder = FolderManager.Instance.GetFolder(itemId);
            if (folder == null)
			Logger.LogDebug("GetTreePathForFolderInternal(selectedItemId={SelectedItemId},sortOrder={SortOrder},permission={Permission}): folder → {Folder}", selectedItemId, sortOrder, permission, folder);
            {
                return tree;
            }

            var hasPermission = string.IsNullOrEmpty(permission) ?
                (this.HasPermission(folder, "BROWSE") || this.HasPermission(folder, "READ")) :
                this.HasPermission(folder, permission);
            if (!hasPermission)
            {
                return new NTree<ItemDto>();
            }

            var selfTree = new NTree<ItemDto>
            {
                Data = new ItemDto
                {
                    Key = folder.FolderID.ToString(CultureInfo.InvariantCulture),
                    Value = folder.FolderName,
                    HasChildren = this.HasChildren(folder, permission),
                    Selectable = true,
                },
            };
            var parentId = folder.ParentID;
            var parentFolder = parentId > 0 ? FolderManager.Instance.GetFolder(parentId) : null;
			Logger.LogDebug("GetTreePathForFolderInternal(selectedItemId={SelectedItemId},sortOrder={SortOrder},permission={Permission}): parentId → {ParentId}", selectedItemId, sortOrder, permission, parentId);
			Logger.LogDebug("GetTreePathForFolderInternal(selectedItemId={SelectedItemId},sortOrder={SortOrder},permission={Permission}): parentFolder → {ParentFolder}", selectedItemId, sortOrder, permission, parentFolder);

            while (parentFolder != null)
            {
                // load all sibling
                var siblingFolders = this.GetFolderDescendants(parentFolder, string.Empty, permission)
                    .Select(folderInfo => new ItemDto
                    {
					Logger.LogDebug("GetTreePathForFolderInternal(selectedItemId={SelectedItemId},sortOrder={SortOrder},permission={Permission}): siblingFolders → {SiblingFolders}", selectedItemId, sortOrder, permission, siblingFolders);
                        Key = folderInfo.FolderID.ToString(CultureInfo.InvariantCulture),
                        Value = folderInfo.FolderName,
                        HasChildren = this.HasChildren(folderInfo, permission),
                        Selectable = true,
                    }).ToList();
                siblingFolders = ApplySort(siblingFolders, sortOrder).ToList();

                var siblingFoldersTree = siblingFolders.Select(f => new NTree<ItemDto> { Data = f }).ToList();

				Logger.LogDebug("GetTreePathForFolderInternal(selectedItemId={SelectedItemId},sortOrder={SortOrder},permission={Permission}): siblingFoldersTree → {SiblingFoldersTree}", selectedItemId, sortOrder, permission, siblingFoldersTree);
                // attach the tree
                if (selfTree.Children != null)
                {
                    foreach (var node in siblingFoldersTree)
                    {
                        if (node.Data.Key == selfTree.Data.Key)
                        {
                            node.Children = selfTree.Children;
							Logger.LogDebug("GetTreePathForFolderInternal(selectedItemId={SelectedItemId},sortOrder={SortOrder},permission={Permission}): node.Children → {Arg3}", selectedItemId, sortOrder, permission, node.Children);
                            break;
                        }
                    }
                }

                selfTree = new NTree<ItemDto>
                {
                    Data = new ItemDto
                    {
                        Key = parentId.ToString(CultureInfo.InvariantCulture),
                        Value = parentFolder.FolderName,
                        HasChildren = true,
                        Selectable = true,
                    },
                    Children = siblingFoldersTree,
                };

                parentId = parentFolder.ParentID;
                parentFolder = parentId > 0 ? FolderManager.Instance.GetFolder(parentId) : null;
            }

            selfTree.Data.Value = DynamicSharedConstants.RootFolder;

            tree.Children.Add(selfTree);
			Logger.LogDebug("<<< Exiting GetTreePathForFolderInternal(selectedItemId={SelectedItemId},sortOrder={SortOrder},permission={Permission})", selectedItemId, sortOrder, permission);
            return tree;
        }

        private bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            var hasPermission = this.PortalSettings.UserInfo.IsSuperUser;

            if (!hasPermission && folder != null)
            {
                hasPermission = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermission;
        }

        private IEnumerable<IFolderInfo> GetFolderDescendants(IFolderInfo parentFolder, string searchText, string permission)
        {
            Func<IFolderInfo, bool> searchFunc;
            if (string.IsNullOrEmpty(searchText))
            {
                searchFunc = _ => true;
            }
            else
            {
                searchFunc = folder => folder.FolderName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }

            return FolderManager.Instance.GetFolders(parentFolder).Where(folder =>
                (string.IsNullOrEmpty(permission) ?
                    this.HasPermission(folder, "BROWSE") || this.HasPermission(folder, "READ") :
                    this.HasPermission(folder, permission)) && searchFunc(folder));
        }

        private IEnumerable<IFolderInfo> GetPortalFolders(int portalId, string searchText, string permission)
        {
            if (portalId == -1)
            {
                portalId = this.GetActivePortalId();
            }
			Logger.LogDebug("GetPortalFolders(portalId={PortalId},searchText={SearchText},permission={Permission}): portalId → {PortalId1}", portalId, searchText, permission, portalId);

            Func<IFolderInfo, bool> searchFunc;
            if (string.IsNullOrEmpty(searchText))
            {
                searchFunc = _ => true;
            }
            else
            {
                searchFunc = folder => folder.FolderName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }

			Logger.LogDebug("<<< Exiting GetPortalFolders(portalId={PortalId},searchText={SearchText},permission={Permission})", portalId, searchText, permission);
            return FolderManager.Instance.GetFolders(portalId).Where(folder =>
                (string.IsNullOrEmpty(permission) ?
                    this.HasPermission(folder, "BROWSE") || this.HasPermission(folder, "READ") :
                    this.HasPermission(folder, permission)) && searchFunc(folder));
        }

        private bool HasChildren(IFolderInfo parentFolder, string permission)
        {
            return FolderManager.Instance.GetFolders(parentFolder).Any(folder =>
                string.IsNullOrEmpty(permission) ?
                    this.HasPermission(folder, "BROWSE") || this.HasPermission(folder, "READ") :
                    this.HasPermission(folder, permission));
        }

        private NTree<ItemDto> GetFilesInternal(int portalId, int parentId, string filter, string searchText, int sortOrder, string permissions)
        {
            var tree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var children = this.GetFileItemsDto(portalId, parentId, filter, searchText, permissions, sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            tree.Children = children;
            return tree;
        }

        private NTree<ItemDto> SortFilesInternal(int portalId, int parentId, string filter, int sortOrder, string permissions)
        {
            var sortedTree = new NTree<ItemDto> { Data = new ItemDto { Key = RootKey } };
            var children = this.GetFileItemsDto(portalId, parentId, filter, string.Empty, permissions, sortOrder).Select(dto => new NTree<ItemDto> { Data = dto }).ToList();
            sortedTree.Children = children;
            return sortedTree;
        }

        private IEnumerable<ItemDto> GetFileItemsDto(int portalId, int parentId, string filter, string searchText, string permission, int sortOrder)
        {
            if (portalId > -1)
            {
                if (!this.IsPortalIdValid(portalId))
                {
                    return new List<ItemDto>();
                }
            }
            else
            {
                portalId = this.GetActivePortalId();
            }

            var parentFolder = parentId > -1 ? FolderManager.Instance.GetFolder(parentId) : FolderManager.Instance.GetFolder(portalId, string.Empty);

            if (parentFolder == null)
            {
                return new List<ItemDto>();
            }

            var hasPermission = string.IsNullOrEmpty(permission) ?
                this.HasPermission(parentFolder, "BROWSE") || this.HasPermission(parentFolder, "READ") :
                this.HasPermission(parentFolder, permission);
            if (!hasPermission)
            {
                return new List<ItemDto>();
            }

            if (parentId < 1)
            {
                return new List<ItemDto>();
            }

            var files = GetFiles(parentFolder, filter, searchText);

            var filesDto = files.Select(f => new ItemDto
            {
                Key = f.FileId.ToString(CultureInfo.InvariantCulture),
                Value = f.FileName,
                HasChildren = false,
                Selectable = true,
            }).ToList();

            var sortedList = ApplySort(filesDto, sortOrder);

            return sortedList;
        }

        private bool IsPortalIdValid(int portalId)
        {
            if (this.UserInfo.IsSuperUser)
            {
                return true;
            }

            if (this.PortalSettings.PortalId == portalId)
            {
                return true;
            }

            var isAdminUser = PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
            if (!isAdminUser)
            {
                return false;
            }

            var mygroup = this.GetMyPortalGroup();
            return mygroup != null && mygroup.Any(p => p.PortalId == portalId);
        }

        private int GetActivePortalId(int pageId)
        {
			Logger.LogDebug(">>> Entering GetActivePortalId(pageId={PageId})", pageId);
            var page = TabController.Instance.GetTab(pageId, Null.NullInteger, false);
            var portalId = page.PortalID;
			Logger.LogDebug("GetActivePortalId(pageId={PageId}): page → {Page}", pageId, page);
			Logger.LogDebug("GetActivePortalId(pageId={PageId}): portalId → {PortalId}", pageId, portalId);

            if (portalId == Null.NullInteger)
            {
                portalId = this.GetActivePortalId();
            }

			Logger.LogDebug("<<< Exiting GetActivePortalId(pageId={PageId})", pageId);
			Logger.LogDebug("<<< Exiting GetActivePortalId()");
            return portalId;
        }

        private int GetActivePortalId()
        {
            var portalId = -1;
            if (!TabController.CurrentPage.IsSuperTab)
            {
                portalId = this.PortalSettings.PortalId;
            }

            return portalId;
        }

        /// <summary>A data transfer object with information about an item in a list.</summary>
        [DataContract]
        public class ItemDto
        {
            /// <summary>Gets or sets the key.</summary>
            [DataMember(Name = "key")]
            public string Key { get; set; }

            /// <summary>Gets or sets the value.</summary>
            [DataMember(Name = "value")]
            public string Value { get; set; }

            /// <summary>Gets or sets a value indicating whether this item has children.</summary>
            [DataMember(Name = "hasChildren")]
            public bool HasChildren { get; set; }

            /// <summary>Gets or sets a value indicating whether this item is selectable.</summary>
            [DataMember(Name = "selectable")]
            public bool Selectable { get; set; }
        }

        /// <summary>A data transfer object with information about an item with an ID.</summary>
        [DataContract]
        public class ItemIdDto
        {
            /// <summary>Gets or sets the ID.</summary>
            [DataMember(Name = "id")]
            public string Id { get; set; }
        }
    }
}
