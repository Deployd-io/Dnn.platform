import { siteBehavior as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const siteBehaviorActions = {
    getDefaultPagesSettings(portalId, cultureCode, callback) {
		logger.error(`>>> Entering getDefaultPagesSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getDefaultPagesSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
		logger.warn(`<<< Exiting getDefaultPagesSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getDefaultPagesSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getDefaultPagesSettings(portalId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_DEFAULT_PAGES_SETTINGS,
                    data: {
                        settings: data.Settings,
                        defaultPagesSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateDefaultPagesSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateDefaultPagesSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateDefaultPagesSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateDefaultPagesSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateDefaultPagesSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateDefaultPagesSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_DEFAULT_PAGES_SETTINGS,
                    data: {
                        defaultPagesSettingsClientModified: false
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
    defaultPagesSettingsClientModified(parameter) {
		logger.error(`>>> Entering defaultPagesSettingsClientModified(parameter=${parameter})`, { "event.action": "defaultPagesSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting defaultPagesSettingsClientModified(parameter=${parameter})`, { "event.action": "defaultPagesSettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_DEFAULT_PAGES_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    defaultPagesSettingsClientModified: true
                }
            });
        };
    },
    getMessagingSettings(portalId, callback) {
		logger.error(`>>> Entering getMessagingSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getMessagingSettings", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getMessagingSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getMessagingSettings", "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getMessagingSettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_MESSAGING_SETTINGS,
                    data: {
                        settings: data.Settings,
                        messagingSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateMessagingSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateMessagingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateMessagingSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateMessagingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateMessagingSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateMessagingSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_MESSAGING_SETTINGS,
                    data: {
                        messagingSettingsClientModified: false
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
    messagingSettingsClientModified(parameter) {
		logger.error(`>>> Entering messagingSettingsClientModified(parameter=${parameter})`, { "event.action": "messagingSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting messagingSettingsClientModified(parameter=${parameter})`, { "event.action": "messagingSettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_MESSAGING_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    messagingSettingsClientModified: true
                }
            });
        };
    },
    getProfileSettings(portalId, callback) {
		logger.error(`>>> Entering getProfileSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getProfileSettings", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getProfileSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getProfileSettings", "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getProfileSettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_SETTINGS,
                    data: {
                        settings: data.Settings,
                        userVisibilityOptions: data.UserVisibilityOptions,
                        profileSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateProfileSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateProfileSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfileSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateProfileSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfileSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateProfileSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_SETTINGS,
                    data: {
                        profileSettingsClientModified: false
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
    profileSettingsClientModified(parameter) {
		logger.error(`>>> Entering profileSettingsClientModified(parameter=${parameter})`, { "event.action": "profileSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting profileSettingsClientModified(parameter=${parameter})`, { "event.action": "profileSettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_PROFILE_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    profileSettingsClientModified: true
                }
            });
        };
    },
    getProfileProperties(portalId, callback) {
		logger.error(`>>> Entering getProfileProperties(portalId=${portalId},callback=${callback})`, { "event.action": "getProfileProperties", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getProfileProperties(portalId=${portalId},callback=${callback})`, { "event.action": "getProfileProperties", "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getProfileProperties(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_PROPERTIES,
                    data: {
                        profileProperties: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getProfileProperty(propertyId, portalId, callback) {
		logger.error(`>>> Entering getProfileProperty(propertyId=${propertyId},portalId=${portalId},callback=${callback})`, { "event.action": "getProfileProperty", "logrite.propertyId": propertyId, "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getProfileProperty(propertyId=${propertyId},portalId=${portalId},callback=${callback})`, { "event.action": "getProfileProperty", "logrite.propertyId": propertyId, "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getProfileProperty(propertyId, portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_PROPERTY,
                    data: {
                        profileProperty: data.ProfileProperty,
                        userVisibilityOptions: data.UserVisibilityOptions,
                        dataTypeOptions: data.DataTypeOptions,
                        languageOptions: data.LanguageOptions,
                        profilePropertyClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    profilePropertyClientModified(parameter) {
		logger.error(`>>> Entering profilePropertyClientModified(parameter=${parameter})`, { "event.action": "profilePropertyClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting profilePropertyClientModified(parameter=${parameter})`, { "event.action": "profilePropertyClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_PROFILE_PROPERTY_CLIENT_MODIFIED,
                data: {
                    profileProperty: parameter,
                    profilePropertyClientModified: true
                }
            });
        };
    },
    propertyLocalizationClientModified(parameter) {
		logger.error(`>>> Entering propertyLocalizationClientModified(parameter=${parameter})`, { "event.action": "propertyLocalizationClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting propertyLocalizationClientModified(parameter=${parameter})`, { "event.action": "propertyLocalizationClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION_CLIENT_MODIFIED,
                data: {
                    propertyLocalization: parameter,
                    propertyLocalizationClientModified: true
                }
            });
        };
    },
    cancelProfilePropertyClientModified() {
		logger.error('>>> Entering cancelProfilePropertyClientModified()', { "event.action": "cancelProfilePropertyClientModified" });
		logger.warn('<<< Exiting cancelProfilePropertyClientModified()', { "event.action": "cancelProfilePropertyClientModified" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_PROFILE_PROPERTY_CLIENT_MODIFIED,
                data: {
                    profilePropertyClientModified: false
                }
            });
        };
    },
    cancelPropertyLocalizationClientModified() {
		logger.error('>>> Entering cancelPropertyLocalizationClientModified()', { "event.action": "cancelPropertyLocalizationClientModified" });
		logger.warn('<<< Exiting cancelPropertyLocalizationClientModified()', { "event.action": "cancelPropertyLocalizationClientModified" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION_CLIENT_MODIFIED,
                data: {
                    propertyLocalizationClientModified: false
                }
            });
        };
    },
    getProfilePropertyLocalization(portalId, propertyName, propertyCategory, cultureCode, callback) {
		logger.error(`>>> Entering getProfilePropertyLocalization(portalId=${portalId},propertyName=${propertyName},propertyCategory=${propertyCategory},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getProfilePropertyLocalization", "logrite.portalId": portalId, "logrite.propertyName": propertyName, "logrite.propertyCategory": propertyCategory, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
		logger.warn(`<<< Exiting getProfilePropertyLocalization(portalId=${portalId},propertyName=${propertyName},propertyCategory=${propertyCategory},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getProfilePropertyLocalization", "logrite.portalId": portalId, "logrite.propertyName": propertyName, "logrite.propertyCategory": propertyCategory, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getProfilePropertyLocalization(portalId, propertyName, propertyCategory, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION,
                    data: {
                        propertyLocalization: data.PropertyLocalization,
                        propertyLocalizationClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateProfileProperty(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfileProperty", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfileProperty", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateProfileProperty(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY,
                    data: {
                        profilePropertyClientModified: false
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
    addProfileProperty(payload, callback, failureCallback) {
		logger.error(`>>> Entering addProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addProfileProperty", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting addProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addProfileProperty", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.addProfileProperty(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_PROFILE_PROPERTY,
                    data: {
                        profilePropertyClientModified: false
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
    deleteProfileProperty(propertyId, object, callback, failureCallback) {
		logger.error(`>>> Entering deleteProfileProperty(propertyId=${propertyId},object=${object},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteProfileProperty", "logrite.propertyId": propertyId, "logrite.object": object, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting deleteProfileProperty(propertyId=${propertyId},object=${object},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteProfileProperty", "logrite.propertyId": propertyId, "logrite.object": object, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.deleteProfileProperty(propertyId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_PROFILE_PROPERTY,
                    data: {
                        profileProperties: object
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
    updateProfilePropertyLocalization(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateProfilePropertyLocalization(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfilePropertyLocalization", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateProfilePropertyLocalization(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfilePropertyLocalization", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateProfilePropertyLocalization(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY_LOCALIZATION,
                    data: {
                        propertyLocalizationClientModified: false
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
    getUrlMappingSettings(portalId, callback) {
		logger.error(`>>> Entering getUrlMappingSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getUrlMappingSettings", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getUrlMappingSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getUrlMappingSettings", "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getUrlMappingSettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_ALIAS_SETTINGS,
                    data: {
                        urlMappingSettings: data.Settings,
                        portalAliasMappingModes: data.PortalAliasMappingModes,
                        urlMappingSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getSiteAliases(portalId, callback) {
		logger.error(`>>> Entering getSiteAliases(portalId=${portalId},callback=${callback})`, { "event.action": "getSiteAliases", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getSiteAliases(portalId=${portalId},callback=${callback})`, { "event.action": "getSiteAliases", "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getSiteAliases(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_ALIASES,
                    data: {
                        siteAliases: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    urlMappingSettingsClientModified(parameter) {
		logger.error(`>>> Entering urlMappingSettingsClientModified(parameter=${parameter})`, { "event.action": "urlMappingSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting urlMappingSettingsClientModified(parameter=${parameter})`, { "event.action": "urlMappingSettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_URL_MAPPING_SETTINGS_CLIENT_MODIFIED,
                data: {
                    urlMappingSettings: parameter,
                    urlMappingSettingsClientModified: true
                }
            });
        };
    },
    updateUrlMappingSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateUrlMappingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateUrlMappingSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateUrlMappingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateUrlMappingSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateUrlMappingSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_URL_MAPPING_SETTINGS,
                    data: {
                        urlMappingSettingsClientModified: false
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
    getSiteAlias(aliasId, callback) {
		logger.error(`>>> Entering getSiteAlias(aliasId=${aliasId},callback=${callback})`, { "event.action": "getSiteAlias", "logrite.aliasId": aliasId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getSiteAlias(aliasId=${aliasId},callback=${callback})`, { "event.action": "getSiteAlias", "logrite.aliasId": aliasId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getSiteAlias(aliasId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_ALIAS,
                    data: {
                        aliasDetail: data.PortalAlias
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    cancelSiteAliasClientModified() {
		logger.error('>>> Entering cancelSiteAliasClientModified()', { "event.action": "cancelSiteAliasClientModified" });
		logger.warn('<<< Exiting cancelSiteAliasClientModified()', { "event.action": "cancelSiteAliasClientModified" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_SITE_ALIAS_CLIENT_MODIFIED,
                data: {
                    siteAliasClientModified: false
                }
            });
        };
    },
    siteAliasClientModified(parameter) {
		logger.error(`>>> Entering siteAliasClientModified(parameter=${parameter})`, { "event.action": "siteAliasClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting siteAliasClientModified(parameter=${parameter})`, { "event.action": "siteAliasClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_SITE_ALIAS_CLIENT_MODIFIED,
                data: {
                    aliasDetail: parameter,
                    siteAliasClientModified: true
                }
            });
        };
    },
    addSiteAlias(payload, callback, failureCallback) {
		logger.error(`>>> Entering addSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addSiteAlias", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting addSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addSiteAlias", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.addSiteAlias(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_SITE_ALIAS,
                    data: {
                        siteAliasClientModified: false
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
    updateSiteAlias(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSiteAlias", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSiteAlias", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateSiteAlias(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_SITE_ALIAS,
                    data: {
                        siteAliasClientModified: false
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
    deleteSiteAlias(aliasId, aliases, callback, failureCallback) {
		logger.error(`>>> Entering deleteSiteAlias(aliasId=${aliasId},aliases=${aliases},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteSiteAlias", "logrite.aliasId": aliasId, "logrite.aliases": aliases, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting deleteSiteAlias(aliasId=${aliasId},aliases=${aliases},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteSiteAlias", "logrite.aliasId": aliasId, "logrite.aliases": aliases, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.deleteSiteAlias(aliasId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_SITE_ALIAS,
                    data: {
                        siteAliases: aliases
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
    getPrivacySettings(portalId, callback) {
		logger.error(`>>> Entering getPrivacySettings(portalId=${portalId},callback=${callback})`, { "event.action": "getPrivacySettings", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getPrivacySettings(portalId=${portalId},callback=${callback})`, { "event.action": "getPrivacySettings", "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getPrivacySettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PRIVACY_SETTINGS,
                    data: {
                        settings: data.Settings,
                        privacySettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updatePrivacySettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updatePrivacySettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updatePrivacySettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updatePrivacySettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updatePrivacySettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updatePrivacySettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PRIVACY_SETTINGS,
                    data: {
                        privacySettingsClientModified: false
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
    resetTermsAgreement(payload, callback, failureCallback) {
		logger.error(`>>> Entering resetTermsAgreement(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "resetTermsAgreement", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        ApplicationService.resetTermsAgreement(payload, data => {
            if (callback) {
                callback(data);
            }
        }, data => {
            if (failureCallback) {
                failureCallback(data);
				logger.warn(`<<< Exiting resetTermsAgreement(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "resetTermsAgreement", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
            }
        });
    },
    privacySettingsClientModified(parameter) {
		logger.error(`>>> Entering privacySettingsClientModified(parameter=${parameter})`, { "event.action": "privacySettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting privacySettingsClientModified(parameter=${parameter})`, { "event.action": "privacySettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_PRIVACY_SETTINGS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    privacySettingsClientModified: true
                }
            });
        };
    },
    getOtherSettings(portalId, callback) {
		logger.error(`>>> Entering getOtherSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getOtherSettings", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getOtherSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getOtherSettings", "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getOtherSettings(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_OTHER_SETTINGS,
                    data: {
                        settings: data.Settings,
                        otherSettingsClientModified: false,
                        workflows: data.Workflows,
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateOtherSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateOtherSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateOtherSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateOtherSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateOtherSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateOtherSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_OTHER_SETTINGS,
                    data: {
                        otherSettingsClientModified: false
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
    otherSettingsClientModified(parameter) {
		logger.error(`>>> Entering otherSettingsClientModified(parameter=${parameter})`, { "event.action": "otherSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting otherSettingsClientModified(parameter=${parameter})`, { "event.action": "otherSettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_OTHER_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    otherSettingsClientModified: true
                }
            });
        };
    },
    cancelOtherSettingsClientModified() {
		logger.error('>>> Entering cancelOtherSettingsClientModified()', { "event.action": "cancelOtherSettingsClientModified" });
		logger.warn('<<< Exiting cancelOtherSettingsClientModified()', { "event.action": "cancelOtherSettingsClientModified" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_OTHER_SETTINGS_CLIENT_MODIFIED,
                data: {
                    otherSettingsClientModified: false
                }
            });
        };
    },
    updateProfilePropertyOrders(payload, object, callback, failureCallback) {
		logger.error(`>>> Entering updateProfilePropertyOrders(payload=${payload},object=${object},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfilePropertyOrders", "logrite.payload": payload, "logrite.object": object, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateProfilePropertyOrders(payload=${payload},object=${object},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfilePropertyOrders", "logrite.payload": payload, "logrite.object": object, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateProfilePropertyOrders(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY_ORDER,
                    data: {
                        profileProperties: object

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
    sortProfileProperty(profileProperties) {
		logger.error(`>>> Entering sortProfileProperty(profileProperties=${profileProperties})`, { "event.action": "sortProfileProperty", "logrite.profileProperties": profileProperties });
		logger.warn(`<<< Exiting sortProfileProperty(profileProperties=${profileProperties})`, { "event.action": "sortProfileProperty", "logrite.profileProperties": profileProperties });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY_ORDER,
                data: { profileProperties }
            });
        };
    },
    getListInfo(listName, portalId, callback) {
		logger.error(`>>> Entering getListInfo(listName=${listName},portalId=${portalId},callback=${callback})`, { "event.action": "getListInfo", "logrite.listName": listName, "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getListInfo(listName=${listName},portalId=${portalId},callback=${callback})`, { "event.action": "getListInfo", "logrite.listName": listName, "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getListInfo(listName, portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LIST_INFO,
                    data: {
                        enableSortOrder: data.EnableSortOrder,
                        entries: data.Entries
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateListEntry(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateListEntry(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateListEntry", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateListEntry(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateListEntry", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateListEntry(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LIST_ENTRY,
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
    deleteListEntry(entryId, portalId, callback, failureCallback) {
		logger.error(`>>> Entering deleteListEntry(entryId=${entryId},portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteListEntry", "logrite.entryId": entryId, "logrite.portalId": portalId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting deleteListEntry(entryId=${entryId},portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteListEntry", "logrite.entryId": entryId, "logrite.portalId": portalId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.deleteListEntry(entryId, portalId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_LIST_ENTRY,
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
    updateListEntryOrders(payload, object, callback, failureCallback) {
		logger.error(`>>> Entering updateListEntryOrders(payload=${payload},object=${object},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateListEntryOrders", "logrite.payload": payload, "logrite.object": object, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateListEntryOrders(payload=${payload},object=${object},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateListEntryOrders", "logrite.payload": payload, "logrite.object": object, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateListEntryOrders(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LIST_ENTRY_ORDER,
                    data: {
                        entries: object

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

export default siteBehaviorActions;
