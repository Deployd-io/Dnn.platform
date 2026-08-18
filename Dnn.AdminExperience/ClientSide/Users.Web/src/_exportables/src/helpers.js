/* eslint-disable no-useless-escape */
import utilities from "utils";
import * as dayjs from "dayjs";
const logger = console;

export function formatDate(dateValue, longformat) {
	logger.debug(`>>> Entering formatDate(dateValue=${dateValue},longformat=${longformat})`);
    if (!dateValue) {
        return "";
    }
    let date = new Date(dateValue);
	logger.debug(`formatDate(dateValue=${dateValue},longformat=${longformat}): date → ${date}`);
    let yearValue = date.getFullYear();
	logger.debug(`formatDate(dateValue=${dateValue},longformat=${longformat}): yearValue → ${yearValue}`);
    if (yearValue < 1900) {
        return "-";
    }

    const localizedFormat = require("dayjs/plugin/localizedFormat");
	logger.debug(`formatDate(dateValue=${dateValue},longformat=${longformat}): localizedFormat → ${localizedFormat}`);
    dayjs.extend(localizedFormat);
    require("dayjs/locale/" + utilities.getCulture().substring(0,2));

	logger.debug(`<<< Exiting formatDate(dateValue=${dateValue},longformat=${longformat})`);
    return dayjs(dateValue).locale(utilities.getCulture().substring(0,2)).format(longformat === true ? "LLL" : "L");
}

export function validateEmail(value) {
    const re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(value);
}

export function formatString() {
	logger.debug('>>> Entering formatString()');
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/{(\d+)}/gi, function (value, index) {
        let argsIndex = parseInt(index) + 1;
		logger.debug(`<<< Exiting function(argsIndex=${argsIndex})`);
	logger.debug('<<< Exiting formatString()');
        return methodsArgs[argsIndex];
    });
}
export function sort(items, column, order) {
	logger.debug(`>>> Entering sort(items=${items},column=${column},order=${order})`);
    order = order === undefined ? "asc" : order;
    items = items.sort(function (a, b) {
        if (a[column] > b[column]) //sort string descending
			logger.debug(`sort(items=${items},column=${column},order=${order}): order → ${order}`);
            return order === "asc" ? 1 : -1;
        if (a[column] < b[column])
            return order === "asc" ? -1 : 1;
	logger.debug(`<<< Exiting sort(order=${order})`);
        return 0;//default return value (no sorting)
    });
    return items;
}

//Reducer helpers
export function updateUsersList(userList, userDetails) {
	logger.debug(`>>> Entering updateUsersList(userList=${userList},userDetails=${userDetails})`);
    let userListCopy = Object.assign([], utilities.getObjectCopy(userList));
    let userDetailsCopy = Object.assign({}, utilities.getObjectCopy(userDetails));
	logger.debug(`updateUsersList(userList=${userList},userDetails=${userDetails}): userDetailsCopy → ${userDetailsCopy}`);

    if (userListCopy.some(user => user.userId === userDetailsCopy.userId)) {
        userListCopy = userListCopy.filter(user => {
            return user.userId !== userDetailsCopy.userId;
        });
    }
    if (!userListCopy.some(role => role.userId === userDetailsCopy.userId)) {
        userListCopy = [userDetailsCopy].concat(userListCopy);
        userListCopy = userListCopy.sort(function (a, b) {
            let createdOnDateA = a.createdOnDate;
            let createdOnDateB = b.createdOnDate;
            if (createdOnDateA > createdOnDateB) //sort string descending
                return -1;
            if (createdOnDateA < createdOnDateB)
                return 1;
            return 0;//default return value (no sorting)
        });
	logger.debug(`<<< Exiting updateUsersList(userList=${userList},userDetails=${userDetails})`);
        return userListCopy;
    }
}
export function removeUser(userList, userId) {
	logger.debug(`>>> Entering removeUser(userList=${userList},userId=${userId})`);
	logger.debug(`<<< Exiting removeUser(userList=${userList},userId=${userId})`);
    let userListCopy = Object.assign([], utilities.getObjectCopy(userList));

logger.debug(`removeUser(userList=${userList},userId=${userId}): userListCopy → ${userListCopy}`);

    if (userListCopy.some(user => user.userId === userId)) {
        userListCopy = userListCopy.filter(user => {
            return user.userId !== userId;
        });
    }
    return userListCopy;
}
export function updateUser(userList, userId, deleteStatus, authorizeStatus, superUserStatus) {
	logger.debug(`>>> Entering updateUser(userList=${userList},userId=${userId},deleteStatus=${deleteStatus},authorizeStatus=${authorizeStatus},superUserStatus=${superUserStatus})`);
    let userListCopy = Object.assign([], utilities.getObjectCopy(userList));
    if (userListCopy.some(user => user.userId === userId)) {
        userListCopy = userListCopy.filter(user => {
            if (user.userId === userId) {
                if (deleteStatus !== undefined && deleteStatus !== null) {
                    user.isDeleted = deleteStatus;
					logger.debug(`updateUser(userList=${userList},userId=${userId},deleteStatus=${deleteStatus},authorizeStatus=${authorizeStatus},superUserStatus=${superUserStatus}): user.isDeleted → ${user.isDeleted}`);
                }
                if (authorizeStatus !== undefined && authorizeStatus !== null) {
                    user.authorized = authorizeStatus;
					logger.debug(`updateUser(userList=${userList},userId=${userId},deleteStatus=${deleteStatus},authorizeStatus=${authorizeStatus},superUserStatus=${superUserStatus}): user.authorized → ${user.authorized}`);
                }
                if (superUserStatus !== undefined && superUserStatus !== null) {
                    user.isSuperUser = superUserStatus;
					logger.debug(`updateUser(userList=${userList},userId=${userId},deleteStatus=${deleteStatus},authorizeStatus=${authorizeStatus},superUserStatus=${superUserStatus}): user.isSuperUser → ${user.isSuperUser}`);
                }
            }
	logger.debug(`updateUser(userList=${userList},userId=${userId},deleteStatus=${deleteStatus},authorizeStatus=${authorizeStatus},superUserStatus=${superUserStatus}): userListCopy → ${userListCopy}`);
	logger.debug(`<<< Exiting updateUser(userList=${userList},userId=${userId},deleteStatus=${deleteStatus},authorizeStatus=${authorizeStatus},superUserStatus=${superUserStatus})`);
            return true;
        });
    }
    return userListCopy;
}
export function removeUserRoleFromList(userRoles, roleId) {
	logger.debug(`>>> Entering removeUserRoleFromList(userRoles=${userRoles},roleId=${roleId})`);
    let userRolesCopy = Object.assign([], utilities.getObjectCopy(userRoles));

	logger.debug(`removeUserRoleFromList(userRoles=${userRoles},roleId=${roleId}): userRolesCopy → ${userRolesCopy}`);
    if (userRolesCopy.some(role => role.roleId === roleId)) {
        userRolesCopy = userRolesCopy.filter(role => {
	logger.debug(`<<< Exiting removeUserRoleFromList(userRoles=${userRoles},roleId=${roleId})`);
            return role.roleId !== roleId;
        });
    }
    return userRolesCopy;
}
export function updateUserRoleList(userRoles, roleUserDetails) {
	logger.debug(`>>> Entering updateUserRoleList(userRoles=${userRoles},roleUserDetails=${roleUserDetails})`);
    let userRolesCopy = Object.assign([], utilities.getObjectCopy(userRoles));
    let roleUserDetailsCopy = Object.assign({}, utilities.getObjectCopy(roleUserDetails));
	logger.debug(`updateUserRoleList(userRoles=${userRoles},roleUserDetails=${roleUserDetails}): roleUserDetailsCopy → ${roleUserDetailsCopy}`);

    if (userRolesCopy.some(role => role.roleId === roleUserDetailsCopy.roleId)) {
        userRolesCopy = userRolesCopy.filter(role => {
            return role.roleId !== roleUserDetailsCopy.roleId;
        });
    }
    if (!userRolesCopy.some(role => role.roleId === roleUserDetailsCopy.roleId)) {
        userRolesCopy = [roleUserDetailsCopy].concat(userRolesCopy);
        userRolesCopy = userRolesCopy.sort(function (a, b) {
            let roleIdA = a.roleId;
            let roleIdB = b.roleId;
            if (roleIdA < roleIdB) //sort string ascending
                return -1;
            if (roleIdA > roleIdB)
                return 1;
            return 0;//default return value (no sorting)
        });
	logger.debug(`<<< Exiting updateUserRoleList(userRoles=${userRoles},roleUserDetails=${roleUserDetails})`);
        return userRolesCopy;
    }
}
