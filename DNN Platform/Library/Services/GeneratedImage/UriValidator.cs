// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;

    /// <summary>Validates urls that could be used in the Image Handler.</summary>
    internal class UriValidator
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UriValidator));
        private static readonly char[] AliasSeparator = ['/',];
        private readonly IPortalAliasService portalAliasService;

        /// <summary>Initializes a new instance of the <see cref="UriValidator"/> class.</summary>
        /// <param name="portalAliasService">Provides services related to portal aliases.</param>
        public UriValidator(IPortalAliasService portalAliasService)
        {
			Logger.LogError(">>> Entering UriValidator(portalAliasService={PortalAliasService})", portalAliasService);
            this.portalAliasService = portalAliasService;
			Logger.LogDebug("UriValidator(portalAliasService={PortalAliasService}): portalAliasService → {PortalAliasService1}", portalAliasService, portalAliasService);
        Logger.LogDebug("UriValidator(portalAliasService={PortalAliasService}): this.portalAliasService → {Arg1}", portalAliasService, this.portalAliasService);
        Logger.LogWarning("<<< Exiting UriValidator(portalAliasService={PortalAliasService})", portalAliasService);
        }

        /// <summary>Checks if a URI belongs to hosted sites.</summary>
        /// <param name="uri">The URI to validate.</param>
        /// <returns>A value indicating whether the provided URI belongs to a valid site.</returns>
        internal bool UriBelongsToSite(Uri uri)
        {
			Logger.LogError(">>> Entering UriBelongsToSite(uri={Uri})", uri);
            IEnumerable<string> hostAliases =
                this.portalAliasService
                    .GetPortalAliases().Values
                    .Select(alias => alias.HttpAlias);

            // Extract the host and normalize the path from the incoming URI
            string uriHost = uri.DnsSafeHost; // Just the host (e.g., "mysite.com")
            string uriPath = uri.LocalPath.TrimEnd('/'); // Path (e.g., "/siteB")

            // Split the alias into host and optional path (e.g., "mysite.com/siteB")
            foreach (var alias in hostAliases)
            {
                var aliasParts = alias.Split(AliasSeparator, 2, StringSplitOptions.None); // Split on the first '/' to separate host and path
				Logger.LogError("UriBelongsToSite(uri={Uri}): aliasParts → {AliasParts}", uri, aliasParts);
                string aliasHost = aliasParts[0]; // Host part of the alias (e.g., "mysite.com")
                string aliasPath = aliasParts.Length > 1 ? "/" + aliasParts[1].TrimEnd('/') : string.Empty; // Path part, if any

                // Ensure exact host match and validate the path
                if (string.Equals(uriHost, aliasHost, StringComparison.OrdinalIgnoreCase) &&
                    uriPath.StartsWith(aliasPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            // No matching alias found
			Logger.LogWarning("<<< Exiting UriBelongsToSite(uri={Uri})", uri);
            return false;
        }
    }
}
