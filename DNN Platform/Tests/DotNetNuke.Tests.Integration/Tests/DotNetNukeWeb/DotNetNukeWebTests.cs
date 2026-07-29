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

    [TestFixture]
    public class DotNetNukeWebTests : IntegrationTestBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string GetMonikerQuery = "/API/web/mobilehelper/monikers?moduleList=";
        private const string GetModuleDetailsQuery = "/API/web/mobilehelper/moduledetails?moduleList=";

        private readonly HttpClient _httpClient;

        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

        public DotNetNukeWebTests()
        {
			Logger.LogDebug(">>> Entering DotNetNukeWebTests()");
            var url = ConfigurationManager.AppSettings["siteUrl"];
            var siteUri = new Uri(url);
			Logger.LogDebug("DotNetNukeWebTests(): url → {Url}", url);
			Logger.LogDebug("DotNetNukeWebTests(): siteUri → {SiteUri}", siteUri);
            this._httpClient = new HttpClient { BaseAddress = siteUri, Timeout = this._timeout };
			Logger.LogDebug("<<< Exiting DotNetNukeWebTests()");
			Logger.LogDebug("DotNetNukeWebTests(): _httpClient → {_httpClient}", _httpClient);
			Logger.LogDebug("DotNetNukeWebTests(): this._httpClient → {Arg0}", this._httpClient);
        }

        [OneTimeTearDown]
        public override void TestFixtureTearDown()
        {
			Logger.LogDebug(">>> Entering TestFixtureTearDown()");
            base.TestFixtureTearDown();
            this._httpClient?.Dispose();
			Logger.LogDebug("<<< Exiting TestFixtureTearDown()");
        }

        [Test]
        [TestCase(GetMonikerQuery)]
        [TestCase(GetModuleDetailsQuery)]
        public void CallingHelperForAnonymousUserShouldReturnSuccess(string query)
        {
			Logger.LogDebug(">>> Entering CallingHelperForAnonymousUserShouldReturnSuccess(query={Query})");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var result = this._httpClient.GetAsync(query + HttpUtility.UrlEncode("ViewProfile")).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("CallingHelperForAnonymousUserShouldReturnSuccess(query)={Query}: external service call took {Arg1} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("CallingHelperForAnonymousUserShouldReturnSuccess(query={Query}): result → {Result}", result);
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Logger.LogDebug("<<< Exiting CallingHelperForAnonymousUserShouldReturnSuccess(query={Query})");
			Logger.LogDebug("CallingHelperForAnonymousUserShouldReturnSuccess(query={Query}): content → {Content}", content);
        }
    }
}
