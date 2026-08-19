const logger = console;
let utilities = null;
let config = null;
let initialized = false;
let settings = null;
let moduleName = null;

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
    utilities = options.utilities;
    config = options.config;
    moduleName = options.moduleName;
    settings = options.settings;
    initialized = true;
	logger.debug(`init(options=${options}): settings → ${settings}`);
	logger.debug(`<<< Exiting init(options=${options})`);
}

function formatDateNoTime(date) {
	logger.debug(`>>> Entering formatDateNoTime(date=${date})`);
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
    const dateOptions = { year: "numeric", month: "numeric", day: "numeric" };
	logger.debug(`formatDateNoTime(date=${date}): dateOptions → ${dateOptions}`);
	logger.debug(`<<< Exiting formatDateNoTime(date=${date})`);
    return new Date(date).toLocaleDateString(config.culture, dateOptions);
}

function formatDateAndTime(date) {
	logger.debug(`>>> Entering formatDateAndTime(date=${date})`);
    if (!initialized) {
        throw new Error("Utils have not been initialized");
    }
    const dateOptions = { dateStyle: "short", timeStyle: "short" };
	logger.debug(`formatDateAndTime(date=${date}): dateOptions → ${dateOptions}`);
	logger.debug(`<<< Exiting formatDateAndTime(date=${date})`);
    return new Date(date).toLocaleString(config.culture, dateOptions);
}

function formatNumeric(value) {
	logger.debug(`>>> Entering formatNumeric(value=${value})`);
    if (!initialized) {
	logger.debug(`<<< Exiting formatNumeric(value=${value})`);
        throw new Error("Utils have not been initialized");
    }
    return value.toLocaleString(config.culture);
}

function formatNumeric2Decimals(value) {
	logger.debug(`>>> Entering formatNumeric2Decimals(value=${value})`);
	logger.debug(`<<< Exiting formatNumeric2Decimals(value=${value})`);
    return parseFloat(Math.round(value * 100) / 100).toFixed(2);
}

function notify(message) {
	logger.debug(`>>> Entering notify(message=${message})`);
    if (!initialized) {
	logger.debug(`<<< Exiting notify(message=${message})`);
        throw new Error("Utils have not been initialized");
    }
    return utilities.notify(message);
}

function notifyError(message) {
	logger.debug(`>>> Entering notifyError(message=${message})`);
    if (!initialized) {
	logger.debug(`<<< Exiting notifyError(message=${message})`);
        throw new Error("Utils have not been initialized");
    }
    return utilities.notifyError(message);
}

function confirm(message, confirmText, cancelText, confirmHandler, cancelHandler) {
	logger.debug(`>>> Entering confirm(message=${message},confirmText=${confirmText},cancelText=${cancelText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`);
    if (!initialized) {
	logger.debug(`<<< Exiting confirm(message=${message},confirmText=${confirmText},cancelText=${cancelText},confirmHandler=${confirmHandler},cancelHandler=${cancelHandler})`);
        throw new Error("Utils have not been initialized");
    }
    return utilities.confirm(message, confirmText, cancelText, confirmHandler, cancelHandler);
}

function getResx(key) {
	logger.debug(`>>> Entering getResx(key=${key})`);
    if (!initialized) {
	logger.debug(`<<< Exiting getResx(key=${key})`);
        throw new Error("Utils have not been initialized");
    }
    return utilities.getResx(moduleName, key);
}

function isHostUser() {
	logger.debug('>>> Entering isHostUser()');
    if (!initialized) {
	logger.debug('<<< Exiting isHostUser()');
        throw new Error("Utils have not been initialized");
    }
    return settings.isHost;
}

function getServiceFramework() {
	logger.debug('>>> Entering getServiceFramework()');
    if (!initialized) {
	logger.debug('<<< Exiting getServiceFramework()');
        throw new Error("Utils have not been initialized");
    }
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
