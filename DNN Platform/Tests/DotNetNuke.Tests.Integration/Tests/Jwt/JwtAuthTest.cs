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

    [TestFixture]
    public class JwtAuthTest : IntegrationTestBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
			Logger.LogDebug(">>> Entering JwtAuthTest()");
            var url = ConfigurationManager.AppSettings["siteUrl"];
            var siteUri = new Uri(url);
			Logger.LogDebug("JwtAuthTest(): url → {Url}", url);
			Logger.LogDebug("JwtAuthTest(): siteUri → {SiteUri}", siteUri);
            this._httpClient = new HttpClient { BaseAddress = siteUri, Timeout = this._timeout };
            this._hostName = ConfigurationManager.AppSettings["hostUsername"];
			Logger.LogDebug("JwtAuthTest(): _httpClient → {_httpClient}", _httpClient);
			Logger.LogDebug("JwtAuthTest(): this._httpClient → {Arg0}", this._httpClient);
			Logger.LogDebug("JwtAuthTest(): this._hostName → {Arg0}", this._hostName);
            this._hostPass = ConfigurationManager.AppSettings["hostPassword"];
			Logger.LogDebug("<<< Exiting JwtAuthTest()");
			Logger.LogDebug("JwtAuthTest(): this._hostPass → {Arg0}", this._hostPass);
        }

        [OneTimeSetUp]
        public override async Task TestFixtureSetUp()
        {
            await base.TestFixtureSetUp();
            try
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
                {
                    DatabaseHelper.ExecuteNonQuery("TRUNCATE TABLE {objectQualifier}JsonWebTokens");
					Logger.LogInformation("TestFixtureSetUp(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in TestFixtureSetUp(): {Arg0}", ex.Message);
                // ignored
            }
			Logger.LogDebug("<<< Exiting TestFixtureSetUp()");
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
        public void InvalidUserLoginShouldFail()
        {
			Logger.LogDebug(">>> Entering InvalidUserLoginShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var credentials = new { u = this._hostName, p = this._hostPass + "." };
            var result = this._httpClient.PostAsJsonAsync(LoginQuery, credentials).Result;
			Logger.LogDebug("InvalidUserLoginShouldFail(): credentials → {Credentials}", credentials);
			Logger.LogInformation("InvalidUserLoginShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("InvalidUserLoginShouldFail(): result → {Result}", result);
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("<<< Exiting InvalidUserLoginShouldFail()");
			Logger.LogDebug("InvalidUserLoginShouldFail(): content → {Content}", content);
        }

        [Test]
        public void ValidUserLoginShouldPass()
        {
			Logger.LogDebug(">>> Entering ValidUserLoginShouldPass()");
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            using (Assert.EnterMultipleScope())
            {
			Logger.LogWarning("ValidUserLoginShouldPass(): token → {Token}", token);
                Assert.That(token.AccessToken, Is.Not.Null);
                Assert.That(token.DisplayName, Is.Not.Null);
                Assert.That(token.RenewalToken, Is.Not.Null);
            }

            var parts = token.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
			Logger.LogDebug("ValidUserLoginShouldPass(): parts → {Parts}", parts);
			Logger.LogDebug("ValidUserLoginShouldPass(): decoded → {Decoded}", decoded);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            long claimExpiry = claims.exp;
			Logger.LogDebug("ValidUserLoginShouldPass(): claims → {Claims}", claims);
            var expiryInToken = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(claimExpiry);
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("ValidUserLoginShouldPass(): expiryInToken → {ExpiryInToken}", expiryInToken);
                Assert.That(DateTime.UtcNow, Is.LessThan(expiryInToken));
                Assert.That(expiryInToken, Is.LessThanOrEqualTo(DateTime.UtcNow.AddHours(1)));
            }
			Logger.LogDebug("<<< Exiting ValidUserLoginShouldPass()");
        }

        [Test]
        public void RequestUsingInvaldatedTokenAfterLogoutShouldFail()
        {
			Logger.LogDebug(">>> Entering RequestUsingInvaldatedTokenAfterLogoutShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);

			Logger.LogWarning("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): token → {Token}", token);
            this.SetAuthHeaderToken(token.AccessToken);
            var result1 = this._httpClient.GetAsync(TestGetQuery).Result;
            var content1 = result1.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): result1 → {Result1}", result1);
            LogText(@"content1 => " + content1);
            Assert.That(result1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Logger.LogDebug("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): content1 → {Content1}", content1);

            this.LogoutUser(token.AccessToken);

            this.SetAuthHeaderToken(token.AccessToken);
            var result2 = this._httpClient.GetAsync(TestGetQuery).Result;
            var content2 = result2.Content.ReadAsStringAsync().Result;
			Logger.LogDebug("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): result2 → {Result2}", result2);
            LogText(@"content2 => " + content2);
			Logger.LogDebug("RequestUsingInvaldatedTokenAfterLogoutShouldFail(): content2 → {Content2}", content2);
            Assert.That(result2.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("<<< Exiting RequestUsingInvaldatedTokenAfterLogoutShouldFail()");
        }

        [Test]
        public void RequestWithoutTokenShouldFail()
        {
			Logger.LogDebug(">>> Entering RequestWithoutTokenShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("RequestWithoutTokenShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("RequestWithoutTokenShouldFail(): result → {Result}", result);
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("<<< Exiting RequestWithoutTokenShouldFail()");
			Logger.LogDebug("RequestWithoutTokenShouldFail(): content → {Content}", content);
        }

        [Test]
        public void ValidatedUserGetRequestShouldPass()
        {
			Logger.LogDebug(">>> Entering ValidatedUserGetRequestShouldPass()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
			Logger.LogWarning("ValidatedUserGetRequestShouldPass(): token → {Token}", token);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("ValidatedUserGetRequestShouldPass(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("ValidatedUserGetRequestShouldPass(): result → {Result}", result);
            LogText(@"content => " + content);
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("ValidatedUserGetRequestShouldPass(): content → {Content}", content);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(content.IndexOf("You are authenticated through JWT", StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
            }
			Logger.LogDebug("<<< Exiting ValidatedUserGetRequestShouldPass()");
        }

        [Test]
        public void ValidatedUserPostRequestShouldPass()
        {
			Logger.LogDebug(">>> Entering ValidatedUserPostRequestShouldPass()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
			Logger.LogWarning("ValidatedUserPostRequestShouldPass(): token → {Token}", token);
            var result = this._httpClient.PostAsJsonAsync(TestPostQuery, new { text = "Integraton Testing Rocks!" }).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("ValidatedUserPostRequestShouldPass(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("ValidatedUserPostRequestShouldPass(): result → {Result}", result);
            LogText(@"content => " + content);
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("ValidatedUserPostRequestShouldPass(): content → {Content}", content);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(content.IndexOf("You are authenticated through JWT", StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
                Assert.That(content.IndexOf("You said: (Integraton Testing Rocks!)", StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
            }
			Logger.LogDebug("<<< Exiting ValidatedUserPostRequestShouldPass()");
        }

        [Test]
        public void RenewValidTokenShouldPass()
        {
			Logger.LogDebug(">>> Entering RenewValidTokenShouldPass()");
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var token2 = this.RenewAuthorizationToken(token1);
			Logger.LogDebug("RenewValidTokenShouldPass(): token1 → {Token1}", token1);
			Logger.LogDebug("RenewValidTokenShouldPass(): token2 → {Token2}", token2);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(token2.AccessToken, Is.Not.EqualTo(token1.AccessToken));
                Assert.That(token2.RenewalToken, Is.EqualTo(token1.RenewalToken));
                Assert.That(token2.DisplayName, Is.EqualTo(token1.DisplayName));
            }
			Logger.LogDebug("<<< Exiting RenewValidTokenShouldPass()");
        }

        [Test]
        public void UsingOriginalAccessTokenAfterRenewalShouldFail()
        {
			Logger.LogDebug(">>> Entering UsingOriginalAccessTokenAfterRenewalShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var token2 = this.RenewAuthorizationToken(token1);
			Logger.LogDebug("UsingOriginalAccessTokenAfterRenewalShouldFail(): token1 → {Token1}", token1);
			Logger.LogDebug("UsingOriginalAccessTokenAfterRenewalShouldFail(): token2 → {Token2}", token2);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(token2.AccessToken, Is.Not.EqualTo(token1.AccessToken));
                Assert.That(token2.RenewalToken, Is.EqualTo(token1.RenewalToken));
                Assert.That(token2.DisplayName, Is.EqualTo(token1.DisplayName));
            }

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("UsingOriginalAccessTokenAfterRenewalShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("UsingOriginalAccessTokenAfterRenewalShouldFail(): result → {Result}", result);
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("<<< Exiting UsingOriginalAccessTokenAfterRenewalShouldFail()");
			Logger.LogDebug("UsingOriginalAccessTokenAfterRenewalShouldFail(): content → {Content}", content);
        }

        [Test]
        public void TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail()
        {
			Logger.LogDebug(">>> Entering TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.RenewAuthorizationToken(token1);
			Logger.LogDebug("TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail(): token1 → {Token1}", token1);

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = token1.RenewalToken }).Result;
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("<<< Exiting TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail()");
			Logger.LogInformation("TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail(): result → {Result}", result);
        }

        [Test]
        public void RenewMultipleTimesShouldPass()
        {
			Logger.LogDebug(">>> Entering RenewMultipleTimesShouldPass()");
            this.RenewAuthorizationToken(
                this.RenewAuthorizationToken(
                    this.RenewAuthorizationToken(
                        this.GetAuthorizationTokenFor(this._hostName, this._hostPass))));
						Logger.LogDebug("<<< Exiting RenewMultipleTimesShouldPass()");
        }

        [Test]
        public void UsingTheNewAccessTokenAfterRenewalShouldPass()
        {
			Logger.LogDebug(">>> Entering UsingTheNewAccessTokenAfterRenewalShouldPass()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var token2 = this.RenewAuthorizationToken(token1);
			Logger.LogDebug("UsingTheNewAccessTokenAfterRenewalShouldPass(): token1 → {Token1}", token1);
			Logger.LogDebug("UsingTheNewAccessTokenAfterRenewalShouldPass(): token2 → {Token2}", token2);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(token2.AccessToken, Is.Not.EqualTo(token1.AccessToken));
                Assert.That(token2.RenewalToken, Is.EqualTo(token1.RenewalToken));
                Assert.That(token2.DisplayName, Is.EqualTo(token1.DisplayName));
            }

            this.SetAuthHeaderToken(token2.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("UsingTheNewAccessTokenAfterRenewalShouldPass(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("UsingTheNewAccessTokenAfterRenewalShouldPass(): result → {Result}", result);
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Logger.LogDebug("<<< Exiting UsingTheNewAccessTokenAfterRenewalShouldPass()");
			Logger.LogDebug("UsingTheNewAccessTokenAfterRenewalShouldPass(): content → {Content}", content);
        }

        [Test]
        public void TamperedTokenShouldFail()
        {
			Logger.LogDebug(">>> Entering TamperedTokenShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);

			Logger.LogDebug("TamperedTokenShouldFail(): token1 → {Token1}", token1);
            // tampered header
            this.SetAuthHeaderToken("x" + token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("TamperedTokenShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("TamperedTokenShouldFail(): result → {Result}", result);
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("TamperedTokenShouldFail(): content → {Content}", content);

            // tampered signature
            this.SetAuthHeaderToken(token1.AccessToken + "y");
            result = this._httpClient.GetAsync(TestGetQuery).Result;
            content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

            // tampered claims
            var idx = token1.AccessToken.IndexOf('.');
            var tampered = token1.AccessToken.Substring(0, idx + 10) + "z" + token1.AccessToken.Substring(idx + 10);
			Logger.LogDebug("TamperedTokenShouldFail(): idx → {Idx}", idx);
			Logger.LogDebug("TamperedTokenShouldFail(): tampered → {Tampered}", tampered);
            this.SetAuthHeaderToken(tampered);
            result = this._httpClient.GetAsync(TestGetQuery).Result;
            content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("<<< Exiting TamperedTokenShouldFail()");
        }

        [Test]
        public void ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry()
        {
			Logger.LogDebug(">>> Entering ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry()");
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
			Logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): token1 → {Token1}", token1);
			Logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): parts → {Parts}", parts);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
			Logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): decoded → {Decoded}", decoded);
			Logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): claims → {Claims}", claims);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();
			Logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): TokenId → {TokenId}", TokenId);
			Logger.LogInformation("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            var token2 = this.RenewAuthorizationToken(token1);
            parts = token2.AccessToken.Split('.');
			Logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): token2 → {Token2}", token2);
            decoded = DecodeBase64(parts[1]);
            claims = JsonConvert.DeserializeObject(decoded);
            long claimExpiry = claims.exp;
            var expiryInToken = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(claimExpiry);
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): expiryInToken → {ExpiryInToken}", expiryInToken);
                Assert.That(DateTime.UtcNow, Is.LessThan(expiryInToken));
                Assert.That(expiryInToken, Is.LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(31))); // appears the library rounds the time
            }

            var record = DatabaseHelper.GetRecordById("JsonWebTokens", "TokenId", sessionId);
            var accessExpiry = (DateTime)record["TokenExpiry"];
			Logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): record → {Record}", record);
			Logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): accessExpiry → {AccessExpiry}", accessExpiry);
            var renewalExpiry = (DateTime)record["RenewalExpiry"];
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry(): renewalExpiry → {RenewalExpiry}", renewalExpiry);
                Assert.That(renewalExpiry, Is.EqualTo(accessExpiry));
                Assert.That(DateTime.UtcNow, Is.LessThan(renewalExpiry));
            }
            Assert.That(renewalExpiry, Is.LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(31)));
            Assert.That(expiryInToken, Is.EqualTo(accessExpiry));
			Logger.LogDebug("<<< Exiting ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry()");
        }

        [Test]
        public void UsingExpiredAccessTokenShouldFail()
        {
			Logger.LogDebug(">>> Entering UsingExpiredAccessTokenShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
			Logger.LogDebug("UsingExpiredAccessTokenShouldFail(): token1 → {Token1}", token1);
			Logger.LogDebug("UsingExpiredAccessTokenShouldFail(): parts → {Parts}", parts);
			Logger.LogDebug("UsingExpiredAccessTokenShouldFail(): decoded → {Decoded}", decoded);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET TokenExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
				Logger.LogDebug("UsingExpiredAccessTokenShouldFail(): claims → {Claims}", claims);
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();
			Logger.LogDebug("UsingExpiredAccessTokenShouldFail(): TokenId → {TokenId}", TokenId);
			Logger.LogInformation("UsingExpiredAccessTokenShouldFail(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("UsingExpiredAccessTokenShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("UsingExpiredAccessTokenShouldFail(): result → {Result}", result);
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("<<< Exiting UsingExpiredAccessTokenShouldFail()");
			Logger.LogDebug("UsingExpiredAccessTokenShouldFail(): content → {Content}", content);
        }

        [Test]
        public void UsingExpiredRenewalTokenShouldFail()
        {
			Logger.LogDebug(">>> Entering UsingExpiredRenewalTokenShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
			Logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): token1 → {Token1}", token1);
			Logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): parts → {Parts}", parts);
			Logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): decoded → {Decoded}", decoded);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
				Logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): claims → {Claims}", claims);
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();
			Logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): TokenId → {TokenId}", TokenId);
			Logger.LogInformation("UsingExpiredRenewalTokenShouldFail(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("UsingExpiredRenewalTokenShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): result → {Result}", result);
            LogText(@"content => " + content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("<<< Exiting UsingExpiredRenewalTokenShouldFail()");
			Logger.LogDebug("UsingExpiredRenewalTokenShouldFail(): content → {Content}", content);
        }

        [Test]
        public void TryingToRenewUsingAnExpiredRenewalTokenShouldFail()
        {
			Logger.LogDebug(">>> Entering TryingToRenewUsingAnExpiredRenewalTokenShouldFail()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
			Logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): token1 → {Token1}", token1);
			Logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): parts → {Parts}", parts);
			Logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): decoded → {Decoded}", decoded);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
				Logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): claims → {Claims}", claims);
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();
			Logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): TokenId → {TokenId}", TokenId);
			Logger.LogInformation("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = token1.RenewalToken }).Result;
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("<<< Exiting TryingToRenewUsingAnExpiredRenewalTokenShouldFail()");
			Logger.LogInformation("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("TryingToRenewUsingAnExpiredRenewalTokenShouldFail(): result → {Result}", result);
        }

        [Test]
        [TestCase(GetMonikerQuery)]
        [TestCase(GetModuleDetailsQuery)]
        public void CallingHelperForLoggedinUserShouldReturnSuccess(string query)
        {
			Logger.LogDebug(">>> Entering CallingHelperForLoggedinUserShouldReturnSuccess(query={Query})", query);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
			Logger.LogWarning("CallingHelperForLoggedinUserShouldReturnSuccess(query={Query}): token → {Token}", query, token);
            var result = this._httpClient.GetAsync(query + HttpUtility.UrlEncode("ViewProfile")).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("CallingHelperForLoggedinUserShouldReturnSuccess(query)={Query}: external service call took {Arg1} ms", query, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("CallingHelperForLoggedinUserShouldReturnSuccess(query={Query}): result → {Result}", query, result);
            LogText(@"content => " + content);
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("CallingHelperForLoggedinUserShouldReturnSuccess(query={Query}): content → {Content}", query, content);
                Assert.That(content, Is.Not.Null);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
			Logger.LogDebug("<<< Exiting CallingHelperForLoggedinUserShouldReturnSuccess(query={Query})", query);
        }

        [Test]
        public void ValidatingSuccessWhenUsingExistingMoniker()
        {
			Logger.LogDebug(">>> Entering ValidatingSuccessWhenUsingExistingMoniker()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            const string query1 =
                 @"SELECT TOP(1) TabModuleId FROM {objectQualifier}TabModules
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
	                WHERE TabId IN (SELECT TabId FROM {objectQualifier}Tabs WHERE TabName='Activity Feed')
	                  AND ModuleTitle='Journal';";
            var tabModuleId = DatabaseHelper.ExecuteScalar<int>(query1);
			Logger.LogDebug("ValidatingSuccessWhenUsingExistingMoniker(): ModuleTitle → {ModuleTitle}", ModuleTitle);
			Logger.LogDebug("ValidatingSuccessWhenUsingExistingMoniker(): tabModuleId → {TabModuleId}", tabModuleId);
            Assert.That(tabModuleId, Is.GreaterThan(0));

            // These will set a moniker for the Activity Feed module of the user profile
            DatabaseHelper.ExecuteNonQuery(@"EXEC {objectQualifier}DeleteTabModuleSetting " + tabModuleId + @", 'Moniker'");
			Logger.LogInformation("ValidatingSuccessWhenUsingExistingMoniker(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            DatabaseHelper.ExecuteNonQuery(@"EXEC {objectQualifier}UpdateTabModuleSetting " + tabModuleId + @", 'Moniker', 'myjournal', 1");
            WebApiTestHelper.ClearHostCache();

            // Act
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
			Logger.LogWarning("ValidatingSuccessWhenUsingExistingMoniker(): token → {Token}", token);
            this.SetMonikerHeader("myjournal");
            var postItem = new { ProfileId = 1, GroupId = -1, RowIndex = 0, MaxRows = 1 };
            var result = this._httpClient.PostAsJsonAsync(
                "/API/Journal/Services/GetListForProfile", postItem).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("ValidatingSuccessWhenUsingExistingMoniker(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            LogText(@"content => " + content);
			Logger.LogDebug("ValidatingSuccessWhenUsingExistingMoniker(): content → {Content}", content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Logger.LogDebug("<<< Exiting ValidatingSuccessWhenUsingExistingMoniker()");
        }

        [Test]
        public void ValidatingFailureWhenUsingNonExistingMoniker()
        {
			Logger.LogDebug(">>> Entering ValidatingFailureWhenUsingNonExistingMoniker()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            const string query1 =
                 @"SELECT TOP(1) TabModuleId FROM {objectQualifier}TabModules
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
	                WHERE TabId IN (SELECT TabId FROM {objectQualifier}Tabs WHERE TabName='Activity Feed')
	                  AND ModuleTitle='Journal';";
            var tabModuleId = DatabaseHelper.ExecuteScalar<int>(query1);
			Logger.LogDebug("ValidatingFailureWhenUsingNonExistingMoniker(): ModuleTitle → {ModuleTitle}", ModuleTitle);
			Logger.LogDebug("ValidatingFailureWhenUsingNonExistingMoniker(): tabModuleId → {TabModuleId}", tabModuleId);
            Assert.That(tabModuleId, Is.GreaterThan(0));

            // These will set a moniker for the Activity Feed module of the user profile
            DatabaseHelper.ExecuteNonQuery(@"EXEC {objectQualifier}DeleteTabModuleSetting " + tabModuleId + @", 'Moniker'");
			Logger.LogInformation("ValidatingFailureWhenUsingNonExistingMoniker(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            WebApiTestHelper.ClearHostCache();

            // Act
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
			Logger.LogWarning("ValidatingFailureWhenUsingNonExistingMoniker(): token → {Token}", token);
            this.SetMonikerHeader("myjournal");
            var postItem = new { ProfileId = 1, GroupId = -1, RowIndex = 0, MaxRows = 1 };
            var result = this._httpClient.PostAsJsonAsync(
                "/API/Journal/Services/GetListForProfile", postItem).Result;
            var content = result.Content.ReadAsStringAsync().Result;
			Logger.LogInformation("ValidatingFailureWhenUsingNonExistingMoniker(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
            LogText(@"content => " + content);
			Logger.LogDebug("ValidatingFailureWhenUsingNonExistingMoniker(): content → {Content}", content);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			Logger.LogDebug("<<< Exiting ValidatingFailureWhenUsingNonExistingMoniker()");
        }

        private static string DecodeBase64(string b64Str)
        {
			Logger.LogDebug(">>> Entering DecodeBase64(b64Str={B64Str})", b64Str);
            // fix Base64 string padding
            var mod = b64Str.Length % 4;
            if (mod != 0)
            {
                b64Str += new string('=', 4 - mod);
            }

			Logger.LogDebug("<<< Exiting DecodeBase64(b64Str={B64Str})", b64Str);
            return TextEncoder.GetString(Convert.FromBase64String(b64Str));
        }

        // template to help in copy/paste of new test methods
        /*
        [Test]
        public void TemplateMethod()
        {
			Logger.LogDebug(">>> Entering TemplateMethod()");
            Assert.Fail();
			Logger.LogDebug("<<< Exiting TemplateMethod()");
        }
         */

        private LoginResultData GetAuthorizationTokenFor(string uname, string upass)
        {
			Logger.LogDebug(">>> Entering GetAuthorizationTokenFor(uname={Uname},upass={Upass})", uname, upass);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var credentials = new { u = uname, p = upass };
            var result = this._httpClient.PostAsJsonAsync(LoginQuery, credentials).Result;
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Logger.LogInformation("GetAuthorizationTokenFor(uname,upass)={Uname},{Upass}: external service call took {Arg2} ms", uname, upass, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("GetAuthorizationTokenFor(uname={Uname},upass={Upass}): result → {Result}", uname, upass, result);
            var token = result.Content.ReadAsAsync<LoginResultData>().Result;
            Assert.That(token, Is.Not.Null);
			Logger.LogWarning("GetAuthorizationTokenFor(uname={Uname},upass={Upass}): token → {Token}", uname, upass, token);
            LogText(@"AuthToken => " + JsonConvert.SerializeObject(token));
            this._httpClient.DefaultRequestHeaders.Clear();
			Logger.LogDebug("<<< Exiting GetAuthorizationTokenFor(uname={Uname},upass={Upass})", uname, upass);
            return token;
        }

        private LoginResultData RenewAuthorizationToken(LoginResultData currentToken)
        {
			Logger.LogDebug(">>> Entering RenewAuthorizationToken(currentToken={CurrentToken})", currentToken);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            Thread.Sleep(1000); // must delay at least 1 second so the expiry time is different
            this.SetAuthHeaderToken(currentToken.AccessToken);
            var result = this._httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = currentToken.RenewalToken }).Result;
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Logger.LogInformation("RenewAuthorizationToken(currentToken)={CurrentToken}: external service call took {Arg1} ms", currentToken, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("RenewAuthorizationToken(currentToken={CurrentToken}): result → {Result}", currentToken, result);
            var token = result.Content.ReadAsAsync<LoginResultData>().Result;
            Assert.That(token, Is.Not.Null);
			Logger.LogWarning("RenewAuthorizationToken(currentToken={CurrentToken}): token → {Token}", currentToken, token);
            LogText(@"RenewedToken => " + JsonConvert.SerializeObject(token));
			Logger.LogDebug("<<< Exiting RenewAuthorizationToken(currentToken={CurrentToken})", currentToken);
            return token;
        }

        private void LogoutUser(string accessToken)
        {
			Logger.LogDebug(">>> Entering LogoutUser(accessToken={AccessToken})", accessToken);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.SetAuthHeaderToken(accessToken);
            var result = this._httpClient.GetAsync(LogoutQuery).Result;
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Logger.LogInformation("LogoutUser(accessToken)={AccessToken}: external service call took {Arg1} ms", accessToken, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("LogoutUser(accessToken={AccessToken}): result → {Result}", accessToken, result);
            var content = result.Content.ReadAsStringAsync().Result;
            dynamic response = !string.IsNullOrEmpty(content) ? JsonConvert.DeserializeObject(content) : null;
			Logger.LogDebug("LogoutUser(accessToken={AccessToken}): content → {Content}", accessToken, content);
            Assert.That(Convert.ToBoolean(response?.success), Is.True);
			Logger.LogDebug("<<< Exiting LogoutUser(accessToken={AccessToken})", accessToken);
        }

        private void SetAuthHeaderToken(string token, string scheme = "Bearer")
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this._httpClient.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse(scheme.Trim() + " " + token.Trim());
        }

        private void SetMonikerHeader(string monikerValue)
        {
			Logger.LogDebug(">>> Entering SetMonikerHeader(monikerValue={MonikerValue})", monikerValue);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this._httpClient.DefaultRequestHeaders.Add("X-DNN-MONIKER", monikerValue);
			Logger.LogDebug("<<< Exiting SetMonikerHeader(monikerValue={MonikerValue})", monikerValue);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("SetMonikerHeader(monikerValue)={MonikerValue}: external service call took {Arg1} ms", monikerValue, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("SetMonikerHeader(monikerValue)={MonikerValue}: external service call took {Arg1} ms", monikerValue, __logStart.ElapsedMilliseconds);
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
