import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { DnnTabs as Tabs, PersonaBarPageBody, TextOverflowWrapper, PersonaBarPageHeader, GridCell, Button } from "@dnnsoftware/dnn-react-common";
import { PaginationActions, VisiblePanelActions, ExtensionActions, InstallationActions } from "actions";
import Localization from "localization";
import InstalledExtensions from "./InstalledExtensions";
import AvailableExtensions from "./AvailableExtensions";
import utilities from "utils";
import { validationMapExtensionBeingEdited } from "utils/helperFunctions";
import { ModuleCustomSettings, CoreLanguagePack, ExtensionLanguagePack } from "./packageCustomSettings";
import "./style.less";
const logger = console;

const newExtension = {
    packageType: "",
    name: "",
    friendlyName: "",
    description: "",
    inUse: "",
    upgradeUrl: "",
    packageIcon: "",
    license: "",
    owner: "",
    organization: "",
    url: "",
    email: "",
    version: "0.0.0"
};

class Body extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.handleSelect = this.handleSelect.bind(this);
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const { props } = this;
        this.isHost = utilities.settings.isHost;
        if ((!props.locales || props.locales.length === 0)) {
			logger.debug(`UNSAFE_componentWillMount(): this.isHost → ${this.isHost}`, { "event.action": "UNSAFE_componentWillMount" });
            props.dispatch(ExtensionActions.getLocaleList());
        }
        if ((!props.localePackages || props.localePackages.length === 0)) {
            props.dispatch(ExtensionActions.getLocalePackageList());
			logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        }
    }

    handleSelect(index/*, last*/) {
        const { props } = this;
        props.dispatch(PaginationActions.loadTab(index)); //index acts as scopeTypeId
        this.setState({});
    }
    
    selectPanel(panel, event) {
		logger.error(`>>> Entering selectPanel(panel=${panel},event=${event})`, { "event.action": "selectPanel", "logrite.panel": panel, "logrite.event": event });
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        props.dispatch(InstallationActions.setIsPortalPackage(false, () => {
            props.dispatch(VisiblePanelActions.selectPanel(panel));
			logger.warn(`<<< Exiting selectPanel(panel=${panel},event=${event})`, { "event.action": "selectPanel", "logrite.panel": panel, "logrite.event": event });
        }));
    }

    onEditExtension(extensionBeingEditedIndex, packageId) {
		logger.error(`>>> Entering onEditExtension(extensionBeingEditedIndex=${extensionBeingEditedIndex},packageId=${packageId})`, { "event.action": "onEditExtension", "logrite.extensionBeingEditedIndex": extensionBeingEditedIndex, "logrite.packageId": packageId });
        const { props } = this;
        props.dispatch(ExtensionActions.editExtension(packageId, extensionBeingEditedIndex, () => {
            this.selectPanel(4);
			logger.warn(`<<< Exiting onEditExtension(extensionBeingEditedIndex=${extensionBeingEditedIndex},packageId=${packageId})`, { "event.action": "onEditExtension", "logrite.extensionBeingEditedIndex": extensionBeingEditedIndex, "logrite.packageId": packageId });
        }));
    }

    createExtension() {
		logger.error('>>> Entering createExtension()', { "event.action": "createExtension" });
        const { props } = this;
        let _newExtension = Object.assign(newExtension, ModuleCustomSettings);
        _newExtension = Object.assign(_newExtension, CoreLanguagePack);
        _newExtension = Object.assign(_newExtension, ExtensionLanguagePack);
        _newExtension = Object.assign(_newExtension, { locales: props.locales });
        _newExtension = Object.assign(_newExtension, { packages: props.localePackages });
        props.dispatch(ExtensionActions.addExtension(validationMapExtensionBeingEdited(_newExtension), () => {
            this.selectPanel(2);
			logger.warn('<<< Exiting createExtension()', { "event.action": "createExtension" });
        }));
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell className="extension-body">
                <PersonaBarPageHeader title={Localization.get("ExtensionsLabel")}>
                    {this.isHost && 
                        <Button type="primary" size="large" onClick={this.selectPanel.bind(this, 3)}>
                            <TextOverflowWrapper text={Localization.get("ExtensionInstall.Action") } maxWidth={120}/>
                        </Button>
                    }
                    {this.isHost && 
                        <Button type="secondary" size="large" onClick={this.createExtension.bind(this)}>
                            <TextOverflowWrapper text={Localization.get("CreateExtension.Action") } maxWidth={120}/>
                        </Button>
                    }
                    {this.isHost && 
                        <Button type="secondary" size="large" onClick={this.selectPanel.bind(this, 1)}>
                            <TextOverflowWrapper text={Localization.get("CreateModule.Action") } maxWidth={120}/>
                        </Button>
                    }
                </PersonaBarPageHeader>
                <PersonaBarPageBody>
                    {this.isHost && <Tabs
                        onSelect={this.handleSelect}
                        selectedIndex={props.tabIndex}
                        tabHeaders={[Localization.get("InstalledExtensions"), Localization.get("AvailableExtensions")]}>
                        <InstalledExtensions
                            isHost={this.isHost}
                            onEdit={this.onEditExtension.bind(this)}
                            onCancel={this.selectPanel.bind(this, 0)} />
                        <AvailableExtensions />
                    </Tabs>}
                    {!this.isHost && <InstalledExtensions
                        isHost={this.isHost}
                        onEdit={this.onEditExtension.bind(this)}
                        onCancel={this.selectPanel.bind(this, 0)} />}
                </PersonaBarPageBody>

            </GridCell>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    installedPackages: PropTypes.array,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return { installedPackages: state.extension.installedPackages, selectedInstalledPackageType: state.extension.selectedInstalledPackageType, tabIndex: state.pagination.tabIndex, locales: state.extension.locales, localePackages: state.extension.localePackages };
}

export default connect(mapStateToProps)(Body);
