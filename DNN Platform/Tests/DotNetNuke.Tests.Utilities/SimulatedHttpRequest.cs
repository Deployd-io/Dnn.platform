// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Instance.Utilities
{
    using System;
    using System.IO;
    using System.Web.Hosting;

    /// <summary>Used to simulate an HttpRequest.</summary>
    public class SimulatedHttpRequest : SimpleWorkerRequest
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
			Logger.LogDebug(">>> Entering SimulatedHttpRequest(appVirtualDir={AppVirtualDir},appPhysicalDir={AppPhysicalDir},page={Page},query={Query},output={Output},host={Host})", appVirtualDir, appPhysicalDir, page, output, host);
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException("host", "Host cannot be null nor empty.");
            }

            this._host = host;
			Logger.LogDebug("<<< Exiting SimulatedHttpRequest(appVirtualDir={AppVirtualDir},appPhysicalDir={AppPhysicalDir},page={Page},query={Query},output={Output},host={Host})", appVirtualDir, appPhysicalDir, page, output, host);
			Logger.LogDebug("SimulatedHttpRequest(appVirtualDir={AppVirtualDir},appPhysicalDir={AppPhysicalDir},page={Page},query={Query},output={Output},host={Host}): this._host → {Arg6}", appVirtualDir, appPhysicalDir, page, output, host, this._host);
        }

        /// <summary>Gets the name of the server.</summary>
        /// <returns></returns>
        public override string GetServerName()
        {
			Logger.LogDebug(">>> Entering GetServerName()");
			Logger.LogDebug("<<< Exiting GetServerName()");
            return this._host;
        }

        /// <summary>Maps the path to a filesystem path.</summary>
        /// <param name="virtualPath">Virtual path.</param>
        /// <returns></returns>
        public override string MapPath(string virtualPath)
        {
			Logger.LogDebug(">>> Entering MapPath(virtualPath={VirtualPath})", virtualPath);
            var path = string.Empty;
            var appPath = this.GetAppPath();
			Logger.LogDebug("MapPath(virtualPath={VirtualPath}): path → {Path}", virtualPath, path);
			Logger.LogDebug("MapPath(virtualPath={VirtualPath}): appPath → {AppPath}", virtualPath, appPath);

            if (appPath != null)
            {
                path = Path.Combine(appPath, virtualPath);
            }

			Logger.LogDebug("<<< Exiting MapPath(virtualPath={VirtualPath})", virtualPath);
            return path;
        }
    }
}
