import {roles as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";
const logger = console;

function errorCallback(message) {
	logger.error(`>>> Entering errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
    util.utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.warn(`<<< Exiting errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
}
const rolesActions = {
    getRoleGroupsList(reload, callback) {
		logger.error(`>>> Entering getRoleGroupsList(reload=${reload},callback=${callback})`, { "event.action": "getRoleGroupsList", "logrite.reload": reload, "logrite.callback": callback });
		logger.warn(`<<< Exiting getRoleGroupsList(reload=${reload},callback=${callback})`, { "event.action": "getRoleGroupsList", "logrite.reload": reload, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getRoleGroups(reload, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_GROUPS_LIST,
                    data: { roleGroups: data }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    saveRoleGroup(group, callback) {
		logger.error(`>>> Entering saveRoleGroup(group=${group},callback=${callback})`, { "event.action": "saveRoleGroup", "logrite.group": group, "logrite.callback": callback });
		logger.warn(`<<< Exiting saveRoleGroup(group=${group},callback=${callback})`, { "event.action": "saveRoleGroup", "logrite.group": group, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.saveRoleGroup(group, data => {
                dispatch({
                    type: ActionTypes.UPDATE_ROLEGROUP,
                    data: { roleGroup: data }
                });

                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deleteRoleGroup(group, callback) {
		logger.error(`>>> Entering deleteRoleGroup(group=${group},callback=${callback})`, { "event.action": "deleteRoleGroup", "logrite.group": group, "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteRoleGroup(group=${group},callback=${callback})`, { "event.action": "deleteRoleGroup", "logrite.group": group, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.deleteRoleGroup(group, data => {
                dispatch({
                    type: ActionTypes.DELETE_ROLEGROUP,
                    data: { groupId: data.groupId }
                });

                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getRolesList(parameters, callback) {
		logger.error(`>>> Entering getRolesList(parameters=${parameters},callback=${callback})`, { "event.action": "getRolesList", "logrite.parameters": parameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getRolesList(parameters=${parameters},callback=${callback})`, { "event.action": "getRolesList", "logrite.parameters": parameters, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getRoles(parameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_ROLES_LIST,
                    data: { rolesList: data.roles },
                    loadMore: data.loadMore,
                    reload: parameters.reload,
                    rsvpLink: data.rsvpLink
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    saveRole(currentGroupId, assignExistUsers, role, callback) {
		logger.error(`>>> Entering saveRole(currentGroupId=${currentGroupId},assignExistUsers=${assignExistUsers},role=${role},callback=${callback})`, { "event.action": "saveRole", "logrite.currentGroupId": currentGroupId, "logrite.assignExistUsers": assignExistUsers, "logrite.role": role, "logrite.callback": callback });
		logger.warn(`<<< Exiting saveRole(currentGroupId=${currentGroupId},assignExistUsers=${assignExistUsers},role=${role},callback=${callback})`, { "event.action": "saveRole", "logrite.currentGroupId": currentGroupId, "logrite.assignExistUsers": assignExistUsers, "logrite.role": role, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.saveRole(assignExistUsers, role, data => {
                dispatch({
                    type: ActionTypes.UPDATE_ROLE,
                    data: { roleDetails: data, currentGroupId: currentGroupId }
                });

                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deleteRole(role, callback) {
		logger.error(`>>> Entering deleteRole(role=${role},callback=${callback})`, { "event.action": "deleteRole", "logrite.role": role, "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteRole(role=${role},callback=${callback})`, { "event.action": "deleteRole", "logrite.role": role, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.deleteRole(role, data => {
                dispatch({
                    type: ActionTypes.DELETE_ROLE,
                    data: { roleId: data.roleId }
                });

                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    }
};

export default rolesActions;
