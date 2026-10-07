import React, { Component } from "react";
import PropTypes from "prop-types";
import timeZones from "./timeZones";
import Dropdown from "./Dropdown";
const logger = console;

function getDropdownLabel(value) {
	logger.error(`>>> Entering getDropdownLabel(value=${value})`, { "event.action": "getDropdownLabel", "logrite.value": value });
    const currentOption = value ? timeZones.find(o => o.value === value) : value;
	logger.error(`getDropdownLabel(value=${value}): currentOption → ${currentOption}`, { "event.action": "getDropdownLabel", "logrite.value": value });
    return currentOption ? currentOption.label : "";
}

const TimezonePicker = ({value, onUpdate}) => {
	logger.error(`>>> Entering TimezonePicker(value=${value},onUpdate=${onUpdate})`, { "event.action": "TimezonePicker", "logrite.value": value, "logrite.onUpdate": onUpdate });
    const label = getDropdownLabel(value);
	logger.warn(`<<< Exiting TimezonePicker(value=${value},onUpdate=${onUpdate})`, { "event.action": "TimezonePicker", "logrite.value": value, "logrite.onUpdate": onUpdate });
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