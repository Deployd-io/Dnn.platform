// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Controllers;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Profile;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Localization;
    using NUnit.Framework;

    public class FriendsTests : IntegrationTestBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int PortalId = 0;
        private const string FirstLanguage = "en-US";
        private const string SecondLanguage = "fr-FR";

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            Globals.SetStatus(Globals.UpgradeStatus.None);
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new SqlDataProvider());
            ComponentFactory.RegisterComponentSettings<SqlDataProvider>(new Dictionary<string, string>()
            {
                { "name", "SqlDataProvider" },
                { "type", "DotNetNuke.Data.SqlDataProvider, DotNetNuke" },
                { "connectionStringName", "SiteSqlServer" },
                { "objectQualifier", ConfigurationManager.AppSettings["objectQualifier"] },
                { "databaseOwner", "dbo." },
            });
            ComponentFactory.InstallComponents(new ProviderInstaller("caching", typeof(CachingProvider), typeof(FBCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("members", typeof(MembershipProvider), typeof(AspNetMembershipProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("profiles", typeof(ProfileProvider), typeof(DNNProfileProvider)));
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [Test]
        public async Task Friend_Request_Should_Match_Target_User_Culture()
        {
			Logger.LogDebug(">>> Entering Friend_Request_Should_Match_Target_User_Culture()");
            this.PrepareSecondLanguage();
            var (_, userId1, userName1, _) = await this.CreateNewUser();
            var (_, userId2, _, _) = await this.CreateNewUser();

            this.UpdateUserProfile(userId1, UserProfile.USERPROFILE_PreferredLocale, FirstLanguage);
            this.UpdateUserProfile(userId2, UserProfile.USERPROFILE_PreferredLocale, SecondLanguage);
            WebApiTestHelper.ClearHostCache();

            var connector = WebApiTestHelper.LoginUser(userName1);
            connector.PostJson(
                "API/MemberDirectory/MemberDirectory/AddFriend",
                new { friendId = userId2, },
                this.GetRequestHeaders());
				Logger.LogDebug("Friend_Request_Should_Match_Target_User_Culture(): connector → {Connector}", connector);
            var notificationTitle = this.GetNotificationTitle(userId1);

			Logger.LogDebug("Friend_Request_Should_Match_Target_User_Culture(): notificationTitle → {NotificationTitle}", notificationTitle);
            // the notification should use french language: testuser8836 veut être amis avec vous
            Assert.That(notificationTitle, Is.EqualTo($"{userName1} veut être amis"));
			Logger.LogDebug("<<< Exiting Friend_Request_Should_Match_Target_User_Culture()");
        }

        private void UpdateUserProfile(int userId, string propertyName, string propertyValue)
        {
			Logger.LogDebug(">>> Entering UpdateUserProfile(userId={UserId},propertyName={PropertyName},propertyValue={PropertyValue})", userId, propertyName, propertyValue);
            var user = DotNetNuke.Entities.Users.UserController.Instance.GetUserById(PortalId, userId);
            if (user != null)
			Logger.LogDebug("UpdateUserProfile(userId={UserId},propertyName={PropertyName},propertyValue={PropertyValue}): user → {User}", userId, propertyName, propertyValue, user);
            {
                var userProfile = user.Profile;
                userProfile.SetProfileProperty(propertyName, propertyValue);
                ProfileController.UpdateUserProfile(user);
            }
			Logger.LogDebug("<<< Exiting UpdateUserProfile(userId={UserId},propertyName={PropertyName},propertyValue={PropertyValue})", userId, propertyName, propertyValue);
        }

        private void PrepareSecondLanguage()
        {
			Logger.LogDebug(">>> Entering PrepareSecondLanguage()");
            if (!this.LanguageEnabled(PortalId, SecondLanguage))
            {
            }
			Logger.LogDebug("<<< Exiting PrepareSecondLanguage()");
        }

        private bool LanguageEnabled(int portalId, string secondLanguage)
        {
			Logger.LogDebug(">>> Entering LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage})", portalId, secondLanguage);
            var portalLanguages = CBO.FillDictionary<string, Locale>("CultureCode", DataProvider.Instance().GetLanguagesByPortal(portalId));
            if (!portalLanguages.ContainsKey(secondLanguage))
			Logger.LogDebug("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): portalLanguages → {PortalLanguages}", portalId, secondLanguage, portalLanguages);
            {
                var connector = WebApiTestHelper.LoginHost();
                connector.PostJson($"API/PersonaBar/Extensions/ParseLanguagePackage?cultureCode={secondLanguage}", new { });
				Logger.LogDebug("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): connector → {Connector}", portalId, secondLanguage, connector);
                connector.PostJson($"API/PersonaBar/Extensions/InstallAvailablePackage", new
                {
                    PackageType = "CoreLanguagePack",
                    FileName = "installlanguage.resources",
                });
					Logger.LogDebug("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): PackageType → {PackageType}", portalId, secondLanguage, PackageType);
					Logger.LogDebug("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): FileName → {FileName}", portalId, secondLanguage, FileName);

                var language = CBO.FillDictionary<string, Locale>("CultureCode", DataProvider.Instance().GetLanguages())[secondLanguage];
                connector.PostJson("API/PersonaBar/SiteSettings/UpdateLanguage", new
                {
				Logger.LogDebug("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): language → {Language}", portalId, secondLanguage, language);
                    PortalId = PortalId,
                    LanguageId = language.LanguageId,
                    Code = language.Code,
                    Enabled = true,
                    IsDefault = false,
                    Roles = "Administrators",
                });
					Logger.LogDebug("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): PortalId → {PortalId1}", portalId, secondLanguage, PortalId);
					Logger.LogDebug("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): LanguageId → {LanguageId}", portalId, secondLanguage, LanguageId);
					Logger.LogDebug("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): Code → {Code}", portalId, secondLanguage, Code);
					Logger.LogInformation("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): Enabled → {Enabled}", portalId, secondLanguage, Enabled);
					Logger.LogDebug("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): IsDefault → {IsDefault}", portalId, secondLanguage, IsDefault);
					Logger.LogDebug("LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage}): Roles → {Roles}", portalId, secondLanguage, Roles);
            }

			Logger.LogDebug("<<< Exiting LanguageEnabled(portalId={PortalId},secondLanguage={SecondLanguage})", portalId, secondLanguage);
            return false;
        }

        private async Task<(IWebApiConnector Connector, int UserId, string Username, int FileId)> CreateNewUser()
        {
            return await WebApiTestHelper.PrepareNewUser(PortalId);
        }

        private string GetNotificationTitle(int userId)
        {
			Logger.LogDebug(">>> Entering GetNotificationTitle(userId={UserId})", userId);
			Logger.LogDebug("<<< Exiting GetNotificationTitle(userId={UserId})", userId);
            return DatabaseHelper.ExecuteScalar<string>(
                $"select TOP 1 Subject from {{objectQualifier}}[CoreMessaging_Messages] WHERE SenderUserID = {userId}");
        }

        private IDictionary<string, string> GetRequestHeaders()
        {
            return WebApiTestHelper.GetRequestHeaders("//ActivityFeed", "Member Directory", PortalId);
        }
    }
}
