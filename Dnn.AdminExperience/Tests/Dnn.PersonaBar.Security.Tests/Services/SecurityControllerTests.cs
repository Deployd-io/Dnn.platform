// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Tests.Services
{
    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Security.Services;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.Api.Auth.ApiTokens;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SecurityControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string SomeAlias = "some.domain.com:8080";
        private const string SslUrl = "https://" + SomeAlias + "/";
        private const int PortalId = 0;

        [Test]
        public void Services_Controller_AddPortalAlias_TrimsProtocolAndSlash()
        {
			Logger.LogDebug(">>> Entering Services_Controller_AddPortalAlias_TrimsProtocolAndSlash()");
            // arrange
            var sut = new SecurityController(
                Mock.Of<Components.SecurityController>(),
                Mock.Of<IPagesController>(),
                Mock.Of<IPortalAliasService>(),
                Mock.Of<IApiTokenController>(),
                Mock.Of<IHostSettingsService>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IHostSettings>());

            // act
            var alias = sut.AddPortalAlias(SslUrl, PortalId);

			Logger.LogDebug("Services_Controller_AddPortalAlias_TrimsProtocolAndSlash(): alias → {Alias}", alias);
            // assert
            Assert.That(alias, Is.EqualTo(SomeAlias));
			Logger.LogDebug("<<< Exiting Services_Controller_AddPortalAlias_TrimsProtocolAndSlash()");
        }

        [Test]
        public void Services_Controller_AddPortalAlias_WhenAliasExists_AddIsNotInvoked()
        {
			Logger.LogDebug(">>> Entering Services_Controller_AddPortalAlias_WhenAliasExists_AddIsNotInvoked()");
            // arrange
            var portalAliasControllerMock = new Mock<IPortalAliasService>();
            var portalAliasInfo = new PortalAliasInfo
			Logger.LogDebug("Services_Controller_AddPortalAlias_WhenAliasExists_AddIsNotInvoked(): portalAliasControllerMock → {PortalAliasControllerMock}", portalAliasControllerMock);
            {
                HTTPAlias = SomeAlias,
                PortalID = PortalId,
            };
            portalAliasControllerMock
                .Setup(c => c.GetPortalAlias(SomeAlias, PortalId))
                .Returns(() => portalAliasInfo);
            var sut = new SecurityController(
                Mock.Of<Components.SecurityController>(),
                Mock.Of<IPagesController>(),
                portalAliasControllerMock.Object,
                Mock.Of<IApiTokenController>(),
                Mock.Of<IHostSettingsService>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IHostSettings>());

            // act
            sut.AddPortalAlias(SslUrl, PortalId);

            // assert
            portalAliasControllerMock.Verify(
                c =>
                c.AddPortalAlias(It.Is<IPortalAliasInfo>(match =>
                    match.HttpAlias == SomeAlias && match.PortalId == PortalId)), Times.Never);
					Logger.LogDebug("<<< Exiting Services_Controller_AddPortalAlias_WhenAliasExists_AddIsNotInvoked()");
        }

        [Test]
        public void Services_Controller_AddPortalAlias_WhenAliasDoesNotExist_AddIsInvoked()
        {
			Logger.LogDebug(">>> Entering Services_Controller_AddPortalAlias_WhenAliasDoesNotExist_AddIsInvoked()");
            // arrange
            var portalAliasControllerMock = new Mock<IPortalAliasService>();
            portalAliasControllerMock
				Logger.LogDebug("Services_Controller_AddPortalAlias_WhenAliasDoesNotExist_AddIsInvoked(): portalAliasControllerMock → {PortalAliasControllerMock}", portalAliasControllerMock);
                .Setup(c => c.GetPortalAlias(SomeAlias, PortalId))
                .Returns(() => null);
            var sut = new SecurityController(
                Mock.Of<Components.SecurityController>(),
                Mock.Of<IPagesController>(),
                portalAliasControllerMock.Object,
                Mock.Of<IApiTokenController>(),
                Mock.Of<IHostSettingsService>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IHostSettings>());

            // act
            sut.AddPortalAlias(SslUrl, PortalId);

            // assert
            portalAliasControllerMock.Verify(
                c =>
                c.AddPortalAlias(It.Is<IPortalAliasInfo>(match =>
                    match.HttpAlias == SomeAlias && match.PortalId == PortalId)), Times.Once);
					Logger.LogDebug("<<< Exiting Services_Controller_AddPortalAlias_WhenAliasDoesNotExist_AddIsInvoked()");
        }
    }
}
