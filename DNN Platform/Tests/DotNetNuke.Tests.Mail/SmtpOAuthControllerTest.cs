// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Mail
{
    using Dnn.ExchangeOnlineAuthProvider.Components;
    using Dnn.GoogleMailAuthProvider.Components;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Services.Mail.OAuth;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SmtpOAuthControllerTest
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void Google_OAuth_Provider_Should_Exists()
        {
			Logger.LogDebug(">>> Entering Google_OAuth_Provider_Should_Exists()");
            var provider = CreateSmtpOAuthController().GetOAuthProvider("GoogleMail");

			Logger.LogDebug("Google_OAuth_Provider_Should_Exists(): provider → {Provider}", provider);
            Assert.That(provider, Is.Not.Null);
			Logger.LogDebug("<<< Exiting Google_OAuth_Provider_Should_Exists()");
        }

        [Test]
        public void Exchange_OAuth_Provider_Should_Exists()
        {
			Logger.LogDebug(">>> Entering Exchange_OAuth_Provider_Should_Exists()");
            var provider = CreateSmtpOAuthController().GetOAuthProvider("ExchangeOnline");

			Logger.LogDebug("Exchange_OAuth_Provider_Should_Exists(): provider → {Provider}", provider);
            Assert.That(provider, Is.Not.Null);
			Logger.LogDebug("<<< Exiting Exchange_OAuth_Provider_Should_Exists()");
        }

        [Test]
        public void NonExistent_OAuth_Provider_Should_Be_Null()
        {
			Logger.LogDebug(">>> Entering NonExistent_OAuth_Provider_Should_Be_Null()");
            var provider = CreateSmtpOAuthController().GetOAuthProvider("AnotherProvider");

			Logger.LogDebug("NonExistent_OAuth_Provider_Should_Be_Null(): provider → {Provider}", provider);
            Assert.That(provider, Is.Null);
			Logger.LogDebug("<<< Exiting NonExistent_OAuth_Provider_Should_Be_Null()");
        }

        private static SmtpOAuthController CreateSmtpOAuthController()
        {
			Logger.LogDebug(">>> Entering CreateSmtpOAuthController()");
			Logger.LogDebug("<<< Exiting CreateSmtpOAuthController()");
            return new SmtpOAuthController(
                new ISmtpOAuthProvider[]
                {
                    new GoogleMailOAuthProvider(Mock.Of<IHostSettingsService>(), Mock.Of<IPortalAliasService>()),
                    new ExchangeOnlineOAuthProvider(Mock.Of<IHostSettingsService>(), Mock.Of<IPortalAliasService>()),
                });
        }
    }
}
