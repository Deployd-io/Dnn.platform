import React, {Component} from "react";
import PropTypes from "prop-types";
import "./style.less";
const logger = console;

class MenuItem extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = { hover: false };
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug('<<< Exiting constructor()');
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <li className="dnn-in-context-menu menu-item" onMouseEnter={() => this.setState({ hover: true }) }
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