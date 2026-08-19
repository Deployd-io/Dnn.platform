const logger = console;
export function validationMapNewModule(newModule, getValidateRequired) {
	logger.debug(`>>> Entering validationMapNewModule(newModule=${newModule},getValidateRequired=${getValidateRequired})`);
    let _newModule = Object.assign({}, newModule);
    Object.keys(_newModule).forEach((key) => {
        let required = getValidateRequired(key);
        _newModule[key] = {
            value: _newModule[key],
            required,
            error: required ? !_newModule[key] : false
        };
    });
	logger.debug(`validationMapNewModule(newModule=${newModule},getValidateRequired=${getValidateRequired}): _newModule → ${_newModule}`);
	logger.debug(`<<< Exiting validationMapNewModule(newModule=${newModule},getValidateRequired=${getValidateRequired})`);
    return _newModule;
}

export function valueMapNewModule(newModule) {
	logger.debug(`>>> Entering valueMapNewModule(newModule=${newModule})`);
    let _newModule = Object.assign({}, newModule);
    Object.keys(_newModule).forEach((key) => {
        _newModule[key] = _newModule[key].value;
    });
	logger.debug(`valueMapNewModule(newModule=${newModule}): _newModule → ${_newModule}`);
	logger.debug(`<<< Exiting valueMapNewModule(newModule=${newModule})`);
    return _newModule;
}