const logger = console;
const utils = {
    init(utilities) {
		logger.debug(`>>> Entering init(utilities=${utilities})`);
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities.utility;
        this.moduleName = utilities.moduleName;
		logger.debug(`init(utilities=${utilities}): utilities → ${utilities}`);
		logger.debug(`init(utilities=${utilities}): this.utilities → ${this.utilities}`);
		logger.debug(`init(utilities=${utilities}): this.siteRoot → ${this.siteRoot}`);
        this.settings = utilities.settings;
        this.siteRoot = utilities.siteRoot;    
		logger.debug(`<<< Exiting init(utilities=${utilities})`);
    },
    utilities: null,
    moduleName: "",
    settings: {}
};
export default utils;