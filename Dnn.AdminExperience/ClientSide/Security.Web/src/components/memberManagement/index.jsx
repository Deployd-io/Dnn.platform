import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import { 
    InputGroup, 
    SingleLineInputWithError, 
    Switch,
    Tooltip,
    Label,
    Button
} from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

const re = /^[1-9][0-9]?[0-9]?[0-9]?$/;
const re2 = /^[1-9][0-9]?[0-9]?[0-9]?$|^0$/;

class MemberManagementPanelBody extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            memberSettings: undefined,
            error: {
                membershipResetLinkValidity: true,
                adminMembershipResetLinkValidity: true,
                membershipNumberPasswords: true,
                membershipDaysBeforePasswordReuse: true,
                passwordExpiry: true,
                passwordExpiryReminder: true
            },
            triedToSubmit: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        const {props} = this;
        if (props.memberSettings) {
            this.updateMemberSettings(props.memberSettings);
            return;
        }
        props.dispatch(SecurityActions.getMemberSettings((data) => {
            this.updateMemberSettings(data.Results.Settings);
        }));
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }

    updateMemberSettings(memberSettings) {
        if (!memberSettings) {
            return;
        }
        let {state} = this;
        let membershipResetLinkValidity = memberSettings["MembershipResetLinkValidity"];
        if (membershipResetLinkValidity === "" || !re.test(membershipResetLinkValidity)) {
            state.error["membershipResetLinkValidity"] = true;
        }
        else if (membershipResetLinkValidity !== "" && re.test(membershipResetLinkValidity)) {
            state.error["membershipResetLinkValidity"] = false;
        }

        let adminMembershipResetLinkValidity = memberSettings["AdminMembershipResetLinkValidity"];
        if (adminMembershipResetLinkValidity === "" || !re.test(adminMembershipResetLinkValidity)) {
            state.error["adminMembershipResetLinkValidity"] = true;
        }
        else if (adminMembershipResetLinkValidity !== "" && re.test(adminMembershipResetLinkValidity)) {
            state.error["adminMembershipResetLinkValidity"] = false;
        }

        let membershipNumberPasswords = memberSettings["MembershipNumberPasswords"];
        if (membershipNumberPasswords === "" || !re2.test(membershipNumberPasswords)) {
            state.error["membershipNumberPasswords"] = true;
        }
        else if (membershipNumberPasswords !== "" && re2.test(membershipNumberPasswords)) {
            state.error["membershipNumberPasswords"] = false;
        }

        let membershipDaysBeforePasswordReuse = memberSettings["MembershipDaysBeforePasswordReuse"];
        if (membershipDaysBeforePasswordReuse === "" || !re2.test(membershipDaysBeforePasswordReuse)) {
            state.error["membershipDaysBeforePasswordReuse"] = true;
        }
        else if (membershipDaysBeforePasswordReuse !== "" && re2.test(membershipDaysBeforePasswordReuse)) {
            state.error["membershipDaysBeforePasswordReuse"] = false;
        }

        let passwordExpiry = memberSettings["PasswordExpiry"];
        if (passwordExpiry === "" || !re2.test(passwordExpiry)) {
            state.error["passwordExpiry"] = true;
        }
        else if (passwordExpiry !== "" && re2.test(passwordExpiry)) {
            state.error["passwordExpiry"] = false;
        }
        
        let passwordExpiryReminder = memberSettings["PasswordExpiryReminder"];
        if (passwordExpiryReminder === "" || !re2.test(passwordExpiryReminder)) {
            state.error["passwordExpiryReminder"] = true;
        }
        else if (passwordExpiryReminder !== "" && re2.test(passwordExpiryReminder)) {
            state.error["passwordExpiryReminder"] = false;
        }

        this.setState({
            memberSettings: Object.assign({}, memberSettings),
            triedToSubmit: false,
            error: state.error
        });
		logger.debug(`<<< Exiting updateMemberSettings(memberSettings=${memberSettings})`);
    }

    UNSAFE_componentWillReceiveProps(props) {
		logger.debug(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`);
        this.updateMemberSettings(props.memberSettings);
		logger.debug(`<<< Exiting UNSAFE_componentWillReceiveProps(props=${props})`);
    }

    onSettingChange(key, event) {
		logger.debug(`>>> Entering onSettingChange(key=${key},event=${event})`);
        let {state, props} = this;
        let memberSettings = Object.assign({}, state.memberSettings);
		logger.debug(`onSettingChange(key=${key},event=${event}): memberSettings → ${memberSettings}`);
        memberSettings[key] = typeof (event) === "object" ? event.target.value : event;

        this.updateMemberSettings(memberSettings);
        props.dispatch(SecurityActions.memberSettingsClientModified(memberSettings));
		logger.debug(`<<< Exiting onSettingChange(key=${key},event=${event})`);
    }

    onUpdate(event) {
		logger.debug(`>>> Entering onUpdate(event=${event})`);
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });

        if (state.error.membershipResetLinkValidity
            || state.error.adminMembershipResetLinkValidity
            || state.error.membershipNumberPasswords
            || state.error.membershipDaysBeforePasswordReuse
            || state.error.passwordExpiry
            || state.error.passwordExpiryReminder) {
            return;
        }

        let parameters = Object.assign({}, state.memberSettings);
        props.dispatch(SecurityActions.updateMemberSettings(parameters, () => {
            util.utilities.notify(resx.get("MemberSettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("MemberSettingsError"));
        }));
		logger.debug(`<<< Exiting onUpdate(event=${event})`);
    }

    onCancel() {
		logger.debug('>>> Entering onCancel()');
        const {props} = this;
        util.utilities.confirm(resx.get("MemberSettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SecurityActions.getMemberSettings((data) => {
                this.updateMemberSettings(data.Results.Settings);
				logger.debug('<<< Exiting onCancel()');
            }));
        });
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const {state} = this;
        if (state.memberSettings) {
			logger.debug('<<< Exiting render()');
            return (
                <div className={styles.memberSettings}>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plResetLinkValidity.Help") }
                            label={resx.get("plResetLinkValidity") }
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.membershipResetLinkValidity && this.state.triedToSubmit}
                            errorMessage={resx.get("MembershipResetLinkValidity.ErrorMessage") }
                            value={state.memberSettings.MembershipResetLinkValidity}
                            onChange={this.onSettingChange.bind(this, "MembershipResetLinkValidity") } />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plAdminResetLinkValidity.Help") }
                            label={resx.get("plAdminResetLinkValidity") }
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.adminMembershipResetLinkValidity && this.state.triedToSubmit}
                            errorMessage={resx.get("AdminMembershipResetLinkValidity.ErrorMessage") }
                            value={state.memberSettings.AdminMembershipResetLinkValidity}
                            onChange={this.onSettingChange.bind(this, "AdminMembershipResetLinkValidity") } />
                    </InputGroup>
                    <InputGroup>
                        <div className="memberSettings-row_switch" style={{ margin: "0 0 20px 0" }}>
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plEnablePasswordHistory.Help") }
                                label={resx.get("plEnablePasswordHistory") }
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.memberSettings.EnablePasswordHistory}
                                onChange={this.onSettingChange.bind(this, "EnablePasswordHistory") } />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plNumberPasswords.Help") }
                            label={resx.get("plNumberPasswords") }
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.membershipNumberPasswords && this.state.triedToSubmit}
                            errorMessage={resx.get("MembershipNumberPasswords.ErrorMessage") }
                            value={state.memberSettings.MembershipNumberPasswords}
                            onChange={this.onSettingChange.bind(this, "MembershipNumberPasswords") } />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plPasswordDays.Help")}
                            label={resx.get("plPasswordDays")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.membershipDaysBeforePasswordReuse && this.state.triedToSubmit}
                            errorMessage={resx.get("MembershipDaysBeforePasswordReuse.ErrorMessage")}
                            value={state.memberSettings.MembershipDaysBeforePasswordReuse}
                            onChange={this.onSettingChange.bind(this, "MembershipDaysBeforePasswordReuse")} />
                    </InputGroup>
                    <InputGroup>
                        <div className="memberSettings-row_switch" style={{ margin: "0 0 20px 0" }}>
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plEnableBannedList.Help") }
                                label={resx.get("plEnableBannedList") }
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.memberSettings.EnableBannedList}
                                onChange={this.onSettingChange.bind(this, "EnableBannedList") } />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="memberSettings-row_switch" style={{ margin: "0 0 20px 0" }}>
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plEnableStrengthMeter.Help") }
                                label={resx.get("plEnableStrengthMeter") }
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.memberSettings.EnableStrengthMeter}
                                onChange={this.onSettingChange.bind(this, "EnableStrengthMeter") } />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="memberSettings-row_switch" style={{ margin: "0 0 20px 0" }}>
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plEnableIPChecking.Help") }
                                label={resx.get("plEnableIPChecking") }
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.memberSettings.EnableIPChecking}
                                onChange={this.onSettingChange.bind(this, "EnableIPChecking") } />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="memberSettings-row_switch" style={{ margin: "0 0 20px 0" }}>
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plForceLogOutAfterPasswordChanged.Help") }
                                label={resx.get("plForceLogOutAfterPasswordChanged") }
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.memberSettings.ForceLogoutAfterPasswordChanged}
                                onChange={this.onSettingChange.bind(this, "ForceLogoutAfterPasswordChanged") } />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("PasswordConfig_PasswordExpiry.Help") }
                            label={resx.get("PasswordConfig_PasswordExpiry") }
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.passwordExpiry && this.state.triedToSubmit}
                            errorMessage={resx.get("PasswordExpiry.ErrorMessage") }
                            value={state.memberSettings.PasswordExpiry}
                            onChange={this.onSettingChange.bind(this, "PasswordExpiry") } />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("PasswordConfig_PasswordExpiryReminder.Help") }
                            label={resx.get("PasswordConfig_PasswordExpiryReminder") }
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.passwordExpiryReminder && this.state.triedToSubmit}
                            errorMessage={resx.get("PasswordExpiryReminder.ErrorMessage") }
                            value={state.memberSettings.PasswordExpiryReminder}
                            onChange={this.onSettingChange.bind(this, "PasswordExpiryReminder") } />
                    </InputGroup>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.memberSettingsClientModified}
                            type="neutral"
                            onClick={this.onCancel.bind(this) }>
                            {resx.get("Cancel") }
                        </Button>
                        <Button
                            disabled={!this.props.memberSettingsClientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this) }>
                            {resx.get("Save") }
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

MemberManagementPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    memberSettings: PropTypes.object,
    memberSettingsClientModified: PropTypes.bool
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.tabIndex,
        memberSettings: state.security.memberSettings,
        memberSettingsClientModified: state.security.memberSettingsClientModified
    };
}

export default connect(mapStateToProps)(MemberManagementPanelBody);
