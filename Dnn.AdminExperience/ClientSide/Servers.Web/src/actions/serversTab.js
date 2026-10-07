import { serversTab as ActionTypes } from "../constants/actionTypes";
import serversTabService from "../services/serversTabService";
import localization from "../localization";
const logger = console;

const serversTabActions = {
    loadServers() {
		logger.error('>>> Entering loadServers()', { "event.action": "loadServers" });
		logger.warn('<<< Exiting loadServers()', { "event.action": "loadServers" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_SERVERS,
            });

            serversTabService
                .getServers()
                .then((response) => {
                    dispatch({
                        type: ActionTypes.LOADED_SERVERS,
                        payload: {
                            servers: response,
                        },
                    });
                })
                .catch(() => {
                    dispatch({
                        type: ActionTypes.ERROR_LOADING_SERVERS,
                        payload: {
                            errorMessage: localization.get("errorMessageLoadingServers"),
                        },
                    });
                });
        };
		logger.error('Exception in loadServers()', { "event.action": "loadServers", "event.category": "process" });
    },

    deleteServer(serverId) {
		logger.error(`>>> Entering deleteServer(serverId=${serverId})`, { "event.action": "deleteServer", "logrite.serverId": serverId });
		logger.warn(`<<< Exiting deleteServer(serverId=${serverId})`, { "event.action": "deleteServer", "logrite.serverId": serverId });
        return (dispatch) => {
            serversTabService
                .deleteServer(serverId)
                .then(() => {
                    serversTabActions.loadServers()(dispatch);
                })
                .catch((err) => {
                    // eslint-disable-next-line no-console
                    console.error(err);
                    dispatch({
                        type: ActionTypes.ERROR_DELETING_SERVER,
                        payload: {
                            errorMessage: localization.get("errorMessageDeleteServers"),
                        },
                    });
                });
        };
    },

    deleteNonActiveServers() {
		logger.error('>>> Entering deleteNonActiveServers()', { "event.action": "deleteNonActiveServers" });
		logger.warn('<<< Exiting deleteNonActiveServers()', { "event.action": "deleteNonActiveServers" });
        return (dispatch) => {
            serversTabService
                .deleteNonActiveServers()
                .then(() => {
                    serversTabActions.loadServers()(dispatch);
                })
                .catch(() => {
                    dispatch({
                        type: ActionTypes.ERROR_DELETING_SERVER,
                        payload: {
                            errorMessage: localization.get("errorMessageDeleteServers"),
                        },
                    });
                });
        };
    },
};

export default serversTabActions;
