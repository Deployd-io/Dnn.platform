
import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { PersonaBarPageHeader, PersonaBarPage, Button, GridCell } from "@dnnsoftware/dnn-react-common";
import {
    pageActions as PageActions,
    addPagesActions as AddPagesActions,
    templateActions as TemplateActions,
    visiblePanelActions as VisiblePanelActions,
    visiblePageSettingsActions as VisiblePageSettingsActions,
    languagesActions as LanguagesActions,
    pageHierarchyActions as PageHierarchyActions
} from "../actions";
import PageSettings from "./PageSettings/PageSettings";
import AddPages from "./AddPages/AddPages";
import Localization from "../localization";
import PageList from "./PageList/PageList";
import SaveAsTemplate from "./SaveAsTemplate/SaveAsTemplate";
import utils from "../utils";
import panels from "../constants/panels";
import Sec from "./Security/Sec";
import securityService from "../services/securityService";
import permissionTypes from "../services/permissionTypes";
import BreadCrumbs from "./BreadCrumbs";
import Promise from "promise";
import SearchPageInput from "./SearchPage/SearchPageInput";

import "./style.less";

import {PersonaBarPageTreeviewInteractor} from "./dnn-persona-bar-page-treeview";
import SearchResult from "./SearchPage/SearchResult";
const logger = console;

function getSelectedTabBeingViewed(viewTab) {
	logger.error(`>>> Entering getSelectedTabBeingViewed(viewTab=${viewTab})`, { "event.action": "getSelectedTabBeingViewed", "logrite.viewTab": viewTab });
    switch (viewTab) {
        case "details":
            return 0;
        case "permissions":
            return 1;
        case "localization":
            return 2;
        case "advanced":
			logger.warn(`<<< Exiting getSelectedTabBeingViewed(viewTab=${viewTab})`, { "event.action": "getSelectedTabBeingViewed", "logrite.viewTab": viewTab });
            return 3;
    }
}


class App extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        const date = new Date();
		logger.error(`constructor(): date → ${date}`, { "event.action": "constructor" });
        this.state = {
            referral: "",
            referralText: "",
            busy: false,
            headerDropdownSelection: "Save Page Template",
            
            inSearch: false,
            searchTerm: "",
            filtersUpdated: false,

            startDate: date,
            endDate: date,
            defaultDate: date,
            startAndEndDateDirty: false,

            filterByPageType: null,
            filterByPublishStatus: null,
            filterByWorkflow: null,
            workflowList: [],

            tags: "",
            filters: [],
            pageIndex:0,
            searchFields: {},

            inDuplicateMode: false
        };
        this.lastActivePageId = null;
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
		logger.debug(`constructor(): this.lastActivePageId → ${this.lastActivePageId}`, { "event.action": "constructor" });
		logger.debug(`constructor(): this.noPermissionSelectionPageId → ${this.noPermissionSelectionPageId}`, { "event.action": "constructor" });
        this.shouldRunRecursive = true;
        this.noPermissionSelectionPageId = null;
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        let { selectedPage } = this.props;
        if (securityService.userHasPermission(permissionTypes.MANAGE_PAGE, selectedPage)) {
            this.props.getContentLocalizationEnabled();
        }

        const viewName = utils.getViewName();
		logger.error(`componentDidMount(): viewName → ${viewName}`, { "event.action": "componentDidMount" });
        const viewParams = utils.getViewParams();
		logger.error(`componentDidMount(): viewParams → ${viewParams}`, { "event.action": "componentDidMount" });
        window.dnn.utility.setConfirmationDialogPosition();
        window.dnn.utility.closeSocialTasks();
        this.props.getPageList().then(() => {
            const selectedPageId = utils.getSelectedPageId() || utils.getCurrentPageId();
			logger.error(`componentDidMount(): selectedPageId → ${selectedPageId}`, { "event.action": "componentDidMount" });
            selectedPageId && !utils.getIsAdminHostSystemPage() && this.onLoadPage(selectedPageId);
        
            if (viewName === "edit") {
                this.onLoadPage(utils.getCurrentPageId());
            }
            this.props.getWorkflowsList();

            //Resolve tab being viewed, if view params are present.
            this.resolveTabBeingViewed(viewParams);

            //Listen to event fired to view page settings (from site settings)
            document.addEventListener("viewPageSettings", this.resolveTabBeingViewed.bind(this), false);
        });
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    //Update referral text if coming from a referral. (ex: "SiteSettings", resx.get("BackToLanguages"))
    updateReferral(referral, referralText) {
		logger.error(`>>> Entering updateReferral(referral=${referral},referralText=${referralText})`, { "event.action": "updateReferral", "logrite.referral": referral, "logrite.referralText": referralText });
        this.setState({
            referral,
            referralText
        });
		logger.warn(`<<< Exiting updateReferral(referral=${referral},referralText=${referralText})`, { "event.action": "updateReferral", "logrite.referral": referral, "logrite.referralText": referralText });
    }

    //Method to go back to referral panel.
    goToReferralPanel(referral) {
		logger.error(`>>> Entering goToReferralPanel(referral=${referral})`, { "event.action": "goToReferralPanel", "logrite.referral": referral });
        let personaBar = window.parent.dnn ? window.parent.dnn.PersonaBar : null;
        if (personaBar) {
            personaBar.openPanel(referral, {}); //Open, panel should already be rendered, so no need to pass params.
            this.updateReferral("", "");
			logger.warn(`<<< Exiting goToReferralPanel(referral=${referral})`, { "event.action": "goToReferralPanel", "logrite.referral": referral });
        }
    }

    //Call method to go back to referral panel
    backToReferral(referral) {
		logger.error(`>>> Entering backToReferral(referral=${referral})`, { "event.action": "backToReferral", "logrite.referral": referral });
        this.goToReferralPanel(referral);
		logger.warn(`<<< Exiting backToReferral(referral=${referral})`, { "event.action": "backToReferral", "logrite.referral": referral });
    }

    //Resolves tab being viewed if view params are present.
    resolveTabBeingViewed(viewParams) {
	logger.error(`>>> Entering resolveTabBeingViewed(viewParams=${viewParams})`, { "event.action": "resolveTabBeingViewed", "logrite.viewParams": viewParams });

        if (!viewParams) {
            return;
        }
        if (viewParams.pageId) {
            this.onLoadPage(viewParams.pageId);

        }
        if (viewParams.viewTab) {
            this.selectPageSettingTab(getSelectedTabBeingViewed(viewParams.viewTab));
        }
        if (viewParams.referral) {
            this.updateReferral(viewParams.referral, viewParams.referralText);
        }
		logger.warn(`<<< Exiting resolveTabBeingViewed(viewParams=${viewParams})`, { "event.action": "resolveTabBeingViewed", "logrite.viewParams": viewParams });
    }
    
    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("viewPageSettings", this.resolveTabBeingViewed);
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        if (this.props.error && this.props.error && this.props.error !== prevProps.error) {
            const errorMessage = (this.props.error && this.props.error.message) || Localization.get("AnErrorOccurred");
			logger.error(`componentDidUpdate(prevProps=${prevProps}): errorMessage → ${errorMessage}`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            utils.notifyError(errorMessage);
        }
        window.dnn.utility.closeSocialTasks();
        const { selectedPage } = this.props;
        if (selectedPage && selectedPage.tabId > 0 && selectedPage.canManagePage !== undefined && !selectedPage.canManagePage) {
            this.noPermissionSelectionPageId = utils.getCurrentPageId();
            this.setEmptyStateMessage(Localization.get("NoPermissionEditPage"));
        }
        if (selectedPage && selectedPage.tabId > 0 && this.shouldRunRecursive) {
            this.shouldRunRecursive = false;
            this.buildTree(selectedPage.tabId);
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    buildTree(selectedId) {
		logger.error(`>>> Entering buildTree(selectedId=${selectedId})`, { "event.action": "buildTree", "logrite.selectedId": selectedId });
        const buildTreeInternal = (hierarchy) => {
			logger.error(`>>> Entering buildTreeInternal(hierarchy=${hierarchy})`, { "event.action": "buildTreeInternal", "logrite.hierarchy": hierarchy });
            const callAPI = () => {
				logger.error('>>> Entering callAPI()', { "event.action": "callAPI" });
                const parentId = hierarchy.shift();
                parentId && setTimeout(() => execute(), 100);
				logger.error(`callAPI(): parentId → ${parentId}`, { "event.action": "callAPI" });
                const execute = () => {
                    if (parentId !== selectedId) {
						logger.error('>>> Entering execute()', { "event.action": "execute" });
                        let page = null;
                        this._traverse((item, list, update) => {
                            if (item.id === parentId) {
                                item.isOpen = true;
                                item.hasChildren = true;
                                page = item;
                                update(list);
                                return;
                            }
                        });
                        if (!page || !page.childListItems) {
                            this.props.getChildPageList(parentId)
                                .then(data => {
                                    this._traverse((item, list, update) => {
                                        const left = () => {
											logger.error('>>> Entering left()', { "event.action": "left" });
                                            item.childListItems = data;
                                            item.isOpen = true;
                                            item.hasChildren = true;
                                            update(list);
                                            callAPI();
											logger.warn('<<< Exiting left()', { "event.action": "left" });
                                        };
                                        const right = () => {
                                            logger.error(`>>> Entering right(list=${list})`, { "event.action": "right" });
                                            logger.warn(`<<< Exiting right(list=${list})`, { "event.action": "right" });
                                            update(list);
                                        };
                                        (data.length > 0 && item.id === data[0].parentId) ? left() : right();
                                    });
                                });
                        } else {
                            callAPI();
						logger.warn('<<< Exiting execute()', { "event.action": "execute" });
                        }
                    } else {
                        this.buildBreadCrumbPath(selectedId);
                    }
                };
				logger.warn('<<< Exiting callAPI()', { "event.action": "callAPI" });
            };
            callAPI();
			logger.warn(`<<< Exiting buildTreeInternal(hierarchy=${hierarchy})`, { "event.action": "buildTreeInternal", "logrite.hierarchy": hierarchy });
        };
        this.props.getPageHierarchy(selectedId).then(buildTreeInternal);
		logger.warn(`<<< Exiting buildTree(selectedId=${selectedId})`, { "event.action": "buildTree", "logrite.selectedId": selectedId });
    }

    onPageSettings(pageId) {
		logger.error(`>>> Entering onPageSettings(pageId=${pageId})`, { "event.action": "onPageSettings", "logrite.pageId": pageId });
        this.onLoadPage(pageId);
		logger.warn(`<<< Exiting onPageSettings(pageId=${pageId})`, { "event.action": "onPageSettings", "logrite.pageId": pageId });
    }

    onCreatePage() {
		logger.error('>>> Entering onCreatePage()', { "event.action": "onCreatePage" });
        this.props.onCreatePage((page) => {
            if (page && page.canAddContentToPage || page && page.pageType !== "normal" || utils.getIsSuperUser()) {
                page.selected = true;
                if (page.parentId && page.parentId !== -1) {
                    this._traverse((item, list, updateStore) => {
                        if (item.id === page.parentId) {
                            switch (true) {
                                case item.childCount > 0 && !item.childListItems:
                                    this.props.getChildPageList(item.id).then((data) => {
                                        item.isOpen = true;
                                        data.map(child => {
                                            if (child.id === page.id) {
                                                child.selected = true;
                                            }
                                            return child;
                                        });
                                        item.childListItems = data;
                                        updateStore(list);
                                        if (page.canAddContentToPage)
                                            this.onLoadPage(page.id);
                                        else {
                                            this.onNoPermissionSelection(page.id);
                                        }
                                    });
                                    break;
                                case item.childCount === 0 && !item.childListItems:
                                    item.childCount++;
                                    item.childListItems = [];
                                    item.childListItems.push(page);
                                    if (page.canAddContentToPage)
                                        this.onLoadPage(page.id);
                                    else {
                                        this.onNoPermissionSelection(page.id);
                                    }
                                    break;
                                case Array.isArray(item.childListItems) === true: {
                                    const lastIndex = item.childListItems.length - 1;
                                    if (item.childListItems[lastIndex].name === page.name) {
                                        item.childListItems.pop();
                                    }
                                    item.childListItems.push(page);
                                    item.childCount = item.childListItems.length;
                                    if (page.canAddContentToPage)
                                        this.onLoadPage(page.id);
                                    else {
                                        this.onNoPermissionSelection(page.id);
                                    }
                                    break;
                                }
                            }
                            item.isOpen = true;
                            updateStore(list);
                        }
                    });
                }
                else {
                    this.props.getPageList().then(() => {
                        this._traverse((item, list, updateStore) => {
                            if (item.id === page.id) {
                                item.isOpen = true;
                                item.selected = true;
                                updateStore(list);
                                if (page.canAddContentToPage)
                                    this.onLoadPage(page.id);
                                else {
                                    this.onNoPermissionSelection(page.id);
                                }
                            }
                        });
                    });
                }
            }
            else {
                let self = this;
                self.setEmptyStateMessage();
				logger.error(`onCreatePage(): self → ${self}`, { "event.action": "onCreatePage" });
            }
        });
    }

    onUpdatePage(input) {
		logger.error(`>>> Entering onUpdatePage(input=${input})`, { "event.action": "onUpdatePage", "logrite.input": input });
        this.shouldRunRecursive = false;
		logger.warn(`<<< Exiting onUpdatePage(input=${input})`, { "event.action": "onUpdatePage", "logrite.input": input });
		logger.error(`onUpdatePage(input=${input}): update → ${update}`, { "event.action": "onUpdatePage", "logrite.input": input });
        return new Promise((resolve) => {
            const update = (input && input.tabId) ? input : this.props.selectedPage;
            let cachedItem = null;

            const removeFromOldParent = () => {
                const left = () => {
                    const { pageList } = this.props;
                    pageList.forEach((item, index) => {
                        if (item.id === update.tabId) {
                            cachedItem = item;
                            const arr1 = pageList.slice(0, index);
                            const arr2 = pageList.slice(index + 1);
                            const newPageList = [...arr1, ...arr2];
                            this.props.updatePageListStore(newPageList);
                        }
                    });
                };


                const right = () => {

                    this._traverse((item, list, updateStore) => {
                        if (item.id === update.oldParentId) {
                            item.childListItems.forEach((child, index) => {
                                if (child.id === update.tabId) {
                                    cachedItem = child;
                                    const arr1 = item.childListItems.slice(0, index);
                                    const arr2 = item.childListItems.slice(index + 1);
                                    item.childListItems = [...arr1, ...arr2];
                                    item.childCount--;
                                    updateStore(list);
                                }
                            });
                        }
                    });
					logger.error('>>> Entering right()', { "event.action": "right" });
                };

                (update.oldParentId === -1 || update.parentId === -1) ? left() : right();
            };

            const addToNewParent = () => {
                if (update.parentId === -1) {
                    this._traverse((item, list, updateStore) => {
                        if (item.id === list[list.length - 1].id) {
                            (cachedItem) ? cachedItem.parentId = -1 : null;
                            const listCopy = [...list, cachedItem];
                            updateStore(listCopy);
                        }
                    });
                } else {
                    this._traverse((item, list, updateStore) => {
                        if (item.id === update.parentId) {

                            (cachedItem) ? cachedItem.parentId = item.id : null;

                            switch (true) {
                                case item.childCount > 0 && !item.childListItems:
                                    this.props.getChildPageList(item.id).then((data) => {
                                        item.isOpen = true;
                                        item.childListItems = data;
                                        updateStore(list);
                                    });
                                    break;
                                case item.childCount === 0 && !item.childListItems:
                                    item.childCount++;
                                    item.childListItems = [];
                                    item.childListItems.push(cachedItem);
                                    break;
                                case Array.isArray(item.childListItems) === true:
                                    item.childCount++;
                                    item.childListItems.push(cachedItem);
                                    this.onLoadPage(cachedItem.id);
                                    break;
                            }
                            item.isOpen = true;
                            updateStore(list);
                        }
                    });
                }
            };

            this.props.onUpdatePage(update, (page) => {
                if (update.oldParentId) {
                    if (page.id === utils.getCurrentPageId()) {
                        window.parent.location = page.url;
                    }
                    else {
                        removeFromOldParent();
                        addToNewParent();
                    }
                }

                this._traverse((item, list, updateStore) => {
                    if (item.id === update.tabId) {
                        item.name = page.name;
                        item.pageType = page.pageType;
                        item.url = page.url;
                        item.includeInMenu = update.includeInMenu;
                        updateStore(list);
                    }
                });
                this.buildTree(update.tabId);
                resolve();
                this.setState({inDuplicateMode: false});
            });
        });
    }

    
    onLoadPage(pageId, callback) {
		logger.error(`>>> Entering onLoadPage(pageId=${pageId},callback=${callback})`, { "event.action": "onLoadPage", "logrite.pageId": pageId, "logrite.callback": callback });
        const self = this;
        this.props.onLoadPage(pageId).then((data) => {
            self.buildBreadCrumbPath(data.tabId);
            if (typeof callback === "function")
                callback(data);
        });
    }

    _addPageToTree(parentId) {
		logger.error(`>>> Entering _addPageToTree(parentId=${parentId})`, { "event.action": "_addPageToTree", "logrite.parentId": parentId });
        let runUpdateStore = null;
        let pageList = null;
        
        if (parentId !== null && parentId !== -1) {
            this._traverse((item, list, updateStore) => {
                if (item.id === parentId) {
                    item.isOpen = true;
                    item.hasChildren = true;
                    item.childCount++;
                    const newPageChildItems = item.childListItems.concat(this.props.selectedPage);
					logger.error(`_addPageToTree(parentId=${parentId}): newPageChildItems → ${newPageChildItems}`, { "event.action": "_addPageToTree", "logrite.parentId": parentId });
                    item.childListItems = newPageChildItems;
                    updateStore(list);        
                }
            });

        } else {
            this._traverse((item, list, updateStore) => {
                item.selected = false;
                runUpdateStore = updateStore;
                pageList = list;                
            });
            const newPageList = pageList.concat(this.props.selectedPage);    
            runUpdateStore(newPageList);
			logger.error(`_addPageToTree(parentId=${parentId}): newPageList → ${newPageList}`, { "event.action": "_addPageToTree", "logrite.parentId": parentId });
        }
    }

    _removePageFromTree(parentId) {
		logger.error(`>>> Entering _removePageFromTree(parentId=${parentId})`, { "event.action": "_removePageFromTree", "logrite.parentId": parentId });
        this._traverse((item, list, updateStore) => {
            if (item.id === parentId && parentId !== undefined) {
                let itemIndex = null;
                item.childCount--;
                (item.childCount === 0) ? item.isOpen = false : null;

                item.childListItems.forEach((child, index) => {
                    if (child.tabId !== undefined) {
                        itemIndex = index;
                    }
                });
                const arr1 = item.childListItems.slice(0, itemIndex);
				logger.error(`_removePageFromTree(parentId=${parentId}): arr1 → ${arr1}`, { "event.action": "_removePageFromTree", "logrite.parentId": parentId });
                const arr2 = item.childListItems.slice(itemIndex + 1);
				logger.error(`_removePageFromTree(parentId=${parentId}): arr2 → ${arr2}`, { "event.action": "_removePageFromTree", "logrite.parentId": parentId });
                item.childListItems = [...arr1, ...arr2];
                updateStore(list);
            }
            if ((parentId === undefined || parentId === -1) && item.tabId !== undefined) {
                const newPageList = list.slice(0,list.length-1);
				logger.error(`_removePageFromTree(parentId=${parentId}): newPageList → ${newPageList}`, { "event.action": "_removePageFromTree", "logrite.parentId": parentId });
                updateStore(newPageList);
            }
		logger.warn(`<<< Exiting _removePageFromTree(parentId=${parentId})`, { "event.action": "_removePageFromTree", "logrite.parentId": parentId });
        });


    }

    onCancelPage(parentPageId) {
		logger.error(`>>> Entering onCancelPage(parentPageId=${parentPageId})`, { "event.action": "onCancelPage", "logrite.parentPageId": parentPageId });
        this._removePageFromTree(parentPageId);
        this.props.changeSelectedPagePath([]);
        (parentPageId !== -1) ? this.props.onCancelPage(parentPageId) : this.props.onCancelPage();
        this.setState({inDuplicateMode: false});
		logger.warn(`<<< Exiting onCancelPage(parentPageId=${parentPageId})`, { "event.action": "onCancelPage", "logrite.parentPageId": parentPageId });
    }

    onChangeParentId() {
		logger.error('>>> Entering onChangeParentId()', { "event.action": "onChangeParentId" });
        this.onChangePageField("oldParentId", this.props.selectedPage.parentId);
		logger.warn('<<< Exiting onChangeParentId()', { "event.action": "onChangeParentId" });
    }

    onAddMultiplePage() {
		logger.error('>>> Entering onAddMultiplePage()', { "event.action": "onAddMultiplePage" });
        this._testDirtyPage(()=>{
            this.clearEmptyStateMessage();
            this.selectPageSettingTab(0);
            this.props.clearSelectedPage();
    
            this.props.onLoadAddMultiplePages();
        });  
        
		logger.warn('<<< Exiting onAddMultiplePage()', { "event.action": "onAddMultiplePage" });
    }
    
    /**
     * When on edit mode
     */
    onEditMode() {
		logger.error('>>> Entering onEditMode()', { "event.action": "onEditMode" });
        const {selectedPage, selectedView, selectedPageDirty} = this.props;
		logger.warn('<<< Exiting onEditMode()', { "event.action": "onEditMode" });
        return ((selectedPage && selectedPage.tabId === 0) || selectedPageDirty
            || selectedView === panels.SAVE_AS_TEMPLATE_PANEL
            || selectedView === panels.ADD_MULTIPLE_PAGES_PANEL 
            || selectedView === panels.CUSTOM_PAGE_DETAIL_PANEL);
    }

    onAddPage(parentPage) {
		logger.error(`>>> Entering onAddPage(parentPage=${parentPage})`, { "event.action": "onAddPage", "logrite.parentPage": parentPage });
        this.clearEmptyStateMessage();
        this.selectPageSettingTab(0);
        
        const addPage = () => {
			logger.error('>>> Entering addPage()', { "event.action": "addPage" });
            const { props } = this;
            const { selectedPage } = props;
            let runUpdateStore = null;
            let pageList = null;
        
            this._traverse((item, list, updateStore) => {
                item.selected = false;
                pageList = list;
                runUpdateStore = updateStore;
            });
            runUpdateStore(pageList);

            const onConfirm = () => {
				logger.error('>>> Entering onConfirm()', { "event.action": "onConfirm" });
                this.props.changeSelectedPagePath([]);
                
                this.props.getNewPage(parentPage).then(()=>{
                    if (parentPage && parentPage.id) {
                        this.props.getChildPageList(parentPage.id)
                            .then(pageChildItems => {
                                this._traverse((item, list, update) => {
                                    const updateChildItems = () => {
										logger.error('>>> Entering updateChildItems()', { "event.action": "updateChildItems" });
                                        const newPageChildItems = pageChildItems.concat(this.props.selectedPage);
                                        item.childListItems = newPageChildItems;
										logger.error(`updateChildItems(): newPageChildItems → ${newPageChildItems}`, { "event.action": "updateChildItems" });
                                        item.isOpen = true;
                                        item.hasChildren = true;
                                        update(list);
										logger.warn('<<< Exiting updateChildItems()', { "event.action": "updateChildItems" });
                                    };
                                    (item.id === parentPage.id) ? updateChildItems() : null;
                                });
                            });
                    } else {
                        this._traverse((item, list, updateStore) => {
                            pageList = list;
                            runUpdateStore = updateStore;
                        });
                        const newPageList = pageList.concat(this.props.selectedPage);    
                        runUpdateStore(newPageList);
                    }

                    this.setState({inDuplicateMode: false});
                }); 
				logger.warn('<<< Exiting onConfirm()', { "event.action": "onConfirm" });
            };
       

            if (selectedPage && selectedPage.tabId !== 0 && props.selectedPageDirty) {
                utils.confirm(
                    Localization.get("CancelWithoutSaving"),
                    Localization.get("Close"),
                    Localization.get("Cancel"),
                    onConfirm);

            } else {
                onConfirm();
            }
            
            setTimeout(()=>{
                if (this.node.querySelector("#name")) {
                    this.node.querySelector("#name").focus();
                }    
            },100);
			logger.warn('<<< Exiting addPage()', { "event.action": "addPage" });
        };

        const noPermission = () => this.setEmptyStateMessage("You do not have permission to add a child page to this parent");
        parentPage.canAddPage === undefined || parentPage.canAddPage ? addPage() : noPermission();
		logger.warn(`<<< Exiting onAddPage(parentPage=${parentPage})`, { "event.action": "onAddPage", "logrite.parentPage": parentPage });
    }

    onCancelSettings() {
		logger.error('>>> Entering onCancelSettings()', { "event.action": "onCancelSettings" });
        const { props } = this;
        if (props.selectedPageDirty) {
            
            this.showCancelWithoutSavingDialog();
        }
        else {
            if (props.selectedPage.tabId === 0 && props.selectedPage.isCopy && props.selectedPage.templateTabId) {
                this.onCancelPage(props.selectedPage.parentId);
				logger.warn('<<< Exiting onCancelSettings()', { "event.action": "onCancelSettings" });
            }
            else if (props.selectedPage.tabId === 0 && props.selectedPage.referralTabId) {
                this.onCancelPage(props.selectedPage.referralTabId);
            }
            else {
                this.onCancelPage();
            }
        }
    }

    onDeleteSettings() {
		logger.error('>>> Entering onDeleteSettings()', { "event.action": "onDeleteSettings" });
        const { props } = this;
        const { selectedPage } = props;

        const left = () => {
            return () => {
                this._traverse((item, list, updateStore) => {
                    if (item.id === props.selectedPage.parentId) {
                        let itemIndex = null;
                        item.childCount--;
                        (item.childCount === 0) ? item.isOpen = false : null;

                        item.childListItems.forEach((child, index) => {
                            if (child.id === props.selectedPage.tabId) {
                                itemIndex = index;
                            }
                        });
                        const arr1 = item.childListItems.slice(0, itemIndex);
                        const arr2 = item.childListItems.slice(itemIndex + 1);
                        item.childListItems = [...arr1, ...arr2];
                        props.onDeletePage(props.selectedPage, false, utils.getCurrentPageId() === props.selectedPage.tabId ? item.url : null);
                        updateStore(list);
                        this.onCancelPage();
                    }
                });
            };
        };

        const right = () => {
			logger.error('>>> Entering right()', { "event.action": "right" });
            return () => {
                let itemIndex;
                const pageList = JSON.parse(JSON.stringify(this.props.pageList));
                pageList.forEach((item, index) => {
                    if (item.id === selectedPage.tabId) {
                        itemIndex = index;
                    }
                });

                const arr1 = pageList.slice(0, itemIndex);
                const arr2 = pageList.slice(itemIndex + 1);
                const update = [...arr1, ...arr2];
                this.props.onDeletePage(props.selectedPage, false, utils.getCurrentPageId() === props.selectedPage.tabId ? utils.getDefaultPageUrl() : null);
                this.props.updatePageListStore(update);
                this.onCancelPage();
            };
        };

		logger.warn('<<< Exiting onDeleteSettings()', { "event.action": "onDeleteSettings" });
        const onDelete = (selectedPage.parentId !== -1) ? left() : right();

        utils.confirm(
            Localization.get("DeletePageConfirm"),
            Localization.get("Delete"),
            Localization.get("Cancel"),
            onDelete);
    }

    onClearCache() {
		logger.error('>>> Entering onClearCache()', { "event.action": "onClearCache" });
        const { props } = this;
        props.onClearCache();
		logger.warn('<<< Exiting onClearCache()', { "event.action": "onClearCache" });
    }

    showCancelWithoutSavingDialog() {
		logger.error('>>> Entering showCancelWithoutSavingDialog()', { "event.action": "showCancelWithoutSavingDialog" });
        const { props } = this;
        const onConfirm = () => {
            if (props.selectedPage.tabId === 0 && props.selectedPage.isCopy && props.selectedPage.templateTabId) {
                this.onCancelPage(props.selectedPage.parentId);
				logger.warn('<<< Exiting showCancelWithoutSavingDialog()', { "event.action": "showCancelWithoutSavingDialog" });
            }
            else if (props.selectedPage.tabId === 0 && props.selectedPage.referralTabId) {
                this.onCancelPage(props.selectedPage.referralTabId);
            }
            else {
                this.onCancelPage();
            }
        };

        utils.confirm(
            Localization.get("CancelWithoutSaving"),
            Localization.get("Close"),
            Localization.get("Cancel"),
            onConfirm);
    }


    showCancelWithoutSavingDialogInEditMode(input) {
		logger.error(`>>> Entering showCancelWithoutSavingDialogInEditMode(input=${input})`, { "event.action": "showCancelWithoutSavingDialogInEditMode", "logrite.input": input });
        const id = (typeof input === "object") ? this.props.selectedPage.tabId : input;
        if (this.props.selectedPageDirty) {
            const onConfirm = () => {
                this.onLoadPage(id, () => {
                    this._traverse((item, list) => {
                        if (item.id === id) {
                            Object.keys(this.props.selectedPage).forEach((key) => item[key] = this.props.selectedPage[key]);
                            this.props.updatePageListStore(list);
                            this.selectPageSettingTab(0);
                            this.lastActivePageId = null;
                        }
                    });
                });
            };


            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Continue"),
                Localization.get("Go Back"),
                onConfirm);

        } else {
            this.onCancelPage();
            this.props.changeSelectedPagePath([]);

		logger.warn(`<<< Exiting showCancelWithoutSavingDialogInEditMode(input=${input})`, { "event.action": "showCancelWithoutSavingDialogInEditMode", "logrite.input": input });
        }
    }

    isNewPage() {
		logger.error('>>> Entering isNewPage()', { "event.action": "isNewPage" });
        const { selectedPage } = this.props;
		logger.warn('<<< Exiting isNewPage()', { "event.action": "isNewPage" });
        return selectedPage.tabId === 0;
    }

    getPageTitle() {
		logger.error('>>> Entering getPageTitle()', { "event.action": "getPageTitle" });
        const { selectedPage } = this.props;
		logger.warn('<<< Exiting getPageTitle()', { "event.action": "getPageTitle" });
        return this.isNewPage() ?
            Localization.get("AddPage") :
            selectedPage.name;
    }
    
    _testDirtyPage(callback) {
		logger.error(`>>> Entering _testDirtyPage(callback=${callback})`, { "event.action": "_testDirtyPage", "logrite.callback": callback });
        const {selectedPage, selectedPageDirty} = this.props;
        const onConfirm = () => {
            callback();
            this.onLoadPage(selectedPage.tabId, () => {
                this._traverse((item, list) => {
                    if (item.id === selectedPage.tabId) {
                        Object.keys(this.props.selectedPage).forEach((key) => item[key] = this.props.selectedPage[key]);
                        this.props.updatePageListStore(list);
                        this.selectPageSettingTab(0);
                        this.lastActivePageId = null;
                    }
                });
            });
        };

		logger.debug(`_testDirtyPage(callback=${callback}): this.lastActivePageId → ${this.lastActivePageId}`, { "event.action": "_testDirtyPage", "logrite.callback": callback });
        if (selectedPage && selectedPage.tabId !== 0 && selectedPageDirty) {
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                onConfirm);

        } else {
			logger.warn(`<<< Exiting _testDirtyPage(callback=${callback})`, { "event.action": "_testDirtyPage", "logrite.callback": callback });
            return onConfirm();
        }
    }

    onLoadSavePageAsTemplate() {
		logger.error('>>> Entering onLoadSavePageAsTemplate()', { "event.action": "onLoadSavePageAsTemplate" });
        this._testDirtyPage(this.props.onLoadSavePageAsTemplate.bind(this));
		logger.warn('<<< Exiting onLoadSavePageAsTemplate()', { "event.action": "onLoadSavePageAsTemplate" });
    }

    getSettingsButtons() {
		logger.error('>>> Entering getSettingsButtons()', { "event.action": "getSettingsButtons" });
        const { selectedPage, settingsButtonComponents, onLoadSavePageAsTemplate, onShowPageSettings, onHidePageSettings } = this.props;
        const SaveAsTemplateButton = settingsButtonComponents.SaveAsTemplateButton || Button;
        
		logger.warn('<<< Exiting getSettingsButtons()', { "event.action": "getSettingsButtons" });
        return (
            <div className="heading-buttons">
                <Sec permission={permissionTypes.EXPORT_PAGE} selectedPage={selectedPage}>
                    <SaveAsTemplateButton
                        type="secondary"
                        size="large"
                        disabled={this.onEditMode() || this.state.inSearch}
                        onClick={this.onLoadSavePageAsTemplate.bind(this)}
                        onShowPageSettingsCallback={onShowPageSettings}
                        onHidePageSettings={onHidePageSettings}
                        onSaveAsPlatformTemplate={onLoadSavePageAsTemplate}>
                        {Localization.get("SaveAsTemplate")}
                    </SaveAsTemplateButton>
                </Sec>

            </div>
        );
    }

    selectPageSettingTab(index) {
		logger.error(`>>> Entering selectPageSettingTab(index=${index})`, { "event.action": "selectPageSettingTab", "logrite.index": index });
        this.props.selectPageSettingTab(index);
		logger.warn(`<<< Exiting selectPageSettingTab(index=${index})`, { "event.action": "selectPageSettingTab", "logrite.index": index });
    }

    onSaveMultiplePages() {    
		logger.error('>>> Entering onSaveMultiplePages()', { "event.action": "onSaveMultiplePages" });
		logger.warn('<<< Exiting onSaveMultiplePages()', { "event.action": "onSaveMultiplePages" });
        return this.props.onSaveMultiplePages(()=>{
            this.props.getPageList();
        });
    }
    
    onValidateMultiplePages() {    
		logger.error('>>> Entering onValidateMultiplePages()', { "event.action": "onValidateMultiplePages" });
		logger.warn('<<< Exiting onValidateMultiplePages()', { "event.action": "onValidateMultiplePages" });
        return this.props.onValidateMultiplePages(()=>{
            // stay on same page
        });
    }
    
    onCancelAddMultiplePages() {
		logger.error('>>> Entering onCancelAddMultiplePages()', { "event.action": "onCancelAddMultiplePages" });
        const { props } = this;
        
        if (props.dirtyBulkPage) {
            const onConfirm = () => {
                props.onCancelAddMultiplePages();
            };

            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                onConfirm);
        } else {
            props.onCancelAddMultiplePages();
			logger.warn('<<< Exiting onCancelAddMultiplePages()', { "event.action": "onCancelAddMultiplePages" });
        }    
    }

    getAddPages() {
		logger.error('>>> Entering getAddPages()', { "event.action": "getAddPages" });
        const { props } = this;
		logger.warn('<<< Exiting getAddPages()', { "event.action": "getAddPages" });
        return (
            <AddPages
                bulkPage={props.bulkPage}
                onCancel={this.onCancelAddMultiplePages.bind(this)}
                onValidate={this.onValidateMultiplePages.bind(this)}
                onSave={this.onSaveMultiplePages.bind(this)}
                onChangeField={props.onChangeAddMultiplePagesField}
                components={props.multiplePagesComponents} />);
    }

    onCancelSaveCustomDetail(onCancelSave) {
		logger.error(`>>> Entering onCancelSaveCustomDetail(onCancelSave=${onCancelSave})`, { "event.action": "onCancelSaveCustomDetail", "logrite.onCancelSave": onCancelSave });
		logger.warn(`<<< Exiting onCancelSaveCustomDetail(onCancelSave=${onCancelSave})`, { "event.action": "onCancelSaveCustomDetail", "logrite.onCancelSave": onCancelSave });
        return ((isDirty) =>{
            if (isDirty) {
                const onConfirm = () => {
                    onCancelSave();
                };

                utils.confirm(
                    Localization.get("CancelWithoutSaving"),
                    Localization.get("Close"),
                    Localization.get("Cancel"),
                    onConfirm);
            } else {
                onCancelSave();
            }
        });
    }

    onChangeCustomDetail() {
	logger.error('>>> Entering onChangeCustomDetail()', { "event.action": "onChangeCustomDetail" });

	logger.warn('<<< Exiting onChangeCustomDetail()', { "event.action": "onChangeCustomDetail" });
    }
    
    onCancelSavePageAsTemplate() {
		logger.error('>>> Entering onCancelSavePageAsTemplate()', { "event.action": "onCancelSavePageAsTemplate" });
        const { props } = this;
        
        if (props.dirtyTemplate) {
            const onConfirm = () => {
                props.onCancelSavePageAsTemplate();
            };

            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                onConfirm);
        } else {
            props.onCancelSavePageAsTemplate();
			logger.warn('<<< Exiting onCancelSavePageAsTemplate()', { "event.action": "onCancelSavePageAsTemplate" });
        }
    }

    getSaveAsTemplatePage() {
		logger.error('>>> Entering getSaveAsTemplatePage()', { "event.action": "getSaveAsTemplatePage" });
        
		logger.warn('<<< Exiting getSaveAsTemplatePage()', { "event.action": "getSaveAsTemplatePage" });
        return (
            <SaveAsTemplate
                onCancel={this.onCancelSavePageAsTemplate.bind(this)} />);
    }


    _traverse(comparator, pageListCopy) {
		logger.error(`>>> Entering _traverse(comparator=${comparator},pageListCopy=${pageListCopy})`, { "event.action": "_traverse", "logrite.comparator": comparator, "logrite.pageListCopy": pageListCopy });
        let listItems = pageListCopy || JSON.parse(JSON.stringify(this.props.pageList));
        const cachedChildListItems = [];
		logger.error(`_traverse(comparator=${comparator},pageListCopy=${pageListCopy}): listItems → ${listItems}`, { "event.action": "_traverse", "logrite.comparator": comparator, "logrite.pageListCopy": pageListCopy });
        cachedChildListItems.push(listItems);
        const condition = cachedChildListItems.length > 0;

        const loop = () => {
			logger.error('>>> Entering loop()', { "event.action": "loop" });
            const childItem = cachedChildListItems.length ? cachedChildListItems.shift() : null;
			logger.error(`loop(): childItem → ${childItem}`, { "event.action": "loop" });
            const left = () => childItem.forEach(item => {
                comparator(item, listItems, (pageList) => this.props.updatePageListStore(pageList));
                Array.isArray(item.childListItems) ? cachedChildListItems.push(item.childListItems) : null;
                condition ? loop() : exit();
            });
            const right = () => null;
            childItem ? left() : right();
			logger.warn('<<< Exiting loop()', { "event.action": "loop" });
        };

        const exit = () => null;

        loop();
		logger.warn(`<<< Exiting _traverse(comparator=${comparator},pageListCopy=${pageListCopy})`, { "event.action": "_traverse", "logrite.comparator": comparator, "logrite.pageListCopy": pageListCopy });
        return;
    }

    setActivePage(pageInfo) {
		logger.error(`>>> Entering setActivePage(pageInfo=${pageInfo})`, { "event.action": "setActivePage", "logrite.pageInfo": pageInfo });
		logger.warn(`<<< Exiting setActivePage(pageInfo=${pageInfo})`, { "event.action": "setActivePage", "logrite.pageInfo": pageInfo });
        return new Promise((resolve) => {
            this.selectPageSettingTab(0);
            pageInfo.id = pageInfo.id || pageInfo.tabId;
            pageInfo.tabId = pageInfo.tabId || pageInfo.id;
            this.onLoadPage(pageInfo.tabId);
            resolve();
        });
    }

    onSelection(pageId) {
		logger.error(`>>> Entering onSelection(pageId=${pageId})`, { "event.action": "onSelection", "logrite.pageId": pageId });
        const { selectedPage, selectedPageDirty } = this.props;
        this.selectPageSettingTab(0);
        this.shouldRunRecursive = false;
        const left = () => {
			logger.debug(`onSelection(pageId=${pageId}): this.shouldRunRecursive → ${this.shouldRunRecursive}`, { "event.action": "onSelection", "logrite.pageId": pageId });
            if (!selectedPage || selectedPage.tabId !== pageId) {
                this.onLoadPage(pageId);
                this.selectPageSettingTab(0);
            }
        };
        const right = () => (!selectedPage || pageId !== selectedPage.tabId) ? this.showCancelWithoutSavingDialogInEditMode(pageId) : null;
        (!selectedPageDirty) ? left() : right();
		logger.warn(`<<< Exiting onSelection(pageId=${pageId})`, { "event.action": "onSelection", "logrite.pageId": pageId });
    }

    onNoPermissionSelection(pageId) {
		logger.error(`>>> Entering onNoPermissionSelection(pageId=${pageId})`, { "event.action": "onNoPermissionSelection", "logrite.pageId": pageId });
        const setNoPermissionState = () => {
			logger.error('>>> Entering setNoPermissionState()', { "event.action": "setNoPermissionState" });
            this.onCancelPage();
            this.buildBreadCrumbPath(pageId);
            this.noPermissionSelectionPageId = pageId;
            this.setEmptyStateMessage(Localization.get("NoPermissionEditPage"));
			logger.warn('<<< Exiting setNoPermissionState()', { "event.action": "setNoPermissionState" });
        };
        if (this.props.selectedPageDirty) {
			logger.debug(`onNoPermissionSelection(pageId=${pageId}): this.noPermissionSelectionPageId → ${this.noPermissionSelectionPageId}`, { "event.action": "onNoPermissionSelection", "logrite.pageId": pageId });
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Continue"),
                Localization.get("Go Back"),
                setNoPermissionState);
        }
        else {
            setNoPermissionState();
			logger.warn(`<<< Exiting onNoPermissionSelection(pageId=${pageId})`, { "event.action": "onNoPermissionSelection", "logrite.pageId": pageId });
        }
    }

    onChangePageField(key, value) {
		logger.error(`>>> Entering onChangePageField(key=${key},value=${value})`, { "event.action": "onChangePageField", "logrite.key": key, "logrite.value": value });
        if (this.props.selectedPage[key] !== value) {
            this.props.onChangePageField(key, value);
        }
        this.updatePageNameOnList(key, value);
		logger.warn(`<<< Exiting onChangePageField(key=${key},value=${value})`, { "event.action": "onChangePageField", "logrite.key": key, "logrite.value": value });
    }

    updatePageNameOnList(key, value) {
		logger.error(`>>> Entering updatePageNameOnList(key=${key},value=${value})`, { "event.action": "updatePageNameOnList", "logrite.key": key, "logrite.value": value });
        this._traverse((item, list, updateStore) => {
            if ((item.tabId === 0 || item.id === this.props.selectedPage.tabId) && key === "name") {
                item.name = value;
                item.selected = true;
                item.isOpen = true;
                updateStore(list);
				logger.warn(`<<< Exiting updatePageNameOnList(key=${key},value=${value})`, { "event.action": "updatePageNameOnList", "logrite.key": key, "logrite.value": value });
            } 
        });
    }

    onMovePage({ Action, PageId, ParentId, RelatedPageId }) {
		logger.error(`>>> Entering onMovePage(Action=${Action},PageId=${PageId},ParentId=${ParentId},RelatedPageId=${RelatedPageId})`, { "event.action": "onMovePage", "logrite.Action": Action, "logrite.PageId": PageId, "logrite.ParentId": ParentId, "logrite.RelatedPageId": RelatedPageId });
		logger.warn(`<<< Exiting onMovePage(Action=${Action},PageId=${PageId},ParentId=${ParentId},RelatedPageId=${RelatedPageId})`, { "event.action": "onMovePage", "logrite.Action": Action, "logrite.PageId": PageId, "logrite.ParentId": ParentId, "logrite.RelatedPageId": RelatedPageId });
        return PageActions.movePage({ Action, PageId, ParentId, RelatedPageId });
    }

    CallCustomAction(action) {
		logger.error(`>>> Entering CallCustomAction(action=${action})`, { "event.action": "CallCustomAction", "logrite.action": action });
        const { selectedPage, selectedPageDirty } = this.props;
        const callAction = () => {
			logger.error('>>> Entering callAction()', { "event.action": "callAction" });
            if (selectedPage && selectedPage.tabId !== 0 && selectedPageDirty) {
                const onConfirm = () => {
                    action();
                };
                utils.confirm(
                    Localization.get("CancelWithoutSaving"),
                    Localization.get("Close"),
                    Localization.get("Cancel"),
                    onConfirm);

            } else {
                action();
            }
			logger.warn('<<< Exiting callAction()', { "event.action": "callAction" });
        };
        callAction();
		logger.warn(`<<< Exiting CallCustomAction(action=${action})`, { "event.action": "CallCustomAction", "logrite.action": action });
    }

    onDuplicatePage(item) {
		logger.error(`>>> Entering onDuplicatePage(item=${item})`, { "event.action": "onDuplicatePage", "logrite.item": item });
        const { selectedPage, selectedPageDirty } = this.props;
        const message = Localization.get("NoPermissionCopyPage");
		logger.error(`onDuplicatePage(item=${item}): message → ${message}`, { "event.action": "onDuplicatePage", "logrite.item": item });
        const duplicate = () => {
			logger.error('>>> Entering duplicate()', { "event.action": "duplicate" });
            if (selectedPage && selectedPage.tabId !== 0 && selectedPageDirty) {
                const onConfirm = () => {
                    this.props.onDuplicatePage(true);
                };
                utils.confirm(
                    Localization.get("CancelWithoutSaving"),
                    Localization.get("Close"),
                    Localization.get("Cancel"),
                    onConfirm);

            } else {
                this.props.onDuplicatePage(false).then(data=>{
                    
                    this._addPageToTree(data.parentId);
                });
            }
            this.setState({inDuplicateMode: true});
			logger.warn('<<< Exiting duplicate()', { "event.action": "duplicate" });
        };
        const noPermission = () => this.setEmptyStateMessage(message);
        item.canCopyPage ? duplicate() : noPermission();
		logger.warn(`<<< Exiting onDuplicatePage(item=${item})`, { "event.action": "onDuplicatePage", "logrite.item": item });
    }

    onViewEditPage(item) {
		logger.error(`>>> Entering onViewEditPage(item=${item})`, { "event.action": "onViewEditPage", "logrite.item": item });
        const { selectedPageDirty } = this.props;
        const viewPage = () => PageActions.viewPage(item.id, item.url);

        const showConfirmationDialog = () => {
			logger.error('>>> Entering showConfirmationDialog()', { "event.action": "showConfirmationDialog" });
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                viewPage);
				logger.warn('<<< Exiting showConfirmationDialog()', { "event.action": "showConfirmationDialog" });
        };

        const proceed = () => selectedPageDirty ? showConfirmationDialog() : viewPage();

        this.clearEmptyStateMessage();
        const message = Localization.get("NoPermissionEditPage");
		logger.error(`onViewEditPage(item=${item}): message → ${message}`, { "event.action": "onViewEditPage", "logrite.item": item });
        const noPermission = () => this.setEmptyStateMessage(message);
		logger.warn(`<<< Exiting onViewEditPage(item=${item})`, { "event.action": "onViewEditPage", "logrite.item": item });
        item.canAddContentToPage ? proceed() : noPermission();

    }

    onViewPage(item) {
		logger.error(`>>> Entering onViewPage(item=${item})`, { "event.action": "onViewPage", "logrite.item": item });
        const { selectedPageDirty } = this.props;
        const view = () => {
			logger.error('>>> Entering view()', { "event.action": "view" });
            //this.onLoadPage(item.id);
            utils.getUtilities().closePersonaBar(function () {
                window.parent.location = item.url;
            });
			logger.warn('<<< Exiting view()', { "event.action": "view" });
        };

        const left = () => {
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                view);
        };

        const right = () => view();
        const proceed = () => selectedPageDirty ? left() : right();

        this.clearEmptyStateMessage();
        const message = Localization.get("NoPermissionViewPage");
		logger.error(`onViewPage(item=${item}): message → ${message}`, { "event.action": "onViewPage", "logrite.item": item });
        const noPermission = () => this.setEmptyStateMessage(message);
		logger.warn(`<<< Exiting onViewPage(item=${item})`, { "event.action": "onViewPage", "logrite.item": item });
        item.canViewPage ? proceed() : noPermission();

    }

    setEmptyStateMessage(emptyStateMessage) {
		logger.error(`>>> Entering setEmptyStateMessage(emptyStateMessage=${emptyStateMessage})`, { "event.action": "setEmptyStateMessage", "logrite.emptyStateMessage": emptyStateMessage });
        this.setState({ emptyStateMessage });
        this.props.clearSelectedPage();
		logger.warn(`<<< Exiting setEmptyStateMessage(emptyStateMessage=${emptyStateMessage})`, { "event.action": "setEmptyStateMessage", "logrite.emptyStateMessage": emptyStateMessage });
    }

    clearEmptyStateMessage() {
		logger.error('>>> Entering clearEmptyStateMessage()', { "event.action": "clearEmptyStateMessage" });
        this.noPermissionSelectionPageId = null;
        this.setState({ emptyStateMessage: null });
		logger.warn('<<< Exiting clearEmptyStateMessage()', { "event.action": "clearEmptyStateMessage" });
    }
    
    onDayClick(newDay, isEndDate) {
		logger.error(`>>> Entering onDayClick(newDay=${newDay},isEndDate=${isEndDate})`, { "event.action": "onDayClick", "logrite.newDay": newDay, "logrite.isEndDate": isEndDate });
        this.setState({ startAndEndDateDirty: true });
        const right = () => {
            const condition = newDay.getTime() < this.state.endDate.getTime();
            condition ? this.setState({ startDate: newDay, filtersUpdated: true }) : this.setState({ startDate: newDay, endDate: newDay, filtersUpdated: true });
			logger.error(`right(): condition → ${condition}`, { "event.action": "right" });
        };

        const left = () => {
            const condition = newDay.getTime() >= this.state.startDate.getTime();
            condition ? this.setState({ endDate: newDay, filtersUpdated: true }) : null;
			logger.error(`left(): condition → ${condition}`, { "event.action": "left" });
        };
        isEndDate ? left() : right();
		logger.warn(`<<< Exiting onDayClick(newDay=${newDay},isEndDate=${isEndDate})`, { "event.action": "onDayClick", "logrite.newDay": newDay, "logrite.isEndDate": isEndDate });
    }

    generateFilters() {
		logger.error('>>> Entering generateFilters()', { "event.action": "generateFilters" });
        const { filterByPageType, filterByPublishStatus, filterByWorkflow, filterByWorkflowName, startDate, endDate, startAndEndDateDirty } = this.state;
        const filters = this.state.tags.split(",")
            .filter(e => !!e)
            .map((tag) => {
                return { ref: `tag-${tag}`, tag: `${tag}` };
            });

        filterByPageType ? filters.push({ ref: "filterByPageType", tag: `${Localization.get("PageType")}: ${this.getPageTypeLabel(filterByPageType)}` }) : null;
        filterByPublishStatus ? filters.push({ ref: "filterByPublishStatus", tag: `${Localization.get("lblPublishStatus")}: ${this.getPublishStatusLabel(filterByPublishStatus)}` }) : null;
        filterByWorkflow ? filters.push({ ref: "filterByWorkflow", tag: `${Localization.get("WorkflowTitle")}: ${filterByWorkflowName}` }) : null;
        
        if (startAndEndDateDirty) {
            let dateRangeText = Localization.get("PublishedDateRange");
			logger.error(`generateFilters(): dateRangeText → ${dateRangeText}`, { "event.action": "generateFilters" });
            const fullStartDate = utils.formatDate(startDate);
			logger.error(`generateFilters(): fullStartDate → ${fullStartDate}`, { "event.action": "generateFilters" });
            const fullEndDate = utils.formatDate(endDate);
			logger.error(`generateFilters(): fullEndDate → ${fullEndDate}`, { "event.action": "generateFilters" });
            const dateInterval = () => filters.push({ ref: "startAndEndDateDirty", tag: `${dateRangeText}: ${fullStartDate} - ${fullEndDate} ` });
            const justOneDate = () => filters.push({ ref: "startAndEndDateDirty", tag: `${dateRangeText}: ${fullStartDate}` });

            fullStartDate !== fullEndDate ? dateInterval() : justOneDate();
			logger.warn('<<< Exiting generateFilters()', { "event.action": "generateFilters" });
        }
    }

    saveSearchFilters(searchFields) {
		logger.error(`>>> Entering saveSearchFilters(searchFields=${searchFields})`, { "event.action": "saveSearchFilters", "logrite.searchFields": searchFields });
		logger.warn(`<<< Exiting saveSearchFilters(searchFields=${searchFields})`, { "event.action": "saveSearchFilters", "logrite.searchFields": searchFields });
        return new Promise((resolve) => this.setState({ searchFields }, () => resolve()));
    }

    
    onSearch(term) {
		logger.error(`>>> Entering onSearch(term=${term})`, { "event.action": "onSearch", "logrite.term": term });
        let newTerm = (term !== undefined) ? term : this.state.searchTerm;
        this.setState({
            searchTerm: newTerm,
            filtersUpdated: true,
            pageIndex:0
        },()=>{
            const { selectedPage } = this.props;
            const { filtersUpdated } = this.state;
            if (filtersUpdated) {
                if (selectedPage) {
                    this.lastActivePageId = selectedPage.tabId;
                    if (this.props.selectedPageDirty) {
                        this.showCancelWithoutSavingDialogAndRun(() => {
                            this.doSearch();
                        }, () => {
                            this.clearSearch();
                        });
                    } else {
                        this.doSearch();
                    }
                }
                else {
                    this.doSearch();
                }          
            }
            this.setState({ DropdownCalendarIsActive: null});
        });
		logger.warn(`<<< Exiting onSearch(term=${term})`, { "event.action": "onSearch", "logrite.term": term });
    }

    doSearch() {
		logger.error('>>> Entering doSearch()', { "event.action": "doSearch" });
        const { selectedPage } = this.props;
        if (selectedPage && this.props.selectedPageDirty) {
            this.onCancelPage(selectedPage.tabId);
        }

        let { searchTerm, filterByPageType, filterByPublishStatus, filterByWorkflow, startDate, endDate, startAndEndDateDirty, tags } = this.state;
        const fullStartDate = `${startDate.getDate() < 10 ? "0" + startDate.getDate() : startDate.getDate()}/${((startDate.getMonth() + 1) < 10 ? "0" + (startDate.getMonth() + 1) : (startDate.getMonth() + 1))}/${startDate.getFullYear()} 00:00:00`;
		logger.error(`doSearch(): fullStartDate → ${fullStartDate}`, { "event.action": "doSearch" });
        const fullEndDate = `${endDate.getDate() < 10 ? "0" + endDate.getDate() : endDate.getDate()}/${((endDate.getMonth() + 1) < 10 ? "0" + (endDate.getMonth() + 1) : (endDate.getMonth() + 1))}/${endDate.getFullYear()} 23:59:59`;
		logger.error(`doSearch(): fullEndDate → ${fullEndDate}`, { "event.action": "doSearch" });
        const searchDateRange = startAndEndDateDirty ? { publishDateStart: fullStartDate, publishDateEnd: fullEndDate } : {};

        if (tags) {
            tags = tags[0] === "," ? tags.replace(",", "") : tags;
            tags = tags[tags.length - 1] === "," ? tags.split(",").filter(t => !!t).join() : tags;
			logger.error(`doSearch(): tags → ${tags}`, { "event.action": "doSearch" });
        }

        let search = { 
            tags: tags, 
            searchKey: searchTerm, 
            pageType: filterByPageType, 
            publishStatus: filterByPublishStatus, 
            workflowId: filterByWorkflow,
            pageSize: 10 
        };
        
        search = Object.assign({}, search, searchDateRange);
		logger.error(`doSearch(): search → ${search}`, { "event.action": "doSearch" });
        for (let prop in search) {
            if (!search[prop]) {
                delete search[prop];
            }
        }
        
        search = Object.assign({},search,{pageIndex:this.state.pageIndex});
        
        this.generateFilters();
        const filterUp = this.state.filtersUpdated;
        const filterPage = this.state.pageIndex;
        
        this.saveSearchFilters(search).then(() => {
            this.props.searchAndFilterPagedPageList(search,filterUp,filterPage);
        });        
		logger.warn('<<< Exiting doSearch()', { "event.action": "doSearch" });
        this.setState({ 
            inSearch: true, 
            filtersUpdated: false 
        });
    }

    onSearchScroll(page) {
		logger.error(`>>> Entering onSearchScroll(page=${page})`, { "event.action": "onSearchScroll", "logrite.page": page });
        this.setState({
            pageIndex: page
        },()=>{
            this.doSearch();
        });
		logger.warn(`<<< Exiting onSearchScroll(page=${page})`, { "event.action": "onSearchScroll", "logrite.page": page });
    }
    
    clearAdvancedSearch() {
		logger.error('>>> Entering clearAdvancedSearch()', { "event.action": "clearAdvancedSearch" });
        let date = new Date();
        this.setState({
            startDate: date,
            endDate: date,
            defaultDate: date,
            startAndEndDateDirty: false,
            filterByPageType: null,
            filterByPublishStatus: null,
            filterByWorkflow: null,
            workflowList: [],
            tags: "",
            filters: [],
            searchFields: {},
            filtersUpdated: true,
            pageIndex:0
        },()=>this.onSearch(this.state.searchTerm));
		logger.warn('<<< Exiting clearAdvancedSearch()', { "event.action": "clearAdvancedSearch" });
    }

    clearAdvancedSearchDateInterval() {
		logger.error('>>> Entering clearAdvancedSearchDateInterval()', { "event.action": "clearAdvancedSearchDateInterval" });
        let date = new Date();
        this.setState({
            startDate:date,
            endDate: date,
            startAndEndDateDirty:false
        });
		logger.warn('<<< Exiting clearAdvancedSearchDateInterval()', { "event.action": "clearAdvancedSearchDateInterval" });
    }
    
    clearSearch(callback) {
		logger.error(`>>> Entering clearSearch(callback=${callback})`, { "event.action": "clearSearch", "logrite.callback": callback });
        let date = new Date();
        this.setState({
            filtersUpdated: true,
            pageIndex:0,
            inSearch: false,
            searchTerm: "",
            startDate: date,
            endDate: date,
            defaultDate: date,
            startAndEndDateDirty: false,
            filterByPageType: null,
            filterByPublishStatus: null,
            filterByWorkflow: null,
            workflowList: [],
            tags: "",
            filters: [],
            searchFields: {}
        }, () => {
            if (typeof callback === "function") {
                callback();
            }
            else {
                const { selectedPage } = this.props;
                !selectedPage && this.lastActivePageId && this.onLoadPage(this.lastActivePageId);
				logger.warn(`<<< Exiting clearSearch(callback=${callback})`, { "event.action": "clearSearch", "logrite.callback": callback });
            }
        });
    }
    showCancelWithoutSavingDialogAndRun(callback, cancelCallback) {
	logger.error(`>>> Entering showCancelWithoutSavingDialogAndRun(callback=${callback},cancelCallback=${cancelCallback})`, { "event.action": "showCancelWithoutSavingDialogAndRun", "logrite.callback": callback, "logrite.cancelCallback": cancelCallback });

        const onConfirm = () => {
            if (typeof callback === "function") {
                callback();
            }
        };
        const onCancel = () => {
			logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
            if (typeof cancelCallback === "function") {
                cancelCallback();
            }
        };
        utils.confirm(
            Localization.get("CancelWithoutSaving"),
            Localization.get("Close"),
            Localization.get("Cancel"),
            onConfirm,
            onCancel);
			logger.warn(`<<< Exiting showCancelWithoutSavingDialogAndRun(callback=${callback},cancelCallback=${cancelCallback})`, { "event.action": "showCancelWithoutSavingDialogAndRun", "logrite.callback": callback, "logrite.cancelCallback": cancelCallback });
    }
    buildBreadCrumbPath(pageId) {
		logger.error(`>>> Entering buildBreadCrumbPath(pageId=${pageId})`, { "event.action": "buildBreadCrumbPath", "logrite.pageId": pageId });
        let page = {};
        let selectedPath = [];
        const buildBreadCrumbPathInternal = () => {
			logger.error('>>> Entering buildBreadCrumbPathInternal()', { "event.action": "buildBreadCrumbPathInternal" });
            const addNode = (tabId) => {
				logger.error(`>>> Entering addNode(tabId=${tabId})`, { "event.action": "addNode", "logrite.tabId": tabId });
                this._traverse((item) => {
                    if (item.id === tabId) {
                        page = item;
                        return;
                    }
                });
                
                selectedPath.push({ name: page.name, tabId: (page.id !== pageId ? page.id : null) });
                const left = () => {
                    addNode(page.parentId);
                };
                const right = () => {
                    selectedPath.reverse();
                    this.props.changeSelectedPagePath(selectedPath);
                };
                page.parentId > 0 ? left() : right();
				logger.warn(`<<< Exiting addNode(tabId=${tabId})`, { "event.action": "addNode", "logrite.tabId": tabId });
            };
            addNode(pageId);
			logger.warn('<<< Exiting buildBreadCrumbPathInternal()', { "event.action": "buildBreadCrumbPathInternal" });
        };
        buildBreadCrumbPathInternal();
		logger.warn(`<<< Exiting buildBreadCrumbPath(pageId=${pageId})`, { "event.action": "buildBreadCrumbPath", "logrite.pageId": pageId });
    }

    render_PagesDetailEditor() {
	logger.error('>>> Entering render_PagesDetailEditor()', { "event.action": "render_PagesDetailEditor" });

        const render_emptyState = () => {
			logger.error('>>> Entering render_emptyState()', { "event.action": "render_emptyState" });
            const DefaultMessage = Localization.get("NoPageSelected");
            return (
                <div className="empty-page-state">
                    <div className="empty-page-state-message">
                        <h1>{this.state.emptyStateMessage || DefaultMessage}</h1>
                        <p>Select a page in the tree to manage its settings here.</p>
                    </div>
                </div>
            );
        };


        const render_pageDetails = () => {
			logger.error('>>> Entering render_pageDetails()', { "event.action": "render_pageDetails" });
            const { props } = this;
            const { isContentLocalizationEnabled } = props;
            return (
                <PageSettings key={"pageDetails" + this.props.selectedPage.tabId}
                    selectedPage={this.props.selectedPage}
                    AllowContentLocalization={isContentLocalizationEnabled}
                    selectedPageErrors={{}}
                    selectedPageDirty={props.selectedPageDirty}
                    onCancel={this.showCancelWithoutSavingDialogInEditMode.bind(this)}
                    onDelete={this.onDeleteSettings.bind(this)}
                    onSave={this.onUpdatePage.bind(this)}
                    selectedPageSettingTab={props.selectedPageSettingTab}
                    selectPageSettingTab={this.selectPageSettingTab.bind(this)}
                    onChangeField={this.onChangePageField.bind(this)}
                    onChangeParentId={this.onChangeParentId.bind(this)}
                    onPermissionsChanged={props.onPermissionsChanged}
                    onChangePageType={props.onChangePageType.bind(this)}
                    onDeletePageModule={props.onDeletePageModule}
                    onEditingPageModule={props.onEditingPageModule}
                    onCancelEditingPageModule={props.onCancelEditingPageModule}
                    editingSettingModuleId={props.editingSettingModuleId}
                    onCopyAppearanceToDescendantPages={props.onCopyAppearanceToDescendantPages}
                    onCopyPermissionsToDescendantPages={props.onCopyPermissionsToDescendantPages}
                    pageDetailsFooterComponents={props.pageDetailsFooterComponents}
                    pageTypeSelectorComponents={props.pageTypeSelectorComponents}
                    onGetCachedPageCount={props.onGetCachedPageCount}
                    onClearCache={props.onClearCache}
                    onModuleCopyChange={props.onModuleCopyChange}
                    customPageSettingsComponents={props.customPageSettingsComponents}
                />
            );
        };
        const { selectedPage } = this.props;
		logger.warn('<<< Exiting render_PagesDetailEditor()', { "event.action": "render_PagesDetailEditor" });
        return (
            <GridCell columnSize={100} className="treeview-page-details" >
                {(selectedPage && selectedPage.tabId) ? render_pageDetails() : render_emptyState()}
            </GridCell>
        );
    }

    render_addPageEditor() {
		logger.error('>>> Entering render_addPageEditor()', { "event.action": "render_addPageEditor" });
        const { props } = this;
        const cancelAction = this.onCancelSettings.bind(this);
		logger.error(`render_addPageEditor(): cancelAction → ${cancelAction}`, { "event.action": "render_addPageEditor" });
        const deleteAction = this.onDeleteSettings.bind(this);
		logger.error(`render_addPageEditor(): deleteAction → ${deleteAction}`, { "event.action": "render_addPageEditor" });
        const AllowContentLocalization = !!props.isContentLocalizationEnabled;
		logger.warn('<<< Exiting render_addPageEditor()', { "event.action": "render_addPageEditor" });
        return (
            <GridCell columnSize={100} className="treeview-page-details" >
                <PageSettings key="newPageSettings" selectedPage={props.selectedPage}
                    AllowContentLocalization={AllowContentLocalization}
                    selectedPageErrors={props.selectedPageErrors}
                    selectedPageDirty={props.selectedPageDirty}
                    onCancel={cancelAction}
                    onDelete={deleteAction}
                    onSave={this.onCreatePage.bind(this)}
                    selectedPageSettingTab={props.selectedPageSettingTab}
                    selectPageSettingTab={this.selectPageSettingTab.bind(this)}
                    onChangeField={this.onChangePageField.bind(this)}
                    onChangeParentId={this.onChangeParentId.bind(this)}
                    onPermissionsChanged={props.onPermissionsChanged}
                    onChangePageType={props.onChangePageType}
                    onDeletePageModule={props.onDeletePageModule}
                    onEditingPageModule={props.onEditingPageModule}
                    onCancelEditingPageModule={props.onCancelEditingPageModule}
                    editingSettingModuleId={props.editingSettingModuleId}
                    onCopyAppearanceToDescendantPages={props.onCopyAppearanceToDescendantPages}
                    onCopyPermissionsToDescendantPages={props.onCopyPermissionsToDescendantPages}
                    pageDetailsFooterComponents={props.pageDetailsFooterComponents}
                    pageTypeSelectorComponents={props.pageTypeSelectorComponents}
                    onGetCachedPageCount={props.onGetCachedPageCount}
                    onClearCache={props.onClearCache}
                    onModuleCopyChange={props.onModuleCopyChange} />
            </GridCell>
        );
    }


    render_pageList() {
		logger.error('>>> Entering render_pageList()', { "event.action": "render_pageList" });
		logger.warn('<<< Exiting render_pageList()', { "event.action": "render_pageList" });
        return (
            <PageList onPageSettings={this.onPageSettings.bind(this)} />
        );
    }
    
    getFilterByPageTypeOptions() {
		logger.error('>>> Entering getFilterByPageTypeOptions()', { "event.action": "getFilterByPageTypeOptions" });
		logger.warn('<<< Exiting getFilterByPageTypeOptions()', { "event.action": "getFilterByPageTypeOptions" });
        return [
            { value: "", label: Localization.get("lblNone") },
            { value: "normal", label: Localization.get("lblNormal") },
            { value: "tab", label: Localization.get("Existing") },
            { value: "url", label: Localization.get("lblUrl") },
            { value: "file", label: Localization.get("lblFile") }
        ];
    }

    getFilterByPageStatusOptions() {
		logger.error('>>> Entering getFilterByPageStatusOptions()', { "event.action": "getFilterByPageStatusOptions" });
        let filterByPageStatusOptions = [
            { value: "published", label: Localization.get("lblPublished") }
        ];
		logger.warn('<<< Exiting getFilterByPageStatusOptions()', { "event.action": "getFilterByPageStatusOptions" });
        return ([{ value: "", label: Localization.get("lblNone") }]).concat(filterByPageStatusOptions.concat([{ value: "draft", label: Localization.get("lblDraft") }]));
    }
    getFilterByWorkflowOptions() {
		logger.error('>>> Entering getFilterByWorkflowOptions()', { "event.action": "getFilterByWorkflowOptions" });
        let workflowList = [];
        if (this.props.workflowList.length <= 0) {
            this.props.getWorkflowsList();
        }
        this.props.workflowList.length ? workflowList = this.props.workflowList.map((item => { return { value: item.workflowId, label: item.workflowName }; })) : null;
		logger.warn('<<< Exiting getFilterByWorkflowOptions()', { "event.action": "getFilterByWorkflowOptions" });
        return [{ value: "", label: Localization.get("lblNone") }].concat(workflowList);
    }
    getPageTypeLabel(pageType) {
		logger.error(`>>> Entering getPageTypeLabel(pageType=${pageType})`, { "event.action": "getPageTypeLabel", "logrite.pageType": pageType });
        const filterByPageTypeOptions = this.getFilterByPageTypeOptions();
		logger.error(`getPageTypeLabel(pageType=${pageType}): filterByPageTypeOptions → ${filterByPageTypeOptions}`, { "event.action": "getPageTypeLabel", "logrite.pageType": pageType });
        return filterByPageTypeOptions.find(x => x.value === pageType.toLowerCase()) && filterByPageTypeOptions.find(x => x.value === pageType.toLowerCase()).label;
    }
    getPublishStatusLabel(publishStatus) {
		logger.error(`>>> Entering getPublishStatusLabel(publishStatus=${publishStatus})`, { "event.action": "getPublishStatusLabel", "logrite.publishStatus": publishStatus });
        const filterByPublishStatusOptions = this.getFilterByPageStatusOptions();
		logger.error(`getPublishStatusLabel(publishStatus=${publishStatus}): filterByPublishStatusOptions → ${filterByPublishStatusOptions}`, { "event.action": "getPublishStatusLabel", "logrite.publishStatus": publishStatus });
        return filterByPublishStatusOptions.find(x => x.value === publishStatus.toLowerCase()) && filterByPublishStatusOptions.find(x => x.value === publishStatus.toLowerCase()).label || publishStatus;
    }

    updateSearchAdvancedTags(tags) {
		logger.error(`>>> Entering updateSearchAdvancedTags(tags=${tags})`, { "event.action": "updateSearchAdvancedTags", "logrite.tags": tags });
        this.setState({tags:tags,filtersUpdated:true, pageIndex:0});
		logger.warn(`<<< Exiting updateSearchAdvancedTags(tags=${tags})`, { "event.action": "updateSearchAdvancedTags", "logrite.tags": tags });
    }

    onApplyChangesDropdownDayPicker() {
		logger.error('>>> Entering onApplyChangesDropdownDayPicker()', { "event.action": "onApplyChangesDropdownDayPicker" });
        const { startAndEndDateDirty, startDate, endDate } = this.state;
        const fullStartDate = startDate.getDate() + startDate.getMonth() + startDate.getFullYear();
        const fullEndDate = endDate.getDate() + endDate.getMonth() + endDate.getFullYear();
		logger.error(`onApplyChangesDropdownDayPicker(): fullEndDate → ${fullEndDate}`, { "event.action": "onApplyChangesDropdownDayPicker" });
        const condition = !startAndEndDateDirty && fullStartDate === fullEndDate;
        condition ? this.setState({ startAndEndDateDirty: true, DropdownCalendarIsActive: null }) : this.setState({ DropdownCalendarIsActive: null });
		logger.error(`onApplyChangesDropdownDayPicker(): fullStartDate → ${fullStartDate}`, { "event.action": "onApplyChangesDropdownDayPicker" });
    }
    
    updateFilterByPageTypeOptions(data) { 
		logger.error(`>>> Entering updateFilterByPageTypeOptions(data=${data})`, { "event.action": "updateFilterByPageTypeOptions", "logrite.data": data });
        this.setState({ filterByPageType: data.value, filtersUpdated: true,pageIndex:0 }); 
		logger.warn(`<<< Exiting updateFilterByPageTypeOptions(data=${data})`, { "event.action": "updateFilterByPageTypeOptions", "logrite.data": data });
    }
    
    updateFilterByPageStatusOptions(data) {
		logger.error(`>>> Entering updateFilterByPageStatusOptions(data=${data})`, { "event.action": "updateFilterByPageStatusOptions", "logrite.data": data });
        this.setState({ filterByPublishStatus: data.value, filtersUpdated: true, pageIndex:0 });
		logger.warn(`<<< Exiting updateFilterByPageStatusOptions(data=${data})`, { "event.action": "updateFilterByPageStatusOptions", "logrite.data": data });
    } 
    updateFilterByWorkflowOptions(data) {
		logger.error(`>>> Entering updateFilterByWorkflowOptions(data=${data})`, { "event.action": "updateFilterByWorkflowOptions", "logrite.data": data });
        this.setState({ filterByWorkflow: data.value, filterByWorkflowName: data.label, filtersUpdated: true, pageIndex:0});
		logger.warn(`<<< Exiting updateFilterByWorkflowOptions(data=${data})`, { "event.action": "updateFilterByWorkflowOptions", "logrite.data": data });
    }

    updateFilterStartEndDate(startDate, endDate) {
		logger.error(`>>> Entering updateFilterStartEndDate(startDate=${startDate},endDate=${endDate})`, { "event.action": "updateFilterStartEndDate", "logrite.startDate": startDate, "logrite.endDate": endDate });
        this.setState({
            startDate,endDate,
            startAndEndDateDirty:true,
            filtersUpdated:true,
            pageIndex:0
        });
		logger.warn(`<<< Exiting updateFilterStartEndDate(startDate=${startDate},endDate=${endDate})`, { "event.action": "updateFilterStartEndDate", "logrite.startDate": startDate, "logrite.endDate": endDate });
    }

    render_searchResults() {
		logger.error('>>> Entering render_searchResults()', { "event.action": "render_searchResults" });
		logger.warn('<<< Exiting render_searchResults()', { "event.action": "render_searchResults" });
        return (
            <SearchResult 
                filters={this.state.filters} 
                tags={this.state.tags}
                filterPageByType={this.state.filterByPageType}
                filterByPublishStatus={this.state.filterByPublishStatus}
                filtersUpdated={this.state.filtersUpdated}
                startDate={this.state.startDate} 
                endDate={this.state.endDate}
                getPageTypeLabel={this.getPageTypeLabel.bind(this)}
                getPublishStatusLabel={this.getPublishStatusLabel.bind(this)}
                getFilterByPageTypeOptions={this.getFilterByPageTypeOptions.bind(this)}
                getFilterByPageStatusOptions={this.getFilterByPageStatusOptions.bind(this)}
                getFilterByWorkflowOptions={this.getFilterByWorkflowOptions.bind(this)}
                filterByWorkflow={this.state.filterByWorkflow}
                onApplyChangesDropdownDayPicker={this.onApplyChangesDropdownDayPicker.bind(this)}
                updateFilterByPageTypeOptions={this.updateFilterByPageTypeOptions.bind(this)}
                updateFilterByPageStatusOptions={this.updateFilterByPageStatusOptions.bind(this)}
                updateFilterByWorkflowOptions={this.updateFilterByWorkflowOptions.bind(this)}
                filterByPageType={this.state.filterByPageType}
                onDayClick={this.onDayClick.bind(this)}
                startAndEndDateDirty={this.state.startAndEndDateDirty}
                onSearch={this.onSearch.bind(this)}
                clearSearch={this.clearSearch.bind(this)}
                clearAdvancedSearch={this.clearAdvancedSearch.bind(this)}
                clearAdvancedSearchDateInterval={this.clearAdvancedSearchDateInterval.bind(this)}
                buildBreadCrumbPath={this.buildBreadCrumbPath.bind(this)} 
                setEmptyStateMessage={this.setEmptyStateMessage.bind(this)}
                onViewPage={this.onViewPage.bind(this)}
                onViewEditPage={this.onViewEditPage.bind(this)}
                CallCustomAction={this.CallCustomAction.bind(this)}
                onLoadPage={this.onLoadPage.bind(this)}
                updateSearchAdvancedTags={this.updateSearchAdvancedTags.bind(this)}
                updateFilterStartEndDate={this.updateFilterStartEndDate.bind(this)}
                buildTree={this.buildTree.bind(this)}
                onSearchScroll={this.onSearchScroll.bind(this)}
                pageIndex={this.state.pageIndex}
            />
        );
    }
    
    getAdditionalPageSettings() {
		logger.error('>>> Entering getAdditionalPageSettings()', { "event.action": "getAdditionalPageSettings" });
        const additionalPageSettings = [];

        const { props } = this;
        
        if (props.customPageSettingsComponents) {
            for (let i = 0; i < props.customPageSettingsComponents.length; i++) {
                const customPageSettings = props.customPageSettingsComponents[i];
                if (props.selectedCustomPageSettings.pageSettingsId === customPageSettings.panelId) {
                    const Component = customPageSettings.component;
                    additionalPageSettings.push(
                        <Component
                            onCancel={this.onCancelSaveCustomDetail(this.props.onCancelSavePageAsTemplate)} 
                            onChange={this.props.onChangeCustomDetails}
                            selectedPage={props.selectedPage}
                            disabled={this.onEditMode()}
                            store={customPageSettings.store} />
                    );
                }
            }
        }

		logger.warn('<<< Exiting getAdditionalPageSettings()', { "event.action": "getAdditionalPageSettings" });
        return additionalPageSettings;
    }
    
    render_details() {
		logger.error('>>> Entering render_details()', { "event.action": "render_details" });
        const {selectedPage} = this.props;
        const {inSearch} = this.state;
        const {selectedView} = this.props;

        switch (true) {
            case inSearch:
                return this.render_searchResults();
            case selectedPage && selectedPage.tabId === 0:
                return this.render_addPageEditor();
            case selectedView === panels.ADD_MULTIPLE_PAGES_PANEL:
                return this.getAddPages();
            case selectedView === panels.SAVE_AS_TEMPLATE_PANEL:
                return this.getSaveAsTemplatePage();
            case selectedView === panels.CUSTOM_PAGE_DETAIL_PANEL:
                return this.getAdditionalPageSettings();
            case !selectedPage:
            default:
				logger.warn('<<< Exiting render_details()', { "event.action": "render_details" });
                return this.render_PagesDetailEditor();
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;
        const { selectedPage } = props;
        const { inSearch } = this.state;
        const isListPagesAllowed = securityService.canSeePagesList();
		logger.error(`render(): isListPagesAllowed → ${isListPagesAllowed}`, { "event.action": "render" });
       
         
		logger.debug(`render(): inSearch → ${inSearch}`, { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (

            <div ref={node => this.node = node} className="pages-app personaBar-mainContainer">
                { isListPagesAllowed &&
                    <PersonaBarPage fullWidth={true} isOpen={true}>
                        <PersonaBarPageHeader title={Localization.get(inSearch ? "PagesSearchHeader" : "Pages")}>
                            {securityService.canAddPages() &&
                                <div>
                                    <Button type="primary" disabled={ this.onEditMode()  || this.state.inSearch} size="large" onClick={this.onAddPage.bind(this)}>{Localization.get("AddPage")}</Button>
                                    <Button type="secondary" disabled={ this.onEditMode() || this.state.inSearch } size="large" onClick={this.onAddMultiplePage.bind(this)}>{Localization.get("AddMultiplePages")}</Button>
                                </div>
                            }
                            { 
                                selectedPage && this.getSettingsButtons()
                            }                            
                            {!inSearch && <BreadCrumbs items={this.props.selectedPagePath || []} onSelectedItem={this.onSelection.bind(this)} />}
                        </PersonaBarPageHeader>
                        <GridCell columnSize={100} style={{ padding: "30px 30px 16px 30px" }}>
                            <SearchPageInput 
                                inSearch={this.state.inSearch} 
                                onSearch={this.onSearch.bind(this)}
                                clearSearch={this.clearSearch.bind(this)}  />
                        </GridCell>
                        <GridCell columnSize={100} style={{ padding: "0px 30px 30px 30px" }} >
                            <GridCell columnSize={1096} type={"px"} className="page-container">
                                <div className={(this.onEditMode()|| this.state.inSearch) ? "tree-container disabled" : "tree-container"}>
                                    <PersonaBarPageTreeviewInteractor
                                        clearSelectedPage={this.props.clearSelectedPage}
                                        Localization={Localization}
                                        pageList={this.props.pageList}
                                        getChildPageList={this.props.getChildPageList}
                                        getPage={this.props.getPage.bind(this)}
                                        _traverse={this._traverse.bind(this)}
                                        showCancelDialog={this.showCancelWithoutSavingDialogInEditMode.bind(this)}
                                        selectedPageDirty={this.props.selectedPageDirty}
                                        setEmptyPageMessage={this.setEmptyStateMessage.bind(this)}
                                        setActivePage={this.setActivePage.bind(this)}
                                        saveDropState={this.onUpdatePage.bind(this)}
                                        onMovePage={this.onMovePage.bind(this)}
                                        onViewPage={this.onViewPage.bind(this)}
                                        onViewEditPage={this.onViewEditPage.bind(this)}
                                        onDuplicatePage={this.onDuplicatePage.bind(this)}
                                        onAddPage={this.onAddPage.bind(this)}
                                        onSelection={this.onSelection.bind(this)}
                                        onNoPermissionSelection={this.onNoPermissionSelection.bind(this)}
                                        pageInContextComponents={props.pageInContextComponents}
                                        NoPermissionSelectionPageId={this.noPermissionSelectionPageId}
                                        CallCustomAction={this.CallCustomAction.bind(this)}
                                        enabled={!((selectedPage && selectedPage.tabId === 0) || inSearch)} />
                                </div>
                                <GridCell columnSize={760} type={"px"}>
                                    {this.render_details()}
                                </GridCell>
                            </GridCell>
                        </GridCell>
                    </PersonaBarPage>
                }
               
            </div>
        );
    }
}

App.propTypes = {
    pageList: PropTypes.array.isRequired,
    searchPageList: PropTypes.func.isRequired,
    searchAndFilterPagedPageList: PropTypes.func.isRequired,
    getChildPageList: PropTypes.func.isRequired,
    getWorkflowsList: PropTypes.func.isRequired,
    selectedView: PropTypes.number,
    selectedPage: PropTypes.object,
    selectedPageErrors: PropTypes.object,
    selectedPageDirty: PropTypes.bool,
    selectedTemplateDirty: PropTypes.bool,
    filtersUpdated: PropTypes.bool,
    bulkPage: PropTypes.object,
    dirtyBulkPage : PropTypes.bool, 
    editingSettingModuleId: PropTypes.number,
    onCancelPage: PropTypes.func.isRequired,
    onCreatePage: PropTypes.func.isRequired,
    onUpdatePage: PropTypes.func.isRequired,
    onDeletePage: PropTypes.func.isRequired,
    getPageList: PropTypes.func.isRequired,
    getPage: PropTypes.func.isRequired,
    updatePageListStore: PropTypes.func.isRequired,
    getNewPage: PropTypes.func.isRequired,
    onLoadPage: PropTypes.func.isRequired,
    onCancelAddMultiplePages: PropTypes.func.isRequired,
    onValidateMultiplePages: PropTypes.func.isRequired,
    onSaveMultiplePages: PropTypes.func.isRequired,
    onLoadAddMultiplePages: PropTypes.func.isRequired,
    onChangeAddMultiplePagesField: PropTypes.func.isRequired,
    onChangePageField: PropTypes.func.isRequired,
    onChangePageType: PropTypes.func.isRequired,
    onPermissionsChanged: PropTypes.func.isRequired,
    onDeletePageModule: PropTypes.func.isRequired,
    onEditingPageModule: PropTypes.func.isRequired,
    onCancelEditingPageModule: PropTypes.func.isRequired,
    onCopyAppearanceToDescendantPages: PropTypes.func.isRequired,
    onCopyPermissionsToDescendantPages: PropTypes.func.isRequired,
    onLoadSavePageAsTemplate: PropTypes.func.isRequired,
    onCancelSavePageAsTemplate: PropTypes.func.isRequired,
    onDuplicatePage: PropTypes.func.isRequired,
    error: PropTypes.object,
    multiplePagesComponents: PropTypes.array.isRequired,
    pageDetailsFooterComponents: PropTypes.array.isRequired,
    settingsButtonComponents: PropTypes.object.isRequired,
    pageTypeSelectorComponents: PropTypes.array.isRequired,
    pageInContextComponents: PropTypes.array.isRequired,
    selectedPageSettingTab: PropTypes.number,
    selectPageSettingTab: PropTypes.func,
    additionalPanels: PropTypes.array.isRequired,
    onShowPanel: PropTypes.func.isRequired,
    onHidePanel: PropTypes.func.isRequired,
    onShowPageSettings: PropTypes.func,
    onHidePageSettings: PropTypes.func,
    isContentLocalizationEnabled: PropTypes.bool,
    getContentLocalizationEnabled: PropTypes.func.isRequired,
    selectPage: PropTypes.func.isRequired,
    selectedPagePath: PropTypes.array.isRequired,
    changeSelectedPagePath: PropTypes.func.isRequired,
    onGetCachedPageCount: PropTypes.func,
    onClearCache: PropTypes.func.isRequired,
    clearSelectedPage: PropTypes.func.isRequired,
    onModuleCopyChange: PropTypes.func,
    workflowList: PropTypes.array.isRequired,
    customPageSettingsComponents: PropTypes.array,
    getPageHierarchy: PropTypes.func.isRequired,
    dirtyTemplate: PropTypes.bool,
    dirtyCustomDetails: PropTypes.bool,
    onChangeCustomDetails: PropTypes.func,
    selectedCustomPageSettings: PropTypes.object,
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        pageList: state.pageList.pageList,
        selectedView: state.visiblePanel.selectedPage,
        selectedCustomPageSettings : state.visiblePageSettings.panelId,
        selectedPage: state.pages.selectedPage,
        selectedPageErrors: state.pages.errors,
        selectedPageDirty: state.pages.dirtyPage,
        filtersUpdated: state.pages.filtersUpdated,
        dirtyTemplate: state.template.dirtyTemplate,
        dirtyEvoqTemplate: state.template.dirtyEvoqTemplate,
        bulkPage: state.addPages.bulkPage,
        dirtyBulkPage : state.addPages.dirtyBulkPage,
        editingSettingModuleId: state.pages.editingSettingModuleId,
        error: state.errors.error,
        multiplePagesComponents: state.extensions.multiplePagesComponents,
        pageDetailsFooterComponents: state.extensions.pageDetailsFooterComponents,
        pageInContextComponents: state.extensions.pageInContextComponents,
        settingsButtonComponents: state.extensions.settingsButtonComponents,
        pageTypeSelectorComponents: state.extensions.pageTypeSelectorComponents,
        selectedPageSettingTab: state.pages.selectedPageSettingTab,
        additionalPanels: state.extensions.additionalPanels,
        isContentLocalizationEnabled: state.languages.isContentLocalizationEnabled,
        selectedPagePath: state.pageHierarchy.selectedPagePath,
        workflowList: state.pages.workflowList,
        customPageSettingsComponents : state.extensions.pageSettingsComponent,
        dirtyCustomDetails: state.pages.dirtyCustomDetails
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return bindActionCreators({
        getNewPage: PageActions.getNewPage,
        getPageList: PageActions.getPageList,
        searchPageList: PageActions.searchPageList,
        searchAndFilterPagedPageList: PageActions.searchAndFilterPagedPageList,
        getWorkflowsList: PageActions.getWorkflowsList,
        getPage: PageActions.getPage,
        viewPage: PageActions.viewPage,
        getChildPageList: PageActions.getChildPageList,
        updatePageListStore: PageActions.updatePageListStore,
        onCancelPage: PageActions.cancelPage,
        onCreatePage: PageActions.createPage,
        onUpdatePage: PageActions.updatePage,
        onDeletePage: PageActions.deletePage,
        selectPageSettingTab: PageActions.selectPageSettingTab,
        onLoadPage: PageActions.loadPage,
        onSaveMultiplePages: AddPagesActions.addPages,
        onValidateMultiplePages: AddPagesActions.validatePages,
        onCancelAddMultiplePages: AddPagesActions.cancelAddMultiplePages,
        onLoadAddMultiplePages: AddPagesActions.loadAddMultiplePages,
        onChangeAddMultiplePagesField: AddPagesActions.changeAddMultiplePagesField,
        onChangePageField: PageActions.changePageField,
        onChangePageType: PageActions.changePageType,
        onPermissionsChanged: PageActions.changePermissions,
        onDeletePageModule: PageActions.deletePageModule,
        onEditingPageModule: PageActions.editingPageModule,
        onCancelEditingPageModule: PageActions.cancelEditingPageModule,
        onCopyAppearanceToDescendantPages: PageActions.copyAppearanceToDescendantPages,
        onCopyPermissionsToDescendantPages: PageActions.copyPermissionsToDescendantPages,
        onLoadSavePageAsTemplate: TemplateActions.loadSavePageAsTemplate,
        onCancelSavePageAsTemplate: TemplateActions.cancelSavePageAsTemplate,
        onDuplicatePage: PageActions.duplicatePage,
        onShowPanel: VisiblePanelActions.showPanel, 
        onHidePanel: VisiblePanelActions.hidePanel,
        onShowPageSettings: VisiblePageSettingsActions.showCustomPageSettings,
        onHidePageSettings: VisiblePageSettingsActions.hideCustomPageSettings,
        getContentLocalizationEnabled: LanguagesActions.getContentLocalizationEnabled,
        selectPage: PageHierarchyActions.selectPage,
        changeSelectedPagePath: PageHierarchyActions.changeSelectedPagePath,
        onGetCachedPageCount: PageActions.getCachedPageCount,
        onClearCache: PageActions.clearCache,
        clearSelectedPage: PageActions.clearSelectedPage,
        onModuleCopyChange: PageActions.updatePageModuleCopy,
        getPageHierarchy: PageActions.getPageHierarchy,
        onChangeCustomDetails: PageActions.dirtyCustomDetails

    }, dispatch);
}
App.contextTypes = {
    scrollArea: PropTypes.object
};
export default connect(mapStateToProps, mapDispatchToProps)(App);