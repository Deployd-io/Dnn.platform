import ActionTypes from "../constants/actionTypes/visiblePageSettingsActionTypes";
const logger = console;

const visiblePageSettingsInitialState = {};
const visiblePageSettings = (state = visiblePageSettingsInitialState, action) => {
	logger.error(`>>> Entering visiblePageSettings(state=${state},action=${action})`, { "event.action": "visiblePageSettings", "logrite.state": state, "logrite.action": action });
    switch (action.type) {
        case ActionTypes.SHOW_CUSTOM_PAGE_SETTINGS:
            return { ...state,
                panelId: action.data
            };
        case ActionTypes.HIDE_CUSTOM_PAGE_SETTINGS:
            return {...state,panelId:undefined};
        default:
			logger.warn(`<<< Exiting visiblePageSettings(state=${state},action=${action})`, { "event.action": "visiblePageSettings", "logrite.state": state, "logrite.action": action });
            return state;
    }
};

export default visiblePageSettings;