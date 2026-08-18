import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import { 
    InputGroup,
    Switch,
    Label,
    Button,
    SingleLineInputWithError,
    MultiLineInputWithError,
    Tooltip
} from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

const re = /^[1-9][0-9]?[0-9]?$|^0$/;
const re2 = /^([9]\d|\d{3,4})$/;
const re3 = /^([1][2-9]|[2-9][0-9]|\d{3,})$/;

class OtherSettingsPanelBody extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            otherSettings: undefined,
            error: {
                autoAccountUnlockDuration: false,
                asyncTimeout: false,
                maxUploadSize: false
            },
            triedToSubmit: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        const {props} = this;
        if (props.otherSettings) {
            this.setState({
                otherSettings: props.otherSettings
            });
            return;
        }
        props.dispatch(SecurityActions.getOtherSettings((data) => {
            let otherSettings = Object.assign({}, data.Results.Settings);
            this.setState({
                otherSettings
            });
        }));
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }

    UNSAFE_componentWillReceiveProps(props) {
		logger.debug(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`);
        let {state} = this;

        let autoAccountUnlockDuration = props.otherSettings["AutoAccountUnlockDuration"];
		logger.debug(`UNSAFE_componentWillReceiveProps(props=${props}): autoAccountUnlockDuration → ${autoAccountUnlockDuration}`);
        if (autoAccountUnlockDuration === "" || !re.test(autoAccountUnlockDuration)) {
            state.error["autoAccountUnlockDuration"] = true;
        }
        else if (autoAccountUnlockDuration !== "" && re.test(autoAccountUnlockDuration)) {
            state.error["autoAccountUnlockDuration"] = false;
        }
        let asyncTimeout = props.otherSettings["AsyncTimeout"];
		logger.debug(`UNSAFE_componentWillReceiveProps(props=${props}): asyncTimeout → ${asyncTimeout}`);
        if (asyncTimeout === "" || !re2.test(asyncTimeout)) {
            state.error["asyncTimeout"] = true;
        }
        else if (asyncTimeout !== "" && re2.test(asyncTimeout)) {
            state.error["asyncTimeout"] = false;
        }
        let maxUploadSize = props.otherSettings["MaxUploadSize"];
		logger.debug(`UNSAFE_componentWillReceiveProps(props=${props}): maxUploadSize → ${maxUploadSize}`);
        if (!this.isMaxUploadSizeValid(maxUploadSize)) {
            state.error["maxUploadSize"] = true;
        }
        else if (this.isMaxUploadSizeValid(maxUploadSize)) {
            state.error["maxUploadSize"] = false;
			logger.debug(`<<< Exiting UNSAFE_componentWillReceiveProps(props=${props})`);
        }

        this.setState({
            otherSettings: Object.assign({}, props.otherSettings),
            triedToSubmit: false,
            error: state.error
        });
    }

    isMaxUploadSizeValid(size) {
		logger.debug(`>>> Entering isMaxUploadSizeValid(size=${size})`);
        let {props} = this;
        if (props.otherSettings === undefined) {
            return true;
        }
        let rangeUploadSize = parseInt(props.otherSettings["RangeUploadSize"]);
        if (size === "") {
            return false;
        }
        else {
            let maxUploadSize = parseInt(size);
            if (!re3.test(maxUploadSize) || maxUploadSize > rangeUploadSize) {
                return false;
            }
            else {
				logger.debug(`<<< Exiting isMaxUploadSizeValid(size=${size})`);
                return true;
            }
        }
    }

    onSettingChange(key, event) {
		logger.debug(`>>> Entering onSettingChange(key=${key},event=${event})`);
        let {state, props} = this;
        let otherSettings = Object.assign({}, state.otherSettings);
		logger.debug(`onSettingChange(key=${key},event=${event}): otherSettings → ${otherSettings}`);
        otherSettings[key] = typeof (event) === "object" ? event.target.value : event;

        if (!re.test(otherSettings[key]) && key === "AutoAccountUnlockDuration") {
            state.error["autoAccountUnlockDuration"] = true;
        }
        else if (re.test(otherSettings[key]) && key === "AutoAccountUnlockDuration") {
            state.error["autoAccountUnlockDuration"] = false;
        }
        if (!re2.test(otherSettings[key]) && key === "AsyncTimeout") {
            state.error["asyncTimeout"] = true;
        }
        else if (re2.test(otherSettings[key]) && key === "AsyncTimeout") {
            state.error["asyncTimeout"] = false;
        }
        if (key === "MaxUploadSize" && !this.isMaxUploadSizeValid(otherSettings[key])) {
            state.error["maxUploadSize"] = true;
        }
        else if (key === "MaxUploadSize" && this.isMaxUploadSizeValid(otherSettings[key])) {
            state.error["maxUploadSize"] = false;
        }

        this.setState({
            otherSettings: otherSettings,
            error: state.error,
            triedToSubmit: false
        });
        props.dispatch(SecurityActions.otherSettingsClientModified(otherSettings));
		logger.debug(`<<< Exiting onSettingChange(key=${key},event=${event})`);
    }

    onUpdate(event) {
		logger.debug(`>>> Entering onUpdate(event=${event})`);
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });

        if (state.error.autoAccountUnlockDuration || state.error.asyncTimeout || state.error.maxUploadSize) {
            return;
        }

        let parameters = Object.assign({}, state.otherSettings);
        props.dispatch(SecurityActions.updateOtherSettings(parameters, () => {
            util.utilities.notify(resx.get("OtherSettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("OtherSettingsError"));
        }));
		logger.debug(`<<< Exiting onUpdate(event=${event})`);
    }

    onCancel() {
		logger.debug('>>> Entering onCancel()');
        const {props} = this;
        util.utilities.confirm(resx.get("OtherSettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SecurityActions.getOtherSettings((data) => {
				logger.debug('<<< Exiting onCancel()');
                let otherSettings = Object.assign({}, data.Results.Settings);
                this.setState({
                    otherSettings
                });
            }));
        });
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const { state } = this;
        if (state.otherSettings) {
			logger.debug('<<< Exiting render()');
            return (
                <div className={styles.otherSettings}>
                    <InputGroup>
                        <div className="otherSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plShowCriticalErrors.Help")}
                                label={resx.get("plShowCriticalErrors")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.otherSettings.ShowCriticalErrors}
                                onChange={this.onSettingChange.bind(this, "ShowCriticalErrors")} />
                        </div>
                    </InputGroup>
                    <InputGroup> 
                        <div className="otherSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plDebugMode.Help")}
                                label={resx.get("plDebugMode")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.otherSettings.DebugMode}
                                onChange={this.onSettingChange.bind(this, "DebugMode")} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="otherSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plRememberMe.Help")}
                                label={resx.get("plRememberMe")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.otherSettings.RememberCheckbox}
                                onChange={this.onSettingChange.bind(this, "RememberCheckbox")} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="otherSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plAllowOverrideThemeViaQueryString.Help")}
                                label={resx.get("plAllowOverrideThemeViaQueryString")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }}
                                    />
                                }
                            />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.otherSettings.AllowOverrideThemeViaQueryString}
                                onChange={this.onSettingChange.bind(this, "AllowOverrideThemeViaQueryString")}
                            />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="otherSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plAllowRichTextModuleTitle.Help")}
                                label={resx.get("plAllowRichTextModuleTitle")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }}
                                    />
                                }
                            />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.otherSettings.AllowRichTextModuleTitle}
                                onChange={this.onSettingChange.bind(this, "AllowRichTextModuleTitle")}
                            />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plAutoAccountUnlock.Help")}
                            label={resx.get("plAutoAccountUnlock")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.autoAccountUnlockDuration && this.state.triedToSubmit}
                            errorMessage={resx.get("AutoAccountUnlockDuration.ErrorMessage")}
                            value={state.otherSettings.AutoAccountUnlockDuration}
                            onChange={this.onSettingChange.bind(this, "AutoAccountUnlockDuration")} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plAsyncTimeout.Help")}
                            label={resx.get("plAsyncTimeout")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.asyncTimeout && this.state.triedToSubmit}
                            errorMessage={resx.get("AsyncTimeout.ErrorMessage")}
                            value={state.otherSettings.AsyncTimeout}
                            onChange={this.onSettingChange.bind(this, "AsyncTimeout")} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plMaxUploadSize.Help")}
                            label={resx.get("plMaxUploadSize")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <SingleLineInputWithError
                            withLabel={false}
                            error={this.state.error.maxUploadSize && this.state.triedToSubmit}
                            errorMessage={resx.get("maxUploadSize.Error").replace("{0}", state.otherSettings.RangeUploadSize)}
                            value={state.otherSettings.MaxUploadSize}
                            onChange={this.onSettingChange.bind(this, "MaxUploadSize")} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plFileExtensions.Help")}
                            label={resx.get("plFileExtensions")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <MultiLineInputWithError
                            value={state.otherSettings.AllowedExtensionWhitelist}
                            onChange={this.onSettingChange.bind(this, "AllowedExtensionWhitelist")} />
                    </InputGroup>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plDefaultEndUserExtensionWhitelist.Help")}
                            label={resx.get("plDefaultEndUserExtensionWhitelist")}
                            extra={
                                <Tooltip
                                    messages={[resx.get("GlobalSetting")]}
                                    type="global"
                                    style={{ float: "left", position: "static" }} />
                            } />
                        <MultiLineInputWithError
                            value={state.otherSettings.DefaultEndUserExtensionWhitelist}
                            onChange={this.onSettingChange.bind(this, "DefaultEndUserExtensionWhitelist")} />
                    </InputGroup>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.otherSettingsClientModified}
                            type="neutral"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.otherSettingsClientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

OtherSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    otherSettings: PropTypes.object,
    otherSettingsClientModified: PropTypes.bool
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.tabIndex,
        otherSettings: state.security.otherSettings,
        otherSettingsClientModified: state.security.otherSettingsClientModified
    };
}

export default connect(mapStateToProps)(OtherSettingsPanelBody);