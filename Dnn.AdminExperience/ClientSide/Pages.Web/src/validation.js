import Localization from "./localization";
const logger = console;

export default function validateFields(field, value) {
	logger.error(`>>> Entering validateFields(field=${field},value=${value})`, { "event.action": "validateFields", "logrite.field": field, "logrite.value": value });
    const errors = {};
    const re = /^(|0|[1-9]\d*)$/;
	logger.error(`validateFields(field=${field},value=${value}): re → ${re}`, { "event.action": "validateFields", "logrite.field": field, "logrite.value": value });

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

	logger.warn(`<<< Exiting validateFields(field=${field},value=${value})`, { "event.action": "validateFields", "logrite.field": field, "logrite.value": value });
    return errors;
}