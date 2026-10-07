import PropTypes from "prop-types";
import React, { Component } from "react";
import "./style.less";
const logger = console;

class MenuItem extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = { hover: false };
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <li className="dnn-user-menu menu-item" onMouseEnter={() => this.setState({ hover: true }) }
                onMouseLeave={() => this.setState({ hover: false }) }
                onClick={this.props.onMenuAction}>
                {this.props.children}
            </li>
        );
    }
}

MenuItem.propTypes = {
    onMenuAction: PropTypes.func.isRequired,
    children: PropTypes.node.isRequired
};

export default MenuItem;