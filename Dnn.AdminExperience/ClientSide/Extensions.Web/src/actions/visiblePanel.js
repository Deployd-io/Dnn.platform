const logger = console;
import {visiblePanel as ActionTypes}  from "../constants/actionTypes";
const visiblePanelActions = {
    selectPanel(panel) {
		logger.error(`>>> Entering selectPanel(panel=${panel})`, { "event.action": "selectPanel", "logrite.panel": panel });
		logger.warn(`<<< Exiting selectPanel(panel=${panel})`, { "event.action": "selectPanel", "logrite.panel": panel });
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
