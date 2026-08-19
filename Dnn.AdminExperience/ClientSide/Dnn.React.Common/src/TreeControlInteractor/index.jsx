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
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.cachedChildTabs;
        this.icon = IconSelector("arrow_bullet");
        this.PortalTabsParameters = props.PortalTabsParameters;
        this.fullyChecked = 2;
        this.individuallyChecked = 1;
		logger.debug(`constructor(props=${props}): this.PortalTabsParameters → ${this.PortalTabsParameters}`);
        this.unchecked = 0;
        this.scrollbar = {};
		logger.debug(`constructor(props=${props}): this.fullyChecked → ${this.fullyChecked}`);
		logger.debug(`constructor(props=${props}): unchecked → ${unchecked}`);
        this.state = {
            tabs: []
        };
    }

    componentDidMount() {        
		logger.debug('>>> Entering componentDidMount()');
        this.init();
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentDidUpdate() {
		logger.debug('>>> Entering componentDidUpdate()');
        if (this.scrollbar.getClientWidth() > 200) {
            this.scrollbar.scrollToRight();
			logger.debug('<<< Exiting componentDidUpdate()');
        }
    }

    init() {
	logger.debug('>>> Entering init()');

        const ExportInitialSelection = () => {
			logger.debug('>>> Entering ExportInitialSelection()');
            this.export();
			logger.debug('<<< Exiting ExportInitialSelection()');
        };

        this.props.getInitialPortalTabs(this.PortalTabsParameters, (response) => {
            const tabs = [response.Results];
            tabs[0].CheckedState = 2;
            this.setState({ tabs: tabs }, () => {
                ExportInitialSelection();
            });

        });

	logger.debug('<<< Exiting init()');
    }

    requestDescendantTabs(ParentTabId, callback) {
	logger.debug(`>>> Entering requestDescendantTabs(ParentTabId=${ParentTabId},callback=${callback})`);

        let descendantTabs = [];
        const captureDecendants = (tabs) => descendantTabs = tabs.map(tab => {
            !Array.isArray(tab.ChildTabs) ? tab.ChildTabs = [] : null;
            return tab;
        });

        const input = (tabs) => compose(tabs, captureDecendants);
        const compose = (tabs, ...fns) => fns.forEach(fn => fn(tabs));
        const appendDescendants = (parentTab) => {
		logger.debug(`>>> Entering appendDescendants(parentTab=${parentTab})`);

            descendantTabs.forEach((tab) => {
                const condition = parseInt(tab.ParentTabId) === parseInt(parentTab.TabId);
                condition ? parentTab.ChildTabs.push(tab) : null;
            });
			logger.debug(`<<< Exiting appendDescendants(parentTab=${parentTab})`);
        };

        this.props.getDescendantPortalTabs(this.PortalTabsParameters, ParentTabId, (response) => {

            const setCheckedState = (tab) => {
				logger.debug(`>>> Entering setCheckedState(tab=${tab})`);
                const left = () => {
					logger.debug('>>> Entering left()');
                    const select = () => {
						logger.debug('>>> Entering select()');
                        tab.ChildTabs = tab.ChildTabs.map(child => {
                            child.CheckedState = child.HasChildren ? this.fullyChecked : this.individuallyChecked;
							logger.debug('<<< Exiting select()');
                            return child;
                        });
                    };

                    const unselect = () => {
						logger.debug('>>> Entering unselect()');
                        tab.ChildTabs = tab.ChildTabs.map(child => {
                            child.CheckedState = this.unchecked;
							logger.debug('<<< Exiting unselect()');
                            return child;
                        });
                    };

                    tab.CheckedState ? select() : unselect();
					logger.debug('<<< Exiting left()');
                };

                const right = () => null;
                parseInt(tab.TabId) === parseInt(ParentTabId) ? left() : right();
				logger.debug(`<<< Exiting setCheckedState(tab=${tab})`);
            };

            const tabs = response.Results;
            input(tabs);
            this.traverse(appendDescendants);
            this.traverse(setCheckedState);

            this.setState({ tabs: this.state.tabs });
            callback();
        });
		logger.debug(`<<< Exiting requestDescendantTabs(ParentTabId=${ParentTabId},callback=${callback})`);
    }


    generateSelectionObject(tab) {
		logger.debug(`>>> Entering generateSelectionObject(tab=${tab})`);
		logger.debug(`<<< Exiting generateSelectionObject(tab=${tab})`);
        return {
            "TabId": parseInt(tab.TabId),
            "ParentTabId": parseInt(tab.ParentTabId),
            "CheckedState": tab.CheckedState,
            "Name": tab.Name
        };
    }


    traverse(comparator) {
		logger.debug(`>>> Entering traverse(comparator=${comparator})`);
        const copy = this.state.tabs;
        let ChildTabs = copy;
        const cachedChildTabs = [];
        cachedChildTabs.push(ChildTabs);
        const condition = cachedChildTabs.length > 0;

        const loop = () => {
			logger.debug('>>> Entering loop()');
            const childTab = cachedChildTabs.length ? cachedChildTabs.shift() : null;
            const left = () => childTab.forEach(tab => {
                Array.isArray(tab.ChildTabs) ? comparator(tab, copy) : null;
                Array.isArray(tab.ChildTabs) && tab.ChildTabs.length ? cachedChildTabs.push(tab.ChildTabs) : null;
                condition ? loop() : exit();
            });
            const right = () => null;
            childTab ? left() : right();
			logger.debug('<<< Exiting loop()');
        };

        const exit = () => null;
        loop();
		logger.debug(`<<< Exiting traverse(comparator=${comparator})`);
        return;
    }

    findParent(tabData) {
		logger.debug(`>>> Entering findParent(tabData=${tabData})`);
        let parent = {};
        const capture = (tab) => {
			logger.debug(`>>> Entering capture(tab=${tab})`);
            parent = tab || {};
			logger.debug(`<<< Exiting capture(tab=${tab})`);
			logger.debug(`capture(tab=${tab},copy=${copy}): tab → ${tab}`);
        };
        const find = (tab) => {
			logger.debug(`>>> Entering find(tab=${tab})`);
            const condition = parseInt(tab.TabId) === parseInt(tabData.ParentTabId);
			logger.debug(`<<< Exiting find(tab=${tab})`);
			logger.debug(`find(tab=${tab}): condition → ${condition}`);
            condition ? capture(tab) : null;
        };
        this.traverse(find);
		logger.debug(`<<< Exiting findParent(tabData=${tabData})`);
        return parent;
    }

    updateTree(tabData) {
		logger.debug(`>>> Entering updateTree(tabData=${tabData})`);
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
		logger.debug(`<<< Exiting updateTree(tabData=${tabData})`);
    }

    reAlignTree() {
	logger.debug('>>> Entering reAlignTree()');

        let iterationsArray = [];
        const iterations = (t) => t.ChildTabs.length ? iterationsArray.push(...t.ChildTabs) : null;
        this.traverse(iterations);

        const realign = (tab) => {
			logger.debug(`>>> Entering realign(tab=${tab})`);
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

			logger.debug(`reAlignTree(): tab.CheckedState → ${tab.CheckedState}`);
            this.updateTree(tab);
			logger.debug(`<<< Exiting realign(tab=${tab})`);
        };

        iterationsArray.forEach(() => this.traverse(realign));
		logger.debug('<<< Exiting reAlignTree()');
    }


    export() {

        let tabs = [];
        let children = [];
        let parents = [];
        const filterOutZeros = (tabs) => tabs.filter(tab => !!tab.CheckedState);
        const filterOnlyParents = (tabs) => tabs.filter(tab => tab.CheckedState === this.fullyChecked);
        const filterOnlyChildren = (tabs) => tabs.filter(tab => tab.CheckedState === this.individuallyChecked);
        const filterOutIfAllSelected = (arr1, arr2) => arr1.filter(tab => {
			logger.debug(`>>> Entering filterOutIfAllSelected(arr1=${arr1},arr2=${arr2})`);
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
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
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
