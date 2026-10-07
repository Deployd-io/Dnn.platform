import util from "../utils";
const logger = console;

function serializeQueryStringParameters(obj) {
	logger.error(`>>> Entering serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    let s = [];
    // p is used in the loop but eslist is not seeting that.
     
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.warn(`<<< Exiting serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    return s.join("&");
}

class ApplicationService {
    getServiceFramework(moduleRoot, controller) {
		logger.error(`>>> Entering getServiceFramework(moduleRoot=${moduleRoot},controller=${controller})`, { "event.action": "getServiceFramework", "logrite.moduleRoot": moduleRoot, "logrite.controller": controller });
        let sf = util.utilities.sf;
        sf.moduleRoot = moduleRoot;
		logger.debug(`getServiceFramework(moduleRoot=${moduleRoot},controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.moduleRoot": moduleRoot, "logrite.controller": controller });
        sf.controller = controller;
		logger.debug(`getServiceFramework(moduleRoot=${moduleRoot},controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.moduleRoot": moduleRoot, "logrite.controller": controller });
		logger.warn(`<<< Exiting getServiceFramework(moduleRoot=${moduleRoot},controller=${controller})`, { "event.action": "getServiceFramework", "logrite.moduleRoot": moduleRoot, "logrite.controller": controller });
        return sf;
    }

    getPortals(callback, errorCallback) {
		logger.error(`>>> Entering getPortals(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPortals", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("PersonaBar", "Portals");
        sf.get("getPortals", {}, callback, errorCallback);
		logger.error(`getPortals(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getPortals", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getInitialPortalTabs(parameters, callback, errorCallback) {
		logger.error(`>>> Entering getInitialPortalTabs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getInitialPortalTabs", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("PersonaBar", "Tabs");
        sf.get("GetPortalTabs?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.error(`getInitialPortalTabs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getInitialPortalTabs", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getDescendantPortalTabs(parameters, parentIdObj, callback, errorCallback) {
		logger.error(`>>> Entering getDescendantPortalTabs(parameters=${parameters},parentIdObj=${parentIdObj},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getDescendantPortalTabs", "logrite.parameters": parameters, "logrite.parentIdObj": parentIdObj, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const params = Object.assign({}, parameters, parentIdObj);
        const sf = this.getServiceFramework("PersonaBar", "Tabs");
		logger.error(`getDescendantPortalTabs(parameters=${parameters},parentIdObj=${parentIdObj},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getDescendantPortalTabs", "logrite.parameters": parameters, "logrite.parentIdObj": parentIdObj, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const serializedParams = serializeQueryStringParameters(params);
        sf.get(`GetTabsDescendants?${serializedParams}`, {}, callback, errorCallback);
		logger.error(`getDescendantPortalTabs(parameters=${parameters},parentIdObj=${parentIdObj},callback=${callback},errorCallback=${errorCallback}): serializedParams → ${serializedParams}`, { "event.action": "getDescendantPortalTabs", "logrite.parameters": parameters, "logrite.parentIdObj": parentIdObj, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getAllJobs(parameters, callback, errorCallback) {
		logger.error(`>>> Entering getAllJobs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getAllJobs", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");
        sf.getsilence("AllJobs?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.error(`getAllJobs(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getAllJobs", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getLastJobTime(parameters, callback, errorCallback) {
		logger.error(`>>> Entering getLastJobTime(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getLastJobTime", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");
        sf.get("LastJobTime?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.error(`getLastJobTime(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getLastJobTime", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getJobDetails(jobId, callback, errorCallback) {
		logger.error(`>>> Entering getJobDetails(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getJobDetails", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.getsilence("JobDetails?jobId=" + jobId, {}, callback, errorCallback);
		logger.error(`getJobDetails(jobId=${jobId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getJobDetails", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    exportSite(payload, callback, errorCallback) {
		logger.error(`>>> Entering exportSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "exportSite", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("Export", payload, callback, errorCallback);
		logger.error(`exportSite(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "exportSite", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    importSite(payload, callback, errorCallback) {
		logger.error(`>>> Entering importSite(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "importSite", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("Import", payload, callback, errorCallback);
		logger.error(`importSite(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "importSite", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getImportPackages(parameters, callback, errorCallback) {
		logger.error(`>>> Entering getImportPackages(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getImportPackages", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");
        sf.get("GetImportPackages?"+ serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.error(`getImportPackages(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getImportPackages", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    verifyImportPackage(packageId, callback, errorCallback) {
		logger.error(`>>> Entering verifyImportPackage(packageId=${packageId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "verifyImportPackage", "logrite.packageId": packageId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.get("VerifyImportPackage?packageId=" + packageId, {}, callback, errorCallback);
		logger.error(`verifyImportPackage(packageId=${packageId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "verifyImportPackage", "logrite.packageId": packageId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    cancelJob(jobId, callback, errorCallback) {
		logger.error(`>>> Entering cancelJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "cancelJob", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("CancelProcess?jobId=" + jobId, {}, callback, errorCallback);
		logger.error(`cancelJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "cancelJob", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    deleteJob(jobId, callback, errorCallback) {
		logger.error(`>>> Entering deleteJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "deleteJob", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("SiteExportImport", "ExportImport");        
        sf.post("RemoveJob?jobId=" + jobId, {}, callback, errorCallback);
		logger.error(`deleteJob(jobId=${jobId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "deleteJob", "logrite.jobId": jobId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

}
const applicationService = new ApplicationService();
export default applicationService;