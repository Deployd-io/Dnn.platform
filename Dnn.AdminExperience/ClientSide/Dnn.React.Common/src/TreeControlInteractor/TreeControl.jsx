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
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        const icon_type = props.icon_type;
        this.icon = IconSelector(icon_type);
        this.export = props.export;
        this.getDescendantPortalTabs = props.getDescendantPortalTabs;
		logger.debug(`constructor(props=${props}): this.export → ${this.export}`, { "event.action": "constructor", "logrite.props": props });
        this.textOverflowRefs = [];
		logger.debug(`constructor(props=${props}): this.textOverflowRefs → ${this.textOverflowRefs}`, { "event.action": "constructor", "logrite.props": props });
    }

    _traverse(comparator) {
		logger.error(`>>> Entering _traverse(comparator=${comparator})`, { "event.action": "_traverse", "logrite.comparator": comparator });
        let ChildTabs = this.props.tabs;
        const cachedChildTabs = [];
        cachedChildTabs.push(ChildTabs);
        const condition = cachedChildTabs.length > 0;

        const loop = () => {
			logger.error('>>> Entering loop()', { "event.action": "loop" });
            const childTab = cachedChildTabs.length ? cachedChildTabs.shift() : null;
			logger.error(`loop(): childTab → ${childTab}`, { "event.action": "loop" });
            const left = () => childTab.forEach(tab => {
				logger.error('>>> Entering left()', { "event.action": "left" });
                Array.isArray(tab.ChildTabs) ? comparator(tab, this.props.tabs) : null;
                Array.isArray(tab.ChildTabs) && tab.ChildTabs.length ? cachedChildTabs.push(tab.ChildTabs) : null;
                condition ? loop() : exit();
            });
            const right = () => null;
			logger.warn('<<< Exiting loop()', { "event.action": "loop" });
            childTab ? left() : right();
        };

        const exit = () => null;

        loop();
		logger.warn(`<<< Exiting _traverse(comparator=${comparator})`, { "event.action": "_traverse", "logrite.comparator": comparator });
        return;
    }

    _mapToParentTabs(parent, fn) {
		logger.error(`>>> Entering _mapToParentTabs(parent=${parent},fn=${fn})`, { "event.action": "_mapToParentTabs", "logrite.parent": parent, "logrite.fn": fn });
        const condition = (par) => Object.keys(par).length > 0;
        parent = this.props.findParent(parent);
        const loop = () => {
			logger.error(`_mapToParentTabs(parent=${parent},fn=${fn}): parent → ${parent}`, { "event.action": "_mapToParentTabs", "logrite.parent": parent, "logrite.fn": fn });
            parent ? fn(parent) : null;
            parent = parseInt(parent.TabId) !== -1 ? this.props.findParent(parent) : {};
            condition(parent) ? loop() : exit();
        };
        const exit = () => null;
        loop();
		logger.warn(`<<< Exiting _mapToParentTabs(parent=${parent},fn=${fn})`, { "event.action": "_mapToParentTabs", "logrite.parent": parent, "logrite.fn": fn });
        return;
    }

    _mapToChildTabs(tab, fn) {
		logger.error(`>>> Entering _mapToChildTabs(tab=${tab},fn=${fn})`, { "event.action": "_mapToChildTabs", "logrite.tab": tab, "logrite.fn": fn });
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
		logger.warn(`<<< Exiting _mapToChildTabs(tab=${tab},fn=${fn})`, { "event.action": "_mapToChildTabs", "logrite.tab": tab, "logrite.fn": fn });
        return;
    }

    setCheckedState(tab) {
		logger.error(`>>> Entering setCheckedState(tab=${tab})`, { "event.action": "setCheckedState", "logrite.tab": tab });
        const set = () => tab.HasChildren ? this.selectParent(tab) : this.selectIndividual(tab);
        tab.CheckedState ? this.resetCheckedState(tab) : set();
		logger.warn(`<<< Exiting setCheckedState(tab=${tab})`, { "event.action": "setCheckedState", "logrite.tab": tab });
    }

    mapParentCheckedState(parent) {
		logger.error(`>>> Entering mapParentCheckedState(parent=${parent})`, { "event.action": "mapParentCheckedState", "logrite.parent": parent });
        const checkedArray = [];
        parent.ChildTabs.map(tab => tab.CheckedState !== this.props.unchecked ? checkedArray.push(true) : checkedArray.push(false));
        checkedArray.indexOf(false) !== -1 ? parent.CheckedState = this.props.individuallyChecked : parent.CheckedState = this.props.fullyChecked;
        this.props.updateTree(parent);
		logger.warn(`<<< Exiting mapParentCheckedState(parent=${parent})`, { "event.action": "mapParentCheckedState", "logrite.parent": parent });
    }

    resetCheckedState(tab) {
		logger.error(`>>> Entering resetCheckedState(tab=${tab})`, { "event.action": "resetCheckedState", "logrite.tab": tab });
        const unselectChildren = (childTab) => {
            childTab.CheckedState = this.props.unchecked;
            childTab.ChildrenSelected = false;
            this.props.updateTree(childTab);
			logger.warn(`<<< Exiting unselectChildren(childTab=${childTab})`, { "event.action": "unselectChildren", "logrite.childTab": childTab });
        };

        const unselectIndividual = () => {
			logger.error('>>> Entering unselectIndividual()', { "event.action": "unselectIndividual" });
            tab.CheckedState = this.props.unchecked;
            tab.ChildrenSelected = false;
            this.props.updateTree(tab);
			logger.warn('<<< Exiting unselectIndividual()', { "event.action": "unselectIndividual" });
        };

        tab.HasChildren ? this._mapToChildTabs(tab, unselectChildren) : unselectIndividual();
        tab.CheckedState = this.props.unchecked;
		logger.debug(`resetCheckedState(tab=${tab}): tab.CheckedState → ${tab.CheckedState}`, { "event.action": "resetCheckedState", "logrite.tab": tab });
        tab.ChildrenSelected = false;
        this.props.updateTree(tab);

		logger.debug(`resetCheckedState(tab=${tab}): tab.ChildrenSelected → ${tab.ChildrenSelected}`, { "event.action": "resetCheckedState", "logrite.tab": tab });
        const parent = this.props.findParent(tab);
		logger.error(`resetCheckedState(tab=${tab}): parent → ${parent}`, { "event.action": "resetCheckedState", "logrite.tab": tab });
        this.setParentCheckedState(parent);
        this.props.reAlignTree();
		logger.warn(`<<< Exiting resetCheckedState(tab=${tab})`, { "event.action": "resetCheckedState", "logrite.tab": tab });
    }

    setParentCheckedState(parent) {
		logger.error(`>>> Entering setParentCheckedState(parent=${parent})`, { "event.action": "setParentCheckedState", "logrite.parent": parent });
        const ChildTabs = parent.ChildTabs || [];
        const length = ChildTabs.length;
        const checkedArray = [];

		logger.debug(`setParentCheckedState(parent=${parent}): parent.ChildrenSelected → ${parent.ChildrenSelected}`, { "event.action": "setParentCheckedState", "logrite.parent": parent });
        const checkParent = () => {
			logger.error('>>> Entering checkParent()', { "event.action": "checkParent" });
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
			logger.warn('<<< Exiting checkParent()', { "event.action": "checkParent" });
        };

        const noChildrenSelected = () => {
			logger.error('>>> Entering noChildrenSelected()', { "event.action": "noChildrenSelected" });
            parent.ChildrenSelected = false;
            this.props.updateTree(parent);
			logger.warn('<<< Exiting noChildrenSelected()', { "event.action": "noChildrenSelected" });
        };

        ChildTabs.forEach(tab => tab.CheckedState ? checkedArray.push(true) : checkedArray.push(false));
        checkedArray.indexOf(true) !== -1 ? checkParent() : noChildrenSelected();
		logger.warn(`<<< Exiting setParentCheckedState(parent=${parent})`, { "event.action": "setParentCheckedState", "logrite.parent": parent });
    }

    selectParent(tab) {
		logger.error(`>>> Entering selectParent(tab=${tab})`, { "event.action": "selectParent", "logrite.tab": tab });
		logger.debug(`selectParent(tab=${tab}): tab.CheckedState → ${tab.CheckedState}`, { "event.action": "selectParent", "logrite.tab": tab });
        const select = (tab) => {
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
					logger.warn(`<<< Exiting select(tab=${tab})`, { "event.action": "select", "logrite.tab": tab });
                    return;
            }
        };
        tab.CheckedState = this.props.fullyChecked;
        tab.ChildrenSelected = true;
        this._mapToChildTabs(tab, select);
        this.props.updateTree(tab);
        const parent = this.props.findParent(tab);
		logger.error(`selectParent(tab=${tab}): parent → ${parent}`, { "event.action": "selectParent", "logrite.tab": tab });
        this.setParentCheckedState(parent);
        this.props.reAlignTree();
		logger.warn(`<<< Exiting selectParent(tab=${tab})`, { "event.action": "selectParent", "logrite.tab": tab });
    }

    selectIndividual(tab) {
		logger.error(`>>> Entering selectIndividual(tab=${tab})`, { "event.action": "selectIndividual", "logrite.tab": tab });
        tab.CheckedState = this.props.individuallyChecked;
        tab.ChildrenSelected = false;
		logger.debug(`selectIndividual(tab=${tab}): tab.CheckedState → ${tab.CheckedState}`, { "event.action": "selectIndividual", "logrite.tab": tab });
		logger.debug(`selectIndividual(tab=${tab}): tab.ChildrenSelected → ${tab.ChildrenSelected}`, { "event.action": "selectIndividual", "logrite.tab": tab });
        this.props.updateTree(tab);
        const parent = this.props.findParent(tab);
		logger.error(`selectIndividual(tab=${tab}): parent → ${parent}`, { "event.action": "selectIndividual", "logrite.tab": tab });
        this.setParentCheckedState(parent);
        this.props.reAlignTree();
		logger.warn(`<<< Exiting selectIndividual(tab=${tab})`, { "event.action": "selectIndividual", "logrite.tab": tab });
    }


    expandParent(tab) {
		logger.error(`>>> Entering expandParent(tab=${tab})`, { "event.action": "expandParent", "logrite.tab": tab });
        const condition = tab.HasChildren && tab.ChildTabs.length > 0;
        const left = () => tab.IsOpen = !tab.IsOpen;

		logger.debug(`expandParent(tab=${tab}): tab.IsOpen → ${tab.IsOpen}`, { "event.action": "expandParent", "logrite.tab": tab });
        const right = () => {
			logger.error('>>> Entering right()', { "event.action": "right" });
            this.props.getDescendantPortalTabs(tab.TabId, () => {
                tab.IsOpen = !tab.IsOpen;
                this.props.updateTree(tab);
			logger.warn('<<< Exiting right()', { "event.action": "right" });
            });
        };
        condition ? left() : right();
        this.props.updateTree(tab);
		logger.warn(`<<< Exiting expandParent(tab=${tab})`, { "event.action": "expandParent", "logrite.tab": tab });
    }


    render_icon(direction) {
		logger.error(`>>> Entering render_icon(direction=${direction})`, { "event.action": "render_icon", "logrite.direction": direction });
        const animate = direction === "90deg" ? true : false;
		logger.warn(`<<< Exiting render_icon(direction=${direction})`, { "event.action": "render_icon", "logrite.direction": direction });
        return <this.icon animate={animate} shouldAnimate={true} reset={false} direction={direction} />;
    }

    render_Bullet(tab) {
		logger.error(`>>> Entering render_Bullet(tab=${tab})`, { "event.action": "render_Bullet", "logrite.tab": tab });
        const conditional = tab.HasChildren;
        const direction = tab.IsOpen && tab.ChildTabs.length ? "90deg" : "0deg";
		logger.error(`render_Bullet(tab=${tab}): direction → ${direction}`, { "event.action": "render_Bullet", "logrite.tab": tab });
        const render = (conditional ? this.render_icon(direction) : () => null);
		logger.warn(`<<< Exiting render_Bullet(tab=${tab})`, { "event.action": "render_Bullet", "logrite.tab": tab });
        return render;
    }

    render_ListBullet(tab, fn) {
		logger.error(`>>> Entering render_ListBullet(tab=${tab},fn=${fn})`, { "event.action": "render_ListBullet", "logrite.tab": tab, "logrite.fn": fn });
        const bullet = (() => {
            return (
                <div
                    className="arrow-bullet"
                    onClick={() => fn()}>
                    {this.render_Bullet.call(this, tab)}
                </div>);
        })();
		logger.error(`render_ListBullet(tab=${tab},fn=${fn}): bullet → ${bullet}`, { "event.action": "render_ListBullet", "logrite.tab": tab, "logrite.fn": fn });
        return bullet;
    }

    render_ListCheckbox(tab) {
		logger.error(`>>> Entering render_ListCheckbox(tab=${tab})`, { "event.action": "render_ListCheckbox", "logrite.tab": tab });
        const checkbox = (() => {
            const position = { position: "absolute" };
			logger.error(`render_ListCheckbox(tab=${tab}): position → ${position}`, { "event.action": "render_ListCheckbox", "logrite.tab": tab });
            const anyChildrenSelected = (tab) => {
				logger.error(`>>> Entering anyChildrenSelected(tab=${tab})`, { "event.action": "anyChildrenSelected", "logrite.tab": tab });
                const ChildTabs = tab.ChildTabs;
                const left = () => {
                    const trueCheckedState = [];
                    const AreChildrenChecked = (t) => {
						logger.error(`>>> Entering AreChildrenChecked(t=${t})`, { "event.action": "AreChildrenChecked", "logrite.t": t });
                        const condition = t.CheckedState !== this.props.unchecked;
                        condition ? trueCheckedState.push(true) : trueCheckedState.push(false);
						logger.warn(`<<< Exiting AreChildrenChecked(t=${t})`, { "event.action": "AreChildrenChecked", "logrite.t": t });
                    };
                    this._mapToChildTabs(tab, AreChildrenChecked);
                    const bool = trueCheckedState.indexOf(true) !== -1 ? true : false;
					logger.error(`left(): bool → ${bool}`, { "event.action": "left" });
                    return bool;
                };
                const right = () => null;
				logger.warn(`<<< Exiting anyChildrenSelected(tab=${tab})`, { "event.action": "anyChildrenSelected", "logrite.tab": tab });
                return ChildTabs.length ? left() : right();
            };

            const renderCheckBoxClassName = () => {
				logger.error('>>> Entering renderCheckBoxClassName()', { "event.action": "renderCheckBoxClassName" });
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
		logger.error(`render_ListCheckbox(tab=${tab}): checkbox → ${checkbox}`, { "event.action": "render_ListCheckbox", "logrite.tab": tab });
        return checkbox;
    }

    render_tabName(tab) {
		logger.error(`>>> Entering render_tabName(tab=${tab})`, { "event.action": "render_tabName", "logrite.tab": tab });
        const charLimit = 15;
        const name = tab.Name.length > charLimit ? `${tab.Name.substr(0, charLimit)}...` : tab.Name;
        const render = (() => {
            const margin = styles.margin({ top: 0 });
			logger.error(`render_tabName(tab=${tab}): margin → ${margin}`, { "event.action": "render_tabName", "logrite.tab": tab });
            const padding = styles.padding({ left: 20 });
			logger.error(`render_tabName(tab=${tab}): padding → ${padding}`, { "event.action": "render_tabName", "logrite.tab": tab });
            const nowrap = { whiteSpace: "nowrap" };
			logger.error(`render_tabName(tab=${tab}): nowrap → ${nowrap}`, { "event.action": "render_tabName", "logrite.tab": tab });
            const bold = { fontWeight: 400 };
            const color = () => {
				logger.error('>>> Entering color()', { "event.action": "color" });
                return tab.CheckedState ? { color: this.props.selectedColor } : {};
            };
            const clr = color();
			logger.error(`render_tabName(tab=${tab}): clr → ${clr}`, { "event.action": "render_tabName", "logrite.tab": tab });

            const renderName = () => {
				logger.error('>>> Entering renderName()', { "event.action": "renderName" });
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
        })();
        return render;
    }

    render_li(tabs) {
		logger.error(`>>> Entering render_li(tabs=${tabs})`, { "event.action": "render_li", "logrite.tabs": tabs });
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
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return render;
    }

    render_tree(ChildTabs) {
		logger.error(`>>> Entering render_tree(ChildTabs=${ChildTabs})`, { "event.action": "render_tree", "logrite.ChildTabs": ChildTabs });
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
