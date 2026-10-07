import ActionTypes from "../constants/actionTypes/languagesActionTypes";
const logger = console;

export default function languages(state = {}, action) {
	logger.error(`>>> Entering languages(state=${state},action=${action})`, { "event.action": "languages", "logrite.state": state, "logrite.action": action });
    switch (action.type) {
        case ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_SETTINGS:
            return { ...state,
                isContentLocalizationEnabled: action.data.ContentLocalizationEnabled
            };
        default:
			logger.warn(`<<< Exiting languages(state=${state},action=${action})`, { "event.action": "languages", "logrite.state": state, "logrite.action": action });
            return state;
    }
}
