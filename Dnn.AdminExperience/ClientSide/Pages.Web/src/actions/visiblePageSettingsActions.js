import PageSettingsActionTypes from "../constants/actionTypes/visiblePageSettingsActionTypes";
const logger = console;


const visiblePageSettingsActions = {

    showCustomPageSettings(pageSettingsId) {    
		logger.debug(`>>> Entering showCustomPageSettings(pageSettingsId=${pageSettingsId})`);
		logger.debug(`<<< Exiting showCustomPageSettings(pageSettingsId=${pageSettingsId})`);
        return (dispatch) => {
            dispatch({
                type: PageSettingsActionTypes.SHOW_CUSTOM_PAGE_SETTINGS,
                data: { pageSettingsId }
            });
        };
    },
    hideCustomPageSettings() {
		logger.debug('>>> Entering hideCustomPageSettings()');
		logger.debug('<<< Exiting hideCustomPageSettings()');
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