import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { GridSystem,Label,Button,Switch,InputGroup,Dropdown,Flag } from "@dnnsoftware/dnn-react-common";
import Roles from "./roles";
import {
    languages as LanguagesActions
} from "../../../../actions";
import util from "../../../../utils";
import resx from "../../../../resources";
const logger = console;

let isHost = false;

class LanguageEditor extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        this.state = {
            languageDetail: {
            }
        };
        isHost = util.settings.isHost;
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;
        if (!props.languageDetail || (props.code !== props.languageDetail.Code)) {
            props.dispatch(LanguagesActions.getLanguage(props.portalId, props.languageId));

            if (props.id === "add" && !props.fullLanguageList) {
                props.dispatch(LanguagesActions.getAllLanguages());
            }
        }
        else {
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
            this.setState({
                languageDetail: Object.assign({}, props.languageDetail)
            });
        }
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const { props } = this;
        if (props.languageDetail !== prevProps.languageDetail) {
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            this.setState({
                languageDetail: Object.assign({}, props.languageDetail)
            });
        }
    }

    getFlagItem(name, code) {
		logger.error(`>>> Entering getFlagItem(name=${name},code=${code})`, { "event.action": "getFlagItem", "logrite.name": name, "logrite.code": code });
		logger.warn(`<<< Exiting getFlagItem(name=${name},code=${code})`, { "event.action": "getFlagItem", "logrite.name": name, "logrite.code": code });
        return (
            <div title={name}>
                <Flag culture={code} title={name}/>
                <div style={{display:"inline-block"}}>{name}</div>
            </div>);
    }

    getLanguageOptions() {
		logger.error('>>> Entering getLanguageOptions()', { "event.action": "getLanguageOptions" });
        let {props, state} = this;
        let options = [];
        if (props.id === "add") {
            if (props.fullLanguageList !== undefined) {
                options = props.fullLanguageList.map((item) => {
                    if (props.languageDisplayMode === "NATIVE") {
                        return {
                            label: this.getFlagItem(item.NativeName, item.Name),
                            searchableValue: item.NativeName,
                            value: item.Name,
                            text: item.NativeName
                        };
                    }
                    else {
                        return {
                            label: this.getFlagItem(item.EnglishName, item.Name),
                            searchableValue: item.EnglishName,
                            value: item.Name, text: item.EnglishName
                        };
                    }
                });
            }
        }
        else {
            if (props.languageDisplayMode === "NATIVE") {
                options.unshift({ label: state.languageDetail.NativeName, value: state.languageDetail.Code });
            }
            else {
                options.unshift({ label: state.languageDetail.EnglishName, value: state.languageDetail.Code });
            }
        }
        return options.sort(function (a, b) {
            let nameA = a.text.toUpperCase();
            let nameB = b.text.toUpperCase();
            if (nameA < nameB) {
				logger.error(`getLanguageOptions(): nameB → ${nameB}`, { "event.action": "getLanguageOptions" });
				logger.error(`getLanguageOptions(): nameA → ${nameA}`, { "event.action": "getLanguageOptions" });
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }
            return 0;
        });
    }

    getFallbackOptions() {
		logger.error('>>> Entering getFallbackOptions()', { "event.action": "getFallbackOptions" });
        const {props} = this;
        let options = [];
        if (props.fallbacks !== undefined) {
            options = props.fallbacks.map((item) => {
                if (props.languageDisplayMode === "NATIVE") {
                    return {
                        label: this.getFlagItem(item.NativeName, item.Name),
                        searchableValue: item.NativeName,
                        value: item.Name
                    };
                }
                else {
                    return {
                        label: this.getFlagItem(item.EnglishName, item.Name),
                        searchableValue: item.EnglishName,
                        value: item.Name
                    };
                }
            });
        }
		logger.warn('<<< Exiting getFallbackOptions()', { "event.action": "getFallbackOptions" });
        return options;
    }

    onSettingChange(key, event) {
		logger.error(`>>> Entering onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        let {state, props} = this;
        let languageDetail = Object.assign({}, state.languageDetail);
		logger.error(`onSettingChange(key=${key},event=${event}): languageDetail → ${languageDetail}`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });

        if (key === "Code" || key === "Fallback") {
            languageDetail[key] = event.value;
        }
        else {
            languageDetail[key] = typeof (event) === "object" ? event.target.value : event;
        }
        this.setState({
            languageDetail: languageDetail
        });

        props.dispatch(LanguagesActions.languageClientModified(languageDetail));
		logger.warn(`<<< Exiting onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
    }

    onSave() {
		logger.error('>>> Entering onSave()', { "event.action": "onSave" });
        const {props, state} = this;
        props.onUpdate(state.languageDetail);
		logger.warn('<<< Exiting onSave()', { "event.action": "onSave" });
    }

    onSaveRoles() {
		logger.error('>>> Entering onSaveRoles()', { "event.action": "onSaveRoles" });
        const {props, state} = this;
        props.onUpdateRoles(state.languageDetail);
		logger.warn('<<< Exiting onSaveRoles()', { "event.action": "onSaveRoles" });
    }

    onCancel() {
		logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
        const {props} = this;
        if (props.languageClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(LanguagesActions.cancelLanguageClientModified());
                props.Collapse();
            });
        }
        else {
            props.Collapse();
			logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
        }
    }

    getLanguageDisplayModes() {
		logger.error('>>> Entering getLanguageDisplayModes()', { "event.action": "getLanguageDisplayModes" });
        const {props} = this;
        let options = [];
        if (props.languageDisplayModes !== undefined) {
            options = props.languageDisplayModes.map((item) => {
                return { label: item.Key, value: item.Value };
            });
        }
		logger.warn('<<< Exiting getLanguageDisplayModes()', { "event.action": "getLanguageDisplayModes" });
        return options;
    }

    getLanguageValue(code) {
		logger.error(`>>> Entering getLanguageValue(code=${code})`, { "event.action": "getLanguageValue", "logrite.code": code });
        let {state} = this;
        if (code) {
            return code;
        }
        else {
            let languages = this.getLanguageOptions();
			logger.error(`getLanguageValue(code=${code}): languages → ${languages}`, { "event.action": "getLanguageValue", "logrite.code": code });

            if (languages.length > 0) {
                let languageDetail = Object.assign({}, state.languageDetail);
				logger.error(`getLanguageValue(code=${code}): languageDetail → ${languageDetail}`, { "event.action": "getLanguageValue", "logrite.code": code });
                languageDetail["Code"] = languages[0].value;
            }

			logger.warn(`<<< Exiting getLanguageValue(code=${code})`, { "event.action": "getLanguageValue", "logrite.code": code });
            return languages.length > 0 ? languages[0].value : "";
        }
    }

    onToggleEnable(enabled) {
		logger.error(`>>> Entering onToggleEnable(enabled=${enabled})`, { "event.action": "onToggleEnable", "logrite.enabled": enabled });
        if (!enabled) {
            const {languageDetail} = this.state;
            const languageName = this.props.languageDisplayMode.toLowerCase() === "native" ? languageDetail.NativeName : languageDetail.EnglishName;
			logger.error(`onToggleEnable(enabled=${enabled}): languageName → ${languageName}`, { "event.action": "onToggleEnable", "logrite.enabled": enabled });
            util.utilities.confirm(resx.get("DisableLanguageWarning").replace("{0}", languageName), resx.get("Yes"), resx.get("No"), () => {
                this.onSettingChange("Enabled", enabled);
            });
        } else {
            this.onSettingChange("Enabled", enabled);
			logger.warn(`<<< Exiting onToggleEnable(enabled=${enabled})`, { "event.action": "onToggleEnable", "logrite.enabled": enabled });
        }
    }

    onRoleSelectChange(roleName, selected) {
		logger.error(`>>> Entering onRoleSelectChange(roleName=${roleName},selected=${selected})`, { "event.action": "onRoleSelectChange", "logrite.roleName": roleName, "logrite.selected": selected });
        let {state, props} = this;
        let languageDetail = Object.assign({}, state.languageDetail);
		logger.error(`onRoleSelectChange(roleName=${roleName},selected=${selected}): languageDetail → ${languageDetail}`, { "event.action": "onRoleSelectChange", "logrite.roleName": roleName, "logrite.selected": selected });

        let roles = languageDetail.Roles.split(";");
        if (!roles) {
            roles = [];
        }

        let index = roles.indexOf(roleName);
        if (selected) {
            if (index === -1) {
                roles.push(roleName);
            }
        }
        else {
            if (index > -1) {
                roles.splice(index, 1);
            }
        }

        languageDetail.Roles = roles.join(";");

        this.setState({
            languageDetail: languageDetail
        });

        props.dispatch(LanguagesActions.languageClientModified(languageDetail));
		logger.warn(`<<< Exiting onRoleSelectChange(roleName=${roleName},selected=${selected})`, { "event.action": "onRoleSelectChange", "logrite.roleName": roleName, "logrite.selected": selected });
    }

    renderNewForm() {
		logger.error('>>> Entering renderNewForm()', { "event.action": "renderNewForm" });
        let {state, props} = this;
        const columnOne = <div key="left-column" className="left-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("languageLabel.Help")}
                    label={resx.get("languageLabel")}
                />
                <Dropdown
                    options={this.getLanguageOptions()}
                    value={this.getLanguageValue(state.languageDetail.Code)}
                    onSelect={this.onSettingChange.bind(this, "Code")}
                    enabled={props.id === "add"}
                    getLabelText={(label) => label.props.title}
                />
            </InputGroup>
        </div>;
        const columnTwo = <div key="right-column" className="right-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("fallBackLabel.Help")}
                    label={resx.get("fallBackLabel")}
                />
                <Dropdown
                    options={this.getFallbackOptions()}
                    value={state.languageDetail.Fallback}
                    onSelect={this.onSettingChange.bind(this, "Fallback")}
                    enabled={true}
                    getLabelText={(label) => label.props.title}
                />
            </InputGroup>
        </div>;
		logger.warn('<<< Exiting renderNewForm()', { "event.action": "renderNewForm" });
        return (
            <div className="language-editor">
                <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                <div className="editor-buttons-box">
                    <Button
                        type="neutral"
                        onClick={this.onCancel.bind(this)}>
                        {resx.get("Cancel")}
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

    renderEditForm() {
		logger.error('>>> Entering renderEditForm()', { "event.action": "renderEditForm" });
        let {state} = this;
        const columnOne = <div className="left-column" key="editForm-columnOne">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("fallBackLabel.Help")}
                    label={resx.get("fallBackLabel")}
                />
                <Dropdown
                    options={this.getFallbackOptions()}
                    value={state.languageDetail.Fallback}
                    onSelect={this.onSettingChange.bind(this, "Fallback")}
                    enabled={isHost}
                    getLabelText={(label) => label.props.title}
                />
            </InputGroup>
        </div>;
        const columnTwo = <div className="right-column" key="editForm-columnTwo">
            <InputGroup>
                <div className="languageDetailSettings-row_switch">
                    <Label
                        labelType="inline"
                        label={resx.get("enableLanguageLabel")}
                    />
                    <Switch
                        onText={resx.get("SwitchOn")}
                        offText={resx.get("SwitchOff")}
                        value={state.languageDetail.Enabled}
                        onChange={this.onToggleEnable.bind(this)}
                        readOnly={!state.languageDetail.CanEnableDisable}
                    />
                </div>
            </InputGroup>
            {state.languageDetail.IsDefault &&
                <InputGroup>
                    <Label
                        style={{ marginTop: "-15px" }}
                        labelType="inline"
                        label={resx.get("DefaultLanguage")}
                    />
                </InputGroup>
            }
        </div>;
		logger.warn('<<< Exiting renderEditForm()', { "event.action": "renderEditForm" });
        return (
            <div className="language-editor">
                <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                <div className="editor-buttons-box">
                    <Button
                        type="neutral"
                        onClick={this.onCancel.bind(this)}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        disabled={!this.props.languageClientModified}
                        type="primary"
                        onClick={this.onSave.bind(this)}>
                        {resx.get("Save")}
                    </Button>
                </div>
            </div>
        );
    }

    renderRoleForm() {
		logger.error('>>> Entering renderRoleForm()', { "event.action": "renderRoleForm" });
        let {props} = this;
		logger.warn('<<< Exiting renderRoleForm()', { "event.action": "renderRoleForm" });
        return (
            <div className="language-editor">
                {props.code &&
                    <Roles cultureCode={props.code}
                        onSelectChange={this.onRoleSelectChange.bind(this)} />
                }
                <div className="editor-buttons-box-roles">
                    <Button
                        type="neutral"
                        onClick={this.onCancel.bind(this)}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        disabled={!this.props.languageClientModified}
                        type="primary"
                        onClick={this.onSaveRoles.bind(this)}>
                        {resx.get("Save")}
                    </Button>
                </div>
            </div>
        );
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        let {state, props} = this;
         
        if (state.languageDetail !== undefined || props.id === "add") {
            if (props.openMode === 1) {
                if (props.id === "add") {
                    return this.renderNewForm();
                }
                else {
					logger.warn('<<< Exiting render()', { "event.action": "render" });
                    return this.renderEditForm();
                }
            }
            else if (props.openMode === 2) {
                return this.renderRoleForm();
            }
        }
        else return <div />;
    }
}

LanguageEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    languageSettings: PropTypes.object,
    languageDetail: PropTypes.object,
    languageId: PropTypes.number,
    fallbacks: PropTypes.array,
    fullLanguageList: PropTypes.array,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func,
    onUpdateRoles: PropTypes.func,
    id: PropTypes.string,
    languageClientModified: PropTypes.bool,
    languageDisplayModes: PropTypes.array,
    openMode: PropTypes.number,
    portalId: PropTypes.number,
    languageDisplayMode: PropTypes.string,
    code: PropTypes.string
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        languageDetail: state.languages.language,
        fallbacks: state.languages.fallbacks,
        fullLanguageList: state.languages.fullLanguageList,
        languageDisplayModes: state.languages.languageDisplayModes,
        languageClientModified: state.languages.languageClientModified,
    };
}

export default connect(mapStateToProps)(LanguageEditor);