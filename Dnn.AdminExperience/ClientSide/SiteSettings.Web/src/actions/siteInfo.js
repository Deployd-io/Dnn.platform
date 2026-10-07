import { siteInfo as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const siteInfoActions = {
    updatePortalId(portalId) {
		logger.error(`>>> Entering updatePortalId(portalId=${portalId})`, { "event.action": "updatePortalId", "logrite.portalId": portalId });
		logger.warn(`<<< Exiting updatePortalId(portalId=${portalId})`, { "event.action": "updatePortalId", "logrite.portalId": portalId });
        return {
            type: ActionTypes.CHANGED_PORTAL_ID,
            data: {
                portalId
            }
        };
    },
    getPortals(callback) {
		logger.error(`>>> Entering getPortals(callback=${callback})`, { "event.action": "getPortals", "logrite.callback": callback });
		logger.warn(`<<< Exiting getPortals(callback=${callback})`, { "event.action": "getPortals", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getPortals(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTALS,
                    data: {
                        portals: data.Results
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getPortalSettings(portalId, cultureCode, callback) {
		logger.error(`>>> Entering getPortalSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getPortalSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
		logger.warn(`<<< Exiting getPortalSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getPortalSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getPortalSettings(portalId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_PORTAL_SETTINGS,
                    data: {
                        settings: data.Settings,
                        timeZones: data.TimeZones,
                        iconSets: data.IconSets,
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updatePortalSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updatePortalSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updatePortalSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updatePortalSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updatePortalSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updatePortalSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_PORTAL_SETTINGS,
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
    portalSettingsClientModified(parameter) {
		logger.error(`>>> Entering portalSettingsClientModified(parameter=${parameter})`, { "event.action": "portalSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting portalSettingsClientModified(parameter=${parameter})`, { "event.action": "portalSettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_PORTAL_SETTINS_CLIENT_MODIFIED,
                data: {
                    settings: parameter,
                    clientModified: true
                }
            });
        };
    }
};

export default siteInfoActions;
