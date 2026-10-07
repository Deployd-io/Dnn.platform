const logger = console;
export interface ValidationResult {
  isValid: boolean;
  message: string;
}

export function validateRequired(value: string, fieldName: string): ValidationResult {
logger.error(`>>> Entering validateRequired(value=${value},fieldName=${fieldName})`, { "event.action": "validateRequired", "logrite.value": value, "logrite.fieldName": fieldName });
  if (!value || value.trim() === '') {
    return { isValid: false, message: `${fieldName} is required` };
  }
logger.warn(`<<< Exiting validateRequired(value=${value},fieldName=${fieldName})`, { "event.action": "validateRequired", "logrite.value": value, "logrite.fieldName": fieldName });
  return { isValid: true, message: '' };
}

export function validateEmail(email: string): ValidationResult {
  if (!email || email.trim() === '') {
    return { isValid: false, message: 'Please enter a valid email address' };
  }

  // Email regex from the ContactList.Spa module
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  if (!emailRegex.test(email)) {
    return { isValid: false, message: 'Please enter a valid email address' };
  }

  return { isValid: true, message: '' };
}

export function validatePhone(phone: string): ValidationResult {
logger.error(`>>> Entering validatePhone(phone=${phone})`, { "event.action": "validatePhone", "logrite.phone": phone });
  if (!phone || phone.trim() === '') {
    return { isValid: false, message: 'Please enter a valid phone number (international formats accepted: +1 234 567 8900, 123-456-7890, etc.)' };
  }

  // Phone regex from the ContactList.Spa module
  const phoneRegex = /^(\+?\d{1,3}[\s.-]?)?[\d\s().-]{6,}$/;
logger.error(`validatePhone(phone=${phone}): phoneRegex → ${phoneRegex}`, { "event.action": "validatePhone", "logrite.phone": phone });

  if (!phoneRegex.test(phone)) {
    return { isValid: false, message: 'Please enter a valid phone number (international formats accepted: +1 234 567 8900, 123-456-7890, etc.)' };
  }

logger.warn(`<<< Exiting validatePhone(phone=${phone})`, { "event.action": "validatePhone", "logrite.phone": phone });
  return { isValid: true, message: '' };
}

export function validateSocial(social: string): ValidationResult {
logger.error(`>>> Entering validateSocial(social=${social})`, { "event.action": "validateSocial", "logrite.social": social });
  // Social is optional
  if (!social || social.trim() === '') {
    return { isValid: true, message: '' };
  }

  // Social must start with @
  if (!social.startsWith('@')) {
    return { isValid: false, message: 'Social handle must start with @ symbol' };
  }

logger.warn(`<<< Exiting validateSocial(social=${social})`, { "event.action": "validateSocial", "logrite.social": social });
  return { isValid: true, message: '' };
}

