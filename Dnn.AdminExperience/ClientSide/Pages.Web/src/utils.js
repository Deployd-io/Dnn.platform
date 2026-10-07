import UrlParse from "url-parse";
import * as dayjs from "dayjs";
import localizedFormat from "dayjs/plugin/localizedFormat";
const logger = console;
let utilities = null;
let config = null;
let moduleName = null;
let viewName = null;
let viewParams = null;
let initialized = false;
let settings = null;

function init(options) {
	logger.error(`>>> Entering init(options=${options})`, { "event.action": "init", "logrite.options": options });
    if (!options) {
        throw new Error("This method needs to have an options object as an input parameter");
    }
    if (!options.utilities) {
        throw new Error("This method needs to have an options.utilities object as an input parameter");
    }
    if (!options.config) {
        throw new Error("This method needs to have an options.config object as an input parameter");
    }
    if (!options.moduleName) {
        throw new Error("This method needs to have an options.moduleName string as an input parameter");
    }
    utilities = options.utilities;
    config = options.config;
    moduleName = options.moduleName;
    viewName = options.viewName;
    viewParams = options.viewParams;
    initialized = true;
    settings = options.settings;
	logger.warn(`<<< Exiting init(options=${options})`, { "event.action": "init", "logrite.options": options });
}

function load(options) {
	logger.error(`>>> Entering load(options=${options})`, { "event.action": "load", "logrite.options": options });
    viewName = options.viewName;
    viewParams = options.viewParams;
    settings = options.settings;
	logger.warn(`<<< Exiting load(options=${options})`, { "event.action": "load", "logrite.options": options });
}

function checkInit() {
	logger.error('>>> Entering checkInit()', { "event.action": "checkInit" });
    if (!initialized) {
		logger.warn('<<< Exiting checkInit()', { "event.action": "checkInit" });
        throw new Error("Utils have not been initialized");
    }
}

function formatDateNoTime(date) {
	logger.error(`>>> Entering formatDateNoTime(date=${date})`, { "event.action": "formatDateNoTime", "logrite.date": date });
    checkInit();
    const dateOptions = { year: "numeric", month: "numeric", day: "numeric" };
	logger.error(`formatDateNoTime(date=${date}): dateOptions → ${dateOptions}`, { "event.action": "formatDateNoTime", "logrite.date": date });
    return new Date(date).toLocaleDateString(config.culture, dateOptions);
}

function formatNumeric(value) {
	logger.error(`>>> Entering formatNumeric(value=${value})`, { "event.action": "formatNumeric", "logrite.value": value });
    checkInit();
	logger.warn(`<<< Exiting formatNumeric(value=${value})`, { "event.action": "formatNumeric", "logrite.value": value });
    return value.toLocaleString(config.culture);
}

function formatNumeric2Decimals(value) {
	logger.error(`>>> Entering formatNumeric2Decimals(value=${value})`, { "event.action": "formatNumeric2Decimals", "logrite.value": value });
	logger.warn(`<<< Exiting formatNumeric2Decimals(value=${value})`, { "event.action": "formatNumeric2Decimals", "logrite.value": value });
    return parseFloat(Math.round(value * 100) / 100).toFixed(2);
}

function areEqualInvariantCase(a, b) {
	logger.error(`>>> Entering areEqualInvariantCase(a=${a},b=${b})`, { "event.action": "areEqualInvariantCase", "logrite.a": a, "logrite.b": b });
    if (!a || !b) {
        return a === b;
    }
	logger.warn(`<<< Exiting areEqualInvariantCase(a=${a},b=${b})`, { "event.action": "areEqualInvariantCase", "logrite.a": a, "logrite.b": b });
    return a.toLowerCase() === b.toLowerCase();
}

function notify(message, options) {
	logger.error(`>>> Entering notify(message=${message},options=${options})`, { "event.action": "notify", "logrite.message": message, "logrite.options": options });
    checkInit();
	logger.warn(`<<< Exiting notify(message=${message},options=${options})`, { "event.action": "notify", "logrite.message": message, "logrite.options": options });
    return utilities.notify(message, options);
}

function notifyError(message, options) {
	logger.error(`>>> Entering notifyError(message=${message},options=${options})`, { "event.action": "notifyError", "logrite.message": message, "logrite.options": options });
    checkInit();
	logger.warn(`<<< Exiting notifyError(message=${message},options=${options})`, { "event.action": "notifyError", "logrite.message": message, "logrite.options": options });
    return utilities.notifyError(message, options);
}

function confirm(message, confirmText, cancelText, confirmHandler, cancelHandler) {
	logger.error(`>>> Entering confirm(message=${message},confirmText=${confirmText},cancelText=${cancelText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "confirm", "logrite.message": message, "logrite.confirmText": confirmText, "logrite.cancelText": cancelText, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
    checkInit();
	logger.warn(`<<< Exiting confirm(message=${message},confirmText=${confirmText},cancelText=${cancelText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "confirm", "logrite.message": message, "logrite.confirmText": confirmText, "logrite.cancelText": cancelText, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
    return utilities.confirm(message, confirmText, cancelText, confirmHandler, cancelHandler);
}

function getServiceFramework() {
	logger.error('>>> Entering getServiceFramework()', { "event.action": "getServiceFramework" });
    checkInit();
	logger.warn('<<< Exiting getServiceFramework()', { "event.action": "getServiceFramework" });
    return utilities.sf;
}

function getUtilities() {
	logger.error('>>> Entering getUtilities()', { "event.action": "getUtilities" });
    checkInit();
	logger.warn('<<< Exiting getUtilities()', { "event.action": "getUtilities" });
    return utilities;
}

function getModuleName() {
	logger.error('>>> Entering getModuleName()', { "event.action": "getModuleName" });
    checkInit();
	logger.warn('<<< Exiting getModuleName()', { "event.action": "getModuleName" });
    return moduleName;
}

function getCurrentPageId() {
	logger.error('>>> Entering getCurrentPageId()', { "event.action": "getCurrentPageId" });
    checkInit();
	logger.warn('<<< Exiting getCurrentPageId()', { "event.action": "getCurrentPageId" });
    return parseInt(config.tabId);
}

function getSelectedPageId() {
	logger.error('>>> Entering getSelectedPageId()', { "event.action": "getSelectedPageId" });
    checkInit();
	logger.warn('<<< Exiting getSelectedPageId()', { "event.action": "getSelectedPageId" });
    return viewParams && viewParams.pageId ? parseInt(viewParams.pageId) : null;
}

function getViewName() {
	logger.error('>>> Entering getViewName()', { "event.action": "getViewName" });
    checkInit();
	logger.warn('<<< Exiting getViewName()', { "event.action": "getViewName" });
    return viewName;
}

function closePersonaBar(callback) {
	logger.error(`>>> Entering closePersonaBar(callback=${callback})`, { "event.action": "closePersonaBar", "logrite.callback": callback });
    checkInit();
    utilities.closePersonaBar(callback);
	logger.warn(`<<< Exiting closePersonaBar(callback=${callback})`, { "event.action": "closePersonaBar", "logrite.callback": callback });
}

function getViewParams() {
	logger.error('>>> Entering getViewParams()', { "event.action": "getViewParams" });
    checkInit();
	logger.warn('<<< Exiting getViewParams()', { "event.action": "getViewParams" });
    return viewParams;
}

function getResx(moduleName, key) {
	logger.error(`>>> Entering getResx(moduleName=${moduleName},key=${key})`, { "event.action": "getResx", "logrite.moduleName": moduleName, "logrite.key": key });
    checkInit();
	logger.warn(`<<< Exiting getResx(moduleName=${moduleName},key=${key})`, { "event.action": "getResx", "logrite.moduleName": moduleName, "logrite.key": key });
    return utilities.getResx(moduleName, key);
}

function getSiteRoot() {
	logger.error('>>> Entering getSiteRoot()', { "event.action": "getSiteRoot" });
    checkInit();
	logger.warn('<<< Exiting getSiteRoot()', { "event.action": "getSiteRoot" });
    return config.siteRoot;
}

function getPortalName() {
	logger.error('>>> Entering getPortalName()', { "event.action": "getPortalName" });
    checkInit();
	logger.warn('<<< Exiting getPortalName()', { "event.action": "getPortalName" });
    return settings && settings.portalName;
}

function getTemplateFolder() {
	logger.error('>>> Entering getTemplateFolder()', { "event.action": "getTemplateFolder" });
    checkInit();
	logger.warn('<<< Exiting getTemplateFolder()', { "event.action": "getTemplateFolder" });
    return settings.templateFolder;
}

function getIsSuperUser() {
	logger.error('>>> Entering getIsSuperUser()', { "event.action": "getIsSuperUser" });
    checkInit();
	logger.warn('<<< Exiting getIsSuperUser()', { "event.action": "getIsSuperUser" });
    return settings.isHost || settings.isAdmin;
}

function canSeePagesList() {
	logger.error('>>> Entering canSeePagesList()', { "event.action": "canSeePagesList" });
    checkInit();
	logger.warn('<<< Exiting canSeePagesList()', { "event.action": "canSeePagesList" });
    return settings.isHost || settings.isAdmin || settings.canSeePagesList;
}

function canAddPages() {
	logger.error('>>> Entering canAddPages()', { "event.action": "canAddPages" });
    checkInit();
	logger.warn('<<< Exiting canAddPages()', { "event.action": "canAddPages" });
    return settings.isHost || settings.isAdmin || settings.canAddPages;
}

function getCurrentPagePermissions() {
	logger.error('>>> Entering getCurrentPagePermissions()', { "event.action": "getCurrentPagePermissions" });
    checkInit();
	logger.warn('<<< Exiting getCurrentPagePermissions()', { "event.action": "getCurrentPagePermissions" });
    return settings.currentPagePermissions;
}

function getCurrentParentHasChildren() {
	logger.error('>>> Entering getCurrentParentHasChildren()', { "event.action": "getCurrentParentHasChildren" });
    checkInit();
	logger.warn('<<< Exiting getCurrentParentHasChildren()', { "event.action": "getCurrentParentHasChildren" });
    return settings.currentParentHasChildren;
}

function getCurrentPageName() {
	logger.error('>>> Entering getCurrentPageName()', { "event.action": "getCurrentPageName" });
    checkInit();
	logger.warn('<<< Exiting getCurrentPageName()', { "event.action": "getCurrentPageName" });
    return settings.currentPageName;
}

function getDefaultPageUrl() {
	logger.error('>>> Entering getDefaultPageUrl()', { "event.action": "getDefaultPageUrl" });
    checkInit();
	logger.warn('<<< Exiting getDefaultPageUrl()', { "event.action": "getDefaultPageUrl" });
    return config.siteRoot;
}
function getProductSKU() {
	logger.error('>>> Entering getProductSKU()', { "event.action": "getProductSKU" });
    checkInit();
	logger.warn('<<< Exiting getProductSKU()', { "event.action": "getProductSKU" });
    return settings.productSKU;
}
function isPlatform() {
	logger.error('>>> Entering isPlatform()', { "event.action": "isPlatform" });
    checkInit();
	logger.warn('<<< Exiting isPlatform()', { "event.action": "isPlatform" });
    return settings.productSKU.toLowerCase() === "dnn";
}
function getIsAdminHostSystemPage() {
	logger.error('>>> Entering getIsAdminHostSystemPage()', { "event.action": "getIsAdminHostSystemPage" });
    checkInit();
	logger.warn('<<< Exiting getIsAdminHostSystemPage()', { "event.action": "getIsAdminHostSystemPage" });
    return settings.isAdminHostSystemPage;
}
function formatDate(dateValue, longformat) {
	logger.error(`>>> Entering formatDate(dateValue=${dateValue},longformat=${longformat})`, { "event.action": "formatDate", "logrite.dateValue": dateValue, "logrite.longformat": longformat });
    if (!dateValue) {
        return "";
    }
    let date = new Date(dateValue);
	logger.error(`formatDate(dateValue=${dateValue},longformat=${longformat}): date → ${date}`, { "event.action": "formatDate", "logrite.dateValue": dateValue, "logrite.longformat": longformat });
    let yearValue = date.getFullYear();
	logger.error(`formatDate(dateValue=${dateValue},longformat=${longformat}): yearValue → ${yearValue}`, { "event.action": "formatDate", "logrite.dateValue": dateValue, "logrite.longformat": longformat });
    if (yearValue < 1900) {
        return "-";
    }

    dayjs.extend(localizedFormat);
    // eslint-disable-next-line no-undef
    require("dayjs/locale/" + utilities.getCulture().substring(0,2));
	logger.warn(`<<< Exiting formatDate(dateValue=${dateValue},longformat=${longformat})`, { "event.action": "formatDate", "logrite.dateValue": dateValue, "logrite.longformat": longformat });
    return dayjs(dateValue).locale(utilities.getCulture().substring(0,2)).format(longformat === true ? "LLL" : "L");
}
function getUserMode() {
	logger.error('>>> Entering getUserMode()', { "event.action": "getUserMode" });
	logger.warn('<<< Exiting getUserMode()', { "event.action": "getUserMode" });
    return config.userMode;
}
const url = {
    appendQueryString: function (url, params) {
		logger.error(`>>> Entering appendQueryString(url=${url},params=${params})`, { "event.action": "appendQueryString", "logrite.url": url, "logrite.params": params });
        let urlParse = new UrlParse(url, true);
        let newParams = Object.assign({}, urlParse.query, params);
		logger.error(`appendQueryString(url=${url},params=${params}): urlParse → ${urlParse}`, { "event.action": "appendQueryString", "logrite.url": url, "logrite.params": params });
		logger.error(`appendQueryString(url=${url},params=${params}): newParams → ${newParams}`, { "event.action": "appendQueryString", "logrite.url": url, "logrite.params": params });
        urlParse.set("query", newParams);
		logger.warn(`<<< Exiting appendQueryString(url=${url},params=${params})`, { "event.action": "appendQueryString", "logrite.url": url, "logrite.params": params });
        return urlParse.href;
    }
};
const utils = {
    init,
    load,
    formatDateNoTime,
    formatNumeric,
    formatNumeric2Decimals,
    notify,
    notifyError,
    confirm,
    getServiceFramework,
    getUtilities,
    getModuleName,
    getCurrentPageId,
    getSelectedPageId,
    getViewName,
    closePersonaBar,
    getViewParams,
    getResx,
    getSiteRoot,
    areEqualInvariantCase,
    getPortalName,
    getTemplateFolder,
    getIsSuperUser,
    canSeePagesList,
    canAddPages,
    getCurrentPagePermissions,
    getCurrentParentHasChildren,
    getCurrentPageName,
    getDefaultPageUrl,
    getProductSKU,
    isPlatform,
    getIsAdminHostSystemPage,
    formatDate,
    getUserMode,
    url
};

export default utils;
