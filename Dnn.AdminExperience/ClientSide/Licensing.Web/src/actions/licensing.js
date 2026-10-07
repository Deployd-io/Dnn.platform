import { licensing as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const licensingActions = {
    getProduct(callback) {
		logger.error(`>>> Entering getProduct(callback=${callback})`, { "event.action": "getProduct", "logrite.callback": callback });
		logger.warn(`<<< Exiting getProduct(callback=${callback})`, { "event.action": "getProduct", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getProduct(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LICENSING_PRODUCT,
                    data: {
                        productName: data.Results.Name
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },

    getServerInfo(callback) {
		logger.error(`>>> Entering getServerInfo(callback=${callback})`, { "event.action": "getServerInfo", "logrite.callback": callback });
		logger.warn(`<<< Exiting getServerInfo(callback=${callback})`, { "event.action": "getServerInfo", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getServerInfo(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SERVER_INFO,
                    data: {
                        productVersion: data.ProductVersion
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    }
};

export default licensingActions;
