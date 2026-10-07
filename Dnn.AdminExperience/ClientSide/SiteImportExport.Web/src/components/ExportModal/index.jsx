import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions,
    visiblePanel as VisiblePanelActions
} from "../../actions";
import Localization from "localization";
import utilities from "utils";
import itemsToExportService from "../../services/itemsToExportService";
import styles from "./style.module.less";
const logger = console;

import {
    InputGroup,
    Label,
    Dropdown,
    SingleLineInputWithError,
    MultiLineInputWithError,
    GridCell,
    GridSystem,
    Switch,
    RadioButtons,
    Button,
    TreeControlInteractor } from "@dnnsoftware/dnn-react-common";

const keysToValidate = ["ExportName"];

class ExportModal extends Component {
    constructor(props) {
        super();
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        this.state = {
            wizardStep: 0,
            exportRequest: {
                PortalId: -1,
                ExportName: "",
                ExportDescription: "",
                IncludeUsers: true,
                IncludeVocabularies: true,
                IncludeTemplates: true,
                IncludeProperfileProperties: true,
                IncludeRoles: true,
                IncludePermissions: true,
                IncludeDeletions: false,
                IncludeContent: true,
                IncludeExtensions: true,
                IncludeFiles: true,
                ExportMode: props.lastExportTime ? "Differential" : "Full",
                ItemsToExport: this.getRegisteredItemsToExport(),
                RunNow: true
            },
            errors: {
                ExportName: false
            },
            IncludeContentEnabled: true,
            reloadPages: false,
            requestSubmitting: false
        };

        this.getInitialPortalTabs = props.getInitialPortalTabs;
		logger.debug(`constructor(props=${props}): this.getInitialPortalTabs → ${this.getInitialPortalTabs}`, { "event.action": "constructor", "logrite.props": props });
    }

    getRegisteredItemsToExport() {
		logger.error('>>> Entering getRegisteredItemsToExport()', { "event.action": "getRegisteredItemsToExport" });
		logger.warn('<<< Exiting getRegisteredItemsToExport()', { "event.action": "getRegisteredItemsToExport" });
        return itemsToExportService.getRegisteredItemsToExport()
            .filter(x => x.defaultSelected)
            .map(item => item.category);
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const { props, state } = this;
        const { exportRequest } = state;
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        exportRequest.PortalId = props.portalId;
        this.setState({
            exportRequest
        });
    }

    componentDidUpdate() {
		logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
        const { props, state } = this;
        const { exportRequest } = state;
		logger.debug(`componentDidUpdate(): exportRequest.PortalId → ${exportRequest.PortalId}`, { "event.action": "componentDidUpdate" });
        if (exportRequest.PortalId !== props.portalId) {
            exportRequest.PortalId = props.portalId;
            let reloadPages = true;
            this.setState({ exportRequest, reloadPages }, () => {
				logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
                this.setState({
                    reloadPages: false
                });
            });
        }
    }

    goToStep(wizardStep) {
		logger.error(`>>> Entering goToStep(wizardStep=${wizardStep})`, { "event.action": "goToStep", "logrite.wizardStep": wizardStep });
        const { props } = this;
        props.dispatch(ImportExportActions.navigateWizard(wizardStep));
		logger.warn(`<<< Exiting goToStep(wizardStep=${wizardStep})`, { "event.action": "goToStep", "logrite.wizardStep": wizardStep });
    }

    cancelExport() {
		logger.error('>>> Entering cancelExport()', { "event.action": "cancelExport" });
        const { props } = this;
        this.goToStep(0);
        props.onCancel();
		logger.warn('<<< Exiting cancelExport()', { "event.action": "cancelExport" });
    }

    onExport() {
		logger.error('>>> Entering onExport()', { "event.action": "onExport" });
        const { props, state } = this;
        if (this.Validate()) {
            this.setState({
                requestSubmitting: true
            });

            props.dispatch(ImportExportActions.exportSite(state.exportRequest, (data) => {
                this.setState({
                    requestSubmitting: false
                });
                utilities.utilities.notify(Localization.get("ExportRequestSubmitted"));
                props.dispatch(ImportExportActions.getAllJobs({
                    portal: props.portalId,
                    pageIndex: 0,
                    pageSize: 10
                }, () => {
                    props.dispatch(ImportExportActions.jobSelected(data.jobId));
                }));
                props.dispatch(VisiblePanelActions.selectPanel(0));
            }, () => {
                this.setState({
                    requestSubmitting: false
                });
                utilities.utilities.notifyError(Localization.get("ExportRequestSubmit.ErrorMessage"));
				logger.warn('<<< Exiting onExport()', { "event.action": "onExport" });
            }));
        }
    }

    Validate() {
        let success = this.ValidateTexts();
        if (success && !this.ValidateHasExportItem()) {
            utilities.utilities.notifyError(Localization.get("NoExportItem.ErrorMessage"));
            success = false;
        }
        return success;
    }

    getDescendantPortalTabs(PortalTabsParameters, ParentTabId, callback) {
		logger.error(`>>> Entering getDescendantPortalTabs(PortalTabsParameters=${PortalTabsParameters},ParentTabId=${ParentTabId},callback=${callback})`, { "event.action": "getDescendantPortalTabs", "logrite.PortalTabsParameters": PortalTabsParameters, "logrite.ParentTabId": ParentTabId, "logrite.callback": callback });
        this.props.dispatch(ImportExportActions.getDescendantPortalTabs(PortalTabsParameters, ParentTabId, callback));
		logger.warn(`<<< Exiting getDescendantPortalTabs(PortalTabsParameters=${PortalTabsParameters},ParentTabId=${ParentTabId},callback=${callback})`, { "event.action": "getDescendantPortalTabs", "logrite.PortalTabsParameters": PortalTabsParameters, "logrite.ParentTabId": ParentTabId, "logrite.callback": callback });
    }

    ValidateTexts(key) {
		logger.error(`>>> Entering ValidateTexts(key=${key})`, { "event.action": "ValidateTexts", "logrite.key": key });
        let success = true;
        const { exportRequest } = this.state;
        const { errors } = this.state;
        keysToValidate.map(vkey => {
            if (key === undefined || key === vkey) {
                if (exportRequest[vkey] === "") {
                    success = false;
                    errors[vkey] = true;
                } else {
                    errors[vkey] = false;
                }
            }
            this.setState({});
        });
		logger.warn(`<<< Exiting ValidateTexts(key=${key})`, { "event.action": "ValidateTexts", "logrite.key": key });
        return success;
    }

    ValidateHasExportItem() {
		logger.error('>>> Entering ValidateHasExportItem()', { "event.action": "ValidateHasExportItem" });
        let success;
        const { exportRequest } = this.state;
        if (exportRequest.IncludeContent || exportRequest.IncludeFiles || exportRequest.IncludeUsers || exportRequest.IncludeRoles ||
            exportRequest.IncludeVocabularies || exportRequest.IncludeTemplates || exportRequest.IncludeProperfileProperties ||
            exportRequest.IncludePermissions || exportRequest.IncludeExtensions || (exportRequest.pages && exportRequest.pages.length > 0)
            || (exportRequest.ItemsToExport && exportRequest.ItemsToExport.length > 0)) {
            success = true;
        }
        else {
            success = false;
        }
        return success;
    }

    getPortalOptions() {
		logger.error('>>> Entering getPortalOptions()', { "event.action": "getPortalOptions" });
        const { props } = this;
        let options = [{ label: props.portalName, value: props.portalId }];
		logger.warn('<<< Exiting getPortalOptions()', { "event.action": "getPortalOptions" });
        return options;
    }

    onChange(key, event) {
        const value = typeof event === "object" ? event.target.value : event;
        let { exportRequest } = this.state;
        exportRequest[key] = value;
        this.setState({
            exportRequest
        });
        if (keysToValidate.some(vkey => vkey === key))
            this.ValidateTexts(key);
    }

    onChangeItemsToExport(name) {
		logger.error(`>>> Entering onChangeItemsToExport(name=${name})`, { "event.action": "onChangeItemsToExport", "logrite.name": name });
        const { exportRequest } = this.state;
        const index = exportRequest.ItemsToExport.indexOf(name);
        if (index !== -1) {
            exportRequest.ItemsToExport.splice(index, 1);
        } else {
            exportRequest.ItemsToExport.push(name);
			logger.warn(`<<< Exiting onChangeItemsToExport(name=${name})`, { "event.action": "onChangeItemsToExport", "logrite.name": name });
        }
        this.setState({
            exportRequest
        });
    }


    updatePagesToExport(selectedPages) {
		logger.error(`>>> Entering updatePagesToExport(selectedPages=${selectedPages})`, { "event.action": "updatePagesToExport", "logrite.selectedPages": selectedPages });
        let { exportRequest } = this.state;
        let prevCount = (exportRequest.pages !== null && exportRequest.pages !== undefined) ? exportRequest.pages.length : 0;
		logger.error(`updatePagesToExport(selectedPages=${selectedPages}): prevCount → ${prevCount}`, { "event.action": "updatePagesToExport", "logrite.selectedPages": selectedPages });
        exportRequest.pages = selectedPages;
		logger.debug(`updatePagesToExport(selectedPages=${selectedPages}): exportRequest.pages → ${exportRequest.pages}`, { "event.action": "updatePagesToExport", "logrite.selectedPages": selectedPages });
        if (prevCount === 0) {
            exportRequest["IncludeContent"] = !(selectedPages === undefined || selectedPages.length <= 0);
        }
        this.setState({ exportRequest }, () => {
            if (keysToValidate.some(vkey => vkey === "IncludeContent"))
                this.ValidateTexts("IncludeContent");
            let { state } = this;
            state.IncludeContentEnabled = !(selectedPages === undefined || selectedPages.length <= 0);
            this.setState({ state });
        });
		logger.warn(`<<< Exiting updatePagesToExport(selectedPages=${selectedPages})`, { "event.action": "updatePagesToExport", "logrite.selectedPages": selectedPages });
    }

    getExportModeOptions() {
		logger.error('>>> Entering getExportModeOptions()', { "event.action": "getExportModeOptions" });
        let options = [
            { label: Localization.get("ExportModeDifferential"), value: "Differential" },
            { label: Localization.get("ExportModeComplete"), value: "Full" }
        ];
		logger.warn('<<< Exiting getExportModeOptions()', { "event.action": "getExportModeOptions" });
        return options;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { state, props } = this;
        const PortalTabsParameters = {
            portalId: props.portalId,
            cultureCode: "",
            isMultiLanguage: false,
            excludeAdminTabs: true,
            disabledNotSelectable: false,
            roles: "",
            sortOrder: 0,
            includeDisabled: true
        };
        const registeredItemsToExport = itemsToExportService.getRegisteredItemsToExport();
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.exportModal}>
                <div className="pageTitle">{Localization.get("ExportSettings")}</div>
                <GridCell className="export-site-container">
                    <div className="left-column">
                        <GridCell>
                            <Label
                                labelType="block"
                                label={Localization.get("Site")} />
                            <Dropdown
                                enabled={false}
                                options={this.getPortalOptions()}
                                value={props.portalId}
                                onSelect={this.onChange.bind(this, "PortalId")}
                            />
                        </GridCell>
                        <GridCell>
                            <SingleLineInputWithError
                                label={Localization.get("Name") + "*"}
                                inputStyle={{ margin: "0" }}
                                withLabel={false}
                                maxLength={100}
                                error={state.errors.ExportName}
                                errorMessage={Localization.get("ExportName.ErrorMessage")}
                                value={state.exportRequest.ExportName}
                                onChange={this.onChange.bind(this, "ExportName")}
                                style={{ width: "100%" }}
                            />
                        </GridCell>
                    </div>
                    <div className="right-column">
                        <GridCell>
                            <MultiLineInputWithError
                                inputStyle={{ "minHeight": 110 }}
                                maxLength={250}
                                style={{ "width": "100%" }}
                                label={Localization.get("Description")}
                                value={state.exportRequest.ExportDescription}
                                onChange={this.onChange.bind(this, "ExportDescription")}
                            />
                        </GridCell>
                    </div>
                    <div className="seperator">
                        <hr />
                    </div>
                    <GridSystem>
                        <div className="export-switches">
                            <div className="sectionTitle">{Localization.get("IncludeInExport")}</div>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Content")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.IncludeContent}
                                    onChange={this.onChange.bind(this, "IncludeContent")}
                                    readOnly={!state.IncludeContentEnabled}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Assets")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.IncludeFiles}
                                    onChange={this.onChange.bind(this, "IncludeFiles")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Users")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.IncludeUsers}
                                    onChange={this.onChange.bind(this, "IncludeUsers")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Roles")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.IncludeRoles}
                                    onChange={this.onChange.bind(this, "IncludeRoles")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Vocabularies")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.IncludeVocabularies}
                                    onChange={this.onChange.bind(this, "IncludeVocabularies")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("PageTemplates")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.IncludeTemplates}
                                    onChange={this.onChange.bind(this, "IncludeTemplates")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("ProfileProperties")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.IncludeProperfileProperties}
                                    onChange={this.onChange.bind(this, "IncludeProperfileProperties")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Permissions")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.IncludePermissions}
                                    onChange={this.onChange.bind(this, "IncludePermissions")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("Extensions")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.IncludeExtensions}
                                    onChange={this.onChange.bind(this, "IncludeExtensions")}
                                />
                            </InputGroup>
                            {registeredItemsToExport.map((item, i) =>
                                <InputGroup key={i}>
                                    <Label
                                        labelType="inline"
                                        label={item.name}
                                    />
                                    <Switch
                                        onText={Localization.get("SwitchOn")}
                                        offText={Localization.get("SwitchOff")}
                                        value={state.exportRequest.ItemsToExport.indexOf(item.category) !== -1}
                                        onChange={this.onChangeItemsToExport.bind(this, item.category)}
                                    />

                                </InputGroup>)
                            }
                            <InputGroup>
                                <div style={{ "paddingBottom": 20, "paddingTop": 10 }}><hr /></div>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("DeletionsInExport")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.IncludeDeletions}
                                    onChange={this.onChange.bind(this, "IncludeDeletions")}
                                />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("RunNow")}
                                />
                                <Switch
                                    onText={Localization.get("SwitchOn")}
                                    offText={Localization.get("SwitchOff")}
                                    value={state.exportRequest.RunNow}
                                    onChange={this.onChange.bind(this, "RunNow")}
                                />
                            </InputGroup>
                        </div>
                        <div className="export-pages">
                            <div className="sectionTitle">{Localization.get("PagesInExport")}</div>
                            <TreeControlInteractor
                                PortalTabsParameters={PortalTabsParameters}
                                OnSelect={this.updatePagesToExport.bind(this)}
                                moduleRoot={"PersonaBar"}
                                controller={"Tabs"}
                                serviceFramework={utilities.utilities.sf}
                                getInitialPortalTabs={this.getInitialPortalTabs}
                                getDescendantPortalTabs={this.getDescendantPortalTabs.bind(this)}
                            />


                            <div className="seperator2">
                                <hr />
                            </div>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("ExportMode")}
                                />
                                <RadioButtons
                                    disabled={!props.lastExportTime}
                                    onChange={this.onChange.bind(this, "ExportMode")}
                                    options={this.getExportModeOptions()}
                                    buttonGroup="exportMode"
                                    value={props.lastExportTime ? state.exportRequest.ExportMode : "Full"} />
                            </InputGroup>
                            <InputGroup>
                                <Label
                                    labelType="inline"
                                    label={Localization.get("LastExport")}
                                />
                                <div className="lastExport">{props.lastExportTime || Localization.get("EmptyDateTime")}</div>
                            </InputGroup>
                        </div>
                    </GridSystem>
                    <GridCell className="action-buttons">
                        <Button type="neutral" onClick={this.cancelExport.bind(this)}>{Localization.get("Cancel")}</Button>
                        <Button type="primary"
                            disabled={state.requestSubmitting}
                            onClick={this.onExport.bind(this)}>
                            {Localization.get("BeginExport")}
                        </Button>
                    </GridCell>
                </GridCell>
            </div>
        );
    }
}

ExportModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func,
    wizardStep: PropTypes.number,
    exportLogs: PropTypes.array,
    viewingLog: PropTypes.bool,
    portalId: PropTypes.number.isRequired,
    portalName: PropTypes.string,
    exportJobId: PropTypes.number,
    lastExportTime: PropTypes.string,
    getInitialPortalTabs: PropTypes.func.isRequired
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        wizardStep: state.importExport.exportWizardStep,
        exportLogs: state.importExport.exportLogs,
        viewingLog: state.importExport.viewingLog,
        exportJobId: state.importExport.exportJobId,
        lastExportTime: state.importExport.lastExportTime,
        portalId: state.importExport.portalId,
        portalName: state.importExport.portalName
    };
}

export default connect(mapStateToProps)(ExportModal);
