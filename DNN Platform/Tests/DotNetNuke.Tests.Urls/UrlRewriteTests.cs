// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class UrlRewriteTests : UrlTestBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string DefaultPage = Globals.glbDefaultPage;
        private const string TestPage = "Test Page";
        private const string AboutUsPageName = "About Us";
        private int tabId;
        private string redirectMode;
        private Locale customLocale;
        private string securePageName;
        private PortalAliasInfo primaryAlias;
        private bool sslEnforced;
        private bool sslEnabled;

        [SetUp]
        public override void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            base.SetUp();

            this.DeleteTab(TestPage);
            this.CreateTab(TestPage);
            this.UpdateTabName(this.tabId, "About Us");
            this.UpdateTabSkin(this.tabId, string.Empty);
            CacheController.FlushPageIndexFromCache();
            this.GetDefaultAlias();
            this.redirectMode = PortalController.GetPortalSetting("PortalAliasMapping", this.PortalId, "CANONICALURL");
            this.sslEnforced = PortalController.GetPortalSettingAsBoolean("SSLEnforced", this.PortalId, false);
            this.sslEnabled = PortalController.GetPortalSettingAsInteger("SSLSetup", this.PortalId, 0) != 0;
            this.primaryAlias = null;
			_logger.LogDebug("SetUp(): primaryAlias → {PrimaryAlias}", primaryAlias);
			_logger.LogDebug("SetUp(): this.primaryAlias → {Arg0}", this.primaryAlias);
            this.customLocale = null;
            DataCache.ClearCache();
			_logger.LogDebug("SetUp(): customLocale → {CustomLocale}", customLocale);
			_logger.LogDebug("SetUp(): this.customLocale → {Arg0}", this.customLocale);
			_logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public override void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            base.TearDown();

            this.DeleteTab(TestPage);
            this.UpdateTabName(this.tabId, "About Us");
            this.UpdateTabSkin(this.tabId, "[G]Skins/Aperture/default.ascx");

            if (!string.IsNullOrEmpty(this.securePageName))
            {
                var tab = TabController.Instance.GetTabByName(this.securePageName, this.PortalId);
				_logger.LogDebug("TearDown(): tab → {Tab}", tab);
                if (tab != null)
                {
                    tab.IsSecure = false;
					_logger.LogDebug("TearDown(): tab.IsSecure → {Arg0}", tab.IsSecure);

                    this.UpdateTab(tab);
                }
            }

            if (this.customLocale != null)
            {
                Localization.RemoveLanguageFromPortals(this.customLocale.LanguageId, true);
                Localization.DeleteLanguage(this.customLocale, true);
            }

            if (this.primaryAlias != null)
            {
                Globals.DependencyProvider.GetRequiredService<IPortalAliasService>().DeletePortalAlias(this.primaryAlias);
            }

            this.SetDefaultAlias(this.DefaultAlias);
            PortalController.UpdatePortalSetting(this.PortalId, "PortalAliasMapping", this.redirectMode, true, "en-us");
            PortalController.UpdatePortalSetting(this.PortalId, "SSLEnforced", this.sslEnforced.ToString(), true, "en-us");
            PortalController.UpdatePortalSetting(this.PortalId, "SSLSetup", this.sslEnabled ? "2" : "0", true, "en-us");

            foreach (var tabUrl in CBO.FillCollection<TabUrlInfo>(DataProvider.Instance().GetTabUrls(this.PortalId)))
            {
                TabController.Instance.DeleteTabUrl(tabUrl, this.PortalId, true);
            }
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
			_logger.LogDebug(">>> Entering OneTimeSetUp()");
            base.OneTimeSetUp();

            var tab = TabController.Instance.GetTabByName(AboutUsPageName, this.PortalId);
			_logger.LogDebug("OneTimeSetUp(): tab → {Tab}", tab);
            if (tab == null)
            {
                this.CreateTab(AboutUsPageName);
                tab = TabController.Instance.GetTabByName(AboutUsPageName, this.PortalId);
            }

            this.tabId = tab.TabID;
			_logger.LogDebug("OneTimeSetUp(): tabId → {TabId}", tabId);
			_logger.LogDebug("OneTimeSetUp(): this.tabId → {Arg0}", this.tabId);

            // Add Portal Aliases
            var aliasController = Globals.DependencyProvider.GetRequiredService<IPortalAliasService>();
			_logger.LogDebug("OneTimeSetUp(): aliasController → {AliasController}", aliasController);
            TestUtil.ReadStream("Aliases", (line, header) =>
                            {
                                var fields = line.Split(',');
								_logger.LogDebug("OneTimeSetUp(): fields → {Fields}", fields);
                                var alias = aliasController.GetPortalAlias(fields[0], this.PortalId);
								_logger.LogDebug("OneTimeSetUp(): alias → {Alias}", alias);
                                if (alias == null)
                                {
                                    alias = new PortalAliasInfo();
                                    alias.HttpAlias = fields[0];
									_logger.LogDebug("OneTimeSetUp(): alias.HttpAlias → {Arg0}", alias.HttpAlias);
									_logger.LogDebug("OneTimeSetUp(): alias.PortalId → {Arg0}", alias.PortalId);
                                    alias.PortalId = this.PortalId;
                                    aliasController.AddPortalAlias(alias);
                                }
                            });
            TestUtil.ReadStream("Users", (line, header) =>
                                {
                                    var fields = line.Split(',');

                                    TestUtil.AddUser(this.PortalId, fields[0].Trim(), fields[1].Trim(), fields[2].Trim());
                                });
								_logger.LogDebug("<<< Exiting OneTimeSetUp()");
        }

        [OneTimeTearDown]
        public override void OneTimeTearDown()
        {
			_logger.LogDebug(">>> Entering OneTimeTearDown()");
            base.OneTimeTearDown();

            var aliasController = Globals.DependencyProvider.GetRequiredService<IPortalAliasService>();
			_logger.LogDebug("OneTimeTearDown(): aliasController → {AliasController}", aliasController);
            TestUtil.ReadStream("Aliases", (line, header) =>
                            {
                                var fields = line.Split(',');
								_logger.LogDebug("OneTimeTearDown(): fields → {Fields}", fields);
                                var alias = aliasController.GetPortalAlias(fields[0], this.PortalId);
								_logger.LogDebug("OneTimeTearDown(): alias → {Alias}", alias);
                                aliasController.DeletePortalAlias(alias);
                            });
            TestUtil.ReadStream("Users", (line, header) =>
                            {
                                var fields = line.Split(',');

                                TestUtil.DeleteUser(this.PortalId, fields[0]);
                            });
							_logger.LogDebug("<<< Exiting OneTimeTearDown()");
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_BasicTestCases))]
        public void AdvancedUrlRewriter_BasicTest(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_BasicTest(testFields={TestFields})", testFields);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

			_logger.LogDebug("AdvancedUrlRewriter_BasicTest(testFields={TestFields}): settings → {Settings}", testFields, settings);
            this.ExecuteTest(settings, testFields, true);
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_BasicTest(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_DeletedTabHandlingTestCases))]
        public void AdvancedUrlRewriter_DeletedTabHandling(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_DeletedTabHandling(testFields={TestFields})", testFields);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

			_logger.LogDebug("AdvancedUrlRewriter_DeletedTabHandling(testFields={TestFields}): settings → {Settings}", testFields, settings);
            var tab = TabController.Instance.GetTabByName(TestPage, this.PortalId);
			_logger.LogDebug("AdvancedUrlRewriter_DeletedTabHandling(testFields={TestFields}): tab → {Tab}", testFields, tab);
            if (Convert.ToBoolean(testFields["HardDeleted"]))
            {
                this.DeleteTab(TestPage);
                CacheController.FlushPageIndexFromCache();
            }
            else
            {
                tab.IsDeleted = Convert.ToBoolean(testFields["SoftDeleted"]);
                tab.DisableLink = Convert.ToBoolean(testFields["Disabled"]);
                if (Convert.ToBoolean(testFields["Expired"]))
                {
                    tab.EndDate = DateTime.Now - TimeSpan.FromDays(1);
                }

                this.UpdateTab(tab);
                CacheController.FlushPageIndexFromCache();
            }

            var deletedTabHandling = testFields.GetValue("DeletedTabHandling");
			_logger.LogDebug("AdvancedUrlRewriter_DeletedTabHandling(testFields={TestFields}): deletedTabHandling → {DeletedTabHandling}", testFields, deletedTabHandling);

            if (!string.IsNullOrEmpty(deletedTabHandling))
            {
                switch (deletedTabHandling)
                {
                    case "Do404Error":
                        settings.DeletedTabHandlingType = DeletedTabHandlingType.Do404Error;
						_logger.LogDebug("AdvancedUrlRewriter_DeletedTabHandling(testFields={TestFields}): settings.DeletedTabHandlingType → {Arg1}", testFields, settings.DeletedTabHandlingType);
                        break;
                    default:
                        settings.DeletedTabHandlingType = DeletedTabHandlingType.Do301RedirectToPortalHome;
                        break;
                }
            }

            this.SetDefaultAlias(testFields);

            this.ExecuteTest(settings, testFields, true);
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_DeletedTabHandling(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_DoNotRedirect))]
        public void AdvancedUrlRewriter_DoNotRedirect(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_DoNotRedirect(testFields={TestFields})", testFields);
            var tabName = testFields["Page Name"];
            var doNotRedirect = testFields["DoNotRedirect"];
			_logger.LogDebug("AdvancedUrlRewriter_DoNotRedirect(testFields={TestFields}): doNotRedirect → {DoNotRedirect}", testFields, doNotRedirect);
			_logger.LogDebug("AdvancedUrlRewriter_DoNotRedirect(testFields={TestFields}): tabName → {TabName}", testFields, tabName);

            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);
			_logger.LogDebug("AdvancedUrlRewriter_DoNotRedirect(testFields={TestFields}): settings → {Settings}", testFields, settings);

            this.UpdateTabSetting(tabName, "DoNotRedirect", doNotRedirect);
            settings.UseBaseFriendlyUrls = testFields["UseBaseFriendlyUrls"];
			_logger.LogDebug("AdvancedUrlRewriter_DoNotRedirect(testFields={TestFields}): settings.UseBaseFriendlyUrls → {Arg1}", testFields, settings.UseBaseFriendlyUrls);

            this.ExecuteTest(settings, testFields, true);

            this.UpdateTabSetting(tabName, "DoNotRedirect", "False");
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_DoNotRedirect(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_ForwardExternalUrlTestCases))]
        public void AdvancedUrlRewriter_ForwardExternalUrls(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_ForwardExternalUrls(testFields={TestFields})", testFields);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

			_logger.LogDebug("AdvancedUrlRewriter_ForwardExternalUrls(testFields={TestFields}): settings → {Settings}", testFields, settings);
            var tab = TabController.Instance.GetTabByName(TestPage, this.PortalId);
			_logger.LogDebug("AdvancedUrlRewriter_ForwardExternalUrls(testFields={TestFields}): tab → {Tab}", testFields, tab);
			_logger.LogDebug("AdvancedUrlRewriter_ForwardExternalUrls(testFields={TestFields}): tab.Url → {Arg1}", testFields, tab.Url);
            tab.Url = testFields["ExternalUrl"];
            TabController.Instance.UpdateTab(tab);

            this.ExecuteTest(settings, testFields, true);
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_ForwardExternalUrls(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_ForceLowerCaseTestCases))]
        public void AdvancedUrlRewriter_ForceLowerCase(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_ForceLowerCase(testFields={TestFields})", testFields);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

			_logger.LogDebug("AdvancedUrlRewriter_ForceLowerCase(testFields={TestFields}): settings → {Settings}", testFields, settings);
            var forceLowerCaseRegex = testFields.GetValue("ForceLowerCaseRegex");
			_logger.LogDebug("AdvancedUrlRewriter_ForceLowerCase(testFields={TestFields}): forceLowerCaseRegex → {ForceLowerCaseRegex}", testFields, forceLowerCaseRegex);

            if (!string.IsNullOrEmpty(forceLowerCaseRegex))
            {
                settings.ForceLowerCaseRegex = forceLowerCaseRegex;
				_logger.LogDebug("AdvancedUrlRewriter_ForceLowerCase(testFields={TestFields}): settings.ForceLowerCaseRegex → {Arg1}", testFields, settings.ForceLowerCaseRegex);
            }

            this.ExecuteTest(settings, testFields, true);
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_ForceLowerCase(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_RegexTestCases))]
        public void AdvancedUrlRewriter_Regex(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_Regex(testFields={TestFields})", testFields);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

			_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings → {Settings}", testFields, settings);
            var regexSetting = testFields["Setting"];
			_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): regexSetting → {RegexSetting}", testFields, regexSetting);
            var regexValue = testFields["Value"];
			_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): regexValue → {RegexValue}", testFields, regexValue);
            if (!string.IsNullOrEmpty(regexValue))
            {
                switch (regexSetting)
                {
                    case "IgnoreRegex":
                        settings.IgnoreRegex = regexValue;
						_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings.IgnoreRegex → {Arg1}", testFields, settings.IgnoreRegex);
                        break;
                    case "DoNotRewriteRegex":
                        settings.DoNotRewriteRegex = regexValue;
						_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings.DoNotRewriteRegex → {Arg1}", testFields, settings.DoNotRewriteRegex);
                        break;
                    case "UseSiteUrlsRegex":
                        settings.UseSiteUrlsRegex = regexValue;
						_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings.UseSiteUrlsRegex → {Arg1}", testFields, settings.UseSiteUrlsRegex);
                        break;
                    case "DoNotRedirectRegex":
                        settings.DoNotRedirectRegex = regexValue;
						_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings.DoNotRedirectRegex → {Arg1}", testFields, settings.DoNotRedirectRegex);
                        break;
                    case "DoNotRedirectSecureRegex":
                        settings.DoNotRedirectSecureRegex = regexValue;
						_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings.DoNotRedirectSecureRegex → {Arg1}", testFields, settings.DoNotRedirectSecureRegex);
                        break;
                    case "ForceLowerCaseRegex":
                        settings.ForceLowerCaseRegex = regexValue;
						_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings.ForceLowerCaseRegex → {Arg1}", testFields, settings.ForceLowerCaseRegex);
                        break;
                    case "NoFriendlyUrlRegex":
                        settings.NoFriendlyUrlRegex = regexValue;
						_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings.NoFriendlyUrlRegex → {Arg1}", testFields, settings.NoFriendlyUrlRegex);
                        break;
                    case "DoNotIncludeInPathRegex":
                        settings.DoNotIncludeInPathRegex = regexValue;
						_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings.DoNotIncludeInPathRegex → {Arg1}", testFields, settings.DoNotIncludeInPathRegex);
                        break;
                    case "ValidExtensionlessUrlsRegex":
                        settings.ValidExtensionlessUrlsRegex = regexValue;
						_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings.ValidExtensionlessUrlsRegex → {Arg1}", testFields, settings.ValidExtensionlessUrlsRegex);
                        break;
                    case "RegexMatch":
                        settings.RegexMatch = regexValue;
						_logger.LogDebug("AdvancedUrlRewriter_Regex(testFields={TestFields}): settings.RegexMatch → {Arg1}", testFields, settings.RegexMatch);
                        break;
                }
            }

            this.ExecuteTest(settings, testFields, true);
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_Regex(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_ReplaceCharsTestCases))]
        public void AdvancedUrlRewriter_ReplaceChars(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_ReplaceChars(testFields={TestFields})", testFields);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

			_logger.LogDebug("AdvancedUrlRewriter_ReplaceChars(testFields={TestFields}): settings → {Settings}", testFields, settings);
            var testPageName = testFields.GetValue("TestPageName");
			_logger.LogDebug("AdvancedUrlRewriter_ReplaceChars(testFields={TestFields}): testPageName → {TestPageName}", testFields, testPageName);
            TabInfo tab = null;
			_logger.LogDebug("AdvancedUrlRewriter_ReplaceChars(testFields={TestFields}): tab → {Tab}", testFields, tab);
            if (!string.IsNullOrEmpty(testPageName))
            {
                var tabName = testFields["Page Name"];
				_logger.LogDebug("AdvancedUrlRewriter_ReplaceChars(testFields={TestFields}): tabName → {TabName}", testFields, tabName);
                tab = TabController.Instance.GetTabByName(tabName, this.PortalId);
                tab.TabName = testPageName;
				_logger.LogDebug("AdvancedUrlRewriter_ReplaceChars(testFields={TestFields}): tab.TabName → {Arg1}", testFields, tab.TabName);
                TabController.Instance.UpdateTab(tab);

                // Refetch tab from DB
                tab = TabController.Instance.GetTab(tab.TabID, tab.PortalID, false);
            }

            var autoAscii = testFields.GetValue("AutoAscii");
			_logger.LogDebug("AdvancedUrlRewriter_ReplaceChars(testFields={TestFields}): autoAscii → {AutoAscii}", testFields, autoAscii);

            if (!string.IsNullOrEmpty(autoAscii))
            {
                settings.AutoAsciiConvert = Convert.ToBoolean(autoAscii);
            }

            TestUtil.GetReplaceCharDictionary(testFields, settings.ReplaceCharacterDictionary);

            this.SetDefaultAlias(testFields);

            this.ExecuteTestForTab(tab, settings, testFields);
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_ReplaceChars(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_ReplaceSpaceTestCases))]
        public void AdvancedUrlRewriter_ReplaceSpace(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_ReplaceSpace(testFields={TestFields})", testFields);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

			_logger.LogDebug("AdvancedUrlRewriter_ReplaceSpace(testFields={TestFields}): settings → {Settings}", testFields, settings);
            var replaceSpaceWith = testFields.GetValue("ReplaceSpaceWith");
			_logger.LogDebug("AdvancedUrlRewriter_ReplaceSpace(testFields={TestFields}): replaceSpaceWith → {ReplaceSpaceWith}", testFields, replaceSpaceWith);
            if (!string.IsNullOrEmpty(replaceSpaceWith))
            {
                settings.ReplaceSpaceWith = replaceSpaceWith;
				_logger.LogDebug("AdvancedUrlRewriter_ReplaceSpace(testFields={TestFields}): settings.ReplaceSpaceWith → {Arg1}", testFields, settings.ReplaceSpaceWith);
            }

            this.ExecuteTest(settings, testFields, true);
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_ReplaceSpace(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_SiteRootRedirectTestCases))]
        public void AdvancedUrlRewriter_SiteRootRedirect(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_SiteRootRedirect(testFields={TestFields})", testFields);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", "SiteRootRedirect", this.PortalId);

			_logger.LogDebug("AdvancedUrlRewriter_SiteRootRedirect(testFields={TestFields}): settings → {Settings}", testFields, settings);
            var scheme = testFields["Scheme"];
			_logger.LogDebug("AdvancedUrlRewriter_SiteRootRedirect(testFields={TestFields}): scheme → {Scheme}", testFields, scheme);

            this.ExecuteTest(settings, testFields, true);
            if (testFields["TestName"].Contains("Resubmit"))
            {
                var httpAlias = testFields["Alias"];
				_logger.LogDebug("AdvancedUrlRewriter_SiteRootRedirect(testFields={TestFields}): httpAlias → {HttpAlias}", testFields, httpAlias);
				_logger.LogDebug("AdvancedUrlRewriter_SiteRootRedirect(testFields={TestFields}): settings.DoNotRedirectRegex → {Arg1}", testFields, settings.DoNotRedirectRegex);
                settings.DoNotRedirectRegex = scheme + httpAlias;

                this.ExecuteTest(settings, testFields, true);
            }
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_SiteRootRedirect(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_PrimaryPortalAliasTestCases))]
        public void AdvancedUrlRewriter_0_PrimaryPortalAlias(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields})", testFields);
            var defaultAlias = testFields["DefaultAlias"];

			_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): defaultAlias → {DefaultAlias}", testFields, defaultAlias);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);
			_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): settings → {Settings}", testFields, settings);

            var language = testFields["Language"].Trim();
			_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): language → {Language}", testFields, language);
            var skin = testFields["Skin"].Trim();
			_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): skin → {Skin}", testFields, skin);
            if (!string.IsNullOrEmpty(language))
            {
                this.customLocale = new Locale { Code = language, Fallback = "en-US" };
				_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): customLocale → {CustomLocale}", testFields, customLocale);
				_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): this.customLocale → {Arg1}", testFields, this.customLocale);
                this.customLocale.Text = CultureInfo.GetCultureInfo(this.customLocale.Code).NativeName;
                Localization.SaveLanguage(this.customLocale);
                Localization.AddLanguageToPortals(this.customLocale.LanguageId);
            }

            if (testFields.ContainsKey("Final Url"))
            {
                testFields["Final Url"] = testFields["Final Url"].Replace("{useAlias}", defaultAlias);
            }

            PortalController.UpdatePortalSetting(this.PortalId, "PortalAliasMapping", "REDIRECT", true, "en-us");

            var portalAliasService = Globals.DependencyProvider.GetRequiredService<IPortalAliasService>();
			_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): portalAliasService → {PortalAliasService}", testFields, portalAliasService);
            var alias = portalAliasService.GetPortalAlias(defaultAlias, this.PortalId);
			_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): alias → {Alias}", testFields, alias);
            if (alias == null)
            {
                alias = new PortalAliasInfo();
                alias.HttpAlias = defaultAlias;
				_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): alias.HttpAlias → {Arg1}", testFields, alias.HttpAlias);
				_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): alias.PortalId → {Arg1}", testFields, alias.PortalId);
				_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): alias.IsPrimary → {Arg1}", testFields, alias.IsPrimary);
                alias.PortalId = this.PortalId;
                alias.IsPrimary = true;
                if (!(string.IsNullOrEmpty(language) && string.IsNullOrEmpty(skin)))
                {
                    alias.CultureCode = language;
					_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): alias.CultureCode → {Arg1}", testFields, alias.CultureCode);
					_logger.LogDebug("AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields}): alias.Skin → {Arg1}", testFields, alias.Skin);
                    alias.Skin = skin;
                }

                portalAliasService.AddPortalAlias(alias);
            }

            this.SetDefaultAlias(defaultAlias);
            this.ExecuteTest(settings, testFields, false);

            alias = portalAliasService.GetPortalAlias(defaultAlias, this.PortalId);
            if (alias != null)
            {
                portalAliasService.DeletePortalAlias(alias);
            }
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_0_PrimaryPortalAlias(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_VanityUrlTestCases))]
        public void AdvancedUrlRewriter_VanityUrl(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_VanityUrl(testFields={TestFields})", testFields);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);
            settings.DeletedTabHandlingType = DeletedTabHandlingType.Do301RedirectToPortalHome;
			_logger.LogDebug("AdvancedUrlRewriter_VanityUrl(testFields={TestFields}): settings.DeletedTabHandlingType → {Arg1}", testFields, settings.DeletedTabHandlingType);
			_logger.LogDebug("AdvancedUrlRewriter_VanityUrl(testFields={TestFields}): settings → {Settings}", testFields, settings);

            var vanityUrl = testFields.GetValue("VanityUrl", string.Empty);
			_logger.LogDebug("AdvancedUrlRewriter_VanityUrl(testFields={TestFields}): vanityUrl → {VanityUrl}", testFields, vanityUrl);
            var userName = testFields.GetValue("UserName", string.Empty);
			_logger.LogDebug("AdvancedUrlRewriter_VanityUrl(testFields={TestFields}): userName → {UserName}", testFields, userName);
            var redirectOld = testFields.GetValue("RedirectOldProfileUrl", string.Empty);
			_logger.LogDebug("AdvancedUrlRewriter_VanityUrl(testFields={TestFields}): redirectOld → {RedirectOld}", testFields, redirectOld);

            if (!string.IsNullOrEmpty(userName))
            {
                var user = UserController.GetUserByName(this.PortalId, userName);
				_logger.LogDebug("AdvancedUrlRewriter_VanityUrl(testFields={TestFields}): user → {User}", testFields, user);
                if (user != null)
                {
                    user.VanityUrl = vanityUrl;
					_logger.LogDebug("AdvancedUrlRewriter_VanityUrl(testFields={TestFields}): user.VanityUrl → {Arg1}", testFields, user.VanityUrl);
                    UserController.UpdateUser(this.PortalId, user);
                }
            }

            if (!string.IsNullOrEmpty(redirectOld))
            {
                settings.RedirectOldProfileUrl = Convert.ToBoolean(redirectOld);
            }

            this.ExecuteTest(settings, testFields, true);
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_VanityUrl(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_SecureRedirectTestCases))]
        public void AdvancedUrlRewriter_SecureRedirect(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_SecureRedirect(testFields={TestFields})", testFields);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);
            var isClient = Convert.ToBoolean(testFields["Client"]);
			_logger.LogDebug("AdvancedUrlRewriter_SecureRedirect(testFields={TestFields}): isClient → {IsClient}", testFields, isClient);
			_logger.LogDebug("AdvancedUrlRewriter_SecureRedirect(testFields={TestFields}): settings → {Settings}", testFields, settings);

            this.securePageName = testFields["Page Name"].Trim();

            PortalController.UpdatePortalSetting(this.PortalId, "SSLEnforced", testFields["Enforced"].Trim(), true, "en-us");
            PortalController.UpdatePortalSetting(this.PortalId, "SSLSetup", testFields["Enabled"].Trim(), true, "en-us");

            var isSecure = Convert.ToBoolean(testFields["IsSecure"].Trim());
			_logger.LogDebug("AdvancedUrlRewriter_SecureRedirect(testFields={TestFields}): isSecure → {IsSecure}", testFields, isSecure);

            if (isSecure)
            {
                var tab = TabController.Instance.GetTabByName(this.securePageName, this.PortalId);
				_logger.LogDebug("AdvancedUrlRewriter_SecureRedirect(testFields={TestFields}): tab → {Tab}", testFields, tab);
				_logger.LogDebug("AdvancedUrlRewriter_SecureRedirect(testFields={TestFields}): tab.IsSecure → {Arg1}", testFields, tab.IsSecure);
                tab.IsSecure = true;

                this.UpdateTab(tab);
            }

            settings.SSLClientRedirect = isClient;
			_logger.LogDebug("AdvancedUrlRewriter_SecureRedirect(testFields={TestFields}): settings.SSLClientRedirect → {Arg1}", testFields, settings.SSLClientRedirect);

            this.ExecuteTest(settings, testFields, true);
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_SecureRedirect(testFields={TestFields})", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.UrlRewrite_JiraTests))]
        public void AdvancedUrlRewriter_JiraTests(Dictionary<string, string> testFields)
        {
			_logger.LogDebug(">>> Entering AdvancedUrlRewriter_JiraTests(testFields={TestFields})", testFields);
            var testName = testFields.GetValue("Test File", string.Empty);

			_logger.LogDebug("AdvancedUrlRewriter_JiraTests(testFields={TestFields}): testName → {TestName}", testFields, testName);
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", "Jira_Tests", testName + ".csv", this.PortalId);
			_logger.LogDebug("AdvancedUrlRewriter_JiraTests(testFields={TestFields}): settings → {Settings}", testFields, settings);
            var dictionary = UrlTestFactoryClass.GetDictionary("UrlRewrite", "Jira_Tests", testName + "_dic.csv");
			_logger.LogDebug("AdvancedUrlRewriter_JiraTests(testFields={TestFields}): dictionary → {Dictionary}", testFields, dictionary);

            var homeTabId = -1;
			_logger.LogDebug("AdvancedUrlRewriter_JiraTests(testFields={TestFields}): homeTabId → {HomeTabId}", testFields, homeTabId);
            foreach (var keyValuePair in dictionary)
            {
                switch (keyValuePair.Key)
                {
                    case "HomeTabId":
                        homeTabId = this.UpdateHomeTab(int.Parse(keyValuePair.Value));
                        break;
                    default:
                        break;
                }
            }

            this.ExecuteTest(settings, testFields, true);

            if (homeTabId != -1)
            {
                this.UpdateHomeTab(homeTabId);
            }
			_logger.LogDebug("<<< Exiting AdvancedUrlRewriter_JiraTests(testFields={TestFields})", testFields);
        }

        private void CreateSimulatedRequest(Uri url)
        {
            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", this.WebsitePhysicalAppPath);
            simulator.SimulateRequest(url);

            var browserCaps = new HttpBrowserCapabilities { Capabilities = new Hashtable() };
            HttpContext.Current.Request.Browser = browserCaps;
			_logger.LogDebug("<<< Exiting CreateSimulatedRequest(url={Url})", url);
        }

        private void ProcessRequest(FriendlyUrlSettings settings, UrlTestHelper testHelper)
        {
            var provider = new AdvancedUrlRewriter();

            provider.ProcessTestRequestWithContext(
                HttpContext.Current,
                HttpContext.Current.Request.Url,
                true,
                testHelper.Result,
                settings);
            testHelper.Response = HttpContext.Current.Response;
			_logger.LogDebug("<<< Exiting ProcessRequest(settings={Settings},testHelper={TestHelper})", settings, testHelper);
        }

        private string ReplaceTokens(Dictionary<string, string> testFields, string url, string theTabId)
        {
            var defaultAlias = testFields.GetValue("DefaultAlias", string.Empty);
            var httpAlias = testFields.GetValue("Alias", string.Empty);
            var tabName = testFields["Page Name"];
            var vanityUrl = testFields.GetValue("VanityUrl", string.Empty);
            var homeTabId = testFields.GetValue("HomeTabId", string.Empty);

            var userName = testFields.GetValue("UserName", string.Empty);
            var userId = string.Empty;
            if (!string.IsNullOrEmpty(userName))
            {
                var user = UserController.GetUserByName(this.PortalId, userName);
                if (user != null)
                {
                    userId = user.UserID.ToString();
                }
            }

            return url.Replace("{alias}", httpAlias)
                            .Replace("{usealias}", defaultAlias)
                            .Replace("{tabName}", tabName)
                            .Replace("{tabId}", theTabId)
                            .Replace("{portalId}", this.PortalId.ToString())
                            .Replace("{vanityUrl}", vanityUrl)
                            .Replace("{userId}", userId)
                            .Replace("{defaultPage}", DefaultPage);
        }

        private void DeleteTab(string tabName)
        {
            var tab = TabController.Instance.GetTabByName(tabName, this.PortalId);

            if (tab != null)
            {
                TabController.Instance.DeleteTab(tab.TabID, this.PortalId);
            }
			_logger.LogDebug("<<< Exiting DeleteTab(tabName={TabName})", tabName);
        }

        private void ExecuteTestForTab(TabInfo tab, FriendlyUrlSettings settings, Dictionary<string, string> testFields)
        {
            var httpAlias = testFields.GetValue("Alias", string.Empty);
            var scheme = testFields["Scheme"];
            var url = testFields["Test Url"];
            var result = testFields["Expected Url"];
            var expectedStatus = int.Parse(testFields["Status"]);
            var redirectUrl = testFields.GetValue("Final Url");
            var redirectReason = testFields.GetValue("RedirectReason");

            var theTabId = (tab == null) ? "-1" : tab.TabID.ToString();

            var expectedResult = this.ReplaceTokens(testFields, result, theTabId);
            var testUrl = this.ReplaceTokens(testFields, url, theTabId);
            var expectedRedirectUrl = this.ReplaceTokens(testFields, redirectUrl, theTabId);

            this.CreateSimulatedRequest(new Uri(testUrl));

            var request = HttpContext.Current.Request;
            var testHelper = new UrlTestHelper
            {
                HttpAliasFull = scheme + httpAlias + "/",
                Result = new UrlAction(request)
                {
                    IsSecureConnection = request.IsSecureConnection,
                    RawUrl = request.RawUrl,
                },
                RequestUri = new Uri(testUrl),
                QueryStringCol = new NameValueCollection(),
            };

            this.ProcessRequest(settings, testHelper);

            // Test expected response status
            Assert.That(testHelper.Response.StatusCode, Is.EqualTo(expectedStatus));

            switch (expectedStatus)
            {
                case 200:
                    // Test expected rewrite path
                    if (!string.IsNullOrEmpty(expectedResult))
                    {
                        Assert.That(testHelper.Result.RewritePath.TrimStart('/'), Is.EqualTo(expectedResult));
                    }

                    break;
                case 301:
                case 302:
                    // Test for final Url if redirected
                    Assert.That(testHelper.Result.FinalUrl.TrimStart('/'), Is.EqualTo(expectedRedirectUrl).IgnoreCase);
                    Assert.That(testHelper.Result.Reason.ToString(), Is.EqualTo(redirectReason), "Redirect reason incorrect");
                    break;
            }
			_logger.LogDebug("<<< Exiting ExecuteTestForTab(tab={Tab},settings={Settings},testFields={TestFields})", tab, settings, testFields);
        }

        private void ExecuteTest(FriendlyUrlSettings settings, Dictionary<string, string> testFields, bool setDefaultAlias)
        {
            var tabName = testFields["Page Name"];
            var tab = TabController.Instance.GetTabByName(tabName, this.PortalId);

            if (setDefaultAlias)
            {
                this.SetDefaultAlias(testFields);
            }

            this.ExecuteTestForTab(tab, settings, testFields);
        }

        private void UpdateTab(TabInfo tab)
        {
            if (tab != null)
            {
                TabController.Instance.UpdateTab(tab);
            }
			_logger.LogDebug("<<< Exiting UpdateTab(tab={Tab})", tab);
        }

        private void UpdateTabName(int theTabId, string newName)
        {
            var tab = TabController.Instance.GetTab(theTabId, this.PortalId, false);
            tab.TabName = newName;
            TabController.Instance.UpdateTab(tab);
			_logger.LogDebug("<<< Exiting UpdateTabName(theTabId={TheTabId},newName={NewName})", theTabId, newName);
        }

        private void UpdateTabSkin(int theTabId, string newSkin)
        {
            var tab = TabController.Instance.GetTab(theTabId, this.PortalId, false);
            tab.SkinSrc = newSkin;
            TabController.Instance.UpdateTab(tab);
			_logger.LogDebug("<<< Exiting UpdateTabSkin(theTabId={TheTabId},newSkin={NewSkin})", theTabId, newSkin);
        }

        private int UpdateHomeTab(int homeTabId)
        {
            var portalInfo = PortalController.Instance.GetPortal(this.PortalId);
            var oldHomeTabId = portalInfo.HomeTabId;
            portalInfo.HomeTabId = homeTabId;

            return oldHomeTabId;
        }

        private void UpdateTabSetting(string tabName, string settingName, string settingValue)
        {
            var tab = TabController.Instance.GetTabByName(tabName, this.PortalId);
            tab.TabSettings[settingName] = settingValue;
            TabController.Instance.UpdateTab(tab);
			_logger.LogDebug("<<< Exiting UpdateTabSetting(tabName={TabName},settingName={SettingName},settingValue={SettingValue})", tabName, settingName, settingValue);
        }
    }
}
