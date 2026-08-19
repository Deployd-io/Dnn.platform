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
	logger.debug(`>>> Entering init(options=${options})`);
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
	logger.debug(`init(options=${options}): initialized → ${initialized}`);
	logger.debug(`<<< Exiting init(options=${options})`);
}

function load(options) {
	logger.debug(`>>> Entering load(options=${options})`);
    viewName = options.viewName;
    viewParams = options.viewParams;
    settings = options.settings;
	logger.debug(`load(options=${options}): viewParams → ${viewParams}`);
	logger.debug(`<<< Exiting load(options=${options})`);
}

function checkInit() {
	logger.debug('>>> Entering checkInit()');
    if (!initialized) {
		logger.debug('<<< Exiting checkInit()');
        throw new Error("Utils have not been initialized");
    }
}

function formatDateNoTime(date) {
	logger.debug(`>>> Entering formatDateNoTime(date=${date})`);
    checkInit();
    const dateOptions = { year: "numeric", month: "numeric", day: "numeric" };
	logger.debug(`formatDateNoTime(date=${date}): dateOptions → ${dateOptions}`);
	logger.debug(`<<< Exiting formatDateNoTime(date=${date})`);
    return new Date(date).toLocaleDateString(config.culture, dateOptions);
}

function formatNumeric(value) {
	logger.debug(`>>> Entering formatNumeric(value=${value})`);
    checkInit();
	logger.debug(`<<< Exiting formatNumeric(value=${value})`);
    return value.toLocaleString(config.culture);
}

function formatNumeric2Decimals(value) {
	logger.debug(`>>> Entering formatNumeric2Decimals(value=${value})`);
	logger.debug(`<<< Exiting formatNumeric2Decimals(value=${value})`);
    return parseFloat(Math.round(value * 100) / 100).toFixed(2);
}

function areEqualInvariantCase(a, b) {
	logger.debug(`>>> Entering areEqualInvariantCase(a=${a},b=${b})`);
    if (!a || !b) {
	logger.debug(`<<< Exiting areEqualInvariantCase(a=${a},b=${b})`);
        return a === b;
    }
    return a.toLowerCase() === b.toLowerCase();
}

function notify(message, options) {
	logger.debug(`>>> Entering notify(message=${message},options=${options})`);
    checkInit();
	logger.debug(`<<< Exiting notify(message=${message},options=${options})`);
    return utilities.notify(message, options);
}

function notifyError(message, options) {
	logger.debug(`>>> Entering notifyError(message=${message},options=${options})`);
    checkInit();
	logger.debug(`<<< Exiting notifyError(message=${message},options=${options})`);
    return utilities.notifyError(message, options);
}

function confirm(message, confirmText, cancelText, confirmHandler, cancelHandler) {
	logger.debug(`>>> Entering confirm(message=${message},confirmText=${confirmText},cancelText=${cancelText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`);
    checkInit();
	logger.debug(`<<< Exiting confirm(message=${message},confirmText=${confirmText},cancelText=${cancelText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`);
    return utilities.confirm(message, confirmText, cancelText, confirmHandler, cancelHandler);
}

function getServiceFramework() {
	logger.debug('>>> Entering getServiceFramework()');
    checkInit();
	logger.debug('<<< Exiting getServiceFramework()');
    return utilities.sf;
}

function getUtilities() {
	logger.debug('>>> Entering getUtilities()');
    checkInit();
	logger.debug('<<< Exiting getUtilities()');
    return utilities;
}

function getModuleName() {
	logger.debug('>>> Entering getModuleName()');
    checkInit();
	logger.debug('<<< Exiting getModuleName()');
    return moduleName;
}

function getCurrentPageId() {
	logger.debug('>>> Entering getCurrentPageId()');
    checkInit();
	logger.debug('<<< Exiting getCurrentPageId()');
    return parseInt(config.tabId);
}

function getSelectedPageId() {
	logger.debug('>>> Entering getSelectedPageId()');
    checkInit();
	logger.debug('<<< Exiting getSelectedPageId()');
    return viewParams && viewParams.pageId ? parseInt(viewParams.pageId) : null;
}

function getViewName() {
	logger.debug('>>> Entering getViewName()');
    checkInit();
	logger.debug('<<< Exiting getViewName()');
    return viewName;
}

function closePersonaBar(callback) {
	logger.debug(`>>> Entering closePersonaBar(callback=${callback})`);
    checkInit();
    utilities.closePersonaBar(callback);
	logger.debug(`<<< Exiting closePersonaBar(callback=${callback})`);
}

function getViewParams() {
	logger.debug('>>> Entering getViewParams()');
    checkInit();
	logger.debug('<<< Exiting getViewParams()');
    return viewParams;
}

function getResx(moduleName, key) {
	logger.debug(`>>> Entering getResx(moduleName=${moduleName},key=${key})`);
    checkInit();
	logger.debug(`<<< Exiting getResx(moduleName=${moduleName},key=${key})`);
    return utilities.getResx(moduleName, key);
}

function getSiteRoot() {
	logger.debug('>>> Entering getSiteRoot()');
    checkInit();
	logger.debug('<<< Exiting getSiteRoot()');
    return config.siteRoot;
}

function getPortalName() {
	logger.debug('>>> Entering getPortalName()');
    checkInit();
	logger.debug('<<< Exiting getPortalName()');
    return settings && settings.portalName;
}

function getTemplateFolder() {
	logger.debug('>>> Entering getTemplateFolder()');
    checkInit();
	logger.debug('<<< Exiting getTemplateFolder()');
    return settings.templateFolder;
}

function getIsSuperUser() {
	logger.debug('>>> Entering getIsSuperUser()');
    checkInit();
	logger.debug('<<< Exiting getIsSuperUser()');
    return settings.isHost || settings.isAdmin;
}

function canSeePagesList() {
	logger.debug('>>> Entering canSeePagesList()');
    checkInit();
	logger.debug('<<< Exiting canSeePagesList()');
    return settings.isHost || settings.isAdmin || settings.canSeePagesList;
}

function canAddPages() {
	logger.debug('>>> Entering canAddPages()');
    checkInit();
	logger.debug('<<< Exiting canAddPages()');
    return settings.isHost || settings.isAdmin || settings.canAddPages;
}

function getCurrentPagePermissions() {
	logger.debug('>>> Entering getCurrentPagePermissions()');
    checkInit();
	logger.debug('<<< Exiting getCurrentPagePermissions()');
    return settings.currentPagePermissions;
}

function getCurrentParentHasChildren() {
	logger.debug('>>> Entering getCurrentParentHasChildren()');
    checkInit();
	logger.debug('<<< Exiting getCurrentParentHasChildren()');
    return settings.currentParentHasChildren;
}

function getCurrentPageName() {
	logger.debug('>>> Entering getCurrentPageName()');
    checkInit();
	logger.debug('<<< Exiting getCurrentPageName()');
    return settings.currentPageName;
}

function getDefaultPageUrl() {
	logger.debug('>>> Entering getDefaultPageUrl()');
    checkInit();
	logger.debug('<<< Exiting getDefaultPageUrl()');
    return config.siteRoot;
}
function getProductSKU() {
	logger.debug('>>> Entering getProductSKU()');
    checkInit();
	logger.debug('<<< Exiting getProductSKU()');
    return settings.productSKU;
}
function isPlatform() {
	logger.debug('>>> Entering isPlatform()');
    checkInit();
	logger.debug('<<< Exiting isPlatform()');
    return settings.productSKU.toLowerCase() === "dnn";
}
function getIsAdminHostSystemPage() {
	logger.debug('>>> Entering getIsAdminHostSystemPage()');
    checkInit();
	logger.debug('<<< Exiting getIsAdminHostSystemPage()');
    return settings.isAdminHostSystemPage;
}
function formatDate(dateValue, longformat) {
	logger.debug(`>>> Entering formatDate(dateValue=${dateValue},longformat=${longformat})`);
    if (!dateValue) {
        return "";
    }
    let date = new Date(dateValue);
	logger.debug(`formatDate(dateValue=${dateValue},longformat=${longformat}): date → ${date}`);
    let yearValue = date.getFullYear();
	logger.debug(`formatDate(dateValue=${dateValue},longformat=${longformat}): yearValue → ${yearValue}`);
    if (yearValue < 1900) {
        return "-";
    }

    dayjs.extend(localizedFormat);
    // eslint-disable-next-line no-undef
    require("dayjs/locale/" + utilities.getCulture().substring(0,2));
	logger.debug(`<<< Exiting formatDate(dateValue=${dateValue},longformat=${longformat})`);
    return dayjs(dateValue).locale(utilities.getCulture().substring(0,2)).format(longformat === true ? "LLL" : "L");
}
function getUserMode() {
	logger.debug('>>> Entering getUserMode()');
	logger.debug('<<< Exiting getUserMode()');
    return config.userMode;
}
const url = {
    appendQueryString: function (url, params) {
		logger.debug(`>>> Entering appendQueryString(url=${url},params=${params})`);
        let urlParse = new UrlParse(url, true);
        let newParams = Object.assign({}, urlParse.query, params);
        urlParse.set("query", newParams);
		logger.debug(`appendQueryString(url=${url},params=${params}): newParams → ${newParams}`);
		logger.debug(`appendQueryString(url=${url},params=${params}): urlParse → ${urlParse}`);
		logger.debug(`<<< Exiting appendQueryString(url=${url},params=${params})`);
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
