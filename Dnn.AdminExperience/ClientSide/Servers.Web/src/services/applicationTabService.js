import serviceFramework from "./serviceFramework";
import utils from "../utils";
const logger = console;

function getControllerName() {
	logger.debug('>>> Entering getControllerName()');
	logger.debug('<<< Exiting getControllerName()');
    return utils.isHostUser() ? "SystemInfoApplicationHost" : "SystemInfoApplicationAdmin";
}

const getApplicationInfo = function () {
	logger.debug('>>> Entering getApplicationInfo()');
	logger.debug('<<< Exiting getApplicationInfo()');
    return serviceFramework.get(getControllerName(), "GetApplicationInfo");
};

const applicationTabService = {
    getApplicationInfo: getApplicationInfo
};

export default applicationTabService; 