// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.PersonaBar.Manage.Users
{
    using System;
    using System.Threading.Tasks;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class UsersFiltersTests : IntegrationTestBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string UnauthorizeApi = "/API/PersonaBar/Users/UpdateAuthorizeStatus?userId={0}&authorized=false";
        private const string DeleteApi = "/API/PersonaBar/Users/SoftDeleteUser?userId={0}";
        private const string MakeAdminApi = "/API/PersonaBar/Users/SaveUserRole?notifyUser=true&isOwner=false";

        private const int MaxUsers = 8;
        private int[] _userIds = new int[MaxUsers];
        private string[] _userNames = new string[MaxUsers];

        [OneTimeSetUp]
        public override async Task TestFixtureSetUp()
        {
			Logger.LogDebug(">>> Entering TestFixtureSetUp()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // clear all existing users except the HOST superuser
            DatabaseHelper.ExecuteNonQuery("DELETE FROM {objectQualifier}UserRelationships");
			Logger.LogInformation("TestFixtureSetUp(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            DatabaseHelper.ExecuteNonQuery("DELETE FROM {objectQualifier}UserPortals");
            DatabaseHelper.ExecuteNonQuery("DELETE FROM {objectQualifier}Users WHERE UserID > 1");

            // create MaxUsers new users
            for (var i = 0; i < MaxUsers; i++)
            {
                int userId, fileId;
                string userName;
                (_, userId, userName, fileId) = await WebApiTestHelper.PrepareNewUser();
                this._userIds[i] = userId;
                this._userNames[i] = userName;
                Console.WriteLine(@"Created test users => id: {0}, username: {1}", userId, userName);
            }

            var hostConnector = WebApiTestHelper.LoginHost();
            var userIdx = 0;
			Logger.LogDebug("TestFixtureSetUp(): hostConnector → {HostConnector}", hostConnector);

            // make first user as admin
            var makeAdminItem = new { RoleId = 0, UserId = this._userIds[userIdx] };
            var response = hostConnector.PostJson(MakeAdminApi, makeAdminItem).Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(response);
			Logger.LogDebug("TestFixtureSetUp(): response → {Response}", response);
			Logger.LogDebug("TestFixtureSetUp(): result → {Result}", result);
            Assert.That(result.displayName.ToString(), Is.EqualTo(this._userNames[userIdx]));

            // Unauthorize the next 2 new users
            for (userIdx = 1; userIdx <= 2; userIdx++)
            {
                var unauthorizeLink = string.Format(UnauthorizeApi, this._userIds[userIdx]);
                response = hostConnector.PostJson(unauthorizeLink, string.Empty).Content.ReadAsStringAsync().Result;
				Logger.LogDebug("TestFixtureSetUp(): unauthorizeLink → {UnauthorizeLink}", unauthorizeLink);
                result = JsonConvert.DeserializeObject<dynamic>(response);
                Assert.That(bool.Parse(result.Success.ToString()), Is.True);
            }

            // soft delete the next new user
            var deleteLink = string.Format(DeleteApi, this._userIds[userIdx]);
            response = hostConnector.PostJson(deleteLink, string.Empty).Content.ReadAsStringAsync().Result;
			Logger.LogDebug("TestFixtureSetUp(): deleteLink → {DeleteLink}", deleteLink);
            result = JsonConvert.DeserializeObject<dynamic>(response);
            Assert.That(bool.Parse(result.Success.ToString()), Is.True);
			Logger.LogDebug("<<< Exiting TestFixtureSetUp()");
        }

        [TestCase("All", MaxUsers, "/API/PersonaBar/Users/GetUsers?searchText=&filter=5&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Authorized", MaxUsers - 3, "/API/PersonaBar/Users/GetUsers?searchText=&filter=0&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Unauthorized", 2, "/API/PersonaBar/Users/GetUsers?searchText=&filter=1&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Deleted", 1, "/API/PersonaBar/Users/GetUsers?searchText=&filter=2&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Superusers", 1, "/API/PersonaBar/Users/GetUsers?searchText=&filter=3&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        public void GetUsersAsHostWithVariousFiltersShoudlReturnExpectedResult(string actionName, int expectedTotal, string apiMethod)
        {
			Logger.LogDebug(">>> Entering GetUsersAsHostWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod})", actionName, expectedTotal, apiMethod);
            // Arrange: all is done in TestFixtureSetUp()

            // Act
            var hostConnector = WebApiTestHelper.LoginHost();
            var response = hostConnector.GetContent(apiMethod, null).Content.ReadAsStringAsync().Result;
			Logger.LogDebug("GetUsersAsHostWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod}): hostConnector → {HostConnector}", actionName, expectedTotal, apiMethod, hostConnector);
			Logger.LogDebug("GetUsersAsHostWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod}): response → {Response}", actionName, expectedTotal, apiMethod, response);
            var result = JsonConvert.DeserializeObject<dynamic>(response);

			Logger.LogDebug("GetUsersAsHostWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod}): result → {Result}", actionName, expectedTotal, apiMethod, result);
            // Assert
            var totalResults = int.Parse(result.TotalResults.ToString());
            Assert.That(totalResults, Is.EqualTo(expectedTotal), $"Total results {totalResults} is incorrect for action [{actionName}]");
			Logger.LogDebug("<<< Exiting GetUsersAsHostWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod})", actionName, expectedTotal, apiMethod);
			Logger.LogDebug("GetUsersAsHostWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod}): totalResults → {TotalResults}", actionName, expectedTotal, apiMethod, totalResults);
        }

        [TestCase("All", MaxUsers, "/API/PersonaBar/Users/GetUsers?searchText=&filter=5&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Authorized", MaxUsers - 3, "/API/PersonaBar/Users/GetUsers?searchText=&filter=0&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Unauthorized", 2, "/API/PersonaBar/Users/GetUsers?searchText=&filter=1&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Deleted", 1, "/API/PersonaBar/Users/GetUsers?searchText=&filter=2&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Superusers", 0, "/API/PersonaBar/Users/GetUsers?searchText=&filter=3&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        public void GetUsersAsAdminWithVariousFiltersShoudlReturnExpectedResult(string actionName, int expectedTotal, string apiMethod)
        {
			Logger.LogDebug(">>> Entering GetUsersAsAdminWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod})", actionName, expectedTotal, apiMethod);
            // Arrange: all is done in TestFixtureSetUp()

            // Act
            var adminConnector = WebApiTestHelper.LoginUser(this._userNames[0]);
            var response = adminConnector.GetContent(apiMethod, null).Content.ReadAsStringAsync().Result;
			Logger.LogDebug("GetUsersAsAdminWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod}): adminConnector → {AdminConnector}", actionName, expectedTotal, apiMethod, adminConnector);
			Logger.LogDebug("GetUsersAsAdminWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod}): response → {Response}", actionName, expectedTotal, apiMethod, response);
            var result = JsonConvert.DeserializeObject<dynamic>(response);

			Logger.LogDebug("GetUsersAsAdminWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod}): result → {Result}", actionName, expectedTotal, apiMethod, result);
            // Assert
            var totalResults = int.Parse(result.TotalResults.ToString());
            Assert.That(totalResults, Is.EqualTo(expectedTotal), $"Total results {totalResults} is incorrect for action [{actionName}]");
			Logger.LogDebug("<<< Exiting GetUsersAsAdminWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod})", actionName, expectedTotal, apiMethod);
			Logger.LogDebug("GetUsersAsAdminWithVariousFiltersShoudlReturnExpectedResult(actionName={ActionName},expectedTotal={ExpectedTotal},apiMethod={ApiMethod}): totalResults → {TotalResults}", actionName, expectedTotal, apiMethod, totalResults);
        }
    }
}
