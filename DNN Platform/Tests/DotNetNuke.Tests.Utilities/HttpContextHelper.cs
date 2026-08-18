// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Web;

    using DotNetNuke.Common;
    using Moq;
	using Microsoft.Extensions.Logging;

    public class HttpContextHelper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>
        /// Return Response object with default values for missing ones
        /// _mockRequest = Mock.Get(_mockhttpContext.Object.Request);
        /// syntax _mockRequest.SetupGet(x => x.[PropertyName]).Returns(...);
        /// e.g. SetupGet(x => x.ServerVariables).Returns(new NameValueCollection()).
        /// </summary>
        /// <returns>HttpResponseBase.</returns>
        public static Mock<HttpContextBase> RegisterMockHttpContext()
        {
			_logger.LogDebug(">>> Entering RegisterMockHttpContext()");
            var mock = CreateMockHttpContext();
            HttpContextSource.RegisterInstance(mock.Object);
			_logger.LogDebug("RegisterMockHttpContext(): mock → {Mock}", mock);
			_logger.LogDebug("<<< Exiting RegisterMockHttpContext()");
            return mock;
        }

        private static Mock<HttpContextBase> CreateMockHttpContext()
        {
			_logger.LogDebug(">>> Entering CreateMockHttpContext()");
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Items).Returns(new Hashtable());
			_logger.LogDebug("CreateMockHttpContext(): context → {Context}", context);
            context.SetupGet(x => x.Request).Returns(GetMockRequestBase());
            context.SetupGet(x => x.Response).Returns(GetMockResponseBase());
            context.SetupGet(x => x.Session).Returns(GetMockSessionStateBase());
            context.SetupGet(x => x.Server).Returns(GetMockServerUtilityBase());
			_logger.LogDebug("<<< Exiting CreateMockHttpContext()");
            return context;
        }

        private static HttpServerUtilityBase GetMockServerUtilityBase()
        {
			_logger.LogDebug(">>> Entering GetMockServerUtilityBase()");
            var mockServerUtility = new Mock<HttpServerUtilityBase>();
            mockServerUtility.SetupAllProperties();
			_logger.LogDebug("GetMockServerUtilityBase(): mockServerUtility → {MockServerUtility}", mockServerUtility);
			_logger.LogDebug("<<< Exiting GetMockServerUtilityBase()");
            return mockServerUtility.Object;
        }

        private static HttpSessionStateBase GetMockSessionStateBase()
        {
			_logger.LogDebug(">>> Entering GetMockSessionStateBase()");
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupAllProperties();
			_logger.LogDebug("GetMockSessionStateBase(): mockSession → {MockSession}", mockSession);
			_logger.LogDebug("<<< Exiting GetMockSessionStateBase()");
            return mockSession.Object;
        }

        private static HttpResponseBase GetMockResponseBase()
        {
			_logger.LogDebug(">>> Entering GetMockResponseBase()");
            var mockResponse = new Mock<HttpResponseBase>();
            mockResponse.SetupAllProperties();
			_logger.LogDebug("GetMockResponseBase(): mockResponse → {MockResponse}", mockResponse);
			_logger.LogDebug("<<< Exiting GetMockResponseBase()");
            return mockResponse.Object;
        }

        private static HttpRequestBase GetMockRequestBase()
        {
			_logger.LogDebug(">>> Entering GetMockRequestBase()");
            var request = new Mock<HttpRequestBase>();
            request.SetupAllProperties();
			_logger.LogDebug("GetMockRequestBase(): request → {Request}", request);
            request.SetupGet(x => x.Headers).Returns(new NameValueCollection());
            request.SetupGet(x => x.InputStream).Returns(new MemoryStream());
            request.SetupGet(x => x.Params).Returns(new NameValueCollection());
            request.SetupGet(x => x.QueryString).Returns(new NameValueCollection());
            request.SetupGet(x => x.ServerVariables).Returns(new NameValueCollection());
            request.SetupGet(x => x.UserAgent).Returns(string.Empty);
            request.SetupGet(x => x.UserHostAddress).Returns(string.Empty);
            request.SetupGet(x => x.UserLanguages).Returns(new[] { string.Empty });
			_logger.LogDebug("<<< Exiting GetMockRequestBase()");
            return request.Object;
        }
    }
}
