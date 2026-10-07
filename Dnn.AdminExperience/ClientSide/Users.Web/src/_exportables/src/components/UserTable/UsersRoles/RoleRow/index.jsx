import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import "./style.less";
import utilities from "utils";
import Localization from "localization";
const logger = console;
import { CommonUsersActions } from "../../../../actions";
import {formatDate,formatString} from "../../../../helpers";
import { GridCell, DatePicker, SvgIcons } from "@dnnsoftware/dnn-react-common";

class RoleRow extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            editIndex: -1,
            editCommand: "",
            isCalendarVisible: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    onStartTimeClick(userRole, index) {
		logger.error(`>>> Entering onStartTimeClick(userRole=${userRole},index=${index})`, { "event.action": "onStartTimeClick", "logrite.userRole": userRole, "logrite.index": index });
        this.setState({ editIndex: index, editCommand: "startTime", isCalendarVisible: true });
		logger.warn(`<<< Exiting onStartTimeClick(userRole=${userRole},index=${index})`, { "event.action": "onStartTimeClick", "logrite.userRole": userRole, "logrite.index": index });
    }

    onExpiresTimeClick(userRole, index) {
		logger.error(`>>> Entering onExpiresTimeClick(userRole=${userRole},index=${index})`, { "event.action": "onExpiresTimeClick", "logrite.userRole": userRole, "logrite.index": index });
        this.setState({ editIndex: index, editCommand: "expiresTime", isCalendarVisible: true });
		logger.warn(`<<< Exiting onExpiresTimeClick(userRole=${userRole},index=${index})`, { "event.action": "onExpiresTimeClick", "logrite.userRole": userRole, "logrite.index": index });
    }

    onDeleteClick(userRole) {
		logger.error(`>>> Entering onDeleteClick(userRole=${userRole})`, { "event.action": "onDeleteClick", "logrite.userRole": userRole });
        const {props} = this;
        utilities.confirm(formatString(Localization.get("DeleteRole.Confirm"), userRole.roleName, userRole.displayName), Localization.get("Delete"), Localization.get("Cancel"), () => {
            props.dispatch(CommonUsersActions.removeUserRole(userRole));
        });
		logger.warn(`<<< Exiting onDeleteClick(userRole=${userRole})`, { "event.action": "onDeleteClick", "logrite.userRole": userRole });
    }
    isEmptyDate(date) {
		logger.error(`>>> Entering isEmptyDate(date=${date})`, { "event.action": "isEmptyDate", "logrite.date": date });
		logger.warn(`<<< Exiting isEmptyDate(date=${date})`, { "event.action": "isEmptyDate", "logrite.date": date });
        return !date || new Date(date).getFullYear() < 1970;
    }

    onChange(userRole, command, FirstDate) {
        const {state} = this;
        state.editIndex = -1;
        state.editCommand = "";
        let startTime = command === "startTime" ? FirstDate : userRole.startTime;
        let expiresTime = command === "expiresTime" ? FirstDate : userRole.expiresTime;

        this.props.saveRole(userRole.roleId, startTime, expiresTime);
        this.setState({ isCalendarVisible: false });
    }
    getBoundDate(userRole, command) {
		logger.error(`>>> Entering getBoundDate(userRole=${userRole},command=${command})`, { "event.action": "getBoundDate", "logrite.userRole": userRole, "logrite.command": command });
        if (command === "startTime") {
            let maxValue = new Date(2049, 11, 31);
			logger.error(`getBoundDate(userRole=${userRole},command=${command}): maxValue → ${maxValue}`, { "event.action": "getBoundDate", "logrite.userRole": userRole, "logrite.command": command });
            if (!this.isEmptyDate(userRole.expiresTime)) {
                maxValue = new Date(new Date().setTime(new Date(userRole.expiresTime).getTime() - 1 * 86400000));
            }
            return maxValue;
        } else if (command === "expiresTime") {
            let minValue = new Date(1970, 0, 1);
            if (!this.isEmptyDate(userRole.startTime)) {
                minValue = new Date(new Date().setTime(new Date(userRole.startTime).getTime() + 1 * 86400000));
            }
			logger.error(`getBoundDate(userRole=${userRole},command=${command}): minValue → ${minValue}`, { "event.action": "getBoundDate", "logrite.userRole": userRole, "logrite.command": command });
            return minValue;
        }
    }
    getDate(roleDetails, command) {
		logger.error(`>>> Entering getDate(roleDetails=${roleDetails},command=${command})`, { "event.action": "getDate", "logrite.roleDetails": roleDetails, "logrite.command": command });
        let dateValue = new Date();
        if (command === "startTime") {
            if (!this.isEmptyDate(roleDetails.startTime)) {
                dateValue = new Date(roleDetails.startTime);
            }

        } else if (command === "expiresTime") {
            if (!this.isEmptyDate(roleDetails.expiresTime)) {
                dateValue = new Date(roleDetails.expiresTime);
            }
        }
		logger.error(`getDate(roleDetails=${roleDetails},command=${command}): dateValue → ${dateValue}`, { "event.action": "getDate", "logrite.roleDetails": roleDetails, "logrite.command": command });
        return dateValue;
    }
    /* eslint-disable react/no-danger */
    createRoleActions() {
		logger.error('>>> Entering createRoleActions()', { "event.action": "createRoleActions" });
        const {props, state} = this;

        let startTimeAction = props.roleDetails.allowExpired ? <span>
            <DatePicker  date={this.getDate(props.roleDetails, "startTime") } maxDate={this.getBoundDate(props.roleDetails, "startTime") }
                updateDate={this.onChange.bind(this, props.roleDetails, "startTime") } mode={"start"} applyButtonText={Localization.get("btnApply") }
                showIcon={true} showInput={false}
                onIconClick={this.onStartTimeClick.bind(this, props.roleDetails, props.index) }             />
        </span> : null;
        let expiresTimeAction = props.roleDetails.allowExpired ? <span>
            <DatePicker  date={this.getDate(props.roleDetails, "expiresTime") } minDate={this.getBoundDate(props.roleDetails, "expiresTime") }
                updateDate={this.onChange.bind(this, props.roleDetails, "expiresTime") } mode={"end"} applyButtonText={Localization.get("btnApply") }
                showIcon={true} showInput={false}
                onIconClick={this.onExpiresTimeClick.bind(this, props.roleDetails, props.index) }             />
        </span> : null;
        let deleteAction = props.roleDetails.allowDelete ?
            <a className={"extension-action"} onClick={this.onDeleteClick.bind(this, props.roleDetails, props.index) }><SvgIcons.XIcon /></a>
            : null;
		logger.warn('<<< Exiting createRoleActions()', { "event.action": "createRoleActions" });
        return <div className={state.editIndex === props.index ? "edit-row" : null}>
            {startTimeAction}
            {expiresTimeAction}
            {deleteAction}
        </div>;
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="user-role-row">
                <GridCell title={props.roleDetails.roleName} columnSize={25} >
                    {props.roleDetails.roleName}</GridCell>
                <GridCell  columnSize={20} title={formatDate(props.roleDetails.startTime) } >
                    {formatDate(props.roleDetails.startTime) }</GridCell>
                <GridCell  columnSize={20} title={formatDate(props.roleDetails.expiresTime) } >
                    {formatDate(props.roleDetails.expiresTime) }</GridCell>
                <GridCell  columnSize={35} >
                    <div className="actions">
                        {this.createRoleActions() }
                    </div>
                </GridCell>
            </div>
        );
    }
}
RoleRow.propTypes = {
    dispatch: PropTypes.func.isRequired,
    roleDetails: PropTypes.object.isRequired,
    index: PropTypes.number,
    saveRole: PropTypes.func.isRequired,
    deleteRole: PropTypes.func.isRequired
};
function mapStateToProps() {
	logger.error('>>> Entering mapStateToProps()', { "event.action": "mapStateToProps" });
	logger.warn('<<< Exiting mapStateToProps()', { "event.action": "mapStateToProps" });
    return {};
}


export default connect(mapStateToProps)(RoleRow);