import { importExport as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const jobsActions = {

    getInitialPortalTabs(PortalTabsParameters, callback) {
		logger.debug(`>>> Entering getInitialPortalTabs(PortalTabsParameters=${PortalTabsParameters},callback=${callback})`);
        ApplicationService.getInitialPortalTabs(PortalTabsParameters, (response) => {
            if (callback) {
                callback(response);
				logger.debug(`<<< Exiting getInitialPortalTabs(PortalTabsParameters=${PortalTabsParameters},callback=${callback})`);
            }
        });
    },

    getDescendantPortalTabs(PortalTabsParameters, ParentTabId, callback) {
		logger.debug(`>>> Entering getDescendantPortalTabs(PortalTabsParameters=${PortalTabsParameters},ParentTabId=${ParentTabId},callback=${callback})`);
        const ParentTabIdObj = {parentId: ParentTabId};
        ApplicationService.getDescendantPortalTabs(PortalTabsParameters, ParentTabIdObj, (response) => {
            if (callback) {
                callback(response);
				logger.debug(`<<< Exiting getDescendantPortalTabs(PortalTabsParameters=${PortalTabsParameters},ParentTabId=${ParentTabId},callback=${callback})`);
            }
        });
    },

    siteSelected(portalId, portalName, callback) {
		logger.debug(`>>> Entering siteSelected(portalId=${portalId},portalName=${portalName},callback=${callback})`);
		logger.debug(`<<< Exiting siteSelected(portalId=${portalId},portalName=${portalName},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_SITE,
                portalId: portalId === undefined ? -1 : portalId,
                portalName: portalName,
                currentJobId: null
            });
            if (callback) {
                callback();
            }
        };
    },

    navigateWizard(wizardStep, callback) {
		logger.debug(`>>> Entering navigateWizard(wizardStep=${wizardStep},callback=${callback})`);
		logger.debug(`<<< Exiting navigateWizard(wizardStep=${wizardStep},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.GO_TO_WIZARD_STEP,
                payload: {
                    wizardStep
                }
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },

    getPortals(callback, errorCallback) {
		logger.debug(`>>> Entering getPortals(callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting getPortals(callback=${callback},errorCallback=${errorCallback})`);
        return (dispatch) => {
            ApplicationService.getPortals((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTALS,
                    portals: data.Results
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },

    jobSelected(jobId, callback) {
		logger.debug(`>>> Entering jobSelected(jobId=${jobId},callback=${callback})`);
		logger.debug(`<<< Exiting jobSelected(jobId=${jobId},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_JOB,
                currentJobId: jobId === undefined ? null : jobId
            });
            if (callback) {
                callback();
            }
        };
    },

    getAllJobs(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getAllJobs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting getAllJobs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        return (dispatch) => {
            ApplicationService.getAllJobs(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_JOBS,
                    jobs: data.Jobs,
                    totalJobs: data.TotalJobs,
                    lastExportTime: data.LastExportTimeString,
                    lastImportTime: data.LastImportTimeString
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },

    getLastExportTime(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getLastExportTime(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting getLastExportTime(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        return (dispatch) => {
            ApplicationService.getLastJobTime(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LAST_EXPORT_DATE,
                    lastExportTime: data.lastTime
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },

    getJobDetails(jobId, callback, errorCallback) {
		logger.debug(`>>> Entering getJobDetails(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting getJobDetails(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
        return (dispatch) => {
            ApplicationService.getJobDetails(jobId, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_JOB_DETAILS,
                    job: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },

    exportSite(payload, callback, errorCallback) {
		logger.debug(`>>> Entering exportSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting exportSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        return (dispatch) => {
            ApplicationService.exportSite(payload, data => {
                dispatch({
                    type: ActionTypes.SUBMITTED_EXPORT_REQUEST,
                    jobId: data.jobId
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    },

    importSite(payload, callback, errorCallback) {
		logger.debug(`>>> Entering importSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting importSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        return (dispatch) => {
            ApplicationService.importSite(payload, data => {
                dispatch({
                    type: ActionTypes.SUBMITTED_IMPORT_REQUEST,
                    jobId: data.jobId,
                    packageVerified: false
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    },

    importWizardGoToSetp(step, callback) {
		logger.debug(`>>> Entering importWizardGoToSetp(step=${step},callback=${callback})`);
		logger.debug(`<<< Exiting importWizardGoToSetp(step=${step},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.GO_TO_IMPORT_WIZARD_STEP,
                importWizardStep: step
            });
            if (callback) {
                callback();
            }
        };
    },

    selectPackage(pkg, callback) {
		logger.debug(`>>> Entering selectPackage(pkg=${pkg},callback=${callback})`);
		logger.debug(`<<< Exiting selectPackage(pkg=${pkg},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_PACKAGE,
                selectedPackage: pkg,
                importSummary: undefined
            });
            if (callback) {
                callback();
            }
        };
    },

    packageVerified(verified, callback) {
		logger.debug(`>>> Entering packageVerified(verified=${verified},callback=${callback})`);
		logger.debug(`<<< Exiting packageVerified(verified=${verified},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.VERIFIED_PACKAGE,
                packageVerified: verified
            });
            if (callback) {
                callback();
            }
        };
    },

    getImportPackages(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getImportPackages(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting getImportPackages(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        return (dispatch) => {
            ApplicationService.getImportPackages(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_IMPORT_PACKAGES,
                    importPackages: data.packages,
                    total: data.total,
                    selectedPackage: null,
                    importWizardStep: 0,
                    packageVerified: false
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },

    verifyImportPackage(packageId, callback, errorCallback) {
		logger.debug(`>>> Entering verifyImportPackage(packageId=${packageId},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting verifyImportPackage(packageId=${packageId},callback=${callback},errorCallback=${errorCallback})`);
        return (dispatch) => {
            setTimeout(() => {
                ApplicationService.verifyImportPackage(packageId, (data) => {
                    dispatch({
                        type: ActionTypes.VERIFIED_IMPORT_PACKAGE,
                        importSummary: data
                    });
                    if (callback) {
                        callback();
                    }
                }, errorCallback);
            }, 2000);
        };
    },

    cancelJob(jobId, callback, errorCallback) {
		logger.debug(`>>> Entering cancelJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting cancelJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
        return (dispatch) => {
            ApplicationService.cancelJob(jobId, data => {
                dispatch({
                    type: ActionTypes.CANCELLED_JOB
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    },

    deleteJob(jobId, callback, errorCallback) {
		logger.debug(`>>> Entering deleteJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting deleteJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
        return (dispatch) => {
            ApplicationService.deleteJob(jobId, data => {
                dispatch({
                    type: ActionTypes.REMOVED_JOB
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    }
};

export default jobsActions;
