// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.Localization
{
    using System;
    using System.Web;

    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Localization.Internal;
    using DotNetNuke.Tests.Utilities;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class LocalizationTests
    {
        private readonly string[] _standardCultureCodes = new[] { "en-US", "de-DE", "fr-CA", "Lt-sr-SP", "kok-IN" };
        private Mock<HttpContextBase> _mockHttpContext;
        private HttpContextBase _httpContext;

        [SetUp]
        public void Setup()
        {
			mockEventLogger.LogDebug(">>> Entering Setup()");
            this._mockHttpContext = HttpContextHelper.RegisterMockHttpContext();
            this._httpContext = this._mockHttpContext.Object;
			mockEventLogger.LogDebug("Setup(): this._httpContext → {Arg0}", this._httpContext);
			mockEventLogger.LogDebug("Setup(): _httpContext → {_httpContext}", _httpContext);
			mockEventLogger.LogDebug("<<< Exiting Setup()");
        }

        [Test]
        public void NoMatchReturnsFallback()
        {
			mockEventLogger.LogDebug(">>> Entering NoMatchReturnsFallback()");
            const string fallback = "fallback";

            // Arrange

            // Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages([], fallback);
			mockEventLogger.LogDebug("NoMatchReturnsFallback(): ret → {Ret}", ret);

            // Assert
            Assert.That(ret, Is.EqualTo(fallback));
			mockEventLogger.LogDebug("<<< Exiting NoMatchReturnsFallback()");
        }

        [Test]
        public void CultureCodesCannotBeNull()
        {
			mockEventLogger.LogDebug(">>> Entering CultureCodesCannotBeNull()");
            // Arrange

            // Act
            Assert.Throws<ArgumentException>(() => TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(null));

            // Assert
			mockEventLogger.LogDebug("<<< Exiting CultureCodesCannotBeNull()");
        }

        [Test]
        public void FallBackCannotBeNull()
        {
			mockEventLogger.LogDebug(">>> Entering FallBackCannotBeNull()");
            // Arrange

            // Act
            Assert.Throws<ArgumentException>(() => TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages([], null));

            // Assert
			mockEventLogger.LogDebug("<<< Exiting FallBackCannotBeNull()");
        }

        [Test]
        public void PerfectMatchPossible()
        {
			mockEventLogger.LogDebug(">>> Entering PerfectMatchPossible()");
            // Arrange
            this._mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "de-DE" });

            // Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(this._standardCultureCodes);
			mockEventLogger.LogDebug("PerfectMatchPossible(): ret → {Ret}", ret);

            // Assert
            Assert.That(ret, Is.EqualTo("de-DE"));
			mockEventLogger.LogDebug("<<< Exiting PerfectMatchPossible()");
        }

        [Test]
        public void QParamsAreIgnored()
        {
			mockEventLogger.LogDebug(">>> Entering QParamsAreIgnored()");
            // Arrange
            this._mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "de-DE;q=0.8" });

            // Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(this._standardCultureCodes);
			mockEventLogger.LogDebug("QParamsAreIgnored(): ret → {Ret}", ret);

            // Assert
            Assert.That(ret, Is.EqualTo("de-DE"));
			mockEventLogger.LogDebug("<<< Exiting QParamsAreIgnored()");
        }

        [Test]
        public void MatchOnOnlyLanguage()
        {
			mockEventLogger.LogDebug(">>> Entering MatchOnOnlyLanguage()");
            // Arrange
            this._mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "fr-FR" });

            // Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(this._standardCultureCodes);
			mockEventLogger.LogDebug("MatchOnOnlyLanguage(): ret → {Ret}", ret);

            // Assert
            Assert.That(ret, Is.EqualTo("fr-CA"));
			mockEventLogger.LogDebug("<<< Exiting MatchOnOnlyLanguage()");
        }

        [Test]
        public void PerfectMatchPreferredToFirstMatch()
        {
			mockEventLogger.LogDebug(">>> Entering PerfectMatchPreferredToFirstMatch()");
            // Arrange
            this._mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "fr-FR" });

            // Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(new[] { "fr-CA", "fr-FR" });
			mockEventLogger.LogDebug("PerfectMatchPreferredToFirstMatch(): ret → {Ret}", ret);

            // Assert
            Assert.That(ret, Is.EqualTo("fr-FR"));
			mockEventLogger.LogDebug("<<< Exiting PerfectMatchPreferredToFirstMatch()");
        }

        [Test]
        [TestCase("My/Path/To/File with.locale-extension")]
        [TestCase("My\\Path\\To\\File with.locale-extension")]
        public void ParseLocaleFromResxFileName(string fileName)
        {
			mockEventLogger.LogDebug(">>> Entering ParseLocaleFromResxFileName(fileName={FileName})", fileName);
            foreach (var standardCultureCode in this._standardCultureCodes)
            {
                var f = fileName + "." + standardCultureCode + ".resx";
				mockEventLogger.LogDebug("ParseLocaleFromResxFileName(fileName={FileName}): f → {F}", fileName, f);
                Assert.That(standardCultureCode, Is.EqualTo(f.GetLocaleCodeFromFileName()));
            }
			mockEventLogger.LogDebug("<<< Exiting ParseLocaleFromResxFileName(fileName={FileName})", fileName);
        }

        [Test]
        [TestCase("My/Path/To/File with.locale-extension")]
        [TestCase("My\\Path\\To\\File with.locale-extension")]
        public void ParseFileNameFromResxFile(string fileName)
        {
			mockEventLogger.LogDebug(">>> Entering ParseFileNameFromResxFile(fileName={FileName})", fileName);
            foreach (var standardCultureCode in this._standardCultureCodes)
            {
                var f = fileName + "." + standardCultureCode + ".resx";
				mockEventLogger.LogDebug("ParseFileNameFromResxFile(fileName={FileName}): f → {F}", fileName, f);
                Assert.That(fileName, Is.EqualTo(f.GetFileNameFromLocalizedResxFile()));
            }
			mockEventLogger.LogDebug("<<< Exiting ParseFileNameFromResxFile(fileName={FileName})", fileName);
        }
    }
}
