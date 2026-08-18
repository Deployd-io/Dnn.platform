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
    private static readonly Dictionary<string, IConfigurationSetting> HostSettings = new Dictionary<string, IConfigurationSetting>();

    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
		mockEventLogger.LogDebug(">>> Entering OneTimeSetUp()");
        ComponentFactory.RegisterComponent<CryptographyProvider, CoreCryptographyProvider>();
        MockComponentProvider.CreateDataCacheProvider();
        var hostController = new FakeHostController(HostSettings);
		mockEventLogger.LogDebug("OneTimeSetUp(): hostController → {HostController}", hostController);

        var serviceCollection = new ServiceCollection();
		mockEventLogger.LogDebug("OneTimeSetUp(): serviceCollection → {ServiceCollection}", serviceCollection);
        serviceCollection.AddSingleton(Mock.Of<IApplicationStatusInfo>());
        serviceCollection.AddSingleton(Mock.Of<INavigationManager>());
        serviceCollection.AddSingleton(Mock.Of<IPortalSettingsController>());
        serviceCollection.AddSingleton(new ListController(Mock.Of<IEventLogger>(), Mock.Of<IHostSettings>()));
        serviceCollection.AddSingleton(Mock.Of<IPortalController>());
        serviceCollection.AddSingleton(Mock.Of<IPortalGroupController>());
        serviceCollection.AddSingleton<ICryptographyProvider>(new CoreCryptographyProvider());
        serviceCollection.AddSingleton<IHostSettingsService>(hostController);
        serviceCollection.AddSingleton<IHostSettings>(new HostSettings(hostController));

        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
		mockEventLogger.LogDebug("<<< Exiting OneTimeSetUp()");
    }

    [OneTimeTearDown]
    public static void OneTimeTearDown()
    {
		mockEventLogger.LogDebug(">>> Entering OneTimeTearDown()");
        if (Globals.DependencyProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
		mockEventLogger.LogDebug("<<< Exiting OneTimeTearDown()");
    }

    [Test]
    public void CombineEmptyBase()
    {
		mockEventLogger.LogDebug(">>> Entering CombineEmptyBase()");
        var result = UrlUtils.Combine(string.Empty, "a/b/c");
        Assert.That(result, Is.EqualTo("a/b/c"));
		mockEventLogger.LogDebug("CombineEmptyBase(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting CombineEmptyBase()");
    }

    [Test]
    public void CombineEmptyRelative()
    {
		mockEventLogger.LogDebug(">>> Entering CombineEmptyRelative()");
        var result = UrlUtils.Combine("/a/b/c", string.Empty);
        Assert.That(result, Is.EqualTo("/a/b/c"));
		mockEventLogger.LogDebug("CombineEmptyRelative(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting CombineEmptyRelative()");
    }

    [Test]
    public void CombineRelativeWithBaseTrimsSlashes()
    {
		mockEventLogger.LogDebug(">>> Entering CombineRelativeWithBaseTrimsSlashes()");
        var result = UrlUtils.Combine("/a/b/c/", "/d/e/f/");
        Assert.That(result, Is.EqualTo("/a/b/c/d/e/f/"));
		mockEventLogger.LogDebug("CombineRelativeWithBaseTrimsSlashes(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting CombineRelativeWithBaseTrimsSlashes()");
    }

    [Test]
    public void DecodeParameterHandlesRoundTrip()
    {
		mockEventLogger.LogDebug(">>> Entering DecodeParameterHandlesRoundTrip()");
        const string input = "DNN Platform!";
        var encodedValue = UrlUtils.EncodeParameter(input);
		mockEventLogger.LogDebug("DecodeParameterHandlesRoundTrip(): encodedValue → {EncodedValue}", encodedValue);
        var result = UrlUtils.DecodeParameter(encodedValue);
		mockEventLogger.LogDebug("DecodeParameterHandlesRoundTrip(): result → {Result}", result);
        Assert.That(result, Is.EqualTo(input));
		mockEventLogger.LogDebug("<<< Exiting DecodeParameterHandlesRoundTrip()");
    }

    [Test]
    public void DecodeParameterHandlesSpecialCharacters()
    {
		mockEventLogger.LogDebug(">>> Entering DecodeParameterHandlesSpecialCharacters()");
        var result = UrlUtils.DecodeParameter("RE5_O1-$");
        Assert.That(result, Is.EqualTo("DN;_"));
		mockEventLogger.LogDebug("DecodeParameterHandlesSpecialCharacters(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting DecodeParameterHandlesSpecialCharacters()");
    }

    [Test]
    public void DecryptParameterHandlesRoundTrip()
    {
		mockEventLogger.LogDebug(">>> Entering DecryptParameterHandlesRoundTrip()");
        const string input = "DNN Platform!";
        var key = Guid.NewGuid().ToString();
		mockEventLogger.LogWarning("DecryptParameterHandlesRoundTrip(): key → {Key}", key);
        var encodedValue = UrlUtils.EncryptParameter(input, key);
		mockEventLogger.LogDebug("DecryptParameterHandlesRoundTrip(): encodedValue → {EncodedValue}", encodedValue);
        var result = UrlUtils.DecryptParameter(encodedValue, key);
		mockEventLogger.LogDebug("DecryptParameterHandlesRoundTrip(): result → {Result}", result);
        Assert.That(result, Is.EqualTo(input));
		mockEventLogger.LogDebug("<<< Exiting DecryptParameterHandlesRoundTrip()");
    }

    [Test]
    public void EncodeParameterReplacesPaddingSymbols()
    {
		mockEventLogger.LogDebug(">>> Entering EncodeParameterReplacesPaddingSymbols()");
        var result = UrlUtils.EncodeParameter("D");
        Assert.That(result, Is.EqualTo("RA$$"));
		mockEventLogger.LogDebug("EncodeParameterReplacesPaddingSymbols(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting EncodeParameterReplacesPaddingSymbols()");
    }

    [Test]
    public void EncryptParameterReplacesPaddingSymbols()
    {
		mockEventLogger.LogDebug(">>> Entering EncryptParameterReplacesPaddingSymbols()");
        var result = UrlUtils.EncryptParameter("D", "key");
        Assert.That(result, Does.EndWith("%3d"));
		mockEventLogger.LogDebug("EncryptParameterReplacesPaddingSymbols(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting EncryptParameterReplacesPaddingSymbols()");
    }

    [Test]
    public void GetParameterNameReturnsName()
    {
		mockEventLogger.LogDebug(">>> Entering GetParameterNameReturnsName()");
        var result = UrlUtils.GetParameterName("key=value");
        Assert.That(result, Is.EqualTo("key"));
		mockEventLogger.LogDebug("GetParameterNameReturnsName(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting GetParameterNameReturnsName()");
    }

    [Test]
    public void GetParameterNameReturnsEntireStringIfNoEqualsSign()
    {
		mockEventLogger.LogDebug(">>> Entering GetParameterNameReturnsEntireStringIfNoEqualsSign()");
        var result = UrlUtils.GetParameterName("just-a-key");
        Assert.That(result, Is.EqualTo("just-a-key"));
		mockEventLogger.LogDebug("GetParameterNameReturnsEntireStringIfNoEqualsSign(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting GetParameterNameReturnsEntireStringIfNoEqualsSign()");
    }

    [Test]
    public void GetParameterNameReturnsEmptyIfStartsWithEqualsSign()
    {
		mockEventLogger.LogDebug(">>> Entering GetParameterNameReturnsEmptyIfStartsWithEqualsSign()");
        var result = UrlUtils.GetParameterName("=just-a-value");
        Assert.That(result, Is.EqualTo(string.Empty));
		mockEventLogger.LogDebug("GetParameterNameReturnsEmptyIfStartsWithEqualsSign(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting GetParameterNameReturnsEmptyIfStartsWithEqualsSign()");
    }

    [Test]
    public void GetParameterValueReturnsName()
    {
		mockEventLogger.LogDebug(">>> Entering GetParameterValueReturnsName()");
        var result = UrlUtils.GetParameterValue("key=value");
        Assert.That(result, Is.EqualTo("value"));
		mockEventLogger.LogDebug("GetParameterValueReturnsName(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting GetParameterValueReturnsName()");
    }

    [Test]
    public void GetParameterValueReturnsEmptyStringIfNoEqualsSign()
    {
		mockEventLogger.LogDebug(">>> Entering GetParameterValueReturnsEmptyStringIfNoEqualsSign()");
        var result = UrlUtils.GetParameterValue("just-a-key");
        Assert.That(result, Is.EqualTo(string.Empty));
		mockEventLogger.LogDebug("GetParameterValueReturnsEmptyStringIfNoEqualsSign(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting GetParameterValueReturnsEmptyStringIfNoEqualsSign()");
    }

    [Test]
    public void GetParameterValueReturnsEntireStringIfStartsWithEqualsSign()
    {
		mockEventLogger.LogDebug(">>> Entering GetParameterValueReturnsEntireStringIfStartsWithEqualsSign()");
        var result = UrlUtils.GetParameterValue("=just-a-value");
        Assert.That(result, Is.EqualTo("just-a-value"));
		mockEventLogger.LogDebug("GetParameterValueReturnsEntireStringIfStartsWithEqualsSign(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting GetParameterValueReturnsEntireStringIfStartsWithEqualsSign()");
    }

    [Test]
    public void ClosePopUpGeneratesAJavaScriptUrlWithValues()
    {
		mockEventLogger.LogDebug(">>> Entering ClosePopUpGeneratesAJavaScriptUrlWithValues()");
        var result = UrlUtils.ClosePopUp(false, "/hello",  false);
        Assert.That(result, Is.EqualTo("javascript:dnnModal.closePopUp(false, '/hello')"));
		mockEventLogger.LogDebug("ClosePopUpGeneratesAJavaScriptUrlWithValues(): result → {Result}", result);

        result = UrlUtils.ClosePopUp(true, "blah", false);
        Assert.That(result, Is.EqualTo("javascript:dnnModal.closePopUp(true, 'blah')"));
		mockEventLogger.LogDebug("<<< Exiting ClosePopUpGeneratesAJavaScriptUrlWithValues()");
    }

    [Test]
    public void ClosePopUpGeneratesAScriptWhenOnClickEventIsTrue()
    {
		mockEventLogger.LogDebug(">>> Entering ClosePopUpGeneratesAScriptWhenOnClickEventIsTrue()");
        var result = UrlUtils.ClosePopUp(false, "/somewhere",  true);
        Assert.That(result, Is.EqualTo("dnnModal.closePopUp(false, '/somewhere')"));
		mockEventLogger.LogDebug("ClosePopUpGeneratesAScriptWhenOnClickEventIsTrue(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting ClosePopUpGeneratesAScriptWhenOnClickEventIsTrue()");
    }

    [Test]
    public void ClosePopUpEncodesUrlParameter()
    {
		mockEventLogger.LogDebug(">>> Entering ClosePopUpEncodesUrlParameter()");
        var result = UrlUtils.ClosePopUp(false, "/somewhere?value=%20hi&two='hey'",  true);
        Assert.That(result, Is.EqualTo("""dnnModal.closePopUp(false, '/somewhere?value=%20hi\u0026two=\u0027hey\u0027')"""));
		mockEventLogger.LogDebug("ClosePopUpEncodesUrlParameter(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting ClosePopUpEncodesUrlParameter()");
    }

    [Test]
    public void ReplaceQSParamReplacesUnfriendlyParam()
    {
		mockEventLogger.LogDebug(">>> Entering ReplaceQSParamReplacesUnfriendlyParam()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.ReplaceQSParam("/somewhere?value=hi&two=hey", "two", "what");
		mockEventLogger.LogDebug("ReplaceQSParamReplacesUnfriendlyParam(): result → {Result}", result);
        Assert.That(result, Is.EqualTo("/somewhere?value=hi&two=what"));
		mockEventLogger.LogDebug("<<< Exiting ReplaceQSParamReplacesUnfriendlyParam()");
    }

    [Test]
    public void ReplaceQSParamReplacesFriendlyParam()
    {
		mockEventLogger.LogDebug(">>> Entering ReplaceQSParamReplacesFriendlyParam()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "true", };

        var result = UrlUtils.ReplaceQSParam("/somewhere/value/hi/two/hey/", "two", "what");
		mockEventLogger.LogDebug("ReplaceQSParamReplacesFriendlyParam(): result → {Result}", result);
        Assert.That(result, Is.EqualTo("/somewhere/value/hi/two/what/"));
		mockEventLogger.LogDebug("<<< Exiting ReplaceQSParamReplacesFriendlyParam()");
    }

    [Test]
    public void ReplaceQSParamHandlesSpecialCharacters()
    {
		mockEventLogger.LogDebug(">>> Entering ReplaceQSParamHandlesSpecialCharacters()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.ReplaceQSParam("/somewhere?one.two=three$four&one_two=123", "one.two", "four$3");
		mockEventLogger.LogDebug("ReplaceQSParamHandlesSpecialCharacters(): result → {Result}", result);
        Assert.That(result, Is.EqualTo("/somewhere?one.two=four$3&one_two=123"));
		mockEventLogger.LogDebug("<<< Exiting ReplaceQSParamHandlesSpecialCharacters()");
    }

    [Test]
    public void StripQSParamRemovesUnfriendlyParam()
    {
		mockEventLogger.LogDebug(">>> Entering StripQSParamRemovesUnfriendlyParam()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.StripQSParam("/somewhere?value=hi&two=hey&three=x", "two");
		mockEventLogger.LogDebug("StripQSParamRemovesUnfriendlyParam(): result → {Result}", result);
        Assert.That(result, Is.EqualTo("/somewhere?value=hi&three=x"));
		mockEventLogger.LogDebug("<<< Exiting StripQSParamRemovesUnfriendlyParam()");
    }

    [Test]
    public void StripQSParamRemovesFriendlyParam()
    {
		mockEventLogger.LogDebug(">>> Entering StripQSParamRemovesFriendlyParam()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "true", };

        var result = UrlUtils.StripQSParam("/somewhere/value/hi/two/hey/", "two");
		mockEventLogger.LogDebug("StripQSParamRemovesFriendlyParam(): result → {Result}", result);
        Assert.That(result, Is.EqualTo("/somewhere/value/hi/"));
		mockEventLogger.LogDebug("<<< Exiting StripQSParamRemovesFriendlyParam()");
    }

    [Test]
    public void StripQSParamHandlesSpecialCharacters()
    {
		mockEventLogger.LogDebug(">>> Entering StripQSParamHandlesSpecialCharacters()");
        HostSettings["UseFriendlyUrls"] = new ConfigurationSetting { Key = "UseFriendlyUrls", Value = "false", };

        var result = UrlUtils.StripQSParam("/somewhere?one.two=three$four&one_two=123", "one.two");
		mockEventLogger.LogDebug("StripQSParamHandlesSpecialCharacters(): result → {Result}", result);
        Assert.That(result, Is.EqualTo("/somewhere?one_two=123"));
		mockEventLogger.LogDebug("<<< Exiting StripQSParamHandlesSpecialCharacters()");
    }

    [Test]
    public void ValidateReturnUrlReturnsNullWhenInputIsNull()
    {
		mockEventLogger.LogDebug(">>> Entering ValidateReturnUrlReturnsNullWhenInputIsNull()");
        var result = UrlUtils.ValidReturnUrl(null);
        Assert.That(result, Is.Null);
		mockEventLogger.LogDebug("ValidateReturnUrlReturnsNullWhenInputIsNull(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting ValidateReturnUrlReturnsNullWhenInputIsNull()");
    }

    [Test]
    public void ValidateReturnUrlReturnsEmptyWhenInputIsEmpty()
    {
		mockEventLogger.LogDebug(">>> Entering ValidateReturnUrlReturnsEmptyWhenInputIsEmpty()");
        var result = UrlUtils.ValidReturnUrl(string.Empty);
        Assert.That(result, Is.Empty);
		mockEventLogger.LogDebug("ValidateReturnUrlReturnsEmptyWhenInputIsEmpty(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting ValidateReturnUrlReturnsEmptyWhenInputIsEmpty()");
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptDataUrl()
    {
		mockEventLogger.LogDebug(">>> Entering ValidateReturnUrlDoesNotAcceptDataUrl()");
        var result = UrlUtils.ValidReturnUrl("data:text/plain,I am text file");
        Assert.That(result, Is.Empty);
		mockEventLogger.LogDebug("ValidateReturnUrlDoesNotAcceptDataUrl(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting ValidateReturnUrlDoesNotAcceptDataUrl()");
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptXssAttack()
    {
		mockEventLogger.LogDebug(">>> Entering ValidateReturnUrlDoesNotAcceptXssAttack()");
        var result = UrlUtils.ValidReturnUrl("/return?onclick=alert()");
        Assert.That(result, Is.Empty);
		mockEventLogger.LogDebug("ValidateReturnUrlDoesNotAcceptXssAttack(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting ValidateReturnUrlDoesNotAcceptXssAttack()");
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain()
    {
		mockEventLogger.LogDebug(">>> Entering ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain()");
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
		mockEventLogger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain(): portalSettings → {PortalSettings}", portalSettings);
        var portalControllerMock = new Mock<IPortalController>();
		mockEventLogger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
		mockEventLogger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain(): portalAlias → {PortalAlias}", portalAlias);
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("https://another.evil/return");
		mockEventLogger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain(): result → {Result}", result);
        Assert.That(result, Is.Empty);
		mockEventLogger.LogDebug("<<< Exiting ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutMatchingDomain()");
    }

    [Test]
    public void ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain()
    {
		mockEventLogger.LogDebug(">>> Entering ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain()");
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
		mockEventLogger.LogDebug("ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain(): portalSettings → {PortalSettings}", portalSettings);
        var portalControllerMock = new Mock<IPortalController>();
		mockEventLogger.LogDebug("ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
		mockEventLogger.LogDebug("ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain(): portalAlias → {PortalAlias}", portalAlias);
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("https://dnncommunity.org/return");
		mockEventLogger.LogDebug("ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain(): result → {Result}", result);
        Assert.That(result, Is.EqualTo("https://dnncommunity.org/return"));
		mockEventLogger.LogDebug("<<< Exiting ValidateReturnUrlDoesAcceptAbsoluteUrlWithMatchingDomain()");
    }

    [Test]
    public void ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch()
    {
		mockEventLogger.LogDebug(">>> Entering ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch()");
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
		mockEventLogger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch(): portalSettings → {PortalSettings}", portalSettings);
        var portalControllerMock = new Mock<IPortalController>();
		mockEventLogger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
		mockEventLogger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch(): portalAlias → {PortalAlias}", portalAlias);
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("/////dnncommunity.net/return");
		mockEventLogger.LogDebug("ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch(): result → {Result}", result);
        Assert.That(result, Is.Empty);
		mockEventLogger.LogDebug("<<< Exiting ValidateReturnUrlDoesNotAcceptAbsoluteUrlWithoutProtocolWhenDomainDoesNotMatch()");
    }

    [Test]
    public void ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches()
    {
		mockEventLogger.LogDebug(">>> Entering ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches()");
        var portalAlias = new PortalAliasInfo { HTTPAlias = "dnncommunity.org", };
        var portalSettings = new PortalSettings(-1, portal: null) { PortalAlias = portalAlias, };
		mockEventLogger.LogDebug("ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches(): portalSettings → {PortalSettings}", portalSettings);
        var portalControllerMock = new Mock<IPortalController>();
		mockEventLogger.LogDebug("ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
		mockEventLogger.LogDebug("ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches(): portalAlias → {PortalAlias}", portalAlias);
        portalControllerMock.Setup(c => c.GetCurrentPortalSettings()).Returns(portalSettings);
        PortalController.SetTestableInstance(portalControllerMock.Object);

        var result = UrlUtils.ValidReturnUrl("/////dnncommunity.org/return");
		mockEventLogger.LogDebug("ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches(): result → {Result}", result);
        Assert.That(result, Is.EqualTo("//dnncommunity.org/return"));
		mockEventLogger.LogDebug("<<< Exiting ValidateReturnUrlAcceptsAbsoluteUrlWithoutProtocolWhenDomainDoesMatches()");
    }

    [Test]
    public void IsPopUpIsTrueWhenPopUpParameterIsOnUrl()
    {
		mockEventLogger.LogDebug(">>> Entering IsPopUpIsTrueWhenPopUpParameterIsOnUrl()");
        var result = UrlUtils.IsPopUp("/page?popUp=true");
        Assert.That(result, Is.True);
		mockEventLogger.LogDebug("IsPopUpIsTrueWhenPopUpParameterIsOnUrl(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting IsPopUpIsTrueWhenPopUpParameterIsOnUrl()");
    }

    [Test]
    public void IsPopUpIsFalseWhenPopUpParameterIsNotOnUrl()
    {
		mockEventLogger.LogDebug(">>> Entering IsPopUpIsFalseWhenPopUpParameterIsNotOnUrl()");
        var result = UrlUtils.IsPopUp("/page");
        Assert.That(result, Is.False);
		mockEventLogger.LogDebug("IsPopUpIsFalseWhenPopUpParameterIsNotOnUrl(): result → {Result}", result);
		mockEventLogger.LogDebug("<<< Exiting IsPopUpIsFalseWhenPopUpParameterIsNotOnUrl()");
    }
}
