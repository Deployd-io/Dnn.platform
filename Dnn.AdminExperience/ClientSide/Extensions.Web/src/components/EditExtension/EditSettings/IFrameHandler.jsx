import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;
import { connect } from "react-redux";

class IFrameHandler extends Component {
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <iframe src={props.extensionBeingEdited.siteSettingsLink.value} style={{ width: "100%", height: "750px" }} />
        );
    }
}

IFrameHandler.propTypes = {
    dispatch: PropTypes.func.isRequired,
    extensionBeingEdited: PropTypes.object
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}

export default connect(mapStateToProps)(IFrameHandler);
