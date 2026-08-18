// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using System.Net;
    using System.Text;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Base class for link resource types.
    /// </summary>
    public abstract class LinkResource : ResourceBase, ILinkResource
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LinkResource));
        /// <inheritdoc />
        public bool Preload { get; set; }

        /// <inheritdoc />
        public string Media { get; set; }

        /// <summary>
        /// Renders the media attribute if set.
        /// </summary>
        /// <param name="htmlString">The HTML string builder to append to.</param>
        protected void RenderMedia(StringBuilder htmlString)
        {
			Logger.LogDebug(">>> Entering RenderMedia(htmlString={HtmlString})", htmlString);
            if (!string.IsNullOrEmpty(this.Media) && !this.Attributes.ContainsKey("media"))
            {
                htmlString.Append($" media=\"{this.Media}\"");
            }
			Logger.LogDebug("<<< Exiting RenderMedia(htmlString={HtmlString})", htmlString);
        }
    }
}
