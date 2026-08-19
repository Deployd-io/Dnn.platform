import serviceFramework from "./serviceFramework";
const logger = console;

const getWebServerInfo = function () {
	logger.debug('>>> Entering getWebServerInfo()');
	logger.debug('<<< Exiting getWebServerInfo()');
    return serviceFramework.get("SystemInfoWeb", "GetWebServerInfo");
};

const webTabService = {
    getWebServerInfo: getWebServerInfo
};

export default webTabService; 