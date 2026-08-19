import React, { Component } from "react";
import { PropTypes } from "prop-types";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";
import { PersonaBarTreeInContextMenu } from "./_PersonaBarTreeInContextMenu";
import "./styles.less";
const logger = console;

export default class PersonaBarSelectionArrow extends Component {

    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            showInContext: false,
            showMenu: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    hasAtLeastOnePermission(item) {
		logger.debug(`>>> Entering hasAtLeastOnePermission(item=${item})`);
        switch (true) {
            case item.canViewPage:
            case item.canManagePage:
            case item.canEditPage:
            case item.canAddContentToPage:
            case item.canCopyPage:
                return true;
            default:
				logger.debug(`<<< Exiting hasAtLeastOnePermission(item=${item})`);
                return false;
        }
    }

    toggleInContext(li) {
		logger.debug(`>>> Entering toggleInContext(li=${li})`);
        const { _traverse } = this.props;
        let updateReduxStore, pageList = null;
        _traverse((item, list, updateStore) => {
            item.showInContextMenu = item.id === li.id ? !item.showInContextMenu : delete item.showInContextMenu;
            updateReduxStore = updateStore;
            pageList = list;
        });
        updateReduxStore(pageList);
		logger.debug(`toggleInContext(li=${li}): pageList → ${pageList}`);
		logger.debug(`<<< Exiting toggleInContext(li=${li})`);
    }

    onMouseEnter() {
		logger.debug('>>> Entering onMouseEnter()');
        this.setState({ showMenu: true });
		logger.debug('<<< Exiting onMouseEnter()');
    }

    onMouseLeave() {
		logger.debug('>>> Entering onMouseLeave()');
        this.setState({ showMenu: false });
		logger.debug('<<< Exiting onMouseLeave()');
    }

     
    renderMoreActions() {
		logger.debug('>>> Entering renderMoreActions()');
		logger.debug('<<< Exiting renderMoreActions()');
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
		logger.debug('>>> Entering render()');
        const { item } = this.props;
         
		logger.debug('<<< Exiting render()');
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