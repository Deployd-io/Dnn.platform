// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;

    using Dnn.AuthServices.Jwt.Auth;
    using Dnn.AuthServices.Jwt.Components.Entity;
    using Dnn.AuthServices.Jwt.Data;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class JwtAuthMessageHandlerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        // { "type":"JWT","alg":"HS256"} . {"sub":"host","nbf":1,"exp":4102444799,"sid":"0123456789ABCDEF"} . (HS256_KEY="secret")
        private const string ValidToken =
            "eyJ0eXBlIjoiSldUIiwiYWxnIjoiSFMyNTYifQ.eyJzdWIiOiJ1c2VybmFtZSIsIm5iZiI6MSwiZXhwIjo0MTAyNDQ0Nzk5LCJzaWQiOiIwMTIzNDU2Nzg5QUJDREVGIn0.nfWCOVNk5M7L7EPDe3i3j4aAPRerbxgmcjOxaC-LWUQ";

        private Mock<DataProvider> mockDataProvider;
        private Mock<MembershipProvider> mockMembership;
        private Mock<Entities.Portals.Data.IDataService> mockDataService;
        private Mock<IDataService> mockJwtDataService;
        private Mock<IUserController> mockUserController;
        private Mock<IPortalController> mockPortalController;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetupMockServices()
        {
			Logger.LogDebug(">>> Entering SetupMockServices()");
            MockComponentProvider.CreateDataCacheProvider();

            this.mockDataService = new Mock<Entities.Portals.Data.IDataService>();
			Logger.LogDebug("SetupMockServices(): mockDataService → {MockDataService}", mockDataService);
            this.mockDataProvider = MockComponentProvider.CreateDataProvider();
			Logger.LogDebug("SetupMockServices(): mockDataProvider → {MockDataProvider}", mockDataProvider);
            this.mockPortalController = new Mock<IPortalController>();
			Logger.LogDebug("SetupMockServices(): mockPortalController → {MockPortalController}", mockPortalController);
            this.mockUserController = new Mock<IUserController>();
			Logger.LogDebug("SetupMockServices(): mockUserController → {MockUserController}", mockUserController);
            this.mockMembership = MockComponentProvider.CreateNew<MembershipProvider>();
			Logger.LogDebug("SetupMockServices(): mockMembership → {MockMembership}", mockMembership);

            this.mockDataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);
            this.mockDataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(GetPortalsCallBack);
            this.mockDataProvider.Setup(d => d.GetUser(It.IsAny<int>(), It.IsAny<int>())).Returns(GetUser);
            this.mockDataService.Setup(ds => ds.GetPortalGroups()).Returns(this.GetPortalGroups);
            Entities.Portals.Data.DataService.RegisterInstance(this.mockDataService.Object);

            this.mockMembership.Setup(m => m.PasswordRetrievalEnabled).Returns(true);
            this.mockMembership.Setup(m => m.GetUser(It.IsAny<int>(), It.IsAny<int>())).Returns((int portalId, int userId) => GetUserByIdCallback(portalId, userId));

            this.mockJwtDataService = new Mock<IDataService>();
			Logger.LogDebug("SetupMockServices(): mockJwtDataService → {MockJwtDataService}", mockJwtDataService);
            this.mockJwtDataService.Setup(x => x.GetTokenById(It.IsAny<string>())).Returns((string sid) => GetPersistedToken(sid));
            DataService.RegisterInstance(this.mockJwtDataService.Object);

            PortalController.SetTestableInstance(this.mockPortalController.Object);
            this.mockPortalController.Setup(x => x.GetPortal(It.IsAny<int>())).Returns(
                new PortalInfo { PortalID = 0, PortalGroupID = -1, UserTabId = 55 });
            this.mockPortalController.Setup(x => x.GetPortalSettings(It.IsAny<int>())).Returns((int portalId) => new Dictionary<string, string>());
            this.mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(() => new PortalSettings());

            UserController.SetTestableInstance(this.mockUserController.Object);
            this.mockUserController.Setup(x => x.GetUserById(It.IsAny<int>(), It.IsAny<int>())).Returns(
                (int portalId, int userId) => GetUserByIdCallback(portalId, userId));
            ////mockUserController.Setup(x => x.ValidateUser(It.IsAny<UserInfo>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(UserValidStatus.VALID);

            this.serviceProvider =
                FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));
				Logger.LogDebug("<<< Exiting SetupMockServices()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void ReturnsResponseAsReceived()
        {
			Logger.LogDebug(">>> Entering ReturnsResponseAsReceived()");
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = new HttpRequestMessage() };

			Logger.LogDebug("ReturnsResponseAsReceived(): response → {Response}", response);
            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response2 = handler.OnOutboundResponse(response, CancellationToken.None);
			Logger.LogDebug("ReturnsResponseAsReceived(): handler → {Handler}", handler);
			Logger.LogDebug("ReturnsResponseAsReceived(): response2 → {Response2}", response2);

            // Assert
            Assert.That(response2, Is.EqualTo(response));
			Logger.LogDebug("<<< Exiting ReturnsResponseAsReceived()");
        }

        [Test]
        public void MissingAuthorizationHeaderReturnsNullResponse()
        {
			Logger.LogDebug(">>> Entering MissingAuthorizationHeaderReturnsNullResponse()");
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");

			Logger.LogDebug("MissingAuthorizationHeaderReturnsNullResponse(): request → {Request}", request);
            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);
			Logger.LogDebug("MissingAuthorizationHeaderReturnsNullResponse(): handler → {Handler}", handler);
			Logger.LogDebug("MissingAuthorizationHeaderReturnsNullResponse(): response → {Response}", response);

            // Assert
            Assert.That(response, Is.Null);
			Logger.LogDebug("<<< Exiting MissingAuthorizationHeaderReturnsNullResponse()");
        }

        [Test]
        public void WrongAuthorizationSchemeReturnsNullResponse()
        {
			Logger.LogDebug(">>> Entering WrongAuthorizationSchemeReturnsNullResponse()");
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Basic ");
			Logger.LogDebug("WrongAuthorizationSchemeReturnsNullResponse(): request → {Request}", request);

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);
			Logger.LogDebug("WrongAuthorizationSchemeReturnsNullResponse(): handler → {Handler}", handler);
			Logger.LogDebug("WrongAuthorizationSchemeReturnsNullResponse(): response → {Response}", response);

            // Assert
            Assert.That(response, Is.Null);
			Logger.LogDebug("<<< Exiting WrongAuthorizationSchemeReturnsNullResponse()");
        }

        [Test]
        public void MissingAuthorizationTokenReturnsNullResponse()
        {
			Logger.LogDebug(">>> Entering MissingAuthorizationTokenReturnsNullResponse()");
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer ");
			Logger.LogDebug("MissingAuthorizationTokenReturnsNullResponse(): request → {Request}", request);

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);
			Logger.LogDebug("MissingAuthorizationTokenReturnsNullResponse(): handler → {Handler}", handler);
			Logger.LogDebug("MissingAuthorizationTokenReturnsNullResponse(): response → {Response}", response);

            // Assert
            Assert.That(response, Is.Null);
			Logger.LogDebug("<<< Exiting MissingAuthorizationTokenReturnsNullResponse()");
        }

        [Test]
        public void AuthorizationTokenWithMissingComponentsReturnsNullResponse()
        {
			Logger.LogDebug(">>> Entering AuthorizationTokenWithMissingComponentsReturnsNullResponse()");
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(
                "Bearer eyJ0eXBlIjoiSldUIiwiYWxnIjoiSFMyNTYifQ.eyJzdWIiOiJ1c2VybmFtZSIsIm5iZiI6MSwiZXhwIjo0MTAyNDQ0Nzk5LCJzaWQiOiIwMTIzNDU2Nzg5QUJDREVGIn0");
				Logger.LogDebug("AuthorizationTokenWithMissingComponentsReturnsNullResponse(): request → {Request}", request);

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);
			Logger.LogDebug("AuthorizationTokenWithMissingComponentsReturnsNullResponse(): handler → {Handler}", handler);
			Logger.LogDebug("AuthorizationTokenWithMissingComponentsReturnsNullResponse(): response → {Response}", response);

            // Assert
            Assert.That(response, Is.Null);
			Logger.LogDebug("<<< Exiting AuthorizationTokenWithMissingComponentsReturnsNullResponse()");
        }

        [Test]
        public void AuthorizationTokenWithWrongSchemeTypeReturnsNullResponse()
        {
			Logger.LogDebug(">>> Entering AuthorizationTokenWithWrongSchemeTypeReturnsNullResponse()");
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(
                "Bearer eyJ0eXBlIjoieHh4IiwiYWxnIjoiSFMyNTYifQ.eyJzdWIiOiJ1c2VybmFtZSIsIm5iZiI6MSwiZXhwIjo0MTAyNDQ0Nzk5LCJzaWQiOiIwMTIzNDU2Nzg5QUJDREVGIn0.nfWCOVNk5M7L7EPDe3i3j4aAPRerbxgmcjOxaC-LWUQ");
				Logger.LogDebug("AuthorizationTokenWithWrongSchemeTypeReturnsNullResponse(): request → {Request}", request);

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);
			Logger.LogDebug("AuthorizationTokenWithWrongSchemeTypeReturnsNullResponse(): handler → {Handler}", handler);
			Logger.LogDebug("AuthorizationTokenWithWrongSchemeTypeReturnsNullResponse(): response → {Response}", response);

            // Assert
            Assert.That(response, Is.Null);
			Logger.LogDebug("<<< Exiting AuthorizationTokenWithWrongSchemeTypeReturnsNullResponse()");
        }

        [Test]
        public void AuthorizationTokenWithoutResponse()
        {
			Logger.LogDebug(">>> Entering AuthorizationTokenWithoutResponse()");
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer " + ValidToken);
			Logger.LogDebug("AuthorizationTokenWithoutResponse(): request → {Request}", request);

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);
			Logger.LogDebug("AuthorizationTokenWithoutResponse(): handler → {Handler}", handler);
			Logger.LogDebug("AuthorizationTokenWithoutResponse(): response → {Response}", response);

            // Assert
            Assert.That(response, Is.Null);
			Logger.LogDebug("<<< Exiting AuthorizationTokenWithoutResponse()");
        }

        private static IDataReader GetUser()
        {
			Logger.LogDebug(">>> Entering GetUser()");
            var table = new DataTable("Users");
            table.Columns.Add("UserID", typeof(int));
			Logger.LogDebug("GetUser(): table → {Table}", table);
            table.Columns.Add("PortalId", typeof(int));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("LastName", typeof(string));
            table.Columns.Add("DisplayName", typeof(string));
            table.Columns.Add("IsSuperUser", typeof(byte));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("VanityUrl", typeof(string));
            table.Columns.Add("AffiliateId", typeof(int));
            table.Columns.Add("IsDeleted", typeof(byte));
            table.Columns.Add("RefreshRoles", typeof(byte));
            table.Columns.Add("LastIPAddress", typeof(string));
            table.Columns.Add("UpdatePassword", typeof(byte));
            table.Columns.Add("PasswordResetToken", typeof(Guid));
            table.Columns.Add("PasswordResetExpiration", typeof(DateTime));
            table.Columns.Add("Authorised", typeof(byte));

            table.Columns.Add("CreatedByUserID", typeof(int));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("LastModifiedByUserID", typeof(int));
            table.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            table.Rows.Add(1, null, "host", "host", "host", "host", 1, "host@changeme.invalid", null, null, 0, null,
                           "127.0.0.1", 0, "8D3C800F-7A40-45D6-BA4D-E59A393F9800", DateTime.Now, null, -1, DateTime.Now,
                           -1, DateTime.Now);
			Logger.LogDebug("<<< Exiting GetUser()");
            return table.CreateDataReader();
        }

        private static IDataReader GetPortalsCallBack(string culture)
        {
			Logger.LogDebug(">>> Entering GetPortalsCallBack(culture={Culture})", culture);
			Logger.LogDebug("<<< Exiting GetPortalsCallBack(culture={Culture})", culture);
            return GetPortalCallBack(0, culture);
        }

        private static IDataReader GetPortalCallBack(int portalId, string culture)
        {
			Logger.LogDebug(">>> Entering GetPortalCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            var table = new DataTable("Portal");

			Logger.LogDebug("GetPortalCallBack(portalId={PortalId},culture={Culture}): table → {Table}", portalId, culture, table);
            var cols = new[]
            {
                "PortalID", "PortalGroupID", "PortalName", "LogoFile", "FooterText", "ExpiryDate",
                "UserRegistration", "BannerAdvertising", "AdministratorId", "Currency", "HostFee",
                "HostSpace", "PageQuota", "UserQuota", "AdministratorRoleId", "RegisteredRoleId",
                "Description", "KeyWords", "BackgroundFile", "GUID", "PaymentProcessor",
                "ProcessorUserId",
                "ProcessorPassword", "SiteLogHistory", "Email", "DefaultLanguage", "TimezoneOffset",
                "AdminTabId", "HomeDirectory", "SplashTabId", "HomeTabId", "LoginTabId", "RegisterTabId",
                "UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "TermsTabId", "PrivacyTabId", "SuperTabId",
                "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate",
                "CultureCode",
            };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            const int homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright (c) 2018 DNN Corp.", null,
                "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website",
                "DotNetNuke, DNN, Content, Management, CMS", null, "1057AC7A-3C08-4849-A3A6-3D2AB4662020",
                null, null, null, "0", "admin@changeme.invalid", "en-US", "-8", "58", "Portals/0", null,
                homePage.ToString(), null, null, "57", "56", "-1", "-1", null, null, "7", "-1", "2011-08-25 07:34:11",
                "-1", "2011-08-25 07:34:29", culture);

			Logger.LogDebug("<<< Exiting GetPortalCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            return table.CreateDataReader();
        }

        private static PersistedToken GetPersistedToken(string sessionId)
        {
			Logger.LogDebug(">>> Entering GetPersistedToken(sessionId={SessionId})", sessionId);
            if ("0123456789ABCDEF".Equals(sessionId))
            {
                return new PersistedToken
                {
                    TokenId = sessionId,
                    RenewalExpiry = DateTime.UtcNow.AddDays(14),
                    TokenExpiry = DateTime.UtcNow.AddHours(1),
                    UserId = 1,
                    TokenHash = GetHashedStr(ValidToken),
                    RenewalHash = "renewal-hash",
                };
            }

			Logger.LogDebug("<<< Exiting GetPersistedToken(sessionId={SessionId})", sessionId);
            return null;
        }

        private static UserInfo GetUserByIdCallback(int portalId, int userId)
        {
			Logger.LogDebug(">>> Entering GetUserByIdCallback(portalId={PortalId},userId={UserId})", portalId, userId);
            if (portalId == 0)
            {
                switch (userId)
                {
                    case 1:
                        return new UserInfo { UserID = userId, Username = "host", DisplayName = "Host User" };
                    case 2:
                        return new UserInfo { UserID = userId, Username = "admin", DisplayName = "Admin User" };
                    case 3:
                        return new UserInfo { UserID = userId, Username = "reguser", DisplayName = "Registered User" };
                }
            }

			Logger.LogDebug("<<< Exiting GetUserByIdCallback(portalId={PortalId},userId={UserId})", portalId, userId);
            return null;
        }

        // todo unit test actual authentication code
        // very hard to unit test inbound authentication code as it dips into untestable bits of
        // UserController, etc. Need to write controllers with interfaces and ServiceLocator<>.

        // private static string DecodeBase64(string b64Str)
        // {
        //    // fix Base64 string padding
        //    var mod = b64Str.Length % 4;
        //    if (mod != 0) b64Str += new string('=', 4 - mod);
        //    return Encoding.UTF8.GetString(Convert.FromBase64String(b64Str));
        // }
        private static string EncodeBase64(byte[] data)
        {
			Logger.LogDebug(">>> Entering EncodeBase64(data={Data})", data);
			Logger.LogDebug("<<< Exiting EncodeBase64(data={Data})", data);
            return Convert.ToBase64String(data).TrimEnd('=');
        }

        private static string GetHashedStr(string data)
        {
			Logger.LogDebug(">>> Entering GetHashedStr(data={Data})", data);
            using (var hasher = SHA384.Create())
            {
				Logger.LogDebug("<<< Exiting GetHashedStr(data={Data})", data);
                return EncodeBase64(hasher.ComputeHash(Encoding.UTF8.GetBytes(data)));
            }
        }

        private IDataReader GetPortalGroups()
        {
			Logger.LogDebug(">>> Entering GetPortalGroups()");
            var table = new DataTable("ModuleDefinitions");
            var pkId = table.Columns.Add("PortalGroupID", typeof(int));
			Logger.LogDebug("GetPortalGroups(): table → {Table}", table);
			Logger.LogDebug("GetPortalGroups(): pkId → {PkId}", pkId);
            table.Columns.Add("MasterPortalID", typeof(int));
            table.Columns.Add("PortalGroupName", typeof(string));
            table.Columns.Add("PortalGroupDescription", typeof(string));
            table.Columns.Add("AuthenticationDomain", typeof(string));
            table.Columns.Add("CreatedByUserID", typeof(int));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("LastModifiedByUserID", typeof(int));
            table.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            table.PrimaryKey = new[] { pkId };
			Logger.LogDebug("GetPortalGroups(): table.PrimaryKey → {Arg0}", table.PrimaryKey);

            table.Rows.Add(0, 0, "test", "descr", "domain", -1, DateTime.Now, -1, DateTime.Now);
			Logger.LogDebug("<<< Exiting GetPortalGroups()");
            return table.CreateDataReader();
        }
    }
}
