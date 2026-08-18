import ActionTypes from "../constants/actionTypes/languagesActionTypes";
const logger = console;

export default function languages(state = {}, action) {
	logger.debug(`>>> Entering languages(state=${state},action=${action})`);
    switch (action.type) {
        case ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_SETTINGS:
            return { ...state,
                isContentLocalizationEnabled: action.data.ContentLocalizationEnabled
            };
        default:
			logger.debug(`<<< Exiting languages(state=${state},action=${action})`);
            return state;
    }
}
