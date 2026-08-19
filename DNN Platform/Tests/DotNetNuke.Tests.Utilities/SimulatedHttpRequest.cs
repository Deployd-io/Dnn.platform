// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Instance.Utilities
{
    using System;
    using System.IO;
    using System.Web.Hosting;
	using Microsoft.Extensions.Logging;

    /// <summary>Used to simulate an HttpRequest.</summary>
    public class SimulatedHttpRequest : SimpleWorkerRequest
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly string _host;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulatedHttpRequest"/> class.
        /// Creates a new <see cref="SimulatedHttpRequest"/> instance.
        /// </summary>
        /// <param name="appVirtualDir">App virtual dir.</param>
        /// <param name="appPhysicalDir">App physical dir.</param>
        /// <param name="page">Page.</param>
        /// <param name="query">Query.</param>
        /// <param name="output">Output.</param>
        /// <param name="host">Host.</param>
        public SimulatedHttpRequest(string appVirtualDir, string appPhysicalDir, string page, string query, TextWriter output, string host)
            : base(appVirtualDir, appPhysicalDir, page, query, output)
        {
			_logger.LogDebug(">>> Entering SimulatedHttpRequest(appVirtualDir={AppVirtualDir},appPhysicalDir={AppPhysicalDir},page={Page},query={Query},output={Output},host={Host})", appVirtualDir, appPhysicalDir, page, output, host);
            if (string.IsNullOrEmpty(host))
            {
				_logger.LogWarning("Throwing ArgumentNullException from SimulatedHttpRequest(appVirtualDir={AppVirtualDir},appPhysicalDir={AppPhysicalDir},page={Page},query={Query},output={Output},host={Host})", appVirtualDir, appPhysicalDir, page, output, host);
                throw new ArgumentNullException("host", "Host cannot be null nor empty.");
            }

            this._host = host;
			_logger.LogDebug("SimulatedHttpRequest(appVirtualDir={AppVirtualDir},appPhysicalDir={AppPhysicalDir},page={Page},query={Query},output={Output},host={Host}): this._host → {Arg6}", appVirtualDir, appPhysicalDir, page, output, host, this._host);
			_logger.LogDebug("<<< Exiting SimulatedHttpRequest(appVirtualDir={AppVirtualDir},appPhysicalDir={AppPhysicalDir},page={Page},query={Query},output={Output},host={Host})", appVirtualDir, appPhysicalDir, page, output, host);
        }

        /// <summary>Gets the name of the server.</summary>
        /// <returns></returns>
        public override string GetServerName()
        {
			_logger.LogDebug(">>> Entering GetServerName()");
			_logger.LogDebug("<<< Exiting GetServerName()");
            return this._host;
        }

        /// <summary>Maps the path to a filesystem path.</summary>
        /// <param name="virtualPath">Virtual path.</param>
        /// <returns></returns>
        public override string MapPath(string virtualPath)
        {
			_logger.LogDebug(">>> Entering MapPath(virtualPath={VirtualPath})", virtualPath);
            var path = string.Empty;
            var appPath = this.GetAppPath();
			_logger.LogDebug("MapPath(virtualPath={VirtualPath}): appPath → {AppPath}", virtualPath, appPath);
			_logger.LogDebug("MapPath(virtualPath={VirtualPath}): path → {Path}", virtualPath, path);

            if (appPath != null)
            {
                path = Path.Combine(appPath, virtualPath);
            }

			_logger.LogDebug("<<< Exiting MapPath(virtualPath={VirtualPath})", virtualPath);
            return path;
        }
    }
}
