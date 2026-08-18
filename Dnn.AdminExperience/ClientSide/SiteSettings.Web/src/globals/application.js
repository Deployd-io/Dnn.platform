import utilities from "../utils";
import "../less/style.less";
const logger = console;

const boilerPlate = {
    init() {
		logger.debug('>>> Entering init()');
        let options = window.dnn.initSiteSettings();

		logger.debug(`init(): options → ${options}`);
        utilities.init(options.utility);
        utilities.moduleName = options.moduleName;
		logger.debug(`init(): utilities.moduleName → ${utilities.moduleName}`);
		logger.debug(`init(): utilities.identifier → ${utilities.identifier}`);
		logger.debug(`init(): utilities.siteRoot → ${utilities.siteRoot}`);
        utilities.settings = options.settings;
		logger.debug(`init(): utilities.settings → ${utilities.settings}`);
        utilities.identifier = options.identifier;
        utilities.siteRoot = options.siteRoot;

        if (!window.dnn.SiteSettings) {
            window.dnn.SiteSettings = {};
        }

        window.dnn.SiteSettings.bundleLoaded = true;
		logger.debug('<<< Exiting init()');
    },
    dispatch() {
		logger.debug('>>> Entering dispatch()');
		logger.debug('<<< Exiting dispatch()');
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default boilerPlate;