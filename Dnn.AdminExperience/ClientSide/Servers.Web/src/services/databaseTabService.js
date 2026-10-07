import serviceFramework from "./serviceFramework";
const logger = console;

const getDataBaseServerInfo = function () {
	logger.error('>>> Entering getDataBaseServerInfo()', { "event.action": "getDataBaseServerInfo" });
	logger.warn('<<< Exiting getDataBaseServerInfo()', { "event.action": "getDataBaseServerInfo" });
    return serviceFramework.get("SystemInfoDatabase", "GetDatabaseServerInfo");
};

const databaseTabService = {
    getDataBaseServerInfo: getDataBaseServerInfo
};

export default databaseTabService; 