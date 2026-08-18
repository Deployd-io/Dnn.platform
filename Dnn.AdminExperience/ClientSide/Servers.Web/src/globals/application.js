import utilities from "../utils";
import "../less/style.less";
const logger = console;

const serverTabsList = [];

function init(initCallback) { 
	logger.debug(`>>> Entering init(initCallback=${initCallback})`);
    let options = window.dnn[initCallback]();
    utilities.init(options);
	logger.debug(`init(initCallback=${initCallback}): options → ${options}`);
	logger.debug(`init(initCallback=${initCallback}): utilities.updatePanelTabView → ${utilities.updatePanelTabView}`);
	logger.debug(`init(initCallback=${initCallback}): utilities.panelViewData → ${utilities.panelViewData}`);
	logger.debug(`init(initCallback=${initCallback}): utilities.path → ${utilities.path}`);
    utilities.getPanelIdFromPath = options.utilities.getPanelIdFromPath;
	logger.debug(`init(initCallback=${initCallback}): utilities.getPanelIdFromPath → ${utilities.getPanelIdFromPath}`);
    utilities.updatePanelTabView = options.utilities.updatePanelTabView;
    utilities.panelViewData = options.utilities.panelViewData;
    utilities.path = options.path;
	logger.debug(`<<< Exiting init(initCallback=${initCallback})`);
}

function dispatch() {
	logger.debug('>>> Entering dispatch()');
	logger.debug('<<< Exiting dispatch()');
    throw new Error("dispatch method needs to be overwritten from the Redux store");
}

function registerServerTab(serverTab) {
	logger.debug(`>>> Entering registerServerTab(serverTab=${serverTab})`);
    serverTabsList.push(serverTab);

    let panelId = utilities.getPanelIdFromPath(utilities.path);
    utilities.updatePanelTabView(panelId);
	logger.debug(`registerServerTab(serverTab=${serverTab}): panelId → ${panelId}`);
	logger.debug(`<<< Exiting registerServerTab(serverTab=${serverTab})`);
}

function getRegisteredServerTabs() {
	logger.debug('>>> Entering getRegisteredServerTabs()');
	logger.debug('<<< Exiting getRegisteredServerTabs()');
    return serverTabsList;
}

const application = {
    init,
    dispatch,
    registerServerTab,
    getRegisteredServerTabs
};


export default application;