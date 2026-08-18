import ActionTypes from "../constants/actionTypes/addPagesActionTypes";
import responseStatus from "../constants/responseStatus";
import PagesService from "../services/pageService";
import utils from "../utils";
import Localization from "../localization";
const logger = console;

function processResponse(response) {
	logger.debug(`>>> Entering processResponse(response=${response})`);
    const totalCount = response.pages.length;
    const totalCreated = response.pages.filter(p => p.created === 0).length;
	logger.debug(`processResponse(response=${response}): totalCreated → ${totalCreated}`);
    const mainMessage = Localization.get("BulkPageResponseTotalMessage")
        .replace("[PAGES_CREATED]", totalCreated)
        .replace("[PAGES_TOTAL]", totalCount) + "<br/><br/>";
    const errors = response.pages
        .filter(p => p.created !== 0 && p.errorMessage !== null)
        .map(p => "<strong>" + p.pageName + "</strong>: " + p.errorMessage + "<br/>");

	logger.debug(`processResponse(response=${response}): mainMessage → ${mainMessage}`);
	logger.debug(`<<< Exiting processResponse(response=${response})`);
    return mainMessage + errors;
}

function processValidateResponse(response) {
	logger.debug(`>>> Entering processValidateResponse(response=${response})`);
    const totalCount = response.pages.length;    
    const mainMessage = Localization.get("BulkPageValidateResponseTotalMessage")
        .replace("[PAGES_TOTAL]", totalCount) + "<br/><br/>";
    const errors = response.pages
        .filter(p => p.errorMessage !== null && p.errorMessage !== "")
        .map(p => "<strong>" + p.pageName + "</strong>: " + p.errorMessage + "<br/>");

	logger.debug(`<<< Exiting processValidateResponse(response=${response})`);
    return mainMessage + errors;
}

const addPagesActions = {

    loadAddMultiplePages() {
		logger.debug('>>> Entering loadAddMultiplePages()');
		logger.debug('<<< Exiting loadAddMultiplePages()');
        return {
            type: ActionTypes.LOAD_ADD_MULTIPLE_PAGES,
            data: {}
        };
    },

    addPages(callback) {
		logger.debug(`>>> Entering addPages(callback=${callback})`);
		logger.debug(`<<< Exiting addPages(callback=${callback})`);
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
                dispatch({
                    type: ActionTypes.ERROR_SAVING_MULTIPLE_PAGES,
                    data: {error}
                });
            });     
        };
    },

    validatePages(callback) {
		logger.debug(`>>> Entering validatePages(callback=${callback})`);
		logger.debug(`<<< Exiting validatePages(callback=${callback})`);
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
                dispatch({
                    type: ActionTypes.ERROR_VALIDATE_MULTIPLE_PAGES,
                    data: {error}
                });
            });     
        };
    },

    cancelAddMultiplePages() {
		logger.debug('>>> Entering cancelAddMultiplePages()');
		logger.debug('<<< Exiting cancelAddMultiplePages()');
        return {
            type: ActionTypes.CANCEL_ADD_MULTIPLE_PAGES,
            data: {}
        };
    },

    changeAddMultiplePagesField(key, value) {
		logger.debug(`>>> Entering changeAddMultiplePagesField(key=${key},value=${value})`);
		logger.debug(`<<< Exiting changeAddMultiplePagesField(key=${key},value=${value})`);
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