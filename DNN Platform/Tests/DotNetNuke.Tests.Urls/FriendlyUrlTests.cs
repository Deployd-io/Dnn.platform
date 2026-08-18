// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Urls;

using System;
using System.Collections.Generic;
using System.Globalization;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using NUnit.Framework;
using Microsoft.Extensions.Logging;

[TestFixture]
public class FriendlyUrlTests : UrlTestBase
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    private const string _defaultPage = Globals.glbDefaultPage;
    private const string _aboutUsPageName = "About Us";
    private int _tabId;
    private string _redirectMode;
    private Locale _customLocale;
    private PortalAliasInfo _primaryAlias;

    [SetUp]
    public override void SetUp()
    {
		_logger.LogDebug(">>> Entering SetUp()");
        base.SetUp();

        this.UpdateTabName(this._tabId, "About Us");
        CacheController.FlushPageIndexFromCache();
        this.GetDefaultAlias();
        this._redirectMode = PortalController.GetPortalSetting("PortalAliasMapping", this.PortalId, "CANONICALURL");
        this._primaryAlias = null;
		_logger.LogDebug("SetUp(): _primaryAlias → {_primaryAlias}", _primaryAlias);
		_logger.LogDebug("SetUp(): this._primaryAlias → {Arg0}", this._primaryAlias);
        this._customLocale = null;
		_logger.LogDebug("SetUp(): _customLocale → {_customLocale}", _customLocale);
		_logger.LogDebug("<<< Exiting SetUp()");
    }

    [OneTimeSetUp]
    public override void OneTimeSetUp()
    {
		_logger.LogDebug(">>> Entering OneTimeSetUp()");
        base.OneTimeSetUp();

        var tab = TabController.Instance.GetTabByName(_aboutUsPageName, this.PortalId);
		_logger.LogDebug("OneTimeSetUp(): tab → {Tab}", tab);
        if (tab == null)
        {
            this.CreateTab(_aboutUsPageName);
            tab = TabController.Instance.GetTabByName(_aboutUsPageName, this.PortalId);
        }

        this._tabId = tab.TabID;
		_logger.LogDebug("OneTimeSetUp(): _tabId → {_tabId}", _tabId);
		_logger.LogDebug("OneTimeSetUp(): this._tabId → {Arg0}", this._tabId);

        // Add Portal Aliases
        var aliasController = PortalAliasController.Instance;
        TestUtil.ReadStream("Aliases", (line, header) =>
        {
            var fields = line.Split(',');
			_logger.LogDebug("OneTimeSetUp(): fields → {Fields}", fields);
            var alias = aliasController.GetPortalAlias(fields[0], this.PortalId);
			_logger.LogDebug("OneTimeSetUp(): alias → {Alias}", alias);
            if (alias == null)
            {
                alias = new PortalAliasInfo
                {
                    HTTPAlias = fields[0],
                    PortalID = this.PortalId,
                };
                PortalAliasController.Instance.AddPortalAlias(alias);
            }
        });
        TestUtil.ReadStream("Users", (line, header) =>
        {
            var fields = line.Split(',');
            TestUtil.AddUser(this.PortalId, fields[0].Trim(), fields[1].Trim(), fields[2].Trim());
        });
		_logger.LogDebug("<<< Exiting OneTimeSetUp()");
    }

    [TearDown]
    public override void TearDown()
    {
		_logger.LogDebug(">>> Entering TearDown()");
        base.TearDown();

        this.UpdateTabName(this._tabId, "About Us");

        if (this._customLocale != null)
        {
            Localization.RemoveLanguageFromPortals(this._customLocale.LanguageId, true);
            Localization.DeleteLanguage(this._customLocale, true);
        }

        if (this._primaryAlias != null)
        {
            PortalAliasController.Instance.DeletePortalAlias(this._primaryAlias);
        }

        this.SetDefaultAlias(this.DefaultAlias);
        PortalController.UpdatePortalSetting(this.PortalId, "PortalAliasMapping", this._redirectMode);

        foreach (var tabUrl in CBO.FillCollection<TabUrlInfo>(DataProvider.Instance().GetTabUrls(this.PortalId)))
        {
            TabController.Instance.DeleteTabUrl(tabUrl, this.PortalId, true);
        }
		_logger.LogDebug("<<< Exiting TearDown()");
    }

    [OneTimeTearDown]
    public override void OneTimeTearDown()
    {
		_logger.LogDebug(">>> Entering OneTimeTearDown()");
        base.OneTimeTearDown();

        var aliasController = PortalAliasController.Instance;
        TestUtil.ReadStream("Aliases", (line, header) =>
        {
            var fields = line.Split(',');
			_logger.LogDebug("OneTimeTearDown(): fields → {Fields}", fields);
            var alias = aliasController.GetPortalAlias(fields[0], this.PortalId);
			_logger.LogDebug("OneTimeTearDown(): alias → {Alias}", alias);
            PortalAliasController.Instance.DeletePortalAlias(alias);
        });
        TestUtil.ReadStream("Users", (line, header) =>
        {
            var fields = line.Split(',');

            TestUtil.DeleteUser(this.PortalId, fields[0]);
        });
		_logger.LogDebug("<<< Exiting OneTimeTearDown()");
    }

    [Test]
    [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.FriendlyUrl_BaseTestCases))]
    public void AdvancedUrlProvider_BaseFriendlyUrl(Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering AdvancedUrlProvider_BaseFriendlyUrl(testFields={TestFields})", testFields);
        this.ExecuteTest("Base", testFields);
		_logger.LogDebug("<<< Exiting AdvancedUrlProvider_BaseFriendlyUrl(testFields={TestFields})", testFields);
    }

    [Test]
    [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.FriendlyUrl_ImprovedTestCases))]
    public void AdvancedUrlProvider_ImprovedFriendlyUrl(Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering AdvancedUrlProvider_ImprovedFriendlyUrl(testFields={TestFields})", testFields);
        this.ExecuteTest("Improved", testFields);
		_logger.LogDebug("<<< Exiting AdvancedUrlProvider_ImprovedFriendlyUrl(testFields={TestFields})", testFields);
    }

    [Test]
    [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.FriendlyUrl_SpaceEncodingTestCases))]
    public void AdvancedUrlProvider_SpaceEncoding(Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering AdvancedUrlProvider_SpaceEncoding(testFields={TestFields})", testFields);
        var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", "SpaceEncoding", this.PortalId);
        settings.ReplaceSpaceWith = " ";
		_logger.LogDebug("AdvancedUrlProvider_SpaceEncoding(testFields={TestFields}): settings.ReplaceSpaceWith → {Arg1}", testFields, settings.ReplaceSpaceWith);
		_logger.LogDebug("AdvancedUrlProvider_SpaceEncoding(testFields={TestFields}): settings → {Settings}", testFields, settings);

        string spaceEncoding = testFields.GetValue("SpaceEncoding");

        if (!string.IsNullOrEmpty(spaceEncoding))
        {
            settings.SpaceEncodingValue = spaceEncoding;
			_logger.LogDebug("AdvancedUrlProvider_SpaceEncoding(testFields={TestFields}): settings.SpaceEncodingValue → {Arg1}", testFields, settings.SpaceEncodingValue);
        }

        this.ExecuteTest("Improved", settings, testFields);
		_logger.LogDebug("<<< Exiting AdvancedUrlProvider_SpaceEncoding(testFields={TestFields})", testFields);
    }

    [Test]
    [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.FriendlyUrl_PageExtensionTestCases))]
    public void AdvancedUrlProvider_PageExtension(Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering AdvancedUrlProvider_PageExtension(testFields={TestFields})", testFields);
        var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", "PageExtension", this.PortalId);

		_logger.LogDebug("AdvancedUrlProvider_PageExtension(testFields={TestFields}): settings → {Settings}", testFields, settings);
        string pageExtensionUsageType = testFields.GetValue("PageExtensionUsageType");
        string pageExtension = testFields.GetValue("PageExtension");

        if (!string.IsNullOrEmpty(pageExtension))
        {
            settings.PageExtension = pageExtension;
			_logger.LogDebug("AdvancedUrlProvider_PageExtension(testFields={TestFields}): settings.PageExtension → {Arg1}", testFields, settings.PageExtension);
        }

        switch (pageExtensionUsageType)
        {
            case "AlwaysUse":
                settings.PageExtensionUsageType = PageExtensionUsageType.AlwaysUse;
				_logger.LogDebug("AdvancedUrlProvider_PageExtension(testFields={TestFields}): settings.PageExtensionUsageType → {Arg1}", testFields, settings.PageExtensionUsageType);
                break;
            case "PageOnly":
                settings.PageExtensionUsageType = PageExtensionUsageType.PageOnly;
                break;
            case "Never":
                settings.PageExtensionUsageType = PageExtensionUsageType.Never;
                break;
        }

        this.ExecuteTest("Improved", settings, testFields);
		_logger.LogDebug("<<< Exiting AdvancedUrlProvider_PageExtension(testFields={TestFields})", testFields);
    }

    [Test]
    [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.FriendlyUrl_PrimaryPortalAliasTestCases))]
    public void AdvancedUrlProvider_PrimaryPortalAlias(Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering AdvancedUrlProvider_PrimaryPortalAlias(testFields={TestFields})", testFields);
        string defaultAlias = testFields["DefaultAlias"];
        string redirectMode = testFields["RedirectMode"];
		_logger.LogDebug("AdvancedUrlProvider_PrimaryPortalAlias(testFields={TestFields}): defaultAlias → {DefaultAlias}", testFields, defaultAlias);

        var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", "PrimaryPortalAlias", this.PortalId);
		_logger.LogDebug("AdvancedUrlProvider_PrimaryPortalAlias(testFields={TestFields}): settings → {Settings}", testFields, settings);

        string language = testFields["Language"].Trim();
        if (!string.IsNullOrEmpty(language))
        {
            this._customLocale = new Locale { Code = language, Fallback = "en-US" };
			_logger.LogDebug("AdvancedUrlProvider_PrimaryPortalAlias(testFields={TestFields}): _customLocale → {_customLocale}", testFields, _customLocale);
			_logger.LogDebug("AdvancedUrlProvider_PrimaryPortalAlias(testFields={TestFields}): this._customLocale → {Arg1}", testFields, this._customLocale);
            this._customLocale.Text = CultureInfo.GetCultureInfo(this._customLocale.Code).NativeName;
            Localization.SaveLanguage(this._customLocale);
            Localization.AddLanguageToPortals(this._customLocale.LanguageId);

            // add new primary alias
            this._primaryAlias = new PortalAliasInfo
			_logger.LogDebug("AdvancedUrlProvider_PrimaryPortalAlias(testFields={TestFields}): _primaryAlias → {_primaryAlias}", testFields, _primaryAlias);
			_logger.LogDebug("AdvancedUrlProvider_PrimaryPortalAlias(testFields={TestFields}): this._primaryAlias → {Arg1}", testFields, this._primaryAlias);
            {
                PortalID = this.PortalId,
                HTTPAlias = defaultAlias,
                CultureCode = language,
                IsPrimary = true,
            };
            this._primaryAlias.PortalAliasID = PortalAliasController.Instance.AddPortalAlias(this._primaryAlias);
        }
        else
        {
            this.SetDefaultAlias(defaultAlias);
        }

        PortalController.UpdatePortalSetting(this.PortalId, "PortalAliasMapping", redirectMode);

        this.ExecuteTest("Improved", settings, testFields);
		_logger.LogDebug("<<< Exiting AdvancedUrlProvider_PrimaryPortalAlias(testFields={TestFields})", testFields);
    }

    [Test]
    [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.FriendlyUrl_RegexTestCases))]
    public void AdvancedUrlProvider_Regex(Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering AdvancedUrlProvider_Regex(testFields={TestFields})", testFields);
        var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

		_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings → {Settings}", testFields, settings);
        string regexSetting = testFields["Setting"];
        string regexValue = testFields["Value"];
        if (!string.IsNullOrEmpty(regexValue))
        {
            switch (regexSetting)
            {
                case "IgnoreRegex":
                    settings.IgnoreRegex = regexValue;
					_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings.IgnoreRegex → {Arg1}", testFields, settings.IgnoreRegex);
                    break;
                case "DoNotRewriteRegex":
                    settings.DoNotRewriteRegex = regexValue;
					_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings.DoNotRewriteRegex → {Arg1}", testFields, settings.DoNotRewriteRegex);
                    break;
                case "UseSiteUrlsRegex":
                    settings.UseSiteUrlsRegex = regexValue;
					_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings.UseSiteUrlsRegex → {Arg1}", testFields, settings.UseSiteUrlsRegex);
                    break;
                case "DoNotRedirectRegex":
                    settings.DoNotRedirectRegex = regexValue;
					_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings.DoNotRedirectRegex → {Arg1}", testFields, settings.DoNotRedirectRegex);
                    break;
                case "DoNotRedirectSecureRegex":
                    settings.DoNotRedirectSecureRegex = regexValue;
					_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings.DoNotRedirectSecureRegex → {Arg1}", testFields, settings.DoNotRedirectSecureRegex);
                    break;
                case "ForceLowerCaseRegex":
                    settings.ForceLowerCaseRegex = regexValue;
					_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings.ForceLowerCaseRegex → {Arg1}", testFields, settings.ForceLowerCaseRegex);
                    break;
                case "NoFriendlyUrlRegex":
                    settings.NoFriendlyUrlRegex = regexValue;
					_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings.NoFriendlyUrlRegex → {Arg1}", testFields, settings.NoFriendlyUrlRegex);
                    break;
                case "DoNotIncludeInPathRegex":
                    settings.DoNotIncludeInPathRegex = regexValue;
					_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings.DoNotIncludeInPathRegex → {Arg1}", testFields, settings.DoNotIncludeInPathRegex);
                    break;
                case "ValidExtensionlessUrlsRegex":
                    settings.ValidExtensionlessUrlsRegex = regexValue;
					_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings.ValidExtensionlessUrlsRegex → {Arg1}", testFields, settings.ValidExtensionlessUrlsRegex);
                    break;
                case "RegexMatch":
                    settings.RegexMatch = regexValue;
					_logger.LogDebug("AdvancedUrlProvider_Regex(testFields={TestFields}): settings.RegexMatch → {Arg1}", testFields, settings.RegexMatch);
                    break;
            }
        }

        this.ExecuteTest("Improved", settings, testFields);
		_logger.LogDebug("<<< Exiting AdvancedUrlProvider_Regex(testFields={TestFields})", testFields);
    }

    [Test]
    [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.FriendlyUrl_ReplaceCharsTestCases))]
    public void AdvancedUrlProvider_ReplaceChars(Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering AdvancedUrlProvider_ReplaceChars(testFields={TestFields})", testFields);
        var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

		_logger.LogDebug("AdvancedUrlProvider_ReplaceChars(testFields={TestFields}): settings → {Settings}", testFields, settings);
        string testPageName = testFields.GetValue("TestPageName");
        TabInfo tab = null;
		_logger.LogDebug("AdvancedUrlProvider_ReplaceChars(testFields={TestFields}): tab → {Tab}", testFields, tab);
        if (!string.IsNullOrEmpty(testPageName))
        {
            var tabName = testFields["Page Name"];
			_logger.LogDebug("AdvancedUrlProvider_ReplaceChars(testFields={TestFields}): tabName → {TabName}", testFields, tabName);
            tab = TabController.Instance.GetTabByName(tabName, this.PortalId);
            tab.TabName = testPageName;
			_logger.LogDebug("AdvancedUrlProvider_ReplaceChars(testFields={TestFields}): tab.TabName → {Arg1}", testFields, tab.TabName);
            TabController.Instance.UpdateTab(tab);

            // Refetch tab from DB
            tab = TabController.Instance.GetTab(tab.TabID, tab.PortalID, false);
        }

        string autoAscii = testFields.GetValue("AutoAscii");

        if (!string.IsNullOrEmpty(autoAscii))
        {
            settings.AutoAsciiConvert = Convert.ToBoolean(autoAscii);
        }

        TestUtil.GetReplaceCharDictionary(testFields, settings.ReplaceCharacterDictionary);

        this.ExecuteTestForTab("Improved", tab, settings, testFields);
		_logger.LogDebug("<<< Exiting AdvancedUrlProvider_ReplaceChars(testFields={TestFields})", testFields);
    }

    [Test]
    [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.FriendlyUrl_ReplaceSpaceTestCases))]
    public void AdvancedUrlProvider_ReplaceSpace(Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering AdvancedUrlProvider_ReplaceSpace(testFields={TestFields})", testFields);
        var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

		_logger.LogDebug("AdvancedUrlProvider_ReplaceSpace(testFields={TestFields}): settings → {Settings}", testFields, settings);
        string replaceSpaceWith = testFields.GetValue("ReplaceSpaceWith");
        if (!string.IsNullOrEmpty(replaceSpaceWith))
        {
            settings.ReplaceSpaceWith = replaceSpaceWith;
			_logger.LogDebug("AdvancedUrlProvider_ReplaceSpace(testFields={TestFields}): settings.ReplaceSpaceWith → {Arg1}", testFields, settings.ReplaceSpaceWith);
        }

        this.ExecuteTest("Improved", settings, testFields);
		_logger.LogDebug("<<< Exiting AdvancedUrlProvider_ReplaceSpace(testFields={TestFields})", testFields);
    }

    [Test]
    [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.FriendlyUrl_VanityUrlTestCases))]
    public void AdvancedUrlProvider_VanityUrl(Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering AdvancedUrlProvider_VanityUrl(testFields={TestFields})", testFields);
        var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

		_logger.LogDebug("AdvancedUrlProvider_VanityUrl(testFields={TestFields}): settings → {Settings}", testFields, settings);
        var vanityUrl = testFields.GetValue("VanityUrl", string.Empty);
		_logger.LogDebug("AdvancedUrlProvider_VanityUrl(testFields={TestFields}): vanityUrl → {VanityUrl}", testFields, vanityUrl);
        var userName = testFields.GetValue("UserName", string.Empty);
		_logger.LogDebug("AdvancedUrlProvider_VanityUrl(testFields={TestFields}): userName → {UserName}", testFields, userName);
        var vanityUrlPrefix = testFields.GetValue("VanityUrlPrefix", string.Empty);
		_logger.LogDebug("AdvancedUrlProvider_VanityUrl(testFields={TestFields}): vanityUrlPrefix → {VanityUrlPrefix}", testFields, vanityUrlPrefix);
        if (!string.IsNullOrEmpty(vanityUrlPrefix))
        {
            settings.VanityUrlPrefix = vanityUrlPrefix;
			_logger.LogDebug("AdvancedUrlProvider_VanityUrl(testFields={TestFields}): settings.VanityUrlPrefix → {Arg1}", testFields, settings.VanityUrlPrefix);
        }

        if (!string.IsNullOrEmpty(userName))
        {
            var user = UserController.GetUserByName(this.PortalId, userName);
			_logger.LogDebug("AdvancedUrlProvider_VanityUrl(testFields={TestFields}): user → {User}", testFields, user);
            if (user != null)
            {
                user.VanityUrl = vanityUrl;
				_logger.LogDebug("AdvancedUrlProvider_VanityUrl(testFields={TestFields}): user.VanityUrl → {Arg1}", testFields, user.VanityUrl);
                UserController.UpdateUser(this.PortalId, user);
            }
        }

        this.ExecuteTest("Improved", settings, testFields);
		_logger.LogDebug("<<< Exiting AdvancedUrlProvider_VanityUrl(testFields={TestFields})", testFields);
    }

    [Test]
    [TestCaseSource(typeof(UrlTestFactoryClass), nameof(UrlTestFactoryClass.FriendlyUrl_ForceLowerCaseTestCases))]
    public void AdvancedUrlProvider_ForceLowerCase(Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering AdvancedUrlProvider_ForceLowerCase(testFields={TestFields})", testFields);
        var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

		_logger.LogDebug("AdvancedUrlProvider_ForceLowerCase(testFields={TestFields}): settings → {Settings}", testFields, settings);
        string forceLowerCaseRegex = testFields.GetValue("ForceLowerCaseRegex");

        if (!string.IsNullOrEmpty(forceLowerCaseRegex))
        {
            settings.ForceLowerCaseRegex = forceLowerCaseRegex;
			_logger.LogDebug("AdvancedUrlProvider_ForceLowerCase(testFields={TestFields}): settings.ForceLowerCaseRegex → {Arg1}", testFields, settings.ForceLowerCaseRegex);
        }

        this.ExecuteTest("Improved", settings, testFields);
		_logger.LogDebug("<<< Exiting AdvancedUrlProvider_ForceLowerCase(testFields={TestFields})", testFields);
    }

    private void ExecuteTest(string test, Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering ExecuteTest(test={Test},testFields={TestFields})", test, testFields);
        var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

		_logger.LogDebug("ExecuteTest(test={Test},testFields={TestFields}): settings → {Settings}", test, testFields, settings);
        this.SetDefaultAlias(testFields);

        this.ExecuteTest(test, settings, testFields);
		_logger.LogDebug("<<< Exiting ExecuteTest(test={Test},testFields={TestFields})", test, testFields);
    }

    private void ExecuteTest(string test, FriendlyUrlSettings settings, Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering ExecuteTest(test={Test},settings={Settings},testFields={TestFields})", test, settings, testFields);
        var tabName = testFields["Page Name"];
        var tab = TabController.Instance.GetTabByName(tabName, this.PortalId);
		_logger.LogDebug("ExecuteTest(test={Test},settings={Settings},testFields={TestFields}): tab → {Tab}", test, settings, testFields, tab);
		_logger.LogDebug("ExecuteTest(test={Test},settings={Settings},testFields={TestFields}): tabName → {TabName}", test, settings, testFields, tabName);
        if (tab == null)
        {
			_logger.LogDebug("<<< Exiting ExecuteTest(test={Test},testFields={TestFields})", test, testFields);
            Assert.Fail($"TAB with name [{tabName}] is not found!");
        }

        this.ExecuteTestForTab(test, tab, settings, testFields);
    }

    private void ExecuteTestForTab(string test, TabInfo tab, FriendlyUrlSettings settings, Dictionary<string, string> testFields)
    {
		_logger.LogDebug(">>> Entering ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields})", test, tab, settings, testFields);
        var httpAlias = testFields["Alias"];
        var defaultAlias = testFields.GetValue("DefaultAlias", string.Empty);
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): defaultAlias → {DefaultAlias}", test, tab, settings, testFields, defaultAlias);
        var tabName = testFields["Page Name"];
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): tabName → {TabName}", test, tab, settings, testFields, tabName);
        var scheme = testFields["Scheme"];
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): scheme → {Scheme}", test, tab, settings, testFields, scheme);
        var parameters = testFields["Params"];
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): parameters → {Parameters}", test, tab, settings, testFields, parameters);
        var result = testFields["Expected Url"];
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): result → {Result}", test, tab, settings, testFields, result);
        var customPage = testFields.GetValue("Custom Page Name", _defaultPage);
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): customPage → {CustomPage}", test, tab, settings, testFields, customPage);
        string vanityUrl = testFields.GetValue("VanityUrl", string.Empty);
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): vanityUrl → {VanityUrl}", test, tab, settings, testFields, vanityUrl);
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): httpAlias → {HttpAlias}", test, tab, settings, testFields, httpAlias);

        var httpAliasFull = scheme + httpAlias + "/";
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): httpAliasFull → {HttpAliasFull}", test, tab, settings, testFields, httpAliasFull);
        var expectedResult = result.Replace("{alias}", httpAlias)
            .Replace("{usealias}", defaultAlias)
            .Replace("{tabName}", tabName)
            .Replace("{tabId}", tab.TabID.ToString())
            .Replace("{vanityUrl}", vanityUrl)
            .Replace("{defaultPage}", _defaultPage);
			_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): expectedResult → {ExpectedResult}", test, tab, settings, testFields, expectedResult);

        if (!string.IsNullOrEmpty(parameters) && !parameters.StartsWith("&", StringComparison.Ordinal))
        {
            parameters = "&" + parameters;
        }

        var userName = testFields.GetValue("UserName", string.Empty);
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): userName → {UserName}", test, tab, settings, testFields, userName);
        if (!string.IsNullOrEmpty(userName))
        {
            var user = UserController.GetUserByName(this.PortalId, userName);
			_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): user → {User}", test, tab, settings, testFields, user);
            if (user != null)
            {
                expectedResult = expectedResult.Replace("{userId}", user.UserID.ToString());
                parameters = parameters.Replace("{userId}", user.UserID.ToString());
            }
        }

        var baseUrl = httpAliasFull + "Default.aspx?TabId=" + tab.TabID + parameters;
		_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): baseUrl → {BaseUrl}", test, tab, settings, testFields, baseUrl);
        string testUrl;
        if (test == "Base")
        {
            testUrl = AdvancedFriendlyUrlProvider.BaseFriendlyUrl(
                tab,
                baseUrl,
                customPage,
                httpAlias,
                settings);
				_logger.LogDebug("ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields}): testUrl → {TestUrl}", test, tab, settings, testFields, testUrl);
        }
        else
        {
            testUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                tab,
                baseUrl,
                customPage,
                httpAlias,
                true,
                settings,
                Guid.Empty);
        }

        Assert.That(testUrl, Is.EqualTo(expectedResult).IgnoreCase);
		_logger.LogDebug("<<< Exiting ExecuteTestForTab(test={Test},tab={Tab},settings={Settings},testFields={TestFields})", test, tab, settings, testFields);
    }

    private void UpdateTabName(int tabId, string newName)
    {
		_logger.LogDebug(">>> Entering UpdateTabName(tabId={TabId},newName={NewName})", tabId, newName);
        var tab = TabController.Instance.GetTab(tabId, this.PortalId, false);
        tab.TabName = newName;
		_logger.LogDebug("UpdateTabName(tabId={TabId},newName={NewName}): tab.TabName → {Arg2}", tabId, newName, tab.TabName);
		_logger.LogDebug("UpdateTabName(tabId={TabId},newName={NewName}): tab → {Tab}", tabId, newName, tab);
        TabController.Instance.UpdateTab(tab);
		_logger.LogDebug("<<< Exiting UpdateTabName(tabId={TabId},newName={NewName})", tabId, newName);
    }
}
