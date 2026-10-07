import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridSystem, Button } from "@dnnsoftware/dnn-react-common";
import RadioButtonBlock from "../common/RadioButtonBlock";
import EditBlock from "../common/EditBlock";
import EditPwdBlock from "../common/EditPwdBlock";
import SwitchBlock from "../common/SwitchBlock";
import DropdownBlock from "../common/DropdownBlock";
import localization from "../../localization";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import SmtpServerTabActions from "../../actions/smtpServerTab";
import utils from "../../utils";
const logger = console;

class SmtpServer extends Component {
    componentDidMount() {
	logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });

        const loadSettings = () => {
			logger.error('>>> Entering loadSettings()', { "event.action": "loadSettings" });
            const selectedSmtpSettings = this.getSelectedSmtpSettings();
      
			logger.error(`loadSettings(): selectedSmtpSettings → ${selectedSmtpSettings}`, { "event.action": "loadSettings" });
            this.onChangeAuthProvider({value: selectedSmtpSettings.authProvider}, true);
			logger.warn('<<< Exiting loadSettings()', { "event.action": "loadSettings" });
        };

        this.props.onRetrieveSmtpServerInfo(loadSettings.bind(this));
        this.props.onRetrieveAuthProviders(loadSettings.bind(this));
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    UNSAFE_componentWillReceiveProps(newProps) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
        if (
            this.props.infoMessage !== newProps.infoMessage &&
      newProps.infoMessage
        ) {
            utils.notify(newProps.infoMessage);
        }

        if (
            this.props.errorMessage !== newProps.errorMessage &&
      newProps.errorMessage
        ) {
            utils.notifyError(newProps.errorMessage);
        }

        const selectedSmtpSettings = this.getSelectedSmtpSettings();
        if (newProps.providerChanged && selectedSmtpSettings.smtpAuthentication === "3") {
            this.props.onRetrieveAuthProviders();
        }
    }

    onChangeSmtpServerMode(mode) {
		logger.error(`>>> Entering onChangeSmtpServerMode(mode=${mode})`, { "event.action": "onChangeSmtpServerMode", "logrite.mode": mode });
        const loadSettings = () => {
            const selectedSmtpSettings = this.getSelectedSmtpSettings();
            this.onChangeAuthProvider({value: selectedSmtpSettings.authProvider}, true);
        };
    
        this.props.onChangeSmtpServerMode(mode, loadSettings.bind(this));
		logger.warn(`<<< Exiting onChangeSmtpServerMode(mode=${mode})`, { "event.action": "onChangeSmtpServerMode", "logrite.mode": mode });
    }

    onChangeAuthenticationMode(authentication) {
		logger.error(`>>> Entering onChangeAuthenticationMode(authentication=${authentication})`, { "event.action": "onChangeAuthenticationMode", "logrite.authentication": authentication });
        this.props.onChangeSmtpAuthentication(authentication);
		logger.warn(`<<< Exiting onChangeAuthenticationMode(authentication=${authentication})`, { "event.action": "onChangeAuthenticationMode", "logrite.authentication": authentication });
    }

    onChangeSmtpEnableSsl(enabled) {
		logger.error(`>>> Entering onChangeSmtpEnableSsl(enabled=${enabled})`, { "event.action": "onChangeSmtpEnableSsl", "logrite.enabled": enabled });
        this.props.onChangeSmtpConfigurationValue("enableSmtpSsl", enabled);
		logger.warn(`<<< Exiting onChangeSmtpEnableSsl(enabled=${enabled})`, { "event.action": "onChangeSmtpEnableSsl", "logrite.enabled": enabled });
    }

    onChangeField(key, event) {
		logger.error(`>>> Entering onChangeField(key=${key},event=${event})`, { "event.action": "onChangeField", "logrite.key": key, "logrite.event": event });
        this.props.onChangeSmtpConfigurationValue(key, event.target.value);
		logger.warn(`<<< Exiting onChangeField(key=${key},event=${event})`, { "event.action": "onChangeField", "logrite.key": key, "logrite.event": event });
    }

    onChangeAuthProvider(provider, passCheck) {
		logger.error(`>>> Entering onChangeAuthProvider(provider=${provider},passCheck=${passCheck})`, { "event.action": "onChangeAuthProvider", "logrite.provider": provider, "logrite.passCheck": passCheck });
        const authProvider = this.getSelectedAuthProviders().filter(function (item) {
		logger.error(`onChangeAuthProvider(provider=${provider},passCheck=${passCheck}): authProvider → ${authProvider}`, { "event.action": "onChangeAuthProvider", "logrite.provider": provider, "logrite.passCheck": passCheck });
            return item.name === provider.value;
        }).pop();

        this.props.onChangeSmtpConfigurationValue("authProvider", provider.value, passCheck);

        //initialize settings
        let settings = [];
        if (typeof authProvider !== "undefined") {
            for (let i = 0; i < authProvider.settings.length; i++) {
                const setting = authProvider.settings[i];
                settings.push({
                    name: setting.name, 
                    value: setting.value, 
                    label: setting.label,
                    help: setting.help,
                    isSecure: setting.isSecure,
                    isRequired: setting.isRequired
                });
            }
        }
        this.props.onChangeSmtpConfigurationValue("authProviderSettings", settings, passCheck);
    }

    onChangeAuthSetting(name, event) {
		logger.error(`>>> Entering onChangeAuthSetting(name=${name},event=${event})`, { "event.action": "onChangeAuthSetting", "logrite.name": name, "logrite.event": event });
        const { props } = this;

        const selectedSmtpSettings = this.getSelectedSmtpSettings();
		logger.error(`onChangeAuthSetting(name=${name},event=${event}): selectedSmtpSettings → ${selectedSmtpSettings}`, { "event.action": "onChangeAuthSetting", "logrite.name": name, "logrite.event": event });
        const settings = selectedSmtpSettings.authProviderSettings;

        let newSettings = [];
        for (let i = 0; i < settings.length; i++) {
            newSettings.push({
                name: settings[i].name,
                value: settings[i].name === name ? event.target.value : settings[i].value,
                label: settings[i].label,
                help: settings[i].help,
                isSecure: settings[i].isSecure,
                isRequired: settings[i].isRequired
            });
        }

        props.onChangeSmtpConfigurationValue("authProviderSettings", newSettings);
		logger.warn(`<<< Exiting onChangeAuthSetting(name=${name},event=${event})`, { "event.action": "onChangeAuthSetting", "logrite.name": name, "logrite.event": event });
    }

    onSave() {
		logger.error('>>> Entering onSave()', { "event.action": "onSave" });
        const { props } = this;

        if (this.areThereValidationError()) {
            return;
        }

		logger.warn('<<< Exiting onSave()', { "event.action": "onSave" });
        const smtpSettings =
      props.smtpServerInfo.smtpServerMode === "h" && utils.isHostUser()
          ? props.smtpServerInfo.host
          : props.smtpServerInfo.site;

        const updateRequest = {
            smtpServerMode: props.smtpServerInfo.smtpServerMode,
            smtpServer: smtpSettings.smtpServer,
            smtpConnectionLimit: smtpSettings.smtpConnectionLimit,
            smtpMaxIdleTime: smtpSettings.smtpMaxIdleTime,
            smtpAuthentication: smtpSettings.smtpAuthentication,
            smtpUsername: smtpSettings.smtpUserName,
            smtpPassword: smtpSettings.smtpPassword,
            smtpHostEmail: smtpSettings.smtpHostEmail,
            enableSmtpSsl: smtpSettings.enableSmtpSsl,
            authProvider: smtpSettings.authProvider,
            AuthProviderSettings: this.toDictionary(smtpSettings.authProviderSettings),
            messageSchedulerBatchSize:
        props.smtpServerInfo.host.messageSchedulerBatchSize,
        };
        props.onUpdateSmtpServerSettings(updateRequest, this.onSettingsUpdated);
    }

    toDictionary(settings) {
		logger.error(`>>> Entering toDictionary(settings=${settings})`, { "event.action": "toDictionary", "logrite.settings": settings });
        let dict = {};
        settings = settings || [];
        for (let i = 0; i < settings.length; i++) {
            dict[settings[i].name] = settings[i].value;
        }

		logger.warn(`<<< Exiting toDictionary(settings=${settings})`, { "event.action": "toDictionary", "logrite.settings": settings });
        return dict;
    }

    getSelectedSmtpSettings() {
		logger.error('>>> Entering getSelectedSmtpSettings()', { "event.action": "getSelectedSmtpSettings" });
        const { props } = this;

        const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
		logger.error(`getSelectedSmtpSettings(): areGlobalSettings → ${areGlobalSettings}`, { "event.action": "getSelectedSmtpSettings" });
        const selectedSmtpSettings =
        (areGlobalSettings
            ? props.smtpServerInfo.host
            : props.smtpServerInfo.site) || {};
		logger.warn('<<< Exiting getSelectedSmtpSettings()', { "event.action": "getSelectedSmtpSettings" });
        return selectedSmtpSettings;
    }

    getSelectedAuthProviders() {
		logger.error('>>> Entering getSelectedAuthProviders()', { "event.action": "getSelectedAuthProviders" });
        const { props } = this;

        if (typeof props.smtpServerInfo === "undefined" || typeof props.authProviders === "undefined") {
            return [];
        }

        const isGlobal = props.smtpServerInfo.smtpServerMode === "h";
		logger.warn('<<< Exiting getSelectedAuthProviders()', { "event.action": "getSelectedAuthProviders" });
        return (isGlobal ? props.authProviders.host : props.authProviders.site) || [];
    }

    areThereValidationError() {
		logger.error('>>> Entering areThereValidationError()', { "event.action": "areThereValidationError" });
        let areErrors = false;
        const errors = this.props.errors;
        for (let prop in errors) {
            if (errors[prop]) {
                return true;
            }
        }

		logger.warn('<<< Exiting areThereValidationError()', { "event.action": "areThereValidationError" });
        return areErrors;
    }

    onTestSmtpSettings() {
		logger.error('>>> Entering onTestSmtpSettings()', { "event.action": "onTestSmtpSettings" });
        const { props } = this;

        if (this.areThereValidationError()) {
            return;
        }

        let smtpSettings = {};
        if (props.smtpServerInfo.smtpServerMode === "h" && utils.isHostUser()) {
            smtpSettings = props.smtpServerInfo.host;
        }
        if (props.smtpServerInfo.smtpServerMode === "p") {
            smtpSettings = props.smtpServerInfo.site;
        }
        const authProvider = this.getSelectedAuthProviders().filter(function (item) {
		logger.error(`onTestSmtpSettings(): authProvider → ${authProvider}`, { "event.action": "onTestSmtpSettings" });
            return item.name === smtpSettings.authProvider;
        }).pop() || {};

        if (smtpSettings.smtpAuthentication === "3" && (props.isDirty || !authProvider.isAuthorized)) {
            utils.notifyError(localization.get("OAuthConfigurationNotSaved"));
            return;
        }

        const sendEmailRequest = {
            smtpServerMode: props.smtpServerInfo.smtpServerMode,
            smtpServer: smtpSettings.smtpServer,
            smtpAuthentication: smtpSettings.smtpAuthentication,
            smtpUsername: smtpSettings.smtpUserName,
            smtpPassword: smtpSettings.smtpPassword,
            enableSmtpSsl: smtpSettings.enableSmtpSsl,
            authProvider: smtpSettings.authProvider
        };
        props.onSendTestEmail(sendEmailRequest);
		logger.warn('<<< Exiting onTestSmtpSettings()', { "event.action": "onTestSmtpSettings" });
    }

    onCompleteAuthorize() {
		logger.error('>>> Entering onCompleteAuthorize()', { "event.action": "onCompleteAuthorize" });
        const selectedSmtpSettings = this.getSelectedSmtpSettings();
        const authProvider = this.getSelectedAuthProviders().filter(function (item) {
			logger.error(`onCompleteAuthorize(): selectedSmtpSettings → ${selectedSmtpSettings}`, { "event.action": "onCompleteAuthorize" });
		logger.error(`onCompleteAuthorize(): authProvider → ${authProvider}`, { "event.action": "onCompleteAuthorize" });
            return item.name === selectedSmtpSettings.authProvider;
        }).pop();

        if (authProvider) {
            const myWindow = this.popupWindow("about:blank", "OAuth", 1090, 600);
			logger.error(`onCompleteAuthorize(): myWindow → ${myWindow}`, { "event.action": "onCompleteAuthorize" });
            myWindow.location = authProvider.authorizeUrl;
			logger.debug(`onCompleteAuthorize(): myWindow.location → ${myWindow.location}`, { "event.action": "onCompleteAuthorize" });

            const intervalHandler = window.setInterval((() => {
                if (myWindow.window === null || myWindow.closed === true) {
                    this.props.onRetrieveAuthProviders();
                    window.clearInterval(intervalHandler);
                }
            }).bind(this), 1000);
        }
    
		logger.warn('<<< Exiting onCompleteAuthorize()', { "event.action": "onCompleteAuthorize" });
        return false;
    }

    popupWindow(url, title, w, h) {
		logger.error(`>>> Entering popupWindow(url=${url},title=${title},w=${w},h=${h})`, { "event.action": "popupWindow", "logrite.url": url, "logrite.title": title, "logrite.w": w, "logrite.h": h });
        const left = (screen.width / 2) - (w / 2);
        const top = (screen.height / 2) - (h / 2);
		logger.error(`popupWindow(url=${url},title=${title},w=${w},h=${h}): left → ${left}`, { "event.action": "popupWindow", "logrite.url": url, "logrite.title": title, "logrite.w": w, "logrite.h": h });
		logger.error(`popupWindow(url=${url},title=${title},w=${w},h=${h}): top → ${top}`, { "event.action": "popupWindow", "logrite.url": url, "logrite.title": title, "logrite.w": w, "logrite.h": h });
        return window.open(url, title, "toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=" + w + ", height=" + h + ", top=" + top + ", left=" + left);
    }

    getSmtpServerOptions() {
		logger.error('>>> Entering getSmtpServerOptions()', { "event.action": "getSmtpServerOptions" });
		logger.warn('<<< Exiting getSmtpServerOptions()', { "event.action": "getSmtpServerOptions" });
        return [
            {
                label: localization.get("GlobalSmtpHostSetting"),
                value: "h",
            },
            {
                label: localization
                    .get("SiteSmtpHostSetting")
                    .replace("{0}", this.props.smtpServerInfo.portalName || ""),
                value: "p",
            },
        ];
    }

    getSmtpAuthenticationOptions() {
		logger.error('>>> Entering getSmtpAuthenticationOptions()', { "event.action": "getSmtpAuthenticationOptions" });
        const hasAuthProviders = this.getSelectedAuthProviders().length > 0;
        let options = [
            {
                label: localization.get("SMTPAnonymous"),
                value: "0",
            },
            {
                label: localization.get("SMTPBasic"),
                value: "1",
            },
            {
                label: localization.get("SMTPNTLM"),
                value: "2",
            }
        ];

		logger.error(`getSmtpAuthenticationOptions(): hasAuthProviders → ${hasAuthProviders}`, { "event.action": "getSmtpAuthenticationOptions" });
        if (hasAuthProviders) {
            options.push({
                label: localization.get("SMTPOAUTH"),
                value: "3",
            });
        }

		logger.warn('<<< Exiting getSmtpAuthenticationOptions()', { "event.action": "getSmtpAuthenticationOptions" });
        return options;
    }

    renderAuthSettings() {
		logger.error('>>> Entering renderAuthSettings()', { "event.action": "renderAuthSettings" });
        const { props } = this;

        const authProvidersOptions = this.getSelectedAuthProviders().map(function (item) {
		logger.error(`renderAuthSettings(): authProvidersOptions → ${authProvidersOptions}`, { "event.action": "renderAuthSettings" });
            return {label: item.localizedName, value: item.name};
        });

        const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
		logger.error(`renderAuthSettings(): areGlobalSettings → ${areGlobalSettings}`, { "event.action": "renderAuthSettings" });
        const selectedSmtpSettings = this.getSelectedSmtpSettings();
		logger.error(`renderAuthSettings(): selectedSmtpSettings → ${selectedSmtpSettings}`, { "event.action": "renderAuthSettings" });
        const authProvider = this.getSelectedAuthProviders().filter(function (item) {
		logger.error(`renderAuthSettings(): authProvider → ${authProvider}`, { "event.action": "renderAuthSettings" });
            return item.name === selectedSmtpSettings.authProvider;
        }).pop();

        let settingFields = null;
        if (typeof authProvider !== "undefined") {
            const settings = selectedSmtpSettings.authProviderSettings || [];

            settingFields = settings.map(function (setting) {
                return <EditBlock
                    key={setting.name}
                    label={setting.label}
                    tooltip={setting.help}
                    value={setting.value}
                    isGlobal={areGlobalSettings}
                    onChange={this.onChangeAuthSetting.bind(this, setting.name)}
                    type={setting.isSecure ? "password" : "text"}
                />;
            }.bind(this));
        }

        const settingCompleted = (selectedSmtpSettings.authProviderSettings || []).filter(i => i.isRequired && (i.value || "") === "").length === 0;
		logger.warn('<<< Exiting renderAuthSettings()', { "event.action": "renderAuthSettings" });
        return <div>
            <div style={{paddingBottom: "22px", float: "left", width: "100%"}}>
                <DropdownBlock
                    tooltip={localization.get("SmtpTab_OAuthProviders.Help")}
                    label={localization.get("SmtpTab_OAuthProviders")}
                    options={authProvidersOptions}
                    value={selectedSmtpSettings.authProvider}
                    onSelect={this.onChangeAuthProvider.bind(this)} />
            </div>
            {settingFields}
            {authProvider && !authProvider.isAuthorized && authProvider.authorizeUrl && settingCompleted && !props.isDirty && 
              <div className="warningBox authorize-box">
                  <div className="warningText">
                      {localization.get("CompleteAuthorize")}
                  </div>
                  <div className="warningButton">
                      <Button
                          type="secondary"
                          onClick={this.onCompleteAuthorize.bind(this)}
                      >
                          {localization.get("Authorize")}
                      </Button>
                  </div>
              </div>
            }
            {authProvider && authProvider.isAuthorized && !props.isDirty && 
              <div className="warningBox authorize-box success">
                  <div className="warningText">
                      {localization.get("AuthorizeCompleted")}
                  </div>
              </div>
            }
        </div>;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;

        const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
		logger.error(`render(): areGlobalSettings → ${areGlobalSettings}`, { "event.action": "render" });
        const selectedSmtpSettings = this.getSelectedSmtpSettings();
		logger.error(`render(): selectedSmtpSettings → ${selectedSmtpSettings}`, { "event.action": "render" });
        const credentialVisible = selectedSmtpSettings.smtpAuthentication === "1";
        const oauthEnabled = selectedSmtpSettings.smtpAuthentication === "3";
		logger.error(`render(): oauthEnabled → ${oauthEnabled}`, { "event.action": "render" });
        const smtpSettingsVisible = utils.isHostUser() || !areGlobalSettings;
		logger.error(`render(): smtpSettingsVisible → ${smtpSettingsVisible}`, { "event.action": "render" });

        if (props.smtpServerInfo.hideCoreSettings) {
            return (
                <div className="dnn-servers-info-panel-big smtpServerSettingsTab">
                    <div className="warningBox">
                        <div className="warningText">
                            {localization.get("NonCoreMailProvider")}
                        </div>
                    </div>
                </div>
            );
        }

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="dnn-servers-info-panel-big smtpServerSettingsTab">
                <GridSystem>
                    <div className="leftPane">
                        <div className="tooltipAdjustment border-bottom">
                            <RadioButtonBlock
                                options={this.getSmtpServerOptions()}
                                label={localization.get("plSMTPMode")}
                                tooltip={localization.get("plSMTPMode.Help")}
                                onChange={this.onChangeSmtpServerMode.bind(this)}
                                value={props.smtpServerInfo.smtpServerMode}
                            />
                        </div>
                        <div className="tooltipAdjustment">
                            {smtpSettingsVisible && (
                                <div>
                                    <EditBlock
                                        label={localization.get("plSMTPServer")}
                                        tooltip={localization.get("plSMTPServer.Help")}
                                        value={selectedSmtpSettings.smtpServer}
                                        isGlobal={areGlobalSettings}
                                        onChange={this.onChangeField.bind(this, "smtpServer")}
                                        error={props.errors["smtpServer"]}
                                    />

                                    <EditBlock
                                        label={localization.get("plConnectionLimit")}
                                        tooltip={localization.get("plConnectionLimit.Help")}
                                        value={selectedSmtpSettings.smtpConnectionLimit}
                                        isGlobal={areGlobalSettings}
                                        onChange={this.onChangeField.bind(
                                            this,
                                            "smtpConnectionLimit"
                                        )}
                                        error={props.errors["smtpConnectionLimit"]}
                                    />

                                    <EditBlock
                                        label={localization.get("plMaxIdleTime")}
                                        tooltip={localization.get("plMaxIdleTime.Help")}
                                        value={selectedSmtpSettings.smtpMaxIdleTime}
                                        isGlobal={areGlobalSettings}
                                        onChange={this.onChangeField.bind(this, "smtpMaxIdleTime")}
                                        error={props.errors["smtpMaxIdleTime"]}
                                    />
                                </div>
                            )}
                            {smtpSettingsVisible && areGlobalSettings && (
                                <EditBlock
                                    label={localization.get("plBatch")}
                                    tooltip={localization.get("plBatch.Help")}
                                    value={props.smtpServerInfo.host.messageSchedulerBatchSize}
                                    isGlobal={areGlobalSettings}
                                    onChange={this.onChangeField.bind(
                                        this,
                                        "messageSchedulerBatchSize"
                                    )}
                                    error={props.errors["messageSchedulerBatchSize"]}
                                />
                            )}
                        </div>
                    </div>
                    <div className="rightPane">
                        {smtpSettingsVisible && (
                            <div className="tooltipAdjustment border-bottom smtp-authentication-mode">
                                <RadioButtonBlock
                                    options={this.getSmtpAuthenticationOptions()}
                                    label={localization.get("plSMTPAuthentication")}
                                    tooltip={localization.get("plSMTPAuthentication.Help")}
                                    onChange={this.onChangeAuthenticationMode.bind(this)}
                                    value={selectedSmtpSettings.smtpAuthentication || "0"}
                                    isGlobal={areGlobalSettings}
                                />
                            </div>
                        )}
                        {smtpSettingsVisible && credentialVisible && (
                            <div className="tooltipAdjustment">
                                <EditBlock
                                    label={localization.get("plSMTPUsername")}
                                    tooltip={localization.get("plSMTPUsername.Help")}
                                    value={selectedSmtpSettings.smtpUserName}
                                    isGlobal={areGlobalSettings}
                                    onChange={this.onChangeField.bind(this, "smtpUserName")}
                                    error={props.errors["smtpUserName"]}
                                />

                                <EditPwdBlock
                                    label={localization.get("plSMTPPassword")}
                                    changeButtonText={localization.get("Change")}
                                    tooltip={localization.get("plSMTPPassword.Help")}
                                    value={selectedSmtpSettings.smtpPassword}
                                    isGlobal={areGlobalSettings}
                                    onChange={this.onChangeField.bind(this, "smtpPassword")}
                                    onClear={() =>
                                        props.onChangeSmtpConfigurationValue("smtpPassword", "")
                                    }
                                    error={props.errors["smtpPassword"]}
                                />
                            </div>
                        )}
                        {smtpSettingsVisible && !oauthEnabled && (
                            <div className="tooltipAdjustment border-bottom" style={{paddingBottom: "22px"}}>
                                <SwitchBlock
                                    label={localization.get("plSMTPEnableSSL")}
                                    onText={localization.get("SwitchOn")}
                                    offText={localization.get("SwitchOff")}
                                    tooltip={localization.get("plSMTPEnableSSL.Help")}
                                    value={selectedSmtpSettings.enableSmtpSsl}
                                    onChange={this.onChangeSmtpEnableSsl.bind(this)}
                                    isGlobal={areGlobalSettings}
                                />
                            </div>
                        )}
                        {oauthEnabled && (
                            <div className="tooltipAdjustment border-bottom" style={{paddingBottom: "22px"}}>
                                {this.renderAuthSettings()}
                            </div>
                        )}
                        {smtpSettingsVisible && areGlobalSettings && (
                            <EditBlock
                                label={localization.get("plHostEmail")}
                                tooltip={localization.get("plHostEmail.Help")}
                                value={selectedSmtpSettings.smtpHostEmail}
                                isGlobal={true}
                                onChange={this.onChangeField.bind(this, "smtpHostEmail")}
                                error={props.errors["smtpHostEmail"]}
                            />
                        )}
                    </div>
                </GridSystem>
                <div className="clear" />
                <div className="buttons-panel">
                    <Button type="secondary" onClick={this.onTestSmtpSettings.bind(this)}>
                        {localization.get("EmailTest")}
                    </Button>
                    <Button type="primary" onClick={this.onSave.bind(this)} disabled={!props.isDirty}>
                        {localization.get("SaveButtonText")}
                    </Button>
                </div>
            </div>
        );
    }
}

SmtpServer.propTypes = {
    smtpServerInfo: PropTypes.object.isRequired,
    authProviders: PropTypes.object,
    authProvider: PropTypes.string,
    errorMessage: PropTypes.string,
    onRetrieveSmtpServerInfo: PropTypes.func.isRequired,
    onRetrieveAuthProviders: PropTypes.func.isRequired,
    onChangeSmtpServerMode: PropTypes.func.isRequired,
    onChangeSmtpAuthentication: PropTypes.func.isRequired,
    onChangeSmtpConfigurationValue: PropTypes.func.isRequired,
    onUpdateSmtpServerSettings: PropTypes.func.isRequired,
    infoMessage: PropTypes.string,
    onSendTestEmail: PropTypes.func.isRequired,
    errors: PropTypes.array,
    isDirty: PropTypes.bool
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        smtpServerInfo: state.smtpServer.smtpServerInfo,
        authProviders: state.smtpServer.authProviders,
        errorMessage: state.smtpServer.errorMessage,
        infoMessage: state.smtpServer.infoMessage,
        success: state.smtpServer.success,
        providerChanged: state.smtpServer.providerChanged,
        errors: state.smtpServer.errors,
        isDirty: state.smtpServer.isDirty
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return {
        ...bindActionCreators(
            {
                onRetrieveSmtpServerInfo: SmtpServerTabActions.loadSmtpServerInfo,
                onRetrieveAuthProviders: SmtpServerTabActions.loadAuthProviders,
                onChangeSmtpServerMode: SmtpServerTabActions.changeSmtpServerMode,
                onChangeSmtpAuthentication:
          SmtpServerTabActions.changeSmtpAuthentication,
                onChangeSmtpConfigurationValue:
          SmtpServerTabActions.changeSmtpConfigurationValue,
                onUpdateSmtpServerSettings:
          SmtpServerTabActions.updateSmtpServerSettings,
                onSendTestEmail: SmtpServerTabActions.sendTestEmail,
            },
            dispatch
        ),
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(SmtpServer);
