const logger = console;
function getValidateRequired(key) {
	logger.debug(`>>> Entering getValidateRequired(key=${key})`);
    switch (key) {
        case "friendlyName":
            return true;
        default:
			logger.debug(`<<< Exiting getValidateRequired(key=${key})`);
            return false;
    }
}

function getTabMapping(key) {
	logger.debug(`>>> Entering getTabMapping(key=${key})`);
    switch (key) {
        case "license":
            return 3;
        case "releaseNotes":
            return 4;
        default:
			logger.debug(`<<< Exiting getTabMapping(key=${key})`);
            return 0;
    }
}

export function validationMapExtensionBeingEdited(extensionBeingEdited) {
	logger.debug(`>>> Entering validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`);
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
	logger.debug(`validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited}): _extensionBeingEdited → ${_extensionBeingEdited}`);
	logger.debug(`<<< Exiting validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`);
    return _extensionBeingEdited;
}

export function getVersionDropdownValues() {
	logger.debug('>>> Entering getVersionDropdownValues()');
    let values = [];
    for (let i = 0; i < 100; i++) {
        values.push({
            label: formatVersionNumber(i),
            value: i
        });
    }
	logger.debug('<<< Exiting getVersionDropdownValues()');
    return values;
}
export function formatVersionNumber(n) {
	logger.debug(`>>> Entering formatVersionNumber(n=${n})`);
	logger.debug(`<<< Exiting formatVersionNumber(n=${n})`);
    return n > 9 ? "" + n : "0" + n;
}