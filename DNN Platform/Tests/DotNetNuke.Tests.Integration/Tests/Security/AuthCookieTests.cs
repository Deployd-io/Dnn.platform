// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Security
{
    using System;
    using System.Net;
    using System.Net.Http;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class AuthCookieTests : IntegrationTestBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string GetPortaslApi = "/API/PersonaBar/SiteSettings/GetPortals";

        [Test]
        public void Using_Logged_Out_Cookie_Should_Be_Unauthorized()
        {
			_logger.LogDebug(">>> Entering Using_Logged_Out_Cookie_Should_Be_Unauthorized()");
            var session = WebApiTestHelper.LoginHost();
            Assert.That(session.IsLoggedIn, Is.True);
			_logger.LogDebug("Using_Logged_Out_Cookie_Should_Be_Unauthorized(): session → {Session}", session);

            // clone the cookies as after closing they will be removed from the container
            var cookies = new CookieContainer();
			_logger.LogDebug("Using_Logged_Out_Cookie_Should_Be_Unauthorized(): cookies → {Cookies}", cookies);
            cookies.Add(session.SessionCookies.GetCookies(session.Domain));

            // make sure the request succeeds when the user is logged in
            // var result1 = session.GetContent(GetPortaslApi, null, false); -- use same method to validate
            var result1 = this.SendDirectGetRequest(session.Domain, GetPortaslApi, session.Timeout, cookies);
			_logger.LogDebug("Using_Logged_Out_Cookie_Should_Be_Unauthorized(): result1 → {Result1}", result1);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result1.IsSuccessStatusCode, Is.True);
                Assert.That(result1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }

            session.Logout();
            Assert.That(session.IsLoggedIn, Is.False);

            // make sure the request fails when using the same cookies before logging out
            var result2 = this.SendDirectGetRequest(session.Domain, GetPortaslApi, session.Timeout, cookies);
			_logger.LogDebug("Using_Logged_Out_Cookie_Should_Be_Unauthorized(): result2 → {Result2}", result2);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result2.IsSuccessStatusCode, Is.False);
                Assert.That(result2.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            }
			_logger.LogDebug("<<< Exiting Using_Logged_Out_Cookie_Should_Be_Unauthorized()");
        }

        private static HttpClient CreateHttpClient(Uri domain, TimeSpan timeout, CookieContainer cookies)
        {
			_logger.LogDebug(">>> Entering CreateHttpClient(domain={Domain},timeout={Timeout},cookies=<redacted>)", domain, timeout);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var clientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                CookieContainer = cookies,
            };

            var client = new HttpClient(clientHandler)
            {
                BaseAddress = domain,
                Timeout = timeout,
            };

			_logger.LogInformation("CreateHttpClient(domain,timeout,cookies)={Domain},{Timeout},{Cookies}: external service call took {Arg3} ms", domain, timeout, cookies, __logStart.ElapsedMilliseconds);
			_logger.LogDebug("<<< Exiting CreateHttpClient(domain={Domain},timeout={Timeout},cookies=<redacted>)", domain, timeout);
            return client;
        }

        private HttpResponseMessage SendDirectGetRequest(Uri domain, string path, TimeSpan timeout, CookieContainer cookies)
        {
			_logger.LogDebug(">>> Entering SendDirectGetRequest(domain={Domain},path={Path},timeout={Timeout},cookies=<redacted>)", domain, path, timeout);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var client = CreateHttpClient(domain, timeout, cookies);
			_logger.LogDebug("SendDirectGetRequest(domain={Domain},path={Path},timeout={Timeout},cookies=<redacted>): client → {Client}", domain, path, timeout, client);
			_logger.LogInformation("SendDirectGetRequest(domain,path,timeout,cookies)={Domain},{Path},{Timeout},{Cookies}: external service call took {Arg4} ms", domain, path, timeout, cookies, __logStart.ElapsedMilliseconds);
			_logger.LogDebug("<<< Exiting SendDirectGetRequest(domain={Domain},path={Path},timeout={Timeout},cookies=<redacted>)", domain, path, timeout);
            return client.GetAsync(path).Result;
        }
    }
}
