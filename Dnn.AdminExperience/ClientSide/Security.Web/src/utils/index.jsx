const logger = console;
const dateFormatter = new Intl.DateTimeFormat(window.dnn.utility.getCulture(), { dateStyle: "short" });
const dateTimeFormatter = new Intl.DateTimeFormat(window.dnn.utility.getCulture(), { dateStyle: "full", timeStyle: "long" });

const utils = {
    init(utilities) {
		logger.debug(`>>> Entering init(utilities=${utilities})`);
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;
		logger.debug(`init(utilities=${utilities}): this.utilities → ${this.utilities}`);
		logger.debug(`<<< Exiting init(utilities=${utilities})`);
    },
    utilities: null,
    formatDate(datestringOrNull) {
		logger.debug(`>>> Entering formatDate(datestringOrNull=${datestringOrNull})`);
        if (!datestringOrNull) {
		logger.debug(`<<< Exiting formatDate(datestringOrNull=${datestringOrNull})`);
            return "";
        }
        return dateFormatter.format(new Date(datestringOrNull));
    },
    formatDateTime(datestringOrNull) {
		logger.debug(`>>> Entering formatDateTime(datestringOrNull=${datestringOrNull})`);
        if (!datestringOrNull) {
		logger.debug(`<<< Exiting formatDateTime(datestringOrNull=${datestringOrNull})`);
            return "";
        }
        return dateTimeFormatter.format(new Date(datestringOrNull));
    },
    dateInPast(firstDate, secondDate) {
		logger.debug(`>>> Entering dateInPast(firstDate=${firstDate},secondDate=${secondDate})`);
        if (firstDate.setHours(0, 0, 0, 0) <= secondDate.setHours(0, 0, 0, 0)) {
		logger.debug(`<<< Exiting dateInPast(firstDate=${firstDate},secondDate=${secondDate})`);
            return true;
        }
        return false;
    }
};
export default utils;