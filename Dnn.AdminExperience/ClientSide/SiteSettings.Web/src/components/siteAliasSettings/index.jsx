import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteBehavior as SiteBehaviorActions
} from "../../actions";
import SiteAliases from "./siteAliases";
import { InputGroup, Switch, RadioButtons, Label, Button } from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

class SiteAliasSettingsPanelBody extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            urlMappingSettings: undefined
        };
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.loadData();
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentDidUpdate() {
		logger.debug('>>> Entering componentDidUpdate()');
        const { props } = this;        
        if (props.urlMappingSettings) {
            let portalIdChanged = props.portalId !== undefined && props.urlMappingSettings.PortalId !== props.portalId;
            if (portalIdChanged) {
                this.loadData();
				logger.debug('<<< Exiting componentDidUpdate()');
            }
        }
    }

    loadData() {
		logger.debug('>>> Entering loadData()');
        const {props} = this;
        props.dispatch(SiteBehaviorActions.getUrlMappingSettings(props.portalId, (data) => {
			logger.debug('<<< Exiting loadData()');
            this.setState({
                urlMappingSettings: Object.assign({}, data.Settings)
            });
        }));
    }

    getMappingModeOptions() {
		logger.debug('>>> Entering getMappingModeOptions()');
        let options = [];
        if (this.props.portalAliasMappingModes !== undefined) {
            options = this.props.portalAliasMappingModes.map((item) => {
		logger.debug('<<< Exiting getMappingModeOptions()');
                return { label: item.Key, value: item.Value };
            });
        }
        return options;
    }

    onSettingChange(key, event) {
		logger.debug(`>>> Entering onSettingChange(key=${key},event=${event})`);
        let {state, props} = this;

        let urlMappingSettings = Object.assign({}, state.urlMappingSettings);

        if (key === "PortalAliasMapping") {
            urlMappingSettings[key] = event;
        }
        else {
            urlMappingSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }
        this.setState({
            urlMappingSettings: urlMappingSettings
        });

        props.dispatch(SiteBehaviorActions.urlMappingSettingsClientModified(urlMappingSettings));
		logger.debug(`<<< Exiting onSettingChange(key=${key},event=${event})`);
    }

    onUpdate(event) {
		logger.debug(`>>> Entering onUpdate(event=${event})`);
        event.preventDefault();
        const {props, state} = this;
        props.dispatch(SiteBehaviorActions.updateUrlMappingSettings(state.urlMappingSettings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
			logger.debug(`<<< Exiting onUpdate(event=${event})`);
        }));
    }

    onCancel() {
		logger.debug('>>> Entering onCancel()');
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SiteBehaviorActions.getUrlMappingSettings(props.portalId, (data) => {
				logger.debug('<<< Exiting onCancel()');
                let urlMappingSettings = Object.assign({}, data.Settings);
                this.setState({
                    urlMappingSettings
                });
            }));
        });
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
        if (state.urlMappingSettings) {
			logger.debug('<<< Exiting render()');
            return (
                <div className={styles.siteAliasSettings}>
                    <SiteAliases portalId={props.portalId}/>
                    <div className="sectionTitleNoBorder">{resx.get("UrlMappingSettings")}</div>
                    <div className="urlMappingSettings">
                        <InputGroup>
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("portalAliasModeButtonListLabel.Help")}
                                label={resx.get("portalAliasModeButtonListLabel")}
                            />
                            <RadioButtons
                                onChange={this.onSettingChange.bind(this, "PortalAliasMapping")}
                                options={this.getMappingModeOptions()}
                                buttonGroup="aliasMode"
                                value={state.urlMappingSettings.PortalAliasMapping} />
                        </InputGroup>
                        <InputGroup>
                            <div className="urlMappingSettings-row_switch">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("plAutoAddPortalAlias.Help")}
                                    label={resx.get("plAutoAddPortalAlias")}
                                />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    readOnly={!state.urlMappingSettings.AutoAddPortalAliasEnabled}
                                    value={state.urlMappingSettings.AutoAddPortalAlias}
                                    onChange={this.onSettingChange.bind(this, "AutoAddPortalAlias")}
                                />
                            </div>
                        </InputGroup>
                    </div>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.urlMappingSettingsClientModified}
                            type="neutral"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.urlMappingSettingsClientModified}
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

SiteAliasSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    urlMappingSettings: PropTypes.object,
    portalAliasMappingModes: PropTypes.array,
    urlMappingSettingsClientModified: PropTypes.bool,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.tabIndex,
        urlMappingSettings: state.siteBehavior.urlMappingSettings,
        portalAliasMappingModes: state.siteBehavior.portalAliasMappingModes,
        urlMappingSettingsClientModified: state.siteBehavior.urlMappingSettingsClientModified,
    };
}

export default connect(mapStateToProps)(SiteAliasSettingsPanelBody);