// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host
{
    using System;
    using System.Net;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;

    public class ServerWebRequestAdapter : IServerWebRequestAdapter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerWebRequestAdapter));
        /// <inheritdoc />
        public virtual string GetServerUrl()
        {
			Logger.LogDebug(">>> Entering GetServerUrl()");
            var domainName = string.Empty;
            if (HttpContext.Current != null)
            {
                domainName = Globals.GetDomainName(HttpContext.Current.Request);

                if (domainName.Contains("/", StringComparison.Ordinal))
                {
                    domainName = domainName.Substring(0, domainName.IndexOf("/", StringComparison.Ordinal));

                    if (!string.IsNullOrEmpty(Globals.ApplicationPath))
                    {
                        domainName = $"{domainName}{Globals.ApplicationPath}";
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetServerUrl()");
            return domainName;
        }

        /// <inheritdoc />
        public virtual string GetServerUniqueId()
        {
			Logger.LogDebug(">>> Entering GetServerUniqueId()");
			Logger.LogDebug("<<< Exiting GetServerUniqueId()");
            return string.Empty;
        }

        /// <inheritdoc />
        public virtual void ProcessRequest(HttpWebRequest request, ServerInfo server)
        {
		Logger.LogDebug(">>> Entering ProcessRequest(request={Request},server={Server})", request, server);
		Logger.LogDebug("<<< Exiting ProcessRequest(request={Request},server={Server})", request, server);
        }

        /// <inheritdoc />
        public virtual void CheckResponse(HttpWebResponse response, ServerInfo server, ref HttpStatusCode statusCode)
        {
		Logger.LogDebug(">>> Entering CheckResponse(response={Response},server={Server},statusCode={StatusCode})", response, server, statusCode);
		Logger.LogDebug("<<< Exiting CheckResponse(response={Response},server={Server},statusCode={StatusCode})", response, server, statusCode);
        }
    }
}
