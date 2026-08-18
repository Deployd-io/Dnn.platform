import ActionTypes from "../constants/actionTypes/visiblePageSettingsActionTypes";
const logger = console;

const visiblePageSettingsInitialState = {};
const visiblePageSettings = (state = visiblePageSettingsInitialState, action) => {
	logger.debug(`>>> Entering visiblePageSettings(state=${state},action=${action})`);
    switch (action.type) {
        case ActionTypes.SHOW_CUSTOM_PAGE_SETTINGS:
            return { ...state,
                panelId: action.data
            };
        case ActionTypes.HIDE_CUSTOM_PAGE_SETTINGS:
            return {...state,panelId:undefined};
        default:
			logger.debug(`<<< Exiting visiblePageSettings(state=${state},action=${action})`);
            return state;
    }
};

export default visiblePageSettings;