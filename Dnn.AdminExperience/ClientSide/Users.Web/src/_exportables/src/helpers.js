/* eslint-disable no-useless-escape */
import utilities from "utils";
import * as dayjs from "dayjs";
const logger = console;

export function formatDate(dateValue, longformat) {
	logger.error(`>>> Entering formatDate(dateValue=${dateValue},longformat=${longformat})`, { "event.action": "formatDate", "logrite.dateValue": dateValue, "logrite.longformat": longformat });
    if (!dateValue) {
        return "";
    }
    let date = new Date(dateValue);
	logger.error(`formatDate(dateValue=${dateValue},longformat=${longformat}): date → ${date}`, { "event.action": "formatDate", "logrite.dateValue": dateValue, "logrite.longformat": longformat });
    let yearValue = date.getFullYear();
	logger.error(`formatDate(dateValue=${dateValue},longformat=${longformat}): yearValue → ${yearValue}`, { "event.action": "formatDate", "logrite.dateValue": dateValue, "logrite.longformat": longformat });
    if (yearValue < 1900) {
        return "-";
    }

    const localizedFormat = require("dayjs/plugin/localizedFormat");
	logger.error(`formatDate(dateValue=${dateValue},longformat=${longformat}): localizedFormat → ${localizedFormat}`, { "event.action": "formatDate", "logrite.dateValue": dateValue, "logrite.longformat": longformat });
    dayjs.extend(localizedFormat);
    require("dayjs/locale/" + utilities.getCulture().substring(0,2));

	logger.warn(`<<< Exiting formatDate(dateValue=${dateValue},longformat=${longformat})`, { "event.action": "formatDate", "logrite.dateValue": dateValue, "logrite.longformat": longformat });
    return dayjs(dateValue).locale(utilities.getCulture().substring(0,2)).format(longformat === true ? "LLL" : "L");
}

export function validateEmail(value) {
    const re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(value);
}

export function formatString() {
	logger.error('>>> Entering formatString()', { "event.action": "formatString" });
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/{(\d+)}/gi, function (value, index) {
        let argsIndex = parseInt(index) + 1;
	logger.warn('<<< Exiting formatString()', { "event.action": "formatString" });
        return methodsArgs[argsIndex];
    });
}
export function sort(items, column, order) {
	logger.error(`>>> Entering sort(items=${items},column=${column},order=${order})`, { "event.action": "sort", "logrite.items": items, "logrite.column": column, "logrite.order": order });
    order = order === undefined ? "asc" : order;
    items = items.sort(function (a, b) {
        if (a[column] > b[column]) //sort string descending
            return order === "asc" ? 1 : -1;
        if (a[column] < b[column])
            return order === "asc" ? -1 : 1;
        return 0;//default return value (no sorting)
    });
    return items;
}

//Reducer helpers
export function updateUsersList(userList, userDetails) {
	logger.error(`>>> Entering updateUsersList(userList=${userList},userDetails=${userDetails})`, { "event.action": "updateUsersList", "logrite.userList": userList, "logrite.userDetails": userDetails });
    let userListCopy = Object.assign([], utilities.getObjectCopy(userList));
    let userDetailsCopy = Object.assign({}, utilities.getObjectCopy(userDetails));
	logger.error(`updateUsersList(userList=${userList},userDetails=${userDetails}): userDetailsCopy → ${userDetailsCopy}`, { "event.action": "updateUsersList", "logrite.userList": userList, "logrite.userDetails": userDetails });

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
        return userListCopy;
    }
}
export function removeUser(userList, userId) {
	logger.error(`>>> Entering removeUser(userList=${userList},userId=${userId})`, { "event.action": "removeUser", "logrite.userList": userList, "user.name": String(userId) });
	logger.warn(`<<< Exiting removeUser(userList=${userList},userId=${userId})`, { "event.action": "removeUser", "logrite.userList": userList, "user.name": String(userId) });
    let userListCopy = Object.assign([], utilities.getObjectCopy(userList));

logger.error(`removeUser(userList=${userList},userId=${userId}): userListCopy → ${userListCopy}`, { "event.action": "removeUser", "logrite.userList": userList, "user.name": String(userId) });

    if (userListCopy.some(user => user.userId === userId)) {
        userListCopy = userListCopy.filter(user => {
            return user.userId !== userId;
        });
    }
    return userListCopy;
}
export function updateUser(userList, userId, deleteStatus, authorizeStatus, superUserStatus) {
	logger.error(`>>> Entering updateUser(userList=${userList},userId=${userId},deleteStatus=${deleteStatus},authorizeStatus=${authorizeStatus},superUserStatus=${superUserStatus})`, { "event.action": "updateUser", "logrite.userList": userList, "user.name": String(userId), "logrite.deleteStatus": deleteStatus, "logrite.authorizeStatus": authorizeStatus, "logrite.superUserStatus": superUserStatus });
    let userListCopy = Object.assign([], utilities.getObjectCopy(userList));
    if (userListCopy.some(user => user.userId === userId)) {
        userListCopy = userListCopy.filter(user => {
            if (user.userId === userId) {
                if (deleteStatus !== undefined && deleteStatus !== null) {
                    user.isDeleted = deleteStatus;
                }
                if (authorizeStatus !== undefined && authorizeStatus !== null) {
                    user.authorized = authorizeStatus;
                }
                if (superUserStatus !== undefined && superUserStatus !== null) {
                    user.isSuperUser = superUserStatus;
                }
            }
	logger.error(`updateUser(userList=${userList},userId=${userId},deleteStatus=${deleteStatus},authorizeStatus=${authorizeStatus},superUserStatus=${superUserStatus}): userListCopy → ${userListCopy}`, { "event.action": "updateUser", "logrite.userList": userList, "user.name": String(userId), "logrite.deleteStatus": deleteStatus, "logrite.authorizeStatus": authorizeStatus, "logrite.superUserStatus": superUserStatus });
            return true;
        });
    }
    return userListCopy;
}
export function removeUserRoleFromList(userRoles, roleId) {
	logger.error(`>>> Entering removeUserRoleFromList(userRoles=${userRoles},roleId=${roleId})`, { "event.action": "removeUserRoleFromList", "logrite.userRoles": userRoles, "logrite.roleId": roleId });
    let userRolesCopy = Object.assign([], utilities.getObjectCopy(userRoles));

	logger.error(`removeUserRoleFromList(userRoles=${userRoles},roleId=${roleId}): userRolesCopy → ${userRolesCopy}`, { "event.action": "removeUserRoleFromList", "logrite.userRoles": userRoles, "logrite.roleId": roleId });
    if (userRolesCopy.some(role => role.roleId === roleId)) {
        userRolesCopy = userRolesCopy.filter(role => {
            return role.roleId !== roleId;
        });
    }
	logger.warn(`<<< Exiting removeUserRoleFromList(userRoles=${userRoles},roleId=${roleId})`, { "event.action": "removeUserRoleFromList", "logrite.userRoles": userRoles, "logrite.roleId": roleId });
    return userRolesCopy;
}
export function updateUserRoleList(userRoles, roleUserDetails) {
	logger.error(`>>> Entering updateUserRoleList(userRoles=${userRoles},roleUserDetails=${roleUserDetails})`, { "event.action": "updateUserRoleList", "logrite.userRoles": userRoles, "logrite.roleUserDetails": roleUserDetails });
    let userRolesCopy = Object.assign([], utilities.getObjectCopy(userRoles));
    let roleUserDetailsCopy = Object.assign({}, utilities.getObjectCopy(roleUserDetails));
	logger.error(`updateUserRoleList(userRoles=${userRoles},roleUserDetails=${roleUserDetails}): roleUserDetailsCopy → ${roleUserDetailsCopy}`, { "event.action": "updateUserRoleList", "logrite.userRoles": userRoles, "logrite.roleUserDetails": roleUserDetails });

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
        return userRolesCopy;
    }
}
