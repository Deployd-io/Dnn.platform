import { security as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";
const logger = console;

const securityActions = {
    getIpFilters(callback) {
		logger.debug(`>>> Entering getIpFilters(callback=${callback})`);
		logger.debug(`<<< Exiting getIpFilters(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getIpFilters(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_IP_FILTERS,
                    data: {
                        ipFilters: data.Results.Filters,
                        enableIPChecking: data.Results.EnableIPChecking
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getIpFilter(searchParameters, callback) {
		logger.debug(`>>> Entering getIpFilter(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getIpFilter(searchParameters=${searchParameters},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getIpFilter(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_IP_FILTER,
                    data: {
                        ipFilter: data.Results
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    deleteIpFilter(filterId, ipFilters, callback) {
		logger.debug(`>>> Entering deleteIpFilter(filterId=${filterId},ipFilters=${ipFilters},callback=${callback})`);
		logger.debug(`<<< Exiting deleteIpFilter(filterId=${filterId},ipFilters=${ipFilters},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.deleteIpFilter(filterId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SECURITY_IP_FILTER,
                    data: {
                        ipFilters: ipFilters
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                util.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    getBasicLoginSettings(cultureCode, callback) {
		logger.debug(`>>> Entering getBasicLoginSettings(cultureCode=${cultureCode},callback=${callback})`);
		logger.debug(`<<< Exiting getBasicLoginSettings(cultureCode=${cultureCode},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getBasicLoginSettings(cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_BASIC_LOGIN_SETTINS,
                    data: {
                        basicLoginSettings: data.Results.Settings,
                        adminUsers: data.Results.Administrators,
                        authProviders: data.Results.AuthProviders,
                        basicLoginSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateBasicLoginSettings(payload, callback) {
		logger.debug(`>>> Entering updateBasicLoginSettings(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting updateBasicLoginSettings(payload=${payload},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.updateBasicLoginSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_BASIC_LOGIN_SETTINS,
                    data: {
                        basicLoginSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    basicLoginSettingsClientModified(parameter) {
		logger.debug(`>>> Entering basicLoginSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting basicLoginSettingsClientModified(parameter=${parameter})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_BASIC_LOGIN_SETTINS_CLIENT_MODIFIED,
                data: {
                    basicLoginSettings: parameter,
                    basicLoginSettingsClientModified: true
                }
            });
        };
    },
    memberSettingsClientModified(parameter) {
		logger.debug(`>>> Entering memberSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting memberSettingsClientModified(parameter=${parameter})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_MEMBER_SETTINS_CLIENT_MODIFIED,
                data: {
                    memberSettings: parameter,
                    memberSettingsClientModified: true
                }
            });
        };
    },
    registrationSettingsClientModified(parameter) {
		logger.debug(`>>> Entering registrationSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting registrationSettingsClientModified(parameter=${parameter})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_REGISTRATION_SETTINS_CLIENT_MODIFIED,
                data: {
                    registrationSettings: parameter,
                    registrationSettingsClientModified: true
                }
            });
        };
    },
    sslSettingsClientModified(parameter) {
		logger.debug(`>>> Entering sslSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting sslSettingsClientModified(parameter=${parameter})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_SSL_SETTINS_CLIENT_MODIFIED,
                data: {
                    sslSettings: parameter,
                    sslSettingsClientModified: true
                }
            });
        };
    },
    otherSettingsClientModified(parameter) {
		logger.debug(`>>> Entering otherSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting otherSettingsClientModified(parameter=${parameter})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_OTHER_SETTINS_CLIENT_MODIFIED,
                data: {
                    otherSettings: parameter,
                    otherSettingsClientModified: true
                }
            });
        };
    },
    updateIpFilter(payload, callback) {
		logger.debug(`>>> Entering updateIpFilter(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting updateIpFilter(payload=${payload},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.updateIpFilter(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_IP_FILTER,
                    data: {

                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getMemberSettings(callback) {
		logger.debug(`>>> Entering getMemberSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getMemberSettings(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getMemberSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_MEMBER_SETTINS,
                    data: {
                        memberSettings: data.Results.Settings,
                        memberSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateMemberSettings(payload, callback) {
		logger.debug(`>>> Entering updateMemberSettings(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting updateMemberSettings(payload=${payload},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.updateMemberSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_MEMBER_SETTINS,
                    data: {
                        memberSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getRegistrationSettings(callback) {
		logger.debug(`>>> Entering getRegistrationSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getRegistrationSettings(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getRegistrationSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_REGISTRATION_SETTINS,
                    data: {
                        registrationSettings: data.Results.Settings,
                        userRegistrationOptions: data.Results.UserRegistrationOptions,
                        registrationFormTypeOptions: data.Results.RegistrationFormTypeOptions,
                        registrationSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateRegistrationSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateRegistrationSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateRegistrationSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.updateRegistrationSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_REGISTRATION_SETTINS,
                    data: {
                        registrationSettingsClientModified: false
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
    getSslSettings(callback) {
		logger.debug(`>>> Entering getSslSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getSslSettings(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getSslSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_SSL_SETTINS,
                    data: {
                        sslSettings: data.Results.Settings,
                        sslSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateSslSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateSslSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateSslSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.updateSslSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_SSL_SETTINS,
                    data: {
                        sslSettingsClientModified: false
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
    setAllPagesSecure(callback, failureCallback) {
		logger.debug(`>>> Entering setAllPagesSecure(callback=${callback},failureCallback=${failureCallback})`);
        ApplicationService.setAllPagesSecure(data => {
            if (callback) {
                callback(data);
            }
        }, data => {
            if (failureCallback) {
                failureCallback(data);
				logger.debug(`<<< Exiting setAllPagesSecure(callback=${callback},failureCallback=${failureCallback})`);
            }
        });
    },
    getOtherSettings(callback) {
		logger.debug(`>>> Entering getOtherSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getOtherSettings(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getOtherSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_OTHER_SETTINS,
                    data: {
                        otherSettings: data.Results.Settings,
                        otherSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateOtherSettings(payload, callback) {
		logger.debug(`>>> Entering updateOtherSettings(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting updateOtherSettings(payload=${payload},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.updateOtherSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_OTHER_SETTINS,
                    data: {
                        otherSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getSecurityBulletins(callback, failureCallback) {
		logger.debug(`>>> Entering getSecurityBulletins(callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting getSecurityBulletins(callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.getSecurityBulletins(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_BULLETINS,
                    data: {
                        platformVersion: data.Results.PlatformVersion,
                        securityBulletins: data.Results.SecurityBulletins
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
    getSuperuserActivities(callback) {
		logger.debug(`>>> Entering getSuperuserActivities(callback=${callback})`);
		logger.debug(`<<< Exiting getSuperuserActivities(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getSuperuserActivities(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_SUPERUSER_ACTIVITIES,
                    data: {
                        activities: data.Results.Activities
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getAuditCheckResults(callback) {
		logger.debug(`>>> Entering getAuditCheckResults(callback=${callback})`);
		logger.debug(`<<< Exiting getAuditCheckResults(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getAuditCheckResults(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_AUDITCHECK_RESULTS,
                    data: {
                        auditCheckResults: data.Results
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getAuditCheckResult(id, callback) {
		logger.debug(`>>> Entering getAuditCheckResult(id=${id},callback=${callback})`);
		logger.debug(`<<< Exiting getAuditCheckResult(id=${id},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getAuditCheckResult(id, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_AUDITCHECK_RESULT,
                    data: {
                        auditCheckResult: data.Result
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    searchFileSystemAndDatabase(searchParameters, callback) {
		logger.debug(`>>> Entering searchFileSystemAndDatabase(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting searchFileSystemAndDatabase(searchParameters=${searchParameters},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.searchFileSystemAndDatabase(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE,
                    data: {
                        searchResults: data.Results,
                        scannerCheckKeyword: searchParameters.term,
                        scannerCheckActiveTab: "search"
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    clearFileSystemAndDatabaseSearch() {
		logger.debug('>>> Entering clearFileSystemAndDatabaseSearch()');
		logger.debug('<<< Exiting clearFileSystemAndDatabaseSearch()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.RETRIEVED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE,
                data: {
                    searchResults: undefined,
                    scannerCheckKeyword: "",
                    scannerCheckActiveTab: "search"
                }
            });
        };
    },
    getLastModifiedSettings(callback) {
		logger.debug(`>>> Entering getLastModifiedSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getLastModifiedSettings(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getLastModifiedSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_LAST_MODIFIED_SETTINGS,
                    data: {
                        modifiedSettings: data.Results,
                        scannerCheckActiveTab: "settings"
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getLastModifiedFiles(callback) {
		logger.debug(`>>> Entering getLastModifiedFiles(callback=${callback})`);
		logger.debug(`<<< Exiting getLastModifiedFiles(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getLastModifiedFiles(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_LAST_MODIFIED_FILES,
                    data: {
                        modifiedFiles: data.Results,
                        scannerCheckActiveTab: "files"
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updatefileSystemAndDatabaseSearchKeyword(keyword) {
		logger.debug(`>>> Entering updatefileSystemAndDatabaseSearchKeyword(keyword=${keyword})`);
		logger.debug(`<<< Exiting updatefileSystemAndDatabaseSearchKeyword(keyword=${keyword})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE_KEYWORD,
                data: {
                    scannerCheckKeyword: keyword,
                    scannerCheckActiveTab: "search"
                }
            });
        };
    },
    updatefileSystemAndDatabaseActiveTab(tab) {
		logger.debug(`>>> Entering updatefileSystemAndDatabaseActiveTab(tab=${tab})`);
		logger.debug(`<<< Exiting updatefileSystemAndDatabaseActiveTab(tab=${tab})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_SECURITY_SEARCH_FILE_SYSTEM_DATABASE_ACTIVE_TAB,
                data: {
                    scannerCheckActiveTab: tab
                }
            });
        };
    },
    getPortalList(addAll, callback) {
		logger.debug(`>>> Entering getPortalList(addAll=${addAll},callback=${callback})`);
		logger.debug(`<<< Exiting getPortalList(addAll=${addAll},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getPortalList(addAll, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTAL_LIST,
                    data: {
                        portalList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getApiTokenSettings(callback) {
		logger.debug(`>>> Entering getApiTokenSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getApiTokenSettings(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getApiTokenSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_APITOKEN_SETTINGS,
                    data: {
                        apiTokenSettings: data.Results.ApiTokenSettings,
                        apiTokenSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateApiTokenSettings(payload, callback) {
		logger.debug(`>>> Entering updateApiTokenSettings(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting updateApiTokenSettings(payload=${payload},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.updateApiTokenSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SECURITY_APITOKEN_SETTINGS,
                    data: {
                        apiTokenSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    apiTokenSettingsClientModified(parameter) {
		logger.debug(`>>> Entering apiTokenSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting apiTokenSettingsClientModified(parameter=${parameter})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_APITOKEN_SETTINS_CLIENT_MODIFIED,
                data: {
                    apiTokenSettings: parameter,
                    apiTokenSettingsClientModified: true
                }
            });
        };
    },
    getApiTokenKeys(callback) {
		logger.debug(`>>> Entering getApiTokenKeys(callback=${callback})`);
		logger.debug(`<<< Exiting getApiTokenKeys(callback=${callback})`);
        return () => {
            ApplicationService.getApiTokenKeys(data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getApiTokens(portalId, filter, apiKey, scope, pageIndex, pageSize, callback) {
        return () => {
            ApplicationService.getApiTokens(portalId, filter, apiKey, scope, pageIndex, pageSize, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    createApiToken(name, scope, tokenTimespan, apiKeys, callback) {
        return () => {
            ApplicationService.createApiToken({
                TokenName: name,
                Scope: scope,
                TokenTimespan: tokenTimespan,
                ApiKeys: apiKeys
            }, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    revokeOrDeleteApiToken(apiTokenId, isDelete, callback) {
        return () => {
            ApplicationService.revokeOrDeleteApiToken({
                ApiTokenId: apiTokenId,
                Delete: isDelete
            }, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    deleteExpiredApiTokens(callback) {
		logger.debug(`>>> Entering deleteExpiredApiTokens(callback=${callback})`);
		logger.debug(`<<< Exiting deleteExpiredApiTokens(callback=${callback})`);
        return () => {
            ApplicationService.deleteExpiredApiTokens(data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    deleteExpiredAndRevokedApiTokens(callback) {
		logger.debug(`>>> Entering deleteExpiredAndRevokedApiTokens(callback=${callback})`);
		logger.debug(`<<< Exiting deleteExpiredAndRevokedApiTokens(callback=${callback})`);
        return () => {
            ApplicationService.deleteExpiredAndRevokedApiTokens(data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getCspSettings(callback) {
		logger.debug(`>>> Entering getCspSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getCspSettings(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getCspSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SECURITY_CSP_SETTINGS,
                    data: {
                        cspSettings: data.Results.Settings,
                        cspSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateCspSettings(payload, callback) {
		logger.debug(`>>> Entering updateCspSettings(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting updateCspSettings(payload=${payload},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.updateCspSettings(payload, data => {
                if (data.Success) {
                    dispatch({
                        type: ActionTypes.UPDATED_SECURITY_CSP_SETTINGS,
                        data: {
                            cspSettingsClientModified: false
                        }
                    });
                }
                if (callback) {
                    callback(data);
                }
            });
            
        };
    },
    cspSettingsClientModified(parameter) {
		logger.debug(`>>> Entering cspSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting cspSettingsClientModified(parameter=${parameter})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SECURITY_CSP_SETTINS_CLIENT_MODIFIED,
                data: {
                    cspSettings: parameter,
                    cspSettingsClientModified: true
                }
            });
        };
    }
};

export default securityActions;
