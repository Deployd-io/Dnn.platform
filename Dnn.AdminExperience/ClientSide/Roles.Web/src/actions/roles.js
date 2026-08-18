import {roles as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import util from "../utils";
const logger = console;

function errorCallback(message) {
	logger.debug(`>>> Entering errorCallback(message=${message})`);
    util.utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.debug(`<<< Exiting errorCallback(message=${message})`);
}
const rolesActions = {
    getRoleGroupsList(reload, callback) {
		logger.debug(`>>> Entering getRoleGroupsList(reload=${reload},callback=${callback})`);
		logger.debug(`<<< Exiting getRoleGroupsList(reload=${reload},callback=${callback})`);
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
		logger.debug(`>>> Entering saveRoleGroup(group=${group},callback=${callback})`);
		logger.debug(`<<< Exiting saveRoleGroup(group=${group},callback=${callback})`);
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
		logger.debug(`>>> Entering deleteRoleGroup(group=${group},callback=${callback})`);
		logger.debug(`<<< Exiting deleteRoleGroup(group=${group},callback=${callback})`);
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
		logger.debug(`>>> Entering getRolesList(parameters=${parameters},callback=${callback})`);
		logger.debug(`<<< Exiting getRolesList(parameters=${parameters},callback=${callback})`);
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
		logger.debug(`>>> Entering saveRole(currentGroupId=${currentGroupId},assignExistUsers=${assignExistUsers},role=${role},callback=${callback})`);
		logger.debug(`<<< Exiting saveRole(currentGroupId=${currentGroupId},assignExistUsers=${assignExistUsers},role=${role},callback=${callback})`);
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
		logger.debug(`>>> Entering deleteRole(role=${role},callback=${callback})`);
		logger.debug(`<<< Exiting deleteRole(role=${role},callback=${callback})`);
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
