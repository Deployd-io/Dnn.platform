// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System.Collections.Specialized;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Tabs;

    public abstract class FriendlyUrlProviderBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FriendlyUrlProviderBase));
        /// <summary>Initializes a new instance of the <see cref="FriendlyUrlProviderBase"/> class.</summary>
        /// <param name="attributes">The provider attributes.</param>
        internal FriendlyUrlProviderBase(NameValueCollection attributes)
        {
			Logger.LogDebug(">>> Entering FriendlyUrlProviderBase(attributes={Attributes})", attributes);
            if (!string.IsNullOrEmpty(attributes["urlFormat"]))
            {
                switch (attributes["urlFormat"].ToLowerInvariant())
                {
                    case "searchfriendly":
                        this.UrlFormat = UrlFormatType.SearchFriendly;
						Logger.LogDebug("FriendlyUrlProviderBase(attributes={Attributes}): this.UrlFormat → {Arg1}", attributes, this.UrlFormat);
                        break;
                    case "humanfriendly":
                        this.UrlFormat = UrlFormatType.HumanFriendly;
                        break;
                    case "advanced":
                    case "customonly":
                        this.UrlFormat = UrlFormatType.Advanced;
                        break;
                    default:
                        this.UrlFormat = UrlFormatType.SearchFriendly;
                        break;
                }
            }
			Logger.LogDebug("<<< Exiting FriendlyUrlProviderBase(attributes={Attributes})", attributes);
        }

        protected UrlFormatType UrlFormat { get; private set; }

        internal abstract string FriendlyUrl(TabInfo tab, string path);

        internal abstract string FriendlyUrl(TabInfo tab, string path, string pageName);

        internal abstract string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings portalSettings);

        internal abstract string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias);
    }
}
