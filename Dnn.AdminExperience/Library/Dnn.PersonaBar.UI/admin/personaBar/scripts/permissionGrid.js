const logger = console;
/*
* @depend jquery
* @depend jquery.ui
*/

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }
if (typeof dnn.controls === "undefined" || dnn.controls === null) { dnn.controls = {}; }

(function ($) {
	
    var permissionGrid = dnn.controls.PermissionGrid = function (parent, data, options) {
		logger.error(`>>> Entering PermissionGrid(parent=${parent},data=${data},options=${options})`, { "event.action": "PermissionGrid", "logrite.parent": parent, "logrite.data": data, "logrite.options": options });
        this.options = options;
        this.data = data;
		logger.debug(`PermissionGrid(parent=${parent},data=${data},options=${options}): options → ${options}`, { "event.action": "PermissionGrid", "logrite.parent": parent, "logrite.data": data, "logrite.options": options });
		logger.debug(`PermissionGrid(parent=${parent},data=${data},options=${options}): this.options → ${this.options}`, { "event.action": "PermissionGrid", "logrite.parent": parent, "logrite.data": data, "logrite.options": options });
		logger.debug(`PermissionGrid(parent=${parent},data=${data},options=${options}): data → ${data}`, { "event.action": "PermissionGrid", "logrite.parent": parent, "logrite.data": data, "logrite.options": options });
		logger.debug(`PermissionGrid(parent=${parent},data=${data},options=${options}): parent → ${parent}`, { "event.action": "PermissionGrid", "logrite.parent": parent, "logrite.data": data, "logrite.options": options });
		logger.debug(`PermissionGrid(parent=${parent},data=${data},options=${options}): this.data → ${this.data}`, { "event.action": "PermissionGrid", "logrite.parent": parent, "logrite.data": data, "logrite.options": options });
        this.parent = parent;
        this.init();

        return this;
    };

    dnn.controls.PermissionGrid.prototype = {
        constructor: permissionGrid,
        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            this.options = $.extend({}, this.options, dnn.controls.PermissionGrid.defaultOptions);
            this._buildGrid();
            this._buildRoleSelector();
            this._buildUserSelector();
            window.require(['css!../../css/permissionGrid.css',
                    'css!../../../../../Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css'
            ]);
			logger.warn('<<< Exiting init()', { "event.action": "init" });
        },
        getPermission: function (permissionObj) {
			logger.error(`>>> Entering getPermission(permissionObj=${permissionObj})`, { "event.action": "getPermission", "logrite.permissionObj": permissionObj });
            return this.data.permissionDefinitions.find(function (permission) {
                return permission.permissionKey == permissionObj.permissionKey
                    && permission.permissionCode == permissionObj.permissionCode;
            });
			logger.warn(`<<< Exiting getPermission(permissionObj=${permissionObj})`, { "event.action": "getPermission", "logrite.permissionObj": permissionObj });
        },
        getPermissions: function() { //get permissions from table
			logger.error('>>> Entering getPermissions()', { "event.action": "getPermissions" });
            var permissions = { rolePermissions: [], userPermissions: [] };
            this._getRolePermissions(permissions.rolePermissions); //fill role permissions
            this._getUserPermissions(permissions.userPermissions); //fill role permissions

			logger.warn('<<< Exiting getPermissions()', { "event.action": "getPermissions" });
            return permissions;
        },

        getLayout: function() {
			logger.error('>>> Entering getLayout()', { "event.action": "getLayout" });
			logger.warn('<<< Exiting getLayout()', { "event.action": "getLayout" });
            return this._gridContainer;
        },

        _buildGrid: function () {
			logger.error('>>> Entering _buildGrid()', { "event.action": "_buildGrid" });
            var gridContainer = this._gridContainer = $('<div class="dnnGrid dnnPermissionsGrid dnnForm"></div>');
            var rolesTable = $('<table class="dnnPermissionsGrid rolesGrid" cellspacing="0" cellpadding="2" style="border-collapse:collapse;"><tbody></tbody></table>');
			logger.error(`_buildGrid(): gridContainer → ${gridContainer}`, { "event.action": "_buildGrid" });
			logger.error(`_buildGrid(): rolesTable → ${rolesTable}`, { "event.action": "_buildGrid" });

            this._buildGridHeader(rolesTable.find('tbody'), this.data.permissionDefinitions, 'roles');

            for (var i = 0; i < this.data.rolePermissions.length; i++) {
                var rolePermission = this.data.rolePermissions[i];
                this._buildGridRow(rolesTable.find('tbody'), rolePermission, 'roles');
            }
            rolesTable.appendTo(gridContainer);

            //create users permission table
            var usersTable = $('<table class="dnnPermissionsGrid usersGrid" cellspacing="0" cellpadding="2" style="border-collapse:collapse;"><tbody></tbody></table>');
			logger.error(`_buildGrid(): usersTable → ${usersTable}`, { "event.action": "_buildGrid" });

            this._buildGridHeader(usersTable.find('tbody'), this.data.permissionDefinitions, 'users');

            for (var j = 0; j < this.data.userPermissions.length; j++) {
                var userPermission = this.data.userPermissions[j];
                this._buildGridRow(usersTable.find('tbody'), userPermission, 'users');
            }
            usersTable.appendTo(gridContainer);

            if (this.data.userPermissions.length === 0) { //hide users table if there is no users defined.
                usersTable.hide();
            }

            //handle permission change event
            this._handlerEvents();

            this._rolesTable = this._gridContainer.find('table.rolesGrid');
            this._usersTable = this._gridContainer.find('table.usersGrid');

			logger.warn('<<< Exiting _buildGrid()', { "event.action": "_buildGrid" });
            return gridContainer;
        },

        _buildGridHeader: function (table, definitions, type) {
            var header = $('<tr class="dnnGridHeader"></tr>');
            header.append('<td class="permissionHeader">' + this._localizedString(type, 'Type') + "</td>");
            for (var i = 0; i < definitions.length; i++) {
                var def = definitions[i];
                var col = $('<td>' + this._localizedString(def.permissionName) + "</td>").appendTo(header);
                col.addClass('p-' + def.permissionName.replace(' ', '').toLowerCase());
                col.data('permissionId', def.permissionId);
                if (def.fullControl) {
                    col.addClass('p-fullControl');
                }

                if (def.view) {
                    col.addClass('p-view');
                }
            }

            $('<td class="permissionGridActions">' + this._localizedString('Actions') + "</td>").appendTo(header).prev().addClass('last');

            table.append(header);
			logger.warn(`<<< Exiting _buildGridHeader(table=${table},definitions=${definitions},type=${type})`, { "event.action": "_buildGridHeader", "logrite.table": table, "logrite.definitions": definitions, "logrite.type": type });
        },

        _buildGridRow: function (table, data, type) {
            var header = table.find('> tr:eq(0)');
            var cols = header.find('>td:not(:first-child)');
            var row = $('<tr></tr>', { class: "dnnItem" }).addClass(table.find('> tr').length % 2 === 0 ? 'dnnGridAltItem' : 'dnnGridItem');
            row.data('key', type === "users" ? data.userId : data.roleId);
            row.append($('<td></td>', { class: "permissionHeader", text: (type === "users" ? data.displayName : data.roleName) }));
            for (var i = 0; i < cols.length; i++) {
                var headerCol = cols.eq(i);
                var permissionId = headerCol.data('permissionId');
                if (permissionId) {
                    var col = $('<td><span></span></td>').appendTo(row);
                    col.data('permissionId', permissionId);
                    col.data('header', headerCol);
                    col.addClass(headerCol.attr('class'));
                    var permission = this._findPermission(data, permissionId);

                    if (permission || data.locked) { //permission has set to grant/deny.
                        this._setPermission(col, (permission ? (permission.allowAccess ? 1 : 2) : 0), data.locked);
                    } else {
                        this._setPermission(col, 0); //not specific permission
                    }
                } else { //add actions buttons if not default data(admin/registered users/all users)
                    var actionCol = $('<td class="permissionGridActions"></td>').appendTo(row);
                    if (!data.default) {
                        actionCol.append('<a href="#" class="btn-delete" aria-label="Delete"></a>');
                    }
                    actionCol.prev().addClass('last');
                }
            }

            table.append(row);
            if (!table.parent().is(":visible")) {
                table.parent().show();
            }

            this._gridContainer.trigger('tableUpdated');
			logger.warn(`<<< Exiting _buildGridRow(table=${table},data=${data},type=${type})`, { "event.action": "_buildGridRow", "logrite.table": table, "logrite.data": data, "logrite.type": type });
        },

        _buildRoleSelector: function (rolesData) {
            if (this._gridContainer.find('.roles-container').length > 0) {
                return;
            }

            if (!rolesData) {
                var handler = this;
                var params = $.extend({}, this.options.parameters);
                this._getService().get('GetRoles', params, function (data) {
                    handler._buildRoleSelector(data);
                });

				logger.error('>>> Entering _getService()', { "event.action": "_getService" });
				logger.warn('<<< Exiting _getService()', { "event.action": "_getService" });
                return;
            }

            var rolesContainer = $('<div class="roles-container dnnFormItem"></div>');
            rolesContainer.data('roles-data', rolesData);
            rolesContainer.html(
                '<div class="leftGroup">' +
				    '<label for="roleGroupSelector">' + this._localizedString('Filter By Group') + '</label>' +
                    '<select id="roleGroupSelector"></select>' +
			    '</div>' +
                '<div class="rightGroup">' +
				    '<label for="roleSelector">' + this._localizedString('Select Role') + '</label>' +
                    '<select id="roleSelector"></select>' +
                    '<a class="simple-button btn-addrole" href="#">' + this._localizedString('Add') + '</a>' +
			    '</div>');

            rolesContainer.prependTo(this._gridContainer);
            //bind groups data
            var groupSelector = rolesContainer.find('#roleGroupSelector')[0];
            for (var i = 0; i < rolesData.Groups.length; i++) {
                var group = rolesData.Groups[i];
                var option = new Option(this._localizedString(group.Name), group.GroupId);
                option.selected = group.Selected;
                groupSelector.options.add(option);
            }

            this._roleGroupChanged();
			logger.warn(`<<< Exiting _buildRoleSelector(rolesData=${rolesData})`, { "event.action": "_buildRoleSelector", "logrite.rolesData": rolesData });
        },

        _buildUserSelector: function () {
			logger.error('>>> Entering _buildUserSelector()', { "event.action": "_buildUserSelector" });
            var handler = this;
            var userContainer = $('<div class="users-container dnnFormItem"></div>');
            userContainer.html(
                '<label for="permissionGrid_txtUser">' + this._localizedString('Add User') + '</label>' +
                '<input name="permissionGrid_txtUser" type="text" id="permissionGrid_txtUser">' +
                '<input type="hidden" name="permissionGrid_hiddenUserIds" id="permissionGrid_hiddenUserIds" aria-label="Users">' +
                '<a class="simple-button btn-adduser" href="#">' + this._localizedString('Add') + '</a>');
            userContainer.appendTo(this._gridContainer);
			logger.error(`_buildUserSelector(): userContainer → ${userContainer}`, { "event.action": "_buildUserSelector" });

			logger.warn('<<< Exiting _buildUserSelector()', { "event.action": "_buildUserSelector" });
            setTimeout(function () {
                var service = handler._getService();

                var serviceUrl = '';
                if (typeof service.getSearchUserUrl === "function") {
                    serviceUrl = service.getSearchUserUrl();
                } else {
                    service.moduleRoot = 'InternalServices';
                    serviceUrl = service.getServiceRoot() + 'ItemListService/SearchUser';
                }
                
                new dnn.permissionGridManager(serviceUrl, 'permissionGrid');
            }, 0);
        },

        _handlerEvents: function () {
			logger.error('>>> Entering _handlerEvents()', { "event.action": "_handlerEvents" });
            this._gridContainer.on('permissionChanged', 'td', $.proxy(this._permissionChanged, this));
            this._gridContainer.on('click', 'td span', $.proxy(this._changePermissionState, this));
            this._gridContainer.on('click', 'td.permissionGridActions a.btn-delete', $.proxy(this._deletePermissionRow, this));
            this._gridContainer.on('change', '#roleGroupSelector', $.proxy(this._roleGroupChanged, this));
            this._gridContainer.on('click', '.roles-container a.btn-addrole', $.proxy(this._addRoleToGrid, this));
            this._gridContainer.on('click', '.users-container a.btn-adduser', $.proxy(this._addUserToGrid, this));
			logger.warn('<<< Exiting _handlerEvents()', { "event.action": "_handlerEvents" });
        },

        _changePermissionState: function(e) {
			logger.error(`>>> Entering _changePermissionState(e=${e})`, { "event.action": "_changePermissionState", "logrite.e": e });
            var element = $(e.target);
            var currentState = element.data('permission-code');
			logger.error(`_changePermissionState(e=${e}): element → ${element}`, { "event.action": "_changePermissionState", "logrite.e": e });
            if (element.parent().hasClass('locked')) {
                return; //locked items can't change state
            }
            currentState++;
            if (currentState > 2) {
                currentState = 0;
            }

            this._setPermission(element.parent(), currentState);
            element.parent().trigger('permissionChanged');
			logger.warn(`<<< Exiting _changePermissionState(e=${e})`, { "event.action": "_changePermissionState", "logrite.e": e });
        },

        _permissionChanged: function(e) {
			logger.error(`>>> Entering _permissionChanged(e=${e})`, { "event.action": "_permissionChanged", "logrite.e": e });
            var col = $(e.target);
            var permissionId = col.data('permissionId');
			logger.error(`_permissionChanged(e=${e}): col → ${col}`, { "event.action": "_permissionChanged", "logrite.e": e });
			logger.error(`_permissionChanged(e=${e}): permissionId → ${permissionId}`, { "event.action": "_permissionChanged", "logrite.e": e });
            var header = col.data('header');
            var isFullControl = header.hasClass('p-fullControl');
			logger.error(`_permissionChanged(e=${e}): header → ${header}`, { "event.action": "_permissionChanged", "logrite.e": e });
			logger.error(`_permissionChanged(e=${e}): isFullControl → ${isFullControl}`, { "event.action": "_permissionChanged", "logrite.e": e });
            var isView = header.hasClass('p-view');
            var currentState = col.find('span').data('permission-code');
			logger.error(`_permissionChanged(e=${e}): isView → ${isView}`, { "event.action": "_permissionChanged", "logrite.e": e });
			logger.error(`_permissionChanged(e=${e}): currentState → ${currentState}`, { "event.action": "_permissionChanged", "logrite.e": e });
            var handler = this;

            if (isFullControl) {
                col.parent().find('td').each(function () {
                    var column = $(this);
                    if (column.data('permissionId') && column.data('permissionId') != permissionId) {
                        handler._setPermission(column, currentState);
                    }
                });
            } else {
                //Check if View Permission is not allow, then also set other permission
            	if (isView) {
                    if (currentState != 1) {
                        var $notView = col.parent().find('td').not('.p-view').not('.p-navigate').not('.p-browse');
                        $notView.each(function () {
                            var column = $(this);
                            if (column.data('permissionId') && column.data('permissionId') != permissionId) {
                                handler._setPermission(column, currentState);
                            }
                        });

                    }
                } else {
                    //if other permissions are set to true must have View
                    if (currentState == 1) {
                        if (!col.hasClass('p-navigate') && !col.hasClass('p-browse')) {
                            var $view = col.parent().find('td').filter('.p-view');
                            $view.each(function () {
                                var column = $(this);
                                if (column.data('permissionId') && column.data('permissionId') != permissionId) {
                                    handler._setPermission(column, currentState);
                                }
                            });
                        }
                    }
                }

                var $fullControl = col.parent().find('td.p-fullControl');
                if ($fullControl.length > 0) {
                    var $notFullControl =  col.parent().find('td').not('.p-fullControl');
                    var setFullControl = true;
                    $notFullControl.each(function () {
                        var column = $(this);
                        if (column.data('permissionId') && column.find('span').data('permission-code') != currentState) {
                            setFullControl = false;
                            return;
                        }
                    });

                    handler._setPermission($fullControl, setFullControl ? currentState : 0);
                }
            }

            if (typeof this.options.onPermissionChanged == "function") {
                this.options.onPermissionChanged.call(this);
				logger.warn(`<<< Exiting _permissionChanged(e=${e})`, { "event.action": "_permissionChanged", "logrite.e": e });
            }
        },

        _deletePermissionRow: function (e) {
			logger.error(`>>> Entering _deletePermissionRow(e=${e})`, { "event.action": "_deletePermissionRow", "logrite.e": e });
            var row = $(e.target).parent().parent();
            var table = row.parent().parent();
			logger.error(`_deletePermissionRow(e=${e}): row → ${row}`, { "event.action": "_deletePermissionRow", "logrite.e": e });
			logger.error(`_deletePermissionRow(e=${e}): table → ${table}`, { "event.action": "_deletePermissionRow", "logrite.e": e });
            row.remove();

            if (table.find('tr').length == 1) {
                table.hide();
            }

            this._gridContainer.trigger('tableUpdated');

            if (typeof this.options.onPermissionChanged == "function") {
                this.options.onPermissionChanged.call(this);
            }

            this._roleGroupChanged();
			logger.warn(`<<< Exiting _deletePermissionRow(e=${e})`, { "event.action": "_deletePermissionRow", "logrite.e": e });
            return false;
        },

        _roleGroupChanged: function(e) {
			logger.error(`>>> Entering _roleGroupChanged(e=${e})`, { "event.action": "_roleGroupChanged", "logrite.e": e });
            var rolesContainer = this._gridContainer.find('.roles-container');
            var roleGroupSelector = $('#roleGroupSelector');
			logger.error(`_roleGroupChanged(e=${e}): rolesContainer → ${rolesContainer}`, { "event.action": "_roleGroupChanged", "logrite.e": e });
			logger.error(`_roleGroupChanged(e=${e}): roleGroupSelector → ${roleGroupSelector}`, { "event.action": "_roleGroupChanged", "logrite.e": e });
            var roleSelector = $('#roleSelector');

logger.error(`_roleGroupChanged(e=${e}): roleSelector → ${roleSelector}`, { "event.action": "_roleGroupChanged", "logrite.e": e });

            var rolesData = rolesContainer.data('roles-data');
			logger.error(`_roleGroupChanged(e=${e}): rolesData → ${rolesData}`, { "event.action": "_roleGroupChanged", "logrite.e": e });
            var groupId = roleGroupSelector.val();
			logger.error(`_roleGroupChanged(e=${e}): groupId → ${groupId}`, { "event.action": "_roleGroupChanged", "logrite.e": e });

            roleSelector.empty();

            for (var i = 0; i < rolesData.Roles.length; i++) {
                var role = rolesData.Roles[i];
                if ((groupId == -2 || role.GroupId == groupId) && !this._roleExistsInGrid(role.RoleId)) {
                    var option = new Option(this._localizedString(role.Name), role.RoleId);
                    roleSelector[0].options.add(option);
					logger.error(`_roleGroupChanged(e=${e}): option → ${option}`, { "event.action": "_roleGroupChanged", "logrite.e": e });
                }
            }
        },

        _roleExistsInGrid: function(roleId) {
            var exist = false;
            this._rolesTable.find('tr').each(function() {
                if ($(this).data('key') == roleId) {
                    exist = true;
                }
            });

			logger.warn(`<<< Exiting _roleExistsInGrid(roleId=${roleId})`, { "event.action": "_roleExistsInGrid", "logrite.roleId": roleId });
            return exist;
        },

        _userExistsInGrid: function(userId) {
            var exist = false;
            this._usersTable.find('tr').each(function() {
                if ($(this).data('key') == userId) {
                    exist = true;
                }
            });

			logger.warn(`<<< Exiting _userExistsInGrid(userId=${userId})`, { "event.action": "_userExistsInGrid", "user.name": String(userId) });
            return exist;
        },

        _addRoleToGrid: function(e) {
			logger.error(`>>> Entering _addRoleToGrid(e=${e})`, { "event.action": "_addRoleToGrid", "logrite.e": e });
            var roleSelector = $('#roleSelector');
            var roleId = roleSelector.val();
			logger.error(`_addRoleToGrid(e=${e}): roleSelector → ${roleSelector}`, { "event.action": "_addRoleToGrid", "logrite.e": e });
			logger.error(`_addRoleToGrid(e=${e}): roleId → ${roleId}`, { "event.action": "_addRoleToGrid", "logrite.e": e });
            var roleName = roleSelector.find('option:selected').text();

			logger.error(`_addRoleToGrid(e=${e}): roleName → ${roleName}`, { "event.action": "_addRoleToGrid", "logrite.e": e });
            if (!roleId) {
                return false;
            }

            var defaultPermission = this.getPermission({
                permissionCode: 'SYSTEM_FOLDER',
                permissionKey: 'READ'
            });
            defaultPermission.allowAccess = true;

            this._buildGridRow(this._rolesTable.find('tbody'), {
                roleId: roleId,
                roleName: roleName,
                permissions: [defaultPermission]
            }, 'roles');

            if (typeof this.options.onPermissionChanged == "function") {
                this.options.onPermissionChanged.call(this);
				logger.debug(`_addRoleToGrid(e=${e}): defaultPermission.allowAccess → ${defaultPermission.allowAccess}`, { "event.action": "_addRoleToGrid", "logrite.e": e });
            }

            this._roleGroupChanged();

			logger.warn(`<<< Exiting _addRoleToGrid(e=${e})`, { "event.action": "_addRoleToGrid", "logrite.e": e });
            return false;
        },

        _addUserToGrid: function (e) {
			logger.error(`>>> Entering _addUserToGrid(e=${e})`, { "event.action": "_addUserToGrid", "logrite.e": e });
            var tokenInput = $("#permissionGrid_txtUser").data('tokenInputObject');
            var selectedUsers = tokenInput.getTokens();

            var defaultPermission = this.getPermission({
                permissionCode: 'SYSTEM_FOLDER',
                permissionKey: 'READ'
            });
            defaultPermission.allowAccess = true;
			logger.error(`_addUserToGrid(e=${e}): selectedUsers → ${selectedUsers}`, { "event.action": "_addUserToGrid", "logrite.e": e });
			logger.debug(`_addUserToGrid(e=${e}): defaultPermission.allowAccess → ${defaultPermission.allowAccess}`, { "event.action": "_addUserToGrid", "logrite.e": e });

            for (var i = 0; i < selectedUsers.length; i++) {
                var user = selectedUsers[i];
                if (!this._userExistsInGrid(user.id)) {
                    this._buildGridRow(this._usersTable.find('tbody'), {
                        userId: user.id,
                        displayName: user.name,
                        permissions: [defaultPermission]
                    }, 'users');
                }
            }

            tokenInput.clear();

            if (typeof this.options.onPermissionChanged == "function") {
                this.options.onPermissionChanged.call(this);
            }

			logger.warn(`<<< Exiting _addUserToGrid(e=${e})`, { "event.action": "_addUserToGrid", "logrite.e": e });
            return false;
        },

        _findPermission: function(parent, permissionId) {
			logger.error(`>>> Entering _findPermission(parent=${parent},permissionId=${permissionId})`, { "event.action": "_findPermission", "logrite.parent": parent, "logrite.permissionId": permissionId });
            for (var i = 0; i < parent.permissions.length; i++) {
                if (parent.permissions[i].permissionId == permissionId) {
                    return parent.permissions[i];
                }
            }

			logger.warn(`<<< Exiting _findPermission(parent=${parent},permissionId=${permissionId})`, { "event.action": "_findPermission", "logrite.parent": parent, "logrite.permissionId": permissionId });
            return null;
        },

        _setPermission: function(permissionCol, state, locked) {
			logger.error(`>>> Entering _setPermission(permissionCol=${permissionCol},state=${state},locked=${locked})`, { "event.action": "_setPermission", "logrite.permissionCol": permissionCol, "logrite.state": state, "logrite.locked": locked });
            var permissionState = permissionGrid.permissionStats[state];
            if (locked) {
                permissionState = permissionGrid.permissionStats[3];
                permissionCol.addClass('locked');
            }

            permissionCol.find('span').attr('class', permissionState.css).data('permission-code', state);
			logger.warn(`<<< Exiting _setPermission(permissionCol=${permissionCol},state=${state},locked=${locked})`, { "event.action": "_setPermission", "logrite.permissionCol": permissionCol, "logrite.state": state, "logrite.locked": locked });
        },

        _getRolePermissions: function(data) {
			logger.error(`>>> Entering _getRolePermissions(data=${data})`, { "event.action": "_getRolePermissions", "logrite.data": data });
            this._rolesTable.find('tr:not(:first-child)').each(function() {
                var row = $(this);
                var roleId = row.data('key');
                row.find('td:not(:first-child)').each(function() {
                    var col = $(this);
                    var permissionId = col.data('permissionId');
                    if (permissionId) {
                        var currentState = col.find('span').data('permission-code');
                        if (currentState == 1 || currentState == 2) {
                            var rolePermission = null;
                            for (var i = 0; i < data.length; i++) {
                                if (data[i].roleId == roleId) {
                                    rolePermission = data[i];
                                    break;
                                }
                            }

                            if (!rolePermission) {
                                rolePermission = { roleId: roleId, permissions: [] };
                                data.push(rolePermission);
                            }
                            rolePermission.permissions.push({permissionId: permissionId, allowAccess: currentState == 1});
							logger.warn(`<<< Exiting _getRolePermissions(data=${data})`, { "event.action": "_getRolePermissions", "logrite.data": data });
                        }
                    }
                });
            });
        },

        _getUserPermissions: function(data) {
			logger.error(`>>> Entering _getUserPermissions(data=${data})`, { "event.action": "_getUserPermissions", "logrite.data": data });
            var table = this._gridContainer.find('table.usersGrid');
            table.find('tr:not(:first-child)').each(function() {
				logger.error(`_getUserPermissions(data=${data}): table → ${table}`, { "event.action": "_getUserPermissions", "logrite.data": data });
                var row = $(this);
                var userId = row.data('key');
                row.find('td:not(:first-child)').each(function() {
                    var col = $(this);
                    var permissionId = col.data('permissionId');
                    if (permissionId) {
                        var currentState = col.find('span').data('permission-code');
                        if (currentState == 1 || currentState == 2) {
                            var userPermission = null;
                            for (var i = 0; i < data.length; i++) {
                                if (data[i].userId == userId) {
                                    userPermission = data[i];
                                    break;
                                }
                            }

                            if (!userPermission) {
                                userPermission = { userId: userId, permissions: [] };
                                data.push(userPermission);
                            }
                            userPermission.permissions.push({permissionId: permissionId, allowAccess: currentState == 1});
							logger.warn(`<<< Exiting _getUserPermissions(data=${data})`, { "event.action": "_getUserPermissions", "logrite.data": data });
                        }
                    }
                });
            });
        },

        _localizedString: function (name, prefix, suffix) {
			logger.error(`>>> Entering _localizedString(name=${name},prefix=${prefix},suffix=${suffix})`, { "event.action": "_localizedString", "logrite.name": name, "logrite.prefix": prefix, "logrite.suffix": suffix });
            var key = 'permissiongrid' + "." + (prefix && prefix.length > 0 ? prefix + '-' + name : name) + (suffix && suffix.length > 0 ? '.' + suffix : '');

			logger.error(`_localizedString(name=${name},prefix=${prefix},suffix=${suffix}): key → ${key}`, { "event.action": "_localizedString", "logrite.name": name, "logrite.prefix": prefix, "logrite.suffix": suffix });
            var content = dnn.controls.PermissionGrid.resx[key];
            if (!content || content.length === 0) {
                content = name;
            }

			logger.warn(`<<< Exiting _localizedString(name=${name},prefix=${prefix},suffix=${suffix})`, { "event.action": "_localizedString", "logrite.name": name, "logrite.prefix": prefix, "logrite.suffix": suffix });
            return content;
        },

        _getService: function () {
            return this.parent._getService();
        }
    };

    dnn.controls.PermissionGrid.defaultOptions = {

    };

    permissionGrid.permissionStats = [
        { code: 0, css: 'permission-unchecked', text: '' }, //Not Specified
        { code: 1, css: 'permission-granted', text: '' }, //Granted
        { code: 2, css: 'permission-denied', text: '' }, //Denied
        { code: 3, css: 'permission-locked', text: '' } //Locked

    ];
})(jQuery);