// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal
{
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Localization.Internal;

    /// <summary>Sets up DNN context information upon a request.</summary>
    public class DnnContextMessageHandler : MessageProcessingHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering ProcessRequest(request={Request},cancellationToken={CancellationToken})", request, cancellationToken);
            var portalSettings = SetupPortalSettings(request);
            SetThreadCulture(portalSettings);
			Logger.LogDebug("ProcessRequest(request={Request},cancellationToken={CancellationToken}): portalSettings → {PortalSettings}", request, cancellationToken, portalSettings);

			Logger.LogDebug("<<< Exiting ProcessRequest(request={Request},cancellationToken={CancellationToken})", request, cancellationToken);
            return request;
        }

        /// <inheritdoc />
        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering ProcessResponse(response={Response},cancellationToken={CancellationToken})", response, cancellationToken);
			Logger.LogDebug("<<< Exiting ProcessResponse(response={Response},cancellationToken={CancellationToken})", response, cancellationToken);
            return response;
        }

        private static void SetThreadCulture(PortalSettings portalSettings)
        {
            CultureInfo pageLocale = TestableLocalization.Instance.GetPageLocale(portalSettings);
            if (pageLocale != null)
            {
                TestableLocalization.Instance.SetThreadCultures(pageLocale, portalSettings);
            }
        }

        private static PortalSettings SetupPortalSettings(HttpRequestMessage request)
        {
		Logger.LogDebug(">>> Entering SetupPortalSettings(request={Request})", request);
#pragma warning disable CS0618 // Type or member is obsolete
            var domainName = TestableGlobals.Instance.GetDomainName(request.RequestUri);
            var alias = PortalAliasController.Instance.GetPortalAlias(domainName);
			Logger.LogDebug("SetupPortalSettings(request={Request}): domainName → {DomainName}", request, domainName);
			Logger.LogDebug("SetupPortalSettings(request={Request}): alias → {Alias}", request, alias);

            ValidateTabAndModuleContext(request, alias.PortalID, out var tabId);

            var portalSettings = new PortalSettings(tabId, alias);

			Logger.LogDebug("SetupPortalSettings(request={Request}): portalSettings → {PortalSettings}", request, portalSettings);
            request.GetHttpContext().Items["PortalSettings"] = portalSettings;
			Logger.LogDebug("<<< Exiting SetupPortalSettings(request={Request})", request);
            return portalSettings;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static bool TabIsInPortalOrHost(int tabId, int portalId)
        {
			Logger.LogDebug(">>> Entering TabIsInPortalOrHost(tabId={TabId},portalId={PortalId})", tabId, portalId);
            var tab = TabController.Instance.GetTab(tabId, portalId);

			Logger.LogDebug("TabIsInPortalOrHost(tabId={TabId},portalId={PortalId}): tab → {Tab}", tabId, portalId, tab);
			Logger.LogDebug("<<< Exiting TabIsInPortalOrHost(tabId={TabId},portalId={PortalId})", tabId, portalId);
            return tab != null && (IsHostTab(tab) || tab.PortalID == portalId || InSamePortalGroup(tab.PortalID, portalId));
        }

        private static bool InSamePortalGroup(int portalId1, int portalId2)
        {
			Logger.LogDebug(">>> Entering InSamePortalGroup(portalId1={PortalId1},portalId2={PortalId2})", portalId1, portalId2);
            IPortalInfo portal1 = PortalController.Instance.GetPortal(portalId1);
            IPortalInfo portal2 = PortalController.Instance.GetPortal(portalId2);
			Logger.LogDebug("InSamePortalGroup(portalId1={PortalId1},portalId2={PortalId2}): portal1 → {Portal1}", portalId1, portalId2, portal1);
			Logger.LogDebug("InSamePortalGroup(portalId1={PortalId1},portalId2={PortalId2}): portal2 → {Portal2}", portalId1, portalId2, portal2);

			Logger.LogDebug("<<< Exiting InSamePortalGroup(portalId1={PortalId1},portalId2={PortalId2})", portalId1, portalId2);
            return portal1 != null &&
                   portal2 != null &&
                   portal1.PortalGroupId > Null.NullInteger &&
                   portal2.PortalGroupId > Null.NullInteger &&
                   portal1.PortalGroupId == portal2.PortalGroupId;
        }

        private static bool IsHostTab(TabInfo tab)
        {
            return tab.PortalID == Null.NullInteger;
        }

        private static void ValidateTabAndModuleContext(HttpRequestMessage request, int portalId, out int tabId)
        {
			Logger.LogDebug(">>> Entering ValidateTabAndModuleContext(request={Request},portalId={PortalId})", request, portalId);
            tabId = request.FindTabId();

			Logger.LogDebug("ValidateTabAndModuleContext(request={Request},portalId={PortalId}): tabId → {TabId}", request, portalId, tabId);
            if (tabId != Null.NullInteger)
            {
                if (!TabIsInPortalOrHost(tabId, portalId))
                {
                    throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("TabNotInPortal", Localization.ExceptionsResourceFile)));
                }

                int moduleId = request.FindModuleId();

				Logger.LogDebug("ValidateTabAndModuleContext(request={Request},portalId={PortalId}): moduleId → {ModuleId}", request, portalId, moduleId);
                if (moduleId != Null.NullInteger)
                {
                    var module = ModuleController.Instance.GetModule(moduleId, tabId, false);
                    if (module == null)
					Logger.LogDebug("ValidateTabAndModuleContext(request={Request},portalId={PortalId}): module → {Module}", request, portalId, module);
                    {
                        throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("TabModuleNotExist", Localization.ExceptionsResourceFile)));
                    }
                }
            }
			Logger.LogDebug("<<< Exiting ValidateTabAndModuleContext(request={Request},portalId={PortalId})", request, portalId);
        }
    }
}
