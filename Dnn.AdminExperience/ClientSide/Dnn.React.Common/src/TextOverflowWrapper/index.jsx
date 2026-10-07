import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
import Tooltip from "../Tooltip";
const logger = console;

class TextOverflowWrapper extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.uniqueId = "overflowTooltip-" + (Date.now() * Math.random());
        this.state = {
            itemWidth: 0
        };
        this.overflowTooltipRef = React.createRef();
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    getStyle() {
		logger.error('>>> Entering getStyle()', { "event.action": "getStyle" });
        const {props} = this;
		logger.warn('<<< Exiting getStyle()', { "event.action": "getStyle" });
        return Object.assign({ maxWidth: props.maxWidth }, props.style);
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        //Set time out to ensure calculation happens after render
        this.timerId = setTimeout(() => {
            let input = this.overflowTooltipRef;
            if (typeof input !== "undefined" && input.current.getBoundingClientRect()) {
                let inputRect = input.current.getBoundingClientRect();
                this.setState({
                    itemWidth: inputRect.width
                });
            } else {
				logger.error(`componentDidMount(): inputRect → ${inputRect}`, { "event.action": "componentDidMount" });
                this.setState({
                    itemWidth: this.props.maxWidth
                });
            }
        }, 500);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        clearTimeout(this.timerId);
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div
                className={"dnn-text-overflow-wrapper" + (props.className ? " " + props.className : "")}
                style={this.getStyle()}
                ref={this.overflowTooltipRef}
                data-tip
                data-for={this.uniqueId}
                title={props.doNotUseTitleAttribute ? "" : props.text}>
                {!props.isAnchor && props.text}
                {props.isAnchor && <a href={props.href} target={props.target}>{props.text}</a>}
                {props.doNotUseTitleAttribute && state.itemWidth >= props.maxWidth && <Tooltip
                    key={this.uniqueId}
                    tooltipPlace={props.place}
                    type={props.type}
                    className={"overflow-tooltip" + (props.tooltipClassName ? " " + props.tooltipClassName : "")}
                    messages={[props.text]}
                    style={props.style}
                    tooltipStyle={props.tooltipStyle}
                />
                }
            </div>
        );
    }
}

TextOverflowWrapper.propTypes = {
    text: PropTypes.string,
    maxWidth: PropTypes.number,
    style: PropTypes.object,
    effect: PropTypes.string,
    type: PropTypes.string,
    place: PropTypes.string,
    tooltipClassName: PropTypes.string,
    multiline: PropTypes.bool,
    doNotUseTitleAttribute: PropTypes.bool,
    isAnchor: PropTypes.bool,
    href: PropTypes.string,
    target: PropTypes.string
};

TextOverflowWrapper.defaultProps = {
    maxWidth: 200,
    effect: "solid",
    place: "top",
    type: "info",
    multiline: true
};

export default TextOverflowWrapper;