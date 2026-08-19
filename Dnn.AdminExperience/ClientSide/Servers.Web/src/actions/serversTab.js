import { serversTab as ActionTypes } from "../constants/actionTypes";
import serversTabService from "../services/serversTabService";
import localization from "../localization";
const logger = console;

const serversTabActions = {
    loadServers() {
		logger.debug('>>> Entering loadServers()');
		logger.debug('<<< Exiting loadServers()');
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
		logger.error('Exception in loadServers()');
    },

    deleteServer(serverId) {
		logger.debug(`>>> Entering deleteServer(serverId=${serverId})`);
		logger.debug(`<<< Exiting deleteServer(serverId=${serverId})`);
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
		logger.debug('>>> Entering deleteNonActiveServers()');
		logger.debug('<<< Exiting deleteNonActiveServers()');
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
