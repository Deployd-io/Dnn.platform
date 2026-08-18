import util from "../utils";
const logger = console;

function serializeQueryStringParameters(obj) {
	logger.debug(`>>> Entering serializeQueryStringParameters(obj=${obj})`);
    let s = [];
    // p is used in the loop but eslist is not seeting that.
     
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.debug(`<<< Exiting serializeQueryStringParameters(obj=${obj})`);
    return s.join("&");
}

class ApplicationService {
    getServiceFramework(moduleRoot, controller) {
		logger.debug(`>>> Entering getServiceFramework(moduleRoot=${moduleRoot},controller=${controller})`);
        let sf = util.utilities.sf;
        sf.moduleRoot = moduleRoot;
		logger.debug(`getServiceFramework(moduleRoot=${moduleRoot},controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`getServiceFramework(moduleRoot=${moduleRoot},controller=${controller}): sf.controller → ${sf.controller}`);
		logger.debug(`<<< Exiting getServiceFramework(moduleRoot=${moduleRoot},controller=${controller})`);
        return sf;
    }

    getPortals(callback, errorCallback) {
		logger.debug(`>>> Entering getPortals(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("PersonaBar", "Portals");
        sf.get("getPortals", {}, callback, errorCallback);
		logger.debug(`getPortals(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPortals(callback=${callback},errorCallback=${errorCallback})`);
    }

    getInitialPortalTabs(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getInitialPortalTabs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("PersonaBar", "Tabs");
        sf.get("GetPortalTabs?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.debug(`getInitialPortalTabs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getInitialPortalTabs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
    }

    getDescendantPortalTabs(parameters, parentIdObj, callback, errorCallback) {
		logger.debug(`>>> Entering getDescendantPortalTabs(parameters=${parameters},parentIdObj=${parentIdObj},callback=${callback},errorCallback=${errorCallback})`);
        const params = Object.assign({}, parameters, parentIdObj);
        const sf = this.getServiceFramework("PersonaBar", "Tabs");
		logger.debug(`getDescendantPortalTabs(parameters=${parameters},parentIdObj=${parentIdObj},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
        const serializedParams = serializeQueryStringParameters(params);
        sf.get(`GetTabsDescendants?${serializedParams}`, {}, callback, errorCallback);
		logger.debug(`getDescendantPortalTabs(parameters=${parameters},parentIdObj=${parentIdObj},callback=${callback},errorCallback=${errorCallback}): serializedParams → ${serializedParams}`);
		logger.debug(`<<< Exiting getDescendantPortalTabs(parameters=${parameters},parentIdObj=${parentIdObj},callback=${callback},errorCallback=${errorCallback})`);
    }

    getAllJobs(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getAllJobs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");
        sf.getsilence("AllJobs?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.debug(`getAllJobs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getAllJobs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
    }

    getLastJobTime(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getLastJobTime(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");
        sf.get("LastJobTime?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.debug(`getLastJobTime(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLastJobTime(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
    }

    getJobDetails(jobId, callback, errorCallback) {
		logger.debug(`>>> Entering getJobDetails(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.getsilence("JobDetails?jobId=" + jobId, {}, callback, errorCallback);
		logger.debug(`getJobDetails(jobId=${jobId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getJobDetails(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
    }

    exportSite(payload, callback, errorCallback) {
		logger.debug(`>>> Entering exportSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("Export", payload, callback, errorCallback);
		logger.debug(`exportSite(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting exportSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    }

    importSite(payload, callback, errorCallback) {
		logger.debug(`>>> Entering importSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("Import", payload, callback, errorCallback);
		logger.debug(`importSite(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting importSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    }

    getImportPackages(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getImportPackages(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");
        sf.get("GetImportPackages?"+ serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.debug(`getImportPackages(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getImportPackages(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
    }

    verifyImportPackage(packageId, callback, errorCallback) {
		logger.debug(`>>> Entering verifyImportPackage(packageId=${packageId},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.get("VerifyImportPackage?packageId=" + packageId, {}, callback, errorCallback);
		logger.debug(`verifyImportPackage(packageId=${packageId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting verifyImportPackage(packageId=${packageId},callback=${callback},errorCallback=${errorCallback})`);
    }

    cancelJob(jobId, callback, errorCallback) {
		logger.debug(`>>> Entering cancelJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("CancelProcess?jobId=" + jobId, {}, callback, errorCallback);
		logger.debug(`cancelJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting cancelJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
    }

    deleteJob(jobId, callback, errorCallback) {
		logger.debug(`>>> Entering deleteJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("RemoveJob?jobId=" + jobId, {}, callback, errorCallback);
		logger.debug(`deleteJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`);
    }

}
const applicationService = new ApplicationService();
export default applicationService;