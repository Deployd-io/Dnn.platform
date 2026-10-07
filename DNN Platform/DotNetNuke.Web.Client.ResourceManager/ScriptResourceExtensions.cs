// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Provides extension methods for <see cref="IScriptResource"/> to set various script resource properties in a fluent manner.
    /// </summary>
    public static class ScriptResourceExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ScriptResourceExtensions));
        /// <summary>
        /// Sets the priority of the resource.
        /// </summary>
        /// <param name="input">The resource to set the priority for.</param>
        /// <param name="priority">The priority value to set.</param>
        /// <returns>The resource with the priority set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetPriority<T>(this T input, FileOrder.Js priority)
            where T : IScriptResource
        {
			Logger.LogError(">>> Entering SetPriority(input={Input},priority={Priority})", input, priority);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetPriority(input={Input},priority={Priority})", input, priority);
                throw new ArgumentNullException(nameof(input));
            }

            input.Priority = (int)priority;
			Logger.LogWarning("<<< Exiting SetPriority(input={Input},priority={Priority})", input, priority);
            return input;
        }

        /// <summary>
        /// Sets the async attribute of the script resource.
        /// </summary>
        /// <param name="input">The script resource to set the async attribute for.</param>
        /// <returns>The script resource with the async attribute set.</returns>
        public static IScriptResource SetAsync(this IScriptResource input)
        {
			Logger.LogError(">>> Entering SetAsync(input={Input})", input);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetAsync(input={Input})", input);
                throw new ArgumentNullException(nameof(input));
            }

            input.Async = true;
			Logger.LogDebug("SetAsync(input={Input}): input.Async → {Arg1}", input, input.Async);
			Logger.LogWarning("<<< Exiting SetAsync(input={Input})", input);
            return input;
        }

        /// <summary>
        /// Sets the defer attribute of the script resource.
        /// </summary>
        /// <param name="input">The script resource to set the defer attribute for.</param>
        /// <returns>The script resource with the defer attribute set.</returns>
        public static IScriptResource SetDefer(this IScriptResource input)
        {
			Logger.LogError(">>> Entering SetDefer(input={Input})", input);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetDefer(input={Input})", input);
                throw new ArgumentNullException(nameof(input));
            }

            input.Defer = true;
			Logger.LogDebug("SetDefer(input={Input}): input.Defer → {Arg1}", input, input.Defer);
			Logger.LogWarning("<<< Exiting SetDefer(input={Input})", input);
            return input;
        }

        /// <summary>
        /// Sets the no module attribute of the script resource.
        /// </summary>
        /// <param name="input">The script resource to set the no module attribute for.</param>
        /// <returns>The script resource with the no module attribute set.</returns>
        public static IScriptResource SetNoModule(this IScriptResource input)
        {
			Logger.LogError(">>> Entering SetNoModule(input={Input})", input);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetNoModule(input={Input})", input);
                throw new ArgumentNullException(nameof(input));
            }

            input.NoModule = true;
			Logger.LogDebug("SetNoModule(input={Input}): input.NoModule → {Arg1}", input, input.NoModule);
			Logger.LogWarning("<<< Exiting SetNoModule(input={Input})", input);
            return input;
        }

        /// <summary>
        /// Sets the type attribute of the script resource.
        /// </summary>
        /// <param name="input">The script resource to set the type attribute for.</param>
        /// <param name="type">The type attribute to set.</param>
        /// <returns>The script resource with the type attribute set.</returns>
        public static IScriptResource SetType(this IScriptResource input, string type)
        {
			Logger.LogError(">>> Entering SetType(input={Input},type={Type})", input, type);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetType(input={Input},type={Type})", input, type);
                throw new ArgumentNullException(nameof(input));
            }

            input.Type = type;
			Logger.LogDebug("SetType(input={Input},type={Type}): input.Type → {Arg2}", input, type, input.Type);
			Logger.LogWarning("<<< Exiting SetType(input={Input},type={Type})", input, type);
            return input;
        }
    }
}
