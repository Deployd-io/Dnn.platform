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
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class SmtpOAuthControllerTest
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [Test]
        public void Google_OAuth_Provider_Should_Exists()
        {
			_logger.LogDebug(">>> Entering Google_OAuth_Provider_Should_Exists()");
            var provider = CreateSmtpOAuthController().GetOAuthProvider("GoogleMail");

			_logger.LogDebug("Google_OAuth_Provider_Should_Exists(): provider → {Provider}", provider);
            Assert.That(provider, Is.Not.Null);
			_logger.LogDebug("<<< Exiting Google_OAuth_Provider_Should_Exists()");
        }

        [Test]
        public void Exchange_OAuth_Provider_Should_Exists()
        {
			_logger.LogDebug(">>> Entering Exchange_OAuth_Provider_Should_Exists()");
            var provider = CreateSmtpOAuthController().GetOAuthProvider("ExchangeOnline");

			_logger.LogDebug("Exchange_OAuth_Provider_Should_Exists(): provider → {Provider}", provider);
            Assert.That(provider, Is.Not.Null);
			_logger.LogDebug("<<< Exiting Exchange_OAuth_Provider_Should_Exists()");
        }

        [Test]
        public void NonExistent_OAuth_Provider_Should_Be_Null()
        {
			_logger.LogDebug(">>> Entering NonExistent_OAuth_Provider_Should_Be_Null()");
            var provider = CreateSmtpOAuthController().GetOAuthProvider("AnotherProvider");

			_logger.LogDebug("NonExistent_OAuth_Provider_Should_Be_Null(): provider → {Provider}", provider);
            Assert.That(provider, Is.Null);
			_logger.LogDebug("<<< Exiting NonExistent_OAuth_Provider_Should_Be_Null()");
        }

        private static SmtpOAuthController CreateSmtpOAuthController()
        {
			_logger.LogDebug(">>> Entering CreateSmtpOAuthController()");
			_logger.LogDebug("<<< Exiting CreateSmtpOAuthController()");
            return new SmtpOAuthController(
                new ISmtpOAuthProvider[]
                {
                    new GoogleMailOAuthProvider(Mock.Of<IHostSettingsService>(), Mock.Of<IPortalAliasService>()),
                    new ExchangeOnlineOAuthProvider(Mock.Of<IHostSettingsService>(), Mock.Of<IPortalAliasService>()),
                });
        }
    }
}
