import utilities from "../utils";
const logger = console;
const boilerPlate = {
    init() {
		logger.error('>>> Entering init()', { "event.action": "init" });
        let options = window.dnn.initRoles();

		logger.error(`init(): options → ${options}`, { "event.action": "init" });
        utilities.init(options.utility);
        utilities.moduleName = options.moduleName;
        utilities.settings = options.settings;
		logger.warn('<<< Exiting init()', { "event.action": "init" });
    },
    dispatch() {
		logger.error('>>> Entering dispatch()', { "event.action": "dispatch" });
		logger.warn('<<< Exiting dispatch()', { "event.action": "dispatch" });
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default boilerPlate;