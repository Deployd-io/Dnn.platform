const logger = console;
const utils = {
    init(options) {
		logger.debug(`>>> Entering init(options=${options})`);
        if (!options || !options.utility) {
            throw new Error("Utilities is undefined.");
        }

        this.utilities = options.utility;
        this.moduleName = options.moduleName;
		logger.debug(`init(options=${options}): this.utilities → ${this.utilities}`);
        this.settings = options.settings;    
		logger.debug(`init(options=${options}): this.settings → ${this.settings}`);
		logger.debug(`<<< Exiting init(options=${options})`);
    },
    canEdit: function () {
        return this.settings.isHost || this.settings.isAdmin || (this.settings.permissions && this.settings.permissions.EDIT === true);
    },
    isHost: function () {
        return this.settings.isHost;
    },
    utilities: null,
    moduleName: null,
    settings: null
};
export default utils;