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
		logger.debug('>>> Entering constructor()');
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
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        const { props, state } = this;
        props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));

        const { importRequest } = state;
        importRequest.PortalId = props.portalId;
		logger.debug(`componentDidMount(): importRequest.PortalId → ${importRequest.PortalId}`);
        importRequest.PackageId = props.selectedPackage ? props.selectedPackage.PackageId : -1;
		logger.debug('<<< Exiting componentDidMount()');
        this.setState({
            importRequest
        });
    }

    componentDidUpdate() {
		logger.debug('>>> Entering componentDidUpdate()');
        const { props, state } = this;
        const { importRequest } = state;
        if (importRequest.PortalId === -1 || importRequest.PortalId !== props.portalId) {
			logger.debug('<<< Exiting componentDidUpdate()');
            importRequest.PortalId = props.portalId;
            this.setState({
                importRequest
            });
        }
    }

    goToStep(wizardStep) {
		logger.debug(`>>> Entering goToStep(wizardStep=${wizardStep})`);
        const { props } = this;
        props.dispatch(ImportExportActions.navigateWizard(wizardStep));
        props.dispatch(ImportExportActions.importWizardGoToSetp(0, () => {
            props.dispatch(ImportExportActions.selectPackage());
			logger.debug(`<<< Exiting goToStep(wizardStep=${wizardStep})`);
        }));
    }

    selectPackage(pkg) {
		logger.debug(`>>> Entering selectPackage(pkg=${pkg})`);
        const { props } = this;
        if (props.selectedPackage && props.selectedPackage.PackageId === pkg.PackageId) {
            props.dispatch(ImportExportActions.selectPackage());
        }
        else {
            props.dispatch(ImportExportActions.selectPackage(pkg, () => {
                let { importRequest } = this.state;
				logger.debug(`<<< Exiting selectPackage(pkg=${pkg})`);
                importRequest.PackageId = pkg.PackageId;
                this.setState({
                    importRequest
                });
            }));
        }
    }

    cancelImport() {
		logger.debug('>>> Entering cancelImport()');
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
				logger.debug('<<< Exiting cancelImport()');
        }
    }

    onAnalyze() {
		logger.debug('>>> Entering onAnalyze()');
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
			logger.debug('<<< Exiting onAnalyze()');
        }
    }

    onImport() {
		logger.debug('>>> Entering onImport()');
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
			logger.debug('<<< Exiting onImport()');
        }));
    }

    getSummaryItem(category) {
		logger.debug(`>>> Entering getSummaryItem(category=${category})`);
        const { props } = this;
        let detail = props.importSummary.SummaryItems.find(c => c.Category === category.toUpperCase());
		logger.debug(`getSummaryItem(category=${category}): detail → ${detail}`);
		logger.debug(`<<< Exiting getSummaryItem(category=${category})`);
        return detail ? detail.TotalItemsString : "-";
    }

    onSwitchChange(key, event) {
		logger.debug(`>>> Entering onSwitchChange(key=${key},event=${event})`);
        const value = typeof event === "object" ? event.target.value : event;
        let { importRequest } = this.state;
		logger.debug(`onSwitchChange(key=${key},event=${event}): value → ${value}`);
        if (key === "CollisionResolution") {
            importRequest.CollisionResolution = value ? 1 : 0;
			logger.debug(`<<< Exiting onSwitchChange(key=${key},event=${event})`);
        }
        else if (key === "RunNow") {
            importRequest.RunNow = value;
        }
        this.setState({
            importRequest
        });
    }

    getNextPage() {
		logger.debug('>>> Entering getNextPage()');
        const { state } = this;
		logger.debug('<<< Exiting getNextPage()');
        return {
            pageIndex: state.pageIndex || 0,
            pageSize: state.pageSize,
            order: state.filter,
            keyword: state.keyword
        };
    }

    onFilterChanged(filter) {
		logger.debug(`>>> Entering onFilterChanged(filter=${filter})`);
        const { props } = this;
        this.setState({
            pageIndex: 0,
            filter: filter.value
        }, () => {
            props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));
        });
		logger.debug(`<<< Exiting onFilterChanged(filter=${filter})`);
    }

    onKeywordChanged(keyword) {
		logger.debug(`>>> Entering onKeywordChanged(keyword=${keyword})`);
        const { props } = this;
        this.setState({
            pageIndex: 0,
            keyword: keyword
        }, () => {
            props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));
        });
		logger.debug(`<<< Exiting onKeywordChanged(keyword=${keyword})`);
    }

    onPageChange(currentPage, pageSize) {
		logger.debug(`>>> Entering onPageChange(currentPage=${currentPage},pageSize=${pageSize})`);
        let { state, props } = this;
        if (pageSize !== undefined && state.pageSize !== pageSize) {
            state.pageSize = pageSize;
        }
        state.pageIndex = currentPage;
        this.setState({
            state
        }, () => {
            props.dispatch(ImportExportActions.getImportPackages(this.getNextPage()));
        });
		logger.debug(`onPageChange(currentPage=${currentPage},pageSize=${pageSize}): state.pageSize → ${state.pageSize}`);
		logger.debug(`<<< Exiting onPageChange(currentPage=${currentPage},pageSize=${pageSize})`);
    }

     
    renderPackageVerification() {
		logger.debug('>>> Entering renderPackageVerification()');
        const { props } = this;
		logger.debug('<<< Exiting renderPackageVerification()');
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
		logger.debug('>>> Entering renderPager()');
        const { props, state } = this;
		logger.debug('<<< Exiting renderPager()');
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
		logger.debug('>>> Entering render()');
        const { props, state } = this;
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
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