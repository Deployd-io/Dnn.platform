import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;
import { connect } from "react-redux";

class IFrameHandler extends Component {
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;

		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}

export default connect(mapStateToProps)(IFrameHandler);
