// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Provides extension methods for <see cref="ILinkResource"/> to set various resource properties in a fluent manner.
    /// </summary>
    public static class LinkResourceExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LinkResourceExtensions));
        /// <summary>
        /// Marks the resource for preload.
        /// </summary>
        /// <param name="input">The resource to mark for preload.</param>
        /// <returns>The resource marked for preload.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetPreload<T>(this T input)
            where T : ILinkResource
        {
			Logger.LogError(">>> Entering SetPreload(input={Input})", input);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetPreload(input={Input})", input);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetPreload(input={Input})", input);
            input.Preload = true;
			Logger.LogDebug("SetPreload(input={Input}): input.Preload → {Arg1}", input, input.Preload);
            return input;
        }

        /// <summary>
        /// Sets the media attribute for the link resource.
        /// </summary>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        /// <param name="input">The resource to set the media attribute for.</param>
        /// <param name="media">The value of the media attribute.</param>
        /// <returns>The resource with media attribute.</returns>
        public static T SetMedia<T>(this T input, string media)
            where T : ILinkResource
        {
			Logger.LogError(">>> Entering SetMedia(input={Input},media={Media})", input, media);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetMedia(input={Input},media={Media})", input, media);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetMedia(input={Input},media={Media})", input, media);
            input.Media = media;
			Logger.LogDebug("SetMedia(input={Input},media={Media}): input.Media → {Arg2}", input, media, input.Media);
            return input;
        }
    }
}
