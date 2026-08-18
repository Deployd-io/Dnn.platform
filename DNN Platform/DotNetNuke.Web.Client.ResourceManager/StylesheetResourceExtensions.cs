// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Provides extension methods for <see cref="IStylesheetResource"/> to set various stylesheet resource properties in a fluent manner.
    /// </summary>
    public static class StylesheetResourceExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StylesheetResourceExtensions));
        /// <summary>
        /// Sets the priority of the resource.
        /// </summary>
        /// <param name="input">The resource to set the priority for.</param>
        /// <param name="priority">The priority value to set.</param>
        /// <returns>The resource with the priority set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetPriority<T>(this T input, FileOrder.Css priority)
            where T : IStylesheetResource
        {
			Logger.LogDebug(">>> Entering SetPriority(input={Input},priority={Priority})", input, priority);
            if (input == null)
            {
				Logger.LogWarning("Throwing ArgumentNullException from SetPriority(input={Input},priority={Priority})", input, priority);
                throw new ArgumentNullException(nameof(input));
            }

            input.Priority = (int)priority;
			Logger.LogDebug("<<< Exiting SetPriority(input={Input},priority={Priority})", input, priority);
            return input;
        }

        /// <summary>
        /// Sets the disabled attribute of the stylesheet resource.
        /// </summary>
        /// <param name="input">The stylesheet resource to set the disabled attribute for.</param>
        /// <returns>The stylesheet resource with the disabled attribute set.</returns>
        public static IStylesheetResource SetDisabled(this IStylesheetResource input)
        {
			Logger.LogDebug(">>> Entering SetDisabled(input={Input})", input);
            if (input == null)
            {
				Logger.LogWarning("Throwing ArgumentNullException from SetDisabled(input={Input})", input);
                throw new ArgumentNullException(nameof(input));
            }

            input.Disabled = true;
			Logger.LogDebug("SetDisabled(input={Input}): input.Disabled → {Arg1}", input, input.Disabled);
			Logger.LogDebug("<<< Exiting SetDisabled(input={Input})", input);
            return input;
        }
    }
}
