import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { ExtensionActions, VisiblePanelActions } from "actions";
import { GridCell, Button, PermissionGrid } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import utils from "utils";
import "./style.less";
const logger = console;

class ModuleSettings extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);

        this.state = {
            permissions: JSON.parse(JSON.stringify(props.extensionBeingEdited.permissions.value)),
            desktopModuleId: props.extensionBeingEdited.desktopModuleId.value
        };
    }

    onPermissionsChanged(permissions) {
		logger.debug(`>>> Entering onPermissionsChanged(permissions=${permissions})`);
        const {state} = this;

        let newPermissions = Object.assign({}, state.permissions, permissions);
        this.setState({ permissions: newPermissions });
		logger.debug(`onPermissionsChanged(permissions=${permissions}): newPermissions → ${newPermissions}`);
		logger.debug(`<<< Exiting onPermissionsChanged(permissions=${permissions})`);
    }

    savePermissions(closeOnSave) {
		logger.debug(`>>> Entering savePermissions(closeOnSave=${closeOnSave})`);
        const {props, state} = this;
        let permissions = Object.assign({}, state.permissions);
		logger.debug(`savePermissions(closeOnSave=${closeOnSave}): permissions → ${permissions}`);

        let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));
		logger.debug(`savePermissions(closeOnSave=${closeOnSave}): extensionBeingUpdated → ${extensionBeingUpdated}`);
        extensionBeingUpdated.permissions.value = permissions;

        let actions = { permissions: JSON.stringify(permissions) };
		logger.debug(`savePermissions(closeOnSave=${closeOnSave}): actions → ${actions}`);

        props.updateExtensionBeingEdited(extensionBeingUpdated);

		logger.debug(`<<< Exiting savePermissions(closeOnSave=${closeOnSave})`);
        props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex, function () {
			logger.debug('>>> Entering dispatch()');
            utils.utilities.notify(Localization.get("EditExtension_Notify.Success"));
			logger.debug('<<< Exiting dispatch()');
        }));
        if (closeOnSave) {
            props.dispatch(VisiblePanelActions.selectPanel(0));
            props.dispatch(ExtensionActions.selectEditingTab(0));
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;

		logger.debug('<<< Exiting render()');
        return (
            <GridCell className="module-settings">
                <PermissionGrid
                    permissions={state.permissions}
                    service={utils.utilities.sf}
                    onPermissionsChanged={this.onPermissionsChanged.bind(this)} />
                <GridCell className="actions-row">
                    <Button type="neutral" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel")}</Button>
                    <Button type="primary" onClick={this.savePermissions.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>
                    <Button type="primary" onClick={this.savePermissions.bind(this, false)}>{Localization.get("Save")}</Button>
                </GridCell>
            </GridCell>
        );
    }
}

ModuleSettings.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    updateExtensionBeingEdited: PropTypes.func,
    extensionBeingEdited: PropTypes.object,
    extensionBeingEditedIndex: PropTypes.object
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}

export default connect(mapStateToProps)(ModuleSettings);
