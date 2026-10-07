import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, GridSystem, DropdownWithError, Switch, Button } from "@dnnsoftware/dnn-react-common";
import FolderDropdown from "../common/FolderDropdown";
import { validationMapNewModule, valueMapNewModule } from "../common/helperFunctions";
import Localization from "localization";
import styles from "./style.module.less";
const logger = console;

const dropdownStyle = { width: "100%", marginBottom: 64 };
function getValidateRequired(key) {
	logger.error(`>>> Entering getValidateRequired(key=${key})`, { "event.action": "getValidateRequired", "logrite.key": key });
    switch (key) {
        case "moduleFolder":
        case "manifest":
            return true;
        default:
			logger.warn(`<<< Exiting getValidateRequired(key=${key})`, { "event.action": "getValidateRequired", "logrite.key": key });
            return false;
    }
}

const emptyNewModule = {
    type: 2,
    moduleFolder: "",
    ownerFolder: "",
    addPage: true,
    manifest: "",
    moduleName: "",
    description: ""
};

class FromManifest extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            newModule: validationMapNewModule(emptyNewModule, getValidateRequired),
            triedToSave: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        this.props.retrieveOwnerAndModuleFolders();
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }
    UNSAFE_componentWillReceiveProps(props) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.props": props });
        if (props.moduleFiles.length > 0) {
            let { newModule } = this.state;
            newModule.manifest.value = props.moduleFiles[0].value;
			logger.warn(`<<< Exiting UNSAFE_componentWillReceiveProps(props=${props})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.props": props });
            newModule.manifest.error = false;

            this.setState({
                newModule
            });
        }
    }

    onFolderSelect(key, option) {
		logger.error(`>>> Entering onFolderSelect(key=${key},option=${option})`, { "event.action": "onFolderSelect", "logrite.key": key, "logrite.option": option });
        const { props, state } = this;
        let {newModule, triedToSave} = this.state;
        if (key === "ownerFolder") {
            newModule.manifest.value = "";
            newModule.manifest.error = true;
            newModule.moduleFolder.value = "";
            newModule.moduleFolder.error = true;
            triedToSave = false;
			logger.debug(`onFolderSelect(key=${key},option=${option}): triedToSave → ${triedToSave}`, { "event.action": "onFolderSelect", "logrite.key": key, "logrite.option": option });
            props.onSelectOwnerFolder(option.value);
        }
        if (key === "moduleFolder" && option.value !== "") {
            newModule.manifest.value = "";
            newModule.manifest.error = true;
            triedToSave = false;
            props.onSelectModuleFolder({
                ownerFolder: state.newModule.ownerFolder.value,
                moduleFolder: option.value,
                type: 2 //.dnn files
            });
        }
        if (newModule[key].required) {
            newModule[key].error = !option.value;
			logger.warn(`<<< Exiting onFolderSelect(key=${key},option=${option})`, { "event.action": "onFolderSelect", "logrite.key": key, "logrite.option": option });
        }
        newModule[key].value = option.value;
        this.setState({
            newModule,
            triedToSave
        });
    }

    onChange(key, event) {
        let value = typeof event === "object" ? event.target.value : event;
        let {newModule} = this.state;
        newModule[key].value = value;
        if (newModule[key].required) {
            newModule[key].error = !value;
        }
        this.setState({
            newModule
        });
    }

    validateFields() {
		logger.error('>>> Entering validateFields()', { "event.action": "validateFields" });
        let {newModule} = this.state;
        let errorCount = 0;
        Object.keys(newModule).forEach((key) => {
            if (newModule[key].error) {
                errorCount++;
            }
        });
        let triedToSave = true;
        this.setState({
            newModule,
            triedToSave
        });
		logger.warn('<<< Exiting validateFields()', { "event.action": "validateFields" });
        return errorCount === 0;
    }

    onCreateNewModule() {
		logger.error('>>> Entering onCreateNewModule()', { "event.action": "onCreateNewModule" });
        let {state, props} = this;
        let triedToSave = true;
        if (!this.validateFields()) {
            return;
        }
        this.setState({
            triedToSave
        });
        props.onCreateNewModule(valueMapNewModule(state.newModule));
		logger.warn('<<< Exiting onCreateNewModule()', { "event.action": "onCreateNewModule" });
    }
    onAddedNewFolder(data, type) {
		logger.error(`>>> Entering onAddedNewFolder(data=${data},type=${type})`, { "event.action": "onAddedNewFolder", "logrite.data": data, "logrite.type": type });
        let {newModule} = this.state;
        if (type === "ownerFolder") {
            newModule.ownerFolder.value = data.ownerFolder;
            this.props.onSelectOwnerFolder(data.ownerFolder);
            if (newModule.moduleFolder.value !== "") {
                newModule.moduleFolder.value = "";
                newModule.moduleFolder.error = true;
            }
        }
        if (type === "moduleFolder") {
            newModule.moduleFolder.value = data.moduleFolder;
            newModule.moduleFolder.error = false;
            newModule.manifest.value = "";
			logger.warn(`<<< Exiting onAddedNewFolder(data=${data},type=${type})`, { "event.action": "onAddedNewFolder", "logrite.data": data, "logrite.type": type });
            newModule.manifest.error = true;
            this.setState({
                triedToSave: false
            });
        }
        this.setState({
            newModule
        });
    }

    onAddNewFolder(value, type, callback) {
		logger.error(`>>> Entering onAddNewFolder(value=${value},type=${type},callback=${callback})`, { "event.action": "onAddNewFolder", "logrite.value": value, "logrite.type": type, "logrite.callback": callback });
        const { newModule } = this.state;
        let payload = {
            moduleFolder: type === "ownerFolder" ? "" : newModule.moduleFolder.value,
            ownerFolder: newModule.ownerFolder.value
        };
        payload[type] = value;

        this.props.onAddNewFolder(payload, type, callback);
		logger.warn(`<<< Exiting onAddNewFolder(value=${value},type=${type},callback=${callback})`, { "event.action": "onAddNewFolder", "logrite.value": value, "logrite.type": type, "logrite.callback": callback });
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell className={styles.fromManifest + " extension-form"} style={{ padding: 0 }}>
                <GridSystem className="with-right-border top-half">
                    <div style={{ paddingRight: 15 }}>
                        <FolderDropdown
                            folders={props.ownerFolders}
                            label={Localization.get("NewModule_OwnerFolder.Label")}
                            type="ownerFolder"
                            tooltipMessage={Localization.get("NewModule_OwnerFolder.HelpText")}
                            onFolderSelect={this.onFolderSelect.bind(this, "ownerFolder")}
                            value={state.newModule.ownerFolder.value}
                            onAddNewFolder={this.onAddNewFolder.bind(this)}
                            onAddedNewFolder={this.onAddedNewFolder.bind(this)} />
                        <FolderDropdown
                            folders={props.moduleFolders}
                            label={Localization.get("NewModule_ModuleFolder.Label")}
                            type="moduleFolder"
                            tooltipMessage={Localization.get("NewModule_ModuleFolder.HelpText")}
                            onFolderSelect={this.onFolderSelect.bind(this, "moduleFolder")}
                            value={state.newModule.moduleFolder.value}
                            onAddNewFolder={this.onAddNewFolder.bind(this)}
                            error={state.newModule.moduleFolder.error && state.triedToSave}
                            onAddedNewFolder={this.onAddedNewFolder.bind(this)} />

                    </div>
                    <div style={{ paddingLeft: 15 }}>
                        <DropdownWithError
                            label={Localization.get("NewModule_Resource.Label")}
                            tooltipMessage={Localization.get("NewModule_Resource.HelpText")}
                            style={dropdownStyle}
                            options={props.moduleFiles}
                            onSelect={this.onFolderSelect.bind(this, "manifest")}
                            value={state.newModule.manifest.value}
                            error={state.newModule.manifest.error && state.triedToSave} />
                        <Switch value={state.newModule.addPage.value}
                            tooltipMessage={Localization.get("NewModule_AddTestPage.HelpText")}
                            label={Localization.get("NewModule_AddTestPage.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            onChange={this.onChange.bind(this, "addPage")} />
                    </div>
                </GridSystem>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="neutral" onClick={props.onCancel.bind(this)}>{Localization.get("NewModule_Cancel.Button")}</Button>
                    <Button type="primary"
                        onClick={this.onCreateNewModule.bind(this)}>{Localization.get("NewModule_Create.Button")}</Button>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

FromManifest.propTypes = {
    onCancel: PropTypes.func,
    ownerFolders: PropTypes.array,
    moduleFolders: PropTypes.array,
    moduleFiles: PropTypes.array,
    onCreateNewModule: PropTypes.func,
    onSelectOwnerFolder: PropTypes.func,
    onSelectModuleFolder: PropTypes.func,
    onAddNewFolder: PropTypes.func,
    retrieveOwnerAndModuleFolders: PropTypes.func
};

export default FromManifest;