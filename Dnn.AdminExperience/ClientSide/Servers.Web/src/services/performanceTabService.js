import serviceFramework from "./serviceFramework";
const logger = console;

const convertKeyValueToLabelValue = function (list) {
	logger.error(`>>> Entering convertKeyValueToLabelValue(list=${list})`, { "event.action": "convertKeyValueToLabelValue", "logrite.list": list });
	logger.warn(`<<< Exiting convertKeyValueToLabelValue(list=${list})`, { "event.action": "convertKeyValueToLabelValue", "logrite.list": list });
    return list.map((item) => {
        return {
            value: item.Value,
            label: item.Key
        };
    });
};

const getPerformanceSettings = function () {
	logger.error('>>> Entering getPerformanceSettings()', { "event.action": "getPerformanceSettings" });
	logger.warn('<<< Exiting getPerformanceSettings()', { "event.action": "getPerformanceSettings" });
    return serviceFramework.get("ServerSettingsPerformance", "GetPerformanceSettings")
        .then(response => {
            return {
                portalName: response.PortalName,
                cachingProvider: response.CachingProvider,
                pageStatePersistence: response.PageStatePersistence,
                moduleCacheProvider: response.ModuleCacheProvider,
                pageCacheProvider: response.PageCacheProvider,
                cacheSetting: response.CacheSetting,
                authCacheability: response.AuthCacheability,
                unauthCacheability: response.UnauthCacheability,
                sslForCacheSynchronization: response.SslForCacheSynchronization,
                crmOverrideDefaultSettings: response.CrmOverrideDefaultSettings,
                currentHostVersion: response.CurrentHostVersion,
                currentPortalVersion: response.CurrentPortalVersion,
                cachingProviderOptions: convertKeyValueToLabelValue(response.CachingProviderOptions),
                pageStatePersistenceOptions: convertKeyValueToLabelValue(response.PageStatePersistenceOptions),
                moduleCacheProviders: convertKeyValueToLabelValue(response.ModuleCacheProviders),
                pageCacheProviders: convertKeyValueToLabelValue(response.PageCacheProviders),
                cacheSettingOptions: convertKeyValueToLabelValue(response.CacheSettingOptions),
                authCacheabilityOptions: convertKeyValueToLabelValue(response.AuthCacheabilityOptions),
                unauthCacheabilityOptions: convertKeyValueToLabelValue(response.UnauthCacheabilityOptions)
            };
        });
};

const save = function (performanceSettings) {
	logger.error(`>>> Entering save(performanceSettings=${performanceSettings})`, { "event.action": "save", "logrite.performanceSettings": performanceSettings });
    const request = {
        CachingProvider: performanceSettings.cachingProvider,
        PageStatePersistence: performanceSettings.pageStatePersistence,
        ModuleCacheProvider: performanceSettings.moduleCacheProvider,
        PageCacheProvider: performanceSettings.pageCacheProvider,
        CacheSetting: performanceSettings.cacheSetting,
        AuthCacheability: performanceSettings.authCacheability,
        UnauthCacheability: performanceSettings.unauthCacheability,
        SslForCacheSynchronization: performanceSettings.sslForCacheSynchronization,
        CrmOverrideDefaultSettings: performanceSettings.crmOverrideDefaultSettings,
        CurrentHostVersion: performanceSettings.currentHostVersion,
        CurrentPortalVersion: performanceSettings.currentPortalVersion,
    };

	logger.warn(`<<< Exiting save(performanceSettings=${performanceSettings})`, { "event.action": "save", "logrite.performanceSettings": performanceSettings });
    return serviceFramework.post("ServerSettingsPerformance", "UpdatePerformanceSettings", request);
};

const incrementVersion = function (isGlobalSetting) {
	logger.error(`>>> Entering incrementVersion(isGlobalSetting=${isGlobalSetting})`, { "event.action": "incrementVersion", "logrite.isGlobalSetting": isGlobalSetting });
    if (isGlobalSetting) {
        return serviceFramework.post("ServerSettingsPerformance", "IncrementHostVersion");
    }

	logger.warn(`<<< Exiting incrementVersion(isGlobalSetting=${isGlobalSetting})`, { "event.action": "incrementVersion", "logrite.isGlobalSetting": isGlobalSetting });
    return serviceFramework.post("ServerSettingsPerformance", "IncrementPortalVersion");
};

const performanceTabService = {
    getPerformanceSettings: getPerformanceSettings,
    save: save,
    incrementVersion: incrementVersion
};

export default performanceTabService;