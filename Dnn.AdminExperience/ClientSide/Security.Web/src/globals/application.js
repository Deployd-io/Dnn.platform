import utilities from "../utils";
import "../less/style.less";
const logger = console;

const boilerPlate = {
    init() {
		logger.debug('>>> Entering init()');
        let options = window.dnn.initSecurity();
        utilities.init(options.utility);
        utilities.moduleName = options.moduleName;
		logger.debug(`init(): utilities.moduleName → ${utilities.moduleName}`);
        const rawSettings = options && options.settings ? options.settings : {};
		logger.debug('<<< Exiting init()');
        utilities.settings = {
            ...rawSettings,
            isHost: !!rawSettings.isHost,
            isAdmin: !!rawSettings.isAdmin,
            permissions: rawSettings.permissions || {}
        };
    },
    dispatch() {
		logger.debug('>>> Entering dispatch()');
		logger.debug('<<< Exiting dispatch()');
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default boilerPlate;