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
		logger.debug('>>> Entering constructor()');
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug('<<< Exiting constructor()');
    }

    render_tree(childListItems) {
		logger.debug(`>>> Entering render_tree(childListItems=${childListItems})`);
		logger.debug(`<<< Exiting render_tree(childListItems=${childListItems})`);
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
		logger.debug(`>>> Entering render_parentExpandIcon(item=${item})`);
		logger.debug(`<<< Exiting render_parentExpandIcon(item=${item})`);
        return (
            <PersonaBarExpandCollapseIcon isOpen={item.isOpen} item={item} />
        );
    }

    render_parentExpandButton(item) {
		logger.debug(`>>> Entering render_parentExpandButton(item=${item})`);
		logger.debug(`<<< Exiting render_parentExpandButton(item=${item})`);
        return (
            <div className="parent-expand-button" style={{ visibility: "hidden" }}>
                {item.childCount > 0 ? this.render_parentExpandIcon(item) : <div className="parent-expand-icon"></div>}
            </div>
        );
    }


    render_li() {
		logger.debug('>>> Entering render_li()');
        const { listItems, _traverse } = this.props;

		logger.debug(`render_li(): _traverse → ${_traverse}`);
		logger.debug('<<< Exiting render_li()');
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
	logger.debug('>>> Entering render()');

		logger.debug('<<< Exiting render()');
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