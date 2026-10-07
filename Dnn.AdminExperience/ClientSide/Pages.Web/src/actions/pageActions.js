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
	logger.error(`>>> Entering loadPage(dispatch=${dispatch},pageId=${pageId},callback=${callback})`, { "event.action": "loadPage", "logrite.pageId": pageId });
	logger.warn(`<<< Exiting loadPage(dispatch=${dispatch},pageId=${pageId},callback=${callback})`, { "event.action": "loadPage", "logrite.pageId": pageId });
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
				logger.error(`Exception in loadPage(dispatch=${dispatch},pageId=${pageId},callback=${callback}): (no error binding)`, { "event.action": "loadPage", "event.category": "process", "logrite.pageId": pageId });
                dispatch({
                    type: ActionTypes.ERROR_LOADING_PAGE,
                    data: { error }
                });
                resolve();
            });
        }
    });
	logger.error(`>>> Entering loadPage(pageId=${pageId})`, { "event.action": "loadPage", "logrite.pageId": pageId });
	logger.warn(`<<< Exiting loadPage(pageId=${pageId})`, { "event.action": "loadPage", "logrite.pageId": pageId });
};

const redirectFromNonExistingPage = function (redirectUrl) {
	logger.error(`>>> Entering redirectFromNonExistingPage(redirectUrl=${redirectUrl})`, { "event.action": "redirectFromNonExistingPage", "logrite.redirectUrl": redirectUrl });
    window.top.location.href = redirectUrl ? redirectUrl : utils.getDefaultPageUrl();
	logger.warn(`<<< Exiting redirectFromNonExistingPage(redirectUrl=${redirectUrl})`, { "event.action": "redirectFromNonExistingPage", "logrite.redirectUrl": redirectUrl });
};

const pageActions = {
    getPageList(id) {
		logger.error(`>>> Entering getPageList(id=${id})`, { "event.action": "getPageList", "logrite.id": id });
		logger.warn(`<<< Exiting getPageList(id=${id})`, { "event.action": "getPageList", "logrite.id": id });
        return (dispatch) => PagesService.getPageList(id).then(pageList => {

            dispatch({
                type: PageListActionTypes.SAVE,
                data: { pageList }
            });
        });
    },

    searchPageList(searchKey) {
		logger.error(`>>> Entering searchPageList(searchKey=${searchKey})`, { "event.action": "searchPageList", "logrite.searchKey": searchKey });
		logger.warn(`<<< Exiting searchPageList(searchKey=${searchKey})`, { "event.action": "searchPageList", "logrite.searchKey": searchKey });
        return (dispatch) => PagesService.searchPageList(searchKey).then((searchList) => {
            dispatch({
                type: SearchListActionTypes.SAVE_SEARCH_LIST,
                data: { searchList }
            });
        });
    },


    getPageHierarchy(id) {
		logger.error(`>>> Entering getPageHierarchy(id=${id})`, { "event.action": "getPageHierarchy", "logrite.id": id });
		logger.warn(`<<< Exiting getPageHierarchy(id=${id})`, { "event.action": "getPageHierarchy", "logrite.id": id });
        return () => PagesService.getPageHierarchy(id);
    },

    searchAndFilterPageList(params) {
		logger.error(`>>> Entering searchAndFilterPageList(params=${params})`, { "event.action": "searchAndFilterPageList", "logrite.params": params });
		logger.warn(`<<< Exiting searchAndFilterPageList(params=${params})`, { "event.action": "searchAndFilterPageList", "logrite.params": params });
        return (dispatch) => PagesService.searchAndFilterPageList(params).then((searchList) => {
            searchList = searchList.Results;
            dispatch({
                type: SearchListActionTypes.SAVE_SEARCH_LIST,
                data: { searchList }
            });
        });
    },

    searchAndFilterPagedPageList(params,filtersUpdated) {
		logger.error(`>>> Entering searchAndFilterPagedPageList(params=${params},filtersUpdated=${filtersUpdated})`, { "event.action": "searchAndFilterPagedPageList", "logrite.params": params, "logrite.filtersUpdated": filtersUpdated });
		logger.warn(`<<< Exiting searchAndFilterPagedPageList(params=${params},filtersUpdated=${filtersUpdated})`, { "event.action": "searchAndFilterPagedPageList", "logrite.params": params, "logrite.filtersUpdated": filtersUpdated });
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
		logger.error('>>> Entering getWorkflowsList()', { "event.action": "getWorkflowsList" });
		logger.warn('<<< Exiting getWorkflowsList()', { "event.action": "getWorkflowsList" });
        return (dispatch) => PagesService.getWorkflowsList().then(workflowList => {
            dispatch({
                type: ActionTypes.GET_WORKFLOW_LIST,
                data: { workflowList }
            });
        });
    },

    getPage(id) {
		logger.error(`>>> Entering getPage(id=${id})`, { "event.action": "getPage", "logrite.id": id });
		logger.warn(`<<< Exiting getPage(id=${id})`, { "event.action": "getPage", "logrite.id": id });
        return () => PagesService.getPage(id);
    },

    getChildPageList(id) {
		logger.error(`>>> Entering getChildPageList(id=${id})`, { "event.action": "getChildPageList", "logrite.id": id });
		logger.warn(`<<< Exiting getChildPageList(id=${id})`, { "event.action": "getChildPageList", "logrite.id": id });
        return () => PagesService.getChildPageList(id);
    },

    updatePageListStore(pageList) {
		logger.error(`>>> Entering updatePageListStore(pageList=${pageList})`, { "event.action": "updatePageListStore", "logrite.pageList": pageList });
		logger.warn(`<<< Exiting updatePageListStore(pageList=${pageList})`, { "event.action": "updatePageListStore", "logrite.pageList": pageList });
        return (dispatch) => {
            dispatch({
                type: PageListActionTypes.SAVE,
                data: { pageList }
            });
        };
    },

    selectPageSettingTab(selectedPageSettingTab) {
		logger.error(`>>> Entering selectPageSettingTab(selectedPageSettingTab=${selectedPageSettingTab})`, { "event.action": "selectPageSettingTab", "logrite.selectedPageSettingTab": selectedPageSettingTab });
		logger.warn(`<<< Exiting selectPageSettingTab(selectedPageSettingTab=${selectedPageSettingTab})`, { "event.action": "selectPageSettingTab", "logrite.selectedPageSettingTab": selectedPageSettingTab });
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
		logger.error(`>>> Entering viewPage(id=${id},url=${url},callback=${callback})`, { "event.action": "viewPage", "logrite.id": id, "logrite.url": url, "logrite.callback": callback });
        PagesService.openPageInEditMode(id, url, callback);
		logger.warn(`<<< Exiting viewPage(id=${id},url=${url},callback=${callback})`, { "event.action": "viewPage", "logrite.id": id, "logrite.url": url, "logrite.callback": callback });
        return () => {

        };
    },

    duplicatePage() {
		logger.error('>>> Entering duplicatePage()', { "event.action": "duplicatePage" });
        return (dispatch, getState) => {
            const { pages } = getState();
			logger.warn('<<< Exiting duplicatePage()', { "event.action": "duplicatePage" });
            const duplicate = (page) => {
                const duplicatedPage = cloneDeep(page);
				logger.error(`>>> Entering duplicate(page=${page})`, { "event.action": "duplicate", "logrite.page": page });

                duplicatedPage.templateTabId = duplicatedPage.tabId;
                duplicatedPage.tabId = 0;
                duplicatedPage.name = "";
                duplicatedPage.url = "";
				logger.warn(`<<< Exiting duplicate(page=${page})`, { "event.action": "duplicate", "logrite.page": page });
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
		logger.error(`>>> Entering getNewPage(parentPage=${parentPage})`, { "event.action": "getNewPage", "logrite.parentPage": parentPage });
        return (dispatch) => PagesService.getNewPage(parentPage).then((page) => {
            page.referralTabId = parentPage.id;
			logger.warn(`<<< Exiting getNewPage(parentPage=${parentPage})`, { "event.action": "getNewPage", "logrite.parentPage": parentPage });
            dispatch({
                type: ActionTypes.LOADED_PAGE,
                data: { page },
                selectedPageSettingTab: 0
            });
        });
    },

    cancelPage(reloadPageId) {
		logger.error(`>>> Entering cancelPage(reloadPageId=${reloadPageId})`, { "event.action": "cancelPage", "logrite.reloadPageId": reloadPageId });
        if (reloadPageId) {
            return (dispatch) => {
                return loadPage(dispatch, reloadPageId);
            };
        }
		logger.warn(`<<< Exiting cancelPage(reloadPageId=${reloadPageId})`, { "event.action": "cancelPage", "logrite.reloadPageId": reloadPageId });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCEL_PAGE,
                data: {}
            });
        };
    },

    deletePage(page, redirectUrl) {
		logger.error(`>>> Entering deletePage(page=${page},redirectUrl=${redirectUrl})`, { "event.action": "deletePage", "logrite.page": page, "logrite.redirectUrl": redirectUrl });
        
		logger.warn(`<<< Exiting deletePage(page=${page},redirectUrl=${redirectUrl})`, { "event.action": "deletePage", "logrite.page": page, "logrite.redirectUrl": redirectUrl });
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
    },
    deleteLocalizePage(page) {
		logger.error(`>>> Entering deleteLocalizePage(page=${page})`, { "event.action": "deleteLocalizePage", "logrite.page": page });
		logger.warn(`<<< Exiting deleteLocalizePage(page=${page})`, { "event.action": "deleteLocalizePage", "logrite.page": page });
        return (dispatch) => {
            PagesService.deletePage(page, true).then(response => {
                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return;
                }
            }).catch((error) => {
				logger.error(`Exception in deleteLocalizePage(page=${page}): (no error binding)`, { "event.action": "deleteLocalizePage", "event.category": "process", "logrite.page": page });
                dispatch({
                    type: ActionTypes.ERROR_DELETING_PAGE,
                    data: { error }
                });
            });
        };
    },
    createPage(callback) {
	logger.error(`>>> Entering createPage(callback=${callback})`, { "event.action": "createPage", "logrite.callback": callback });

		logger.warn(`<<< Exiting createPage(callback=${callback})`, { "event.action": "createPage", "logrite.callback": callback });
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
		logger.error(`>>> Entering updatePage(page=${page},callback=${callback})`, { "event.action": "updatePage", "logrite.page": page, "logrite.callback": callback });
		logger.warn(`<<< Exiting updatePage(page=${page},callback=${callback})`, { "event.action": "updatePage", "logrite.page": page, "logrite.callback": callback });
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
				logger.error(`Exception in updatePage(page=${page},callback=${callback}): (no error binding)`, { "event.action": "updatePage", "event.category": "process", "logrite.page": page, "logrite.callback": callback });
                dispatch({
                    type: ActionTypes.ERROR_SAVING_PAGE,
                    data: { error }
                });
            });
        };
    },


    changePageField(key, value) {
		logger.error(`>>> Entering changePageField(key=${key},value=${value})`, { "event.action": "changePageField", "logrite.key": key, "logrite.value": value });
		logger.warn(`<<< Exiting changePageField(key=${key},value=${value})`, { "event.action": "changePageField", "logrite.key": key, "logrite.value": value });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_FIELD_VALUE,
                field: key,
                value
            });
        };
    },

    changePageType(value) {
		logger.error(`>>> Entering changePageType(value=${value})`, { "event.action": "changePageType", "logrite.value": value });
		logger.warn(`<<< Exiting changePageType(value=${value})`, { "event.action": "changePageType", "logrite.value": value });
        return {
            type: ActionTypes.CHANGE_FIELD_VALUE,
            field: "pageType",
            value
        };
    },

    changePermissions(permissions) {
		logger.error(`>>> Entering changePermissions(permissions=${permissions})`, { "event.action": "changePermissions", "logrite.permissions": permissions });
		logger.warn(`<<< Exiting changePermissions(permissions=${permissions})`, { "event.action": "changePermissions", "logrite.permissions": permissions });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_PERMISSIONS,
                permissions
            });
        };
    },

    fetchCacheProviderList() {
		logger.error('>>> Entering fetchCacheProviderList()', { "event.action": "fetchCacheProviderList" });
		logger.warn('<<< Exiting fetchCacheProviderList()', { "event.action": "fetchCacheProviderList" });
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
					logger.error('Exception in fetchCacheProviderList()', { "event.action": "fetchCacheProviderList", "event.category": "process" });
                    dispatch({
                        type: ActionTypes.ERROR_FETCHING_CACHE_PROVIDER_LIST,
                        data: { error }
                    });
                });
            }
        };
    },

    deletePageModule(module) {
		logger.error(`>>> Entering deletePageModule(module=${module})`, { "event.action": "deletePageModule", "logrite.module": module });
		logger.warn(`<<< Exiting deletePageModule(module=${module})`, { "event.action": "deletePageModule", "logrite.module": module });
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.DELETING_PAGE_MODULE
            });

            const pageId = getState().pages.selectedPage.tabId;
			logger.error(`deletePageModule(module=${module}): pageId → ${pageId}`, { "event.action": "deletePageModule", "logrite.module": module });
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
				logger.error(`Exception in deletePageModule(module=${module}): (no error binding)`, { "event.action": "deletePageModule", "event.category": "process", "logrite.module": module });
                dispatch({
                    type: ActionTypes.ERROR_DELETING_PAGE_MODULE,
                    data: { error }
                });
            });
        };
    },

    updatePageModuleCopy(id, key, event) {
		logger.error(`>>> Entering updatePageModuleCopy(id=${id},key=${key},event=${event})`, { "event.action": "updatePageModuleCopy", "logrite.id": id, "logrite.key": key, "logrite.event": event });
		logger.warn(`<<< Exiting updatePageModuleCopy(id=${id},key=${key},event=${event})`, { "event.action": "updatePageModuleCopy", "logrite.id": id, "logrite.key": key, "logrite.event": event });
        return {
            type: ActionTypes.UPDATED_PAGE_MODULE_COPY,
            data: { id, key, event }
        };
    },

    editingPageModule(module) {
		logger.error(`>>> Entering editingPageModule(module=${module})`, { "event.action": "editingPageModule", "logrite.module": module });
		logger.warn(`<<< Exiting editingPageModule(module=${module})`, { "event.action": "editingPageModule", "logrite.module": module });
        return {
            type: ActionTypes.EDITING_PAGE_MODULE,
            data: { module }
        };
    },

    cancelEditingPageModule() {
		logger.error('>>> Entering cancelEditingPageModule()', { "event.action": "cancelEditingPageModule" });
		logger.warn('<<< Exiting cancelEditingPageModule()', { "event.action": "cancelEditingPageModule" });
        return {
            type: ActionTypes.CANCEL_EDITING_PAGE_MODULE,
            data: {}
        };
    },

    copyAppearanceToDescendantPages() {
		logger.error('>>> Entering copyAppearanceToDescendantPages()', { "event.action": "copyAppearanceToDescendantPages" });
		logger.warn('<<< Exiting copyAppearanceToDescendantPages()', { "event.action": "copyAppearanceToDescendantPages" });
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
				logger.error('Exception in copyAppearanceToDescendantPages()', { "event.action": "copyAppearanceToDescendantPages", "event.category": "process" });
                dispatch({
                    type: ActionTypes.ERROR_COPYING_APPEARANCE_TO_DESCENDANT_PAGES,
                    data: { error }
                });
            });
        };
    },

    copyPermissionsToDescendantPages() {
		logger.error('>>> Entering copyPermissionsToDescendantPages()', { "event.action": "copyPermissionsToDescendantPages" });
		logger.warn('<<< Exiting copyPermissionsToDescendantPages()', { "event.action": "copyPermissionsToDescendantPages" });
        return (dispatch, getState) => {
            dispatch({
                type: ActionTypes.COPYING_PERMISSIONS_TO_DESCENDANT_PAGES
            });

            const page = getState().pages.selectedPage;
			logger.error(`copyPermissionsToDescendantPages(): page → ${page}`, { "event.action": "copyPermissionsToDescendantPages" });
            PagesService.copyPermissionsToDescendantPages(page.tabId).then(() => {
                utils.notify(Localization.get("CopyPermissionsToDescendantPagesSuccess"));
                dispatch({
                    type: ActionTypes.COPIED_PERMISSIONS_TO_DESCENDANT_PAGES,
                    data: {}
                });
            }).catch((error) => {
				logger.error('Exception in copyPermissionsToDescendantPages()', { "event.action": "copyPermissionsToDescendantPages", "event.category": "process" });
                dispatch({
                    type: ActionTypes.ERROR_COPYING_PERMISSIONS_TO_DESCENDANT_PAGES,
                    data: { error }
                });
            });
        };
    },

    getCachedPageCount(cacheProvider) {
		logger.error(`>>> Entering getCachedPageCount(cacheProvider=${cacheProvider})`, { "event.action": "getCachedPageCount", "logrite.cacheProvider": cacheProvider });
		logger.error(`getCachedPageCount(cacheProvider=${cacheProvider}): page → ${page}`, { "event.action": "getCachedPageCount", "logrite.cacheProvider": cacheProvider });
		logger.warn(`<<< Exiting getCachedPageCount(cacheProvider=${cacheProvider})`, { "event.action": "getCachedPageCount", "logrite.cacheProvider": cacheProvider });
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
		logger.error(`>>> Entering clearCache(cacheProvider=${cacheProvider})`, { "event.action": "clearCache", "logrite.cacheProvider": cacheProvider });
		logger.error(`clearCache(cacheProvider=${cacheProvider}): page → ${page}`, { "event.action": "clearCache", "logrite.cacheProvider": cacheProvider });
		logger.warn(`<<< Exiting clearCache(cacheProvider=${cacheProvider})`, { "event.action": "clearCache", "logrite.cacheProvider": cacheProvider });
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
		logger.error('>>> Entering clearSelectedPage()', { "event.action": "clearSelectedPage" });
		logger.warn('<<< Exiting clearSelectedPage()', { "event.action": "clearSelectedPage" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEAR_SELECTED_PAGE,
                data: {}
            });
        };
    },

    movePage({ Action, PageId, ParentId, RelatedPageId }) {
		logger.error(`>>> Entering movePage(Action=${Action},PageId=${PageId},ParentId=${ParentId},RelatedPageId=${RelatedPageId})`, { "event.action": "movePage", "logrite.Action": Action, "logrite.PageId": PageId, "logrite.ParentId": ParentId, "logrite.RelatedPageId": RelatedPageId });
		logger.warn(`<<< Exiting movePage(Action=${Action},PageId=${PageId},ParentId=${ParentId},RelatedPageId=${RelatedPageId})`, { "event.action": "movePage", "logrite.Action": Action, "logrite.PageId": PageId, "logrite.ParentId": ParentId, "logrite.RelatedPageId": RelatedPageId });
        return PagesService.movePage({ Action, PageId, ParentId, RelatedPageId }).catch(() => {
            utils.notifyError(Localization.get("AnErrorOccurred"));
        });
		logger.error(`Exception in movePage(Action=${Action},PageId=${PageId},ParentId=${ParentId},RelatedPageId=${RelatedPageId}): (no error binding)`, { "event.action": "movePage", "event.category": "process", "logrite.Action": Action, "logrite.PageId": PageId, "logrite.ParentId": ParentId, "logrite.RelatedPageId": RelatedPageId });
    },

    dirtyCustomDetails() {
		logger.error('>>> Entering dirtyCustomDetails()', { "event.action": "dirtyCustomDetails" });
		logger.warn('<<< Exiting dirtyCustomDetails()', { "event.action": "dirtyCustomDetails" });
        return {
            type: ActionTypes.CUSTOM_PAGE_DETAILS_UPDATED,
            data: {}
        };
    }
};

export default pageActions;