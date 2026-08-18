import appSettings from "utils/applicationSettings";
import "../less/style.less";
const logger = console;

const usersApplication = {
    init() {
		logger.debug('>>> Entering init()');
        let options = window.dnn.initUsers();
        appSettings.init(options);
		logger.debug('<<< Exiting init()');
    },
    dispatch() {
		logger.debug('>>> Entering dispatch()');
		logger.debug('<<< Exiting dispatch()');
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default usersApplication;