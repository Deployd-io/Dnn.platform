const logger = console;
const utils = {
    init(utilities) {
		logger.error(`>>> Entering init(utilities=${utilities})`, { "event.action": "init", "logrite.utilities": utilities });
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities.utility;
        this.moduleName = utilities.moduleName;
		logger.debug(`init(utilities=${utilities}): utilities → ${utilities}`, { "event.action": "init", "logrite.utilities": utilities });
		logger.debug(`init(utilities=${utilities}): this.utilities → ${this.utilities}`, { "event.action": "init", "logrite.utilities": utilities });
		logger.debug(`init(utilities=${utilities}): this.siteRoot → ${this.siteRoot}`, { "event.action": "init", "logrite.utilities": utilities });
        this.settings = utilities.settings;
        this.siteRoot = utilities.siteRoot;    
    },
    utilities: null,
    moduleName: "",
    settings: {}
};
export default utils;