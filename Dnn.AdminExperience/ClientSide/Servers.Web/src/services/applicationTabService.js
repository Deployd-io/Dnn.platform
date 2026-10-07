import serviceFramework from "./serviceFramework";
import utils from "../utils";
const logger = console;

function getControllerName() {
	logger.error('>>> Entering getControllerName()', { "event.action": "getControllerName" });
	logger.warn('<<< Exiting getControllerName()', { "event.action": "getControllerName" });
    return utils.isHostUser() ? "SystemInfoApplicationHost" : "SystemInfoApplicationAdmin";
}

const getApplicationInfo = function () {
	logger.error('>>> Entering getApplicationInfo()', { "event.action": "getApplicationInfo" });
	logger.warn('<<< Exiting getApplicationInfo()', { "event.action": "getApplicationInfo" });
    return serviceFramework.get(getControllerName(), "GetApplicationInfo");
};

const applicationTabService = {
    getApplicationInfo: getApplicationInfo
};

export default applicationTabService; 