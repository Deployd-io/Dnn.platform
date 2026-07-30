// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Journal.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.Services.Search.Entities;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The Controller class for Journal.</summary>
    /// <param name="navigationManager">The navigation manager.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class FeatureController(INavigationManager navigationManager, IHostSettings hostSettings)
        : ModuleSearchBase, IModuleSearchResultController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug(">>> Entering FeatureController(navigationManager={NavigationManager},hostSettings={HostSettings})", navigationManager, hostSettings);
		Logger.LogDebug("FeatureController(navigationManager={NavigationManager},hostSettings={HostSettings}): hostSettings → {HostSettings1}", navigationManager, hostSettings, hostSettings);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();

        /// <summary>Initializes a new instance of the <see cref="FeatureController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public FeatureController()
            : this(null)
        {
		Logger.LogInformation("FeatureController(navigationManager,hostSettings)={NavigationManager},{HostSettings}: database query executed in {Arg2} ms", navigationManager, hostSettings, __logStart.ElapsedMilliseconds);
		Logger.LogError("Exception in FeatureController(navigationManager={NavigationManager},hostSettings={HostSettings}): {Arg2}", navigationManager, hostSettings, ex.Message);
		Logger.LogDebug("<<< Exiting FeatureController(navigationManager={NavigationManager},hostSettings={HostSettings})", navigationManager, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="FeatureController"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public FeatureController(INavigationManager navigationManager)
            : this(navigationManager, null)
        {
        }

        /// <summary>Gets the navigation manager.</summary>
        protected INavigationManager NavigationManager { get; } = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        /// <inheritdoc cref="IPortable.ExportModule" />
        public string ExportModule(int moduleID)
        {
			Logger.LogDebug(">>> Entering ExportModule(moduleID={ModuleID})", moduleID);
            ////string strXML = "";

            ////List<JournalInfo> colJournals = GetJournals(ModuleID);
            ////if (colJournals.Count != 0)
            ////{
            ////   strXML += "<Journals>";

            ////foreach (JournalInfo objJournal in colJournals)
            ////   {
            ////       strXML += "<Journal>";
            ////       strXML += "<content>" + DotNetNuke.Common.Utilities.XmlUtils.XMLEncode(objJournal.Content) + "</content>";
            ////       strXML += "</Journal>";
            ////   }
            ////   strXML += "</Journals>";
            ////}

            ////return strXML;
			Logger.LogDebug("<<< Exiting ExportModule(moduleID={ModuleID})", moduleID);
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <inheritdoc cref="IPortable.ImportModule" />
        public void ImportModule(int moduleID, string content, string version, int userId)
        {
            ////XmlNode xmlJournals = DotNetNuke.Common.Globals.GetContent(Content, "Journals");
            ////foreach (XmlNode xmlJournal in xmlJournals.SelectNodes("Journal"))
            ////{
            ////   JournalInfo objJournal = new JournalInfo();
            ////   objJournal.ModuleId = ModuleID;
            ////   objJournal.Content = xmlJournal.SelectSingleNode("content").InnerText;
            ////   objJournal.CreatedByUser = UserID;
            ////   AddJournal(objJournal);
            ////}
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <inheritdoc cref="IUpgradeable.UpgradeModule" />
        public string UpgradeModule(string version)
        {
			Logger.LogDebug(">>> Entering UpgradeModule(version={Version})", version);
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <inheritdoc />
        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
        {
			Logger.LogDebug(">>> Entering GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc})", moduleInfo, beginDateUtc);
            var searchDocuments = new Dictionary<string, SearchDocument>();
            var lastJournalId = Null.NullInteger;
			Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): searchDocuments → {SearchDocuments}", moduleInfo, beginDateUtc, searchDocuments);
			Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): lastJournalId → {LastJournalId}", moduleInfo, beginDateUtc, lastJournalId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                while (true)
                {
                    using (var reader = DataProvider.Instance().ExecuteReader("Journal_GetSearchItems", moduleInfo.PortalID, moduleInfo.TabModuleID, beginDateUtc, lastJournalId, Constants.SearchBatchSize))
                    {
					Logger.LogInformation("FeatureController(navigationManager,hostSettings)={NavigationManager},{HostSettings}: database query executed in {Arg2} ms", navigationManager, hostSettings, __logStart.ElapsedMilliseconds);
                        var journalIds = new Dictionary<int, int>();

						Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): journalIds → {JournalIds}", moduleInfo, beginDateUtc, journalIds);
						Logger.LogInformation("GetModifiedSearchDocuments(moduleInfo,beginDateUtc)={ModuleInfo},{BeginDateUtc}: database query executed in {Arg2} ms", moduleInfo, beginDateUtc, __logStart.ElapsedMilliseconds);
                        while (reader.Read())
                        {
                            var journalId = Convert.ToInt32(reader["JournalId"]);

							Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): journalId → {JournalId}", moduleInfo, beginDateUtc, journalId);
                            ////var journalTypeId = reader["JournalTypeId"].ToString();
                            var userId = Convert.ToInt32(reader["UserId"]);
                            var dateUpdated = Convert.ToDateTime(reader["DateUpdated"]);
							Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): userId → {UserId}", moduleInfo, beginDateUtc, userId);
							Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): dateUpdated → {DateUpdated}", moduleInfo, beginDateUtc, dateUpdated);
                            var profileId = reader["ProfileId"].ToString();
                            var groupId = reader["GroupId"].ToString();
							Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): profileId → {ProfileId}", moduleInfo, beginDateUtc, profileId);
							Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): groupId → {GroupId}", moduleInfo, beginDateUtc, groupId);
                            var title = reader["Title"].ToString();
                            var summary = reader["Summary"].ToString();
							Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): title → {Title}", moduleInfo, beginDateUtc, title);
							Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): summary → {Summary}", moduleInfo, beginDateUtc, summary);
                            var securityKey = reader["SecurityKey"].ToString();
                            var tabId = reader["TabId"].ToString();
							Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): securityKey → {SecurityKey}", moduleInfo, beginDateUtc, securityKey);
							Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): tabId → {TabId}", moduleInfo, beginDateUtc, tabId);
                            var tabModuleId = reader["ModuleId"].ToString();

							Logger.LogDebug("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): tabModuleId → {TabModuleId}", moduleInfo, beginDateUtc, tabModuleId);
                            var key = string.Format("JI_{0}", journalId);
                            if (searchDocuments.ContainsKey(key))
							Logger.LogWarning("GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): key → {Key}", moduleInfo, beginDateUtc, key);
                            {
                                searchDocuments[key].UniqueKey +=
                                    string.Format(",{0}", securityKey);
                            }
                            else
                            {
                                var searchDocument = new SearchDocument
                                {
                                    UniqueKey = string.Format("JI_{0}_{1}", journalId, securityKey),
                                    PortalId = moduleInfo.PortalID,
                                    Body = summary,
                                    ModifiedTimeUtc = dateUpdated,
                                    Title = title,
                                    AuthorUserId = userId,
                                    Keywords = new Dictionary<string, string>
                                    {
                                        { "TabId", tabId },
                                        { "TabModuleId", tabModuleId },
                                        { "ProfileId", profileId },
                                        { "GroupId", groupId },
                                    },
                                };

                                searchDocuments.Add(key, searchDocument);
                            }

                            if (journalId > lastJournalId)
                            {
                                lastJournalId = journalId;
                            }

                            if (!journalIds.ContainsKey(journalId))
                            {
                                journalIds.Add(journalId, userId);
                            }
                        }

                        if (journalIds.Count == 0)
                        {
                            break;
                        }

                        // index comments for this journal
                        this.AddCommentItems(journalIds, searchDocuments);
                    }
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in FeatureController(navigationManager={NavigationManager},hostSettings={HostSettings}): {Arg2}", navigationManager, hostSettings, ex.Message);
                Exceptions.LogException(ex);
				Logger.LogError("Exception in GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc}): {Arg2}", moduleInfo, beginDateUtc, ex.Message);
            }

			Logger.LogDebug("<<< Exiting GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc})", moduleInfo, beginDateUtc);
            return searchDocuments.Values.ToList();
        }

        /// <inheritdoc />
        public bool HasViewPermission(SearchResult searchResult)
        {
			Logger.LogDebug(">>> Entering HasViewPermission(searchResult={SearchResult})", searchResult);
            if (!searchResult.UniqueKey.StartsWith("JI_", StringComparison.InvariantCultureIgnoreCase))
            {
                if (HttpContext.Current == null || PortalSettings.Current == null)
                {
                    return false;
                }

                var portalSettings = PortalSettings.Current;
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                var isAdmin = currentUser.IsInRole(RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId).RoleName);
				Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): currentUser → {CurrentUser}", searchResult, currentUser);
				Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): isAdmin → {IsAdmin}", searchResult, isAdmin);
                if (!HttpContext.Current.Request.IsAuthenticated || (!currentUser.IsSuperUser && !isAdmin && currentUser.IsInRole("Unverified Users")))
                {
                    return false;
                }

                return true;
            }

            var securityKeys = searchResult.UniqueKey.Split('_')[2].Split(',');
            var userInfo = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): securityKeys → {SecurityKeys}", searchResult, securityKeys);
			Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): userInfo → {UserInfo}", searchResult, userInfo);

            var selfKey = string.Format("U{0}", userInfo.UserID);

			Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): selfKey → {SelfKey}", searchResult, selfKey);
            if (securityKeys.Contains("E") || securityKeys.Contains(selfKey))
            {
                return true;
            }

            // do not show items in private group
            if (securityKeys.Any(s => s.StartsWith("R")))
            {
                var groupId = Convert.ToInt32(securityKeys.First(s => s.StartsWith("R")).Substring(1));
                var role = RoleController.Instance.GetRoleById(searchResult.PortalId, groupId);
				Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): groupId → {GroupId}", searchResult, groupId);
				Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): role → {Role}", searchResult, role);
                if (role != null && !role.IsPublic && !userInfo.IsInRole(role.RoleName))
                {
                    return false;
                }
            }

            if (securityKeys.Contains("C"))
            {
                return userInfo.UserID > 0;
            }

            if (securityKeys.Any(s => s.StartsWith("F")))
            {
                var targetUser = UserController.GetUserById(this.hostSettings, searchResult.PortalId, searchResult.AuthorUserId);

				Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): targetUser → {TargetUser}", searchResult, targetUser);
                return targetUser?.Social.Friend is { Status: RelationshipStatus.Accepted, };
            }

			Logger.LogDebug("<<< Exiting HasViewPermission(searchResult={SearchResult})", searchResult);
            return false;
        }

        /// <inheritdoc />
        public string GetDocUrl(SearchResult searchResult)
        {
			Logger.LogDebug(">>> Entering GetDocUrl(searchResult={SearchResult})", searchResult);
            if (!searchResult.UniqueKey.StartsWith("JI_", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }

            string url;
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var journalId = Convert.ToInt32(searchResult.UniqueKey.Split('_')[1]);
			Logger.LogDebug("GetDocUrl(searchResult={SearchResult}): portalSettings → {PortalSettings}", searchResult, portalSettings);
			Logger.LogDebug("GetDocUrl(searchResult={SearchResult}): journalId → {JournalId}", searchResult, journalId);
            var groupId = Convert.ToInt32(searchResult.Keywords["GroupId"]);
            var tabId = Convert.ToInt32(searchResult.Keywords["TabId"]);
			Logger.LogDebug("GetDocUrl(searchResult={SearchResult}): groupId → {GroupId}", searchResult, groupId);
			Logger.LogDebug("GetDocUrl(searchResult={SearchResult}): tabId → {TabId}", searchResult, tabId);

            ////var tabModuleId = Convert.ToInt32(searchResult.Keywords["TabModuleId"]);
            var profileId = Convert.ToInt32(searchResult.Keywords["ProfileId"]);

			Logger.LogDebug("GetDocUrl(searchResult={SearchResult}): profileId → {ProfileId}", searchResult, profileId);
            if (groupId > 0 && tabId > 0)
            {
                url = this.NavigationManager.NavigateURL(tabId, string.Empty, "GroupId=" + groupId, "jid=" + journalId);
            }
            else if (tabId == portalSettings.UserTabId)
            {
                url = this.NavigationManager.NavigateURL(portalSettings.UserTabId, string.Empty, string.Format("userId={0}", profileId), "jid=" + journalId);
            }
            else
            {
                url = this.NavigationManager.NavigateURL(tabId, string.Empty, "jid=" + journalId);
            }

			Logger.LogDebug("<<< Exiting GetDocUrl(searchResult={SearchResult})", searchResult);
            return url;
        }

        private void AddCommentItems(Dictionary<int, int> journalIds, IDictionary<string, SearchDocument> searchDocuments)
        {
            var comments = JournalController.Instance.GetCommentsByJournalIds(journalIds.Keys.ToList());
            foreach (var commentInfo in comments)
            {
                var journalResult = searchDocuments[string.Format("JI_{0}", commentInfo.JournalId)];
                if (journalResult != null)
                {
                    journalResult.Body += string.Format(" {0}", commentInfo.Comment);
                    if (commentInfo.DateCreated > journalResult.ModifiedTimeUtc)
                    {
                        journalResult.ModifiedTimeUtc = commentInfo.DateUpdated;
                    }
                }
            }
        }
    }
}
