import PageSettingsActionTypes from "../constants/actionTypes/visiblePageSettingsActionTypes";
const logger = console;


const visiblePageSettingsActions = {

    showCustomPageSettings(pageSettingsId) {    
		logger.error(`>>> Entering showCustomPageSettings(pageSettingsId=${pageSettingsId})`, { "event.action": "showCustomPageSettings", "logrite.pageSettingsId": pageSettingsId });
		logger.warn(`<<< Exiting showCustomPageSettings(pageSettingsId=${pageSettingsId})`, { "event.action": "showCustomPageSettings", "logrite.pageSettingsId": pageSettingsId });
        return (dispatch) => {
            dispatch({
                type: PageSettingsActionTypes.SHOW_CUSTOM_PAGE_SETTINGS,
                data: { pageSettingsId }
            });
        };
    },
    hideCustomPageSettings() {
		logger.error('>>> Entering hideCustomPageSettings()', { "event.action": "hideCustomPageSettings" });
		logger.warn('<<< Exiting hideCustomPageSettings()', { "event.action": "hideCustomPageSettings" });
        return (dispatch) => {
            dispatch({
                type: PageSettingsActionTypes.HIDE_CUSTOM_PAGE_SETTINGS,
                data: {}
            });
        };       
    }
};
export default visiblePageSettingsActions
;