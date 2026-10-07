import ActionTypes from "../constants/actionTypes/addPagesActionTypes";
import responseStatus from "../constants/responseStatus";
import PagesService from "../services/pageService";
import utils from "../utils";
import Localization from "../localization";
const logger = console;

function processResponse(response) {
	logger.error(`>>> Entering processResponse(response=${response})`, { "event.action": "processResponse", "logrite.response": response });
    const totalCount = response.pages.length;
    const totalCreated = response.pages.filter(p => p.created === 0).length;
	logger.error(`processResponse(response=${response}): totalCreated → ${totalCreated}`, { "event.action": "processResponse", "logrite.response": response });
    const mainMessage = Localization.get("BulkPageResponseTotalMessage")
        .replace("[PAGES_CREATED]", totalCreated)
        .replace("[PAGES_TOTAL]", totalCount) + "<br/><br/>";
    const errors = response.pages
        .filter(p => p.created !== 0 && p.errorMessage !== null)
        .map(p => "<strong>" + p.pageName + "</strong>: " + p.errorMessage + "<br/>");

	logger.error(`processResponse(response=${response}): mainMessage → ${mainMessage}`, { "event.action": "processResponse", "logrite.response": response });
	logger.warn(`<<< Exiting processResponse(response=${response})`, { "event.action": "processResponse", "logrite.response": response });
    return mainMessage + errors;
}

function processValidateResponse(response) {
	logger.error(`>>> Entering processValidateResponse(response=${response})`, { "event.action": "processValidateResponse", "logrite.response": response });
    const totalCount = response.pages.length;    
    const mainMessage = Localization.get("BulkPageValidateResponseTotalMessage")
        .replace("[PAGES_TOTAL]", totalCount) + "<br/><br/>";
    const errors = response.pages
        .filter(p => p.errorMessage !== null && p.errorMessage !== "")
        .map(p => "<strong>" + p.pageName + "</strong>: " + p.errorMessage + "<br/>");

	logger.error(`processValidateResponse(response=${response}): mainMessage → ${mainMessage}`, { "event.action": "processValidateResponse", "logrite.response": response });
	logger.warn(`<<< Exiting processValidateResponse(response=${response})`, { "event.action": "processValidateResponse", "logrite.response": response });
    return mainMessage + errors;
}

const addPagesActions = {

    loadAddMultiplePages() {
		logger.error('>>> Entering loadAddMultiplePages()', { "event.action": "loadAddMultiplePages" });
		logger.warn('<<< Exiting loadAddMultiplePages()', { "event.action": "loadAddMultiplePages" });
        return {
            type: ActionTypes.LOAD_ADD_MULTIPLE_PAGES,
            data: {}
        };
    },

    addPages(callback) {
		logger.error(`>>> Entering addPages(callback=${callback})`, { "event.action": "addPages", "logrite.callback": callback });
		logger.warn(`<<< Exiting addPages(callback=${callback})`, { "event.action": "addPages", "logrite.callback": callback });
        return (dispatch, getState) => {
            const {addPages} = getState();
            dispatch({
                type: ActionTypes.SAVE_ADD_MULTIPLE_PAGES
            });    

            PagesService.addPages(addPages.bulkPage).then(response => {
                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return;
                }

                utils.notify(processResponse(response.Response), 5000);

                dispatch({
                    type: ActionTypes.SAVED_MULTIPLE_PAGES,
                    data: {
                        response 
                    }
                });
                
                callback();

            }).catch((error) => {
				logger.error(`Exception in addPages(callback=${callback}): (no error binding)`, { "event.action": "addPages", "event.category": "process", "logrite.callback": callback });
                dispatch({
                    type: ActionTypes.ERROR_SAVING_MULTIPLE_PAGES,
                    data: {error}
                });
            });     
        };
    },

    validatePages(callback) {
		logger.error(`>>> Entering validatePages(callback=${callback})`, { "event.action": "validatePages", "logrite.callback": callback });
		logger.warn(`<<< Exiting validatePages(callback=${callback})`, { "event.action": "validatePages", "logrite.callback": callback });
        return (dispatch, getState) => {
            const {addPages} = getState();
            dispatch({
                type: ActionTypes.VALIDATE_ADD_MULTIPLE_PAGES
            });    

            PagesService.validatePages(addPages.bulkPage).then(response => {
                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return;
                }
                
                if (response.Response.pages.filter(p => p.errorMessage !== null && p.errorMessage !== "").length > 0) {                    
                    utils.notifyError(processValidateResponse(response.Response), { clickToClose: true });
                }
                else {
                    utils.notify(processValidateResponse(response.Response));
                }

                dispatch({
                    type: ActionTypes.VALIDATE_ADD_MULTIPLE_PAGES,
                    data: {
                        response 
                    }
                });
                
                callback();

            }).catch((error) => {
				logger.error(`Exception in validatePages(callback=${callback}): (no error binding)`, { "event.action": "validatePages", "event.category": "process", "logrite.callback": callback });
                dispatch({
                    type: ActionTypes.ERROR_VALIDATE_MULTIPLE_PAGES,
                    data: {error}
                });
            });     
        };
    },

    cancelAddMultiplePages() {
		logger.error('>>> Entering cancelAddMultiplePages()', { "event.action": "cancelAddMultiplePages" });
		logger.warn('<<< Exiting cancelAddMultiplePages()', { "event.action": "cancelAddMultiplePages" });
        return {
            type: ActionTypes.CANCEL_ADD_MULTIPLE_PAGES,
            data: {}
        };
    },

    changeAddMultiplePagesField(key, value) {
		logger.error(`>>> Entering changeAddMultiplePagesField(key=${key},value=${value})`, { "event.action": "changeAddMultiplePagesField", "logrite.key": key, "logrite.value": value });
		logger.warn(`<<< Exiting changeAddMultiplePagesField(key=${key},value=${value})`, { "event.action": "changeAddMultiplePagesField", "logrite.key": key, "logrite.value": value });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_MULTIPLE_PAGE_VALUE,
                field: key,
                value
            });
        };
    }
};

export default addPagesActions;