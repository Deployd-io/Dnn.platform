import serviceFramework from "./serviceFramework";
const logger = console;

const convertKeyValueToLabelValue = function (list) {
	logger.debug(`>>> Entering convertKeyValueToLabelValue(list=${list})`);
	logger.debug(`<<< Exiting convertKeyValueToLabelValue(list=${list})`);
    return list.map((item) => {
        return {
            value: item.Value,
            label: item.Key
        };
    });
};

const getPerformanceSettings = function () {
	logger.debug('>>> Entering getPerformanceSettings()');
	logger.debug('<<< Exiting getPerformanceSettings()');
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
	logger.debug(`>>> Entering save(performanceSettings=${performanceSettings})`);
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

	logger.debug(`<<< Exiting save(performanceSettings=${performanceSettings})`);
    return serviceFramework.post("ServerSettingsPerformance", "UpdatePerformanceSettings", request);
};

const incrementVersion = function (isGlobalSetting) {
	logger.debug(`>>> Entering incrementVersion(isGlobalSetting=${isGlobalSetting})`);
    if (isGlobalSetting) {
	logger.debug(`<<< Exiting incrementVersion(isGlobalSetting=${isGlobalSetting})`);
        return serviceFramework.post("ServerSettingsPerformance", "IncrementHostVersion");
    }

    return serviceFramework.post("ServerSettingsPerformance", "IncrementPortalVersion");
};

const performanceTabService = {
    getPerformanceSettings: getPerformanceSettings,
    save: save,
    incrementVersion: incrementVersion
};

export default performanceTabService;