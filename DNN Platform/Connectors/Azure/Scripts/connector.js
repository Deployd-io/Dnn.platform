const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

"use strict";
define(["jquery", "knockout", "templatePath/scripts/config", "templatePath/scripts/PersonaBarDialog"], function ($, ko, cf) {
	logger.debug('>>> Entering define()');
    var helper,
        bindViewModel,
        rootFolder,
        utility,
        appId,
        appSecret,
        container;

    var init = function (koObject, connectionHelper, pluginFolder, util) {
		logger.debug(`>>> Entering init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util})`);
        helper = connectionHelper;
        bindViewModel = koObject;
        rootFolder = pluginFolder;
        utility = util;
        koObject.container = ko.observable("");
        koObject.loadingFolders = ko.observable(false);
        koObject.foldersData = ko.observable([]);

        if (typeof dnn === "undefined") {
            window.Sys = !window.Sys ? window.top.Sys : window.Sys;
			logger.debug(`define(): window.Sys → ${window.Sys}`);
			logger.debug(`define(): window.String → ${window.String}`);
            window.dnn = !window.dnn ? window.top.dnn : window.dnn;
			logger.debug(`define(): window.dnn → ${window.dnn}`);
            window.String = !window.String ? window.top.String : window.String;
            window.String.format = !window.String.format ? window.top.String.format : window.String.format;
			logger.debug(`init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util}): window.Sys → ${window.Sys}`);
        }
        processData(koObject);
		logger.debug(`init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util}): window.dnn → ${window.dnn}`);
		logger.debug(`<<< Exiting init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util})`);
		logger.debug(`init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util}): window.String → ${window.String}`);

    };

    var fieldsHaveValues = function (conn) {
		logger.debug(`>>> Entering fieldsHaveValues(conn=${conn})`);
		logger.debug(`<<< Exiting fieldsHaveValues(conn=${conn})`);
        return conn.configurations[0].value() !== "" &&
            conn.configurations[1].value() !== "";
    };

    var onSave = function (conn) {
		logger.debug(`>>> Entering onSave(conn=${conn})`);
        if (typeof conn.container !== "undefined") {
            conn.configurations.push({
                name: "Container",
                value: ko.protectedObservable(conn.container())
            });
			logger.debug(`<<< Exiting onSave(conn=${conn})`);
        }
    };

    var processData = function (conn, id) {
		logger.debug(`>>> Entering processData(conn=${conn},id=${id})`);
        conn.id = id || conn.id;
        for (var i = conn.configurations.length - 1; i >= 0; i--) {
            var config = conn.configurations[i];
			logger.debug(`processData(conn=${conn},id=${id}): config → ${config}`);
			logger.debug(`processData(conn=${conn},id=${id}): conn.id → ${conn.id}`);
            if (config.name !== "AccountName" && config.name !== "AccountKey" && config.name != "Id") {
                switch (config.name.toLowerCase()) {
                    case "connected":
                        conn.connected(config.value() === "true");
						logger.debug(`define(): conn.id → ${conn.id}`);
                        conn.configurations.splice(i, 1);
                        break;
                    case "container":
                        conn.container(conn.container() || config.value());
                        conn.configurations.splice(i, 1);
                        break;
                }
            } else if (config.name === "AccountName") {
                appId = config.value();
            } else if (config.name === "AccountKey") {
                appSecret = config.value();
            } else if (config.name === "Id") {
                id = conn.id;
            }
        }

		logger.debug(`processData(conn=${conn},id=${id}): id → ${id}`);
        conn.connected(!(typeof conn.container() === "undefined" || conn.container() === ""));
        if (conn.id && conn.foldersData().length === 0)
			logger.debug(`<<< Exiting processData(conn=${conn},id=${id})`);
            loadFoldersData(conn);
    };

    var loadFoldersData = function (conn) {
		logger.debug(`>>> Entering loadFoldersData(conn=${conn})`);
        utility.sf.moduleRoot = "AzureConnector";
        utility.sf.controller = "Services";
        utility.sf.get("GetAllContainers?id=" + conn.id, {}, foldersLoaded.bind(this, conn), folderLoadFailed.bind(this, conn));
		logger.debug(`<<< Exiting loadFoldersData(conn=${conn})`);
    };

    var foldersLoaded = function (conn, data) {
		logger.debug(`>>> Entering foldersLoaded(conn=${conn},data=${data})`);
        conn.loadingFolders(false);
        var foldersData = [conn.resx.SelectContainer];
        for (var i = 0; i < data.length; i++) {
            foldersData.push(data[i]);
        }
        conn.foldersData(foldersData);
        if (typeof conn.container() === "undefined" || conn.container() === "") {
            conn.container("/");
        }

        $('#connector-Azure').find('select').easyDropDown({ wrapperClass: 'pb-dropdown', cutOff: 10, inFocus: true });
		logger.debug(`<<< Exiting foldersLoaded(conn=${conn},data=${data})`);
    };

    var folderLoadFailed = function (conn, xhr) {
		logger.debug(`>>> Entering folderLoadFailed(conn=${conn},xhr=${xhr})`);
        conn.loadingFolders(false);
        var message = xhr.responseJSON.Message;
        if (message.indexOf("{") === 0) {
            message = JSON.parse(message)["error"];
        }
        utility.notifyError(message);
		logger.debug(`folderLoadFailed(conn=${conn},xhr=${xhr}): message → ${message}`);
		logger.debug(`<<< Exiting folderLoadFailed(conn=${conn},xhr=${xhr})`);
    };

    var onSaveComplete = function (conn, id) {
		logger.debug(`>>> Entering onSaveComplete(conn=${conn},id=${id})`);
        if (!fieldsHaveValues(conn)) {
            conn.foldersData([]);
            return;
        }
        processData(conn, id);
        utility.notify((utility.resx.Connectors || utility.resx.PersonaBar).txt_Saved, true);
		logger.debug(`<<< Exiting onSaveComplete(conn=${conn},id=${id})`);
    };


    var authenticate = function (conn, e) {
		logger.debug(`>>> Entering authenticate(conn=${conn},e=${e})`);
        e.preventDefault();
        $.each(conn.configurations, function (i, v) {
			logger.debug('>>> Entering each()');
			logger.debug('<<< Exiting each()');
            v.value.commit();
        });
        saveConnection(conn, onSaveComplete.bind(this, conn));
		logger.debug(`<<< Exiting authenticate(conn=${conn},e=${e})`);
    };

    //copy the save connection method to save sync.
    var onSaveConnection = false;
    var saveConnection = function (conn, success) {
		logger.debug(`>>> Entering saveConnection(conn=${conn},success=${success})`);
        if (onSaveConnection) return;
        onSaveConnection = true;
        if (conn.connector()) {
            conn.connector().onSave(conn);
        }

        utility.sf.moduleRoot = "personaBar";
        utility.sf.controller = "Connectors";
        var postData = {
            name: conn.name,
            displayName: conn.displayName(),
            id: conn.id,
            configurations: []
        };

        for (var i = 0; i < conn.configurations.length; i++) {
            var c = conn.configurations[i];
            var value = c.value();
            postData.configurations.push({
                name: c.name,
                value: c.value()
            });

		logger.debug(`saveConnection(conn=${conn},success=${success}): value → ${value}`);
        }

        if (!postData.configurations[0].value && !postData.configurations[1].value) {
			logger.debug(`<<< Exiting saveConnection(conn=${conn},success=${success})`);
            return conn.onDelete(conn);
        }

        utility.sf.call("POST", "SaveConnection", postData, function (d) {
			logger.debug(`>>> Entering call(postData=${postData},message=${message},null=${null},null=${null},true=${true},false=${false})`);
            if (d && d.Success) {
                success(d.Id);
            } else {
                var message = d.responseJSON.Message;
                utility.notifyError(message || "Failed...");
            }
            onSaveConnection = false;
			logger.debug(`<<< Exiting call(postData=${postData},message=${message},null=${null},null=${null},true=${true},false=${false})`);
        }, saveFailed.bind(this, conn), null, null, true, false);
    };

    var saveFailed = function (conn, d) {
		logger.debug(`>>> Entering saveFailed(conn=${conn},d=${d})`);
        var message = d.responseJSON.Message;
        utility.notifyError(message || "Failed...");
        onSaveConnection = false;
        conn.cancel();
		logger.debug(`saveFailed(conn=${conn},d=${d}): onSaveConnection → ${onSaveConnection}`);
		logger.debug(`<<< Exiting saveFailed(conn=${conn},d=${d})`);
    };

    var getActionButtons = function () {
		logger.debug('>>> Entering getActionButtons()');
        return [
            {
                className: "primarybtn",
                text: "Save",
                action: function (conn, e) {
					logger.debug(`>>> Entering action(conn=${conn},e=${e})`);
                    conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
					logger.debug(`<<< Exiting action(conn=${conn},e=${e})`);
                }
            }
        ];
		logger.debug('<<< Exiting getActionButtons()');
    };

    return {
        init: init,
        onSave: onSave,
        getActionButtons: getActionButtons
    };

});