import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "react-collapse";
import SearchBox from "../SearchBox";
import {ArrowDownIcon, ArrowRightIcon, CheckboxUncheckedIcon, CheckboxCheckedIcon, CheckboxPartialCheckedIcon, PagesIcon} from "../SvgIcons";
import Html from "../Html";
import "./style.less";
import Service from "./Service";
import PagePickerScrollbar from "./PagePickerScrollbar";
const logger = console;


function format() {
    let format = arguments[0];
    let methodsArguments = arguments;
    return format.replace(/{(\d+)}/gi, function (value, index) {
        let argumentsIndex = parseInt(index) + 1;
	logger.warn('<<< Exiting format()', { "event.action": "format" });
        return methodsArguments[argumentsIndex];
    });
}

class PagePicker extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            dropDownOpen: false,
            portalTabs: [],
            selectedPages: [],
            totalCount: 0,
            selectedPage: props.defaultLabel
        };
        this.loaded = false;
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
		logger.debug(`constructor(props=${props}): this.loaded → ${this.loaded}`, { "event.action": "constructor", "logrite.props": props });
        this.handleClick = this.handleClick.bind(this);

        this.dnnPagePickerRef = React.createRef();
        this.pagePickerContent = React.createRef();
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        this._isMounted = false;
        if (!this.props.IsMultiSelect) {
            this.setDefaultPage(this.props);
        }
        if (!this.props.IsInDropDown) {
			logger.debug(`UNSAFE_componentWillMount(): this._isMounted → ${this._isMounted}`, { "event.action": "UNSAFE_componentWillMount" });
            this.initialize(this.props);
			logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        }
        this.setState({
            selectedPage: this.props.defaultLabel
        });
    }

    UNSAFE_componentWillReceiveProps(newProps) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
        //reload if any query param changed
        const {props} = this;
        if (props.PortalTabsParameters.portalId !== newProps.PortalTabsParameters.portalId ||
            props.PortalTabsParameters.cultureCode !== newProps.PortalTabsParameters.cultureCode ||
            props.PortalTabsParameters.isMultiLanguage !== newProps.PortalTabsParameters.isMultiLanguage ||
            props.PortalTabsParameters.excludeAdminTabs !== newProps.PortalTabsParameters.excludeAdminTabs ||
            props.PortalTabsParameters.roles !== newProps.PortalTabsParameters.roles) {
            this.setState({portalTabs: []});
            this.loaded = false;
		logger.debug(`UNSAFE_componentWillReceiveProps(newProps=${newProps}): this.loaded → ${this.loaded}`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
            return;
        }

        if (newProps.Reload && !newProps.IsInDropDown && this._isMounted) {
            if (!newProps.IsMultiSelect) {
                if (newProps.selectedTabId !== this.props.selectedTabId) {
                    this.setDefaultPage(newProps);
                }
            }
            this.setState({ selectedPages: [], dropDownOpen: false, portalTabs: [] }, () => {
                this.initialize(newProps);
            });
        }
        else if (!this.props.IsMultiSelect && newProps.IsInDropDown && this._isMounted) {
            if (newProps.selectedTabId !== this.props.selectedTabId) {
                this.setDefaultPage(newProps);
            }
        }
		logger.warn(`<<< Exiting UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this._isMounted = true;
        const {props} = this;
        if (props.closeOnBlur) {
			logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentDidMount" });
            document.addEventListener("click", this.handleClick);
        }
        this._isMounted = true;
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentWillUnmount" });
    }

    initialize(props, callback) {
		logger.error(`>>> Entering initialize(props=${props},callback=${callback})`, { "event.action": "initialize", "logrite.props": props, "logrite.callback": callback });
        this.getPortalTabs(props.cultureCode, () => {
            this.setPages(() => {
                if (callback)
                    callback();
            });
        });
    }

    getPortalTabs(cultureCode, callback) {
		logger.error(`>>> Entering getPortalTabs(cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getPortalTabs", "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        const { props } = this;
        let service = new Service(this.props.serviceFramework, this.props.moduleRoot, this.props.controller);
		logger.error(`getPortalTabs(cultureCode=${cultureCode},callback=${callback}): service → ${service}`, { "event.action": "getPortalTabs", "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        const portalTabsParameters = !props.IsMultiSelect ? Object.assign(props.PortalTabsParameters, { selectedTabId: props.selectedTabId }) : Object.assign(props.PortalTabsParameters);
        service.getPortalTabs(portalTabsParameters, (data) => {
            this.setState({
                portalTabs: [data.Results]
            }, () => {
                if (callback) {
                    callback();
					logger.warn(`<<< Exiting getPortalTabs(cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getPortalTabs", "logrite.cultureCode": cultureCode, "logrite.callback": callback });
                }
            });
        });
    }

    setPages(callback) {
		logger.error(`>>> Entering setPages(callback=${callback})`, { "event.action": "setPages", "logrite.callback": callback });
        const {props} = this;
        this.loaded = true;
        let {portalTabs} = this.state;
		logger.debug(`setPages(callback=${callback}): this.loaded → ${this.loaded}`, { "event.action": "setPages", "logrite.callback": callback });
        if (!props.IsMultiSelect) {
            portalTabs[0].Name = props.noneSpecifiedText;
        }
        if (portalTabs[0].ChildTabs !== undefined) {
            portalTabs[0].IsOpen = true;
            portalTabs[0].Processed = true;
            portalTabs[0].ParentTabId = undefined;
            portalTabs[0].ChildTabs = portalTabs[0].ChildTabs.map(tab => {
                tab.TabId = tab.TabId !== null ? parseInt(tab.TabId) : -1;
                tab.ChildTabs = tab.ChildTabs !== null ? tab.ChildTabs : [];
                return tab;
            });
        }
        this.setState({
            portalTabs
        }, () => {
            this.SetOpenNodesCount(this.state.portalTabs[0], this.state.portalTabs[0].TabId, 0);
            if (props.IsMultiSelect && props.allSelected) {
                this.onPageSelect(portalTabs[0]);
            }
            if (typeof callback === "function")
                callback();
        });
    }

    getClassName() {
		logger.error('>>> Entering getClassName()', { "event.action": "getClassName" });
        const {props} = this;
        let className = "dnn-page-picker";
        if (props.IsInDropDown) {
            className += (props.withBorder ? " with-border" : "");
        }

        className += (" " + props.className);

        if (!props.enabled) {
            className += " disabled";
        }
		logger.warn('<<< Exiting getClassName()', { "event.action": "getClassName" });
        return className;
    }

    getDropdownLabel() {
		logger.error('>>> Entering getDropdownLabel()', { "event.action": "getDropdownLabel" });
        const { state } = this;
        let label = state.selectedPage;
		logger.warn('<<< Exiting getDropdownLabel()', { "event.action": "getDropdownLabel" });
        return label;
    }

    SetOpenNodesCount(root, lastId, count) {
		logger.error(`>>> Entering SetOpenNodesCount(root=${root},lastId=${lastId},count=${count})`, { "event.action": "SetOpenNodesCount", "logrite.root": root, "logrite.lastId": lastId, "logrite.count": count });
        if (root.IsOpen && root.ChildTabs !== null && root.ChildTabs.length > 0) {
            root.ChildTabs.map((child) => {
                count++;
                if (child.IsOpen && child.ChildTabs !== null && child.ChildTabs.length > 0) {
                    count = this.SetOpenNodesCount(child, lastId, count);
                }
                return count;
            });
        }
        if (root.TabId === lastId) {
            this.setState({
                totalCount: count
            });
        }
		logger.warn(`<<< Exiting SetOpenNodesCount(root=${root},lastId=${lastId},count=${count})`, { "event.action": "SetOpenNodesCount", "logrite.root": root, "logrite.lastId": lastId, "logrite.count": count });
        return count;
    }

    onPageSelect(page, keepOpen) {
		logger.error(`>>> Entering onPageSelect(page=${page},keepOpen=${keepOpen})`, { "event.action": "onPageSelect", "logrite.page": page, "logrite.keepOpen": keepOpen });
        keepOpen = keepOpen === undefined || (typeof keepOpen) !== "boolean" ? false : keepOpen;
        const {props} = this;
		logger.error(`onPageSelect(page=${page},keepOpen=${keepOpen}): keepOpen → ${keepOpen}`, { "event.action": "onPageSelect", "logrite.page": page, "logrite.keepOpen": keepOpen });
        if (props.IsMultiSelect) {
            page.CheckedState = page.CheckedState === 1 ? 0 : 1;
            let selectedPagesC = Object.assign([], this.state.selectedPages);
            selectedPagesC = this.CheckUnCheckAllChildren(page, page.CheckedState, selectedPagesC);
			logger.error(`onPageSelect(page=${page},keepOpen=${keepOpen}): selectedPagesC → ${selectedPagesC}`, { "event.action": "onPageSelect", "logrite.page": page, "logrite.keepOpen": keepOpen });
            this.setState({ selectedPages: selectedPagesC }, () => {
                this.DoRecursiveSelect(page, page.TabId, page.CheckedState, () => {
                    let {selectedPages} = this.state;
                    this.props.OnSelect(selectedPages);
                });
            });
        } else {
            let {portalTabs} = this.state;
            this.UnCheckAllExcept(Object.assign([], portalTabs[0]), page, (data) => {
                if (props.selectedTabId === -1) {
                    data.CheckedState = 0;
                } else {
                    data.CheckedState = 1;
                }
                portalTabs[0] = data;
                page.CheckedState = 0;
                this.setState({ portalTabs }, () => {
                    if (page.TabId !== props.selectedTabId) {
                        this.props.OnSelect(page.TabId, page.Name);
                    }
                    if (props.IsInDropDown) {
						logger.warn(`<<< Exiting onPageSelect(page=${page},keepOpen=${keepOpen})`, { "event.action": "onPageSelect", "logrite.page": page, "logrite.keepOpen": keepOpen });
                        this.setState({
                            selectedPage: page.Name,
                            dropDownOpen: keepOpen
                        });
                    }
                });
            });
        }
    }
    //page is passed by reference.
    getDescendants(page, callback, event) {
		logger.error(`>>> Entering getDescendants(page=${page},callback=${callback},event=${event})`, { "event.action": "getDescendants", "logrite.page": page, "logrite.callback": callback, "logrite.event": event });
        if (event) {
            event.stopPropagation();
        }
        const { props } = this;
        let parentToAddChildrenTo = page;
		logger.debug(`getDescendants(page=${page},callback=${callback},event=${event}): parentToAddChildrenTo.IsOpen → ${parentToAddChildrenTo.IsOpen}`, { "event.action": "getDescendants", "logrite.page": page, "logrite.callback": callback, "logrite.event": event });
        if (parentToAddChildrenTo.ChildTabs && parentToAddChildrenTo.ChildTabs.length > 0) {
            parentToAddChildrenTo.IsOpen = !parentToAddChildrenTo.IsOpen;

            //Add timeout to let render through - the dropdown will close on collapse of children if this is not here.
            setTimeout(() => {
                this.setState({}, () => {
                    this.SetOpenNodesCount(this.state.portalTabs[0], this.state.portalTabs[0].TabId, 0);
                });
            }, 0);

            return;
        }
        const portalTabsDescendantsParameters = Object.assign(props.PortalTabsParameters, { parentId: page.TabId });
		logger.error(`getDescendants(page=${page},callback=${callback},event=${event}): portalTabsDescendantsParameters → ${portalTabsDescendantsParameters}`, { "event.action": "getDescendants", "logrite.page": page, "logrite.callback": callback, "logrite.event": event });

        let service = new Service(this.props.serviceFramework, this.props.moduleRoot, this.props.controller);
        service.getTabsDescendants(portalTabsDescendantsParameters,
            (data) => {
                parentToAddChildrenTo.IsOpen = true;
                parentToAddChildrenTo.ChildTabs = data.Results.map((child) => {
                    if (page.CheckedState === 0) {
                        let selectedPages = Object.assign([], this.state.selectedPages);
                        selectedPages = this.UpdateSelectedData(child, page.CheckedState !== undefined ? page.CheckedState : 1, selectedPages);
                        if (props.IsMultiSelect && page.CheckedState === 0) {
                            this.setState({ selectedPages }, () => {
                                let {selectedPages} = this.state;
                                this.props.OnSelect(selectedPages);
                            });
                        }
                    }
                    return {
                        HasChildren: child.HasChildren,
                        ImageUrl: child.ImageUrl,
                        Name: child.Name,
                        ParentTabId: page.TabId,
                        TabId: child.TabId,
                        Tooltip: child.Tooltip,
                        CheckedState: this.props.IsMultiSelect ? (page.CheckedState !== undefined ? page.CheckedState : 1) : 1,
                        ChildTabs: child.ChildTabs !== null ? child.ChildTabs : [],
                        Selectable: child.Selectable
                    };
                });
                this.setState({}, () => {
                    this.SetOpenNodesCount(this.state.portalTabs[0], this.state.portalTabs[0].TabId, 0);
                });
                if (callback) {
                    callback();
                }
            });
			logger.warn(`<<< Exiting getDescendants(page=${page},callback=${callback},event=${event})`, { "event.action": "getDescendants", "logrite.page": page, "logrite.callback": callback, "logrite.event": event });
    }

    getSelected(page, isCurrentOrDescendant) {
		logger.error(`>>> Entering getSelected(page=${page},isCurrentOrDescendant=${isCurrentOrDescendant})`, { "event.action": "getSelected", "logrite.page": page, "logrite.isCurrentOrDescendant": isCurrentOrDescendant });
        let className = "page-value";
        if (page.Selectable && !isCurrentOrDescendant)      
            className += (page.CheckedState !== 1 ? " selected" : "");
        else
            className += " non-selectable";
		logger.warn(`<<< Exiting getSelected(page=${page},isCurrentOrDescendant=${isCurrentOrDescendant})`, { "event.action": "getSelected", "logrite.page": page, "logrite.isCurrentOrDescendant": isCurrentOrDescendant });
        return className;
    }

     
    getChildItems(children, isCurrentOrDescendant) {
		logger.error(`>>> Entering getChildItems(children=${children},isCurrentOrDescendant=${isCurrentOrDescendant})`, { "event.action": "getChildItems", "logrite.children": children, "logrite.isCurrentOrDescendant": isCurrentOrDescendant });
        if (!children) {
            return [];
        }        
        return children.map((page) => {
            const {props} = this;
            const checkboxIcon = this.getCheckboxIcon(page.CheckedState);
			logger.error(`getChildItems(children=${children},isCurrentOrDescendant=${isCurrentOrDescendant}): checkboxIcon → ${checkboxIcon}`, { "event.action": "getChildItems", "logrite.children": children, "logrite.isCurrentOrDescendant": isCurrentOrDescendant });
            const pageIcon = PagesIcon;
            const textClass = props.IsMultiSelect && props.ShowIcon ? " text-with-page-icon" : (!props.IsMultiSelect && !props.ShowIcon ? "no-icon" : "");
			logger.error(`getChildItems(children=${children},isCurrentOrDescendant=${isCurrentOrDescendant}): textClass → ${textClass}`, { "event.action": "getChildItems", "logrite.children": children, "logrite.isCurrentOrDescendant": isCurrentOrDescendant });
			logger.error(`getChildItems(children=${children},isCurrentOrDescendant=${isCurrentOrDescendant}): parentNotSelectable → ${parentNotSelectable}`, { "event.action": "getChildItems", "logrite.children": children, "logrite.isCurrentOrDescendant": isCurrentOrDescendant });
            const pageClass = props.IsMultiSelect && props.ShowIcon ? " page-icon" : "";
			logger.error(`getChildItems(children=${children},isCurrentOrDescendant=${isCurrentOrDescendant}): pageClass → ${pageClass}`, { "event.action": "getChildItems", "logrite.children": children, "logrite.isCurrentOrDescendant": isCurrentOrDescendant });
            const parentNotSelectable = isCurrentOrDescendant ? isCurrentOrDescendant : this.props.currentTabId === parseInt(page.TabId);
            
            return <li key={page.TabId} className={"page-item page-" + page.TabId + (page.HasChildren ? " has-children" : "") + (page.IsOpen ? " opened" : " closed") }>
                {(!page.IsOpen && page.HasChildren) && <div className="arrow-icon" onClick={this.getDescendants.bind(this, page, null) }><ArrowRightIcon /></div>}
                {(page.IsOpen && page.HasChildren) && <div className="arrow-icon" onClick={this.getDescendants.bind(this, page, null) }><ArrowDownIcon /></div>}
                <div className={this.getSelected(page, parentNotSelectable) } onClick={page.Selectable && !parentNotSelectable ? this.onPageSelect.bind(this, page) : void (0) }>
                    { props.ShowIcon &&
                        <div className={pageClass }><pageIcon /></div>
                    }
                    {props.IsMultiSelect && <div ><checkboxIcon /></div>}
                    <div className={textClass }>{page.Name}</div>
                </div>
                {page.ChildTabs !== null && page.ChildTabs.length > 0 &&
                    <ul className={"child-pages" + (page.IsOpen ? " opened" : "") }>
                        {this.getChildItems(page.ChildTabs, parentNotSelectable) }
                    </ul>
                }
            </li>;
        });
		logger.warn(`<<< Exiting getChildItems(children=${children},isCurrentOrDescendant=${isCurrentOrDescendant})`, { "event.action": "getChildItems", "logrite.children": children, "logrite.isCurrentOrDescendant": isCurrentOrDescendant });
    }

    toggleDropdown() {
		logger.error('>>> Entering toggleDropdown()', { "event.action": "toggleDropdown" });
        const {props} = this;
        if (!this.loaded) {
            this.initialize(props, () => {
                if (props.selectedTabId === -1) {
                    this.setDefaultPage(props, true);
                }
            });
        }
        if (props.enabled) {
            this.setState({
                dropDownOpen: !this.state.dropDownOpen
            });
        }
        else {
			logger.warn('<<< Exiting toggleDropdown()', { "event.action": "toggleDropdown" });
            this.setState({
                dropDownOpen: false
            });
        }
    }

    getCountText() {
		logger.error('>>> Entering getCountText()', { "event.action": "getCountText" });
		logger.warn('<<< Exiting getCountText()', { "event.action": "getCountText" });
        return format(this.props.CountText, this.state.totalCount);
    }

    findTabById(tabId, tabs, tab) {
		logger.error(`>>> Entering findTabById(tabId=${tabId},tabs=${tabs},tab=${tab})`, { "event.action": "findTabById", "logrite.tabId": tabId, "logrite.tabs": tabs, "logrite.tab": tab });
        if (tab === null) {
            for (let i = 0; i < tabs.length; i++) {
                let child = tabs[i];
                if (tab === null) {
                    if (child.TabId === tabId) {
                        tab = child;
                    }
                    else if (child.HasChildren && child.ChildTabs !== null && child.ChildTabs.length > 0) {
                        tab = this.findTabById(tabId, child.ChildTabs, tab);
                    }
                } else {
                    break;
                }
            }
        }
		logger.warn(`<<< Exiting findTabById(tabId=${tabId},tabs=${tabs},tab=${tab})`, { "event.action": "findTabById", "logrite.tabId": tabId, "logrite.tabs": tabs, "logrite.tab": tab });
        return tab;
    }
    //Single Selection Methods
    //Set the dropdown label to the default selected page name.
    setDefaultPage(props, keepOpen) {
		logger.error(`>>> Entering setDefaultPage(props=${props},keepOpen=${keepOpen})`, { "event.action": "setDefaultPage", "logrite.props": props, "logrite.keepOpen": keepOpen });
        if (props.IsInDropDown && !props.IsMultiSelect) {
            let {portalTabs} = this.state;
            if (portalTabs !== undefined && portalTabs.length > 0) {
                let page = null;
				logger.error(`setDefaultPage(props=${props},keepOpen=${keepOpen}): page → ${page}`, { "event.action": "setDefaultPage", "logrite.props": props, "logrite.keepOpen": keepOpen });
                if (props.selectedTabId === -1) {
                    page = portalTabs[0];
                } else {
                    page = this.findTabById(props.selectedTabId, portalTabs[0].ChildTabs, null);
                }
                if (page !== undefined && page !== null) {
                    this.onPageSelect(page, keepOpen);
					logger.warn(`<<< Exiting setDefaultPage(props=${props},keepOpen=${keepOpen})`, { "event.action": "setDefaultPage", "logrite.props": props, "logrite.keepOpen": keepOpen });
                }
            }
            else if (props.selectedTabId >= 0) {
                let service = new Service(this.props.serviceFramework, this.props.moduleRoot, this.props.controller);
                service.getPortalTab({
                    portalId: props.PortalTabsParameters.portalId,
                    tabId: props.selectedTabId,
                    cultureCode: props.PortalTabsParameters.cultureCode
                }, (tab) => {
                    let {selectedPage} = this.state;
                    selectedPage = tab.Results.Name;
                    this.setState({ selectedPage });
                });
            }
            else if (props.selectedTabId === -1) {
                let {selectedPage} = this.state;
                selectedPage = props.noneSpecifiedText;
                this.setState({ selectedPage });
            }
        }
    }

    handleClick(event) {
		logger.error(`>>> Entering handleClick(event=${event})`, { "event.action": "handleClick", "logrite.event": event });
        const {props} = this;
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted || !props.closeOnBlur || !props.IsInDropDown) { return; }
        if (!this.dnnPagePickerRef.current.contains(event.target) && (typeof event.target.className === "string" && event.target.className.indexOf("do-not-close") === -1)) {
			logger.warn(`<<< Exiting handleClick(event=${event})`, { "event.action": "handleClick", "logrite.event": event });
            this.setState({
                dropDownOpen: false
            });
        }
    }

    UnCheckAllExcept(root, page, callback) {
		logger.error(`>>> Entering UnCheckAllExcept(root=${root},page=${page},callback=${callback})`, { "event.action": "UnCheckAllExcept", "logrite.root": root, "logrite.page": page, "logrite.callback": callback });
        if (page.CheckedState !== 0) {
            if (root.HasChildren && root.ChildTabs !== null && root.ChildTabs.length > 0) {
                root.ChildTabs = root.ChildTabs.map(child => {
                    if (child.TabId !== page.TabId) {
                        child.CheckedState = 1;
                    }
                    if (child.HasChildren && child.ChildTabs !== null && child.ChildTabs.length > 0) {
                        child = this.UnCheckAllExcept(child, page);
                    }
                    return child;
                });
            }
            if (callback !== undefined) {
                callback(root);
            }
        }
        if (callback !== undefined) {
            callback(root);
        }
		logger.warn(`<<< Exiting UnCheckAllExcept(root=${root},page=${page},callback=${callback})`, { "event.action": "UnCheckAllExcept", "logrite.root": root, "logrite.page": page, "logrite.callback": callback });
        return root;
    }

    onSearch(value) {
		logger.error(`>>> Entering onSearch(value=${value})`, { "event.action": "onSearch", "logrite.value": value });
        const {props} = this;

        if (value !== "") {
            const apiParameters = Object.assign(props.PortalTabsParameters, { searchText: value });
			logger.error(`onSearch(value=${value}): apiParameters → ${apiParameters}`, { "event.action": "onSearch", "logrite.value": value });
            let service = new Service(this.props.serviceFramework, this.props.moduleRoot, this.props.controller);
			logger.error(`onSearch(value=${value}): service → ${service}`, { "event.action": "onSearch", "logrite.value": value });

            service.searchPortalTabs(apiParameters, (data) => {
                this.setState({
                    portalTabs: [data.Results]
                }, () => {
                    this.setPages();
                });
            });
        } else {
            this.initialize(props);
			logger.warn(`<<< Exiting onSearch(value=${value})`, { "event.action": "onSearch", "logrite.value": value });
        }
    }
    //Single Selection Methods Ends

    //Multi Selection Methods
    CheckUnCheckAllChildren(root, checked, selectedPages) {
		logger.error(`>>> Entering CheckUnCheckAllChildren(root=${root},checked=${checked},selectedPages=${selectedPages})`, { "event.action": "CheckUnCheckAllChildren", "logrite.root": root, "logrite.checked": checked, "logrite.selectedPages": selectedPages });
        selectedPages = this.UpdateSelectedData(root, checked, selectedPages);
        root.CheckedState = checked;
        this.setState({});
		logger.debug(`CheckUnCheckAllChildren(root=${root},checked=${checked},selectedPages=${selectedPages}): root.CheckedState → ${root.CheckedState}`, { "event.action": "CheckUnCheckAllChildren", "logrite.root": root, "logrite.checked": checked, "logrite.selectedPages": selectedPages });
        if (root.HasChildren && root.ChildTabs.length > 0) {
            root.ChildTabs.forEach(child => {
                selectedPages = this.CheckUnCheckAllChildren(child, checked, selectedPages);
            });
        }
		logger.warn(`<<< Exiting CheckUnCheckAllChildren(root=${root},checked=${checked},selectedPages=${selectedPages})`, { "event.action": "CheckUnCheckAllChildren", "logrite.root": root, "logrite.checked": checked, "logrite.selectedPages": selectedPages });
        return selectedPages;
    }

    UpdateSelectedData(Tab, checked, selectedPages) {
		logger.error(`>>> Entering UpdateSelectedData(Tab=${Tab},checked=${checked},selectedPages=${selectedPages})`, { "event.action": "UpdateSelectedData", "logrite.Tab": Tab, "logrite.checked": checked, "logrite.selectedPages": selectedPages });
        if ((checked === 0 || checked === 2) && !selectedPages.some(page => page.TabId === Tab.TabId && page.CheckedState === checked)) {
            selectedPages = selectedPages.filter(page => {
                return page.TabId !== Tab.TabId;
            });
            selectedPages = [{
                TabId: Tab.TabId,
                ParentTabId: Tab.ParentTabId !== undefined ? Tab.ParentTabId : -1,
                CheckedState: checked
            }].concat(selectedPages);

        } else if (checked === 1 && selectedPages.some(page => page.TabId === Tab.TabId)) {
            selectedPages = selectedPages.filter(page => {
                return page.TabId !== Tab.TabId;
            });
        }
		logger.warn(`<<< Exiting UpdateSelectedData(Tab=${Tab},checked=${checked},selectedPages=${selectedPages})`, { "event.action": "UpdateSelectedData", "logrite.Tab": Tab, "logrite.checked": checked, "logrite.selectedPages": selectedPages });
        return selectedPages;
    }

    DoRecursiveSelect(page, mainKey, originalChecked, callback) {
		logger.error(`>>> Entering DoRecursiveSelect(page=${page},mainKey=${mainKey},originalChecked=${originalChecked},callback=${callback})`, { "event.action": "DoRecursiveSelect", "logrite.page": page, "logrite.mainKey": mainKey, "logrite.originalChecked": originalChecked, "logrite.callback": callback });
        let {portalTabs} = this.state;
        let checked = originalChecked;
        if (mainKey !== page.TabId && page.HasChildren && page.ChildTabs.length > 0) {
            if (!page.ChildTabs.some(child => child.CheckedState === undefined || child.CheckedState === 1 || child.CheckedState === 2))
                checked = 0;
            else if (page.ChildTabs.some(child => child.CheckedState !== undefined && child.CheckedState === 0 && child.CheckedState === 2))
                checked = 1;
            else if (page.ChildTabs.some(child => child.CheckedState !== undefined && (child.CheckedState === 2 || child.CheckedState === 0)))
                checked = 2;
        }
        let selectedPages = Object.assign([], this.state.selectedPages);
        selectedPages = this.UpdateSelectedData(page, checked, selectedPages);
        this.setState({ selectedPages }, () => {
            page.CheckedState = checked;
            this.setState({});
            let parentId = page.ParentTabId;
            if (parentId !== undefined) {
                let children = portalTabs;
                let parent = this.RecursiveFindParent(parentId, children);
				logger.error(`DoRecursiveSelect(page=${page},mainKey=${mainKey},originalChecked=${originalChecked},callback=${callback}): parent → ${parent}`, { "event.action": "DoRecursiveSelect", "logrite.page": page, "logrite.mainKey": mainKey, "logrite.originalChecked": originalChecked, "logrite.callback": callback });
                if (parent !== undefined && parent !== null) {
                    this.setState({});
                    if (parent.HasChildren && parent.ChildTabs.length > 0) {
                        this.DoRecursiveSelect(parent, mainKey, originalChecked, callback);
                    } else {
                        return;
                    }
                }
            } else {
                let selectedPages = Object.assign([], this.state.selectedPages);
                selectedPages = this.UpdateSelectedData(this.state.portalTabs[0], checked, selectedPages);
				logger.error(`DoRecursiveSelect(page=${page},mainKey=${mainKey},originalChecked=${originalChecked},callback=${callback}): selectedPages → ${selectedPages}`, { "event.action": "DoRecursiveSelect", "logrite.page": page, "logrite.mainKey": mainKey, "logrite.originalChecked": originalChecked, "logrite.callback": callback });
                this.setState({ selectedPages });
                if (callback)
                    callback();
            }
        });
        if (callback)
            callback();
    }

    RecursiveFindParent(parentId, currentTree) {
		logger.error(`>>> Entering RecursiveFindParent(parentId=${parentId},currentTree=${currentTree})`, { "event.action": "RecursiveFindParent", "logrite.parentId": parentId, "logrite.currentTree": currentTree });
        let parent = null;
        if (currentTree.some(child => child.IsOpen && child.TabId === parentId)) {
            parent = currentTree.find(child => child.IsOpen && child.TabId === parentId);
        } else if (parent === undefined || parent === null) {
            for (let child of currentTree) {
                if (child.HasChildren && child.ChildTabs.length > 0) {
                    parent = this.RecursiveFindParent(parentId, child.ChildTabs);
                    if (parent !== undefined && parent !== null) {
                        break;
                    }
                }
            }
        }
        return parent;
    }

    getCheckboxIcon(selected) {
		logger.error(`>>> Entering getCheckboxIcon(selected=${selected})`, { "event.action": "getCheckboxIcon", "logrite.selected": selected });
        switch (selected) {
            case 0:
                return CheckboxCheckedIcon;
            case 2:
                return CheckboxPartialCheckedIcon;
            case 1:
                return CheckboxUncheckedIcon;
            default:
				logger.warn(`<<< Exiting getCheckboxIcon(selected=${selected})`, { "event.action": "getCheckboxIcon", "logrite.selected": selected });
                return CheckboxUncheckedIcon;
        }
    }

    //Multi Selection Methods Ends
     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
        
        if (this.loaded || props.IsInDropDown) {
            let picker = <div/>;
            if (state.portalTabs.length > 0 && state.portalTabs[0].Processed) {
                let children = state.portalTabs.length > 0 && state.portalTabs[0].Processed && this.getChildItems(state.portalTabs);
				logger.error(`render(): children → ${children}`, { "event.action": "render" });
                picker =
                    <PagePickerScrollbar style={props.scrollAreaStyle} ref={node => this.node = node}>
                        <div className="pages-container">
                            <ul>
                                {state.portalTabs.length > 0 && state.portalTabs[0].Processed && children}
                                {state.portalTabs.length <= 0 || !state.portalTabs[0].Processed && props.noneSpecified}
                            </ul>
                        </div>
                    </PagePickerScrollbar>;
            }
            return (
                <div className={this.getClassName() } style={props.style} ref={this.dnnPagePickerRef}>
                    {
                        props.IsInDropDown &&
                        <div className="collapsible-label" onClick={this.toggleDropdown.bind(this) }>
                            {this.getDropdownLabel() }
                        </div>
                    }
                    {props.IsInDropDown && props.withIcon &&
                        <div className="dropdown-icon" onClick={this.toggleDropdown.bind(this) }><ArrowDownIcon /></div>
                    }
                    {props.IsInDropDown &&
                        <div className={"collapsible-content" + (state.dropDownOpen ? " open" : "") } style={props.style}>
                            <Collapse
                                theme={{collapse: "page-picker-content ReactCollapse--collapse", content: "ReactCollapse--content"}}
                                ref={this.pagePickerContentRef}
                                isOpened={state.dropDownOpen}>
                                {!props.IsMultiSelect && props.SearchEnabled &&
                                    <SearchBox
                                        style={Object.assign({ display: "block" }, props.searchBoxStyle) }
                                        placeholder={props.placeholderText}
                                        className="page-picker-search"
                                        onSearch={this.onSearch.bind(this) }
                                        iconStyle={Object.assign({ right: 4 }, props.iconStyle) }
                                        inputDisabled={!state.dropDownOpen}
                                    />
                                }
                                {picker}
                                {props.ShowCount && <div className="count"><Html html={this.getCountText()} /></div>}
                            </Collapse>
                        </div>
                    }
                    {
                        !props.IsInDropDown && <div className="page-picker-content">{ picker }
                            {props.ShowCount && <div className="count"><Html html={this.getCountText()} /></div>}
                        </div>
                    }

                </div>
            );
        }
        else {
			logger.warn('<<< Exiting render()', { "event.action": "render" });
            return (<div/>);
        }
    }
}

PagePicker.propTypes = {

    //Class name to attach to parent
    className: PropTypes.string,

    //Style of scrollarea - page picker box, override if necessary.
    scrollAreaStyle: PropTypes.object,

    //Whether or not the dropdown toggle has a border.
    withBorder: PropTypes.bool,

    //Show the dropdown icon
    withIcon: PropTypes.bool,

    //Set to false to disable the picker (readonly)
    enabled: PropTypes.bool,

    //Close the picker on click outside.
    closeOnBlur: PropTypes.bool,

    //Placeholder text in searchbox.
    placeholderText: PropTypes.string,

    //None specified Text- default if none is selected
    noneSpecifiedText: PropTypes.string,

    //Default label shown before pinging API.
    defaultLabel: PropTypes.string,

    //callback method on selection of a tab. 
    //In case of "Single" selection mode it will send back currently selected tabid and tabname.
    //In case of "Multi" selection mode it will send back array currently selected tabs. 
    OnSelect: PropTypes.func.isRequired,

    //Send as true if you all the tabs to be pre selected.  This will work only in case of IsMultiSelect=true
    allSelected: PropTypes.bool,

    //The parameters to list the portal tabs. Default is : {portalId: 0,cultureCode: "",isMultiLanguage: false,excludeAdminTabs: false,disabledNotSelectable: false,roles: "1;-1",sortOrder: 0}
    PortalTabsParameters: PropTypes.object,

    //Id of the tab which should be selected by default. Default is -1 
    //This will work only in case of IsMultiSelect=false.
    selectedTabId: PropTypes.number,

    //Tells the component to reload. Use this if circumstances have changed at UI end and need the page picker to reload with new props. e.g. cultureCode code changed.
    Reload: PropTypes.bool,

    //Selection mode of the tree. Default is "Single"
    IsMultiSelect: PropTypes.bool,

    //Default: true, Shows the tree in the dropdown mode.
    IsInDropDown: PropTypes.bool,

    ShowIcon: PropTypes.bool,

    //Default: true, Enables the search of the tabs. This will work only in case of IsMultiSelect=false
    SearchEnabled: PropTypes.bool,

    //If true, show the count of total tabs in the tree.
    ShowCount: PropTypes.bool,

    //The text to show in the count. Default is "<strong>{0}</strong> Results". Please include {0} at the place you want to show the count.
    CountText: PropTypes.string,

    //Service Framework module root
    moduleRoot: PropTypes.string,

    //Service Framework controller
    controller: PropTypes.string,

    serviceFramework: PropTypes.object,

    currentTabId: PropTypes.number,

    scrollbarThumbStyleDefault: PropTypes.object
};

PagePicker.defaultProps = {
    defaultLabel: "-- Select --",
    IsMultiSelect: false,
    SearchEnabled: true,
    ShowCount: true,
    CountText: "<strong>{0}</strong> Results",
    ShowIcon: true,
    IsInDropDown: true,
    Reload: false,
    noneSpecifiedText: "< None Specified >",
    withIcon: true,
    withBorder: true,
    closeOnBlur: true,
    enabled: true,
    className: "",
    placeholderText: "Search pages..",
    style: {
        width: 300
    },
    scrollAreaStyle: {
        width: "100%",
        minHeight: 300,
        marginTop: 0,
        border: "1px solid #c8c8c8"
    },
    moduleRoot: "PersonaBar",
    controller: "Tabs",
    PortalTabsParameters: {
        portalId: -2,
        cultureCode: "",
        isMultiLanguage: false,
        excludeAdminTabs: false,
        disabledNotSelectable: false,
        roles: "1;-1",
        sortOrder: 0,
        includeDeletedChildren: true
    },
    selectedTabId: -1
};


export default PagePicker;