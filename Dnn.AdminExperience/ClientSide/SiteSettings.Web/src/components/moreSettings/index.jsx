import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { siteBehavior as SiteBehaviorActions } from "../../actions";
import {
    Button,
    Dropdown,
    GridSystem,
    InputGroup,
    Label,
    MultiLineInputWithError,
    Switch,
} from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

function getError(errors) {
	logger.debug(`>>> Entering getError(errors=${errors})`);
    let hasError = false;
    Object.keys(errors).forEach((key) => {
        if (errors[key] === true) {
            hasError = true;
        }
    });
	logger.debug(`<<< Exiting getError(errors=${errors})`);
    return hasError;
}

let isHost = false;

class MoreSettingsPanelBody extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            otherSettings: undefined,
            siteBehaviorExtrasRendered: true,
            errorInSave: false,
            whitelistOption: 0,
        };
        isHost = util.settings.isHost;
		logger.debug('<<< Exiting constructor()');
    }

    loadData() {
		logger.debug('>>> Entering loadData()');
        const { props } = this;
        props.dispatch(
            SiteBehaviorActions.getOtherSettings(props.portalId, (data) => {
                let whitelistOption = 1;
                if (data.Settings.AllowedExtensionsWhitelist === data.Settings.HostAllowedExtensionsWhitelists) {
                    whitelistOption = 0;
                } else if (data.Settings.AllowedExtensionsWhitelist === data.Settings.ImageExtensionsList) {
                    whitelistOption = 2;
					logger.debug('<<< Exiting loadData()');
                }
                this.setState({
                    otherSettings: Object.assign({}, data.Settings),
                    whitelistOption: whitelistOption,
                    workflows: [...data.Workflows],
                });
            })
        );
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        const { props } = this;
        if (props.otherSettings) {
            this.setState({
                otherSettings: props.otherSettings,
            });
            return;
        }
        this.setState({
            siteBehaviorExtrasRendered: true,
            whitelistOption: 0,
        });
        this.loadData();
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        const { props } = this;
        if (props.otherSettings) {
            let portalIdChanged = props.portalId !== undefined && prevProps.portalId !== props.portalId;
            let cultureCodeChanged = props.cultureCode !== undefined && prevProps.cultureCode !== props.cultureCode;

            if (portalIdChanged || cultureCodeChanged) {
                this.loadData();
				logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
            }
        }
    }

    onSettingChange(key, event) {
		logger.debug(`>>> Entering onSettingChange(key=${key},event=${event})`);
        let { state, props } = this;
        let otherSettings = Object.assign({}, state.otherSettings);

		logger.debug(`onSettingChange(key=${key},event=${event}): otherSettings → ${otherSettings}`);
        otherSettings[key] = typeof event === "object" ? event.target.value : event;
		logger.debug(`<<< Exiting onSettingChange(key=${key},event=${event})`);

        this.setState({
            otherSettings: otherSettings,
        });

        props.dispatch(
            SiteBehaviorActions.otherSettingsClientModified(otherSettings)
        );
    }

    onUpdate(event) {
		logger.debug(`>>> Entering onUpdate(event=${event})`);
        if (event) {
            event.preventDefault();
			logger.debug(`<<< Exiting onUpdate(event=${event})`);
        }
        const { props, state } = this;

        props.dispatch(
            SiteBehaviorActions.updateOtherSettings(
                state.otherSettings,
                () => { },
                () => {
                    this.setState({
                        errorInSave: true,
                    });
                }
            )
        );
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
                    SiteBehaviorActions.getOtherSettings((data) => {
                        this.setState({
                            otherSettings: Object.assign({}, data.Settings),
                        });
                    })
                );
                this.setState(
                    {
                        siteBehaviorExtrasRendered: false,
                    },
                    () => {
                        this.setState({
                            siteBehaviorExtrasRendered: true,
                        });
                    }
                );
            }
        );
		logger.debug('<<< Exiting onCancel()');
    }

    renderSiteBehaviorExtensions() {
		logger.debug('>>> Entering renderSiteBehaviorExtensions()');
        const SiteBehaviorExtras =
            window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras;
        if (!SiteBehaviorExtras || SiteBehaviorExtras.length === 0) {
            return;
        }
        return SiteBehaviorExtras.sort(function (a, b) {
		logger.debug('>>> Entering sort()');
            if (a.RenderOrder < b.RenderOrder) return -1;
            if (a.RenderOrder > b.RenderOrder) return 1;
            return 0;
        }).map((data) => {
		logger.debug('<<< Exiting renderSiteBehaviorExtensions()');
            return data.Component;
        });
    }

    onSaveMoreSettings() {
		logger.debug('>>> Entering onSaveMoreSettings()');
        const SiteBehaviorExtras =
            window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras;
        let _errorInSave = false;

        if (SiteBehaviorExtras && SiteBehaviorExtras.length > 0) {
            //First, loop through and check there are no errors.
            SiteBehaviorExtras.forEach((extra) => {
                const currentReducer = this.props[extra.ReducerKey];

                if (currentReducer.errors && getError(currentReducer.errors)) {
                    extra.SetTriedToSave &&
                        this.props.dispatch(extra.SetTriedToSave(true));
                    _errorInSave = true;
                } else {
                    extra.SetTriedToSave &&
                        this.props.dispatch(extra.SetTriedToSave(false));
                }
            });

            //Only go through the save if none of them have an error.
            if (!_errorInSave) {
                SiteBehaviorExtras.forEach((extra) => {
                    if (typeof extra.SaveMethod === "function") {
                        const currentReducer = this.props[extra.ReducerKey];

                        //Call the Save Method of each SiteBehaviorExtra.
                        this.props.dispatch(
                            extra.SaveMethod(
                                Object.assign(
                                    { formDirty: currentReducer.formDirty },
                                    currentReducer.onSavePayload
                                ),
                                () => { }, //Save Callback
                                () => {
                                    this.setState({ errorInSave: true });
                                } // Error Callback
                            )
                        );
                    }
                });
            } else {
                //Return if there is an error.
                return;
            }
        }

        if (this.props.otherSettingsClientModified) {
            this.onUpdate();
        }

        if (this.state.errorInSave) {
            util.utilities.notifyError(resx.get("SettingsError"));
            this.setState({
                errorInSave: false,
            });
        } else {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }
		logger.debug('<<< Exiting onSaveMoreSettings()');
    }

    getOverallFormDirty() {
		logger.debug('>>> Entering getOverallFormDirty()');
        let formDirty = false;
        const SiteBehaviorExtras =
            window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras;
        if (SiteBehaviorExtras && SiteBehaviorExtras.length > 0) {
            SiteBehaviorExtras.forEach((extra) => {
                if (
                    this.props[extra.ReducerKey] &&
                    this.props[extra.ReducerKey].formDirty
                ) {
                    formDirty = true;
                }
            });
        }
        if (this.props.otherSettingsClientModified) {
            formDirty = true;
        }
		logger.debug('<<< Exiting getOverallFormDirty()');
        return formDirty;
    }

    getWhiteListOptions() {
		logger.debug('>>> Entering getWhiteListOptions()');
		logger.debug('<<< Exiting getWhiteListOptions()');
        return [
            { label: resx.get("Default"), value: 0 },
            { label: resx.get("Custom"), value: 1 },
            { label: resx.get("OnlyImages"), value: 2 },
        ];
    }

    onWhitelistOptionChange(e) {
		logger.debug(`>>> Entering onWhitelistOptionChange(e=${e})`);
        let newState = this.state;
        switch (e.value) {
            case 0:
                newState.otherSettings.AllowedExtensionsWhitelist = this.state.otherSettings.HostAllowedExtensionsWhitelists;
                break;
            case 2:
                newState.otherSettings.AllowedExtensionsWhitelist = this.state.otherSettings.ImageExtensionsList;
                break;
        }
        newState.whitelistOption = e.value;
		logger.debug(`onWhitelistOptionChange(e=${e}): newState.whitelistOption → ${newState.whitelistOption}`);
        this.setState(newState);
		logger.debug(`<<< Exiting onWhitelistOptionChange(e=${e})`);
        this.props.dispatch(
            SiteBehaviorActions.otherSettingsClientModified(newState.otherSettings)
        );
    }

    getMaxNumberOfVersionsOptions(maxValue) {
		logger.debug(`>>> Entering getMaxNumberOfVersionsOptions(maxValue=${maxValue})`);
        const maxNumberOfVersionsOptions = [];
        for (let i = 0; i <= maxValue; i++) {
            maxNumberOfVersionsOptions.push({ "value": i, "label": i.toString() });
        }
		logger.debug(`<<< Exiting getMaxNumberOfVersionsOptions(maxValue=${maxValue})`);
        return maxNumberOfVersionsOptions;
    }

    getWorkflowsOptions() {
		logger.debug('>>> Entering getWorkflowsOptions()');
        let options = [];
        const { props } = this;
        if (props.workflows !== undefined) {
            options = this.props.workflows.map((item) => {
		logger.debug('<<< Exiting getWorkflowsOptions()');
                return { label: item.label, value: item.value };
            });
        }
        return options;
    }
    
    onDropDownChange(key, option) {
		logger.debug(`>>> Entering onDropDownChange(key=${key},option=${option})`);
        let { state, props } = this;
        let otherSettings = Object.assign({}, state.otherSettings);

		logger.debug(`onDropDownChange(key=${key},option=${option}): otherSettings → ${otherSettings}`);
        otherSettings[key] = typeof option === "object" ? option.value : option;
		logger.debug(`<<< Exiting onDropDownChange(key=${key},option=${option})`);

        this.setState({
            otherSettings: otherSettings,
        });

        props.dispatch(
            SiteBehaviorActions.otherSettingsClientModified(otherSettings)
        );
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const { props, state } = this;
        let htmlEditor = isHost ? (
            <div>
                <div className="sectionTitle">{resx.get("HtmlEditor")}</div>
                <div className="htmlEditorWrapper">
                    <div className="htmlEditorWrapper-left">
                        <div className="htmlEditorWarning">
                            {resx.get("HtmlEditorWarning")}
                        </div>
                    </div>
                    <div className="htmlEditorWrapper-right">
                        <Button
                            type="secondary"
                            onClick={props.openHtmlEditorManager.bind(this)}
                        >
                            {resx.get("OpenHtmlEditor")}
                        </Button>
                    </div>
                </div>
            </div>
        ) : null;
        if (state.otherSettings) {
			logger.debug('<<< Exiting render()');
            return (
                <div className={styles.moreSettings}>
                    {state.siteBehaviorExtrasRendered &&
                        this.renderSiteBehaviorExtensions()}
                    {htmlEditor}
                    <div className="sectionTitle">{resx.get("MoreSettings")}</div>
                    <GridSystem numberOfColumns={2}>
                        <div key="column-one-left" className="left-column">
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("plEnablePopups.Help")}
                                    label={resx.get("plEnablePopups")}
                                />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    value={state.otherSettings.EnablePopups}
                                    onChange={this.onSettingChange.bind(this, "EnablePopups")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("plInlineEditorEnabled.Help")}
                                    label={resx.get("plInlineEditorEnabled")}
                                />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    value={state.otherSettings.InlineEditorEnabled}
                                    onChange={this.onSettingChange.bind(
                                        this,
                                        "InlineEditorEnabled"
                                    )}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("plAllowJsInHtmlModule.Help")}
                                    label={resx.get("plAllowJsInHtmlModule")}
                                />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    value={state.otherSettings.AllowJsInHtmlModule}
                                    onChange={this.onSettingChange.bind(
                                        this,
                                        "AllowJsInHtmlModule"
                                    )}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("plAllowJsInModuleHeaders.Help")}
                                    label={resx.get("plAllowJsInModuleHeaders")}
                                />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    value={state.otherSettings.AllowJsInModuleHeaders}
                                    onChange={this.onSettingChange.bind(
                                        this,
                                        "AllowJsInModuleHeaders"
                                    )}
                                />
                            </InputGroup>
                        </div>
                        <div key="column-one-right" className="right-column">
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("plInjectModuleHyperLink.Help")}
                                    label={resx.get("plInjectModuleHyperLink")}
                                />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    value={state.otherSettings.InjectModuleHyperLink}
                                    onChange={this.onSettingChange.bind(
                                        this,
                                        "InjectModuleHyperLink"
                                    )}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("plShowQuickModuleAddMenu.Help")}
                                    label={resx.get("plShowQuickModuleAddMenu")}
                                />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    value={state.otherSettings.ShowQuickModuleAddMenu}
                                    onChange={this.onSettingChange.bind(
                                        this,
                                        "ShowQuickModuleAddMenu"
                                    )}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("plAllowJsInModuleFooters.Help")}
                                    label={resx.get("plAllowJsInModuleFooters")}
                                />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    value={state.otherSettings.AllowJsInModuleFooters}
                                    onChange={this.onSettingChange.bind(
                                        this,
                                        "AllowJsInModuleFooters"
                                    )}
                                />
                            </InputGroup>
                        </div>
                    </GridSystem>
                    <div className="sectionTitle">{resx.get("WhitelistSettings")}</div>
                    <GridSystem numberOfColumns={2}>
                        <div key="column-two-left" className="left-column">
                            <InputGroup>
                                <Label
                                    tooltipMessage={resx.get("plWhitelistOption.Help")}
                                    label={resx.get("plWhitelistOption")}
                                />
                                <Dropdown
                                    options={this.getWhiteListOptions()}
                                    value={state.whitelistOption}
                                    onSelect={(e) => this.onWhitelistOptionChange(e)}
                                />
                            </InputGroup>
                        </div>
                        <div key="column-two-right" className="right-column">
                            <InputGroup>
                                <Label
                                    tooltipMessage={resx.get("plAllowedExtensionsWhitelist.Help")}
                                    label={resx.get("plAllowedExtensionsWhitelist")}
                                />
                                <MultiLineInputWithError
                                    value={state.otherSettings.AllowedExtensionsWhitelist}
                                    onChange={this.onSettingChange.bind(
                                        this,
                                        "AllowedExtensionsWhitelist"
                                    )}
                                    enabled={state.whitelistOption === 1}
                                />
                            </InputGroup>
                        </div>
                    </GridSystem>
                    {util.isPlatform() && <>
                        <div className="sectionTitle">{resx.get("WorkflowSettings")}</div>
                        <GridSystem numberOfColumns={2}>
                            <div key="column-one-left" className="left-column">
                                <InputGroup>
                                    <Label
                                        labelType="inline"
                                        tooltipMessage={resx.get("plEnabledVersioning.Help")}
                                        label={resx.get("plEnabledVersioning")} />
                                    <Switch
                                        onText={resx.get("SwitchOn")}
                                        offText={resx.get("SwitchOff")}
                                        value={state.otherSettings.EnabledVersioning}
                                        onChange={this.onSettingChange.bind(this, "EnabledVersioning")} />
                                </InputGroup>
                                {state.otherSettings.EnabledVersioning &&
                                    <InputGroup>
                                        <Label
                                            tooltipMessage={resx.get("plMaxNumberOfVersions.Help")}
                                            label={resx.get("plMaxNumberOfVersions")} />
                                        <Dropdown
                                            options={this.getMaxNumberOfVersionsOptions(20)}
                                            value={state.otherSettings.MaxNumberOfVersions}
                                            onSelect={this.onDropDownChange.bind(this, "MaxNumberOfVersions")} />
                                    </InputGroup>
                                }
                            </div>
                            {state.otherSettings.EnabledVersioning && <>
                                <div key="column-one-right" className="right-column">
                                    <InputGroup>
                                        <Label
                                            labelType="inline"
                                            tooltipMessage={resx.get("plWorkflowEnabled.Help")}
                                            label={resx.get("plWorkflowEnabled")} />
                                        <Switch
                                            onText={resx.get("SwitchOn")}
                                            offText={resx.get("SwitchOff")}
                                            value={state.otherSettings.WorkflowEnabled}
                                            onChange={this.onSettingChange.bind(this, "WorkflowEnabled")} />
                                    </InputGroup>
                                    {state.otherSettings.WorkflowEnabled &&
                                        <InputGroup>
                                            <Label
                                                tooltipMessage={resx.get("plDefaultTabWorkflowId.Help")}
                                                label={resx.get("plDefaultTabWorkflowId")} />
                                            <Dropdown
                                                options={this.getWorkflowsOptions()}
                                                value={state.otherSettings.DefaultTabWorkflowId}
                                                style={{ width: "100%" }}
                                                onSelect={this.onDropDownChange.bind(this, "DefaultTabWorkflowId")} />
                                        </InputGroup>
                                    }
                                </div></>
                            }
                        </GridSystem></>
                    }
                    <div className="buttons-box">
                        <Button
                            type="neutral"
                            disabled={!this.getOverallFormDirty()}
                            onClick={this.onCancel.bind(this)}
                        >
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            type="primary"
                            disabled={!this.getOverallFormDirty()}
                            onClick={this.onSaveMoreSettings.bind(this)}
                        >
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        } else return <div />;
    }
}

MoreSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    portalId: PropTypes.number,
    openHtmlEditorManager: PropTypes.func,
    otherSettings: PropTypes.object,
    otherSettingsClientModified: PropTypes.bool,
    cultureCode: PropTypes.string,
    workflows: PropTypes.array,
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        ...state,
        otherSettings: state.siteBehavior.otherSettings,
        otherSettingsClientModified: state.siteBehavior.otherSettingsClientModified,
        workflows: state.siteBehavior.workflows,
    };
}

export default connect(mapStateToProps)(MoreSettingsPanelBody);
