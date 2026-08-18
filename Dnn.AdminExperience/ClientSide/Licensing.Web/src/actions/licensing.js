import { licensing as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const licensingActions = {
    getProduct(callback) {
		logger.debug(`>>> Entering getProduct(callback=${callback})`);
		logger.debug(`<<< Exiting getProduct(callback=${callback})`);
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
		logger.debug(`>>> Entering getServerInfo(callback=${callback})`);
		logger.debug(`<<< Exiting getServerInfo(callback=${callback})`);
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
