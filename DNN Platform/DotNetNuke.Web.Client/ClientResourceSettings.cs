// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Settings related to managing client resources.</summary>
    public partial class ClientResourceSettings
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings));
        // public keys used to identify the dictionaries stored in the application context

        /// <summary>The host settings dictionary key.</summary>
        public static readonly string HostSettingsDictionaryKey = "HostSettingsDictionary";

        /// <summary>The portal settings dictionary key.</summary>
        public static readonly string PortalSettingsDictionaryKey = "PortalSettingsDictionary";

        // public keys used to identify the various host and portal level settings

        /// <summary>The composite files key.</summary>
        [Obsolete("Composite Files (bundling) have been deprecated in DNN 10.2.0. Scheduled for removal in DNN 12.")]
        public static readonly string EnableCompositeFilesKey = "CrmEnableCompositeFiles";

        /// <summary>The minify CSS key.</summary>
        [Obsolete("Minification has been deprecated in DNN 10.2.0. Scheduled for removal in DNN 12.")]
        public static readonly string MinifyCssKey = "CrmMinifyCss";

        /// <summary>The minify JS key.</summary>
        [Obsolete("Minification has been deprecated in DNN 10.2.0. Scheduled for removal in DNN 12.")]
        public static readonly string MinifyJsKey = "CrmMinifyJs";

        /// <summary>The override default settings key.</summary>
        public static readonly string OverrideDefaultSettingsKey = "CrmOverrideDefaultSettings";

        /// <summary>The version key.</summary>
        public static readonly string VersionKey = "CrmVersion";

        private static readonly Type PortalControllerType;

        private bool statusChecked;
        private UpgradeStatus status;

        static ClientResourceSettings()
        {
			Logger.LogError(">>> Entering ClientResourceSettings()");
            try
            {
                PortalControllerType = Type.GetType("DotNetNuke.Entities.Portals.PortalController, DotNetNuke");
				Logger.LogError("ClientResourceSettings(): PortalControllerType → {PortalControllerType}", PortalControllerType);
            }
            catch (Exception exception)
            {
				Logger.LogDebug("ClientResourceSettings(): PortalControllerType → {PortalControllerType}", PortalControllerType);
                LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to get get types for reflection", exception);
            }
			Logger.LogWarning("<<< Exiting ClientResourceSettings()");
        }

        private UpgradeStatus Status
        {
            get
            {
                if (!this.statusChecked)
                {
                    this.status = GetStatusByReflection();
                    this.statusChecked = true;
                }

                return this.status;
            }
        }

        /// <inheritdoc cref="IsOverridingDefaultSettingsEnabled(int?)"/>
        [DnnDeprecated(9, 10, 3, "Use overload taking portalId")]
        public partial bool IsOverridingDefaultSettingsEnabled()
        {
			Logger.LogError(">>> Entering IsOverridingDefaultSettingsEnabled()");
            int? portalId = GetPortalIdThroughReflection();
			Logger.LogWarning("<<< Exiting IsOverridingDefaultSettingsEnabled()");
			Logger.LogWarning("<<< Exiting IsOverridingDefaultSettingsEnabled(portalId={PortalId})", portalId);
            return this.IsOverridingDefaultSettingsEnabled(portalId);
        }

        /// <summary>Gets a value indicating whether overriding the default settings is enabled.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns><see langword="true"/> if it's enabled, otherwise <see langword="false"/>.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool IsOverridingDefaultSettingsEnabled(int? portalId)
        {
            var portalVersion = GetIntegerSetting(portalId, PortalSettingsDictionaryKey, VersionKey);
            var overrideDefaultSettings = GetBooleanSetting(portalId, PortalSettingsDictionaryKey, OverrideDefaultSettingsKey);

            // if portal version is set
            // and the portal "override default settings" flag is set and set to true
            return portalVersion.HasValue && overrideDefaultSettings.HasValue && overrideDefaultSettings.Value;
        }

        /// <inheritdoc cref="GetVersion(int?)"/>
        [DnnDeprecated(9, 10, 3, "Use overload taking portalId")]
        public partial int? GetVersion()
        {
			Logger.LogError(">>> Entering GetVersion()");
            int? portalId = GetPortalIdThroughReflection();
			Logger.LogWarning("<<< Exiting GetVersion()");
			Logger.LogWarning("<<< Exiting GetVersion(portalId={PortalId})", portalId);
            return this.GetVersion(portalId);
        }

        /// <summary>Gets the version.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The version or <see langword="null"/>.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int? GetVersion(int? portalId)
        {
            var portalVersion = GetIntegerSetting(portalId, PortalSettingsDictionaryKey, VersionKey);
            var overrideDefaultSettings = GetBooleanSetting(portalId, PortalSettingsDictionaryKey, OverrideDefaultSettingsKey);

            // if portal version is set
            // and the portal "override default settings" flag is set and set to true
            if (portalVersion.HasValue && overrideDefaultSettings.HasValue && overrideDefaultSettings.Value)
            {
                return portalVersion.Value;
            }

            // otherwise return the host setting
            var hostVersion = GetIntegerSetting(portalId, HostSettingsDictionaryKey, VersionKey);
            if (hostVersion.HasValue)
            {
                return hostVersion.Value;
            }

            // otherwise tell the calling method that nothing is set
            return null;
        }

        /// <summary>Gets a value indicating whether composite files are enabled.</summary>
        /// <returns>Whether it's enabled.</returns>
        [DnnDeprecated(10, 2, 0, "Composite files (bundling) have been deprecated.")]
        public partial bool? AreCompositeFilesEnabled()
        {
			Logger.LogError(">>> Entering AreCompositeFilesEnabled()");
			Logger.LogWarning("<<< Exiting AreCompositeFilesEnabled()");
            int? portalId = GetPortalIdThroughReflection();
#pragma warning disable CS0618 // Type or member is obsolete
            return this.IsBooleanSettingEnabled(portalId, EnableCompositeFilesKey);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>Gets a value indicating whether CSS minification is enabled.</summary>
        /// <returns>Whether it's enabled.</returns>
        [DnnDeprecated(10, 2, 0, "Minification has been deprecated.")]
        public partial bool? EnableCssMinification()
        {
			Logger.LogError(">>> Entering EnableCssMinification()");
			Logger.LogWarning("<<< Exiting EnableCssMinification()");
            int? portalId = GetPortalIdThroughReflection();
#pragma warning disable CS0618 // Type or member is obsolete
            return this.IsBooleanSettingEnabled(portalId, MinifyCssKey);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>Gets a value indicating whether JS minification is enabled.</summary>
        /// <returns>Whether it's enabled.</returns>
        [DnnDeprecated(10, 2, 0, "Minification has been deprecated.")]
        public partial bool? EnableJsMinification()
        {
			Logger.LogError(">>> Entering EnableJsMinification()");
			Logger.LogWarning("<<< Exiting EnableJsMinification()");
            int? portalId = GetPortalIdThroughReflection();
#pragma warning disable CS0618 // Type or member is obsolete
            return this.IsBooleanSettingEnabled(portalId, MinifyJsKey);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static bool? GetBooleanSetting(int? portalId, string dictionaryKey, string settingKey)
        {
			Logger.LogError(">>> Entering GetBooleanSetting(portalId={PortalId},dictionaryKey={DictionaryKey},settingKey={SettingKey})", portalId, dictionaryKey, settingKey);
            var setting = GetSetting(portalId, dictionaryKey, settingKey);
            bool result;
			Logger.LogError("GetBooleanSetting(portalId={PortalId},dictionaryKey={DictionaryKey},settingKey={SettingKey}): setting → {Setting}", portalId, dictionaryKey, settingKey, setting);
            if (setting != null && bool.TryParse(setting, out result))
            {
                return result;
            }

			Logger.LogWarning("<<< Exiting GetBooleanSetting(portalId={PortalId},dictionaryKey={DictionaryKey},settingKey={SettingKey})", portalId, dictionaryKey, settingKey);
            return null;
        }

        private static int? GetIntegerSetting(int? portalId, string dictionaryKey, string settingKey)
        {
			Logger.LogError(">>> Entering GetIntegerSetting(portalId={PortalId},dictionaryKey={DictionaryKey},settingKey={SettingKey})", portalId, dictionaryKey, settingKey);
            var setting = GetSetting(portalId, dictionaryKey, settingKey);
            int version;
			Logger.LogError("GetIntegerSetting(portalId={PortalId},dictionaryKey={DictionaryKey},settingKey={SettingKey}): setting → {Setting}", portalId, dictionaryKey, settingKey, setting);
            if (setting != null && int.TryParse(setting, out version))
            {
                if (version > -1)
                {
                    return version;
                }
            }

			Logger.LogWarning("<<< Exiting GetIntegerSetting(portalId={PortalId},dictionaryKey={DictionaryKey},settingKey={SettingKey})", portalId, dictionaryKey, settingKey);
            return null;
        }

        private static string GetSetting(int? portalId, string dictionaryKey, string settingKey)
        {
			Logger.LogError(">>> Entering GetSetting(portalId={PortalId},dictionaryKey={DictionaryKey},settingKey={SettingKey})", portalId, dictionaryKey, settingKey);
            bool isHttpContext = HttpContext.Current != null && HttpContext.Current.Items.Contains(dictionaryKey);
            var settings = isHttpContext ? HttpContext.Current.Items[dictionaryKey] : null;
            if (settings == null)
            {
                if (dictionaryKey == HostSettingsDictionaryKey)
                {
                    return GetHostSettingThroughReflection(settingKey);
                }

                return GetPortalSettingThroughReflection(portalId, settingKey);
            }

            string value;
            var dictionary = (Dictionary<string, string>)settings;
			Logger.LogError("GetSetting(portalId={PortalId},dictionaryKey={DictionaryKey},settingKey={SettingKey}): dictionary → {Dictionary}", portalId, dictionaryKey, settingKey, dictionary);
            if (dictionary.TryGetValue(settingKey, out value))
            {
                return value;
            }

            // no valid setting was found
			Logger.LogWarning("<<< Exiting GetSetting(portalId={PortalId},dictionaryKey={DictionaryKey},settingKey={SettingKey})", portalId, dictionaryKey, settingKey);
            return null;
        }

        private static string GetPortalSettingThroughReflection(int? portalId, string settingKey)
        {
			Logger.LogError(">>> Entering GetPortalSettingThroughReflection(portalId={PortalId},settingKey={SettingKey})", portalId, settingKey);
            if (portalId is null)
            {
                return null;
            }

            try
            {
                using var scope = DependencyInjection.GetOrCreateServiceScope();
				Logger.LogError("GetPortalSettingThroughReflection(portalId={PortalId},settingKey={SettingKey}): scope → {Scope}", portalId, settingKey, scope);
                var portalController = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, PortalControllerType);
				Logger.LogError("GetPortalSettingThroughReflection(portalId={PortalId},settingKey={SettingKey}): portalController → {PortalController}", portalId, settingKey, portalController);
                var method = PortalControllerType.GetMethod("GetPortalSettings", BindingFlags.Public | BindingFlags.Instance, null, [typeof(int),], null);
				Logger.LogError("GetPortalSettingThroughReflection(portalId={PortalId},settingKey={SettingKey}): method → {Method}", portalId, settingKey, method);
                var dictionary = (Dictionary<string, string>)method.Invoke(portalController, [portalId.Value,]);
				Logger.LogError("GetPortalSettingThroughReflection(portalId={PortalId},settingKey={SettingKey}): dictionary → {Dictionary}", portalId, settingKey, dictionary);

                if (dictionary.TryGetValue(settingKey, out var value))
                {
                    return value;
                }
            }
            catch (Exception exception)
            {
                LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to Get Portal Setting Through Reflection", exception);
            }

			Logger.LogWarning("<<< Exiting GetPortalSettingThroughReflection(portalId={PortalId},settingKey={SettingKey})", portalId, settingKey);
            return null;
        }

        private static int? GetPortalIdThroughReflection()
        {
			Logger.LogError(">>> Entering GetPortalIdThroughReflection()");
            try
            {
                if (HttpContext.Current == null)
                {
                    return null;
                }

                using var scope = DependencyInjection.GetOrCreateServiceScope();
				Logger.LogError("GetPortalIdThroughReflection(): scope → {Scope}", scope);
                var portalAliasService = scope.ServiceProvider.GetRequiredService<IPortalAliasService>();
				Logger.LogError("GetPortalIdThroughReflection(): portalAliasService → {PortalAliasService}", portalAliasService);
                var alias = portalAliasService.GetPortalAlias(HttpContext.Current.Request.Url.Host);
				Logger.LogError("GetPortalIdThroughReflection(): alias → {Alias}", alias);

                return alias.PortalId;
            }
            catch (Exception exception)
            {
                LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to Get Portal ID Through Reflection", exception);
            }

			Logger.LogWarning("<<< Exiting GetPortalIdThroughReflection()");
            return null;
        }

        private static string GetHostSettingThroughReflection(string settingKey)
        {
			Logger.LogError(">>> Entering GetHostSettingThroughReflection(settingKey={SettingKey})", settingKey);
            try
            {
                using var scope = DependencyInjection.GetOrCreateServiceScope();
				Logger.LogError("GetHostSettingThroughReflection(settingKey={SettingKey}): scope → {Scope}", settingKey, scope);
                var hostSettingsService = scope.ServiceProvider.GetRequiredService<IHostSettingsService>();
				Logger.LogError("GetHostSettingThroughReflection(settingKey={SettingKey}): hostSettingsService → {HostSettingsService}", settingKey, hostSettingsService);

                var dictionary = hostSettingsService.GetSettingsDictionary();
				Logger.LogError("GetHostSettingThroughReflection(settingKey={SettingKey}): dictionary → {Dictionary}", settingKey, dictionary);
                if (dictionary.TryGetValue(settingKey, out var value))
                {
                    return value;
                }
            }
            catch (Exception exception)
            {
                LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to Get Host Setting Through Reflection", exception);
            }

			Logger.LogWarning("<<< Exiting GetHostSettingThroughReflection(settingKey={SettingKey})", settingKey);
            return null;
        }

        private static UpgradeStatus GetStatusByReflection()
        {
			Logger.LogError(">>> Entering GetStatusByReflection()");
            try
            {
                using var scope = DependencyInjection.GetOrCreateServiceScope();
				Logger.LogError("GetStatusByReflection(): scope → {Scope}", scope);
                var applicationStatusInfo = scope.ServiceProvider.GetRequiredService<IApplicationStatusInfo>();
				Logger.LogError("GetStatusByReflection(): applicationStatusInfo → {ApplicationStatusInfo}", applicationStatusInfo);
                return applicationStatusInfo.Status;
            }
            catch (Exception exception)
            {
                LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to Get Status By Reflection", exception);
				Logger.LogWarning("<<< Exiting GetStatusByReflection()");
                return UpgradeStatus.Unknown;
            }
        }

        private bool? IsBooleanSettingEnabled(int? portalId, string settingKey)
        {
			Logger.LogError(">>> Entering IsBooleanSettingEnabled(portalId={PortalId},settingKey={SettingKey})", portalId, settingKey);
            if (this.Status != UpgradeStatus.None)
            {
                return false;
            }

            var portalEnabled = GetBooleanSetting(portalId, PortalSettingsDictionaryKey, settingKey);
			Logger.LogError("IsBooleanSettingEnabled(portalId={PortalId},settingKey={SettingKey}): portalEnabled → {PortalEnabled}", portalId, settingKey, portalEnabled);
            var overrideDefaultSettings = GetBooleanSetting(portalId, PortalSettingsDictionaryKey, OverrideDefaultSettingsKey);
			Logger.LogError("IsBooleanSettingEnabled(portalId={PortalId},settingKey={SettingKey}): overrideDefaultSettings → {OverrideDefaultSettings}", portalId, settingKey, overrideDefaultSettings);

            // if portal version is set
            // and the portal "override default settings" flag is set and set to true
            if (portalEnabled.HasValue && overrideDefaultSettings.HasValue && overrideDefaultSettings.Value)
            {
                return portalEnabled.Value;
            }

            // otherwise return the host setting
            var hostEnabled = GetBooleanSetting(portalId, HostSettingsDictionaryKey, settingKey);
			Logger.LogError("IsBooleanSettingEnabled(portalId={PortalId},settingKey={SettingKey}): hostEnabled → {HostEnabled}", portalId, settingKey, hostEnabled);
            if (hostEnabled.HasValue)
            {
                return hostEnabled.Value;
            }

            // otherwise tell the calling method that nothing is set
			Logger.LogWarning("<<< Exiting IsBooleanSettingEnabled(portalId={PortalId},settingKey={SettingKey})", portalId, settingKey);
            return null;
        }
    }
}
