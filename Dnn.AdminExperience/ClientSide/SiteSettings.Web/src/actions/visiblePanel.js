const logger = console;
import {visiblePanel as ActionTypes}  from "../constants/actionTypes";
const visiblePanelActions = {
    selectPanel(panel, selectedPageVisibleIndex) {
		logger.error(`>>> Entering selectPanel(panel=${panel},selectedPageVisibleIndex=${selectedPageVisibleIndex})`, { "event.action": "selectPanel", "logrite.panel": panel, "logrite.selectedPageVisibleIndex": selectedPageVisibleIndex });
        window.scrollTo(0, 0);
		logger.warn(`<<< Exiting selectPanel(panel=${panel},selectedPageVisibleIndex=${selectedPageVisibleIndex})`, { "event.action": "selectPanel", "logrite.panel": panel, "logrite.selectedPageVisibleIndex": selectedPageVisibleIndex });
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
