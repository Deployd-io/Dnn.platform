import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "../GridCell";
import GridCaption from "./GridCaption";
import GridHeader from "./GridHeader";
import GridRow from "./GridRow";
const logger = console;

class Grid extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        this.state = {
        };
    }

    onPermissionChange(permission) {
		logger.error(`>>> Entering onPermissionChange(permission=${permission})`, { "event.action": "onPermissionChange", "logrite.permission": permission });
        const {props, state} = this;

        let permissions = props.permissions.map(function (p) {
            switch (props.type) {
                case "role":
                    if (p.roleId === permission.roleId) {
                        return permission;
                    }

                    return p;
                case "user":
                    if (p.userId === permission.userId) {
                        return permission;
                    }

                    return p;
            }
        });

		logger.error(`onPermissionChange(permission=${permission}): permissions → ${permissions}`, { "event.action": "onPermissionChange", "logrite.permission": permission });
        if (typeof props.onChange === "function") {
            props.onChange(permissions);
        }
		logger.warn(`<<< Exiting onPermissionChange(permission=${permission})`, { "event.action": "onPermissionChange", "logrite.permission": permission });
    }

    onPermissionDeleted(permission) {
		logger.error(`>>> Entering onPermissionDeleted(permission=${permission})`, { "event.action": "onPermissionDeleted", "logrite.permission": permission });
        const {props, state} = this;

        let permissions = props.permissions.filter(function (p) {
            switch (props.type) {
                case "role":
                    return p.roleId !== permission.roleId;
                case "user":
                    return p.userId !== permission.userId;
            }
        });

		logger.error(`onPermissionDeleted(permission=${permission}): permissions → ${permissions}`, { "event.action": "onPermissionDeleted", "logrite.permission": permission });
        if (typeof props.onChange === "function") {
            props.onChange(permissions);
        }
		logger.warn(`<<< Exiting onPermissionDeleted(permission=${permission})`, { "event.action": "onPermissionDeleted", "logrite.permission": permission });
    }

    onSuggestion(value) {
		logger.error(`>>> Entering onSuggestion(value=${value})`, { "event.action": "onSuggestion", "logrite.value": value });
        const {props, state} = this;

        let permission = props.permissions.filter(function (p) {
            if (props.type === "role") {
                return p.roleId === value.value;
            } else {
                return p.userId === value.value;
            }
        });

		logger.error(`onSuggestion(value=${value}): permission → ${permission}`, { "event.action": "onSuggestion", "logrite.value": value });
        if (!permission.length && typeof props.onAddPermission === "function") {
            let newPermission = props.type === "role" ? {
                roleId: value.value,
                roleName: value.label,
                locked: false,
                default: false,
                permissions: []
            } : {
                userId: value.value,
                displayName: value.label,
                locked: false,
                default: false,
                permissions: []
            };
            props.onAddPermission(newPermission);
			logger.error(`onSuggestion(value=${value}): newPermission → ${newPermission}`, { "event.action": "onSuggestion", "logrite.value": value });
        }
    }

    renderGrid() {
		logger.error('>>> Entering renderGrid()', { "event.action": "renderGrid" });
        const {props, state} = this;

        const totalWidth = 100;
        const defaultColumnWith = 10;
        let roleColumnWidth = 20;
        const minimumActionColumnWidth = 8;

        let columnWidth = defaultColumnWith;
		logger.error(`renderGrid(): columnWidth → ${columnWidth}`, { "event.action": "renderGrid" });
        let actionsWidth;
        const tooManyColumns = totalWidth < roleColumnWidth + props.definitions.length * columnWidth;
        if (tooManyColumns) {
            roleColumnWidth = 13;
            columnWidth = (totalWidth - roleColumnWidth - minimumActionColumnWidth) / props.definitions.length;
            actionsWidth = totalWidth - roleColumnWidth - props.definitions.length * columnWidth;
        } else {
            actionsWidth = totalWidth - roleColumnWidth - props.definitions.length * columnWidth;
        }

        let self = this;
		logger.warn('<<< Exiting renderGrid()', { "event.action": "renderGrid" });
        return (
            <GridCell style={{"display":"table"}} className={props.type + "-permissions-grid"}>
                <GridCaption service={props.service} localization={props.localization} type={props.type} onSuggestion={this.onSuggestion.bind(this) } />
                <table>
                    <GridHeader type={props.type} definitions={props.definitions}
                        roleColumnWidth={roleColumnWidth}
                        columnWidth={columnWidth}
                        actionsWidth={actionsWidth}
                        localization={props.localization}  />
                    {props.permissions.map((permission) => {
                        return (
                            <GridRow
                                key={permission}
                                type={props.type}
                                definitions={props.definitions}
                                permission={permission}
                                onChange={self.onPermissionChange.bind(self) }
                                onDeletePermisson={self.onPermissionDeleted.bind(self) }
                                roleColumnWidth={roleColumnWidth}
                                columnWidth={columnWidth}
                                actionsWidth={actionsWidth} />);
                    }) }
                    {props.permissions.length === 0 && <GridCell className="empty-row">{props.type === "role" ? props.localization.emptyRole : props.localization.emptyUser}</GridCell>}
                </table>
            </GridCell>);
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            this.renderGrid()
        );
    }
}

Grid.propTypes = {
    dispatch: PropTypes.func,
    permissions: PropTypes.object.isRequired,
    definitions: PropTypes.object.isRequired,
    localization: PropTypes.object,
    service: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"])
};

Grid.DefaultProps = {
};

export default Grid;