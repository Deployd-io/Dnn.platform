import utilities from "../utils";
const logger = console;

function mapPackageInformation(extensionBeingUpdated, addMode) {
	logger.debug(`>>> Entering mapPackageInformation(extensionBeingUpdated=${extensionBeingUpdated},addMode=${addMode})`);
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
        delete _extensionBeingUpdated.License;
        delete _extensionBeingUpdated.ReleaseNotes;
        _extensionBeingUpdated.PackageType = extensionBeingUpdated.packageType.value;
		logger.debug(`mapPackageInformation(extensionBeingUpdated=${extensionBeingUpdated},addMode=${addMode}): _extensionBeingUpdated.PackageType → ${_extensionBeingUpdated.PackageType}`);
    }
	logger.debug(`<<< Exiting mapPackageInformation(extensionBeingUpdated=${extensionBeingUpdated},addMode=${addMode})`);
    return _extensionBeingUpdated;
}

function serializeQueryStringParameters(obj) {
	logger.debug(`>>> Entering serializeQueryStringParameters(obj=${obj})`);
    let s = [];
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.debug(`<<< Exiting serializeQueryStringParameters(obj=${obj})`);
    return s.join("&");
}
class ExtensionService {
    getServiceFramework(controller) {
		logger.debug(`>>> Entering getServiceFramework(controller=${controller})`);
        let sf = utilities.utilities.sf;

        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`);

		logger.debug(`<<< Exiting getServiceFramework(controller=${controller})`);
        return sf;
    }
    getInstalledPackages(type, callback) {
		logger.debug(`>>> Entering getInstalledPackages(type=${type},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetInstalledPackages?packageType=" + type, {}, callback);
		logger.debug(`getInstalledPackages(type=${type},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getInstalledPackages(type=${type},callback=${callback})`);
    }
    getAvailablePackages(type, callback) {
		logger.debug(`>>> Entering getAvailablePackages(type=${type},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetAvailablePackages?packageType=" + type, {}, callback);
		logger.debug(`getAvailablePackages(type=${type},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getAvailablePackages(type=${type},callback=${callback})`);
    }
    getPackageTypes(callback) {
		logger.debug(`>>> Entering getPackageTypes(callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetPackageTypes", {}, callback);
		logger.debug(`getPackageTypes(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPackageTypes(callback=${callback})`);
    }
    getAvailablePackageTypes(callback) {
		logger.debug(`>>> Entering getAvailablePackageTypes(callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetAvailablePackageTypes", {}, callback);
		logger.debug(`getAvailablePackageTypes(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getAvailablePackageTypes(callback=${callback})`);
    }
    updateExtension(extensionBeingUpdated, editorActions, callback) {
		logger.debug(`>>> Entering updateExtension(extensionBeingUpdated=${extensionBeingUpdated},editorActions=${editorActions},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            packageId: extensionBeingUpdated.packageId.value,
            portalId: utilities.settings.portalId,
            settings: mapPackageInformation(extensionBeingUpdated),
            editorActions: editorActions
        };
        sf.post("SavePackageSettings", payload, callback);
		logger.debug(`updateExtension(extensionBeingUpdated=${extensionBeingUpdated},editorActions=${editorActions},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateExtension(extensionBeingUpdated=${extensionBeingUpdated},editorActions=${editorActions},callback=${callback})`);
    }
    createNewExtension(extensionBeingUpdated, editorActions, callback, errorCallback) {
		logger.debug(`>>> Entering createNewExtension(extensionBeingUpdated=${extensionBeingUpdated},editorActions=${editorActions},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            packageId: -1,
            portalId: -1,
            settings: mapPackageInformation(extensionBeingUpdated, true),
            editorActions: editorActions
        };
        sf.post("CreateExtension", payload, callback, errorCallback);
		logger.debug(`createNewExtension(extensionBeingUpdated=${extensionBeingUpdated},editorActions=${editorActions},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting createNewExtension(extensionBeingUpdated=${extensionBeingUpdated},editorActions=${editorActions},callback=${callback},errorCallback=${errorCallback})`);
    }
    downloadPackage(PackageType, FileName, callback) {
		logger.debug(`>>> Entering downloadPackage(PackageType=${PackageType},FileName=${FileName},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            PackageType,
            FileName
        };
        sf.get("DownloadPackage", payload, callback);
		logger.debug(`downloadPackage(PackageType=${PackageType},FileName=${FileName},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting downloadPackage(PackageType=${PackageType},FileName=${FileName},callback=${callback})`);
    }
    installAvailablePackage(PackageType, FileName, callback) {
		logger.debug(`>>> Entering installAvailablePackage(PackageType=${PackageType},FileName=${FileName},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        const payload = {
            PackageType,
            FileName
        };
        sf.post("InstallAvailablePackage", payload, callback);
		logger.debug(`installAvailablePackage(PackageType=${PackageType},FileName=${FileName},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting installAvailablePackage(PackageType=${PackageType},FileName=${FileName},callback=${callback})`);
    }
    deletePackage(payload, callback) {
		logger.debug(`>>> Entering deletePackage(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.post("DeletePackage", payload, callback);
		logger.debug(`deletePackage(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deletePackage(payload=${payload},callback=${callback})`);
    }
    parsePackage(file, callback, errorCallback) {
		logger.debug(`>>> Entering parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
        let formData = new FormData();
		logger.debug(`parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback}): formData → ${formData}`);
        formData.append("POSTFILE", file);


        sf.postfile("ParsePackage", formData, callback, errorCallback);
		logger.debug(`<<< Exiting parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`);
    }
    deployAvailablePackage(cultureCode, callback, errorCallback) {
		logger.debug(`>>> Entering deployAvailablePackage(cultureCode=${cultureCode},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.post("ParseLanguagePackage?cultureCode=" + cultureCode, {}, callback, errorCallback);
		logger.debug(`deployAvailablePackage(cultureCode=${cultureCode},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deployAvailablePackage(cultureCode=${cultureCode},callback=${callback},errorCallback=${errorCallback})`);
    }
    installPackage(file, callback) {
		logger.debug(`>>> Entering installPackage(file=${file},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`installPackage(file=${file},callback=${callback}): sf → ${sf}`);
        let formData = new FormData();
		logger.debug(`installPackage(file=${file},callback=${callback}): formData → ${formData}`);
        formData.append("POSTFILE", file);

        sf.postfile("InstallPackage", formData, callback);
		logger.debug(`<<< Exiting installPackage(file=${file},callback=${callback})`);
    }
    createNewModule(payload, callback, errorCallback) {
		logger.debug(`>>> Entering createNewModule(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`createNewModule(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
        sf.post("CreateModule", payload, callback, errorCallback);
		logger.debug(`<<< Exiting createNewModule(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    }
    getPackageSettings(packageId, callback, errorCallback) {
		logger.debug(`>>> Entering getPackageSettings(packageId=${packageId},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");
        const parameters = {
            siteId: utilities.settings.portalId,
            packageId
        };
        sf.get("GetPackageSettings?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.debug(`getPackageSettings(packageId=${packageId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPackageSettings(packageId=${packageId},callback=${callback},errorCallback=${errorCallback})`);
    }
    getModuleCategories(callback, errorCallback) {
		logger.debug(`>>> Entering getModuleCategories(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("getModuleCategories", {}, callback, errorCallback);
		logger.debug(`getModuleCategories(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getModuleCategories(callback=${callback},errorCallback=${errorCallback})`);
    }
    getDesktopModulePermissions(desktopModuleId, callback, errorCallback) {
		logger.debug(`>>> Entering getDesktopModulePermissions(desktopModuleId=${desktopModuleId},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetDesktopModulePermissions", { desktopModuleId: desktopModuleId }, callback, errorCallback);
		logger.debug(`getDesktopModulePermissions(desktopModuleId=${desktopModuleId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getDesktopModulePermissions(desktopModuleId=${desktopModuleId},callback=${callback},errorCallback=${errorCallback})`);
    }
    saveDesktopModulePermissions(permissions, callback, errorCallback) {
		logger.debug(`>>> Entering saveDesktopModulePermissions(permissions=${permissions},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.post("saveDesktopModulePermissions", { permissions: permissions }, callback, errorCallback);
		logger.debug(`saveDesktopModulePermissions(permissions=${permissions},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting saveDesktopModulePermissions(permissions=${permissions},callback=${callback},errorCallback=${errorCallback})`);
    }
    parseAvailablePackage(FileName, PackageType, callback, errorCallback) {
		logger.debug(`>>> Entering parseAvailablePackage(FileName=${FileName},PackageType=${PackageType},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.post("ParsePackageFile", { FileName, PackageType }, callback, errorCallback);
		logger.debug(`parseAvailablePackage(FileName=${FileName},PackageType=${PackageType},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting parseAvailablePackage(FileName=${FileName},PackageType=${PackageType},callback=${callback},errorCallback=${errorCallback})`);
    }
    getLocaleList(callback, errorCallback) {
		logger.debug(`>>> Entering getLocaleList(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetLanguagesList", {}, callback, errorCallback);
		logger.debug(`getLocaleList(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLocaleList(callback=${callback},errorCallback=${errorCallback})`);
    }
    getLocalePackagesList(callback, errorCallback) {
		logger.debug(`>>> Entering getLocalePackagesList(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetAllPackagesListExceptLangPacks", {}, callback, errorCallback);
		logger.debug(`getLocalePackagesList(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLocalePackagesList(callback=${callback},errorCallback=${errorCallback})`);
    }
    getPackageUsageFilter(callback) {
		logger.debug(`>>> Entering getPackageUsageFilter(callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetPackageUsageFilter", {}, callback);
		logger.debug(`getPackageUsageFilter(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPackageUsageFilter(callback=${callback})`);
    }
    getPackageUsage(portalId, packageId, callback) {
		logger.debug(`>>> Entering getPackageUsage(portalId=${portalId},packageId=${packageId},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        const parameters = {
            portalId: portalId,
            packageId: packageId
        };
        sf.get("GetPackageUsage?" + serializeQueryStringParameters(parameters), {}, callback);
		logger.debug(`getPackageUsage(portalId=${portalId},packageId=${packageId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPackageUsage(portalId=${portalId},packageId=${packageId},callback=${callback})`);
    }
}
const extensionService = new ExtensionService();
export default extensionService;
