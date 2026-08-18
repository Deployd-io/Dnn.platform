import utilities from "../utils";
import PageHierarchyActions from "../actions/pageHierarchyActions";
import ExtensionsActions from "../actions/extensionsActions";
import PageActions from "../actions/pageActions";
import utils from "../utils";
import { languagesActions } from "../actions/index";
import "../less/style.less";
const logger = console;

const application = {
    init(initCallback) {
	logger.debug(`>>> Entering init(initCallback=${initCallback})`);

        const options = window.dnn[initCallback]();
		logger.debug(`init(initCallback=${initCallback}): options → ${options}`);
        utilities.init(options);
        if (window.dnn.pages.itemTemplate) {
            application.dispatch(PageHierarchyActions.setItemTemplate(window.dnn.pages.itemTemplate));
			logger.debug(`<<< Exiting init(initCallback=${initCallback})`);
        }
    },
    load(options) {
        utilities.load(options);
        const viewName = utils.getViewName();

        //Check if page translation is enabled 
        application.dispatch(languagesActions.getContentLocalizationEnabled());

        if (viewName === "edit") {
            application.dispatch(PageActions.loadPage(utils.getCurrentPageId()));
        }
		logger.debug(`<<< Exiting load(options=${options})`);
    },
    dispatch() {
		logger.debug('>>> Entering dispatch()');
		logger.debug('<<< Exiting dispatch()');
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    },
    setItemTemplate(template) {
		logger.debug(`>>> Entering setItemTemplate(template=${template})`);
        application.dispatch(PageHierarchyActions.setItemTemplate(template));
		logger.debug(`<<< Exiting setItemTemplate(template=${template})`);
    },
    setDragItemTemplate(template) {
		logger.debug(`>>> Entering setDragItemTemplate(template=${template})`);
        application.dispatch(PageHierarchyActions.setDragItemTemplate(template));
		logger.debug(`<<< Exiting setDragItemTemplate(template=${template})`);
    },
    registerToolbarComponent(component) {
		logger.debug(`>>> Entering registerToolbarComponent(component=${component})`);
        application.dispatch(ExtensionsActions.registerToolbarComponent(component));
		logger.debug(`<<< Exiting registerToolbarComponent(component=${component})`);
    },
    registerInContextMenuComponent(component) {
		logger.debug(`>>> Entering registerInContextMenuComponent(component=${component})`);
        application.dispatch(ExtensionsActions.registerInContextMenuComponent(component));
		logger.debug(`<<< Exiting registerInContextMenuComponent(component=${component})`);
    },
    registerPageSettingsComponent(component) {
		logger.debug(`>>> Entering registerPageSettingsComponent(component=${component})`);
        application.dispatch(ExtensionsActions.registerPageSettingsComponent(component));
		logger.debug(`<<< Exiting registerPageSettingsComponent(component=${component})`);
    },
    registerPageDetailFooterComponent(component) {
		logger.debug(`>>> Entering registerPageDetailFooterComponent(component=${component})`);
        application.dispatch(ExtensionsActions.registerPageDetailFooterComponent(component));
		logger.debug(`<<< Exiting registerPageDetailFooterComponent(component=${component})`);
    },
    registerMultiplePagesComponent(component) {
		logger.debug(`>>> Entering registerMultiplePagesComponent(component=${component})`);
        application.dispatch(ExtensionsActions.registerMultiplePagesComponent(component));
		logger.debug(`<<< Exiting registerMultiplePagesComponent(component=${component})`);
    },
    registerSettingsButtonComponent(component) {
		logger.debug(`>>> Entering registerSettingsButtonComponent(component=${component})`);
        application.dispatch(ExtensionsActions.registerSettingsButtonComponent(component));
		logger.debug(`<<< Exiting registerSettingsButtonComponent(component=${component})`);
    },
    registerPageTypeSelectorComponent(component) {
		logger.debug(`>>> Entering registerPageTypeSelectorComponent(component=${component})`);
        application.dispatch(ExtensionsActions.registerPageTypeSelectorComponent(component));
		logger.debug(`<<< Exiting registerPageTypeSelectorComponent(component=${component})`);
    },
    isSuperUserForPages() {
		logger.debug('>>> Entering isSuperUserForPages()');
		logger.debug('<<< Exiting isSuperUserForPages()');
        return utilities.getIsSuperUser();
    },
    getProductSKU() {
		logger.debug('>>> Entering getProductSKU()');
		logger.debug('<<< Exiting getProductSKU()');
        return utilities.getProductSKU();
    }
};

export default application;