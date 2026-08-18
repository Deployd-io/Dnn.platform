import React from "react";
import PropTypes from "prop-types";
import "./style.less";
const logger = console;

function getHandler(handler, enabled) {
	logger.debug(`>>> Entering getHandler(handler=${handler},enabled=${enabled})`);
	logger.debug(`<<< Exiting getHandler(handler=${handler},enabled=${enabled})`);
    return handler && enabled ? handler : null; 
}

function getOpts(enabled, multipleSelect) {
	logger.debug(`>>> Entering getOpts(enabled=${enabled},multipleSelect=${multipleSelect})`);
    const opts = {};
    if (!enabled) {
        opts["disabled"] = "disabled"; 
    }
    if (multipleSelect) {
        opts["multiple"] = "multiple"; 
    }
	logger.debug(`<<< Exiting getOpts(enabled=${enabled},multipleSelect=${multipleSelect})`);
    return opts;
}

function getOptionsList(options) {
	logger.debug(`>>> Entering getOptionsList(options=${options})`);
	logger.debug(`<<< Exiting getOptionsList(options=${options})`);
    return options.map((option, index) => {
        return <option key={option.value + "_" + index} value={option.value}>{option.label}</option>;
    });  
}

function getActualValue(multipleSelect, value, valueArray) {
    if (!multipleSelect) {
        if (!value && valueArray && valueArray.length > 0) {
            return valueArray[0];
        }
        return value;
    } else {
        if (!valueArray || valueArray.length === 0) {
            return [value];
        }
        return valueArray;
    } 
}

const Select = ({onChange, value, valueArray, options, enabled, multipleSelect, style}) => (
    <select 
        className="dnn-uicommon-select"
        onChange={getHandler(onChange, enabled)}            
        value={getActualValue(multipleSelect, value, valueArray)}            
        style={style}    
        aria-label="Select"        
        {...getOpts(enabled, multipleSelect)}>
        {getOptionsList(options)}
    </select> 
);

Select.propTypes = {
    onChange: PropTypes.func,
    options: PropTypes.arrayOf(PropTypes.object).isRequired,
    value: PropTypes.string,
    valueArray: PropTypes.array,    
    enabled: PropTypes.bool,
    multipleSelect: PropTypes.bool,
    style: PropTypes.object 
};

Select.defaultProps = {
    enabled: true,
    multipleSelect: false
};

export default Select;