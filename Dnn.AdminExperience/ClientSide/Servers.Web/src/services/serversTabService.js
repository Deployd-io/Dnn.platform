import serviceFramework from "./serviceFramework";
const logger = console;

const getServers = function () {
	logger.debug('>>> Entering getServers()');
	logger.debug('<<< Exiting getServers()');
    return serviceFramework.get("SystemInfoServers", "GetServers");
};

const deleteServer = function (serverId) {
	logger.debug(`>>> Entering deleteServer(serverId=${serverId})`);
	logger.debug(`<<< Exiting deleteServer(serverId=${serverId})`);
    return serviceFramework.post("SystemInfoServers", "DeleteServer", {
        serverId: serverId
    });
};

const editServerUrl = function (serverId, newUrl) {
	logger.debug(`>>> Entering editServerUrl(serverId=${serverId},newUrl=${newUrl})`);
	logger.debug(`<<< Exiting editServerUrl(serverId=${serverId},newUrl=${newUrl})`);
    return serviceFramework.post("SystemInfoServers", "EditServerUrl", {
        serverId: serverId,
        newUrl: newUrl
    });
};

const deleteNonActiveServers = function () {
	logger.debug('>>> Entering deleteNonActiveServers()');
	logger.debug('<<< Exiting deleteNonActiveServers()');
    return serviceFramework.post("SystemInfoServers", "DeleteNonActiveServers");
};

const serversTabService = {
    getServers: getServers,
    deleteServer: deleteServer,
    editServerUrl: editServerUrl,
    deleteNonActiveServers: deleteNonActiveServers
};

export default serversTabService; 