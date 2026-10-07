import {users as ActionTypes}  from "../actionTypes";
import {CommonUsersService as UserService} from "../services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.error(`>>> Entering errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.warn(`<<< Exiting errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
}
const userActions = {
    getUsers(searchParameters, callback) {
		logger.error(`>>> Entering getUsers(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getUsers", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getUsers(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getUsers", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        return (dispatch) => {
            UserService.getUsers(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USERS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getUserDetails(userDetailsParameters, callback) {
		logger.error(`>>> Entering getUserDetails(userDetailsParameters=${userDetailsParameters},callback=${callback})`, { "event.action": "getUserDetails", "logrite.userDetailsParameters": userDetailsParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getUserDetails(userDetailsParameters=${userDetailsParameters},callback=${callback})`, { "event.action": "getUserDetails", "logrite.userDetailsParameters": userDetailsParameters, "logrite.callback": callback });
        return (dispatch) => {
            UserService.getUserDetails(userDetailsParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USER_DETAILS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getUserFilters(callback) {
		logger.error(`>>> Entering getUserFilters(callback=${callback})`, { "event.action": "getUserFilters", "logrite.callback": callback });
		logger.warn(`<<< Exiting getUserFilters(callback=${callback})`, { "event.action": "getUserFilters", "logrite.callback": callback });
        return () => {
            UserService.getUserFilters(data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    createUser(userDetails, filter, callback) {
		logger.error(`>>> Entering createUser(userDetails=${userDetails},filter=${filter},callback=${callback})`, { "event.action": "createUser", "logrite.userDetails": userDetails, "logrite.filter": filter, "logrite.callback": callback });
		logger.warn(`<<< Exiting createUser(userDetails=${userDetails},filter=${filter},callback=${callback})`, { "event.action": "createUser", "logrite.userDetails": userDetails, "logrite.filter": filter, "logrite.callback": callback });
        return (dispatch) => {
            UserService.createUser(userDetails, data => {
                dispatch({
                    type: ActionTypes.CREATE_USER,
                    payload: data,
                    filter: filter
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    updateUserBasicInfo(userDetails, callback) {
		logger.error(`>>> Entering updateUserBasicInfo(userDetails=${userDetails},callback=${callback})`, { "event.action": "updateUserBasicInfo", "logrite.userDetails": userDetails, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateUserBasicInfo(userDetails=${userDetails},callback=${callback})`, { "event.action": "updateUserBasicInfo", "logrite.userDetails": userDetails, "logrite.callback": callback });
        return (dispatch) => {
            UserService.updateUserBasicInfo(userDetails, data => {
                dispatch({
                    type: ActionTypes.UPDATE_USER,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    changePassword(payload, callback) {
		logger.error(`>>> Entering changePassword(payload=${payload},callback=${callback})`, { "event.action": "changePassword", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting changePassword(payload=${payload},callback=${callback})`, { "event.action": "changePassword", "logrite.payload": payload, "logrite.callback": callback });
        return () => {
            UserService.changePassword(payload, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    forceChangePassword(payload, callback) {
		logger.error(`>>> Entering forceChangePassword(payload=${payload},callback=${callback})`, { "event.action": "forceChangePassword", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting forceChangePassword(payload=${payload},callback=${callback})`, { "event.action": "forceChangePassword", "logrite.payload": payload, "logrite.callback": callback });
        return () => {
            UserService.forceChangePassword(payload, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    sendPasswordResetLink(payload, callback) {
		logger.error(`>>> Entering sendPasswordResetLink(payload=${payload},callback=${callback})`, { "event.action": "sendPasswordResetLink", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting sendPasswordResetLink(payload=${payload},callback=${callback})`, { "event.action": "sendPasswordResetLink", "logrite.payload": payload, "logrite.callback": callback });
        return () => {
            UserService.sendPasswordResetLink(payload, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deleteUser(payload, filter, callback) {
		logger.error(`>>> Entering deleteUser(payload=${payload},filter=${filter},callback=${callback})`, { "event.action": "deleteUser", "logrite.payload": payload, "logrite.filter": filter, "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteUser(payload=${payload},filter=${filter},callback=${callback})`, { "event.action": "deleteUser", "logrite.payload": payload, "logrite.filter": filter, "logrite.callback": callback });
        return (dispatch) => {
            let deletedUser = Object.assign({}, payload.userDetails);
			logger.error(`deleteUser(payload=${payload},filter=${filter},callback=${callback}): deletedUser → ${deletedUser}`, { "event.action": "deleteUser", "logrite.payload": payload, "logrite.filter": filter, "logrite.callback": callback });
            deletedUser.isDeleted = true;
			logger.debug(`deleteUser(payload=${payload},filter=${filter},callback=${callback}): deletedUser.isDeleted → ${deletedUser.isDeleted}`, { "event.action": "deleteUser", "logrite.payload": payload, "logrite.filter": filter, "logrite.callback": callback });
            UserService.deleteUser({userId: payload.userDetails.userId}, data => {
                dispatch({
                    type: ActionTypes.DELETE_USER,
                    payload: deletedUser,
                    filter: filter
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    eraseUser(payload, callback) {
		logger.error(`>>> Entering eraseUser(payload=${payload},callback=${callback})`, { "event.action": "eraseUser", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting eraseUser(payload=${payload},callback=${callback})`, { "event.action": "eraseUser", "logrite.payload": payload, "logrite.callback": callback });
        return (dispatch) => {
            UserService.hardDeleteUser(payload, data => {
                dispatch({
                    type: ActionTypes.ERASE_USER,
                    payload: { userId: payload.userId }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    removeDeletedUsers(callback) {
		logger.error(`>>> Entering removeDeletedUsers(callback=${callback})`, { "event.action": "removeDeletedUsers", "logrite.callback": callback });
		logger.warn(`<<< Exiting removeDeletedUsers(callback=${callback})`, { "event.action": "removeDeletedUsers", "logrite.callback": callback });
        return (dispatch) => {
            UserService.removeDeletedUsers(data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    restoreUser(payload, filter, callback) {
		logger.error(`>>> Entering restoreUser(payload=${payload},filter=${filter},callback=${callback})`, { "event.action": "restoreUser", "logrite.payload": payload, "logrite.filter": filter, "logrite.callback": callback });
		logger.warn(`<<< Exiting restoreUser(payload=${payload},filter=${filter},callback=${callback})`, { "event.action": "restoreUser", "logrite.payload": payload, "logrite.filter": filter, "logrite.callback": callback });
        return (dispatch) => {
            let restoredUser = Object.assign({}, payload.userDetails);
			logger.error(`restoreUser(payload=${payload},filter=${filter},callback=${callback}): restoredUser → ${restoredUser}`, { "event.action": "restoreUser", "logrite.payload": payload, "logrite.filter": filter, "logrite.callback": callback });
            restoredUser.isDeleted = false;
			logger.debug(`restoreUser(payload=${payload},filter=${filter},callback=${callback}): restoredUser.isDeleted → ${restoredUser.isDeleted}`, { "event.action": "restoreUser", "logrite.payload": payload, "logrite.filter": filter, "logrite.callback": callback });
            UserService.restoreUser({userId: payload.userDetails.userId}, data => {
                dispatch({
                    type: ActionTypes.RESTORE_USER,
                    payload: restoredUser,
                    filter: filter
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    updateSuperUserStatus(payload, filter, callback) {
		logger.error(`>>> Entering updateSuperUserStatus(payload=${payload},filter=${filter},callback=${callback})`, { "event.action": "updateSuperUserStatus", "logrite.payload": payload, "logrite.filter": filter, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateSuperUserStatus(payload=${payload},filter=${filter},callback=${callback})`, { "event.action": "updateSuperUserStatus", "logrite.payload": payload, "logrite.filter": filter, "logrite.callback": callback });
        return (dispatch) => {
            UserService.updateSuperUserStatus(payload, data => {
                dispatch({
                    type: ActionTypes.USER_MADE_SUPERUSER,
                    payload: { userId: payload.userId, setSuperUser: payload.setSuperUser },
                    filter: filter
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    updateAuthorizeStatus(payload, authorized, filter, callback) {
		logger.error(`>>> Entering updateAuthorizeStatus(payload=${payload},authorized=${authorized},filter=${filter},callback=${callback})`, { "event.action": "updateAuthorizeStatus", "logrite.payload": payload, "logrite.authorized": authorized, "logrite.filter": filter, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateAuthorizeStatus(payload=${payload},authorized=${authorized},filter=${filter},callback=${callback})`, { "event.action": "updateAuthorizeStatus", "logrite.payload": payload, "logrite.authorized": authorized, "logrite.filter": filter, "logrite.callback": callback });
        return (dispatch) => {
            let user = Object.assign({}, payload.userDetails);
			logger.error(`updateAuthorizeStatus(payload=${payload},authorized=${authorized},filter=${filter},callback=${callback}): user → ${user}`, { "event.action": "updateAuthorizeStatus", "logrite.payload": payload, "logrite.authorized": authorized, "logrite.filter": filter, "logrite.callback": callback });
            user.authorized = authorized;
			logger.debug(`updateAuthorizeStatus(payload=${payload},authorized=${authorized},filter=${filter},callback=${callback}): user.authorized → ${user.authorized}`, { "event.action": "updateAuthorizeStatus", "logrite.payload": payload, "logrite.authorized": authorized, "logrite.filter": filter, "logrite.callback": callback });
            UserService.updateAuthorizeStatus({ userId: payload.userDetails.userId, authorized: authorized }, data => {
                dispatch({
                    type: ActionTypes.UPDATE_USER_AUTHORIZE_STATUS,
                    payload: user,
                    filter: filter
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    unLockUser(payload, callback) {
		logger.error(`>>> Entering unLockUser(payload=${payload},callback=${callback})`, { "event.action": "unLockUser", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting unLockUser(payload=${payload},callback=${callback})`, { "event.action": "unLockUser", "logrite.payload": payload, "logrite.callback": callback });
        return (dispatch) => {
            let user = Object.assign({}, payload.userDetails);
			logger.error(`unLockUser(payload=${payload},callback=${callback}): user → ${user}`, { "event.action": "unLockUser", "logrite.payload": payload, "logrite.callback": callback });
            UserService.unlockUser({ userId: payload.userDetails.userId }, data => {
                dispatch({
                    type: ActionTypes.USER_UNLOCKED,
                    payload: user
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getUserRoles(searchParameters, callback) {
		logger.error(`>>> Entering getUserRoles(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getUserRoles", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getUserRoles(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getUserRoles", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        return (dispatch) => {
            UserService.getUserRoles(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USERS_ROLES,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    passwordStrength() {
		logger.error('>>> Entering passwordStrength()', { "event.action": "passwordStrength" });
		logger.warn('<<< Exiting passwordStrength()', { "event.action": "passwordStrength" });
        return (dispatch) => {
            UserService.passwordStrengthOptions(pStrength=>{
                dispatch({
                    type: ActionTypes.RETRIEVED_PASSWORD_STRENGTH_OPTIONS,
                    payload: pStrength
                });
            });
        };
    },
    getSuggestRoles(searchParameters, callback) {
		logger.error(`>>> Entering getSuggestRoles(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getSuggestRoles", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getSuggestRoles(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getSuggestRoles", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        return (dispatch) => {
            UserService.getSuggestRoles(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SUGGEST_ROLES,
                    payload: { matchedRoles: data }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    saveUserRole(payload, notifyUser, isOwner, callback) {
		logger.error(`>>> Entering saveUserRole(payload=${payload},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback})`, { "event.action": "saveUserRole", "logrite.payload": payload, "logrite.notifyUser": notifyUser, "logrite.isOwner": isOwner, "logrite.callback": callback });
		logger.warn(`<<< Exiting saveUserRole(payload=${payload},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback})`, { "event.action": "saveUserRole", "logrite.payload": payload, "logrite.notifyUser": notifyUser, "logrite.isOwner": isOwner, "logrite.callback": callback });
        return (dispatch) => {
            UserService.saveUserRole(payload, notifyUser, isOwner, data => {
                dispatch({
                    type: ActionTypes.SAVE_USER_ROLE,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    removeUserRole(payload, callback) {
		logger.error(`>>> Entering removeUserRole(payload=${payload},callback=${callback})`, { "event.action": "removeUserRole", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting removeUserRole(payload=${payload},callback=${callback})`, { "event.action": "removeUserRole", "logrite.payload": payload, "logrite.callback": callback });
        return (dispatch) => {
            UserService.removeUserRole(payload, data => {
                dispatch({
                    type: ActionTypes.REMOVE_USER_ROLE,
                    payload: { userId: payload.userId, roleId: payload.roleId }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default userActions;
