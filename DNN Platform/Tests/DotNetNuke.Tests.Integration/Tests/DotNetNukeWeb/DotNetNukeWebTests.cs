// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.DotNetNukeWeb
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Web;

    using DNN.Integration.Test.Framework;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DotNetNukeWebTests : IntegrationTestBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string GetMonikerQuery = "/API/web/mobilehelper/monikers?moduleList=";
        private const string GetModuleDetailsQuery = "/API/web/mobilehelper/moduledetails?moduleList=";

        private readonly HttpClient _httpClient;

        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

        public DotNetNukeWebTests()
        {
			_logger.LogDebug(">>> Entering DotNetNukeWebTests()");
            var url = ConfigurationManager.AppSettings["siteUrl"];
            var siteUri = new Uri(url);
			_logger.LogDebug("DotNetNukeWebTests(): siteUri → {SiteUri}", siteUri);
            this._httpClient = new HttpClient { BaseAddress = siteUri, Timeout = this._timeout };
			_logger.LogDebug("DotNetNukeWebTests(): _httpClient → {_httpClient}", _httpClient);
			_logger.LogDebug("DotNetNukeWebTests(): this._httpClient → {Arg0}", this._httpClient);
			_logger.LogDebug("<<< Exiting DotNetNukeWebTests()");
        }

        [OneTimeTearDown]
        public override void TestFixtureTearDown()
        {
			_logger.LogDebug(">>> Entering TestFixtureTearDown()");
            base.TestFixtureTearDown();
            this._httpClient?.Dispose();
			_logger.LogDebug("<<< Exiting TestFixtureTearDown()");
        }

        [Test]
        [TestCase(GetMonikerQuery)]
        [TestCase(GetModuleDetailsQuery)]
        public void CallingHelperForAnonymousUserShouldReturnSuccess(string query)
        {
			_logger.LogDebug(">>> Entering CallingHelperForAnonymousUserShouldReturnSuccess(query={Query})");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var result = this._httpClient.GetAsync(query + HttpUtility.UrlEncode("ViewProfile")).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			_logger.LogInformation("CallingHelperForAnonymousUserShouldReturnSuccess(query)={Query}: external service call took {Arg1} ms", __logStart.ElapsedMilliseconds);
			_logger.LogDebug("CallingHelperForAnonymousUserShouldReturnSuccess(query={Query}): result → {Result}", result);
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			_logger.LogDebug("CallingHelperForAnonymousUserShouldReturnSuccess(query={Query}): content → {Content}", content);
			_logger.LogDebug("<<< Exiting CallingHelperForAnonymousUserShouldReturnSuccess(query={Query})");
        }
    }
}
