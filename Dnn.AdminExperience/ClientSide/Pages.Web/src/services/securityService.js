import utils from "../utils";
const logger = console;

const securityService = {
    userHasPermission(permission, selectedPage) {
		logger.debug(`>>> Entering userHasPermission(permission=${permission},selectedPage=${selectedPage})`);
        const isSuperUser = utils.getIsSuperUser();
        if (isSuperUser) {
			logger.debug('>>> Entering isSuperUser()');
			logger.debug('<<< Exiting isSuperUser()');
            return true;
        }

        if (!permission) {
            return true;
        }
        const userPermissionsOverPage = selectedPage && selectedPage.pagePermissions || utils.getCurrentPagePermissions();
		logger.debug(`userHasPermission(permission=${permission},selectedPage=${selectedPage}): userPermissionsOverPage → ${userPermissionsOverPage}`);

		logger.debug(`<<< Exiting userHasPermission(permission=${permission},selectedPage=${selectedPage})`);
        return userPermissionsOverPage[permission];
    },
    isSuperUser() {
        return utils.getIsSuperUser();
    },
    canSeePagesList() {
		logger.debug('>>> Entering canSeePagesList()');
		logger.debug('<<< Exiting canSeePagesList()');
        return utils.canSeePagesList();
    },
    canAddPages() {
		logger.debug('>>> Entering canAddPages()');
		logger.debug('<<< Exiting canAddPages()');
        return utils.canAddPages();
    },
};
export default securityService;