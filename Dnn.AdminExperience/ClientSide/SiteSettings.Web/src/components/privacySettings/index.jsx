import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { siteBehavior as SiteBehaviorActions } from "../../actions";
import {
    InputGroup,
    Switch,
    GridSystem,
    Tooltip,
    Label,
    Button,
    SingleLineInputWithError,
    PagePicker,
    Dropdown
} from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

let isHost = false;

class PrivacySettingsPanelBody extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            privacySettings: undefined
        };
        isHost = util.settings.isHost;
		logger.debug('<<< Exiting constructor()');
    }

    loadData() {
		logger.debug('>>> Entering loadData()');
        const { props } = this;
        props.dispatch(
            SiteBehaviorActions.getPrivacySettings(props.portalId, data => {
                this.setState({
                    privacySettings: Object.assign({}, data.Settings)
                });
            })
        );
		logger.debug('<<< Exiting loadData()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.loadData();
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        const { props } = this;
        if (props.privacySettings) {
            let portalIdChanged = props.portalId !== undefined && prevProps.portalId !== props.portalId;
            let cultureCodeChanged = props.cultureCode !== undefined && prevProps.cultureCode !== props.cultureCode;

            if (portalIdChanged || cultureCodeChanged) {
                this.loadData();
				logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
            }
        }
    }

    getUserDeleteOptions() {
		logger.debug('>>> Entering getUserDeleteOptions()');
		logger.debug('<<< Exiting getUserDeleteOptions()');
        return [
            { label: resx.get("Off"), value: 0 },
            { label: resx.get("DataConsentUserDeleteManual"), value: 1 },
            { label: resx.get("DataConsentUserDelayedHardDelete"), value: 2 },
            { label: resx.get("DataConsentUserHardDelete"), value: 3 }
        ];
    }

    getTimeLapseMeasurements() {
		logger.debug('>>> Entering getTimeLapseMeasurements()');
		logger.debug('<<< Exiting getTimeLapseMeasurements()');
        return [
            { value: "h", label: resx.get("Hours") },
            { value: "d", label: resx.get("Days") },
            { value: "w", label: resx.get("Weeks") }
        ];
    }

    onDataConsentResetTerms() {
		logger.debug('>>> Entering onDataConsentResetTerms()');
        util.utilities.confirm(
            resx.get("DataConsentResetTerms.Confirm"),
            resx.get("Yes"),
            resx.get("No"),
            function onDataConsentResetTermsConfirm() {
				logger.debug('>>> Entering onDataConsentResetTermsConfirm()');
                SiteBehaviorActions.resetTermsAgreement(
                    {
                        PortalId: this.state.privacySettings.PortalId
                    },
                    () => {
                        util.utilities.notify(resx.get("DataConsentResetTerms.Completed"));
						logger.debug('<<< Exiting onDataConsentResetTermsConfirm()');
                    }
                );
            }.bind(this)
        );
    }

    onSettingChange(key, event) {
        let { state, props } = this;
        let privacySettings = Object.assign({}, state.privacySettings);

        if (
            key === "DataConsentUserDeleteAction" ||
      key === "DataConsentDelayMeasurement"
        ) {
            privacySettings[key] = event.value;
        } else {
            privacySettings[key] =
        typeof event === "object" ? event.target.value : event;
		logger.debug(`<<< Exiting onSettingChange(key=${key},event=${event})`);
        }

        this.setState({
            privacySettings: privacySettings
        });

        props.dispatch(
            SiteBehaviorActions.privacySettingsClientModified(privacySettings)
        );
    }

    onUpdate(event) {
        event.preventDefault();
        const { props, state } = this;

        props.dispatch(
            SiteBehaviorActions.updatePrivacySettings(
                state.privacySettings,
                () => {
                    util.utilities.notify(resx.get("SettingsUpdateSuccess"));
                    this.setState({
                        privacySettings: Object.assign({}, this.state.privacySettings, {
                            DataConsentResetTerms: false
                        })
                    });
                },
                () => {
                    util.utilities.notifyError(resx.get("SettingsError"));
                }
            )
        );
		logger.debug(`<<< Exiting onUpdate(event=${event})`);
    }

    onCancel() {
		logger.debug('>>> Entering onCancel()');
        const { props } = this;
        util.utilities.confirm(
            resx.get("SettingsRestoreWarning"),
            resx.get("Yes"),
            resx.get("No"),
            () => {
                props.dispatch(
                    SiteBehaviorActions.getPrivacySettings(props.portalId, data => {
                        this.setState({
                            privacySettings: Object.assign({}, data.Settings)
                        });
                    })
                );
            }
        );
		logger.debug('<<< Exiting onCancel()');
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const { props, state } = this;
        const TabParameters = {
            portalId: props.portalId !== undefined ? props.portalId : -2,
            cultureCode: props.cultureCode || "",
            isMultiLanguage: false,
            excludeAdminTabs: false,
            roles: "",
            sortOrder: 0
        };
        let TabParameters_1 = Object.assign(Object.assign({}, TabParameters), {
            disabledNotSelectable: false
        });
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        const columnOneLeft = state.privacySettings ? (
            <div key="column-one-left" className="left-column">
                <InputGroup>
                    <Label
                        labelType="inline"
                        tooltipMessage={resx.get("plUpgrade.Help")}
                        label={resx.get("plUpgrade")}
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
                        value={state.privacySettings.CheckUpgrade}
                        onChange={this.onSettingChange.bind(this, "CheckUpgrade")}
                    />
                </InputGroup>
            </div>
        ) : (
            <div key="column-one-left" className="left-column" />
        );
		logger.debug(`render(): TabParameters_1 → ${TabParameters_1}`);
        const columnTwoLeft = state.privacySettings ? (
            <div key="column-two-left" className="left-column">
				logger.debug(`render(): noneSpecifiedText → ${noneSpecifiedText}`);
                <InputGroup>
                    <Label
                        labelType="inline"
                        tooltipMessage={resx.get("plShowCookieConsent.Help")}
                        label={resx.get("plShowCookieConsent")}
                    />
                    <Switch
                        onText={resx.get("SwitchOn")}
                        offText={resx.get("SwitchOff")}
                        value={state.privacySettings.ShowCookieConsent}
                        onChange={this.onSettingChange.bind(this, "ShowCookieConsent")}
                    />
                </InputGroup>
            </div>
        ) : (
            <div key="column-two-left" className="left-column" />
        );
        const columnTwoRight = state.privacySettings ? (
            <div key="column-two-right" className="right-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plCookieMoreLink.Help")}
                        label={resx.get("plCookieMoreLink")}
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        enabled={state.privacySettings.ShowCookieConsent}
                        error={false}
                        value={state.privacySettings.CookieMoreLink}
                        onChange={this.onSettingChange.bind(this, "CookieMoreLink")}
                    />
                </InputGroup>
            </div>
        ) : (
            <div key="column-two-right" className="right-column" />
        );
		logger.debug('<<< Exiting render()');
        const hardDeleteDelay =
      state.privacySettings &&
      state.privacySettings.DataConsentUserDeleteAction === 2 ? (
              <div className="editor-row divider">
                  <SingleLineInputWithError
                      withLabel={true}
                      style={{ float: "left", width: "47.5%", whiteSpace: "pre" }}
                      label={resx.get("DataConsentDelay")}
                      error={false}
                      errorMessage={resx.get("DataConsentDelay.ErrorMessage")}
                      value={state.privacySettings.DataConsentDelay}
                      onChange={this.onSettingChange.bind(this, "DataConsentDelay")}
                  />
                  <div className="text-section">&nbsp; </div>
                  <Dropdown
                      style={{ width: 46 + "%", float: "right", margin: "25px 0 0 0" }}
                      options={this.getTimeLapseMeasurements()}
                      value={state.privacySettings.DataConsentDelayMeasurement}
                      onSelect={this.onSettingChange.bind(
                          this,
                          "DataConsentDelayMeasurement"
                      )}
                  />
              </div>
          ) : null;
        const columnThreeLeft = state.privacySettings ? (
            <div key="column-two-left" className="left-column">
                <InputGroup>
                    <Label
                        labelType="inline"
                        tooltipMessage={resx.get("DataConsentActive.Help")}
                        label={resx.get("DataConsentActive")}
                    />
                    <Switch
                        onText={resx.get("SwitchOn")}
                        offText={resx.get("SwitchOff")}
                        value={state.privacySettings.DataConsentActive}
                        onChange={this.onSettingChange.bind(this, "DataConsentActive")}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("DataConsentConsentRedirect.Help")}
                        label={resx.get("DataConsentConsentRedirect")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 5 }}
                        selectedTabId={
                            state.privacySettings.DataConsentConsentRedirect
                                ? state.privacySettings.DataConsentConsentRedirect
                                : -1
                        }
                        OnSelect={this.onSettingChange.bind(
                            this,
                            "DataConsentConsentRedirect"
                        )}
                        defaultLabel={
                            state.privacySettings.DataConsentConsentRedirectName
                                ? state.privacySettings.DataConsentConsentRedirectName
                                : noneSpecifiedText
                        }
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("DataConsentUserDeleteAction.Help")}
                        label={resx.get("DataConsentUserDeleteAction")}
                    />
                    <Dropdown
                        options={this.getUserDeleteOptions()}
                        value={state.privacySettings.DataConsentUserDeleteAction}
                        onSelect={this.onSettingChange.bind(
                            this,
                            "DataConsentUserDeleteAction"
                        )}
                    />
                </InputGroup>
                {hardDeleteDelay}
            </div>
        ) : (
            <div key="column-two-left" className="left-column" />
        );
        const columnThreeRight = state.privacySettings ? (
            <div key="column-two-right" className="right-column">
                <div className="warningBox">
                    <div className="warningText">
                        {resx.get("DataConsentResetTerms.Warning")}
                    </div>
                    <div className="warningButton">
                        <Button
                            type="secondary"
                            onClick={this.onDataConsentResetTerms.bind(this)}
                        >
                            {resx.get("DataConsentResetTerms")}
                        </Button>
                    </div>
                </div>
            </div>
        ) : (
            <div key="column-two-right" className="right-column" />
        );

        let comSettings = isHost ? (
            <div>
                <div className="sectionTitle">
                    {resx.get("PrivacyCommunicationSettings")}
                </div>
                <GridSystem numberOfColumns={2}>
                    {[columnOneLeft]}
                </GridSystem>
            </div>
        ) : null;

        return (
            <div className={styles.privacySettings}>
                {comSettings}
                <div className="sectionTitle">
                    {resx.get("PrivacyCookieConsentSettings")}
                </div>
                <GridSystem numberOfColumns={2}>
                    {[columnTwoLeft, columnTwoRight]}
                </GridSystem>
                <div className="sectionTitle">{resx.get("DataConsentSettings")}</div>
                <GridSystem numberOfColumns={2}>
                    {[columnThreeLeft, columnThreeRight]}
                </GridSystem>
                <div className="buttons-box">
                    <Button
                        disabled={!props.privacySettingsClientModified}
                        type="neutral"
                        onClick={this.onCancel.bind(this)}
                    >
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        disabled={!props.privacySettingsClientModified}
                        type="primary"
                        onClick={this.onUpdate.bind(this)}
                    >
                        {resx.get("Save")}
                    </Button>
                </div>
            </div>
        );
    }
}

PrivacySettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    privacySettings: PropTypes.object,
    privacySettingsClientModified: PropTypes.bool,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.tabIndex,
        privacySettings: state.siteBehavior.privacySettings,
        privacySettingsClientModified:
      state.siteBehavior.privacySettingsClientModified
    };
}

export default connect(mapStateToProps)(PrivacySettingsPanelBody);
