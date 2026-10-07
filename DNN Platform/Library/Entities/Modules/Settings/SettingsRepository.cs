// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Web.Caching;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    public abstract class SettingsRepository<T> : ISettingsRepository<T>
        where T : class, new()
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SettingsRepository));
        private readonly IModuleController moduleController;
        private readonly IHostSettings hostSettings;
        private readonly IHostSettingsService hostSettingsService;
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="SettingsRepository{T}"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        protected SettingsRepository()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SettingsRepository{T}"/> class.</summary>
        /// <param name="moduleController">The module controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        protected SettingsRepository(IModuleController moduleController, IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController)
        {
			Logger.LogError(">>> Entering SettingsRepository(moduleController={ModuleController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController})", moduleController, hostSettings, hostSettingsService, portalController);
			Logger.LogDebug("SettingsRepository(moduleController={ModuleController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController}): moduleController → {ModuleController1}", moduleController, hostSettings, hostSettingsService, portalController, moduleController);
			Logger.LogDebug("SettingsRepository(moduleController={ModuleController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController}): hostSettings → {HostSettings1}", moduleController, hostSettings, hostSettingsService, portalController, hostSettings);
			Logger.LogDebug("SettingsRepository(moduleController={ModuleController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController}): hostSettingsService → {HostSettingsService1}", moduleController, hostSettings, hostSettingsService, portalController, hostSettingsService);
            this.Mapping = this.LoadMapping();
			Logger.LogWarning("<<< Exiting SettingsRepository(moduleController={ModuleController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController})", moduleController, hostSettings, hostSettingsService, portalController);
            this.moduleController = moduleController ?? ModuleController.Instance;
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("SettingsRepository(moduleController={ModuleController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController}): this.moduleController → {Arg4}", moduleController, hostSettings, hostSettingsService, portalController, this.moduleController);
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        }

        /// <summary>Gets cache key for this class. Used for parameter mapping storage as well as entire class persistence.</summary>
        protected virtual string MappingCacheKey => "SettingsRepository_" + typeof(T).FullName.Replace(".", "_");

        private static ISerializationManager SerializationManager => Globals.GetCurrentServiceProvider().GetRequiredService<ISerializationManager>();

        private IList<ParameterMapping> Mapping { get; }

        /// <inheritdoc />
        public T GetSettings(ModuleInfo moduleContext)
        {
			Logger.LogError(">>> Entering GetSettings(moduleContext={ModuleContext})", moduleContext);
            Logger.LogWarning("<<< Exiting GetSettings(moduleContext={ModuleContext})", moduleContext);
            return CBO.GetCachedObject<T>(
                this.hostSettings,
                new CacheItemArgs(this.CacheKey(moduleContext.PortalID, moduleContext.TabModuleID), 20, CacheItemPriority.AboveNormal, moduleContext),
                this.Load,
                false);
        }

        /// <inheritdoc />
        public T GetSettings(int portalId)
        {
            return CBO.GetCachedObject<T>(
                this.hostSettings,
                new CacheItemArgs(this.CacheKey(portalId, -1), 20, CacheItemPriority.AboveNormal, null, portalId),
                this.Load,
                false);
        }

        /// <inheritdoc />
        public void SaveSettings(ModuleInfo moduleContext, T settings)
        {
			Logger.LogError(">>> Entering SaveSettings(moduleContext={ModuleContext},settings={Settings})", moduleContext, settings);
            Requires.NotNull("settings", settings);
            Requires.NotNull("ctlModule", moduleContext);
            this.SaveSettings(moduleContext.PortalID, moduleContext, settings);
			Logger.LogWarning("<<< Exiting SaveSettings(moduleContext={ModuleContext},settings={Settings})", moduleContext, settings);
        }

        /// <inheritdoc />
        public void SaveSettings(int portalId, T settings)
        {
			Logger.LogError(">>> Entering SaveSettings(portalId={PortalId},settings={Settings})", portalId, settings);
            Requires.NotNull("settings", settings);
            this.SaveSettings(portalId, null, settings);
			Logger.LogWarning("<<< Exiting SaveSettings(portalId={PortalId},settings={Settings})", portalId, settings);
        }

        /// <summary>Retrieves the parameter mapping from cache if still there, otherwise recreates it.</summary>
        /// <returns>List of parameters.</returns>
        protected IList<ParameterMapping> LoadMapping()
        {
			Logger.LogError(">>> Entering LoadMapping()");
            var cacheKey = this.MappingCacheKey;
            var mapping = CachingProvider.Instance().GetItem(cacheKey) as IList<ParameterMapping>;
			Logger.LogError("LoadMapping(): mapping → {Mapping}", mapping);
            if (mapping == null)
            {
                mapping = this.CreateMapping();

                // HARDCODED: 2 hour expiration.
                // Note that "caching" can also be accomplished with a static dictionary since the Attribute/Property mapping does not change unless the module is updated.
                CachingProvider.Instance().Insert(cacheKey, mapping, (DNNCacheDependency)null, DateTime.Now.AddHours(2), Cache.NoSlidingExpiration);
            }

			Logger.LogWarning("<<< Exiting LoadMapping()");
            return mapping;
        }

        /// <summary>Rebuilds parameter mapping of the class.</summary>
        /// <returns>List of parameters.</returns>
        protected virtual IList<ParameterMapping> CreateMapping()
        {
			Logger.LogError(">>> Entering CreateMapping()");
            var mapping = new List<ParameterMapping>();
            var type = typeof(T);
			Logger.LogError("CreateMapping(): type → {Type}", type);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
			Logger.LogError("CreateMapping(): properties → {Properties}", properties);
			Logger.LogError("CreateMapping(): mapping → {Mapping}", mapping);

            properties.ForEach(property =>
            {
                var attributes = property.GetCustomAttributes<ParameterAttributeBase>(true);
				Logger.LogError("CreateMapping(): attributes → {Attributes}", attributes);
                attributes.ForEach(attribute => mapping.Add(new ParameterMapping(attribute, property)));
            });

			Logger.LogWarning("<<< Exiting CreateMapping()");
            return mapping;
        }

        /// <summary>Deserializes the property.</summary>
        /// <param name="settings">The settings.</param>
        /// <param name="property">The property.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="InvalidCastException">Thrown if string value cannot be deserialized to desired type.</exception>
        private static void DeserializeProperty(T settings, PropertyInfo property, ParameterAttributeBase attribute, string propertyValue)
        {
			Logger.LogError(">>> Entering DeserializeProperty(settings={Settings},property={Property},attribute={Attribute},propertyValue={PropertyValue})", settings, property, attribute, propertyValue);
            SerializationManager.DeserializeProperty(settings, property, propertyValue, attribute.Serializer);
			Logger.LogWarning("<<< Exiting DeserializeProperty(settings={Settings},property={Property},attribute={Attribute},propertyValue={PropertyValue})", settings, property, attribute, propertyValue);
        }

        private static string GetAlgorithmNameSettingKey(ParameterMapping mapping)
        {
			Logger.LogError(">>> Entering GetAlgorithmNameSettingKey(mapping=<redacted>)");
            var settingKey = mapping.FullParameterName;
			Logger.LogWarning("<<< Exiting GetAlgorithmNameSettingKey(mapping=<redacted>)");
            return CryptographyUtils.GetAlgorithmNameSettingKey(settingKey);
        }

        private void SaveSettings(int portalId, ModuleInfo moduleContext, T settings)
        {
			Logger.LogError(">>> Entering SaveSettings(portalId={PortalId},moduleContext={ModuleContext},settings={Settings})", portalId, moduleContext, settings);
            this.Mapping.ForEach(mapping =>
            {
                var attribute = mapping.Attribute;
                var property = mapping.Property;
				Logger.LogError("SaveSettings(portalId={PortalId},moduleContext={ModuleContext},settings={Settings}): property → {Property}", portalId, moduleContext, settings, property);

                // Should be, because we asked for properties with a Get accessor.
                if (property.CanRead)
                {
                    var settingValueAsString = SerializationManager.SerializeProperty(settings, property, attribute.Serializer);
					Logger.LogError("SaveSettings(portalId={PortalId},moduleContext={ModuleContext},settings={Settings}): settingValueAsString → {SettingValueAsString}", portalId, moduleContext, settings, settingValueAsString);

                    if (attribute is ModuleSettingAttribute msa && moduleContext != null)
                    {
                        if (msa.IsSecure)
                        {
                            var hashAlgorithmName = HashAlgorithmName.SHA512;
                            settingValueAsString = Security.FIPSCompliant.EncryptAES(hashAlgorithmName, settingValueAsString, Config.GetDecryptionkey(), this.hostSettings.Guid);
                            this.moduleController.UpdateModuleSetting(moduleContext.ModuleID, GetAlgorithmNameSettingKey(mapping), hashAlgorithmName.Name);
                            moduleContext.ModuleSettings[GetAlgorithmNameSettingKey(mapping)] = hashAlgorithmName.Name;
                        }

                        this.moduleController.UpdateModuleSetting(moduleContext.ModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.ModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is TabModuleSettingAttribute tmsa && moduleContext != null)
                    {
                        if (tmsa.IsSecure)
                        {
                            var hashAlgorithmName = HashAlgorithmName.SHA512;
                            settingValueAsString = Security.FIPSCompliant.EncryptAES(hashAlgorithmName, settingValueAsString, Config.GetDecryptionkey(), this.hostSettings.Guid);
                            this.moduleController.UpdateTabModuleSetting(moduleContext.TabModuleID, GetAlgorithmNameSettingKey(mapping), hashAlgorithmName.Name);
                            moduleContext.TabModuleSettings[GetAlgorithmNameSettingKey(mapping)] = hashAlgorithmName.Name;
                        }

                        this.moduleController.UpdateTabModuleSetting(moduleContext.TabModuleID, mapping.FullParameterName, settingValueAsString);
                        moduleContext.TabModuleSettings[mapping.FullParameterName] = settingValueAsString; // temporary fix for issue 3692
                    }
                    else if (attribute is PortalSettingAttribute psa && portalId != -1)
                    {
                        this.portalController.UpdatePortalSetting(portalId, mapping.FullParameterName, settingValueAsString, true, Null.NullString, psa.IsSecure);
                    }
                    else if (attribute is HostSettingAttribute hsa)
                    {
                        if (hsa.IsSecure)
                        {
                            var hashAlgorithmName = HashAlgorithmName.SHA512;
                            settingValueAsString = Security.FIPSCompliant.EncryptAES(hashAlgorithmName, settingValueAsString, Config.GetDecryptionkey(), this.hostSettings.Guid);
                            this.hostSettingsService.Update(GetAlgorithmNameSettingKey(mapping), hashAlgorithmName.Name);
                        }

                        this.hostSettingsService.Update(mapping.FullParameterName, settingValueAsString);
                    }
                }
            });

            DataCache.ClearCache(this.CacheKeyPortalPrefix(portalId));
            DataCache.SetCache(this.CacheKey(portalId, moduleContext?.TabModuleID ?? -1), settings);
			Logger.LogWarning("<<< Exiting SaveSettings(portalId={PortalId},moduleContext={ModuleContext},settings={Settings})", portalId, moduleContext, settings);
        }

        private T Load(CacheItemArgs args)
        {
			Logger.LogError(">>> Entering Load(args={Args})", args);
            var ctlModule = (ModuleInfo)args.ParamList[0];
            var portalId = ctlModule?.PortalID ?? (int)args.ParamList[1];
			Logger.LogError("Load(args={Args}): portalId → {PortalId}", args, portalId);
            var settings = new T();
			Logger.LogError("Load(args={Args}): settings → {Settings}", args, settings);
            var hostSettingsDictionary = this.hostSettingsService.GetSettings();
			Logger.LogError("Load(args={Args}): hostSettingsDictionary → {HostSettingsDictionary}", args, hostSettingsDictionary);
            var portalSettingsDictionary = this.portalController.GetPortalSettings(portalId);
			Logger.LogError("Load(args={Args}): portalSettingsDictionary → {PortalSettingsDictionary}", args, portalSettingsDictionary);
			Logger.LogError("Load(args={Args}): ctlModule → {CtlModule}", args, ctlModule);

            this.Mapping.ForEach(mapping =>
            {
                string settingValue = null;
				Logger.LogError("Load(args={Args}): settingValue → {SettingValue}", args, settingValue);
                string algorithmName = null;
				Logger.LogError("Load(args={Args}): algorithmName → {AlgorithmName}", args, algorithmName);

                var attribute = mapping.Attribute;
                var property = mapping.Property;
				Logger.LogError("Load(args={Args}): property → {Property}", args, property);

                // TODO: Make more extensible, enable other attributes to be defined
                if (attribute is HostSettingAttribute && hostSettingsDictionary.TryGetValue(mapping.FullParameterName, out var hostSetting))
                {
                    settingValue = hostSetting.Value;
                    if (attribute.IsSecure && hostSettingsDictionary.TryGetValue(GetAlgorithmNameSettingKey(mapping), out var algorithmNameSetting))
                    {
                        algorithmName = algorithmNameSetting.Value;
                    }
                }
                else if (attribute is PortalSettingAttribute && portalId != -1 && portalSettingsDictionary.TryGetValue(mapping.FullParameterName, out settingValue))
                {
                    if (attribute.IsSecure && !portalSettingsDictionary.TryGetValue(GetAlgorithmNameSettingKey(mapping), out algorithmName))
                    {
                        algorithmName = null;
                    }
                }
                else if (attribute is TabModuleSettingAttribute && ctlModule?.TabModuleSettings.ContainsKey(mapping.FullParameterName) == true)
                {
                    settingValue = (string)ctlModule.TabModuleSettings[mapping.FullParameterName];
                    if (attribute.IsSecure)
                    {
                        algorithmName = ctlModule.TabModuleSettings[GetAlgorithmNameSettingKey(mapping)] as string;
                    }
                }
                else if (attribute is ModuleSettingAttribute && ctlModule?.ModuleSettings.ContainsKey(mapping.FullParameterName) == true)
                {
                    settingValue = (string)ctlModule.ModuleSettings[mapping.FullParameterName];
                    if (attribute.IsSecure)
                    {
                        algorithmName = ctlModule.ModuleSettings[GetAlgorithmNameSettingKey(mapping)] as string;
                    }
                }

                if (attribute.IsSecure)
                {
                    var algorithm = string.IsNullOrWhiteSpace(algorithmName) ? HashAlgorithmName.SHA1 : new HashAlgorithmName(algorithmName);
					Logger.LogError("Load(args={Args}): algorithm → {Algorithm}", args, algorithm);
                    try
                    {
                        settingValue = Security.FIPSCompliant.DecryptAES(algorithm, settingValue, Config.GetDecryptionkey(), this.hostSettings.Guid);
                    }
                    catch (Exception ex)
                    {
						Logger.LogError("Exception in Load(args={Args}): {Arg1}", args, ex.Message);
                        Exceptions.LogException(new ModuleLoadException(string.Format(CultureInfo.CurrentCulture, Localization.GetString("ErrorDecryptingSetting", Localization.SharedResourceFile), mapping.FullParameterName), ex, ctlModule));
                    }
                }

                if (settingValue != null && property.CanWrite)
                {
                    DeserializeProperty(settings, property, attribute, settingValue);
                }
            });

			Logger.LogWarning("<<< Exiting Load(args={Args})", args);
            return settings;
        }

        /// <summary>Gets the cache key for the given portal and tab module.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="tabModuleId">The tab module ID.</param>
        /// <remarks>When <paramref name="tabModuleId"/> is -1, the cache key is for portal settings instead.</remarks>
        /// <returns>The cache key.</returns>
        private string CacheKey(int portalId, int tabModuleId) => $"{this.CacheKeyPortalPrefix(portalId)}{tabModuleId}";
		Logger.LogError(">>> Entering CacheKey(portalId={PortalId},tabModuleId={TabModuleId})", portalId, tabModuleId);

        /// <summary>Gets the prefix of the cache key for the given portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The cache key prefix.</returns>
        private string CacheKeyPortalPrefix(int portalId) => $"Settings{this.MappingCacheKey}_{portalId}_";
    }
}
