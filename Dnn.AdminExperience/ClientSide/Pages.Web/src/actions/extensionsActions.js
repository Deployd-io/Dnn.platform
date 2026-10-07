import ActionTypes from "../constants/actionTypes/extensionsActionTypes";
const logger = console;

const extensionsActions = {
    registerToolbarComponent(component) {
		logger.error(`>>> Entering registerToolbarComponent(component=${component})`, { "event.action": "registerToolbarComponent", "logrite.component": component });
		logger.warn(`<<< Exiting registerToolbarComponent(component=${component})`, { "event.action": "registerToolbarComponent", "logrite.component": component });
        return {
            type: ActionTypes.REGISTER_TOOLBAR_COMPONENT,
            data: { component }
        };
    },
    registerInContextMenuComponent(component) {
		logger.error(`>>> Entering registerInContextMenuComponent(component=${component})`, { "event.action": "registerInContextMenuComponent", "logrite.component": component });
		logger.warn(`<<< Exiting registerInContextMenuComponent(component=${component})`, { "event.action": "registerInContextMenuComponent", "logrite.component": component });
        return {
            type: ActionTypes.REGISTER_INCONTEXTMENU_COMPONENT,
            data: { component }
        };
    },
    registerMultiplePagesComponent(component) {
		logger.error(`>>> Entering registerMultiplePagesComponent(component=${component})`, { "event.action": "registerMultiplePagesComponent", "logrite.component": component });
		logger.warn(`<<< Exiting registerMultiplePagesComponent(component=${component})`, { "event.action": "registerMultiplePagesComponent", "logrite.component": component });
        return {
            type: ActionTypes.REGISTER_MULTIPLE_PAGES_COMPONENT,
            data: { component }
        };
    },
    registerPageSettingsComponent(component) {
		logger.error(`>>> Entering registerPageSettingsComponent(component=${component})`, { "event.action": "registerPageSettingsComponent", "logrite.component": component });
		logger.warn(`<<< Exiting registerPageSettingsComponent(component=${component})`, { "event.action": "registerPageSettingsComponent", "logrite.component": component });
        return {
            type: ActionTypes.REGISTER_PAGE_SETTINGS_COMPONENT,
            data: { component }
        };
    },
    registerPageDetailFooterComponent(component) {
		logger.error(`>>> Entering registerPageDetailFooterComponent(component=${component})`, { "event.action": "registerPageDetailFooterComponent", "logrite.component": component });
		logger.warn(`<<< Exiting registerPageDetailFooterComponent(component=${component})`, { "event.action": "registerPageDetailFooterComponent", "logrite.component": component });
        return {
            type: ActionTypes.REGISTER_PAGE_DETAILS_FOOTER_COMPONENT,
            data: { component }
        };
    },
    registerSettingsButtonComponent(component) {
		logger.error(`>>> Entering registerSettingsButtonComponent(component=${component})`, { "event.action": "registerSettingsButtonComponent", "logrite.component": component });
		logger.warn(`<<< Exiting registerSettingsButtonComponent(component=${component})`, { "event.action": "registerSettingsButtonComponent", "logrite.component": component });
        return {
            type: ActionTypes.REGISTER_SETTINGS_BUTTON_COMPONENT,
            data: { component }
        };
    },
    registerPageTypeSelectorComponent(component) {
		logger.error(`>>> Entering registerPageTypeSelectorComponent(component=${component})`, { "event.action": "registerPageTypeSelectorComponent", "logrite.component": component });
		logger.warn(`<<< Exiting registerPageTypeSelectorComponent(component=${component})`, { "event.action": "registerPageTypeSelectorComponent", "logrite.component": component });
        return {
            type: ActionTypes.REGISTER_PAGE_TYPE_SELECTOR_COMPONENT,
            data: { component }
        };
    }
};

export default extensionsActions;