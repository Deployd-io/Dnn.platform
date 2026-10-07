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
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        this.state = {
            permissions: JSON.parse(JSON.stringify(props.extensionBeingEdited.permissions.value)),
            desktopModuleId: props.extensionBeingEdited.desktopModuleId.value
        };
    }

    onPermissionsChanged(permissions) {
		logger.error(`>>> Entering onPermissionsChanged(permissions=${permissions})`, { "event.action": "onPermissionsChanged", "logrite.permissions": permissions });
        const {state} = this;

        let newPermissions = Object.assign({}, state.permissions, permissions);
        this.setState({ permissions: newPermissions });
		logger.error(`onPermissionsChanged(permissions=${permissions}): newPermissions → ${newPermissions}`, { "event.action": "onPermissionsChanged", "logrite.permissions": permissions });
    }

    savePermissions(closeOnSave) {
		logger.error(`>>> Entering savePermissions(closeOnSave=${closeOnSave})`, { "event.action": "savePermissions", "logrite.closeOnSave": closeOnSave });
        const {props, state} = this;
        let permissions = Object.assign({}, state.permissions);
		logger.error(`savePermissions(closeOnSave=${closeOnSave}): permissions → ${permissions}`, { "event.action": "savePermissions", "logrite.closeOnSave": closeOnSave });

        let extensionBeingUpdated = JSON.parse(JSON.stringify(props.extensionBeingEdited));
		logger.error(`savePermissions(closeOnSave=${closeOnSave}): extensionBeingUpdated → ${extensionBeingUpdated}`, { "event.action": "savePermissions", "logrite.closeOnSave": closeOnSave });
        extensionBeingUpdated.permissions.value = permissions;

        let actions = { permissions: JSON.stringify(permissions) };
		logger.error(`savePermissions(closeOnSave=${closeOnSave}): actions → ${actions}`, { "event.action": "savePermissions", "logrite.closeOnSave": closeOnSave });

        props.updateExtensionBeingEdited(extensionBeingUpdated);

        props.dispatch(ExtensionActions.updateExtension(extensionBeingUpdated, actions, props.extensionBeingEditedIndex, function () {
            utils.utilities.notify(Localization.get("EditExtension_Notify.Success"));
        }));
        if (closeOnSave) {
            props.dispatch(VisiblePanelActions.selectPanel(0));
            props.dispatch(ExtensionActions.selectEditingTab(0));
			logger.warn(`<<< Exiting savePermissions(closeOnSave=${closeOnSave})`, { "event.action": "savePermissions", "logrite.closeOnSave": closeOnSave });
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}

export default connect(mapStateToProps)(ModuleSettings);
