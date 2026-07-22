// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Security.PortalSecurity
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using ICryptographyProvider = DotNetNuke.Abstractions.Security.ICryptographyProvider;

    [TestFixture]
    public class PortalSecurityTest
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            this.serviceProvider = FakeServiceProvider.Setup(services =>
            {
				Logger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
                services.AddSingleton(Mock.Of<ICryptographyProvider>());
                services.AddSingleton(new ListController(Mock.Of<IEventLogger>(), Mock.Of<IHostSettings>()));
                services.AddSingleton(Mock.Of<IPortalController>());
                services.AddSingleton(Mock.Of<IApplicationStatusInfo>());
                services.AddSingleton(Mock.Of<IPortalGroupController>());
            });
			Logger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        // NoMarkup | NoScripting | NoAngleBrackets
        [TestCase("<source></source>", " ", PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<source>Hi this is personal Test for source tag</source>   ", "    ", PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<SOURCE>{Upper case}</SOURCE>", " ", PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<source src=\"https://google.com\">Source with attribute</source>", " ", PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets)]

        // NoMarkup | NoScripting
        [TestCase("<source></source>", " ", PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<source>Hi this is personal Test for source tag</source>   ", "    ", PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<SOURCE>Upper case</SOURCE>", " ", PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<source src=\"https://google.com\">Source with attribute</source>", " ", PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoScripting)]

        // NoScripting | NoAngleBrackets
        [TestCase("<source></source>", " ", PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<source>Hi this is personal Test for source tag</source>   ", "    ", PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<SOURCE>{Upper case}</SOURCE>", " ", PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<source src=\"https://google.com\">Source with attribute</source>", " ", PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets)]

        // NoMarkup | NoAngleBrackets
        [TestCase("<source></source>", "&lt;source&gt;&lt;/source&gt;", PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase(
            "<source>Hi this is personal Test for source tag</source>   ",
            "&lt;source&gt;Hi this is personal Test for source tag&lt;/source&gt;   ",
            PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<SOURCE>{Upper case}</SOURCE>", "&lt;SOURCE&gt;{Upper case}&lt;/SOURCE&gt;", PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase(
            "<source src=\"https://google.com\">Source with attribute</source>",
            "&lt;source src=&quot;https://google.com&quot;&gt;Source with attribute&lt;/source&gt;",
            PortalSecurity.FilterFlag.NoMarkup | PortalSecurity.FilterFlag.NoAngleBrackets)]

        // NoMarkup
        [TestCase("<source></source>", "&lt;source&gt;&lt;/source&gt;", PortalSecurity.FilterFlag.NoMarkup)]
        [TestCase(
            "<source>Hi this is personal Test for source tag</source>   ",
            "&lt;source&gt;Hi this is personal Test for source tag&lt;/source&gt;   ",
            PortalSecurity.FilterFlag.NoMarkup)]
        [TestCase("<SOURCE>Upper case</SOURCE>", "&lt;SOURCE&gt;Upper case&lt;/SOURCE&gt;", PortalSecurity.FilterFlag.NoMarkup)]
        [TestCase(
            "<source src=\"https://google.com\">Source with attribute</source>",
            "&lt;source src=&quot;https://google.com&quot;&gt;Source with attribute&lt;/source&gt;",
            PortalSecurity.FilterFlag.NoMarkup)]

        // NoScripting
        [TestCase("<source></source>", " ", PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<source>Hi this is personal Test for source tag</source>   ", "    ", PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<SOURCE>Upper case</SOURCE>", " ", PortalSecurity.FilterFlag.NoScripting)]
        [TestCase("<source src=\"https://google.com\">Source with attribute</source>", " ", PortalSecurity.FilterFlag.NoScripting)]

        // NoAngleBrackets
        [TestCase("<source></source>", "<source></source>", PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase(
            "<source>Hi this is personal Test for source tag</source>   ",
            "<source>Hi this is personal Test for source tag</source>   ",
            PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase("<SOURCE>Upper case</SOURCE>", "<SOURCE>Upper case</SOURCE>",
            PortalSecurity.FilterFlag.NoAngleBrackets)]
        [TestCase(
            "<source src=\"https://google.com\">Source with attribute</source>",
            "<source src=\"https://google.com\">Source with attribute</source>",
            PortalSecurity.FilterFlag.NoAngleBrackets)]
        public void Html_Source_Tag_Should_Not_Be_Allowed(string html, string expectedOutput, PortalSecurity.FilterFlag markup)
        {
			Logger.LogDebug(">>> Entering Html_Source_Tag_Should_Not_Be_Allowed(html={Html},expectedOutput={ExpectedOutput},markup={Markup})", html, expectedOutput, markup);
            // Arrange
            var portalSecurity = this.CreatePortalSecurity();

			Logger.LogDebug("Html_Source_Tag_Should_Not_Be_Allowed(html={Html},expectedOutput={ExpectedOutput},markup={Markup}): portalSecurity → {PortalSecurity}", html, expectedOutput, markup, portalSecurity);
            // Act
            var filterOutput = portalSecurity.InputFilter(html, markup);

			Logger.LogDebug("Html_Source_Tag_Should_Not_Be_Allowed(html={Html},expectedOutput={ExpectedOutput},markup={Markup}): filterOutput → {FilterOutput}", html, expectedOutput, markup, filterOutput);
            // Assert
            Assert.That(expectedOutput, Is.EqualTo(filterOutput));
			Logger.LogDebug("<<< Exiting Html_Source_Tag_Should_Not_Be_Allowed(html={Html},expectedOutput={ExpectedOutput},markup={Markup})", html, expectedOutput, markup);
        }

        [TestCase("User\0name", "Username", PortalSecurity.FilterFlag.NoControlCharacters)]
        [TestCase("O'\0Example", "O'Example", PortalSecurity.FilterFlag.NoControlCharacters)]
        [TestCase("My\r\nUsername", "My Username", PortalSecurity.FilterFlag.NoControlCharacters)]
        [TestCase("My\rUsername", "My Username", PortalSecurity.FilterFlag.NoControlCharacters)]
        [TestCase("My\nUsername", "My Username", PortalSecurity.FilterFlag.NoControlCharacters)]
        [TestCase("My\tUsername", "My Username", PortalSecurity.FilterFlag.NoControlCharacters)]
        [TestCase("mail@example.com", "mail@example.com", PortalSecurity.FilterFlag.NoControlCharacters)]
        public void Control_Character_Should_Not_Be_Allowed(string html, string expectedOutput, PortalSecurity.FilterFlag markup)
        {
			Logger.LogDebug(">>> Entering Control_Character_Should_Not_Be_Allowed(html={Html},expectedOutput={ExpectedOutput},markup={Markup})", html, expectedOutput, markup);
            // Arrange
            var portalSecurity = this.CreatePortalSecurity();

			Logger.LogDebug("Control_Character_Should_Not_Be_Allowed(html={Html},expectedOutput={ExpectedOutput},markup={Markup}): portalSecurity → {PortalSecurity}", html, expectedOutput, markup, portalSecurity);
            // Act
            var filterOutput = portalSecurity.InputFilter(html, markup);

			Logger.LogDebug("Control_Character_Should_Not_Be_Allowed(html={Html},expectedOutput={ExpectedOutput},markup={Markup}): filterOutput → {FilterOutput}", html, expectedOutput, markup, filterOutput);
            // Assert
            Assert.That(expectedOutput, Is.EqualTo(filterOutput));
			Logger.LogDebug("<<< Exiting Control_Character_Should_Not_Be_Allowed(html={Html},expectedOutput={ExpectedOutput},markup={Markup})", html, expectedOutput, markup);
        }

        [Test]
        public void EncryptString_RoundTrips_ViaDecryptString_WithCoreCryptographyProvider()
        {
			Logger.LogDebug(">>> Entering EncryptString_RoundTrips_ViaDecryptString_WithCoreCryptographyProvider()");
            // Arrange
            var cryptographyProvider = new CoreCryptographyProvider();
            ComponentFactory.RegisterComponentInstance<CryptographyProvider>(cryptographyProvider);
			Logger.LogDebug("EncryptString_RoundTrips_ViaDecryptString_WithCoreCryptographyProvider(): cryptographyProvider → {CryptographyProvider}", cryptographyProvider);
            var portalSecurity = this.CreatePortalSecurity(cryptographyProvider);

			Logger.LogDebug("EncryptString_RoundTrips_ViaDecryptString_WithCoreCryptographyProvider(): portalSecurity → {PortalSecurity}", portalSecurity);
            // Act
            var passphrase = Guid.NewGuid().ToString();
            var encryptedMessage = portalSecurity.EncryptString("cat", passphrase);
			Logger.LogDebug("EncryptString_RoundTrips_ViaDecryptString_WithCoreCryptographyProvider(): passphrase → {Passphrase}", passphrase);
			Logger.LogDebug("EncryptString_RoundTrips_ViaDecryptString_WithCoreCryptographyProvider(): encryptedMessage → {EncryptedMessage}", encryptedMessage);
            var decryptedMessage = portalSecurity.DecryptString(encryptedMessage, passphrase);

			Logger.LogDebug("EncryptString_RoundTrips_ViaDecryptString_WithCoreCryptographyProvider(): decryptedMessage → {DecryptedMessage}", decryptedMessage);
            // Assert
            Assert.That(decryptedMessage, Is.EqualTo("cat"));
			Logger.LogDebug("<<< Exiting EncryptString_RoundTrips_ViaDecryptString_WithCoreCryptographyProvider()");
        }

        [Test]
        public void EncryptString_RoundTrips_ViaDecryptString_WithFipsCompilanceCryptographyProvider()
        {
			Logger.LogDebug(">>> Entering EncryptString_RoundTrips_ViaDecryptString_WithFipsCompilanceCryptographyProvider()");
            // Arrange
            var cryptographyProvider = new FipsCompilanceCryptographyProvider();
            ComponentFactory.RegisterComponentInstance<CryptographyProvider>(cryptographyProvider);
			Logger.LogDebug("EncryptString_RoundTrips_ViaDecryptString_WithFipsCompilanceCryptographyProvider(): cryptographyProvider → {CryptographyProvider}", cryptographyProvider);
            var portalSecurity = this.CreatePortalSecurity(cryptographyProvider);

			Logger.LogDebug("EncryptString_RoundTrips_ViaDecryptString_WithFipsCompilanceCryptographyProvider(): portalSecurity → {PortalSecurity}", portalSecurity);
            // Act
            var passphrase = Guid.NewGuid().ToString();
            var encryptedMessage = portalSecurity.EncryptString("cat", passphrase);
			Logger.LogDebug("EncryptString_RoundTrips_ViaDecryptString_WithFipsCompilanceCryptographyProvider(): passphrase → {Passphrase}", passphrase);
			Logger.LogDebug("EncryptString_RoundTrips_ViaDecryptString_WithFipsCompilanceCryptographyProvider(): encryptedMessage → {EncryptedMessage}", encryptedMessage);
            var decryptedMessage = portalSecurity.DecryptString(encryptedMessage, passphrase);

			Logger.LogDebug("EncryptString_RoundTrips_ViaDecryptString_WithFipsCompilanceCryptographyProvider(): decryptedMessage → {DecryptedMessage}", decryptedMessage);
            // Assert
            Assert.That(decryptedMessage, Is.EqualTo("cat"));
			Logger.LogDebug("<<< Exiting EncryptString_RoundTrips_ViaDecryptString_WithFipsCompilanceCryptographyProvider()");
        }

        private PortalSecurity CreatePortalSecurity(ICryptographyProvider cryptographyProvider = null)
        {
			Logger.LogDebug(">>> Entering CreatePortalSecurity(cryptographyProvider={CryptographyProvider})", cryptographyProvider);
			Logger.LogDebug("<<< Exiting CreatePortalSecurity(cryptographyProvider={CryptographyProvider})", cryptographyProvider);
            return new PortalSecurity(
                cryptographyProvider ?? this.serviceProvider.GetRequiredService<ICryptographyProvider>(),
                this.serviceProvider.GetRequiredService<ListController>(),
                this.serviceProvider.GetRequiredService<IPortalController>(),
                this.serviceProvider.GetRequiredService<IApplicationStatusInfo>(),
                this.serviceProvider.GetRequiredService<IPortalGroupController>());
        }
    }
}
