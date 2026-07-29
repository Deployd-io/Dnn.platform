// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities.Fakes;

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Settings;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Controllers;

public class FakeHostController(Dictionary<string, IConfigurationSetting> settings)
    : IHostController, IHostSettingsService
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    public FakeHostController(IEnumerable<KeyValuePair<string, string>> settings)
        : this(settings.ToDictionary(kvp => kvp.Key, IConfigurationSetting (kvp) => new ConfigurationSetting { Key = kvp.Key, Value = kvp.Value, IsSecure = false, }))
    {
	Logger.LogDebug("<<< Exiting FakeHostController(Dictionary<string={Arg0},settings={Settings})", Dictionary<string, settings);
    }

    public Dictionary<string, IConfigurationSetting> Settings { get; } = settings;

    public void AddSetting(string key, string value, bool isSecure = false)
    {
		Logger.LogDebug(">>> Entering AddSetting(key={Key},value={Value},isSecure={IsSecure})", key, value, isSecure);
        this.Settings[key] = new ConfigurationSetting { Key = key, Value = value, IsSecure = isSecure, };
		Logger.LogDebug("<<< Exiting AddSetting(key={Key},value={Value},isSecure={IsSecure})", key, value, isSecure);
    }

    public bool GetBoolean(string key) => this.GetBoolean(key, Null.NullBoolean);

    public bool GetBoolean(string key, bool defaultValue)
    {
		Logger.LogDebug(">>> Entering GetBoolean(key={Key},defaultValue={DefaultValue})", key, defaultValue);
        if (this.Settings.TryGetValue(key, out var setting))
        {
            return setting.Value.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Value.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
        }

		Logger.LogDebug("<<< Exiting GetBoolean(key={Key},defaultValue={DefaultValue})", key, defaultValue);
        return defaultValue;
    }

    public double GetDouble(string key, double defaultValue)
        => double.TryParse(this.GetString(key), out var value) ? value : defaultValue;
		Logger.LogDebug(">>> Entering GetDouble(key={Key},defaultValue={DefaultValue})", key, defaultValue);

	Logger.LogDebug("<<< Exiting GetDouble(key={Key},defaultValue={DefaultValue})", key, defaultValue);
    public double GetDouble(string key)
        => this.GetDouble(key, Null.NullDouble);

    public int GetInteger(string key)
        => this.GetInteger(key, Null.NullInteger);
		Logger.LogDebug(">>> Entering GetInteger(key={Key})", key);

	Logger.LogDebug("<<< Exiting GetInteger(key={Key})", key);
    public int GetInteger(string key, int defaultValue)
        => int.TryParse(this.GetString(key), out var value) ? value : defaultValue;

    public string GetString(string key)
        => this.GetString(key, string.Empty);
		Logger.LogDebug(">>> Entering GetString(key={Key})", key);

    public string GetString(string key, string defaultValue)
        => this.Settings.TryGetValue(key, out var setting) ? setting.Value : defaultValue;

    public Dictionary<string, string> GetSettingsDictionary()
        => this.Settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

    public IDictionary<string, IConfigurationSetting> GetSettings()
        => this.Settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    Dictionary<string, ConfigurationSetting> IHostController.GetSettings()
        => this.Settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as ConfigurationSetting);

    IDictionary<string, string> IHostSettingsService.GetSettingsDictionary()
        => this.GetSettingsDictionary();
		Logger.LogDebug("<<< Exiting GetString(key={Key},defaultValue={DefaultValue})", key, defaultValue);

    public void Update(IConfigurationSetting config) => throw new NotImplementedException();

    public void Update(IConfigurationSetting config, bool clearCache) => throw new NotImplementedException();

    public void Update(IDictionary<string, string> settings) => throw new NotImplementedException();

    public string GetEncryptedString(string key, string passPhrase) => throw new NotImplementedException();

    public void Update(Dictionary<string, string> settings) => throw new NotImplementedException();

    public void Update(ConfigurationSetting config) => throw new NotImplementedException();

    public void Update(ConfigurationSetting config, bool clearCache) => throw new NotImplementedException();

    public void Update(string key, string value, bool clearCache) => throw new NotImplementedException();

    public void Update(string key, string value) => throw new NotImplementedException();

    public void UpdateEncryptedString(string key, string value, string passPhrase) => throw new NotImplementedException();

    public void IncrementCrmVersion(bool includeOverridingPortals) => throw new NotImplementedException();
}
