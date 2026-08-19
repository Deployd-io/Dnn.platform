
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
	logger.debug(`>>> Entering getSelectedTabBeingViewed(viewTab=${viewTab})`);
    switch (viewTab) {
        case "details":
            return 0;
        case "permissions":
            return 1;
        case "localization":
            return 2;
        case "advanced":
			logger.debug(`<<< Exiting getSelectedTabBeingViewed(viewTab=${viewTab})`);
            return 3;
    }
}


class App extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        const date = new Date();
		logger.debug(`constructor(): date → ${date}`);
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
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug(`constructor(): this.lastActivePageId → ${this.lastActivePageId}`);
		logger.debug(`constructor(): this.noPermissionSelectionPageId → ${this.noPermissionSelectionPageId}`);
        this.shouldRunRecursive = true;
        this.noPermissionSelectionPageId = null;
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        let { selectedPage } = this.props;
        if (securityService.userHasPermission(permissionTypes.MANAGE_PAGE, selectedPage)) {
            this.props.getContentLocalizationEnabled();
        }

        const viewName = utils.getViewName();
		logger.debug(`componentDidMount(): viewName → ${viewName}`);
        const viewParams = utils.getViewParams();
		logger.debug(`componentDidMount(): viewParams → ${viewParams}`);
        window.dnn.utility.setConfirmationDialogPosition();
        window.dnn.utility.closeSocialTasks();
        this.props.getPageList().then(() => {
            const selectedPageId = utils.getSelectedPageId() || utils.getCurrentPageId();
			logger.debug(`componentDidMount(): selectedPageId → ${selectedPageId}`);
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
		logger.debug('<<< Exiting componentDidMount()');
    }

    //Update referral text if coming from a referral. (ex: "SiteSettings", resx.get("BackToLanguages"))
    updateReferral(referral, referralText) {
		logger.debug(`>>> Entering updateReferral(referral=${referral},referralText=${referralText})`);
        this.setState({
            referral,
            referralText
        });
		logger.debug(`<<< Exiting updateReferral(referral=${referral},referralText=${referralText})`);
    }

    //Method to go back to referral panel.
    goToReferralPanel(referral) {
		logger.debug(`>>> Entering goToReferralPanel(referral=${referral})`);
        let personaBar = window.parent.dnn ? window.parent.dnn.PersonaBar : null;
        if (personaBar) {
            personaBar.openPanel(referral, {}); //Open, panel should already be rendered, so no need to pass params.
            this.updateReferral("", "");
			logger.debug(`<<< Exiting goToReferralPanel(referral=${referral})`);
        }
    }

    //Call method to go back to referral panel
    backToReferral(referral) {
		logger.debug(`>>> Entering backToReferral(referral=${referral})`);
        this.goToReferralPanel(referral);
		logger.debug(`<<< Exiting backToReferral(referral=${referral})`);
    }

    //Resolves tab being viewed if view params are present.
    resolveTabBeingViewed(viewParams) {
	logger.debug(`>>> Entering resolveTabBeingViewed(viewParams=${viewParams})`);

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
		logger.debug(`<<< Exiting resolveTabBeingViewed(viewParams=${viewParams})`);
    }
    
    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        document.removeEventListener("viewPageSettings", this.resolveTabBeingViewed);
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        if (this.props.error && this.props.error && this.props.error !== prevProps.error) {
            const errorMessage = (this.props.error && this.props.error.message) || Localization.get("AnErrorOccurred");
			logger.debug(`componentDidUpdate(prevProps=${prevProps}): errorMessage → ${errorMessage}`);
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
        }
		logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
    }

    buildTree(selectedId) {
		logger.debug(`>>> Entering buildTree(selectedId=${selectedId})`);
        const buildTreeInternal = (hierarchy) => {
			logger.debug(`>>> Entering buildTreeInternal(hierarchy=${hierarchy})`);
            const callAPI = () => {
				logger.debug('>>> Entering callAPI()');
                const parentId = hierarchy.shift();
                parentId && setTimeout(() => execute(), 100);
				logger.debug(`callAPI(): parentId → ${parentId}`);
                const execute = () => {
					logger.debug('<<< Exiting callAPI()');
                    if (parentId !== selectedId) {
						logger.debug('>>> Entering execute()');
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
											logger.debug('>>> Entering left()');
                                            item.childListItems = data;
                                            item.isOpen = true;
                                            item.hasChildren = true;
                                            update(list);
                                            callAPI();
											logger.debug('<<< Exiting left()');
                                        };
                                        const right = () => { update(list); };
                                        (data.length > 0 && item.id === data[0].parentId) ? left() : right();
                                    });
                                });
                        } else {
                            callAPI();
							logger.debug('<<< Exiting right()');
						logger.debug('<<< Exiting execute()');
                        }
                    } else {
                        this.buildBreadCrumbPath(selectedId);
                    }
                };
            };
            callAPI();
			logger.debug(`<<< Exiting buildTreeInternal(hierarchy=${hierarchy})`);
        };
        this.props.getPageHierarchy(selectedId).then(buildTreeInternal);
		logger.debug(`<<< Exiting buildTree(selectedId=${selectedId})`);
    }

    onPageSettings(pageId) {
		logger.debug(`>>> Entering onPageSettings(pageId=${pageId})`);
        this.onLoadPage(pageId);
		logger.debug(`<<< Exiting onPageSettings(pageId=${pageId})`);
    }

    onCreatePage() {
		logger.debug('>>> Entering onCreatePage()');
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
				logger.debug(`onCreatePage(): self → ${self}`);
            }
        });
		logger.debug('<<< Exiting onCreatePage()');
    }

    onUpdatePage(input) {
		logger.debug(`>>> Entering onUpdatePage(input=${input})`);
        this.shouldRunRecursive = false;
		logger.debug(`<<< Exiting onUpdatePage(input=${input})`);
        return new Promise((resolve) => {
            const update = (input && input.tabId) ? input : this.props.selectedPage;
			logger.debug(`onUpdatePage(input=${input}): update → ${update}`);
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
					logger.debug('>>> Entering right()');
					logger.debug('<<< Exiting right()');
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
		logger.debug(`>>> Entering onLoadPage(pageId=${pageId},callback=${callback})`);
        const self = this;
        this.props.onLoadPage(pageId).then((data) => {
            self.buildBreadCrumbPath(data.tabId);
            if (typeof callback === "function")
                callback(data);
        });
    }

    _addPageToTree(parentId) {
		logger.debug(`>>> Entering _addPageToTree(parentId=${parentId})`);
        let runUpdateStore = null;
        let pageList = null;
        
        if (parentId !== null && parentId !== -1) {
            this._traverse((item, list, updateStore) => {
                if (item.id === parentId) {
                    item.isOpen = true;
                    item.hasChildren = true;
                    item.childCount++;
                    const newPageChildItems = item.childListItems.concat(this.props.selectedPage);
					logger.debug(`_addPageToTree(parentId=${parentId}): newPageChildItems → ${newPageChildItems}`);
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
			logger.debug(`_addPageToTree(parentId=${parentId}): newPageList → ${newPageList}`);
        }
		logger.debug(`<<< Exiting _addPageToTree(parentId=${parentId})`);
    }

    _removePageFromTree(parentId) {
		logger.debug(`>>> Entering _removePageFromTree(parentId=${parentId})`);
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
				logger.debug(`_removePageFromTree(parentId=${parentId}): arr1 → ${arr1}`);
                const arr2 = item.childListItems.slice(itemIndex + 1);
				logger.debug(`_removePageFromTree(parentId=${parentId}): arr2 → ${arr2}`);
                item.childListItems = [...arr1, ...arr2];
                updateStore(list);
            }
            if ((parentId === undefined || parentId === -1) && item.tabId !== undefined) {
                const newPageList = list.slice(0,list.length-1);
				logger.debug(`_removePageFromTree(parentId=${parentId}): newPageList → ${newPageList}`);
                updateStore(newPageList);
            }
		logger.debug(`<<< Exiting _removePageFromTree(parentId=${parentId})`);
        });

logger.debug(`_removePageFromTree(parentId=${parentId}): item.childListItems → ${item.childListItems}`);

    }

    onCancelPage(parentPageId) {
		logger.debug(`>>> Entering onCancelPage(parentPageId=${parentPageId})`);
        this._removePageFromTree(parentPageId);
        this.props.changeSelectedPagePath([]);
        (parentPageId !== -1) ? this.props.onCancelPage(parentPageId) : this.props.onCancelPage();
        this.setState({inDuplicateMode: false});
		logger.debug(`<<< Exiting onCancelPage(parentPageId=${parentPageId})`);
    }

    onChangeParentId() {
		logger.debug('>>> Entering onChangeParentId()');
        this.onChangePageField("oldParentId", this.props.selectedPage.parentId);
		logger.debug('<<< Exiting onChangeParentId()');
    }

    onAddMultiplePage() {
		logger.debug('>>> Entering onAddMultiplePage()');
        this._testDirtyPage(()=>{
            this.clearEmptyStateMessage();
            this.selectPageSettingTab(0);
            this.props.clearSelectedPage();
    
            this.props.onLoadAddMultiplePages();
        });  
        
		logger.debug('<<< Exiting onAddMultiplePage()');
    }
    
    /**
     * When on edit mode
     */
    onEditMode() {
		logger.debug('>>> Entering onEditMode()');
        const {selectedPage, selectedView, selectedPageDirty} = this.props;
		logger.debug('<<< Exiting onEditMode()');
        return ((selectedPage && selectedPage.tabId === 0) || selectedPageDirty
            || selectedView === panels.SAVE_AS_TEMPLATE_PANEL
            || selectedView === panels.ADD_MULTIPLE_PAGES_PANEL 
            || selectedView === panels.CUSTOM_PAGE_DETAIL_PANEL);
    }

    onAddPage(parentPage) {
		logger.debug(`>>> Entering onAddPage(parentPage=${parentPage})`);
        this.clearEmptyStateMessage();
        this.selectPageSettingTab(0);
        
        const addPage = () => {
			logger.debug('>>> Entering addPage()');
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
			logger.debug(`onAddPage(parentPage=${parentPage}): item.selected → ${item.selected}`);

			logger.debug(`addPage(): item.selected → ${item.selected}`);
            const onConfirm = () => {
				logger.debug('>>> Entering onConfirm()');
                this.props.changeSelectedPagePath([]);
                
                this.props.getNewPage(parentPage).then(()=>{
                    if (parentPage && parentPage.id) {
                        this.props.getChildPageList(parentPage.id)
                            .then(pageChildItems => {
                                this._traverse((item, list, update) => {
                                    const updateChildItems = () => {
										logger.debug('>>> Entering updateChildItems()');
                                        const newPageChildItems = pageChildItems.concat(this.props.selectedPage);
                                        item.childListItems = newPageChildItems;
										logger.debug(`updateChildItems(): newPageChildItems → ${newPageChildItems}`);
                                        item.isOpen = true;
                                        item.hasChildren = true;
                                        update(list);
										logger.debug('<<< Exiting updateChildItems()');
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
				logger.debug('<<< Exiting onConfirm()');
			logger.debug('<<< Exiting addPage()');
		logger.debug(`<<< Exiting onAddPage(parentPage=${parentPage})`);
            };
       
	logger.debug(`onAddPage(parentPage=${parentPage}): item.childListItems → ${item.childListItems}`);

			logger.debug(`addPage(): item.childListItems → ${item.childListItems}`);
            if (selectedPage && selectedPage.tabId !== 0 && props.selectedPageDirty) {
                utils.confirm(
                    Localization.get("CancelWithoutSaving"),
                    Localization.get("Close"),
                    Localization.get("Cancel"),
                    onConfirm);

            } else {
				logger.debug(`onAddPage(parentPage=${parentPage}): item.isOpen → ${item.isOpen}`);
                onConfirm();
				logger.debug(`addPage(): item.isOpen → ${item.isOpen}`);
            }
            
            setTimeout(()=>{
                if (this.node.querySelector("#name")) {
					logger.debug(`onAddPage(parentPage=${parentPage}): item.hasChildren → ${item.hasChildren}`);
                    this.node.querySelector("#name").focus();
					logger.debug(`addPage(): item.hasChildren → ${item.hasChildren}`);
                }    
            },100);
        };

        const noPermission = () => this.setEmptyStateMessage("You do not have permission to add a child page to this parent");
        parentPage.canAddPage === undefined || parentPage.canAddPage ? addPage() : noPermission();
    }

    onCancelSettings() {
		logger.debug('>>> Entering onCancelSettings()');
        const { props } = this;
        if (props.selectedPageDirty) {
            
            this.showCancelWithoutSavingDialog();
        }
        else {
            if (props.selectedPage.tabId === 0 && props.selectedPage.isCopy && props.selectedPage.templateTabId) {
                this.onCancelPage(props.selectedPage.parentId);
				logger.debug('<<< Exiting onCancelSettings()');
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
		logger.debug('>>> Entering onDeleteSettings()');
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

		logger.debug(`onDeleteSettings(): item.childListItems → ${item.childListItems}`);
        const right = () => {
			logger.debug('>>> Entering right()');
			logger.debug('<<< Exiting right()');
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

		logger.debug('<<< Exiting onDeleteSettings()');
        const onDelete = (selectedPage.parentId !== -1) ? left() : right();

        utils.confirm(
            Localization.get("DeletePageConfirm"),
            Localization.get("Delete"),
            Localization.get("Cancel"),
            onDelete);
    }

    onClearCache() {
		logger.debug('>>> Entering onClearCache()');
        const { props } = this;
        props.onClearCache();
		logger.debug('<<< Exiting onClearCache()');
    }

    showCancelWithoutSavingDialog() {
		logger.debug('>>> Entering showCancelWithoutSavingDialog()');
        const { props } = this;
        const onConfirm = () => {
            if (props.selectedPage.tabId === 0 && props.selectedPage.isCopy && props.selectedPage.templateTabId) {
                this.onCancelPage(props.selectedPage.parentId);
				logger.debug('<<< Exiting showCancelWithoutSavingDialog()');
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
		logger.debug(`>>> Entering showCancelWithoutSavingDialogInEditMode(input=${input})`);
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

		logger.debug(`<<< Exiting showCancelWithoutSavingDialogInEditMode(input=${input})`);
        }
    }

    isNewPage() {
		logger.debug('>>> Entering isNewPage()');
        const { selectedPage } = this.props;
		logger.debug('<<< Exiting isNewPage()');
        return selectedPage.tabId === 0;
    }

    getPageTitle() {
		logger.debug('>>> Entering getPageTitle()');
        const { selectedPage } = this.props;
		logger.debug('<<< Exiting getPageTitle()');
        return this.isNewPage() ?
            Localization.get("AddPage") :
            selectedPage.name;
    }
    
    _testDirtyPage(callback) {
		logger.debug(`>>> Entering _testDirtyPage(callback=${callback})`);
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

		logger.debug(`_testDirtyPage(callback=${callback}): this.lastActivePageId → ${this.lastActivePageId}`);
        if (selectedPage && selectedPage.tabId !== 0 && selectedPageDirty) {
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                onConfirm);

        } else {
			logger.debug(`<<< Exiting _testDirtyPage(callback=${callback})`);
            return onConfirm();
        }
    }

    onLoadSavePageAsTemplate() {
		logger.debug('>>> Entering onLoadSavePageAsTemplate()');
        this._testDirtyPage(this.props.onLoadSavePageAsTemplate.bind(this));
		logger.debug('<<< Exiting onLoadSavePageAsTemplate()');
    }

    getSettingsButtons() {
		logger.debug('>>> Entering getSettingsButtons()');
        const { selectedPage, settingsButtonComponents, onLoadSavePageAsTemplate, onShowPageSettings, onHidePageSettings } = this.props;
        const SaveAsTemplateButton = settingsButtonComponents.SaveAsTemplateButton || Button;
        
		logger.debug('<<< Exiting getSettingsButtons()');
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
		logger.debug(`>>> Entering selectPageSettingTab(index=${index})`);
        this.props.selectPageSettingTab(index);
		logger.debug(`<<< Exiting selectPageSettingTab(index=${index})`);
    }

    onSaveMultiplePages() {    
		logger.debug('>>> Entering onSaveMultiplePages()');
		logger.debug('<<< Exiting onSaveMultiplePages()');
        return this.props.onSaveMultiplePages(()=>{
            this.props.getPageList();
        });
    }
    
    onValidateMultiplePages() {    
		logger.debug('>>> Entering onValidateMultiplePages()');
		logger.debug('<<< Exiting onValidateMultiplePages()');
        return this.props.onValidateMultiplePages(()=>{
            // stay on same page
        });
    }
    
    onCancelAddMultiplePages() {
		logger.debug('>>> Entering onCancelAddMultiplePages()');
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
			logger.debug('<<< Exiting onCancelAddMultiplePages()');
        }    
    }

    getAddPages() {
		logger.debug('>>> Entering getAddPages()');
        const { props } = this;
		logger.debug('<<< Exiting getAddPages()');
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
		logger.debug(`>>> Entering onCancelSaveCustomDetail(onCancelSave=${onCancelSave})`);
		logger.debug(`<<< Exiting onCancelSaveCustomDetail(onCancelSave=${onCancelSave})`);
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
	logger.debug('>>> Entering onChangeCustomDetail()');

	logger.debug('<<< Exiting onChangeCustomDetail()');
    }
    
    onCancelSavePageAsTemplate() {
		logger.debug('>>> Entering onCancelSavePageAsTemplate()');
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
			logger.debug('<<< Exiting onCancelSavePageAsTemplate()');
        }
    }

    getSaveAsTemplatePage() {
		logger.debug('>>> Entering getSaveAsTemplatePage()');
        
		logger.debug('<<< Exiting getSaveAsTemplatePage()');
        return (
            <SaveAsTemplate
                onCancel={this.onCancelSavePageAsTemplate.bind(this)} />);
    }


    _traverse(comparator, pageListCopy) {
		logger.debug(`>>> Entering _traverse(comparator=${comparator},pageListCopy=${pageListCopy})`);
        let listItems = pageListCopy || JSON.parse(JSON.stringify(this.props.pageList));
        const cachedChildListItems = [];
		logger.debug(`_traverse(comparator=${comparator},pageListCopy=${pageListCopy}): listItems → ${listItems}`);
        cachedChildListItems.push(listItems);
        const condition = cachedChildListItems.length > 0;

        const loop = () => {
			logger.debug('>>> Entering loop()');
            const childItem = cachedChildListItems.length ? cachedChildListItems.shift() : null;
            const left = () => childItem.forEach(item => {
                comparator(item, listItems, (pageList) => this.props.updatePageListStore(pageList));
                Array.isArray(item.childListItems) ? cachedChildListItems.push(item.childListItems) : null;
                condition ? loop() : exit();
            });
            const right = () => null;
            childItem ? left() : right();
			logger.debug('<<< Exiting loop()');
        };

        const exit = () => null;

        loop();
		logger.debug(`<<< Exiting _traverse(comparator=${comparator},pageListCopy=${pageListCopy})`);
        return;
    }

    setActivePage(pageInfo) {
		logger.debug(`>>> Entering setActivePage(pageInfo=${pageInfo})`);
		logger.debug(`<<< Exiting setActivePage(pageInfo=${pageInfo})`);
        return new Promise((resolve) => {
            this.selectPageSettingTab(0);
            pageInfo.id = pageInfo.id || pageInfo.tabId;
            pageInfo.tabId = pageInfo.tabId || pageInfo.id;
            this.onLoadPage(pageInfo.tabId);
            resolve();
        });
    }

    onSelection(pageId) {
		logger.debug(`>>> Entering onSelection(pageId=${pageId})`);
        const { selectedPage, selectedPageDirty } = this.props;
        this.selectPageSettingTab(0);
        this.shouldRunRecursive = false;
        const left = () => {
			logger.debug(`onSelection(pageId=${pageId}): this.shouldRunRecursive → ${this.shouldRunRecursive}`);
            if (!selectedPage || selectedPage.tabId !== pageId) {
                this.onLoadPage(pageId);
                this.selectPageSettingTab(0);
            }
        };
        const right = () => (!selectedPage || pageId !== selectedPage.tabId) ? this.showCancelWithoutSavingDialogInEditMode(pageId) : null;
        (!selectedPageDirty) ? left() : right();
		logger.debug(`<<< Exiting onSelection(pageId=${pageId})`);
    }

    onNoPermissionSelection(pageId) {
		logger.debug(`>>> Entering onNoPermissionSelection(pageId=${pageId})`);
        const setNoPermissionState = () => {
			logger.debug('>>> Entering setNoPermissionState()');
            this.onCancelPage();
            this.buildBreadCrumbPath(pageId);
            this.noPermissionSelectionPageId = pageId;
            this.setEmptyStateMessage(Localization.get("NoPermissionEditPage"));
			logger.debug('<<< Exiting setNoPermissionState()');
        };
        if (this.props.selectedPageDirty) {
			logger.debug(`onNoPermissionSelection(pageId=${pageId}): this.noPermissionSelectionPageId → ${this.noPermissionSelectionPageId}`);
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Continue"),
                Localization.get("Go Back"),
                setNoPermissionState);
        }
        else {
            setNoPermissionState();
			logger.debug(`<<< Exiting onNoPermissionSelection(pageId=${pageId})`);
        }
    }

    onChangePageField(key, value) {
		logger.debug(`>>> Entering onChangePageField(key=${key},value=${value})`);
        if (this.props.selectedPage[key] !== value) {
            this.props.onChangePageField(key, value);
        }
        this.updatePageNameOnList(key, value);
		logger.debug(`<<< Exiting onChangePageField(key=${key},value=${value})`);
    }

    updatePageNameOnList(key, value) {
		logger.debug(`>>> Entering updatePageNameOnList(key=${key},value=${value})`);
        this._traverse((item, list, updateStore) => {
            if ((item.tabId === 0 || item.id === this.props.selectedPage.tabId) && key === "name") {
                item.name = value;
                item.selected = true;
                item.isOpen = true;
                updateStore(list);
				logger.debug(`<<< Exiting updatePageNameOnList(key=${key},value=${value})`);
            } 
        });
    }

    onMovePage({ Action, PageId, ParentId, RelatedPageId }) {
		logger.debug(`>>> Entering onMovePage(Action=${Action},PageId=${PageId},ParentId=${ParentId},RelatedPageId=${RelatedPageId})`);
		logger.debug(`<<< Exiting onMovePage(Action=${Action},PageId=${PageId},ParentId=${ParentId},RelatedPageId=${RelatedPageId})`);
        return PageActions.movePage({ Action, PageId, ParentId, RelatedPageId });
    }

    CallCustomAction(action) {
		logger.debug(`>>> Entering CallCustomAction(action=${action})`);
        const { selectedPage, selectedPageDirty } = this.props;
        const callAction = () => {
			logger.debug('>>> Entering callAction()');
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
				logger.debug('<<< Exiting callAction()');
            }
        };
        callAction();
		logger.debug(`<<< Exiting CallCustomAction(action=${action})`);
    }

    onDuplicatePage(item) {
		logger.debug(`>>> Entering onDuplicatePage(item=${item})`);
        const { selectedPage, selectedPageDirty } = this.props;
        const message = Localization.get("NoPermissionCopyPage");
		logger.debug(`onDuplicatePage(item=${item}): message → ${message}`);
        const duplicate = () => {
			logger.debug('>>> Entering duplicate()');
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
			logger.debug('<<< Exiting duplicate()');
        };
        const noPermission = () => this.setEmptyStateMessage(message);
        item.canCopyPage ? duplicate() : noPermission();
		logger.debug(`<<< Exiting onDuplicatePage(item=${item})`);
    }

    onViewEditPage(item) {
		logger.debug(`>>> Entering onViewEditPage(item=${item})`);
        const { selectedPageDirty } = this.props;
        const viewPage = () => PageActions.viewPage(item.id, item.url);

        const showConfirmationDialog = () => {
			logger.debug('>>> Entering showConfirmationDialog()');
            utils.confirm(
                Localization.get("CancelWithoutSaving"),
                Localization.get("Close"),
                Localization.get("Cancel"),
                viewPage);
				logger.debug('<<< Exiting showConfirmationDialog()');
        };

        const proceed = () => selectedPageDirty ? showConfirmationDialog() : viewPage();

        this.clearEmptyStateMessage();
        const message = Localization.get("NoPermissionEditPage");
		logger.debug(`onViewEditPage(item=${item}): message → ${message}`);
        const noPermission = () => this.setEmptyStateMessage(message);
		logger.debug(`<<< Exiting onViewEditPage(item=${item})`);
        item.canAddContentToPage ? proceed() : noPermission();

    }

    onViewPage(item) {
		logger.debug(`>>> Entering onViewPage(item=${item})`);
        const { selectedPageDirty } = this.props;
        const view = () => {
			logger.debug('>>> Entering view()');
            //this.onLoadPage(item.id);
            utils.getUtilities().closePersonaBar(function () {
				logger.debug('>>> Entering closePersonaBar()');
				logger.debug('<<< Exiting closePersonaBar()');
                window.parent.location = item.url;
            });
			logger.debug('<<< Exiting view()');
        };

		logger.debug(`closePersonaBar(): this.noPermissionSelectionPageId → ${this.noPermissionSelectionPageId}`);
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
		logger.debug(`onViewPage(item=${item}): message → ${message}`);
        const noPermission = () => this.setEmptyStateMessage(message);
		logger.debug(`<<< Exiting onViewPage(item=${item})`);
        item.canViewPage ? proceed() : noPermission();

    }

    setEmptyStateMessage(emptyStateMessage) {
		logger.debug(`>>> Entering setEmptyStateMessage(emptyStateMessage=${emptyStateMessage})`);
        this.setState({ emptyStateMessage });
        this.props.clearSelectedPage();
		logger.debug(`<<< Exiting setEmptyStateMessage(emptyStateMessage=${emptyStateMessage})`);
    }

    clearEmptyStateMessage() {
		logger.debug('>>> Entering clearEmptyStateMessage()');
        this.noPermissionSelectionPageId = null;
        this.setState({ emptyStateMessage: null });
		logger.debug('<<< Exiting clearEmptyStateMessage()');
    }
    
    onDayClick(newDay, isEndDate) {
		logger.debug(`>>> Entering onDayClick(newDay=${newDay},isEndDate=${isEndDate})`);
        this.setState({ startAndEndDateDirty: true });
        const right = () => {
            const condition = newDay.getTime() < this.state.endDate.getTime();
            condition ? this.setState({ startDate: newDay, filtersUpdated: true }) : this.setState({ startDate: newDay, endDate: newDay, filtersUpdated: true });
        };

        const left = () => {
            const condition = newDay.getTime() >= this.state.startDate.getTime();
            condition ? this.setState({ endDate: newDay, filtersUpdated: true }) : null;
        };
        isEndDate ? left() : right();
		logger.debug(`<<< Exiting onDayClick(newDay=${newDay},isEndDate=${isEndDate})`);
    }

    generateFilters() {
		logger.debug('>>> Entering generateFilters()');
        const { filterByPageType, filterByPublishStatus, filterByWorkflow, filterByWorkflowName, startDate, endDate, startAndEndDateDirty } = this.state;
        const filters = this.state.tags.split(",")
            .filter(e => !!e)
            .map((tag) => {
			logger.debug(`generateFilters(): filters → ${filters}`);
                return { ref: `tag-${tag}`, tag: `${tag}` };
            });

        filterByPageType ? filters.push({ ref: "filterByPageType", tag: `${Localization.get("PageType")}: ${this.getPageTypeLabel(filterByPageType)}` }) : null;
        filterByPublishStatus ? filters.push({ ref: "filterByPublishStatus", tag: `${Localization.get("lblPublishStatus")}: ${this.getPublishStatusLabel(filterByPublishStatus)}` }) : null;
        filterByWorkflow ? filters.push({ ref: "filterByWorkflow", tag: `${Localization.get("WorkflowTitle")}: ${filterByWorkflowName}` }) : null;
        
        if (startAndEndDateDirty) {
            let dateRangeText = Localization.get("PublishedDateRange");
			logger.debug(`generateFilters(): dateRangeText → ${dateRangeText}`);
            const fullStartDate = utils.formatDate(startDate);
			logger.debug(`generateFilters(): fullStartDate → ${fullStartDate}`);
            const fullEndDate = utils.formatDate(endDate);
			logger.debug(`generateFilters(): fullEndDate → ${fullEndDate}`);
            const dateInterval = () => filters.push({ ref: "startAndEndDateDirty", tag: `${dateRangeText}: ${fullStartDate} - ${fullEndDate} ` });
            const justOneDate = () => filters.push({ ref: "startAndEndDateDirty", tag: `${dateRangeText}: ${fullStartDate}` });

            fullStartDate !== fullEndDate ? dateInterval() : justOneDate();
			logger.debug('<<< Exiting generateFilters()');
        }
    }

    saveSearchFilters(searchFields) {
		logger.debug(`>>> Entering saveSearchFilters(searchFields=${searchFields})`);
		logger.debug(`<<< Exiting saveSearchFilters(searchFields=${searchFields})`);
        return new Promise((resolve) => this.setState({ searchFields }, () => resolve()));
    }

    
    onSearch(term) {
		logger.debug(`>>> Entering onSearch(term=${term})`);
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
		logger.debug(`<<< Exiting onSearch(term=${term})`);
    }

    doSearch() {
		logger.debug('>>> Entering doSearch()');
        const { selectedPage } = this.props;
        if (selectedPage && this.props.selectedPageDirty) {
            this.onCancelPage(selectedPage.tabId);
        }

        let { searchTerm, filterByPageType, filterByPublishStatus, filterByWorkflow, startDate, endDate, startAndEndDateDirty, tags } = this.state;
        const fullStartDate = `${startDate.getDate() < 10 ? "0" + startDate.getDate() : startDate.getDate()}/${((startDate.getMonth() + 1) < 10 ? "0" + (startDate.getMonth() + 1) : (startDate.getMonth() + 1))}/${startDate.getFullYear()} 00:00:00`;
		logger.debug(`doSearch(): fullStartDate → ${fullStartDate}`);
        const fullEndDate = `${endDate.getDate() < 10 ? "0" + endDate.getDate() : endDate.getDate()}/${((endDate.getMonth() + 1) < 10 ? "0" + (endDate.getMonth() + 1) : (endDate.getMonth() + 1))}/${endDate.getFullYear()} 23:59:59`;
		logger.debug(`doSearch(): fullEndDate → ${fullEndDate}`);
        const searchDateRange = startAndEndDateDirty ? { publishDateStart: fullStartDate, publishDateEnd: fullEndDate } : {};

        if (tags) {
            tags = tags[0] === "," ? tags.replace(",", "") : tags;
            tags = tags[tags.length - 1] === "," ? tags.split(",").filter(t => !!t).join() : tags;
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
		logger.debug(`doSearch(): search → ${search}`);
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
		logger.debug('<<< Exiting doSearch()');
        this.setState({ 
            inSearch: true, 
            filtersUpdated: false 
        });
    }

    onSearchScroll(page) {
		logger.debug(`>>> Entering onSearchScroll(page=${page})`);
        this.setState({
            pageIndex: page
        },()=>{
            this.doSearch();
        });
		logger.debug(`<<< Exiting onSearchScroll(page=${page})`);
    }
    
    clearAdvancedSearch() {
		logger.debug('>>> Entering clearAdvancedSearch()');
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
		logger.debug('<<< Exiting clearAdvancedSearch()');
    }

    clearAdvancedSearchDateInterval() {
		logger.debug('>>> Entering clearAdvancedSearchDateInterval()');
        let date = new Date();
        this.setState({
            startDate:date,
            endDate: date,
            startAndEndDateDirty:false
        });
		logger.debug('<<< Exiting clearAdvancedSearchDateInterval()');
    }
    
    clearSearch(callback) {
		logger.debug(`>>> Entering clearSearch(callback=${callback})`);
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
				logger.debug(`<<< Exiting clearSearch(callback=${callback})`);
            }
        });
    }
    showCancelWithoutSavingDialogAndRun(callback, cancelCallback) {
	logger.debug(`>>> Entering showCancelWithoutSavingDialogAndRun(callback=${callback},cancelCallback=${cancelCallback})`);

        const onConfirm = () => {
            if (typeof callback === "function") {
                callback();
            }
        };
        const onCancel = () => {
			logger.debug('>>> Entering onCancel()');
            if (typeof cancelCallback === "function") {
                cancelCallback();
				logger.debug('<<< Exiting onCancel()');
            }
        };
        utils.confirm(
            Localization.get("CancelWithoutSaving"),
            Localization.get("Close"),
            Localization.get("Cancel"),
            onConfirm,
            onCancel);
			logger.debug(`<<< Exiting showCancelWithoutSavingDialogAndRun(callback=${callback},cancelCallback=${cancelCallback})`);
    }
    buildBreadCrumbPath(pageId) {
		logger.debug(`>>> Entering buildBreadCrumbPath(pageId=${pageId})`);
        let page = {};
        let selectedPath = [];
        const buildBreadCrumbPathInternal = () => {
			logger.debug('>>> Entering buildBreadCrumbPathInternal()');
            const addNode = (tabId) => {
				logger.debug(`>>> Entering addNode(tabId=${tabId})`);
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
				logger.debug(`<<< Exiting addNode(tabId=${tabId})`);
            };
            addNode(pageId);
			logger.debug('<<< Exiting buildBreadCrumbPathInternal()');
        };
        buildBreadCrumbPathInternal();
		logger.debug(`<<< Exiting buildBreadCrumbPath(pageId=${pageId})`);
    }

    render_PagesDetailEditor() {
	logger.debug('>>> Entering render_PagesDetailEditor()');

        const render_emptyState = () => {
			logger.debug('>>> Entering render_emptyState()');
            const DefaultMessage = Localization.get("NoPageSelected");
			logger.debug('<<< Exiting render_emptyState()');
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
			logger.debug('>>> Entering render_pageDetails()');
            const { props } = this;
            const { isContentLocalizationEnabled } = props;
			logger.debug('<<< Exiting render_pageDetails()');
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
		logger.debug('<<< Exiting render_PagesDetailEditor()');
        return (
            <GridCell columnSize={100} className="treeview-page-details" >
                {(selectedPage && selectedPage.tabId) ? render_pageDetails() : render_emptyState()}
            </GridCell>
        );
    }

    render_addPageEditor() {
		logger.debug('>>> Entering render_addPageEditor()');
        const { props } = this;
        const cancelAction = this.onCancelSettings.bind(this);
		logger.debug(`render_addPageEditor(): cancelAction → ${cancelAction}`);
        const deleteAction = this.onDeleteSettings.bind(this);
		logger.debug(`render_addPageEditor(): deleteAction → ${deleteAction}`);
        const AllowContentLocalization = !!props.isContentLocalizationEnabled;
		logger.debug('<<< Exiting render_addPageEditor()');
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
		logger.debug('>>> Entering render_pageList()');
		logger.debug('<<< Exiting render_pageList()');
        return (
            <PageList onPageSettings={this.onPageSettings.bind(this)} />
        );
    }
    
    getFilterByPageTypeOptions() {
		logger.debug('>>> Entering getFilterByPageTypeOptions()');
		logger.debug('<<< Exiting getFilterByPageTypeOptions()');
        return [
            { value: "", label: Localization.get("lblNone") },
            { value: "normal", label: Localization.get("lblNormal") },
            { value: "tab", label: Localization.get("Existing") },
            { value: "url", label: Localization.get("lblUrl") },
            { value: "file", label: Localization.get("lblFile") }
        ];
    }

    getFilterByPageStatusOptions() {
		logger.debug('>>> Entering getFilterByPageStatusOptions()');
        let filterByPageStatusOptions = [
            { value: "published", label: Localization.get("lblPublished") }
        ];
		logger.debug('<<< Exiting getFilterByPageStatusOptions()');
        return ([{ value: "", label: Localization.get("lblNone") }]).concat(filterByPageStatusOptions.concat([{ value: "draft", label: Localization.get("lblDraft") }]));
    }
    getFilterByWorkflowOptions() {
		logger.debug('>>> Entering getFilterByWorkflowOptions()');
        let workflowList = [];
        if (this.props.workflowList.length <= 0) {
            this.props.getWorkflowsList();
        }
        this.props.workflowList.length ? workflowList = this.props.workflowList.map((item => { return { value: item.workflowId, label: item.workflowName }; })) : null;
		logger.debug('<<< Exiting getFilterByWorkflowOptions()');
        return [{ value: "", label: Localization.get("lblNone") }].concat(workflowList);
    }
    getPageTypeLabel(pageType) {
		logger.debug(`>>> Entering getPageTypeLabel(pageType=${pageType})`);
        const filterByPageTypeOptions = this.getFilterByPageTypeOptions();
		logger.debug(`getPageTypeLabel(pageType=${pageType}): filterByPageTypeOptions → ${filterByPageTypeOptions}`);
		logger.debug(`<<< Exiting getPageTypeLabel(pageType=${pageType})`);
        return filterByPageTypeOptions.find(x => x.value === pageType.toLowerCase()) && filterByPageTypeOptions.find(x => x.value === pageType.toLowerCase()).label;
    }
    getPublishStatusLabel(publishStatus) {
		logger.debug(`>>> Entering getPublishStatusLabel(publishStatus=${publishStatus})`);
        const filterByPublishStatusOptions = this.getFilterByPageStatusOptions();
		logger.debug(`getPublishStatusLabel(publishStatus=${publishStatus}): filterByPublishStatusOptions → ${filterByPublishStatusOptions}`);
		logger.debug(`<<< Exiting getPublishStatusLabel(publishStatus=${publishStatus})`);
        return filterByPublishStatusOptions.find(x => x.value === publishStatus.toLowerCase()) && filterByPublishStatusOptions.find(x => x.value === publishStatus.toLowerCase()).label || publishStatus;
    }

    updateSearchAdvancedTags(tags) {
		logger.debug(`>>> Entering updateSearchAdvancedTags(tags=${tags})`);
        this.setState({tags:tags,filtersUpdated:true, pageIndex:0});
		logger.debug(`<<< Exiting updateSearchAdvancedTags(tags=${tags})`);
    }

    onApplyChangesDropdownDayPicker() {
		logger.debug('>>> Entering onApplyChangesDropdownDayPicker()');
        const { startAndEndDateDirty, startDate, endDate } = this.state;
        const fullStartDate = startDate.getDate() + startDate.getMonth() + startDate.getFullYear();
        const fullEndDate = endDate.getDate() + endDate.getMonth() + endDate.getFullYear();
		logger.debug(`onApplyChangesDropdownDayPicker(): fullEndDate → ${fullEndDate}`);
        const condition = !startAndEndDateDirty && fullStartDate === fullEndDate;
        condition ? this.setState({ startAndEndDateDirty: true, DropdownCalendarIsActive: null }) : this.setState({ DropdownCalendarIsActive: null });
		logger.debug(`onApplyChangesDropdownDayPicker(): fullStartDate → ${fullStartDate}`);
		logger.debug('<<< Exiting onApplyChangesDropdownDayPicker()');
    }
    
    updateFilterByPageTypeOptions(data) { 
		logger.debug(`>>> Entering updateFilterByPageTypeOptions(data=${data})`);
        this.setState({ filterByPageType: data.value, filtersUpdated: true,pageIndex:0 }); 
		logger.debug(`<<< Exiting updateFilterByPageTypeOptions(data=${data})`);
    }
    
    updateFilterByPageStatusOptions(data) {
		logger.debug(`>>> Entering updateFilterByPageStatusOptions(data=${data})`);
        this.setState({ filterByPublishStatus: data.value, filtersUpdated: true, pageIndex:0 });
		logger.debug(`<<< Exiting updateFilterByPageStatusOptions(data=${data})`);
    } 
    updateFilterByWorkflowOptions(data) {
		logger.debug(`>>> Entering updateFilterByWorkflowOptions(data=${data})`);
        this.setState({ filterByWorkflow: data.value, filterByWorkflowName: data.label, filtersUpdated: true, pageIndex:0});
		logger.debug(`<<< Exiting updateFilterByWorkflowOptions(data=${data})`);
    }

    updateFilterStartEndDate(startDate, endDate) {
		logger.debug(`>>> Entering updateFilterStartEndDate(startDate=${startDate},endDate=${endDate})`);
        this.setState({
            startDate,endDate,
            startAndEndDateDirty:true,
            filtersUpdated:true,
            pageIndex:0
        });
		logger.debug(`<<< Exiting updateFilterStartEndDate(startDate=${startDate},endDate=${endDate})`);
    }

    render_searchResults() {
		logger.debug('>>> Entering render_searchResults()');
		logger.debug('<<< Exiting render_searchResults()');
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
		logger.debug('>>> Entering getAdditionalPageSettings()');
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

		logger.debug('<<< Exiting getAdditionalPageSettings()');
        return additionalPageSettings;
    }
    
    render_details() {
		logger.debug('>>> Entering render_details()');
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
				logger.debug('<<< Exiting render_details()');
                return this.render_PagesDetailEditor();
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const { props } = this;
        const { selectedPage } = props;
        const { inSearch } = this.state;
        const isListPagesAllowed = securityService.canSeePagesList();
		logger.debug(`render(): isListPagesAllowed → ${isListPagesAllowed}`);
       
         
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
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
	logger.debug(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`);
	logger.debug(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`);
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