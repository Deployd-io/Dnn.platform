import { seo as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;
const siteInfoActions = {
    getGeneralSettings(callback) {
		logger.error(`>>> Entering getGeneralSettings(callback=${callback})`, { "event.action": "getGeneralSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getGeneralSettings(callback=${callback})`, { "event.action": "getGeneralSettings", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getGeneralSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_GENERAL_SETTINGS,
                    data: {
                        generalSettings: data.Settings,
                        replacementCharacterList: data.ReplacementCharacterList,
                        deletedPageHandlingTypes: data.DeletedPageHandlingTypes,
                        clientModified: false,
                        adminRoleId: data.AdminRoleId
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateGeneralSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateGeneralSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateGeneralSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateGeneralSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateGeneralSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateGeneralSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_GENERAL_SETTINGS,
                    data: {
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    generalSettingsClientModified(parameter) {
		logger.error(`>>> Entering generalSettingsClientModified(parameter=${parameter})`, { "event.action": "generalSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting generalSettingsClientModified(parameter=${parameter})`, { "event.action": "generalSettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_GENERAL_SETTINS_CLIENT_MODIFIED,
                data: {
                    generalSettings: parameter,
                    clientModified: true
                }
            });
        };
    },
    getRegexSettings(callback) {
		logger.error(`>>> Entering getRegexSettings(callback=${callback})`, { "event.action": "getRegexSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getRegexSettings(callback=${callback})`, { "event.action": "getRegexSettings", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getRegexSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_REGEX_SETTINGS,
                    data: {
                        regexSettings: data.Settings,
                        regexClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateRegexSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateRegexSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateRegexSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateRegexSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateRegexSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateRegexSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_REGEX_SETTINGS,
                    data: {
                        regexClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    regexSettingsClientModified(parameter) {
		logger.error(`>>> Entering regexSettingsClientModified(parameter=${parameter})`, { "event.action": "regexSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting regexSettingsClientModified(parameter=${parameter})`, { "event.action": "regexSettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_REGEX_SETTINS_CLIENT_MODIFIED,
                data: {
                    regexSettings: parameter,
                    regexClientModified: true
                }
            });
        };
    },
    testUrl(pageId, queryString, customPageName, callback) {
		logger.error(`>>> Entering testUrl(pageId=${pageId},queryString=${queryString},customPageName=${customPageName},callback=${callback})`, { "event.action": "testUrl", "logrite.pageId": pageId, "logrite.queryString": queryString, "logrite.customPageName": customPageName, "logrite.callback": callback });
		logger.warn(`<<< Exiting testUrl(pageId=${pageId},queryString=${queryString},customPageName=${customPageName},callback=${callback})`, { "event.action": "testUrl", "logrite.pageId": pageId, "logrite.queryString": queryString, "logrite.customPageName": customPageName, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.testUrl(pageId, queryString, customPageName, data => {
                dispatch({
                    type: ActionTypes.TESTED_SEO_PAGE_URL,
                    data: {
                        urls: data.Urls.join("\n")
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    testUrlRewrite(uri, callback) {
		logger.error(`>>> Entering testUrlRewrite(uri=${uri},callback=${callback})`, { "event.action": "testUrlRewrite", "logrite.uri": uri, "logrite.callback": callback });
		logger.warn(`<<< Exiting testUrlRewrite(uri=${uri},callback=${callback})`, { "event.action": "testUrlRewrite", "logrite.uri": uri, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.testUrlRewrite(uri, data => {
                dispatch({
                    type: ActionTypes.TESTED_SEO_URL_REWRITING,
                    data: {
                        rewritingResult: data.RewritingResult.rewritingResult,
                        culture: data.RewritingResult.culture,
                        identifiedPage: data.RewritingResult.identifiedPage,
                        redirectionReason: data.RewritingResult.redirectionReason,
                        redirectionResult: data.RewritingResult.redirectionResult,
                        operationMessages: data.RewritingResult.operationMessages
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    clearUrlTestResults() {
		logger.error('>>> Entering clearUrlTestResults()', { "event.action": "clearUrlTestResults" });
		logger.warn('<<< Exiting clearUrlTestResults()', { "event.action": "clearUrlTestResults" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEARED_SEO_TEST_PAGE_URL_RESULTS,
                data: {
                    urls: ""
                }
            });
        };
    },
    clearUrlRewritingTestResults() {
		logger.error('>>> Entering clearUrlRewritingTestResults()', { "event.action": "clearUrlRewritingTestResults" });
		logger.warn('<<< Exiting clearUrlRewritingTestResults()', { "event.action": "clearUrlRewritingTestResults" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEARED_SEO_TEST_URL_REWRITING_RESULTS,
                data: {
                    rewritingResult: "",
                    culture: "",
                    identifiedPage: "",
                    redirectionReason: "",
                    redirectionResult: "",
                    operationMessages: ""
                }
            });
        };
    },
    getSitemapSettings(callback) {
		logger.error(`>>> Entering getSitemapSettings(callback=${callback})`, { "event.action": "getSitemapSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getSitemapSettings(callback=${callback})`, { "event.action": "getSitemapSettings", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getSitemapSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_SITEMAP_SETTINGS,
                    data: {
                        sitemapSettings: data.Settings,
                        searchEngineUrls: data.SearchEngineUrls,
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateSitemapSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateSitemapSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSitemapSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateSitemapSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSitemapSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateSitemapSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_SITEMAP_SETTINGS,
                    data: {
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    sitemapSettingsClientModified(parameter) {
		logger.error(`>>> Entering sitemapSettingsClientModified(parameter=${parameter})`, { "event.action": "sitemapSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting sitemapSettingsClientModified(parameter=${parameter})`, { "event.action": "sitemapSettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_SITEMAP_SETTINS_CLIENT_MODIFIED,
                data: {
                    sitemapSettings: parameter,
                    clientModified: true
                }
            });
        };
    },
    clearCache(callback, failureCallback) {
		logger.error(`>>> Entering clearCache(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "clearCache", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting clearCache(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "clearCache", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.clearCache(data => {
                dispatch({
                    type: ActionTypes.CLEARED_SEO_SITEMAP_CACHE,
                    data: {
                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    getSitemapProviders(callback) {
		logger.error(`>>> Entering getSitemapProviders(callback=${callback})`, { "event.action": "getSitemapProviders", "logrite.callback": callback });
		logger.warn(`<<< Exiting getSitemapProviders(callback=${callback})`, { "event.action": "getSitemapProviders", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getSitemapProviders(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_SITEMAP_PROVIDERS,
                    data: {
                        providers: data.Providers
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateSitemapProvider(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateSitemapProvider(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSitemapProvider", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateSitemapProvider(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSitemapProvider", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateSitemapProvider(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_SITEMAP_PROVIDER,
                    data: {
                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    getExtensionUrlProviders(callback) {
		logger.error(`>>> Entering getExtensionUrlProviders(callback=${callback})`, { "event.action": "getExtensionUrlProviders", "logrite.callback": callback });
		logger.warn(`<<< Exiting getExtensionUrlProviders(callback=${callback})`, { "event.action": "getExtensionUrlProviders", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getExtensionUrlProviders(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_EXTENSION_URL_PROVIDERS,
                    data: {
                        providers: data.Providers
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateExtensionUrlProviderStatus(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateExtensionUrlProviderStatus(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateExtensionUrlProviderStatus", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateExtensionUrlProviderStatus(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateExtensionUrlProviderStatus", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateExtensionUrlProviderStatus(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_EXTENSION_URL_PROVIDER,
                    data: {
                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    createVerification(verification, callback, failureCallback) {
		logger.error(`>>> Entering createVerification(verification=${verification},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "createVerification", "logrite.verification": verification, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting createVerification(verification=${verification},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "createVerification", "logrite.verification": verification, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.createVerification(verification, data => {
                dispatch({
                    type: ActionTypes.CREATED_SEO_SITEMAP_VERIFICATION,
                    data: {
                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    }
};

export default siteInfoActions;
