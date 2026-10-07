import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import resx from "../../resources";
import {
    roles as RolesActions
} from "../../actions";
import FiltersBar from "./FiltersBar";
import RoleRow from "./RoleRow";
import { GridCell }  from "@dnnsoftware/dnn-react-common";
import UsersInRole from "./UsersInRole";
import RoleEditor from "./RoleEditor";
import CollapsibleSwitcher from "../common/CollapsibleSwitcher";
const logger = console;
import {
    createRoleGroupOptions
} from "./helpers/roles";

const tableFields = [
    { name: "RoleName", width: 40 },
    { name: "GroupName", width: 20 },
    { name: "Users", width: 15 },
    { name: "Auto", width: 15 },
    { name: "", width: 10 }
];

class RolesPanel extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });

        this.state = {
            defaultRoleGroup: resx.get("AllGroups"),
            openId: "",
            renderIndex: -1,
            deleteAllowed: false,
            parameter: {
                groupId: -1,
                keyword: "",
                startIndex: 0,
                pageSize: 10,
                reload: true
            }
        };
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;
        props.dispatch(RolesActions.getRoleGroupsList(false));
        this.refreshRolesList();
        let deleteAllowed = (props.rolesList !== undefined && props.rolesList.length === 0);
        this.setState({ deleteAllowed, });
		logger.error(`componentDidMount(): deleteAllowed → ${deleteAllowed}`, { "event.action": "componentDidMount" });
    }

    static getDerivedStateFromProps(props) {
		logger.error(`>>> Entering getDerivedStateFromProps(props=${props})`, { "event.action": "getDerivedStateFromProps", "logrite.props": props });
        let deleteAllowed = (props.rolesList !== undefined && props.rolesList.length === 0);
		logger.error(`getDerivedStateFromProps(props=${props}): deleteAllowed → ${deleteAllowed}`, { "event.action": "getDerivedStateFromProps", "logrite.props": props });
        return { deleteAllowed, };
    }

    refreshRolesList() {
		logger.error('>>> Entering refreshRolesList()', { "event.action": "refreshRolesList" });
        const {props, state} = this;
        props.dispatch(RolesActions.getRolesList(state.parameter, (data) => {
			logger.warn('<<< Exiting refreshRolesList()', { "event.action": "refreshRolesList" });
            let rolesList = Object.assign([], data.roles);
            this.setState({
                rolesList
            });
        }));
    }

    onPageChanged() {
		logger.error('>>> Entering onPageChanged()', { "event.action": "onPageChanged" });
        const {props} = this;
        let startIndex = props.rolesList.length;
        let {parameter} = this.state;
		logger.debug(`onPageChanged(): parameter.reload → ${parameter.reload}`, { "event.action": "onPageChanged" });
        parameter.startIndex = startIndex;
		logger.debug(`onPageChanged(): parameter.startIndex → ${parameter.startIndex}`, { "event.action": "onPageChanged" });
        parameter.reload = false;
        this.setState({
            parameter
        });
        this.refreshRolesList(startIndex);
		logger.warn('<<< Exiting onPageChanged()', { "event.action": "onPageChanged" });
    }

    onAddRole() {
		logger.error('>>> Entering onAddRole()', { "event.action": "onAddRole" });
        this.toggle(this.state.openId === "add" ? "" : "add", 1);
		logger.warn('<<< Exiting onAddRole()', { "event.action": "onAddRole" });
    }

    uncollapse(id, index) {
		logger.error(`>>> Entering uncollapse(id=${id},index=${index})`, { "event.action": "uncollapse", "logrite.id": id, "logrite.index": index });
        setTimeout(() => {
            this.setState({
                openId: id,
                renderIndex: index
            });
        }, this.timeout);
		logger.warn(`<<< Exiting uncollapse(id=${id},index=${index})`, { "event.action": "uncollapse", "logrite.id": id, "logrite.index": index });
    }
    collapse() {
		logger.error('>>> Entering collapse()', { "event.action": "collapse" });
        if (this.state.openId !== "") {
            this.setState({
                openId: "",
                renderIndex: -1
            });
			logger.warn('<<< Exiting collapse()', { "event.action": "collapse" });
        }
    }
    toggle(openId, index) {
		logger.error(`>>> Entering toggle(openId=${openId},index=${index})`, { "event.action": "toggle", "logrite.openId": openId, "logrite.index": index });
        if (openId !== "") {
            this.uncollapse(openId, index);
        } else {
            this.collapse();
			logger.warn(`<<< Exiting toggle(openId=${openId},index=${index})`, { "event.action": "toggle", "logrite.openId": openId, "logrite.index": index });
        }
    }
    onRoleGroupChanged(group) {
		logger.error(`>>> Entering onRoleGroupChanged(group=${group})`, { "event.action": "onRoleGroupChanged", "logrite.group": group });
        let {parameter} = this.state;
        parameter.groupId = group.value;
		logger.debug(`onRoleGroupChanged(group=${group}): parameter.groupId → ${parameter.groupId}`, { "event.action": "onRoleGroupChanged", "logrite.group": group });
        parameter.startIndex = 0;
		logger.debug(`onRoleGroupChanged(group=${group}): parameter.startIndex → ${parameter.startIndex}`, { "event.action": "onRoleGroupChanged", "logrite.group": group });
        parameter.reload = true;
        this.setState({
            parameter
        });
        this.refreshRolesList();
		logger.warn(`<<< Exiting onRoleGroupChanged(group=${group})`, { "event.action": "onRoleGroupChanged", "logrite.group": group });
    }

    onKeywordChanged(keyword) {
		logger.error(`>>> Entering onKeywordChanged(keyword=${keyword})`, { "event.action": "onKeywordChanged", "logrite.keyword": keyword });
        let {parameter} = this.state;
        parameter.keyword = keyword;
		logger.debug(`onKeywordChanged(keyword=${keyword}): parameter.keyword → ${parameter.keyword}`, { "event.action": "onKeywordChanged", "logrite.keyword": keyword });
        parameter.startIndex = 0;
		logger.debug(`onKeywordChanged(keyword=${keyword}): parameter.startIndex → ${parameter.startIndex}`, { "event.action": "onKeywordChanged", "logrite.keyword": keyword });
        parameter.reload = true;
        this.setState({
            parameter
        });
        this.refreshRolesList();
		logger.warn(`<<< Exiting onKeywordChanged(keyword=${keyword})`, { "event.action": "onKeywordChanged", "logrite.keyword": keyword });
    }
    GetGroupName(groupId) {
		logger.error(`>>> Entering GetGroupName(groupId=${groupId})`, { "event.action": "GetGroupName", "logrite.groupId": groupId });
        const {props} = this;
        if (props.roleGroups.some(group => group.id === groupId)) {
            return props.roleGroups.filter(group => group.id === groupId)[0].name;
        }

		logger.warn(`<<< Exiting GetGroupName(groupId=${groupId})`, { "event.action": "GetGroupName", "logrite.groupId": groupId });
        return groupId;
    }

    renderHeader() {
		logger.error('>>> Entering renderHeader()', { "event.action": "renderHeader" });
        let tableHeaders = tableFields.map((field) => {
            return <GridCell key={field.name} columnSize={field.width} style={{ fontWeight: "bolder" }}>
                {
                    field.name !== "" ?
                        <span>{resx.get(field.name + ".Header")}</span>
                        : <span>&nbsp; </span>
                }
            </GridCell>;
        });
		logger.warn('<<< Exiting renderHeader()', { "event.action": "renderHeader" });
        return <div id="users-header-row" className="users-header-row">{tableHeaders}</div>;
    }
    renderedRolesList(roleGroupOptions, securityModeOptions, statusOptions) {
        if (this.props.rolesList.length > 0) {
            let validRolesList = this.props.rolesList.filter(logSetting => !!logSetting);
            let i = 0;

            return validRolesList.map((role, index) => {
                let id = "row-" + i++;
                let children = [
                    <UsersInRole key={"userInRole-" + id}
                        Collapse={this.collapse.bind(this)} roleDetails={role} />,
                    <RoleEditor
                        key={"roleeditor-" + id}
                        roleDetails={role}
                        roleGroupOptions={roleGroupOptions}
                        securityModeOptions={securityModeOptions}
                        statusOptions={statusOptions}
                        refreshRolesList={this.refreshRolesList.bind(this)}
                        roleId={role.id}
                        Collapse={this.collapse.bind(this)}
                        currentGroupId={this.state.parameter.groupId}
                    />];
                return (
                    <RoleRow
                        key={"role-" + index}
                        roleName={role.localizedName}
                        groupName={this.GetGroupName(role.groupId)}
                        userCount={role.usersCount}
                        auto={role.autoAssign}
                        index={index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        currentIndex={this.state.renderIndex}
                        OpenCollapseUsers={this.toggle.bind(this, id, 0)}
                        OpenCollapseEditRoles={this.toggle.bind(this, id, 1)}
                        Collapse={this.collapse.bind(this, id)}
                        roleIsApproved={role.status === 1}
                        id={id}>
                        <CollapsibleSwitcher renderIndex={this.state.renderIndex}>{children}</CollapsibleSwitcher>
                    </RoleRow>
                );
            });
        }
        else {
            return <div className="no-users-row">{resx.get("NoData")}</div>;
        }
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
        let opened = (this.state.openId === "add");
        let roleGroupOptions = createRoleGroupOptions(this.props.roleGroups);
        const securityModeOptions = [
            { label: resx.get("SecurityRole"), value: 0 },
            { label: resx.get("SocialGroup"), value: 1 },
            { label: resx.get("Both"), value: 2 }
        ];
        const statusOptions = [
            { label: resx.get("Pending"), value: -1 },
            { label: resx.get("Disabled"), value: 0 },
            { label: resx.get("Approved"), value: 1 }
        ];
        let children = [
            <div key="" />,
            <RoleEditor
                key=""
                roleGroupOptions={roleGroupOptions}
                securityModeOptions={securityModeOptions}
                statusOptions={statusOptions}
                roleId={-1}
                Collapse={this.collapse.bind(this)}
                currentGroupId={this.state.parameter.groupId}
            />];
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            props.roleGroups &&
            <div className="roles-list-container">
                <FiltersBar onRoleGroupChanged={this.onRoleGroupChanged.bind(this)}
                    roleGroups={this.props.roleGroups}
                    onKeywordChanged={this.onKeywordChanged.bind(this)}
                    DeleteAllowed={state.deleteAllowed}
                />
                <div className="container">
                    {this.renderHeader()}
                    <div className="add-setting-editor">
                        <RoleRow
                            roleName={"-"}
                            groupName={"-"}
                            userCount={0}
                            auto={false}
                            index={"add"}
                            key={"role-add"}
                            closeOnClick={true}
                            openId={this.state.openId}
                            currentIndex={this.state.renderIndex}
                            OpenCollapseUsers={this.toggle.bind(this, "add", 1)}
                            OpenCollapseEditRoles={this.toggle.bind(this, "add", 1)}
                            Collapse={this.collapse.bind(this, "add")}
                            id={"add"}
                            addIsClosed={!opened}>
                            {opened && <CollapsibleSwitcher renderIndex={this.state.renderIndex}>{children}</CollapsibleSwitcher>}
                        </RoleRow>
                    </div>
                    {this.renderedRolesList(roleGroupOptions, securityModeOptions, statusOptions)}
                </div>
                {props.loadMore && <div className="loadMore">
                    <a href="#" onClick={this.onPageChanged.bind(this)}>
                        {resx.get("LoadMore")}
                    </a>
                </div>}
            </div>
        );
    }
}
RolesPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    rolesList: PropTypes.array,
    roleGroups: PropTypes.array,
    loadMore: PropTypes.bool
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        roleGroups: state.roles.roleGroups,
        rolesList: state.roles.rolesList,
        loadMore: state.roles.loadMore
    };
}

export default connect(mapStateToProps, null, null, { forwardRef: true })(RolesPanel);