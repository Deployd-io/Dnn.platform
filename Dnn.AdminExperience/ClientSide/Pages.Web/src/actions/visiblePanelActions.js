import ActionTypes from "../constants/actionTypes/visiblePanelActionTypes";
const logger = console;

const visiblePanelActions = {    
    showPanel(panelId) {        
		logger.debug(`>>> Entering showPanel(panelId=${panelId})`);
		logger.debug(`<<< Exiting showPanel(panelId=${panelId})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SHOW_PANEL,
                data: {panelId}
            });
        };
    },
    hidePanel() {
		logger.debug('>>> Entering hidePanel()');
		logger.debug('<<< Exiting hidePanel()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.HIDE_PANEL,
                data: {}
            });
        };       
    }
};
export default visiblePanelActions;