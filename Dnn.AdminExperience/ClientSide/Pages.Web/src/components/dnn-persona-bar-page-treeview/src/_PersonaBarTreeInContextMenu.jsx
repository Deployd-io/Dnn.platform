import React, { Component } from "react";
import { PropTypes } from "prop-types";
import Localization from "localization";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";
import Menu from "./InContextMenu/Menu";
import MenuItem from "./InContextMenu/MenuItem";
import cloneDeep from "lodash/cloneDeep";
import securityService from "../../../services/securityService";

import "./styles.less";
const logger = console;

export class PersonaBarTreeInContextMenu extends Component {

    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {showMenu:false};
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        let { props } = this;
        if (props.item === undefined) {
            this.setState({showMenu: false});
        } else {
            this.setState({showMenu: true});
			logger.debug('<<< Exiting componentDidMount()');
        }
    }

    onItemClick(key, item, customAction) {
		logger.debug(`>>> Entering onItemClick(key=${key},item=${item},customAction=${customAction})`);
        switch (key) {
            case "Add":
                this.props.onAddPage(item);
                this.props.onClose();
                break;
            case "View":
                this.props.onViewPage(item);
                this.props.onClose();
                break;
            case "Edit":
                this.props.onViewEditPage(item);
                this.props.onClose();
                break;
            case "Duplicate":
                this.props.onDuplicatePage(item);
                this.props.onClose();
                break;
            default:
                if (typeof customAction === "function")
                    this.props.CallCustomAction(customAction);
                this.props.onClose();
                break;
        }
		logger.debug(`<<< Exiting onItemClick(key=${key},item=${item},customAction=${customAction})`);
    }

    getContextMenuItems(item) {
		logger.debug(`>>> Entering getContextMenuItems(item=${item})`);
        const visibleMenus = [];

        const editable = item.canAddContentToPage && (item.pageType !== "tab" && item.pageType !== "url" && item.pageType !== "file");
		logger.debug(`getContextMenuItems(item=${item}): editable → ${editable}`);

        if (item.canAddPage) {
            visibleMenus.push({
                key: "Add",
                title: Localization.get("AddPage"),
                index: 10,
                icon: SvgIcons.TreeAddPage,
                onClick: this.onItemClick
            });
        }
        if (item.canViewPage) {
            visibleMenus.push({
                key: "View",
                title: Localization.get("View"),
                index: 20,
                icon: SvgIcons.EyeIcon,
                onClick: this.onItemClick
            });
        }

        if (editable) {
            visibleMenus.push({
                key: "Edit",
                title: Localization.get("Edit"),
                index: 30,
                icon: SvgIcons.TreeEdit,
                onClick: this.onItemClick
            });
        }

        if (item.canCopyPage) {
            visibleMenus.push({
                key: "Duplicate",
                title: Localization.get("Duplicate"),
                index: 40,
                icon: SvgIcons.TreeCopy,
                onClick: this.onItemClick
            });
        }
		logger.debug(`<<< Exiting getContextMenuItems(item=${item})`);
        return visibleMenus;
    }

    getAdditionalMenuItems() {
		logger.debug('>>> Entering getAdditionalMenuItems()');
        if (this.props.pageInContextComponents && securityService.isSuperUser()) {
            let { onItemClick } = this;
            const additionalMenus = cloneDeep(this.props.pageInContextComponents || []);
			logger.debug(`getAdditionalMenuItems(): additionalMenus → ${additionalMenus}`);
            additionalMenus && additionalMenus.forEach((value, index, arr) => {
                arr[index].onClick = onItemClick;
            });
		logger.debug('<<< Exiting getAdditionalMenuItems()');
            return additionalMenus;
        }
        return [];
    }

    buildMenuItems(item) {
		logger.debug(`>>> Entering buildMenuItems(item=${item})`);
        const menuItems = this.getContextMenuItems(item);
        const additionalMenuItems = this.getAdditionalMenuItems();
		logger.debug(`buildMenuItems(item=${item}): menuItems → ${menuItems}`);
		logger.debug(`buildMenuItems(item=${item}): additionalMenuItems → ${additionalMenuItems}`);
        const visibleMenus = [...menuItems, ...additionalMenuItems];
        visibleMenus.sort((a, b) => {
            const column = "index";
			logger.debug(`buildMenuItems(item=${item}): column → ${column}`);
            const aIdx = parseInt(a[column]);
			logger.debug(`buildMenuItems(item=${item}): aIdx → ${aIdx}`);
            const bIdx = parseInt(b[column]);
			logger.debug(`buildMenuItems(item=${item}): bIdx → ${bIdx}`);
		logger.debug(`<<< Exiting buildMenuItems(item=${item})`);
            return aIdx - bIdx;
        });
        return visibleMenus;
    }

    renderActionable(item) {
		logger.debug(`>>> Entering renderActionable(item=${item})`);
        const visibleMenus = this.buildMenuItems(item);
         
		logger.debug(`renderActionable(item=${item}): visibleMenus → ${visibleMenus}`);
        if (this.state.showMenu && visibleMenus.length) {
            return (<Menu>
                {
                    visibleMenus.map(menu => {
                        let MenuIcon = menu.icon;
                        return <MenuItem key={menu.key} onMenuAction={menu.onClick.bind(this, menu.key, item, menu.OnClickAction)}>
                            <div className="icon"><MenuIcon /></div>
                            <div className="label">{menu.title}</div>
                        </MenuItem>;
                    })
                }
            </Menu>);
        }
        else {
			logger.debug(`<<< Exiting renderActionable(item=${item})`);
            return <div />;
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const { item } = this.props;
		logger.debug('<<< Exiting render()');
        return (
            <span>
                {this.renderActionable(item)}
            </span>
        );
    }

}

PersonaBarTreeInContextMenu.propTypes = {
    onViewPage: PropTypes.func.isRequired,
    onViewEditPage: PropTypes.func.isRequired,
    onAddPage: PropTypes.func.isRequired,
    onDuplicatePage: PropTypes.func.isRequired,
    CallCustomAction: PropTypes.func.isRequired,
    item: PropTypes.object.isRequired,
    pageInContextComponents: PropTypes.array.isRequired,
    onClose: PropTypes.func.isRequired
};


