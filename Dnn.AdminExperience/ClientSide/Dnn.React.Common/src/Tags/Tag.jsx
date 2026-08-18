import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
const logger = console;

export default class Tag extends Component {

    render() {
		logger.debug('>>> Entering render()');
        const {onRemove} = this.props;
        const tagName = this.props.tag.trim();
        if (!tagName) {
		logger.debug('<<< Exiting render()');
            return false;
        }
        return (
            <div className={"dnn-uicommon-tag-input" + (this.props.enabled ? "": " disabled")}>
                <span>{this.props.tag}</span>
                {this.props.enabled && <span className="close" onClick={e => {
                    e.stopPropagation();
                    onRemove();
                }}>×</span>}
            </div>
        );
    }
}

Tag.propTypes = {
    tag: PropTypes.string.isRequired,
    onRemove: PropTypes.func.isRequired,
    enabled: PropTypes.bool.isRequired
};

Tag.defaultProps = {
    enabled: true
};