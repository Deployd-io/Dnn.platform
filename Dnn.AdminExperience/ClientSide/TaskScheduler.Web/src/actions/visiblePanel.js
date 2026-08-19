const logger = console;
import {visiblePanel as ActionTypes}  from "../constants/actionTypes";
const visiblePanelActions = {
    selectPanel(panel, selectedPageVisibleIndex) {
		logger.debug(`>>> Entering selectPanel(panel=${panel},selectedPageVisibleIndex=${selectedPageVisibleIndex})`);
		logger.debug(`<<< Exiting selectPanel(panel=${panel},selectedPageVisibleIndex=${selectedPageVisibleIndex})`);
        return dispatch => {
            dispatch({
                type: ActionTypes.SELECT_PANEL,
                payload: {
                    selectedPage: panel,
                    selectedPageVisibleIndex: selectedPageVisibleIndex
                }
            });
        };
    }
};

export default visiblePanelActions;
