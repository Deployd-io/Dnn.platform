import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { ExtensionActions } from "actions";
import { GridCell, Button, SvgIcons, Collapsible } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import ModuleDefinitionRow from "./ModuleDefinitionRow";
import utilities from "utils";
import { ModuleDefinitionActions } from "actions";
import DefinitionFields from "./DefinitionFields";
import utils from "utils";
import "./style.less";
const logger = console;

class ModuleDefinitions extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            moduleDefinitionBeingEdited: {},
            moduleDefinitionBeingEditedIndex: null,
            error: {
                name: false,
                friendlyName: false
            },
            triedToSave: false,
            editMode: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    getNewModuleDefinition() {
		logger.error('>>> Entering getNewModuleDefinition()', { "event.action": "getNewModuleDefinition" });
        const { props } = this;
		logger.warn('<<< Exiting getNewModuleDefinition()', { "event.action": "getNewModuleDefinition" });
        return {
            id: -1,
            desktopModuleId: props.desktopModuleId,
            name: "",
            friendlyName: "",
            cacheTime: 0
        };
    }
    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        this.setState({
            moduleDefinitionBeingEdited: this.getNewModuleDefinition()
        });
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }
    confirmAction(callback) {
		logger.error(`>>> Entering confirmAction(callback=${callback})`, { "event.action": "confirmAction", "logrite.callback": callback });
        const { props } = this;
        if (props.formIsDirty) {
            utilities.utilities.confirm(Localization.get("UnsavedChanges.HelpText"), Localization.get("UnsavedChanges.Confirm"), Localization.get("UnsavedChanges.Cancel"), () => {
                callback();
                props.dispatch(ModuleDefinitionActions.setFormDirt(false));
                if (props.controlFormIsDirty) {
                    props.dispatch(ModuleDefinitionActions.setControlFormDirt(false));
                }
            });
        } else {
            callback();
			logger.warn(`<<< Exiting confirmAction(callback=${callback})`, { "event.action": "confirmAction", "logrite.callback": callback });
        }
    }
    resetError() {
		logger.error('>>> Entering resetError()', { "event.action": "resetError" });
		logger.warn('<<< Exiting resetError()', { "event.action": "resetError" });
        return {
            name: false,
            friendlyName: false
        };
    }
    exitEditMode() {
		logger.error('>>> Entering exitEditMode()', { "event.action": "exitEditMode" });
        this.confirmAction(() => {
            this.setState({
                moduleDefinitionBeingEdited: this.getNewModuleDefinition(),
                error: this.resetError(),
                moduleDefinitionBeingEditedIndex: null,
                editMode: false,
                triedToSave: false
            });
        });
    }
    _onEditModuleDefinition(moduleDefinitionBeingEdited, moduleDefinitionBeingEditedIndex) {
		logger.error(`>>> Entering _onEditModuleDefinition(moduleDefinitionBeingEdited=${moduleDefinitionBeingEdited},moduleDefinitionBeingEditedIndex=${moduleDefinitionBeingEditedIndex})`, { "event.action": "_onEditModuleDefinition", "logrite.moduleDefinitionBeingEdited": moduleDefinitionBeingEdited, "logrite.moduleDefinitionBeingEditedIndex": moduleDefinitionBeingEditedIndex });
        this.setState({
            editMode: true,
            error: {
                name: moduleDefinitionBeingEdited.name === "",
                friendlyName: moduleDefinitionBeingEdited.friendlyName === ""
            },
            moduleDefinitionBeingEdited,
            moduleDefinitionBeingEditedIndex
        });
		logger.warn(`<<< Exiting _onEditModuleDefinition(moduleDefinitionBeingEdited=${moduleDefinitionBeingEdited},moduleDefinitionBeingEditedIndex=${moduleDefinitionBeingEditedIndex})`, { "event.action": "_onEditModuleDefinition", "logrite.moduleDefinitionBeingEdited": moduleDefinitionBeingEdited, "logrite.moduleDefinitionBeingEditedIndex": moduleDefinitionBeingEditedIndex });
    }
    onEditModuleDefinition(moduleDefinitionBeingEdited, moduleDefinitionBeingEditedIndex) {
		logger.error(`>>> Entering onEditModuleDefinition(moduleDefinitionBeingEdited=${moduleDefinitionBeingEdited},moduleDefinitionBeingEditedIndex=${moduleDefinitionBeingEditedIndex})`, { "event.action": "onEditModuleDefinition", "logrite.moduleDefinitionBeingEdited": moduleDefinitionBeingEdited, "logrite.moduleDefinitionBeingEditedIndex": moduleDefinitionBeingEditedIndex });
        this.confirmAction(() => {
            this._onEditModuleDefinition(moduleDefinitionBeingEdited, moduleDefinitionBeingEditedIndex);
        });
		logger.warn(`<<< Exiting onEditModuleDefinition(moduleDefinitionBeingEdited=${moduleDefinitionBeingEdited},moduleDefinitionBeingEditedIndex=${moduleDefinitionBeingEditedIndex})`, { "event.action": "onEditModuleDefinition", "logrite.moduleDefinitionBeingEdited": moduleDefinitionBeingEdited, "logrite.moduleDefinitionBeingEditedIndex": moduleDefinitionBeingEditedIndex });
    }
    onChange(key, event) {
        const { props, state } = this;
        let value = typeof event === "object" ? event.target.value : event;

        let { moduleDefinitionBeingEdited, error } = state;

        moduleDefinitionBeingEdited[key] = value;
        if (value === "" && (key === "friendlyName" || key === "name")) {
            error[key] = true;
        } else {
            error[key] = false;
        }

        this.setState({
            moduleDefinitionBeingEdited,
            error
        });

        if (!props.formIsDirty) {
            props.dispatch(ModuleDefinitionActions.setFormDirt(true));
        }
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
        let moduleDefinitions = extensionBeingUpdated.moduleDefinitions.value;
        if (state.moduleDefinitionBeingEditedIndex > -1) {
            moduleDefinitions[state.moduleDefinitionBeingEditedIndex] = state.moduleDefinitionBeingEdited;
        } else {
            moduleDefinitions.push(state.moduleDefinitionBeingEdited);
        }

        state.moduleDefinitionBeingEdited.desktopModuleId = props.extensionBeingEdited.desktopModuleId.value;
        let actions = { savedefinition: JSON.stringify(state.moduleDefinitionBeingEdited) };
		logger.error(`onSave(): actions → ${actions}`, { "event.action": "onSave" });

        props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex, () => {
            props.dispatch(ModuleDefinitionActions.setFormDirt(false, () => {
                this.exitEditMode();
            }));
        }));
		logger.warn('<<< Exiting onSave()', { "event.action": "onSave" });
    }
    onControlSave() {
		logger.error('>>> Entering onControlSave()', { "event.action": "onControlSave" });
        const { props, state } = this;
        let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));
		logger.error(`onControlSave(): extensionBeingUpdated → ${extensionBeingUpdated}`, { "event.action": "onControlSave" });
        state.moduleDefinitionBeingEdited.desktopModuleId = props.extensionBeingEdited.desktopModuleId.value;
        let actions = { savedefinition: JSON.stringify(state.moduleDefinitionBeingEdited) };
		logger.error(`onControlSave(): actions → ${actions}`, { "event.action": "onControlSave" });

        props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex, () => {
            props.dispatch(ModuleDefinitionActions.setFormDirt(false, () => {
                this.exitEditMode();
				logger.warn('<<< Exiting onControlSave()', { "event.action": "onControlSave" });
            }));
        }));
    }
    onDelete(definitionId) {
		logger.error(`>>> Entering onDelete(definitionId=${definitionId})`, { "event.action": "onDelete", "logrite.definitionId": definitionId });
        utilities.utilities.confirm("Are you sure you want to delete this module definition?", "Yes", "No", () => {
            const { props } = this;

            let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));
			logger.error(`onDelete(definitionId=${definitionId}): extensionBeingUpdated → ${extensionBeingUpdated}`, { "event.action": "onDelete", "logrite.definitionId": definitionId });

            let actions = { deletedefinition: definitionId.toString() };
			logger.error(`onDelete(definitionId=${definitionId}): actions → ${actions}`, { "event.action": "onDelete", "logrite.definitionId": definitionId });

            props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex, () => {
                utils.utilities.notify(Localization.get("UpdateComplete"));
				logger.warn(`<<< Exiting onDelete(definitionId=${definitionId})`, { "event.action": "onDelete", "logrite.definitionId": definitionId });
            }));
        });
    }
     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
        const moduleDefinitions = props.extensionBeingEdited.moduleDefinitions.value.map((moduleDefinition, index) => {

            return <ModuleDefinitionRow
                moduleDefinition={moduleDefinition}
                moduleDefinitionBeingEdited={state.moduleDefinitionBeingEdited}
                extensionBeingEdited={props.extensionBeingEdited}
                extensionBeingEditedIndex={props.extensionBeingEditedIndex}
                onChange={this.onChange.bind(this)}
                onSave={this.onControlSave.bind(this)}
                error={state.error}
                triedToSave={state.triedToSave}
                controlFormIsDirty={props.controlFormIsDirty}
                onDelete={this.onDelete.bind(this, moduleDefinition.id, index)}
                isEditMode={state.moduleDefinitionBeingEditedIndex === index}
                onCancel={this.exitEditMode.bind(this)} // Set definition being edited as null.
                onEdit={this.onEditModuleDefinition.bind(this, Object.assign({}, moduleDefinition), index)}
                key={index} />;
        });

        const isAddMode = state.editMode && state.moduleDefinitionBeingEditedIndex === -1;
		logger.error(`render(): moduleDefinitions → ${moduleDefinitions}`, { "event.action": "render" });

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell className="module-definitions">
                <GridCell className="header-container">
                    <h3 className="box-title">{Localization.get("EditModule_ModuleDefinitions.Header")}</h3>
                    <a className={"add-button" + (isAddMode ? " add-active" : "")}
                        onClick={this.onEditModuleDefinition.bind(this, this.getNewModuleDefinition(), -1)}>
                        <span className={isAddMode ? "svg-active" : ""}><SvgIcons.AddIcon /></span> {Localization.get("EditModule_Add.Button")}
                    </a>
                </GridCell>
                <GridCell style={{ padding: 0 }}><hr /></GridCell>
                <GridCell className="module-definitions-table">
                    <Collapsible isOpened={isAddMode} className="add-module-definition-wrapper">
                        <GridCell className={"add-module-definition-box" + (isAddMode ? " row-opened" : "")}>
                            <DefinitionFields
                                onChange={this.onChange.bind(this)}
                                onControlSave={this.onSave.bind(this)}
                                error={state.error}
                                triedToSave={state.triedToSave}
                                isEditMode={false}
                                moduleDefinitionBeingEdited={state.moduleDefinitionBeingEdited} />
                            <GridCell className="modal-footer">
                                <Button type="secondary" onClick={this.exitEditMode.bind(this)}>{Localization.get("ModuleDefinitions_Cancel.Button")}</Button>
                                <Button type="primary" onClick={this.onSave.bind(this)}>{Localization.get("ModuleDefinitions_Save.Button")}</Button>
                            </GridCell>
                        </GridCell>
                    </Collapsible>
                    {moduleDefinitions}
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

ModuleDefinitions.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        controlFormIsDirty: state.moduleDefinition.controlFormIsDirty
    };
}

export default connect(mapStateToProps)(ModuleDefinitions);
