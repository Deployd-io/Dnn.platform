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
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class PagesManagementTests : IntegrationTestBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly string _hostName;
        private readonly string _hostPass;

        private readonly int PortalId = 0;

        public PagesManagementTests()
        {
			_logger.LogDebug(">>> Entering PagesManagementTests()");
            var url = ConfigurationManager.AppSettings["siteUrl"];
            this._hostName = ConfigurationManager.AppSettings["hostUsername"];
			_logger.LogDebug("PagesManagementTests(): this._hostName → {Arg0}", this._hostName);
            this._hostPass = ConfigurationManager.AppSettings["hostPassword"];
			_logger.LogDebug("<<< Exiting PagesManagementTests()");
        }

        [OneTimeSetUp]
        public override async Task TestFixtureSetUp()
        {
			_logger.LogDebug(">>> Entering TestFixtureSetUp()");
            await base.TestFixtureSetUp();

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            this.UpdateSslSettings(true);

            this.UpdateContentLocalization(true);
			_logger.LogDebug("<<< Exiting TestFixtureSetUp()");
        }

        [OneTimeTearDown]
        public override void TestFixtureTearDown()
        {
			_logger.LogDebug(">>> Entering TestFixtureTearDown()");
            base.TestFixtureTearDown();

            this.UpdateSslSettings(false);

            this.UpdateContentLocalization(false);
			_logger.LogDebug("<<< Exiting TestFixtureTearDown()");
        }

        [Test]
        public void Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel()
        {
			_logger.LogDebug(">>> Entering Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel()");
            int tabId;
            var connector = this.CreateNewSecurePage(out tabId);
			_logger.LogDebug("Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel(): connector → {Connector}", connector);

            // Try to request the GetLocalization API
            var response = connector.GetContent($"API/PersonaBar/Pages/GetTabLocalization?pageId={tabId}", null, true, false);
			_logger.LogDebug("Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel(): response → {Response}", response);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			_logger.LogDebug("<<< Exiting Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel()");
        }

        private void UpdateContentLocalization(bool enabled)
        {
			_logger.LogDebug(">>> Entering UpdateContentLocalization(enabled={Enabled})", enabled);
            var connector = WebApiTestHelper.LoginHost();

			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): connector → {Connector}", enabled, connector);
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
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): SystemDefaultLanguage → {SystemDefaultLanguage}", enabled, SystemDefaultLanguage);
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): EnableBrowserLanguage → {EnableBrowserLanguage}", enabled, EnableBrowserLanguage);
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): PortalId → {PortalId}", enabled, PortalId);
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): SystemDefaultLanguageIcon → {SystemDefaultLanguageIcon}", enabled, SystemDefaultLanguageIcon);
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): SiteDefaultLanguage → {SiteDefaultLanguage}", enabled, SiteDefaultLanguage);
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): AllowContentLocalization → {AllowContentLocalization}", enabled, AllowContentLocalization);
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): CultureCode → {CultureCode}", enabled, CultureCode);
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): ContentLocalizationEnabled → {ContentLocalizationEnabled}", enabled, ContentLocalizationEnabled);
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): EnableUrlLanguage → {EnableUrlLanguage}", enabled, EnableUrlLanguage);
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): LanguageDisplayMode → {LanguageDisplayMode}", enabled, LanguageDisplayMode);
			_logger.LogDebug("UpdateContentLocalization(enabled={Enabled}): AllowUserUICulture → {AllowUserUICulture}", enabled, AllowUserUICulture);

            connector.PostJson("API/PersonaBar/SiteSettings/UpdateLanguageSettings", postData);

            connector.PostJson(
                enabled
                    ? $"API/PersonaBar/Languages/EnableLocalizedContent?portalId={this.PortalId}&translatePages=false"
                    : $"API/PersonaBar/Languages/DisableLocalizedContent?portalId={this.PortalId}", new { });
					_logger.LogDebug("<<< Exiting UpdateContentLocalization(enabled={Enabled})", enabled);
        }

        private void UpdateSslSettings(bool sslEnabled)
        {
            var connector = WebApiTestHelper.LoginHost();
			_logger.LogDebug(">>> Entering UpdateSslSettings(sslEnabled={SslEnabled})", sslEnabled);

            var postData = new { SSLSetup = 2, SSLEnforced = false, SSLURL = string.Empty, STDURL = string.Empty, SSLOffloadHeader = string.Empty };
            connector.PostJson("API/PersonaBar/Security/UpdateSslSettings", postData);
			_logger.LogDebug("<<< Exiting UpdateSslSettings(sslEnabled={SslEnabled})", sslEnabled);
        }

        private IWebApiConnector CreateNewSecurePage(out int tabId)
        {
			_logger.LogDebug(">>> Entering CreateNewSecurePage()");
            var pagesExecuter = new PagesExecuter { Connector = WebApiTestHelper.LoginHost() };

			_logger.LogDebug("CreateNewSecurePage(): pagesExecuter → {PagesExecuter}", pagesExecuter);
            var pageSettingsBuilder = new PageSettingsBuilder();
			_logger.LogDebug("CreateNewSecurePage(): pageSettingsBuilder → {PageSettingsBuilder}", pageSettingsBuilder);
            pageSettingsBuilder.WithPermission(new TabPermissionsBuilder().Build());
            pageSettingsBuilder.WithSecure(true);

            var pageDetail = pagesExecuter.SavePageDetails(pageSettingsBuilder.Build());
			_logger.LogDebug("CreateNewSecurePage(): pageDetail → {PageDetail}", pageDetail);

            Assert.That(pageDetail.Page, Is.Not.Null, "The system must create the page and return its details in the response");

            tabId = (int)pageDetail.Page.id;

			_logger.LogDebug("CreateNewSecurePage(): tabId → {TabId}", tabId);
			_logger.LogDebug("<<< Exiting CreateNewSecurePage()");
            return pagesExecuter.Connector;
        }
    }
}
