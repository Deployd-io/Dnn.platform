const logger = console;
const applicationSettings = {
    init(applicationSettings) {
		logger.error(`>>> Entering init(applicationSettings=${applicationSettings})`, { "event.action": "init", "logrite.applicationSettings": applicationSettings });
        if (!applicationSettings) {
            this.applicationSettings = {};
        }
        this.applicationSettings = applicationSettings;      
		logger.debug(`init(applicationSettings=${applicationSettings}): this.applicationSettings → ${this.applicationSettings}`, { "event.action": "init", "logrite.applicationSettings": applicationSettings });
    },
    applicationSettings: null
};
export default applicationSettings;