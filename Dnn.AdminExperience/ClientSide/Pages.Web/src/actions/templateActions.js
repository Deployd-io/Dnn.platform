import ActionTypes from "../constants/actionTypes/templateActionTypes";
import TemplateService from "../services/templateService";
import utils from "../utils";
import cloneDeep from "lodash/cloneDeep";
const logger = console;

const ERROR_STATUS = 1;
const templateActions = {
    
    loadSavePageAsTemplate() {
		logger.debug('>>> Entering loadSavePageAsTemplate()');
		logger.debug('<<< Exiting loadSavePageAsTemplate()');
        return {
            type: ActionTypes.LOAD_SAVE_AS_TEMPLATE,
            data: {}
        };
    },

    cancelSavePageAsTemplate() {
		logger.debug('>>> Entering cancelSavePageAsTemplate()');
		logger.debug('<<< Exiting cancelSavePageAsTemplate()');
        return {
            type: ActionTypes.CANCEL_SAVE_AS_TEMPLATE,
            data: {}
        };
    },

    savePageAsTemplate() {
		logger.debug('>>> Entering savePageAsTemplate()');
		logger.debug('<<< Exiting savePageAsTemplate()');
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SAVING_TEMPLATE
            });    

            const {pages, template} = getState();
            const page = pages.selectedPage;
            const pageTemplate = cloneDeep(template.template);
            pageTemplate.tabId = page.tabId;

			logger.debug(`savePageAsTemplate(): pageTemplate.tabId → ${pageTemplate.tabId}`);
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
				logger.error('Exception in savePageAsTemplate()');
                dispatch({
                    type: ActionTypes.ERROR_SAVING_TEMPLATE,
                    data: {error}
                });
            });
        };
    },

    changeTemplateField(key, value) {
		logger.debug(`>>> Entering changeTemplateField(key=${key},value=${value})`);
		logger.debug(`<<< Exiting changeTemplateField(key=${key},value=${value})`);
        return {
            type: ActionTypes.CHANGE_TEMPLATE_FIELD_VALUE,
            field: key,
            value
        };
    }
};
export default templateActions;