const logger = console;
import { visiblePanel as ActionTypes } from "../actionTypes";

const visiblePanelActions = {
    selectPanel(panel) {
		logger.debug(`>>> Entering selectPanel(panel=${panel})`);
		logger.debug(`<<< Exiting selectPanel(panel=${panel})`);
        return dispatch => {
            dispatch({
                type: ActionTypes.SELECT_PANEL,
                payload: {
                    selectedPage: panel
                }
            });
        };
    }
};

export default visiblePanelActions;
