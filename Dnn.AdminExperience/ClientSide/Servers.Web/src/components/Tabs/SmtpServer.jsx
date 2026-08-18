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
	logger.debug('>>> Entering componentDidMount()');

        const loadSettings = () => {
			logger.debug('>>> Entering loadSettings()');
            const selectedSmtpSettings = this.getSelectedSmtpSettings();
      
            this.onChangeAuthProvider({value: selectedSmtpSettings.authProvider}, true);
			logger.debug('<<< Exiting loadSettings()');
			logger.debug(`loadSettings(): selectedSmtpSettings → ${selectedSmtpSettings}`);
        };

        this.props.onRetrieveSmtpServerInfo(loadSettings.bind(this));
        this.props.onRetrieveAuthProviders(loadSettings.bind(this));
		logger.debug('<<< Exiting componentDidMount()');
    }

    UNSAFE_componentWillReceiveProps(newProps) {
		logger.debug(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`);
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
		logger.debug(`>>> Entering onChangeSmtpServerMode(mode=${mode})`);
        const loadSettings = () => {
            const selectedSmtpSettings = this.getSelectedSmtpSettings();
            this.onChangeAuthProvider({value: selectedSmtpSettings.authProvider}, true);
        };
    
        this.props.onChangeSmtpServerMode(mode, loadSettings.bind(this));
		logger.debug(`<<< Exiting onChangeSmtpServerMode(mode=${mode})`);
    }

    onChangeAuthenticationMode(authentication) {
		logger.debug(`>>> Entering onChangeAuthenticationMode(authentication=${authentication})`);
        this.props.onChangeSmtpAuthentication(authentication);
		logger.debug(`<<< Exiting onChangeAuthenticationMode(authentication=${authentication})`);
    }

    onChangeSmtpEnableSsl(enabled) {
		logger.debug(`>>> Entering onChangeSmtpEnableSsl(enabled=${enabled})`);
        this.props.onChangeSmtpConfigurationValue("enableSmtpSsl", enabled);
		logger.debug(`<<< Exiting onChangeSmtpEnableSsl(enabled=${enabled})`);
    }

    onChangeField(key, event) {
		logger.debug(`>>> Entering onChangeField(key=${key},event=${event})`);
        this.props.onChangeSmtpConfigurationValue(key, event.target.value);
		logger.debug(`<<< Exiting onChangeField(key=${key},event=${event})`);
    }

    onChangeAuthProvider(provider, passCheck) {
		logger.debug(`>>> Entering onChangeAuthProvider(provider=${provider},passCheck=${passCheck})`);
        const authProvider = this.getSelectedAuthProviders().filter(function (item) {
			logger.debug('>>> Entering filter()');
			logger.debug('<<< Exiting filter()');
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
		logger.debug(`<<< Exiting onChangeAuthProvider(provider=${provider},passCheck=${passCheck})`);
    }

    onChangeAuthSetting(name, event) {
		logger.debug(`>>> Entering onChangeAuthSetting(name=${name},event=${event})`);
        const { props } = this;

        const selectedSmtpSettings = this.getSelectedSmtpSettings();
		logger.debug(`onChangeAuthSetting(name=${name},event=${event}): selectedSmtpSettings → ${selectedSmtpSettings}`);
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
		logger.debug(`<<< Exiting onChangeAuthSetting(name=${name},event=${event})`);
    }

    onSave() {
		logger.debug('>>> Entering onSave()');
        const { props } = this;

        if (this.areThereValidationError()) {
            return;
        }

		logger.debug('<<< Exiting onSave()');
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
		logger.debug(`>>> Entering toDictionary(settings=${settings})`);
        let dict = {};
        settings = settings || [];
        for (let i = 0; i < settings.length; i++) {
            dict[settings[i].name] = settings[i].value;
        }

		logger.debug(`<<< Exiting toDictionary(settings=${settings})`);
        return dict;
    }

    getSelectedSmtpSettings() {
		logger.debug('>>> Entering getSelectedSmtpSettings()');
        const { props } = this;

        const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
		logger.debug(`getSelectedSmtpSettings(): areGlobalSettings → ${areGlobalSettings}`);
        const selectedSmtpSettings =
        (areGlobalSettings
            ? props.smtpServerInfo.host
            : props.smtpServerInfo.site) || {};
		logger.debug('<<< Exiting getSelectedSmtpSettings()');
        return selectedSmtpSettings;
    }

    getSelectedAuthProviders() {
		logger.debug('>>> Entering getSelectedAuthProviders()');
        const { props } = this;

        if (typeof props.smtpServerInfo === "undefined" || typeof props.authProviders === "undefined") {
            return [];
        }

        const isGlobal = props.smtpServerInfo.smtpServerMode === "h";
		logger.debug('<<< Exiting getSelectedAuthProviders()');
        return (isGlobal ? props.authProviders.host : props.authProviders.site) || [];
    }

    areThereValidationError() {
		logger.debug('>>> Entering areThereValidationError()');
        let areErrors = false;
        const errors = this.props.errors;
        for (let prop in errors) {
            if (errors[prop]) {
                return true;
            }
        }

		logger.debug('<<< Exiting areThereValidationError()');
        return areErrors;
    }

    onTestSmtpSettings() {
		logger.debug('>>> Entering onTestSmtpSettings()');
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
		logger.debug('<<< Exiting onTestSmtpSettings()');
    }

    onCompleteAuthorize() {
		logger.debug('>>> Entering onCompleteAuthorize()');
        const selectedSmtpSettings = this.getSelectedSmtpSettings();
        const authProvider = this.getSelectedAuthProviders().filter(function (item) {
            return item.name === selectedSmtpSettings.authProvider;
        }).pop();

        if (authProvider) {
            const myWindow = this.popupWindow("about:blank", "OAuth", 1090, 600);
            myWindow.location = authProvider.authorizeUrl;

            const intervalHandler = window.setInterval((() => {
                if (myWindow.window === null || myWindow.closed === true) {
                    this.props.onRetrieveAuthProviders();
                    window.clearInterval(intervalHandler);
                }
            }).bind(this), 1000);
        }
    
		logger.debug('<<< Exiting onCompleteAuthorize()');
        return false;
    }

    popupWindow(url, title, w, h) {
		logger.debug(`>>> Entering popupWindow(url=${url},title=${title},w=${w},h=${h})`);
        const left = (screen.width / 2) - (w / 2);
        const top = (screen.height / 2) - (h / 2);
		logger.debug(`popupWindow(url=${url},title=${title},w=${w},h=${h}): left → ${left}`);
		logger.debug(`popupWindow(url=${url},title=${title},w=${w},h=${h}): top → ${top}`);
		logger.debug(`<<< Exiting popupWindow(url=${url},title=${title},w=${w},h=${h})`);
        return window.open(url, title, "toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=" + w + ", height=" + h + ", top=" + top + ", left=" + left);
    }

    getSmtpServerOptions() {
		logger.debug('>>> Entering getSmtpServerOptions()');
		logger.debug('<<< Exiting getSmtpServerOptions()');
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
		logger.debug('>>> Entering getSmtpAuthenticationOptions()');
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

		logger.debug(`getSmtpAuthenticationOptions(): hasAuthProviders → ${hasAuthProviders}`);
        if (hasAuthProviders) {
            options.push({
                label: localization.get("SMTPOAUTH"),
                value: "3",
            });
        }

		logger.debug('<<< Exiting getSmtpAuthenticationOptions()');
        return options;
    }

    renderAuthSettings() {
		logger.debug('>>> Entering renderAuthSettings()');
        const { props } = this;

        const authProvidersOptions = this.getSelectedAuthProviders().map(function (item) {
			logger.debug('>>> Entering map()');
			logger.debug('<<< Exiting map()');
            return {label: item.localizedName, value: item.name};
        });

        const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
		logger.debug(`map(): areGlobalSettings → ${areGlobalSettings}`);
        const selectedSmtpSettings = this.getSelectedSmtpSettings();
        const authProvider = this.getSelectedAuthProviders().filter(function (item) {
			logger.debug(`filter(): authProvider → ${authProvider}`);
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
		logger.debug('<<< Exiting renderAuthSettings()');
    }

    render() {
		logger.debug('>>> Entering render()');
        const { props } = this;

        const areGlobalSettings = props.smtpServerInfo.smtpServerMode === "h";
		logger.debug(`render(): areGlobalSettings → ${areGlobalSettings}`);
        const selectedSmtpSettings = this.getSelectedSmtpSettings();
		logger.debug(`render(): selectedSmtpSettings → ${selectedSmtpSettings}`);
        const credentialVisible = selectedSmtpSettings.smtpAuthentication === "1";
        const oauthEnabled = selectedSmtpSettings.smtpAuthentication === "3";
		logger.debug(`render(): oauthEnabled → ${oauthEnabled}`);
        const smtpSettingsVisible = utils.isHostUser() || !areGlobalSettings;
		logger.debug(`render(): smtpSettingsVisible → ${smtpSettingsVisible}`);

        if (props.smtpServerInfo.hideCoreSettings) {
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
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
	logger.debug(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`);
	logger.debug(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`);
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
