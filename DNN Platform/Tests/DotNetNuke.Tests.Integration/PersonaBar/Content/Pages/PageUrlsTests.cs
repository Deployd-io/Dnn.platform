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

    [TestFixture]
    public class PageUrlsTests : IntegrationTestBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
			Logger.LogDebug(">>> Entering Page_Url_Should_Able_To_Add_On_English_Disabled_Site()");
            var isEnglishEnabled = true;
            dynamic languageSettings = null;

            // set default language to Spanish and disable en-US if it's enabled
            var connector = PrepareTest(out isEnglishEnabled, out languageSettings);
            var postData = new
			Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): connector → {Connector}", connector);
            {
                tabId = CreateNewPage(connector),
                saveUrl = new
				Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): tabId → {TabId}", tabId);
				Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): saveUrl → {SaveUrl}", saveUrl);
                {
                    SiteAliasKey = 1,
                    Path = "/Path" + Guid.NewGuid().ToString().Replace("-", string.Empty),
                    QueryString = string.Empty,
                    LocaleKey = 1,
                    StatusCodeKey = 200,
                    SiteAliasUsage = 2,
                },
            };
					Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): SiteAliasKey → {SiteAliasKey}", SiteAliasKey);
					Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): Path → {Path}", Path);
					Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): QueryString → {QueryString}", QueryString);
					Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): LocaleKey → {LocaleKey}", LocaleKey);
					Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): StatusCodeKey → {StatusCodeKey}", StatusCodeKey);
					Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): SiteAliasUsage → {SiteAliasUsage}", SiteAliasUsage);
            var response = connector.PostJson(CreateCustomUrlApi, postData, null).Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(response);
			Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): response → {Response}", response);
			Logger.LogDebug("Page_Url_Should_Able_To_Add_On_English_Disabled_Site(): result → {Result}", result);

            // reset changes
            ResetChanges(connector, isEnglishEnabled, languageSettings);

            Assert.That(bool.Parse(result.Success.ToString()), Is.True);
			Logger.LogDebug("<<< Exiting Page_Url_Should_Able_To_Add_On_English_Disabled_Site()");
        }

        private static IWebApiConnector PrepareTest(out bool isEnglishEnabled, out dynamic languageSettings)
        {
			Logger.LogDebug(">>> Entering PrepareTest()");
            var connector = WebApiTestHelper.LoginHost();

			Logger.LogDebug("PrepareTest(): connector → {Connector}", connector);
            var response = connector.GetContent(GetLanguagesApi).Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(response);
			Logger.LogDebug("PrepareTest(): response → {Response}", response);
			Logger.LogDebug("PrepareTest(): result → {Result}", result);
            isEnglishEnabled = result.Languages[0].Enabled;

			Logger.LogDebug("PrepareTest(): isEnglishEnabled → {IsEnglishEnabled}", isEnglishEnabled);
            response = connector.GetContent(GetLanguageSettingsApi).Content.ReadAsStringAsync().Result;
            result = JsonConvert.DeserializeObject<dynamic>(response);
            languageSettings = result.Settings;

			Logger.LogDebug("PrepareTest(): languageSettings → {LanguageSettings}", languageSettings);
            if (string.Compare(languageSettings.SiteDefaultLanguage.Value, EnglishLanguageCode, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                connector.PostJson(AddLanguageApi, new { Code = SpainishLanguageCode });
                UpdateLanguageSettings(connector, languageSettings, SpainishLanguageCode);
            }

            if (isEnglishEnabled)
            {
                EnableEnglish(connector, false);
            }

			Logger.LogDebug("<<< Exiting PrepareTest()");
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
        }

        private static void EnableEnglish(IWebApiConnector connector, bool enabled)
        {
            connector.PostJson(UpdateLanguageApi, new
            {
                LanguageId = 1,
                Code = EnglishLanguageCode,
                Enabled = enabled,
            });
        }

        private static int CreateNewPage(IWebApiConnector connector)
        {
            var pagesExecuter = new PagesExecuter { Connector = connector };
            var pageSettingsBuilder = new PageSettingsBuilder();
            pageSettingsBuilder.WithPermission(new TabPermissionsBuilder().Build());
            var pageDetail = pagesExecuter.SavePageDetails(pageSettingsBuilder.Build());
            Assert.That(pageDetail.Page, Is.Not.Null, "The system must create the page and return its details in the response");
            return (int)pageDetail.Page.id;
        }
    }
}
