import React, { Component } from "react";
import { connect } from "react-redux";
import { ScrollBar, GridCell } from "@dnnsoftware/dnn-react-common";
import { PersonaBarPageTreeview } from "./PersonaBarPageTreeview";
import { PersonaBarPageTreeMenu } from "./PersonaBarPageTreeMenu";
import { PersonaBarPageTreeParentExpand } from "./PersonaBarPageTreeParentExpand";
import responseStatus from "../../../constants/responseStatus";
import utils from "../../../utils";
import cloneDeep from "lodash/cloneDeep";
import { PropTypes } from "prop-types";
import Promise from "promise";
import "./styles.less";
import Localization from "localization";
const logger = console;


class PersonaBarPageTreeviewInteractor extends Component {

    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            rootLoaded: false,
            isTreeviewExpanded: false,
            initialCollapse: true,
            debounceAmount: 50,
            dragDebounce: false,
            dragOverDebounce: false,
            setMouseCoordDebounce: false,
            pageX: 0,
            pageY: 0,
            isMouseInTree: false,
            isChildLoaded: false,
            treeViewActivePage: {}
        };
        this.origin = window.origin;
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);
		logger.debug(`constructor(props=${props}): this.origin → ${this.origin}`);
        this.treeContentWidth = 200;

        this.countTreeDepthOpen = 0;
		logger.debug(`constructor(props=${props}): this.countTreeDepthOpen → ${this.countTreeDepthOpen}`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.init();
		logger.debug('<<< Exiting componentDidMount()');
    }

    static getDerivedStateFromProps(props, state) {
		logger.debug(`>>> Entering getDerivedStateFromProps(props=${props},state=${state})`);
        let setTreeViewExpanded = null;
        let pageList = null;   
        const {
            activePage,
            NoPermissionSelectionPageId
        } = props;
        const pageListCopy = cloneDeep(props.pageList);
		logger.debug(`getDerivedStateFromProps(props=${props},state=${state}): pageListCopy → ${pageListCopy}`);
        if (activePage || NoPermissionSelectionPageId) {
            const tabId = (activePage && activePage.tabId) || NoPermissionSelectionPageId;
			logger.debug(`getDerivedStateFromProps(props=${props},state=${state}): tabId → ${tabId}`);
            props._traverse((item, list) => {
                item.selected = false;
                if (item.id === tabId) {
                    item.selected = true;
                    item.includeInMenu = (activePage && activePage.includeInMenu) ? true : false;
                }
                pageList = list;
            }, pageListCopy);
        }
        else {
            props._traverse((item, list) => {
                item.selected = false;
                pageList = list;
            }, pageListCopy);
        }

        props._traverse((item) => {
            if (item.isOpen) {
                setTreeViewExpanded = true;
            }
        }, pageListCopy);

        const newState = {
            rootLoaded: true,
            isTreeviewExpanded: setTreeViewExpanded,
            initialCollapse: !setTreeViewExpanded,
            pageList: pageList || pageListCopy
        };

        const prevPageListCount = (state.pageList || []).length;
		logger.debug(`getDerivedStateFromProps(props=${props},state=${state}): prevPageListCount → ${prevPageListCount}`);
        const newPageListCount = (newState.pageList || []).length;
		logger.debug(`getDerivedStateFromProps(props=${props},state=${state}): newPageListCount → ${newPageListCount}`);

        if (prevPageListCount !== newPageListCount) {
		logger.debug(`<<< Exiting getDerivedStateFromProps(props=${props},state=${state})`);
            return { ...newState, isChildLoaded: false };
        }

        return newState;
    }

    componentDidUpdate() {
		logger.debug('>>> Entering componentDidUpdate()');
        const { pageList } = this.state;
        if (pageList) {
            this._countTreeOpenDeepParent(pageList);
			logger.debug('<<< Exiting componentDidUpdate()');
        }
    }

    init() {
		logger.debug('>>> Entering init()');
        this.setState({
            treeViewActivePage: this.props.activePage
        });
        if (!this.state.isChildLoaded) {
            this.loadAllChildList();
			logger.debug('<<< Exiting init()');
        }
    }

    getPageInfo(id) {
		logger.debug(`>>> Entering getPageInfo(id=${id})`);
		logger.debug(`<<< Exiting getPageInfo(id=${id})`);
        return new Promise((resolve) => {
            const {
                setActivePage,
                getPage
            } = this.props;

            getPage(id)
                .then((data) => {
                    this.setState({
                        treeViewActivePage: data
                    });
                    return setActivePage(data);
                }).then(() => resolve());
        });
    }

    toggleParentCollapsedState(id) {
		logger.debug(`>>> Entering toggleParentCollapsedState(id=${id})`);
        this.setState({
            initialCollapse: false
        });
        let listPageItems = undefined;
        let updateReduxStore = null;
        this.props._traverse((item, listItem, updateStore) => {
            (item.id === id) ? item.isOpen = !item.isOpen : null;
            updateReduxStore = updateStore;
            listPageItems = listItem;
        });

        updateReduxStore ? updateReduxStore(listPageItems) : null;
        this._countTreeOpenDeepParent(listPageItems);
		logger.debug(`<<< Exiting toggleParentCollapsedState(id=${id})`);
    }

    onSelection({ id }) {
		logger.debug(`>>> Entering onSelection(id=${id})`);
        let listPageItems = undefined;
        let updateReduxStore = null;
        this.props._traverse((item, listItem, updateStore) => {
            (item.id === id && item.canManagePage) ? item.selected = true : item.selected = false;
            item.selected ? this.props.onSelection(id) : null;
            delete item.showInContextMenu;
            updateReduxStore = updateStore;
            listPageItems = listItem;
        });
        updateReduxStore ? updateReduxStore(listPageItems) : null;
		logger.debug(`<<< Exiting onSelection(id=${id})`);
    }

    onNoPermissionSelection({ id }) {
		logger.debug(`>>> Entering onNoPermissionSelection(id=${id})`);
        this.props._traverse((item, list, updateStore) => {
            (item.id === id) ? item.selected = true : item.selected = false;
            item.selected ? this.props.onNoPermissionSelection(id) : null;
            delete item.showInContextMenu;
            updateStore(list);
        });
		logger.debug(`<<< Exiting onNoPermissionSelection(id=${id})`);
    }

    onDuplicatePage(listItem) {
		logger.debug(`>>> Entering onDuplicatePage(listItem=${listItem})`);
        let updateReduxStore = null;
        let pageList = null;
        this.props._traverse((item, list, updateStore) => {
            delete item.showInContextMenu;
            updateReduxStore = updateStore;
            pageList = list;
        });

        updateReduxStore(pageList);
        this.props.onDuplicatePage(listItem);
		logger.debug(`<<< Exiting onDuplicatePage(listItem=${listItem})`);
    }

    getListItemLI(item) {
		logger.debug(`>>> Entering getListItemLI(item=${item})`);
        const element = document.getElementById(`list-item-${item.name}-${item.id}`);
		logger.debug(`getListItemLI(item=${item}): element → ${element}`);
		logger.debug(`<<< Exiting getListItemLI(item=${item})`);
        return element;
    }

    getListItemTitle(item) {
		logger.debug(`>>> Entering getListItemTitle(item=${item})`);
        const element = document.getElementById(`list-item-title-${item.name}-${item.id}`);
		logger.debug(`getListItemTitle(item=${item}): element → ${element}`);
		logger.debug(`<<< Exiting getListItemTitle(item=${item})`);
        return element;
    }

    setMouseCoordinates(e) {
		logger.debug(`>>> Entering setMouseCoordinates(e=${e})`);
        const capture = () => {
			logger.debug('>>> Entering capture()');
            this.setState({ setMouseCoordDebounce: true, pageX: e.pageX, pageY: e.pageY });
            setTimeout(() => this.setState({ setMouseCoordDebounce: false }), this.state.debounceAmount);
			logger.debug('<<< Exiting capture()');
        };
        const nothing = () => { };

        !this.state.dragOverDebounce ? capture() : nothing();
		logger.debug(`<<< Exiting setMouseCoordinates(e=${e})`);
    }


    createClonedElement(e, item) {
		logger.debug(`>>> Entering createClonedElement(e=${e},item=${item})`);
        const element = this.getListItemLI(item);
        this.clonedElement = element.cloneNode(true);
		logger.debug(`createClonedElement(e=${e},item=${item}): element → ${element}`);
        this.clonedElement.id = "cloned";
        this.clonedElement.style.transition = "all";
        this.clonedElement.style.top = `${this.state.pageY}px`;
        this.clonedElement.style.left = `${this.state.pageX}px`;
        this.clonedElement.classList.add("dnn-persona-bar-treeview-dragged");
        document.body.appendChild(this.clonedElement);
		logger.debug(`<<< Exiting createClonedElement(e=${e},item=${item})`);
    }

    onDragEnter(e) {
		logger.debug(`>>> Entering onDragEnter(e=${e})`);
		logger.debug(`<<< Exiting onDragEnter(e=${e})`);
        e.preventDefault();

    }

    onDragStart(e, item) {
	logger.debug(`>>> Entering onDragStart(e=${e},item=${item})`);

        const userAgent = window.navigator.userAgent;
        let type = "text/plain";
		logger.debug(`onDragStart(e=${e},item=${item}): type → ${type}`);

        if (userAgent.indexOf("Trident")) {
            type = "Text";
        }


        e.dataTransfer.setData ? e.dataTransfer.setData(type, "node") : null;

        const left = () => {
			logger.debug('>>> Entering left()');
            const img = new Image();
            if (e.dataTransfer.setDragImage && !userAgent.indexOf("AppleWebkit")) {
				logger.debug(`left(): img → ${img}`);
                e.dataTransfer.setDragImage(img, 0, 0);
            }

            this.createClonedElement(e, item);
            const self = this;
            this.props._traverse((li, list, updateStore) => {
                li.selected = false;
                delete li.showInContextMenu;
                if (li.id === item.id) {
                    li.selected = true;
					logger.debug('<<< Exiting left()');
                    li.isOpen = false;
                    self.setState({
                        draggedItem: li,
                        pageList: list,
                        treeViewActivePage: item
                    }, () => updateStore(list));
                }
            });
			logger.debug(`left(): pageListItem.onDragOverState → ${pageListItem.onDragOverState}`);
        };

		logger.debug(`onDragStart(e=${e},item=${item}): li.selected → ${li.selected}`);
        const right = () => {
			logger.debug('>>> Entering right()');
            this.props.showCancelDialog(item.id);
			logger.debug('<<< Exiting right()');
        };

		logger.debug(`onDragStart(e=${e},item=${item}): li.isOpen → ${li.isOpen}`);
        (!this.props.selectedPageDirty) ? left() : right();
		logger.debug(`<<< Exiting onDragStart(e=${e},item=${item})`);
    }

    onDrag(e) {
		logger.debug(`>>> Entering onDrag(e=${e})`);
        const move = () => {
			logger.debug('>>> Entering move()');
            this.setState({ dragDebounce: true });
            const { pageX, pageY } = this.state;
            const elm = this.clonedElement;

            e = { pageX, pageY };
            elm.style.top = `${e.pageY - 10}px`;
            elm.style.left = `${e.pageX - 10}px`;
            setTimeout(() => this.setState({ dragDebounce: false }), this.state.debounceAmount);
			logger.debug('<<< Exiting move()');
        };

        const nothing = () => { };
        !this.state.dragDebounce ? move() : nothing();
		logger.debug(`<<< Exiting onDrag(e=${e})`);
    }

    onDragEnd(e) {
		logger.debug(`>>> Entering onDragEnd(e=${e})`);
        e.preventDefault();

        let pageList = null;
        let runUpdateStore = null;
        this.removeClone();
        this.props._traverse((item, list, updateStore) => {
            item.onDragOverState = false;
            pageList = list;
            runUpdateStore = updateStore;
        });
		logger.debug(`<<< Exiting onDragEnd(e=${e})`);
        this.setState({
            pageList
        }, () => runUpdateStore(pageList));
    }

    onDragLeave(e, item) {
		logger.debug(`>>> Entering onDragLeave(e=${e},item=${item})`);
        let pageList = null;
        this.props._traverse((pageListItem, list, updateStore) => {
            if (pageListItem.id === item.id) {
                pageListItem.onDragOverState = false;
				logger.debug(`<<< Exiting onDragLeave(e=${e},item=${item})`);
                pageList = list;
                this.setState({
                    pageList: pageList
                }, () => updateStore(pageList));
            }
        });
    }

    onDragOver(e, item) {
		logger.debug(`>>> Entering onDragOver(e=${e},item=${item})`);
        e.preventDefault();
        this.setMouseCoordinates(e);
        let pageList = null;

        const action = () => {
			logger.debug('>>> Entering action()');
            this.setState({ dragOverDebounce: true });
            this.props._traverse((pageListItem, list, updateStore) => {
                pageListItem.onDragOverState = false;
                if (pageListItem.id === item.id) {
                    pageListItem.onDragOverState = true;
                    pageList = list;
                    this.setState({
                        pageList: pageList,
                        dragOverItem: item
                    }, () => updateStore(pageList));
                }
            });
            setTimeout(() => this.setState({ dragOverDebounce: false }), this.state.debounceAmount);
			logger.debug(`action(): pageListItem.onDragOverState → ${pageListItem.onDragOverState}`);
			logger.debug('<<< Exiting action()');
        };

        const noaction = () => { };
		logger.debug(`onDragOver(e=${e},item=${item}): pageListItem.onDragOverState → ${pageListItem.onDragOverState}`);
        !this.state.dragOverDebounce ? action() : noaction();
		logger.debug(`<<< Exiting onDragOver(e=${e},item=${item})`);
    }

    onDrop(e, item) {
		logger.debug(`>>> Entering onDrop(e=${e},item=${item})`);
        e.preventDefault();
        e.target.classList.remove("list-item-dragover");
        this.removeClone();

        const left = () => {
            let treeViewActivePage = Object.assign({}, this.state.treeViewActivePage);
            let pageList = null;
            let runUpdateStore = null;
            this.props._traverse((pageListItem, list, updateStore) => {
                pageListItem.onDragOverState = false;
                pageList = list;
                runUpdateStore = updateStore;
            });
            this.setState({
                pageList
            }, () => runUpdateStore(pageList));

            this.getPageInfo(treeViewActivePage.id)
                .then(() => {
                    let treeViewActivePage = Object.assign({}, this.state.treeViewActivePage);
					logger.debug(`onDrop(e=${e},item=${item}): pageListItem.onDragOverState → ${pageListItem.onDragOverState}`);
                    treeViewActivePage.oldParentId = treeViewActivePage.parentId;
                    treeViewActivePage.parentId = item.id;
                    return this.props.saveDropState(treeViewActivePage);
                })
                .then(this.getPageInfo.bind(this, treeViewActivePage.id))
                .then(() => this.setState({
                    droppedItem: item
                }));
        };

        const right = () => null;
        (item.id !== this.state.draggedItem.id && item.id !== this.state.draggedItem.parentId) ? left() : right();
		logger.debug(`<<< Exiting onDrop(e=${e},item=${item})`);
    }


    onMovePage({
        e,
        Action,
        PageId,
        ParentId,
        RelatedPageId,
        RelatedPageParentId
    }) {
	logger.debug('>>> Entering onMovePage()');

        e.preventDefault();
        const { onMovePage } = this.props;

        onMovePage({
            Action,
            PageId,
            ParentId,
            RelatedPageId
        })
            .then((response) => {
                this.removeDropZones();
                if (response.Status === responseStatus.ERROR) {
                    utils.notifyError(response.Message, 3000);
                    return 0;
                }
                utils.notify(Localization.get("PageUpdatedMessage"));
                if (RelatedPageParentId === -1 && ParentId !== -1 && utils.getCurrentPageId() === response.Page.id) {
                    window.parent.location = response.Page.url;
                }
                return 1;
            }).then((response) => {
                response === 1 && this.reOrderPage({
                    Action,
                    PageId,
                    ParentId,
                    RelatedPageId
                });
            });
    }

    removeClone() {
		logger.debug('>>> Entering removeClone()');
        this.clonedElement ? document.body.removeChild(this.clonedElement) : null;
        this.clonedElement = null;
		logger.debug(`removeClone(): this.clonedElement → ${this.clonedElement}`);
		logger.debug('<<< Exiting removeClone()');
    }

    removeDropZones() {
		logger.debug('>>> Entering removeDropZones()');
		logger.debug('<<< Exiting removeDropZones()');
        return new Promise((resolve) => {
            let pageList = null;
            let runUpdateStore = null;
            this.props._traverse((item, list, updateStore) => {
                item.onDragOverState = false;
                pageList = list;
                runUpdateStore = updateStore;
            });

            this.setState({
                pageList
            }, () => {
                runUpdateStore(pageList);
                resolve();
            });
        });
    }

    reOrderPage({
        Action,
        PageId,
        ParentId,
        RelatedPageId
    }) {
        return new Promise((resolve) => {

            let cachedItem = null;
            let itemIndex = null;
            let pageList = null;
            let newParentId = null;
            let newSiblingIndex = null;
            let runUpdateStore = null;

            const removeFromPageList = () => new Promise((rez) => {
                this.props._traverse((item, list, updateStore) => { // remove item from pagelist and cache
                    runUpdateStore = updateStore;
                    switch (true) {
                        case item.id === RelatedPageId && Action === "before":
                            newParentId = item.parentId;
                            this.props._traverse((child) => {
                                if (child.id === PageId) {
                                    const parentId = child.parentId;
                                    this.props._traverse((parent, list) => {
                                        if (parent.id === parentId) {
                                            parent.childListItems.forEach((elm, index) => {
                                                if (elm.id === child.id) {
                                                    cachedItem = child;
                                                    const arr1 = parent.childListItems.slice(0, index);
                                                    const arr2 = parent.childListItems.slice(index + 1);
                                                    const copy = [...arr1, ...arr2];
                                                    parent.childCount--;
                                                    parent.childListItems = copy;
                                                    pageList = list;
                                                }
                                            });
                                        }
                                    });
                                }

                            });

                            break;

                        case item.id === RelatedPageId:
                            newParentId = item.parentId;
                            return;

                        case ParentId === -1 && item.parentId === -1:
                            list.forEach((child, index) => {
                                if (child.id === PageId) {
                                    cachedItem = child;
                                    itemIndex = index;
                                    const arr1 = list.slice(0, index);
                                    const arr2 = list.slice(index + 1);
                                    const copy = [...arr1, ...arr2];
                                    pageList = copy;
                                }
                            });
                            return;

                        case item.id === ParentId:
                            item.childListItems.forEach((child, index) => {
                                if (child.id === PageId) {
                                    child.selected = true;
                                    cachedItem = child;
                                    itemIndex = index;
                                    const arr1 = item.childListItems.slice(0, itemIndex);
                                    const arr2 = item.childListItems.slice(itemIndex + 1);
                                    item.childCount--;
                                    item.childListItems = [...arr1, ...arr2];
                                    pageList = list;
                                }
                            });
                            return;

                        default:
                            list.forEach((item) => {
                                if (item.id === ParentId) {
                                    item.childListItems.forEach((child, index) => {
                                        if (child.id === PageId) {
                                            cachedItem = child;
                                            itemIndex = index;
                                            item.childCount--;
                                            const arr1 = item.childListItems.slice(0, itemIndex);
                                            const arr2 = item.childListItems.slice(itemIndex + 1);
                                            item.childListItems = [...arr1, ...arr2];
                                            pageList = list;
                                        }
                                    });
                                }
                            });
                    }
                });

                this.setState({
                    pageList: pageList
                }, () => {
                    this.getPageInfo(cachedItem.id).then(() => {
                        cachedItem.url = `${window.origin}/${this.props.activePage.url}`;
                        if (pageList)
                            runUpdateStore(pageList);
                        rez();
                    });
                });
            });


            const updateNewParent = () => new Promise((rez) => {
                this.props._traverse((item, list, updateStore) => {
                    runUpdateStore = updateStore;
                    switch (true) {
                        case item.id === newParentId:
                            item.childListItems.forEach((child, index) => {
                                if (child.id === RelatedPageId) {
                                    newSiblingIndex = index;
                                    item.childCount++;
                                    (Action === "after") ? newSiblingIndex++ : null;

                                    const arr1 = item.childListItems.slice(0, newSiblingIndex);
                                    const arr2 = item.childListItems.slice(newSiblingIndex);
                                    cachedItem.parentId = item.id;
                                    item.childListItems = [...arr1, cachedItem, ...arr2];
                                    pageList = list;
                                }
                            });
                            return;
                        case ParentId === -1 || newParentId === -1:
                            list.forEach((child, index) => {
                                if (child.id === RelatedPageId) {
                                    newSiblingIndex = index;
                                    (Action === "after") ? newSiblingIndex++ : null;

                                    const arr1 = list.slice(0, newSiblingIndex);
                                    const arr2 = list.slice(newSiblingIndex);
                                    cachedItem.parentId = -1;
                                    const listCopy = [...arr1, cachedItem, ...arr2];
                                    pageList = listCopy;
                                }
                            });
                            return;
                        default:
                            list.forEach((child, index) => {
                                if (child.id === RelatedPageId && child.parentId === -1) {
                                    newSiblingIndex = index;
                                    (Action === "after") ? newSiblingIndex++ : null;

                                    const arr1 = list.slice(0, index);
                                    const arr2 = list.slice(index);
                                    cachedItem.parentId = -1;
                                    const listCopy = [...arr1, cachedItem, ...arr2];
                                    pageList = listCopy;
                                }
                            });
                    }
                });
                this.setState({
                    pageList
                }, () => {
                    runUpdateStore(pageList);
                    rez();
                });
            });

            removeFromPageList()
                .then(() => updateNewParent())
                .then(() => resolve());
        });
    }

    _countTreeOpenDeepParent(listItems) {
		logger.debug(`>>> Entering _countTreeOpenDeepParent(listItems=${listItems})`);
        let maxCount = 0;
        for (let i = 0; i < listItems.length; i++) {
            let item = listItems[i];
            if (item.isOpen) {
                if (item.childListItems) {
                    let count = this._countTreeDeep(item.childListItems);
					logger.debug(`_countTreeOpenDeepParent(listItems=${listItems}): count → ${count}`);
                    if (count > maxCount) {
                        maxCount = count;
                    }
                }
            }
        }
        this.countTreeDepthOpen = maxCount + 1;
		logger.debug(`<<< Exiting _countTreeOpenDeepParent(listItems=${listItems})`);
        return this.countTreeDepthOpen;
    }

    _countTreeDeep(listItems) {
		logger.debug(`>>> Entering _countTreeDeep(listItems=${listItems})`);
        let count = 0;

		logger.debug(`_countTreeDeep(listItems=${listItems}): count → ${count}`);
        for (let i = 0; i < listItems.length; i++) {
            let item = listItems[i];
            if (item.isOpen) {
                count++;
                if (item.childListItems) {
                    count += this._countTreeDeep(item.childListItems);
                }
                return count;
            }
        }
		logger.debug(`<<< Exiting _countTreeDeep(listItems=${listItems})`);
        return count;
    }

    getChildListItems(id) {
		logger.debug(`>>> Entering getChildListItems(id=${id})`);
        let promise = this.loadAllChildListItems(id);
        promise.then(this.toggleParentCollapsedState(id));
		logger.debug(`<<< Exiting getChildListItems(id=${id})`);
    }

    loadAllChildListItems(id) {
		logger.debug(`>>> Entering loadAllChildListItems(id=${id})`);
		logger.debug(`<<< Exiting loadAllChildListItems(id=${id})`);
        return new Promise((resolve) => {
            const getChildListItems = () => {
                this.props.getChildPageList(id)
                    .then((childListItems) => {
                        this.props._traverse((item, listItems, updateStore) => {
                            const getChildListItemsService = () => {
                                item.childListItems = childListItems;
                            };
                            (item.id === id) ? getChildListItemsService() : null;
                            if (item.id === id) {
                                this.setState({
                                    pageList: listItems
                                }, () => {
                                    updateStore(listItems);
                                    resolve();
                                });
                            }
                        });
                    });
            };
            this.props._traverse((item) => (item.childCount > 0 && item.id === id && !Object.prototype.hasOwnProperty.call(item, "childListItems")) ? getChildListItems() : resolve());
        });
    }

    loadAllChildList() {
		logger.debug('>>> Entering loadAllChildList()');
        let promises = [];
        this.props._traverse((item) => {
            if (item.childCount > 0 && !Object.prototype.hasOwnProperty.call(item, "childListItems")) {
                promises.push(this.loadAllChildListItems(item.id));
            }
        });

        if (promises.length > 0) {
            Promise.all(promises).then(this.setState({ isChildLoaded: true }));
			logger.debug('<<< Exiting loadAllChildList()');
        }
    }

    toggleExpandAll() {
		logger.debug('>>> Entering toggleExpandAll()');
        const {
            isTreeviewExpanded
        } = this.state;

        this.props._traverse((item, list, updateStore) => {
            if (Object.prototype.hasOwnProperty.call(item, "childListItems") && item.childListItems.length > 0) {
                item.isOpen = (isTreeviewExpanded) ? false : true;
                updateStore(list);
                this.setState({
                    isTreeviewExpanded: !this.state.isTreeviewExpanded
                });
            } else if (item.childCount === 0) {
                item.childListItems = [];
                item.isOpen = false;
                item.hasChildren = false;
                updateStore(list);
				logger.debug('<<< Exiting toggleExpandAll()');
            }
        });
    }

    render_treeview() {
		logger.debug('>>> Entering render_treeview()');
		logger.debug('<<< Exiting render_treeview()');
        return (
            <span className="dnn-persona-bar-treeview-ul tree" onMouseOver={(e) => this.props.enabled ? this.setState({ pageX: e.pageX, pageY: e.pageY }) : () => {}} style={{ paddingBottom: "10px" }}>
                {this.state.rootLoaded ?
                    <PersonaBarPageTreeview
                        draggedItem={this.state.draggedItem}
                        droppedItem={this.state.droppedItem}
                        dragOverItem={this.state.dragOverItem}
                        listItems={this.state.pageList}
                        setEmptyPageMessage={this.props.setEmptyPageMessage}
                        getChildListItems={this.getChildListItems.bind(this)}
                        onSelection={this.props.enabled ? this.onSelection.bind(this) : () => {}}
                        onNoPermissionSelection={this.props.enabled ? this.onNoPermissionSelection.bind(this) : () => {}}
                        onDragEnter={this.props.enabled ? this.onDragEnter.bind(this) : () => {}}
                        onDrag={this.props.enabled ? this.onDrag.bind(this) : () => {}}
                        onDragStart={this.props.enabled ? this.onDragStart.bind(this) : () => {}}
                        onDragOver={this.props.enabled ? this.onDragOver.bind(this) : () => {}}
                        onDragLeave={this.props.enabled ? this.onDragLeave.bind(this) : () => {}}
                        onDragEnd={this.props.enabled ? this.onDragEnd.bind(this) : () => {}}
                        onDrop={this.props.enabled ? this.onDrop.bind(this) : () => {}}
                        onMovePage={this.props.enabled ? this.onMovePage.bind(this) : () => {}}
                        getPageInfo={this.getPageInfo.bind(this)}
                        selectedPageDirty={this.props.selectedPageDirty}
                        Localization={this.props.Localization}
                    />
                    : null}
            </span>
        );
    }

    render_treemenu() {
		logger.debug('>>> Entering render_treemenu()');
		logger.debug('<<< Exiting render_treemenu()');
        return (
            <span className="dnn-persona-bar-treeview-ul" >
                {this.state.rootLoaded ?
                    <PersonaBarPageTreeMenu
                        CallCustomAction={this.props.enabled ? this.props.CallCustomAction : () => {}}
                        onAddPage={this.props.enabled ? this.props.onAddPage : () => {}}
                        onViewPage={this.props.enabled ? this.props.onViewPage : () => {}}
                        onViewEditPage={this.props.enabled ? this.props.onViewEditPage : () => {}}
                        onDuplicatePage={this.props.enabled ? this.onDuplicatePage.bind(this) : () => {}}
                        listItems={this.state.pageList}
                        _traverse={this.props._traverse.bind(this)}
                        pageInContextComponents={this.props.pageInContextComponents}
                    /> : null}

            </span>
        );
    }

    render_tree_parent_expand() {
		logger.debug('>>> Entering render_tree_parent_expand()');
		logger.debug('<<< Exiting render_tree_parent_expand()');
        return (
            <span
                className="dnn-persona-bar-treeview-ul" >
                {this.state.rootLoaded ? <PersonaBarPageTreeParentExpand listItems={this.state.pageList} getChildListItems={this.getChildListItems.bind(this)} /> : null}
            </span>
        );
    }

    render_collapseExpand() {
		logger.debug('>>> Entering render_collapseExpand()');
		logger.debug('<<< Exiting render_collapseExpand()');
        return (
            <div
                onClick={this.props.enabled ? this.toggleExpandAll.bind(this) : () => {}}
                className="collapse-expand" >
                [{this.state.isTreeviewExpanded ? Localization.get("lblCollapseAll").toUpperCase() : Localization.get("lblExpandAll").toUpperCase()}]
            </div>
        );
    }

    setMouseOver(isMouseOver) {
		logger.debug(`>>> Entering setMouseOver(isMouseOver=${isMouseOver})`);
        let hasChildren = this.state.pageList && this.state.pageList.some((page) => page.childCount > 0);

        this.setState({
            isMouseInTree: (isMouseOver && hasChildren)
        });
		logger.debug(`<<< Exiting setMouseOver(isMouseOver=${isMouseOver})`);
    }

    calculateTreeContentArea() {
		logger.debug('>>> Entering calculateTreeContentArea()');
		logger.debug('<<< Exiting calculateTreeContentArea()');
        return this.treeContentWidth + (this.countTreeDepthOpen * 25);
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <div onMouseEnter={() => this.setMouseOver(true)} onMouseLeave={() => this.setMouseOver(false)}>
                <GridCell
                    columnSize={30}
                    className="dnn-persona-bar-treeview"
                    style={{ "zIndex": 1000 }} >

                    {this.render_collapseExpand()}

                    <GridCell columnSize={15} >
                        <div className="dnn-persona-bar-treeview-menu" >
                            {this.render_tree_parent_expand()}
                        </div>
                    </GridCell>

                    <GridCell
                        columnSize={55}
                        style={{ marginLeft: "-2px" }} >
                        <ScrollBar contentStyle={{ width: this.calculateTreeContentArea() + "px" }}>
                            {this.render_treeview()}
                        </ScrollBar>
                    </GridCell>

                    <GridCell columnSize={30} >
                        <div
                            className="dnn-persona-bar-treeview-menu selection-arrows"
                            style={{ float: "right" }} >
                            {this.render_treemenu()}
                        </div>
                    </GridCell>

                </GridCell>
            </div>
        );
    }
}

PersonaBarPageTreeviewInteractor.propTypes = {
    _traverse: PropTypes.func.isRequired,
    clearSelectedPage: PropTypes.func.isRequired,
    pageList: PropTypes.array.isRequired,
    showCancelDialog: PropTypes.func.isRequired,
    setEmptyPageMessage: PropTypes.func.isRequired,
    selectedPageDirty: PropTypes.bool.isRequired,
    activePage: PropTypes.object,
    getPage: PropTypes.func.isRequired,
    onSelection: PropTypes.func.isRequired,
    onMovePage: PropTypes.func.isRequired,
    onAddPage: PropTypes.func.isRequired,
    onViewPage: PropTypes.func.isRequired,
    onViewEditPage: PropTypes.func.isRequired,
    onDuplicatePage: PropTypes.func.isRequired,
    CallCustomAction: PropTypes.func.isRequired,
    setActivePage: PropTypes.func.isRequired,
    saveDropState: PropTypes.func.isRequired,
    getChildPageList: PropTypes.func.isRequired,
    getPageList: PropTypes.func,
    pageInContextComponents: PropTypes.array.isRequired,
    Localization: PropTypes.object.isRequired,
    onNoPermissionSelection: PropTypes.func.isRequired,
    NoPermissionSelectionPageId: PropTypes.number,
    enabled: PropTypes.bool
};

PersonaBarPageTreeviewInteractor.defaultProps = {
    enabled: true
};

PersonaBarPageTreeviewInteractor.contextTypes = {
    scrollArea: PropTypes.object
};


const mapStateToProps = (state) => {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return ({
        activePage: state.pages.selectedPage
    });
};
// export default PersonaBarPageTreeviewInteractor;
export const connectedPersonaBarPageTreeviewInteractor = connect(mapStateToProps)(PersonaBarPageTreeviewInteractor);