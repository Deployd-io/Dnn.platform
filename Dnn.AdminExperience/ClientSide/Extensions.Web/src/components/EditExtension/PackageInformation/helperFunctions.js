const logger = console;
export function formatVersionNumber(n) {
	logger.error(`>>> Entering formatVersionNumber(n=${n})`, { "event.action": "formatVersionNumber", "logrite.n": n });
	logger.warn(`<<< Exiting formatVersionNumber(n=${n})`, { "event.action": "formatVersionNumber", "logrite.n": n });
    return n > 9 ? "" + n : "0" + n;
}

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

export function validationMapExtensionBeingEdited(extensionBeingEdited) {
	logger.error(`>>> Entering validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`, { "event.action": "validationMapExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited });
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        let validateRequired = getValidateRequired(key);
        _extensionBeingEdited[key] = {
            value: typeof _extensionBeingEdited[key] !== "object" ? _extensionBeingEdited[key] : _extensionBeingEdited[key].value,
            validateRequired,
            error: false
        };
    });
	logger.error(`validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited}): _extensionBeingEdited → ${_extensionBeingEdited}`, { "event.action": "validationMapExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited });
    return _extensionBeingEdited;
}

export function valueMapExtensionBeingEdited(extensionBeingEdited) {
	logger.error(`>>> Entering valueMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`, { "event.action": "valueMapExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited });
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        _extensionBeingEdited[key] = _extensionBeingEdited[key].value;
    });
	logger.error(`valueMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited}): _extensionBeingEdited → ${_extensionBeingEdited}`, { "event.action": "valueMapExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited });
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