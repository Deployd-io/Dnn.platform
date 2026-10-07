const logger = console;
export function removeRoleUserFromList(roleUsers, userId) {
	logger.error(`>>> Entering removeRoleUserFromList(roleUsers=${roleUsers},userId=${userId})`, { "event.action": "removeRoleUserFromList", "logrite.roleUsers": roleUsers, "user.name": String(userId) });
    if (roleUsers.some(roleGroup => roleGroup.userId === userId)) {
        roleUsers = roleUsers.filter(roleGroup => {
            return roleGroup.userId !== userId;
        });
    }
	logger.warn(`<<< Exiting removeRoleUserFromList(roleUsers=${roleUsers},userId=${userId})`, { "event.action": "removeRoleUserFromList", "logrite.roleUsers": roleUsers, "user.name": String(userId) });
    return roleUsers;
}
export function updateRoleUserList(roleUsers, roleUserDetails) {
	logger.error(`>>> Entering updateRoleUserList(roleUsers=${roleUsers},roleUserDetails=${roleUserDetails})`, { "event.action": "updateRoleUserList", "logrite.roleUsers": roleUsers, "logrite.roleUserDetails": roleUserDetails });
    if (roleUsers.some(role => role.userId === roleUserDetails.userId)) {
        roleUsers = roleUsers.filter(role => {
            return role.userId !== roleUserDetails.userId;
        });
    }
    if (!roleUsers.some(role => role.userId === roleUserDetails.userId)) {
        roleUsers.push(roleUserDetails);
        roleUsers = roleUsers.sort(function (a, b) {
            let userIdA = a.userId;
            let userIdB = b.userId;
            if (userIdA < userIdB) //sort string ascending
                return -1;
            if (userIdA > userIdB)
                return 1;
            return 0;//default return value (no sorting)
        });
		logger.warn(`<<< Exiting updateRoleUserList(roleUsers=${roleUsers},roleUserDetails=${roleUserDetails})`, { "event.action": "updateRoleUserList", "logrite.roleUsers": roleUsers, "logrite.roleUserDetails": roleUserDetails });
        return roleUsers;
    }
}