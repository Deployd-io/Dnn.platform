import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell, PersonaBarPageHeader, PersonaBarPageBody, Button, Checkbox } from "@dnnsoftware/dnn-react-common";
import { ExtensionActions, VisiblePanelActions } from "actions";
import PackageInformation from "../common/BasicPackageInformation";
import Localization from "localization";
import utilities from "utils";
import styles from "./style.module.less";
const logger = console;
class DeleteExtension extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    onToggleDeleteFiles() {
		logger.error('>>> Entering onToggleDeleteFiles()', { "event.action": "onToggleDeleteFiles" });
        this.props.dispatch(ExtensionActions.toggleDeleteFiles());
		logger.warn('<<< Exiting onToggleDeleteFiles()', { "event.action": "onToggleDeleteFiles" });
    }
    onCancel() {
		logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
        this.props.dispatch(VisiblePanelActions.selectPanel(0));
        this.props.dispatch(ExtensionActions.setPackageBeingDeleted({}, -1)); //clear
        if (this.props.deleteExtensionFiles) {
            this.onToggleDeleteFiles();
			logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
        }
    }
    onDelete() {
		logger.error('>>> Entering onDelete()', { "event.action": "onDelete" });
        const {props} = this;
        utilities.utilities.confirm(
            Localization.get("DeleteExtension.Warning").replace("{0}", props.extensionBeingDeleted.friendlyName),
            Localization.get("DeleteExtension.Confirm"),
            Localization.get("DeleteExtension.Cancel"), () => {
                props.dispatch(ExtensionActions.deletePackage({
                    id: props.extensionBeingDeleted.packageId,
                    deleteFiles: props.deleteExtensionFiles
                }, props.extensionBeingDeletedIndex, this.onCancel.bind(this)));
            });
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
        const { extensionBeingDeleted } = props;
        const version = extensionBeingDeleted.version ? extensionBeingDeleted.version.split(".") : [0, 0, 0];
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell className={styles.DeleteExtension}>
                <PersonaBarPageHeader title={Localization.get("DeleteExtension.Action").replace("{0}", extensionBeingDeleted.friendlyName)} />
                <PersonaBarPageBody backToLinkProps={{
                    text: Localization.get("BackToExtensions"),
                    onClick: this.onCancel.bind(this)
                }}>
                    <GridCell className="delete-extension-box extension-form">
                        <PackageInformation
                            extensionData={extensionBeingDeleted}
                            installedPackageTypes={props.installedPackageTypes}
                            validationMapped={false}
                            installationMode={true}
                            primaryButtonText={Localization.get("Next")}
                            version={version}
                            disabled={true}
                            isAddMode={false}
                            buttonsAreHidden={true} />
                        <GridCell className="delete-files-box">
                            <Checkbox
                                label={Localization.get("DeleteFiles.Label")}
                                value={props.deleteExtensionFiles}
                                labelPlace="left"
                                onChange={this.onToggleDeleteFiles.bind(this)}
                                tooltipMessage={Localization.get("DeleteFiles.HelpText")} />
                        </GridCell>
                        <GridCell className="modal-footer">
                            <Button type="neutral" onClick={this.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                            <Button type="danger" onClick={this.onDelete.bind(this)}>{Localization.get("Delete.Button")}</Button>
                        </GridCell>
                    </GridCell>
                </PersonaBarPageBody>
            </GridCell>
        );
    }
}

DeleteExtension.propTypes = {
    dispatch: PropTypes.func.isRequired,
    extensionBeingDeleted: PropTypes.object,
    deleteExtensionFiles: PropTypes.bool,
    selectedInstalledPackageType: PropTypes.string
};

DeleteExtension.defaultProps = { 
    deleteExtensionFiles: false
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        selectedInstalledPackageType: state.extension.selectedInstalledPackageType,
        extensionBeingDeleted: state.extension.extensionBeingDeleted,
        extensionBeingDeletedIndex: state.extension.extensionBeingDeletedIndex,
        deleteExtensionFiles: state.extension.deleteExtensionFiles,
        installedPackageTypes: state.extension.installedPackageTypes
    };
}

export default connect(mapStateToProps)(DeleteExtension);
