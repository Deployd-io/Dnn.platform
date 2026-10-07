import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "react-collapse";
import Button from "../Button";
import "./style.less";
const logger = console;

export default class CollapsibleRow extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = { collapsed: props.collapsed ? props.collapsed : true };
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
        this.handleClick = this.handleClick.bind(this);
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;
        if (props.closeOnBlur) {
            document.addEventListener("click", this.handleClick);
            this._isMounted = true;
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        if (this.props.collapsed !== prevProps.collapsed) {
            this.setState({ collapsed: this.props.collapsed });
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }
    
    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentWillUnmount" });
    }

    handleClick(event) {
		logger.error(`>>> Entering handleClick(event=${event})`, { "event.action": "handleClick", "logrite.event": event });
        const {props} = this;
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted || !props.closeOnBlur) { return; }

        if (!this.node.contains(event.target)) {
            this.collapse();
			logger.warn(`<<< Exiting handleClick(event=${event})`, { "event.action": "handleClick", "logrite.event": event });
        }
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
    onCancel() {
		logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
        this.collapse();
        if (this.props.onChange) {
            this.props.onChange(true);
			logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
        }
    }
    toggle() {
		logger.error('>>> Entering toggle()', { "event.action": "toggle" });
        if (this.props.onChange) {
            this.props.onChange(this.state.collapsed);
        }
        if (this.state.collapsed) {
            this.uncollapse();
        } else {
            this.collapse();
			logger.warn('<<< Exiting toggle()', { "event.action": "toggle" });
        }
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={"dnn-collapsible-component" + (props.className ? (" " + props.className) : "") }>
                <div className="collapsible-header" onClick={this.toggle.bind(this) }>{props.label}
                    {!props.disabled &&
                        <span
                            className={"collapse-icon " + (this.state.collapsed ? "collapsed" : "") }>
                        </span>
                    }
                </div>
                <Collapse
                    ref={node => this.node = node}
                    isOpened={!this.state.collapsed}
                    theme={{collapse: "collapsible-row ReactCollapse--collapse", content: "ReactCollapse--content"}}>
                    {props.children }
                    {!props.buttonsAreHidden &&
                        <div className="collapsible-footer">
                            <Button type="secondary" onClick={this.onCancel.bind(this) }>{props.secondaryButtonText || "Cancel"}</Button>
                            {props.extraFooterButtons}
                        </div>
                    }
                </Collapse>
            </div>
        );
    }
}

CollapsibleRow.propTypes = {
    label: PropTypes.node,
    children: PropTypes.node,
    disabled: PropTypes.bool,
    className: PropTypes.string,
    closeOnBlur: PropTypes.bool,
    secondaryButtonText: PropTypes.string,
    extraFooterButtons: PropTypes.node,
    buttonsAreHidden: PropTypes.bool,
    collapsed: PropTypes.bool,
    onChange: PropTypes.func
};

CollapsibleRow.defaultProps = {
    collapsed: true
};

