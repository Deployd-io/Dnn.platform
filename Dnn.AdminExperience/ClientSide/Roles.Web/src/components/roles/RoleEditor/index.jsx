import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import util from "../../../utils";
import resx from "../../../resources";
import { Dropdown as Select, GridSystem as Grid, Switch, Button, SingleLineInputWithError, MultiLineInput, Label }  from "@dnnsoftware/dnn-react-common";
import RoleGroupEditor from "./RoleGroupEditor";
const logger = console;
import {
    roles as RolesActions
} from "../../../actions";

class RolesEditor extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        let roleDetails = Object.assign({}, props.roleDetails);
        this.state = {
            roleDetails: props.roleId !== -1 ? roleDetails : {
                id: -1,
                name: "",
                groupId: -1,
                description: "",
                securityMode: 0,
                status: 1,
                isPublic: false,
                autoAssign: false,
                isSystem: false
            },
            errors: {
                roleName: false
            },
            groupId: props.roleId !== -1 ? roleDetails.groupId : -1,
            assignToUsers: false,
            formModified: false,
            createGroupVisible: false
        };
        this.submitted = false;
		logger.error(`constructor(props=${props}): roleDetails → ${roleDetails}`, { "event.action": "constructor", "logrite.props": props });
    }

    getValue(selectKey) {
		logger.error(`>>> Entering getValue(selectKey=${selectKey})`, { "event.action": "getValue", "logrite.selectKey": selectKey });
        const { state } = this;
        switch (selectKey) {
            case "RoleGroup":
                return state.roleDetails.groupId !== undefined ? state.roleDetails.groupId : -1;
            case "SecurityMode":
                return state.roleDetails.securityMode !== undefined ? state.roleDetails.securityMode : 0;
            case "Status":
                return state.roleDetails.status !== undefined ? state.roleDetails.status : 1;
            case "AutoAssignment":
                return state.roleDetails.autoAssign !== undefined ? state.roleDetails.autoAssign : false;
            case "Public":
                return state.roleDetails.isPublic !== undefined ? state.roleDetails.isPublic : false;
            default:
                break;
        }
		logger.warn(`<<< Exiting getValue(selectKey=${selectKey})`, { "event.action": "getValue", "logrite.selectKey": selectKey });
    }

    onDropDownChange(key, option) {
		logger.error(`>>> Entering onDropDownChange(key=${key},option=${option})`, { "event.action": "onDropDownChange", "logrite.key": key, "logrite.option": option });
        if (key === "groupId" && option.value === -3) {
            let createGroupVisible = true;
            this.setState({
                createGroupVisible
            });
        }
        this.performChange(key, option.value);
		logger.warn(`<<< Exiting onDropDownChange(key=${key},option=${option})`, { "event.action": "onDropDownChange", "logrite.key": key, "logrite.option": option });
    }
    onTextChange(key, event) {
		logger.error(`>>> Entering onTextChange(key=${key},event=${event})`, { "event.action": "onTextChange", "logrite.key": key, "logrite.event": event });
        this.performChange(key, event.target.value);
		logger.warn(`<<< Exiting onTextChange(key=${key},event=${event})`, { "event.action": "onTextChange", "logrite.key": key, "logrite.event": event });
    }
    performChange(key, value) {
		logger.error(`>>> Entering performChange(key=${key},value=${value})`, { "event.action": "performChange", "logrite.key": key, "logrite.value": value });
        if (key !== "assignToUsers") {
            let { roleDetails } = this.state;
            roleDetails[key] = value;
            this.setState({
                roleDetails
            });
        } else {
            let assignToUsers = value;
            this.setState({
                assignToUsers
            });
        }
        let { state } = this;
        state.formModified = true;
		logger.debug(`performChange(key=${key},value=${value}): state.formModified → ${state.formModified}`, { "event.action": "performChange", "logrite.key": key, "logrite.value": value });
        this.setState({
            state
        }, () => {
            this.validateForm();
        });
		logger.warn(`<<< Exiting performChange(key=${key},value=${value})`, { "event.action": "performChange", "logrite.key": key, "logrite.value": value });
    }

    onSwitchToggle(key, status) {
		logger.error(`>>> Entering onSwitchToggle(key=${key},status=${status})`, { "event.action": "onSwitchToggle", "logrite.key": key, "logrite.status": status });
        this.performChange(key, status);
		logger.warn(`<<< Exiting onSwitchToggle(key=${key},status=${status})`, { "event.action": "onSwitchToggle", "logrite.key": key, "logrite.status": status });
    }
    addUpdateRoleDetails(event) {
		logger.error(`>>> Entering addUpdateRoleDetails(event=${event})`, { "event.action": "addUpdateRoleDetails", "logrite.event": event });
        event.preventDefault();
        const { props, state } = this;
        this.submitted = true;
        if (!this.validateForm()) {
			logger.debug(`addUpdateRoleDetails(event=${event}): this.submitted → ${this.submitted}`, { "event.action": "addUpdateRoleDetails", "logrite.event": event });
            return;
        }

        if (state.formModified) {
            let { roleDetails } = this.state;
            if (roleDetails.groupId === -3) {
                return;//Avoid saving while in create group window
            }

            let successMsg = resx.get("RoleAdded.Message");
			logger.error(`addUpdateRoleDetails(event=${event}): successMsg → ${successMsg}`, { "event.action": "addUpdateRoleDetails", "logrite.event": event });
            let errorMsg = resx.get("RoleAdded.Error");
            if (props.roleId > 0) {
                successMsg = resx.get("RoleUpdated.Message");
                errorMsg = resx.get("RoleUpdated.Error");
            }
            props.dispatch(RolesActions.saveRole(this.props.currentGroupId, state.assignToUsers, roleDetails, () => {
                util.utilities.notify(successMsg);
                props.Collapse(event);
            }, () => {
                util.utilities.notify(errorMsg);
            }));
        }
        else {
            props.Collapse(event);
        }
		logger.warn(`<<< Exiting addUpdateRoleDetails(event=${event})`, { "event.action": "addUpdateRoleDetails", "logrite.event": event });
    }

    validateForm() {
        let valid = true;
        if (this.submitted) {
            let { roleDetails } = this.state;
            let { errors } = this.state;
            errors.roleName = false;
            if (roleDetails.name === "") {
                errors.roleName = true;
                valid = false;
            }
            this.setState({ errors });
        }
        return valid;
    }
    refreshRolesListIfRequired() {
		logger.error('>>> Entering refreshRolesListIfRequired()', { "event.action": "refreshRolesListIfRequired" });
        const { props, state } = this;
        let group = props.roleGroups.find(group => group.id === state.groupId);
        if (group !== undefined && group.rolesCount <= 1) {
            props.refreshRolesList();
			logger.warn('<<< Exiting refreshRolesListIfRequired()', { "event.action": "refreshRolesListIfRequired" });
        }
    }
    deleteRole(event) {
		logger.error(`>>> Entering deleteRole(event=${event})`, { "event.action": "deleteRole", "logrite.event": event });
        let { roleDetails } = this.state;
        const { props } = this;
        if (props.roleId > 0) {
            util.utilities.confirm(resx.get("DeleteRole.Confirm"), resx.get("Delete"), resx.get("Cancel"), () => {
                props.dispatch(RolesActions.deleteRole(roleDetails, () => {
                    util.utilities.notify(resx.get("DeleteRole.Message"));
                    props.Collapse(event);
                }));
            });
        }
        else {
            util.utilities.notify(resx.get("DeleteInconsistency.Error"));
			logger.warn(`<<< Exiting deleteRole(event=${event})`, { "event.action": "deleteRole", "logrite.event": event });
        }
    }
    getRoleGroupOptions() {
		logger.error('>>> Entering getRoleGroupOptions()', { "event.action": "getRoleGroupOptions" });
        let groupOptions = this.props.roleGroupOptions;
        groupOptions = groupOptions.filter(group => {
            return group.value !== -2;
        });
        groupOptions = [{
            label: <span className="do-not-close">{resx.get("lblNewGroup")}</span>, value: -3
        }].concat(groupOptions);
		logger.warn('<<< Exiting getRoleGroupOptions()', { "event.action": "getRoleGroupOptions" });
        return groupOptions;
    }
    CloseCreateGroup() {
        let createGroupVisible = false;
        this.setState({
            createGroupVisible
        });
    }
    onCancelCreateGroup() {
        this.CloseCreateGroup();
        this.onDropDownChange("groupId", { value: -1 });
    }
    onCreateGroup(group) {
        this.CloseCreateGroup();
        this.onDropDownChange("groupId", { value: group.id });
    }
    doNothing() {

    }
     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        let { state, props } = this;
        const columnOne = <div key="editor-container-columnOne" className="editor-container">
            <div className="editor-row divider" style={{marginTop:"-3rem"}}>
                <Label
                    label={resx.get("RoleId") + ": " + state.roleDetails.id} />
            </div>
            <div className="editor-row divider">
                <SingleLineInputWithError
                    value={state.roleDetails.name}
                    enabled={!state.roleDetails.isSystem}
                    onChange={this.onTextChange.bind(this, "name")}
                    maxLength={50}
                    error={state.errors.roleName}
                    label={resx.get("RoleName")}
                    tooltipMessage={resx.get("RoleName.Help")}
                    errorMessage={resx.get("RoleName.Required")}
                    autoComplete="off"
                    inputStyle={{ marginBottom: 0 }}
                    tabIndex={1} />
            </div>
            <div className="editor-row divider">
                <Label
                    label={resx.get("Description")}
                    tooltipMessage={resx.get("Description.Help")}
                    tooltipPlace={"top"} />
                <MultiLineInput
                    value={state.roleDetails.description}
                    enabled={!state.roleDetails.isSystem}
                    onChange={this.onTextChange.bind(this, "description")}
                    maxLength={500} />
            </div>
            <div className="editor-row divider">
                <Label
                    label={resx.get("statusListLabel")}
                    tooltipMessage={resx.get("statusListLabel.Help")}
                    tooltipPlace={"top"} />
                <Select
                    value={this.getValue("Status")}
                    enabled={!state.roleDetails.isSystem}
                    options={this.props.statusOptions}
                    style={{ width: 100 + "%", float: "left" }}
                    onSelect={this.onDropDownChange.bind(this, "status")}
                />
            </div>
            <div className="status-row">
                <div className="left">
                    <Label
                        labelType="inline"
                        label={resx.get("Public")}
                        tooltipMessage={resx.get("PublicRole.Help")}
                        tooltipPlace={"top"} />
                </div>
                <div className="right">
                    <Switch
                        onText={resx.get("SwitchOn")}
                        offText={resx.get("SwitchOff")}
                        readOnly={state.roleDetails.isSystem}
                        value={this.getValue("Public")}
                        onChange={this.onSwitchToggle.bind(this, "isPublic")} />
                </div>
            </div>
        </div>;

        const columnTwo = <div key="editor-container-columnTwo" className="editor-container right-column">
            <div className="editor-row divider">
                <Label
                    label={resx.get("plRoleGroups")}
                    tooltipMessage={resx.get("plRoleGroups.Help")}
                    tooltipPlace={"top"} />
                <Select
                    value={this.getValue("RoleGroup")}
                    enabled={!state.roleDetails.isSystem}
                    options={this.getRoleGroupOptions()}
                    style={{ width: 100 + "%", float: "left" }}
                    onSelect={this.onDropDownChange.bind(this, "groupId")} />
                <div className="new-group-container">
                    <RoleGroupEditor
                        visible={this.state.createGroupVisible}
                        onSave={this.onCreateGroup.bind(this)}
                        onCancel={this.onCancelCreateGroup.bind(this)}
                        onClick={this.doNothing.bind(this)}
                        showPopup={this.state.createGroupVisible}
                        group={{ id: -2, name: "", description: "" }}
                        title="New Group" />
                </div>
            </div>
            <div className="editor-row divider">
                <Label
                    label={resx.get("securityModeListLabel")}
                    tooltipMessage={resx.get("securityModeListLabel.Help")}
                    tooltipPlace={"top"} />
                <Select
                    value={this.getValue("SecurityMode")}
                    enabled={!state.roleDetails.isSystem}
                    options={this.props.securityModeOptions}
                    style={{ width: 100 + "%", float: "left" }}
                    onSelect={this.onDropDownChange.bind(this, "securityMode")}
                />
            </div>
            <div className="editor-row divider">
                <SingleLineInputWithError
                    value={state.roleDetails.rsvpCode}
                    enabled={!state.roleDetails.isSystem}
                    maxLength={50}
                    onChange={this.onTextChange.bind(this, "rsvpCode")}
                    label={resx.get("plRSVPCode")}
                    tooltipMessage={resx.get("plRSVPCode.Help")}
                    inputStyle={{ marginBottom: 5 }}
                    tabIndex={1} />
            </div>
            <div className="editor-row divider">
                <SingleLineInputWithError
                    value={state.roleDetails.rsvpCode &&
                        state.roleDetails.rsvpCode !== "" ?
                        props.rsvpLink + "&rsvp=" + state.roleDetails.rsvpCode : ""}
                    enabled={false}
                    onChange={this.onTextChange.bind(this, "rsvpLink")}
                    label={resx.get("plRSVPLink")}
                    tooltipMessage={resx.get("plRSVPLink.Help")}
                    inputStyle={{ marginBottom: 0 }}
                    tabIndex={1} />
            </div>
            <div className="status-row">
                <div className="left">
                    <Label
                        labelType="inline"
                        label={resx.get("AutoAssignment")}
                        tooltipMessage={resx.get("AutoAssignment.Help")}
                        tooltipPlace={"top"} />
                </div>
                <div className="right">
                    <Switch
                        onText={resx.get("SwitchOn")}
                        offText={resx.get("SwitchOff")}
                        readOnly={state.roleDetails.isSystem}
                        value={state.roleDetails.autoAssign}
                        onChange={this.onSwitchToggle.bind(this, "autoAssign")} />
                </div>
            </div>
            {state.roleDetails.autoAssign &&
                <div className="status-row">
                    <div className="left">
                        <Label
                            labelType="inline"
                            label={resx.get("AssignToExistUsers")}
                            tooltipMessage={resx.get("AssignToExistUsers.Help")}
                            tooltipPlace={"top"} />
                    </div>
                    <div className="right">
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            readOnly={state.roleDetails.isSystem}
                            value={state.assignToUsers}
                            onChange={this.onSwitchToggle.bind(this, "assignToUsers")} />
                    </div>
                </div>
            }
        </div>;
        let children = [];
        children.push(columnOne);
        children.push(columnTwo);
         
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="role-details-editor">
                <Grid numberOfColumns={2}>{children}</Grid>
                <div className="buttons-box">
                    {
                        this.props.roleId > 0 && (!state.roleDetails.isSystem && state.roleDetails.id > -1) ?
                            <Button
                                type="danger"
                                onClick={this.deleteRole.bind(this)}>
                                {resx.get("Delete")}
                            </Button>
                            : null
                    }
                    <Button
                        type="neutral"
                        disabled={state.roleDetails.isSystem}
                        onClick={this.props.Collapse.bind(this)}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        type="primary"
                        disabled={state.roleDetails.isSystem}
                        onClick={this.addUpdateRoleDetails.bind(this)}>
                        {resx.get("Save")}
                    </Button>
                </div>
            </div>
        );
    }
}
RolesEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    roleId: PropTypes.number,
    roleDetails: PropTypes.object,
    roleGroupOptions: PropTypes.array,
    securityModeOptions: PropTypes.array,
    statusOptions: PropTypes.array,
    Collapse: PropTypes.func,
    refreshRolesList: PropTypes.func,
    roleGroups: PropTypes.array,
    currentGroupId: PropTypes.number,
    rsvpLink: PropTypes.string
};
function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        roleGroups: state.roles.roleGroups,
        rsvpLink: state.roles.rsvpLink
    };
}

export default connect(mapStateToProps)(RolesEditor);
