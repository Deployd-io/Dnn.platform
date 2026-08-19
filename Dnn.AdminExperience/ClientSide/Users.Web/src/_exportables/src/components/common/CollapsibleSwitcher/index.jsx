import PropTypes from "prop-types";
import { Component } from "react";
import "./style.less";
const logger = console;

class CollapsibleSwitcher extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
		logger.debug('<<< Exiting render()');
        return props.renderIndex >= 0 && props.children[props.renderIndex];
    }
}
CollapsibleSwitcher.propTypes = {
    renderIndex: PropTypes.number,
    children: PropTypes.node
};
CollapsibleSwitcher.defaultProps = {
    renderIndex: 0
};

export default (CollapsibleSwitcher);