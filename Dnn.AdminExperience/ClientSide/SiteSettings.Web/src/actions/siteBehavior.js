import { siteBehavior as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const siteBehaviorActions = {
    getDefaultPagesSettings(portalId, cultureCode, callback) {
		logger.debug(`>>> Entering getDefaultPagesSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
		logger.debug(`<<< Exiting getDefaultPagesSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
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
		logger.debug(`>>> Entering updateDefaultPagesSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateDefaultPagesSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering defaultPagesSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting defaultPagesSettingsClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering getMessagingSettings(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getMessagingSettings(portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering updateMessagingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateMessagingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering messagingSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting messagingSettingsClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering getProfileSettings(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getProfileSettings(portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering updateProfileSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateProfileSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering profileSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting profileSettingsClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering getProfileProperties(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getProfileProperties(portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering getProfileProperty(propertyId=${propertyId},portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getProfileProperty(propertyId=${propertyId},portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering profilePropertyClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting profilePropertyClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering propertyLocalizationClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting propertyLocalizationClientModified(parameter=${parameter})`);
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
		logger.debug('>>> Entering cancelProfilePropertyClientModified()');
		logger.debug('<<< Exiting cancelProfilePropertyClientModified()');
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
		logger.debug('>>> Entering cancelPropertyLocalizationClientModified()');
		logger.debug('<<< Exiting cancelPropertyLocalizationClientModified()');
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
		logger.debug(`>>> Entering getProfilePropertyLocalization(portalId=${portalId},propertyName=${propertyName},propertyCategory=${propertyCategory},cultureCode=${cultureCode},callback=${callback})`);
		logger.debug(`<<< Exiting getProfilePropertyLocalization(portalId=${portalId},propertyName=${propertyName},propertyCategory=${propertyCategory},cultureCode=${cultureCode},callback=${callback})`);
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
		logger.debug(`>>> Entering updateProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering addProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting addProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering deleteProfileProperty(propertyId=${propertyId},object=${object},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting deleteProfileProperty(propertyId=${propertyId},object=${object},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering updateProfilePropertyLocalization(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateProfilePropertyLocalization(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering getUrlMappingSettings(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getUrlMappingSettings(portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering getSiteAliases(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getSiteAliases(portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering urlMappingSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting urlMappingSettingsClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering updateUrlMappingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateUrlMappingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering getSiteAlias(aliasId=${aliasId},callback=${callback})`);
		logger.debug(`<<< Exiting getSiteAlias(aliasId=${aliasId},callback=${callback})`);
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
		logger.debug('>>> Entering cancelSiteAliasClientModified()');
		logger.debug('<<< Exiting cancelSiteAliasClientModified()');
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
		logger.debug(`>>> Entering siteAliasClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting siteAliasClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering addSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting addSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering updateSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering deleteSiteAlias(aliasId=${aliasId},aliases=${aliases},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting deleteSiteAlias(aliasId=${aliasId},aliases=${aliases},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering getPrivacySettings(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getPrivacySettings(portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering updatePrivacySettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updatePrivacySettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering resetTermsAgreement(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        ApplicationService.resetTermsAgreement(payload, data => {
            if (callback) {
                callback(data);
            }
        }, data => {
            if (failureCallback) {
                failureCallback(data);
				logger.debug(`<<< Exiting resetTermsAgreement(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
            }
        });
    },
    privacySettingsClientModified(parameter) {
		logger.debug(`>>> Entering privacySettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting privacySettingsClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering getOtherSettings(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getOtherSettings(portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering updateOtherSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateOtherSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering otherSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting otherSettingsClientModified(parameter=${parameter})`);
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
		logger.debug('>>> Entering cancelOtherSettingsClientModified()');
		logger.debug('<<< Exiting cancelOtherSettingsClientModified()');
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
		logger.debug(`>>> Entering updateProfilePropertyOrders(payload=${payload},object=${object},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateProfilePropertyOrders(payload=${payload},object=${object},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering sortProfileProperty(profileProperties=${profileProperties})`);
		logger.debug(`<<< Exiting sortProfileProperty(profileProperties=${profileProperties})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_SITESETTINGS_PROFILE_PROPERTY_ORDER,
                data: { profileProperties }
            });
        };
    },
    getListInfo(listName, portalId, callback) {
		logger.debug(`>>> Entering getListInfo(listName=${listName},portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getListInfo(listName=${listName},portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering updateListEntry(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateListEntry(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering deleteListEntry(entryId=${entryId},portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting deleteListEntry(entryId=${entryId},portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering updateListEntryOrders(payload=${payload},object=${object},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateListEntryOrders(payload=${payload},object=${object},callback=${callback},failureCallback=${failureCallback})`);
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
