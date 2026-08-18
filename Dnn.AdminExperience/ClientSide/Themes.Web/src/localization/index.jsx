const logger = console;

const resx = {
    get(key) {
		logger.debug(`>>> Entering get(key=${key})`);
        const util = window.dnn.initThemes();
        let moduleName = util.moduleName;
		logger.debug(`get(key=${key}): util → ${util}`);
		logger.debug(`<<< Exiting get(key=${key})`);
        return util.utility.getResx(moduleName, key);
    }
};
export default resx;