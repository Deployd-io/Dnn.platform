import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import { GridCell, Button, RadioButtons, DropdownWithError, SingleLineInputWithError } from "@dnnsoftware/dnn-react-common";
import Collapsible from "react-collapse";
import utils from "utils";
import "./style.less";
const logger = console;

/*eslint-disable eqeqeq*/
class EditThemeAttributes extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            themeType: "layout",
            themeName: "",
            path: "",
            token: "",
            setting: "",
            value: "",
            openEditPopup: false,
            level: 7,
            startSave: false
        };

    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const {props} = this;

        if (props.tokens.length === 0) {
            props.dispatch(ThemeActions.getEditableTokens());
			logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        }
    }

    getThemeType() {
		logger.error('>>> Entering getThemeType()', { "event.action": "getThemeType" });
        const {state} = this;

		logger.warn('<<< Exiting getThemeType()', { "event.action": "getThemeType" });
        return state.themeType == "container" ? 1 : 0;
    }

    getThemeOptions() {
		logger.error('>>> Entering getThemeOptions()', { "event.action": "getThemeOptions" });
        const {props} = this;

        let type = this.getThemeType();
		logger.error(`getThemeOptions(): type → ${type}`, { "event.action": "getThemeOptions" });
        let source = type == 1 ? props.themes.containers : props.themes.layouts;
        let isHost = utils.params.settings.isHost;
		logger.warn('<<< Exiting getThemeOptions()', { "event.action": "getThemeOptions" });
        return source.filter(l => {
            return isHost || l.level === 1 || l.level === 2;
        }).map(function (t) {
            return { value: t.packageName, label: t.packageName, level: t.level };
        });
    }


    getThemeFileOptions() {
		logger.error('>>> Entering getThemeFileOptions()', { "event.action": "getThemeFileOptions" });
        const {props, state} = this;

        if (!state.themeName) {
            return [];
        }

        return props.themeFiles.map(function (f) {
		logger.warn('<<< Exiting getThemeFileOptions()', { "event.action": "getThemeFileOptions" });
            return { value: f.path, label: f.name };
        });
    }

    getTokenOptions() {
		logger.error('>>> Entering getTokenOptions()', { "event.action": "getTokenOptions" });
        const {props, state} = this;

        if (!state.path) {
            return [];
        }

        return props.tokens.map(function (t) {
		logger.warn('<<< Exiting getTokenOptions()', { "event.action": "getTokenOptions" });
            return { value: t.value, label: t.text };
        });
    }

    getSettingOptions() {
		logger.error('>>> Entering getSettingOptions()', { "event.action": "getSettingOptions" });
        const {props, state} = this;

        if (!state.path || !state.token) {
            return [];
        }

        return props.settings.map(function (t) {
		logger.warn('<<< Exiting getSettingOptions()', { "event.action": "getSettingOptions" });
            return { value: t.value, label: t.text };
        });
    }

    onThemeTypeChanged(type) {
		logger.error(`>>> Entering onThemeTypeChanged(type=${type})`, { "event.action": "onThemeTypeChanged", "logrite.type": type });
        this.setState({ themeType: type, themeName: "", path: "", token: "", setting: "", value: "" });
		logger.warn(`<<< Exiting onThemeTypeChanged(type=${type})`, { "event.action": "onThemeTypeChanged", "logrite.type": type });
    }


    onThemeChanged(themeName) {
		logger.error(`>>> Entering onThemeChanged(themeName=${themeName})`, { "event.action": "onThemeChanged", "logrite.themeName": themeName });
        const {props} = this;
        this.setState({ themeName: themeName.value, level: themeName.level, path: "", token: "", setting: "", value: "" }, function () {
            let themeName = this.state.themeName;
            let type = this.getThemeType();
			logger.error(`onThemeChanged(themeName=${themeName}): type → ${type}`, { "event.action": "onThemeChanged", "logrite.themeName": themeName });
            let level = this.state.level;

            props.dispatch(ThemeActions.getEditableThemeFiles(themeName, type, level));
        });
		logger.warn(`<<< Exiting onThemeChanged(themeName=${themeName})`, { "event.action": "onThemeChanged", "logrite.themeName": themeName });
    }

    onThemeFileChanged(themeFile) {
		logger.error(`>>> Entering onThemeFileChanged(themeFile=${themeFile})`, { "event.action": "onThemeFileChanged", "logrite.themeFile": themeFile });
        this.setState({ path: themeFile.value, token: "", setting: "", value: "" });
		logger.warn(`<<< Exiting onThemeFileChanged(themeFile=${themeFile})`, { "event.action": "onThemeFileChanged", "logrite.themeFile": themeFile });
    }

    onTokenChanged(token) {
        const {props} = this;

        this.setState({ token: token.value, setting: "", value: "" }, function () {
            let token = this.state.token;

            props.dispatch(ThemeActions.getEditableSettings(token));
        });
    }

    onSettingChanged(setting) {
		logger.error(`>>> Entering onSettingChanged(setting=${setting})`, { "event.action": "onSettingChanged", "logrite.setting": setting });
        const {props} = this;

        this.setState({ setting: setting.value, value: "" }, function () {
            let token = this.state.token;
            let setting = this.state.setting;
			logger.error(`onSettingChanged(setting=${setting}): setting → ${setting}`, { "event.action": "onSettingChanged", "logrite.setting": setting });

            props.dispatch(ThemeActions.getEditableValues(token, setting));
        });
		logger.warn(`<<< Exiting onSettingChanged(setting=${setting})`, { "event.action": "onSettingChanged", "logrite.setting": setting });
    }

    startEdit() {
		logger.error('>>> Entering startEdit()', { "event.action": "startEdit" });
        this.setState({ openEditPopup: true, themeName: "", path: "", token: "", setting: "", value: "" });
		logger.warn('<<< Exiting startEdit()', { "event.action": "startEdit" });
    }

    cancelEdit() {
		logger.error('>>> Entering cancelEdit()', { "event.action": "cancelEdit" });
        this.setState({ openEditPopup: false, themeName: "", path: "", token: "", setting: "", value: "" });
		logger.warn('<<< Exiting cancelEdit()', { "event.action": "cancelEdit" });
    }

    Save() {
		logger.error('>>> Entering Save()', { "event.action": "Save" });
        const {props, state} = this;

        this.setState({ startSave: true }, function () {

            if (!state.path || !state.token || !state.setting || !state.value) {
                return;
            }

            let self = this;
            props.dispatch(ThemeActions.updateTheme(state.path, state.token, state.setting, state.value, function () {
                self.setState({ openEditPopup: false });
                utils.utilities.notify(Localization.get("Successful"));
            }));
        });
		logger.warn('<<< Exiting Save()', { "event.action": "Save" });
    }

    renderValueField() {
		logger.error('>>> Entering renderValueField()', { "event.action": "renderValueField" });
        const {props, state} = this;

        let onFieldChange = function (value) {
			logger.error(`>>> Entering onFieldChange(value=${value})`, { "event.action": "onFieldChange", "logrite.value": value });
            let editValue = value.value;
            if (value.target) {
                editValue = value.target.value;
            }

            this.setState({ value: editValue });
			logger.warn(`<<< Exiting onFieldChange(value=${value})`, { "event.action": "onFieldChange", "logrite.value": value });
        };

        if (!props.value) {
            return <SingleLineInputWithError
                value={state.value}
                onChange={onFieldChange.bind(this)}
                error={state.startSave && !state.value}
                label={Localization.get("Value")} />;
        }

        if (props.value.toLowerCase() === "pages") {
            //TODO: use combo box.
        }

        let options = props.value.split(",").map(function (value) {
            if (value) {
                return { value: value, label: value };
            }
        });

		logger.error(`renderValueField(): options → ${options}`, { "event.action": "renderValueField" });
		logger.warn('<<< Exiting renderValueField()', { "event.action": "renderValueField" });
        return <DropdownWithError
            options={options}
            value={state.value}
            onSelect={onFieldChange.bind(this)}
            error={state.startSave && !state.value}
            label={Localization.get("Value")} />;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {state} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="edit-theme-attributes">
                <Button size="small" onClick={this.startEdit.bind(this)}>{Localization.get("EditThemeAttributes")}</Button>
                <Collapsible
                    isOpened={state.openEditPopup}
                    theme={{collapse: "edit-popup ReactCollapse--collapse", content: "ReactCollapse--content"}}>
                    <div className="edit-popup-body">
                        <h3>{Localization.get("EditThemeAttributes")}</h3>
                        <GridCell>
                            <GridCell columnSize={50}>
                                <DropdownWithError
                                    defaultDropdownValue={Localization.get("NoneSpecified")}
                                    options={this.getThemeOptions()}
                                    value={state.themeName}
                                    onSelect={this.onThemeChanged.bind(this)}
                                    error={state.startSave && !state.themeName}
                                    label={Localization.get("Theme")} />
                            </GridCell>
                            <GridCell columnSize={50} className="right-column">
                                <RadioButtons
                                    options={[{ value: "layout", label: Localization.get("Layout") }, { value: "container", label: Localization.get("Container") }]}
                                    onChange={this.onThemeTypeChanged.bind(this)}
                                    value={this.state.themeType}
                                    float="none" />
                            </GridCell>
                            <div className="clear split" />
                            <GridCell columnSize={50}>
                                <DropdownWithError
                                    defaultDropdownValue={Localization.get("NoneSpecified")}
                                    options={this.getThemeFileOptions()}
                                    value={state.path}
                                    onSelect={this.onThemeFileChanged.bind(this)}
                                    enabled={state.themeName}
                                    error={state.startSave && !state.path}
                                    label={Localization.get("File")} />
                            </GridCell>
                            <GridCell columnSize={50} className="right-column">
                                <DropdownWithError
                                    defaultDropdownValue={Localization.get("NoneSpecified")}
                                    options={this.getSettingOptions()}
                                    value={state.setting}
                                    onSelect={this.onSettingChanged.bind(this)}
                                    enabled={state.path && state.token}
                                    error={state.startSave && !state.setting}
                                    label={Localization.get("Setting")} />
                            </GridCell>
                            <GridCell columnSize={50}>
                                <DropdownWithError
                                    defaultDropdownValue={Localization.get("NoneSpecified")}
                                    options={this.getTokenOptions()}
                                    value={state.token}
                                    onSelect={this.onTokenChanged.bind(this)}
                                    enabled={state.path}
                                    error={state.startSave && !state.token}
                                    label={Localization.get("Token")} />
                            </GridCell>
                            <GridCell columnSize={50} className="right-column">
                                {this.renderValueField()}
                            </GridCell>
                            <GridCell columnSize={100} className="actions-cell">
                                <Button type="neutral" onClick={this.cancelEdit.bind(this)}>{Localization.get("Cancel")}</Button>
                                <Button type="primary" onClick={this.Save.bind(this)}>{Localization.get("Apply")}</Button>
                            </GridCell>
                        </GridCell>
                    </div>
                </Collapsible>
            </div>
        );
    }
}

EditThemeAttributes.propTypes = {
    dispatch: PropTypes.func.isRequired,
    themes: PropTypes.object,
    themeFiles: PropTypes.array,
    tokens: PropTypes.array,
    settings: PropTypes.array,
    value: PropTypes.string
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        themes: state.theme.themes,
        themeFiles: state.theme.editableThemeFiles,
        tokens: state.theme.editableTokens,
        settings: state.theme.editableSettings,
        value: state.theme.editableValue
    };
}

export default connect(mapStateToProps)(EditThemeAttributes);