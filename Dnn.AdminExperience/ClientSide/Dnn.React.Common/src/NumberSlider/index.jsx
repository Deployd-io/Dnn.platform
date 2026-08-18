import React, { Component } from "react";
import PropTypes from "prop-types";
import ReactSlider from "react-slider";
import "./style.less";
const logger = console;

export default class NumberSlider extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const minimum = this.props.min;
        const maximum = this.props.max;
        let step = this.props.step || 1;

        const value = this.props.value;

        const className = "slider-container" + (this.props.withMinMax ? " with-min-max": "");
        
		logger.debug('<<< Exiting render()');
        return (
            <div className={className}>
                {this.props.withMinMax && <span>{minimum}</span>}
                <ReactSlider 
                    min={minimum} 
                    value={value}
                    max={maximum}
                    step={step}
                    onChange={this.props.onChange}
                    disabled={!!this.props.disabled}
                    renderThumb={(props, _state) => !this.props.hideValue && <div {...props}><div>{value}</div></div>}>
                </ReactSlider>
                {this.props.withMinMax && <span>{maximum}</span>}
            </div>
        );
    }
}

NumberSlider.propTypes = {
    min: PropTypes.number.isRequired, 
    max: PropTypes.number.isRequired,
    step: PropTypes.number.isRequired,
    value: PropTypes.number.isRequired,
    onChange: PropTypes.func.isRequired,
    disabled: PropTypes.bool,
    withMinMax: PropTypes.bool,
    hideValue: PropTypes.bool
};