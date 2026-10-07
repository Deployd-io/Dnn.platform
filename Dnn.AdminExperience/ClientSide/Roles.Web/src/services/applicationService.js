import util from "../utils";
const logger = console;
class ApplicationService {
    getServiceFramework(controller) {
		logger.error(`>>> Entering getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        let sf = util.utilities.sf;

		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;

		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }

    getRoleGroups(reload, callback, errorCallback) {
		logger.error(`>>> Entering getRoleGroups(reload=${reload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getRoleGroups", "logrite.reload": reload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoleGroups", { reload: reload }, callback, errorCallback);
		logger.error(`getRoleGroups(reload=${reload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getRoleGroups", "logrite.reload": reload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getRoles(parameters, callback, errorCallback) {
		logger.error(`>>> Entering getRoles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getRoles", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoles", parameters, callback, errorCallback);
		logger.error(`getRoles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getRoles", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    saveRoleGroup(group, callback, errorCallback) {
		logger.error(`>>> Entering saveRoleGroup(group=${group},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "saveRoleGroup", "logrite.group": group, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Roles");
        sf.post("SaveRoleGroup", group, callback, errorCallback);
		logger.error(`saveRoleGroup(group=${group},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "saveRoleGroup", "logrite.group": group, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    deleteRoleGroup(group, callback, errorCallback) {
		logger.error(`>>> Entering deleteRoleGroup(group=${group},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "deleteRoleGroup", "logrite.group": group, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Roles");
        sf.post("DeleteRoleGroup", group, callback, errorCallback);
		logger.error(`deleteRoleGroup(group=${group},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "deleteRoleGroup", "logrite.group": group, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    saveRole(assignExistUsers, role, callback, errorCallback) {
		logger.error(`>>> Entering saveRole(assignExistUsers=${assignExistUsers},role=${role},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "saveRole", "logrite.assignExistUsers": assignExistUsers, "logrite.role": role, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Roles");
        sf.post("SaveRole?assignExistUsers=" + assignExistUsers, role, callback, errorCallback);
		logger.error(`saveRole(assignExistUsers=${assignExistUsers},role=${role},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "saveRole", "logrite.assignExistUsers": assignExistUsers, "logrite.role": role, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    deleteRole(role, callback, errorCallback) {
		logger.error(`>>> Entering deleteRole(role=${role},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "deleteRole", "logrite.role": role, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Roles");
        sf.post("DeleteRole", role, callback, errorCallback);
		logger.error(`deleteRole(role=${role},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "deleteRole", "logrite.role": role, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getSuggestUsers(parameters, callback, errorCallback) {
		logger.error(`>>> Entering getSuggestUsers(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getSuggestUsers", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Roles");
        sf.get("GetSuggestUsers", parameters, callback, errorCallback);
		logger.error(`getSuggestUsers(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getSuggestUsers", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    getRoleUsers(parameters, callback, errorCallback) {
		logger.error(`>>> Entering getRoleUsers(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getRoleUsers", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Roles");
        sf.get("GetRoleUsers", parameters, callback, errorCallback);
		logger.error(`getRoleUsers(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getRoleUsers", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    addUserToRole(parameters, notifyUser, isOwner, callback, errorCallback) {
		logger.error(`>>> Entering addUserToRole(parameters=${parameters},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "addUserToRole", "logrite.parameters": parameters, "logrite.notifyUser": notifyUser, "logrite.isOwner": isOwner, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Roles");
        sf.post("AddUserToRole?notifyUser=" + notifyUser + "&isOwner=" + isOwner, parameters, callback, errorCallback);
		logger.error(`addUserToRole(parameters=${parameters},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "addUserToRole", "logrite.parameters": parameters, "logrite.notifyUser": notifyUser, "logrite.isOwner": isOwner, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }

    removeUserFromRole(parameters, callback, errorCallback) {
		logger.error(`>>> Entering removeUserFromRole(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "removeUserFromRole", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Roles");
        sf.post("RemoveUserFromRole", parameters, callback, errorCallback);
		logger.error(`removeUserFromRole(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "removeUserFromRole", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
}
const applicationService = new ApplicationService();
export default applicationService;