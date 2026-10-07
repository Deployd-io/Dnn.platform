const logger = console;
function getValidateRequired(key) {
	logger.error(`>>> Entering getValidateRequired(key=${key})`, { "event.action": "getValidateRequired", "logrite.key": key });
    switch (key) {
        case "friendlyName":
            return true;
        default:
			logger.warn(`<<< Exiting getValidateRequired(key=${key})`, { "event.action": "getValidateRequired", "logrite.key": key });
            return false;
    }
}

function getTabMapping(key) {
	logger.error(`>>> Entering getTabMapping(key=${key})`, { "event.action": "getTabMapping", "logrite.key": key });
    switch (key) {
        case "license":
            return 3;
        case "releaseNotes":
            return 4;
        default:
			logger.warn(`<<< Exiting getTabMapping(key=${key})`, { "event.action": "getTabMapping", "logrite.key": key });
            return 0;
    }
}

export function validationMapExtensionBeingEdited(extensionBeingEdited) {
	logger.error(`>>> Entering validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`, { "event.action": "validationMapExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited });
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        let validateRequired = getValidateRequired(key);
        let tabMapping = getTabMapping(key);
        _extensionBeingEdited[key] = {
            value: _extensionBeingEdited[key] && !Object.prototype.hasOwnProperty.call(_extensionBeingEdited[key], "value") ? _extensionBeingEdited[key] : _extensionBeingEdited[key] && _extensionBeingEdited[key].value,
            validateRequired,
            tabMapping,
            error: false
        };
    });
	logger.error(`validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited}): _extensionBeingEdited → ${_extensionBeingEdited}`, { "event.action": "validationMapExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited });
    return _extensionBeingEdited;
}

export function getVersionDropdownValues() {
	logger.error('>>> Entering getVersionDropdownValues()', { "event.action": "getVersionDropdownValues" });
    let values = [];
    for (let i = 0; i < 100; i++) {
        values.push({
            label: formatVersionNumber(i),
            value: i
        });
    }
	logger.warn('<<< Exiting getVersionDropdownValues()', { "event.action": "getVersionDropdownValues" });
    return values;
}
export function formatVersionNumber(n) {
	logger.error(`>>> Entering formatVersionNumber(n=${n})`, { "event.action": "formatVersionNumber", "logrite.n": n });
	logger.warn(`<<< Exiting formatVersionNumber(n=${n})`, { "event.action": "formatVersionNumber", "logrite.n": n });
    return n > 9 ? "" + n : "0" + n;
}