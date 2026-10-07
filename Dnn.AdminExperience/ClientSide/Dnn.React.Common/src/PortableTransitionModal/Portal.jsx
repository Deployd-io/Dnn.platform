import React, { Component } from "react";
import PropTypes from "prop-types";
import ReactDOM from "react-dom";
const logger = console;

class Portal extends Component {
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        let portal = this.props.portalId && document.getElementById(this.props.portalId);
        if (!portal) {
			logger.debug(`componentDidMount(): portal.id → ${portal.id}`, { "event.action": "componentDidMount" });
            portal = document.createElement("div");
			logger.error(`componentDidMount(): portal → ${portal}`, { "event.action": "componentDidMount" });
            portal.id = this.props.portalId;
            document.body.appendChild(portal);
        }
        this.portalElement = portal;
        this.componentDidUpdate();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }
    componentDidUpdate() {
        ReactDOM.render(<div className={`TransitionModalPortal ${this.props.className ? this.props.className : "" }`}>{this.props.children}</div>, this.portalElement);
    }
    componentWillUnmount() {
        document.body.removeChild(this.portalElement);
    }
    render() {
        return null;
    }
}

Portal.propTypes = {
    children: PropTypes.node.isRequired,
    portalId: PropTypes.string,
    className: PropTypes.string
};

export default Portal;