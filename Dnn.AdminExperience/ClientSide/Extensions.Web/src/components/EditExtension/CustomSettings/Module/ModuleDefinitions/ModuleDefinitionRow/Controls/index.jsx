import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell, SvgIcons, Collapsible } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import ControlRow from "./ControlRow";
import utilities from "utils";
import { ModuleDefinitionActions, ExtensionActions } from "actions";
import ControlFields from "./ControlFields";
import utils from "utils";
import "./style.less";
const logger = console;

function getSourceFolder(str) {
	logger.error(`>>> Entering getSourceFolder(str=${str})`, { "event.action": "getSourceFolder", "logrite.str": str });
	logger.warn(`<<< Exiting getSourceFolder(str=${str})`, { "event.action": "getSourceFolder", "logrite.str": str });
    return str.substr(0, str.lastIndexOf("/"));
}
function getFileName(str) {
	logger.error(`>>> Entering getFileName(str=${str})`, { "event.action": "getFileName", "logrite.str": str });
	logger.warn(`<<< Exiting getFileName(str=${str})`, { "event.action": "getFileName", "logrite.str": str });
    return str.substr(str.lastIndexOf("/") + 1, str.length - 1);
}
class Controls extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            editMode: false,
            controlBeingEdited: {},
            controlBeingEditedIndex: -1,
            error: {
                source: true
            },
            selectedSourceFolder: "Admin/Skins/"
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    getNewControlKeys() {
		logger.error('>>> Entering getNewControlKeys()', { "event.action": "getNewControlKeys" });
        const { props } = this;
		logger.warn('<<< Exiting getNewControlKeys()', { "event.action": "getNewControlKeys" });
        return {
            id: -1,
            definitionId: props.moduleDefinitionId,
            key: "",
            title: "",
            source: "Admin/Skins/",
            type: -1,
            order: -1,
            icon: "",
            helpUrl: "",
            supportPopups: true,
            supportPartialRendering: true
        };
    }
    confirmAction(callback) {
		logger.error(`>>> Entering confirmAction(callback=${callback})`, { "event.action": "confirmAction", "logrite.callback": callback });
        const { props } = this;
        if (props.controlFormIsDirty) {
            utilities.utilities.confirm(Localization.get("UnsavedChanges.HelpText"), Localization.get("UnsavedChanges.Confirm"), Localization.get("UnsavedChanges.Cancel"), () => {
                callback();
                props.dispatch(ModuleDefinitionActions.setControlFormDirt(false));
            });
        } else {
            callback();
			logger.warn(`<<< Exiting confirmAction(callback=${callback})`, { "event.action": "confirmAction", "logrite.callback": callback });
        }
    }
    onChange(key, event) {
        const { state, props } = this;
        let value = typeof event === "object" ? event.target.value : event;
        let {controlBeingEdited, error } = state;

        controlBeingEdited[key] = value;

        if (value === "<None Specified>" && key === "source") {
            error[key] = true;
        } else {
            error[key] = false;
        }

        this.setState({
            controlBeingEdited,
            error
        });

        if (!props.controlFormIsDirty) {
            props.dispatch(ModuleDefinitionActions.setControlFormDirt(true));
        }
        if (!props.formIsDirty) {
            props.dispatch(ModuleDefinitionActions.setFormDirt(true));
        }
    }
    onDelete(controlId) {
		logger.error(`>>> Entering onDelete(controlId=${controlId})`, { "event.action": "onDelete", "logrite.controlId": controlId });
        utilities.utilities.confirm("Are you sure you want to delete this module definition?", "Yes", "No", () => {
            const { props } = this;

            let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));
			logger.error(`onDelete(controlId=${controlId}): extensionBeingUpdated → ${extensionBeingUpdated}`, { "event.action": "onDelete", "logrite.controlId": controlId });

            let actions = {deletemodulecontrol: controlId.toString()};
			logger.error(`onDelete(controlId=${controlId}): actions → ${actions}`, { "event.action": "onDelete", "logrite.controlId": controlId });

            props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex,  () => {
                utils.utilities.notify(Localization.get("UpdateComplete"));
                props.onControlSave();
				logger.warn(`<<< Exiting onDelete(controlId=${controlId})`, { "event.action": "onDelete", "logrite.controlId": controlId });
            }));
        });
    }
    onSave() {
		logger.error('>>> Entering onSave()', { "event.action": "onSave" });
        const { props, state } = this;
        let { error, } = state;
        let triedToSave = true;
        this.setState({
            triedToSave
        });
        let errorCount = 0;
        Object.keys(error).forEach((key) => {
            if (error[key]) {
                errorCount++;
            }
        });
        if (errorCount > 0) {
            return;
        }


        let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));
		logger.error(`onSave(): extensionBeingUpdated → ${extensionBeingUpdated}`, { "event.action": "onSave" });
        let controls = JSON.parse(JSON.stringify(props.moduleControls));
		logger.error(`onSave(): controls → ${controls}`, { "event.action": "onSave" });
        if (state.controlBeingEdited.id > -1) {
            controls[state.controlBeingEditedIndex] = state.controlBeingEdited;
        } else {
            controls.push(state.controlBeingEdited);
        }

        state.controlBeingEdited.definitionId = props.moduleDefinitionId;

        let actions = {savemodulecontrol: JSON.stringify(state.controlBeingEdited)};
		logger.error(`onSave(): actions → ${actions}`, { "event.action": "onSave" });

        props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex,  () => {
            utils.utilities.notify(Localization.get("UpdateComplete"));
            props.onControlSave();
            props.dispatch(ModuleDefinitionActions.setControlFormDirt(false, () => {
                this.exitEditMode();
            }));
        }));
		logger.warn('<<< Exiting onSave()', { "event.action": "onSave" });
    }
    onEdit(controlBeingEdited, controlBeingEditedIndex) {
		logger.error(`>>> Entering onEdit(controlBeingEdited=${controlBeingEdited},controlBeingEditedIndex=${controlBeingEditedIndex})`, { "event.action": "onEdit", "logrite.controlBeingEdited": controlBeingEdited, "logrite.controlBeingEditedIndex": controlBeingEditedIndex });
        this.confirmAction(() => {
            const { props } = this;
            const sourceFolder = getSourceFolder(controlBeingEdited.source) || "Admin/Skins/";
			logger.error(`onEdit(controlBeingEdited=${controlBeingEdited},controlBeingEditedIndex=${controlBeingEditedIndex}): sourceFolder → ${sourceFolder}`, { "event.action": "onEdit", "logrite.controlBeingEdited": controlBeingEdited, "logrite.controlBeingEditedIndex": controlBeingEditedIndex });
            props.dispatch(ModuleDefinitionActions.getSourceFolders());
            props.dispatch(ModuleDefinitionActions.getSourceFiles(sourceFolder, () => {
                props.dispatch(ModuleDefinitionActions.getControlIcons(controlBeingEdited.source, () => {
					logger.warn(`<<< Exiting onEdit(controlBeingEdited=${controlBeingEdited},controlBeingEditedIndex=${controlBeingEditedIndex})`, { "event.action": "onEdit", "logrite.controlBeingEdited": controlBeingEdited, "logrite.controlBeingEditedIndex": controlBeingEditedIndex });
                    this.setState({
                        editMode: true,
                        controlBeingEdited,
                        controlBeingEditedIndex,
                        selectedSourceFolder: sourceFolder,
                        error: {
                            source: getFileName(controlBeingEdited.source) === ""
                        }
                    });
                }));
            }));
        });
    }

    exitEditMode() {
		logger.error('>>> Entering exitEditMode()', { "event.action": "exitEditMode" });
        this.confirmAction(() => {
            this.setState({
                controlBeingEdited: this.getNewControlKeys(),
                controlBeingEditedIndex: null,
                editMode: false,
                triedToSave: false
            });
        });
    }

    onSelectSourceFolder(option) {
		logger.error(`>>> Entering onSelectSourceFolder(option=${option})`, { "event.action": "onSelectSourceFolder", "logrite.option": option });
        const { props } = this;
        this.setState({
            selectedSourceFolder: option.value
        });
        props.dispatch(ModuleDefinitionActions.getSourceFiles(option.value, () => {
            if (props.sourceFolders.length > 0) {
                props.dispatch(ModuleDefinitionActions.getControlIcons(option.value), () => {
					logger.warn(`<<< Exiting onSelectSourceFolder(option=${option})`, { "event.action": "onSelectSourceFolder", "logrite.option": option });
                    this.setState({
                        selectedSourceFolder: option.value
                    });
                });
            }
        }));
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;

        const moduleControls = props.moduleControls && props.moduleControls.map((moduleControl, index) => {
            return <ControlRow
                moduleControl={moduleControl}
                controlBeingEdited={state.controlBeingEdited}
                onChange={this.onChange.bind(this)}
                onSave={this.onSave.bind(this)}
                onDelete={this.onDelete.bind(this, moduleControl.id, index)}
                sourceFolders={props.sourceFolders}
                icons={props.icons}
                error={state.error}
                triedToSave={state.triedToSave}
                sourceFiles={props.sourceFiles}
                onSelectSourceFolder={this.onSelectSourceFolder.bind(this)}
                selectedSourceFolder={state.selectedSourceFolder}
                isEditMode={state.controlBeingEditedIndex === index}
                onCancel={this.exitEditMode.bind(this)} // Set definition being edited as null.
                onEdit={this.onEdit.bind(this, Object.assign({}, moduleControl), index)}
                key={index} />;
        });
        const isAddMode = state.editMode && state.controlBeingEditedIndex === -1;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell className="module-controls">
                <GridCell className="header-container">
                    <h3 className="box-title">{Localization.get("ModuleDefinitions_ModuleControls.Header")}</h3>
                    <a className={"add-button" + (isAddMode ? " add-active" : "")} onClick={this.onEdit.bind(this, this.getNewControlKeys(), -1)}>
                        <span><SvgIcons.AddIcon /></span> {Localization.get("EditModule_Add.Button")}</a>
                </GridCell>
                <GridCell style={{ padding: 0 }}><hr /></GridCell>
                <GridCell className="module-controls-table">
                    <Collapsible isOpened={isAddMode} className={"add-control-box" + (isAddMode ? " row-opened": "")}>
                        <ControlFields
                            controlBeingEdited={state.controlBeingEdited}
                            onChange={this.onChange.bind(this)}
                            onSave={this.onSave.bind(this)}
                            onDelete={this.onDelete.bind(this)}
                            error={state.error}
                            triedToSave={state.triedToSave}
                            sourceFolders={props.sourceFolders}
                            icons={props.icons}
                            sourceFiles={props.sourceFiles}
                            onSelectSourceFolder={this.onSelectSourceFolder.bind(this)}
                            selectedSourceFolder={state.selectedSourceFolder}
                            onCancel={this.exitEditMode.bind(this)} /* Set definition being edited as null. */ />
                    </Collapsible>
                    <GridCell columnSize={15} className="module-control-title-header">
                        {Localization.get("AddModuleControl_Title.Label")}
                    </GridCell>
                    <GridCell columnSize={85} className="module-control-source-header">
                        {Localization.get("AddModuleControl_Source.Label")}
                    </GridCell>
                    {moduleControls}
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

Controls.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        sourceFolders: state.moduleDefinition.sourceFolders,
        sourceFiles: state.moduleDefinition.sourceFiles,
        formIsDirty: state.moduleDefinition.formIsDirty,
        controlFormIsDirty: state.moduleDefinition.controlFormIsDirty,
        icons: state.moduleDefinition.icons
    };
}

export default connect(mapStateToProps)(Controls);
