import React, {Component} from "react";
import PropTypes from "prop-types";
import "./style.less";
const logger = console;

class Menu extends Component {
    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <ul className="dnn-in-context-menu menu">
                {this.props.children}
            </ul>
        );
    }
}

Menu.propTypes = {
    children: PropTypes.node
};

export default Menu;