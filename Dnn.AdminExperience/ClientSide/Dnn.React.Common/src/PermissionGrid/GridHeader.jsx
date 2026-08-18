import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;

class GridHeader extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);

        this.state = {
        };
    }

    getLocalization(key) {
        let localized = this.props.localization[key.replace(" ", "")];
        return localized || key;
    }

    getName(type) {
		logger.debug(`>>> Entering getName(type=${type})`);
        const key = `Permission${type.replace(" ", "")}`;
        const localized = this.props.localization[key];
		logger.warn(`getName(type=${type}): key → ${key}`);
		logger.debug(`<<< Exiting getName(type=${type})`);
        return localized || type;
    }

    getDescription(type) {
		logger.debug(`>>> Entering getDescription(type=${type})`);
        const key = `Permission${type.replace(" ", "")}Description`;
        const localized = this.props.localization[key];
		logger.warn(`getDescription(type=${type}): key → ${key}`);
		logger.debug(`<<< Exiting getDescription(type=${type})`);
        return localized || type;
    }

    renderHeader() {
		logger.debug('>>> Entering renderHeader()');
        const {props} = this;
        const {roleColumnWidth, actionsWidth} = props;

		logger.debug('<<< Exiting renderHeader()');
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
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
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