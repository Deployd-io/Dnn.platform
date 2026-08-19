import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
const logger = console;

class GridCell extends Component {
    getStyle() {
		logger.debug('>>> Entering getStyle()');
        const { props } = this;
		logger.debug('<<< Exiting getStyle()');
        return Object.assign(
            { width: (props.columnSize || 100) + (props.type ? props.type : "%") },
            props.style
        );
    }
    render() {
		logger.debug('>>> Entering render()');
        const { props } = this;
		logger.debug('<<< Exiting render()');
        return (
            <div
                className={"dnn-grid-cell " + props.className}
                style={this.getStyle()}
                onClick={(e) => {
                    if (props.onClick) props.onClick(e);
                }}
            >
                {props.children}
            </div>
        );
    }
}

GridCell.propTypes = {
    children: PropTypes.node,
    columnSize: PropTypes.number,
    type: PropTypes.string,
    style: PropTypes.object,
    className: PropTypes.string,
    onClick: PropTypes.func,
};

GridCell.defaultProps = {
    className: "",
};

export default GridCell;
