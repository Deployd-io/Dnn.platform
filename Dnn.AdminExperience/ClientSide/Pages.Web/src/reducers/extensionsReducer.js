import ActionTypes from "../constants/actionTypes/extensionsActionTypes";
const logger = console;

export default function extensionsReducer(state = {
    toolbarComponents: [],
    multiplePagesComponents: [],
    pageSettingsComponents: [],
    pageDetailsFooterComponents: [],
    settingsButtonComponents: {},
    pageTypeSelectorComponents: [],
    additionalPanels: [],
    pageInContextComponents: []
}, action) {

    function addComponent(component) {
		logger.error(`>>> Entering addComponent(component=${component})`, { "event.action": "addComponent", "logrite.component": component });
        const newSettingsButtonComponents = { ...state.settingsButtonComponents };
        newSettingsButtonComponents[component.id] = component.component;
		logger.warn(`<<< Exiting addComponent(component=${component})`, { "event.action": "addComponent", "logrite.component": component });
        return newSettingsButtonComponents;
    }
    function addInContextComponent(component) {
		logger.error(`>>> Entering addInContextComponent(component=${component})`, { "event.action": "addInContextComponent", "logrite.component": component });
        let newPageInContextComponents = [...state.pageInContextComponents];
        newPageInContextComponents = newPageInContextComponents.concat(component.component);
		logger.error(`addInContextComponent(component=${component}): newPageInContextComponents → ${newPageInContextComponents}`, { "event.action": "addInContextComponent", "logrite.component": component });
        return newPageInContextComponents;
    }

    function addPageSettingsComponent(component) {
		logger.error(`>>> Entering addPageSettingsComponent(component=${component})`, { "event.action": "addPageSettingsComponent", "logrite.component": component });
        let newPageSettingsComponents = [...state.pageSettingsComponents];
        newPageSettingsComponents = newPageSettingsComponents.concat(component);
		logger.error(`addPageSettingsComponent(component=${component}): newPageSettingsComponents → ${newPageSettingsComponents}`, { "event.action": "addPageSettingsComponent", "logrite.component": component });
        return newPageSettingsComponents;
    }

    switch (action.type) {
        case ActionTypes.REGISTER_TOOLBAR_COMPONENT:
            return {
                ...state,
                toolbarComponents: [...state.toolbarComponents, action.data.component]
            };
        case ActionTypes.REGISTER_MULTIPLE_PAGES_COMPONENT:
            return {
                ...state,
                multiplePagesComponents: [...state.multiplePagesComponents, action.data.component]
            };
        case ActionTypes.REGISTER_PAGE_SETTINGS_COMPONENT:
            return {
                ...state,
                pageSettingsComponent : addPageSettingsComponent(action.data.component)
            };
        case ActionTypes.REGISTER_PAGE_DETAILS_FOOTER_COMPONENT:
            return {
                ...state,
                pageDetailsFooterComponents: [...state.pageDetailsFooterComponents, action.data.component]
            };
        case ActionTypes.REGISTER_SETTINGS_BUTTON_COMPONENT:
            return {
                ...state,
                settingsButtonComponents: addComponent(action.data.component)
            };
        case ActionTypes.REGISTER_PAGE_TYPE_SELECTOR_COMPONENT:
            return {
                ...state,
                pageTypeSelectorComponents: [...state.pageTypeSelectorComponents, action.data.component]
            };
        case ActionTypes.REGISTER_INCONTEXTMENU_COMPONENT:
            return {
                ...state,
                pageInContextComponents: addInContextComponent(action.data.component)
            };
        default:
            return state;
    }
}