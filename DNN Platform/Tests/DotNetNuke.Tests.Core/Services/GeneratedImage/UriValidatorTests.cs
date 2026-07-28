// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.GeneratedImage;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class UriValidatorTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [TestCase("https://mysite.com/page", true)]
        [TestCase("http://mysite.com/page", true)]
        [TestCase("https://mysite.com", true)]
        [TestCase("http://mysite.com", true)]
        [TestCase("https://mysite.com/siteB", true)]
        [TestCase("http://mysite.com/siteB", true)]
        [TestCase("https://badactor.com", false)]
        [TestCase("http://badactor.com", false)]
        [TestCase("https://badactor.com/siteB", false)]
        [TestCase("http://badactor.com/siteB", false)]
        [TestCase("https://mysite.com.badactor.com", false)]
        [TestCase("http://mysite.com.badactor.com", false)]
        [TestCase("https://mysite.com.badactor.com/siteB", false)]
        [TestCase("http://mysite.com.badactor.com/siteB", false)]
        [TestCase("https://mysite.com.badactor.com/siteB/page", false)]
        [TestCase("http://mysite.com.badactor.com/siteB/page", false)]
        public void UriBelongsToSite_MultipleScenarios(string uriString, bool expected)
        {
			Logger.LogDebug(">>> Entering UriBelongsToSite_MultipleScenarios(uriString={UriString},expected={Expected})", uriString, expected);
            // Arrange
            var mockPortalAliasController = new Mock<IPortalAliasService>();
            mockPortalAliasController
				Logger.LogDebug("UriBelongsToSite_MultipleScenarios(uriString={UriString},expected={Expected}): mockPortalAliasController → {MockPortalAliasController}", uriString, expected, mockPortalAliasController);
                .Setup(controller => controller.GetPortalAliases())
                .Returns(new Dictionary<string, IPortalAliasInfo>
                {
                    { "mysite", new PortalAliasInfo { HTTPAlias = "mysite.com", } },
                    { "siteB", new PortalAliasInfo { HTTPAlias = "mysite.com/siteB", } },
                });

            var validator = new UriValidator(mockPortalAliasController.Object);

			Logger.LogDebug("UriBelongsToSite_MultipleScenarios(uriString={UriString},expected={Expected}): validator → {Validator}", uriString, expected, validator);
            var testUri = new Uri(uriString);

			Logger.LogDebug("UriBelongsToSite_MultipleScenarios(uriString={UriString},expected={Expected}): testUri → {TestUri}", uriString, expected, testUri);
            // Act
            var result = validator.UriBelongsToSite(testUri);

			Logger.LogDebug("UriBelongsToSite_MultipleScenarios(uriString={UriString},expected={Expected}): result → {Result}", uriString, expected, result);
            Assert.That(result, Is.EqualTo(expected));
			Logger.LogDebug("<<< Exiting UriBelongsToSite_MultipleScenarios(uriString={UriString},expected={Expected})", uriString, expected);
        }
    }
}
