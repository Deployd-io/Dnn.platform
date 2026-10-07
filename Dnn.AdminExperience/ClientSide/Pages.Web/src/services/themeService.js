import Api from "./api";
const logger = console;

const themeService = {
    getThemes() {
		logger.error('>>> Entering getThemes()', { "event.action": "getThemes" });
        const api = new Api("Pages");
		logger.error(`getThemes(): api → ${api}`, { "event.action": "getThemes" });
        return api.get("GetThemes");
    },
    getThemeFiles(themeName, level) {
		logger.error(`>>> Entering getThemeFiles(themeName=${themeName},level=${level})`, { "event.action": "getThemeFiles", "logrite.themeName": themeName, "logrite.level": level });
        const api = new Api("Pages");
		logger.error(`getThemeFiles(themeName=${themeName},level=${level}): api → ${api}`, { "event.action": "getThemeFiles", "logrite.themeName": themeName, "logrite.level": level });
        return api.get("GetThemeFiles", {themeName, level});
    }
};
export default themeService;