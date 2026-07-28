// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Common;

using System;
using System.Collections.Generic;
using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Abstractions.Settings;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cryptography;
using DotNetNuke.Tests.Utilities.Fakes;
using DotNetNuke.Tests.Utilities.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

using ICryptographyProvider = DotNetNuke.Abstractions.Security.ICryptographyProvider;

[TestFixture]
public class UrlUtilsTests
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    private static readonly Dictionary<string, IConfigurationSetting> HostSettings = new Dictionary<string, IConfigurationSetting>();

    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
		Logger.LogDebug(">>> Entering OneTimeSetUp()");
        ComponentFactory.RegisterComponent<CryptographyProvider, CoreCryptographyProvider>();
        MockComponentProvider.CreateDataCacheProvider();
        var hostController = new FakeHostController(HostSettings);

		Logger.LogDebug("OneTimeSetUp(): hostController → {HostController}", hostController);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Mock.Of<IApplicationStatusInfo>());
		Logger.LogDebug("OneTimeSetUp(): serviceCollection → {ServiceCollection}", serviceCollection);
        serviceCollection.AddSingleton(Mock.Of<INavigationManager>());
        serviceCollection.AddSingleton(Mock.Of<IPortalSettingsController>());
        serviceCollection.AddSingleton(new ListController(Mock.Of<IEventLogger>(), Mock.Of<IHostSettings>()));
        serviceCollection.AddSingleton(Mock.Of<IPortalController>());
        serviceCollection.AddSingleton(Mock.Of<IPortalGroupController>());
        serviceCollection.AddSingleton<ICryptographyProvider>(new CoreCryptographyProvider());
        serviceCollection.AddSingleton<IHostSettingsService>(hostController);
        serviceCollection.AddSingleton<IHostSettings>(new HostSettings(hostController));

        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
		Logger.LogDebug("<<< Exiting OneTimeSetUp()");
    }

    [OneTimeTearDown]
    public static void OneTimeTearDown()
    {
		Logger.LogDebug(">>> Entering OneTimeTearDown()");
        if (Globals.DependencyProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
		Logger.LogDebug("<<< Exiting OneTimeTearDown()");
    }

    [Test]
    public void CombineEmptyBase()
    {
		Logger.LogDebug(">>> Entering CombineEmptyBase()");
        var result = UrlUtils.Combine(string.Empty, "a/b/c");
        Assert.That(result, Is.EqualTo("a/b/c"));
		Logger.LogDebug("<<< Exiting CombineEmptyBase()");
		Logger.LogDebug("CombineEmptyBase(): result → {Result}", result);
    }

    [Test]
    public void CombineEmptyRelative()
    {
		Logger.LogDebug(">>> Entering CombineEmptyRelative()");
        var result = UrlUtils.Combine("/a/b/c", string.Empty);
        Assert.That(result, Is.EqualTo("/a/b/c"));
		Logger.LogDebug("<<< Exiting CombineEmptyRelative()");
		Logger.LogDebug("CombineEmptyRelative(): result → {Result}", result);
    }

    [Test]
    public void CombineRelativeWithBaseTrimsSlashes()
    {
		Logger.LogDebug(">>> Entering CombineRelativeWithBaseTrimsSlashes()");
        var result = UrlUtils.Combine("/a/b/c/", "/d/e/f/");
        Assert.That(result, Is.EqualTo("/a/b/c/d/e/f/"));
		Logger.LogDebug("<<< Exiting CombineRelativeWithBaseTrimsSlashes()");
		Logger.LogDebug("CombineRelativeWithBaseTrimsSlashes(): result → {Result}", result);
    }

    [Test]
    public void DecodeParameterHandlesRoundTrip()
    {
		Logger.LogDebug(">>> Entering DecodeParameterHandlesRoundTrip()");
        const string input = "DNN Platform!";
        var encodedValue = UrlUtils.EncodeParameter(input);
		Logger.LogDebug("DecodeParameterHandlesRoundTrip(): encodedValue → {EncodedValue}", encodedValue);
        var result = UrlUtils.DecodeParameter(encodedValue);
        Assert.That(result, Is.EqualTo(input));
		Logger.LogDebug("<<< Exiting DecodeParameterHandlesRoundTrip()");
		Logger.LogDebug("DecodeParameterHandlesRoundTrip(): result → {Result}", result);
    }

    [Test]
    public void DecodeParameterHandlesSpecialCharacters()
    {
		Logger.LogDebug(">>> Entering DecodeParameterHandlesSpecialCharacters()");
        var result = UrlUtils.DecodeParameter("RE5_O1-$");
        Assert.That(result, Is.EqualTo("DN;_"));
		Logger.LogDebug("<<< Exiting DecodeParameterHandlesSpecialCharacters()");
		Logger.LogDebug("DecodeParameterHandlesSpecialCharacters(): result → {Result}", result);
    }

    [Test]
    public void DecryptParameterHandlesRoundTrip()
    {
		Logger.LogDebug(">>> Entering DecryptParameterHandlesRoundTrip()");
        const string input = "DNN Platform!";
        var key = Guid.NewGuid().ToString();
		Logger.LogWarning("DecryptParameterHandlesRoundTrip(): key → {Key}", key);
        var encodedValue = UrlUtils.EncryptParameter(input, key);
        var result = UrlUtils.DecryptParameter(encodedValue, key);
		Logger.LogDebug("DecryptParameterHandlesRoundTrip(): encodedValue → {EncodedValue}", encodedValue);
		Logger.LogDebug("DecryptParameterHandlesRoundTrip(): result → {Result}", result);
        Assert.That(result, Is.EqualTo(input));
		Logger.LogDebug("<<< Exiting DecryptParameterHandlesRoundTrip()");
    }

    [Test]
    public void EncodeParameterReplacesPaddingSymbols()
    {
		Logger.LogDebug(">>> Entering EncodeParameterReplacesPaddingSymbols()");
        var result = UrlUtils.EncodeParameter("D");
        Assert.That(result, Is.EqualTo("RA$$"));
		Logger.LogDebug("<<< Exiting EncodeParameterReplacesPaddingSymbols()");
		Logger.LogDebug("EncodeParameterReplacesPaddingSymbols(): result → {Result}", result);
    }

    [Test]
    public void EncryptParameterReplacesPaddingSymbols()
    {
		Logger.LogDebug(">>> Entering EncryptParameterReplacesPaddingSymbols()");
        var result = UrlUtils.EncryptParameter("D", "key");
        Assert.That(result, Does.EndWith("%3d"));
		Logger.LogDebug("<<< Exiting EncryptParameterReplacesPaddingSymbols()");
		Logger.LogDebug("EncryptParameterReplacesPaddingSymbols(): result → {Result}", result);
    }

    [Test]
    public void GetParameterNameReturnsName()
    {
		Logger.LogDebug(">>> Entering GetParameterNameReturnsName()");
        var result = UrlUtils.GetParameterName("key=value");
        Assert.That(result, Is.EqualTo("key"));
		Logger.LogDebug("<<< Exiting GetParameterNameReturnsName()");
		Logger.LogDebug("GetParameterNameReturnsName(): result → {Result}", result);
    }

    [Test]
    public void GetParameterNameReturnsEntireStringIfNoEqualsSign()
    {
		Logger.LogDebug(">>> Entering GetParameterNameReturnsEntireStringIfNoEqualsSign()");
        var result = UrlUtils.GetParameterName("just-a-key");
        Assert.That(result, Is.EqualTo("just-a-key"));
		Logger.LogDebug("<<< Exiting GetParameterNameReturnsEntireStringIfNoEqualsSign()");
		Logger.LogDebug("GetParameterNameReturnsEntireStringIfNoEqualsSign(): result → {Result}", result);
    }

    [Test]
    public void GetParameterNameReturnsEmptyIfStartsWithEqualsSign()
    {
		Logger.LogDebug(">>> Entering GetParameterNameReturnsEmptyIfStartsWithEqualsSign()");
        var result = UrlUtils.GetParameterName("=just-a-value");
        Assert.That(result, Is.EqualTo(string.Empty));
		Logger.LogDebug("<<< Exiting GetParameterNameReturnsEmptyIfStartsWithEqualsSign()");
		Logger.LogDebug("GetParameterNameReturnsEmptyIfStartsWithEqualsSign(): result → {Result}", result);
    }

    [Test]
    public void GetParameterValueReturnsName()
    {
		Logger.LogDebug(">>> Entering GetParameterValueReturnsName()");
        var result = UrlUtils.GetParameterValue("key=value");
        Assert.That(result, Is.EqualTo("value"));
		Logger.LogDebug("<<< Exiting GetParameterValueReturnsName()");
		Logger.LogDebug("GetParameterValueReturnsName(): result → {Result}", result);
    }

    [Test]
    public void GetParameterValueReturnsEmptyStringIfNoEqualsSign()
    {
		Logger.LogDebug(">>> Entering GetParameterValueReturnsEmptyStringIfNoEqualsSign()");
        var result = UrlUtils.GetParameterValue("just-a-key");
        Assert.That(result, Is.EqualTo(string.Empty));
		Logger.LogDebug("<<< Exiting GetParameterValueReturnsEmptyStringIfNoEqualsSign()");
		Logger.LogDebug("GetParameterValueReturnsEmptyStringIfNoEqualsSign(): result → {Result}", result);
    }

    [Test]
    public void GetParameterValueReturnsEntireStringIfStartsWithEqualsSign()
    {
		Logger.LogDebug(">>> Entering GetParameterValueReturnsEntireStringIfStartsWithEqualsSign()");
        var result = UrlUtils.GetParameterValue("=just-a-value");
        Assert.That(result, Is.EqualTo("just-a-value"));
		Logger.LogDebug("<<< Exiting GetParameterValueReturnsEntireStringIfStartsWithEqualsSign()");
		Logger.LogDebug("GetParameterValueReturnsEntireStringIfStartsWithEqualsSign(): result → {Result}", result);
    }

    [Test]
    public void ClosePopUpGeneratesAJavaScriptUrlWithValues()
    {
		Logger.LogDebug(">>> Entering ClosePopUpGeneratesAJavaScriptUrlWithValues()");
        var result = UrlUtils.ClosePopUp(false, "/hello",  false);
        Assert.That(result, Is.EqualTo("javascript:dnnModal.closePopUp(false, '/hello')"));
		Logger.LogDebug("ClosePopUpGeneratesAJavaScriptUrlWithValues(): result → {Result}", result);

        result = UrlUtils.ClosePopUp(true, "blah", false);
        Assert.That(result, Is.EqualTo("javascript:dnnModal.closePopUp(true, 'blah')"));
		Logger.LogDebug("<<< Exiting ClosePopUpGeneratesAJavaScriptUrlWithValues()");
    }

    [Test]
    public void ClosePopUpGeneratesAScriptWhenOnClickEventIsTrue()
    {
		Logger.LogDebug(">>> Entering ClosePopUpGeneratesAScriptWhenOnClickEventIsTrue()");
        var result = UrlUtils.ClosePopUp(false, "/somewhere",  true);
        Assert.That(result, Is.EqualTo("dnnModal.closePopUp(false, '/somewhere')"));
		Logger.LogDebug("<<< Exiting ClosePopUpGeneratesAScriptWhenOnClickEventIsTrue()");
		Logger.LogDebug("ClosePopUpGeneratesAScriptWhenOnClickEventIsTrue(): result → {Result}", result);
    }

    [Test]
    public void ClosePopUpEncodesUrlParameter()
    {
		Logger.LogDebug(">>> Entering ClosePopUpEncodesUrlParameter()");
        var result = UrlUtils.ClosePopUp(false, "/somewhere?value=%20hi&two='hey'",  true);
        Assert.That(result, Is.EqualTo("""dnnModal.closePopUp(false, '/somewhere?value=%20hi\u0026two=\u0027hey\u0027')"""));
		Logger.LogDebug("<<< Exiting ClosePopUpEncodesUrlParameter()");
		Logger.LogDebug("ClosePopUpEncodesUrlParameter(): result → {Result}", result);
    }

    [Test]
    public void ReplaceQSParamReplacesUnfriendlyParam()
    {
		Logger.LogDebug(">>> Entering ReplaceQSParamReplacesUnfriendlyParam()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.ReplaceQSParam("/somewhere?value=hi&two=hey", "two", "what");
        Assert.That(result, Is.EqualTo("/somewhere?value=hi&two=what"));
		Logger.LogDebug("<<< Exiting ReplaceQSParamReplacesUnfriendlyParam()");
		Logger.LogDebug("ReplaceQSParamReplacesUnfriendlyParam(): result → {Result}", result);
    }

    [Test]
    public void ReplaceQSParamReplacesFriendlyParam()
    {
		Logger.LogDebug(">>> Entering ReplaceQSParamReplacesFriendlyParam()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "true", };

        var result = UrlUtils.ReplaceQSParam("/somewhere/value/hi/two/hey/", "two", "what");
        Assert.That(result, Is.EqualTo("/somewhere/value/hi/two/what/"));
		Logger.LogDebug("<<< Exiting ReplaceQSParamReplacesFriendlyParam()");
		Logger.LogDebug("ReplaceQSParamReplacesFriendlyParam(): result → {Result}", result);
    }

    [Test]
    public void ReplaceQSParamHandlesSpecialCharacters()
    {
		Logger.LogDebug(">>> Entering ReplaceQSParamHandlesSpecialCharacters()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.ReplaceQSParam("/somewhere?one.two=three$four&one_two=123", "one.two", "four$3");
        Assert.That(result, Is.EqualTo("/somewhere?one.two=four$3&one_two=123"));
		Logger.LogDebug("<<< Exiting ReplaceQSParamHandlesSpecialCharacters()");
		Logger.LogDebug("ReplaceQSParamHandlesSpecialCharacters(): result → {Result}", result);
    }

    [Test]
    public void StripQSParamRemovesUnfriendlyParam()
    {
		Logger.LogDebug(">>> Entering StripQSParamRemovesUnfriendlyParam()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.StripQSParam("/somewhere?value=hi&two=hey&three=x", "two");
        Assert.That(result, Is.EqualTo("/somewhere?value=hi&three=x"));
		Logger.LogDebug("<<< Exiting StripQSParamRemovesUnfriendlyParam()");
		Logger.LogDebug("StripQSParamRemovesUnfriendlyParam(): result → {Result}", result);
    }

    [Test]
    public void StripQSParamRemovesFriendlyParam()
    {
		Logger.LogDebug(">>> Entering StripQSParamRemovesFriendlyParam()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "true", };

        var result = UrlUtils.StripQSParam("/somewhere/value/hi/two/hey/", "two");
        Assert.That(result, Is.EqualTo("/somewhere/value/hi/"));
		Logger.LogDebug("<<< Exiting StripQSParamRemovesFriendlyParam()");
		Logger.LogDebug("StripQSParamRemovesFriendlyParam(): result → {Result}", result);
    }

    [Test]
    public void StripQSParamHandlesSpecialCharacters()
    {
		Logger.LogDebug(">>> Entering StripQSParamHandlesSpecialCharacters()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.StripQSParam("/somewhere?one.two=three$four&one_two=123", "one.two");
        Assert.That(result, Is.EqualTo("/somewhere?one_two=123"));
		Logger.LogDebug("<<< Exiting StripQSParamHandlesSpecialCharacters()");
		Logger.LogDebug("StripQSParamHandlesSpecialCharacters(): result → {Result}", result);
    }

    [Test]
    public void ValidateReturnUrlReturnsNullWhenInputIsNull()
    {
		Logger.LogDebug(">>> Entering ValidateReturnUrlReturnsNullWhenInputIsNull()");
        var result = UrlUtils.ValidReturnUrl(null);
        Assert.That(result, Is.Null);
		Logger.LogDebug("<<< Exiting ValidateReturnUrlReturnsNullWhenInputIsNull()");
		Logger.LogDebug("ValidateReturnUrlReturnsNullWhenInputIsNull(): result → {Result}", result);
    }

    [Test]
    public void ValidateReturnUrlReturnsEmptyWhenInputIsEmpty()
    {
		Logger.LogDebug(">>> Entering ValidateReturnUrlReturnsEmptyWhenInputIsEmpty()");
        var result = UrlUtils.ValidReturnUrl(string.Empty);
        Assert.That(result, Is.Empty);
		Logger.LogDebug("<<< Exiting ValidateReturnUrlReturnsEmptyWhenInputIsEmpty()");
		Logger.LogDebug("ValidateReturnUrlReturnsEmptyWhenInputIsEmpty(): result → {Result}", result);
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptDataUrl()
    {
		Logger.LogDebug(">>> Entering ValidateReturnUrlDoesNotAcceptDataUrl()");
        var result = UrlUtils.ValidReturnUrl("data:text/plain,I am text file");
        Assert.That(result, Is.Empty);
		Logger.LogDebug("<<< Exiting ValidateReturnUrlDoesNotAcceptDataUrl()");
		Logger.LogDebug("ValidateReturnUrlDoesNotAcceptDataUrl(): result → {Result}", result);
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptXssAttack()
    {
		Logger.LogDebug(">>> Entering ValidateReturnUrlDoesNotAcceptXssAttack()");
        var result = UrlUtils.ValidReturnUrl("/return?onclick=alert()");
        Assert.That(result, Is.Empty);
		Logger.LogDebug("<<< Exiting ValidateReturnUrlDoesNotAcceptXssAttack()");
		Logger.LogDebug("ValidateReturnUrlDoesNotAcceptXssAttack(): result → {Result}", result);
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain()
    {
		Logger.LogDebug(">>> Entering ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain()");
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
		Logger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain(): portalAlias → {PortalAlias}", portalAlias);
		Logger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain(): portalSettings → {PortalSettings}", portalSettings);
        var portalControllerMock = new Mock<IPortalController>();
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
		Logger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("https://another.evil/return");
        Assert.That(result, Is.Empty);
		Logger.LogDebug("<<< Exiting ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain()");
		Logger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain(): result → {Result}", result);
    }

    [Test]
    public void ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain()
    {
		Logger.LogDebug(">>> Entering ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain()");
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
		Logger.LogDebug("ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain(): portalAlias → {PortalAlias}", portalAlias);
		Logger.LogDebug("ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain(): portalSettings → {PortalSettings}", portalSettings);
        var portalControllerMock = new Mock<IPortalController>();
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
		Logger.LogDebug("ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("https://dnncommunity.org/return");
        Assert.That(result, Is.EqualTo("https://dnncommunity.org/return"));
		Logger.LogDebug("<<< Exiting ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain()");
		Logger.LogDebug("ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain(): result → {Result}", result);
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch()
    {
		Logger.LogDebug(">>> Entering ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch()");
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
		Logger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch(): portalAlias → {PortalAlias}", portalAlias);
		Logger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch(): portalSettings → {PortalSettings}", portalSettings);
        var portalControllerMock = new Mock<IPortalController>();
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
		Logger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("/////dnncommunity.net/return");
        Assert.That(result, Is.Empty);
		Logger.LogDebug("<<< Exiting ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch()");
		Logger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch(): result → {Result}", result);
    }

    [Test]
    public void ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches()
    {
		Logger.LogDebug(">>> Entering ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches()");
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
		Logger.LogDebug("ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches(): portalAlias → {PortalAlias}", portalAlias);
		Logger.LogDebug("ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches(): portalSettings → {PortalSettings}", portalSettings);
        var portalControllerMock = new Mock<IPortalController>();
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
		Logger.LogDebug("ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("/////dnncommunity.org/return");
        Assert.That(result, Is.EqualTo("//dnncommunity.org/return"));
		Logger.LogDebug("<<< Exiting ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches()");
		Logger.LogDebug("ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches(): result → {Result}", result);
    }

    [Test]
    public void IsPopUpIsTrueWhenPopUpParameterIsOnUrl()
    {
		Logger.LogDebug(">>> Entering IsPopUpIsTrueWhenPopUpParameterIsOnUrl()");
        var result = UrlUtils.IsPopUp("/page?popUp=true");
        Assert.That(result, Is.True);
		Logger.LogDebug("<<< Exiting IsPopUpIsTrueWhenPopUpParameterIsOnUrl()");
		Logger.LogDebug("IsPopUpIsTrueWhenPopUpParameterIsOnUrl(): result → {Result}", result);
    }

    [Test]
    public void IsPopUpIsFalseWhenPopUpParameterIsNotOnUrl()
    {
		Logger.LogDebug(">>> Entering IsPopUpIsFalseWhenPopUpParameterIsNotOnUrl()");
        var result = UrlUtils.IsPopUp("/page");
        Assert.That(result, Is.False);
		Logger.LogDebug("<<< Exiting IsPopUpIsFalseWhenPopUpParameterIsNotOnUrl()");
		Logger.LogDebug("IsPopUpIsFalseWhenPopUpParameterIsNotOnUrl(): result → {Result}", result);
    }
}
