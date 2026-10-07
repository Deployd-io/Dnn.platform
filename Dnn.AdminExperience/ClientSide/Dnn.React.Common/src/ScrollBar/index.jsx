import React, { Component } from "react";
import PropTypes from "prop-types";

import ScrollArea from "react-scrollbar/dist/no-css";
import "./styles.less";
const logger = console;

class ScrollBar extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

	logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <ScrollArea
                speed={0.8}
                className="area"
                contentClassName={this.props.contentClassName?this.props.contentClassName:""}
                contentStyle={this.props.contentStyle?this.props.contentStyle:""}>
                {this.props.children}
            </ScrollArea>
        );
    }
}


ScrollBar.propTypes = {
    children : PropTypes.object.isRequired,
    className : PropTypes.object,
    contentClassName : PropTypes.object,
    contentStyle: PropTypes.object
};

export default ScrollBar;