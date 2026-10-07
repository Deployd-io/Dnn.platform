import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible as Collapse, TextOverflowWrapper, GridCell, Button } from "@dnnsoftware/dnn-react-common";
import styles from "./style.module.less";
import util from "utils";
import LocalizedResources from "resources";
import SimpleTypeIcon from "./svg/vocabulary_simple.svg";
import HierarchyTypeIcon from "./svg/vocabulary_hierarchy.svg";
const logger = console;


class TermHeader extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            collapsed: true,
            repainting: false
        };
        this.timeout = 0;
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
        this.handleClick = this.handleClick.bind(this);
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;
        if (!props.closeOnClick) {
            return;
        }
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentWillUnmount" });
    }
    handleClick(event) {
		logger.error(`>>> Entering handleClick(event=${event})`, { "event.action": "handleClick", "logrite.event": event });
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }
        if (!this.node.contains(event.target) &&
            (typeof event.target.className === "string" && event.target.className.indexOf("false") > -1)) {

            if (event.target.className.indexOf("delete-button") > -1) {
                return;
            }
            this.timeout = 475;
            this.collapse();
        } else {

            this.timeout = 0;
        }
		logger.warn(`<<< Exiting handleClick(event=${event})`, { "event.action": "handleClick", "logrite.event": event });
    }
    uncollapse() {
		logger.error('>>> Entering uncollapse()', { "event.action": "uncollapse" });
        this.setState({
            collapsed: false
        });
		logger.warn('<<< Exiting uncollapse()', { "event.action": "uncollapse" });
    }
    collapse() {
		logger.error('>>> Entering collapse()', { "event.action": "collapse" });
        this.setState({
            collapsed: true
        });
		logger.warn('<<< Exiting collapse()', { "event.action": "collapse" });
    }
    toggle(event) {
		logger.error(`>>> Entering toggle(event=${event})`, { "event.action": "toggle", "logrite.event": event });
        if (this.state.collapsed) {
            this.uncollapse();
        } else {
            if (event.target.parentNode.className.indexOf("delete-button") > -1) {
                return;
            }
            this.collapse();
        }
		logger.warn(`<<< Exiting toggle(event=${event})`, { "event.action": "toggle", "logrite.event": event });
    }
    onDelete() {
		logger.error('>>> Entering onDelete()', { "event.action": "onDelete" });
        const {props} = this;
        props.onDelete(props.term, props.index, () => { this.collapse(); });
		logger.warn('<<< Exiting onDelete()', { "event.action": "onDelete" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
        const TypeIcon = props.type === 1 ? SimpleTypeIcon : HierarchyTypeIcon;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div ref={node => this.node = node} className={"" + styles.collapsibleComponent + " " + state.collapsed + (props.className ? (" " + props.className) : "")}>
                <div className={"collapsible-header " + state.collapsed} onClick={this.toggle.bind(this)}>
                    <div className="term-header">
                        <div className="term-icon"><TypeIcon /></div>
                        <div className="term-label">

                            <TextOverflowWrapper text={props.header} maxWidth={200} />
                        </div>
                    </div>
                    {!props.disabled &&
                        <span
                            className={"collapse-icon " + (this.state.collapsed ? "collapsed" : "")}>
                        </span>
                    }
                </div>
                <Collapse
                    className={"term-header-collapsible"}
                    isOpened={!this.state.collapsed}>
                    {!state.collapsed && props.children}
                    {!state.collapsed && <GridCell className="vocab-footer">
                        {!props.term.IsSystem && util.canEdit() && <Button type="danger" onClick={this.onDelete.bind(this)}>{LocalizedResources.get("DeleteTerm")}</Button>}
                        <Button type="secondary" onClick={this.collapse.bind(this)}>{LocalizedResources.get("Close")}</Button>
                    </GridCell>}
                </Collapse>
            </div>
        );
    }
}

TermHeader.propTypes = {
    header: PropTypes.string,
    type: PropTypes.string,
    label: PropTypes.node,
    children: PropTypes.node,
    disabled: PropTypes.bool,
    className: PropTypes.string,
    closeOnClick: PropTypes.bool,
    term: PropTypes.object,
    onDelete: PropTypes.func,
    index: PropTypes.number
};
export default TermHeader;