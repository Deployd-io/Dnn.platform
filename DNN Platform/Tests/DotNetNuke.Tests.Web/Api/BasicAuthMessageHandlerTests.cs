// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using DotNetNuke.Web.Api.Auth;
    using NUnit.Framework;

    [TestFixture]
    public class BasicAuthMessageHandlerTests
    {
        [Test]
        public void SetsWwwAuthenticateHeaderOn401()
        {
			eventLogger.LogDebug(">>> Entering SetsWwwAuthenticateHeaderOn401()");
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };
			eventLogger.LogDebug("SetsWwwAuthenticateHeaderOn401(): response → {Response}", response);

            // Act
            var handler = new BasicAuthMessageHandler(true, false);
			eventLogger.LogDebug("SetsWwwAuthenticateHeaderOn401(): handler → {Handler}", handler);
            handler.OnOutboundResponse(response, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(response.Headers.WwwAuthenticate.First().Scheme, Is.EqualTo("Basic"));
                Assert.That(response.Headers.WwwAuthenticate.First().Parameter, Is.EqualTo("realm=\"DNNAPI\""));
            }
			eventLogger.LogDebug("<<< Exiting SetsWwwAuthenticateHeaderOn401()");
        }

        [Test]
        public void OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest()
        {
			eventLogger.LogDebug(">>> Entering OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest()");
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            { RequestMessage = new HttpRequestMessage() };
            response.RequestMessage.Headers.Add("X-REQUESTED-WITH", "XmlHttpRequest");

            // Act
            var handler = new BasicAuthMessageHandler(true, false);
			eventLogger.LogDebug("OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest(): handler → {Handler}", handler);
            handler.OnOutboundResponse(response, CancellationToken.None);

            // Assert
            Assert.That(response.Headers.WwwAuthenticate, Is.Empty);
			eventLogger.LogDebug("<<< Exiting OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest()");
        }

        [Test]
        public void MissingXmlHttpRequestValueDoesntThrowNullException()
        {
			eventLogger.LogDebug(">>> Entering MissingXmlHttpRequestValueDoesntThrowNullException()");
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };
			eventLogger.LogDebug("MissingXmlHttpRequestValueDoesntThrowNullException(): response → {Response}", response);
            response.RequestMessage.Headers.Add("X-REQUESTED-WITH", string.Empty);

            // Act
            var handler = new BasicAuthMessageHandler(true, false);
			eventLogger.LogDebug("MissingXmlHttpRequestValueDoesntThrowNullException(): handler → {Handler}", handler);
            handler.OnOutboundResponse(response, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(response.Headers.WwwAuthenticate.First().Scheme, Is.EqualTo("Basic"));
                Assert.That(response.Headers.WwwAuthenticate.First().Parameter, Is.EqualTo("realm=\"DNNAPI\""));
            }
			eventLogger.LogDebug("<<< Exiting MissingXmlHttpRequestValueDoesntThrowNullException()");
        }

        [Test]
        public void ResponseWithNullRequestReturnsUnauthorized()
        {
			eventLogger.LogDebug(">>> Entering ResponseWithNullRequestReturnsUnauthorized()");
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = null };
			eventLogger.LogDebug("ResponseWithNullRequestReturnsUnauthorized(): response → {Response}", response);

            // Act
            var handler = new BasicAuthMessageHandler(true, false);
			eventLogger.LogDebug("ResponseWithNullRequestReturnsUnauthorized(): handler → {Handler}", handler);
            handler.OnOutboundResponse(response, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(response.Headers.WwwAuthenticate.First().Scheme, Is.EqualTo("Basic"));
                Assert.That(response.Headers.WwwAuthenticate.First().Parameter, Is.EqualTo("realm=\"DNNAPI\""));
            }
			eventLogger.LogDebug("<<< Exiting ResponseWithNullRequestReturnsUnauthorized()");
        }

        // todo unit test actual authentication code
        // very hard to unit test inbound authentication code as it dips into untestable bits of usercontroller etc.
        // need to write controllers with interfaces and servicelocator
    }
}
