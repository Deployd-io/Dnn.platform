const logger = console;
export function formatVersionNumber(n) {
	logger.debug(`>>> Entering formatVersionNumber(n=${n})`);
	logger.debug(`<<< Exiting formatVersionNumber(n=${n})`);
    return n > 9 ? "" + n : "0" + n;
}

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

export function validationMapExtensionBeingEdited(extensionBeingEdited) {
	logger.debug(`>>> Entering validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`);
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        let validateRequired = getValidateRequired(key);
        _extensionBeingEdited[key] = {
            value: typeof _extensionBeingEdited[key] !== "object" ? _extensionBeingEdited[key] : _extensionBeingEdited[key].value,
            validateRequired,
            error: false
        };
    });
	logger.debug(`validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited}): _extensionBeingEdited → ${_extensionBeingEdited}`);
	logger.debug(`<<< Exiting validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`);
    return _extensionBeingEdited;
}

export function valueMapExtensionBeingEdited(extensionBeingEdited) {
	logger.debug(`>>> Entering valueMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`);
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        _extensionBeingEdited[key] = _extensionBeingEdited[key].value;
    });
	logger.debug(`valueMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited}): _extensionBeingEdited → ${_extensionBeingEdited}`);
	logger.debug(`<<< Exiting valueMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`);
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