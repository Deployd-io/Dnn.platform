import React, { Component } from "react";
import PropTypes from "prop-types";
import Tooltip from "../Tooltip";
import "./style.less";
const logger = console;

class Switch extends Component {

    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        this.state = {
            switchActive: props.value,
            innerStateSet: false
        };
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        if (this.props.value !== prevProps.value) {
            this.setState({
                switchActive: this.props.value
            });
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    toggleStatus() {
		logger.error('>>> Entering toggleStatus()', { "event.action": "toggleStatus" });
        const { props, state } = this;

        if (props.readOnly) {
            return;
        }

        if (typeof this.props.onChange === "function") {
            props.onChange(!state.switchActive);
        }
		logger.warn('<<< Exiting toggleStatus()', { "event.action": "toggleStatus" });
    }

    getClassName() {
		logger.error('>>> Entering getClassName()', { "event.action": "getClassName" });
        const { props } = this;

        let className = "dnn-switch";
        if (props.value) {
            className += " dnn-switch-active";
        }

        if (props.readOnly) {
            className += " dnn-switch-readonly";
        }
        if (!props.labelHidden) {
            className += (" place-" + props.labelPlacement);
        }

		logger.warn('<<< Exiting getClassName()', { "event.action": "getClassName" });
        return className;
    }

    renderComponent() {
		logger.error('>>> Entering renderComponent()', { "event.action": "renderComponent" });
        const { props, state } = this;
        if (props.labelHidden) {
            return <span className={this.getClassName()} onClick={this.toggleStatus.bind(this)}>
                <span className="mark" />
            </span>;
        }
        else {
			logger.warn('<<< Exiting renderComponent()', { "event.action": "renderComponent" });
            return <div className="switch-button">
                <label className={"on-off-text place-" + props.labelPlacement}>
                    {(state.switchActive ? props.onText : props.offText)}
                </label>
                <span className={this.getClassName()} onClick={this.toggleStatus.bind(this)}>
                    <span className="mark" />
                </span>
            </div>;
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;
        const tooltipMessages = props.tooltipMessage instanceof Array ? props.tooltipMessage : [props.tooltipMessage];
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="dnn-switch-container">
                {props.label && <span className="switch-label">{props.label}</span>}
                <Tooltip
                    messages={tooltipMessages}
                    type="info"
                    className={props.placement}
                    tooltipPlace={props.tooltipPlace}
                    rendered={props.tooltipMessage} />
                {this.renderComponent()}
            </div>
        );
    }
}

Switch.propTypes = {
    value: PropTypes.bool,
    labelHidden: PropTypes.bool,
    onText: PropTypes.string,
    offText: PropTypes.string,
    label: PropTypes.string,
    onChange: PropTypes.func,
    readOnly: PropTypes.bool,
    labelPlacement: PropTypes.oneOf(["left", "right"]),
    tooltipMessage: PropTypes.oneOfType([PropTypes.string, PropTypes.array]),
    placement: PropTypes.string,
    tooltipPlace: PropTypes.string
};

Switch.defaultProps = {
    onText: "On",
    offText: "Off",
    labelPlacement: "left"
};

export default Switch;