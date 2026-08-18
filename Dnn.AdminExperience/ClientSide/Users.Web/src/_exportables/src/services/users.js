import utilities from "utils";
const logger = console;
function serializeQueryStringParameters(obj) {
	logger.debug(`>>> Entering serializeQueryStringParameters(obj=${obj})`);
    let s = [];
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.debug(`<<< Exiting serializeQueryStringParameters(obj=${obj})`);
    return s.join("&");
}
class UserService {
    getServiceFramework(controller) {
		logger.debug(`>>> Entering getServiceFramework(controller=${controller})`);
        let sf = utilities.sf;

        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`);

		logger.debug(`<<< Exiting getServiceFramework(controller=${controller})`);
        return sf;
    }
    getUsers(searchParameters, callback, errorCallback) {
		logger.debug(`>>> Entering getUsers(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.get("GetUsers?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
		logger.debug(`getUsers(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getUsers(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`);
    }
    getUserDetails(userDetailsParameters, callback, errorCallback) {
		logger.debug(`>>> Entering getUserDetails(userDetailsParameters=${userDetailsParameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserDetail?" + serializeQueryStringParameters(userDetailsParameters), {}, callback, errorCallback);
		logger.debug(`getUserDetails(userDetailsParameters=${userDetailsParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getUserDetails(userDetailsParameters=${userDetailsParameters},callback=${callback},errorCallback=${errorCallback})`);
    }
    updateUserBasicInfo(userDetails, callback, errorCallback) {
		logger.debug(`>>> Entering updateUserBasicInfo(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("UpdateUserBasicInfo", userDetails, callback, errorCallback);
		logger.debug(`updateUserBasicInfo(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateUserBasicInfo(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
    }
    getUserFilters(callback, errorCallback) {
		logger.debug(`>>> Entering getUserFilters(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserFilters", {}, callback, errorCallback);
		logger.debug(`getUserFilters(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getUserFilters(callback=${callback},errorCallback=${errorCallback})`);
    }
    createUser(userDetails, callback, errorCallback) {
		logger.debug(`>>> Entering createUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("CreateUser", userDetails, callback, errorCallback);
		logger.debug(`createUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting createUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
    }
    changePassword(payload, callback, errorCallback) {
		logger.debug(`>>> Entering changePassword(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("ChangePassword", payload, callback, errorCallback);
		logger.debug(`changePassword(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting changePassword(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    }
    forceChangePassword(userDetails, callback, errorCallback) {
		logger.debug(`>>> Entering forceChangePassword(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("ForceChangePassword?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.debug(`forceChangePassword(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting forceChangePassword(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
    }
    sendPasswordResetLink(userDetails, callback, errorCallback) {
		logger.debug(`>>> Entering sendPasswordResetLink(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("SendPasswordResetLink?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.debug(`sendPasswordResetLink(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting sendPasswordResetLink(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
    }
    deleteUser(userDetails, callback, errorCallback) {
		logger.debug(`>>> Entering deleteUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("SoftDeleteUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.debug(`deleteUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
    }
    hardDeleteUser(userDetails, callback, errorCallback) {
		logger.debug(`>>> Entering hardDeleteUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("HardDeleteUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.debug(`hardDeleteUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting hardDeleteUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
    }
    removeDeletedUsers(callback, errorCallback) {
		logger.debug(`>>> Entering removeDeletedUsers(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("RemoveDeletedUsers", null, callback, errorCallback);
		logger.debug(`removeDeletedUsers(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting removeDeletedUsers(callback=${callback},errorCallback=${errorCallback})`);
    }
    restoreUser(userDetails, callback, errorCallback) {
		logger.debug(`>>> Entering restoreUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("RestoreDeletedUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.debug(`restoreUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting restoreUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
    }
    updateSuperUserStatus(userDetails, callback, errorCallback) {
		logger.debug(`>>> Entering updateSuperUserStatus(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("UpdateSuperUserStatus?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.debug(`updateSuperUserStatus(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateSuperUserStatus(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
    }
    updateAuthorizeStatus(userDetails, callback, errorCallback) {
		logger.debug(`>>> Entering updateAuthorizeStatus(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("UpdateAuthorizeStatus?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.debug(`updateAuthorizeStatus(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateAuthorizeStatus(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
    }
    unlockUser(userDetails, callback, errorCallback) {
		logger.debug(`>>> Entering unlockUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("UnlockUser?" + serializeQueryStringParameters(userDetails), null, callback, errorCallback);
		logger.debug(`unlockUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting unlockUser(userDetails=${userDetails},callback=${callback},errorCallback=${errorCallback})`);
    }
    //User Roles Methods
    getUserRoles(searchParameters, callback, errorCallback) {
		logger.debug(`>>> Entering getUserRoles(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserRoles?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
		logger.debug(`getUserRoles(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getUserRoles(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`);
    }
    getSuggestRoles(searchParameters, callback, errorCallback) {
		logger.debug(`>>> Entering getSuggestRoles(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.get("GetSuggestRoles?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
		logger.debug(`getSuggestRoles(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSuggestRoles(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`);
    }
    saveUserRole(payload, notifyUser, isOwner, callback, errorCallback) {
		logger.debug(`>>> Entering saveUserRole(payload=${payload},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("SaveUserRole?notifyUser=" + notifyUser + "&isOwner=" + isOwner, payload, callback, errorCallback);
		logger.debug(`saveUserRole(payload=${payload},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting saveUserRole(payload=${payload},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback},errorCallback=${errorCallback})`);
    }
    removeUserRole(payload, callback, errorCallback) {
		logger.debug(`>>> Entering removeUserRole(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.post("RemoveUserRole", payload, callback, errorCallback);
		logger.debug(`removeUserRole(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting removeUserRole(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    }
    
    passwordStrengthOptions(callback, errorCallback) {
		logger.debug(`>>> Entering passwordStrengthOptions(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.get("PasswordStrengthOptions",{},callback,errorCallback);
		logger.debug(`passwordStrengthOptions(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting passwordStrengthOptions(callback=${callback},errorCallback=${errorCallback})`);
    }
}
const userService = new UserService();
export default userService;