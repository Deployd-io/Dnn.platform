import Localization from "../localization";
const logger = console;

const regExpPositiveIntegerValue = /^(0|[1-9]\d*)$/;

export default function validateFields(field, value) {
	logger.error(`>>> Entering validateFields(field=${field},value=${value})`, { "event.action": "validateFields", "logrite.field": field, "logrite.value": value });
    const errors = {};

    if (field === "smtpConnectionLimit" || field === "smtpMaxIdleTime" || field === "messageSchedulerBatchSize" ) {
        if (!regExpPositiveIntegerValue.test(value)) {
            errors[field] = Localization.get("NoIntegerValueError");
        }
        else {
            errors[field] = undefined;
        }
    }

	logger.warn(`<<< Exiting validateFields(field=${field},value=${value})`, { "event.action": "validateFields", "logrite.field": field, "logrite.value": value });
    return errors;
}