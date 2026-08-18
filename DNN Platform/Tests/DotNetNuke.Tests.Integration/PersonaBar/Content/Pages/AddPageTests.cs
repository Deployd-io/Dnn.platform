// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.PersonaBar.Content.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using Newtonsoft.Json;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class AddPageTests : IntegrationTestBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string AddBulkPagesApi = "/API/PersonaBar/Pages/SaveBulkPages";
        private const string VerifyBulkPagesApi = "/API/PersonaBar/Pages/PreSaveBulkPagesValidate";

        [Test]
        public void Add_Multi_Pages_For_Existing_Should_Return_Results()
        {
			_logger.LogDebug(">>> Entering Add_Multi_Pages_For_Existing_Should_Return_Results()");
            var rnd = new Random().Next(1000, 10000);
            var addPagesDto = new BulkPage
			_logger.LogDebug("Add_Multi_Pages_For_Existing_Should_Return_Results(): rnd → {Rnd}", rnd);
            {
                BulkPages = "Page_" + rnd,
                ParentId = -1,
                Keywords = string.Empty,
                Tags = string.Empty,
                IncludeInMenu = true,
                StartDate = null,
                EndDate = null,
            };

            Console.WriteLine(@"Add bulk pages request = {0}", JsonConvert.SerializeObject(addPagesDto));
            var connector = WebApiTestHelper.LoginHost();
			_logger.LogDebug("Add_Multi_Pages_For_Existing_Should_Return_Results(): connector → {Connector}", connector);
            var response = connector.PostJson(AddBulkPagesApi, addPagesDto).Content.ReadAsStringAsync().Result;
			_logger.LogDebug("Add_Multi_Pages_For_Existing_Should_Return_Results(): response → {Response}", response);
            var result = JsonConvert.DeserializeObject<BulkPageResponseWrapper>(response);
			_logger.LogDebug("Add_Multi_Pages_For_Existing_Should_Return_Results(): result → {Result}", result);
            Console.WriteLine(@"Add bulk pages ersponse = {0}", response);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Status, Is.EqualTo(0));
                Assert.That(result.Response.Pages.First().ErrorMessage, Is.Null);
            }

            var response2 = connector.PostJson(VerifyBulkPagesApi, addPagesDto).Content.ReadAsStringAsync().Result;
			_logger.LogDebug("Add_Multi_Pages_For_Existing_Should_Return_Results(): response2 → {Response2}", response2);
            var result2 = JsonConvert.DeserializeObject<BulkPageResponseWrapper>(response2);
			_logger.LogDebug("Add_Multi_Pages_For_Existing_Should_Return_Results(): result2 → {Result2}", result2);
            Console.WriteLine(@"Verify bulk pages ersponse = {0}", response2);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(int.Parse(result2.Status.ToString()), Is.EqualTo(0));
                Assert.That(result2.Response.Pages.First().ErrorMessage, Is.Not.Null.And.Not.Empty);
            }
			_logger.LogDebug("<<< Exiting Add_Multi_Pages_For_Existing_Should_Return_Results()");
        }

        [JsonObject]
        public class BulkPage
        {
            public string BulkPages { get; set; }

            public int ParentId { get; set; }

            public string Keywords { get; set; }

            public string Tags { get; set; }

            public bool IncludeInMenu { get; set; }

            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }
        }

        [JsonObject]
        public class BulkPageResponseItem
        {
            public string PageName { get; set; }

            public int Status { get; set; }

            public int TabId { get; set; }

            public string ErrorMessage { get; set; }
        }

        [JsonObject]
        public class BulkPageResponse
        {
            public int OverallStatus { get; set; }

            public IEnumerable<BulkPageResponseItem> Pages { get; set; }
        }

        [JsonObject]
        public class BulkPageResponseWrapper
        {
            public int Status { get; set; }

            public BulkPageResponse Response { get; set; }
        }
    }
}
