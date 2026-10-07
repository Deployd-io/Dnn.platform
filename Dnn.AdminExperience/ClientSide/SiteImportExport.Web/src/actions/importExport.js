import { importExport as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const jobsActions = {

    getInitialPortalTabs(PortalTabsParameters, callback) {
		logger.error(`>>> Entering getInitialPortalTabs(PortalTabsParameters=${PortalTabsParameters},callback=${callback})`, { "event.action": "getInitialPortalTabs", "logrite.PortalTabsParameters": PortalTabsParameters, "logrite.callback": callback });
        ApplicationService.getInitialPortalTabs(PortalTabsParameters, (response) => {
            if (callback) {
                callback(response);
				logger.warn(`<<< Exiting getInitialPortalTabs(PortalTabsParameters=${PortalTabsParameters},callback=${callback})`, { "event.action": "getInitialPortalTabs", "logrite.PortalTabsParameters": PortalTabsParameters, "logrite.callback": callback });
            }
        });
    },

    getDescendantPortalTabs(PortalTabsParameters, ParentTabId, callback) {
		logger.error(`>>> Entering getDescendantPortalTabs(PortalTabsParameters=${PortalTabsParameters},ParentTabId=${ParentTabId},callback=${callback})`, { "event.action": "getDescendantPortalTabs", "logrite.PortalTabsParameters": PortalTabsParameters, "logrite.ParentTabId": ParentTabId, "logrite.callback": callback });
        const ParentTabIdObj = {parentId: ParentTabId};
        ApplicationService.getDescendantPortalTabs(PortalTabsParameters, ParentTabIdObj, (response) => {
            if (callback) {
                callback(response);
				logger.warn(`<<< Exiting getDescendantPortalTabs(PortalTabsParameters=${PortalTabsParameters},ParentTabId=${ParentTabId},callback=${callback})`, { "event.action": "getDescendantPortalTabs", "logrite.PortalTabsParameters": PortalTabsParameters, "logrite.ParentTabId": ParentTabId, "logrite.callback": callback });
            }
        });
    },

    siteSelected(portalId, portalName, callback) {
		logger.error(`>>> Entering siteSelected(portalId=${portalId},portalName=${portalName},callback=${callback})`, { "event.action": "siteSelected", "logrite.portalId": portalId, "logrite.portalName": portalName, "logrite.callback": callback });
		logger.warn(`<<< Exiting siteSelected(portalId=${portalId},portalName=${portalName},callback=${callback})`, { "event.action": "siteSelected", "logrite.portalId": portalId, "logrite.portalName": portalName, "logrite.callback": callback });
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
		logger.error(`>>> Entering navigateWizard(wizardStep=${wizardStep},callback=${callback})`, { "event.action": "navigateWizard", "logrite.wizardStep": wizardStep, "logrite.callback": callback });
		logger.warn(`<<< Exiting navigateWizard(wizardStep=${wizardStep},callback=${callback})`, { "event.action": "navigateWizard", "logrite.wizardStep": wizardStep, "logrite.callback": callback });
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
		logger.error(`>>> Entering getPortals(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPortals", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting getPortals(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPortals", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
		logger.error(`>>> Entering jobSelected(jobId=${jobId},callback=${callback})`, { "event.action": "jobSelected", "logrite.jobId": jobId, "logrite.callback": callback });
		logger.warn(`<<< Exiting jobSelected(jobId=${jobId},callback=${callback})`, { "event.action": "jobSelected", "logrite.jobId": jobId, "logrite.callback": callback });
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
		logger.error(`>>> Entering getAllJobs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getAllJobs", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting getAllJobs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getAllJobs", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
		logger.error(`>>> Entering getLastExportTime(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getLastExportTime", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting getLastExportTime(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getLastExportTime", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
		logger.error(`>>> Entering getJobDetails(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getJobDetails", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting getJobDetails(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getJobDetails", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
		logger.error(`>>> Entering exportSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "exportSite", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting exportSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "exportSite", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
		logger.error(`>>> Entering importSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "importSite", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting importSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "importSite", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
		logger.error(`>>> Entering importWizardGoToSetp(step=${step},callback=${callback})`, { "event.action": "importWizardGoToSetp", "logrite.step": step, "logrite.callback": callback });
		logger.warn(`<<< Exiting importWizardGoToSetp(step=${step},callback=${callback})`, { "event.action": "importWizardGoToSetp", "logrite.step": step, "logrite.callback": callback });
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
		logger.error(`>>> Entering selectPackage(pkg=${pkg},callback=${callback})`, { "event.action": "selectPackage", "logrite.pkg": pkg, "logrite.callback": callback });
		logger.warn(`<<< Exiting selectPackage(pkg=${pkg},callback=${callback})`, { "event.action": "selectPackage", "logrite.pkg": pkg, "logrite.callback": callback });
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
		logger.error(`>>> Entering packageVerified(verified=${verified},callback=${callback})`, { "event.action": "packageVerified", "logrite.verified": verified, "logrite.callback": callback });
		logger.warn(`<<< Exiting packageVerified(verified=${verified},callback=${callback})`, { "event.action": "packageVerified", "logrite.verified": verified, "logrite.callback": callback });
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
		logger.error(`>>> Entering getImportPackages(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getImportPackages", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting getImportPackages(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getImportPackages", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
		logger.error(`>>> Entering verifyImportPackage(packageId=${packageId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "verifyImportPackage", "logrite.packageId": packageId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting verifyImportPackage(packageId=${packageId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "verifyImportPackage", "logrite.packageId": packageId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
		logger.error(`>>> Entering cancelJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "cancelJob", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting cancelJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "cancelJob", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
		logger.error(`>>> Entering deleteJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "deleteJob", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting deleteJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "deleteJob", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
