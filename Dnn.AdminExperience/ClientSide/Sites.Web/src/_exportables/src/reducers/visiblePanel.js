const logger = console;
import {
    visiblePanel as ActionTypes
} from "actionTypes";
import { getFinalSwitchCase } from "./helpers";

const switchCase = [{
    condition: ActionTypes.SELECT_PANEL,
    functionToRun: (state, action) => {
        return {
            selectedPage: action.payload.selectedPage
        };
    }
}];
export default function getReducer(initialState, additionalCases) {
	logger.error(`>>> Entering getReducer(initialState=${initialState},additionalCases=${additionalCases})`, { "event.action": "getReducer", "logrite.initialState": initialState, "logrite.additionalCases": additionalCases });
    return function common(state = Object.assign({
        selectedPage: 0
    }, initialState), action) {
        let _switchCase = getFinalSwitchCase(switchCase, additionalCases);
		logger.error(`getReducer(initialState=${initialState},additionalCases=${additionalCases}): _switchCase → ${_switchCase}`, { "event.action": "getReducer", "logrite.initialState": initialState, "logrite.additionalCases": additionalCases });

        let returnCase = { ...state };

        _switchCase.forEach((to) => {
            if (to.condition === action.type) {
                const stuffToAdd = to.functionToRun(state, action);
                returnCase = Object.assign(returnCase, stuffToAdd);
            }
        });

        return returnCase;
    };
}
