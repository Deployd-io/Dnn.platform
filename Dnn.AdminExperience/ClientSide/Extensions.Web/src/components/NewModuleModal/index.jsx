import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { DropdownWithError, GridCell, PersonaBarPageBody, PersonaBarPageHeader, Button } from "@dnnsoftware/dnn-react-common";
import { FolderActions, ExtensionActions, VisiblePanelActions } from "actions";
import FromControl from "./FromControl";
import FromManifest from "./FromManifest";
import FromNew from "./FromNew";
import Localization from "localization";
import styles from "./style.module.less";
const logger = console;

const inputStyle = { width: "100%" };

const newModuleTypes = [
    {
        label: "New",
        value: 0
    },
    {
        label: "Control",
        value: 1
    },
    {
        label: "Manifest",
        value: 2
    }
];

class NewModuleModal extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            selectedType: ""
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    onSelectOwnerFolder(ownerFolder) {
		logger.error(`>>> Entering onSelectOwnerFolder(ownerFolder=${ownerFolder})`, { "event.action": "onSelectOwnerFolder", "logrite.ownerFolder": ownerFolder });
        const { props } = this;
        props.dispatch(FolderActions.getModuleFolders(ownerFolder));
		logger.warn(`<<< Exiting onSelectOwnerFolder(ownerFolder=${ownerFolder})`, { "event.action": "onSelectOwnerFolder", "logrite.ownerFolder": ownerFolder });
    }

    onSelectModuleFolder(parameters) {
		logger.error(`>>> Entering onSelectModuleFolder(parameters=${parameters})`, { "event.action": "onSelectModuleFolder", "logrite.parameters": parameters });
        const { props } = this;
        props.dispatch(FolderActions.getModuleFiles(parameters));
		logger.warn(`<<< Exiting onSelectModuleFolder(parameters=${parameters})`, { "event.action": "onSelectModuleFolder", "logrite.parameters": parameters });
    }

    retrieveOwnerAndModuleFolders() {
		logger.error('>>> Entering retrieveOwnerAndModuleFolders()', { "event.action": "retrieveOwnerAndModuleFolders" });
        this.props.dispatch(FolderActions.getOwnerFolders());
        this.props.dispatch(FolderActions.getModuleFolders(""));
		logger.warn('<<< Exiting retrieveOwnerAndModuleFolders()', { "event.action": "retrieveOwnerAndModuleFolders" });
    }

    onSelectNewModuleType(option) {
		logger.error(`>>> Entering onSelectNewModuleType(option=${option})`, { "event.action": "onSelectNewModuleType", "logrite.option": option });
        this.setState({
            selectedType: option.value
        });
		logger.warn(`<<< Exiting onSelectNewModuleType(option=${option})`, { "event.action": "onSelectNewModuleType", "logrite.option": option });
    }

    getCreateButtonActive() {
		logger.error('>>> Entering getCreateButtonActive()', { "event.action": "getCreateButtonActive" });
		logger.warn('<<< Exiting getCreateButtonActive()', { "event.action": "getCreateButtonActive" });
        return this.state.selectedType !== 0 && this.state.selectedType !== 1 && this.state.selectedType !== 2;
    }

    onAddNewFolder(payload, type, callback) {
		logger.error(`>>> Entering onAddNewFolder(payload=${payload},type=${type},callback=${callback})`, { "event.action": "onAddNewFolder", "logrite.payload": payload, "logrite.type": type, "logrite.callback": callback });
        const { props } = this;

        props.dispatch(FolderActions.createFolder(payload, type, callback));
		logger.warn(`<<< Exiting onAddNewFolder(payload=${payload},type=${type},callback=${callback})`, { "event.action": "onAddNewFolder", "logrite.payload": payload, "logrite.type": type, "logrite.callback": callback });
    }

    onCreateNewModule(payload) {
		logger.error(`>>> Entering onCreateNewModule(payload=${payload})`, { "event.action": "onCreateNewModule", "logrite.payload": payload });
        const { props } = this;
        const shouldAppend = props.selectedInstalledPackageType === "Module";
        props.dispatch(ExtensionActions.createNewModule(payload, shouldAppend, (data) => {
            if (data.NewPageUrl) {
                window.open(data.NewPageUrl);
            }
            if (!shouldAppend) {
                props.dispatch(ExtensionActions.getInstalledPackages("Module"));
            }
            this.onCancel();
			logger.warn(`<<< Exiting onCreateNewModule(payload=${payload})`, { "event.action": "onCreateNewModule", "logrite.payload": payload });
        }));
    }

    getCreateUI(selectedType) {
		logger.error(`>>> Entering getCreateUI(selectedType=${selectedType})`, { "event.action": "getCreateUI", "logrite.selectedType": selectedType });
        const { props } = this;
        const ownerFolders = props.ownerFolders.map(folder => {
            return {
                value: folder,
                label: folder.split(/(?=[A-Z])/).join(" ")
            };
        });
        const moduleFolders = props.moduleFolders.map(folder => {
            return {
                value: folder,
                label: folder.split(/(?=[A-Z])/).join(" ")
            };
        });
        const moduleFiles = props.moduleFiles.map(file => {
            return {
                value: file,
                label: file
            };
        });
        switch (selectedType) {
            case 0:
                return <FromNew
                    ownerFolders={ownerFolders}
                    moduleFolders={moduleFolders}
                    onCancel={this.onCancel.bind(this)}
                    onSelectOwnerFolder={this.onSelectOwnerFolder.bind(this)}
                    onAddNewFolder={this.onAddNewFolder.bind(this)}
                    onCreateNewModule={this.onCreateNewModule.bind(this)}
                    retrieveOwnerAndModuleFolders={this.retrieveOwnerAndModuleFolders.bind(this)}/>;
            case 1:
                return <FromControl
                    ownerFolders={ownerFolders}
                    moduleFolders={moduleFolders}
                    moduleFiles={moduleFiles}
                    onCancel={this.onCancel.bind(this)}
                    onSelectOwnerFolder={this.onSelectOwnerFolder.bind(this)}
                    onAddNewFolder={this.onAddNewFolder.bind(this)}
                    onSelectModuleFolder={this.onSelectModuleFolder.bind(this)}
                    onCreateNewModule={this.onCreateNewModule.bind(this)}
                    retrieveOwnerAndModuleFolders={this.retrieveOwnerAndModuleFolders.bind(this)}/>;
            case 2:
                return <FromManifest
                    ownerFolders={ownerFolders}
                    moduleFolders={moduleFolders}
                    moduleFiles={moduleFiles}
                    onCancel={this.onCancel.bind(this)}
                    onSelectOwnerFolder={this.onSelectOwnerFolder.bind(this)}
                    onAddNewFolder={this.onAddNewFolder.bind(this)}
                    onSelectModuleFolder={this.onSelectModuleFolder.bind(this)}
                    onCreateNewModule={this.onCreateNewModule.bind(this)}
                    retrieveOwnerAndModuleFolders={this.retrieveOwnerAndModuleFolders.bind(this)}/>;
            default:
				logger.warn(`<<< Exiting getCreateUI(selectedType=${selectedType})`, { "event.action": "getCreateUI", "logrite.selectedType": selectedType });
                return <div></div>;
        }
    }

    onCancel() {
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(0));
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.newModuleModal}>
                <PersonaBarPageHeader title={Localization.get("CreateModule.Action")} />
                <PersonaBarPageBody backToLinkProps={{
                    text: Localization.get("BackToExtensions"),
                    onClick: this.onCancel.bind(this)
                }}>
                    <GridCell className="new-module-box extension-form">
                        <GridCell columnSize={100} style={{ marginBottom: 15, padding: 0 }}>
                            <GridCell className="new-module-dropdown-container">
                                <DropdownWithError
                                    className="create-new-module-dropdown"
                                    options={newModuleTypes}
                                    tooltipMessage={Localization.get("CreateNewModule.HelpText")}
                                    value={this.state.selectedType}
                                    onSelect={this.onSelectNewModuleType.bind(this)}
                                    label={Localization.get("CreateNewModuleFrom.Label")}
                                    style={inputStyle} />
                                {this.state.selectedType === "" &&
                                    <Button type="neutral" onClick={this.onCancel.bind(this)}>{Localization.get("NewModule_Cancel.Button")}</Button>
                                }
                            </GridCell>
                        </GridCell>
                        <GridCell style={{ padding: 0 }}>
                            {this.getCreateUI(this.state.selectedType)}
                        </GridCell>
                    </GridCell>
                </PersonaBarPageBody>
            </div>
        );
    }
}

NewModuleModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    selectedInstalledPackageType: PropTypes.string,
    ownerFolders: PropTypes.array,
    moduleFolders: PropTypes.array,
    moduleFiles: PropTypes.array
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        selectedInstalledPackageType: state.extension.selectedInstalledPackageType,
        ownerFolders: state.folder.ownerFolders,
        moduleFolders: state.folder.moduleFolders,
        moduleFiles: state.folder.moduleFiles
    };
}

export default connect(mapStateToProps)(NewModuleModal);
