import {
    portal as ActionTypes
} from "dnn-sites-common-action-types";
import {
    PortalService
} from "services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.debug(`>>> Entering errorCallback(message=${message})`);
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.debug(`<<< Exiting errorCallback(message=${message})`);
}
const portalActions = {
    loadPortals(searchParameters, callback) {
		logger.debug(`>>> Entering loadPortals(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting loadPortals(searchParameters=${searchParameters},callback=${callback})`);
        return (dispatch) => {
            PortalService.getPortals(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTALS,
                    payload: {
                        portals: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default portalActions;
