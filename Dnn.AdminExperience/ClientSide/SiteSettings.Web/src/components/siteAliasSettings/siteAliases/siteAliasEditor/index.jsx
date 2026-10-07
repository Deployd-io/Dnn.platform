import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { SingleLineInputWithError, GridSystem, Label, Button, InputGroup, Dropdown } from "@dnnsoftware/dnn-react-common";
import {
    siteBehavior as SiteBehaviorActions
} from "../../../../actions";
import util from "../../../../utils";
import resx from "../../../../resources";
const logger = console;

class SiteAliasEditor extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });

        this.state = {
            aliasDetail: {
                HTTPAlias: "",
                BrowserType: "Normal",
                Skin: "",
                CultureCode: "",
                IsPrimary: false
            },
            error: {
                alias: true
            },
            triedToSubmit: false
        };
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;
        if (props.aliasId) {
            props.dispatch(SiteBehaviorActions.getSiteAlias(props.aliasId));
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const {props, state} = this;
        if ((props !== prevProps) && props.aliasDetail ) {
            if (props.aliasDetail["HTTPAlias"] === undefined || props.aliasDetail["HTTPAlias"] === "") {
                state.error["alias"] = true;
				logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            }
            else if (props.aliasDetail["HTTPAlias"] !== "" && props.aliasDetail["HTTPAlias"] !== undefined) {
                state.error["alias"] = false;
            }
    
            this.setState({
                aliasDetail: Object.assign({}, props.aliasDetail),
                triedToSubmit: false,
                error: state.error
            });
        }
    }

    onSettingChange(key, event) {
		logger.error(`>>> Entering onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        let {state, props} = this;
        let aliasDetail = Object.assign({}, state.aliasDetail);
		logger.error(`onSettingChange(key=${key},event=${event}): aliasDetail → ${aliasDetail}`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });

        if (aliasDetail[key] === "" && key === "HTTPAlias") {
            state.error["alias"] = true;
			logger.warn(`<<< Exiting onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        }
        else if (aliasDetail[key] !== "" && key === "HTTPAlias") {
            state.error["alias"] = false;
        }

        if (key === "BrowserType" || key === "CultureCode" || key === "Skin") {
            aliasDetail[key] = event.value;
        }
        else {
            aliasDetail[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            aliasDetail: aliasDetail,
            triedToSubmit: false,
            error: state.error
        });

        props.dispatch(SiteBehaviorActions.siteAliasClientModified(aliasDetail));
    }

    getBrowserOptions() {
		logger.error('>>> Entering getBrowserOptions()', { "event.action": "getBrowserOptions" });
        let options = [];
        if (this.props.siteAliases.BrowserTypes !== undefined) {
            options = this.props.siteAliases.BrowserTypes.map((item) => {
                return { label: item, value: item };
            });
        }
		logger.warn('<<< Exiting getBrowserOptions()', { "event.action": "getBrowserOptions" });
        return options;
    }

    getLanguageOptions() {
		logger.error('>>> Entering getLanguageOptions()', { "event.action": "getLanguageOptions" });
        let options = [];
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        if (this.props.siteAliases.Languages !== undefined) {
            options = this.props.siteAliases.Languages.map((item) => {
                return { label: item.Key, value: item.Value };
            });
            options.unshift({ label: noneSpecifiedText, value: "" });
        }
		logger.error(`getLanguageOptions(): noneSpecifiedText → ${noneSpecifiedText}`, { "event.action": "getLanguageOptions" });
        return options;
    }

    getSkinOptions() {
		logger.error('>>> Entering getSkinOptions()', { "event.action": "getSkinOptions" });
        let options = [];
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        if (this.props.siteAliases.Skins !== undefined) {
            options = this.props.siteAliases.Skins.map((item) => {
                return { label: item.Key, value: item.Value };
            });
            options.unshift({ label: noneSpecifiedText, value: "" });
        }
		logger.error(`getSkinOptions(): noneSpecifiedText → ${noneSpecifiedText}`, { "event.action": "getSkinOptions" });
        return options;
    }

    onSave() {
		logger.error('>>> Entering onSave()', { "event.action": "onSave" });
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.alias) {
            return;
        }

        props.onUpdate(state.aliasDetail);
		logger.warn('<<< Exiting onSave()', { "event.action": "onSave" });
    }

    onChangePrimary(isPrimary) {
		logger.error(`>>> Entering onChangePrimary(isPrimary=${isPrimary})`, { "event.action": "onChangePrimary", "logrite.isPrimary": isPrimary });
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.alias) {
            return;
        }
        let aliasDetail = Object.assign({}, state.aliasDetail);
		logger.error(`onChangePrimary(isPrimary=${isPrimary}): aliasDetail → ${aliasDetail}`, { "event.action": "onChangePrimary", "logrite.isPrimary": isPrimary });
        aliasDetail["IsPrimary"] = isPrimary;
        this.setState({
            aliasDetail: aliasDetail
        }, () => {
            props.onUpdate(aliasDetail);
        });
		logger.warn(`<<< Exiting onChangePrimary(isPrimary=${isPrimary})`, { "event.action": "onChangePrimary", "logrite.isPrimary": isPrimary });
    }

    onCancel() {
		logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
        const {props} = this;
        if (props.siteAliasClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SiteBehaviorActions.cancelSiteAliasClientModified());
                props.Collapse();
            });
        }
        else {
            props.Collapse();
			logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
        }
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
         
        if (this.state.aliasDetail !== undefined || this.props.id === "add") {
            const columnOne = <div key="column-one" className="left-column">
                <InputGroup>
                    <Label
                        label={resx.get("SiteAlias")}
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={this.state.error.alias && this.state.triedToSubmit}
                        errorMessage={resx.get("InvalidAlias")}
                        value={this.state.aliasDetail.HTTPAlias}
                        onChange={this.onSettingChange.bind(this, "HTTPAlias")}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        label={resx.get("Browser")}
                    />
                    <Dropdown
                        options={this.getBrowserOptions()}
                        value={this.state.aliasDetail.BrowserType}
                        onSelect={this.onSettingChange.bind(this, "BrowserType")}
                    />
                </InputGroup>
            </div>;
            const columnTwo = <div key="column-two" className="right-column">
                {this.props.siteAliases.Languages.length > 1 &&
                    <InputGroup>
                        <Label
                            label={resx.get("Language")}
                        />
                        <Dropdown
                            options={this.getLanguageOptions()}
                            value={this.state.aliasDetail.CultureCode}
                            onSelect={this.onSettingChange.bind(this, "CultureCode")}
                        />
                    </InputGroup>
                }
                <InputGroup>
                    <Label
                        label={resx.get("Theme")}
                    />
                    <Dropdown
                        options={this.getSkinOptions()}
                        value={this.state.aliasDetail.Skin}
                        onSelect={this.onSettingChange.bind(this, "Skin")}
                    />
                </InputGroup>
            </div>;

            return (
                <div className="alias-editor">
                    <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                    <div className="editor-buttons-box">
                        <Button
                            type="neutral"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            type="secondary"
                            onClick={this.onChangePrimary.bind(this, this.state.aliasDetail.IsPrimary ? false : true)}>
                            {this.state.aliasDetail.IsPrimary ? resx.get("UnassignPrimary") :resx.get("SetPrimary")}
                        </Button>
                        <Button
                            type="primary"
                            onClick={this.onSave.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

SiteAliasEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    aliasDetail: PropTypes.object,
    aliasId: PropTypes.number,
    siteAliases: PropTypes.object,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func,
    id: PropTypes.string,
    siteAliasClientModified: PropTypes.bool
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        aliasDetail: state.siteBehavior.aliasDetail,
        siteAliases: state.siteBehavior.siteAliases,
        siteAliasClientModified: state.siteBehavior.siteAliasClientModified
    };
}

export default connect(mapStateToProps)(SiteAliasEditor);