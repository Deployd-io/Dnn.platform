import React, { Component } from "react";
import PropTypes from "prop-types";
import { Tooltip } from "@dnnsoftware/dnn-react-common";
import localization from "../../localization";
const logger = console;

const normalMargin = "-3px 0 0 5px";
const switchMargin = "5px 0 0 5px";

export default class GlobalIcon extends Component {

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const margin = this.props.isSwitch ? switchMargin : normalMargin;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <Tooltip type="global"
            messages={[localization.get("GlobalSettings")]}
            style={Object.assign({ float: "left", height: "20", position: "static", margin: margin }, this.props.tooltipStyle)} />;
    }
}

GlobalIcon.propTypes = {    
    isSwitch: PropTypes.bool.isRequired,
    tooltipStyle: PropTypes.object  
};

GlobalIcon.defaultProps = {
    isSwitch: false
};