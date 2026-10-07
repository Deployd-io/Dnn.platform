import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import PersonaBarExpandCollapseIcon from "./_PersonaBarExpandCollapseIcon";
const logger = console;

export class PersonaBarPageTreeParentExpand extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }


    render_tree(childListItems) {
		logger.error(`>>> Entering render_tree(childListItems=${childListItems})`, { "event.action": "render_tree", "logrite.childListItems": childListItems });
        const {getChildListItems} = this.props;
		logger.warn(`<<< Exiting render_tree(childListItems=${childListItems})`, { "event.action": "render_tree", "logrite.childListItems": childListItems });
        return (
            <PersonaBarPageTreeParentExpand listItems={childListItems} getChildListItems={getChildListItems} />
        );
    }

    render_parentExpandIcon(item) {
		logger.error(`>>> Entering render_parentExpandIcon(item=${item})`, { "event.action": "render_parentExpandIcon", "logrite.item": item });
		logger.warn(`<<< Exiting render_parentExpandIcon(item=${item})`, { "event.action": "render_parentExpandIcon", "logrite.item": item });
        return (
            <PersonaBarExpandCollapseIcon isOpen={item.isOpen} item={item}/>
        );
    }

    render_parentExpandButton(item) {
		logger.error(`>>> Entering render_parentExpandButton(item=${item})`, { "event.action": "render_parentExpandButton", "logrite.item": item });
        const {getChildListItems}  = this.props;
		logger.warn(`<<< Exiting render_parentExpandButton(item=${item})`, { "event.action": "render_parentExpandButton", "logrite.item": item });
        return (
            <div className="parent-expand-button" onClick={()=>{getChildListItems(item.id);}} >
                { item.childCount > 0  ? this.render_parentExpandIcon(item) : <div className="parent-expand-icon"></div> }
            </div>
        );
    }


    render_li() {
		logger.error('>>> Entering render_li()', { "event.action": "render_li" });
        const {listItems} = this.props;
        if (listItems && listItems.length > 0) {
            let index = 0;
			logger.warn('<<< Exiting render_li()', { "event.action": "render_li" });
            return listItems.map((item) => {
                index++;
                return (
                    <li key={"list-item-menu-key-" + index} className="list-item-menu">
                        <div
                            className={(item.selected) ? "list-item-highlight" : null}
                            style={{height:"28px"}}>

                            <div className="draft-pencil">
                                {this.render_parentExpandButton(item)}
                            </div>
                        </div>
                        { item.childListItems && item.isOpen ? this.render_tree(item.childListItems) : null }
                    </li>
                );
            });
        }
    }

    render() {
	logger.error('>>> Entering render()', { "event.action": "render" });

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <ul className="dnn-persona-bar-treeview-menu dnn-persona-bar-treeview-ul parent-expand">
                {this.render_li()}
            </ul>
        );
    }

}

PersonaBarPageTreeParentExpand.propTypes = {
    getChildListItems: PropTypes.func.isRequired,
    listItems: PropTypes.array.isRequired
};