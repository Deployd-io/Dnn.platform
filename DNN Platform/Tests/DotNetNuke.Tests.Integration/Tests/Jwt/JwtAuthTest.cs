// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Jwt
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using Newtonsoft.Json;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class JwtAuthTest : IntegrationTestBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public const string ExtendTokenQuery = "/API/JwtAuth/mobile/extendtoken";

        private const string LoginQuery = "/API/JwtAuth/mobile/login";
        private const string LogoutQuery = "/API/JwtAuth/mobile/logout";
        private const string TestGetQuery = "/API/JwtAuth/mobile/testget";
        private const string TestPostQuery = "/API/JwtAuth/mobile/testpost";
        private const string GetMonikerQuery = "/API/web/mobilehelper/monikers?moduleList=";
        private const string GetModuleDetailsQuery = "/API/web/mobilehelper/moduledetails?moduleList=";
        private static readonly Encoding TextEncoder = Encoding.UTF8;
        private readonly string _hostName;
        private readonly string _hostPass;
        private readonly HttpClient _httpClient;
#if DEBUG
        // for degugging and setting breakpoints
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(300);
#else
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
#endif

        public JwtAuthTest()
        {
			_logger.LogDebug(">>> Entering JwtAuthTest()");
            var url = ConfigurationManager.AppSettings["siteUrl"];
            var siteUri = new Uri(url);
			_logger.LogDebug("JwtAuthTest(): siteUri → {SiteUri}", siteUri);
            this._httpClient = new HttpClient { BaseAddress = siteUri, Timeout = this._timeout };
			_logger.LogDebug("JwtAuthTest(): _httpClient → {_httpClient}", _httpClient);
			_logger.LogDebug("JwtAuthTest(): this._httpClient → {Arg0}", this._httpClient);
            this._hostName = ConfigurationManager.AppSettings["hostUsername"];
            this._hostPass = ConfigurationManager.AppSettings["hostPassword"];
			_logger.LogDebug("JwtAuthTest(): this._hostName → {Arg0}", this._hostName);
			_logger.LogDebug("<<< Exiting JwtAuthTest()");
        }

        [OneTimeSetUp]
        public override async Task TestFixtureSetUp()
        {
			_logger.LogDebug(">>> Entering TestFixtureSetUp()");
            await base.TestFixtureSetUp();
            try
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
                {
					var __logStart = System.Diagnostics.Stopwatch.StartNew();
					_logger.LogInformation("TestFixtureSetUp(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
                    DatabaseHelper.ExecuteNonQuery("TRUNCATE TABLE {objectQualifier}JsonWebTokens");
                }
            }
            catch (Exception)
            {
				_logger.LogError("Exception in TestFixtureSetUp(): {Arg0}", Exception.Message);
                // ignored
            }
			_logger.LogDebug("<<< Exiting TestFixtureSetUp()");
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
        public void InvalidUserLoginShouldFail()
        {
			_logger.LogDebug(">>> Entering InvalidUserLoginShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var credentials = new { u = this._hostName, p = this._hostPass + "." };
            var result = this._httpClient.PostAsJsonAsync(LoginQuery, credentials).Result;
			_logger.LogDebug("InvalidUserLoginShouldFail(): credentials → {Credentials}", credentials);
			_logger.LogInformation("InvalidUserLoginShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			_logger.LogDebug("InvalidUserLoginShouldFail(): result → {Result}", result);
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			_logger.LogDebug("InvalidUserLoginShouldFail(): content → {Content}", content);
			_logger.LogDebug("<<< Exiting InvalidUserLoginShouldFail()");
        }

        [Test]
        public void ValidUserLoginShouldPass()
        {
			_logger.LogDebug(">>> Entering ValidUserLoginShouldPass()");
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            using (Assert.EnterMultipleScope())
            {
			_logger.LogWarning("ValidUserLoginShouldPass(): token → {Token}", token);
                Assert.That(token.AccessToken, Is.Not.Null);
                Assert.That(token.DisplayName, Is.Not.Null);
                Assert.That(token.RenewalToken, Is.Not.Null);
            }

            var parts = token.AccessToken.Split('.');
			_logger.LogDebug("ValidUserLoginShouldPass(): parts → {Parts}", parts);
            var decoded = DecodeBase64(parts[1]);
			_logger.LogDebug("ValidUserLoginShouldPass(): decoded → {Decoded}", decoded);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
			_logger.LogDebug("ValidUserLoginShouldPass(): claims → {Claims}", claims);
            long claimExpiry = claims.exp;
            var expiryInToken = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(claimExpiry);
			_logger.LogDebug("ValidUserLoginShouldPass(): expiryInToken → {ExpiryInToken}", expiryInToken);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(DateTime.UtcNow, Is.LessThan(expiryInToken));
                Assert.That(expiryInToken, Is.LessThanOrEqualTo(DateTime.UtcNow.AddHours(1)));
            }
			_logger.LogDebug("<<< Exiting ValidUserLoginShouldPass()");
        }

        [Test]
        public void RequestUsingInvaldatedTokenAfterLogoutShouldFail()
        {
			_logger.LogDebug(">>> Entering RequestUsingInvaldatedTokenAfterLogoutShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);

			_logger.LogWarning("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): token → {Token}", token);
            this.SetAuthHeaderToken(token.AccessToken);
            var result1 = this._httpClient.GetAsync(TestGetQuery).Result;
			_logger.LogDebug("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): result1 → {Result1}", result1);
			_logger.LogInformation("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            var content1 = result1.Content.ReadAsStringAsync().Result;
            LogText(@"content1 => " + content1);
			_logger.LogDebug("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): content1 → {Content1}", content1);
            Assert.That(result1.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            this.LogoutUser(token.AccessToken);

            this.SetAuthHeaderToken(token.AccessToken);
            var result2 = this._httpClient.GetAsync(TestGetQuery).Result;
			_logger.LogDebug("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): result2 → {Result2}", result2);
            var content2 = result2.Content.ReadAsStringAsync().Result;
			_logger.LogDebug("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): content2 → {Content2}", content2);
            LogText(@"content2 => " + content2);
            Assert.That(result2.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			_logger.LogDebug("<<< Exiting RequestUsingInvaldatedTokenAfterLogoutShouldFail()");
        }

        [Test]
        public void RequestWithoutTokenShouldFail()
        {
			_logger.LogDebug(">>> Entering RequestWithoutTokenShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			_logger.LogInformation("RequestWithoutTokenShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			_logger.LogDebug("RequestWithoutTokenShouldFail(): result → {Result}", result);
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			_logger.LogDebug("RequestWithoutTokenShouldFail(): content → {Content}", content);
			_logger.LogDebug("<<< Exiting RequestWithoutTokenShouldFail()");
        }

        [Test]
        public void ValidatedUserGetRequestShouldPass()
        {
			_logger.LogDebug(">>> Entering ValidatedUserGetRequestShouldPass()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
			_logger.LogWarning("ValidatedUserGetRequestShouldPass(): token → {Token}", token);
			_logger.LogInformation("ValidatedUserGetRequestShouldPass(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
			_logger.LogDebug("ValidatedUserGetRequestShouldPass(): result → {Result}", result);
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
			_logger.LogDebug("ValidatedUserGetRequestShouldPass(): content → {Content}", content);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(content.IndexOf("You are authenticated through JWT", StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
            }
			_logger.LogDebug("<<< Exiting ValidatedUserGetRequestShouldPass()");
        }

        [Test]
        public void ValidatedUserPostRequestShouldPass()
        {
			_logger.LogDebug(">>> Entering ValidatedUserPostRequestShouldPass()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
			_logger.LogWarning("ValidatedUserPostRequestShouldPass(): token → {Token}", token);
			_logger.LogInformation("ValidatedUserPostRequestShouldPass(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            var result = this._httpClient.PostAsJsonAsync(TestPostQuery, new { text = "Integraton Testing Rocks!" }).Result;
			_logger.LogDebug("ValidatedUserPostRequestShouldPass(): result → {Result}", result);
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
			_logger.LogDebug("ValidatedUserPostRequestShouldPass(): content → {Content}", content);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(content.IndexOf("You are authenticated through JWT", StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
                Assert.That(content.IndexOf("You said: (Integraton Testing Rocks!)", StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
            }
			_logger.LogDebug("<<< Exiting ValidatedUserPostRequestShouldPass()");
        }

        [Test]
        public void RenewValidTokenShouldPass()
        {
			_logger.LogDebug(">>> Entering RenewValidTokenShouldPass()");
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var token2 = this.RenewAuthorizationToken(token1);
			_logger.LogDebug("RenewValidTokenShouldPass(): token2 → {Token2}", token2);
			_logger.LogDebug("RenewValidTokenShouldPass(): token1 → {Token1}", token1);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(token2.AccessToken, Is.Not.EqualTo(token1.AccessToken));
                Assert.That(token2.RenewalToken, Is.EqualTo(token1.RenewalToken));
                Assert.That(token2.DisplayName, Is.EqualTo(token1.DisplayName));
            }
			_logger.LogDebug("<<< Exiting RenewValidTokenShouldPass()");
        }

        [Test]
        public void UsingOriginalAccessTokenAfterRenewalShouldFail()
        {
			_logger.LogDebug(">>> Entering UsingOriginalAccessTokenAfterRenewalShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var token2 = this.RenewAuthorizationToken(token1);
			_logger.LogDebug("UsingOriginalAccessTokenAfterRenewalShouldFail(): token1 → {Token1}", token1);
			_logger.LogDebug("UsingOriginalAccessTokenAfterRenewalShouldFail(): token2 → {Token2}", token2);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(token2.AccessToken, Is.Not.EqualTo(token1.AccessToken));
                Assert.That(token2.RenewalToken, Is.EqualTo(token1.RenewalToken));
                Assert.That(token2.DisplayName, Is.EqualTo(token1.DisplayName));
            }

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
			_logger.LogDebug("UsingOriginalAccessTokenAfterRenewalShouldFail(): result → {Result}", result);
			_logger.LogInformation("UsingOriginalAccessTokenAfterRenewalShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
			_logger.LogDebug("UsingOriginalAccessTokenAfterRenewalShouldFail(): content → {Content}", content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			_logger.LogDebug("<<< Exiting UsingOriginalAccessTokenAfterRenewalShouldFail()");
        }

        [Test]
        public void TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail()
        {
			_logger.LogDebug(">>> Entering TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.RenewAuthorizationToken(token1);
			_logger.LogDebug("TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail(): token1 → {Token1}", token1);

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = token1.RenewalToken }).Result;
			_logger.LogDebug("TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail(): result → {Result}", result);
			_logger.LogInformation("TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			_logger.LogDebug("<<< Exiting TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail()");
        }

        [Test]
        public void RenewMultipleTimesShouldPass()
        {
			_logger.LogDebug(">>> Entering RenewMultipleTimesShouldPass()");
            this.RenewAuthorizationToken(
                this.RenewAuthorizationToken(
                    this.RenewAuthorizationToken(
                        this.GetAuthorizationTokenFor(this._hostName, this._hostPass))));
						_logger.LogDebug("<<< Exiting RenewMultipleTimesShouldPass()");
        }

        [Test]
        public void UsingTheNewAccessTokenAfterRenewalShouldPass()
        {
			_logger.LogDebug(">>> Entering UsingTheNewAccessTokenAfterRenewalShouldPass()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var token2 = this.RenewAuthorizationToken(token1);
			_logger.LogDebug("UsingTheNewAccessTokenAfterRenewalShouldPass(): token1 → {Token1}", token1);
			_logger.LogDebug("UsingTheNewAccessTokenAfterRenewalShouldPass(): token2 → {Token2}", token2);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(token2.AccessToken, Is.Not.EqualTo(token1.AccessToken));
                Assert.That(token2.RenewalToken, Is.EqualTo(token1.RenewalToken));
                Assert.That(token2.DisplayName, Is.EqualTo(token1.DisplayName));
            }

            this.SetAuthHeaderToken(token2.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
			_logger.LogDebug("UsingTheNewAccessTokenAfterRenewalShouldPass(): result → {Result}", result);
			_logger.LogInformation("UsingTheNewAccessTokenAfterRenewalShouldPass(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
			_logger.LogDebug("UsingTheNewAccessTokenAfterRenewalShouldPass(): content → {Content}", content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			_logger.LogDebug("<<< Exiting UsingTheNewAccessTokenAfterRenewalShouldPass()");
        }

        [Test]
        public void TamperedTokenShouldFail()
        {
			_logger.LogDebug(">>> Entering TamperedTokenShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);

			_logger.LogDebug("TamperedTokenShouldFail(): token1 → {Token1}", token1);
            // tampered header
            this.SetAuthHeaderToken("x" + token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
			_logger.LogDebug("TamperedTokenShouldFail(): result → {Result}", result);
			_logger.LogInformation("TamperedTokenShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
			_logger.LogDebug("TamperedTokenShouldFail(): content → {Content}", content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

            // tampered signature
            this.SetAuthHeaderToken(token1.AccessToken + "y");
            result = this._httpClient.GetAsync(TestGetQuery).Result;
            content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

            // tampered claims
            var idx = token1.AccessToken.IndexOf('.');
			_logger.LogDebug("TamperedTokenShouldFail(): idx → {Idx}", idx);
            var tampered = token1.AccessToken.Substring(0, idx + 10) + "z" + token1.AccessToken.Substring(idx + 10);
			_logger.LogDebug("TamperedTokenShouldFail(): tampered → {Tampered}", tampered);
            this.SetAuthHeaderToken(tampered);
            result = this._httpClient.GetAsync(TestGetQuery).Result;
            content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			_logger.LogDebug("<<< Exiting TamperedTokenShouldFail()");
        }

        [Test]
        public void ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry()
        {
			_logger.LogDebug(">>> Entering ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry()");
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
			_logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): parts → {Parts}", parts);
            var decoded = DecodeBase64(parts[1]);
			_logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): decoded → {Decoded}", decoded);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
			_logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): claims → {Claims}", claims);
			_logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): token1 → {Token1}", token1);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();
			_logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): TokenId → {TokenId}", TokenId);
			_logger.LogInformation("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            var token2 = this.RenewAuthorizationToken(token1);
			_logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): token2 → {Token2}", token2);
            parts = token2.AccessToken.Split('.');
            decoded = DecodeBase64(parts[1]);
            claims = JsonConvert.DeserializeObject(decoded);
            long claimExpiry = claims.exp;
            var expiryInToken = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(claimExpiry);
			_logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): expiryInToken → {ExpiryInToken}", expiryInToken);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(DateTime.UtcNow, Is.LessThan(expiryInToken));
                Assert.That(expiryInToken, Is.LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(31))); // appears the library rounds the time
            }

            var record = DatabaseHelper.GetRecordById("JsonWebTokens", "TokenId", sessionId);
			_logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): record → {Record}", record);
            var accessExpiry = (DateTime)record["TokenExpiry"];
			_logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): accessExpiry → {AccessExpiry}", accessExpiry);
            var renewalExpiry = (DateTime)record["RenewalExpiry"];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(renewalExpiry, Is.EqualTo(accessExpiry));
                Assert.That(DateTime.UtcNow, Is.LessThan(renewalExpiry));
            }
            Assert.That(renewalExpiry, Is.LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(31)));
            Assert.That(expiryInToken, Is.EqualTo(accessExpiry));
			_logger.LogDebug("<<< Exiting ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry()");
        }

        [Test]
        public void UsingExpiredAccessTokenShouldFail()
        {
			_logger.LogDebug(">>> Entering UsingExpiredAccessTokenShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
			_logger.LogDebug("UsingExpiredAccessTokenShouldFail(): token1 → {Token1}", token1);
			_logger.LogDebug("UsingExpiredAccessTokenShouldFail(): parts → {Parts}", parts);
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
			_logger.LogDebug("UsingExpiredAccessTokenShouldFail(): decoded → {Decoded}", decoded);
			_logger.LogDebug("UsingExpiredAccessTokenShouldFail(): claims → {Claims}", claims);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET TokenExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();
			_logger.LogDebug("UsingExpiredAccessTokenShouldFail(): TokenId → {TokenId}", TokenId);
			_logger.LogInformation("UsingExpiredAccessTokenShouldFail(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
			_logger.LogDebug("UsingExpiredAccessTokenShouldFail(): result → {Result}", result);
			_logger.LogInformation("UsingExpiredAccessTokenShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
			_logger.LogDebug("UsingExpiredAccessTokenShouldFail(): content → {Content}", content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			_logger.LogDebug("<<< Exiting UsingExpiredAccessTokenShouldFail()");
        }

        [Test]
        public void UsingExpiredRenewalTokenShouldFail()
        {
			_logger.LogDebug(">>> Entering UsingExpiredRenewalTokenShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
			_logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): token1 → {Token1}", token1);
			_logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): parts → {Parts}", parts);
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
			_logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): decoded → {Decoded}", decoded);
			_logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): claims → {Claims}", claims);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();
			_logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): TokenId → {TokenId}", TokenId);
			_logger.LogInformation("UsingExpiredRenewalTokenShouldFail(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
			_logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): result → {Result}", result);
			_logger.LogInformation("UsingExpiredRenewalTokenShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
			_logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): content → {Content}", content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			_logger.LogDebug("<<< Exiting UsingExpiredRenewalTokenShouldFail()");
        }

        [Test]
        public void TryingToRenewUsingAnExpiredRenewalTokenShouldFail()
        {
			_logger.LogDebug(">>> Entering TryingToRenewUsingAnExpiredRenewalTokenShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
			_logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): token1 → {Token1}", token1);
			_logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): parts → {Parts}", parts);
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
			_logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): decoded → {Decoded}", decoded);
			_logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): claims → {Claims}", claims);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();
			_logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): TokenId → {TokenId}", TokenId);
			_logger.LogInformation("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = token1.RenewalToken }).Result;
			_logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): result → {Result}", result);
			_logger.LogInformation("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			_logger.LogDebug("<<< Exiting TryingToRenewUsingAnExpiredRenewalTokenShouldFail()");
        }

        [Test]
        [TestCase(GetMonikerQuery)]
        [TestCase(GetModuleDetailsQuery)]
        public void CallingHelperForLoggedinUserShouldReturnSuccess(string query)
        {
			_logger.LogDebug(">>> Entering CallingHelperForLoggedinUserShouldReturnSuccess(query={Query})", query);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
			_logger.LogWarning("CallingHelperForLoggedinUserShouldReturnSuccess(query={Query}): token → {Token}", query, token);
			_logger.LogInformation("CallingHelperForLoggedinUserShouldReturnSuccess(query)={Query}: external service call took {Arg1} ms", query, __logStart.ElapsedMilliseconds);
            var result = this._httpClient.GetAsync(query + HttpUtility.UrlEncode("ViewProfile")).Result;
			_logger.LogDebug("CallingHelperForLoggedinUserShouldReturnSuccess(query={Query}): result → {Result}", query, result);
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
			_logger.LogDebug("CallingHelperForLoggedinUserShouldReturnSuccess(query={Query}): content → {Content}", query, content);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(content, Is.Not.Null);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
			_logger.LogDebug("<<< Exiting CallingHelperForLoggedinUserShouldReturnSuccess(query={Query})", query);
        }

        [Test]
        public void ValidatingSuccessWhenUsingExistingMoniker()
        {
			_logger.LogDebug(">>> Entering ValidatingSuccessWhenUsingExistingMoniker()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            const string query1 =
                 @"SELECT TOP(1) TabModuleId FROM {objectQualifier}TabModules
	                WHERE TabId IN (SELECT TabId FROM {objectQualifier}Tabs WHERE TabName='Activity Feed')
	                  AND ModuleTitle='Journal';";
					_logger.LogDebug("ValidatingSuccessWhenUsingExistingMoniker(): ModuleTitle → {ModuleTitle}", ModuleTitle);
            var tabModuleId = DatabaseHelper.ExecuteScalar<int>(query1);
			_logger.LogDebug("ValidatingSuccessWhenUsingExistingMoniker(): tabModuleId → {TabModuleId}", tabModuleId);
            Assert.That(tabModuleId, Is.GreaterThan(0));

            // These will set a moniker for the Activity Feed module of the user profile
			_logger.LogInformation("ValidatingSuccessWhenUsingExistingMoniker(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            DatabaseHelper.ExecuteNonQuery(@"EXEC {objectQualifier}DeleteTabModuleSetting " + tabModuleId + @", 'Moniker'");
            DatabaseHelper.ExecuteNonQuery(@"EXEC {objectQualifier}UpdateTabModuleSetting " + tabModuleId + @", 'Moniker', 'myjournal', 1");
            WebApiTestHelper.ClearHostCache();

            // Act
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
			_logger.LogWarning("ValidatingSuccessWhenUsingExistingMoniker(): token → {Token}", token);
            this.SetAuthHeaderToken(token.AccessToken);
            this.SetMonikerHeader("myjournal");
            var postItem = new { ProfileId = 1, GroupId = -1, RowIndex = 0, MaxRows = 1 };
            var result = this._httpClient.PostAsJsonAsync(
                "/API/Journal/Services/GetListForProfile", postItem).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			_logger.LogInformation("ValidatingSuccessWhenUsingExistingMoniker(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            LogText(@"content => " + content);
			_logger.LogDebug("ValidatingSuccessWhenUsingExistingMoniker(): content → {Content}", content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			_logger.LogDebug("<<< Exiting ValidatingSuccessWhenUsingExistingMoniker()");
        }

        [Test]
        public void ValidatingFailureWhenUsingNonExistingMoniker()
        {
			_logger.LogDebug(">>> Entering ValidatingFailureWhenUsingNonExistingMoniker()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            const string query1 =
                 @"SELECT TOP(1) TabModuleId FROM {objectQualifier}TabModules
	                WHERE TabId IN (SELECT TabId FROM {objectQualifier}Tabs WHERE TabName='Activity Feed')
	                  AND ModuleTitle='Journal';";
					_logger.LogDebug("ValidatingFailureWhenUsingNonExistingMoniker(): ModuleTitle → {ModuleTitle}", ModuleTitle);
            var tabModuleId = DatabaseHelper.ExecuteScalar<int>(query1);
			_logger.LogDebug("ValidatingFailureWhenUsingNonExistingMoniker(): tabModuleId → {TabModuleId}", tabModuleId);
            Assert.That(tabModuleId, Is.GreaterThan(0));

            // These will set a moniker for the Activity Feed module of the user profile
			_logger.LogInformation("ValidatingFailureWhenUsingNonExistingMoniker(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            DatabaseHelper.ExecuteNonQuery(@"EXEC {objectQualifier}DeleteTabModuleSetting " + tabModuleId + @", 'Moniker'");
            WebApiTestHelper.ClearHostCache();

            // Act
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
			_logger.LogWarning("ValidatingFailureWhenUsingNonExistingMoniker(): token → {Token}", token);
            this.SetAuthHeaderToken(token.AccessToken);
            this.SetMonikerHeader("myjournal");
            var postItem = new { ProfileId = 1, GroupId = -1, RowIndex = 0, MaxRows = 1 };
            var result = this._httpClient.PostAsJsonAsync(
                "/API/Journal/Services/GetListForProfile", postItem).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			_logger.LogInformation("ValidatingFailureWhenUsingNonExistingMoniker(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            LogText(@"content => " + content);
			_logger.LogDebug("ValidatingFailureWhenUsingNonExistingMoniker(): content → {Content}", content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			_logger.LogDebug("<<< Exiting ValidatingFailureWhenUsingNonExistingMoniker()");
        }

        private static string DecodeBase64(string b64Str)
        {
			_logger.LogDebug(">>> Entering DecodeBase64(b64Str={B64Str})", b64Str);
            // fix Base64 string padding
            var mod = b64Str.Length % 4;
            if (mod != 0)
            {
                b64Str += new string('=', 4 - mod);
            }

			_logger.LogDebug("<<< Exiting DecodeBase64(b64Str={B64Str})", b64Str);
            return TextEncoder.GetString(Convert.FromBase64String(b64Str));
        }

        // template to help in copy/paste of new test methods
        /*
        [Test]
        public void TemplateMethod()
        {
			_logger.LogDebug(">>> Entering TemplateMethod()");
            Assert.Fail();
			_logger.LogDebug("<<< Exiting TemplateMethod()");
        }
         */

        private LoginResultData GetAuthorizationTokenFor(string uname, string upass)
        {
			_logger.LogDebug(">>> Entering GetAuthorizationTokenFor(uname={Uname},upass={Upass})", uname, upass);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var credentials = new { u = uname, p = upass };
            var result = this._httpClient.PostAsJsonAsync(LoginQuery, credentials).Result;
			_logger.LogDebug("GetAuthorizationTokenFor(uname={Uname},upass={Upass}): result → {Result}", uname, upass, result);
			_logger.LogInformation("GetAuthorizationTokenFor(uname,upass)={Uname},{Upass}: external service call took {Arg2} ms", uname, upass, __logStart.ElapsedMilliseconds);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var token = result.Content.ReadAsAsync<LoginResultData>().Result;
			_logger.LogWarning("GetAuthorizationTokenFor(uname={Uname},upass={Upass}): token → {Token}", uname, upass, token);
            Assert.That(token, Is.Not.Null);
            LogText(@"AuthToken => " + JsonConvert.SerializeObject(token));
            this._httpClient.DefaultRequestHeaders.Clear();
			_logger.LogDebug("<<< Exiting GetAuthorizationTokenFor(uname={Uname},upass={Upass})", uname, upass);
            return token;
        }

        private LoginResultData RenewAuthorizationToken(LoginResultData currentToken)
        {
			_logger.LogDebug(">>> Entering RenewAuthorizationToken(currentToken=<redacted>)");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            Thread.Sleep(1000); // must delay at least 1 second so the expiry time is different
            this.SetAuthHeaderToken(currentToken.AccessToken);
            var result = this._httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = currentToken.RenewalToken }).Result;
			_logger.LogDebug("RenewAuthorizationToken(currentToken=<redacted>): result → {Result}", result);
			_logger.LogInformation("RenewAuthorizationToken(currentToken)={CurrentToken}: external service call took {Arg1} ms", currentToken, __logStart.ElapsedMilliseconds);
			_logger.LogWarning("RenewAuthorizationToken(currentToken=<redacted>): token → {Token}", token);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var token = result.Content.ReadAsAsync<LoginResultData>().Result;
            Assert.That(token, Is.Not.Null);
            LogText(@"RenewedToken => " + JsonConvert.SerializeObject(token));
			_logger.LogDebug("<<< Exiting RenewAuthorizationToken(currentToken=<redacted>)");
            return token;
        }

        private void LogoutUser(string accessToken)
        {
			_logger.LogDebug(">>> Entering LogoutUser(accessToken=<redacted>)");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.SetAuthHeaderToken(accessToken);
            var result = this._httpClient.GetAsync(LogoutQuery).Result;
			_logger.LogDebug("LogoutUser(accessToken=<redacted>): result → {Result}", result);
			_logger.LogInformation("LogoutUser(accessToken)={AccessToken}: external service call took {Arg1} ms", accessToken, __logStart.ElapsedMilliseconds);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = result.Content.ReadAsStringAsync().Result;
			_logger.LogDebug("LogoutUser(accessToken=<redacted>): content → {Content}", content);
            dynamic response = !string.IsNullOrEmpty(content) ? JsonConvert.DeserializeObject(content) : null;
            Assert.That(Convert.ToBoolean(response?.success), Is.True);
			_logger.LogDebug("<<< Exiting LogoutUser(accessToken=<redacted>)");
        }

        private void SetAuthHeaderToken(string token, string scheme = "Bearer")
        {
			_logger.LogDebug(">>> Entering SetAuthHeaderToken(token=<redacted>,scheme={Scheme})", scheme);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this._httpClient.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse(scheme.Trim() + " " + token.Trim());
				_logger.LogInformation("SetAuthHeaderToken(token,scheme)={Token},{Scheme}: external service call took {Arg2} ms", token, scheme, __logStart.ElapsedMilliseconds);
				_logger.LogDebug("<<< Exiting SetAuthHeaderToken(token=<redacted>,scheme={Scheme})", scheme);
        }

        private void SetMonikerHeader(string monikerValue)
        {
			_logger.LogDebug(">>> Entering SetMonikerHeader(monikerValue={MonikerValue})", monikerValue);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this._httpClient.DefaultRequestHeaders.Add("X-DNN-MONIKER", monikerValue);
			_logger.LogInformation("SetMonikerHeader(monikerValue)={MonikerValue}: external service call took {Arg1} ms", monikerValue, __logStart.ElapsedMilliseconds);
			_logger.LogDebug("<<< Exiting SetMonikerHeader(monikerValue={MonikerValue})", monikerValue);
        }

        [JsonObject]
        public class LoginResultData
        {
            [JsonProperty("userId")]
            public int UserId { get; set; }

            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            [JsonProperty("accessToken")]
            public string AccessToken { get; set; }

            [JsonProperty("renewalToken")]
            public string RenewalToken { get; set; }
        }
    }
}
