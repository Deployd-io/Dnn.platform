import React, { Component } from "react";
import PropTypes from "prop-types";
import {
    AddIcon,
    EditIcon,
    CardViewIcon,
    ListViewIcon,
    PreviewIcon,
    SettingsIcon,
    PageIcon,
    TrafficIcon,
    TemplateIcon,
    TrashIcon,
    UserIcon,
    ArrowDownIcon,
    ArrowRightIcon,
    ArrowUpIcon,
    LockClosedIcon,
    CheckMarkIcon,
    CrossOutIcon,
    CheckboxUncheckedIcon,
    CheckboxIcon
} from "../SvgIcons";

import "./style.less";
const logger = console;

export default class IconButton extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    getIcon() {
		logger.debug('>>> Entering getIcon()');
        const {props} = this;

        if (props.customIcon) {
            return props.customIcon;
        }

        switch (props.type.toLowerCase()) {
            case "add":
                return AddIcon;
            case "edit":
                return EditIcon;
            case "card":
                return CardViewIcon;
            case "list":
                return ListViewIcon;
            case "preview":
                return PreviewIcon;
            case "settings":
                return SettingsIcon;
            case "page":
                return PageIcon;
            case "traffic":
                return TrafficIcon;
            case "template":
                return TemplateIcon;
            case "trash":
                return TrashIcon;
            case "user":
                return UserIcon;
            case "arrow-down":
                return ArrowDownIcon;
            case "arrow-right":
                return ArrowRightIcon;
            case "arrow-up":
                return ArrowUpIcon;
            case "lock-closed":
                return LockClosedIcon;
            case "checked":
                return CheckMarkIcon;
            case "denied":
                return  CrossOutIcon;
            case "unchecked":
                return CheckboxUncheckedIcon;
            case "checkbox":
                return CheckboxIcon;
            default:
				logger.debug('<<< Exiting getIcon()');
                return null;
        }
    }

    getClassName() {
		logger.debug('>>> Entering getClassName()');
        const {props} = this;

        let name = "icon-button";
		logger.debug(`getClassName(): name → ${name}`);
        if (props.className) {
            name += " " + props.className;
        }

		logger.debug('<<< Exiting getClassName()');
        return name;
    }

    getStyle() {
		logger.debug('>>> Entering getStyle()');
        const {props} = this;

        let style = {};
        if (props.width && props.width > 0) {
            style["width"] = props.width + "px";
        }
        if (props.height && props.height > 0) {
            style["height"] = props.height + "px";
        }

		logger.debug('<<< Exiting getStyle()');
        return style;
    }

    onClick(event) {
		logger.debug(`>>> Entering onClick(event=${event})`);
        const {props} = this;

        event.preventDefault();

        props.onClick(event);
		logger.debug(`<<< Exiting onClick(event=${event})`);
    }
     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        const Icon = this.getIcon();
		logger.debug(`render(): Icon → ${Icon}`);

        if (typeof props.onClick === "function") {
            return <a href="#" className={this.getClassName()} style={this.getStyle()} onClick={this.onClick.bind(this)} aria-label={props.type}><Icon /></a>;
        } else {
			logger.debug('<<< Exiting render()');
            return <span className="icon-flat" style={this.getStyle()}><Icon /></span>;
        }
    }
}

IconButton.propTypes = {
    type: PropTypes.string,
    onClick: PropTypes.func,
    width: PropTypes.number,
    height: PropTypes.number,
    customIcon: PropTypes.string
};
