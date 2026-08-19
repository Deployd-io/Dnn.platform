import util from "../utils";
const logger = console;
class ApplicationService {
    getServiceFramework(controller) {
		logger.debug(`>>> Entering getServiceFramework(controller=${controller})`);
        let sf = util.utilities.sf;

        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`);

		logger.debug(`<<< Exiting getServiceFramework(controller=${controller})`);
        return sf;
    }

    getRoleGroups(reload, callback, errorCallback) {
		logger.debug(`>>> Entering getRoleGroups(reload=${reload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoleGroups", { reload: reload }, callback, errorCallback);
		logger.debug(`getRoleGroups(reload=${reload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getRoleGroups(reload=${reload},callback=${callback},errorCallback=${errorCallback})`);
    }

    getRoles(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getRoles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoles", parameters, callback, errorCallback);
		logger.debug(`getRoles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getRoles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
    }

    saveRoleGroup(group, callback, errorCallback) {
		logger.debug(`>>> Entering saveRoleGroup(group=${group},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Roles");
        sf.post("SaveRoleGroup", group, callback, errorCallback);
		logger.debug(`saveRoleGroup(group=${group},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting saveRoleGroup(group=${group},callback=${callback},errorCallback=${errorCallback})`);
    }

    deleteRoleGroup(group, callback, errorCallback) {
		logger.debug(`>>> Entering deleteRoleGroup(group=${group},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Roles");
        sf.post("DeleteRoleGroup", group, callback, errorCallback);
		logger.debug(`deleteRoleGroup(group=${group},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteRoleGroup(group=${group},callback=${callback},errorCallback=${errorCallback})`);
    }

    saveRole(assignExistUsers, role, callback, errorCallback) {
		logger.debug(`>>> Entering saveRole(assignExistUsers=${assignExistUsers},role=${role},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Roles");
        sf.post("SaveRole?assignExistUsers=" + assignExistUsers, role, callback, errorCallback);
		logger.debug(`saveRole(assignExistUsers=${assignExistUsers},role=${role},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting saveRole(assignExistUsers=${assignExistUsers},role=${role},callback=${callback},errorCallback=${errorCallback})`);
    }

    deleteRole(role, callback, errorCallback) {
		logger.debug(`>>> Entering deleteRole(role=${role},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Roles");
        sf.post("DeleteRole", role, callback, errorCallback);
		logger.debug(`deleteRole(role=${role},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteRole(role=${role},callback=${callback},errorCallback=${errorCallback})`);
    }

    getSuggestUsers(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getSuggestUsers(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Roles");
        sf.get("GetSuggestUsers", parameters, callback, errorCallback);
		logger.debug(`getSuggestUsers(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSuggestUsers(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
    }

    getRoleUsers(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getRoleUsers(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoleUsers", parameters, callback, errorCallback);
		logger.debug(`getRoleUsers(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getRoleUsers(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
    }

    addUserToRole(parameters, notifyUser, isOwner, callback, errorCallback) {
		logger.debug(`>>> Entering addUserToRole(parameters=${parameters},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Roles");
        sf.post("AddUserToRole?notifyUser=" + notifyUser + "&isOwner=" + isOwner, parameters, callback, errorCallback);
		logger.debug(`addUserToRole(parameters=${parameters},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting addUserToRole(parameters=${parameters},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback},errorCallback=${errorCallback})`);
    }

    removeUserFromRole(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering removeUserFromRole(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Roles");
        sf.post("RemoveUserFromRole", parameters, callback, errorCallback);
		logger.debug(`removeUserFromRole(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting removeUserFromRole(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
    }
}
const applicationService = new ApplicationService();
export default applicationService;