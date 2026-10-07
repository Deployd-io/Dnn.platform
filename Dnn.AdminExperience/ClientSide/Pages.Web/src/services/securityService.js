import utils from "../utils";
const logger = console;

const securityService = {
    userHasPermission(permission, selectedPage) {
		logger.error(`>>> Entering userHasPermission(permission=${permission},selectedPage=${selectedPage})`, { "event.action": "userHasPermission", "logrite.permission": permission, "logrite.selectedPage": selectedPage });
        const isSuperUser = utils.getIsSuperUser();
        if (isSuperUser) {
			logger.error('>>> Entering isSuperUser()', { "event.action": "isSuperUser" });
			logger.warn('<<< Exiting isSuperUser()', { "event.action": "isSuperUser" });
            return true;
        }

        if (!permission) {
            return true;
        }
        const userPermissionsOverPage = selectedPage && selectedPage.pagePermissions || utils.getCurrentPagePermissions();
		logger.error(`userHasPermission(permission=${permission},selectedPage=${selectedPage}): userPermissionsOverPage → ${userPermissionsOverPage}`, { "event.action": "userHasPermission", "logrite.permission": permission, "logrite.selectedPage": selectedPage });

		logger.warn(`<<< Exiting userHasPermission(permission=${permission},selectedPage=${selectedPage})`, { "event.action": "userHasPermission", "logrite.permission": permission, "logrite.selectedPage": selectedPage });
        return userPermissionsOverPage[permission];
    },
    isSuperUser() {
        return utils.getIsSuperUser();
    },
    canSeePagesList() {
		logger.error('>>> Entering canSeePagesList()', { "event.action": "canSeePagesList" });
		logger.warn('<<< Exiting canSeePagesList()', { "event.action": "canSeePagesList" });
        return utils.canSeePagesList();
    },
    canAddPages() {
		logger.error('>>> Entering canAddPages()', { "event.action": "canAddPages" });
		logger.warn('<<< Exiting canAddPages()', { "event.action": "canAddPages" });
        return utils.canAddPages();
    },
};
export default securityService;