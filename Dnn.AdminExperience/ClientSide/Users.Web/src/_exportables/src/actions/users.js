import {users as ActionTypes}  from "../actionTypes";
import {CommonUsersService as UserService} from "../services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.debug(`>>> Entering errorCallback(message=${message})`);
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.debug(`<<< Exiting errorCallback(message=${message})`);
}
const userActions = {
    getUsers(searchParameters, callback) {
		logger.debug(`>>> Entering getUsers(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getUsers(searchParameters=${searchParameters},callback=${callback})`);
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
		logger.debug(`>>> Entering getUserDetails(userDetailsParameters=${userDetailsParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getUserDetails(userDetailsParameters=${userDetailsParameters},callback=${callback})`);
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
		logger.debug(`>>> Entering getUserFilters(callback=${callback})`);
		logger.debug(`<<< Exiting getUserFilters(callback=${callback})`);
        return () => {
            UserService.getUserFilters(data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    createUser(userDetails, filter, callback) {
		logger.debug(`>>> Entering createUser(userDetails=${userDetails},filter=${filter},callback=${callback})`);
		logger.debug(`<<< Exiting createUser(userDetails=${userDetails},filter=${filter},callback=${callback})`);
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
		logger.debug(`>>> Entering updateUserBasicInfo(userDetails=${userDetails},callback=${callback})`);
		logger.debug(`<<< Exiting updateUserBasicInfo(userDetails=${userDetails},callback=${callback})`);
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
		logger.debug(`>>> Entering changePassword(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting changePassword(payload=${payload},callback=${callback})`);
        return () => {
            UserService.changePassword(payload, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    forceChangePassword(payload, callback) {
		logger.debug(`>>> Entering forceChangePassword(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting forceChangePassword(payload=${payload},callback=${callback})`);
        return () => {
            UserService.forceChangePassword(payload, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    sendPasswordResetLink(payload, callback) {
		logger.debug(`>>> Entering sendPasswordResetLink(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting sendPasswordResetLink(payload=${payload},callback=${callback})`);
        return () => {
            UserService.sendPasswordResetLink(payload, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deleteUser(payload, filter, callback) {
		logger.debug(`>>> Entering deleteUser(payload=${payload},filter=${filter},callback=${callback})`);
		logger.debug(`<<< Exiting deleteUser(payload=${payload},filter=${filter},callback=${callback})`);
        return (dispatch) => {
            let deletedUser = Object.assign({}, payload.userDetails);
            deletedUser.isDeleted = true;
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
		logger.debug(`>>> Entering eraseUser(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting eraseUser(payload=${payload},callback=${callback})`);
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
		logger.debug(`>>> Entering removeDeletedUsers(callback=${callback})`);
		logger.debug(`<<< Exiting removeDeletedUsers(callback=${callback})`);
        return (dispatch) => {
            UserService.removeDeletedUsers(data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    restoreUser(payload, filter, callback) {
		logger.debug(`>>> Entering restoreUser(payload=${payload},filter=${filter},callback=${callback})`);
		logger.debug(`<<< Exiting restoreUser(payload=${payload},filter=${filter},callback=${callback})`);
        return (dispatch) => {
            let restoredUser = Object.assign({}, payload.userDetails);
            restoredUser.isDeleted = false;
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
		logger.debug(`>>> Entering updateSuperUserStatus(payload=${payload},filter=${filter},callback=${callback})`);
		logger.debug(`<<< Exiting updateSuperUserStatus(payload=${payload},filter=${filter},callback=${callback})`);
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
		logger.debug(`>>> Entering updateAuthorizeStatus(payload=${payload},authorized=${authorized},filter=${filter},callback=${callback})`);
		logger.debug(`<<< Exiting updateAuthorizeStatus(payload=${payload},authorized=${authorized},filter=${filter},callback=${callback})`);
        return (dispatch) => {
            let user = Object.assign({}, payload.userDetails);
            user.authorized = authorized;
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
		logger.debug(`>>> Entering unLockUser(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting unLockUser(payload=${payload},callback=${callback})`);
        return (dispatch) => {
            let user = Object.assign({}, payload.userDetails);
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
		logger.debug(`>>> Entering getUserRoles(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getUserRoles(searchParameters=${searchParameters},callback=${callback})`);
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
		logger.debug('>>> Entering passwordStrength()');
		logger.debug('<<< Exiting passwordStrength()');
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
		logger.debug(`>>> Entering getSuggestRoles(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getSuggestRoles(searchParameters=${searchParameters},callback=${callback})`);
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
		logger.debug(`>>> Entering saveUserRole(payload=${payload},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback})`);
		logger.debug(`<<< Exiting saveUserRole(payload=${payload},notifyUser=${notifyUser},isOwner=${isOwner},callback=${callback})`);
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
		logger.debug(`>>> Entering removeUserRole(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting removeUserRole(payload=${payload},callback=${callback})`);
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
