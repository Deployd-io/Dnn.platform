const logger = console;
'use strict';
define(['jquery', 'knockout', 'main/pager', 'main/validator', 'main/config', 'main/PersonaBarDialog', 'jquery.easydropdown.min', 'dnn.jquery'],
    function ($, ko, pager, validator, cf, personaBarDialog) {
        var utility = null;
        var config = cf.init();
        var viewModel = {
            connections: ko.observableArray([])
        };

        ko.protectedObservable = function (initialValue) {
			logger.error(`>>> Entering protectedObservable(initialValue=${initialValue})`, { "event.action": "protectedObservable", "logrite.initialValue": initialValue });
            var actualValue = ko.observable(initialValue),
                tempValue = ko.observable(initialValue);
				logger.error(`protectedObservable(initialValue=${initialValue}): actualValue → ${actualValue}`, { "event.action": "protectedObservable", "logrite.initialValue": initialValue });

            var result = tempValue;
			logger.error(`protectedObservable(initialValue=${initialValue}): result → ${result}`, { "event.action": "protectedObservable", "logrite.initialValue": initialValue });
            result.commit = function () {
				logger.error('>>> Entering commit()', { "event.action": "commit" });
                if (tempValue() !== actualValue()) {
                    actualValue(tempValue().trim());
					logger.warn('<<< Exiting commit()', { "event.action": "commit" });
                }
            };
            result.reset = function () {
				logger.error('>>> Entering reset()', { "event.action": "reset" });
                actualValue.valueHasMutated();
                tempValue(actualValue());
				logger.warn('<<< Exiting reset()', { "event.action": "reset" });
            };

            return result;
        };

        var wrapConnection = function (conn) {
            var koConfigurations = [];
            var secureSettingsValue = conn.configurations["SecureSettings"];
            var secureSettings = secureSettingsValue ? secureSettingsValue.split(";") : new Array();
            $.each(conn.configurations, function (key, value) {
                if (key === "SecureSettings") {
                    return;
                }
                var o = {};
                o.name = key;
                o.type = secureSettings.indexOf(key) > -1 ? "password" : "text";
                o.localizedName = key;
                o.value = ko.protectedObservable(value);
                koConfigurations.push(o);
            });

            var koConnectionObject = {
                name: conn.name,
                displayName: ko.observable(conn.displayName),
                wrapperId: ko.computed(function() {
                    return 'connector-' + conn.name.replace(/ /g, '');
                }),
                previousDisplayName: ko.observable(conn.displayName),
                icon: conn.icon,
                supportsMultiple: conn.supportsMultiple,
                previousConnectionState: ko.observable(conn.connected),
                connected: ko.observable(conn.connected),
                id: conn.id,
                uniqueNumber: Math.random() * Date.now(),
                faviconCss: ko.computed(function () {
                    var css = 'socialnetwork-favicon ' + conn.name;
                    return css;
                }),
                initialObject: conn,
                onOpenEdit: onOpenEdit,
                onCancel: onCancel,
                onToggleEditeMode: onToggleEditeMode,
                onDelete: onDelete,
                open: ko.observable(false),
                inEdit: ko.observable(false),
                editMode: ko.observable(false),
                componentInitialized: ko.observable(false),
                editLayout: ko.observable(''),
                connector: ko.observable(),
                buttons: ko.observableArray([]),

                edit: function (item, e) {
					logger.error(`>>> Entering edit(item=${item},e=${e})`, { "event.action": "edit", "logrite.item": item, "logrite.e": e });
                    e.preventDefault();
                    var btn = $(e.target);
					logger.error(`edit(item=${item},e=${e}): btn → ${btn}`, { "event.action": "edit", "logrite.item": item, "logrite.e": e });
                    var editRow = btn.parent().parent().next().find('td > div');
					logger.error(`edit(item=${item},e=${e}): editRow → ${editRow}`, { "event.action": "edit", "logrite.item": item, "logrite.e": e });
                    var showEditRow = function () {
						logger.error('>>> Entering showEditRow()', { "event.action": "showEditRow" });
                        $.each(viewModel.connections(), function (i, v) {
                            v.isOpen(false);
                            $.each(v.connections(), function (i, item) {
                                item.inEdit(false);
                                item.open(false);
                            });
                        });
                        item.inEdit(true);

                        editRow.slideDown(200, 'linear', function () {
                            $(this).find('input:first').select();
                        });
						logger.warn('<<< Exiting showEditRow()', { "event.action": "showEditRow" });
                    };
                    var collapsedEditRow = $('#connectionstbl tr.edit-row > td > div.edit-form:visible');
					logger.error(`edit(item=${item},e=${e}): collapsedEditRow → ${collapsedEditRow}`, { "event.action": "edit", "logrite.item": item, "logrite.e": e });
                    if (collapsedEditRow.length > 0) {
                        collapsedEditRow.slideUp(200, 'linear', showEditRow);
                    } else {
                        showEditRow();
						logger.warn(`<<< Exiting edit(item=${item},e=${e})`, { "event.action": "edit", "logrite.item": item, "logrite.e": e });
                    }
                },

                cancel: function (item, e) {
					logger.error(`>>> Entering cancel(item=${item},e=${e})`, { "event.action": "cancel", "logrite.item": item, "logrite.e": e });
                    if (e) {
                        e.preventDefault();
                    }
                    $.each(item.configurations, function (i, v) {
                        var name = v.name.toLowerCase();
                        v.value.reset();
                        if (item[name] && typeof item[name] === "object" && typeof item[name].reset === "function") {
                            item[name].reset();
                        }
                    });
                    var connected = item.previousConnectionState() && (item.supportsMultiple ? !!item.id : true);
					logger.error(`cancel(item=${item},e=${e}): connected → ${connected}`, { "event.action": "cancel", "logrite.item": item, "logrite.e": e });
                    item.connected(connected);
                    item.displayName(item.previousDisplayName());
                    item.inEdit(false);
                    viewModel.connections().forEach(function (v) {
                        v.connections().forEach(function (t) {
                            t.open(false);
                        });

                    });
                    if (e) {
                        $(e.target).parents('div.edit-form').slideUp(200, 'linear', function () {
                            viewModel.connections().forEach(function (v) {
                                if (v.connections().length === 1) {
                                    v.isOpen(false);
									logger.warn(`<<< Exiting cancel(item=${item},e=${e})`, { "event.action": "cancel", "logrite.item": item, "logrite.e": e });
                                }
                            })
                        });
                    }
                },
                save: function (item, e, cb) {
					logger.error(`>>> Entering save(item=${item},e=${e},cb=${cb})`, { "event.action": "save", "logrite.item": item, "logrite.e": e, "logrite.cb": cb });
                    e.preventDefault();
                    item.connected(false);
					logger.warn(`<<< Exiting save(item=${item},e=${e},cb=${cb})`, { "event.action": "save", "logrite.item": item, "logrite.e": e, "logrite.cb": cb });
                    var container = $(e.target).parents('div.edit-form');
					logger.error(`save(item=${item},e=${e},cb=${cb}): container → ${container}`, { "event.action": "save", "logrite.item": item, "logrite.e": e, "logrite.cb": cb });
                    saveConnection(item, function (success, validated, id) {
                        utility.notify(utility.resx.Connectors.txt_Saved, true);
                        var connected = isConnectionConnected(item);
                        item.connected(connected);
                        if (success) {
                            item.previousConnectionState(connected);
                            item.previousDisplayName(item.displayName());
                            if (id) {
                                item.id = id;
                            }
                            $.each(item.configurations, function (i, v) {
                                v.value.commit();
                            });
                        }

                        if (typeof cb == "function") {
                            cb(item, true);
                        }
                    }, function (xhr, status) {
                        utility.notifyError(status || 'Failed...');
                        if (typeof cb == "function") {
                            cb(item, false);
                        }
                    });
                },
                configurations: koConfigurations
            };

            var pluginFolder = conn.pluginFolder;
            var pluginJs = 'rootPath' + pluginFolder + 'scripts/connector';
            var pluginCss = 'css!rootPath' + pluginFolder + 'connector.css';
            var pluginHtml = 'text!rootPath' + pluginFolder + 'connector.htm';
            require([pluginJs, pluginHtml, pluginCss], function (connector, layout) {
                loadLocalizedStrings(conn.name, function (data) {
                    for (var i = 0; i < koConfigurations.length; i++) {
                        if (typeof data[koConfigurations[i].name] != "undefined") {
                            koConfigurations[i].localizedName = data[koConfigurations[i].name];
                        }
                    }

                    koConnectionObject.resx = data;
                    koConnectionObject.connector(connector);
                    connector.init(koConnectionObject, connectionHelper, pluginFolder, utility);
                    if (!ko.components.isRegistered(koConnectionObject.uniqueNumber + "component")) {
                        ko.components.register(koConnectionObject.uniqueNumber + "component", { template: layout });
                        koConnectionObject.componentInitialized(true);

                        (function (connection) {
                            setTimeout(function () {
                                var $root = $('#' + connection.wrapperId());
                                $root.find('input, select').each(function () {
                                    var $this = $(this);
                                    if (!$this.attr('id') || !$root.find('label[for="' + $this.attr('id') + '"]').length) {
                                        $this.attr('aria-label', 'Value');
                                    }
                                });

                                $root.find('a').each(function () {
                                    var $this = $(this);
                                    if (!$this.text()) {
                                        $this.attr('aria-label', 'Link');
                                    }
                                });
                            }, 100);
                        })(koConnectionObject);
                    }
                    // koConnectionObject.editLayout(layout);

                    //custom buttons
                    var buttons = getDefaultButtons();
                    if (typeof connector.getActionButtons == "function") {
                        buttons = connector.getActionButtons();
                    }
                    for (var i = 0; i < buttons.length; i++) {
                        koConnectionObject.buttons.push(buttons[i]);
                    }


                    $('#' + koConnectionObject.wrapperId()).find('.edit-fields').children().each(function () {
                        ko.applyBindings(koConnectionObject, this);
                    });
                });
            });

			logger.warn(`<<< Exiting wrapConnection(conn=${conn})`, { "event.action": "wrapConnection", "logrite.conn": conn });
            return koConnectionObject;
        };

        var isConnectionConnected = function (item) {
			logger.error(`>>> Entering isConnectionConnected(item=${item})`, { "event.action": "isConnectionConnected", "logrite.item": item });
            if (item.name === "Mailchimp" || item.name === "GoogleTagManager" || item.name === "GoogleAnalytics") {
                return !!item.configurations[0].value();
            }
            if (item.name === "Zendesk") {
                return item.configurations[0].value() && item.configurations[1].value() && item.configurations[2].value();
            }
            return item.configurations.every(function (config) {
			logger.warn(`<<< Exiting isConnectionConnected(item=${item})`, { "event.action": "isConnectionConnected", "logrite.item": item });
                return !!config.value() || config.name == "Id" || config.name == "Connected";
            });
        };

        var getDefaultButtons = function () {
			logger.error('>>> Entering getDefaultButtons()', { "event.action": "getDefaultButtons" });
            return [
                {
                    className: 'primarybtn',
                    text: viewModel.resx.btn_Save,
                    action: function (conn, e) {
                        conn.save(conn, e);
						logger.warn(`<<< Exiting action(conn=${conn},e=${e})`, { "event.action": "action", "logrite.conn": conn, "logrite.e": e });
                    }
                }
            ];
			logger.warn('<<< Exiting getDefaultButtons()', { "event.action": "getDefaultButtons" });
        }

        var validateDisplayName = function (conn) {
			logger.error(`>>> Entering validateDisplayName(conn=${conn})`, { "event.action": "validateDisplayName", "logrite.conn": conn });
            var displayName = conn.displayName();
            var noSpecialChar = /^[a-zA-Z0-9- ]*$/.test(displayName);
			logger.error(`validateDisplayName(conn=${conn}): displayName → ${displayName}`, { "event.action": "validateDisplayName", "logrite.conn": conn });
			logger.error(`validateDisplayName(conn=${conn}): noSpecialChar → ${noSpecialChar}`, { "event.action": "validateDisplayName", "logrite.conn": conn });
            var isNotEmpty = !!displayName.trim();
			logger.error(`validateDisplayName(conn=${conn}): isNotEmpty → ${isNotEmpty}`, { "event.action": "validateDisplayName", "logrite.conn": conn });
            return isNotEmpty && noSpecialChar;
        };

        var isDisplayNameUnique = function (conn) {
			logger.error(`>>> Entering isDisplayNameUnique(conn=${conn})`, { "event.action": "isDisplayNameUnique", "logrite.conn": conn });
            var displayName = conn.displayName();
            var isUnique = viewModel.connections().every(function (v) {
                return v.connections().every(function (t) {
                    return !(t.displayName().toLowerCase() == displayName.toLowerCase() && t.uniqueNumber != conn.uniqueNumber);
                });
            });
            return isUnique;
        };

        var saveConnection = function (conn, success, forceSave) {
            viewModel.errorMessage('');

            if (conn.connector()) {
                conn.connector().onSave(conn);
            }
            if (conn.supportsMultiple) {
                var isDisplayNameValid = validateDisplayName(conn);
                var isUniqueName = isDisplayNameUnique(conn);
                if (!isDisplayNameValid) {
                    return utility.notifyError(viewModel.resx.txt_DisplayNameIsNotValid);
                }
                if (!isUniqueName) {
                    return utility.notifyError(viewModel.resx.txt_DisplayNameIsNotUnique);
                }
            }

            utility.sf.moduleRoot = 'personaBar';
            utility.sf.controller = 'Connectors';
            var postData = {
                name: conn.name,
                displayName: conn.displayName(),
                id: conn.id,
                configurations: []
            };

            //if fields not all empty or all filled, then disable button and not save
            var allEmpty = true;
            var allFilled = true;

            for (var i = 0; i < conn.configurations.length; i++) {
                var c = conn.configurations[i];
                var value = c.value();
                postData.configurations.push({
                    name: c.name,
                    value: value
                });

                if (value != '') {
                    allEmpty = false;
                } else {
                    allFilled = false;
                }
            }

            var primaryButtons = $('#' + conn.wrapperId()).find('a.primarybtn');
            if (!allEmpty && !allFilled && !forceSave) {
                primaryButtons.addClass('disabledbtn');
                return;
            } else {
                primaryButtons.removeClass('disabledbtn');
            }

            viewModel.errorMessage('');
            primaryButtons.html(viewModel.resx.btn_Checking);
            if (postData.name === "UNC" && !postData.configurations[0].value) {
                return onDelete(conn);
            }
            if (conn.supportsMultiple && !postData.configurations[0].value && (!postData.configurations[1] || !postData.configurations[1].value)) {
                return onDelete(conn);
            }
            utility.sf.post('SaveConnection', postData, function (d) {
                primaryButtons.html(viewModel.resx.btn_Save);
                if (d) {
                    if (d.Success) {
                        success(d.Success, d.Validated, d.Id);
                    } else if (d.Message) {
                        utility.notifyError(d.Message || 'Failed...');
                    } else {
                        utility.notifyError('Failed...');
                    }
                } else {
                    utility.notifyError('Failed...');
                }
            }, onSaveFailed.bind(this, conn));
			logger.warn(`<<< Exiting saveConnection(conn=${conn},success=${success},forceSave=${forceSave})`, { "event.action": "saveConnection", "logrite.conn": conn, "logrite.success": success, "logrite.forceSave": forceSave });
        };

        var onSaveFailed = function (conn, xhr, status) {
            var primaryButtons = $('#' + conn.wrapperId()).find('a.primarybtn');
            primaryButtons.html(viewModel.resx.btn_Save);
            if (xhr.responseJSON) {
                utility.notifyError(xhr.responseJSON.Message || status || 'Failed...');
            } else {
                utility.notifyError(status || 'Failed...');
            }
			logger.warn(`<<< Exiting onSaveFailed(conn=${conn},xhr=${xhr},status=${status})`, { "event.action": "onSaveFailed", "logrite.conn": conn, "logrite.xhr": xhr, "logrite.status": status });
        };

        var onAddNew = function (item, e) {
			logger.error(`>>> Entering onAddNew(item=${item},e=${e})`, { "event.action": "onAddNew", "logrite.item": item, "logrite.e": e });
            item.isOpen(true);
            var connections = item.connections();
			logger.error(`onAddNew(item=${item},e=${e}): connections → ${connections}`, { "event.action": "onAddNew", "logrite.item": item, "logrite.e": e });
            var newConnector = connections.find(function (conn) { return !conn.id });
			logger.error(`onAddNew(item=${item},e=${e}): newConnector → ${newConnector}`, { "event.action": "onAddNew", "logrite.item": item, "logrite.e": e });

            if (newConnector && connections.length > 1) {
                closeAllSubConnectors();
                return newConnector.open(true);
            }
            var configurationKeys = Object.keys(connections[0].initialObject.configurations);
			logger.error(`onAddNew(item=${item},e=${e}): configurationKeys → ${configurationKeys}`, { "event.action": "onAddNew", "logrite.item": item, "logrite.e": e });
            var newConfigurations = {};
            configurationKeys.forEach(function (key) {
                if (key === "AuthPage") {
                    newConfigurations[key] = connections[0].initialObject.configurations[key];
                } else {
                    newConfigurations[key] = "";
                }
            });
            var displayName = e === true ? item.displayName : getNewName(item);
			logger.error(`onAddNew(item=${item},e=${e}): displayName → ${displayName}`, { "event.action": "onAddNew", "logrite.item": item, "logrite.e": e });
            closeAllSubConnectors();
            var newConnection = Object.assign(connections[0].initialObject, { id: null, configurations: newConfigurations, displayName: displayName, open: true });
			logger.error(`onAddNew(item=${item},e=${e}): newConnection → ${newConnection}`, { "event.action": "onAddNew", "logrite.item": item, "logrite.e": e });
            var newConn = wrapConnection(newConnection);
			logger.error(`onAddNew(item=${item},e=${e}): newConn → ${newConn}`, { "event.action": "onAddNew", "logrite.item": item, "logrite.e": e });
            newConn.connected(false);
            newConn.previousConnectionState(false);
            newConn.open(true);
            connections.push(newConn);
            item.connections(connections);
            return newConn;
        };

        var getNewName = function (item) {
			logger.error(`>>> Entering getNewName(item=${item})`, { "event.action": "getNewName", "logrite.item": item });
            var connections = item.connections();
            var index = connections.length;
			logger.error(`getNewName(item=${item}): connections → ${connections}`, { "event.action": "getNewName", "logrite.item": item });
            var newName = item.displayName + " - " + index;
            while (connections.some(function (conn) { return conn.displayName() === newName })) {
                newName = item.displayName + " - " + ++index;
            }
            return newName;
        }

        var closeAllSubConnectors = function () {
			logger.error('>>> Entering closeAllSubConnectors()', { "event.action": "closeAllSubConnectors" });
            viewModel.connections().forEach(function (v) {
                v.connections().forEach(function (t) {
                    t.open(false);
                });
            });
        }

        var onOpen = function (item, e) {
			logger.error(`>>> Entering onOpen(item=${item},e=${e})`, { "event.action": "onOpen", "logrite.item": item, "logrite.e": e });
            var isOpen = item.isOpen();
            viewModel.connections().forEach(function (v) {
				logger.error(`onOpen(item=${item},e=${e}): isOpen → ${isOpen}`, { "event.action": "onOpen", "logrite.item": item, "logrite.e": e });
                if (v.name !== item.name) {
                    v.isOpen(false);
                } else {
                    v.height(40 + v.connections().length * 53 + 'px');
                    v.isOpen(!isOpen);
                }
            });
            $.each(viewModel.connections(), function (i, v) {
                $.each(v.connections(), function (i, item) {
                    item.inEdit(false);
                });
            });
            var collapsedEditRow = $('#connectionstbl tr.edit-row > td > div.edit-form:visible');
			logger.error(`onOpen(item=${item},e=${e}): collapsedEditRow → ${collapsedEditRow}`, { "event.action": "onOpen", "logrite.item": item, "logrite.e": e });
            if (collapsedEditRow.length > 0) {
                collapsedEditRow.slideUp(200, 'linear');
            }
            viewModel.connections().forEach(function (v) {
                v.connections().forEach(function (t) {
                    t.open(false);
                });
            });
        };

        var onOpenEdit = function (item, e) {
			logger.error(`>>> Entering onOpenEdit(item=${item},e=${e})`, { "event.action": "onOpenEdit", "logrite.item": item, "logrite.e": e });
            var open = !item.open();
            viewModel.connections().forEach(function (v) {
				logger.error(`onOpenEdit(item=${item},e=${e}): open → ${open}`, { "event.action": "onOpenEdit", "logrite.item": item, "logrite.e": e });
                v.connections().forEach(function (t) {
                    t.open(t.uniqueNumber === item.uniqueNumber ? open : false);
                });
            });
        };

        var onCancel = function (item, e) {
			logger.error(`>>> Entering onCancel(item=${item},e=${e})`, { "event.action": "onCancel", "logrite.item": item, "logrite.e": e });
            item.onOpenEdit(item);
            item.cancel(item, e);
			logger.warn(`<<< Exiting onCancel(item=${item},e=${e})`, { "event.action": "onCancel", "logrite.item": item, "logrite.e": e });
        }

        var onToggleEditeMode = function (item, e) {
			logger.error(`>>> Entering onToggleEditeMode(item=${item},e=${e})`, { "event.action": "onToggleEditeMode", "logrite.item": item, "logrite.e": e });
            item.editMode(!item.editMode);
			logger.warn(`<<< Exiting onToggleEditeMode(item=${item},e=${e})`, { "event.action": "onToggleEditeMode", "logrite.item": item, "logrite.e": e });
        }

        var onDelete = function (item, e) {
			logger.error(`>>> Entering onDelete(item=${item},e=${e})`, { "event.action": "onDelete", "logrite.item": item, "logrite.e": e });
            if (!item.id) {
				logger.warn(`<<< Exiting onDelete(item=${item},e=${e})`, { "event.action": "onDelete", "logrite.item": item, "logrite.e": e });
                return removeItem(item, e);
            }
            utility.sf.moduleRoot = 'personaBar';
            utility.sf.controller = 'Connectors';
            var postData = { id: item.id, name: item.name };
            utility.sf.post('DeleteConnection', postData, function (data) {
                utility.notify(utility.resx.Connectors.txt_Deleted, true);
                removeItem(item, e);
            });
        };

        var removeItem = function (item, e) {
			logger.error(`>>> Entering removeItem(item=${item},e=${e})`, { "event.action": "removeItem", "logrite.item": item, "logrite.e": e });
            var connectionCategory = viewModel.connections().find(function (conn) {
                return conn.connections().some(function (test) {
                    return test.uniqueNumber == item.uniqueNumber;
                });
            });
            if (ko.components.isRegistered(item.uniqueNumber + "component")) {
                ko.components.unregister(item.uniqueNumber + "component");
            }
            var connections = connectionCategory.connections().filter(function (conn) {
                return conn.uniqueNumber != item.uniqueNumber;
            });
            if (!connections.length) {
                var newConn = onAddNew(connectionCategory, true);
				logger.error(`removeItem(item=${item},e=${e}): newConn → ${newConn}`, { "event.action": "removeItem", "logrite.item": item, "logrite.e": e });
                connections.push(newConn);
                connectionCategory.connections(connections);
                $('#' + newConn.wrapperId()).slideUp(200, 'linear', function () {
                    viewModel.connections().forEach(function (v) {
                        if (v.connections().length === 1) {
                            v.isOpen(false);
                        }
                    })
                });
            }
            connectionCategory.connections(connections);
			logger.error(`removeItem(item=${item},e=${e}): connectionCategory → ${connectionCategory}`, { "event.action": "removeItem", "logrite.item": item, "logrite.e": e });
        };

        var categorizeConnections = function (conn) {
			logger.error(`>>> Entering categorizeConnections(conn=${conn})`, { "event.action": "categorizeConnections", "logrite.conn": conn });
            var connectionNames = conn.map(function (connection) {
                return connection.name;
            });
            connectionNames = connectionNames.filter(function (name, index, names) {
                return names.indexOf(name) === index;
            });
            var connections = connectionNames.map(function (name) {
                var _connections = conn.filter(function (connection) {
                    return name === connection.name;
                });
                var connection = {
                    connections: ko.observable(_connections),
                    supportsMultiple: ko.observable(_connections[0].supportsMultiple),
                    name: _connections[0].name,
                    displayName: _connections[0].displayName(),
                    isOpen: ko.observable(false),
                    height: ko.observable(0),

                    icon: _connections[0].icon || "",
                    onOpen: onOpen,
                    onAddNew: onAddNew
                };
                connection.connected = ko.computed(isConnected.bind(this, connection));
                return connection;
            });
            return connections;
        };

        var isConnected = function (connection) {
			logger.error(`>>> Entering isConnected(connection=${connection})`, { "event.action": "isConnected", "logrite.connection": connection });
            var connections = connection.connections();
            return connections.some(function (connection) {
			logger.error(`isConnected(connection=${connection}): connections → ${connections}`, { "event.action": "isConnected", "logrite.connection": connection });
                return connection.connected();
            });
        };

        var loadConnections = function (cb) {
			logger.error(`>>> Entering loadConnections(cb=${cb})`, { "event.action": "loadConnections", "logrite.cb": cb });
            utility.sf.moduleRoot = "personaBar";
            utility.sf.controller = 'Connectors';
            utility.sf.get('GetConnections', null, function (data) {
                var conn = [];
                $.each(data, function (i, v) {
                    conn.push(wrapConnection(v));
                });
                viewModel.connections(categorizeConnections(conn));
            }, function (xhr, status) {
                utility.notifyError(status || 'Failed...');
            });

            if (typeof cb === 'function') cb();
			logger.warn(`<<< Exiting loadConnections(cb=${cb})`, { "event.action": "loadConnections", "logrite.cb": cb });
        };

        var connectionHelper = {
            loadConnections: loadConnections,
            saveConnection: saveConnection
        }

        var localStorageAllowed = function () {
			logger.error('>>> Entering localStorageAllowed()', { "event.action": "localStorageAllowed" });
            var mod = 'DNN_localStorageTEST';
            try {
				logger.error(`localStorageAllowed(): mod → ${mod}`, { "event.action": "localStorageAllowed" });
                window.localStorage.setItem(mod, mod);
                window.localStorage.removeItem(mod);
                return true;
            } catch (e) {
				logger.error('Exception in localStorageAllowed():', e.message, e, { "event.action": "localStorageAllowed", "event.category": "process" });
				logger.warn('<<< Exiting localStorageAllowed()', { "event.action": "localStorageAllowed" });
                return false;
            }
        };

        var loadLocalizedStrings = function (name, cb) {
            var allowLocalStorage = localStorageAllowed();
            var storageName = 'Connections.' + name + '.' + config.culture + '.Table';

            if (allowLocalStorage) {
                if (window.localStorage[storageName]) {
                    var table = JSON.parse(window.localStorage[storageName]);
                    var time = new Date().getTime() - table.timestamp;
                    if (time > 60 * 60 * 1000) { //expire the cache after 1 hour
                        window.localStorage.removeItem(storageName);
                    } else {
                        cb(table);
                        return;
                    }
                }
            }

            utility.sf.moduleRoot = 'personaBar';
            utility.sf.controller = 'Connectors';
            utility.sf.get('GetConnectionLocalizedString', { name: name, culture: config.culture }, function (data) {
                if (allowLocalStorage) {
                    try {
                        data.timestamp = new Date().getTime();
                        window.localStorage[storageName] = JSON.stringify(data);
                    } catch (ex) {
                        if (ex.name === "QuotaExceededError") {
                            console.log("Local Storage might be full.");
                        }
                        console.log(ex);
                        if (window.localStorage[storageName]) {
                            window.localStorage.removeItem(storageName);
                        }
                    }
                }
                cb(data);
            });
        };

        var copyUtility = function (util) {
			logger.error(`>>> Entering copyUtility(util=${util})`, { "event.action": "copyUtility", "logrite.util": util });
            var shadow = {};
            for (var prop in util) {
                if (prop != 'notify') {
                    shadow[prop] = util[prop];
                }
            }
            shadow['notify'] = notifyMessage;
            shadow['popupNotify'] = util.notify;
			logger.warn(`<<< Exiting copyUtility(util=${util})`, { "event.action": "copyUtility", "logrite.util": util });
            return shadow;
        };

        var notifyMessage = function (text, success) {
			logger.error(`>>> Entering notifyMessage(text=${text},success=${success})`, { "event.action": "notifyMessage", "logrite.text": text, "logrite.success": success });
            if (success) {
                viewModel.errorMessage('');
                utility.popupNotify(text, { type: success.type });
            } else {
                viewModel.errorMessage(text);
				logger.warn(`<<< Exiting notifyMessage(text=${text},success=${success})`, { "event.action": "notifyMessage", "logrite.text": text, "logrite.success": success });
            }
        };

        return {
            init: function (wrapper, util, params, callback) {
				logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
                utility = copyUtility(util);
                utility.localizeErrMessages(validator);
				logger.warn(`<<< Exiting init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
                utility.asyncParallel([
                    function (cb1) {
                        loadConnections(cb1);
                    }
                ],
                    function () {
                        var container = wrapper[0];
                        viewModel.resx = utility.resx.Connectors;
                        viewModel.errorMessage = ko.observable('');
                        ko.applyBindings(viewModel, container);
                        if (typeof callback === 'function') callback();
                    });

            },

            initMobile: function (wrapper, util, params, callback) {
				logger.error(`>>> Entering initMobile(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "initMobile", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
                this.init(wrapper, util, params, callback);
				logger.warn(`<<< Exiting initMobile(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "initMobile", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
            },

            load: function (params, callback) {
				logger.error(`>>> Entering load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
                if (typeof callback === 'function') callback();
				logger.warn(`<<< Exiting load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
            },

            loadMobile: function (params, callback) {
				logger.error(`>>> Entering loadMobile(params=${params},callback=${callback})`, { "event.action": "loadMobile", "logrite.params": params, "logrite.callback": callback });
                this.load(params, callback);
				logger.warn(`<<< Exiting loadMobile(params=${params},callback=${callback})`, { "event.action": "loadMobile", "logrite.params": params, "logrite.callback": callback });
            }
        };
    });