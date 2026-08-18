import React, { Component } from "react";
import PropTypes from "prop-types";
import SortableContainer from "./SortableContainer";
import ReactDOM from "react-dom";
import "./style.less";
const logger = console;

export default class Sortable extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);

        const items = this.props.items.map((item, index) => {
            return { index, item, id: index };
        });
        this.currentIndex = -1;
		logger.debug(`constructor(props=${props}): items → ${items}`);
        this.state = {
            items,
            dragging: {
                isDragging: false
            },
            isDraggingOver: false
        };
        this.dnnSortableRef = React.createRef();
		logger.debug(`constructor(props=${props}): currentIndex → ${currentIndex}`);
		logger.debug(`constructor(props=${props}): this.currentIndex → ${this.currentIndex}`);
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    componentDidUpdate(prevProps) {
        if (prevProps.items !== this.props.items) {
			logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
            const items = this.props.items.map((item, index) => {
                return { index, item, id: index };
            });
            this.setState({items});
			logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
			logger.debug(`componentDidUpdate(prevProps=${prevProps}): items → ${items}`);
        }
    }

    sortColumns(a, b) {
		logger.debug(`>>> Entering sortColumns(a=${a},b=${b})`);
        if (a.index < b.index)
        { return -1; }
        else if (a.index > b.index)
        { return 1; }
		logger.debug(`<<< Exiting sortColumns(a=${a},b=${b})`);
        return 0;
    }

    onDragStart(component, event) {
		logger.debug(`>>> Entering onDragStart(component=${component},event=${event})`);
        let dragging = {
            isDragging: true
        };
        this.setState({ dragging });
        const id = event.target.getAttribute("data-dnn-sortable-id");
		logger.debug(`onDragStart(component=${component},event=${event}): id → ${id}`);
        this.setFieldSelected(id, true);
        event.target.classList.add("dnn-sortable-dragging");
		logger.debug(`<<< Exiting onDragStart(component=${component},event=${event})`);
    }

    onDragEnd(component, event) {
		logger.debug(`>>> Entering onDragEnd(component=${component},event=${event})`);
        let dragging = {
            isDragging: false
        };
        this.setState({ dragging });
        const id = event.target.getAttribute("data-dnn-sortable-id");
		logger.debug(`onDragEnd(component=${component},event=${event}): id → ${id}`);
        this.setFieldSelected(id, false);
        this.currentIndex = -1;
        this.unselectAll();
		logger.debug(`onDragEnd(component=${component},event=${event}): currentIndex → ${currentIndex}`);
		logger.debug(`<<< Exiting onDragEnd(component=${component},event=${event})`);
    }

    unselectAll() {
		logger.debug('>>> Entering unselectAll()');
        let {items} = this.state;
        items.forEach(i => i ? i.selected = false : false);
        this.setState({ items });
		logger.debug('<<< Exiting unselectAll()');
    }

    setFieldSelected(id, selected) {
        let {items} = this.state;

        // This file uses == intead of === at multiple places, please do not fix until we rewrite this or the sorting will fail
        items.forEach((items) => {
            if (items.id == id) {
                items.selected = selected;
            } else {
                items.selected = false;
				logger.debug(`<<< Exiting setFieldSelected(id=${id},selected=${selected})`);
            }
        });
    }

    onDragMove() {
	logger.debug('>>> Entering onDragMove()');
    }

    updateItemClass(sortableItems, dropY) {
        [].forEach.call(sortableItems, (item, index) => {
            if (dropY > item.offsetHeight * index + item.getBoundingClientRect().height / 2 &&
                dropY < item.offsetHeight * index + item.getBoundingClientRect().height + item.getBoundingClientRect().height / 2) {
                item.classList.add("dragged-over");
            } else {
                item.classList.remove("dragged-over");
            }
        });
		logger.debug(`<<< Exiting updateItemClass(sortableItems=${sortableItems},dropY=${dropY})`);
    }

    onDropMove(event, dropX, dropY) {
		logger.debug(`>>> Entering onDropMove(event=${event},dropX=${dropX},dropY=${dropY})`);
        this.setState({ isDraggingOver: true });
        const sortableItems = event.target.getElementsByClassName("sortable-item");
        if (this.props.sortOnDrag) {
            this.sortOnDrag(event, dropX, dropY);
        } else {
            this.updateItemClass(sortableItems, dropY);
			logger.debug(`<<< Exiting onDropMove(event=${event},dropX=${dropX},dropY=${dropY})`);
        }
    }

    onDragLeave() {
		logger.debug('>>> Entering onDragLeave()');
        this.setState({ isDraggingOver: false });
        this.removePlaceholder();
		logger.debug('<<< Exiting onDragLeave()');
    }

    removePlaceholder() {
		logger.debug('>>> Entering removePlaceholder()');
        const sortableItems = document.getElementsByClassName("sortable-item");
        this.updateItemClass(sortableItems, -1000);
		logger.debug(`removePlaceholder(): sortableItems → ${sortableItems}`);
		logger.debug('<<< Exiting removePlaceholder()');
    }

    getNewIndex(sortableItems, dropY) {
        let newIndex = 0;
        [].forEach.call(sortableItems, (item, index) => {
            if (dropY > item.offsetHeight * index + item.getBoundingClientRect().height / 2) {
                newIndex = +item.getAttribute("data-index") + 1;
            }
        });
        return newIndex;
    }

    sortItem(newIndex) {
        const curIndex = this.currentIndex;
        let {items} = this.state;
        let currentItem = items.find(i => i.index == curIndex);                
        let itemToReplace = items.find(i => i.index == newIndex);
        if (!itemToReplace) {
            return;
        }
        currentItem.index = newIndex;
        itemToReplace.index = curIndex;
        this.currentIndex = newIndex;
        items.sort(this.sortColumns);
        this.setState({ items }, this.callProps.bind(this));
		logger.debug(`<<< Exiting sortItem(newIndex=${newIndex})`);
    }

    moveItem(items, id, index) {
        let item = items.find(i => i.id == id);
        item.index = index;
		logger.debug(`<<< Exiting moveItem(items=${items},id=${id},index=${index})`);
    }

    updateIndexes(items, index) {
        items.forEach((item) => {
            if (item.index >= index) {
                item.index = item.index + 1;
            }
        });
        return items;
    }

    sortOnDrag(event, dropX, dropY) {
		logger.debug(`>>> Entering sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY})`);
        let id = event.draggable._element.getAttribute("data-dnn-sortable-id");
        if (this.dnnSortableRef.current)
		logger.debug(`sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY}): id → ${id}`);
        {
            const itemElement = this.dnnSortableRef.current.querySelectorAll(`[data-dnn-sortable-id="${id}"]`)[0];
			logger.debug(`sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY}): itemElement → ${itemElement}`);
            itemElement.getAttribute("data-index");
            const sortableItems = document.getElementsByClassName("sortable-item");
			logger.debug(`sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY}): sortableItems → ${sortableItems}`);
            let newIndex = this.getNewIndex(sortableItems, dropY);

            if (this.currentIndex == -1) {
                const currentIndex = itemElement.getAttribute("data-index");
				logger.debug(`sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY}): currentIndex → ${currentIndex}`);
                this.currentIndex = currentIndex;
            }

            const isSameIndex = newIndex == this.currentIndex;
            if (isSameIndex) {
                return;
            }
            this.sortItem(newIndex);
			logger.debug(`sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY}): newIndex → ${newIndex}`);
        }
		logger.debug(`<<< Exiting sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY})`);
    }

    onDrop(event, dropX, dropY) {
		logger.debug(`>>> Entering onDrop(event=${event},dropX=${dropX},dropY=${dropY})`);
        this.currentIndex = -1;
        this.unselectAll();
        this.setState({ isDraggingOver: false });
        if (this.props.sortOnDrag) {
			logger.debug(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): this.currentIndex → ${this.currentIndex}`);
            return;
        }
        let id = event.draggable._element.getAttribute("data-dnn-sortable-id");
		logger.debug(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): id → ${id}`);
        if (this.dnnSortableRef.current) {
            const itemElement = this.dnnSortableRef.current.querySelectorAll(`[data-dnn-sortable-id="${id}"]`)[0];
			logger.debug(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): itemElement → ${itemElement}`);
            itemElement.classList.remove("sortable-selected");
            this.removePlaceholder();
            const sortableItems = document.getElementsByClassName("sortable-item");
			logger.debug(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): sortableItems → ${sortableItems}`);
			logger.debug(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): currentIndex → ${currentIndex}`);
            let newIndex = this.getNewIndex(sortableItems, dropY);
			logger.debug(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): newIndex → ${newIndex}`);
            const currentIndex = event.draggable._element.getAttribute("data-index");
            const isSameIndex = currentIndex != null && (newIndex == currentIndex || newIndex - 1 == currentIndex);
            if (isSameIndex) {
				logger.debug(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): isSameIndex → ${isSameIndex}`);
                return;
            }

            let {items} = this.state;
            items = this.updateIndexes(items, newIndex);
            this.moveItem(items, id, newIndex);
            this.resetIndexes(items);
            this.setState({ items }, this.callProps.bind(this));
        }
    }

    callProps() {
		logger.debug('>>> Entering callProps()');
        const items = this.state.items.map(item => item.item);
        this.props.onSort(items);
		logger.debug(`callProps(): items → ${items}`);
		logger.debug('<<< Exiting callProps()');
    }


    resetIndexes(items = this.state.items) {
		logger.debug(`>>> Entering resetIndexes(items=${items})`);
        items.sort(this.sortColumns);
        items.forEach((item, index) => {
            item.index = index;
        });
		logger.debug(`<<< Exiting resetIndexes(items=${items})`);
    }


    render() {
		logger.debug('>>> Entering render()');
        const {children, items} = this.props;
        if (children.length != items.length) {
            return false;
        }
        const listItems = children.map((child, index) => {
            const item = this.state.items[index] || {};
            return React.cloneElement(child, {
                listItem: item,
                selected: item.selected
            });
        });

		logger.debug(`render(): listItems → ${listItems}`);
		logger.debug('<<< Exiting render()');
        return <div className="dnn-sortable" ref={this.dnnSortableRef}>
            <SortableContainer
                onDragMove={this.onDragMove.bind(this) }
                onDragStart={this.onDragStart.bind(this) }
                onDragEnd={this.onDragEnd.bind(this) }
                onDrop={this.onDrop.bind(this) }
                onDropMove={this.onDropMove.bind(this) }
                onDragLeave={this.onDragLeave.bind(this) }
                isDragging={this.state.dragging.isDragging}
                isDraggingOver={this.state.isDraggingOver}
            >
                {listItems}
            </SortableContainer>
        </div>;
    }
}

Sortable.propTypes = {
    children: PropTypes.node.isRequired,
    items: PropTypes.array.isRequired,
    onSort: PropTypes.func.isRequired,
    sortOnDrag: PropTypes.bool
};