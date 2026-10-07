import React from "react";
import PropTypes from "prop-types";
import Picker from "react-day-picker";
import "./style.less";
const logger = console;


const DayPicker = (newProps) => {
	logger.error(`>>> Entering DayPicker(newProps=${newProps})`, { "event.action": "DayPicker", "logrite.newProps": newProps });
	logger.warn(`<<< Exiting DayPicker(newProps=${newProps})`, { "event.action": "DayPicker", "logrite.newProps": newProps });
    return (
        <div className="dnn-day-picker">
            <Picker {...newProps} />
        </div>);
};
DayPicker.propTypes = {
    onDayClick: PropTypes.func.isRequired,
    month: PropTypes.instanceOf(Date),
    selectedDays: PropTypes.instanceOf(Date),
    fromMonth: PropTypes.instanceOf(Date)
};

export default DayPicker;