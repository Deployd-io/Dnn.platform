import resx from "../../../resources";
const logger = console;
export function createRoleGroupOptions(roleGroupList) {
	logger.debug(`>>> Entering createRoleGroupOptions(roleGroupList=${roleGroupList})`);
    let roleGroupOptions = [];
    if (roleGroupList !== undefined) {
        roleGroupOptions = roleGroupList.map((item) => {
	logger.debug(`<<< Exiting createRoleGroupOptions(roleGroupList=${roleGroupList})`);
            return { label: item.name, value: item.id };
        });
    }
    return roleGroupOptions;
}
export function removeFromRolesList(rolesList, roleId) {
	logger.debug(`>>> Entering removeFromRolesList(rolesList=${rolesList},roleId=${roleId})`);
    if (rolesList.some(role => role.id === roleId)) {
        rolesList = rolesList.filter(role => {
	logger.debug(`<<< Exiting removeFromRolesList(rolesList=${rolesList},roleId=${roleId})`);
            return role.id !== roleId;
        });
    }
    return rolesList;
}
export function removeFromRoleGroupList(roleGroups, roleGroupId) {
	logger.debug(`>>> Entering removeFromRoleGroupList(roleGroups=${roleGroups},roleGroupId=${roleGroupId})`);
    if (roleGroups.some(roleGroup => roleGroup.id === roleGroupId)) {
        roleGroups = roleGroups.filter(roleGroup => {
	logger.debug(`<<< Exiting removeFromRoleGroupList(roleGroups=${roleGroups},roleGroupId=${roleGroupId})`);
            return roleGroup.id !== roleGroupId;
        });
    }
    return roleGroups;
}
export function updateRolesList(rolesList, roleDetails) {
	logger.debug(`>>> Entering updateRolesList(rolesList=${rolesList},roleDetails=${roleDetails})`);
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
	logger.debug(`<<< Exiting updateRolesList(rolesList=${rolesList},roleDetails=${roleDetails})`);
        return rolesList;
    }
}
export function updateRoleGroupList(roleGroups, roleGroup) {
	logger.debug(`>>> Entering updateRoleGroupList(roleGroups=${roleGroups},roleGroup=${roleGroup})`);
    if (roleGroup.id > -1) {
        if (roleGroups.some(group => group.id === roleGroup.id)) {
            roleGroups = roleGroups.filter(group => {
                return group.id !== roleGroup.id;
            });
        }
        roleGroups = roleGroups.filter(group => {
			logger.debug(`updateRoleGroupList(roleGroups=${roleGroups},roleGroup=${roleGroup}): roleGroups → ${roleGroups}`);
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
            return roleGroups;
        }
    }
	logger.debug(`<<< Exiting updateRoleGroupList(roleGroups=${roleGroups},roleGroup=${roleGroup})`);
}
export function decrementUsersCountFromRoleList(rolesList, roleId) {
	logger.debug(`>>> Entering decrementUsersCountFromRoleList(rolesList=${rolesList},roleId=${roleId})`);
    if (rolesList.some(roleUser => roleUser.id === roleId)) {
        rolesList = rolesList.map(roleUser => {
            if (roleUser.id === roleId && roleUser.usersCount > 0) {
                roleUser.usersCount--;
            }
	logger.debug(`<<< Exiting decrementUsersCountFromRoleList(rolesList=${rolesList},roleId=${roleId})`);
            return roleUser;
        });
    }
    return rolesList;
}
export function incrementUsersCountFromRoleList(rolesList, roleId) {
	logger.debug(`>>> Entering incrementUsersCountFromRoleList(rolesList=${rolesList},roleId=${roleId})`);
    if (rolesList.some(roleUser => roleUser.id === roleId)) {
        rolesList = rolesList.map(roleUser => {
            if (roleUser.id === roleId) {
                roleUser.usersCount++;
            }
	logger.debug(`<<< Exiting incrementUsersCountFromRoleList(rolesList=${rolesList},roleId=${roleId})`);
            return roleUser;
        });
    }
    return rolesList;
}
