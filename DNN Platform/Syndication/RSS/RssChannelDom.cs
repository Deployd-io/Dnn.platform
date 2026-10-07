// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;

    /// <summary>Internal representation of parsed RSS channel.</summary>
    internal sealed class RssChannelDom
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RssChannelDom));
        /// <summary>Initializes a new instance of the <see cref="RssChannelDom"/> class.</summary>
        /// <param name="channel">The channel attributes.</param>
        /// <param name="image">The image attributes.</param>
        /// <param name="items">The items.</param>
        internal RssChannelDom(Dictionary<string, string> channel, Dictionary<string, string> image, List<Dictionary<string, string>> items)
        {
			Logger.LogError(">>> Entering RssChannelDom(channel={Channel},image={Image},items={Items})", channel, image, items);
            this.Channel = channel;
			Logger.LogDebug("RssChannelDom(channel={Channel},image={Image},items={Items}): this.Channel → {Arg3}", channel, image, items, this.Channel);
            this.Image = image;
            this.Items = items;
			Logger.LogDebug("RssChannelDom(channel={Channel},image={Image},items={Items}): this.Image → {Arg3}", channel, image, items, this.Image);
        Logger.LogWarning("<<< Exiting RssChannelDom(channel={Channel},image={Image},items={Items})", channel, image, items);
            this.UtcExpiry = DateTime.MaxValue;
        }

        /// <summary>Gets the channel attributes.</summary>
        internal Dictionary<string, string> Channel { get; }

        /// <summary>Gets the image attributes.</summary>
        internal Dictionary<string, string> Image { get; }

        /// <summary>Gets the items.</summary>
        internal List<Dictionary<string, string>> Items { get; }

        /// <summary>Gets the expiration in UTC.</summary>
        internal DateTime UtcExpiry { get; private set; }

        /// <summary>Sets the expiration.</summary>
        /// <param name="utcExpiry">The expiration on UTC.</param>
        internal void SetExpiry(DateTime utcExpiry)
        {
			Logger.LogError(">>> Entering SetExpiry(utcExpiry={UtcExpiry})", utcExpiry);
            this.UtcExpiry = utcExpiry;
        }
    }
}
