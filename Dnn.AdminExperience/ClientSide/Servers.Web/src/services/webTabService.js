import serviceFramework from "./serviceFramework";
const logger = console;

const getWebServerInfo = function () {
	logger.error('>>> Entering getWebServerInfo()', { "event.action": "getWebServerInfo" });
	logger.warn('<<< Exiting getWebServerInfo()', { "event.action": "getWebServerInfo" });
    return serviceFramework.get("SystemInfoWeb", "GetWebServerInfo");
};

const webTabService = {
    getWebServerInfo: getWebServerInfo
};

export default webTabService; 