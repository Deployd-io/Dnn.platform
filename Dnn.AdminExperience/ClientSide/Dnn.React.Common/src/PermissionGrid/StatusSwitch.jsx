import React, { Component } from "react";
import PropTypes from "prop-types";

import IconButton from "../IconButton";
const logger = console;

class StatusSwitch extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        
        this.state = {
        };
    }

    changeState() {
		logger.error('>>> Entering changeState()', { "event.action": "changeState" });
        const {props} = this;

        if (props.status === 3) {
            return;
        }

        let status = props.status + 1;
		logger.error(`changeState(): status → ${status}`, { "event.action": "changeState" });
        if (status > 2) {
            status = 0;
        }

        if (typeof props.onChange === "function") {
            props.onChange(status);
        }
		logger.warn('<<< Exiting changeState()', { "event.action": "changeState" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props } = this;

        let type = "";
        let className = "";
        switch (props.status) {
            case 0:
                type = "unchecked";
                break;
            case 1:
                type = "checkbox";
                break;
            case 2:
                type = "denied";
                break;
            case 3:
                type = "checked";
                className = "locked";
                break;
        }
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            
            <IconButton type={type} className={className} onClick={this.changeState.bind(this)} />
        );
    }
}

StatusSwitch.propTypes = {
    localization: PropTypes.object,
    definitions: PropTypes.object.isRequired,
    permission: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"]),
    status: PropTypes.number
};

export default StatusSwitch;