import utils from "utils";
const logger = console;

class ThemeService {
    getServiceFramework(controller) {
		logger.error(`>>> Entering getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        let sf = window.dnn.initThemes().utility.sf;

		logger.error(`getServiceFramework(controller=${controller}): sf → ${sf}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;
		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }

    getCurrentTheme(callback, errorCallback) {
		logger.error(`>>> Entering getCurrentTheme(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getCurrentTheme", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Themes");
        sf.get("GetCurrentTheme?language=" + utils.utilities.getCulture(), {}, callback, errorCallback);
		logger.error(`getCurrentTheme(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getCurrentTheme", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getThemes(level, callback, errorCallback) {
		logger.error(`>>> Entering getThemes(level=${level},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getThemes", "logrite.level": level, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Themes");
        sf.get("GetThemes", { level: level }, callback, errorCallback);
		logger.error(`getThemes(level=${level},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getThemes", "logrite.level": level, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getThemeFiles(themeName, themeType, themeLevel, callback, errorCallback) {
		logger.error(`>>> Entering getThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getThemeFiles", "logrite.themeName": themeName, "logrite.themeType": themeType, "logrite.themeLevel": themeLevel, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Themes");
        sf.get("GetThemeFiles", { themeName: themeName, type: themeType, level: themeLevel }, callback, errorCallback);
		logger.error(`getThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getThemeFiles", "logrite.themeName": themeName, "logrite.themeType": themeType, "logrite.themeLevel": themeLevel, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    applyTheme(themeFile, scope, callback, errorCallback) {
		logger.error(`>>> Entering applyTheme(themeFile=${themeFile},scope=${scope},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "applyTheme", "logrite.themeFile": themeFile, "logrite.scope": scope, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Themes");
        sf.post("ApplyTheme?language=" + utils.utilities.getCulture(), { themeFile: themeFile, scope: scope }, callback, errorCallback);
		logger.error(`applyTheme(themeFile=${themeFile},scope=${scope},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "applyTheme", "logrite.themeFile": themeFile, "logrite.scope": scope, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getEditableTokens(callback, errorCallback) {
		logger.error(`>>> Entering getEditableTokens(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getEditableTokens", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Themes");
        sf.get("GetEditableTokens", {}, callback, errorCallback);
		logger.error(`getEditableTokens(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getEditableTokens", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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
		logger.error(`>>> Entering parseTheme(themeName=${themeName},parseType=${parseType},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "parseTheme", "logrite.themeName": themeName, "logrite.parseType": parseType, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Themes");
        sf.post("ParseTheme", { themeName: themeName, parseType: parseType }, callback, errorCallback);
		logger.error(`parseTheme(themeName=${themeName},parseType=${parseType},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "parseTheme", "logrite.themeName": themeName, "logrite.parseType": parseType, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    restoreTheme(callback, errorCallback) {
		logger.error(`>>> Entering restoreTheme(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "restoreTheme", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Themes");
        sf.post("RestoreTheme?language=" + utils.utilities.getCulture(), {}, callback, errorCallback);
		logger.error(`restoreTheme(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "restoreTheme", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    applyDefaultTheme(themeName, level, callback, errorCallback) {
		logger.error(`>>> Entering applyDefaultTheme(themeName=${themeName},level=${level},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "applyDefaultTheme", "logrite.themeName": themeName, "logrite.level": level, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Themes");
        sf.post("ApplyDefaultTheme?language=" + utils.utilities.getCulture(), { themeName: themeName, level: level }, callback, errorCallback);
		logger.error(`applyDefaultTheme(themeName=${themeName},level=${level},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "applyDefaultTheme", "logrite.themeName": themeName, "logrite.level": level, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    deleteTheme(theme, callback, errorCallback) {
		logger.error(`>>> Entering deleteTheme(theme=${theme},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "deleteTheme", "logrite.theme": theme, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Themes");
        sf.post("DeleteThemePackage", theme, callback, errorCallback);
		logger.error(`deleteTheme(theme=${theme},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "deleteTheme", "logrite.theme": theme, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
}

const themeService = new ThemeService();
export default themeService;