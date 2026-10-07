const logger = console;
let utilities = null;
let config = null;
let initialized = false;
let settings = null;
let moduleName = null;

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
    utilities = options.utilities;
    config = options.config;
    moduleName = options.moduleName;
    settings = options.settings;
    initialized = true;
	logger.warn(`<<< Exiting init(options=${options})`, { "event.action": "init", "logrite.options": options });
}

function formatDateNoTime(date) {
	logger.error(`>>> Entering formatDateNoTime(date=${date})`, { "event.action": "formatDateNoTime", "logrite.date": date });
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
    const dateOptions = { year: "numeric", month: "numeric", day: "numeric" };
	logger.error(`formatDateNoTime(date=${date}): dateOptions → ${dateOptions}`, { "event.action": "formatDateNoTime", "logrite.date": date });
    return new Date(date).toLocaleDateString(config.culture, dateOptions);
}

function formatDateAndTime(date) {
	logger.error(`>>> Entering formatDateAndTime(date=${date})`, { "event.action": "formatDateAndTime", "logrite.date": date });
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
    const dateOptions = { dateStyle: "short", timeStyle: "short" };
	logger.error(`formatDateAndTime(date=${date}): dateOptions → ${dateOptions}`, { "event.action": "formatDateAndTime", "logrite.date": date });
    return new Date(date).toLocaleString(config.culture, dateOptions);
}

function formatNumeric(value) {
	logger.error(`>>> Entering formatNumeric(value=${value})`, { "event.action": "formatNumeric", "logrite.value": value });
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
	logger.warn(`<<< Exiting formatNumeric(value=${value})`, { "event.action": "formatNumeric", "logrite.value": value });
    return value.toLocaleString(config.culture);
}

function formatNumeric2Decimals(value) {
	logger.error(`>>> Entering formatNumeric2Decimals(value=${value})`, { "event.action": "formatNumeric2Decimals", "logrite.value": value });
	logger.warn(`<<< Exiting formatNumeric2Decimals(value=${value})`, { "event.action": "formatNumeric2Decimals", "logrite.value": value });
    return parseFloat(Math.round(value * 100) / 100).toFixed(2);
}

function notify(message) {
	logger.error(`>>> Entering notify(message=${message})`, { "event.action": "notify", "logrite.message": message });
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
	logger.warn(`<<< Exiting notify(message=${message})`, { "event.action": "notify", "logrite.message": message });
    return utilities.notify(message);
}

function notifyError(message) {
	logger.error(`>>> Entering notifyError(message=${message})`, { "event.action": "notifyError", "logrite.message": message });
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
	logger.warn(`<<< Exiting notifyError(message=${message})`, { "event.action": "notifyError", "logrite.message": message });
    return utilities.notifyError(message);
}

function confirm(message, confirmText, cancelText, confirmHandler, cancelHandler) {
	logger.error(`>>> Entering confirm(message=${message},confirmText=${confirmText},cancelText=${cancelText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "confirm", "logrite.message": message, "logrite.confirmText": confirmText, "logrite.cancelText": cancelText, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
	logger.warn(`<<< Exiting confirm(message=${message},confirmText=${confirmText},cancelText=${cancelText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`, { "event.action": "confirm", "logrite.message": message, "logrite.confirmText": confirmText, "logrite.cancelText": cancelText, "logrite.confirmHandler": confirmHandler, "logrite.cancelHandler": cancelHandler });
    return utilities.confirm(message, confirmText, cancelText, confirmHandler, cancelHandler);
}

function getResx(key) {
	logger.error(`>>> Entering getResx(key=${key})`, { "event.action": "getResx", "logrite.key": key });
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
	logger.warn(`<<< Exiting getResx(key=${key})`, { "event.action": "getResx", "logrite.key": key });
    return utilities.getResx(moduleName, key);
}

function isHostUser() {
	logger.error('>>> Entering isHostUser()', { "event.action": "isHostUser" });
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
	logger.warn('<<< Exiting isHostUser()', { "event.action": "isHostUser" });
    return settings.isHost;
}

function getServiceFramework() {
	logger.error('>>> Entering getServiceFramework()', { "event.action": "getServiceFramework" });
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
	logger.warn('<<< Exiting getServiceFramework()', { "event.action": "getServiceFramework" });
    return utilities.sf;
}

const utils = {
    init,
    formatDateNoTime,
    formatDateAndTime,
    formatNumeric,
    formatNumeric2Decimals,
    notify,
    notifyError,
    getResx,
    isHostUser,
    confirm,
    getServiceFramework
};

export default utils;
