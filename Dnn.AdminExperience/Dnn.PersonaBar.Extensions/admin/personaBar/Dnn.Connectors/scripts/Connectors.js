const logger = console;
﻿'use strict';
define(['jquery', 'knockout', 'main/pager', 'main/validator', 'main/config', 'main/PersonaBarDialog', 'jquery.easydropdown.min', 'dnn.jquery'],
    function ($, ko, pager, validator, cf, personaBarDialog) {
		logger.debug(`>>> Entering function(ko=${ko},pager=${pager},validator=${validator},cf=${cf},personaBarDialog=${personaBarDialog})`);
        var utility = null;
		logger.debug(`>>> Entering function(initialValue=${initialValue})`);
        var config = cf.init();
        var viewModel = {
            connections: ko.observableArray([])
        };

        ko.protectedObservable = function (initialValue) {
            var actualValue = ko.observable(initialValue),
                tempValue = ko.observable(initialValue);

			logger.debug(`function(ko=${ko},pager=${pager},validator=${validator},cf=${cf},personaBarDialog=${personaBarDialog}): config → ${config}`);
			logger.debug(`function(initialValue=${initialValue}): actualValue → ${actualValue}`);
            var result = tempValue;
            result.commit = function () {
				logger.debug(`function(initialValue=${initialValue}): result → ${result}`);
                if (tempValue() !== actualValue()) {
                    actualValue(tempValue().trim());
                }
            };
            result.reset = function () {
                actualValue.valueHasMutated();
                tempValue(actualValue());
            };

            return result;
        };

        var wrapConnection = function (conn) {
            var koConfigurations = [];
            var secureSettingsValue = conn.configurations["SecureSettings"];
			logger.debug(`function(ko=${ko},pager=${pager},validator=${validator},cf=${cf},personaBarDialog=${personaBarDialog}): secureSettingsValue → ${secureSettingsValue}`);
            var secureSettings = secureSettingsValue ? secureSettingsValue.split(";") : new Array();
            $.each(conn.configurations, function (key, value) {
				logger.debug('>>> Entering each()');
                if (key === "SecureSettings") {
                    return;
                }
                var o = {};
                o.name = key;
				logger.debug(`function(ko=${ko},pager=${pager},validator=${validator},cf=${cf},personaBarDialog=${personaBarDialog}): o.name → ${o.name}`);
                o.type = secureSettings.indexOf(key) > -1 ? "password" : "text";
                o.localizedName = key;
				logger.debug(`function(ko=${ko},pager=${pager},validator=${validator},cf=${cf},personaBarDialog=${personaBarDialog}): o.localizedName → ${o.localizedName}`);
                o.value = ko.protectedObservable(value);
				logger.debug(`each(): o.name → ${o.name}`);
				logger.debug('<<< Exiting each()');
                koConfigurations.push(o);
            });

            var koConnectionObject = {
                name: conn.name,
                displayName: ko.observable(conn.displayName),
                wrapperId: ko.computed(function() {
					logger.debug('>>> Entering computed()');
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
					logger.debug(`>>> Entering edit(item=${item},e=${e})`);
                    e.preventDefault();
                    var btn = $(e.target);
                    var editRow = btn.parent().parent().next().find('td > div');
                    var showEditRow = function () {
						logger.debug('>>> Entering showEditRow()');
                        $.each(viewModel.connections(), function (i, v) {
                            v.isOpen(false);
                            $.each(v.connections(), function (i, item) {
                                item.inEdit(false);
                                item.open(false);
                            });
                        });
                        item.inEdit(true);

						logger.debug(`edit(item=${item},e=${e}): btn → ${btn}`);
                        editRow.slideDown(200, 'linear', function () {
							logger.debug('>>> Entering slideDown()');
							logger.debug('<<< Exiting slideDown()');
                            $(this).find('input:first').select();
                        });
						logger.debug('<<< Exiting showEditRow()');
                    };
                    var collapsedEditRow = $('#connectionstbl tr.edit-row > td > div.edit-form:visible');
					logger.debug(`slideDown(): collapsedEditRow → ${collapsedEditRow}`);
                    if (collapsedEditRow.length > 0) {
                        collapsedEditRow.slideUp(200, 'linear', showEditRow);
						logger.debug(`edit(item=${item},e=${e}): editRow → ${editRow}`);
						logger.debug(`edit(item=${item},e=${e}): collapsedEditRow → ${collapsedEditRow}`);
                    } else {
                        showEditRow();
						logger.debug(`<<< Exiting edit(item=${item},e=${e})`);
                    }
                },

                cancel: function (item, e) {
					logger.debug(`>>> Entering cancel(item=${item},e=${e})`);
                    if (e) {
                        e.preventDefault();
                    }
                    $.each(item.configurations, function (i, v) {
                        var name = v.name.toLowerCase();
                        v.value.reset();
                        if (item[name] && typeof item[name] === "object" && typeof item[name].reset === "function") {
							logger.debug(`each(name=${name}): name → ${name}`);
                            item[name].reset();
                        }
                    });
                    var connected = item.previousConnectionState() && (item.supportsMultiple ? !!item.id : true);
                    item.connected(connected);
					logger.debug(`cancel(item=${item},e=${e}): connected → ${connected}`);
                    item.displayName(item.previousDisplayName());
                    item.inEdit(false);
                    viewModel.connections().forEach(function (v) {
						logger.debug('>>> Entering forEach()');
                        v.connections().forEach(function (t) {
                            t.open(false);
						logger.debug('<<< Exiting forEach()');
                        });

                    });
                    if (e) {
						logger.debug(`<<< Exiting cancel(item=${item},e=${e})`);
                        $(e.target).parents('div.edit-form').slideUp(200, 'linear', function () {
							logger.debug('>>> Entering slideUp()');
                            viewModel.connections().forEach(function (v) {
                                if (v.connections().length === 1) {
                                    v.isOpen(false);
                                }
							logger.debug('<<< Exiting slideUp()');
                            })
                        });
                    }
                },
                save: function (item, e, cb) {
					logger.debug(`>>> Entering save(item=${item},e=${e},cb=${cb})`);
                    e.preventDefault();
                    item.connected(false);
					logger.debug(`<<< Exiting save(item=${item},e=${e},cb=${cb})`);
                    var container = $(e.target).parents('div.edit-form');
					logger.debug(`save(item=${item},e=${e},cb=${cb}): container → ${container}`);
                    saveConnection(item, function (success, validated, id) {
						logger.debug('>>> Entering saveConnection()');
                        utility.notify(utility.resx.Connectors.txt_Saved, true);
                        var connected = isConnectionConnected(item);
						logger.debug(`saveConnection(): connected → ${connected}`);
                        item.connected(connected);
                        if (success) {
                            item.previousConnectionState(connected);
                            item.previousDisplayName(item.displayName());
                            if (id) {
                                item.id = id;
                            }
                            $.each(item.configurations, function (i, v) {
								logger.debug(`saveConnection(): item.id → ${item.id}`);
                                v.value.commit();
                            });
                        }

                        if (typeof cb == "function") {
                            cb(item, true);
						logger.debug('<<< Exiting saveConnection()');
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
				logger.debug('>>> Entering require()');
                loadLocalizedStrings(conn.name, function (data) {
					logger.debug('>>> Entering loadLocalizedStrings()');
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
								logger.debug(`>>> Entering setTimeout($root=${$root})`);
                                var $root = $('#' + connection.wrapperId());
                                $root.find('input, select').each(function () {
                                    var $this = $(this);
                                    if (!$this.attr('id') || !$root.find('label[for="' + $this.attr('id') + '"]').length) {
                                        $this.attr('aria-label', 'Value');
                                    }
                                });

                                $root.find('a').each(function () {
									logger.debug(`<<< Exiting setTimeout($root=${$root})`);
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
					logger.debug('<<< Exiting loadLocalizedStrings()');
                    });
                });
				logger.debug('<<< Exiting require()');
            });

			logger.debug(`<<< Exiting wrapConnection(conn=${conn})`);
            return koConnectionObject;
        };

        var isConnectionConnected = function (item) {
			logger.debug(`>>> Entering isConnectionConnected(item=${item})`);
            if (item.name === "Mailchimp" || item.name === "GoogleTagManager" || item.name === "GoogleAnalytics") {
                return !!item.configurations[0].value();
            }
            if (item.name === "Zendesk") {
                return item.configurations[0].value() && item.configurations[1].value() && item.configurations[2].value();
            }
            return item.configurations.every(function (config) {
			logger.debug('>>> Entering every()');
			logger.debug(`<<< Exiting isConnectionConnected(item=${item})`);
                return !!config.value() || config.name == "Id" || config.name == "Connected";
            });
        };

        var getDefaultButtons = function () {
			logger.debug('>>> Entering getDefaultButtons()');
            return [
                {
                    className: 'primarybtn',
                    text: viewModel.resx.btn_Save,
                    action: function (conn, e) {
                        conn.save(conn, e);
						logger.debug(`<<< Exiting action(conn=${conn},e=${e})`);
                    }
                }
            ];
			logger.debug('<<< Exiting getDefaultButtons()');
        }

        var validateDisplayName = function (conn) {
            var displayName = conn.displayName();
            var noSpecialChar = /^[a-zA-Z0-9- ]*$/.test(displayName);
            var isNotEmpty = !!displayName.trim();
            return isNotEmpty && noSpecialChar;
        };

        var isDisplayNameUnique = function (conn) {
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
        };

        var onSaveFailed = function (conn, xhr, status) {
            var primaryButtons = $('#' + conn.wrapperId()).find('a.primarybtn');
            primaryButtons.html(viewModel.resx.btn_Save);
            if (xhr.responseJSON) {
                utility.notifyError(xhr.responseJSON.Message || status || 'Failed...');
            } else {
                utility.notifyError(status || 'Failed...');
            }
			logger.debug(`<<< Exiting onSaveFailed(conn=${conn},xhr=${xhr},status=${status})`);
        };

        var onAddNew = function (item, e) {
			logger.debug(`>>> Entering onAddNew(item=${item},e=${e})`);
            item.isOpen(true);
            var connections = item.connections();
			logger.debug(`onAddNew(item=${item},e=${e}): connections → ${connections}`);
            var newConnector = connections.find(function (conn) { return !conn.id });

            if (newConnector && connections.length > 1) {
                closeAllSubConnectors();
                return newConnector.open(true);
            }
            var configurationKeys = Object.keys(connections[0].initialObject.configurations);
			logger.debug(`onAddNew(item=${item},e=${e}): configurationKeys → ${configurationKeys}`);
            var newConfigurations = {};
            configurationKeys.forEach(function (key) {
                if (key === "AuthPage") {
                    newConfigurations[key] = connections[0].initialObject.configurations[key];
                } else {
                    newConfigurations[key] = "";
                }
            });
            var displayName = e === true ? item.displayName : getNewName(item);
			logger.debug(`onAddNew(item=${item},e=${e}): displayName → ${displayName}`);
            closeAllSubConnectors();
            var newConnection = Object.assign(connections[0].initialObject, { id: null, configurations: newConfigurations, displayName: displayName, open: true });
			logger.debug(`onAddNew(item=${item},e=${e}): newConnection → ${newConnection}`);
            var newConn = wrapConnection(newConnection);
			logger.debug(`onAddNew(item=${item},e=${e}): newConn → ${newConn}`);
            newConn.connected(false);
            newConn.previousConnectionState(false);
            newConn.open(true);
            connections.push(newConn);
            item.connections(connections);
			logger.debug(`<<< Exiting onAddNew(item=${item},e=${e})`);
            return newConn;
        };

        var getNewName = function (item) {
			logger.debug(`>>> Entering getNewName(item=${item})`);
            var connections = item.connections();
            var index = connections.length;
			logger.debug(`getNewName(item=${item}): connections → ${connections}`);
            var newName = item.displayName + " - " + index;
            while (connections.some(function (conn) { return conn.displayName() === newName })) {
                newName = item.displayName + " - " + ++index;
            }
			logger.debug(`getNewName(item=${item}): newName → ${newName}`);
			logger.debug(`<<< Exiting getNewName(item=${item})`);
            return newName;
        }

        var closeAllSubConnectors = function () {
			logger.debug('>>> Entering closeAllSubConnectors()');
            viewModel.connections().forEach(function (v) {
                v.connections().forEach(function (t) {
                    t.open(false);
                });
            });
			logger.debug('<<< Exiting closeAllSubConnectors()');
        }

        var onOpen = function (item, e) {
			logger.debug(`>>> Entering onOpen(item=${item},e=${e})`);
            var isOpen = item.isOpen();
            viewModel.connections().forEach(function (v) {
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
			logger.debug(`onOpen(item=${item},e=${e}): collapsedEditRow → ${collapsedEditRow}`);
            if (collapsedEditRow.length > 0) {
                collapsedEditRow.slideUp(200, 'linear');
				logger.debug(`<<< Exiting onOpen(item=${item},e=${e})`);
            }
            viewModel.connections().forEach(function (v) {
                v.connections().forEach(function (t) {
                    t.open(false);
                });
            });
        };

        var onOpenEdit = function (item, e) {
			logger.debug(`>>> Entering onOpenEdit(item=${item},e=${e})`);
            var open = !item.open();
            viewModel.connections().forEach(function (v) {
                v.connections().forEach(function (t) {
                    t.open(t.uniqueNumber === item.uniqueNumber ? open : false);
                });
            });
			logger.debug(`<<< Exiting onOpenEdit(item=${item},e=${e})`);
        };

        var onCancel = function (item, e) {
			logger.debug(`>>> Entering onCancel(item=${item},e=${e})`);
            item.onOpenEdit(item);
            item.cancel(item, e);
			logger.debug(`<<< Exiting onCancel(item=${item},e=${e})`);
        }

        var onToggleEditeMode = function (item, e) {
			logger.debug(`>>> Entering onToggleEditeMode(item=${item},e=${e})`);
            item.editMode(!item.editMode);
			logger.debug(`<<< Exiting onToggleEditeMode(item=${item},e=${e})`);
        }

        var onDelete = function (item, e) {
			logger.debug(`>>> Entering onDelete(item=${item},e=${e})`);
            if (!item.id) {
				logger.debug(`<<< Exiting onDelete(item=${item},e=${e})`);
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
			logger.debug(`>>> Entering removeItem(item=${item},e=${e})`);
            var connectionCategory = viewModel.connections().find(function (conn) {
				logger.debug('>>> Entering find()');
                return conn.connections().some(function (test) {
				logger.debug('>>> Entering some()');
				logger.debug('<<< Exiting find()');
                    return test.uniqueNumber == item.uniqueNumber;
                });
            });
            if (ko.components.isRegistered(item.uniqueNumber + "component")) {
                ko.components.unregister(item.uniqueNumber + "component");
            }
            var connections = connectionCategory.connections().filter(function (conn) {
				logger.debug('>>> Entering filter()');
                return conn.uniqueNumber != item.uniqueNumber;
            });
            if (!connections.length) {
                var newConn = onAddNew(connectionCategory, true);
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
			logger.debug(`removeItem(item=${item},e=${e}): connections → ${connections}`);
			logger.debug(`<<< Exiting removeItem(item=${item},e=${e})`);
        };

        var categorizeConnections = function (conn) {
            var connectionNames = conn.map(function (connection) {
				logger.debug(`>>> Entering map(_connections=${_connections})`);
				logger.debug(`map(_connections=${_connections}): connections → ${connections}`);
				logger.debug('>>> Entering map()');
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
			logger.debug('<<< Exiting map()');
                return connection;
            });
			logger.debug(`<<< Exiting categorizeConnections(conn=${conn})`);
            return connections;
        };

        var isConnected = function (connection) {
			logger.debug(`>>> Entering isConnected(connection=${connection})`);
            var connections = connection.connections();
            return connections.some(function (connection) {
			logger.debug(`<<< Exiting isConnected(connection=${connection})`);
                return connection.connected();
            });
        };

        var loadConnections = function (cb) {
			logger.debug(`>>> Entering loadConnections(cb=${cb})`);
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
			logger.debug(`<<< Exiting get(null=${null},conn=${conn})`);

            if (typeof cb === 'function') cb();
			logger.debug(`<<< Exiting loadConnections(cb=${cb})`);
        };

        var connectionHelper = {
            loadConnections: loadConnections,
            saveConnection: saveConnection
        }

        var localStorageAllowed = function () {
			logger.debug('>>> Entering localStorageAllowed()');
            var mod = 'DNN_localStorageTEST';
            try {
                window.localStorage.setItem(mod, mod);
				logger.debug(`localStorageAllowed(): mod → ${mod}`);
                window.localStorage.removeItem(mod);
                return true;
            } catch (e) {
				logger.error('Exception in localStorageAllowed():', e.message, e);
				logger.debug('<<< Exiting localStorageAllowed()');
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
			logger.debug(`>>> Entering copyUtility(util=${util})`);
            var shadow = {};
            for (var prop in util) {
                if (prop != 'notify') {
                    shadow[prop] = util[prop];
                }
            }
            shadow['notify'] = notifyMessage;
            shadow['popupNotify'] = util.notify;
			logger.debug(`<<< Exiting copyUtility(util=${util})`);
            return shadow;
        };

        var notifyMessage = function (text, success) {
			logger.debug(`>>> Entering notifyMessage(text=${text},success=${success})`);
            if (success) {
                viewModel.errorMessage('');
                utility.popupNotify(text, { type: success.type });
            } else {
                viewModel.errorMessage(text);
				logger.debug(`<<< Exiting notifyMessage(text=${text},success=${success})`);
            }
        };

        return {
            init: function (wrapper, util, params, callback) {
				logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
                utility = copyUtility(util);
                utility.localizeErrMessages(validator);
				logger.debug(`init(wrapper=${wrapper},util=${util},params=${params},callback=${callback}): utility → ${utility}`);
                utility.asyncParallel([
                    function (cb1) {
					logger.debug(`<<< Exiting init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
                        loadConnections(cb1);
						logger.debug(`<<< Exiting function(initialValue=${initialValue})`);
                    }
                ],
                    function () {
                        var container = wrapper[0];
						logger.debug(`function(ko=${ko},pager=${pager},validator=${validator},cf=${cf},personaBarDialog=${personaBarDialog}): container → ${container}`);
                        viewModel.resx = utility.resx.Connectors;
                        viewModel.errorMessage = ko.observable('');
                        ko.applyBindings(viewModel, container);
                        if (typeof callback === 'function') callback();
                    });

            },

            initMobile: function (wrapper, util, params, callback) {
				logger.debug(`>>> Entering initMobile(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
                this.init(wrapper, util, params, callback);
				logger.debug(`<<< Exiting initMobile(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
            },

            load: function (params, callback) {
				logger.debug(`>>> Entering load(params=${params},callback=${callback})`);
                if (typeof callback === 'function') callback();
				logger.debug(`<<< Exiting load(params=${params},callback=${callback})`);
            },

            loadMobile: function (params, callback) {
				logger.debug(`>>> Entering loadMobile(params=${params},callback=${callback})`);
                this.load(params, callback);
				logger.debug(`<<< Exiting loadMobile(params=${params},callback=${callback})`);
            }
        };
    });