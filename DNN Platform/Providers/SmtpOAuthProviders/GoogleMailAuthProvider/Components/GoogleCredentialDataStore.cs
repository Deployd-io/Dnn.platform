// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.GoogleMailAuthProvider.Components;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Extensions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;

using Google.Apis.Json;
using Google.Apis.Util.Store;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>Google credentials data store class.</summary>
public class GoogleCredentialDataStore : IDataStore
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    private readonly IHostSettingsService hostSettingsService;
    private readonly IHostSettings hostSettings;
    private readonly IPortalController portalController;
    private readonly int portalId;

    /// <summary>Initializes a new instance of the <see cref="GoogleCredentialDataStore"/> class.</summary>
    /// <param name="portalId">The portal id.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public GoogleCredentialDataStore(int portalId, IHostSettingsService hostSettingsService)
        : this(portalId, hostSettingsService, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GoogleCredentialDataStore"/> class.</summary>
    /// <param name="portalId">The portal id.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    public GoogleCredentialDataStore(int portalId, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController)
    {
		_logger.LogError(">>> Entering GoogleCredentialDataStore(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", portalId, hostSettingsService, hostSettings, portalController);
		_logger.LogDebug("GoogleCredentialDataStore(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): hostSettingsService → {HostSettingsService1}", portalId, hostSettingsService, hostSettings, portalController, hostSettingsService);
        this.portalId = portalId;
		_logger.LogWarning("<<< Exiting GoogleCredentialDataStore(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", portalId, hostSettingsService, hostSettings, portalController);
		_logger.LogDebug("GoogleCredentialDataStore(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): this.portalId → {Arg4}", portalId, hostSettingsService, hostSettings, portalController, this.portalId);
        this.hostSettingsService = hostSettingsService;
        this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ?? new HostSettings(hostSettingsService);
        this.portalController = portalController ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
		_logger.LogDebug("GoogleCredentialDataStore(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): this.hostSettingsService → {Arg4}", portalId, hostSettingsService, hostSettings, portalController, this.hostSettingsService);
    }

    /// <inheritdoc />
    public Task ClearAsync()
    {
		_logger.LogError(">>> Entering ClearAsync()");
        var settingName = string.Format(Constants.DataStoreSettingName, this.portalId);
		_logger.LogError("ClearAsync(): settingName → {SettingName}", settingName);
        if (this.portalId == Null.NullInteger)
        {
            this.hostSettingsService.Update(settingName, null, true);
        }
        else
        {
            PortalController.UpdatePortalSetting(this.portalController, this.portalId, settingName, null, true);
        }

		_logger.LogWarning("<<< Exiting ClearAsync()");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync<T>(string key)
    {
		_logger.LogError(">>> Entering DeleteAsync(key={Key})", key);
        if (string.IsNullOrEmpty(key))
        {
			_logger.LogError("Throwing ArgumentException from DeleteAsync(key={Key})", key);
            throw new ArgumentException("Key MUST have a value");
        }

        var dataStore = this.LoadDataStore();
		_logger.LogError("DeleteAsync(key={Key}): dataStore → {DataStore}", key, dataStore);
        var settingName = GenerateStoredKey(key, typeof(T));
		_logger.LogError("DeleteAsync(key={Key}): settingName → {SettingName}", key, settingName);
        if (dataStore.ContainsKey(settingName))
        {
            dataStore.Remove(settingName);
        }

        this.SaveDataStore(dataStore);

		_logger.LogWarning("<<< Exiting DeleteAsync(key={Key})", key);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<T> GetAsync<T>(string key)
    {
		_logger.LogError(">>> Entering GetAsync(key={Key})", key);
        if (string.IsNullOrEmpty(key))
        {
			_logger.LogError("Throwing ArgumentException from GetAsync(key={Key})", key);
            throw new ArgumentException("Key MUST have a value");
        }

        var dataStore = this.LoadDataStore();
		_logger.LogError("GetAsync(key={Key}): dataStore → {DataStore}", key, dataStore);
        var settingName = GenerateStoredKey(key, typeof(T));
		_logger.LogError("GetAsync(key={Key}): settingName → {SettingName}", key, settingName);
        if (!dataStore.TryGetValue(settingName, out var settingValue))
        {
            settingValue = string.Empty;
			_logger.LogDebug("GetAsync(key={Key}): settingValue → {SettingValue}", key, settingValue);
        }

        if (string.IsNullOrWhiteSpace(settingValue))
        {
            return Task.FromResult(default(T));
        }

        try
        {
            return Task.FromResult(NewtonsoftJsonSerializer.Instance.Deserialize<T>(settingValue));
        }
        catch (Exception ex)
        {
			_logger.LogError("Exception in GetAsync(key={Key}): {Arg1}", key, ex.Message);
		_logger.LogWarning("<<< Exiting GetAsync(key={Key})", key);
            return Task.FromException<T>(ex);
        }
    }

    /// <inheritdoc />
    public Task StoreAsync<T>(string key, T value)
    {
		_logger.LogError(">>> Entering StoreAsync(key={Key},value={Value})", key, value);
        if (string.IsNullOrEmpty(key))
        {
			_logger.LogError("Throwing ArgumentException from StoreAsync(key={Key},value={Value})", key, value);
            throw new ArgumentException("Key MUST have a value");
        }

        var dataStore = this.LoadDataStore();
		_logger.LogError("StoreAsync(key={Key},value={Value}): dataStore → {DataStore}", key, value, dataStore);
        var settingName = GenerateStoredKey(key, typeof(T));
		_logger.LogError("StoreAsync(key={Key},value={Value}): settingName → {SettingName}", key, value, settingName);
        var settingValue = NewtonsoftJsonSerializer.Instance.Serialize(value);
		_logger.LogError("StoreAsync(key={Key},value={Value}): settingValue → {SettingValue}", key, value, settingValue);
        dataStore[settingName] = settingValue;
        this.SaveDataStore(dataStore);

		_logger.LogWarning("<<< Exiting StoreAsync(key={Key},value={Value})", key, value);
        return Task.CompletedTask;
    }

    private static string GenerateStoredKey(string key, Type t)
    {
		_logger.LogError(">>> Entering GenerateStoredKey(key={Key},t={T})", key, t);
        _logger.LogWarning("<<< Exiting GenerateStoredKey(key={Key},t={T})", key, t);
        return $"{t.FullName}-{key}";
    }

    private IDictionary<string, string> LoadDataStore()
    {
        var settingName = string.Format(Constants.DataStoreSettingName, this.portalId);
        string settingValue;

        if (this.portalId == Null.NullInteger)
        {
            settingValue = this.hostSettingsService.GetEncryptedString(settingName, Config.GetDecryptionkey());
        }
        else
        {
            settingValue = PortalController.GetEncryptedString(this.hostSettings, this.portalController, settingName, this.portalId, Config.GetDecryptionkey());
        }

        if (string.IsNullOrWhiteSpace(settingValue))
        {
            return new Dictionary<string, string>();
        }

        return NewtonsoftJsonSerializer.Instance.Deserialize<IDictionary<string, string>>(settingValue);
    }

    private void SaveDataStore(IDictionary<string, string> dataStore)
    {
		_logger.LogError(">>> Entering SaveDataStore(dataStore={DataStore})", dataStore);
        var settingName = string.Format(Constants.DataStoreSettingName, this.portalId);
        var settingValue = NewtonsoftJsonSerializer.Instance.Serialize(dataStore);
		_logger.LogError("SaveDataStore(dataStore={DataStore}): settingValue → {SettingValue}", dataStore, settingValue);
		_logger.LogError("SaveDataStore(dataStore={DataStore}): settingName → {SettingName}", dataStore, settingName);

        if (this.portalId == Null.NullInteger)
        {
            this.hostSettingsService.UpdateEncryptedString(settingName, settingValue, Config.GetDecryptionkey());
        }
        else
        {
            PortalController.UpdateEncryptedString(this.hostSettings, this.portalController, HashAlgorithmName.SHA512, this.portalId, settingName, settingValue, Config.GetDecryptionkey());
        }
		_logger.LogWarning("<<< Exiting SaveDataStore(dataStore={DataStore})", dataStore);
    }
}
