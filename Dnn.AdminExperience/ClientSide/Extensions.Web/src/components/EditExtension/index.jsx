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
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
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
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const { props } = this;
        this.isHost = utilities.settings.isHost;
        if ((!props.moduleCategories || props.moduleCategories.length === 0) && this.isHost) {
			logger.debug(`UNSAFE_componentWillMount(): this.isHost → ${this.isHost}`, { "event.action": "UNSAFE_componentWillMount" });
            props.dispatch(ExtensionActions.getModuleCategories());
			logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        }
    }

    onVersionChange(index, option) {
		logger.error(`>>> Entering onVersionChange(index=${index},option=${option})`, { "event.action": "onVersionChange", "logrite.index": index, "logrite.option": option });
        const { props } = this;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
        if (_extensionBeingEdited.version && _extensionBeingEdited.version.value) {
            let versionArray = _extensionBeingEdited.version.value.split(".");
			logger.error(`onVersionChange(index=${index},option=${option}): versionArray → ${versionArray}`, { "event.action": "onVersionChange", "logrite.index": index, "logrite.option": option });
            versionArray[index] = option.value;
            _extensionBeingEdited.version.value = versionArray.join(".");
        }
        this.updateExtensionBeingEdited(_extensionBeingEdited);
		logger.error(`onVersionChange(index=${index},option=${option}): _extensionBeingEdited → ${_extensionBeingEdited}`, { "event.action": "onVersionChange", "logrite.index": index, "logrite.option": option });
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
		logger.error(`>>> Entering onAssignedPortalsChange(key=${key},value=${value},callback=${callback})`, { "event.action": "onAssignedPortalsChange", "logrite.key": key, "logrite.value": value, "logrite.callback": callback });
        const { props } = this;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
		logger.error(`onAssignedPortalsChange(key=${key},value=${value},callback=${callback}): _extensionBeingEdited → ${_extensionBeingEdited}`, { "event.action": "onAssignedPortalsChange", "logrite.key": key, "logrite.value": value, "logrite.callback": callback });
        _extensionBeingEdited[key].value = value;
        this.updateExtensionBeingEdited(_extensionBeingEdited, callback);
		logger.warn(`<<< Exiting onAssignedPortalsChange(key=${key},value=${value},callback=${callback})`, { "event.action": "onAssignedPortalsChange", "logrite.key": key, "logrite.value": value, "logrite.callback": callback });
    }

    updateExtensionBeingEdited(extensionBeingEdited, callback) {
		logger.error(`>>> Entering updateExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited},callback=${callback})`, { "event.action": "updateExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited, "logrite.callback": callback });
        const { props } = this;
        props.dispatch(ExtensionActions.updateExtensionBeingEdited(extensionBeingEdited, callback));
		logger.warn(`<<< Exiting updateExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited},callback=${callback})`, { "event.action": "updateExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited, "logrite.callback": callback });
    }

    getAuthSystemCustomSettings(extension) {
		logger.error(`>>> Entering getAuthSystemCustomSettings(extension=${extension})`, { "event.action": "getAuthSystemCustomSettings", "logrite.extension": extension });
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

		logger.warn(`<<< Exiting getAuthSystemCustomSettings(extension=${extension})`, { "event.action": "getAuthSystemCustomSettings", "logrite.extension": extension });
        return payload;
    }

    parseEditorActions(extension) {
		logger.error(`>>> Entering parseEditorActions(extension=${extension})`, { "event.action": "parseEditorActions", "logrite.extension": extension });
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
				logger.warn(`<<< Exiting parseEditorActions(extension=${extension})`, { "event.action": "parseEditorActions", "logrite.extension": extension });
                return {};
        }
    }

    onSaveExtension(close) {
		logger.error(`>>> Entering onSaveExtension(close=${close})`, { "event.action": "onSaveExtension", "logrite.close": close });
        const {props} = this;

        let editorActions = this.parseEditorActions(props.extensionBeingEdited);
        props.dispatch(ExtensionActions.updateExtension(props.extensionBeingEdited, editorActions, props.extensionBeingEditedIndex, () => {
            if (close) {
                this.selectPanel(0);
                props.dispatch(ExtensionActions.selectEditingTab(0));
				logger.warn(`<<< Exiting onSaveExtension(close=${close})`, { "event.action": "onSaveExtension", "logrite.close": close });
            }
        }));
    }

    toggleTriedToSave() {
		logger.error('>>> Entering toggleTriedToSave()', { "event.action": "toggleTriedToSave" });
        const {props} = this;
        props.dispatch(ExtensionActions.toggleTriedToSave());
		logger.warn('<<< Exiting toggleTriedToSave()', { "event.action": "toggleTriedToSave" });
    }

    toggleTabError(tabWithError, action) {
		logger.error(`>>> Entering toggleTabError(tabWithError=${tabWithError},action=${action})`, { "event.action": "toggleTabError", "logrite.tabWithError": tabWithError, "logrite.action": action });
        const { props } = this;
        props.dispatch(ExtensionActions.toggleTabError(tabWithError, action));
		logger.warn(`<<< Exiting toggleTabError(tabWithError=${tabWithError},action=${action})`, { "event.action": "toggleTabError", "logrite.tabWithError": tabWithError, "logrite.action": action });
    }

    validateFields() {
		logger.error('>>> Entering validateFields()', { "event.action": "validateFields" });
        const { props } = this;
        let errorCount = 0;
        let _extensionBeingEdited = JSON.parse(JSON.stringify(props.extensionBeingEdited));
		logger.error(`validateFields(): _extensionBeingEdited → ${_extensionBeingEdited}`, { "event.action": "validateFields" });
        Object.keys(_extensionBeingEdited).forEach((key) => {
            let field = _extensionBeingEdited[key];
			logger.debug(`validateFields(): field.error → ${field.error}`, { "event.action": "validateFields" });
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
		logger.warn('<<< Exiting validateFields()', { "event.action": "validateFields" });
        return errorCount === 0;
    }

    selectPanel(panel, event) {
		logger.error(`>>> Entering selectPanel(panel=${panel},event=${event})`, { "event.action": "selectPanel", "logrite.panel": panel, "logrite.event": event });
        if (event) {
            event.preventDefault();
        }
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(panel));
		logger.warn(`<<< Exiting selectPanel(panel=${panel},event=${event})`, { "event.action": "selectPanel", "logrite.panel": panel, "logrite.event": event });
    }

    _getTabHeaders() {
		logger.error('>>> Entering _getTabHeaders()', { "event.action": "_getTabHeaders" });
        let tabHeaders = [Localization.get("EditExtension_PackageInformation.TabHeader"),
            Localization.get("EditExtension_ExtensionSettings.TabHeader"),
            Localization.get("EditExtension_SiteSettings.TabHeader"),
            Localization.get("EditExtension_License.TabHeader"),
            Localization.get("EditExtension_ReleaseNotes.TabHeader")];

		logger.error(`_getTabHeaders(): tabHeaders → ${tabHeaders}`, { "event.action": "_getTabHeaders" });
        let siteSettingIndex = 2;
        if (!this.isHost || !this.getExtensionSettingTabVisible(this.props.extensionBeingEdited.packageType.value)) {
            tabHeaders.splice(1, 1);
            siteSettingIndex = 1;
        }
        if (!this.getSiteSettingTabVisible(this.props.extensionBeingEdited.packageType.value)) {
            tabHeaders.splice(siteSettingIndex, 1);
        }
		logger.error('>>> Entering getTabHeaders()', { "event.action": "getTabHeaders" });
		logger.warn('<<< Exiting getTabHeaders()', { "event.action": "getTabHeaders" });
		logger.warn('<<< Exiting _getTabHeaders()', { "event.action": "_getTabHeaders" });
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
		logger.error(`>>> Entering onSave(close=${close})`, { "event.action": "onSave", "logrite.close": close });
        if (!this.validateFields()) {
            return;
        }

        this.onSaveExtension(close === true);
    }
    confirmAction(callback) {
		logger.error(`>>> Entering confirmAction(callback=${callback})`, { "event.action": "confirmAction", "logrite.callback": callback });
        const { props } = this;
        if (props.moduleDefinitionFormIsDirty) {
            this.setState({});
            utilities.utilities.confirm(Localization.get("UnsavedChanges.HelpText"), Localization.get("UnsavedChanges.Confirm"), Localization.get("UnsavedChanges.Cancel"), () => {
                callback();
                props.dispatch(ModuleDefinitionActions.setFormDirt(false));
            });
        } else {
            callback();
			logger.warn(`<<< Exiting confirmAction(callback=${callback})`, { "event.action": "confirmAction", "logrite.callback": callback });
        }
    }
    onTabSelect(index) {
		logger.error(`>>> Entering onTabSelect(index=${index})`, { "event.action": "onTabSelect", "logrite.index": index });
        this.confirmAction(() => {
            const { props } = this;
            props.dispatch(ExtensionActions.selectEditingTab(index));
        });
		logger.warn(`<<< Exiting onTabSelect(index=${index})`, { "event.action": "onTabSelect", "logrite.index": index });
    }

    startCreatePackageWizard() {
		logger.error('>>> Entering startCreatePackageWizard()', { "event.action": "startCreatePackageWizard" });
        const { props } = this;

        props.dispatch(CreatePackageActions.getPackageManifest(props.extensionBeingEdited.packageId.value, () => {
            this.selectPanel(5);
			logger.warn('<<< Exiting startCreatePackageWizard()', { "event.action": "startCreatePackageWizard" });
        }));
    }

    onCancel(event) {
		logger.error(`>>> Entering onCancel(event=${event})`, { "event.action": "onCancel", "logrite.event": event });
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        props.dispatch(ExtensionActions.selectEditingTab(0));
        this.selectPanel(0);
		logger.warn(`<<< Exiting onCancel(event=${event})`, { "event.action": "onCancel", "logrite.event": event });
    }

    getExtensionSettingTabVisible(type) {
		logger.error(`>>> Entering getExtensionSettingTabVisible(type=${type})`, { "event.action": "getExtensionSettingTabVisible", "logrite.type": type });
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
				logger.warn(`<<< Exiting getExtensionSettingTabVisible(type=${type})`, { "event.action": "getExtensionSettingTabVisible", "logrite.type": type });
                return false;
        }
    }

    getSiteSettingTabVisible(type) {
		logger.error(`>>> Entering getSiteSettingTabVisible(type=${type})`, { "event.action": "getSiteSettingTabVisible", "logrite.type": type });
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
				logger.warn(`<<< Exiting getSiteSettingTabVisible(type=${type})`, { "event.action": "getSiteSettingTabVisible", "logrite.type": type });
                return false;
        }
    }
    getTabUI() {
		logger.error('>>> Entering getTabUI()', { "event.action": "getTabUI" });
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
		logger.warn('<<< Exiting getTabUI()', { "event.action": "getTabUI" });
        return allTabs;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
        const {extensionBeingEdited} = props;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
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
