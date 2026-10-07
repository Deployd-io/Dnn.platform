const logger = console;
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

    isPlatform() {       
		logger.error('>>> Entering isPlatform()', { "event.action": "isPlatform" });
		logger.warn('<<< Exiting isPlatform()', { "event.action": "isPlatform" });
        return window.dnn.utility.getSKU().toLowerCase() === "dnn";
    }
};
export default utils;