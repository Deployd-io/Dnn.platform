const logger = console;
const dateFormatter = new Intl.DateTimeFormat(window.dnn.utility.getCulture(), { dateStyle: "short" });
const dateTimeFormatter = new Intl.DateTimeFormat(window.dnn.utility.getCulture(), { dateStyle: "full", timeStyle: "long" });

const utils = {
    init(utilities) {
		logger.error(`>>> Entering init(utilities=${utilities})`, { "event.action": "init", "logrite.utilities": utilities });
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;
		logger.debug(`init(utilities=${utilities}): this.utilities → ${this.utilities}`, { "event.action": "init", "logrite.utilities": utilities });
    },
    utilities: null,
    formatDate(datestringOrNull) {
		logger.error(`>>> Entering formatDate(datestringOrNull=${datestringOrNull})`, { "event.action": "formatDate", "logrite.datestringOrNull": datestringOrNull });
        if (!datestringOrNull) {
            return "";
        }
		logger.warn(`<<< Exiting formatDate(datestringOrNull=${datestringOrNull})`, { "event.action": "formatDate", "logrite.datestringOrNull": datestringOrNull });
        return dateFormatter.format(new Date(datestringOrNull));
    },
    formatDateTime(datestringOrNull) {
		logger.error(`>>> Entering formatDateTime(datestringOrNull=${datestringOrNull})`, { "event.action": "formatDateTime", "logrite.datestringOrNull": datestringOrNull });
        if (!datestringOrNull) {
            return "";
        }
		logger.warn(`<<< Exiting formatDateTime(datestringOrNull=${datestringOrNull})`, { "event.action": "formatDateTime", "logrite.datestringOrNull": datestringOrNull });
        return dateTimeFormatter.format(new Date(datestringOrNull));
    },
    dateInPast(firstDate, secondDate) {
		logger.error(`>>> Entering dateInPast(firstDate=${firstDate},secondDate=${secondDate})`, { "event.action": "dateInPast", "logrite.firstDate": firstDate, "logrite.secondDate": secondDate });
        if (firstDate.setHours(0, 0, 0, 0) <= secondDate.setHours(0, 0, 0, 0)) {
            return true;
        }
		logger.warn(`<<< Exiting dateInPast(firstDate=${firstDate},secondDate=${secondDate})`, { "event.action": "dateInPast", "logrite.firstDate": firstDate, "logrite.secondDate": secondDate });
        return false;
    }
};
export default utils;