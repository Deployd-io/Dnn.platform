import actionTypes from "constants/actionTypes";
import ApplicationService from "services/applicationService";
const logger = console;

export function getCommandList(payload, callback, errorCallback) {
	logger.debug(`>>> Entering getCommandList(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
	logger.debug(`<<< Exiting getCommandList(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    return (dispatch) => {
        ApplicationService.runCommand(payload, data => {
            dispatch({
                type: actionTypes.RETRIEVED_COMMAND_LIST,
                data: {
                    commands: data.data
                }
            });
            if (callback) {
                callback(data.data);
            }
        }, data => {
            if (errorCallback) {
                errorCallback(data);
            }
        });
    };
}
export function runCommand(payload, callback, errorCallback) {
	logger.debug(`>>> Entering runCommand(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
	logger.debug(`<<< Exiting runCommand(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    return (dispatch) => {
        ApplicationService.runCommand(payload, data => {
            dispatch({
                type: actionTypes.EXECUTED_COMMAND,
                data: {
                    result: data
                }
            });
            if (callback) {
                callback(data);
            }
        }, data => {
            if (errorCallback) {
                errorCallback(data);
            }
        });
    };
}
export function runHelpCommand(payload, callback, errorCallback) {
	logger.debug(`>>> Entering runHelpCommand(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
	logger.debug(`<<< Exiting runHelpCommand(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    return (dispatch) => {
        ApplicationService.runCommand(payload, data => {
            dispatch({
                type: actionTypes.EXECUTED_HELP_COMMAND,
                data: {
                    result: data
                }
            });
            if (callback) {
                callback(data);
            }
        }, data => {
            if (errorCallback) {
                errorCallback(data);
            }
        });
    };
}
export function runLocalCommand(command, output, style) {
	logger.debug(`>>> Entering runLocalCommand(command=${command},output=${output},style=${style})`);
	logger.debug(`<<< Exiting runLocalCommand(command=${command},output=${output},style=${style})`);
    return (dispatch) => {
        dispatch({
            type: actionTypes.EXECUTED_LOCAL_COMMAND,
            command: command,
            output: output,
            style: style
        });
    };
}
export function endPaging() {
	logger.debug('>>> Entering endPaging()');
	logger.debug('<<< Exiting endPaging()');
    return (dispatch) => {
        dispatch({
            type: actionTypes.END_PAGING
        });
    };
}
export function changeUserMode(payload, callback, errorCallback) {
	logger.debug(`>>> Entering changeUserMode(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
	logger.debug(`<<< Exiting changeUserMode(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    return (dispatch) => {
        ApplicationService.changeUserMode(payload, data => {
            dispatch({
                type: actionTypes.CHANGE_USER_MODE,
                data: {
                    result: data
                }
            });
            if (callback) {
                callback(data);
            }
        }, data => {
            if (errorCallback) {
                errorCallback(data);
            }
        });
    };
}