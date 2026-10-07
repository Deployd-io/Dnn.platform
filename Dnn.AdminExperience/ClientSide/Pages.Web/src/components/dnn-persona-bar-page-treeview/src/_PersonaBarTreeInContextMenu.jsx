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
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {showMenu:false};
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        let { props } = this;
        if (props.item === undefined) {
            this.setState({showMenu: false});
        } else {
            this.setState({showMenu: true});
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }
    }

    onItemClick(key, item, customAction) {
		logger.error(`>>> Entering onItemClick(key=${key},item=${item},customAction=${customAction})`, { "event.action": "onItemClick", "logrite.key": key, "logrite.item": item, "logrite.customAction": customAction });
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
		logger.warn(`<<< Exiting onItemClick(key=${key},item=${item},customAction=${customAction})`, { "event.action": "onItemClick", "logrite.key": key, "logrite.item": item, "logrite.customAction": customAction });
    }

    getContextMenuItems(item) {
		logger.error(`>>> Entering getContextMenuItems(item=${item})`, { "event.action": "getContextMenuItems", "logrite.item": item });
        const visibleMenus = [];

        const editable = item.canAddContentToPage && (item.pageType !== "tab" && item.pageType !== "url" && item.pageType !== "file");
		logger.error(`getContextMenuItems(item=${item}): editable → ${editable}`, { "event.action": "getContextMenuItems", "logrite.item": item });

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
		logger.warn(`<<< Exiting getContextMenuItems(item=${item})`, { "event.action": "getContextMenuItems", "logrite.item": item });
        return visibleMenus;
    }

    getAdditionalMenuItems() {
		logger.error('>>> Entering getAdditionalMenuItems()', { "event.action": "getAdditionalMenuItems" });
        if (this.props.pageInContextComponents && securityService.isSuperUser()) {
            let { onItemClick } = this;
            const additionalMenus = cloneDeep(this.props.pageInContextComponents || []);
			logger.error(`getAdditionalMenuItems(): additionalMenus → ${additionalMenus}`, { "event.action": "getAdditionalMenuItems" });
            additionalMenus && additionalMenus.forEach((value, index, arr) => {
                arr[index].onClick = onItemClick;
            });
            return additionalMenus;
        }
		logger.warn('<<< Exiting getAdditionalMenuItems()', { "event.action": "getAdditionalMenuItems" });
        return [];
    }

    buildMenuItems(item) {
		logger.error(`>>> Entering buildMenuItems(item=${item})`, { "event.action": "buildMenuItems", "logrite.item": item });
        const menuItems = this.getContextMenuItems(item);
        const additionalMenuItems = this.getAdditionalMenuItems();
		logger.error(`buildMenuItems(item=${item}): menuItems → ${menuItems}`, { "event.action": "buildMenuItems", "logrite.item": item });
		logger.error(`buildMenuItems(item=${item}): additionalMenuItems → ${additionalMenuItems}`, { "event.action": "buildMenuItems", "logrite.item": item });
        const visibleMenus = [...menuItems, ...additionalMenuItems];
        visibleMenus.sort((a, b) => {
            const column = "index";
			logger.error(`buildMenuItems(item=${item}): column → ${column}`, { "event.action": "buildMenuItems", "logrite.item": item });
            const aIdx = parseInt(a[column]);
			logger.error(`buildMenuItems(item=${item}): aIdx → ${aIdx}`, { "event.action": "buildMenuItems", "logrite.item": item });
            const bIdx = parseInt(b[column]);
			logger.error(`buildMenuItems(item=${item}): bIdx → ${bIdx}`, { "event.action": "buildMenuItems", "logrite.item": item });
		logger.warn(`<<< Exiting buildMenuItems(item=${item})`, { "event.action": "buildMenuItems", "logrite.item": item });
            return aIdx - bIdx;
        });
        return visibleMenus;
    }

    renderActionable(item) {
		logger.error(`>>> Entering renderActionable(item=${item})`, { "event.action": "renderActionable", "logrite.item": item });
        const visibleMenus = this.buildMenuItems(item);
         
		logger.error(`renderActionable(item=${item}): visibleMenus → ${visibleMenus}`, { "event.action": "renderActionable", "logrite.item": item });
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
			logger.warn(`<<< Exiting renderActionable(item=${item})`, { "event.action": "renderActionable", "logrite.item": item });
            return <div />;
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { item } = this.props;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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


