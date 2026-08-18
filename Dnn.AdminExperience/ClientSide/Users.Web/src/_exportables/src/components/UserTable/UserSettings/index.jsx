import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import { CommonUsersActions } from "../../../actions";
import {formatDate, validateEmail} from "../../../helpers";
import utilities from "utils";
import styles from "./style.module.less";
import ChangePassword from "../ChangePassword";
const logger = console;
import {canManagePassword, canEditSettings} from "../../permissionHelpers.js";
import { GridCell, GridSystem, SingleLineInputWithError, Button } from "@dnnsoftware/dnn-react-common";

const inputStyle = { width: "100%" };
const blankAccountSettings = {
    userId: 0,
    displayName: "",
    userName: "",
    email: ""
};

class UserSettings extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            accountSettings: Object.assign(blankAccountSettings),
            userDetails: props.userDetails,
            errors: {
                displayName: false,
                userName: false,
                loading: false,
                email: false
            },
            startingValues: {
                userName: undefined,
            },
            ChangePasswordVisible: false
        };
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }
    componentWillMount() {
		logger.debug('>>> Entering componentWillMount()');
        let {props} = this;
        if (props.userDetails === undefined || props.userDetails.userId !== props.userId) {
            this.getUserDetails(props, props.userId);
        }
        else
        {
            this.updateUserDetailsState(props.userDetails);
			logger.debug('<<< Exiting componentWillMount()');
        }
    }
    componentWillReceiveProps(newProps) {
		logger.debug(`>>> Entering componentWillReceiveProps(newProps=${newProps})`);
        if (newProps.userDetails === undefined && newProps.userDetails.userId !== newProps.userId) {
            this.getUserDetails(newProps, newProps.userId);
        }
        else
        {
            this.updateUserDetailsState(newProps.userDetails);
			logger.debug(`<<< Exiting componentWillReceiveProps(newProps=${newProps})`);
        }
    }

    makeBlankObj(obj) {
		logger.debug(`>>> Entering makeBlankObj(obj=${obj})`);
        let newObject = Object.assign({}, obj);
        const keys = Object.keys(newObject);
		logger.debug(`makeBlankObj(obj=${obj}): newObject → ${newObject}`);
		logger.debug(`makeBlankObj(obj=${obj}): keys → ${keys}`);
        keys.forEach(key => newObject[key] = "");
		logger.debug(`<<< Exiting makeBlankObj(obj=${obj})`);
        return newObject; 
    }

    getUserDetails(props, userId) {
		logger.debug(`>>> Entering getUserDetails(props=${props},userId=${userId})`);
        const accountSettings = this.makeBlankObj(this.state.accountSettings);
        const userDetails = this.makeBlankObj(this.state.userDetails);
		logger.debug(`getUserDetails(props=${props},userId=${userId}): accountSettings → ${accountSettings}`);
		logger.debug(`getUserDetails(props=${props},userId=${userId}): userDetails → ${userDetails}`);
        this.setState({accountSettings, userDetails, loading: true});
        props.dispatch(CommonUsersActions.getUserDetails({ userId: userId }, (data) => {
            this.updateUserDetailsState(data);
			logger.debug(`<<< Exiting getUserDetails(props=${props},userId=${userId})`);
        }));
    }
    updateUserDetailsState(details) {
		logger.debug(`>>> Entering updateUserDetailsState(details=${details})`);
        let userDetails = Object.assign({}, details);
        let {accountSettings, startingValues} = this.state;
		logger.debug(`updateUserDetailsState(details=${details}): userDetails → ${userDetails}`);
        accountSettings.displayName = userDetails.displayName;
        accountSettings.userName = userDetails.userName;
        accountSettings.email = userDetails.email;
        accountSettings.userId = userDetails.userId;
        startingValues.userName = userDetails.userName;
		logger.debug(`<<< Exiting updateUserDetailsState(details=${details})`);
        this.setState({
            accountSettings,
            userDetails,
            startingValues,
            loading: false
        });
    }
    
    onChange(key, item) {
        if (this.state.loading) {
            return;
        }
        let {accountSettings} = this.state;
        accountSettings[key] = item.target.value;
        this.setState({ accountSettings }, () => {
            this.validateForm(true);
        });
    }

    save() {
		logger.debug('>>> Entering save()');
        if (this.validateForm()) {
            this.props.dispatch(CommonUsersActions.updateUserBasicInfo(this.state.accountSettings, () => {
                utilities.notify(Localization.get("UserUpdated"), 3000);
                this.getUserDetails(this.props, this.state.accountSettings.userId);
                this.props.collapse();
				logger.debug('<<< Exiting save()');
            }));
        }
    }
  
    validateForm() {
        let valid = true;
        let {errors} = this.state;
        errors.displayName = false;
        errors.userName = false;
        errors.email = false;
        let {accountSettings, startingValues} = this.state;
        if (accountSettings.displayName === "") {
            errors.displayName = true;
            valid = false;
        }
        if (accountSettings.userName === "") {
            errors.userName = true;
            valid = false;
        }
        if (startingValues.userName !== accountSettings.userName && accountSettings.userName.length < this.props.appSettings.applicationSettings.settings.userNameMinLength) {
            errors.userName = true;
            valid = false;
        }
        if (accountSettings.email === "" || !validateEmail(accountSettings.email)) {
            errors.email = true;
            valid = false;
        }
        this.setState({ errors });

        return valid;
    }
    onCancelPassword() {
		logger.debug('>>> Entering onCancelPassword()');
        this.setState({
            ChangePasswordVisible: false
        });
		logger.debug('<<< Exiting onCancelPassword()');
    }
    onChangePassword() {
		logger.debug('>>> Entering onChangePassword()');
        if (this.state.loading) {
            return;
        }
        this.setState({
            ChangePasswordVisible: true
        });
		logger.debug('<<< Exiting onChangePassword()');
    }
    onForcePasswordChange() {
		logger.debug('>>> Entering onForcePasswordChange()');
        if (this.state.loading) {
            return;
        }
        this.props.dispatch(CommonUsersActions.forceChangePassword({ userId: this.props.userId }, () => {
            utilities.notify(Localization.get("UserPasswordUpdateChanged"), 3000);
            let {userDetails} = this.state;
            userDetails.needUpdatePassword = true;
            this.setState({ userDetails });
        }));
		logger.debug('<<< Exiting onForcePasswordChange()');
    }
    onSendPasswordLink() {
		logger.debug('>>> Entering onSendPasswordLink()');
        if (this.state.loading) {
            return;
        }
        this.props.dispatch(CommonUsersActions.sendPasswordResetLink({ userId: this.props.userId }, () => {
            utilities.notify(Localization.get("PasswordSent"), 3000);
        }));
		logger.debug('<<< Exiting onSendPasswordLink()');
    }
    stringifyBoolean(value) {
        if (value === "") {
		logger.debug(`<<< Exiting stringifyBoolean(value=${value})`);
            return "";
        }
        return value ? Localization.get("True") : Localization.get("False");        
    }

    render() {
		logger.debug('>>> Entering render()');
        let {state} = this;
        let agreedToTerms = this.props.appSettings.applicationSettings.settings.dataConsentActive ? (
            <GridSystem>
                <GridCell title={Localization.get("HasAgreedToTerms.Help")}>
                    {Localization.get("HasAgreedToTerms")}:
                </GridCell>
                <GridCell>
                    {this.stringifyBoolean(state.userDetails.hasAgreedToTerms)}
                </GridCell>
            </GridSystem>
        ) : null;
        let agreedToTermsOn = this.props.appSettings.applicationSettings.settings.dataConsentActive ? (
            <GridSystem>
                <GridCell title={Localization.get("LastConsented.Help")}>
                    {Localization.get("LastConsented")}:
                </GridCell>
                <GridCell>
                    {formatDate(state.userDetails.hasAgreedToTermsOn, true) === "-" ? Localization.get("Never") : formatDate(state.userDetails.hasAgreedToTermsOn, true) }
                </GridCell>
            </GridSystem>
        ) : null;
        let userRequestedDeletion = state.userDetails.requestsRemoval ? (
            <span className="importantNote">{Localization.get("RequestedRemoval")}</span>
        ) : null;
		logger.debug('<<< Exiting render()');
        return <GridCell className={styles.userSettings}>
            <GridCell>
                <GridCell className="outer-box" columnSize={50}>
                    <ChangePassword visible={this.state.ChangePasswordVisible} onCancel={this.onCancelPassword.bind(this) } userId={this.props.userId} />
                    <div className="title">
                        {Localization.get("AccountSettings")}
                    </div>
                    <div className={this.state.loading ? "isloading" : ""}>
                        <SingleLineInputWithError value={state.accountSettings.userName}
                            error={state.errors.userName}
                            onChange={this.onChange.bind(this, "userName") }
                            label={Localization.get("Username") }
                            tooltipMessage={Localization.get("Username.Help")}
                            errorMessage={Localization.get("Username.Required") }
                            style={inputStyle}
                            autoComplete="off"
                            enabled={canEditSettings(this.props.appSettings.applicationSettings.settings)}
                            inputStyle={{ marginBottom: 25 }}/>
                        <SingleLineInputWithError value={state.accountSettings.displayName}
                            error={state.errors.displayName}
                            onChange={this.onChange.bind(this, "displayName") }
                            label={Localization.get("DisplayName") }
                            tooltipMessage={Localization.get("DisplayName.Help")}
                            errorMessage={Localization.get("DisplayName.Required") }
                            style={inputStyle}
                            autoComplete="off"
                            enabled={canEditSettings(this.props.appSettings.applicationSettings.settings)}
                            inputStyle={{ marginBottom: 25 }} />
                        <SingleLineInputWithError value={state.accountSettings.email}
                            error={state.errors.email}
                            onChange={this.onChange.bind(this, "email") }
                            label={Localization.get("Email") }
                            tooltipMessage={Localization.get("Email.Help")}
                            errorMessage={Localization.get("Email.Required") }
                            style={inputStyle}
                            autoComplete="off"
                            enabled={canEditSettings(this.props.appSettings.applicationSettings.settings)}
                            inputStyle={{ marginBottom: 25 }}/>
                    </div>
                    {canManagePassword(this.props.appSettings.applicationSettings.settings, this.state.userDetails.userId) &&
                        <GridCell className="no-padding">
                            <div className="title">
                                {Localization.get("PasswordManagement")}
                            </div>
                            <GridCell className={"link" + (this.state.loading ? " disabled" : "")}>
                                <div onClick={this.onChangePassword.bind(this) }>[ {Localization.get("ChangePassword")} ]
                                </div>
                            </GridCell>
                            {!state.userDetails.needUpdatePassword && 
                                <GridCell className={"link" + (this.state.loading ? " disabled" : "")}>
                                    <div onClick={this.onForcePasswordChange.bind(this) }>[ {Localization.get("ForceChangePassword")} ]
                                    </div>
                                </GridCell>
                            }
                            <GridCell className={"link" + (this.state.loading ? " disabled" : "")}>
                                <div onClick={this.onSendPasswordLink.bind(this) }>[ {Localization.get("ResetPassword")} ]
                                </div>
                            </GridCell>
                        </GridCell>
                    }
                </GridCell>
                <GridCell className="outer-box right" columnSize={50}>
                    <div className="title">
                        {Localization.get("AccountData")}
                    </div>
                    <GridSystem className="first">
                        <GridCell  title={Localization.get("UserId.Help")}>
                            {Localization.get("UserId")}
                        </GridCell>
                        <GridCell>
                            {state.userDetails.userId}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell  title={Localization.get("CreatedDate.Help")}>
                            {Localization.get("CreatedDate")}
                        </GridCell>
                        <GridCell>
                            {formatDate(state.userDetails.createdOnDate, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("LastLoginDate.Help")}>
                            {Localization.get("LastLoginDate")}
                        </GridCell>
                        <GridCell>
                            {formatDate(state.userDetails.lastLogin, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("LastActivityDate.Help")}>
                            {Localization.get("LastActivityDate")}
                        </GridCell>
                        <GridCell>
                            {formatDate(state.userDetails.lastActivity, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("LastPasswordChangeDate.Help")}>
                            {Localization.get("LastPasswordChangeDate")}
                        </GridCell>
                        <GridCell>
                            {formatDate(state.userDetails.lastPasswordChange, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("LastLockoutDate.Help")}>
                            {Localization.get("LastLockoutDate")}
                        </GridCell>
                        <GridCell>
                            {formatDate(state.userDetails.lastLockout, true) === "-" ? Localization.get("Never") : formatDate(state.userDetails.lastLockout, true) }
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("IsOnLine.Help")}>
                            {Localization.get("IsOnLine")}
                        </GridCell>
                        <GridCell>
                            {this.stringifyBoolean(state.userDetails.isOnline)}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("LockedOut.Help")}>
                            {Localization.get("LockedOut")}
                        </GridCell>
                        <GridCell>
                            {this.stringifyBoolean(state.userDetails.isLocked)}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("Approved.Help")}>
                            {Localization.get("Approved")}
                        </GridCell>
                        <GridCell>
                            {this.stringifyBoolean(state.userDetails.authorized)}
                        </GridCell>
                    </GridSystem>
                    {agreedToTerms}
                    {agreedToTermsOn}
                    <GridSystem>
                        <GridCell title={Localization.get("UpdatePassword.Help")}>
                            {Localization.get("UpdatePassword")}
                        </GridCell>
                        <GridCell>
                            {this.stringifyBoolean(state.userDetails.needUpdatePassword)}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("IsDeleted.Help")}>
                            {Localization.get("IsDeleted")}
                        </GridCell>
                        <GridCell>
                            {this.stringifyBoolean(state.userDetails.isDeleted)} {userRequestedDeletion}
                        </GridCell>
                    </GridSystem>
                    <GridSystem>
                        <GridCell title={Localization.get("UserFolder.Help")}>
                            {Localization.get("UserFolder")}
                        </GridCell>
                        <GridCell>
                            {state.userDetails.userFolder}
                        </GridCell>
                    </GridSystem>
                </GridCell>
            </GridCell>
            {canEditSettings(this.props.appSettings.applicationSettings.settings) &&
                <GridCell className="buttons">
                    <GridCell columnSize={50} className="leftBtn">
                        <Button id="cancelbtn"  type="neutral" onClick={this.props.collapse.bind(this) }>{Localization.get("btnCancel") }</Button>
                    </GridCell>
                    <GridCell columnSize={50} className="rightBtn">
                        <Button id="confirmbtn" disabled={this.state.loading} type="primary" onClick={this.save.bind(this) }>{Localization.get("btnSave") }</Button>
                    </GridCell>
                </GridCell>
            }
        </GridCell>;
    }
}
UserSettings.propTypes = {
    dispatch: PropTypes.func.isRequired,
    userId: PropTypes.number.isRequired,
    collapse: PropTypes.func.isRequired,
    userDetails: PropTypes.object,
    appSettings: PropTypes.object
};
function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(UserSettings);