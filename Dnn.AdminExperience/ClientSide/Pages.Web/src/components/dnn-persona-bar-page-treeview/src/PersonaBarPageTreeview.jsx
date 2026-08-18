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
		logger.debug('>>> Entering constructor()');
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug('<<< Exiting constructor()');
    }

    render_tree(item, childListItems) {
		logger.debug(`>>> Entering render_tree(item=${item},childListItems=${childListItems})`);
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

		logger.debug(`<<< Exiting render_tree(item=${item},childListItems=${childListItems})`);
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
		logger.debug(`>>> Entering renderDropZone(direction=${direction},item=${item})`);
        const { onMovePage, onDragEnd, draggedItem, dragOverItem, parentItem } = this.props;
        const onDragOver = (e, item, direction) => {
			logger.debug(`>>> Entering onDragOver(e=${e},item=${item},direction=${direction})`);
            e.preventDefault();
            const elm = document.getElementById(`dropzone-${item.name}-${item.id}-${direction}`);
			logger.debug(`onDragOver(e=${e},item=${item},direction=${direction}): elm → ${elm}`);
            (direction === "before") ? elm.classList.add("list-item-border-bottom") : elm.classList.add("list-item-border-top");
            const containerElm = document.getElementById(`list-item-container-${item.name}-${item.id}`);
			logger.debug(`onDragOver(e=${e},item=${item},direction=${direction}): containerElm → ${containerElm}`);
            item.onDragOverState = false;
            containerElm.className = this.getClassName(item);
			logger.debug(`<<< Exiting onDragOver(e=${e},item=${item},direction=${direction})`);
        };

		logger.debug(`renderDropZone(direction=${direction},item=${item}): item.onDragOverState → ${item.onDragOverState}`);
        const onDragLeave = (item, direction) => {
			logger.debug(`>>> Entering onDragLeave(item=${item},direction=${direction})`);
            const elm = document.getElementById(`dropzone-${item.name}-${item.id}-${direction}`);
            (direction === "before") ? elm.classList.remove("list-item-border-bottom") : elm.classList.remove("list-item-border-top");
			logger.debug(`onDragLeave(item=${item},direction=${direction}): elm → ${elm}`);
            const containerElm = document.getElementById(`list-item-container-${item.name}-${item.id}`);
			logger.debug(`onDragLeave(item=${item},direction=${direction}): containerElm → ${containerElm}`);
            item.onDragOverState = true;
            containerElm.className = this.getClassName(item);
			logger.debug(`<<< Exiting onDragLeave(item=${item},direction=${direction})`);
        };
        if (!utils.getIsSuperUser() && (parentItem === undefined || parentItem && !parentItem.canManagePage)) {
            return;
        }

        if (item.onDragOverState) {
			logger.debug(`<<< Exiting renderDropZone(direction=${direction},item=${item})`);
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
		logger.debug(`>>> Entering _checkItemDisabled(item=${item})`);
        if (item.includeInMenu) {
            return false;
        }
        if (item.status !== "Visible") {
		logger.debug(`<<< Exiting _checkItemDisabled(item=${item})`);
            return true;
        }  
        return false;
    }

    getClassName(item) {
		logger.debug(`>>> Entering getClassName(item=${item})`);
        const DRAG_HOVER = item.onDragOverState === true;
        const SELECTED = item.selected === true;
        const DISABLED = this._checkItemDisabled(item);
		logger.debug(`getClassName(item=${item}): DISABLED → ${DISABLED}`);
        
        if (DRAG_HOVER || SELECTED) {
            return "list-item-highlight list-item-dragover";
        } else if (DISABLED) {
		logger.debug(`<<< Exiting getClassName(item=${item})`);
            return "list-item-disabled";
        }

        return "";
    }

    renderLi() {
		logger.debug('>>> Entering renderLi()');
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
		logger.debug('<<< Exiting renderLi()');
        return listItems.map((item) => {
            const canManagePage = (e, item, fn) => {
                const whenCanManage = () => e ? fn(e, item) : fn(item);
                const whenCannotManage = () => this.props.setEmptyPageMessage(Localization.get("NoPermissionManagePage"));
                item.canManagePage ? whenCanManage() : whenCannotManage();
            };

            const onDragLeave = e => e.target.classList.remove("list-item-dragover");
            index++;

            const style = item.canManagePage ? { "whiteSpace": "nowrap", height: "28px", lineHeight: "35px" } : { height: "28px" };
			logger.debug(`renderLi(): style → ${style}`);
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
		logger.debug('>>> Entering render()');
        const { listItems } = this.props;
		logger.debug('<<< Exiting render()');
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