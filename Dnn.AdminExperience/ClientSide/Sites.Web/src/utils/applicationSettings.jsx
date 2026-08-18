const logger = console;
const applicationSettings = {
    init(applicationSettings) {
        if (!applicationSettings) {
            this.applicationSettings = {};
        }
        this.applicationSettings = applicationSettings;      
		logger.debug(`init(applicationSettings=${applicationSettings}): this.applicationSettings → ${this.applicationSettings}`);
		logger.debug(`<<< Exiting init(applicationSettings=${applicationSettings})`);
    },
    applicationSettings: null
};
export default applicationSettings;