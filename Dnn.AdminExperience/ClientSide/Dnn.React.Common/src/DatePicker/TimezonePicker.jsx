import React, { Component } from "react";
import PropTypes from "prop-types";
import timeZones from "./timeZones";
import Dropdown from "./Dropdown";
const logger = console;

function getDropdownLabel(value) {
	logger.debug(`>>> Entering getDropdownLabel(value=${value})`);
    const currentOption = value ? timeZones.find(o => o.value === value) : value;
	logger.debug(`getDropdownLabel(value=${value}): currentOption → ${currentOption}`);
	logger.debug(`<<< Exiting getDropdownLabel(value=${value})`);
    return currentOption ? currentOption.label : "";
}

const TimezonePicker = ({value, onUpdate}) => {
	logger.debug(`>>> Entering TimezonePicker(value=${value},onUpdate=${onUpdate})`);
    const label = getDropdownLabel(value);
	logger.debug(`<<< Exiting TimezonePicker(value=${value},onUpdate=${onUpdate})`);
    return (
        <Dropdown 
            options={timeZones}
            label={label}
            onUpdate={onUpdate}
            className="timezone-picker" />
    );
};

TimezonePicker.propTypes = {
    onUpdate: PropTypes.func.isRequired,
    value: PropTypes.string
};

export default TimezonePicker;