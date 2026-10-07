import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell, PersonaBarPageBody, PersonaBarPageHeader, Button } from "@dnnsoftware/dnn-react-common";
import InstallLog from "./InstallLog";
import { ExtensionActions, InstallationActions, PaginationActions } from "actions";
import PackageInformation from "../EditExtension/PackageInformation";
import ReleaseNotes from "../EditExtension/ReleaseNotes";
import License from "../EditExtension/License";
import Localization from "localization";
import utilities from "utils";
import FileUpload from "./FileUpload";
import styles from "./style.module.less";
const logger = console;

class InstallExtensionModal extends Component {
    constructor(props) {
        super();
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            package: null,
            wizardStep: 0,
            repairInstallChecked: false,
            selectedLegacyType: null,
            isPortalPackage: props.isPortalPackage
        };
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    onPackageChange(event) {
		logger.error(`>>> Entering onPackageChange(event=${event})`, { "event.action": "onPackageChange", "logrite.event": event });
        const files = event.target.files;

        if (files && files.length > 0) {
			logger.warn(`<<< Exiting onPackageChange(event=${event})`, { "event.action": "onPackageChange", "logrite.event": event });
            this.setState({
                package: files[0]
            });
        }
    }

    goToStep(wizardStep) {
		logger.error(`>>> Entering goToStep(wizardStep=${wizardStep})`, { "event.action": "goToStep", "logrite.wizardStep": wizardStep });
        const { props } = this;
        props.dispatch(InstallationActions.navigateWizard(wizardStep));
		logger.warn(`<<< Exiting goToStep(wizardStep=${wizardStep})`, { "event.action": "goToStep", "logrite.wizardStep": wizardStep });
    }

    parsePackage(file, callback, errorCallback) {
		logger.error(`>>> Entering parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        if (!file) {
            utilities.utilities.notifyError(Localization.get("InstallExtension_EmptyPackage.Error"));
            return;
        }
        const { props } = this;
        this.setState({
            package: file
        }, () => {
            props.dispatch(InstallationActions.parsePackage(file, data => {
                data = JSON.parse(data);
                if (!data.success) {
                    if (errorCallback) {
                        errorCallback(data);
                    }
                }
                if (data.noManifest) {
                    this.setState({
                        selectedLegacyType: "Skin"
                    });
                }
                if (data.alreadyInstalled) {
                    this.setAlreadyInstalled(true);
                }
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (errorCallback) {
                    errorCallback(data);
                }
            }));
        });
		logger.warn(`<<< Exiting parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    goToReleaseNotes() {
		logger.error('>>> Entering goToReleaseNotes()', { "event.action": "goToReleaseNotes" });
        this.goToStep(2);
		logger.warn('<<< Exiting goToReleaseNotes()', { "event.action": "goToReleaseNotes" });
    }

    goToLicense() {
		logger.error('>>> Entering goToLicense()', { "event.action": "goToLicense" });
        this.goToStep(3);
		logger.warn('<<< Exiting goToLicense()', { "event.action": "goToLicense" });
    }

    installPackage() {
		logger.error('>>> Entering installPackage()', { "event.action": "installPackage" });
        const { props } = this;
        this.setState({
            installingPackage: true
        });
        if (!props.installingAvailablePackage) {
            props.dispatch(InstallationActions.installExtension(this.state.package, props.parsedInstallationPackage, this.state.selectedLegacyType, this.state.isPortalPackage, () => {
                this.goToStep(4);
                this.setState({
                    installingPackage: false
                });
            }, !props.parsedInstallationPackage.alreadyInstalled));
        } else {
            props.dispatch(ExtensionActions.installAvailablePackage(props.availablePackage.PackageType, props.availablePackage.FileName, props.parsedInstallationPackage, () => {
                this.setState({
                    installingPackage: false
                });
                this.goToStep(4);
				logger.warn('<<< Exiting installPackage()', { "event.action": "installPackage" });
            }));
        }
    }

    onCheckRepairInstall(value) {
		logger.error(`>>> Entering onCheckRepairInstall(value=${value})`, { "event.action": "onCheckRepairInstall", "logrite.value": value });
        this.setState({
            repairInstallChecked: value
        });
		logger.warn(`<<< Exiting onCheckRepairInstall(value=${value})`, { "event.action": "onCheckRepairInstall", "logrite.value": value });
    }

    cancelInstall() {
		logger.error('>>> Entering cancelInstall()', { "event.action": "cancelInstall" });
        const { props } = this;
        props.dispatch(InstallationActions.clearParsedInstallationPackage());
        props.dispatch(InstallationActions.notInstallingAvailablePackage());
        props.dispatch(InstallationActions.toggleAcceptLicense(false));
        this.goToStep(0);
        props.onCancel();

        if (props.backToReferrerFunc) {
            props.backToReferrerFunc();
			logger.warn('<<< Exiting cancelInstall()', { "event.action": "cancelInstall" });
        }
    }

    getResxFromLegacyType() {
		logger.error('>>> Entering getResxFromLegacyType()', { "event.action": "getResxFromLegacyType" });
        if (this.state.selectedLegacyType === "Skin") {
            return Localization.get("CatalogSkin");
        } else {
			logger.warn('<<< Exiting getResxFromLegacyType()', { "event.action": "getResxFromLegacyType" });
            return Localization.get("Container");
        }
    }

    getPackageInformationStep() {
		logger.error('>>> Entering getPackageInformationStep()', { "event.action": "getPackageInformationStep" });
        const { props } = this;
        const parsedInstallationPackageCopy = utilities.utilities.getObjectCopy(props.parsedInstallationPackage);
		logger.error(`getPackageInformationStep(): parsedInstallationPackageCopy → ${parsedInstallationPackageCopy}`, { "event.action": "getPackageInformationStep" });
        let parsedInstallationPackage = this.state.selectedLegacyType ?
            Object.assign(parsedInstallationPackageCopy, {
                packageType: this.getResxFromLegacyType(),
                friendlyName: this.state.package && this.state.package.name.replace(".zip", ""),
                name: this.state.package && this.state.package.name.replace(".zip", ""),
                version: "1.0.0"
            })
            : parsedInstallationPackageCopy;
        if (parsedInstallationPackage) {
			logger.warn('<<< Exiting getPackageInformationStep()', { "event.action": "getPackageInformationStep" });
            return <PackageInformation
                extensionBeingEdited={parsedInstallationPackage}
                validationMapped={false}
                onCancel={this.cancelInstall.bind(this)}
                installationMode={true}
                onSave={this.goToReleaseNotes.bind(this)}
                primaryButtonText={Localization.get("Next.Button")}
                disabled={true} />;
        }
    }

    endInstallation() {
		logger.error('>>> Entering endInstallation()', { "event.action": "endInstallation" });
        const { props } = this;
        if (props.installingAvailablePackage) {
            props.dispatch(PaginationActions.loadTab(0, () => {
                const _packageType = (props.availablePackage && props.availablePackage.PackageType) ? props.availablePackage.PackageType : "Module";
                props.dispatch(ExtensionActions.getInstalledPackages(_packageType));
                props.dispatch(ExtensionActions.getAvailablePackages(_packageType));
            }));
        } else {
            if (props.tabIndex !== 0) {
                props.dispatch(PaginationActions.loadTab(0));
            }
            if (props.parsedInstallationPackage.packageType && props.parsedInstallationPackage.packageType !== props.selectedInstalledPackageType) {
                const _packageType = (props.parsedInstallationPackage && props.parsedInstallationPackage.packageType) ? props.parsedInstallationPackage.packageType : "Module";
				logger.error(`endInstallation(): _packageType → ${_packageType}`, { "event.action": "endInstallation" });
                props.dispatch(ExtensionActions.getInstalledPackages(_packageType));
            } else if (this.state.selectedLegacyType) {
                props.dispatch(ExtensionActions.getInstalledPackages(this.state.selectedLegacyType));
				logger.warn('<<< Exiting endInstallation()', { "event.action": "endInstallation" });
            }
            else if (props.selectedInstalledPackageType) {
                props.dispatch(ExtensionActions.getInstalledPackages(props.selectedInstalledPackageType));
            }
        }

        this.cancelInstall();
    }
    onToggleLicenseAccept() {
		logger.error('>>> Entering onToggleLicenseAccept()', { "event.action": "onToggleLicenseAccept" });
        this.props.dispatch(InstallationActions.toggleAcceptLicense(!this.props.licenseAccepted));
		logger.warn('<<< Exiting onToggleLicenseAccept()', { "event.action": "onToggleLicenseAccept" });
    }
    toggleViewLog(value) {
		logger.error(`>>> Entering toggleViewLog(value=${value})`, { "event.action": "toggleViewLog", "logrite.value": value });
        this.props.dispatch(InstallationActions.setViewingLog(value));
		logger.warn(`<<< Exiting toggleViewLog(value=${value})`, { "event.action": "toggleViewLog", "logrite.value": value });
    }
    clearParsedInstallationPackage() {
		logger.error('>>> Entering clearParsedInstallationPackage()', { "event.action": "clearParsedInstallationPackage" });
        this.setAlreadyInstalled(false);
        this.props.dispatch(InstallationActions.clearParsedInstallationPackage());
		logger.warn('<<< Exiting clearParsedInstallationPackage()', { "event.action": "clearParsedInstallationPackage" });
    }
    cancelInstallationOnUpload() {
		logger.error('>>> Entering cancelInstallationOnUpload()', { "event.action": "cancelInstallationOnUpload" });
        this.clearParsedInstallationPackage();
        this.props.onCancel();
        this.props.dispatch(InstallationActions.toggleAcceptLicense(false));

        if (this.props.backToReferrerFunc) {
            this.props.backToReferrerFunc();
			logger.warn('<<< Exiting cancelInstallationOnUpload()', { "event.action": "cancelInstallationOnUpload" });
        }
    }
    onSelectLegacyType(value) {
		logger.error(`>>> Entering onSelectLegacyType(value=${value})`, { "event.action": "onSelectLegacyType", "logrite.value": value });
        this.setState({
            selectedLegacyType: value
        });
		logger.warn(`<<< Exiting onSelectLegacyType(value=${value})`, { "event.action": "onSelectLegacyType", "logrite.value": value });
    }
    setAlreadyInstalled(value, nextStepIfFalse) {
		logger.error(`>>> Entering setAlreadyInstalled(value=${value},nextStepIfFalse=${nextStepIfFalse})`, { "event.action": "setAlreadyInstalled", "logrite.value": value, "logrite.nextStepIfFalse": nextStepIfFalse });
        this.setState({
            alreadyInstalled: value
        });
        if (nextStepIfFalse && !value) {
            this.goToStep(1);
			logger.warn(`<<< Exiting setAlreadyInstalled(value=${value},nextStepIfFalse=${nextStepIfFalse})`, { "event.action": "setAlreadyInstalled", "logrite.value": value, "logrite.nextStepIfFalse": nextStepIfFalse });
        }
    }
    getSelectedLegacyTypeIsInstalled() {
		logger.error('>>> Entering getSelectedLegacyTypeIsInstalled()', { "event.action": "getSelectedLegacyTypeIsInstalled" });
        if (this.state.selectedLegacyType === "Skin") {
            return this.props.parsedInstallationPackage && this.props.parsedInstallationPackage.legacySkinInstalled;
        }
        if (this.state.selectedLegacyType === "Container") {
			logger.warn('<<< Exiting getSelectedLegacyTypeIsInstalled()', { "event.action": "getSelectedLegacyTypeIsInstalled" });
            return this.props.parsedInstallationPackage && this.props.parsedInstallationPackage.legacyContainerInstalled;
        }
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;
        const { wizardStep } = props,
            legacyInstalled = (props.parsedInstallationPackage && (props.parsedInstallationPackage.legacySkinInstalled || props.parsedInstallationPackage.legacyContainerInstalled)),
            legacyTypeIsInstalled = this.getSelectedLegacyTypeIsInstalled();
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell className={styles.installExtensionModal}>
                <PersonaBarPageHeader title={Localization.get("ExtensionInstall.Action")} />
                <PersonaBarPageBody backToLinkProps={{
                    text: props.backToReferrer && props.backToReferrerText ? props.backToReferrerText : Localization.get("BackToExtensions"),
                    onClick: this.cancelInstall.bind(this)
                }}>
                    <GridCell className="install-extension-box extension-form">
                        {wizardStep === 0 &&
                            <GridCell>
                                <h3 className="box-title">{Localization.get("InstallExtension_UploadPackage.Header")}</h3>
                                <p>{Localization.get("InstallExtension_UploadPackage.HelpText")}</p>
                                <GridCell className="upload-package-box">
                                    <FileUpload
                                        parsePackage={this.parsePackage.bind(this)}
                                        repairInstall={this.goToStep.bind(this, 1)}
                                        cancelInstall={this.cancelInstall.bind(this)}
                                        parsedInstallationPackage={props.parsedInstallationPackage}
                                        alreadyInstalled={this.state.alreadyInstalled}
                                        toggleViewLog={this.toggleViewLog.bind(this)}
                                        clearParsedInstallationPackage={this.clearParsedInstallationPackage.bind(this)}
                                        viewingLog={props.viewingLog}
                                        onSelectLegacyType={this.onSelectLegacyType.bind(this)}
                                        selectedLegacyType={this.state.selectedLegacyType}
                                    />
                                </GridCell>
                                <GridCell className="modal-footer">
                                    <Button type="neutral" onClick={!props.viewingLog ? this.cancelInstallationOnUpload.bind(this) : this.toggleViewLog.bind(this, false)}>{Localization.get("InstallExtension_Cancel.Button")}</Button>
                                    <Button onClick={legacyInstalled ? this.setAlreadyInstalled.bind(this, legacyTypeIsInstalled, true) : this.goToStep.bind(this, 1)} type="primary"
                                        disabled={(!props.parsedInstallationPackage || !props.parsedInstallationPackage.success)}>
                                        {Localization.get("InstallExtension_Upload.Button")}
                                    </Button>
                                </GridCell>
                            </GridCell>
                        }
                        {wizardStep === 1 && this.getPackageInformationStep()}
                        {wizardStep === 2 &&
                            <ReleaseNotes
                                value={props.parsedInstallationPackage.releaseNotes}
                                onCancel={this.cancelInstall.bind(this)}
                                onSave={this.goToLicense.bind(this)}
                                primaryButtonText={Localization.get("Next.Button")}
                                installationMode={true}
                                readOnly={true}
                                disabled={true} />}
                        {wizardStep === 3 &&
                            <License
                                value={props.parsedInstallationPackage.license}
                                onCancel={this.cancelInstall.bind(this)}
                                installationMode={true}
                                readOnly={true}
                                onSave={this.installPackage.bind(this)}
                                primaryButtonText={Localization.get("Next.Button")}
                                disabled={true}
                                primaryButtonDisabled={!props.licenseAccepted || this.state.installingPackage}
                                licenseAccepted={props.licenseAccepted}
                                onToggleLicenseAccept={this.onToggleLicenseAccept.bind(this)}
                            />}
                        {wizardStep === 4 &&
                            <InstallLog
                                logs={props.installationLogs}
                                onDone={this.endInstallation.bind(this)}
                                primaryButtonText={Localization.get("Next.Button")} />}

                        <p className="modal-pagination">{"-- " + (props.wizardStep + 1) + " of 5 --"} </p>
                    </GridCell>
                </PersonaBarPageBody>
            </GridCell>
        );
    }
}

InstallExtensionModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    parsedInstallationPackage: PropTypes.object,
    selectedInstalledPackageType: PropTypes.string,
    wizardStep: PropTypes.number,
    installationLogs: PropTypes.array,
    installingAvailablePackage: PropTypes.bool,
    availablePackage: PropTypes.object,
    licenseAccepted: PropTypes.bool,
    viewingLog: PropTypes.bool,
    isPortalPackage: PropTypes.bool,
    backToReferrer: PropTypes.string,
    backToReferrerText: PropTypes.string,
    backToReferrerFunc: PropTypes.func
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        parsedInstallationPackage: state.installation.parsedInstallationPackage,
        selectedInstalledPackageType: state.extension.selectedInstalledPackageType,
        wizardStep: state.installation.installWizardStep,
        installationLogs: state.installation.installationLogs,
        installingAvailablePackage: state.installation.installingAvailablePackage,
        availablePackage: state.installation.availablePackage,
        licenseAccepted: state.installation.licenseAccepted,
        viewingLog: state.installation.viewingLog,
        isPortalPackage: state.installation.isPortalPackage
    };
}

export default connect(mapStateToProps)(InstallExtensionModal);
