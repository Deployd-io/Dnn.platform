import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import {
    Dropdown,
    InputGroup,
    Switch,
    Label,
    Button,
    Tooltip,
} from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import "./style.less";
const logger = console;

let timespanSiteOptions = [];
let timespanUserOptions = [];

class ApiTokenSettingsPanelBody extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            apiTokenSettings: undefined,
        };
        timespanSiteOptions.push({ "value": 0, "label": resx.get("Days30") });
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
        timespanSiteOptions.push({ "value": 1, "label": resx.get("Days60") });
        timespanSiteOptions.push({ "value": 2, "label": resx.get("Days90") });
        timespanSiteOptions.push({ "value": 3, "label": resx.get("Days180") });
        timespanSiteOptions.push({ "value": 4, "label": resx.get("Years1") });
        timespanSiteOptions.push({ "value": 5, "label": resx.get("Years2") });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const { props } = this;
        if (props.apiTokenSettings) {
            this.setState({
                apiTokenSettings: props.apiTokenSettings,
            });
            return;
        }
        this.getSettings();
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }

    getUserTimespanOptions() {
		logger.error('>>> Entering getUserTimespanOptions()', { "event.action": "getUserTimespanOptions" });
        if (timespanUserOptions.length === 0) {
            const max = this.state.apiTokenSettings.MaximumSiteTimespan;
            timespanUserOptions = timespanSiteOptions.filter((item) => {
                return item.value <= max;
            });
        }
		logger.warn('<<< Exiting getUserTimespanOptions()', { "event.action": "getUserTimespanOptions" });
        return timespanUserOptions;
    }

    onSettingChange(key, event) {
		logger.error(`>>> Entering onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        const { state, props } = this;
        let apiTokenSettings = Object.assign({}, state.apiTokenSettings);
        apiTokenSettings[key] = typeof event === "object" ? event.target.value : event;
        this.setState({
            apiTokenSettings: apiTokenSettings,
        }, () => {
            timespanUserOptions = [];
        });
        props.dispatch(SecurityActions.apiTokenSettingsClientModified(apiTokenSettings));
		logger.warn(`<<< Exiting onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
    }

    onUpdate(event) {
		logger.error(`>>> Entering onUpdate(event=${event})`, { "event.action": "onUpdate", "logrite.event": event });
        event.preventDefault();
        const { props, state } = this;

        props.dispatch(
            SecurityActions.updateApiTokenSettings(
                state.apiTokenSettings,
                () => {
                    util.utilities.notify(resx.get("ApiTokenSettingsUpdateSuccess"));
                    this.getSettings();
                },
                () => {
                    util.utilities.notifyError(resx.get("ApiTokenSettingsError"));
					logger.warn(`<<< Exiting onUpdate(event=${event})`, { "event.action": "onUpdate", "logrite.event": event });
                }
            )
        );
    }

    onCancel() {
		logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
        util.utilities.confirm(
            resx.get("ApiTokenSettingsRestoreWarning"),
            resx.get("Yes"),
            resx.get("No"),
            () => {
                this.getSettings();
				logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
            }
        );
    }

    getSettings() {
		logger.error('>>> Entering getSettings()', { "event.action": "getSettings" });
        const { props } = this;
        props.dispatch(
            SecurityActions.getApiTokenSettings((data) => {
                let apiTokenSettings = Object.assign({}, data.Results.ApiTokenSettings);
                this.setState({
                    apiTokenSettings,
                });
            })
        );
		logger.warn('<<< Exiting getSettings()', { "event.action": "getSettings" });
    }

    testPositiveInt(value) {
		logger.error(`>>> Entering testPositiveInt(value=${value})`, { "event.action": "testPositiveInt", "logrite.value": value });
		logger.warn(`<<< Exiting testPositiveInt(value=${value})`, { "event.action": "testPositiveInt", "logrite.value": value });
        return /^([0-9]+)$/.test(value);
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { state } = this;
        const isHost = util.settings.isHost;

        if (state.apiTokenSettings) {
            let warningBox = <div />;
            if (!state.apiTokenSettings.ApiTokensEnabled) {
                warningBox = (
                    <div className="warningBox">
                        <div className="warningText">{resx.get("ApiTokensDisabled.Help")}</div>
                    </div>
                );
            }
            return (
                <div id="apiTokenSettings-container">
                    {warningBox}
                    <InputGroup>
                        <div className="apiTokenSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plAllowApiTokens.Help")}
                                label={resx.get("plAllowApiTokens")}
                            />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.apiTokenSettings.AllowApiTokens}
                                readOnly={!state.apiTokenSettings.ApiTokensEnabled}
                                onChange={this.onSettingChange.bind(this, "AllowApiTokens")}
                            />
                        </div>
                    </InputGroup>
                    {isHost && (
                        <InputGroup>
                            <Label
                                tooltipMessage={resx.get("plMaximumSiteTimespan.Help")}
                                label={resx.get("plMaximumSiteTimespan")}
                                extra={
                                    <Tooltip
                                        messages={[resx.get("GlobalSetting")]}
                                        type="global"
                                        style={{ float: "left", position: "static" }} />
                                } />
                            <Dropdown
                                options={timespanSiteOptions}
                                value={state.apiTokenSettings.MaximumSiteTimespan}
                                onSelect={(newVal) => {
                                    this.onSettingChange("MaximumSiteTimespan", newVal.value);
                                }}
                                enabled={state.apiTokenSettings.ApiTokensEnabled}
                            />
                        </InputGroup>
                    )}
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plUserTokenTimespan.Help")}
                            label={resx.get("plUserTokenTimespan")}
                        />
                        <Dropdown
                            options={this.getUserTimespanOptions()}
                            value={state.apiTokenSettings.UserTokenTimespan}
                            onSelect={(newVal) => {
                                this.onSettingChange("UserTokenTimespan", newVal.value);
                            }}
                            enabled={state.apiTokenSettings.AllowApiTokens}
                        />
                    </InputGroup>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.apiTokenSettingsClientModified}
                            type="neutral"
                            onClick={this.onCancel.bind(this)}
                        >
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.apiTokenSettingsClientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this)}
                        >
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        } else return <div />;
    }
}

ApiTokenSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    apiTokenSettings: PropTypes.object,
    apiTokenSettingsClientModified: PropTypes.bool,
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        tabIndex: state.pagination.tabIndex,
        apiTokenSettings: state.security.apiTokenSettings,
        apiTokenSettingsClientModified: state.security.apiTokenSettingsClientModified,
    };
}

export default connect(mapStateToProps)(ApiTokenSettingsPanelBody);
