import { seo as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;
const siteInfoActions = {
    getGeneralSettings(callback) {
		logger.debug(`>>> Entering getGeneralSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getGeneralSettings(callback=${callback})`);
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
		logger.debug(`>>> Entering updateGeneralSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateGeneralSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering generalSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting generalSettingsClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering getRegexSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getRegexSettings(callback=${callback})`);
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
		logger.debug(`>>> Entering updateRegexSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateRegexSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering regexSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting regexSettingsClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering testUrl(pageId=${pageId},queryString=${queryString},customPageName=${customPageName},callback=${callback})`);
		logger.debug(`<<< Exiting testUrl(pageId=${pageId},queryString=${queryString},customPageName=${customPageName},callback=${callback})`);
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
		logger.debug(`>>> Entering testUrlRewrite(uri=${uri},callback=${callback})`);
		logger.debug(`<<< Exiting testUrlRewrite(uri=${uri},callback=${callback})`);
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
		logger.debug('>>> Entering clearUrlTestResults()');
		logger.debug('<<< Exiting clearUrlTestResults()');
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
		logger.debug('>>> Entering clearUrlRewritingTestResults()');
		logger.debug('<<< Exiting clearUrlRewritingTestResults()');
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
		logger.debug(`>>> Entering getSitemapSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getSitemapSettings(callback=${callback})`);
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
		logger.debug(`>>> Entering updateSitemapSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateSitemapSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering sitemapSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting sitemapSettingsClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering clearCache(callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting clearCache(callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering getSitemapProviders(callback=${callback})`);
		logger.debug(`<<< Exiting getSitemapProviders(callback=${callback})`);
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
		logger.debug(`>>> Entering updateSitemapProvider(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateSitemapProvider(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering getExtensionUrlProviders(callback=${callback})`);
		logger.debug(`<<< Exiting getExtensionUrlProviders(callback=${callback})`);
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
		logger.debug(`>>> Entering updateExtensionUrlProviderStatus(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateExtensionUrlProviderStatus(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering createVerification(verification=${verification},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting createVerification(verification=${verification},callback=${callback},failureCallback=${failureCallback})`);
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
