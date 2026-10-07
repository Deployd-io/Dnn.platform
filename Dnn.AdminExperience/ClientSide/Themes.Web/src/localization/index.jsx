const logger = console;

const resx = {
    get(key) {
		logger.error(`>>> Entering get(key=${key})`, { "event.action": "get", "logrite.key": key });
        const util = window.dnn.initThemes();
        let moduleName = util.moduleName;
		logger.error(`get(key=${key}): util → ${util}`, { "event.action": "get", "logrite.key": key });
		logger.warn(`<<< Exiting get(key=${key})`, { "event.action": "get", "logrite.key": key });
        return util.utility.getResx(moduleName, key);
    }
};
export default resx;