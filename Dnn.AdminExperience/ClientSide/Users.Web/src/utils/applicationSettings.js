const logger = console;
const applicationSettings = {
    init(applicationSettings) {
		logger.debug(`>>> Entering init(applicationSettings=${applicationSettings})`);
        if (!applicationSettings) {
            this.applicationSettings = {};
        }
        this.applicationSettings = applicationSettings;      
		logger.debug(`<<< Exiting init(applicationSettings=${applicationSettings})`);
    },
    applicationSettings: null
};
export default applicationSettings;