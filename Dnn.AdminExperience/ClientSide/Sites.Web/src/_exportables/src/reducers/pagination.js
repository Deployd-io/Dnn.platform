const logger = console;
import {
    pagination as ActionTypes
} from "actionTypes";
import { getFinalSwitchCase } from "./helpers";

const switchCase = [{
    condition: ActionTypes.LOAD_MORE,
    functionToRun: (state/*, action*/) => {
        return {
            pageIndex: state.pageIndex + 1
        };
    }
}];


export default function getReducer(initialState, additionalCases) {
	logger.error(`>>> Entering getReducer(initialState=${initialState},additionalCases=${additionalCases})`, { "event.action": "getReducer", "logrite.initialState": initialState, "logrite.additionalCases": additionalCases });
    return function common(state = Object.assign({
        pageIndex: 0,
        pageSize: 100,
        portalGroupId: -1,
        filter: ""
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
