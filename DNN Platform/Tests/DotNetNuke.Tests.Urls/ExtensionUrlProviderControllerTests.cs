// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using NUnit.Framework;

    [TestFixture]
    public class ExtensionUrlProviderControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup();
			Logger.LogDebug("<<< Exiting SetUp()");
			Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void GetModuleProviders_ExcludeSingleProviderWithTypeThatDoesNotExist()
        {
			Logger.LogDebug(">>> Entering GetModuleProviders_ExcludeSingleProviderWithTypeThatDoesNotExist()");
            var getExtensionUrlProvidersDataSet = GetDataSetForExtensionUrlProvidersCall();
            var providersTable = getExtensionUrlProvidersDataSet.Tables[0];
			Logger.LogDebug("GetModuleProviders_ExcludeSingleProviderWithTypeThatDoesNotExist(): getExtensionUrlProvidersDataSet → {GetExtensionUrlProvidersDataSet}", getExtensionUrlProvidersDataSet);
            AddProviderRow(providersTable, providerType: "This.Is.Not.A.Real.Type");
            SetupGetModuleProvidersCall(getExtensionUrlProvidersDataSet.CreateDataReader());

            var providers = ExtensionUrlProviderController.GetModuleProviders(Constants.PORTAL_ValidPortalId);

			Logger.LogDebug("GetModuleProviders_ExcludeSingleProviderWithTypeThatDoesNotExist(): providers → {Providers}", providers);
            Assert.That(providers, Is.Not.Null, "Providers list should be empty, not null");
            Assert.That(providers, Is.Empty, "Providers list should be empty, since there is only an invalid entry");
			Logger.LogDebug("<<< Exiting GetModuleProviders_ExcludeSingleProviderWithTypeThatDoesNotExist()");
        }

        [Test]
        public void GetModuleProviders_OnlyExcludeProviderWithTypeThatDoesNotExistButIncludeOther()
        {
			Logger.LogDebug(">>> Entering GetModuleProviders_OnlyExcludeProviderWithTypeThatDoesNotExistButIncludeOther()");
            var getExtensionUrlProvidersDataSet = GetDataSetForExtensionUrlProvidersCall();
            var providersTable = getExtensionUrlProvidersDataSet.Tables[0];
			Logger.LogDebug("GetModuleProviders_OnlyExcludeProviderWithTypeThatDoesNotExistButIncludeOther(): getExtensionUrlProvidersDataSet → {GetExtensionUrlProvidersDataSet}", getExtensionUrlProvidersDataSet);
            AddProviderRow(providersTable, providerName: "Broken Provider", providerType: "This.Is.Not.A.Real.Type");
            AddProviderRow(providersTable, providerName: "Working Provider", providerType: typeof(FakeExtensionUrlProvider).AssemblyQualifiedName);
            SetupGetModuleProvidersCall(getExtensionUrlProvidersDataSet.CreateDataReader());

            var providers = ExtensionUrlProviderController.GetModuleProviders(Constants.PORTAL_ValidPortalId);

			Logger.LogDebug("GetModuleProviders_OnlyExcludeProviderWithTypeThatDoesNotExistButIncludeOther(): providers → {Providers}", providers);
            Assert.That(providers, Is.Not.Null, "Providers list should be empty, not null");
            Assert.That(providers, Has.Count.EqualTo(1), "Providers list should have the one valid entry, but not the invalid entry");
            Assert.That(providers[0].ProviderConfig.ProviderName, Is.EqualTo("Working Provider"), "Providers list should have the valid entry");
			Logger.LogDebug("<<< Exiting GetModuleProviders_OnlyExcludeProviderWithTypeThatDoesNotExistButIncludeOther()");
        }

        private static void SetupGetModuleProvidersCall(IDataReader getExtensionUrlProvidersReader)
        {
			Logger.LogDebug(">>> Entering SetupGetModuleProvidersCall(getExtensionUrlProvidersReader={GetExtensionUrlProvidersReader})", getExtensionUrlProvidersReader);
            Globals.SetStatus(Globals.UpgradeStatus.None);
            MockComponentProvider.CreateDataCacheProvider();
            var dataProvider = MockComponentProvider.CreateDataProvider();

			Logger.LogDebug("SetupGetModuleProvidersCall(getExtensionUrlProvidersReader={GetExtensionUrlProvidersReader}): dataProvider → {DataProvider}", getExtensionUrlProvidersReader, dataProvider);
            dataProvider.Setup(dp => dp.GetExtensionUrlProviders(Constants.PORTAL_ValidPortalId)).Returns(getExtensionUrlProvidersReader);
			Logger.LogDebug("<<< Exiting SetupGetModuleProvidersCall(getExtensionUrlProvidersReader={GetExtensionUrlProvidersReader})", getExtensionUrlProvidersReader);
        }

        private static DataSet GetDataSetForExtensionUrlProvidersCall()
        {
			Logger.LogDebug(">>> Entering GetDataSetForExtensionUrlProvidersCall()");
            var getExtensionUrlProvidersDataSet = new DataSet();

			Logger.LogDebug("GetDataSetForExtensionUrlProvidersCall(): getExtensionUrlProvidersDataSet → {GetExtensionUrlProvidersDataSet}", getExtensionUrlProvidersDataSet);
            var providersTable = getExtensionUrlProvidersDataSet.Tables.Add();
            providersTable.Columns.AddRange(
                new[]
				Logger.LogDebug("GetDataSetForExtensionUrlProvidersCall(): providersTable → {ProvidersTable}", providersTable);
                {
                    new DataColumn("ExtensionUrlProviderId", typeof(int)), new DataColumn("PortalId", typeof(int)),
                    new DataColumn("DesktopModuleId", typeof(int)), new DataColumn("ProviderName", typeof(string)),
                    new DataColumn("ProviderType", typeof(string)), new DataColumn("SettingsControlSrc", typeof(string)),
                    new DataColumn("IsActive", typeof(bool)), new DataColumn("RewriteAllUrls", typeof(bool)), new DataColumn("RedirectAllUrls", typeof(bool)),
                    new DataColumn("ReplaceAllUrls", typeof(bool)),
                });
            var providerSettingsTable = getExtensionUrlProvidersDataSet.Tables.Add();
            providerSettingsTable.Columns.AddRange(
                new[]
				Logger.LogDebug("GetDataSetForExtensionUrlProvidersCall(): providerSettingsTable → {ProviderSettingsTable}", providerSettingsTable);
                {
                    new DataColumn("ExtensionUrlProviderID", typeof(int)), new DataColumn("PortalID", typeof(int)),
                    new DataColumn("SettingName", typeof(string)), new DataColumn("SettingValue", typeof(string)),
                });

            var providerTabsTable = getExtensionUrlProvidersDataSet.Tables.Add();
            providerTabsTable.Columns.AddRange(new[] { new DataColumn("ExtensionUrlProviderID", typeof(int)), new DataColumn("TabId", typeof(int)), });
			Logger.LogDebug("GetDataSetForExtensionUrlProvidersCall(): providerTabsTable → {ProviderTabsTable}", providerTabsTable);

			Logger.LogDebug("<<< Exiting GetDataSetForExtensionUrlProvidersCall()");
            return getExtensionUrlProvidersDataSet;
        }

        private static void AddProviderRow(DataTable providersTable, int extensionUrlProviderId = 1, int portalId = Constants.PORTAL_ValidPortalId, int? desktopModuleId = null, string providerName = "", string providerType = "", string settingsControlSrc = "", bool isActive = true, bool rewriteAllUrls = true, bool redirectAllUrls = true, bool replaceAllUrls = true)
        {
			Logger.LogDebug(">>> Entering AddProviderRow(providersTable={ProvidersTable},extensionUrlProviderId={ExtensionUrlProviderId},portalId={PortalId},desktopModuleId={DesktopModuleId},providerName={ProviderName},providerType={ProviderType},settingsControlSrc={SettingsControlSrc},isActive={IsActive},rewriteAllUrls={RewriteAllUrls},redirectAllUrls={RedirectAllUrls},replaceAllUrls={ReplaceAllUrls})", providersTable, extensionUrlProviderId, portalId, desktopModuleId, providerName, providerType, settingsControlSrc, isActive, rewriteAllUrls, redirectAllUrls, replaceAllUrls);
            providersTable.Rows.Add(
                extensionUrlProviderId,
                portalId,
                desktopModuleId.HasValue ? desktopModuleId : (object)DBNull.Value,
                providerName,
                providerType,
                settingsControlSrc,
                isActive,
                rewriteAllUrls,
                redirectAllUrls,
                replaceAllUrls);
				Logger.LogDebug("<<< Exiting AddProviderRow(providersTable={ProvidersTable},extensionUrlProviderId={ExtensionUrlProviderId},portalId={PortalId},desktopModuleId={DesktopModuleId},providerName={ProviderName},providerType={ProviderType},settingsControlSrc={SettingsControlSrc},isActive={IsActive},rewriteAllUrls={RewriteAllUrls},redirectAllUrls={RedirectAllUrls},replaceAllUrls={ReplaceAllUrls})", providersTable, extensionUrlProviderId, portalId, desktopModuleId, providerName, providerType, settingsControlSrc, isActive, rewriteAllUrls, redirectAllUrls, replaceAllUrls);
        }

        internal class FakeExtensionUrlProvider : ExtensionUrlProvider
        {
            public override bool AlwaysUsesDnnPagePath(int portalId)
            {
				Logger.LogDebug(">>> Entering AlwaysUsesDnnPagePath(portalId={PortalId})", portalId);
                throw new NotImplementedException();
            }

            public override string ChangeFriendlyUrl(TabInfo tab, string friendlyUrlPath, FriendlyUrlOptions options, string cultureCode, ref string endingPageName, out bool useDnnPagePath, ref List<string> messages)
            {
				Logger.LogDebug(">>> Entering ChangeFriendlyUrl(tab={Tab},friendlyUrlPath={FriendlyUrlPath},options={Options},cultureCode={CultureCode},endingPageName={EndingPageName},messages={Messages})", tab, friendlyUrlPath, options, cultureCode, endingPageName, messages);
                throw new NotImplementedException();
            }

            public override bool CheckForRedirect(int tabId, int portalid, string httpAlias, Uri requestUri, NameValueCollection queryStringCol, FriendlyUrlOptions options, out string redirectLocation, ref List<string> messages)
            {
				Logger.LogDebug(">>> Entering CheckForRedirect(tabId={TabId},portalid={Portalid},httpAlias={HttpAlias},requestUri={RequestUri},queryStringCol={QueryStringCol},options={Options},messages={Messages})", tabId, portalid, httpAlias, requestUri, queryStringCol, options, messages);
                throw new NotImplementedException();
            }

            public override Dictionary<string, string> GetProviderPortalSettings()
            {
                throw new NotImplementedException();
            }

            public override string TransformFriendlyUrlToQueryString(string[] urlParms, int tabId, int portalId, FriendlyUrlOptions options, string cultureCode, PortalAliasInfo portalAlias, ref List<string> messages, out int status, out string location)
            {
				Logger.LogDebug(">>> Entering TransformFriendlyUrlToQueryString(urlParms={UrlParms},tabId={TabId},portalId={PortalId},options={Options},cultureCode={CultureCode},portalAlias={PortalAlias},messages={Messages})", urlParms, tabId, portalId, options, cultureCode, portalAlias, messages);
                throw new NotImplementedException();
            }
        }
    }
}
