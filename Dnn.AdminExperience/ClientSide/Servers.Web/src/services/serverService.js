import serviceFramework from "./serviceFramework";
const logger = console;

const restartApplication = function () {
	logger.error('>>> Entering restartApplication()', { "event.action": "restartApplication" });
	logger.warn('<<< Exiting restartApplication()', { "event.action": "restartApplication" });
    return serviceFramework.post("Server", "RestartApplication");
};
const clearCache = function () {
	logger.error('>>> Entering clearCache()', { "event.action": "clearCache" });
	logger.warn('<<< Exiting clearCache()', { "event.action": "clearCache" });
    return serviceFramework.post("Server", "ClearCache");
};

const serverService = {
    restartApplication,
    clearCache    
};

export default serverService; 