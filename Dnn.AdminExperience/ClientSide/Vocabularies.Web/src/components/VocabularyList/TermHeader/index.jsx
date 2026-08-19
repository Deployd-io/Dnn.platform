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
		logger.debug('>>> Entering constructor()');
        this.state = {
            collapsed: true,
            repainting: false
        };
        this.timeout = 0;
		logger.debug(`constructor(): this.state → ${this.state}`);
        this.handleClick = this.handleClick.bind(this);
		logger.debug('<<< Exiting constructor()');
    }
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        const {props} = this;
        if (!props.closeOnClick) {
            return;
        }
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentWillUnmount()');
    }
    handleClick(event) {
		logger.debug(`>>> Entering handleClick(event=${event})`);
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
		logger.debug(`<<< Exiting handleClick(event=${event})`);
    }
    uncollapse() {
		logger.debug('>>> Entering uncollapse()');
        this.setState({
            collapsed: false
        });
		logger.debug('<<< Exiting uncollapse()');
    }
    collapse() {
		logger.debug('>>> Entering collapse()');
        this.setState({
            collapsed: true
        });
		logger.debug('<<< Exiting collapse()');
    }
    toggle(event) {
		logger.debug(`>>> Entering toggle(event=${event})`);
        if (this.state.collapsed) {
            this.uncollapse();
        } else {
            if (event.target.parentNode.className.indexOf("delete-button") > -1) {
                return;
            }
            this.collapse();
        }
		logger.debug(`<<< Exiting toggle(event=${event})`);
    }
    onDelete() {
		logger.debug('>>> Entering onDelete()');
        const {props} = this;
        props.onDelete(props.term, props.index, () => { this.collapse(); });
		logger.debug('<<< Exiting onDelete()');
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
        const TypeIcon = props.type === 1 ? SimpleTypeIcon : HierarchyTypeIcon;
		logger.debug('<<< Exiting render()');
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