import Localization from "../localization";
const logger = console;

const regExpPositiveIntegerValue = /^(0|[1-9]\d*)$/;

export default function validateFields(field, value) {
	logger.debug(`>>> Entering validateFields(field=${field},value=${value})`);
    const errors = {};

    if (field === "smtpConnectionLimit" || field === "smtpMaxIdleTime" || field === "messageSchedulerBatchSize" ) {
        if (!regExpPositiveIntegerValue.test(value)) {
            errors[field] = Localization.get("NoIntegerValueError");
        }
        else {
            errors[field] = undefined;
        }
    }

	logger.debug(`<<< Exiting validateFields(field=${field},value=${value})`);
    return errors;
}