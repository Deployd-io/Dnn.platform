import utilities from "../utils";
import "../less/style.less";
const logger = console;

const serverTabsList = [];

function init(initCallback) { 
	logger.error(`>>> Entering init(initCallback=${initCallback})`, { "event.action": "init", "logrite.initCallback": initCallback });
    let options = window.dnn[initCallback]();
    utilities.init(options);
	logger.error(`init(initCallback=${initCallback}): options → ${options}`, { "event.action": "init", "logrite.initCallback": initCallback });
    utilities.getPanelIdFromPath = options.utilities.getPanelIdFromPath;
    utilities.updatePanelTabView = options.utilities.updatePanelTabView;
    utilities.panelViewData = options.utilities.panelViewData;
    utilities.path = options.path;
	logger.warn(`<<< Exiting init(initCallback=${initCallback})`, { "event.action": "init", "logrite.initCallback": initCallback });
}

function dispatch() {
	logger.error('>>> Entering dispatch()', { "event.action": "dispatch" });
	logger.warn('<<< Exiting dispatch()', { "event.action": "dispatch" });
    throw new Error("dispatch method needs to be overwritten from the Redux store");
}

function registerServerTab(serverTab) {
	logger.error(`>>> Entering registerServerTab(serverTab=${serverTab})`, { "event.action": "registerServerTab", "logrite.serverTab": serverTab });
    serverTabsList.push(serverTab);

    let panelId = utilities.getPanelIdFromPath(utilities.path);
    utilities.updatePanelTabView(panelId);
	logger.error(`registerServerTab(serverTab=${serverTab}): panelId → ${panelId}`, { "event.action": "registerServerTab", "logrite.serverTab": serverTab });
}

function getRegisteredServerTabs() {
	logger.error('>>> Entering getRegisteredServerTabs()', { "event.action": "getRegisteredServerTabs" });
	logger.warn('<<< Exiting getRegisteredServerTabs()', { "event.action": "getRegisteredServerTabs" });
    return serverTabsList;
}

const application = {
    init,
    dispatch,
    registerServerTab,
    getRegisteredServerTabs
};


export default application;