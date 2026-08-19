import serviceFramework from "./serviceFramework";
const logger = console;

const restartApplication = function () {
	logger.debug('>>> Entering restartApplication()');
	logger.debug('<<< Exiting restartApplication()');
    return serviceFramework.post("Server", "RestartApplication");
};
const clearCache = function () {
	logger.debug('>>> Entering clearCache()');
	logger.debug('<<< Exiting clearCache()');
    return serviceFramework.post("Server", "ClearCache");
};

const serverService = {
    restartApplication,
    clearCache    
};

export default serverService; 