import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;

export default class Tag extends Component {

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const tagName = this.props.tag.trim();
        if (!tagName) {
            return false;
        }
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="tag glyph">
                {this.props.tag}
                <span className="close" onClick={this.props.onRemove}>×</span>
            </div>
        );
    }
}

Tag.propTypes = {
    tag: PropTypes.string,
    onRemove: PropTypes.func
};