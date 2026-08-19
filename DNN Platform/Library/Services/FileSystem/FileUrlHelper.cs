// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;

    public class FileUrlHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileUrlHelper));
        private static readonly Regex RegexStandardFile =
            new Regex(@"^/portals/(?<portal>[0-9]+|_default)/(?<filePath>.*\.[a-z0-9]*)$", RegexOptions.Compiled);

        public static bool IsStandardFileURLFormat(string requestPath, out IFileInfo fileRequested)
        {
			Logger.LogDebug(">>> Entering IsStandardFileURLFormat(requestPath={RequestPath})", requestPath);
            var match = RegexStandardFile.Match(requestPath.ToLowerInvariant());
            if (match.Success)
            {
                var filePath = match.Groups["filePath"].Value;
				Logger.LogDebug("IsStandardFileURLFormat(requestPath={RequestPath}): filePath → {FilePath}", requestPath, filePath);
                var portal = match.Groups["portal"].Value;
				Logger.LogDebug("IsStandardFileURLFormat(requestPath={RequestPath}): portal → {Portal}", requestPath, portal);

                var portalId = Null.NullInteger;
				Logger.LogDebug("IsStandardFileURLFormat(requestPath={RequestPath}): portalId → {PortalId}", requestPath, portalId);
                if (portal != "_default")
                {
                    portalId = int.Parse(portal, CultureInfo.InvariantCulture);
                }

                fileRequested = FileManager.Instance.GetFile(portalId, filePath);
				Logger.LogDebug("IsStandardFileURLFormat(requestPath={RequestPath}): fileRequested → {FileRequested}", requestPath, fileRequested);
                return true;
            }

            fileRequested = null;
			Logger.LogDebug("<<< Exiting IsStandardFileURLFormat(requestPath={RequestPath})", requestPath);
            return false;
        }

        public static bool IsLinkClickURLFormat(string requestPath, NameValueCollection requestQueryString, out IFileInfo fileRequested)
        {
			Logger.LogDebug(">>> Entering IsLinkClickURLFormat(requestPath={RequestPath},requestQueryString={RequestQueryString})", requestPath, requestQueryString);
            if (requestPath.EndsWith(@"/LinkClick.aspx", StringComparison.OrdinalIgnoreCase) && requestQueryString["fileticket"] != null)
            {
                int fileId = FileLinkClickController.Instance.GetFileIdFromLinkClick(requestQueryString);
				Logger.LogDebug("IsLinkClickURLFormat(requestPath={RequestPath},requestQueryString={RequestQueryString}): fileId → {FileId}", requestPath, requestQueryString, fileId);
                fileRequested = FileManager.Instance.GetFile(fileId);
				Logger.LogDebug("IsLinkClickURLFormat(requestPath={RequestPath},requestQueryString={RequestQueryString}): fileRequested → {FileRequested}", requestPath, requestQueryString, fileRequested);
                return true;
            }

            fileRequested = null;
			Logger.LogDebug("<<< Exiting IsLinkClickURLFormat(requestPath={RequestPath},requestQueryString={RequestQueryString})", requestPath, requestQueryString);
            return false;
        }
    }
}
