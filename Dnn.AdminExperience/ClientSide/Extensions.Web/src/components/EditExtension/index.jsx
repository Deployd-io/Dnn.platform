import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell, PersonaBarPageBody, PersonaBarPageHeader, DnnTabs, Tooltip, Button } from "@dnnsoftware/dnn-react-common";
import { ExtensionActions, VisiblePanelActions, ModuleDefinitionActions, CreatePackageActions } from "actions";
import License from "./License";
import ReleaseNotes from "./ReleaseNotes";
import PackageInformation from "./PackageInformation";
import CustomSettings from "./CustomSettings";
import EditSettings from "./EditSettings";
import Localization from "localization";
import utilities from "utils";
import styles from "./style.module.less";
const logger = console;


class EditExtension extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            extensionBeingEdited: {
                type: "",
                name: "",
                description: "",
                friendlyName: "",
                version: "9.0.0",
                owner: "",
                url: "",
                organization: "",
                email: ""
            },
            selectedTabIndex: 0
        };
		logger.debug('<<< Exiting constructor()');
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        const { props } = this;
        this.isHost = utilities.settings.isHost;
        if ((!props.moduleCategories || props.moduleCategories.length === 0) && this.isHost) {
			logger.debug(`UNSAFE_componentWillMount(): this.isHost → ${this.isHost}`);
            props.dispatch(ExtensionActions.getModuleCategories());
			logger.debug('<<< Exiting UNSAFE_componentWillMount()');
        }
    }

    onVersionChange(index, option) {
		logger.debug(`>>> Entering onVersionChange(index=${index},option=${option})`);
        const { props } = this;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        if (_extensionBeingEdited.version && _extensionBeingEdited.version.value) {
            let versionArray = _extensionBeingEdited.version.value.split(".");
			logger.debug(`onVersionChange(index=${index},option=${option}): versionArray → ${versionArray}`);
            versionArray[index] = option.value;
            _extensionBeingEdited.version.value = versionArray.join(".");
        }
        this.updateExtensionBeingEdited(_extensionBeingEdited);
		logger.debug(`onVersionChange(index=${index},option=${option}): _extensionBeingEdited → ${_extensionBeingEdited}`);
		logger.debug(`<<< Exiting onVersionChange(index=${index},option=${option})`);
    }

    onChange(key, event) {
        const { props } = this;
        const value = typeof event === "object" ? event.target.value : event;

        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        let field = _extensionBeingEdited[key];
        field.value = value;

        if (field.validateRequired && field.error) {
            field.error = false;
            this.toggleTriedToSave();
            this.toggleTabError(field.tabMapping, "remove");
        }
        this.updateExtensionBeingEdited(_extensionBeingEdited);
    }

    onAssignedPortalsChange(key, value, callback) {
		logger.debug(`>>> Entering onAssignedPortalsChange(key=${key},value=${value},callback=${callback})`);
        const { props } = this;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
		logger.debug(`onAssignedPortalsChange(key=${key},value=${value},callback=${callback}): _extensionBeingEdited → ${_extensionBeingEdited}`);
        _extensionBeingEdited[key].value = value;
        this.updateExtensionBeingEdited(_extensionBeingEdited, callback);
		logger.debug(`<<< Exiting onAssignedPortalsChange(key=${key},value=${value},callback=${callback})`);
    }

    updateExtensionBeingEdited(extensionBeingEdited, callback) {
		logger.debug(`>>> Entering updateExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited},callback=${callback})`);
        const { props } = this;
        props.dispatch(ExtensionActions.updateExtensionBeingEdited(extensionBeingEdited, callback));
		logger.debug(`<<< Exiting updateExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited},callback=${callback})`);
    }

    getAuthSystemCustomSettings(extension) {
		logger.debug(`>>> Entering getAuthSystemCustomSettings(extension=${extension})`);
        let payload = {
            loginControlSource: extension.loginControlSource.value,
            logoffControlSource: extension.logoffControlSource.value,
            enabled: extension.enabled.value,
            settingsControlSource: extension.settingsControlSource.value,
            authenticationType: extension.authenticationType.value,
            appId: extension.appId.value,
            appSecret: extension.appSecret.value,
            appEnabled: extension.appEnabled.value
        };

        if (extension.name.value === "DNNPro_ActiveDirectoryAuthentication") {
            delete payload.appId;
            delete payload.appSecret;
            delete payload.appEnabled;
        }

		logger.debug(`<<< Exiting getAuthSystemCustomSettings(extension=${extension})`);
        return payload;
    }

    parseEditorActions(extension) {
		logger.debug(`>>> Entering parseEditorActions(extension=${extension})`);
        switch (extension.packageType.value.toLowerCase()) {
            case "module":
                return extension.desktopModuleId ? {
                    category: extension.category.value,
                    dependencies: extension.dependencies.value,
                    hostPermissions: extension.hostPermissions.value,
                    shareable: extension.shareable.value,
                    premiumModule: extension.premiumModule.value,
                    assignPortal: JSON.stringify(extension.assignedPortals.value),
                    unassignPortal: JSON.stringify(extension.unassignedPortals.value),
                    folderName: extension.folderName.value,
                    businessController: extension.businessController.value
                } : null;
            case "auth_system":
                return this.getAuthSystemCustomSettings(extension);
            case "javascript_library":
                return {
                    customCdn: extension.customCdn.value
                };
            case "skin":
            case "container":
                return {
                    themePackageName: extension.themePackageName.value
                };
            case "skinobject":
                return {
                    controlKey: extension.controlKey.value,
                    controlSrc: extension.controlSrc.value,
                    supportsPartialRendering: extension.supportsPartialRendering.value
                };
            case "corelanguagepack":
                return {
                    languageId: extension.languageId.value
                };
            case "extensionlanguagepack":
                return {
                    languageId: extension.languageId.value,
                    dependentPackageId: extension.dependentPackageId.value
                };
            default:
				logger.debug(`<<< Exiting parseEditorActions(extension=${extension})`);
                return {};
        }
    }

    onSaveExtension(close) {
		logger.debug(`>>> Entering onSaveExtension(close=${close})`);
        const {props} = this;

        let editorActions = this.parseEditorActions(props.extensionBeingEdited);
        props.dispatch(ExtensionActions.updateExtension(props.extensionBeingEdited, editorActions, props.extensionBeingEditedIndex, () => {
            if (close) {
                this.selectPanel(0);
                props.dispatch(ExtensionActions.selectEditingTab(0));
				logger.debug(`<<< Exiting onSaveExtension(close=${close})`);
            }
        }));
    }

    toggleTriedToSave() {
		logger.debug('>>> Entering toggleTriedToSave()');
        const {props} = this;
        props.dispatch(ExtensionActions.toggleTriedToSave());
		logger.debug('<<< Exiting toggleTriedToSave()');
    }

    toggleTabError(tabWithError, action) {
		logger.debug(`>>> Entering toggleTabError(tabWithError=${tabWithError},action=${action})`);
        const { props } = this;
        props.dispatch(ExtensionActions.toggleTabError(tabWithError, action));
		logger.debug(`<<< Exiting toggleTabError(tabWithError=${tabWithError},action=${action})`);
    }

    validateFields() {
		logger.debug('>>> Entering validateFields()');
        const { props } = this;
        let errorCount = 0;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
		logger.debug(`validateFields(): _extensionBeingEdited → ${_extensionBeingEdited}`);
        Object.keys(_extensionBeingEdited).forEach((key) => {
            let field = _extensionBeingEdited[key];
            if (field.validateRequired && field.value === "") {
                field.error = true;
                errorCount++;
                this.toggleTabError(field.tabMapping);
            }
        });
        if (props.triedToSave === false) {
            this.toggleTriedToSave();
        }
        this.updateExtensionBeingEdited(_extensionBeingEdited);
		logger.debug('<<< Exiting validateFields()');
        return errorCount === 0;
    }

    selectPanel(panel, event) {
		logger.debug(`>>> Entering selectPanel(panel=${panel},event=${event})`);
        if (event) {
            event.preventDefault();
        }
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(panel));
		logger.debug(`<<< Exiting selectPanel(panel=${panel},event=${event})`);
    }

    _getTabHeaders() {
		logger.debug('>>> Entering _getTabHeaders()');
        let tabHeaders = [Localization.get("EditExtension_PackageInformation.TabHeader"),
            Localization.get("EditExtension_ExtensionSettings.TabHeader"),
            Localization.get("EditExtension_SiteSettings.TabHeader"),
            Localization.get("EditExtension_License.TabHeader"),
            Localization.get("EditExtension_ReleaseNotes.TabHeader")];

		logger.debug(`_getTabHeaders(): tabHeaders → ${tabHeaders}`);
        let siteSettingIndex = 2;
        if (!this.isHost || !this.getExtensionSettingTabVisible(this.props.extensionBeingEdited.packageType.value)) {
            tabHeaders.splice(1, 1);
            siteSettingIndex = 1;
        }
        if (!this.getSiteSettingTabVisible(this.props.extensionBeingEdited.packageType.value)) {
            tabHeaders.splice(siteSettingIndex, 1);
        }
		logger.debug('>>> Entering getTabHeaders()');
		logger.debug('<<< Exiting getTabHeaders()');
		logger.debug('<<< Exiting _getTabHeaders()');
        return tabHeaders;
    }

    getTabHeaders() {
        const { props } = this;
        const tabHeaders = this._getTabHeaders();
        return tabHeaders.map((tabHeader, index) => {
            const hasError = props.tabsWithError.indexOf(index) > -1;
            return <span key={index}>{tabHeader} <Tooltip type="error" rendered={hasError} messages={[Localization.get("EditExtensions_TabHasError")]} /></span>;
        });
    }

    onSave(close) {
		logger.debug(`>>> Entering onSave(close=${close})`);
        if (!this.validateFields()) {
            return;
        }

        this.onSaveExtension(close === true);
    }
    confirmAction(callback) {
		logger.debug(`>>> Entering confirmAction(callback=${callback})`);
        const { props } = this;
        if (props.moduleDefinitionFormIsDirty) {
            this.setState({});
            utilities.utilities.confirm(Localization.get("UnsavedChanges.HelpText"), Localization.get("UnsavedChanges.Confirm"), Localization.get("UnsavedChanges.Cancel"), () => {
                callback();
                props.dispatch(ModuleDefinitionActions.setFormDirt(false));
            });
        } else {
            callback();
			logger.debug(`<<< Exiting confirmAction(callback=${callback})`);
        }
    }
    onTabSelect(index) {
		logger.debug(`>>> Entering onTabSelect(index=${index})`);
        this.confirmAction(() => {
            const { props } = this;
            props.dispatch(ExtensionActions.selectEditingTab(index));
        });
		logger.debug(`<<< Exiting onTabSelect(index=${index})`);
    }

    startCreatePackageWizard() {
		logger.debug('>>> Entering startCreatePackageWizard()');
        const { props } = this;

        props.dispatch(CreatePackageActions.getPackageManifest(props.extensionBeingEdited.packageId.value, () => {
            this.selectPanel(5);
			logger.debug('<<< Exiting startCreatePackageWizard()');
        }));
    }

    onCancel(event) {
		logger.debug(`>>> Entering onCancel(event=${event})`);
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        props.dispatch(ExtensionActions.selectEditingTab(0));
        this.selectPanel(0);
		logger.debug(`<<< Exiting onCancel(event=${event})`);
    }

    getExtensionSettingTabVisible(type) {
		logger.debug(`>>> Entering getExtensionSettingTabVisible(type=${type})`);
        switch (type) {
            case "Auth_System":
            case "SkinObject":
            case "Skin":
            case "Container":
            case "ExtensionLanguagePack":
            case "CoreLanguagePack":
            case "JavaScript_Library":
                return true;
            case "Module":
                return this.props.extensionBeingEdited.desktopModuleId ? true : false;
            default:
				logger.debug(`<<< Exiting getExtensionSettingTabVisible(type=${type})`);
                return false;
        }
    }

    getSiteSettingTabVisible(type) {
		logger.debug(`>>> Entering getSiteSettingTabVisible(type=${type})`);
        switch (type) {
            case "SkinObject":
            case "Skin":
            case "Container":
            case "ExtensionLanguagePack":
            case "CoreLanguagePack":
            case "JavaScript_Library":
                return !this.isHost;
            case "Auth_System":
                return true;
            case "Module":
                return this.props.extensionBeingEdited.desktopModuleId ? true : false;
            default:
				logger.debug(`<<< Exiting getSiteSettingTabVisible(type=${type})`);
                return false;
        }
    }
    getTabUI() {
		logger.debug('>>> Entering getTabUI()');
        const {props} = this, {extensionBeingEdited} = props;
        let allTabs = [
            <GridCell key="first" className="package-information-box extension-form">
                <PackageInformation
                    onSave={this.onSave.bind(this)}
                    validationMapped={true}
                    extensionBeingEdited={extensionBeingEdited}
                    onVersionChange={this.onVersionChange.bind(this)}
                    onCancel={this.onCancel.bind(this)}
                    validateFields={this.validateFields.bind(this)}
                    onChange={this.onChange.bind(this)}
                    disabled={!this.isHost}
                    updateExtensionBeingEdited={this.updateExtensionBeingEdited.bind(this)}
                    triedToSave={props.triedToSave}
                    toggleTriedToSave={this.toggleTriedToSave.bind(this)}
                    primaryButtonText={Localization.get("Save.Button")} />
            </GridCell>,
            <GridCell key="second" className="extension-form">
                <CustomSettings
                    type={extensionBeingEdited.packageType.value}
                    primaryButtonText={Localization.get("Save.Button")}
                    onChange={this.onChange.bind(this)}
                    onCancel={this.onCancel.bind(this)}
                    onSave={this.onSave.bind(this)}
                    onAssignedPortalsChange={this.onAssignedPortalsChange.bind(this)} />
            </GridCell>,
            <GridCell key="third">
                <EditSettings
                    type={extensionBeingEdited.packageType.value}
                    onChange={this.onChange.bind(this)}
                    onCancel={this.onCancel.bind(this)}
                    onSave={this.onSave.bind(this)}
                    extensionBeingEdited={extensionBeingEdited}
                    updateExtensionBeingEdited={this.updateExtensionBeingEdited.bind(this)} />
            </GridCell>,
            <License key="fourth" 
                value={extensionBeingEdited.license.value}
                onChange={this.onChange.bind(this)}
                disabled={!this.isHost}
                onCancel={this.onCancel.bind(this)}
                onSave={this.onSave.bind(this)}
                primaryButtonText={Localization.get("Save.Button")} />,
            <ReleaseNotes key="fifth"
                value={extensionBeingEdited.releaseNotes.value}
                onChange={this.onChange.bind(this)}
                onCancel={this.onCancel.bind(this)}
                disabled={!this.isHost}
                onSave={this.onSave.bind(this)}
                primaryButtonText={Localization.get("Save.Button")} />
        ];
        let siteSettingIndex = 2;
        if (!this.isHost || !this.getExtensionSettingTabVisible(extensionBeingEdited.packageType.value)) {
            allTabs.splice(1, 1);
            siteSettingIndex = 1;
        }
        if (!this.getSiteSettingTabVisible(extensionBeingEdited.packageType.value)) {
            allTabs.splice(siteSettingIndex, 1);
        }
		logger.debug('<<< Exiting getTabUI()');
        return allTabs;
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        const {extensionBeingEdited} = props;
		logger.debug('<<< Exiting render()');
        return (
            <GridCell className={styles.editExtension}>
                <PersonaBarPageHeader title={extensionBeingEdited.friendlyName.value + " " + Localization.get("Extension.Header")} >
                    {this.isHost &&
                        <Button type="secondary" size="large" onClick={this.startCreatePackageWizard.bind(this)}>
                            {Localization.get("EditExtension_CreatePackage.Button")}
                        </Button>
                    }
                </PersonaBarPageHeader>
                <PersonaBarPageBody backToLinkProps={{
                    text: Localization.get("BackToExtensions"),
                    onClick: this.onCancel.bind(this)
                }}>
                    <DnnTabs
                        tabHeaders={this.getTabHeaders()}
                        onSelect={this.onTabSelect.bind(this)}
                        selectedIndex={props.selectedEditingTab}
                        type="primary">
                        {this.getTabUI()}
                    </DnnTabs>
                </PersonaBarPageBody>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

EditExtension.propTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    disabled: PropTypes.func,
    extensionBeingEdited: PropTypes.object,
    packageBeingEditedSettings: PropTypes.object
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex,
        packageBeingEditedSettings: state.extension.packageBeingEditedSettings,
        moduleDefinitionFormIsDirty: state.moduleDefinition.formIsDirty,
        triedToSave: state.extension.triedToSave,
        tabsWithError: state.extension.tabsWithError,
        moduleCategories: state.extension.moduleCategories,
        selectedEditingTab: state.extension.tabBeingEdited
    };
}

export default connect(mapStateToProps)(EditExtension);
