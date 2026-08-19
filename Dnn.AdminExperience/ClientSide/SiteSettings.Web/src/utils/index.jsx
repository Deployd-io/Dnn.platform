const logger = console;
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

    isPlatform() {       
		logger.debug('>>> Entering isPlatform()');
		logger.debug('<<< Exiting isPlatform()');
        return window.dnn.utility.getSKU().toLowerCase() === "dnn";
    }
};
export default utils;