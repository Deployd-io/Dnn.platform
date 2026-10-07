import React, { Component } from "react";
import PropTypes from "prop-types";
import { Label, InputGroup, RadioButtons } from "@dnnsoftware/dnn-react-common";
import GlobalIcon from "./GlobalIcon";
const logger = console;

export default class RadioButtonBlock extends Component {

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <InputGroup>
            <Label className="title"
                tooltipMessage={props.tooltip}
                label={props.label} style={{width: "auto"}}/>
            {props.isGlobal && <GlobalIcon /> }
            <RadioButtons
                options={props.options}                
                value={props.value}
                onChange={props.onChange} />
        </InputGroup>;
    }
}

RadioButtonBlock.propTypes = {
    label: PropTypes.string,
    tooltip: PropTypes.string,
    options: PropTypes.array.isRequired,
    value: PropTypes.string,
    onChange: PropTypes.func.isRequired,
    isGlobal: PropTypes.bool.isRequired
};

RadioButtonBlock.defaultProps = {
    isGlobal: false
};