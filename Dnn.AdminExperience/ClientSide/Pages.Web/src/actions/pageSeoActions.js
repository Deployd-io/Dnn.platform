import PageSeoService from "../services/pageSeoService";
import { pageSeoTypes as ActionTypes } from "../constants/actionTypes";
import pageActionTypes from "../constants/actionTypes/pageActionTypes";
import utils from "../utils";
const logger = console;
const pageSeoActions = {
    openNewForm() {
		logger.debug('>>> Entering openNewForm()');
		logger.debug('<<< Exiting openNewForm()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_OPEN_NEW_FORM
            });
        };
    },
    closeNewForm() {
		logger.debug('>>> Entering closeNewForm()');
		logger.debug('<<< Exiting closeNewForm()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_CLOSE_NEW_FORM
            });
        };
    },
    openEditForm(url) {
		logger.debug(`>>> Entering openEditForm(url=${url})`);
		logger.debug(`<<< Exiting openEditForm(url=${url})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_OPEN_EDIT_FORM,
                payload: {
                    url
                }
            });
        };
    },
    closeEditForm() {
		logger.debug('>>> Entering closeEditForm()');
		logger.debug('<<< Exiting closeEditForm()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_CLOSE_EDIT_FORM
            });
        };
    },
    change(key, value) {
		logger.debug(`>>> Entering change(key=${key},value=${value})`);
		logger.debug(`<<< Exiting change(key=${key},value=${value})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_CHANGE_URL,
                payload: {
                    key: key,
                    value: value
                }
            });
        };
    },
    addUrl(url, primaryAliasId) {
		logger.debug(`>>> Entering addUrl(url=${url},primaryAliasId=${primaryAliasId})`);
		logger.debug(`<<< Exiting addUrl(url=${url},primaryAliasId=${primaryAliasId})`);
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SEO_ADD_URL
            });

            const { pages } = getState();
            const tabId = pages.selectedPage.tabId;
            PageSeoService.add(url, tabId, primaryAliasId).then((response) => {
                if (!response.Success) {
                    dispatch({
                        type: ActionTypes.ERROR_SEO_ADDING_URL,
                        data: { error: response.ErrorMessage }
                    });
                    utils.notifyError(response.ErrorMessage);

                    // DNN-30998: Adding SuggestedUrl returned by the server in case of validation errors
                    if (response.SuggestedUrlPath)
                    {
                        dispatch({
                            type: ActionTypes.SEO_CHANGE_URL,
                            payload: {
                                key: "path",
                                value: response.SuggestedUrlPath
                            }
                        });
                    }
                    return;
                }

                dispatch({
                    type: ActionTypes.SEO_ADDED_URL
                });

                PageSeoService.get(tabId).then((response) => {
                    dispatch({
                        type: pageActionTypes.ADD_CUSTOM_URL,
                        payload: {
                            pageUrls: response
                        }
                    });
                });
            }).catch((error) => {
				logger.error(`Exception in addUrl(url=${url},primaryAliasId=${primaryAliasId}): (no error binding)`);
                dispatch({
                    type: ActionTypes.ERROR_SEO_ADDING_URL,
                    data: { error }
                });
            });
        };
    },
    saveUrl(url, primaryAliasId) {
		logger.debug(`>>> Entering saveUrl(url=${url},primaryAliasId=${primaryAliasId})`);
		logger.debug(`<<< Exiting saveUrl(url=${url},primaryAliasId=${primaryAliasId})`);
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SEO_SAVE_URL
            });

            const { pages } = getState();
            const tabId = pages.selectedPage.tabId;
            PageSeoService.save(url, tabId, primaryAliasId).then((response) => {
                if (!response.Success) {
                    dispatch({
                        type: ActionTypes.ERROR_SEO_SAVING_URL,
                        data: { error: response.ErrorMessage }
                    });
                    utils.notifyError(response.ErrorMessage);
                    return;
                }

                dispatch({
                    type: ActionTypes.SEO_SAVED_URL
                });

                PageSeoService.get(tabId).then((response) => {
                    dispatch({
                        type: pageActionTypes.REPLACE_CUSTOM_URL,
                        payload: {
                            pageUrls: response
                        }
                    });
                });
            }).catch((error) => {
				logger.error(`Exception in saveUrl(url=${url},primaryAliasId=${primaryAliasId}): (no error binding)`);
                dispatch({
                    type: ActionTypes.ERROR_SEO_SAVING_URL,
                    data: { error: error.ErrorMessage }
                });
            });
        };
    },
    deleteUrl(url) {
		logger.debug(`>>> Entering deleteUrl(url=${url})`);
		logger.debug(`<<< Exiting deleteUrl(url=${url})`);
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SEO_DELETE_URL
            });

            const { pages } = getState();
            const tabId = pages.selectedPage.tabId;
            PageSeoService.delete(url, tabId).then((response) => {
                if (!response.Success) {
                    dispatch({
                        type: ActionTypes.ERROR_SEO_DELETING_URL,
                        data: { error: response.ErrorMessage }
                    });
                    return;
                }

                dispatch({
                    type: ActionTypes.SEO_DELETED_URL
                });

                PageSeoService.get(tabId).then((response) => {
                    dispatch({
                        type: pageActionTypes.DELETE_CUSTOM_URL,
                        payload: {
                            pageUrls: response
                        }
                    });
                });                
            }).catch((error) => {
				logger.error(`Exception in deleteUrl(url=${url}): (no error binding)`);
                dispatch({
                    type: ActionTypes.ERROR_SEO_DELETING_URL,
                    data: { error: error.ErrorMessage }
                });
            });
        };
    }
};

export default pageSeoActions;
