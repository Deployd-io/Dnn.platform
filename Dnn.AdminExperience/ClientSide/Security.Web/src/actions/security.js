import { security as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";
const logger = console;

const securityActions = {
    getIpFilters(callback) {
		logger.error(`>>> Entering getIpFilters(callback=${callback})`, { "event.action": "getIpFilters", "logrite.callback": callback });
		logger.warn(`<<< Exiting getIpFilters(callback=${callback})`, { "event.action": "getIpFilters", "logrite.callback": callback });
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
		logger.error(`>>> Entering getIpFilter(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getIpFilter", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getIpFilter(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getIpFilter", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
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
		logger.error(`>>> Entering deleteIpFilter(filterId=${filterId},ipFilters=${ipFilters},callback=${callback})`, { "event.action": "deleteIpFilter", "logrite.filterId": filterId, "logrite.ipFilters": ipFilters, "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteIpFilter(filterId=${filterId},ipFilters=${ipFilters},callback=${callback})`, { "event.action": "deleteIpFilter", "logrite.filterId": filterId, "logrite.ipFilters": ipFilters, "logrite.callback": callback });
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
		logger.error(`>>> Entering getBasicLoginSettings(cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getBasicLoginSettings", "logrite.cultureCode": cultureCode, "logrite.callback": callback });
		logger.warn(`<<< Exiting getBasicLoginSettings(cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getBasicLoginSettings", "logrite.cultureCode": cultureCode, "logrite.callback": callback });
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
		logger.error(`>>> Entering updateBasicLoginSettings(payload=${payload},callback=${callback})`, { "event.action": "updateBasicLoginSettings", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateBasicLoginSettings(payload=${payload},callback=${callback})`, { "event.action": "updateBasicLoginSettings", "logrite.payload": payload, "logrite.callback": callback });
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
		logger.error(`>>> Entering basicLoginSettingsClientModified(parameter=${parameter})`, { "event.action": "basicLoginSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting basicLoginSettingsClientModified(parameter=${parameter})`, { "event.action": "basicLoginSettingsClientModified", "logrite.parameter": parameter });
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
		logger.error(`>>> Entering memberSettingsClientModified(parameter=${parameter})`, { "event.action": "memberSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting memberSettingsClientModified(parameter=${parameter})`, { "event.action": "memberSettingsClientModified", "logrite.parameter": parameter });
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
		logger.error(`>>> Entering registrationSettingsClientModified(parameter=${parameter})`, { "event.action": "registrationSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting registrationSettingsClientModified(parameter=${parameter})`, { "event.action": "registrationSettingsClientModified", "logrite.parameter": parameter });
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
		logger.error(`>>> Entering sslSettingsClientModified(parameter=${parameter})`, { "event.action": "sslSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting sslSettingsClientModified(parameter=${parameter})`, { "event.action": "sslSettingsClientModified", "logrite.parameter": parameter });
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
		logger.error(`>>> Entering otherSettingsClientModified(parameter=${parameter})`, { "event.action": "otherSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting otherSettingsClientModified(parameter=${parameter})`, { "event.action": "otherSettingsClientModified", "logrite.parameter": parameter });
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
		logger.error(`>>> Entering updateIpFilter(payload=${payload},callback=${callback})`, { "event.action": "updateIpFilter", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateIpFilter(payload=${payload},callback=${callback})`, { "event.action": "updateIpFilter", "logrite.payload": payload, "logrite.callback": callback });
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
		logger.error(`>>> Entering getMemberSettings(callback=${callback})`, { "event.action": "getMemberSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getMemberSettings(callback=${callback})`, { "event.action": "getMemberSettings", "logrite.callback": callback });
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
		logger.error(`>>> Entering updateMemberSettings(payload=${payload},callback=${callback})`, { "event.action": "updateMemberSettings", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateMemberSettings(payload=${payload},callback=${callback})`, { "event.action": "updateMemberSettings", "logrite.payload": payload, "logrite.callback": callback });
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
		logger.error(`>>> Entering getRegistrationSettings(callback=${callback})`, { "event.action": "getRegistrationSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getRegistrationSettings(callback=${callback})`, { "event.action": "getRegistrationSettings", "logrite.callback": callback });
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
		logger.error(`>>> Entering updateRegistrationSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateRegistrationSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateRegistrationSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateRegistrationSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering getSslSettings(callback=${callback})`, { "event.action": "getSslSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getSslSettings(callback=${callback})`, { "event.action": "getSslSettings", "logrite.callback": callback });
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
		logger.error(`>>> Entering updateSslSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSslSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateSslSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSslSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering setAllPagesSecure(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "setAllPagesSecure", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        ApplicationService.setAllPagesSecure(data => {
            if (callback) {
                callback(data);
            }
        }, data => {
            if (failureCallback) {
                failureCallback(data);
				logger.warn(`<<< Exiting setAllPagesSecure(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "setAllPagesSecure", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
            }
        });
    },
    getOtherSettings(callback) {
		logger.error(`>>> Entering getOtherSettings(callback=${callback})`, { "event.action": "getOtherSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getOtherSettings(callback=${callback})`, { "event.action": "getOtherSettings", "logrite.callback": callback });
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
		logger.error(`>>> Entering updateOtherSettings(payload=${payload},callback=${callback})`, { "event.action": "updateOtherSettings", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateOtherSettings(payload=${payload},callback=${callback})`, { "event.action": "updateOtherSettings", "logrite.payload": payload, "logrite.callback": callback });
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
		logger.error(`>>> Entering getSecurityBulletins(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "getSecurityBulletins", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting getSecurityBulletins(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "getSecurityBulletins", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering getSuperuserActivities(callback=${callback})`, { "event.action": "getSuperuserActivities", "logrite.callback": callback });
		logger.warn(`<<< Exiting getSuperuserActivities(callback=${callback})`, { "event.action": "getSuperuserActivities", "logrite.callback": callback });
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
		logger.error(`>>> Entering getAuditCheckResults(callback=${callback})`, { "event.action": "getAuditCheckResults", "logrite.callback": callback });
		logger.warn(`<<< Exiting getAuditCheckResults(callback=${callback})`, { "event.action": "getAuditCheckResults", "logrite.callback": callback });
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
		logger.error(`>>> Entering getAuditCheckResult(id=${id},callback=${callback})`, { "event.action": "getAuditCheckResult", "logrite.id": id, "logrite.callback": callback });
		logger.warn(`<<< Exiting getAuditCheckResult(id=${id},callback=${callback})`, { "event.action": "getAuditCheckResult", "logrite.id": id, "logrite.callback": callback });
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
		logger.error(`>>> Entering searchFileSystemAndDatabase(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "searchFileSystemAndDatabase", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting searchFileSystemAndDatabase(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "searchFileSystemAndDatabase", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
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
		logger.error('>>> Entering clearFileSystemAndDatabaseSearch()', { "event.action": "clearFileSystemAndDatabaseSearch" });
		logger.warn('<<< Exiting clearFileSystemAndDatabaseSearch()', { "event.action": "clearFileSystemAndDatabaseSearch" });
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
		logger.error(`>>> Entering getLastModifiedSettings(callback=${callback})`, { "event.action": "getLastModifiedSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getLastModifiedSettings(callback=${callback})`, { "event.action": "getLastModifiedSettings", "logrite.callback": callback });
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
		logger.error(`>>> Entering getLastModifiedFiles(callback=${callback})`, { "event.action": "getLastModifiedFiles", "logrite.callback": callback });
		logger.warn(`<<< Exiting getLastModifiedFiles(callback=${callback})`, { "event.action": "getLastModifiedFiles", "logrite.callback": callback });
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
		logger.error(`>>> Entering updatefileSystemAndDatabaseSearchKeyword(keyword=${keyword})`, { "event.action": "updatefileSystemAndDatabaseSearchKeyword", "logrite.keyword": keyword });
		logger.warn(`<<< Exiting updatefileSystemAndDatabaseSearchKeyword(keyword=${keyword})`, { "event.action": "updatefileSystemAndDatabaseSearchKeyword", "logrite.keyword": keyword });
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
		logger.error(`>>> Entering updatefileSystemAndDatabaseActiveTab(tab=${tab})`, { "event.action": "updatefileSystemAndDatabaseActiveTab", "logrite.tab": tab });
		logger.warn(`<<< Exiting updatefileSystemAndDatabaseActiveTab(tab=${tab})`, { "event.action": "updatefileSystemAndDatabaseActiveTab", "logrite.tab": tab });
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
		logger.error(`>>> Entering getPortalList(addAll=${addAll},callback=${callback})`, { "event.action": "getPortalList", "logrite.addAll": addAll, "logrite.callback": callback });
		logger.warn(`<<< Exiting getPortalList(addAll=${addAll},callback=${callback})`, { "event.action": "getPortalList", "logrite.addAll": addAll, "logrite.callback": callback });
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
		logger.error(`>>> Entering getApiTokenSettings(callback=${callback})`, { "event.action": "getApiTokenSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getApiTokenSettings(callback=${callback})`, { "event.action": "getApiTokenSettings", "logrite.callback": callback });
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
		logger.error(`>>> Entering updateApiTokenSettings(payload=${payload},callback=${callback})`, { "event.action": "updateApiTokenSettings", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateApiTokenSettings(payload=${payload},callback=${callback})`, { "event.action": "updateApiTokenSettings", "logrite.payload": payload, "logrite.callback": callback });
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
		logger.error(`>>> Entering apiTokenSettingsClientModified(parameter=${parameter})`, { "event.action": "apiTokenSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting apiTokenSettingsClientModified(parameter=${parameter})`, { "event.action": "apiTokenSettingsClientModified", "logrite.parameter": parameter });
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
		logger.error(`>>> Entering getApiTokenKeys(callback=${callback})`, { "event.action": "getApiTokenKeys", "logrite.callback": callback });
		logger.warn(`<<< Exiting getApiTokenKeys(callback=${callback})`, { "event.action": "getApiTokenKeys", "logrite.callback": callback });
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
		logger.error(`>>> Entering deleteExpiredApiTokens(callback=${callback})`, { "event.action": "deleteExpiredApiTokens", "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteExpiredApiTokens(callback=${callback})`, { "event.action": "deleteExpiredApiTokens", "logrite.callback": callback });
        return () => {
            ApplicationService.deleteExpiredApiTokens(data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    deleteExpiredAndRevokedApiTokens(callback) {
		logger.error(`>>> Entering deleteExpiredAndRevokedApiTokens(callback=${callback})`, { "event.action": "deleteExpiredAndRevokedApiTokens", "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteExpiredAndRevokedApiTokens(callback=${callback})`, { "event.action": "deleteExpiredAndRevokedApiTokens", "logrite.callback": callback });
        return () => {
            ApplicationService.deleteExpiredAndRevokedApiTokens(data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getCspSettings(callback) {
		logger.error(`>>> Entering getCspSettings(callback=${callback})`, { "event.action": "getCspSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getCspSettings(callback=${callback})`, { "event.action": "getCspSettings", "logrite.callback": callback });
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
		logger.error(`>>> Entering updateCspSettings(payload=${payload},callback=${callback})`, { "event.action": "updateCspSettings", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateCspSettings(payload=${payload},callback=${callback})`, { "event.action": "updateCspSettings", "logrite.payload": payload, "logrite.callback": callback });
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
		logger.error(`>>> Entering cspSettingsClientModified(parameter=${parameter})`, { "event.action": "cspSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting cspSettingsClientModified(parameter=${parameter})`, { "event.action": "cspSettingsClientModified", "logrite.parameter": parameter });
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
