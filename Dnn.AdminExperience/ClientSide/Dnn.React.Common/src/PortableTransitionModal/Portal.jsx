import React, { Component } from "react";
import PropTypes from "prop-types";
import ReactDOM from "react-dom";
const logger = console;

class Portal extends Component {
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        let portal = this.props.portalId && document.getElementById(this.props.portalId);
        if (!portal) {
            portal = document.createElement("div");
			logger.debug(`componentDidMount(): portal → ${portal}`);
            portal.id = this.props.portalId;
            document.body.appendChild(portal);
        }
        this.portalElement = portal;
		logger.debug(`componentDidMount(): portal.id → ${portal.id}`);
        this.componentDidUpdate();
		logger.debug('<<< Exiting componentDidMount()');
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