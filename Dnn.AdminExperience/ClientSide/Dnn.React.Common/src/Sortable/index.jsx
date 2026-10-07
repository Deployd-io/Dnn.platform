import React, { Component } from "react";
import PropTypes from "prop-types";
import SortableContainer from "./SortableContainer";
import ReactDOM from "react-dom";
import "./style.less";
const logger = console;

export default class Sortable extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        const items = this.props.items.map((item, index) => {
            return { index, item, id: index };
        });
        this.currentIndex = -1;
		logger.error(`constructor(props=${props}): items → ${items}`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            items,
            dragging: {
                isDragging: false
            },
            isDraggingOver: false
        };
        this.dnnSortableRef = React.createRef();
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        if (prevProps.items !== this.props.items) {
            const items = this.props.items.map((item, index) => {
                return { index, item, id: index };
            });
            this.setState({items});
			logger.error(`componentDidUpdate(prevProps=${prevProps}): items → ${items}`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    sortColumns(a, b) {
		logger.error(`>>> Entering sortColumns(a=${a},b=${b})`, { "event.action": "sortColumns", "logrite.a": a, "logrite.b": b });
        if (a.index < b.index)
        { return -1; }
        else if (a.index > b.index)
        { return 1; }
		logger.warn(`<<< Exiting sortColumns(a=${a},b=${b})`, { "event.action": "sortColumns", "logrite.a": a, "logrite.b": b });
        return 0;
    }

    onDragStart(component, event) {
		logger.error(`>>> Entering onDragStart(component=${component},event=${event})`, { "event.action": "onDragStart", "logrite.component": component, "logrite.event": event });
        let dragging = {
            isDragging: true
        };
        this.setState({ dragging });
        const id = event.target.getAttribute("data-dnn-sortable-id");
		logger.error(`onDragStart(component=${component},event=${event}): id → ${id}`, { "event.action": "onDragStart", "logrite.component": component, "logrite.event": event });
        this.setFieldSelected(id, true);
        event.target.classList.add("dnn-sortable-dragging");
		logger.warn(`<<< Exiting onDragStart(component=${component},event=${event})`, { "event.action": "onDragStart", "logrite.component": component, "logrite.event": event });
    }

    onDragEnd(component, event) {
		logger.error(`>>> Entering onDragEnd(component=${component},event=${event})`, { "event.action": "onDragEnd", "logrite.component": component, "logrite.event": event });
        let dragging = {
            isDragging: false
        };
        this.setState({ dragging });
        const id = event.target.getAttribute("data-dnn-sortable-id");
		logger.error(`onDragEnd(component=${component},event=${event}): id → ${id}`, { "event.action": "onDragEnd", "logrite.component": component, "logrite.event": event });
        this.setFieldSelected(id, false);
        this.currentIndex = -1;
        this.unselectAll();
		logger.warn(`<<< Exiting onDragEnd(component=${component},event=${event})`, { "event.action": "onDragEnd", "logrite.component": component, "logrite.event": event });
    }

    unselectAll() {
		logger.error('>>> Entering unselectAll()', { "event.action": "unselectAll" });
        let {items} = this.state;
        items.forEach(i => i ? i.selected = false : false);
        this.setState({ items });
		logger.warn('<<< Exiting unselectAll()', { "event.action": "unselectAll" });
    }

    setFieldSelected(id, selected) {
		logger.error(`>>> Entering setFieldSelected(id=${id},selected=${selected})`, { "event.action": "setFieldSelected", "logrite.id": id, "logrite.selected": selected });
        let {items} = this.state;

        // This file uses == intead of === at multiple places, please do not fix until we rewrite this or the sorting will fail
        items.forEach((items) => {
            if (items.id == id) {
                items.selected = selected;
            } else {
                items.selected = false;
				logger.warn(`<<< Exiting setFieldSelected(id=${id},selected=${selected})`, { "event.action": "setFieldSelected", "logrite.id": id, "logrite.selected": selected });
            }
        });
    }

    onDragMove() {
	logger.error('>>> Entering onDragMove()', { "event.action": "onDragMove" });
    }

    updateItemClass(sortableItems, dropY) {
		logger.error(`>>> Entering updateItemClass(sortableItems=${sortableItems},dropY=${dropY})`, { "event.action": "updateItemClass", "logrite.sortableItems": sortableItems, "logrite.dropY": dropY });
        [].forEach.call(sortableItems, (item, index) => {
            if (dropY > item.offsetHeight * index + item.getBoundingClientRect().height / 2 &&
                dropY < item.offsetHeight * index + item.getBoundingClientRect().height + item.getBoundingClientRect().height / 2) {
                item.classList.add("dragged-over");
            } else {
                item.classList.remove("dragged-over");
				logger.warn(`<<< Exiting updateItemClass(sortableItems=${sortableItems},dropY=${dropY})`, { "event.action": "updateItemClass", "logrite.sortableItems": sortableItems, "logrite.dropY": dropY });
            }
        });
    }

    onDropMove(event, dropX, dropY) {
		logger.error(`>>> Entering onDropMove(event=${event},dropX=${dropX},dropY=${dropY})`, { "event.action": "onDropMove", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
        this.setState({ isDraggingOver: true });
        const sortableItems = event.target.getElementsByClassName("sortable-item");
        if (this.props.sortOnDrag) {
            this.sortOnDrag(event, dropX, dropY);
        } else {
            this.updateItemClass(sortableItems, dropY);
			logger.warn(`<<< Exiting onDropMove(event=${event},dropX=${dropX},dropY=${dropY})`, { "event.action": "onDropMove", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
        }
    }

    onDragLeave() {
		logger.error('>>> Entering onDragLeave()', { "event.action": "onDragLeave" });
        this.setState({ isDraggingOver: false });
        this.removePlaceholder();
		logger.warn('<<< Exiting onDragLeave()', { "event.action": "onDragLeave" });
    }

    removePlaceholder() {
		logger.error('>>> Entering removePlaceholder()', { "event.action": "removePlaceholder" });
        const sortableItems = document.getElementsByClassName("sortable-item");
        this.updateItemClass(sortableItems, -1000);
		logger.error(`removePlaceholder(): sortableItems → ${sortableItems}`, { "event.action": "removePlaceholder" });
    }

    getNewIndex(sortableItems, dropY) {
		logger.error(`>>> Entering getNewIndex(sortableItems=${sortableItems},dropY=${dropY})`, { "event.action": "getNewIndex", "logrite.sortableItems": sortableItems, "logrite.dropY": dropY });
        let newIndex = 0;
        [].forEach.call(sortableItems, (item, index) => {
            if (dropY > item.offsetHeight * index + item.getBoundingClientRect().height / 2) {
                newIndex = +item.getAttribute("data-index") + 1;
            }
        });
		logger.error(`getNewIndex(sortableItems=${sortableItems},dropY=${dropY}): newIndex → ${newIndex}`, { "event.action": "getNewIndex", "logrite.sortableItems": sortableItems, "logrite.dropY": dropY });
        return newIndex;
    }

    sortItem(newIndex) {
		logger.error(`>>> Entering sortItem(newIndex=${newIndex})`, { "event.action": "sortItem", "logrite.newIndex": newIndex });
        const curIndex = this.currentIndex;
        let {items} = this.state;
        let currentItem = items.find(i => i.index == curIndex);                
		logger.error(`sortItem(newIndex=${newIndex}): currentItem → ${currentItem}`, { "event.action": "sortItem", "logrite.newIndex": newIndex });
		logger.debug(`sortItem(newIndex=${newIndex}): currentItem.index → ${currentItem.index}`, { "event.action": "sortItem", "logrite.newIndex": newIndex });
        let itemToReplace = items.find(i => i.index == newIndex);
		logger.error(`sortItem(newIndex=${newIndex}): itemToReplace → ${itemToReplace}`, { "event.action": "sortItem", "logrite.newIndex": newIndex });
        if (!itemToReplace) {
            return;
        }
        currentItem.index = newIndex;
        itemToReplace.index = curIndex;
		logger.debug(`sortItem(newIndex=${newIndex}): itemToReplace.index → ${itemToReplace.index}`, { "event.action": "sortItem", "logrite.newIndex": newIndex });
        this.currentIndex = newIndex;
        items.sort(this.sortColumns);
		logger.debug(`sortItem(newIndex=${newIndex}): this.currentIndex → ${this.currentIndex}`, { "event.action": "sortItem", "logrite.newIndex": newIndex });
        this.setState({ items }, this.callProps.bind(this));
		logger.warn(`<<< Exiting sortItem(newIndex=${newIndex})`, { "event.action": "sortItem", "logrite.newIndex": newIndex });
    }

    moveItem(items, id, index) {
		logger.error(`>>> Entering moveItem(items=${items},id=${id},index=${index})`, { "event.action": "moveItem", "logrite.items": items, "logrite.id": id, "logrite.index": index });
        let item = items.find(i => i.id == id);
        item.index = index;
		logger.error(`moveItem(items=${items},id=${id},index=${index}): item → ${item}`, { "event.action": "moveItem", "logrite.items": items, "logrite.id": id, "logrite.index": index });
    }

    updateIndexes(items, index) {
		logger.error(`>>> Entering updateIndexes(items=${items},index=${index})`, { "event.action": "updateIndexes", "logrite.items": items, "logrite.index": index });
        items.forEach((item) => {
            if (item.index >= index) {
                item.index = item.index + 1;
            }
        });
		logger.warn(`<<< Exiting updateIndexes(items=${items},index=${index})`, { "event.action": "updateIndexes", "logrite.items": items, "logrite.index": index });
        return items;
    }

    sortOnDrag(event, dropX, dropY) {
		logger.error(`>>> Entering sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY})`, { "event.action": "sortOnDrag", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
        let id = event.draggable._element.getAttribute("data-dnn-sortable-id");
		logger.error(`sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY}): id → ${id}`, { "event.action": "sortOnDrag", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
        if (this.dnnSortableRef.current)
        {
            const itemElement = this.dnnSortableRef.current.querySelectorAll(`[data-dnn-sortable-id="${id}"]`)[0];
			logger.error(`sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY}): itemElement → ${itemElement}`, { "event.action": "sortOnDrag", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
            itemElement.getAttribute("data-index");
            const sortableItems = document.getElementsByClassName("sortable-item");
			logger.error(`sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY}): sortableItems → ${sortableItems}`, { "event.action": "sortOnDrag", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
            let newIndex = this.getNewIndex(sortableItems, dropY);

            if (this.currentIndex == -1) {
                const currentIndex = itemElement.getAttribute("data-index");
				logger.debug(`sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY}): currentIndex → ${currentIndex}`, { "event.action": "sortOnDrag", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
                this.currentIndex = currentIndex;
            }

            const isSameIndex = newIndex == this.currentIndex;
            if (isSameIndex) {
                return;
            }
            this.sortItem(newIndex);
			logger.error(`sortOnDrag(event=${event},dropX=${dropX},dropY=${dropY}): newIndex → ${newIndex}`, { "event.action": "sortOnDrag", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
        }
    }

    onDrop(event, dropX, dropY) {
		logger.error(`>>> Entering onDrop(event=${event},dropX=${dropX},dropY=${dropY})`, { "event.action": "onDrop", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
        this.currentIndex = -1;
        this.unselectAll();
        this.setState({ isDraggingOver: false });
		logger.debug(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): this.currentIndex → ${this.currentIndex}`, { "event.action": "onDrop", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
        if (this.props.sortOnDrag) {
            return;
        }
        let id = event.draggable._element.getAttribute("data-dnn-sortable-id");
		logger.error(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): id → ${id}`, { "event.action": "onDrop", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
        if (this.dnnSortableRef.current) {
            const itemElement = this.dnnSortableRef.current.querySelectorAll(`[data-dnn-sortable-id="${id}"]`)[0];
			logger.error(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): itemElement → ${itemElement}`, { "event.action": "onDrop", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
            itemElement.classList.remove("sortable-selected");
            this.removePlaceholder();
            const sortableItems = document.getElementsByClassName("sortable-item");
			logger.error(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): sortableItems → ${sortableItems}`, { "event.action": "onDrop", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
			logger.error(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): currentIndex → ${currentIndex}`, { "event.action": "onDrop", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
			logger.debug(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): currentIndex → ${currentIndex}`, { "event.action": "onDrop", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
            let newIndex = this.getNewIndex(sortableItems, dropY);
			logger.error(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): newIndex → ${newIndex}`, { "event.action": "onDrop", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
            const currentIndex = event.draggable._element.getAttribute("data-index");
            const isSameIndex = currentIndex != null && (newIndex == currentIndex || newIndex - 1 == currentIndex);
			logger.error(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): isSameIndex → ${isSameIndex}`, { "event.action": "onDrop", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
            if (isSameIndex) {
                return;
            }

            let {items} = this.state;
            items = this.updateIndexes(items, newIndex);
			logger.error(`onDrop(event=${event},dropX=${dropX},dropY=${dropY}): items → ${items}`, { "event.action": "onDrop", "logrite.event": event, "logrite.dropX": dropX, "logrite.dropY": dropY });
            this.moveItem(items, id, newIndex);
            this.resetIndexes(items);
            this.setState({ items }, this.callProps.bind(this));
        }
    }

    callProps() {
		logger.error('>>> Entering callProps()', { "event.action": "callProps" });
        const items = this.state.items.map(item => item.item);
        this.props.onSort(items);
		logger.error(`callProps(): items → ${items}`, { "event.action": "callProps" });
    }


    resetIndexes(items = this.state.items) {
		logger.error(`>>> Entering resetIndexes(items=${items})`, { "event.action": "resetIndexes", "logrite.items": items });
        items.sort(this.sortColumns);
        items.forEach((item, index) => {
            item.index = index;
        });
		logger.warn(`<<< Exiting resetIndexes(items=${items})`, { "event.action": "resetIndexes", "logrite.items": items });
    }


    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
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

		logger.error(`render(): listItems → ${listItems}`, { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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