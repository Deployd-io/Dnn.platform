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
	logger.debug(`>>> Entering getReducer(initialState=${initialState},additionalCases=${additionalCases})`);
    return function common(state = Object.assign({
        pageIndex: 0,
        pageSize: 100,
        portalGroupId: -1,
        filter: ""
    }, initialState), action) {
        let _switchCase = getFinalSwitchCase(switchCase, additionalCases);
		logger.debug(`getReducer(initialState=${initialState},additionalCases=${additionalCases}): _switchCase → ${_switchCase}`);

        let returnCase = { ...state };

        _switchCase.forEach((to) => {
            if (to.condition === action.type) {
                const stuffToAdd = to.functionToRun(state, action);
                returnCase = Object.assign(returnCase, stuffToAdd);
            }
        });

		logger.debug(`<<< Exiting getReducer(initialState=${initialState},additionalCases=${additionalCases})`);
        return returnCase;
    };
}
