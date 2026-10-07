import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;

class GridHeader extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        this.state = {
        };
    }

    getLocalization(key) {
		logger.error(`>>> Entering getLocalization(key=${key})`, { "event.action": "getLocalization", "logrite.key": key });
        let localized = this.props.localization[key.replace(" ", "")];
		logger.error(`getLocalization(key=${key}): localized → ${localized}`, { "event.action": "getLocalization", "logrite.key": key });
        return localized || key;
    }

    getName(type) {
		logger.error(`>>> Entering getName(type=${type})`, { "event.action": "getName", "logrite.type": type });
        const key = `Permission${type.replace(" ", "")}`;
        const localized = this.props.localization[key];
		logger.error(`getName(type=${type}): key → ${key}`, { "event.action": "getName", "logrite.type": type });
		logger.warn(`<<< Exiting getName(type=${type})`, { "event.action": "getName", "logrite.type": type });
        return localized || type;
    }

    getDescription(type) {
		logger.error(`>>> Entering getDescription(type=${type})`, { "event.action": "getDescription", "logrite.type": type });
        const key = `Permission${type.replace(" ", "")}Description`;
        const localized = this.props.localization[key];
		logger.error(`getDescription(type=${type}): key → ${key}`, { "event.action": "getDescription", "logrite.type": type });
		logger.warn(`<<< Exiting getDescription(type=${type})`, { "event.action": "getDescription", "logrite.type": type });
        return localized || type;
    }

    renderHeader() {
		logger.error('>>> Entering renderHeader()', { "event.action": "renderHeader" });
        const {props} = this;
        const {roleColumnWidth, actionsWidth} = props;

		logger.warn('<<< Exiting renderHeader()', { "event.action": "renderHeader" });
        return <tr className="grid-header">
            <th columnSize={roleColumnWidth}><span title={props.type}>{this.getLocalization(props.type)}</span></th>
            {props.definitions.map((def) => {
                return <th key={def.permissionName}>
                    <span title={this.getDescription(def.permissionName)}>
                        {this.getName(def.permissionName)}
                    </span>
                </th>;
            }) }
            <th columnSize={actionsWidth} />
        </tr>;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            this.renderHeader()
        );
    }
}

GridHeader.propTypes = {
    localization: PropTypes.object,
    definitions: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"]),
    roleColumnWidth: PropTypes.number.isRequired,
    columnWidth: PropTypes.number.isRequired,
    actionsWidth: PropTypes.number.isRequired
};

GridHeader.DefaultProps = {
};

export default GridHeader;