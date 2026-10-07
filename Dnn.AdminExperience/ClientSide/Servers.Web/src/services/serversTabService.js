import serviceFramework from "./serviceFramework";
const logger = console;

const getServers = function () {
	logger.error('>>> Entering getServers()', { "event.action": "getServers" });
	logger.warn('<<< Exiting getServers()', { "event.action": "getServers" });
    return serviceFramework.get("SystemInfoServers", "GetServers");
};

const deleteServer = function (serverId) {
	logger.error(`>>> Entering deleteServer(serverId=${serverId})`, { "event.action": "deleteServer", "logrite.serverId": serverId });
	logger.warn(`<<< Exiting deleteServer(serverId=${serverId})`, { "event.action": "deleteServer", "logrite.serverId": serverId });
    return serviceFramework.post("SystemInfoServers", "DeleteServer", {
        serverId: serverId
    });
};

const editServerUrl = function (serverId, newUrl) {
	logger.error(`>>> Entering editServerUrl(serverId=${serverId},newUrl=${newUrl})`, { "event.action": "editServerUrl", "logrite.serverId": serverId, "logrite.newUrl": newUrl });
	logger.warn(`<<< Exiting editServerUrl(serverId=${serverId},newUrl=${newUrl})`, { "event.action": "editServerUrl", "logrite.serverId": serverId, "logrite.newUrl": newUrl });
    return serviceFramework.post("SystemInfoServers", "EditServerUrl", {
        serverId: serverId,
        newUrl: newUrl
    });
};

const deleteNonActiveServers = function () {
	logger.error('>>> Entering deleteNonActiveServers()', { "event.action": "deleteNonActiveServers" });
	logger.warn('<<< Exiting deleteNonActiveServers()', { "event.action": "deleteNonActiveServers" });
    return serviceFramework.post("SystemInfoServers", "DeleteNonActiveServers");
};

const serversTabService = {
    getServers: getServers,
    deleteServer: deleteServer,
    editServerUrl: editServerUrl,
    deleteNonActiveServers: deleteNonActiveServers
};

export default serversTabService; 