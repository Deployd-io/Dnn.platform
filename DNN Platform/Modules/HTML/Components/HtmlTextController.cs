// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Services.Tokens;
    using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;

    /// <summary>The HtmlTextController is the Controller class for managing HtmlText information the HtmlText module.</summary>
    /// <param name="navigationManager">A navigation manager.</param>
    /// <param name="portalAliasService">A portal alias service.</param>
    /// <param name="portalController">A portal controller.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="settingsRepository">The HTML settings repository.</param>
    public partial class HtmlTextController(INavigationManager navigationManager, IPortalAliasService portalAliasService, IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, HtmlModuleSettingsRepository settingsRepository)
        : ModuleSearchBase, IPortable, IUpgradeable, IVersionable
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Breaking Change")]
        public const int MAX_DESCRIPTION_LENGTH = 100;

        private const string PortalRootToken = "{{PortalRoot}}";

        private readonly IWorkflowManager workflowManager = WorkflowManager.Instance;
        private readonly IPortalAliasService portalAliasService = portalAliasService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly HtmlModuleSettingsRepository settingsRepository = settingsRepository ?? Globals.GetCurrentServiceProvider().GetRequiredService<HtmlModuleSettingsRepository>();

        /// <summary>Initializes a new instance of the <see cref="HtmlTextController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPortalAliasService. Scheduled removal in v12.0.0.")]
        public HtmlTextController()
            : this(null, null, null, null, null, null)
        {
		_logger.LogDebug(">>> Entering HtmlTextController()");
        }

        /// <summary>Initializes a new instance of the <see cref="HtmlTextController"/> class.</summary>
        /// <param name="navigationManager">A navigation manager.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPortalAliasService. Scheduled removal in v12.0.0.")]
        public HtmlTextController(INavigationManager navigationManager)
            : this(navigationManager, null, null, null, null, null)
        {
		_logger.LogDebug("<<< Exiting HtmlTextController()");
        }

        /// <summary>Initializes a new instance of the <see cref="HtmlTextController"/> class.</summary>
        /// <param name="navigationManager">A navigation manager.</param>
        /// <param name="portalAliasService">A portal alias service.</param>
        /// <param name="portalController">A portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public HtmlTextController(INavigationManager navigationManager, IPortalAliasService portalAliasService, IPortalController portalController, IApplicationStatusInfo appStatus)
            : this(navigationManager, portalAliasService, portalController, appStatus, null, null)
        {
        }

        /// <summary>Gets the navigation manager.</summary>
        protected INavigationManager NavigationManager { get; } = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        /// <summary>FormatHtmlText formats HtmlText content for display in the browser.</summary>
        /// <param name="moduleId">The ModuleID.</param>
        /// <param name="content">The HtmlText Content.</param>
        /// <param name="settings">Module Settings.</param>
        /// <param name="portalSettings">The Portal Settings.</param>
        /// <param name="page">The Page Instance.</param>
        /// <returns>The formatted HTML content.</returns>
        public static string FormatHtmlText(int moduleId, string content, HtmlModuleSettings settings, PortalSettings portalSettings, Page page)
        {
			_logger.LogDebug(">>> Entering FormatHtmlText(moduleId={ModuleId},content={Content},settings={Settings},portalSettings={PortalSettings},page={Page})", moduleId, content, settings, portalSettings, page);
            // Html decode content
            content = HttpUtility.HtmlDecode(content);
			_logger.LogDebug("FormatHtmlText(moduleId={ModuleId},content={Content},settings={Settings},portalSettings={PortalSettings},page={Page}): content → {Content1}", moduleId, content, settings, portalSettings, page, content);

            // token replace
            if (settings.ReplaceTokens)
            {
                var tr = new HtmlTokenReplace(page)
                {
                    AccessingUser = UserController.Instance.GetCurrentUserInfo(),
                    DebugMessages = Personalization.GetUserMode() != PortalSettings.Mode.View,
                    ModuleId = moduleId,
                    PortalSettings = portalSettings,
                };
                content = tr.ReplaceEnvironmentTokens(content);
            }

            // manage relative paths
            content = ManageRelativePaths(content, portalSettings.HomeDirectory, "src");
            content = ManageRelativePaths(content, portalSettings.HomeDirectory, "background");

			_logger.LogDebug("<<< Exiting FormatHtmlText(moduleId={ModuleId},content={Content},settings={Settings},portalSettings={PortalSettings},page={Page})", moduleId, content, settings, portalSettings, page);
            return content;
        }

        /// <inheritdoc cref="ManageRelativePaths(string,string,string)"/>
        [DnnDeprecated(9, 11, 0, "Use overload without int")]
        public static partial string ManageRelativePaths(string htmlContent, string strUploadDirectory, string strToken, int intPortalID)
        {
			_logger.LogDebug(">>> Entering ManageRelativePaths(htmlContent={HtmlContent},strUploadDirectory={StrUploadDirectory},strToken=<redacted>,intPortalID={IntPortalID})", htmlContent, strUploadDirectory, intPortalID);
			_logger.LogDebug("<<< Exiting ManageRelativePaths(htmlContent={HtmlContent},strUploadDirectory={StrUploadDirectory},strToken=<redacted>,intPortalID={IntPortalID})", htmlContent, strUploadDirectory, intPortalID);
			_logger.LogDebug("<<< Exiting ManageRelativePaths(htmlContent={HtmlContent},uploadDirectory={UploadDirectory},token=<redacted>)", htmlContent, uploadDirectory);
            return ManageRelativePaths(htmlContent, strUploadDirectory, strToken);
        }

        public static string ManageRelativePaths(string htmlContent, string uploadDirectory, string token)
        {
            var htmlContentIndex = 0;
            var sbBuff = new StringBuilder(string.Empty);

            if (string.IsNullOrEmpty(htmlContent))
            {
                return string.Empty;
            }

            token = token + "=\"";
            var tokenLength = token.Length;
            uploadDirectory = uploadDirectory.ToLowerInvariant();

            // find position of first occurrence:
            var tokenIndex = htmlContent.IndexOf(token, StringComparison.InvariantCultureIgnoreCase);
            while (tokenIndex != -1)
            {
                sbBuff.Append(htmlContent.Substring(htmlContentIndex, tokenIndex - htmlContentIndex + tokenLength));

                // keep characters left of URL
                htmlContentIndex = tokenIndex + tokenLength;

                // save start position of URL
                var urlEndIndex = htmlContent.IndexOf('\"', htmlContentIndex);

                // end of URL
                string strURL;
                if (urlEndIndex >= 0 && urlEndIndex < htmlContent.Length - 2)
                {
                    strURL = htmlContent.Substring(htmlContentIndex, urlEndIndex - htmlContentIndex).ToLowerInvariant();
                }
                else
                {
                    tokenIndex = -1;
                    continue;
                }

                if (htmlContent.Substring(tokenIndex + tokenLength, 10).Equals("data:image", StringComparison.InvariantCultureIgnoreCase))
                {
                    tokenIndex = htmlContent.IndexOf(token, htmlContentIndex + strURL.Length + 2, StringComparison.InvariantCultureIgnoreCase);
                    continue;
                }

                // if we are linking internally
                if (!strURL.Contains("://"))
                {
                    // remove the leading portion of the path if the URL contains the upload directory structure
                    var strDirectory = uploadDirectory;
                    if (!strDirectory.EndsWith("/", StringComparison.Ordinal))
                    {
                        strDirectory += "/";
                    }

                    if (strURL.IndexOf(strDirectory, StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        htmlContentIndex = htmlContentIndex + strURL.IndexOf(strDirectory, StringComparison.InvariantCultureIgnoreCase) + strDirectory.Length;
                        strURL = strURL.Substring(strURL.IndexOf(strDirectory, StringComparison.InvariantCultureIgnoreCase) + strDirectory.Length);
                    }

                    // add upload directory
                    // We don't write the UploadDirectory if the token/attribute has not value. Therefore we will avoid an unnecessary request
                    if (!strURL.StartsWith("/", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(strURL))
                    {
                        sbBuff.Append(uploadDirectory);
                    }
                }

                // find position of next occurrence
                tokenIndex = htmlContent.IndexOf(token, htmlContentIndex + strURL.Length + 2, StringComparison.InvariantCultureIgnoreCase);
            }

            // append characters of last URL and behind
            if (htmlContentIndex > -1)
            {
                sbBuff.Append(htmlContent.Substring(htmlContentIndex));
            }

            return sbBuff.ToString();
        }

        public string ReplaceWithRootToken(Match m)
        {
			_logger.LogDebug(">>> Entering ReplaceWithRootToken(m={M})", m);
            var domain = m.Groups["domain"].Value;

			_logger.LogDebug("ReplaceWithRootToken(m={M}): domain → {Domain}", m, domain);
            // Relative url
            if (string.IsNullOrEmpty(domain))
            {
                return PortalRootToken;
            }

            var aliases = this.portalAliasService.GetPortalAliases();
			_logger.LogDebug("ReplaceWithRootToken(m={M}): aliases → {Aliases}", m, aliases);
            if (!aliases.ContainsKey(domain))
            {
                // this is no not a portal url so even if it contains /portals/..
                // we do not need to replace it with a token
                return m.ToString();
            }

            // full qualified portal url that needs to be tokenized
            var result = domain + PortalRootToken;
			_logger.LogDebug("ReplaceWithRootToken(m={M}): result → {Result}", m, result);
            var protocol = m.Groups["protocol"].Value;
			_logger.LogDebug("ReplaceWithRootToken(m={M}): protocol → {Protocol}", m, protocol);
			_logger.LogDebug("<<< Exiting ReplaceWithRootToken(m={M})", m);
            return string.IsNullOrEmpty(protocol) ? result : protocol + result;
        }

        /// <summary>DeleteHtmlText deletes an HtmlTextInfo object for the Module and Item.</summary>
        /// <param name="moduleID">The ID of the Module.</param>
        /// <param name="itemID">The ID of the Item.</param>
        public void DeleteHtmlText(int moduleID, int itemID)
        {
			_logger.LogDebug(">>> Entering DeleteHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
            var moduleVersion = this.GetHtmlText(moduleID, itemID).Version;

			_logger.LogDebug("DeleteHtmlText(moduleID={ModuleID},itemID={ItemID}): moduleVersion → {ModuleVersion}", moduleID, itemID, moduleVersion);
            DataProvider.Instance().DeleteHtmlText(moduleID, itemID);

            // refresh output cache
            ModuleController.SynchronizeModule(moduleID);

            TrackModuleModification(moduleID, moduleVersion);
			_logger.LogDebug("<<< Exiting DeleteHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
        }

        /// <summary>GetAllHtmlText gets a collection of HtmlTextInfo objects for the Module and Workflow.</summary>
        /// <param name="moduleID">The ID of the Module.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="HtmlTextInfo"/> instances.</returns>
        public List<HtmlTextInfo> GetAllHtmlText(int moduleID)
        {
			_logger.LogDebug(">>> Entering GetAllHtmlText(moduleID={ModuleID})", moduleID);
			_logger.LogDebug("<<< Exiting GetAllHtmlText(moduleID={ModuleID})", moduleID);
            return CBO.FillCollection<HtmlTextInfo>(DataProvider.Instance().GetAllHtmlText(moduleID));
        }

        /// <summary>GetHtmlText gets the HtmlTextInfo object for the Module, Item, and Workflow.</summary>
        /// <param name="moduleID">The ID of the Module.</param>
        /// <param name="itemID">The ID of the Item.</param>
        /// <returns>An <see cref="HtmlTextInfo"/> instance or <see langword="null"/>.</returns>
        public HtmlTextInfo GetHtmlText(int moduleID, int itemID)
        {
			_logger.LogDebug(">>> Entering GetHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
			_logger.LogDebug("<<< Exiting GetHtmlText(moduleID={ModuleID},itemID={ItemID})", moduleID, itemID);
            return CBO.FillObject<HtmlTextInfo>(DataProvider.Instance().GetHtmlText(moduleID, itemID));
        }

        /// <summary>GetTopHtmlText gets the most recent HtmlTextInfo object for the Module, Workflow, and State.</summary>
        /// <param name="moduleId">The ID of the Module.</param>
        /// <param name="isPublished">Whether the content has been published or not.</param>
        /// <param name="workflowId">The Workflow ID.</param>
        /// <returns>An <see cref="HtmlTextInfo"/> instance or <see langword="null"/>.</returns>
        public HtmlTextInfo GetTopHtmlText(int moduleId, bool isPublished, int workflowId)
        {
			_logger.LogDebug(">>> Entering GetTopHtmlText(moduleId={ModuleId},isPublished={IsPublished},workflowId={WorkflowId})", moduleId, isPublished, workflowId);
            var htmlText = CBO.FillObject<HtmlTextInfo>(DataProvider.Instance().GetTopHtmlText(moduleId, isPublished));
            if (htmlText != null)
			_logger.LogDebug("GetTopHtmlText(moduleId={ModuleId},isPublished={IsPublished},workflowId={WorkflowId}): htmlText → {HtmlText}", moduleId, isPublished, workflowId, htmlText);
            {
                // check if workflow has changed
                if (isPublished == false && htmlText.WorkflowID != workflowId)
                {
                    // get proper state for workflow
                    htmlText.WorkflowID = workflowId;
					_logger.LogDebug("GetTopHtmlText(moduleId={ModuleId},isPublished={IsPublished},workflowId={WorkflowId}): htmlText.WorkflowID → {Arg3}", moduleId, isPublished, workflowId, htmlText.WorkflowID);
					_logger.LogDebug("GetTopHtmlText(moduleId={ModuleId},isPublished={IsPublished},workflowId={WorkflowId}): htmlText.WorkflowName → {Arg3}", moduleId, isPublished, workflowId, htmlText.WorkflowName);
                    htmlText.WorkflowName = "[REPAIR_WORKFLOW]";
                    htmlText.StateID = htmlText.IsPublished
                                        ? this.workflowManager.GetWorkflow(workflowId).LastState.StateID
                                        : this.workflowManager.GetWorkflow(workflowId).FirstState.StateID;
										_logger.LogDebug("GetTopHtmlText(moduleId={ModuleId},isPublished={IsPublished},workflowId={WorkflowId}): htmlText.StateID → {Arg3}", moduleId, isPublished, workflowId, htmlText.StateID);

                    // update object
                    this.UpdateHtmlText(htmlText, this.GetMaximumVersionHistory(htmlText.PortalID));

                    // get object again
                    htmlText = CBO.FillObject<HtmlTextInfo>(DataProvider.Instance().GetTopHtmlText(moduleId, false));
                }
            }

			_logger.LogDebug("<<< Exiting GetTopHtmlText(moduleId={ModuleId},isPublished={IsPublished},workflowId={WorkflowId})", moduleId, isPublished, workflowId);
            return htmlText;
        }

        /// <summary>GetWorkFlow retrieves the currently active Workflow for the Portal.</summary>
        /// <param name="moduleId">The ID of the Module.</param>
        /// <param name="tabId">The Tab ID.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <returns>A <see cref="KeyValuePair{TKey,TValue}"/> where the <see cref="KeyValuePair{TKey,TValue}.Key"/> is the workflow type, and the <see cref="KeyValuePair{TKey,TValue}.Value"/> is the workflow ID.</returns>
        public KeyValuePair<string, int> GetWorkflow(int moduleId, int tabId, int portalId)
        {
            var tab = TabController.Instance.GetTab(tabId, portalId);

            var workFlowId = tab.StateID == Null.NullInteger
                ? TabWorkflowSettings.Instance.GetDefaultTabWorkflowId(portalId)
                : WorkflowStateManager.Instance.GetWorkflowState(tab.StateID).WorkflowID;

            var workFlowType = WorkflowManager.Instance.GetWorkflow(workFlowId).WorkflowName;

            return new KeyValuePair<string, int>(workFlowType, workFlowId);
        }

        /// <summary>UpdateHtmlText creates a new HtmlTextInfo object or updates an existing HtmlTextInfo object.</summary>
        /// <param name="htmlContent">An HtmlTextInfo object.</param>
        /// <param name="maximumVersionHistory">The maximum number of versions to retain.</param>
        public void UpdateHtmlText(HtmlTextInfo htmlContent, int maximumVersionHistory)
        {
			_logger.LogDebug(">>> Entering UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory})", htmlContent, maximumVersionHistory);
            bool blnCreateNewVersion = false;

			_logger.LogDebug("UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory}): blnCreateNewVersion → {BlnCreateNewVersion}", htmlContent, maximumVersionHistory, blnCreateNewVersion);
            // determine if we are creating a new version of content or updating an existing version
            if (htmlContent.ItemID != -1)
            {
                if (htmlContent.WorkflowName != "[REPAIR_WORKFLOW]")
                {
                    HtmlTextInfo objContent = this.GetTopHtmlText(htmlContent.ModuleID, false, htmlContent.WorkflowID);
					_logger.LogDebug("UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory}): objContent → {ObjContent}", htmlContent, maximumVersionHistory, objContent);
                    if (objContent != null)
                    {
                        if (objContent.StateID == this.workflowManager.GetWorkflow(htmlContent.WorkflowID).LastState.StateID)
                        {
                            blnCreateNewVersion = true;
                        }
                    }
                }
            }
            else
            {
                blnCreateNewVersion = true;
            }

            // determine if content is published
            if (htmlContent.StateID == this.workflowManager.GetWorkflow(htmlContent.WorkflowID).LastState.StateID)
            {
                htmlContent.IsPublished = true;
				_logger.LogDebug("UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory}): htmlContent.IsPublished → {Arg2}", htmlContent, maximumVersionHistory, htmlContent.IsPublished);
            }
            else
            {
                htmlContent.IsPublished = false;
            }

            if (blnCreateNewVersion)
            {
                // add content
                htmlContent.ItemID = DataProvider.Instance().AddHtmlText(
                    htmlContent.ModuleID,
                    htmlContent.Content,
                    htmlContent.Summary,
                    htmlContent.StateID,
                    htmlContent.IsPublished,
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    maximumVersionHistory);
            }
            else
            {
                // update content
                DataProvider.Instance().UpdateHtmlText(htmlContent.ItemID, htmlContent.Content, htmlContent.Summary, htmlContent.StateID, htmlContent.IsPublished, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // refresh to get version
            htmlContent = this.GetHtmlText(htmlContent.ModuleID, htmlContent.ItemID);
			_logger.LogDebug("UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory}): htmlContent → {HtmlContent1}", htmlContent, maximumVersionHistory, htmlContent);

            // add log history
            var logInfo = new HtmlTextLogInfo();
			_logger.LogDebug("UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory}): logInfo → {LogInfo}", htmlContent, maximumVersionHistory, logInfo);
			_logger.LogDebug("UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory}): logInfo.ItemID → {Arg2}", htmlContent, maximumVersionHistory, logInfo.ItemID);
			_logger.LogDebug("UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory}): logInfo.StateID → {Arg2}", htmlContent, maximumVersionHistory, logInfo.StateID);
			_logger.LogDebug("UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory}): logInfo.Approved → {Arg2}", htmlContent, maximumVersionHistory, logInfo.Approved);
			_logger.LogDebug("UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory}): logInfo.Comment → {Arg2}", htmlContent, maximumVersionHistory, logInfo.Comment);
            logInfo.ItemID = htmlContent.ItemID;
            logInfo.StateID = htmlContent.StateID;
            logInfo.Approved = htmlContent.Approved;
            logInfo.Comment = htmlContent.Comment;
            var objLogs = new HtmlTextLogController();
			_logger.LogDebug("UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory}): objLogs → {ObjLogs}", htmlContent, maximumVersionHistory, objLogs);
            objLogs.AddHtmlTextLog(logInfo);

            // create user notifications
            this.CreateUserNotifications(htmlContent);

            // refresh output cache
            ModuleController.SynchronizeModule(htmlContent.ModuleID);

            var moduleVersion = htmlContent.Version < 1 ? 1 : htmlContent.Version;

            TrackModuleModification(htmlContent.ModuleID, moduleVersion);
			_logger.LogDebug("<<< Exiting UpdateHtmlText(htmlContent={HtmlContent},maximumVersionHistory={MaximumVersionHistory})", htmlContent, maximumVersionHistory);
        }

        /// <summary>GetMaximumVersionHistory retrieves the maximum number of versions to store for a module.</summary>
        /// <param name="portalID">The ID of the Portal.</param>
        /// <returns>The maximum number of versions for the portal (defaults to <c>5</c>).</returns>
        public int GetMaximumVersionHistory(int portalID)
        {
			_logger.LogDebug(">>> Entering GetMaximumVersionHistory(portalID={PortalID})", portalID);
            int intMaximumVersionHistory = -1;

			_logger.LogDebug("GetMaximumVersionHistory(portalID={PortalID}): intMaximumVersionHistory → {IntMaximumVersionHistory}", portalID, intMaximumVersionHistory);
            // get from portal settings
            intMaximumVersionHistory = int.Parse(PortalController.GetPortalSetting(this.portalController, "MaximumVersionHistory", portalID, "-1"));

            // if undefined at portal level, set portal default
            if (intMaximumVersionHistory == -1)
            {
                intMaximumVersionHistory = 5;

                // default
                PortalController.UpdatePortalSetting(this.portalController, portalID, "MaximumVersionHistory", intMaximumVersionHistory.ToString());
            }

			_logger.LogDebug("<<< Exiting GetMaximumVersionHistory(portalID={PortalID})", portalID);
            return intMaximumVersionHistory;
        }

        /// <summary>UpdateWorkFlowID updates the currently active WorkflowID for the Portal.</summary>
        /// <param name="portalID">The ID of the Portal.</param>
        /// <param name="maximumVersionHistory">The MaximumVersionHistory.</param>
        public void UpdateMaximumVersionHistory(int portalID, int maximumVersionHistory)
        {
			_logger.LogDebug(">>> Entering UpdateMaximumVersionHistory(portalID={PortalID},maximumVersionHistory={MaximumVersionHistory})", portalID, maximumVersionHistory);
            // data integrity check
            if (maximumVersionHistory < 0)
            {
                maximumVersionHistory = 5;

                // default
            }

            // save portal setting
            var objPortalSettings = PortalController.Instance.GetCurrentSettings();
			_logger.LogDebug("UpdateMaximumVersionHistory(portalID={PortalID},maximumVersionHistory={MaximumVersionHistory}): objPortalSettings → {ObjPortalSettings}", portalID, maximumVersionHistory, objPortalSettings);
            if (PortalSecurity.IsInRole(objPortalSettings.AdministratorRoleName))
            {
                PortalController.UpdatePortalSetting(this.portalController, portalID, "MaximumVersionHistory", maximumVersionHistory.ToString());
            }
			_logger.LogDebug("<<< Exiting UpdateMaximumVersionHistory(portalID={PortalID},maximumVersionHistory={MaximumVersionHistory})", portalID, maximumVersionHistory);
        }

        public void DeleteVersion(int moduleId, int version)
        {
			_logger.LogDebug(">>> Entering DeleteVersion(moduleId={ModuleId},version={Version})", moduleId, version);
            var htmlContent = this.GetAllHtmlText(moduleId).First(h => h.Version == version || h.Version == this.GetLatestVersion(moduleId));

			_logger.LogDebug("DeleteVersion(moduleId={ModuleId},version={Version}): htmlContent → {HtmlContent}", moduleId, version, htmlContent);
            this.DeleteHtmlText(moduleId, htmlContent.ItemID);
			_logger.LogDebug("<<< Exiting DeleteVersion(moduleId={ModuleId},version={Version})", moduleId, version);
        }

        public int RollBackVersion(int moduleId, int version)
        {
			_logger.LogDebug(">>> Entering RollBackVersion(moduleId={ModuleId},version={Version})", moduleId, version);
			_logger.LogWarning("Throwing NotImplementedException from RollBackVersion(moduleId={ModuleId},version={Version})", moduleId, version);
			_logger.LogDebug("<<< Exiting RollBackVersion(moduleId={ModuleId},version={Version})", moduleId, version);
            throw new NotImplementedException();
        }

        public void PublishVersion(int moduleId, int version)
        {
			_logger.LogDebug(">>> Entering PublishVersion(moduleId={ModuleId},version={Version})", moduleId, version);
            var htmlContent = this.GetAllHtmlText(moduleId).First(h => h.Version == version || h.Version == this.GetLatestVersion(moduleId));
            htmlContent.StateID = this.workflowManager.GetWorkflow(htmlContent.WorkflowID).LastState.StateID;
			_logger.LogDebug("PublishVersion(moduleId={ModuleId},version={Version}): htmlContent → {HtmlContent}", moduleId, version, htmlContent);
            this.UpdateHtmlText(htmlContent, this.GetMaximumVersionHistory(htmlContent.PortalID));
			_logger.LogDebug("<<< Exiting PublishVersion(moduleId={ModuleId},version={Version})", moduleId, version);
        }

        public int GetPublishedVersion(int moduleId)
        {
			_logger.LogDebug(">>> Entering GetPublishedVersion(moduleId={ModuleId})", moduleId);
            var htmlText = CBO.FillObject<HtmlTextInfo>(DataProvider.Instance().GetTopHtmlText(moduleId, true));
			_logger.LogDebug("GetPublishedVersion(moduleId={ModuleId}): htmlText → {HtmlText}", moduleId, htmlText);
			_logger.LogDebug("<<< Exiting GetPublishedVersion(moduleId={ModuleId})", moduleId);
            return htmlText.Version;
        }

        public int GetLatestVersion(int moduleId)
        {
			_logger.LogDebug(">>> Entering GetLatestVersion(moduleId={ModuleId})", moduleId);
            var htmlText = CBO.FillObject<HtmlTextInfo>(DataProvider.Instance().GetTopHtmlText(moduleId, false));
			_logger.LogDebug("GetLatestVersion(moduleId={ModuleId}): htmlText → {HtmlText}", moduleId, htmlText);
			_logger.LogDebug("<<< Exiting GetLatestVersion(moduleId={ModuleId})", moduleId);
            return htmlText.Version;
        }

        /// <inheritdoc />
        public string ExportModule(int moduleId)
        {
			_logger.LogDebug(">>> Entering ExportModule(moduleId={ModuleId})", moduleId);
            string xml = string.Empty;

            ModuleInfo module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
			_logger.LogDebug("ExportModule(moduleId={ModuleId}): module → {Module}", moduleId, module);
            int workflowID = this.GetWorkflow(moduleId, module.TabID, module.PortalID).Value;
			_logger.LogDebug("ExportModule(moduleId={ModuleId}): workflowID → {WorkflowID}", moduleId, workflowID);

            HtmlTextInfo content = this.GetTopHtmlText(moduleId, true, workflowID);
			_logger.LogDebug("ExportModule(moduleId={ModuleId}): content → {Content}", moduleId, content);
            if (content != null)
            {
                xml += "<htmltext>";
                xml += "<content>" + XmlUtils.XMLEncode(this.TokeniseLinks(content.Content, module.PortalID)) + "</content>";
                xml += "</htmltext>";
            }

			_logger.LogDebug("<<< Exiting ExportModule(moduleId={ModuleId})", moduleId);
            return xml;
        }

        /// <inheritdoc />
        public void ImportModule(int moduleID, string content, string version, int userId)
        {
			_logger.LogDebug(">>> Entering ImportModule(moduleID={ModuleID},content={Content},version={Version},userId={UserId})", moduleID, content, version, userId);
            ModuleInfo module = ModuleController.Instance.GetModule(moduleID, Null.NullInteger, true);
            int workflowID = this.GetWorkflow(moduleID, module.TabID, module.PortalID).Value;
			_logger.LogDebug("ImportModule(moduleID={ModuleID},content={Content},version={Version},userId={UserId}): workflowID → {WorkflowID}", moduleID, content, version, userId, workflowID);
            XmlNode xml = Globals.GetContent(content, "htmltext");
			_logger.LogDebug("ImportModule(moduleID={ModuleID},content={Content},version={Version},userId={UserId}): xml → {Xml}", moduleID, content, version, userId, xml);
			_logger.LogDebug("ImportModule(moduleID={ModuleID},content={Content},version={Version},userId={UserId}): module → {Module}", moduleID, content, version, userId, module);

            var htmlContent = new HtmlTextInfo();
			_logger.LogDebug("ImportModule(moduleID={ModuleID},content={Content},version={Version},userId={UserId}): htmlContent → {HtmlContent}", moduleID, content, version, userId, htmlContent);
			_logger.LogDebug("ImportModule(moduleID={ModuleID},content={Content},version={Version},userId={UserId}): htmlContent.ModuleID → {Arg4}", moduleID, content, version, userId, htmlContent.ModuleID);
            htmlContent.ModuleID = moduleID;

            // convert Version to System.Version
            var objVersion = new Version(version);
			_logger.LogDebug("ImportModule(moduleID={ModuleID},content={Content},version={Version},userId={UserId}): objVersion → {ObjVersion}", moduleID, content, version, userId, objVersion);
            if (objVersion >= new Version(5, 1, 0))
            {
                // current module content
                htmlContent.Content = this.DeTokeniseLinks(xml.SelectSingleNode("content").InnerText, module.PortalID);
            }
            else
            {
                // legacy module content
                htmlContent.Content = this.DeTokeniseLinks(xml.SelectSingleNode("desktophtml").InnerText, module.PortalID);
            }

            htmlContent.WorkflowID = workflowID;
			_logger.LogDebug("ImportModule(moduleID={ModuleID},content={Content},version={Version},userId={UserId}): htmlContent.WorkflowID → {Arg4}", moduleID, content, version, userId, htmlContent.WorkflowID);
            htmlContent.StateID = this.workflowManager.GetWorkflow(workflowID).FirstState.StateID;

            // import
            this.UpdateHtmlText(htmlContent, this.GetMaximumVersionHistory(module.PortalID));
			_logger.LogDebug("<<< Exiting ImportModule(moduleID={ModuleID},content={Content},version={Version},userId={UserId})", moduleID, content, version, userId);
        }

        /// <inheritdoc />
        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo modInfo, DateTime beginDateUtc)
        {
			_logger.LogDebug(">>> Entering GetModifiedSearchDocuments(modInfo={ModInfo},beginDateUtc={BeginDateUtc})", modInfo, beginDateUtc);
            var workflowId = this.GetWorkflow(modInfo.ModuleID, modInfo.TabID, modInfo.PortalID).Value;
            var searchDocuments = new List<SearchDocument>();
			_logger.LogDebug("GetModifiedSearchDocuments(modInfo={ModInfo},beginDateUtc={BeginDateUtc}): searchDocuments → {SearchDocuments}", modInfo, beginDateUtc, searchDocuments);
			_logger.LogDebug("GetModifiedSearchDocuments(modInfo={ModInfo},beginDateUtc={BeginDateUtc}): workflowId → {WorkflowId}", modInfo, beginDateUtc, workflowId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var htmlTextInfo = this.GetTopHtmlText(modInfo.ModuleID, true, workflowId);
            var settings = this.settingsRepository.GetSettings(modInfo);

			_logger.LogDebug("GetModifiedSearchDocuments(modInfo={ModInfo},beginDateUtc={BeginDateUtc}): htmlTextInfo → {HtmlTextInfo}", modInfo, beginDateUtc, htmlTextInfo);
            if (htmlTextInfo != null &&
                (htmlTextInfo.LastModifiedOnDate.ToUniversalTime() > beginDateUtc &&
                 htmlTextInfo.LastModifiedOnDate.ToUniversalTime() < DateTime.UtcNow))
            {
				_logger.LogDebug("GetModifiedSearchDocuments(modInfo={ModInfo},beginDateUtc={BeginDateUtc}): settings → {Settings}", modInfo, beginDateUtc, settings);
				_logger.LogInformation("GetModifiedSearchDocuments(modInfo,beginDateUtc)={ModInfo},{BeginDateUtc}: find query executed in {Arg2} ms", modInfo, beginDateUtc, __logStart.ElapsedMilliseconds);
                var strContent = HtmlUtils.Clean(htmlTextInfo.Content, false);
				_logger.LogDebug("GetModifiedSearchDocuments(modInfo={ModInfo},beginDateUtc={BeginDateUtc}): strContent → {StrContent}", modInfo, beginDateUtc, strContent);

                // Get the description string
                var description = strContent.Length <= settings.SearchDescLength ? strContent : HtmlUtils.Shorten(strContent, settings.SearchDescLength, "...");
				_logger.LogDebug("GetModifiedSearchDocuments(modInfo={ModInfo},beginDateUtc={BeginDateUtc}): description → {Description}", modInfo, beginDateUtc, description);

                var searchDoc = new SearchDocument
                {
                    UniqueKey = modInfo.ModuleID.ToString(),
                    PortalId = modInfo.PortalID,
                    Title = modInfo.ModuleTitle,
                    Description = description,
                    Body = strContent,
                    ModifiedTimeUtc = htmlTextInfo.LastModifiedOnDate.ToUniversalTime(),
                };

                if (modInfo.Terms != null && modInfo.Terms.Count > 0)
                {
                    searchDoc.Tags = CollectHierarchicalTags(modInfo.Terms);
                }

                searchDocuments.Add(searchDoc);
            }

			_logger.LogDebug("<<< Exiting GetModifiedSearchDocuments(modInfo={ModInfo},beginDateUtc={BeginDateUtc})", modInfo, beginDateUtc);
            return searchDocuments;
        }

        /// <inheritdoc />
        public string UpgradeModule(string version)
        {
			_logger.LogDebug(">>> Entering UpgradeModule(version={Version})", version);
            switch (version)
            {
                case "05.01.02":
                    // remove the Code SubDirectory
                    Config.RemoveCodeSubDirectory(this.appStatus, "HTML");

                    // Once the web.config entry is done we can safely remove the HTML folder
                    var arrPaths = new string[1];
                    arrPaths[0] = "App_Code\\HTML\\";
                    FileSystemUtils.DeleteFiles(this.appStatus, arrPaths);
                    break;
                case "06.00.00":
                    DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(this.hostSettings, "DNN_HTML", Null.NullInteger);
					_logger.LogDebug("UpgradeModule(version={Version}): desktopModule → {DesktopModule}", version, desktopModule);
					_logger.LogDebug("UpgradeModule(version={Version}): desktopModule.Category → {Arg1}", version, desktopModule.Category);
                    desktopModule.Category = "Common";
                    DesktopModuleController.SaveDesktopModule(desktopModule, false, false);
                    break;

                case "06.02.00":
                    this.AddNotificationTypes();
                    break;

                case "10.00.00":
                    MigrateHelper.MigrateHtmlWorkflows();
                    break;
            }

			_logger.LogDebug("<<< Exiting UpgradeModule(version={Version})", version);
            return "Success";
        }

        private static void AddHtmlNotification(IHostSettings hostSettings, string subject, string body, UserInfo user)
        {
			_logger.LogDebug(">>> Entering AddHtmlNotification(hostSettings={HostSettings},subject={Subject},body={Body},user={User})", hostSettings, subject, body, user);
            var notificationType = NotificationsController.Instance.GetNotificationType("HtmlNotification");
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			_logger.LogDebug("AddHtmlNotification(hostSettings={HostSettings},subject={Subject},body={Body},user={User}): portalSettings → {PortalSettings}", hostSettings, subject, body, user, portalSettings);
            var sender = UserController.GetUserById(hostSettings, portalSettings.PortalId, portalSettings.AdministratorId);
			_logger.LogDebug("AddHtmlNotification(hostSettings={HostSettings},subject={Subject},body={Body},user={User}): sender → {Sender}", hostSettings, subject, body, user, sender);
			_logger.LogDebug("AddHtmlNotification(hostSettings={HostSettings},subject={Subject},body={Body},user={User}): notificationType → {NotificationType}", hostSettings, subject, body, user, notificationType);

            var notification = new Notification { NotificationTypeID = notificationType.NotificationTypeId, Subject = subject, Body = body, IncludeDismissAction = true, SenderUserID = sender.UserID };
            NotificationsController.Instance.SendNotification(notification, portalSettings.PortalId, null, new List<UserInfo> { user });
			_logger.LogDebug("<<< Exiting AddHtmlNotification(hostSettings={HostSettings},subject={Subject},body={Body},user={User})", hostSettings, subject, body, user);
        }

        private static List<string> CollectHierarchicalTags(List<Term> terms)
        {
			_logger.LogDebug(">>> Entering CollectHierarchicalTags(terms={Terms})", terms);
            Func<List<Term>, List<string>, List<string>> collectTagsFunc = null;
            collectTagsFunc = (ts, tags) =>
            {
			_logger.LogDebug("CollectHierarchicalTags(terms={Terms}): collectTagsFunc → {CollectTagsFunc}", terms, collectTagsFunc);
                if (ts != null && ts.Count > 0)
                {
                    foreach (var t in ts)
                    {
                        tags.Add(t.Name);
                        tags.AddRange(collectTagsFunc(t.ChildTerms, new List<string>()));
                    }
                }

			_logger.LogDebug("<<< Exiting CollectHierarchicalTags(terms={Terms})", terms);
                return tags;
            };

            return collectTagsFunc(terms, new List<string>());
        }

        private static void TrackModuleModification(int moduleId, int moduleVersion)
        {
			_logger.LogDebug(">>> Entering TrackModuleModification(moduleId={ModuleId},moduleVersion={ModuleVersion})", moduleId, moduleVersion);
            var moduleInfo = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
            TabChangeTracker.Instance.TrackModuleModification(moduleInfo, moduleVersion, UserController.Instance.GetCurrentUserInfo().UserID);
			_logger.LogDebug("TrackModuleModification(moduleId={ModuleId},moduleVersion={ModuleVersion}): moduleInfo → {ModuleInfo}", moduleId, moduleVersion, moduleInfo);
			_logger.LogDebug("<<< Exiting TrackModuleModification(moduleId={ModuleId},moduleVersion={ModuleVersion})", moduleId, moduleVersion);
        }

        private void ClearModuleSettings(ModuleInfo objModule)
        {
			_logger.LogDebug(">>> Entering ClearModuleSettings(objModule={ObjModule})", objModule);
            if (objModule.ModuleDefinition.FriendlyName == "Text/HTML")
            {
                ModuleController.Instance.DeleteModuleSetting(objModule.ModuleID, "WorkFlowID");
            }
			_logger.LogDebug("<<< Exiting ClearModuleSettings(objModule={ObjModule})", objModule);
        }

        /// <summary>CreateUserNotifications creates HtmlTextUser records and optionally sends email notifications to participants in a Workflow.</summary>
        /// <param name="objHtmlText">An HtmlTextInfo object.</param>
        private void CreateUserNotifications(HtmlTextInfo objHtmlText)
        {
			_logger.LogDebug(">>> Entering CreateUserNotifications(objHtmlText={ObjHtmlText})", objHtmlText);
            var htmlTextUserController = new HtmlTextUserController();

			_logger.LogDebug("CreateUserNotifications(objHtmlText={ObjHtmlText}): htmlTextUserController → {HtmlTextUserController}", objHtmlText, htmlTextUserController);
            // clean up old user notification records
            htmlTextUserController.DeleteHtmlTextUsers();

            // ensure we have latest htmltext object loaded
            objHtmlText = this.GetHtmlText(objHtmlText.ModuleID, objHtmlText.ItemID);
			_logger.LogDebug("CreateUserNotifications(objHtmlText={ObjHtmlText}): objHtmlText → {ObjHtmlText1}", objHtmlText, objHtmlText);

            // build collection of users to notify
            var arrUsers = new ArrayList();
			_logger.LogDebug("CreateUserNotifications(objHtmlText={ObjHtmlText}): arrUsers → {ArrUsers}", objHtmlText, arrUsers);

            // if not published
            if (!objHtmlText.IsPublished)
            {
                arrUsers.Add(objHtmlText.CreatedByUserID); // include content owner
            }

            // if not draft and not published
            if (objHtmlText.StateID != this.workflowManager.GetWorkflow(objHtmlText.WorkflowID).FirstState.StateID && objHtmlText.IsPublished == false)
            {
                // get users from permissions for state
                foreach (IPermissionInfo permission in WorkflowStatePermissionsRepository.Instance.GetWorkflowStatePermissionByState(objHtmlText.StateID))
                {
                    if (permission.AllowAccess)
                    {
                        if (Null.IsNull(permission.UserId))
                        {
                            int roleId = permission.RoleId;
                            RoleInfo objRole = RoleController.Instance.GetRole(objHtmlText.PortalID, r => r.RoleID == roleId);
							_logger.LogDebug("CreateUserNotifications(objHtmlText={ObjHtmlText}): objRole → {ObjRole}", objHtmlText, objRole);
                            if (objRole != null)
                            {
                                foreach (UserRoleInfo objUserRole in RoleController.Instance.GetUserRoles(objHtmlText.PortalID, null, objRole.RoleName))
                                {
                                    if (!arrUsers.Contains(objUserRole.UserID))
                                    {
                                        arrUsers.Add(objUserRole.UserID);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!arrUsers.Contains(permission.UserId))
                            {
                                arrUsers.Add(permission.UserId);
                            }
                        }
                    }
                }
            }

            // process notifications
            if (arrUsers.Count > 0 || (objHtmlText.IsPublished && objHtmlText.Notify))
            {
                // get tabid from module
                ModuleInfo objModule = ModuleController.Instance.GetModule(objHtmlText.ModuleID, Null.NullInteger, true);
				_logger.LogDebug("CreateUserNotifications(objHtmlText={ObjHtmlText}): objModule → {ObjModule}", objHtmlText, objModule);

                var objPortalSettings = PortalController.Instance.GetCurrentSettings();
				_logger.LogDebug("CreateUserNotifications(objHtmlText={ObjHtmlText}): objPortalSettings → {ObjPortalSettings}", objHtmlText, objPortalSettings);
                if (objPortalSettings != null)
                {
                    string strResourceFile =
                        $"{Globals.ApplicationPath}/DesktopModules/{objModule.DesktopModule.FolderName}/{Localization.LocalResourceDirectory}/{Localization.LocalSharedResourceFile}";
                    string strSubject = Localization.GetString("NotificationSubject", strResourceFile);
                    string strBody = Localization.GetString("NotificationBody", strResourceFile);
					_logger.LogDebug("CreateUserNotifications(objHtmlText={ObjHtmlText}): strBody → {StrBody}", objHtmlText, strBody);
                    strBody = strBody.Replace("[URL]", this.NavigationManager.NavigateURL(objModule.TabID));
                    strBody = strBody.Replace("[STATE]", objHtmlText.StateName);

                    // process user notification collection
                    UserInfo user;
                    foreach (int intUserID in arrUsers)
                    {
                        // create user notification record
                        var htmlTextUser = new HtmlTextUserInfo
                        {
                            ItemID = objHtmlText.ItemID,
                            StateID = objHtmlText.StateID,
                            ModuleID = objHtmlText.ModuleID,
                            TabID = objModule.TabID,
                            UserID = intUserID,
                        };
                        htmlTextUserController.AddHtmlTextUser(htmlTextUser);

                        // send an email notification to a user if the state indicates to do so
                        if (objHtmlText.Notify)
                        {
                            user = UserController.GetUserById(this.hostSettings, objHtmlText.PortalID, intUserID);
                            if (user != null)
							_logger.LogDebug("CreateUserNotifications(objHtmlText={ObjHtmlText}): user → {User}", objHtmlText, user);
                            {
                                AddHtmlNotification(this.hostSettings, strSubject, strBody, user);
                            }
                        }
                    }

                    // if published and the published state specifies to notify members of the workflow
                    if (objHtmlText.IsPublished && objHtmlText.Notify)
                    {
                        // send email notification to the author
                        user = UserController.GetUserById(this.hostSettings, objHtmlText.PortalID, objHtmlText.CreatedByUserID);
                        if (user != null)
                        {
                            try
                            {
                                Services.Mail.Mail.SendEmail(objPortalSettings.Email, objPortalSettings.Email, strSubject, strBody);
                            }
                            catch (Exception exc)
                            {
								_logger.LogError("Exception in CreateUserNotifications(objHtmlText={ObjHtmlText}): {Arg1}", objHtmlText, exc.getMessage());
                                Exceptions.LogException(exc);
                            }
                        }
                    }
                }
            }
			_logger.LogDebug("<<< Exiting CreateUserNotifications(objHtmlText={ObjHtmlText})", objHtmlText);
        }

        private string DeTokeniseLinks(string content, int portalId)
        {
			_logger.LogDebug(">>> Entering DeTokeniseLinks(content={Content},portalId={PortalId})", content, portalId);
            var portal = PortalController.Instance.GetPortal(portalId);
            var portalRoot = UrlUtils.Combine(Globals.ApplicationPath, portal.HomeDirectory);
			_logger.LogDebug("DeTokeniseLinks(content={Content},portalId={PortalId}): portalRoot → {PortalRoot}", content, portalId, portalRoot);
			_logger.LogDebug("DeTokeniseLinks(content={Content},portalId={PortalId}): portal → {Portal}", content, portalId, portal);
            if (!portalRoot.StartsWith("/", StringComparison.Ordinal))
            {
                portalRoot = "/" + portalRoot;
            }

            if (!portalRoot.EndsWith("/", StringComparison.Ordinal))
            {
                portalRoot = portalRoot + "/";
            }

            content = Regex.Replace(content, PortalRootToken + "\\/{0,1}", portalRoot, RegexOptions.IgnoreCase);
			_logger.LogDebug("DeTokeniseLinks(content={Content},portalId={PortalId}): content → {Content1}", content, portalId, content);

			_logger.LogDebug(">>> Entering TokeniseLinks(content={Content},portalId={PortalId})", content, portalId);
			_logger.LogDebug("<<< Exiting TokeniseLinks(content={Content},portalId={PortalId})", content, portalId);
			_logger.LogDebug("<<< Exiting DeTokeniseLinks(content={Content},portalId={PortalId})", content, portalId);
            return content;
        }

        private string TokeniseLinks(string content, int portalId)
        {
            // Replace any relative portal root reference by a token "{{PortalRoot}}"
            var portal = PortalController.Instance.GetPortal(portalId);
            var portalRoot = UrlUtils.Combine(Globals.ApplicationPath, portal.HomeDirectory);
            if (!portalRoot.StartsWith("/", StringComparison.Ordinal))
            {
                portalRoot = "/" + portalRoot;
            }

            if (!portalRoot.EndsWith("/", StringComparison.Ordinal))
            {
                portalRoot = portalRoot + "/";
            }

            // Portal Root regular expression
            var regex = @"(?<url>
                        (?<host>
                        (?<protocol>[A-Za-z]{3,9}:(?:\/\/)?)
                        (?<domain>(?:[\-;:&=\+\$,\w]+@)?[A-Za-z0-9\.\-]+|(?:www\.|[\-;:&=\+\$,\w]+@)[A-Za-z0-9\.\-]+))?
                        (?<portalRoot>" + portalRoot + "))";

            var matchEvaluator = new MatchEvaluator(this.ReplaceWithRootToken);
            var exp = RegexUtils.GetCachedRegex(regex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return exp.Replace(content, matchEvaluator);
        }

        private void AddNotificationTypes()
        {
			_logger.LogDebug(">>> Entering AddNotificationTypes()");
            var type = new NotificationType { Name = "HtmlNotification", Description = "Html Module Notification" };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
			_logger.LogDebug("AddNotificationTypes(): type → {Type}", type);
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
			_logger.LogDebug("<<< Exiting AddNotificationTypes()");
        }
    }
}
