import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import RoleRow from "./RoleRow";
import "./style.less";
import { CommonUsersActions } from "../../../actions";
import utilities from "utils";
import Combobox from "react-widgets/Combobox";
const logger = console;
import { GridCell, Checkbox, Pager, SvgIcons } from "@dnnsoftware/dnn-react-common";

class UserRoles extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            roleSelectState: { userId: -1, keyword: "" },
            currentPage: 0,
            pageSize: 10,
            roleKeyword: "",
            sendEmail: true,
            isOwner: false,
            allowOwner: false
        };
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.getRoles();
		logger.debug('<<< Exiting componentDidMount()');
    }


    getRoles() {
		logger.debug('>>> Entering getRoles()');
        const {props, state} = this;

        let parameter = {
            userId: props.userDetails.userId,
            keyword: state.roleKeyword,
            pageIndex: state.currentPage,
            pageSize: state.pageSize
        };
        props.dispatch(CommonUsersActions.getUserRoles(parameter));
		logger.debug('<<< Exiting getRoles()');
    }

    getSuggestRoles() {
		logger.debug('>>> Entering getSuggestRoles()');
        const {props, state} = this;
        let keyword = state.roleSelectState.roleId >= 0 ? "" : state.roleSelectState.keyword;
        props.dispatch(CommonUsersActions.getSuggestRoles({ keyword: keyword, count: 10 }));
		logger.debug('<<< Exiting getSuggestRoles()');
    }
    debounceGetSuggestRoles() {
		logger.debug('>>> Entering debounceGetSuggestRoles()');
        this.getSuggestRoles();
		logger.debug('<<< Exiting debounceGetSuggestRoles()');
    }

    onRoleSelectorChanged(item) {
		logger.debug(`>>> Entering onRoleSelectorChanged(item=${item})`);
        if (item.roleId || item.roleName) {
            return;
        }
        this.setState({ roleSelectState: { roleId: -1, keyword: item } });
        this.debounceGetSuggestRoles();
		logger.debug(`<<< Exiting onRoleSelectorChanged(item=${item})`);
    }

    onRoleSelectorSelected(item) {
		logger.debug(`>>> Entering onRoleSelectorSelected(item=${item})`);
        this.onRoleSelected(item.roleId, () => {
            this.setState({ roleSelectState: { roleId: item.roleId, keyword: item.roleName } }, () => {
                this.getSuggestRoles();
            });
        });
		logger.debug(`<<< Exiting onRoleSelectorSelected(item=${item})`);
    }

    onRoleSelectorToggle() {
	logger.debug('>>> Entering onRoleSelectorToggle()');
    }

    onAddRole() {
		logger.debug('>>> Entering onAddRole()');
        const {state} = this;
        let roleId = state.roleSelectState.roleId;
        if (roleId === -1 || roleId=== undefined) {
            return;
        }
        this.saveRole(roleId);
        this.setState({ roleSelectState: { roleId: -1, keyword: "" } });
		logger.debug('<<< Exiting onAddRole()');
    }
    saveRole(roleId, startTime, expiresTime) {
        const {props} = this;

        let parameter = { roleId: roleId, userId: props.userDetails.userId, startTime: startTime, expiresTime: expiresTime };
        props.dispatch(CommonUsersActions.saveUserRole(parameter, this.state.sendEmail, this.state.isOwner));
        this.setState({ sendEmail: true, isOwner: false, allowOwner: false });
		logger.debug(`<<< Exiting saveRole(roleId=${roleId},startTime=${startTime},expiresTime=${expiresTime})`);
    }

    onPageChanged(currentPage, pageSize) {
		logger.debug(`>>> Entering onPageChanged(currentPage=${currentPage},pageSize=${pageSize})`);
        let {state} = this;
        if (pageSize !== undefined && state.pageSize !== pageSize) {
            state.pageSize = pageSize;
        }
        state.currentPage = currentPage;
        this.setState({
            state
        });
        this.getRoles();
		logger.debug(`<<< Exiting onPageChanged(currentPage=${currentPage},pageSize=${pageSize})`);
    }
    getRoleRows() {
		logger.debug('>>> Entering getRoleRows()');
        let userRoles = this.props.userRoles;
        let roleRows = userRoles.map((role, index) => {
            return <RoleRow
                roleDetails={role}
                index={index}
                key={`role_row_${index}`}
                saveRole={this.saveRole.bind(this) }>
            </RoleRow>;
        });
		logger.debug('<<< Exiting getRoleRows()');
        return <div className="user-role-body">{(userRoles.length > 0) ?
            roleRows :
            <div className="no-roles-row">{Localization.get("NoRoles") }</div> }
        </div>;
    }
    onRoleSelected(roleId, callback) {
        if (this.props.matchedRoles !== undefined && this.props.matchedRoles.length > 0 && this.props.matchedRoles.some(r => r.roleId === roleId)) {
            let role = this.props.matchedRoles.filter(r => r.roleId === roleId)[0];
            this.setState({ allowOwner: role.allowOwner }, () => {
                if (typeof callback === "function") {
                    callback();
                }
            });
        }
    }
    onSendEmailClick(sendEmail) {
        this.setState({ sendEmail });
    }
    onIsOwnerClick(isOwner) {
        this.setState({ isOwner });
    }
    renderHeader() {
        const tableFields = [
            { name: "Role", width: 25 },
            { name: "Start", width: 20 },
            { name: "Expires", width: 20 },
            { name: "", width: 35 }
        ];
        let tableHeaders = tableFields.map((field, index) => {
            return <GridCell key={`grid_cell_${index}`} columnSize={field.width} style={{ fontWeight: "bolder" }}>
                {
                    field.name !== "" ?
                        <span>{Localization.get(field.name + ".Header") }</span>
                        : <div></div>
                }
            </GridCell>;
        });
        return <div className="user-role-header-row">{tableHeaders}</div>;
    }
    renderPaging() {
        if (this.props.totalRecords > 0)
            return <Pager
                showStartEndButtons={false}
                showPageSizeOptions={false}
                numericCounters={0}
                summaryText={Localization.get("rolesSummaryText")}
                pageInfoText={Localization.get("rolesPageInfoText")}
                showPageInfo={true}
                pageSize={this.state.pageSize}
                totalRecords={this.props.totalRecords}
                onPageChanged={this.onPageChanged.bind(this) }
                culture={utilities.getCulture()}
            />;
    }
    render() {
		logger.debug('>>> Entering render()');
        const {state} = this;
        /* eslint-disable react/no-danger */
		logger.debug('<<< Exiting render()');
        return <div className="userroles-form-form">
            <div className="header">
                <div className="header-title">{Localization.get("Roles.Title") }</div>
                <div className="add-box">
                    <GridCell columnSize={30}>
                        <div className="send-email-box">
                            <Checkbox value={this.state.sendEmail} onChange={this.onSendEmailClick.bind(this) }
                                label={  Localization.get("SendEmail") } labelPlace="right"    />
                            {this.state.allowOwner && <Checkbox value={this.state.isOwner} onChange={this.onIsOwnerClick.bind(this) }
                                label={  Localization.get("IsOwner") } labelPlace="right"   />}
                        </div>
                    </GridCell>
                    <GridCell columnSize={70}>
                        <span>
                            <Combobox suggest={false}
                                placeholder={Localization.get("AddRolePlaceHolder") }
                                open={this.props.matchedRoles && this.props.matchedRoles.length > 0 }
                                onToggle={this.onRoleSelectorToggle.bind(this) }
                                onChange={this.onRoleSelectorChanged.bind(this) }
                                onSelect={this.onRoleSelectorSelected.bind(this) }
                                data={this.props.matchedRoles }
                                value={state.roleSelectState.keyword}
                                valueField="roleId"
                                textField="roleName"/>
                            <div className="add-role-button" onClick={this.onAddRole.bind(this) }>
                                <div className={"extension-action"} title={Localization.get("Add")}><SvgIcons.AddIcon /></div>
                                {Localization.get("Add") }
                            </div>
                        </span>
                    </GridCell>
                </div>
            </div>
            <div className="user-roles-list">
                {this.renderHeader() }
                {this.getRoleRows() }
            </div>
            <div className="user-roles-list-paging">
                {this.renderPaging() }
            </div>
        </div>;
    }
}
UserRoles.propTypes = {
    dispatch: PropTypes.func.isRequired,
    userDetails: PropTypes.object.isRequired,
    userRoles: PropTypes.array.isRequired,
    totalRecords: PropTypes.number,
    matchedRoles: PropTypes.array
};
UserRoles.defaultProps = {
    matchedRoles: []
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        matchedRoles: state.users.matchedRoles,
        userRoles: state.users.userRoles,
        totalRecords: state.users.userRolesCount
    };
}

export default connect(mapStateToProps)(UserRoles);