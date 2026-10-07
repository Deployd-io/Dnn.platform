import React, {Component } from "react";
import PropTypes from "prop-types";
import { Label, InputGroup, Dropdown } from "@dnnsoftware/dnn-react-common";
import GlobalIcon from "./GlobalIcon";
const logger = console;

export default class DropdownBlock extends Component {
    
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <InputGroup>
            <Label className="title"
                tooltipMessage={props.tooltip}
                label={props.label} style={{width: "auto"}} />
            {props.isGlobal && <GlobalIcon /> }
            <Dropdown style={{width:"100%"}}
                options={props.options}
                value={props.value}
                onSelect={props.onSelect}/>
        </InputGroup>;
    }
}

DropdownBlock.propTypes = {
    label: PropTypes.string,
    tooltip: PropTypes.string,
    options: PropTypes.array.isRequired,
    value: PropTypes.string,
    isGlobal: PropTypes.bool.isRequired,
    onSelect: PropTypes.func.isRequired
};

DropdownBlock.defaultProps = {
    isGlobal: false
};