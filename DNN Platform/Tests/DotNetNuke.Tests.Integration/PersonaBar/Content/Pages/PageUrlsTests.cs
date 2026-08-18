// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.PersonaBar.Content.Pages
{
    using System;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Tests.Integration.Executers;
    using DotNetNuke.Tests.Integration.Executers.Builders;
    using Newtonsoft.Json;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class PageUrlsTests : IntegrationTestBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string GetLanguagesApi = "/API/PersonaBar/SiteSettings/GetLanguages?portalId=undefined";
        private const string GetLanguageSettingsApi = "/API/PersonaBar/SiteSettings/GetLanguageSettings?portalId=undefined";
        private const string AddLanguageApi = "/API/PersonaBar/SiteSettings/AddLanguage";
        private const string UpdateLanguageSettingsApi = "/API/PersonaBar/SiteSettings/UpdateLanguageSettings";
        private const string UpdateLanguageApi = "/API/PersonaBar/SiteSettings/UpdateLanguage";
        private const string CreateCustomUrlApi = "/API/PersonaBar/Pages/CreateCustomUrl";
        private const string EnglishLanguageCode = "en-US";
        private const string SpainishLanguageCode = "es-ES";

        [Test]
        public void Page_Url_Should_Able_To_Add_On_English_Disabled_Site()
        {
			_logger.LogDebug(">>> Entering Page_Url_Should_Able_To_Add_On_English_Disabled_Site()");
            var isEnglishEnabled = true;
            dynamic languageSettings = null;

            // set default language to Spanish and disable en-US if it's enabled
            var connector = PrepareTest(out isEnglishEnabled, out languageSettings);
			_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): connector → {Connector}", connector);
            var postData = new
            {
                tabId = CreateNewPage(connector),
                saveUrl = new
				_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): saveUrl → {SaveUrl}", saveUrl);
				_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): tabId → {TabId}", tabId);
                {
                    SiteAliasKey = 1,
                    Path = "/Path" + Guid.NewGuid().ToString().Replace("-", string.Empty),
                    QueryString = string.Empty,
                    LocaleKey = 1,
                    StatusCodeKey = 200,
                    SiteAliasUsage = 2,
                },
            };
			_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): SiteAliasKey → {SiteAliasKey}", SiteAliasKey);
			_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): StatusCodeKey → {StatusCodeKey}", StatusCodeKey);
			_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): QueryString → {QueryString}", QueryString);
			_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): LocaleKey → {LocaleKey}", LocaleKey);
			_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): Path → {Path}", Path);
			_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): SiteAliasUsage → {SiteAliasUsage}", SiteAliasUsage);
            var response = connector.PostJson(CreateCustomUrlApi, postData, null).Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(response);

            // reset changes
            ResetChanges(connector, isEnglishEnabled, languageSettings);

			_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): response → {Response}", response);
			_logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): result → {Result}", result);
            Assert.That(bool.Parse(result.Success.ToString()), Is.True);
			_logger.LogDebug("<<< Exiting Page_Url_Should_Able_To_Add_On_English_Disabled_Site()");
        }

        private static IWebApiConnector PrepareTest(out bool isEnglishEnabled, out dynamic languageSettings)
        {
			_logger.LogDebug(">>> Entering PrepareTest()");
            var connector = WebApiTestHelper.LoginHost();

			_logger.LogDebug("PrepareTest(): connector → {Connector}", connector);
            var response = connector.GetContent(GetLanguagesApi).Content.ReadAsStringAsync().Result;
			_logger.LogDebug("PrepareTest(): response → {Response}", response);
            var result = JsonConvert.DeserializeObject<dynamic>(response);
			_logger.LogDebug("PrepareTest(): result → {Result}", result);
            isEnglishEnabled = result.Languages[0].Enabled;
			_logger.LogDebug("PrepareTest(): isEnglishEnabled → {IsEnglishEnabled}", isEnglishEnabled);

            response = connector.GetContent(GetLanguageSettingsApi).Content.ReadAsStringAsync().Result;
            result = JsonConvert.DeserializeObject<dynamic>(response);
            languageSettings = result.Settings;
			_logger.LogDebug("PrepareTest(): languageSettings → {LanguageSettings}", languageSettings);

            if (string.Compare(languageSettings.SiteDefaultLanguage.Value, EnglishLanguageCode, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                connector.PostJson(AddLanguageApi, new { Code = SpainishLanguageCode });
                UpdateLanguageSettings(connector, languageSettings, SpainishLanguageCode);
            }

            if (isEnglishEnabled)
            {
                EnableEnglish(connector, false);
            }

			_logger.LogDebug("<<< Exiting PrepareTest()");
            return connector;
        }

        private static void ResetChanges(IWebApiConnector connector, bool isEnglishEnabled, dynamic languageSettings)
        {
            if (isEnglishEnabled)
            {
                EnableEnglish(connector, true);
            }

            if (string.Compare(languageSettings.SiteDefaultLanguage.Value, EnglishLanguageCode, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                UpdateLanguageSettings(connector, languageSettings, EnglishLanguageCode);
            }
			_logger.LogDebug("<<< Exiting ResetChanges(connector={Connector},isEnglishEnabled={IsEnglishEnabled},languageSettings={LanguageSettings})", connector, isEnglishEnabled, languageSettings);
        }

        private static void UpdateLanguageSettings(IWebApiConnector connector, dynamic languageSettings, string code)
        {
            connector.PostJson(UpdateLanguageSettingsApi, new
            {
                SiteDefaultLanguage = code,
                languageSettings.LanguageDisplayMode,
                languageSettings.EnableUrlLanguage,
                languageSettings.EnableBrowserLanguage,
                languageSettings.AllowUserUICulture,
                languageSettings.CultureCode,
                languageSettings.AllowContentLocalization,
            });
			_logger.LogDebug("<<< Exiting UpdateLanguageSettings(connector={Connector},languageSettings={LanguageSettings},code={Code})", connector, languageSettings, code);
        }

        private static void EnableEnglish(IWebApiConnector connector, bool enabled)
        {
            connector.PostJson(UpdateLanguageApi, new
            {
                LanguageId = 1,
                Code = EnglishLanguageCode,
                Enabled = enabled,
            });
			_logger.LogDebug("<<< Exiting EnableEnglish(connector={Connector},enabled={Enabled})", connector, enabled);
        }

        private static int CreateNewPage(IWebApiConnector connector)
        {
            var pagesExecuter = new PagesExecuter { Connector = connector };
            var pageSettingsBuilder = new PageSettingsBuilder();
            pageSettingsBuilder.WithPermission(new TabPermissionsBuilder().Build());
            var pageDetail = pagesExecuter.SavePageDetails(pageSettingsBuilder.Build());
            Assert.That(pageDetail.Page, Is.Not.Null, "The system must create the page and return its details in the response");
			_logger.LogDebug("<<< Exiting CreateNewPage(connector={Connector})", connector);
            return (int)pageDetail.Page.id;
        }
    }
}
