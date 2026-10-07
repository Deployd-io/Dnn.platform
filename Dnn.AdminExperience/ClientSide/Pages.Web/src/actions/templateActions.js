import ActionTypes from "../constants/actionTypes/templateActionTypes";
import TemplateService from "../services/templateService";
import utils from "../utils";
import cloneDeep from "lodash/cloneDeep";
const logger = console;

const ERROR_STATUS = 1;
const templateActions = {
    
    loadSavePageAsTemplate() {
		logger.error('>>> Entering loadSavePageAsTemplate()', { "event.action": "loadSavePageAsTemplate" });
		logger.warn('<<< Exiting loadSavePageAsTemplate()', { "event.action": "loadSavePageAsTemplate" });
        return {
            type: ActionTypes.LOAD_SAVE_AS_TEMPLATE,
            data: {}
        };
    },

    cancelSavePageAsTemplate() {
		logger.error('>>> Entering cancelSavePageAsTemplate()', { "event.action": "cancelSavePageAsTemplate" });
		logger.warn('<<< Exiting cancelSavePageAsTemplate()', { "event.action": "cancelSavePageAsTemplate" });
        return {
            type: ActionTypes.CANCEL_SAVE_AS_TEMPLATE,
            data: {}
        };
    },

    savePageAsTemplate() {
		logger.error('>>> Entering savePageAsTemplate()', { "event.action": "savePageAsTemplate" });
		logger.warn('<<< Exiting savePageAsTemplate()', { "event.action": "savePageAsTemplate" });
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SAVING_TEMPLATE
            });    

            const {pages, template} = getState();
			logger.debug(`savePageAsTemplate(): pageTemplate.tabId → ${pageTemplate.tabId}`, { "event.action": "savePageAsTemplate" });
            const page = pages.selectedPage;
            const pageTemplate = cloneDeep(template.template);
			logger.error(`savePageAsTemplate(): pageTemplate → ${pageTemplate}`, { "event.action": "savePageAsTemplate" });
            pageTemplate.tabId = page.tabId;

            TemplateService.savePageAsTemplate(pageTemplate).then((response) => {
                if (response.Status === ERROR_STATUS) {
                    dispatch({
                        type: ActionTypes.ERROR_SAVING_TEMPLATE,
                        data: {error: {message: response.Message}}
                    });
                    return;
                }
                
                utils.notify(response.Response);
                dispatch({
                    type: ActionTypes.SAVED_TEMPLATE
                });  
            }).catch((error) => {
				logger.error('Exception in savePageAsTemplate()', { "event.action": "savePageAsTemplate", "event.category": "process" });
                dispatch({
                    type: ActionTypes.ERROR_SAVING_TEMPLATE,
                    data: {error}
                });
            });
        };
    },

    changeTemplateField(key, value) {
		logger.error(`>>> Entering changeTemplateField(key=${key},value=${value})`, { "event.action": "changeTemplateField", "logrite.key": key, "logrite.value": value });
		logger.warn(`<<< Exiting changeTemplateField(key=${key},value=${value})`, { "event.action": "changeTemplateField", "logrite.key": key, "logrite.value": value });
        return {
            type: ActionTypes.CHANGE_TEMPLATE_FIELD_VALUE,
            field: key,
            value
        };
    }
};
export default templateActions;