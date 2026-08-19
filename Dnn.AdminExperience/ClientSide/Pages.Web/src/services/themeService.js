import Api from "./api";
const logger = console;

const themeService = {
    getThemes() {
		logger.debug('>>> Entering getThemes()'); const _start = Date.now();
        const api = new Api("Pages");
		logger.debug(`getThemes(): api → ${api}`);
		logger.info(`getThemes(): API call took ${Date.now() - _start} ms`);
		logger.debug('<<< Exiting getThemes()');
        return api.get("GetThemes");
    },
    getThemeFiles(themeName, level) {
        const api = new Api("Pages");
		logger.debug(`>>> Entering getThemeFiles(themeName=${themeName},level=${level})`); const _start = Date.now();
		logger.info(`getThemeFiles(themeName=${themeName}, level=${level}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting getThemeFiles(themeName=${themeName},level=${level})`);
        return api.get("GetThemeFiles", {themeName, level});
    }
};
export default themeService;