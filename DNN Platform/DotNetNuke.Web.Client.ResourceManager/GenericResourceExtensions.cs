// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>Provides extension methods for <see cref="IResource"/> to set various resource properties in a fluent manner.</summary>
    public static class GenericResourceExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GenericResourceExtensions));
        /// <summary>Sets the priority of the resource.</summary>
        /// <param name="input">The resource to set the priority for.</param>
        /// <param name="priority">The priority value to set.</param>
        /// <returns>The resource with the priority set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetPriority<T>(this T input, int priority)
            where T : IResource
        {
			Logger.LogError(">>> Entering SetPriority(input={Input},priority={Priority})", input, priority);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetPriority(input={Input},priority={Priority})", input, priority);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetPriority(input={Input},priority={Priority})", input, priority);
            input.Priority = priority;
			Logger.LogDebug("SetPriority(input={Input},priority={Priority}): input.Priority → {Arg2}", input, priority, input.Priority);
            return input;
        }

        /// <summary>Sets the name, version, and force version flag of the resource.</summary>
        /// <param name="input">The resource to set the name and version for.</param>
        /// <param name="name">The name to set.</param>
        /// <param name="version">The version to set.</param>
        /// <param name="forceVersion">Whether to force the version.</param>
        /// <returns>The resource with the name and version set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetNameAndVersion<T>(this T input, string name, string version, bool forceVersion)
            where T : IResource
        {
			Logger.LogError(">>> Entering SetNameAndVersion(input={Input},name={Name},version={Version},forceVersion={ForceVersion})", input, name, version, forceVersion);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetNameAndVersion(input={Input},name={Name},version={Version},forceVersion={ForceVersion})", input, name, version, forceVersion);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetNameAndVersion(input={Input},name={Name},version={Version},forceVersion={ForceVersion})", input, name, version, forceVersion);
			Logger.LogDebug("SetNameAndVersion(input={Input},name={Name},version={Version},forceVersion={ForceVersion}): input.Version → {Arg4}", input, name, version, forceVersion, input.Version);
			Logger.LogDebug("SetNameAndVersion(input={Input},name={Name},version={Version},forceVersion={ForceVersion}): input.ForceVersion → {Arg4}", input, name, version, forceVersion, input.ForceVersion);
            input.Name = name;
			Logger.LogDebug("SetNameAndVersion(input={Input},name={Name},version={Version},forceVersion={ForceVersion}): input.Name → {Arg4}", input, name, version, forceVersion, input.Name);
            input.Version = version;
            input.ForceVersion = forceVersion;
            return input;
        }

        /// <summary>Sets the CDN URL of the resource.</summary>
        /// <param name="input">The resource to set the CDN URL for.</param>
        /// <param name="cdnUrl">The CDN URL to set.</param>
        /// <returns>The resource with the CDN URL set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetCdnUrl<T>(this T input, string cdnUrl)
            where T : IResource
        {
			Logger.LogError(">>> Entering SetCdnUrl(input={Input},cdnUrl={CdnUrl})", input, cdnUrl);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetCdnUrl(input={Input},cdnUrl={CdnUrl})", input, cdnUrl);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetCdnUrl(input={Input},cdnUrl={CdnUrl})", input, cdnUrl);
            input.CdnUrl = cdnUrl;
			Logger.LogDebug("SetCdnUrl(input={Input},cdnUrl={CdnUrl}): input.CdnUrl → {Arg2}", input, cdnUrl, input.CdnUrl);
            return input;
        }

        /// <summary>Sets the provider of the resource.</summary>
        /// <param name="input">The resource to set the provider for.</param>
        /// <param name="provider">The provider to set.</param>
        /// <returns>The resource with the provider set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetProvider<T>(this T input, string provider)
            where T : IResource
        {
			Logger.LogError(">>> Entering SetProvider(input={Input},provider={Provider})", input, provider);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetProvider(input={Input},provider={Provider})", input, provider);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetProvider(input={Input},provider={Provider})", input, provider);
            input.Provider = provider;
			Logger.LogDebug("SetProvider(input={Input},provider={Provider}): input.Provider → {Arg2}", input, provider, input.Provider);
            return input;
        }

        /// <summary>Marks the resource as blocking.</summary>
        /// <param name="input">The resource to mark as blocking.</param>
        /// <returns>The resource marked as blocking.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetBlocking<T>(this T input)
            where T : IResource
        {
			Logger.LogError(">>> Entering SetBlocking(input={Input})", input);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetBlocking(input={Input})", input);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetBlocking(input={Input})", input);
            input.Blocking = true;
			Logger.LogDebug("SetBlocking(input={Input}): input.Blocking → {Arg1}", input, input.Blocking);
            return input;
        }

        /// <summary>Sets the integrity hash of the resource.</summary>
        /// <param name="input">The resource to set the integrity hash for.</param>
        /// <param name="hash">The integrity hash to set.</param>
        /// <returns>The resource with the integrity hash set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetIntegrity<T>(this T input, string hash)
            where T : IResource
        {
			Logger.LogError(">>> Entering SetIntegrity(input={Input},hash=<redacted>)", input);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetIntegrity(input={Input},hash=<redacted>)", input);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetIntegrity(input={Input},hash=<redacted>)", input);
            input.Integrity = hash;
			Logger.LogDebug("SetIntegrity(input={Input},hash=<redacted>): input.Integrity → {Arg2}", input, input.Integrity);
            return input;
        }

        /// <summary>Sets the Cross-Origin attribute of the resource.</summary>
        /// <param name="input">The resource to set the Cross-Origin attribute for.</param>
        /// <param name="crossOrigin">The Cross-Origin value to set.</param>
        /// <returns>The resource with the Cross-Origin attribute set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetCrossOrigin<T>(this T input, CrossOrigin crossOrigin)
            where T : IResource
        {
			Logger.LogError(">>> Entering SetCrossOrigin(input={Input},crossOrigin={CrossOrigin})", input, crossOrigin);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetCrossOrigin(input={Input},crossOrigin={CrossOrigin})", input, crossOrigin);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetCrossOrigin(input={Input},crossOrigin={CrossOrigin})", input, crossOrigin);
            input.CrossOrigin = crossOrigin;
			Logger.LogDebug("SetCrossOrigin(input={Input},crossOrigin={CrossOrigin}): input.CrossOrigin → {Arg2}", input, crossOrigin, input.CrossOrigin);
            return input;
        }

        /// <summary>Sets the fetch priority of the resource.</summary>
        /// <param name="input">The resource to set the fetch priority for.</param>
        /// <param name="fetchPriority">The fetch priority to set.</param>
        /// <returns>The resource with the fetch priority set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetFetchPriority<T>(this T input, FetchPriority fetchPriority)
            where T : IResource
        {
			Logger.LogError(">>> Entering SetFetchPriority(input={Input},fetchPriority={FetchPriority})", input, fetchPriority);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetFetchPriority(input={Input},fetchPriority={FetchPriority})", input, fetchPriority);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetFetchPriority(input={Input},fetchPriority={FetchPriority})", input, fetchPriority);
            input.FetchPriority = fetchPriority;
			Logger.LogDebug("SetFetchPriority(input={Input},fetchPriority={FetchPriority}): input.FetchPriority → {Arg2}", input, fetchPriority, input.FetchPriority);
            return input;
        }

        /// <summary>Sets the referrer policy of the resource.</summary>
        /// <param name="input">The resource to set the referrer policy for.</param>
        /// <param name="referrerPolicy">The referrer policy to set.</param>
        /// <returns>The resource with the referrer policy set.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T SetReferrerPolicy<T>(this T input, ReferrerPolicy referrerPolicy)
            where T : IResource
        {
			Logger.LogError(">>> Entering SetReferrerPolicy(input={Input},referrerPolicy={ReferrerPolicy})", input, referrerPolicy);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetReferrerPolicy(input={Input},referrerPolicy={ReferrerPolicy})", input, referrerPolicy);
                throw new ArgumentNullException(nameof(input));
            }

			Logger.LogWarning("<<< Exiting SetReferrerPolicy(input={Input},referrerPolicy={ReferrerPolicy})", input, referrerPolicy);
            input.ReferrerPolicy = referrerPolicy;
			Logger.LogDebug("SetReferrerPolicy(input={Input},referrerPolicy={ReferrerPolicy}): input.ReferrerPolicy → {Arg2}", input, referrerPolicy, input.ReferrerPolicy);
            return input;
        }

        /// <summary>Adds or updates an attribute of the resource.</summary>
        /// <param name="input">The resource to add or update the attribute for.</param>
        /// <param name="attributeName">The name of the attribute to add or update.</param>
        /// <param name="attributeValue">The value of the attribute to add or update.</param>
        /// <returns>The resource with the attribute added or updated.</returns>
        /// <typeparam name="T">The type of resource, which must implement <see cref="IResource"/>.</typeparam>
        public static T AddAttribute<T>(this T input, string attributeName, string attributeValue)
            where T : IResource
        {
			Logger.LogError(">>> Entering AddAttribute(input={Input},attributeName={AttributeName},attributeValue={AttributeValue})", input, attributeName, attributeValue);
            if (input == null)
            {
				Logger.LogError("Throwing ArgumentNullException from AddAttribute(input={Input},attributeName={AttributeName},attributeValue={AttributeValue})", input, attributeName, attributeValue);
                throw new ArgumentNullException(nameof(input));
            }

            if (!string.IsNullOrEmpty(attributeName))
            {
                input.Attributes[attributeName.ToLowerInvariant()] = attributeValue;
            }

			Logger.LogWarning("<<< Exiting AddAttribute(input={Input},attributeName={AttributeName},attributeValue={AttributeValue})", input, attributeName, attributeValue);
            return input;
        }
    }
}
