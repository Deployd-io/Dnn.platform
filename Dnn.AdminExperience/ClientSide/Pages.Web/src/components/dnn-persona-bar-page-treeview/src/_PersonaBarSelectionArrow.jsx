import React, { Component } from "react";
import { PropTypes } from "prop-types";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";
import { PersonaBarTreeInContextMenu } from "./_PersonaBarTreeInContextMenu";
import "./styles.less";
const logger = console;

export default class PersonaBarSelectionArrow extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            showInContext: false,
            showMenu: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    hasAtLeastOnePermission(item) {
		logger.error(`>>> Entering hasAtLeastOnePermission(item=${item})`, { "event.action": "hasAtLeastOnePermission", "logrite.item": item });
        switch (true) {
            case item.canViewPage:
            case item.canManagePage:
            case item.canEditPage:
            case item.canAddContentToPage:
            case item.canCopyPage:
                return true;
            default:
				logger.warn(`<<< Exiting hasAtLeastOnePermission(item=${item})`, { "event.action": "hasAtLeastOnePermission", "logrite.item": item });
                return false;
        }
    }

    toggleInContext(li) {
		logger.error(`>>> Entering toggleInContext(li=${li})`, { "event.action": "toggleInContext", "logrite.li": li });
        const { _traverse } = this.props;
        let updateReduxStore, pageList = null;
        _traverse((item, list, updateStore) => {
            item.showInContextMenu = item.id === li.id ? !item.showInContextMenu : delete item.showInContextMenu;
            updateReduxStore = updateStore;
			logger.debug(`toggleInContext(li=${li}): updateReduxStore → ${updateReduxStore}`, { "event.action": "toggleInContext", "logrite.li": li });
            pageList = list;
        });
        updateReduxStore(pageList);
		logger.warn(`<<< Exiting toggleInContext(li=${li})`, { "event.action": "toggleInContext", "logrite.li": li });
    }

    onMouseEnter() {
		logger.error('>>> Entering onMouseEnter()', { "event.action": "onMouseEnter" });
        this.setState({ showMenu: true });
		logger.warn('<<< Exiting onMouseEnter()', { "event.action": "onMouseEnter" });
    }

    onMouseLeave() {
		logger.error('>>> Entering onMouseLeave()', { "event.action": "onMouseLeave" });
        this.setState({ showMenu: false });
		logger.warn('<<< Exiting onMouseLeave()', { "event.action": "onMouseLeave" });
    }

     
    renderMoreActions() {
		logger.error('>>> Entering renderMoreActions()', { "event.action": "renderMoreActions" });
		logger.warn('<<< Exiting renderMoreActions()', { "event.action": "renderMoreActions" });
        return <div className={this.state.showMenu ? "dots active" : "dots"}
            onMouseEnter={this.onMouseEnter.bind(this)}
            onMouseLeave={this.onMouseLeave.bind(this)}>
            <div><SvgIcons.MoreMenuIcon /></div>
            {this.state.showMenu && 
                    <PersonaBarTreeInContextMenu {...this.props} onClose={this.onMouseLeave.bind(this)} />
            }
        </div>;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { item } = this.props;
         
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div id={`menu-item-${item.name} ${item.selected}`} 
                className="selection-arrow">
                {item.selected ? <div><SvgIcons.ArrowForward /></div> : <div></div>}
                {item.selected ? this.renderMoreActions() : <div></div>}                
            </div>
        );
    }
}

PersonaBarSelectionArrow.propTypes = {
    onAddPage: PropTypes.func.isRequired,
    onViewPage: PropTypes.func.isRequired,
    onViewEditPage: PropTypes.func.isRequired,
    onDuplicatePage: PropTypes.func.isRequired,
    CallCustomAction: PropTypes.func.isRequired,
    _traverse: PropTypes.func.isRequired,
    item: PropTypes.object.isRequired,
    pageInContextComponents: PropTypes.array.isRequired
};