// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using DotNetNuke.Security.Membership;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.Web.Api.Auth;
    using NUnit.Framework;

    [TestFixture]
    public class DigestAuthMessageHandlerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            var mockMembership = MockComponentProvider.CreateNew<MembershipProvider>();
            mockMembership.Setup(m => m.PasswordRetrievalEnabled).Returns(true);
			Logger.LogDebug("<<< Exiting Setup()");
        }

        [Test]
        public void SetsWwwAuthenticateHeaderOn401()
        {
			Logger.LogDebug(">>> Entering SetsWwwAuthenticateHeaderOn401()");
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };

			Logger.LogDebug("SetsWwwAuthenticateHeaderOn401(): response → {Response}", response);
            // Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);
			Logger.LogDebug("SetsWwwAuthenticateHeaderOn401(): handler → {Handler}", handler);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(response.Headers.WwwAuthenticate.First().Scheme, Is.EqualTo("Digest"));
                Assert.That(response.Headers.WwwAuthenticate.First().Parameter, Does.Contain("realm=\"DNNAPI\""));
            }
			Logger.LogDebug("<<< Exiting SetsWwwAuthenticateHeaderOn401()");
        }

        [Test]
        public void OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest()
        {
			Logger.LogDebug(">>> Entering OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest()");
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };
            response.RequestMessage.Headers.Add("X-REQUESTED-WITH", "XmlHttpRequest");
			Logger.LogDebug("OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest(): response → {Response}", response);

            // Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);
			Logger.LogDebug("OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest(): handler → {Handler}", handler);

            // Assert
            Assert.That(response.Headers.WwwAuthenticate, Is.Empty);
			Logger.LogDebug("<<< Exiting OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest()");
        }

        [Test]
        public void MissingXmlHttpRequestValueDoesntThrowNullException()
        {
			Logger.LogDebug(">>> Entering MissingXmlHttpRequestValueDoesntThrowNullException()");
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };
            response.RequestMessage.Headers.Add("X-REQUESTED-WITH", string.Empty);
			Logger.LogDebug("MissingXmlHttpRequestValueDoesntThrowNullException(): response → {Response}", response);

            // Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);
			Logger.LogDebug("MissingXmlHttpRequestValueDoesntThrowNullException(): handler → {Handler}", handler);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(response.Headers.WwwAuthenticate.First().Scheme, Is.EqualTo("Digest"));
                Assert.That(response.Headers.WwwAuthenticate.First().Parameter, Does.Contain("realm=\"DNNAPI\""));
            }
			Logger.LogDebug("<<< Exiting MissingXmlHttpRequestValueDoesntThrowNullException()");
        }

        [Test]
        public void ResponseWithNullRequestReturnsUnauthorized()
        {
			Logger.LogDebug(">>> Entering ResponseWithNullRequestReturnsUnauthorized()");
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = null };

			Logger.LogDebug("ResponseWithNullRequestReturnsUnauthorized(): response → {Response}", response);
            // Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);
			Logger.LogDebug("ResponseWithNullRequestReturnsUnauthorized(): handler → {Handler}", handler);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(response.Headers.WwwAuthenticate.First().Scheme, Is.EqualTo("Digest"));
                Assert.That(response.Headers.WwwAuthenticate.First().Parameter, Does.Contain("realm=\"DNNAPI\""));
            }
			Logger.LogDebug("<<< Exiting ResponseWithNullRequestReturnsUnauthorized()");
        }

        // todo unit test actual authentication code
        // very hard to unit test inbound authentication code as it dips into untestable bits of usercontroller etc.
        // need to write controllers with interfaces and servicelocator
    }
}
