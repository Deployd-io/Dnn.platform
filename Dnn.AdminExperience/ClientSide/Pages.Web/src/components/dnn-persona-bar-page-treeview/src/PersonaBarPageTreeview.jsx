import React, { Component } from "react";
import { PropTypes } from "prop-types";
import utils from "utils";
import "./styles.less";
import { SingleLineInput } from "@dnnsoftware/dnn-react-common";
import PersonaBarPageIcon from "./_PersonaBarPageIcon";
const logger = console;

export class PersonaBarPageTreeview extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }

    render_tree(item, childListItems) {
		logger.error(`>>> Entering render_tree(item=${item},childListItems=${childListItems})`, { "event.action": "render_tree", "logrite.item": item, "logrite.childListItems": childListItems });
        const {
            draggedItem,
            droppedItem,
            dragOverItem,
            getChildListItems,
            onSelection,
            onNoPermissionSelection,
            onDrop,
            onDrag,
            onDragStart,
            onDragEnter,
            onDragOver,
            onDragLeave,
            onDragEnd,
            onMovePage,
            setEmptyPageMessage,
            Localization
        } = this.props;

		logger.warn(`<<< Exiting render_tree(item=${item},childListItems=${childListItems})`, { "event.action": "render_tree", "logrite.item": item, "logrite.childListItems": childListItems });
        return (
            <PersonaBarPageTreeview
                draggedItem={draggedItem}
                droppedItem={droppedItem}
                dragOverItem={dragOverItem}
                getChildListItems={getChildListItems}
                listItems={childListItems}
                onSelection={onSelection}
                onNoPermissionSelection={onNoPermissionSelection}
                onDrop={onDrop}
                onDrag={onDrag}
                onDragStart={onDragStart}
                onDragEnter={onDragEnter}
                onDragOver={onDragOver}
                onDragLeave={onDragLeave}
                onDragEnd={onDragEnd}
                onMovePage={onMovePage}
                setEmptyPageMessage={setEmptyPageMessage}
                Localization={Localization}
                parentItem={item}
            />
        );
    }

    renderDropZone(direction, item) {
		logger.error(`>>> Entering renderDropZone(direction=${direction},item=${item})`, { "event.action": "renderDropZone", "logrite.direction": direction, "logrite.item": item });
		logger.debug(`renderDropZone(direction=${direction},item=${item}): item.onDragOverState → ${item.onDragOverState}`, { "event.action": "renderDropZone", "logrite.direction": direction, "logrite.item": item });
        const { onMovePage, onDragEnd, draggedItem, dragOverItem, parentItem } = this.props;
        const onDragOver = (e, item, direction) => {
			logger.error(`>>> Entering onDragOver(e=${e},item=${item},direction=${direction})`, { "event.action": "onDragOver", "logrite.e": e, "logrite.item": item, "logrite.direction": direction });
            e.preventDefault();
            const elm = document.getElementById(`dropzone-${item.name}-${item.id}-${direction}`);
			logger.error(`onDragOver(e=${e},item=${item},direction=${direction}): elm → ${elm}`, { "event.action": "onDragOver", "logrite.e": e, "logrite.item": item, "logrite.direction": direction });
            (direction === "before") ? elm.classList.add("list-item-border-bottom") : elm.classList.add("list-item-border-top");
            const containerElm = document.getElementById(`list-item-container-${item.name}-${item.id}`);
			logger.error(`onDragOver(e=${e},item=${item},direction=${direction}): containerElm → ${containerElm}`, { "event.action": "onDragOver", "logrite.e": e, "logrite.item": item, "logrite.direction": direction });
            item.onDragOverState = false;
            containerElm.className = this.getClassName(item);
			logger.warn(`<<< Exiting onDragOver(e=${e},item=${item},direction=${direction})`, { "event.action": "onDragOver", "logrite.e": e, "logrite.item": item, "logrite.direction": direction });
        };

        const onDragLeave = (item, direction) => {
			logger.error(`>>> Entering onDragLeave(item=${item},direction=${direction})`, { "event.action": "onDragLeave", "logrite.item": item, "logrite.direction": direction });
            const elm = document.getElementById(`dropzone-${item.name}-${item.id}-${direction}`);
            (direction === "before") ? elm.classList.remove("list-item-border-bottom") : elm.classList.remove("list-item-border-top");
			logger.error(`onDragLeave(item=${item},direction=${direction}): elm → ${elm}`, { "event.action": "onDragLeave", "logrite.item": item, "logrite.direction": direction });
            const containerElm = document.getElementById(`list-item-container-${item.name}-${item.id}`);
			logger.error(`onDragLeave(item=${item},direction=${direction}): containerElm → ${containerElm}`, { "event.action": "onDragLeave", "logrite.item": item, "logrite.direction": direction });
            item.onDragOverState = true;
            containerElm.className = this.getClassName(item);
			logger.warn(`<<< Exiting onDragLeave(item=${item},direction=${direction})`, { "event.action": "onDragLeave", "logrite.item": item, "logrite.direction": direction });
        };
        if (!utils.getIsSuperUser() && (parentItem === undefined || parentItem && !parentItem.canManagePage)) {
            return;
        }

        if (item.onDragOverState) {
			logger.warn(`<<< Exiting renderDropZone(direction=${direction},item=${item})`, { "event.action": "renderDropZone", "logrite.direction": direction, "logrite.item": item });
            return (
                <div
                    id={`dropzone-${item.name}-${item.id}-${direction}`}
                    className={(item.id !== draggedItem.id) ? "dropZoneArea" : ""}
                    style={(item.id === draggedItem.id) ? { display: "none" } : {}}
                    draggable="false"
                    onDragOver={(e) => onDragOver(e, item, direction)}
                    onDragLeave={() => onDragLeave(item, direction)}
                    onDragEnd={() => onDragEnd(item, direction)}
                    onDrop={(e) => onMovePage({ e: e, Action: direction, PageId: draggedItem.id, ParentId: draggedItem.parentId, RelatedPageId: dragOverItem.id, RelatedPageParentId: dragOverItem.parentId })} >
                </div>
            );
        }
    }

    _checkItemDisabled(item) {
		logger.error(`>>> Entering _checkItemDisabled(item=${item})`, { "event.action": "_checkItemDisabled", "logrite.item": item });
        if (item.includeInMenu) {
            return false;
        }
        if (item.status !== "Visible") {
            return true;
        }  
		logger.warn(`<<< Exiting _checkItemDisabled(item=${item})`, { "event.action": "_checkItemDisabled", "logrite.item": item });
        return false;
    }

    getClassName(item) {
		logger.error(`>>> Entering getClassName(item=${item})`, { "event.action": "getClassName", "logrite.item": item });
        const DRAG_HOVER = item.onDragOverState === true;
        const SELECTED = item.selected === true;
        const DISABLED = this._checkItemDisabled(item);
		logger.error(`getClassName(item=${item}): DISABLED → ${DISABLED}`, { "event.action": "getClassName", "logrite.item": item });
        
        if (DRAG_HOVER || SELECTED) {
            return "list-item-highlight list-item-dragover";
        } else if (DISABLED) {
            return "list-item-disabled";
        }

		logger.warn(`<<< Exiting getClassName(item=${item})`, { "event.action": "getClassName", "logrite.item": item });
        return "";
    }

    renderLi() {
		logger.error('>>> Entering renderLi()', { "event.action": "renderLi" });
        const {
            listItems,
            onSelection,
            onNoPermissionSelection,
            onDrop,
            onDrag,
            onDragStart,
            onDragEnter,
            onDragOver,
            onDragEnd,
            draggedItem,
            Localization, 
            selectedPageDirty } = this.props;

        let index = 0;
        let total = listItems.length;
		logger.warn('<<< Exiting renderLi()', { "event.action": "renderLi" });
        return listItems.map((item) => {
            const canManagePage = (e, item, fn) => {
                const whenCanManage = () => e ? fn(e, item) : fn(item);
                const whenCannotManage = () => this.props.setEmptyPageMessage(Localization.get("NoPermissionManagePage"));
                item.canManagePage ? whenCanManage() : whenCannotManage();
            };

            const onDragLeave = e => e.target.classList.remove("list-item-dragover");
            index++;

            const style = item.canManagePage ? { "whiteSpace": "nowrap", height: "28px", lineHeight: "35px" } : { height: "28px" };
			logger.error(`renderLi(): style → ${style}`, { "event.action": "renderLi" });
            const itemNameHidden = item.status === "Hidden" ? "item-name-hidden" : "";
            return (
                <li key={"list-item-key-" + index} id={`list-item-${item.name}-${item.id}`} title={item.name}>
                    <div className={item.onDragOverState && item.id !== draggedItem.id ? "dropZoneActive" : "dropZoneInactive"} >
                        {this.renderDropZone("before", item)}
                        <div
                            style={style}
                            id={`list-item-title-${item.name}-${item.id}`}
                            className="dragged-proxy"
                            draggable={item.canManagePage ? "true" : "false"}
                            onDrop={(e) => { canManagePage(e, item, onDrop); }}
                            onDrag={(e) => { canManagePage(e, item, onDrag); }}
                            onDragOver={(e) => { canManagePage(e, item, onDragOver); }}
                            onDragEnter={(e) => canManagePage(e, item, onDragEnter)}
                            onDragStart={(e) => { canManagePage(e, item, onDragStart); }}
                            onDragLeave={(e) => canManagePage(e, item, onDragLeave)}
                            onDragEnd={(e) => { canManagePage(e, item, onDragEnd); }}
                            onClick={() => { item.canManagePage ? onSelection(item) : onNoPermissionSelection(item); }}
                        >
                        </div>

                        <div id={`list-item-container-${item.name}-${item.id}`} style={style} className={this.getClassName(item)}>
                            <PersonaBarPageIcon iconType={item.pageType} selected={item.selected || false} />
                            <span
                                className={`item-name ${itemNameHidden}`}
                                onClick={() => item.canManagePage ? onSelection(item) : onNoPermissionSelection(item)}
                            >
                                { (item.tabId === 0) || (item.selected && selectedPageDirty) ? 
                                    (
                                        <SingleLineInput 
                                            style={{ marginBottom: "0px", width:"80%", height:"100%"}}
                                            onChange={() => void(0)}
                                            value={item.name}/>
                                    ):
                                    item.name
                                }
                            </span>
                        </div>
                        {((item.childListItems && !item.isOpen) || !item.childListItems) && index === total && this.renderDropZone("after", item)}
                    </div>
                    {item.childListItems && item.isOpen ? this.render_tree(item, item.childListItems) : null}
                </li>
            );
        });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { listItems } = this.props;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <ul style={!listItems.length ? { padding: "0px", height: "0px" } : null}>
                {this.renderLi()}
            </ul>
        );
    }

}

PersonaBarPageTreeview.propTypes = {
    draggedItem: PropTypes.object,
    droppedItem: PropTypes.object,
    dragOverItem: PropTypes.object,
    onDrop: PropTypes.func.isRequired,
    onDrag: PropTypes.func.isRequired,
    onDragOver: PropTypes.func.isRequired,
    onDragLeave: PropTypes.func.isRequired,
    onDragEnter: PropTypes.func.isRequired,
    onDragStart: PropTypes.func.isRequired,
    onDragEnd: PropTypes.func.isRequired,
    onMovePage: PropTypes.func.isRequired,
    listItems: PropTypes.array.isRequired,
    getChildListItems: PropTypes.func.isRequired,
    onSelection: PropTypes.func.isRequired,
    onNoPermissionSelection: PropTypes.func.isRequired,
    icons: PropTypes.object,
    onSelect: PropTypes.func,
    setEmptyPageMessage: PropTypes.func.isRequired,
    Localization: PropTypes.object.isRequired,
    parentItem: PropTypes.object,
    selectedPageDirty: PropTypes.bool
};