const logger = console;
const utils = {
    init(options) {
		logger.error(`>>> Entering init(options=${options})`, { "event.action": "init", "logrite.options": options });
        if (!options || !options.utility) {
            throw new Error("Utilities is undefined.");
        }

        this.utilities = options.utility;
        this.moduleName = options.moduleName;
		logger.debug(`init(options=${options}): this.utilities → ${this.utilities}`, { "event.action": "init", "logrite.options": options });
        this.settings = options.settings;    
		logger.debug(`init(options=${options}): this.settings → ${this.settings}`, { "event.action": "init", "logrite.options": options });
    },
    canEdit: function () {
		logger.error('>>> Entering canEdit()', { "event.action": "canEdit" });
		logger.warn('<<< Exiting canEdit()', { "event.action": "canEdit" });
        return this.settings.isHost || this.settings.isAdmin || (this.settings.permissions && this.settings.permissions.EDIT === true);
    },
    isHost: function () {
		logger.error('>>> Entering isHost()', { "event.action": "isHost" });
		logger.warn('<<< Exiting isHost()', { "event.action": "isHost" });
        return this.settings.isHost;
    },
    utilities: null,
    moduleName: null,
    settings: null
};
export default utils;