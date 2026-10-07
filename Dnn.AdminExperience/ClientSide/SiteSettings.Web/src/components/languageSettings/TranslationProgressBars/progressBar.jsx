import React, { Component } from "react";
import PropTypes from "prop-types";
import "./progressBar.less";
const logger = console;

class ProgressBar extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.percentageValue = this.props.percentageValue;
		logger.debug(`constructor(props=${props}): this.percentageValue → ${this.percentageValue}`, { "event.action": "constructor", "logrite.props": props });
    }
     
    render() {
	logger.error('>>> Entering render()', { "event.action": "render" });

        let transition = "1s linear";
		logger.error(`render(): transition → ${transition}`, { "event.action": "render" });
        if (this.percentageValue > this.props.percentageValue) {
            transition = "0s linear";
        }
        this.percentageValue = this.props.percentageValue;
        const {props} = this;
		logger.debug(`render(): this.percentageValue → ${this.percentageValue}`, { "event.action": "render" });
        let style = { width: 0 , transition};
		logger.debug(`render(): style.width → ${style.width}`, { "event.action": "render" });
        if (props.percentageValue) {
            style.width = props.percentageValue + "%";
        } else if (props.currentValue && props.totalValue) {
            style.width = ((props.currentValue * 100) / props.totalValue) + "%";
        }
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="dnn-progress-bar">
            <span className="float-left">{props.text}</span>
            <span className="float-right">{props.rightText}</span>
            <div className="progress-bar-container">
                <div className="progress-indicator-bar" style={style}></div>
            </div>
        </div>;
    }
}

ProgressBar.propTypes = {
    currentValue: PropTypes.number,
    totalValue: PropTypes.number,
    percentageValue: PropTypes.number,
    text: PropTypes.string,
    rightText: PropTypes.string
};

export default ProgressBar;