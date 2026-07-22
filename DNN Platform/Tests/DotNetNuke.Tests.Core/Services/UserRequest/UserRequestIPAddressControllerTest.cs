// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.UserRequest
{
    using System.Collections.Specialized;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class UserRequestIPAddressControllerTest
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<IPortalController> mockPortalController;
        private Mock<IHostSettingsService> hostSettingsService;
        private Mock<HttpRequestBase> mockRequest;

        private UserRequestIPAddressController userRequestIPAddressController;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            // Setup Mock
            this.hostSettingsService = new Mock<IHostSettingsService>();
			Logger.LogDebug("Setup(): hostSettingsService → {HostSettingsService}", hostSettingsService);
            this.mockPortalController = MockComponentProvider.CreateNew<IPortalController>();
			Logger.LogDebug("Setup(): mockPortalController → {MockPortalController}", mockPortalController);
            PortalController.SetTestableInstance(this.mockPortalController.Object);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("Setup(): services → {Services}", services);
                    services.AddSingleton(this.mockPortalController.Object);
                    services.AddSingleton(this.hostSettingsService.Object);
                    services.AddSingleton((IHostSettingsService)this.hostSettingsService.Object);
                });
            var mockHttpContext = Mock.Get(HttpContextSource.Current);
            this.mockRequest = Mock.Get(mockHttpContext.Object.Request);
			Logger.LogDebug("Setup(): mockHttpContext → {MockHttpContext}", mockHttpContext);
			Logger.LogDebug("Setup(): mockRequest → {MockRequest}", mockRequest);
            this.mockRequest.Setup(x => x.ServerVariables).Returns(new NameValueCollection());

            // System under test
            this.userRequestIPAddressController = new UserRequestIPAddressController();
			Logger.LogDebug("<<< Exiting Setup()");
			Logger.LogDebug("Setup(): userRequestIPAddressController → {UserRequestIPAddressController}", userRequestIPAddressController);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            MockComponentProvider.ResetContainer();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [TestCase("111.111.111.111", "X-Forwarded-For")]
        [TestCase("111.111.111.111,123.112.11.33", "X-Forwarded-For")]
        [TestCase("111.111.111.111", "X-ProxyUser-Ip")]
        [TestCase("111.111.111.111,23.112.11.33", "X-ProxyUser-Ip")]
        public void UserRequestIPAddress_ShouldReturnIP_IfAnyHeaderIsPresent(string requestIp, string headerName)
        {
            // Arrange
            var expectedIp = "111.111.111.111";

            NameValueCollection headersWithXForwardedHeaders = new NameValueCollection();
            headersWithXForwardedHeaders.Add(headerName, requestIp);
            this.hostSettingsService.As<IHostSettingsService>().Setup(hc => hc.GetString(It.IsAny<string>())).Returns(headerName);
            this.mockRequest.Setup(x => x.Headers).Returns(headersWithXForwardedHeaders);

            // Act
            string userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockRequest.Object);

            // Assert
            Assert.That(userRequestIPAddress, Is.EqualTo(expectedIp));
        }

        [Test]
        public void UserRequestIPAddress_ShouldReturnIP_IfRemoteAddrServerVariablePresent()
        {
			Logger.LogDebug(">>> Entering UserRequestIPAddress_ShouldReturnIP_IfRemoteAddrServerVariablePresent()");
            // Arrange
            var expectedIp = "111.111.111.111";
            var remoteVariable = "REMOTE_ADDR";
			Logger.LogDebug("UserRequestIPAddress_ShouldReturnIP_IfRemoteAddrServerVariablePresent(): expectedIp → {ExpectedIp}", expectedIp);
			Logger.LogDebug("UserRequestIPAddress_ShouldReturnIP_IfRemoteAddrServerVariablePresent(): remoteVariable → {RemoteVariable}", remoteVariable);
            var requestIp = "111.111.111.111";

			Logger.LogDebug("UserRequestIPAddress_ShouldReturnIP_IfRemoteAddrServerVariablePresent(): requestIp → {RequestIp}", requestIp);
            NameValueCollection serverVariables = new NameValueCollection();
            serverVariables.Add(remoteVariable, requestIp);
			Logger.LogDebug("UserRequestIPAddress_ShouldReturnIP_IfRemoteAddrServerVariablePresent(): serverVariables → {ServerVariables}", serverVariables);
            this.mockRequest.Setup(x => x.ServerVariables).Returns(serverVariables);

            // Act
            var userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockRequest.Object);

			Logger.LogDebug("UserRequestIPAddress_ShouldReturnIP_IfRemoteAddrServerVariablePresent(): userRequestIPAddress → {UserRequestIPAddress}", userRequestIPAddress);
            // Assert
            Assert.That(userRequestIPAddress, Is.SameAs(expectedIp));
            this.mockRequest.VerifyGet(r => r.ServerVariables);
			Logger.LogDebug("<<< Exiting UserRequestIPAddress_ShouldReturnIP_IfRemoteAddrServerVariablePresent()");
        }

        [Test]
        public void UserRequestIPAddress_ShouldReturnIP_IfUserHostAddress()
        {
			Logger.LogDebug(">>> Entering UserRequestIPAddress_ShouldReturnIP_IfUserHostAddress()");
            // Arrange
            var expectedIp = "111.111.111.111";
            this.mockRequest.Setup(x => x.UserHostAddress).Returns(expectedIp);
			Logger.LogDebug("UserRequestIPAddress_ShouldReturnIP_IfUserHostAddress(): expectedIp → {ExpectedIp}", expectedIp);

            // Act
            var userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockRequest.Object);

			Logger.LogDebug("UserRequestIPAddress_ShouldReturnIP_IfUserHostAddress(): userRequestIPAddress → {UserRequestIPAddress}", userRequestIPAddress);
            // Assert
            Assert.That(userRequestIPAddress, Is.SameAs(expectedIp));
            this.mockRequest.VerifyGet(r => r.UserHostAddress);
			Logger.LogDebug("<<< Exiting UserRequestIPAddress_ShouldReturnIP_IfUserHostAddress()");
        }

        [TestCase("abc.111.eer")]
        [TestCase("somedomain.com")]
        [TestCase("244.275.111.111")]
        public void UserRequestIPAddress_ShouldReturnEmptyString_IfIPAddressIsNotValid(string requestIp)
        {
            // Arrange
            var headerName = "X-Forwarded-For";

            NameValueCollection headersWithXForwardedHeaders = new NameValueCollection();
            headersWithXForwardedHeaders.Add(headerName, requestIp);
            this.mockRequest.Setup(x => x.Headers).Returns(headersWithXForwardedHeaders);
            this.hostSettingsService.Setup(hc => hc.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns(headerName);

            // Act
            var userRequestIPAddress = this.userRequestIPAddressController.GetUserRequestIPAddress(this.mockRequest.Object);

            // Assert
            Assert.That(userRequestIPAddress, Is.SameAs(string.Empty));
        }
    }
}
