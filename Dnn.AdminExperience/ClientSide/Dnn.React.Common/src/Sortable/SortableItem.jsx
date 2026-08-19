import React, { Component } from "react";
import PropTypes from "prop-types";
import Draggable from "./Draggable";
const logger = console;

class SortableItem extends Component {
    render() {
		logger.debug('>>> Entering render()');
        const item = this.props.children.props.listItem;
        const selected = this.props.children.props.selected;

        const className = "sortable-item drag-element" + (selected ? " sortable-selected" : "");
		logger.debug('<<< Exiting render()');
        return <div className={className} data-index={item.index} data-dnn-sortable-id={item.id}>
            {this.props.children}
        </div>;
    }
}

SortableItem.propTypes = {
    children: PropTypes.node
};

export default Draggable(SortableItem);