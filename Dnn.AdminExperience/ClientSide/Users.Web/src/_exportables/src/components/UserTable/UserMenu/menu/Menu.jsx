import PropTypes from "prop-types";
import React, { Component } from "react";
import "./style.less";
const logger = console;

class Menu extends Component {
    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <ul className="dnn-user-menu menu">
                {this.props.children}
            </ul>
        );
    }
}

Menu.propTypes = {
    children: PropTypes.node
};

export default Menu;