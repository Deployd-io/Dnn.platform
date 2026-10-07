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
	logger.error(`>>> Entering init(initCallback=${initCallback})`, { "event.action": "init", "logrite.initCallback": initCallback });

        const options = window.dnn[initCallback]();
		logger.error(`init(initCallback=${initCallback}): options → ${options}`, { "event.action": "init", "logrite.initCallback": initCallback });
        utilities.init(options);
        if (window.dnn.pages.itemTemplate) {
            application.dispatch(PageHierarchyActions.setItemTemplate(window.dnn.pages.itemTemplate));
			logger.warn(`<<< Exiting init(initCallback=${initCallback})`, { "event.action": "init", "logrite.initCallback": initCallback });
        }
    },
    load(options) {
		logger.error(`>>> Entering load(options=${options})`, { "event.action": "load", "logrite.options": options });
        utilities.load(options);
        const viewName = utils.getViewName();
		logger.error(`load(options=${options}): viewName → ${viewName}`, { "event.action": "load", "logrite.options": options });

        //Check if page translation is enabled 
        application.dispatch(languagesActions.getContentLocalizationEnabled());

        if (viewName === "edit") {
            application.dispatch(PageActions.loadPage(utils.getCurrentPageId()));
			logger.warn(`<<< Exiting load(options=${options})`, { "event.action": "load", "logrite.options": options });
        }
    },
    dispatch() {
		logger.error('>>> Entering dispatch()', { "event.action": "dispatch" });
		logger.warn('<<< Exiting dispatch()', { "event.action": "dispatch" });
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    },
    setItemTemplate(template) {
		logger.error(`>>> Entering setItemTemplate(template=${template})`, { "event.action": "setItemTemplate", "logrite.template": template });
        application.dispatch(PageHierarchyActions.setItemTemplate(template));
		logger.warn(`<<< Exiting setItemTemplate(template=${template})`, { "event.action": "setItemTemplate", "logrite.template": template });
    },
    setDragItemTemplate(template) {
		logger.error(`>>> Entering setDragItemTemplate(template=${template})`, { "event.action": "setDragItemTemplate", "logrite.template": template });
        application.dispatch(PageHierarchyActions.setDragItemTemplate(template));
		logger.warn(`<<< Exiting setDragItemTemplate(template=${template})`, { "event.action": "setDragItemTemplate", "logrite.template": template });
    },
    registerToolbarComponent(component) {
		logger.error(`>>> Entering registerToolbarComponent(component=${component})`, { "event.action": "registerToolbarComponent", "logrite.component": component });
        application.dispatch(ExtensionsActions.registerToolbarComponent(component));
		logger.warn(`<<< Exiting registerToolbarComponent(component=${component})`, { "event.action": "registerToolbarComponent", "logrite.component": component });
    },
    registerInContextMenuComponent(component) {
		logger.error(`>>> Entering registerInContextMenuComponent(component=${component})`, { "event.action": "registerInContextMenuComponent", "logrite.component": component });
        application.dispatch(ExtensionsActions.registerInContextMenuComponent(component));
		logger.warn(`<<< Exiting registerInContextMenuComponent(component=${component})`, { "event.action": "registerInContextMenuComponent", "logrite.component": component });
    },
    registerPageSettingsComponent(component) {
		logger.error(`>>> Entering registerPageSettingsComponent(component=${component})`, { "event.action": "registerPageSettingsComponent", "logrite.component": component });
        application.dispatch(ExtensionsActions.registerPageSettingsComponent(component));
		logger.warn(`<<< Exiting registerPageSettingsComponent(component=${component})`, { "event.action": "registerPageSettingsComponent", "logrite.component": component });
    },
    registerPageDetailFooterComponent(component) {
		logger.error(`>>> Entering registerPageDetailFooterComponent(component=${component})`, { "event.action": "registerPageDetailFooterComponent", "logrite.component": component });
        application.dispatch(ExtensionsActions.registerPageDetailFooterComponent(component));
		logger.warn(`<<< Exiting registerPageDetailFooterComponent(component=${component})`, { "event.action": "registerPageDetailFooterComponent", "logrite.component": component });
    },
    registerMultiplePagesComponent(component) {
		logger.error(`>>> Entering registerMultiplePagesComponent(component=${component})`, { "event.action": "registerMultiplePagesComponent", "logrite.component": component });
        application.dispatch(ExtensionsActions.registerMultiplePagesComponent(component));
		logger.warn(`<<< Exiting registerMultiplePagesComponent(component=${component})`, { "event.action": "registerMultiplePagesComponent", "logrite.component": component });
    },
    registerSettingsButtonComponent(component) {
		logger.error(`>>> Entering registerSettingsButtonComponent(component=${component})`, { "event.action": "registerSettingsButtonComponent", "logrite.component": component });
        application.dispatch(ExtensionsActions.registerSettingsButtonComponent(component));
		logger.warn(`<<< Exiting registerSettingsButtonComponent(component=${component})`, { "event.action": "registerSettingsButtonComponent", "logrite.component": component });
    },
    registerPageTypeSelectorComponent(component) {
		logger.error(`>>> Entering registerPageTypeSelectorComponent(component=${component})`, { "event.action": "registerPageTypeSelectorComponent", "logrite.component": component });
        application.dispatch(ExtensionsActions.registerPageTypeSelectorComponent(component));
		logger.warn(`<<< Exiting registerPageTypeSelectorComponent(component=${component})`, { "event.action": "registerPageTypeSelectorComponent", "logrite.component": component });
    },
    isSuperUserForPages() {
		logger.error('>>> Entering isSuperUserForPages()', { "event.action": "isSuperUserForPages" });
		logger.warn('<<< Exiting isSuperUserForPages()', { "event.action": "isSuperUserForPages" });
        return utilities.getIsSuperUser();
    },
    getProductSKU() {
		logger.error('>>> Entering getProductSKU()', { "event.action": "getProductSKU" });
		logger.warn('<<< Exiting getProductSKU()', { "event.action": "getProductSKU" });
        return utilities.getProductSKU();
    }
};

export default application;