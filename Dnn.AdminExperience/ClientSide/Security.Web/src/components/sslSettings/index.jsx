import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import {
    Dropdown,
    InputGroup,
    SingleLineInputWithError,
    Switch,
    Label,
    Button,
    Tooltip,
} from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

class SslSettingsPanelBody extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            sslSettings: undefined,
        };
		logger.debug('<<< Exiting constructor()');
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        const { props } = this;
        if (props.sslSettings) {
            this.setState({
                sslSettings: props.sslSettings,
            });
            return;
        }
        this.getSslSettings();
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }

    onSettingChange(key, event) {
		logger.debug(`>>> Entering onSettingChange(key=${key},event=${event})`);
        const { state, props } = this;
        let sslSettings = Object.assign({}, state.sslSettings);
		logger.debug(`onSettingChange(key=${key},event=${event}): sslSettings → ${sslSettings}`);
        sslSettings[key] = typeof event === "object" ? event.target.value : event;
        this.setState({
            sslSettings: sslSettings,
        });
        props.dispatch(SecurityActions.sslSettingsClientModified(sslSettings));
		logger.debug(`<<< Exiting onSettingChange(key=${key},event=${event})`);
    }

    onUpdate(event) {
		logger.debug(`>>> Entering onUpdate(event=${event})`);
        event.preventDefault();
        const { props, state } = this;

        props.dispatch(
            SecurityActions.updateSslSettings(
                state.sslSettings,
                () => {
                    util.utilities.notify(resx.get("SslSettingsUpdateSuccess"));
                    this.getSslSettings();
                },
                () => {
                    util.utilities.notifyError(resx.get("SslSettingsError"));
					logger.debug(`<<< Exiting onUpdate(event=${event})`);
                }
            )
        );
    }

    onCancel() {
		logger.debug('>>> Entering onCancel()');
        util.utilities.confirm(
            resx.get("SslSettingsRestoreWarning"),
            resx.get("Yes"),
            resx.get("No"),
            () => {
                this.getSslSettings();
				logger.debug('<<< Exiting onCancel()');
            }
        );
    }

    onSetAllPagesSecure() {
		logger.debug('>>> Entering onSetAllPagesSecure()');
        util.utilities.confirm(
            resx.get("SslSetAllPagesSecure.Confirm"),
            resx.get("Yes"),
            resx.get("No"),
            function onSetAllPagesSecureConfirm() {
				logger.debug('>>> Entering onSetAllPagesSecureConfirm()');
                this.props.dispatch(
                    SecurityActions.updateSslSettings(
                        this.state.sslSettings,
                        () => {
                            SecurityActions.setAllPagesSecure(() => {
                                util.utilities.notify(
                                    resx.get("SslSetAllPagesSecure.Completed")
                                );
                                this.getSslSettings();
                            });
                        },
                        () => {
                            util.utilities.notifyError(resx.get("SslSettingsError"));
							logger.debug('<<< Exiting onSetAllPagesSecureConfirm()');
                        }
                    )
                );
            }.bind(this)
        );
    }

    getSslSettings() {
		logger.debug('>>> Entering getSslSettings()');
        const { props } = this;
        props.dispatch(
            SecurityActions.getSslSettings((data) => {
                let sslSettings = Object.assign({}, data.Results.Settings);
                this.setState({
                    sslSettings,
                });
            })
        );
		logger.debug('<<< Exiting getSslSettings()');
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const { state } = this;
        if (state.sslSettings) {
            let warningBox = <div />;
            switch (state.sslSettings.SSLSetup) {
                case 0:
                    warningBox = (
                        <div className="warningBox">
                            <div className="warningText">{resx.get("SslOff.Help")}</div>
                        </div>
                    );
                    break;
                case 1:
                    warningBox = (
                        <div className="warningBox">
                            <div className="warningText">{resx.get("SslOn.Help")}</div>
                        </div>
                    );
                    break;
                case 2:
                    warningBox = (
                        <div className="warningBox">
                            <div className="warningText">
                                {resx
                                    .get("SslAdvanced.Help")
                                    .replace(
                                        "[NumberOfSecureTabs]",
                                        state.sslSettings.NumberOfSecureTabs.toString()
                                    )
                                    .replace(
                                        "[NumberOfNonSecureTabs]",
                                        state.sslSettings.NumberOfNonSecureTabs.toString()
                                    )}
                            </div>
                            <div className="warningButton">
                                <Button
                                    type="secondary"
                                    onClick={this.onSetAllPagesSecure.bind(this)}
                                >
                                    {resx.get("SslSetAllPagesSecure")}
                                </Button>
                            </div>
                        </div>
                    );
                    break;
            }
			logger.debug('<<< Exiting render()');
            return (
                <div className={styles.sslSettings}>
                    <InputGroup>
                        <Dropdown
                            options={[
                                { label: resx.get("SslOff"), value: "0" },
                                { label: resx.get("SslOn"), value: "1" },
                                { label: resx.get("SslAdvanced"), value: "2" },
                            ]}
                            value={state.sslSettings.SSLSetup.toString()}
                            onSelect={(newVal) => {
                                let sslSettings = state.sslSettings;
                                sslSettings.SSLSetup = parseInt(newVal.value);
                                this.setState({
                                    sslSettings,
                                });
                                this.props.dispatch(
                                    SecurityActions.sslSettingsClientModified(sslSettings)
                                );
                            }}
                            enabled={true}
                        />
                    </InputGroup>
                    {warningBox}
                    {Number.parseInt(state.sslSettings.SSLSetup) === 2 && (
                        <>
                            <InputGroup>
                                <div className="sslSettings-row_switch">
                                    <Label
                                        labelType="inline"
                                        tooltipMessage={resx.get("plSSLEnforced.Help")}
                                        label={resx.get("plSSLEnforced")}
                                    />
                                    <Switch
                                        onText={resx.get("SwitchOn")}
                                        offText={resx.get("SwitchOff")}
                                        value={state.sslSettings.SSLEnforced}
                                        onChange={this.onSettingChange.bind(this, "SSLEnforced")}
                                    />
                                </div>
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    tooltipMessage={resx.get("plSSLURL.Help")}
                                    label={resx.get("plSSLURL")}
                                />
                                <SingleLineInputWithError
                                    withLabel={false}
                                    error={false}
                                    value={state.sslSettings.SSLURL}
                                    onChange={this.onSettingChange.bind(this, "SSLURL")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    tooltipMessage={resx.get("plSTDURL.Help")}
                                    label={resx.get("plSTDURL")}
                                />
                                <SingleLineInputWithError
                                    withLabel={false}
                                    error={false}
                                    value={state.sslSettings.STDURL}
                                    onChange={this.onSettingChange.bind(this, "STDURL")}
                                />
                            </InputGroup>
                        </>
                    )}
                    {
                        /*eslint-disable eqeqeq*/
                        state.sslSettings.SSLOffloadHeader != undefined && state.sslSettings.SSLSetup != 1 && (
                            <InputGroup>
                                <Label
                                    tooltipMessage={resx.get("plSSLOffload.Help")}
                                    label={resx.get("plSSLOffload")}
                                    extra={
                                        <Tooltip
                                            messages={[resx.get("GlobalSetting")]}
                                            type="global"
                                            style={{ float: "left", position: "static" }}
                                        />
                                    }
                                />
                                <SingleLineInputWithError
                                    withLabel={false}
                                    error={false}
                                    value={state.sslSettings.SSLOffloadHeader}
                                    onChange={this.onSettingChange.bind(this, "SSLOffloadHeader")}
                                />
                            </InputGroup>
                        )
                    }
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.sslSettingsClientModified}
                            type="neutral"
                            onClick={this.onCancel.bind(this)}
                        >
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.sslSettingsClientModified}
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

SslSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    sslSettings: PropTypes.object,
    sslSettingsClientModified: PropTypes.bool,
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.tabIndex,
        sslSettings: state.security.sslSettings,
        sslSettingsClientModified: state.security.sslSettingsClientModified,
    };
}

export default connect(mapStateToProps)(SslSettingsPanelBody);
