import utils from "utils";
const logger = console;

class ThemeService {
    getServiceFramework(controller) {
		logger.debug(`>>> Entering getServiceFramework(controller=${controller})`);
        let sf = window.dnn.initThemes().utility.sf;

		logger.debug(`getServiceFramework(controller=${controller}): sf → ${sf}`);
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`);
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`<<< Exiting getServiceFramework(controller=${controller})`);
        return sf;
    }

    getCurrentTheme(callback, errorCallback) {
		logger.debug(`>>> Entering getCurrentTheme(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Themes");
        sf.get("GetCurrentTheme?language=" + utils.utilities.getCulture(), {}, callback, errorCallback);
		logger.debug(`getCurrentTheme(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getCurrentTheme(callback=${callback},errorCallback=${errorCallback})`);
    }

    getThemes(level, callback, errorCallback) {
		logger.debug(`>>> Entering getThemes(level=${level},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Themes");
        sf.get("GetThemes", { level: level }, callback, errorCallback);
		logger.debug(`getThemes(level=${level},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getThemes(level=${level},callback=${callback},errorCallback=${errorCallback})`);
    }

    getThemeFiles(themeName, themeType, themeLevel, callback, errorCallback) {
		logger.debug(`>>> Entering getThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Themes");
        sf.get("GetThemeFiles", { themeName: themeName, type: themeType, level: themeLevel }, callback, errorCallback);
		logger.debug(`getThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback},errorCallback=${errorCallback})`);
    }

    applyTheme(themeFile, scope, callback, errorCallback) {
		logger.debug(`>>> Entering applyTheme(themeFile=${themeFile},scope=${scope},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Themes");
        sf.post("ApplyTheme?language=" + utils.utilities.getCulture(), { themeFile: themeFile, scope: scope }, callback, errorCallback);
		logger.debug(`applyTheme(themeFile=${themeFile},scope=${scope},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting applyTheme(themeFile=${themeFile},scope=${scope},callback=${callback},errorCallback=${errorCallback})`);
    }

    getEditableTokens(callback, errorCallback) {
		logger.debug(`>>> Entering getEditableTokens(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Themes");
        sf.get("GetEditableTokens", {}, callback, errorCallback);
		logger.debug(`getEditableTokens(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getEditableTokens(callback=${callback},errorCallback=${errorCallback})`);
    }

    getEditableSettings(token, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.get("GetEditableSettings", { token: token }, callback, errorCallback);
    }

    getEditableValues(token, setting, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.get("GetEditableValues", { token: token, setting: setting }, callback, errorCallback);
    }

    updateTheme(path, token, setting, value, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.post("UpdateTheme", { path: path, token: token, setting: setting, value: value }, callback, errorCallback);
    }

    parseTheme(themeName, parseType, callback, errorCallback) {
		logger.debug(`>>> Entering parseTheme(themeName=${themeName},parseType=${parseType},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Themes");
        sf.post("ParseTheme", { themeName: themeName, parseType: parseType }, callback, errorCallback);
		logger.debug(`parseTheme(themeName=${themeName},parseType=${parseType},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting parseTheme(themeName=${themeName},parseType=${parseType},callback=${callback},errorCallback=${errorCallback})`);
    }

    restoreTheme(callback, errorCallback) {
		logger.debug(`>>> Entering restoreTheme(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Themes");
        sf.post("RestoreTheme?language=" + utils.utilities.getCulture(), {}, callback, errorCallback);
		logger.debug(`restoreTheme(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting restoreTheme(callback=${callback},errorCallback=${errorCallback})`);
    }

    applyDefaultTheme(themeName, level, callback, errorCallback) {
		logger.debug(`>>> Entering applyDefaultTheme(themeName=${themeName},level=${level},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Themes");
        sf.post("ApplyDefaultTheme?language=" + utils.utilities.getCulture(), { themeName: themeName, level: level }, callback, errorCallback);
		logger.debug(`applyDefaultTheme(themeName=${themeName},level=${level},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting applyDefaultTheme(themeName=${themeName},level=${level},callback=${callback},errorCallback=${errorCallback})`);
    }

    deleteTheme(theme, callback, errorCallback) {
		logger.debug(`>>> Entering deleteTheme(theme=${theme},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Themes");
        sf.post("DeleteThemePackage", theme, callback, errorCallback);
		logger.debug(`deleteTheme(theme=${theme},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteTheme(theme=${theme},callback=${callback},errorCallback=${errorCallback})`);
    }
}

const themeService = new ThemeService();
export default themeService;