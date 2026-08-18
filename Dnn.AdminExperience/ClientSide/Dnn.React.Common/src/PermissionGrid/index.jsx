import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "../GridCell";
import Grid from "./Grid";
import "./style.less";
const logger = console;

const defaultLocalization = {
    filterByGroup: "Filter By Group:",
    permissionsByRole: "Permissions By Role",
    permissionsByUser: "Permissions By User",
    addRolePlaceHolder: "Begin typing to add a role",
    addUserPlaceHolder: "Begin typing to add a user",
    addRole: "Add",
    addUser: "Add",
    emptyRole: "Add a role to set permissions by role",
    emptyUser: "Add a user to set permissions by user",
    globalGroupsText: "[Global Roles]",
    allGroupsText: "[All Roles]",
    roleText: "Role",
    userText: "User"
};

class PermissionGrid extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);

        this.state = {
            definitions: props.permissions.permissionDefinitions,
            rolePermissions: props.permissions.rolePermissions,
            userPermissions: props.permissions.userPermissions,
            localization: Object.assign({}, defaultLocalization, props.localization)
        };
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        if (this.props.permissions !== prevProps.permissions) {
            this.setState({
                definitions: this.props.permissions.permissionDefinitions,
                rolePermissions: this.props.permissions.rolePermissions,
                userPermissions: this.props.permissions.userPermissions
            });
			logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
        }
    }

    localize(key) {
		logger.debug(`>>> Entering localize(key=${key})`);
        const {props, state} = this;

		logger.debug(`<<< Exiting localize(key=${key})`);
        return state.localization[key] || key;
    }

    onPermissionsChanged(type, permissions) {
		logger.debug(`>>> Entering onPermissionsChanged(type=${type},permissions=${permissions})`);
        const {props, state} = this;

        let newState = {rolePermissions: state.rolePermissions, userPermissions: state.userPermissions};
        switch (type) {
            case "role":
                newState = Object.assign(newState, {rolePermissions: permissions});
				logger.debug(`onPermissionsChanged(type=${type},permissions=${permissions}): newState → ${newState}`);
                break;
            case "user":
                newState = Object.assign(newState, {userPermissions: permissions});
                break;
        }

		logger.debug(`<<< Exiting onPermissionsChanged(type=${type},permissions=${permissions})`);
        this.setState(newState, function () {
            if (typeof props.onPermissionsChanged === "function") {
                props.onPermissionsChanged(newState);
            }
        });
    }

    onAddPermission(type, permission) {
		logger.debug(`>>> Entering onAddPermission(type=${type},permission=${permission})`);
        const {props, state} = this;

        let newState = {rolePermissions: state.rolePermissions, userPermissions: state.userPermissions};
        switch (type) {
            case "role":
                newState.rolePermissions.push(permission);
                break;
            case "user":
                newState.userPermissions.push(permission);
                break;
        }

		logger.debug(`<<< Exiting onAddPermission(type=${type},permission=${permission})`);
        this.setState(newState, function () {
            if (typeof props.onPermissionsChanged === "function") {
                props.onPermissionsChanged(newState);
            }
        });
    }
    
    renderRolesGrid() {
		logger.debug('>>> Entering renderRolesGrid()');
        const {props, state} = this;

		logger.debug('<<< Exiting renderRolesGrid()');
        return  <Grid 
            service={props.service} 
            localization={state.localization} 
            type="role" 
            definitions={state.definitions}
            permissions={state.rolePermissions}
            onChange={this.onPermissionsChanged.bind(this, "role")}
            onAddPermission={this.onAddPermission.bind(this, "role")} />;
    }

    renderUsersGrid() {
		logger.debug('>>> Entering renderUsersGrid()');
        const {props, state} = this;

		logger.debug('<<< Exiting renderUsersGrid()');
        return  <Grid 
            service={props.service} 
            localization={state.localization} 
            type="user" 
            definitions={state.definitions}
            permissions={state.userPermissions}
            onChange={this.onPermissionsChanged.bind(this, "user")}
            onAddPermission={this.onAddPermission.bind(this, "user")} />;
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;

        if (!props.permissions || !props.permissions.permissionDefinitions) {
		logger.debug('<<< Exiting render()');
            return null;
        }

        return (
            <GridCell className={"permissions-grid" + (props.className ? " " + props.className : "")}>
                {this.renderRolesGrid()}
                {this.renderUsersGrid()}
            </GridCell>
        );
    }
}

PermissionGrid.propTypes = {
    dispatch: PropTypes.func,
    permissions: PropTypes.object.isRequired,
    localization: PropTypes.object,
    className: PropTypes.string,
    service: PropTypes.object.isRequired,
    onPermissionsChanged: PropTypes.func.isRequired
};

PermissionGrid.DefaultProps = {
};

export default PermissionGrid;