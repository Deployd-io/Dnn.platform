import utilities from "utils";
const logger = console;
function serializeQueryStringParameters(obj) {
	logger.error(`>>> Entering serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    let s = [];
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.warn(`<<< Exiting serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    return s.join("&");
}
class UserService {
    getServiceFramework(controller) {
		logger.error(`>>> Entering getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        let sf = utilities.sf;

		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;

		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }
    getUsers(searchParameters, callback, errorCallback) {
		logger.error(`>>> Entering getUsers(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getUsers", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.get("GetUsers?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
		logger.error(`getUsers(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getUsers", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getUserDetails(userDetailsParameters, callback, errorCallback) {
		logger.error(`>>> Entering getUserDetails(userDetailsParameters=${userDetailsParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getUserDetails", "logrite.userDetailsParameters": userDetailsParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserDetail?" + serializeQueryStringParameters(userDetailsParameters), {}, callback, errorCallback);
		logger.error(`getUserDetails(userDetailsParameters=${userDetailsParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getUserDetails", "logrite.userDetailsParameters": userDetailsParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    updateUserBasicInfo(userDetails, callback, errorCallback) {
		logger.error(`>>> Entering updateUserBasicInfo(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "updateUserBasicInfo", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("UpdateUserBasicInfo", userDetails, callback, errorCallback);
		logger.error(`updateUserBasicInfo(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "updateUserBasicInfo", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getUserFilters(callback, errorCallback) {
		logger.error(`>>> Entering getUserFilters(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getUserFilters", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserFilters", {}, callback, errorCallback);
		logger.error(`getUserFilters(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getUserFilters", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    createUser(userDetails, callback, errorCallback) {
		logger.error(`>>> Entering createUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createUser", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("CreateUser", userDetails, callback, errorCallback);
		logger.error(`createUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "createUser", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    changePassword(payload, callback, errorCallback) {
		logger.error(`>>> Entering changePassword(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "changePassword", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("ChangePassword", payload, callback, errorCallback);
		logger.error(`changePassword(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "changePassword", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    forceChangePassword(userDetails, callback, errorCallback) {
		logger.error(`>>> Entering forceChangePassword(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "forceChangePassword", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("ForceChangePassword?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.error(`forceChangePassword(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "forceChangePassword", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    sendPasswordResetLink(userDetails, callback, errorCallback) {
		logger.error(`>>> Entering sendPasswordResetLink(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "sendPasswordResetLink", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("SendPasswordResetLink?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.error(`sendPasswordResetLink(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "sendPasswordResetLink", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    deleteUser(userDetails, callback, errorCallback) {
		logger.error(`>>> Entering deleteUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "deleteUser", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("SoftDeleteUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.error(`deleteUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "deleteUser", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    hardDeleteUser(userDetails, callback, errorCallback) {
		logger.error(`>>> Entering hardDeleteUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "hardDeleteUser", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("HardDeleteUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.error(`hardDeleteUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "hardDeleteUser", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    removeDeletedUsers(callback, errorCallback) {
		logger.error(`>>> Entering removeDeletedUsers(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "removeDeletedUsers", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("RemoveDeletedUsers", null, callback, errorCallback);
		logger.error(`removeDeletedUsers(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "removeDeletedUsers", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    restoreUser(userDetails, callback, errorCallback) {
		logger.error(`>>> Entering restoreUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "restoreUser", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("RestoreDeletedUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.error(`restoreUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "restoreUser", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    updateSuperUserStatus(userDetails, callback, errorCallback) {
		logger.error(`>>> Entering updateSuperUserStatus(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "updateSuperUserStatus", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("UpdateSuperUserStatus?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.error(`updateSuperUserStatus(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "updateSuperUserStatus", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    updateAuthorizeStatus(userDetails, callback, errorCallback) {
		logger.error(`>>> Entering updateAuthorizeStatus(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "updateAuthorizeStatus", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("UpdateAuthorizeStatus?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.error(`updateAuthorizeStatus(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "updateAuthorizeStatus", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    unlockUser(userDetails, callback, errorCallback) {
		logger.error(`>>> Entering unlockUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "unlockUser", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("UnlockUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.error(`unlockUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "unlockUser", "logrite.userDetails": userDetails, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    //User Roles Methods
    getUserRoles(searchParameters, callback, errorCallback) {
		logger.error(`>>> Entering getUserRoles(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getUserRoles", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserRoles?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
		logger.error(`getUserRoles(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getUserRoles", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getSuggestRoles(searchParameters, callback, errorCallback) {
		logger.error(`>>> Entering getSuggestRoles(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getSuggestRoles", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.get("GetSuggestRoles?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
		logger.error(`getSuggestRoles(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getSuggestRoles", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    saveUserRole(payload, notifyUser, isOwner, callback, errorCallback) {
		logger.error(`>>> Entering saveUserRole(payload=${payload},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "saveUserRole", "logrite.payload": payload, "logrite.notifyUser": notifyUser, "logrite.isOwner": isOwner, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("SaveUserRole?notifyUser=" + notifyUser + "&isOwner=" + isOwner, payload, callback, errorCallback);
		logger.error(`saveUserRole(payload=${payload},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "saveUserRole", "logrite.payload": payload, "logrite.notifyUser": notifyUser, "logrite.isOwner": isOwner, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    removeUserRole(payload, callback, errorCallback) {
		logger.error(`>>> Entering removeUserRole(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "removeUserRole", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.post("RemoveUserRole", payload, callback, errorCallback);
		logger.error(`removeUserRole(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "removeUserRole", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    
    passwordStrengthOptions(callback, errorCallback) {
		logger.error(`>>> Entering passwordStrengthOptions(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "passwordStrengthOptions", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.get("PasswordStrengthOptions",{},callback,errorCallback);
		logger.error(`passwordStrengthOptions(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "passwordStrengthOptions", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
}
const userService = new UserService();
export default userService;