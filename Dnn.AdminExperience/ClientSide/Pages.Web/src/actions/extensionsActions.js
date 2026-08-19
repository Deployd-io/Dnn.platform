import ActionTypes from "../constants/actionTypes/extensionsActionTypes";
const logger = console;

const extensionsActions = {
    registerToolbarComponent(component) {
		logger.debug(`>>> Entering registerToolbarComponent(component=${component})`);
		logger.debug(`<<< Exiting registerToolbarComponent(component=${component})`);
        return {
            type: ActionTypes.REGISTER_TOOLBAR_COMPONENT,
            data: { component }
        };
    },
    registerInContextMenuComponent(component) {
		logger.debug(`>>> Entering registerInContextMenuComponent(component=${component})`);
		logger.debug(`<<< Exiting registerInContextMenuComponent(component=${component})`);
        return {
            type: ActionTypes.REGISTER_INCONTEXTMENU_COMPONENT,
            data: { component }
        };
    },
    registerMultiplePagesComponent(component) {
		logger.debug(`>>> Entering registerMultiplePagesComponent(component=${component})`);
		logger.debug(`<<< Exiting registerMultiplePagesComponent(component=${component})`);
        return {
            type: ActionTypes.REGISTER_MULTIPLE_PAGES_COMPONENT,
            data: { component }
        };
    },
    registerPageSettingsComponent(component) {
		logger.debug(`>>> Entering registerPageSettingsComponent(component=${component})`);
		logger.debug(`<<< Exiting registerPageSettingsComponent(component=${component})`);
        return {
            type: ActionTypes.REGISTER_PAGE_SETTINGS_COMPONENT,
            data: { component }
        };
    },
    registerPageDetailFooterComponent(component) {
		logger.debug(`>>> Entering registerPageDetailFooterComponent(component=${component})`);
		logger.debug(`<<< Exiting registerPageDetailFooterComponent(component=${component})`);
        return {
            type: ActionTypes.REGISTER_PAGE_DETAILS_FOOTER_COMPONENT,
            data: { component }
        };
    },
    registerSettingsButtonComponent(component) {
		logger.debug(`>>> Entering registerSettingsButtonComponent(component=${component})`);
		logger.debug(`<<< Exiting registerSettingsButtonComponent(component=${component})`);
        return {
            type: ActionTypes.REGISTER_SETTINGS_BUTTON_COMPONENT,
            data: { component }
        };
    },
    registerPageTypeSelectorComponent(component) {
		logger.debug(`>>> Entering registerPageTypeSelectorComponent(component=${component})`);
		logger.debug(`<<< Exiting registerPageTypeSelectorComponent(component=${component})`);
        return {
            type: ActionTypes.REGISTER_PAGE_TYPE_SELECTOR_COMPONENT,
            data: { component }
        };
    }
};

export default extensionsActions;