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
		logger.debug('>>> Entering constructor()');
        this.state = {
            userSelectState: { userId: -1, keyword: "" },
            currentPage: 0,
            pageSize: 10,
            usersKeyword: "",
            editIndex: -1,
            editCommand: "",
            isCalendarVisible: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    formateDate(dateValue) {
        let date = new Date(dateValue);

        let dayValue = date.getDate(),
            monthValue = date.getMonth() + 1,
            yearValue = date.getFullYear();

        if (yearValue < 1900) {
            return "-";
        }

        return monthValue + "/" + dayValue + "/" + yearValue;
    }
    onStartTimeClick(userRole, index) {
		logger.debug(`>>> Entering onStartTimeClick(userRole=${userRole},index=${index})`);
        this.setState({ editIndex: index, editCommand: "startTime", isCalendarVisible: true });
		logger.debug(`<<< Exiting onStartTimeClick(userRole=${userRole},index=${index})`);
    }

    onExpiresTimeClick(userRole, index) {
		logger.debug(`>>> Entering onExpiresTimeClick(userRole=${userRole},index=${index})`);
        this.setState({ editIndex: index, editCommand: "expiresTime", isCalendarVisible: true });
		logger.debug(`<<< Exiting onExpiresTimeClick(userRole=${userRole},index=${index})`);
    }

    onDeleteClick(userRole) {
		logger.debug(`>>> Entering onDeleteClick(userRole=${userRole})`);
        const {props} = this;
        util.utilities.confirm(resx.get("DeleteUser.Confirm"), resx.get("Delete"), resx.get("Cancel"), () => {
            props.dispatch(RoleUsersActions.removeUserFromRole(userRole));
        });
		logger.debug(`<<< Exiting onDeleteClick(userRole=${userRole})`);
    }
    isEmptyDate(date) {
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
		logger.debug(`>>> Entering getBoundDate(userRole=${userRole},command=${command})`);
        if (command === "startTime") {
            let maxValue = new Date(2049, 11, 31);
			logger.debug(`getBoundDate(userRole=${userRole},command=${command}): maxValue → ${maxValue}`);
            if (!this.isEmptyDate(userRole.expiresTime)) {
                maxValue = new Date(new Date().setTime(new Date(userRole.expiresTime).getTime() - 1 * 86400000));
            }
            return maxValue;
        } else if (command === "expiresTime") {
            let minValue = new Date(1970, 0, 1);
            if (!this.isEmptyDate(userRole.startTime)) {
                minValue = new Date(new Date().setTime(new Date(userRole.startTime).getTime() + 1 * 86400000));
            }
			logger.debug(`getBoundDate(userRole=${userRole},command=${command}): minValue → ${minValue}`);
			logger.debug(`<<< Exiting getBoundDate(userRole=${userRole},command=${command})`);
            return minValue;
        }
    }
    getDate(userDetails, command) {
		logger.debug(`>>> Entering getDate(userDetails=${userDetails},command=${command})`);
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
		logger.debug(`getDate(userDetails=${userDetails},command=${command}): dateValue → ${dateValue}`);
		logger.debug(`<<< Exiting getDate(userDetails=${userDetails},command=${command})`);
        return dateValue;
    }
    createUserActions() {
		logger.debug('>>> Entering createUserActions()');
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
		logger.debug('<<< Exiting createUserActions()');
        return <div className={state.editIndex === props.index ? "edit-row" : null}>
            {deleteAction}
            {expiresTimeAction}
            {startTimeAction}
        </div>;
    }
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
		logger.debug('<<< Exiting render()');
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
	logger.debug('>>> Entering mapStateToProps()');
	logger.debug('<<< Exiting mapStateToProps()');
    return {};
}


export default connect(mapStateToProps)(UserRow);