import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "../GridCell";
import "./style.less";
const logger = console;

class GridSystem extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.uniqueId = "Grid-" + (Date.now() * Math.random());
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    getStyle() {
		logger.error('>>> Entering getStyle()', { "event.action": "getStyle" });
        const {props} = this;
		logger.warn('<<< Exiting getStyle()', { "event.action": "getStyle" });
        return Object.assign({ width: (props.width || 100) + (props.type ? props.type : "%") }, props.style);
    }
    /*
        getColumns() method.
        This method calculates the width of each column based on the number of columns specified.
        If no number is specified, then the width is calculated via the length of the children inside.
     */
    getColumns() {
		logger.error('>>> Entering getColumns()', { "event.action": "getColumns" });
        const {props} = this;
        let children = [];
        for (let i = 0; i < (props.numberOfColumns || props.children.length); i++) {
            children.push(<GridCell key={this.uniqueId + i} columnSize={100 / (props.numberOfColumns || props.children.length)} style={props.gridCellStyle}>{props.children[i]}</GridCell>);
        }
		logger.warn('<<< Exiting getColumns()', { "event.action": "getColumns" });
        return children;
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={"dnn-grid-system " + props.className}  style={this.getStyle() }>
                {this.getColumns() }
            </div>
        );
    }
}

GridSystem.propTypes = {
    children: PropTypes.node,
    width: PropTypes.number,
    type: PropTypes.string,
    numberOfColumns: PropTypes.number,
    gridCellStyle: PropTypes.object,
    style: PropTypes.object,
    className: PropTypes.string
};

GridSystem.defaultProps = {
    className: ""
};

export default GridSystem;