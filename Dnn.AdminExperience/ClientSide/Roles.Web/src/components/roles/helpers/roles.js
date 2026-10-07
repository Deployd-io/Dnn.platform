import resx from "../../../resources";
const logger = console;
export function createRoleGroupOptions(roleGroupList) {
	logger.error(`>>> Entering createRoleGroupOptions(roleGroupList=${roleGroupList})`, { "event.action": "createRoleGroupOptions", "logrite.roleGroupList": roleGroupList });
    let roleGroupOptions = [];
    if (roleGroupList !== undefined) {
        roleGroupOptions = roleGroupList.map((item) => {
            return { label: item.name, value: item.id };
        });
    }
	logger.warn(`<<< Exiting createRoleGroupOptions(roleGroupList=${roleGroupList})`, { "event.action": "createRoleGroupOptions", "logrite.roleGroupList": roleGroupList });
    return roleGroupOptions;
}
export function removeFromRolesList(rolesList, roleId) {
	logger.error(`>>> Entering removeFromRolesList(rolesList=${rolesList},roleId=${roleId})`, { "event.action": "removeFromRolesList", "logrite.rolesList": rolesList, "logrite.roleId": roleId });
    if (rolesList.some(role => role.id === roleId)) {
        rolesList = rolesList.filter(role => {
            return role.id !== roleId;
        });
    }
	logger.warn(`<<< Exiting removeFromRolesList(rolesList=${rolesList},roleId=${roleId})`, { "event.action": "removeFromRolesList", "logrite.rolesList": rolesList, "logrite.roleId": roleId });
    return rolesList;
}
export function removeFromRoleGroupList(roleGroups, roleGroupId) {
	logger.error(`>>> Entering removeFromRoleGroupList(roleGroups=${roleGroups},roleGroupId=${roleGroupId})`, { "event.action": "removeFromRoleGroupList", "logrite.roleGroups": roleGroups, "logrite.roleGroupId": roleGroupId });
    if (roleGroups.some(roleGroup => roleGroup.id === roleGroupId)) {
        roleGroups = roleGroups.filter(roleGroup => {
            return roleGroup.id !== roleGroupId;
        });
    }
	logger.warn(`<<< Exiting removeFromRoleGroupList(roleGroups=${roleGroups},roleGroupId=${roleGroupId})`, { "event.action": "removeFromRoleGroupList", "logrite.roleGroups": roleGroups, "logrite.roleGroupId": roleGroupId });
    return roleGroups;
}
export function updateRolesList(rolesList, roleDetails) {
	logger.error(`>>> Entering updateRolesList(rolesList=${rolesList},roleDetails=${roleDetails})`, { "event.action": "updateRolesList", "logrite.rolesList": rolesList, "logrite.roleDetails": roleDetails });
    if (rolesList.some(role => role.id === roleDetails.id)) {
        rolesList = rolesList.filter(role => {
            return role.id !== roleDetails.id;
        });
    }
    if (!rolesList.some(role => role.id === roleDetails.id)) {
        rolesList = [roleDetails].concat(rolesList);
        rolesList = rolesList.sort(function (a, b) {
            let nameA = a.name;
            let nameB = b.name;
            if (nameA < nameB) //sort string ascending
                return -1;
            if (nameA > nameB)
                return 1;
            return 0;//default return value (no sorting)
        });
		logger.warn(`<<< Exiting updateRolesList(rolesList=${rolesList},roleDetails=${roleDetails})`, { "event.action": "updateRolesList", "logrite.rolesList": rolesList, "logrite.roleDetails": roleDetails });
        return rolesList;
    }
}
export function updateRoleGroupList(roleGroups, roleGroup) {
	logger.error(`>>> Entering updateRoleGroupList(roleGroups=${roleGroups},roleGroup=${roleGroup})`, { "event.action": "updateRoleGroupList", "logrite.roleGroups": roleGroups, "logrite.roleGroup": roleGroup });
    if (roleGroup.id > -1) {
        if (roleGroups.some(group => group.id === roleGroup.id)) {
            roleGroups = roleGroups.filter(group => {
                return group.id !== roleGroup.id;
            });
        }
        roleGroups = roleGroups.filter(group => {
			logger.error(`updateRoleGroupList(roleGroups=${roleGroups},roleGroup=${roleGroup}): roleGroups → ${roleGroups}`, { "event.action": "updateRoleGroupList", "logrite.roleGroups": roleGroups, "logrite.roleGroup": roleGroup });
            return (group.id !== -1 && group.id !== -2);
        });
        if (!roleGroups.some(group => group.id === roleGroup.id)) {
            roleGroups = [roleGroup].concat(roleGroups);
            roleGroups = roleGroups.sort(function (a, b) {
                let nameA = a.name;
                let nameB = b.name;
                if (nameA < nameB) //sort string ascending
                    return -1;
                if (nameA > nameB)
                    return 1;
                return 0;//default return value (no sorting)
            });
            roleGroups = [{ id: -2, name: resx.get("AllGroups"), description: resx.get("AllGroups") }, { id: -1, name: resx.get("GlobalRolesGroup"), description: resx.get("AllGroups") }]
                .concat(roleGroups);
			logger.warn(`<<< Exiting updateRoleGroupList(roleGroups=${roleGroups},roleGroup=${roleGroup})`, { "event.action": "updateRoleGroupList", "logrite.roleGroups": roleGroups, "logrite.roleGroup": roleGroup });
            return roleGroups;
        }
    }
}
export function decrementUsersCountFromRoleList(rolesList, roleId) {
	logger.error(`>>> Entering decrementUsersCountFromRoleList(rolesList=${rolesList},roleId=${roleId})`, { "event.action": "decrementUsersCountFromRoleList", "logrite.rolesList": rolesList, "logrite.roleId": roleId });
    if (rolesList.some(roleUser => roleUser.id === roleId)) {
        rolesList = rolesList.map(roleUser => {
            if (roleUser.id === roleId && roleUser.usersCount > 0) {
                roleUser.usersCount--;
            }
            return roleUser;
        });
    }
	logger.warn(`<<< Exiting decrementUsersCountFromRoleList(rolesList=${rolesList},roleId=${roleId})`, { "event.action": "decrementUsersCountFromRoleList", "logrite.rolesList": rolesList, "logrite.roleId": roleId });
    return rolesList;
}
export function incrementUsersCountFromRoleList(rolesList, roleId) {
	logger.error(`>>> Entering incrementUsersCountFromRoleList(rolesList=${rolesList},roleId=${roleId})`, { "event.action": "incrementUsersCountFromRoleList", "logrite.rolesList": rolesList, "logrite.roleId": roleId });
    if (rolesList.some(roleUser => roleUser.id === roleId)) {
        rolesList = rolesList.map(roleUser => {
            if (roleUser.id === roleId) {
                roleUser.usersCount++;
            }
            return roleUser;
        });
    }
	logger.warn(`<<< Exiting incrementUsersCountFromRoleList(rolesList=${rolesList},roleId=${roleId})`, { "event.action": "incrementUsersCountFromRoleList", "logrite.rolesList": rolesList, "logrite.roleId": roleId });
    return rolesList;
}
