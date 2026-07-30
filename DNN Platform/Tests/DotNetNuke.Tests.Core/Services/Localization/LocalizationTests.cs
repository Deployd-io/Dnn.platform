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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly string[] _standardCultureCodes = new[] { "en-US", "de-DE", "fr-CA", "Lt-sr-SP", "kok-IN" };
        private Mock<HttpContextBase> _mockHttpContext;
        private HttpContextBase _httpContext;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            this._mockHttpContext = HttpContextHelper.RegisterMockHttpContext();
            this._httpContext = this._mockHttpContext.Object;
			Logger.LogDebug("<<< Exiting Setup()");
			Logger.LogDebug("Setup(): _httpContext → {_httpContext}", _httpContext);
			Logger.LogDebug("Setup(): this._httpContext → {Arg0}", this._httpContext);
			Logger.LogDebug("Setup(): _mockHttpContext → {_mockHttpContext}", _mockHttpContext);
        }

        [Test]
        public void NoMatchReturnsFallback()
        {
			Logger.LogDebug(">>> Entering NoMatchReturnsFallback()");
            const string fallback = "fallback";

            // Arrange

            // Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages([], fallback);

			Logger.LogDebug("NoMatchReturnsFallback(): ret → {Ret}", ret);
            // Assert
            Assert.That(ret, Is.EqualTo(fallback));
			Logger.LogDebug("<<< Exiting NoMatchReturnsFallback()");
        }

        [Test]
        public void CultureCodesCannotBeNull()
        {
			Logger.LogDebug(">>> Entering CultureCodesCannotBeNull()");
            // Arrange

            // Act
            Assert.Throws<ArgumentException>(() => TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(null));

            // Assert
			Logger.LogDebug("<<< Exiting CultureCodesCannotBeNull()");
        }

        [Test]
        public void FallBackCannotBeNull()
        {
			Logger.LogDebug(">>> Entering FallBackCannotBeNull()");
            // Arrange

            // Act
            Assert.Throws<ArgumentException>(() => TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages([], null));

            // Assert
			Logger.LogDebug("<<< Exiting FallBackCannotBeNull()");
        }

        [Test]
        public void PerfectMatchPossible()
        {
			Logger.LogDebug(">>> Entering PerfectMatchPossible()");
            // Arrange
            this._mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "de-DE" });

            // Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(this._standardCultureCodes);

			Logger.LogDebug("PerfectMatchPossible(): ret → {Ret}", ret);
            // Assert
            Assert.That(ret, Is.EqualTo("de-DE"));
			Logger.LogDebug("<<< Exiting PerfectMatchPossible()");
        }

        [Test]
        public void QParamsAreIgnored()
        {
			Logger.LogDebug(">>> Entering QParamsAreIgnored()");
            // Arrange
            this._mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "de-DE;q=0.8" });

            // Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(this._standardCultureCodes);

			Logger.LogDebug("QParamsAreIgnored(): ret → {Ret}", ret);
            // Assert
            Assert.That(ret, Is.EqualTo("de-DE"));
			Logger.LogDebug("<<< Exiting QParamsAreIgnored()");
        }

        [Test]
        public void MatchOnOnlyLanguage()
        {
			Logger.LogDebug(">>> Entering MatchOnOnlyLanguage()");
            // Arrange
            this._mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "fr-FR" });

            // Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(this._standardCultureCodes);

			Logger.LogDebug("MatchOnOnlyLanguage(): ret → {Ret}", ret);
            // Assert
            Assert.That(ret, Is.EqualTo("fr-CA"));
			Logger.LogDebug("<<< Exiting MatchOnOnlyLanguage()");
        }

        [Test]
        public void PerfectMatchPreferredToFirstMatch()
        {
			Logger.LogDebug(">>> Entering PerfectMatchPreferredToFirstMatch()");
            // Arrange
            this._mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "fr-FR" });

            // Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(new[] { "fr-CA", "fr-FR" });

			Logger.LogDebug("PerfectMatchPreferredToFirstMatch(): ret → {Ret}", ret);
            // Assert
            Assert.That(ret, Is.EqualTo("fr-FR"));
			Logger.LogDebug("<<< Exiting PerfectMatchPreferredToFirstMatch()");
        }

        [Test]
        [TestCase("My/Path/To/File with.locale-extension")]
        [TestCase("My\\Path\\To\\File with.locale-extension")]
        public void ParseLocaleFromResxFileName(string fileName)
        {
			Logger.LogDebug(">>> Entering ParseLocaleFromResxFileName(fileName={FileName})", fileName);
            foreach (var standardCultureCode in this._standardCultureCodes)
            {
                var f = fileName + "." + standardCultureCode + ".resx";
                Assert.That(standardCultureCode, Is.EqualTo(f.GetLocaleCodeFromFileName()));
				Logger.LogDebug("ParseLocaleFromResxFileName(fileName={FileName}): f → {F}", fileName, f);
            }
			Logger.LogDebug("<<< Exiting ParseLocaleFromResxFileName(fileName={FileName})", fileName);
        }

        [Test]
        [TestCase("My/Path/To/File with.locale-extension")]
        [TestCase("My\\Path\\To\\File with.locale-extension")]
        public void ParseFileNameFromResxFile(string fileName)
        {
			Logger.LogDebug(">>> Entering ParseFileNameFromResxFile(fileName={FileName})", fileName);
            foreach (var standardCultureCode in this._standardCultureCodes)
            {
                var f = fileName + "." + standardCultureCode + ".resx";
                Assert.That(fileName, Is.EqualTo(f.GetFileNameFromLocalizedResxFile()));
				Logger.LogDebug("ParseFileNameFromResxFile(fileName={FileName}): f → {F}", fileName, f);
            }
			Logger.LogDebug("<<< Exiting ParseFileNameFromResxFile(fileName={FileName})", fileName);
        }
    }
}
