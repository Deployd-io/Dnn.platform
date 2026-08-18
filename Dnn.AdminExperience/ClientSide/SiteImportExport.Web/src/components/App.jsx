import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    visiblePanel as VisiblePanelActions,
    importExport as ImportExportActions
} from "../actions";
import Localization from "localization";
import Dashboard from "./Dashboard";
import ExportModal from "./ExportModal";
import ImportModal from "./ImportModal";
import util from "../utils";
const logger = console;
import { PersonaBarPageHeader, PersonaBarPageBody, PersonaBarPage } from "@dnnsoftware/dnn-react-common";

class App extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            referrer: util.settings.referrer,
            referrerText: util.settings.referrerText,
            backToReferrerFunc: null
        };
		logger.debug('<<< Exiting constructor()');
    }

    backToReferrer(callback) {
		logger.debug(`>>> Entering backToReferrer(callback=${callback})`);
        if (typeof callback === "function") {
            callback();
        }
        setTimeout(() => {
            this.setState({
                referrer: "",
                referrerText: "",
                backToReferrerFunc: null
            });
        }, 750);
		logger.debug(`<<< Exiting backToReferrer(callback=${callback})`);
    }

    updateReferrerInfo(event) {
		logger.debug(`>>> Entering updateReferrerInfo(event=${event})`);
        this.setState({
            referrer: event.referrer,
            referrerText: event.referrerText,
            backToReferrerFunc: this.backToReferrer.bind(this, event.backToReferrerFunc)
        });
		logger.debug(`<<< Exiting updateReferrerInfo(event=${event})`);
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        const { props } = this;

        document.addEventListener("siteImportExport", (e) => {
            props.dispatch(ImportExportActions.siteSelected(e.importExportJob.portalId, e.importExportJob.portalName, () => {
                props.dispatch(ImportExportActions.getLastExportTime({ "portal": e.importExportJob.portalId, "jobType": "Export" }));
                this.selectPanel(e.importExportJob.jobType === "Export" ? 1 : 2);
            }));
            this.updateReferrerInfo(e);
        }, false);

        if (util.settings.importExportJob) {
            props.dispatch(ImportExportActions.siteSelected(util.settings.importExportJob.portalId, util.settings.importExportJob.portalName, () => {
                props.dispatch(ImportExportActions.getLastExportTime({ "portal": util.settings.importExportJob.portalId, "jobType": "Export" }));
                this.selectPanel(util.settings.importExportJob.jobType === "Export" ? 1 : 2);
            }));
            this.updateReferrerInfo(util.settings);
			logger.debug('<<< Exiting componentDidMount()');
        }
    }

    selectPanel(panel, event) {
		logger.debug(`>>> Entering selectPanel(panel=${panel},event=${event})`);
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        props.dispatch(VisiblePanelActions.selectPanel(panel));
        props.dispatch(ImportExportActions.jobSelected());
		logger.debug(`<<< Exiting selectPanel(panel=${panel},event=${event})`);
    }

    selectPanelInternal(panel, event) {
		logger.debug(`>>> Entering selectPanelInternal(panel=${panel},event=${event})`);
        if (event) {
            event.preventDefault();
        }
        const { props } = this;
        if (props.importWizardStep > 0) {
            util.utilities.confirm(Localization.get("CancelImportMessage"),
                Localization.get("ConfirmCancel"),
                Localization.get("KeepImport"),
                () => {
                    props.dispatch(ImportExportActions.packageVerified(false));
                    props.dispatch(ImportExportActions.importWizardGoToSetp(0));
                    props.dispatch(VisiblePanelActions.selectPanel(panel));
                });
        }
        else {
            props.dispatch(VisiblePanelActions.selectPanel(panel));
        }
        props.dispatch(ImportExportActions.jobSelected());
		logger.debug(`<<< Exiting selectPanelInternal(panel=${panel},event=${event})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const { props, state } = this;
        const PageHeaderSubtitleStyles = {
            clear:"both",
            display:"block",
            position:"relative",
            top:"-10px"
        };


		logger.debug('<<< Exiting render()');
        return (
            <div>
                <PersonaBarPage isOpen={props.selectedPage === 0} className={(props.selectedPage !== 0 ? "hidden" : "")}>
                    <PersonaBarPageHeader title={Localization.get("SiteImportExport.Header")}>
                    </PersonaBarPageHeader>
                    <PersonaBarPageBody backToLinkProps={{
                        text: state.referrer && state.referrerText,
                        onClick: state.backToReferrerFunc
                    }}>
                        {props.selectedPage === 0 &&
                            <Dashboard selectPanel={this.selectPanel.bind(this)} />
                        }
                    </PersonaBarPageBody>
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 1}>
                    <PersonaBarPageHeader title={Localization.get("Export")}>
                    </PersonaBarPageHeader>
                    <PersonaBarPageBody backToLinkProps={{
                        text: state.referrer && state.referrerText || Localization.get("BackToImportExport"),
                        onClick: state.backToReferrerFunc || this.selectPanel.bind(this, 0)
                    }}>
                        {props.selectedPage === 1 &&
                            <ExportModal
                                onCancel={this.selectPanelInternal.bind(this, 0)}
                                getInitialPortalTabs={ImportExportActions.getInitialPortalTabs}
                            />}

                    </PersonaBarPageBody>
                </PersonaBarPage>
                <PersonaBarPage isOpen={props.selectedPage === 2}>
                    <PersonaBarPageHeader title={Localization.get("Import")}>
                        <div style={PageHeaderSubtitleStyles}>{props.portalName}</div>
                    </PersonaBarPageHeader>
                    <PersonaBarPageBody backToLinkProps={{
                        text: state.referrer && state.referrerText || Localization.get("BackToImportExport"),
                        onClick: state.backToReferrerFunc || this.selectPanelInternal.bind(this, 0)
                    }}>
                        {props.selectedPage === 2 &&
                            <ImportModal onCancel={this.selectPanel.bind(this, 0)} />}
                    </PersonaBarPageBody>
                </PersonaBarPage>
            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number,
    portalId: PropTypes.number,
    portalName: PropTypes.string,
    importWizardStep: PropTypes.number
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex,
        portalId: state.importExport.portalId,
        portalName: state.importExport.portalName,
        importWizardStep: state.importExport.importWizardStep
    };
}

export default connect(mapStateToProps)(App);