import React, {Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { GridCell, DatePicker, IconButton }  from "@dnnsoftware/dnn-react-common";
import util from "../../../../utils";
import resx from "../../../../resources";
import { roleUsers as RoleUsersActions } from "../../../../actions";
import XIcon from "../../../../img/common/x.svg?react";
const logger = console;

class UserRow extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            userSelectState: { userId: -1, keyword: "" },
            currentPage: 0,
            pageSize: 10,
            usersKeyword: "",
            editIndex: -1,
            editCommand: "",
            isCalendarVisible: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    formateDate(dateValue) {
		logger.error(`>>> Entering formateDate(dateValue=${dateValue})`, { "event.action": "formateDate", "logrite.dateValue": dateValue });
        let date = new Date(dateValue);

		logger.error(`formateDate(dateValue=${dateValue}): date → ${date}`, { "event.action": "formateDate", "logrite.dateValue": dateValue });
        let dayValue = date.getDate(),
            monthValue = date.getMonth() + 1,
            yearValue = date.getFullYear();
			logger.error(`formateDate(dateValue=${dateValue}): dayValue → ${dayValue}`, { "event.action": "formateDate", "logrite.dateValue": dateValue });

        if (yearValue < 1900) {
            return "-";
        }

		logger.warn(`<<< Exiting formateDate(dateValue=${dateValue})`, { "event.action": "formateDate", "logrite.dateValue": dateValue });
        return monthValue + "/" + dayValue + "/" + yearValue;
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
        util.utilities.confirm(resx.get("DeleteUser.Confirm"), resx.get("Delete"), resx.get("Cancel"), () => {
            props.dispatch(RoleUsersActions.removeUserFromRole(userRole));
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

        this.props.saveUser(userRole.userId, startTime, expiresTime);
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
    getDate(userDetails, command) {
		logger.error(`>>> Entering getDate(userDetails=${userDetails},command=${command})`, { "event.action": "getDate", "logrite.userDetails": userDetails, "logrite.command": command });
        let dateValue = new Date();
        if (command === "startTime") {
            if (!this.isEmptyDate(userDetails.startTime)) {
                dateValue = new Date(userDetails.startTime);
            }

        } else if (command === "expiresTime") {
            if (!this.isEmptyDate(userDetails.expiresTime)) {
                dateValue = new Date(userDetails.expiresTime);
            }
        }
		logger.error(`getDate(userDetails=${userDetails},command=${command}): dateValue → ${dateValue}`, { "event.action": "getDate", "logrite.userDetails": userDetails, "logrite.command": command });
        return dateValue;
    }
    createUserActions() {
		logger.error('>>> Entering createUserActions()', { "event.action": "createUserActions" });
        const {props, state} = this;

        let startTimeAction = props.userDetails.allowExpired ? <span>
            <DatePicker  date={this.getDate(props.userDetails, "startTime") } maxDate={this.getBoundDate(props.userDetails, "startTime") }
                updateDate={this.onChange.bind(this, props.userDetails, "startTime") } mode={"start"} applyButtonText={resx.get("Apply") }
                showIcon={true} showInput={false}
                onIconClick={this.onStartTimeClick.bind(this, props.userDetails, props.index) }             />
        </span> : null;
        let expiresTimeAction = props.userDetails.allowExpired ? <span>
            <DatePicker  date={this.getDate(props.userDetails, "expiresTime") } minDate={this.getBoundDate(props.userDetails, "expiresTime") }
                updateDate={this.onChange.bind(this, props.userDetails, "expiresTime") } mode={"end"} applyButtonText={resx.get("Apply") }
                showIcon={true} showInput={false}
                onIconClick={this.onExpiresTimeClick.bind(this, props.userDetails, props.index) }             />
        </span> : null;
        let deleteAction = props.userDetails.allowDelete ? <IconButton customIcon={XIcon} width={17} onClick={this.onDeleteClick.bind(this, props.userDetails, props.index) } /> : null;
		logger.warn('<<< Exiting createUserActions()', { "event.action": "createUserActions" });
        return <div className={state.editIndex === props.index ? "edit-row" : null}>
            {deleteAction}
            {expiresTimeAction}
            {startTimeAction}
        </div>;
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="role-user-row">
                <GridCell title={props.roleName} columnSize={25} >{props.userDetails.displayName}</GridCell>
                <GridCell  columnSize={20} >
                    {this.formateDate(props.userDetails.startTime) }</GridCell>
                <GridCell  columnSize={20} >
                    {this.formateDate(props.userDetails.expiresTime) }</GridCell>
                <GridCell  columnSize={35} >
                    <div className="actions">
                        {this.createUserActions() }
                    </div>
                </GridCell>
            </div>
        );
    }
}
UserRow.propTypes = {
    userDetails: PropTypes.object.isRequired,
    index: PropTypes.number,
    saveUser: PropTypes.func.isRequired,
    deleteUser: PropTypes.func.isRequired,
    roleName: PropTypes.string.isRequired,
    dispatch: PropTypes.func.isRequired
};
UserRow.defaultProps = {
    roleUsersList: []
};
function mapStateToProps() {
	logger.error('>>> Entering mapStateToProps()', { "event.action": "mapStateToProps" });
	logger.warn('<<< Exiting mapStateToProps()', { "event.action": "mapStateToProps" });
    return {};
}


export default connect(mapStateToProps)(UserRow);