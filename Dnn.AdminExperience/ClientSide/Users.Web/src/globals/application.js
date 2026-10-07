import appSettings from "utils/applicationSettings";
import "../less/style.less";
const logger = console;

const usersApplication = {
    init() {
		logger.error('>>> Entering init()', { "event.action": "init" });
        let options = window.dnn.initUsers();
        appSettings.init(options);
		logger.error(`init(): options → ${options}`, { "event.action": "init" });
    },
    dispatch() {
		logger.error('>>> Entering dispatch()', { "event.action": "dispatch" });
		logger.warn('<<< Exiting dispatch()', { "event.action": "dispatch" });
        throw new Error("dispatch method needs to be overwritten from the Redux store");
    }
};


export default usersApplication;