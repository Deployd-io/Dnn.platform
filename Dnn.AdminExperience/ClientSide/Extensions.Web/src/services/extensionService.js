import utilities from "../utils";
const logger = console;

function mapPackageInformation(extensionBeingUpdated, addMode) {
	logger.error(`>>> Entering mapPackageInformation(extensionBeingUpdated=${extensionBeingUpdated},addMode=${addMode})`, { "event.action": "mapPackageInformation", "logrite.extensionBeingUpdated": extensionBeingUpdated, "logrite.addMode": addMode });
    let _extensionBeingUpdated = {
        Name: extensionBeingUpdated.name.value,
        FriendlyName: extensionBeingUpdated.friendlyName.value,
        Description: extensionBeingUpdated.description.value,
        Version: extensionBeingUpdated.version.value,
        Owner: extensionBeingUpdated.owner.value,
        Url: extensionBeingUpdated.url.value,
        Organization: extensionBeingUpdated.organization.value,
        Email: extensionBeingUpdated.email.value,
        License: extensionBeingUpdated.releaseNotes && extensionBeingUpdated.license.value,
        ReleaseNotes: extensionBeingUpdated.releaseNotes && extensionBeingUpdated.releaseNotes.value
    };
    if (addMode) {
		logger.debug(`mapPackageInformation(extensionBeingUpdated=${extensionBeingUpdated},addMode=${addMode}): _extensionBeingUpdated.PackageType → ${_extensionBeingUpdated.PackageType}`, { "event.action": "mapPackageInformation", "logrite.extensionBeingUpdated": extensionBeingUpdated, "logrite.addMode": addMode });
        delete _extensionBeingUpdated.License;
        delete _extensionBeingUpdated.ReleaseNotes;
        _extensionBeingUpdated.PackageType = extensionBeingUpdated.packageType.value;
    }
	logger.warn(`<<< Exiting mapPackageInformation(extensionBeingUpdated=${extensionBeingUpdated},addMode=${addMode})`, { "event.action": "mapPackageInformation", "logrite.extensionBeingUpdated": extensionBeingUpdated, "logrite.addMode": addMode });
    return _extensionBeingUpdated;
}

function serializeQueryStringParameters(obj) {
	logger.error(`>>> Entering serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    let s = [];
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.warn(`<<< Exiting serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    return s.join("&");
}
class ExtensionService {
    getServiceFramework(controller) {
		logger.error(`>>> Entering getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        let sf = utilities.utilities.sf;

		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;

		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }
    getInstalledPackages(type, callback) {
		logger.error(`>>> Entering getInstalledPackages(type=${type},callback=${callback})`, { "event.action": "getInstalledPackages", "logrite.type": type, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetInstalledPackages?packageType=" + type, {}, callback);
		logger.error(`getInstalledPackages(type=${type},callback=${callback}): sf → ${sf}`, { "event.action": "getInstalledPackages", "logrite.type": type, "logrite.callback": callback });
    }
    getAvailablePackages(type, callback) {
		logger.error(`>>> Entering getAvailablePackages(type=${type},callback=${callback})`, { "event.action": "getAvailablePackages", "logrite.type": type, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetAvailablePackages?packageType=" + type, {}, callback);
		logger.error(`getAvailablePackages(type=${type},callback=${callback}): sf → ${sf}`, { "event.action": "getAvailablePackages", "logrite.type": type, "logrite.callback": callback });
    }
    getPackageTypes(callback) {
		logger.error(`>>> Entering getPackageTypes(callback=${callback})`, { "event.action": "getPackageTypes", "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetPackageTypes", {}, callback);
		logger.error(`getPackageTypes(callback=${callback}): sf → ${sf}`, { "event.action": "getPackageTypes", "logrite.callback": callback });
    }
    getAvailablePackageTypes(callback) {
		logger.error(`>>> Entering getAvailablePackageTypes(callback=${callback})`, { "event.action": "getAvailablePackageTypes", "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetAvailablePackageTypes", {}, callback);
		logger.error(`getAvailablePackageTypes(callback=${callback}): sf → ${sf}`, { "event.action": "getAvailablePackageTypes", "logrite.callback": callback });
    }
    updateExtension(extensionBeingUpdated, editorActions, callback) {
		logger.error(`>>> Entering updateExtension(extensionBeingUpdated=${extensionBeingUpdated},editorActions=${editorActions},callback=${callback})`, { "event.action": "updateExtension", "logrite.extensionBeingUpdated": extensionBeingUpdated, "logrite.editorActions": editorActions, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            packageId: extensionBeingUpdated.packageId.value,
            portalId: utilities.settings.portalId,
            settings: mapPackageInformation(extensionBeingUpdated),
            editorActions: editorActions
        };
        sf.post("SavePackageSettings", payload, callback);
		logger.error(`updateExtension(extensionBeingUpdated=${extensionBeingUpdated},editorActions=${editorActions},callback=${callback}): sf → ${sf}`, { "event.action": "updateExtension", "logrite.extensionBeingUpdated": extensionBeingUpdated, "logrite.editorActions": editorActions, "logrite.callback": callback });
    }
    createNewExtension(extensionBeingUpdated, editorActions, callback, errorCallback) {
		logger.error(`>>> Entering createNewExtension(extensionBeingUpdated=${extensionBeingUpdated},editorActions=${editorActions},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createNewExtension", "logrite.extensionBeingUpdated": extensionBeingUpdated, "logrite.editorActions": editorActions, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            packageId: -1,
            portalId: -1,
            settings: mapPackageInformation(extensionBeingUpdated, true),
            editorActions: editorActions
        };
        sf.post("CreateExtension", payload, callback, errorCallback);
		logger.error(`createNewExtension(extensionBeingUpdated=${extensionBeingUpdated},editorActions=${editorActions},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "createNewExtension", "logrite.extensionBeingUpdated": extensionBeingUpdated, "logrite.editorActions": editorActions, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    downloadPackage(PackageType, FileName, callback) {
		logger.error(`>>> Entering downloadPackage(PackageType=${PackageType},FileName=${FileName},callback=${callback})`, { "event.action": "downloadPackage", "logrite.PackageType": PackageType, "logrite.FileName": FileName, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            PackageType,
            FileName
        };
        sf.get("DownloadPackage", payload, callback);
		logger.error(`downloadPackage(PackageType=${PackageType},FileName=${FileName},callback=${callback}): sf → ${sf}`, { "event.action": "downloadPackage", "logrite.PackageType": PackageType, "logrite.FileName": FileName, "logrite.callback": callback });
    }
    installAvailablePackage(PackageType, FileName, callback) {
		logger.error(`>>> Entering installAvailablePackage(PackageType=${PackageType},FileName=${FileName},callback=${callback})`, { "event.action": "installAvailablePackage", "logrite.PackageType": PackageType, "logrite.FileName": FileName, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            PackageType,
            FileName
        };
        sf.post("InstallAvailablePackage", payload, callback);
		logger.error(`installAvailablePackage(PackageType=${PackageType},FileName=${FileName},callback=${callback}): sf → ${sf}`, { "event.action": "installAvailablePackage", "logrite.PackageType": PackageType, "logrite.FileName": FileName, "logrite.callback": callback });
    }
    deletePackage(payload, callback) {
		logger.error(`>>> Entering deletePackage(payload=${payload},callback=${callback})`, { "event.action": "deletePackage", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        sf.post("DeletePackage", payload, callback);
		logger.error(`deletePackage(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "deletePackage", "logrite.payload": payload, "logrite.callback": callback });
    }
    parsePackage(file, callback, errorCallback) {
		logger.error(`>>> Entering parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        let formData = new FormData();
		logger.error(`parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback}): formData → ${formData}`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        formData.append("POSTFILE", file);


        sf.postfile("ParsePackage", formData, callback, errorCallback);
		logger.warn(`<<< Exiting parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    deployAvailablePackage(cultureCode, callback, errorCallback) {
		logger.error(`>>> Entering deployAvailablePackage(cultureCode=${cultureCode},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "deployAvailablePackage", "logrite.cultureCode": cultureCode, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");
        sf.post("ParseLanguagePackage?cultureCode=" + cultureCode, {}, callback, errorCallback);
		logger.error(`deployAvailablePackage(cultureCode=${cultureCode},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "deployAvailablePackage", "logrite.cultureCode": cultureCode, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    installPackage(file, callback) {
		logger.error(`>>> Entering installPackage(file=${file},callback=${callback})`, { "event.action": "installPackage", "logrite.file": file, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`installPackage(file=${file},callback=${callback}): sf → ${sf}`, { "event.action": "installPackage", "logrite.file": file, "logrite.callback": callback });
        let formData = new FormData();
		logger.error(`installPackage(file=${file},callback=${callback}): formData → ${formData}`, { "event.action": "installPackage", "logrite.file": file, "logrite.callback": callback });
        formData.append("POSTFILE", file);

        sf.postfile("InstallPackage", formData, callback);
		logger.warn(`<<< Exiting installPackage(file=${file},callback=${callback})`, { "event.action": "installPackage", "logrite.file": file, "logrite.callback": callback });
    }
    createNewModule(payload, callback, errorCallback) {
		logger.error(`>>> Entering createNewModule(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createNewModule", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`createNewModule(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "createNewModule", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.post("CreateModule", payload, callback, errorCallback);
		logger.warn(`<<< Exiting createNewModule(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createNewModule", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getPackageSettings(packageId, callback, errorCallback) {
		logger.error(`>>> Entering getPackageSettings(packageId=${packageId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPackageSettings", "logrite.packageId": packageId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");
        const parameters = {
            siteId: utilities.settings.portalId,
            packageId
        };
        sf.get("GetPackageSettings?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.error(`getPackageSettings(packageId=${packageId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getPackageSettings", "logrite.packageId": packageId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getModuleCategories(callback, errorCallback) {
		logger.error(`>>> Entering getModuleCategories(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getModuleCategories", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("getModuleCategories", {}, callback, errorCallback);
		logger.error(`getModuleCategories(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getModuleCategories", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getDesktopModulePermissions(desktopModuleId, callback, errorCallback) {
		logger.error(`>>> Entering getDesktopModulePermissions(desktopModuleId=${desktopModuleId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getDesktopModulePermissions", "logrite.desktopModuleId": desktopModuleId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetDesktopModulePermissions", { desktopModuleId: desktopModuleId }, callback, errorCallback);
		logger.error(`getDesktopModulePermissions(desktopModuleId=${desktopModuleId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getDesktopModulePermissions", "logrite.desktopModuleId": desktopModuleId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    saveDesktopModulePermissions(permissions, callback, errorCallback) {
		logger.error(`>>> Entering saveDesktopModulePermissions(permissions=${permissions},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "saveDesktopModulePermissions", "logrite.permissions": permissions, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");
        sf.post("saveDesktopModulePermissions", { permissions: permissions }, callback, errorCallback);
		logger.error(`saveDesktopModulePermissions(permissions=${permissions},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "saveDesktopModulePermissions", "logrite.permissions": permissions, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    parseAvailablePackage(FileName, PackageType, callback, errorCallback) {
		logger.error(`>>> Entering parseAvailablePackage(FileName=${FileName},PackageType=${PackageType},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "parseAvailablePackage", "logrite.FileName": FileName, "logrite.PackageType": PackageType, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");
        sf.post("ParsePackageFile", { FileName, PackageType }, callback, errorCallback);
		logger.error(`parseAvailablePackage(FileName=${FileName},PackageType=${PackageType},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "parseAvailablePackage", "logrite.FileName": FileName, "logrite.PackageType": PackageType, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getLocaleList(callback, errorCallback) {
		logger.error(`>>> Entering getLocaleList(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getLocaleList", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetLanguagesList", {}, callback, errorCallback);
		logger.error(`getLocaleList(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getLocaleList", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getLocalePackagesList(callback, errorCallback) {
		logger.error(`>>> Entering getLocalePackagesList(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getLocalePackagesList", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetAllPackagesListExceptLangPacks", {}, callback, errorCallback);
		logger.error(`getLocalePackagesList(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getLocalePackagesList", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getPackageUsageFilter(callback) {
		logger.error(`>>> Entering getPackageUsageFilter(callback=${callback})`, { "event.action": "getPackageUsageFilter", "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetPackageUsageFilter", {}, callback);
		logger.error(`getPackageUsageFilter(callback=${callback}): sf → ${sf}`, { "event.action": "getPackageUsageFilter", "logrite.callback": callback });
    }
    getPackageUsage(portalId, packageId, callback) {
		logger.error(`>>> Entering getPackageUsage(portalId=${portalId},packageId=${packageId},callback=${callback})`, { "event.action": "getPackageUsage", "logrite.portalId": portalId, "logrite.packageId": packageId, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        const parameters = {
            portalId: portalId,
            packageId: packageId
        };
        sf.get("GetPackageUsage?" + serializeQueryStringParameters(parameters), {}, callback);
		logger.error(`getPackageUsage(portalId=${portalId},packageId=${packageId},callback=${callback}): sf → ${sf}`, { "event.action": "getPackageUsage", "logrite.portalId": portalId, "logrite.packageId": packageId, "logrite.callback": callback });
    }
}
const extensionService = new ExtensionService();
export default extensionService;
