// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Instance.Utilities
{
    using System;
    using System.IO;
    using System.Web;
	using Microsoft.Extensions.Logging;

    public static class UnitTestHelper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>Sets the HTTP context with a valid simulated request.</summary>
        /// <param name="host">Host.</param>
        /// <param name="application">Application.</param>
        /// <param name="appPhysicalDir"></param>
        /// <param name="pageName"></param>
        public static void SetHttpContextWithSimulatedRequest(string host, string application, string appPhysicalDir, string pageName)
        {
			_logger.LogDebug(">>> Entering SetHttpContextWithSimulatedRequest(host={Host},application={Application},appPhysicalDir={AppPhysicalDir},pageName={PageName})", host, application, appPhysicalDir, pageName);
            const string appVirtualDir = "/";
            var page = application.Replace("/", string.Empty) + "/" + pageName;
			_logger.LogDebug("SetHttpContextWithSimulatedRequest(host={Host},application={Application},appPhysicalDir={AppPhysicalDir},pageName={PageName}): page → {Page}", host, application, appPhysicalDir, pageName, page);
            var query = string.Empty;
            TextWriter output = null;

            var workerRequest = new SimulatedHttpRequest(appVirtualDir, appPhysicalDir, page, query, output, host);
			_logger.LogDebug("SetHttpContextWithSimulatedRequest(host={Host},application={Application},appPhysicalDir={AppPhysicalDir},pageName={PageName}): workerRequest → {WorkerRequest}", host, application, appPhysicalDir, pageName, workerRequest);
            HttpContext.Current = new HttpContext(workerRequest);
			_logger.LogDebug("<<< Exiting SetHttpContextWithSimulatedRequest(host={Host},application={Application},appPhysicalDir={AppPhysicalDir},pageName={PageName})", host, application, appPhysicalDir, pageName);
        }

        public static void ClearHttpContext()
        {
			_logger.LogDebug(">>> Entering ClearHttpContext()");
			_logger.LogDebug("ClearHttpContext(): HttpContext.Current → {Arg0}", HttpContext.Current);
            HttpContext.Current = null;
			_logger.LogDebug("<<< Exiting ClearHttpContext()");
        }
    }
}
