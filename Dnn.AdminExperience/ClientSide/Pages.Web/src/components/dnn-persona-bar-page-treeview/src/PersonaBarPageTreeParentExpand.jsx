import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import PersonaBarExpandCollapseIcon from "./_PersonaBarExpandCollapseIcon";
const logger = console;

export class PersonaBarPageTreeParentExpand extends Component {

    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug('<<< Exiting constructor()');
    }


    render_tree(childListItems) {
		logger.debug(`>>> Entering render_tree(childListItems=${childListItems})`);
        const {getChildListItems} = this.props;
		logger.debug(`<<< Exiting render_tree(childListItems=${childListItems})`);
        return (
            <PersonaBarPageTreeParentExpand listItems={childListItems} getChildListItems={getChildListItems} />
        );
    }

    render_parentExpandIcon(item) {
		logger.debug(`>>> Entering render_parentExpandIcon(item=${item})`);
		logger.debug(`<<< Exiting render_parentExpandIcon(item=${item})`);
        return (
            <PersonaBarExpandCollapseIcon isOpen={item.isOpen} item={item}/>
        );
    }

    render_parentExpandButton(item) {
		logger.debug(`>>> Entering render_parentExpandButton(item=${item})`);
        const {getChildListItems}  = this.props;
		logger.debug(`<<< Exiting render_parentExpandButton(item=${item})`);
        return (
            <div className="parent-expand-button" onClick={()=>{getChildListItems(item.id);}} >
                { item.childCount > 0  ? this.render_parentExpandIcon(item) : <div className="parent-expand-icon"></div> }
            </div>
        );
    }


    render_li() {
		logger.debug('>>> Entering render_li()');
        const {listItems} = this.props;
        if (listItems && listItems.length > 0) {
            let index = 0;
			logger.debug('<<< Exiting render_li()');
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
	logger.debug('>>> Entering render()');

		logger.debug('<<< Exiting render()');
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