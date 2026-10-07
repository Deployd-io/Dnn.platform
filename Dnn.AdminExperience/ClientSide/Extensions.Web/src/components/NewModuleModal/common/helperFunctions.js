const logger = console;
export function validationMapNewModule(newModule, getValidateRequired) {
	logger.error(`>>> Entering validationMapNewModule(newModule=${newModule},getValidateRequired=${getValidateRequired})`, { "event.action": "validationMapNewModule", "logrite.newModule": newModule, "logrite.getValidateRequired": getValidateRequired });
    let _newModule = Object.assign({}, newModule);
    Object.keys(_newModule).forEach((key) => {
        let required = getValidateRequired(key);
        _newModule[key] = {
            value: _newModule[key],
            required,
            error: required ? !_newModule[key] : false
        };
    });
	logger.error(`validationMapNewModule(newModule=${newModule},getValidateRequired=${getValidateRequired}): _newModule → ${_newModule}`, { "event.action": "validationMapNewModule", "logrite.newModule": newModule, "logrite.getValidateRequired": getValidateRequired });
    return _newModule;
}

export function valueMapNewModule(newModule) {
	logger.error(`>>> Entering valueMapNewModule(newModule=${newModule})`, { "event.action": "valueMapNewModule", "logrite.newModule": newModule });
    let _newModule = Object.assign({}, newModule);
    Object.keys(_newModule).forEach((key) => {
        _newModule[key] = _newModule[key].value;
    });
	logger.error(`valueMapNewModule(newModule=${newModule}): _newModule → ${_newModule}`, { "event.action": "valueMapNewModule", "logrite.newModule": newModule });
    return _newModule;
}