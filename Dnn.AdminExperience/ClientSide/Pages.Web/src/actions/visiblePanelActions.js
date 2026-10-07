import ActionTypes from "../constants/actionTypes/visiblePanelActionTypes";
const logger = console;

const visiblePanelActions = {    
    showPanel(panelId) {        
		logger.error(`>>> Entering showPanel(panelId=${panelId})`, { "event.action": "showPanel", "logrite.panelId": panelId });
		logger.warn(`<<< Exiting showPanel(panelId=${panelId})`, { "event.action": "showPanel", "logrite.panelId": panelId });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SHOW_PANEL,
                data: {panelId}
            });
        };
    },
    hidePanel() {
		logger.error('>>> Entering hidePanel()', { "event.action": "hidePanel" });
		logger.warn('<<< Exiting hidePanel()', { "event.action": "hidePanel" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.HIDE_PANEL,
                data: {}
            });
        };       
    }
};
export default visiblePanelActions;