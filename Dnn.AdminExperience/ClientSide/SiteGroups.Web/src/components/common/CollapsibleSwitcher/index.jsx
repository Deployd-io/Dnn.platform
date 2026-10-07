import PropTypes from "prop-types";
import { Component } from "react";
import "./style.less";
const logger = console;

class CollapsibleSwitcher extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return props.renderIndex >= 0 && props.children[props.renderIndex];
    }
}
CollapsibleSwitcher.propTypes = {
    renderIndex: PropTypes.number,
    children: PropTypes.array
};
CollapsibleSwitcher.defaultProps = {
    renderIndex: 0
};

export default (CollapsibleSwitcher);