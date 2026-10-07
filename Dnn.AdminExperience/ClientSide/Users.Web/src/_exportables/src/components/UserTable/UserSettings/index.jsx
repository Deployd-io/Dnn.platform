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
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
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
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }
    componentWillMount() {
		logger.error('>>> Entering componentWillMount()', { "event.action": "componentWillMount" });
        let {props} = this;
        if (props.userDetails === undefined || props.userDetails.userId !== props.userId) {
            this.getUserDetails(props, props.userId);
        }
        else
        {
            this.updateUserDetailsState(props.userDetails);
			logger.warn('<<< Exiting componentWillMount()', { "event.action": "componentWillMount" });
        }
    }
    componentWillReceiveProps(newProps) {
		logger.error(`>>> Entering componentWillReceiveProps(newProps=${newProps})`, { "event.action": "componentWillReceiveProps", "logrite.newProps": newProps });
        if (newProps.userDetails === undefined && newProps.userDetails.userId !== newProps.userId) {
            this.getUserDetails(newProps, newProps.userId);
        }
        else
        {
            this.updateUserDetailsState(newProps.userDetails);
			logger.warn(`<<< Exiting componentWillReceiveProps(newProps=${newProps})`, { "event.action": "componentWillReceiveProps", "logrite.newProps": newProps });
        }
    }

    makeBlankObj(obj) {
		logger.error(`>>> Entering makeBlankObj(obj=${obj})`, { "event.action": "makeBlankObj", "logrite.obj": obj });
        let newObject = Object.assign({}, obj);
        const keys = Object.keys(newObject);
		logger.error(`makeBlankObj(obj=${obj}): newObject → ${newObject}`, { "event.action": "makeBlankObj", "logrite.obj": obj });
		logger.error(`makeBlankObj(obj=${obj}): keys → ${keys}`, { "event.action": "makeBlankObj", "logrite.obj": obj });
        keys.forEach(key => newObject[key] = "");
		logger.warn(`<<< Exiting makeBlankObj(obj=${obj})`, { "event.action": "makeBlankObj", "logrite.obj": obj });
        return newObject; 
    }

    getUserDetails(props, userId) {
		logger.error(`>>> Entering getUserDetails(props=${props},userId=${userId})`, { "event.action": "getUserDetails", "logrite.props": props, "user.name": String(userId) });
        const accountSettings = this.makeBlankObj(this.state.accountSettings);
        const userDetails = this.makeBlankObj(this.state.userDetails);
		logger.error(`getUserDetails(props=${props},userId=${userId}): accountSettings → ${accountSettings}`, { "event.action": "getUserDetails", "logrite.props": props, "user.name": String(userId) });
		logger.error(`getUserDetails(props=${props},userId=${userId}): userDetails → ${userDetails}`, { "event.action": "getUserDetails", "logrite.props": props, "user.name": String(userId) });
        this.setState({accountSettings, userDetails, loading: true});
        props.dispatch(CommonUsersActions.getUserDetails({ userId: userId }, (data) => {
            this.updateUserDetailsState(data);
			logger.warn(`<<< Exiting getUserDetails(props=${props},userId=${userId})`, { "event.action": "getUserDetails", "logrite.props": props, "user.name": String(userId) });
        }));
    }
    updateUserDetailsState(details) {
		logger.error(`>>> Entering updateUserDetailsState(details=${details})`, { "event.action": "updateUserDetailsState", "logrite.details": details });
        let userDetails = Object.assign({}, details);
        let {accountSettings, startingValues} = this.state;
		logger.error(`updateUserDetailsState(details=${details}): userDetails → ${userDetails}`, { "event.action": "updateUserDetailsState", "logrite.details": details });
        accountSettings.displayName = userDetails.displayName;
        accountSettings.userName = userDetails.userName;
        accountSettings.email = userDetails.email;
        accountSettings.userId = userDetails.userId;
        startingValues.userName = userDetails.userName;
		logger.warn(`<<< Exiting updateUserDetailsState(details=${details})`, { "event.action": "updateUserDetailsState", "logrite.details": details });
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
		logger.error('>>> Entering save()', { "event.action": "save" });
        if (this.validateForm()) {
            this.props.dispatch(CommonUsersActions.updateUserBasicInfo(this.state.accountSettings, () => {
                utilities.notify(Localization.get("UserUpdated"), 3000);
                this.getUserDetails(this.props, this.state.accountSettings.userId);
                this.props.collapse();
				logger.warn('<<< Exiting save()', { "event.action": "save" });
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
		logger.error('>>> Entering onCancelPassword()', { "event.action": "onCancelPassword" });
        this.setState({
            ChangePasswordVisible: false
        });
		logger.warn('<<< Exiting onCancelPassword()', { "event.action": "onCancelPassword" });
    }
    onChangePassword() {
		logger.error('>>> Entering onChangePassword()', { "event.action": "onChangePassword" });
        if (this.state.loading) {
            return;
        }
        this.setState({
            ChangePasswordVisible: true
        });
		logger.warn('<<< Exiting onChangePassword()', { "event.action": "onChangePassword" });
    }
    onForcePasswordChange() {
		logger.error('>>> Entering onForcePasswordChange()', { "event.action": "onForcePasswordChange" });
        if (this.state.loading) {
            return;
        }
        this.props.dispatch(CommonUsersActions.forceChangePassword({ userId: this.props.userId }, () => {
            utilities.notify(Localization.get("UserPasswordUpdateChanged"), 3000);
            let {userDetails} = this.state;
            userDetails.needUpdatePassword = true;
            this.setState({ userDetails });
        }));
		logger.warn('<<< Exiting onForcePasswordChange()', { "event.action": "onForcePasswordChange" });
    }
    onSendPasswordLink() {
		logger.error('>>> Entering onSendPasswordLink()', { "event.action": "onSendPasswordLink" });
        if (this.state.loading) {
            return;
        }
        this.props.dispatch(CommonUsersActions.sendPasswordResetLink({ userId: this.props.userId }, () => {
            utilities.notify(Localization.get("PasswordSent"), 3000);
        }));
		logger.warn('<<< Exiting onSendPasswordLink()', { "event.action": "onSendPasswordLink" });
    }
    stringifyBoolean(value) {
		logger.error(`>>> Entering stringifyBoolean(value=${value})`, { "event.action": "stringifyBoolean", "logrite.value": value });
        if (value === "") {
            return "";
        }
		logger.warn(`<<< Exiting stringifyBoolean(value=${value})`, { "event.action": "stringifyBoolean", "logrite.value": value });
        return value ? Localization.get("True") : Localization.get("False");        
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
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
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(UserSettings);