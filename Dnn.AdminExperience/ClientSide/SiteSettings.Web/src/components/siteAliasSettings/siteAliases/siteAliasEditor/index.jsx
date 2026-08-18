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
		logger.debug('>>> Entering constructor()');

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
		logger.debug('>>> Entering componentDidMount()');
        const {props} = this;
        if (props.aliasId) {
            props.dispatch(SiteBehaviorActions.getSiteAlias(props.aliasId));
			logger.debug('<<< Exiting componentDidMount()');
        }
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        const {props, state} = this;
        if ((props !== prevProps) && props.aliasDetail ) {
            if (props.aliasDetail["HTTPAlias"] === undefined || props.aliasDetail["HTTPAlias"] === "") {
                state.error["alias"] = true;
				logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
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
		logger.debug(`>>> Entering onSettingChange(key=${key},event=${event})`);
        let {state, props} = this;
        let aliasDetail = Object.assign({}, state.aliasDetail);
		logger.debug(`onSettingChange(key=${key},event=${event}): aliasDetail → ${aliasDetail}`);

        if (aliasDetail[key] === "" && key === "HTTPAlias") {
            state.error["alias"] = true;
			logger.debug(`<<< Exiting onSettingChange(key=${key},event=${event})`);
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
		logger.debug('>>> Entering getBrowserOptions()');
        let options = [];
        if (this.props.siteAliases.BrowserTypes !== undefined) {
            options = this.props.siteAliases.BrowserTypes.map((item) => {
		logger.debug('<<< Exiting getBrowserOptions()');
                return { label: item, value: item };
            });
        }
        return options;
    }

    getLanguageOptions() {
		logger.debug('>>> Entering getLanguageOptions()');
        let options = [];
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        if (this.props.siteAliases.Languages !== undefined) {
            options = this.props.siteAliases.Languages.map((item) => {
                return { label: item.Key, value: item.Value };
            });
            options.unshift({ label: noneSpecifiedText, value: "" });
        }
		logger.debug(`getLanguageOptions(): noneSpecifiedText → ${noneSpecifiedText}`);
		logger.debug('<<< Exiting getLanguageOptions()');
        return options;
    }

    getSkinOptions() {
		logger.debug('>>> Entering getSkinOptions()');
        let options = [];
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        if (this.props.siteAliases.Skins !== undefined) {
            options = this.props.siteAliases.Skins.map((item) => {
                return { label: item.Key, value: item.Value };
            });
            options.unshift({ label: noneSpecifiedText, value: "" });
        }
		logger.debug(`getSkinOptions(): noneSpecifiedText → ${noneSpecifiedText}`);
		logger.debug('<<< Exiting getSkinOptions()');
        return options;
    }

    onSave() {
		logger.debug('>>> Entering onSave()');
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.alias) {
            return;
        }

        props.onUpdate(state.aliasDetail);
		logger.debug('<<< Exiting onSave()');
    }

    onChangePrimary(isPrimary) {
		logger.debug(`>>> Entering onChangePrimary(isPrimary=${isPrimary})`);
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.alias) {
            return;
        }
        let aliasDetail = Object.assign({}, state.aliasDetail);
		logger.debug(`onChangePrimary(isPrimary=${isPrimary}): aliasDetail → ${aliasDetail}`);
        aliasDetail["IsPrimary"] = isPrimary;
        this.setState({
            aliasDetail: aliasDetail
        }, () => {
            props.onUpdate(aliasDetail);
        });
		logger.debug(`<<< Exiting onChangePrimary(isPrimary=${isPrimary})`);
    }

    onCancel() {
		logger.debug('>>> Entering onCancel()');
        const {props} = this;
        if (props.siteAliasClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SiteBehaviorActions.cancelSiteAliasClientModified());
                props.Collapse();
            });
        }
        else {
            props.Collapse();
			logger.debug('<<< Exiting onCancel()');
        }
    }

     
    render() {
		logger.debug('>>> Entering render()');
         
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

			logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        aliasDetail: state.siteBehavior.aliasDetail,
        siteAliases: state.siteBehavior.siteAliases,
        siteAliasClientModified: state.siteBehavior.siteAliasClientModified
    };
}

export default connect(mapStateToProps)(SiteAliasEditor);