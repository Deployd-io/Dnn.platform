import { siteInfo as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const siteInfoActions = {
    updatePortalId(portalId) {
		logger.debug(`>>> Entering updatePortalId(portalId=${portalId})`);
		logger.debug(`<<< Exiting updatePortalId(portalId=${portalId})`);
        return {
            type: ActionTypes.CHANGED_PORTAL_ID,
            data: {
                portalId
            }
        };
    },
    getPortals(callback) {
		logger.debug(`>>> Entering getPortals(callback=${callback})`);
		logger.debug(`<<< Exiting getPortals(callback=${callback})`);
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
		logger.debug(`>>> Entering getPortalSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
		logger.debug(`<<< Exiting getPortalSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
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
		logger.debug(`>>> Entering updatePortalSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updatePortalSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering portalSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting portalSettingsClientModified(parameter=${parameter})`);
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
