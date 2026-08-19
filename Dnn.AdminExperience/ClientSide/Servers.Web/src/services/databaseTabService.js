import serviceFramework from "./serviceFramework";
const logger = console;

const getDataBaseServerInfo = function () {
	logger.debug('>>> Entering getDataBaseServerInfo()');
	logger.debug('<<< Exiting getDataBaseServerInfo()');
    return serviceFramework.get("SystemInfoDatabase", "GetDatabaseServerInfo");
};

const databaseTabService = {
    getDataBaseServerInfo: getDataBaseServerInfo
};

export default databaseTabService; 