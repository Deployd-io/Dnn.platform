// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.PersonaBar.Pages
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Tests.Integration.Executers;
    using DotNetNuke.Tests.Integration.Executers.Builders;
    using DotNetNuke.Tests.Integration.Executers.Dto;
    using NUnit.Framework;

    [TestFixture]
    public class PagesManagementTests : IntegrationTestBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly string _hostName;
        private readonly string _hostPass;

        private readonly int PortalId = 0;

        public PagesManagementTests()
        {
			Logger.LogDebug(">>> Entering PagesManagementTests()");
            var url = ConfigurationManager.AppSettings["siteUrl"];
            this._hostName = ConfigurationManager.AppSettings["hostUsername"];
			Logger.LogDebug("PagesManagementTests(): url → {Url}", url);
			Logger.LogDebug("PagesManagementTests(): this._hostName → {Arg0}", this._hostName);
            this._hostPass = ConfigurationManager.AppSettings["hostPassword"];
			Logger.LogDebug("<<< Exiting PagesManagementTests()");
			Logger.LogDebug("PagesManagementTests(): this._hostPass → {Arg0}", this._hostPass);
        }

        [OneTimeSetUp]
        public override async Task TestFixtureSetUp()
        {
			Logger.LogDebug(">>> Entering TestFixtureSetUp()");
            await base.TestFixtureSetUp();

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            this.UpdateSslSettings(true);

            this.UpdateContentLocalization(true);
			Logger.LogDebug("<<< Exiting TestFixtureSetUp()");
        }

        [OneTimeTearDown]
        public override void TestFixtureTearDown()
        {
			Logger.LogDebug(">>> Entering TestFixtureTearDown()");
            base.TestFixtureTearDown();

            this.UpdateSslSettings(false);

            this.UpdateContentLocalization(false);
			Logger.LogDebug("<<< Exiting TestFixtureTearDown()");
        }

        [Test]
        public void Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel()
        {
			Logger.LogDebug(">>> Entering Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel()");
            int tabId;
            var connector = this.CreateNewSecurePage(out tabId);

			Logger.LogDebug("Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel(): connector → {Connector}", connector);
            // Try to request the GetLocalization API
            var response = connector.GetContent($"API/PersonaBar/Pages/GetTabLocalization?pageId={tabId}", null, true, false);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Logger.LogDebug("<<< Exiting Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel()");
			Logger.LogDebug("Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel(): response → {Response}", response);
        }

        private void UpdateContentLocalization(bool enabled)
        {
			Logger.LogDebug(">>> Entering UpdateContentLocalization(enabled={Enabled})", enabled);
            var connector = WebApiTestHelper.LoginHost();

			Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): connector → {Connector}", enabled, connector);
            var postData = new
            {
                PortalId = this.PortalId,
                ContentLocalizationEnabled = false,
                SystemDefaultLanguage = "English (United States)",
                SystemDefaultLanguageIcon = "/images/Flags/en-US.gif",
                SiteDefaultLanguage = "en-US",
                LanguageDisplayMode = "NATIVE",
                EnableUrlLanguage = true,
                EnableBrowserLanguage = true,
                AllowUserUICulture = false,
                CultureCode = "en-US",
                AllowContentLocalization = enabled,
            };
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): PortalId → {PortalId}", enabled, PortalId);
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): ContentLocalizationEnabled → {ContentLocalizationEnabled}", enabled, ContentLocalizationEnabled);
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): SystemDefaultLanguage → {SystemDefaultLanguage}", enabled, SystemDefaultLanguage);
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): SystemDefaultLanguageIcon → {SystemDefaultLanguageIcon}", enabled, SystemDefaultLanguageIcon);
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): SiteDefaultLanguage → {SiteDefaultLanguage}", enabled, SiteDefaultLanguage);
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): LanguageDisplayMode → {LanguageDisplayMode}", enabled, LanguageDisplayMode);
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): EnableUrlLanguage → {EnableUrlLanguage}", enabled, EnableUrlLanguage);
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): EnableBrowserLanguage → {EnableBrowserLanguage}", enabled, EnableBrowserLanguage);
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): AllowUserUICulture → {AllowUserUICulture}", enabled, AllowUserUICulture);
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): CultureCode → {CultureCode}", enabled, CultureCode);
				Logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): AllowContentLocalization → {AllowContentLocalization}", enabled, AllowContentLocalization);

            connector.PostJson("API/PersonaBar/SiteSettings/UpdateLanguageSettings", postData);

            connector.PostJson(
                enabled
                    ? $"API/PersonaBar/Languages/EnableLocalizedContent?portalId={this.PortalId}&translatePages=false"
                    : $"API/PersonaBar/Languages/DisableLocalizedContent?portalId={this.PortalId}", new { });
					Logger.LogDebug("<<< Exiting UpdateContentLocalization(enabled={Enabled})", enabled);
        }

        private void UpdateSslSettings(bool sslEnabled)
        {
			Logger.LogDebug(">>> Entering UpdateSslSettings(sslEnabled={SslEnabled})", sslEnabled);
            var connector = WebApiTestHelper.LoginHost();

			Logger.LogDebug("UpdateSslSettings(sslEnabled={SslEnabled}): connector → {Connector}", sslEnabled, connector);
            var postData = new { SSLSetup = 2, SSLEnforced = false, SSLURL = string.Empty, STDURL = string.Empty, SSLOffloadHeader = string.Empty };
            connector.PostJson("API/PersonaBar/Security/UpdateSslSettings", postData);
			Logger.LogDebug("<<< Exiting UpdateSslSettings(sslEnabled={SslEnabled})", sslEnabled);
        }

        private IWebApiConnector CreateNewSecurePage(out int tabId)
        {
			Logger.LogDebug(">>> Entering CreateNewSecurePage()");
            var pagesExecuter = new PagesExecuter { Connector = WebApiTestHelper.LoginHost() };

			Logger.LogDebug("CreateNewSecurePage(): pagesExecuter → {PagesExecuter}", pagesExecuter);
            var pageSettingsBuilder = new PageSettingsBuilder();
            pageSettingsBuilder.WithPermission(new TabPermissionsBuilder().Build());
			Logger.LogDebug("CreateNewSecurePage(): pageSettingsBuilder → {PageSettingsBuilder}", pageSettingsBuilder);
            pageSettingsBuilder.WithSecure(true);

            var pageDetail = pagesExecuter.SavePageDetails(pageSettingsBuilder.Build());

			Logger.LogDebug("CreateNewSecurePage(): pageDetail → {PageDetail}", pageDetail);
            Assert.That(pageDetail.Page, Is.Not.Null, "The system must create the page and return its details in the response");

            tabId = (int)pageDetail.Page.id;
			Logger.LogDebug("CreateNewSecurePage(): tabId → {TabId}", tabId);

			Logger.LogDebug("<<< Exiting CreateNewSecurePage()");
            return pagesExecuter.Connector;
        }
    }
}
