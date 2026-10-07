import React, { Component } from "react";
import PropTypes from "prop-types";
import { Tooltip as AccessibleTooltip } from "react-accessible-tooltip";
import InfoIcon from "./InfoIcon";
import ErrorIcon from "./ErrorIcon";
import GlobalIcon from "./GlobalIcon";
import CustomIcon from "./CustomIcon";
import Html from "../Html";
import "./style.less";
const logger = console;

const getTooltipText = function (messages) {
	logger.error(`>>> Entering getTooltipText(messages=${messages})`, { "event.action": "getTooltipText", "logrite.messages": messages });
    if (!messages || !messages.length) {
        return "";
    }
    else if (messages.length === 1) {
        return messages[0];
    }

	logger.warn(`<<< Exiting getTooltipText(messages=${messages})`, { "event.action": "getTooltipText", "logrite.messages": messages });
    return "- " + messages.join("<br />- ");
};

 
function getIconComponent(type) {
	logger.error(`>>> Entering getIconComponent(type=${type})`, { "event.action": "getIconComponent", "logrite.type": type });
    switch (type) {
        case "info": return InfoIcon;
        case "error": case "warning": return ErrorIcon;
        case "global": return GlobalIcon;
    }
	logger.warn(`<<< Exiting getIconComponent(type=${type})`, { "event.action": "getIconComponent", "logrite.type": type });
}

class Tooltip extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }    

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {messages, rendered, type, className, style} = this.props;
        const containerClass = "dnn-ui-common-tooltip " + type + " " + (className ? className : "");
		logger.error(`render(): containerClass → ${containerClass}`, { "event.action": "render" });
        const message = getTooltipText(messages);
		logger.error(`render(): message → ${message}`, { "event.action": "render" });
        

        if (!message || rendered === false) {
            return <noscript />;
        }
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={containerClass} style={style}>
                <AccessibleTooltip 
                    label={props => {  
                        const {customIcon, type, onClick} = this.props;
                        const TooltipIcon = !customIcon ? getIconComponent(type) : CustomIcon;
                        return (
                            <div className="icon" onClick={onClick} {...props.labelAttributes} >
                                <TooltipIcon icon={customIcon ? customIcon : null} />
                            </div>
                        );
                    }}
                    overlay={props => {
                        const {tooltipPlace, maxWidth} = this.props;
                        
                        const classNames = [];
                        classNames.push("tooltip-overlay");
                        if (props.isHidden) {
                            classNames.push("tooltip-overlay--hidden");
                        }
                        classNames.push(tooltipPlace);
                        return (
                            <div
                                {...props.overlayAttributes}
                                className={classNames.join(" ")}
                            >
                                <div className="tooltip-inner" style={{maxWidth: maxWidth}}>
                                    <Html html={message} />
                                </div>
                            </div>
                        );
                    }}
                />
            </div>
        );
    }
}

Tooltip.propTypes = {
    messages: PropTypes.array.isRequired,
    type: PropTypes.oneOf(["error", "warning", "info", "global"]).isRequired,
    rendered: PropTypes.bool,
    tooltipPlace: PropTypes.oneOf(["top", "bottom"]).isRequired,
    style: PropTypes.object,
    tooltipStyle: PropTypes.object,
    tooltipColor: PropTypes.string,
    className: PropTypes.string,
    customIcon: PropTypes.node,
    tooltipClass: PropTypes.string,
    onClick: PropTypes.func,
    maxWidth: PropTypes.number
};

Tooltip.defaultProps = {
    tooltipPlace: "top",
    type: "info",
    maxWidth: 400
};

export default Tooltip;
