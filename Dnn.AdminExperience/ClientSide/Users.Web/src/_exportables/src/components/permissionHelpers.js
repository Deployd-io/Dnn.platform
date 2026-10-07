const logger = console;
export function canManageRoles(settings, user) {
	logger.error(`>>> Entering canManageRoles(settings=${settings},user=${user})`, { "event.action": "canManageRoles", "logrite.settings": settings, "user.name": String(user) });
	logger.warn(`<<< Exiting canManageRoles(settings=${settings},user=${user})`, { "event.action": "canManageRoles", "logrite.settings": settings, "user.name": String(user) });
    return !user.isSuperUser && (settings.isAdmin || settings.permissions.MANAGE_ROLES);
}

export function canManageProfile(settings) {
	logger.error(`>>> Entering canManageProfile(settings=${settings})`, { "event.action": "canManageProfile", "logrite.settings": settings });
	logger.warn(`<<< Exiting canManageProfile(settings=${settings})`, { "event.action": "canManageProfile", "logrite.settings": settings });
    return settings.isAdmin || settings.permissions.MANAGE_PROFILE;
}

export function canViewSettings(settings) {
	logger.error(`>>> Entering canViewSettings(settings=${settings})`, { "event.action": "canViewSettings", "logrite.settings": settings });
	logger.warn(`<<< Exiting canViewSettings(settings=${settings})`, { "event.action": "canViewSettings", "logrite.settings": settings });
    return settings.isAdmin || settings.permissions.VIEW_SETTINGS;
}

export function canAddUser(settings) {
	logger.error(`>>> Entering canAddUser(settings=${settings})`, { "event.action": "canAddUser", "logrite.settings": settings });
	logger.warn(`<<< Exiting canAddUser(settings=${settings})`, { "event.action": "canAddUser", "logrite.settings": settings });
    return (settings.isAdmin || settings.permissions.ADD_USER);
}

export function canManagePassword(settings)
{
	logger.error(`>>> Entering canManagePassword(settings=${settings})`, { "event.action": "canManagePassword", "logrite.settings": settings });
	logger.warn(`<<< Exiting canManagePassword(settings=${settings})`, { "event.action": "canManagePassword", "logrite.settings": settings });
    return (settings.isAdmin || settings.permissions.MANAGE_PASSWORD);
}

export function canEditSettings(settings)
{
	logger.error(`>>> Entering canEditSettings(settings=${settings})`, { "event.action": "canEditSettings", "logrite.settings": settings });
	logger.warn(`<<< Exiting canEditSettings(settings=${settings})`, { "event.action": "canEditSettings", "logrite.settings": settings });
    return (settings.isAdmin || settings.permissions.EDIT_SETTINGS);
}

export function canDeleteUser(settings, userId)
{
	logger.error(`>>> Entering canDeleteUser(settings=${settings},userId=${userId})`, { "event.action": "canDeleteUser", "logrite.settings": settings, "user.name": String(userId) });
	logger.warn(`<<< Exiting canDeleteUser(settings=${settings},userId=${userId})`, { "event.action": "canDeleteUser", "logrite.settings": settings, "user.name": String(userId) });
    return (settings.isAdmin || settings.permissions.DELETE_USER) 
        && userId!==settings.userId;
}

export function canAuthorizeUnAuthorizeUser(settings, userId)
{
	logger.error(`>>> Entering canAuthorizeUnAuthorizeUser(settings=${settings},userId=${userId})`, { "event.action": "canAuthorizeUnAuthorizeUser", "logrite.settings": settings, "user.name": String(userId) });
	logger.warn(`<<< Exiting canAuthorizeUnAuthorizeUser(settings=${settings},userId=${userId})`, { "event.action": "canAuthorizeUnAuthorizeUser", "logrite.settings": settings, "user.name": String(userId) });
    return (settings.isAdmin || settings.permissions.AUTHORIZE_UNAUTHORIZE_USER) 
        && userId!==settings.userId;
}

export function canPromoteDemote(settings, userId) {
	logger.error(`>>> Entering canPromoteDemote(settings=${settings},userId=${userId})`, { "event.action": "canPromoteDemote", "logrite.settings": settings, "user.name": String(userId) });
	logger.warn(`<<< Exiting canPromoteDemote(settings=${settings},userId=${userId})`, { "event.action": "canPromoteDemote", "logrite.settings": settings, "user.name": String(userId) });
    return settings.isHost && userId!==settings.userId;
}