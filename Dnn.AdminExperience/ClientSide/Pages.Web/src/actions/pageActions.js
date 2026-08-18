import ActionTypes from "../constants/actionTypes/pageActionTypes";
import PageListActionTypes from "../constants/actionTypes/pageListActionTypes";
import SearchListActionTypes from "../constants/actionTypes/searchListActionTypes";
import responseStatus from "../constants/responseStatus";
import PagesService from "../services/pageService";
import utils from "../utils";
import Localization from "../localization";
import cloneDeep from "lodash/cloneDeep";
import Promise from "promise";
const logger = console;

const loadPage = function (dispatch, pageId, callback) {
    return new Promise((resolve) => {
        const currentPageId = utils.getCurrentPageId();
        if (pageId === currentPageId && !utils.getCurrentPagePermissions().managePage) {
            let permissions = utils.getCurrentPagePermissions();
            dispatch({
                type: ActionTypes.LOADED_PAGE,
                data: {
                    page: {
                        tabId: pageId,
                        isOpen: false,
                        hasChild: utils.getCurrentParentHasChildren(),
                        canManagePage: permissions.managePage,
                        canAddContentToPage: permissions.addContentToPage,
                        canViewPage: true,
                        canAddPage: permissions.addPage,
                        canAdminPage: permissions.adminPage,
                        canCopyPage: permissions.copyPage,
                        canDeletePage: permissions.deletePage,
                        canNavigateToPage: true,
                        name: utils.getCurrentPageName()
                    }
                },
                selectedPageSettingTab: 0
            });
        }
        else {
            PagesService.getPage(pageId).then(response => {
                dispatch({
                    type: ActionTypes.LOADED_PAGE,
                    data: {
                        page: response
                    },
                    selectedPageSettingTab: 0
                });
                if (callback) {
                    callback(response);
                }
                resolve(response);
            }).catch((error) => {
				logger.error(`Exception in loadPage(dispatch=${dispatch},pageId=${pageId},callback=${callback}): (no error binding)`);
                dispatch({
                    type: ActionTypes.ERROR_LOADING_PAGE,
                    data: { error }
                });
                resolve();
            });
        }
    });
	logger.debug(`<<< Exiting loadPage(dispatch=${dispatch},pageId=${pageId},callback=${callback})`);
};

const redirectFromNonExistingPage = function (redirectUrl) {
	logger.debug(`>>> Entering redirectFromNonExistingPage(redirectUrl=${redirectUrl})`);
    window.top.location.href = redirectUrl ? redirectUrl : utils.getDefaultPageUrl();
	logger.debug(`<<< Exiting redirectFromNonExistingPage(redirectUrl=${redirectUrl})`);
};

const pageActions = {
    getPageList(id) {
		logger.debug(`>>> Entering getPageList(id=${id})`);
		logger.debug(`<<< Exiting getPageList(id=${id})`);
        return (dispatch) => PagesService.getPageList(id).then(pageList => {

            dispatch({
                type: PageListActionTypes.SAVE,
                data: { pageList }
            });
        });
    },

    searchPageList(searchKey) {
		logger.debug(`>>> Entering searchPageList(searchKey=${searchKey})`);
		logger.debug(`<<< Exiting searchPageList(searchKey=${searchKey})`);
        return (dispatch) => PagesService.searchPageList(searchKey).then((searchList) => {
            dispatch({
                type: SearchListActionTypes.SAVE_SEARCH_LIST,
                data: { searchList }
            });
        });
    },


    getPageHierarchy(id) {
		logger.debug(`>>> Entering getPageHierarchy(id=${id})`);
		logger.debug(`<<< Exiting getPageHierarchy(id=${id})`);
        return () => PagesService.getPageHierarchy(id);
    },

    searchAndFilterPageList(params) {
		logger.debug(`>>> Entering searchAndFilterPageList(params=${params})`);
		logger.debug(`<<< Exiting searchAndFilterPageList(params=${params})`);
        return (dispatch) => PagesService.searchAndFilterPageList(params).then((searchList) => {
            searchList = searchList.Results;
            dispatch({
                type: SearchListActionTypes.SAVE_SEARCH_LIST,
                data: { searchList }
            });
        });
    },

    searchAndFilterPagedPageList(params,filtersUpdated) {
		logger.debug(`>>> Entering searchAndFilterPagedPageList(params=${params},filtersUpdated=${filtersUpdated})`);
		logger.debug(`<<< Exiting searchAndFilterPagedPageList(params=${params},filtersUpdated=${filtersUpdated})`);
        return (dispatch) => PagesService.searchAndFilterPageList(params).then(searchResult => {
            dispatch({
                type: SearchListActionTypes.SAVE_SEARCH_RESULT,
                data: { 
                    searchResult,
                    filtersUpdated 
                }
            });
        });
    },

    getWorkflowsList() {
		logger.debug('>>> Entering getWorkflowsList()');
		logger.debug('<<< Exiting getWorkflowsList()');
        return (dispatch) => PagesService.getWorkflowsList().then(workflowList => {
            dispatch({
                type: ActionTypes.GET_WORKFLOW_LIST,
                data: { workflowList }
            });
        });
    },

    getPage(id) {
		logger.debug(`>>> Entering getPage(id=${id})`);
		logger.debug(`<<< Exiting getPage(id=${id})`);
        return () => PagesService.getPage(id);
    },

    getChildPageList(id) {
		logger.debug(`>>> Entering getChildPageList(id=${id})`);
		logger.debug(`<<< Exiting getChildPageList(id=${id})`);
        return () => PagesService.getChildPageList(id);
    },

    updatePageListStore(pageList) {
		logger.debug(`>>> Entering updatePageListStore(pageList=${pageList})`);
		logger.debug(`<<< Exiting updatePageListStore(pageList=${pageList})`);
        return (dispatch) => {
            dispatch({
                type: PageListActionTypes.SAVE,
                data: { pageList }
            });
        };
    },

    selectPageSettingTab(selectedPageSettingTab) {
		logger.debug(`>>> Entering selectPageSettingTab(selectedPageSettingTab=${selectedPageSettingTab})`);
		logger.debug(`<<< Exiting selectPageSettingTab(selectedPageSettingTab=${selectedPageSettingTab})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECT_PAGE_SETTING_TAB,
                selectedPageSettingTab
            });
        };
    },

    loadPage(pageId) {
        return (dispatch) => {
            return loadPage(dispatch, pageId);
        };
    },

    viewPage(id, url, callback) {
		logger.debug(`>>> Entering viewPage(id=${id},url=${url},callback=${callback})`);
        PagesService.openPageInEditMode(id, url, callback);
		logger.debug(`<<< Exiting viewPage(id=${id},url=${url},callback=${callback})`);
        return () => {

        };
    },

    duplicatePage() {
		logger.debug('>>> Entering duplicatePage()');
		logger.debug('<<< Exiting duplicatePage()');
        return (dispatch, getState) => {
            const { pages } = getState();
            const duplicate = (page) => {
                const duplicatedPage = cloneDeep(page);

                duplicatedPage.templateTabId = duplicatedPage.tabId;
				logger.debug(`duplicatePage(): duplicatedPage.templateTabId → ${duplicatedPage.templateTabId}`);
				logger.debug(`duplicatePage(): duplicatedPage.name → ${duplicatedPage.name}`);
                duplicatedPage.tabId = 0;
				logger.debug(`duplicatePage(): duplicatedPage.tabId → ${duplicatedPage.tabId}`);
                duplicatedPage.name = "";
                duplicatedPage.url = "";
                duplicatedPage.isCopy = true;

                dispatch({
                    type: ActionTypes.LOADED_PAGE,
                    data: {
                        page: duplicatedPage
                    },
                    selectedPageSettingTab: 0
                });
            };

            return loadPage(dispatch, pages.selectedPage.tabId, (page) => {
                duplicate(page);
            });
        };
    },


    getNewPage(parentPage) {
		logger.debug(`>>> Entering getNewPage(parentPage=${parentPage})`);
		logger.debug(`<<< Exiting getNewPage(parentPage=${parentPage})`);
        return (dispatch) => PagesService.getNewPage(parentPage).then((page) => {
            page.referralTabId = parentPage.id;
            dispatch({
                type: ActionTypes.LOADED_PAGE,
                data: { page },
                selectedPageSettingTab: 0
            });
        });
    },

    cancelPage(reloadPageId) {
		logger.debug(`>>> Entering cancelPage(reloadPageId=${reloadPageId})`);
        if (reloadPageId) {
            return (dispatch) => {
                return loadPage(dispatch, reloadPageId);
            };
        }
		logger.debug(`<<< Exiting cancelPage(reloadPageId=${reloadPageId})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCEL_PAGE,
                data: {}
            });
        };
    },

    deletePage(page, redirectUrl) {
        
        return (dispatch) => {
            dispatch({
                type: ActionTypes.DELETE_PAGE
            });

            PagesService.deletePage(page).then(response => {

                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return;
                }

                dispatch({
                    type: ActionTypes.DELETED_PAGE
                });
                if (page.tabId !== 0 && (page.tabId === utils.getCurrentPageId()) || redirectUrl) {
                    redirectFromNonExistingPage(redirectUrl);
                }
                else if (page.hasChild === true) {
                    let currentPageId = utils.getCurrentPageId();
                    PagesService.getPageHierarchy(currentPageId).then(hierarchy => {
                        if (Array.isArray(hierarchy) && hierarchy.indexOf(page.tabId) > -1) {
                            redirectFromNonExistingPage(redirectUrl);
                        }
                    });
                }
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_DELETING_PAGE,
                    data: { error }
                });
            });
        };
		logger.debug(`<<< Exiting deletePage(page=${page},redirectUrl=${redirectUrl})`);
    },
    deleteLocalizePage(page) {
        return (dispatch) => {
            PagesService.deletePage(page, true).then(response => {
                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return;
                }
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_DELETING_PAGE,
                    data: { error }
                });
            });
        };
		logger.debug(`<<< Exiting deleteLocalizePage(page=${page})`);
    },
    createPage(callback) {
	logger.debug(`>>> Entering createPage(callback=${callback})`);

		logger.debug(`<<< Exiting createPage(callback=${callback})`);
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.SAVE_PAGE
            });
            const { pages } = getState();
            const selectedPage = pages.selectedPage;

            PagesService.savePage(selectedPage).then(response => {

                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return;
                }

                if (selectedPage.tabId > 0) {
                    utils.notify(Localization.get("PageUpdatedMessage"));
                }
                if (response.Page.canAddContentToPage && selectedPage.pageType === "normal" && !selectedPage.disableLink) {
                    PagesService.openPageInEditMode(response.Page.id, response.Page.url);
                }
                else if (typeof callback === "function") {
                    utils.notify(Localization.get("PageCreatedMessage"));
                    callback(response.Page);
                }
            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SAVING_PAGE,
                    data: { error }
                });
            });
        };
    },

    updatePage(page, callback) {
        return (dispatch) => {

            PagesService.savePage(page).then(response => {

                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return;
                }

                if (page.tabId > 0) {
                    utils.notify(Localization.get("PageUpdatedMessage"));
                }

                dispatch({
                    type: ActionTypes.SAVE_PAGE,
                    data: response.Page
                });

                callback(response.Page);

            }).catch((error) => {
                dispatch({
                    type: ActionTypes.ERROR_SAVING_PAGE,
                    data: { error }
                });
            });
        };
    },


    changePageField(key, value) {
		logger.debug(`>>> Entering changePageField(key=${key},value=${value})`);
		logger.debug(`<<< Exiting changePageField(key=${key},value=${value})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_FIELD_VALUE,
                field: key,
                value
            });
        };
    },

    changePageType(value) {
		logger.debug(`>>> Entering changePageType(value=${value})`);
		logger.debug(`<<< Exiting changePageType(value=${value})`);
        return {
            type: ActionTypes.CHANGE_FIELD_VALUE,
            field: "pageType",
            value
        };
    },

    changePermissions(permissions) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_PERMISSIONS,
                permissions
            });
        };
		logger.debug(`<<< Exiting changePermissions(permissions=${permissions})`);
    },

    fetchCacheProviderList() {
		logger.debug('>>> Entering fetchCacheProviderList()');
		logger.debug('<<< Exiting fetchCacheProviderList()');
        return (dispatch, getState) => {
            if (!getState().pages.cacheProviderList) {
                dispatch({
                    type: ActionTypes.FETCH_CACHE_PROVIDER_LIST
                });

                PagesService.getCacheProviderList().then(cacheProviderList => {
                    dispatch({
                        type: ActionTypes.FETCHED_CACHE_PROVIDER_LIST,
                        data: { cacheProviderList }
                    });
                }).catch((error) => {
					logger.error('Exception in fetchCacheProviderList()');
                    dispatch({
                        type: ActionTypes.ERROR_FETCHING_CACHE_PROVIDER_LIST,
                        data: { error }
                    });
                });
            }
        };
    },

    deletePageModule(module) {
		logger.debug(`>>> Entering deletePageModule(module=${module})`);
		logger.debug(`<<< Exiting deletePageModule(module=${module})`);
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.DELETING_PAGE_MODULE
            });

            const pageId = getState().pages.selectedPage.tabId;
            const moduleToDelete = {
                moduleId: module.id,
                pageId
            };
            PagesService.deletePageModule(moduleToDelete).then(() => {
                utils.notify(Localization.get("DeletePageModuleSuccess").replace("[MODULETITLE]", module.title));
                dispatch({
                    type: ActionTypes.DELETED_PAGE_MODULE,
                    data: { module }
                });
            }).catch((error) => {
				logger.error(`Exception in deletePageModule(module=${module}): (no error binding)`);
                dispatch({
                    type: ActionTypes.ERROR_DELETING_PAGE_MODULE,
                    data: { error }
                });
            });
        };
    },

    updatePageModuleCopy(id, key, event) {
		logger.debug(`>>> Entering updatePageModuleCopy(id=${id},key=${key},event=${event})`);
		logger.debug(`<<< Exiting updatePageModuleCopy(id=${id},key=${key},event=${event})`);
        return {
            type: ActionTypes.UPDATED_PAGE_MODULE_COPY,
            data: { id, key, event }
        };
    },

    editingPageModule(module) {
		logger.debug(`>>> Entering editingPageModule(module=${module})`);
		logger.debug(`<<< Exiting editingPageModule(module=${module})`);
        return {
            type: ActionTypes.EDITING_PAGE_MODULE,
            data: { module }
        };
    },

    cancelEditingPageModule() {
		logger.debug('>>> Entering cancelEditingPageModule()');
		logger.debug('<<< Exiting cancelEditingPageModule()');
        return {
            type: ActionTypes.CANCEL_EDITING_PAGE_MODULE,
            data: {}
        };
    },

    copyAppearanceToDescendantPages() {
		logger.debug('>>> Entering copyAppearanceToDescendantPages()');
		logger.debug('<<< Exiting copyAppearanceToDescendantPages()');
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.COPYING_APPEARANCE_TO_DESCENDANT_PAGES
            });

            const state = getState();
            const page = state.pages.selectedPage;
            const { defaultPortalLayout, defaultPortalContainer } = state.theme;
            const theme = {
                skinSrc: page.skinSrc || defaultPortalLayout,
                containerSrc: page.containerSrc || defaultPortalContainer
            };

            if (!theme.skinSrc || !theme.containerSrc) {
                utils.notifyError(Localization.get("PleaseSelectLayoutContainer"));
                return;
            }

            PagesService.copyAppearanceToDescendantPages(page.tabId, theme).then(() => {
                utils.notify(Localization.get("CopyAppearanceToDescendantPagesSuccess"));
                dispatch({
                    type: ActionTypes.COPIED_APPEARANCE_TO_DESCENDANT_PAGES,
                    data: {}
                });
            }).catch((error) => {
				logger.error('Exception in copyAppearanceToDescendantPages()');
                dispatch({
                    type: ActionTypes.ERROR_COPYING_APPEARANCE_TO_DESCENDANT_PAGES,
                    data: { error }
                });
            });
        };
    },

    copyPermissionsToDescendantPages() {
		logger.debug('>>> Entering copyPermissionsToDescendantPages()');
		logger.debug('<<< Exiting copyPermissionsToDescendantPages()');
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.COPYING_PERMISSIONS_TO_DESCENDANT_PAGES
            });

            const page = getState().pages.selectedPage;
            PagesService.copyPermissionsToDescendantPages(page.tabId).then(() => {
                utils.notify(Localization.get("CopyPermissionsToDescendantPagesSuccess"));
                dispatch({
                    type: ActionTypes.COPIED_PERMISSIONS_TO_DESCENDANT_PAGES,
                    data: {}
                });
            }).catch((error) => {
				logger.error('Exception in copyPermissionsToDescendantPages()');
                dispatch({
                    type: ActionTypes.ERROR_COPYING_PERMISSIONS_TO_DESCENDANT_PAGES,
                    data: { error }
                });
            });
        };
    },

    getCachedPageCount(cacheProvider) {
		logger.debug(`>>> Entering getCachedPageCount(cacheProvider=${cacheProvider})`);
		logger.debug(`<<< Exiting getCachedPageCount(cacheProvider=${cacheProvider})`);
        return (dispatch, getState) => {
            const page = getState().pages.selectedPage;
            PagesService.getCachedPageCount(cacheProvider, page.tabId).then(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_CACHED_PAGE_COUNT,
                    data: {
                        cachedPageCount: data.Count
                    }
                });
            });
        };
    },

    clearCache(cacheProvider) {
		logger.debug(`>>> Entering clearCache(cacheProvider=${cacheProvider})`);
		logger.debug(`<<< Exiting clearCache(cacheProvider=${cacheProvider})`);
        return (dispatch, getState) => {
            const page = getState().pages.selectedPage;
            PagesService.clearCache(cacheProvider, page.tabId).then(() => {
                dispatch({
                    type: ActionTypes.CLEARED_CACHED_PAGE,
                    data: {
                        cachedPageCount: 0
                    }
                });
            });
        };
    },

    clearSelectedPage() {
		logger.debug('>>> Entering clearSelectedPage()');
		logger.debug('<<< Exiting clearSelectedPage()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEAR_SELECTED_PAGE,
                data: {}
            });
        };
    },

    movePage({ Action, PageId, ParentId, RelatedPageId }) {
		logger.debug(`>>> Entering movePage(Action=${Action},PageId=${PageId},ParentId=${ParentId},RelatedPageId=${RelatedPageId})`);
		logger.debug(`<<< Exiting movePage(Action=${Action},PageId=${PageId},ParentId=${ParentId},RelatedPageId=${RelatedPageId})`);
        return PagesService.movePage({ Action, PageId, ParentId, RelatedPageId }).catch(() => {
            utils.notifyError(Localization.get("AnErrorOccurred"));
        });
		logger.error(`Exception in movePage(Action=${Action},PageId=${PageId},ParentId=${ParentId},RelatedPageId=${RelatedPageId}): (no error binding)`);
    },

    dirtyCustomDetails() {
		logger.debug('>>> Entering dirtyCustomDetails()');
		logger.debug('<<< Exiting dirtyCustomDetails()');
        return {
            type: ActionTypes.CUSTOM_PAGE_DETAILS_UPDATED,
            data: {}
        };
    }
};

export default pageActions;