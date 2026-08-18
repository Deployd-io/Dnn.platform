import React, { Component } from "react";
import PropTypes from "prop-types";
import IconSelector from "./icons/IconSelector";
import { global } from "./global";
import TextOverflowWrapperNew from "../TextOverflowWrapperNew";
const logger = console;

const styles = global.styles;
const floatLeft = styles.float();
const merge = styles.merge;

import "./styles.less";

export default class TreeControl extends Component {

    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        const icon_type = props.icon_type;
        this.icon = IconSelector(icon_type);
        this.export = props.export;
        this.getDescendantPortalTabs = props.getDescendantPortalTabs;
        this.textOverflowRefs = [];
		logger.debug(`constructor(props=${props}): this.textOverflowRefs → ${this.textOverflowRefs}`);
		logger.debug(`constructor(props=${props}): getDescendantPortalTabs → ${getDescendantPortalTabs}`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    _traverse(comparator) {
		logger.debug(`>>> Entering _traverse(comparator=${comparator})`);
        let ChildTabs = this.props.tabs;
        const cachedChildTabs = [];
        cachedChildTabs.push(ChildTabs);
        const condition = cachedChildTabs.length > 0;

        const loop = () => {
			logger.debug('>>> Entering loop()');
            const childTab = cachedChildTabs.length ? cachedChildTabs.shift() : null;
            const left = () => childTab.forEach(tab => {
				logger.debug('>>> Entering left()');
                Array.isArray(tab.ChildTabs) ? comparator(tab, this.props.tabs) : null;
                Array.isArray(tab.ChildTabs) && tab.ChildTabs.length ? cachedChildTabs.push(tab.ChildTabs) : null;
				logger.debug('<<< Exiting left()');
                condition ? loop() : exit();
            });
            const right = () => null;
            childTab ? left() : right();
			logger.debug('<<< Exiting loop()');
        };

        const exit = () => null;

        loop();
		logger.debug(`<<< Exiting _traverse(comparator=${comparator})`);
        return;
    }

    _mapToParentTabs(parent, fn) {
        const condition = (par) => Object.keys(par).length > 0;
        parent = this.props.findParent(parent);
		logger.debug(`_mapToParentTabs(parent=${parent},fn=${fn}): parent → ${parent}`);
        const loop = () => {
            parent ? fn(parent) : null;
            parent = parseInt(parent.TabId) !== -1 ? this.props.findParent(parent) : {};
            condition(parent) ? loop() : exit();
        };
        const exit = () => null;
        loop();
		logger.debug(`<<< Exiting _mapToParentTabs(parent=${parent},fn=${fn})`);
        return;
    }

    _mapToChildTabs(tab, fn) {
		logger.debug(`>>> Entering _mapToChildTabs(tab=${tab},fn=${fn})`);
        let ChildTabs = tab.ChildTabs;
        const cachedChildTabs = [];
        cachedChildTabs.push(ChildTabs);
        const condition = cachedChildTabs.length > 0;

        const loop = () => {
            const childTab = cachedChildTabs.length ? cachedChildTabs.shift() : null;
            const left = () => childTab.forEach(tab => {
                Array.isArray(tab.ChildTabs) ? fn(tab) : null;
                Array.isArray(tab.ChildTabs) && tab.ChildTabs.length ? cachedChildTabs.push(tab.ChildTabs) : null;
                condition ? loop() : exit();
            });
            const right = () => null;
            childTab ? left() : right();
        };
        const exit = () => null;

        loop();
		logger.debug(`<<< Exiting _mapToChildTabs(tab=${tab},fn=${fn})`);
        return;
    }

    setCheckedState(tab) {
		logger.debug(`>>> Entering setCheckedState(tab=${tab})`);
        const set = () => tab.HasChildren ? this.selectParent(tab) : this.selectIndividual(tab);
        tab.CheckedState ? this.resetCheckedState(tab) : set();
		logger.debug(`<<< Exiting setCheckedState(tab=${tab})`);
    }

    mapParentCheckedState(parent) {
        const checkedArray = [];
        parent.ChildTabs.map(tab => tab.CheckedState !== this.props.unchecked ? checkedArray.push(true) : checkedArray.push(false));
        checkedArray.indexOf(false) !== -1 ? parent.CheckedState = this.props.individuallyChecked : parent.CheckedState = this.props.fullyChecked;
        this.props.updateTree(parent);
		logger.debug(`<<< Exiting mapParentCheckedState(parent=${parent})`);
    }

    resetCheckedState(tab) {
		logger.debug(`>>> Entering resetCheckedState(tab=${tab})`);
        const unselectChildren = (childTab) => {
            childTab.CheckedState = this.props.unchecked;
            childTab.ChildrenSelected = false;
            this.props.updateTree(childTab);
			logger.debug(`<<< Exiting unselectChildren(childTab=${childTab})`);
        };

        const unselectIndividual = () => {
			logger.debug('>>> Entering unselectIndividual()');
            tab.CheckedState = this.props.unchecked;
            tab.ChildrenSelected = false;
			logger.debug(`unselectIndividual(): tab.CheckedState → ${tab.CheckedState}`);
            this.props.updateTree(tab);
			logger.debug('<<< Exiting unselectIndividual()');
        };

        tab.HasChildren ? this._mapToChildTabs(tab, unselectChildren) : unselectIndividual();
        tab.CheckedState = this.props.unchecked;
		logger.debug(`resetCheckedState(tab=${tab}): tab.CheckedState → ${tab.CheckedState}`);
        tab.ChildrenSelected = false;
        this.props.updateTree(tab);
		logger.debug(`resetCheckedState(tab=${tab}): tab.ChildrenSelected → ${tab.ChildrenSelected}`);

        const parent = this.props.findParent(tab);
		logger.debug(`resetCheckedState(tab=${tab}): parent → ${parent}`);
        this.setParentCheckedState(parent);
        this.props.reAlignTree();
		logger.debug(`<<< Exiting resetCheckedState(tab=${tab})`);
    }

    setParentCheckedState(parent) {
        const ChildTabs = parent.ChildTabs || [];
        const length = ChildTabs.length;
        const checkedArray = [];

        const checkParent = () => {
			logger.debug('>>> Entering checkParent()');
            parent.ChildrenSelected = true;
            switch (true) {
                case checkedArray.filter(bool => !!bool).length === length:
                    parent.CheckedState = parent.CheckedState === this.props.individuallyChecked ? this.props.fullyChecked : parent.CheckedState;
                    this.props.updateTree(parent);
                    return;

                case checkedArray.indexOf(true) !== -1:
                    parent.CheckedState = parent.CheckedState === this.props.fullyChecked ? this.props.individuallyChecked : parent.CheckedState;
                    this.props.updateTree(parent);
                    return;
            }

            this.props.updateTree(parent);
			logger.debug('<<< Exiting checkParent()');
        };

        const noChildrenSelected = () => {
			logger.debug('>>> Entering noChildrenSelected()');
            parent.ChildrenSelected = false;
            this.props.updateTree(parent);
			logger.debug('<<< Exiting noChildrenSelected()');
        };

        ChildTabs.forEach(tab => tab.CheckedState ? checkedArray.push(true) : checkedArray.push(false));
        checkedArray.indexOf(true) !== -1 ? checkParent() : noChildrenSelected();
		logger.debug(`<<< Exiting setParentCheckedState(parent=${parent})`);
    }

    selectParent(tab) {
		logger.debug(`>>> Entering selectParent(tab=${tab})`);
        const select = (tab) => {
			logger.debug(`>>> Entering select(tab=${tab})`);
            switch (true) {
                case tab.HasChildren === true:
                    tab.CheckedState = this.props.fullyChecked;
                    tab.ChildrenSelected = true;
                    this.props.updateTree(tab);
                    return;
                case tab.HasChildren === false:
                    tab.CheckedState = this.props.individuallyChecked;
                    tab.ChildrenSelected = false;
                    this.props.updateTree(tab);
					logger.debug(`<<< Exiting select(tab=${tab})`);
                    return;
            }
        };
        tab.CheckedState = this.props.fullyChecked;
		logger.debug(`selectParent(tab=${tab}): tab.CheckedState → ${tab.CheckedState}`);
        tab.ChildrenSelected = true;
        this._mapToChildTabs(tab, select);
		logger.debug(`selectParent(tab=${tab}): tab.ChildrenSelected → ${tab.ChildrenSelected}`);
        this.props.updateTree(tab);
        const parent = this.props.findParent(tab);
		logger.debug(`selectParent(tab=${tab}): parent → ${parent}`);
        this.setParentCheckedState(parent);
        this.props.reAlignTree();
		logger.debug(`<<< Exiting selectParent(tab=${tab})`);
    }

    selectIndividual(tab) {
		logger.debug(`>>> Entering selectIndividual(tab=${tab})`);
        tab.CheckedState = this.props.individuallyChecked;
        tab.ChildrenSelected = false;
		logger.debug(`selectIndividual(tab=${tab}): tab.CheckedState → ${tab.CheckedState}`);
		logger.debug(`selectIndividual(tab=${tab}): tab.ChildrenSelected → ${tab.ChildrenSelected}`);
        this.props.updateTree(tab);
        const parent = this.props.findParent(tab);
		logger.debug(`selectIndividual(tab=${tab}): parent → ${parent}`);
        this.setParentCheckedState(parent);
        this.props.reAlignTree();
		logger.debug(`<<< Exiting selectIndividual(tab=${tab})`);
    }


    expandParent(tab) {
		logger.debug(`>>> Entering expandParent(tab=${tab})`);
        const condition = tab.HasChildren && tab.ChildTabs.length > 0;
        const left = () => tab.IsOpen = !tab.IsOpen;

        const right = () => {
			logger.debug('>>> Entering right()');
            this.props.getDescendantPortalTabs(tab.TabId, () => {
                tab.IsOpen = !tab.IsOpen;
                this.props.updateTree(tab);
			logger.debug('<<< Exiting right()');
            });
        };
        condition ? left() : right();
		logger.debug(`expandParent(tab=${tab}): tab.IsOpen → ${tab.IsOpen}`);
        this.props.updateTree(tab);
		logger.debug(`<<< Exiting expandParent(tab=${tab})`);
    }


    render_icon(direction) {
        const animate = direction === "90deg" ? true : false;
		logger.debug(`<<< Exiting render_icon(direction=${direction})`);
        return <this.icon animate={animate} shouldAnimate={true} reset={false} direction={direction} />;
    }

    render_Bullet(tab) {
		logger.debug(`>>> Entering render_Bullet(tab=${tab})`);
        const conditional = tab.HasChildren;
        const direction = tab.IsOpen && tab.ChildTabs.length ? "90deg" : "0deg";
		logger.debug(`render_Bullet(tab=${tab}): direction → ${direction}`);
        const render = (conditional ? this.render_icon(direction) : () => null);
		logger.debug(`<<< Exiting render_Bullet(tab=${tab})`);
        return render;
    }

    render_ListBullet(tab, fn) {
		logger.debug(`>>> Entering render_ListBullet(tab=${tab},fn=${fn})`);
        const bullet = (() => {
            return (
                <div
                    className="arrow-bullet"
                    onClick={() => fn()}>
                    {this.render_Bullet.call(this, tab)}
                </div>);
        })();
		logger.debug(`render_ListBullet(tab=${tab},fn=${fn}): bullet → ${bullet}`);
		logger.debug(`<<< Exiting render_ListBullet(tab=${tab},fn=${fn})`);
        return bullet;
    }

    render_ListCheckbox(tab) {
		logger.debug(`>>> Entering render_ListCheckbox(tab=${tab})`);
        const checkbox = (() => {
            const position = { position: "absolute" };
			logger.debug(`render_ListCheckbox(tab=${tab}): position → ${position}`);
            const anyChildrenSelected = (tab) => {
				logger.debug(`>>> Entering anyChildrenSelected(tab=${tab})`);
                const ChildTabs = tab.ChildTabs;
                const left = () => {
                    const trueCheckedState = [];
                    const AreChildrenChecked = (t) => {
						logger.debug(`>>> Entering AreChildrenChecked(t=${t})`);
                        const condition = t.CheckedState !== this.props.unchecked;
                        condition ? trueCheckedState.push(true) : trueCheckedState.push(false);
						logger.debug(`<<< Exiting AreChildrenChecked(t=${t})`);
                    };
                    this._mapToChildTabs(tab, AreChildrenChecked);
                    const bool = trueCheckedState.indexOf(true) !== -1 ? true : false;
                    return bool;
                };
                const right = () => null;
				logger.debug(`<<< Exiting anyChildrenSelected(tab=${tab})`);
                return ChildTabs.length ? left() : right();
            };

            const renderCheckBoxClassName = () => {
				logger.debug('>>> Entering renderCheckBoxClassName()');
                switch (true) {
                    case !!tab.HasChildren === true && !!tab.CheckedState === true:
                        return "treecontrol-label-parent";

                    case !!tab.HasChildren === true && !!tab.CheckedState === false && anyChildrenSelected(tab):
                        return "treecontrol-label-parent-unselected";

                    default:
                        return "treecontrol-label-normal";
                }
            };

            return (
                <div style={merge(floatLeft, position)}>
                    <input
                        style={{ border: "3px solid black" }}
                        type="checkbox"
                        onChange={() => this.setCheckedState.call(this, tab)}
                        checked={tab.CheckedState} />
                    <label
                        className={renderCheckBoxClassName()}
                        onClick={() => this.setCheckedState.call(this, tab)} >
                    </label>
                </div>);
        })();
		logger.debug(`render_ListCheckbox(tab=${tab}): checkbox → ${checkbox}`);
		logger.debug(`<<< Exiting render_ListCheckbox(tab=${tab})`);
        return checkbox;
    }

    render_tabName(tab) {
		logger.debug(`>>> Entering render_tabName(tab=${tab})`);
        const charLimit = 15;
        const name = tab.Name.length > charLimit ? `${tab.Name.substr(0, charLimit)}...` : tab.Name;
        const render = (() => {
            const margin = styles.margin({ top: 0 });
			logger.debug(`render_tabName(tab=${tab}): margin → ${margin}`);
            const padding = styles.padding({ left: 20 });
			logger.debug(`render_tabName(tab=${tab}): padding → ${padding}`);
            const nowrap = { whiteSpace: "nowrap" };
			logger.debug(`render_tabName(tab=${tab}): nowrap → ${nowrap}`);
            const bold = { fontWeight: 400 };
            const color = () => {
				logger.debug('>>> Entering color()');
                return tab.CheckedState ? { color: this.props.selectedColor } : {};
            };
            const clr = color();
			logger.debug(`render_tabName(tab=${tab}): clr → ${clr}`);

            const renderName = () => {
				logger.debug('>>> Entering renderName()');
                switch (true) {
                    case name.length > charLimit:
                        return (
                            <span>
                                { name }
                                <TextOverflowWrapperNew
                                    text={ tab.Name }
                                    border={ true}
                                    className="page-picker-tooltip-styles" />
                            </span>
                        );
                    default:
                        return (
                            <span>
                                {name}
                                <div></div>
                            </span>
                        );

                }
            };

            return (
                <span style={merge(margin, padding, nowrap, clr, bold)}>
                    {renderName()}
                </span>
            );
		logger.debug(`<<< Exiting render_tabName(tab=${tab})`);
        })();
        return render;
    }

    render_li(tabs) {
		logger.debug(`>>> Entering render_li(tabs=${tabs})`);
        const render = (() => {
            return tabs.map(tab => {
                const tabName = this.render_tabName(tab);
                const checkbox = this.render_ListCheckbox(tab);
                const bullet = this.render_ListBullet.call(this, tab, this.expandParent.bind(this, tab));
                const tree = this.render_tree(tab.ChildTabs);
                const anyChildrenSelected = (tab) => {
                    const ChildTabs = tab.ChildTabs;
                    const left = () => {
                        const trueCheckedState = [];
                        const AreChildrenChecked = (t) => {
                            const condition = t.CheckedState !== this.props.unchecked;
                            condition ? trueCheckedState.push(true) : trueCheckedState.push(false);
                        };
                        this._mapToChildTabs(tab, AreChildrenChecked);
                        const bool = trueCheckedState.indexOf(true) !== -1 ? true : false;
						logger.debug(`left(): bool → ${bool}`);
                        return bool;
                    };
                    const right = () => null;
                    return ChildTabs.length ? left() : right();
                };

                const li = () => (
                    <li
                        key={tab.Name}>
                        {tab.HasChildren ? bullet : null}
                        {checkbox}
                        {tabName}
                        {tree}
                    </li>);
                const parent = this.props.findParent(tab);
                const show = parent.IsOpen || parseInt(tab.TabId) === -1 ? li() : null;

                return (
                    show
                );
            });
        })();
		logger.debug('<<< Exiting render()');
        return render;
    }

    render_tree(ChildTabs) {
        const render = (() => {
            return (
                <TreeControl
                    tabs={ChildTabs}
                    icon_type="arrow_bullet"
                    updateTree={this.props.updateTree.bind(this)}
                    reAlignTree={this.props.reAlignTree.bind(this)}
                    findParent={this.props.findParent.bind(this)}
                    fullyChecked={this.props.fullyChecked}
                    individuallyChecked={this.props.individuallyChecked}
                    unchecked={this.props.unchecked}
                    selectedColor={this.props.selectedColor}
                    characterLimit={this.props.characterLimit}
                    export={this.props.export}
                    PortalTabsParameters={this.props.PortalTabsParameters}
                    getDescendantPortalTabs={this.props.getDescendantPortalTabs}
                />
            );
        })();
        return render;
    }


    render() {
        const listStyle = styles.listStyle();
        const list_items = this.render_li(this.props.tabs);

        return (
            <ul className="page-picker" style={merge(listStyle)} >
                {list_items}
            </ul>
        );
    }
}

TreeControl.propTypes = {
    selectedColor: PropTypes.string.isRequired,
    characterLimit: PropTypes.number.isRequired,
    tabs: PropTypes.object.isRequired,
    icon_type: PropTypes.object.isRequired,
    export: PropTypes.func.isRequired,
    getDescendantPortalTabs: PropTypes.func.isRequired,
    PortalTabsParameters: PropTypes.object.isRequired,
    updateTree: PropTypes.func.isRequired,
    fullyChecked: PropTypes.number.isRequired,
    individuallyChecked: PropTypes.number.isRequired,
    unchecked: PropTypes.number.isRequired,
    reAlignTree: PropTypes.func.isRequired,
    findParent: PropTypes.func.isRequired
};
