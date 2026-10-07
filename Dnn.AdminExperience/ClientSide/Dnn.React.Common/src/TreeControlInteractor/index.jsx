import React, { Component } from "react";
import PropTypes from "prop-types";
import TreeControl from "./TreeControl";
import IconSelector from "./icons/IconSelector";
import { Scrollbars } from "react-custom-scrollbars";

import "./styles.less";
const logger = console;

export default class TreeControlInteractor extends Component {

    constructor(props) {
        super();
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.cachedChildTabs;
        this.icon = IconSelector("arrow_bullet");
        this.PortalTabsParameters = props.PortalTabsParameters;
        this.fullyChecked = 2;
		logger.debug(`constructor(props=${props}): this.PortalTabsParameters → ${this.PortalTabsParameters}`, { "event.action": "constructor", "logrite.props": props });
        this.individuallyChecked = 1;
        this.unchecked = 0;
		logger.debug(`constructor(props=${props}): this.fullyChecked → ${this.fullyChecked}`, { "event.action": "constructor", "logrite.props": props });
		logger.debug(`constructor(props=${props}): this.individuallyChecked → ${this.individuallyChecked}`, { "event.action": "constructor", "logrite.props": props });
        this.scrollbar = {};
        this.state = {
            tabs: []
        };
    }

    componentDidMount() {        
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.init();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentDidUpdate() {
		logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
        if (this.scrollbar.getClientWidth() > 200) {
            this.scrollbar.scrollToRight();
			logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
        }
    }

    init() {
	logger.error('>>> Entering init()', { "event.action": "init" });

        const ExportInitialSelection = () => {
			logger.error('>>> Entering ExportInitialSelection()', { "event.action": "ExportInitialSelection" });
            this.export();
			logger.warn('<<< Exiting ExportInitialSelection()', { "event.action": "ExportInitialSelection" });
        };

        this.props.getInitialPortalTabs(this.PortalTabsParameters, (response) => {
            const tabs = [response.Results];
            tabs[0].CheckedState = 2;
            this.setState({ tabs: tabs }, () => {
                ExportInitialSelection();
            });

        });

	logger.warn('<<< Exiting init()', { "event.action": "init" });
    }

    requestDescendantTabs(ParentTabId, callback) {
	logger.error(`>>> Entering requestDescendantTabs(ParentTabId=${ParentTabId},callback=${callback})`, { "event.action": "requestDescendantTabs", "logrite.ParentTabId": ParentTabId, "logrite.callback": callback });

        let descendantTabs = [];
        const captureDecendants = (tabs) => descendantTabs = tabs.map(tab => {
			logger.error('>>> Entering captureDecendants()', { "event.action": "captureDecendants" });
            !Array.isArray(tab.ChildTabs) ? tab.ChildTabs = [] : null;
            return tab;
        });

        const input = (tabs) => compose(tabs, captureDecendants);
        const compose = (tabs, ...fns) => fns.forEach(fn => fn(tabs));
        const appendDescendants = (parentTab) => {
		logger.error(`>>> Entering appendDescendants(parentTab=${parentTab})`, { "event.action": "appendDescendants", "logrite.parentTab": parentTab });

            descendantTabs.forEach((tab) => {
                const condition = parseInt(tab.ParentTabId) === parseInt(parentTab.TabId);
                condition ? parentTab.ChildTabs.push(tab) : null;
            });
			logger.warn(`<<< Exiting appendDescendants(parentTab=${parentTab})`, { "event.action": "appendDescendants", "logrite.parentTab": parentTab });
        };

        this.props.getDescendantPortalTabs(this.PortalTabsParameters, ParentTabId, (response) => {

            const setCheckedState = (tab) => {
				logger.error(`>>> Entering setCheckedState(tab=${tab})`, { "event.action": "setCheckedState", "logrite.tab": tab });
                const left = () => {
					logger.error('>>> Entering left()', { "event.action": "left" });
                    const select = () => {
						logger.error('>>> Entering select()', { "event.action": "select" });
                        tab.ChildTabs = tab.ChildTabs.map(child => {
                            child.CheckedState = child.HasChildren ? this.fullyChecked : this.individuallyChecked;
						logger.warn('<<< Exiting select()', { "event.action": "select" });
                            return child;
                        });
                    };

                    const unselect = () => {
						logger.error('>>> Entering unselect()', { "event.action": "unselect" });
                        tab.ChildTabs = tab.ChildTabs.map(child => {
                            child.CheckedState = this.unchecked;
						logger.warn('<<< Exiting unselect()', { "event.action": "unselect" });
                            return child;
                        });
                    };

                    tab.CheckedState ? select() : unselect();
					logger.warn('<<< Exiting left()', { "event.action": "left" });
                };

                const right = () => null;
                parseInt(tab.TabId) === parseInt(ParentTabId) ? left() : right();
				logger.warn(`<<< Exiting setCheckedState(tab=${tab})`, { "event.action": "setCheckedState", "logrite.tab": tab });
            };

            const tabs = response.Results;
            input(tabs);
            this.traverse(appendDescendants);
            this.traverse(setCheckedState);

            this.setState({ tabs: this.state.tabs });
            callback();
        });
		logger.warn(`<<< Exiting requestDescendantTabs(ParentTabId=${ParentTabId},callback=${callback})`, { "event.action": "requestDescendantTabs", "logrite.ParentTabId": ParentTabId, "logrite.callback": callback });
    }


    generateSelectionObject(tab) {
		logger.error(`>>> Entering generateSelectionObject(tab=${tab})`, { "event.action": "generateSelectionObject", "logrite.tab": tab });
		logger.warn(`<<< Exiting generateSelectionObject(tab=${tab})`, { "event.action": "generateSelectionObject", "logrite.tab": tab });
        return {
            "TabId": parseInt(tab.TabId),
            "ParentTabId": parseInt(tab.ParentTabId),
            "CheckedState": tab.CheckedState,
            "Name": tab.Name
        };
    }


    traverse(comparator) {
		logger.error(`>>> Entering traverse(comparator=${comparator})`, { "event.action": "traverse", "logrite.comparator": comparator });
        const copy = this.state.tabs;
        let ChildTabs = copy;
        const cachedChildTabs = [];
        cachedChildTabs.push(ChildTabs);
        const condition = cachedChildTabs.length > 0;

        const loop = () => {
			logger.error('>>> Entering loop()', { "event.action": "loop" });
            const childTab = cachedChildTabs.length ? cachedChildTabs.shift() : null;
			logger.error(`loop(): childTab → ${childTab}`, { "event.action": "loop" });
            const left = () => childTab.forEach(tab => {
                Array.isArray(tab.ChildTabs) ? comparator(tab, copy) : null;
                Array.isArray(tab.ChildTabs) && tab.ChildTabs.length ? cachedChildTabs.push(tab.ChildTabs) : null;
                condition ? loop() : exit();
            });
            const right = () => null;
            childTab ? left() : right();
			logger.warn('<<< Exiting loop()', { "event.action": "loop" });
        };

        const exit = () => null;
        loop();
		logger.warn(`<<< Exiting traverse(comparator=${comparator})`, { "event.action": "traverse", "logrite.comparator": comparator });
        return;
    }

    findParent(tabData) {
		logger.error(`>>> Entering findParent(tabData=${tabData})`, { "event.action": "findParent", "logrite.tabData": tabData });
        let parent = {};
        const capture = (tab) => {
			logger.error(`>>> Entering capture(tab=${tab})`, { "event.action": "capture", "logrite.tab": tab, "logrite.copy": copy });
            parent = tab || {};
			logger.warn(`<<< Exiting capture(tab=${tab})`, { "event.action": "capture", "logrite.tab": tab, "logrite.copy": copy });
        };
        const find = (tab) => {
			logger.error(`>>> Entering find(tab=${tab})`, { "event.action": "find", "logrite.tab": tab, "logrite.copy": copy });
            const condition = parseInt(tab.TabId) === parseInt(tabData.ParentTabId);
			logger.warn(`<<< Exiting find(tab=${tab})`, { "event.action": "find", "logrite.tab": tab, "logrite.copy": copy });
			logger.error(`find(tab=${tab}): condition → ${condition}`, { "event.action": "find", "logrite.tab": tab, "logrite.copy": copy });
            condition ? capture(tab) : null;
        };
        this.traverse(find);
		logger.warn(`<<< Exiting findParent(tabData=${tabData})`, { "event.action": "findParent", "logrite.tabData": tabData });
        return parent;
    }

    updateTree(tabData) {
		logger.error(`>>> Entering updateTree(tabData=${tabData})`, { "event.action": "updateTree", "logrite.tabData": tabData });
        let newState = null;
        const capture = (tab, copy) => {
            tab = JSON.parse(JSON.stringify(tabData));
            newState = copy;
        };
        const find = (tab, copy) => {
            parseInt(tab.TabId) === parseInt(tabData.TabId) ? capture(tab, copy) : null;
        };
        this.traverse(find);
        this.setState({ tabs: newState });
        this.export(this.state.tabs);
		logger.warn(`<<< Exiting updateTree(tabData=${tabData})`, { "event.action": "updateTree", "logrite.tabData": tabData });
    }

    reAlignTree() {
	logger.error('>>> Entering reAlignTree()', { "event.action": "reAlignTree" });

        let iterationsArray = [];
        const iterations = (t) => t.ChildTabs.length ? iterationsArray.push(...t.ChildTabs) : null;
        this.traverse(iterations);

        const realign = (tab) => {
			logger.error(`>>> Entering realign(tab=${tab})`, { "event.action": "realign", "logrite.tab": tab });
            iterationsArray = [];
            let sum = 0;
            const tabsWithChildren = [];
            const tabsWithoutChildren = [];
            tab.ChildTabs.forEach((t) => {
                t.HasChildren ? tabsWithChildren.push(t) : tabsWithoutChildren.push(t);
            });
            const expect = tabsWithoutChildren.length + tabsWithChildren.length * this.fullyChecked;
            tabsWithoutChildren.forEach(t => {
                t.CheckedState === this.individuallyChecked ? sum += 1 : null;
            });

            tabsWithChildren.forEach(t => {
                switch (true) {
                    case t.CheckedState === this.fullyChecked:
                        sum += 2;
                        return;
                    case t.CheckedState === this.individuallyChecked:
                        sum += 1;
                        return;
                    default:
                        return;

                }
            });

            switch (true) {
                case sum === expect && tab.HasChildren:
                    tab.CheckedState = tab.CheckedState ? this.fullyChecked : tab.CheckedState;

                    break;
                case sum !== 0 && sum === expect && !tab.HasChildren:
                    tab.CheckedState = this.individuallyChecked;

                    break;
                case sum !== 0 && sum < expect:
                    tab.CheckedState = tab.CheckedState === this.fullyChecked ? this.individuallyChecked : tab.CheckedState;
                    break;
                default:
                    break;
            }

			logger.debug(`realign(tab=${tab}): tab.CheckedState → ${tab.CheckedState}`, { "event.action": "realign", "logrite.tab": tab });
            this.updateTree(tab);
			logger.warn(`<<< Exiting realign(tab=${tab})`, { "event.action": "realign", "logrite.tab": tab });
        };

        iterationsArray.forEach(() => this.traverse(realign));
		logger.warn('<<< Exiting reAlignTree()', { "event.action": "reAlignTree" });
    }


    export() {

        let tabs = [];
        let children = [];
        let parents = [];
        const filterOutZeros = (tabs) => tabs.filter(tab => !!tab.CheckedState);
        const filterOnlyParents = (tabs) => tabs.filter(tab => tab.CheckedState === this.fullyChecked);
        const filterOnlyChildren = (tabs) => tabs.filter(tab => tab.CheckedState === this.individuallyChecked);
        const filterOutIfAllSelected = (arr1, arr2) => arr1.filter(tab => {
			logger.error('>>> Entering filterOutIfAllSelected()', { "event.action": "filterOutIfAllSelected" });
            let exists = false;
            arr2.forEach(t => t.TabId === tab.ParentTabId ? exists = t : null);
            return exists === false;
        });

        const generateSelections = (tab) => tabs.push(this.generateSelectionObject(tab));
        this.traverse(generateSelections);
        tabs = filterOutZeros(tabs);
        parents = filterOnlyParents(tabs);
        children = filterOnlyChildren(tabs);
        children = filterOutIfAllSelected(children, parents);
        parents = filterOutIfAllSelected(parents, parents);
        const exports = parents.concat(children);
        this.props.OnSelect(exports);

    }


    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <Scrollbars
                autoHeight
                autoHeightMin={405}
                autoHeightMax={405}
                style={{ width: 340, maxWidth: "100%" }}
                ref={(scrollbar) => { this.scrollbar = scrollbar; }}
            >

                <TreeControl
                    characterLimit={this.props.characterLimit}
                    selectedColor={this.props.selectedColor}
                    icon_type="arrow_bullet"
                    tabs={this.state.tabs}
                    export={this.export}
                    PortalTabsParameters={this.PortalTabsParameters}
                    getDescendantPortalTabs={this.requestDescendantTabs.bind(this)}
                    fullyChecked={this.fullyChecked}
                    individuallyChecked={this.individuallyChecked}
                    unchecked={this.unchecked}
                    updateTree={this.updateTree.bind(this)}
                    reAlignTree={this.reAlignTree.bind(this)}
                    findParent={this.findParent.bind(this)}
                />
            </Scrollbars>
        );

    }
}

TreeControlInteractor.propTypes = {
    selectedColor: PropTypes.string.isRequired,
    characterLimit: PropTypes.number.isRequired,
    PortalTabsParameters: PropTypes.object.isRequired,
    OnSelect: PropTypes.func.isRequired,
    PortalTabParameters: PropTypes.object.isRequired,
    getInitialPortalTabs: PropTypes.func.isRequired,
    getDescendantPortalTabs: PropTypes.func.isRequired
};
