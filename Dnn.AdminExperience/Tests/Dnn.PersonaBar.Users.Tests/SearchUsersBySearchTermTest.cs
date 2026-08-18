// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Tests
{
    using System.Data;

    using Dnn.PersonaBar.Users.Components;
    using Dnn.PersonaBar.Users.Components.Contracts;
    using Dnn.PersonaBar.Users.Components.Dto;
    using Dnn.PersonaBar.Users.Components.Helpers;
    using Moq;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class SearchUsersBySearchTermTest
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private GetUsersContract usersContract;
        private UsersControllerTestable usersCtrl;

        [SetUp]
        public void Init()
        {
			_logger.LogDebug(">>> Entering Init()");
            this.usersContract = new GetUsersContract
            {
				_logger.LogDebug("Init(): this.usersContract → {Arg0}", this.usersContract);
                SearchText = null,
                PageIndex = 0,
                PageSize = 10,
                SortColumn = "displayname",
                SortAscending = true,
                PortalId = 0,
                Filter = UserFilters.All,
            };
			_logger.LogDebug("Init(): usersContract → {UsersContract}", usersContract);

            this.usersCtrl = new UsersControllerTestable();
			_logger.LogDebug("Init(): usersCtrl → {UsersCtrl}", usersCtrl);
			_logger.LogDebug("<<< Exiting Init()");
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("search_text", "search_text%")]
        [TestCase("*search_text", "%search_text%")]
        [TestCase("%search_text", "%search_text%")]
        [TestCase("search_text%", "search_text%")]
        [TestCase("search_text*", "search_text%")]
        [TestCase("*search_text*", "%search_text%")]
        [TestCase("%search_text%", "%search_text%")]
        [TestCase("*search_text%", "%search_text%")]
        [TestCase("%search_text*", "%search_text%")]
        [TestCase("*search*_text*", "%search_text%")]
        [TestCase("*search**_text*", "%search_text%")]
        [TestCase("*search*%_text*", "%search_text%")]
        [TestCase("*search%_text*", "%search_text%")]
        [TestCase("search text", "search text%")]
        public void FilteredSearchTest(string searchText, string expectedFilteredText)
        {
			_logger.LogDebug(">>> Entering FilteredSearchTest(searchText={SearchText},expectedFilteredText={ExpectedFilteredText})", searchText, expectedFilteredText);
            int totalRecords;
            this.usersContract.SearchText = searchText;
            this.usersCtrl.GetUsers(this.usersContract, true, out totalRecords);

            Assert.That(this.usersCtrl.LastSearch, Is.EqualTo(expectedFilteredText));
			_logger.LogDebug("<<< Exiting FilteredSearchTest(searchText={SearchText},expectedFilteredText={ExpectedFilteredText})", searchText, expectedFilteredText);
        }

        private class UsersControllerTestable : UsersController
        {
            private Mock<IDataReader> dataReader;

            public string LastSearch { get; set; }

            protected override IDataReader CallGetUsersBySearchTerm(
                GetUsersContract usersContract,
                bool? includeAuthorized,
                bool? includeDeleted,
                bool? includeSuperUsers,
                bool? hasAgreedToTerms,
                bool? requestsRemoval)
            {
				_logger.LogDebug(">>> Entering CallGetUsersBySearchTerm(usersContract={UsersContract},includeAuthorized={IncludeAuthorized},includeDeleted={IncludeDeleted},includeSuperUsers={IncludeSuperUsers},hasAgreedToTerms={HasAgreedToTerms},requestsRemoval={RequestsRemoval})", usersContract, includeAuthorized, includeDeleted, includeSuperUsers, hasAgreedToTerms, requestsRemoval);
                this.LastSearch = SearchTextFilter.CleanWildcards(usersContract.SearchText);
                this.dataReader = new Mock<IDataReader>();
				_logger.LogDebug("CallGetUsersBySearchTerm(usersContract={UsersContract},includeAuthorized={IncludeAuthorized},includeDeleted={IncludeDeleted},includeSuperUsers={IncludeSuperUsers},hasAgreedToTerms={HasAgreedToTerms},requestsRemoval={RequestsRemoval}): dataReader → {DataReader}", usersContract, includeAuthorized, includeDeleted, includeSuperUsers, hasAgreedToTerms, requestsRemoval, dataReader);
				_logger.LogDebug("<<< Exiting CallGetUsersBySearchTerm(usersContract={UsersContract},includeAuthorized={IncludeAuthorized},includeDeleted={IncludeDeleted},includeSuperUsers={IncludeSuperUsers},hasAgreedToTerms={HasAgreedToTerms},requestsRemoval={RequestsRemoval})", usersContract, includeAuthorized, includeDeleted, includeSuperUsers, hasAgreedToTerms, requestsRemoval);
                return this.dataReader.Object;
            }
        }
    }
}
