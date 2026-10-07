import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions,
    visiblePanel as VisiblePanelActions
} from "../../actions";
import Localization from "localization";
import ImportSummary from "./ImportSummary";
import PackagesList from "./PackagesList";
import PackageCard from "./PackageCard";
import FiltersBar from "./FiltersBar";
import ProgressBar from "./ProgressBar";
import styles from "./style.module.less";
import util from "utils";
import NoDataIcon from "./img/nodata.svg";
const logger = console;

import { Button, GridCell, Pager } from "@dnnsoftware/dnn-react-common";


class ImportModal extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            importRequest: {
                PortalId: -1,
                PackageId: -1,
                CollisionResolution: 1,
                RunNow: true
            },
            pageIndex: 0,
            pageSize: 5,
            filter: "newest",
            keyword: "",
            requestSubmitting: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const { props, state } = this;
        props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));

        const { importRequest } = state;
		logger.debug(`componentDidMount(): importRequest.PackageId → ${importRequest.PackageId}`, { "event.action": "componentDidMount" });
        importRequest.PortalId = props.portalId;
		logger.debug(`componentDidMount(): importRequest.PortalId → ${importRequest.PortalId}`, { "event.action": "componentDidMount" });
        importRequest.PackageId = props.selectedPackage ? props.selectedPackage.PackageId : -1;
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        this.setState({
            importRequest
        });
    }

    componentDidUpdate() {
		logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
        const { props, state } = this;
        const { importRequest } = state;
		logger.debug(`componentDidUpdate(): importRequest.PortalId → ${importRequest.PortalId}`, { "event.action": "componentDidUpdate" });
        if (importRequest.PortalId === -1 || importRequest.PortalId !== props.portalId) {
			logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
            importRequest.PortalId = props.portalId;
            this.setState({
                importRequest
            });
        }
    }

    goToStep(wizardStep) {
		logger.error(`>>> Entering goToStep(wizardStep=${wizardStep})`, { "event.action": "goToStep", "logrite.wizardStep": wizardStep });
        const { props } = this;
        props.dispatch(ImportExportActions.navigateWizard(wizardStep));
        props.dispatch(ImportExportActions.importWizardGoToSetp(0, () => {
            props.dispatch(ImportExportActions.selectPackage());
			logger.warn(`<<< Exiting goToStep(wizardStep=${wizardStep})`, { "event.action": "goToStep", "logrite.wizardStep": wizardStep });
        }));
    }

    selectPackage(pkg) {
		logger.error(`>>> Entering selectPackage(pkg=${pkg})`, { "event.action": "selectPackage", "logrite.pkg": pkg });
        const { props } = this;
        if (props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId) {
            props.dispatch(ImportExportActions.selectPackage());
        }
        else {
            props.dispatch(ImportExportActions.selectPackage(pkg, () => {
                let { importRequest } = this.state;
				logger.warn(`<<< Exiting selectPackage(pkg=${pkg})`, { "event.action": "selectPackage", "logrite.pkg": pkg });
                importRequest.PackageId = pkg.PackageId;
                this.setState({
                    importRequest
                });
            }));
        }
    }

    cancelImport() {
		logger.error('>>> Entering cancelImport()', { "event.action": "cancelImport" });
        const { props } = this;
        if (props.wizardStep === 0) {
            this.goToStep(0);
            props.onCancel();
            props.dispatch(ImportExportActions.packageVerified(false));
            props.dispatch(ImportExportActions.importWizardGoToSetp(0));
        }
        else {
            util.utilities.confirm(Localization.get("CancelImportMessage"),
                Localization.get("ConfirmCancel"),
                Localization.get("KeepImport"),
                () => {
                    props.dispatch(ImportExportActions.packageVerified(false));
                    props.dispatch(ImportExportActions.selectPackage());
                    props.dispatch(ImportExportActions.importWizardGoToSetp(0));
                });
				logger.warn('<<< Exiting cancelImport()', { "event.action": "cancelImport" });
        }
    }

    onAnalyze() {
		logger.error('>>> Entering onAnalyze()', { "event.action": "onAnalyze" });
        const { props } = this;
        if (props.selectedPackage) {
            props.dispatch(ImportExportActions.importWizardGoToSetp(1, () => {
                props.dispatch(ImportExportActions.verifyImportPackage(props.selectedPackage.PackageId, () => { }, () => {
                    props.dispatch(ImportExportActions.packageVerified(true));
                }));
            }));
        }
        else {
            util.utilities.notifyError(Localization.get("SelectException"));
			logger.warn('<<< Exiting onAnalyze()', { "event.action": "onAnalyze" });
        }
    }

    onImport() {
		logger.error('>>> Entering onImport()', { "event.action": "onImport" });
        const { props, state } = this;
        this.setState({
            requestSubmitting: true
        });
        props.dispatch(ImportExportActions.importSite(state.importRequest, (data) => {
            this.setState({
                requestSubmitting: false
            });
            util.utilities.notify(Localization.get("ImportRequestSubmitted"));
            props.dispatch(ImportExportActions.packageVerified(false));
            this.goToStep(0);
            props.dispatch(ImportExportActions.getAllJobs({
                portal: state.importRequest.PortalId,
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
            util.utilities.notifyError(Localization.get("ImportRequestSubmit.ErrorMessage"));
			logger.warn('<<< Exiting onImport()', { "event.action": "onImport" });
        }));
    }

    getSummaryItem(category) {
		logger.error(`>>> Entering getSummaryItem(category=${category})`, { "event.action": "getSummaryItem", "logrite.category": category });
        const { props } = this;
        let detail = props.importSummary.SummaryItems.find(c => c.Category === category.toUpperCase());
		logger.error(`getSummaryItem(category=${category}): detail → ${detail}`, { "event.action": "getSummaryItem", "logrite.category": category });
        return detail ? detail.TotalItemsString : "-";
    }

    onSwitchChange(key, event) {
		logger.error(`>>> Entering onSwitchChange(key=${key},event=${event})`, { "event.action": "onSwitchChange", "logrite.key": key, "logrite.event": event });
        const value = typeof event === "object" ? event.target.value : event;
        let { importRequest } = this.state;
		logger.error(`onSwitchChange(key=${key},event=${event}): value → ${value}`, { "event.action": "onSwitchChange", "logrite.key": key, "logrite.event": event });
		logger.debug(`onSwitchChange(key=${key},event=${event}): importRequest.CollisionResolution → ${importRequest.CollisionResolution}`, { "event.action": "onSwitchChange", "logrite.key": key, "logrite.event": event });
        if (key === "CollisionResolution") {
            importRequest.CollisionResolution = value ? 1 : 0;
			logger.warn(`<<< Exiting onSwitchChange(key=${key},event=${event})`, { "event.action": "onSwitchChange", "logrite.key": key, "logrite.event": event });
        }
        else if (key === "RunNow") {
            importRequest.RunNow = value;
        }
        this.setState({
            importRequest
        });
    }

    getNextPage() {
		logger.error('>>> Entering getNextPage()', { "event.action": "getNextPage" });
        const { state } = this;
		logger.warn('<<< Exiting getNextPage()', { "event.action": "getNextPage" });
        return {
            pageIndex: state.pageIndex || 0,
            pageSize: state.pageSize,
            order: state.filter,
            keyword: state.keyword
        };
    }

    onFilterChanged(filter) {
		logger.error(`>>> Entering onFilterChanged(filter=${filter})`, { "event.action": "onFilterChanged", "logrite.filter": filter });
        const { props } = this;
        this.setState({
            pageIndex: 0,
            filter: filter.value
        }, () => {
            props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));
        });
		logger.warn(`<<< Exiting onFilterChanged(filter=${filter})`, { "event.action": "onFilterChanged", "logrite.filter": filter });
    }

    onKeywordChanged(keyword) {
		logger.error(`>>> Entering onKeywordChanged(keyword=${keyword})`, { "event.action": "onKeywordChanged", "logrite.keyword": keyword });
        const { props } = this;
        this.setState({
            pageIndex: 0,
            keyword: keyword
        }, () => {
            props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));
        });
		logger.warn(`<<< Exiting onKeywordChanged(keyword=${keyword})`, { "event.action": "onKeywordChanged", "logrite.keyword": keyword });
    }

    onPageChange(currentPage, pageSize) {
		logger.error(`>>> Entering onPageChange(currentPage=${currentPage},pageSize=${pageSize})`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
        let { state, props } = this;
        if (pageSize !== undefined && state.pageSize !== pageSize) {
			logger.debug(`onPageChange(currentPage=${currentPage},pageSize=${pageSize}): state.pageIndex → ${state.pageIndex}`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
            state.pageSize = pageSize;
			logger.debug(`onPageChange(currentPage=${currentPage},pageSize=${pageSize}): state.pageSize → ${state.pageSize}`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
        }
        state.pageIndex = currentPage;
        this.setState({
            state
        }, () => {
            props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));
        });
		logger.warn(`<<< Exiting onPageChange(currentPage=${currentPage},pageSize=${pageSize})`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
    }

     
    renderPackageVerification() {
		logger.error('>>> Entering renderPackageVerification()', { "event.action": "renderPackageVerification" });
        const { props } = this;
		logger.warn('<<< Exiting renderPackageVerification()', { "event.action": "renderPackageVerification" });
        return <div>
            {props.selectedPackage && !props.packageVerified &&
                <div className="package-analyzing">
                    <div className="noDataText">{Localization.get("VerifyPackage")}</div>
                    <div className="noDataImage"><NoDataIcon /></div>
                    <ProgressBar className="progressCards" visible={true} loaded={props.importSummary} />
                </div>}
        </div>;
    }

    renderPager() {
		logger.error('>>> Entering renderPager()', { "event.action": "renderPager" });
        const { props, state } = this;
		logger.warn('<<< Exiting renderPager()', { "event.action": "renderPager" });
        return (
            <div className="packagePager">
                {props.importPackages && <Pager
                    showStartEndButtons={true}
                    showPageSizeOptions={true}
                    showPageInfo={false}
                    numericCounters={4}
                    pageSize={state.pageSize}
                    totalRecords={props.totalPackages}
                    onPageChanged={this.onPageChange.bind(this)}
                    pageSizeDropDownWithoutBorder={true}
                    pageSizeOptionText={"{0} results per page"}
                    summaryText={"Showing {0}-{1} of {2} results"}
                    culture={util.utilities.getCulture()}
                />}
            </div>
        );
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props, state } = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.importModal}>
                <div className="pageTitle">{Localization.get("SelectImportPackage")}</div>
                <div className="packages-wrapper">
                    {props.wizardStep === 0 &&
                        <FiltersBar onFilterChanged={this.onFilterChanged.bind(this)}
                            onKeywordChanged={this.onKeywordChanged.bind(this)}
                        />
                    }
                    <div className="packages">
                        {props.wizardStep === 0 &&
                            <PackagesList selectPackage={this.selectPackage.bind(this)} />
                        }
                        {props.wizardStep === 1 && props.selectedPackage &&
                            <div className="package-card-wrapper">
                                <PackageCard selectedPackage={props.selectedPackage} />
                            </div>
                        }
                        {props.wizardStep === 1 && !props.packageVerified &&
                            this.renderPackageVerification()
                        }
                    </div>
                    {props.wizardStep === 0 &&
                        this.renderPager()
                    }
                    {props.wizardStep === 1 && props.packageVerified &&
                        <ImportSummary
                            collisionResolution={state.importRequest.CollisionResolution}
                            runNow={state.importRequest.RunNow}
                            onSwitchChange={this.onSwitchChange.bind(this)} />
                    }
                </div>
                <GridCell className="action-buttons">
                    <Button type="neutral" onClick={this.cancelImport.bind(this)}>{Localization.get("Cancel")}</Button>
                    {props.wizardStep === 0 &&
                        <Button type="primary"
                            disabled={props.selectedPackage ? false : true}
                            onClick={this.onAnalyze.bind(this)}>
                            {Localization.get("Continue")}
                        </Button>
                    }
                    {props.wizardStep === 1 &&
                        <Button type="primary"
                            disabled={!props.importSummary || state.requestSubmitting}
                            onClick={this.onImport.bind(this)}>
                            {Localization.get("Continue")}
                        </Button>
                    }
                </GridCell>
            </div>
        );
    }
}

ImportModal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    portalId: PropTypes.number.isRequired,
    portalName: PropTypes.string,
    onCancel: PropTypes.func,
    wizardStep: PropTypes.number,
    importPackages: PropTypes.array,
    selectedPackage: PropTypes.object,
    importSummary: PropTypes.object,
    totalPackages: PropTypes.number,
    packageVerified: PropTypes.bool
};

ImportModal.defaultProps = {
    totalPackages: 0
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        wizardStep: state.importExport.importWizardStep,
        importPackages: state.importExport.importPackages,
        selectedPackage: state.importExport.selectedPackage,
        importSummary: state.importExport.importSummary,
        totalPackages: state.importExport.totalPackages,
        portalId: state.importExport.portalId,
        packageVerified: state.importExport.packageVerified
    };
}

export default connect(mapStateToProps)(ImportModal);