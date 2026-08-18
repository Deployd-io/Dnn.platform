import Localization from "./localization";
const logger = console;

export default function validateFields(field, value) {
	logger.debug(`>>> Entering validateFields(field=${field},value=${value})`);
    const errors = {};
    const re = /^(|0|[1-9]\d*)$/;
	logger.debug(`validateFields(field=${field},value=${value}): re → ${re}`);

    if (field === "name") {
        if (!value) {
            errors[field] = Localization.get("NotEmptyNameError");
        }
        else {
            errors[field] = undefined;
        }
    }
    else if (field === "cacheDuration" || field === "cacheMaxVaryByCount") {
        if (!re.test(value)) {
            errors[field] = Localization.get(field + ".ErrorMessage");
        }
        else {
            errors[field] = undefined;
        }
    }
    else if (field === "cacheProvider" && !value) {
        errors["cacheDuration"] = undefined;
        errors["cacheMaxVaryByCount"] = undefined;
    }

	logger.debug(`<<< Exiting validateFields(field=${field},value=${value})`);
    return errors;
}