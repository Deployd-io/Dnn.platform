const logger = console;
export function canManageRoles(settings, user) {
	logger.debug(`>>> Entering canManageRoles(settings=${settings},user=${user})`);
	logger.debug(`<<< Exiting canManageRoles(settings=${settings},user=${user})`);
    return !user.isSuperUser && (settings.isAdmin || settings.permissions.MANAGE_ROLES);
}

export function canManageProfile(settings) {
	logger.debug(`>>> Entering canManageProfile(settings=${settings})`);
	logger.debug(`<<< Exiting canManageProfile(settings=${settings})`);
    return settings.isAdmin || settings.permissions.MANAGE_PROFILE;
}

export function canViewSettings(settings) {
	logger.debug(`>>> Entering canViewSettings(settings=${settings})`);
	logger.debug(`<<< Exiting canViewSettings(settings=${settings})`);
    return settings.isAdmin || settings.permissions.VIEW_SETTINGS;
}

export function canAddUser(settings) {
	logger.debug(`>>> Entering canAddUser(settings=${settings})`);
	logger.debug(`<<< Exiting canAddUser(settings=${settings})`);
    return (settings.isAdmin || settings.permissions.ADD_USER);
}

export function canManagePassword(settings)
{
	logger.debug(`>>> Entering canManagePassword(settings=${settings})`);
	logger.debug(`<<< Exiting canManagePassword(settings=${settings})`);
    return (settings.isAdmin || settings.permissions.MANAGE_PASSWORD);
}

export function canEditSettings(settings)
{
	logger.debug(`>>> Entering canEditSettings(settings=${settings})`);
	logger.debug(`<<< Exiting canEditSettings(settings=${settings})`);
    return (settings.isAdmin || settings.permissions.EDIT_SETTINGS);
}

export function canDeleteUser(settings, userId)
{
	logger.debug(`>>> Entering canDeleteUser(settings=${settings},userId=${userId})`);
	logger.debug(`<<< Exiting canDeleteUser(settings=${settings},userId=${userId})`);
    return (settings.isAdmin || settings.permissions.DELETE_USER) 
        && userId!==settings.userId;
}

export function canAuthorizeUnAuthorizeUser(settings, userId)
{
	logger.debug(`>>> Entering canAuthorizeUnAuthorizeUser(settings=${settings},userId=${userId})`);
	logger.debug(`<<< Exiting canAuthorizeUnAuthorizeUser(settings=${settings},userId=${userId})`);
    return (settings.isAdmin || settings.permissions.AUTHORIZE_UNAUTHORIZE_USER) 
        && userId!==settings.userId;
}

export function canPromoteDemote(settings, userId) {
	logger.debug(`>>> Entering canPromoteDemote(settings=${settings},userId=${userId})`);
	logger.debug(`<<< Exiting canPromoteDemote(settings=${settings},userId=${userId})`);
    return settings.isHost && userId!==settings.userId;
}