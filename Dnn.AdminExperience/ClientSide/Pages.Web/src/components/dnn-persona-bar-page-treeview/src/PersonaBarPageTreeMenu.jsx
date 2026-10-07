import React, { Component } from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import PersonaBarSelectionArrow from "./_PersonaBarSelectionArrow";
import PersonaBarExpandCollapseIcon from "./_PersonaBarExpandCollapseIcon";
import PersonaBarDraftPencilIcon from "./_PersonaBarDraftPencilIcon";
const logger = console;

export class PersonaBarPageTreeMenu extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }

    render_tree(childListItems) {
		logger.error(`>>> Entering render_tree(childListItems=${childListItems})`, { "event.action": "render_tree", "logrite.childListItems": childListItems });
		logger.warn(`<<< Exiting render_tree(childListItems=${childListItems})`, { "event.action": "render_tree", "logrite.childListItems": childListItems });
        return (
            <PersonaBarPageTreeMenu
                CallCustomAction={this.props.CallCustomAction}
                onAddPage={this.props.onAddPage}
                onViewPage={this.props.onViewPage}
                onViewEditPage={this.props.onViewEditPage}
                onDuplicatePage={this.props.onDuplicatePage}
                listItems={childListItems}
                _traverse={this.props._traverse}
                pageInContextComponents={this.props.pageInContextComponents}
            />
        );
    }


    render_parentExpandIcon(item) {
		logger.error(`>>> Entering render_parentExpandIcon(item=${item})`, { "event.action": "render_parentExpandIcon", "logrite.item": item });
		logger.warn(`<<< Exiting render_parentExpandIcon(item=${item})`, { "event.action": "render_parentExpandIcon", "logrite.item": item });
        return (
            <PersonaBarExpandCollapseIcon isOpen={item.isOpen} item={item} />
        );
    }

    render_parentExpandButton(item) {
		logger.error(`>>> Entering render_parentExpandButton(item=${item})`, { "event.action": "render_parentExpandButton", "logrite.item": item });
		logger.warn(`<<< Exiting render_parentExpandButton(item=${item})`, { "event.action": "render_parentExpandButton", "logrite.item": item });
        return (
            <div className="parent-expand-button" style={{ visibility: "hidden" }}>
                {item.childCount > 0 ? this.render_parentExpandIcon(item) : <div className="parent-expand-icon"></div>}
            </div>
        );
    }


    render_li() {
		logger.error('>>> Entering render_li()', { "event.action": "render_li" });
        const { listItems, _traverse } = this.props;

		logger.warn('<<< Exiting render_li()', { "event.action": "render_li" });
        return listItems.map((item, index) => {
            return (
                <li key={"pageTreeListItem" + index} className="list-item-menu-right">
                    <div
                        className={(item.selected) ? "list-item-highlight" : null}
                        style={{ height: "28px", lineHeight: "35px" }}>
                        <div className="draft-pencil">
                            <PersonaBarDraftPencilIcon display={item.hasUnpublishedChanges} />
                            {item.canViewPage && <PersonaBarSelectionArrow
                                CallCustomAction={this.props.CallCustomAction}
                                onAddPage={this.props.onAddPage}
                                onViewPage={this.props.onViewPage}
                                onViewEditPage={this.props.onViewEditPage}
                                onDuplicatePage={this.props.onDuplicatePage}
                                item={item}
                                pageInContextComponents={this.props.pageInContextComponents}
                                _traverse={_traverse} />
                            }
                        </div>
                    </div>
                    {item.childListItems && item.isOpen ? this.render_tree(item.childListItems) : null}
                </li>
            );
        });
    }

    render() {
	logger.error('>>> Entering render()', { "event.action": "render" });

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <ul className="dnn-persona-bar-treeview-menu dnn-persona-bar-treeview-ul">
                {this.render_li()}
            </ul>
        );
    }

}

PersonaBarPageTreeMenu.propTypes = {
    CallCustomAction: PropTypes.func.isRequired,
    onAddPage: PropTypes.func.isRequired,
    onViewPage: PropTypes.func.isRequired,
    onViewEditPage: PropTypes.func.isRequired,
    onDuplicatePage: PropTypes.func.isRequired,
    _traverse: PropTypes.func.isRequired,
    listItems: PropTypes.array.isRequired,
    pageInContextComponents: PropTypes.array.isRequired
};