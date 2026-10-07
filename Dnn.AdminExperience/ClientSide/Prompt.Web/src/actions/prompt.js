import actionTypes from "constants/actionTypes";
import ApplicationService from "services/applicationService";
const logger = console;

export function getCommandList(payload, callback, errorCallback) {
	logger.error(`>>> Entering getCommandList(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getCommandList", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
	logger.warn(`<<< Exiting getCommandList(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getCommandList", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
	logger.error(`>>> Entering runCommand(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "runCommand", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
	logger.warn(`<<< Exiting runCommand(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "runCommand", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
	logger.error(`>>> Entering runHelpCommand(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "runHelpCommand", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
	logger.warn(`<<< Exiting runHelpCommand(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "runHelpCommand", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
	logger.error(`>>> Entering runLocalCommand(command=${command},output=${output},style=${style})`, { "event.action": "runLocalCommand", "logrite.command": command, "logrite.output": output, "logrite.style": style });
	logger.warn(`<<< Exiting runLocalCommand(command=${command},output=${output},style=${style})`, { "event.action": "runLocalCommand", "logrite.command": command, "logrite.output": output, "logrite.style": style });
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
	logger.error('>>> Entering endPaging()', { "event.action": "endPaging" });
	logger.warn('<<< Exiting endPaging()', { "event.action": "endPaging" });
    return (dispatch) => {
        dispatch({
            type: actionTypes.END_PAGING
        });
    };
}
export function changeUserMode(payload, callback, errorCallback) {
	logger.error(`>>> Entering changeUserMode(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "changeUserMode", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
	logger.warn(`<<< Exiting changeUserMode(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "changeUserMode", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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