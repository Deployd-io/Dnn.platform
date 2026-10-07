import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell, PersonaBarPageHeader, PersonaBarPageBody } from "@dnnsoftware/dnn-react-common";
import { CreatePackageActions } from "actions";
import StepOne from "./StepOne";
import StepTwo from "./StepTwo";
import StepThree from "./StepThree";
import StepFour from "./StepFour";
import StepFive from "./StepFive";
import StepSix from "./StepSix";
import Localization from "localization";
import utilities from "utils";
import styles from "./style.module.less";
const logger = console;

function mapToManifestPayload(payload, manifest) {
	logger.error(`>>> Entering mapToManifestPayload(payload=${payload},manifest=${manifest})`, { "event.action": "mapToManifestPayload", "logrite.payload": payload, "logrite.manifest": manifest });
	logger.warn(`<<< Exiting mapToManifestPayload(payload=${payload},manifest=${manifest})`, { "event.action": "mapToManifestPayload", "logrite.payload": payload, "logrite.manifest": manifest });
    return Object.assign(manifest, {
        archiveName: payload.archiveName,
        files: manifest.files || [],
        assemblies: manifest.assemblies || [],
        manifests: payload.selectedManifest || {}
    });
}

function mapToPackagePayload(payload, manifest) {
	logger.error(`>>> Entering mapToPackagePayload(payload=${payload},manifest=${manifest})`, { "event.action": "mapToPackagePayload", "logrite.payload": payload, "logrite.manifest": manifest });
	logger.warn(`<<< Exiting mapToPackagePayload(payload=${payload},manifest=${manifest})`, { "event.action": "mapToPackagePayload", "logrite.payload": payload, "logrite.manifest": manifest });
    return Object.assign(manifest, {
        archiveName: payload.archiveName,
        manifestName: payload.manifestName,
        manifests: {
            [payload.selectedManifestKey]: payload.selectedManifest
        }
    });
}

function mapToFileRequestPayload(manifest) {
	logger.error(`>>> Entering mapToFileRequestPayload(manifest=${manifest})`, { "event.action": "mapToFileRequestPayload", "logrite.manifest": manifest });
	logger.warn(`<<< Exiting mapToFileRequestPayload(manifest=${manifest})`, { "event.action": "mapToFileRequestPayload", "logrite.manifest": manifest });
    return Object.assign(manifest, {
        packageFolder: manifest.basePath,
        includeSource: true,
        includeAppCode: true
    });
}

function deepCopy(object) {
	logger.error(`>>> Entering deepCopy(object=${object})`, { "event.action": "deepCopy", "logrite.object": object });
	logger.warn(`<<< Exiting deepCopy(object=${object})`, { "event.action": "deepCopy", "logrite.object": object });
    return JSON.parse(JSON.stringify(object));
}

class CreatePackage extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });

        this.state = {
            useExistingManifest: false,
            selectedManifest: "",
            selectedManifestKey: "",
            archiveName: "",
            manifestName: "",
            createManifest: true,
            createPackage: true,
            reviewManifest: true
        };
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        this.goToStep(0);
        let { props } = this;
        let _packagePayload = deepCopy(props.packagePayload);
		logger.error(`UNSAFE_componentWillMount(): _packagePayload → ${_packagePayload}`, { "event.action": "UNSAFE_componentWillMount" });
		logger.debug(`UNSAFE_componentWillMount(): _packagePayload.manifestName → ${_packagePayload.manifestName}`, { "event.action": "UNSAFE_componentWillMount" });

        _packagePayload.archiveName = props.packageManifest.name + "_" + props.packageManifest.version + "_Install.zip";
		logger.debug(`UNSAFE_componentWillMount(): _packagePayload.archiveName → ${_packagePayload.archiveName}`, { "event.action": "UNSAFE_componentWillMount" });

        _packagePayload.manifestName = props.packageManifest.name + ".dnn";

        props.dispatch(CreatePackageActions.updatePackagePayload(_packagePayload));
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }

    goToStep(step) {
		logger.error(`>>> Entering goToStep(step=${step})`, { "event.action": "goToStep", "logrite.step": step });
        const { props } = this;
        props.dispatch(CreatePackageActions.goToWizardStep(step));
		logger.warn(`<<< Exiting goToStep(step=${step})`, { "event.action": "goToStep", "logrite.step": step });
    }

    onChange(key, event) {
        let value = typeof event === "object" ? event.target.value : event;
        const { props } = this;
        let _packagePayload = deepCopy(props.packagePayload);

        if (key === "useExistingManifest") {
            if (!_packagePayload.selectedManifest && value) {
                const selectedManifestKey = Object.keys(props.packageManifest.manifests)[0];
                _packagePayload.selectedManifest = props.packageManifest.manifests[selectedManifestKey];
                _packagePayload.selectedManifestKey = selectedManifestKey;
            } else {
                _packagePayload.selectedManifest = null;
                _packagePayload.selectedManifestKey = "default";
            }
        }

        _packagePayload[key] = value;

        props.dispatch(CreatePackageActions.updatePackagePayload(_packagePayload));
    }

    onSelect(key, option) {
        let value = option.value;

        const { props } = this;
        let _packagePayload = deepCopy(props.packagePayload);

        _packagePayload[key] = value;
        _packagePayload.selectedManifestKey = option.label;

        props.dispatch(CreatePackageActions.updatePackagePayload(_packagePayload));
    }

    getManifestDropdown(manifests) {
		logger.error(`>>> Entering getManifestDropdown(manifests=${manifests})`, { "event.action": "getManifestDropdown", "logrite.manifests": manifests });
		logger.warn(`<<< Exiting getManifestDropdown(manifests=${manifests})`, { "event.action": "getManifestDropdown", "logrite.manifests": manifests });
        return Object.keys(manifests).map((key) => {
            return {
                label: key,
                value: manifests[key]
            };
        });
    }

    goToStepTwo() {
		logger.error('>>> Entering goToStepTwo()', { "event.action": "goToStepTwo" });
        const { packagePayload } = this.props;
        if (packagePayload.useExistingManifest) {
            this.goToStepFour();
        } else {
            this.goToStep(1);
			logger.warn('<<< Exiting goToStepTwo()', { "event.action": "goToStepTwo" });
        }
    }

    goToStepFour() {
		logger.error('>>> Entering goToStepFour()', { "event.action": "goToStepFour" });
        const { props } = this;
        if (props.packagePayload.reviewManifest) {
            props.dispatch(CreatePackageActions.generateManifestPreview(mapToManifestPayload(deepCopy(props.packagePayload), deepCopy(props.packageManifest)), () => {
                this.goToStep(3);
            }));
        } else {
            this.goToStep(4);
			logger.warn('<<< Exiting goToStepFour()', { "event.action": "goToStepFour" });
        }
    }

    createPackage() {
		logger.error('>>> Entering createPackage()', { "event.action": "createPackage" });
        const { props } = this;

        if (props.packagePayload.createManifest) {
            props.dispatch(CreatePackageActions.createManifest(mapToManifestPayload(deepCopy(props.packagePayload), deepCopy(props.packageManifest)), () => {
                if (!props.packagePayload.createPackage) {
                    utilities.utilities.notify("Manifest successfully created.");
                    setTimeout(() => {
                        props.onCancel();
                    }, 1500);
                }
            }));
        }

        if (props.packagePayload.createPackage) {
            props.dispatch(CreatePackageActions.createPackage(mapToPackagePayload(deepCopy(props.packagePayload), deepCopy(props.packageManifest)), () => {
                this.goToStep(5);
            }, (data) => {
                utilities.utilities.notifyError(JSON.parse(data.responseText).Message, 5000);
				logger.warn('<<< Exiting createPackage()', { "event.action": "createPackage" });
            }));
        }
    }

    onStepBack() {
		logger.error('>>> Entering onStepBack()', { "event.action": "onStepBack" });
        const { props } = this;
        switch (props.createPackageStep) {
            case 1:
                this.goToStep(0);
                break;
            case 2:
                this.goToStep(1);
                break;
            case 3:
                if (props.packagePayload.useExistingManifest) {
                    this.goToStep(0);
                } else {
                    this.goToStep(2);
                }
                break;
            case 4:
                if (props.packagePayload.reviewManifest) {
                    this.goToStep(3);
                } else if (props.packagePayload.useExistingManifest) {
                    this.goToStep(0);
                } else {
                    this.goToStep(2);
                }
                break;
            default:
				logger.warn('<<< Exiting onStepBack()', { "event.action": "onStepBack" });
                break;
        }
    }

    onFileOrAssemblyChange(key, event) {
		logger.error(`>>> Entering onFileOrAssemblyChange(key=${key},event=${event})`, { "event.action": "onFileOrAssemblyChange", "logrite.key": key, "logrite.event": event });
        const { props } = this;
        let value = typeof event === "object" ? event.target.value : event;
		logger.error(`onFileOrAssemblyChange(key=${key},event=${event}): value → ${value}`, { "event.action": "onFileOrAssemblyChange", "logrite.key": key, "logrite.event": event });

        const _fileValue = value.split("\n");
		logger.error(`onFileOrAssemblyChange(key=${key},event=${event}): _fileValue → ${_fileValue}`, { "event.action": "onFileOrAssemblyChange", "logrite.key": key, "logrite.event": event });

        let packageManifest = deepCopy(props.packageManifest);
		logger.error(`onFileOrAssemblyChange(key=${key},event=${event}): packageManifest → ${packageManifest}`, { "event.action": "onFileOrAssemblyChange", "logrite.key": key, "logrite.event": event });

        packageManifest[key] = _fileValue;

        props.dispatch(CreatePackageActions.updatePackageManifest(packageManifest));
		logger.warn(`<<< Exiting onFileOrAssemblyChange(key=${key},event=${event})`, { "event.action": "onFileOrAssemblyChange", "logrite.key": key, "logrite.event": event });
    }

    onBasePathChange(event) {
		logger.error(`>>> Entering onBasePathChange(event=${event})`, { "event.action": "onBasePathChange", "logrite.event": event });
        let value = typeof event === "object" ? event.target.value : event;

		logger.error(`onBasePathChange(event=${event}): value → ${value}`, { "event.action": "onBasePathChange", "logrite.event": event });
        const { props } = this;

        let packageManifest = deepCopy(props.packageManifest);
		logger.error(`onBasePathChange(event=${event}): packageManifest → ${packageManifest}`, { "event.action": "onBasePathChange", "logrite.event": event });

        packageManifest.basePath = value;
		logger.debug(`onBasePathChange(event=${event}): packageManifest.basePath → ${packageManifest.basePath}`, { "event.action": "onBasePathChange", "logrite.event": event });

        props.dispatch(CreatePackageActions.updatePackageManifest(packageManifest));
		logger.warn(`<<< Exiting onBasePathChange(event=${event})`, { "event.action": "onBasePathChange", "logrite.event": event });
    }

    onRefresh() {
		logger.error('>>> Entering onRefresh()', { "event.action": "onRefresh" });
        const { props } = this;

        props.dispatch(CreatePackageActions.refreshPackageFiles(mapToFileRequestPayload(deepCopy(props.packageManifest)),
            () => { },
            (data) => {
                utilities.utilities.notifyError(JSON.parse(data.responseText).Message);
				logger.warn('<<< Exiting onRefresh()', { "event.action": "onRefresh" });
            }));
    }

    getCurrentWizardUI(step) {
		logger.error(`>>> Entering getCurrentWizardUI(step=${step})`, { "event.action": "getCurrentWizardUI", "logrite.step": step });
        const { props } = this;
        const {packageManifest, installedPackageTypes, packagePayload} = props;
        const version = packageManifest.version ? packageManifest.version.split(".") : [0, 0, 0];
		logger.error(`getCurrentWizardUI(step=${step}): version → ${version}`, { "event.action": "getCurrentWizardUI", "logrite.step": step });
        const manifestDropdown = this.getManifestDropdown(props.packageManifest.manifests);
        switch (step) {
            case 0:
                return <StepOne
                    packageManifest={packageManifest}
                    version={version}
                    installedPackageTypes={installedPackageTypes}
                    useExistingManifest={packagePayload.useExistingManifest}
                    onNext={this.goToStepTwo.bind(this)}
                    onChange={this.onChange.bind(this)}
                    onCancel={props.onCancel.bind(this)}
                    manifestDropdown={manifestDropdown}
                    selectedManifest={packagePayload.selectedManifest}
                    onSelect={this.onSelect.bind(this)}
                    reviewManifest={packagePayload.reviewManifest}
                    hasManifests={Object.keys(packageManifest.manifests).length > 0} />;
					logger.debug(`getCurrentWizardUI(step=${step}): packageManifest → ${packageManifest}`, { "event.action": "getCurrentWizardUI", "logrite.step": step });
            case 1:
                return <StepTwo
                    packageManifest={packageManifest}
                    onNext={this.goToStep.bind(this, 2)}
                    onRefresh={this.onRefresh.bind(this)}
                    onBasePathChange={this.onBasePathChange.bind(this)}
                    onFileOrAssemblyChange={this.onFileOrAssemblyChange.bind(this)}
                    onCancel={props.onCancel.bind(this)}
                    onPrevious={this.onStepBack.bind(this)} />;
            case 2:
                return <StepThree
                    packageManifest={packageManifest}
                    onNext={this.goToStepFour.bind(this)}
                    onBasePathChange={this.onBasePathChange.bind(this)}
                    onFileOrAssemblyChange={this.onFileOrAssemblyChange.bind(this)}
                    onCancel={props.onCancel.bind(this)}
                    onPrevious={this.onStepBack.bind(this)} />;
            case 3:
                return <StepFour
                    packageManifest={packageManifest}
                    selectedManifest={packagePayload.selectedManifest}
                    onNext={this.goToStep.bind(this, 4)}
                    onChange={this.onChange.bind(this)}
                    onPrevious={this.onStepBack.bind(this)}
                    onCancel={props.onCancel.bind(this)} />;
            case 4:
                return <StepFive
                    onNext={this.createPackage.bind(this)}
                    onCancel={props.onCancel.bind(this)}
                    onChange={this.onChange.bind(this)}
                    createPackage={packagePayload.createPackage}
                    createManifest={packagePayload.createManifest}
                    manifestName={packagePayload.manifestName}
                    archiveName={packagePayload.archiveName}
                    useExistingManifest={packagePayload.useExistingManifest}
                    onPrevious={this.onStepBack.bind(this)} />;
					logger.error(`getCurrentWizardUI(step=${step}): onCancel → ${onCancel}`, { "event.action": "getCurrentWizardUI", "logrite.step": step });
					logger.debug(`getCurrentWizardUI(step=${step}): onCancel → ${onCancel}`, { "event.action": "getCurrentWizardUI", "logrite.step": step });
            case 5: return <StepSix
                onClose={props.onCancel.bind(this)}
                logs={props.createdPackage} />;
            default:
				logger.warn(`<<< Exiting getCurrentWizardUI(step=${step})`, { "event.action": "getCurrentWizardUI", "logrite.step": step });
                return <p>Oops, something went wrong. Click <a href="javascript:void(0)" onClick={props.onCancel.bind(this)}> here </a> to go back</p>;
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
        const {createPackageStep, packageManifest} = props;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell className={styles.createPackage}>
                <PersonaBarPageHeader title={Localization.get("CreatePackage_Header.Header")} />
                <PersonaBarPageBody backToLinkProps={{
                    text: Localization.get("BackToEditExtension").replace("{0}", packageManifest.friendlyName),
                    onClick: props.onCancel.bind(this)
                }}>
                    <GridCell className="extension-form create-package-wizard">
                        {this.getCurrentWizardUI(createPackageStep)}
                    </GridCell>
                </PersonaBarPageBody>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

CreatePackage.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    packageManifest: PropTypes.object,
    installedPackageTypes: PropTypes.array,
    createPackageStep: PropTypes.number,
    packagePayload: PropTypes.object
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        installedPackageTypes: state.extension.installedPackageTypes,
        createdPackage: state.createPackage.createdPackage,
        packageManifest: state.createPackage.packageManifest,
        packagePayload: state.createPackage.packagePayload,
        createPackageStep: state.createPackage.currentStep
    };
}

export default connect(mapStateToProps)(CreatePackage);
