var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

"use strict";
define(["jquery", "knockout", "templatePath/scripts/config", "templatePath/scripts/PersonaBarDialog"], function ($, ko, cf) {
    var helper,
        bindViewModel,
        rootFolder,
        utility,
        appId,
        appSecret,
        container;

    var init = function (koObject, connectionHelper, pluginFolder, util) {
		logger.error(`>>> Entering init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util})`, { "event.action": "init", "logrite.koObject": koObject, "logrite.connectionHelper": connectionHelper, "logrite.pluginFolder": pluginFolder, "logrite.util": util });
        helper = connectionHelper;
        bindViewModel = koObject;
        rootFolder = pluginFolder;
        utility = util;
        koObject.container = ko.observable("");
        koObject.loadingFolders = ko.observable(false);
        koObject.foldersData = ko.observable([]);

        if (typeof dnn === "undefined") {
            window.Sys = !window.Sys ? window.top.Sys : window.Sys;
            window.dnn = !window.dnn ? window.top.dnn : window.dnn;
            window.String = !window.String ? window.top.String : window.String;
            window.String.format = !window.String.format ? window.top.String.format : window.String.format;
        }
		logger.warn(`<<< Exiting init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util})`, { "event.action": "init", "logrite.koObject": koObject, "logrite.connectionHelper": connectionHelper, "logrite.pluginFolder": pluginFolder, "logrite.util": util });
        processData(koObject);

    };

    var fieldsHaveValues = function (conn) {
		logger.error(`>>> Entering fieldsHaveValues(conn=${conn})`, { "event.action": "fieldsHaveValues", "logrite.conn": conn });
		logger.warn(`<<< Exiting fieldsHaveValues(conn=${conn})`, { "event.action": "fieldsHaveValues", "logrite.conn": conn });
        return conn.configurations[0].value() !== "" &&
            conn.configurations[1].value() !== "";
    };

    var onSave = function (conn) {
		logger.error(`>>> Entering onSave(conn=${conn})`, { "event.action": "onSave", "logrite.conn": conn });
        if (typeof conn.container !== "undefined") {
            conn.configurations.push({
                name: "Container",
                value: ko.protectedObservable(conn.container())
            });
			logger.warn(`<<< Exiting onSave(conn=${conn})`, { "event.action": "onSave", "logrite.conn": conn });
        }
    };

    var processData = function (conn, id) {
		logger.error(`>>> Entering processData(conn=${conn},id=${id})`, { "event.action": "processData", "logrite.conn": conn, "logrite.id": id });
        conn.id = id || conn.id;
        for (var i = conn.configurations.length - 1; i >= 0; i--) {
			logger.debug(`processData(conn=${conn},id=${id}): conn.id → ${conn.id}`, { "event.action": "processData", "logrite.conn": conn, "logrite.id": id });
            var config = conn.configurations[i];
			logger.error(`processData(conn=${conn},id=${id}): config → ${config}`, { "event.action": "processData", "logrite.conn": conn, "logrite.id": id });
            if (config.name !== "AccountName" && config.name !== "AccountKey" && config.name != "Id") {
                switch (config.name.toLowerCase()) {
                    case "connected":
                        conn.connected(config.value() === "true");
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

		logger.error(`processData(conn=${conn},id=${id}): id → ${id}`, { "event.action": "processData", "logrite.conn": conn, "logrite.id": id });
        conn.connected(!(typeof conn.container() === "undefined" || conn.container() === ""));
        if (conn.id && conn.foldersData().length === 0)
            loadFoldersData(conn);
    };

    var loadFoldersData = function (conn) {
		logger.error(`>>> Entering loadFoldersData(conn=${conn})`, { "event.action": "loadFoldersData", "logrite.conn": conn });
        utility.sf.moduleRoot = "AzureConnector";
        utility.sf.controller = "Services";
        utility.sf.get("GetAllContainers?id=" + conn.id, {}, foldersLoaded.bind(this, conn), folderLoadFailed.bind(this, conn));
		logger.warn(`<<< Exiting loadFoldersData(conn=${conn})`, { "event.action": "loadFoldersData", "logrite.conn": conn });
    };

    var foldersLoaded = function (conn, data) {
		logger.error(`>>> Entering foldersLoaded(conn=${conn},data=${data})`, { "event.action": "foldersLoaded", "logrite.conn": conn, "logrite.data": data });
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
		logger.warn(`<<< Exiting foldersLoaded(conn=${conn},data=${data})`, { "event.action": "foldersLoaded", "logrite.conn": conn, "logrite.data": data });
    };

    var folderLoadFailed = function (conn, xhr) {
		logger.error(`>>> Entering folderLoadFailed(conn=${conn},xhr=${xhr})`, { "event.action": "folderLoadFailed", "logrite.conn": conn, "logrite.xhr": xhr });
        conn.loadingFolders(false);
        var message = xhr.responseJSON.Message;
        if (message.indexOf("{") === 0) {
            message = JSON.parse(message)["error"];
        }
        utility.notifyError(message);
		logger.error(`folderLoadFailed(conn=${conn},xhr=${xhr}): message → ${message}`, { "event.action": "folderLoadFailed", "logrite.conn": conn, "logrite.xhr": xhr });
    };

    var onSaveComplete = function (conn, id) {
		logger.error(`>>> Entering onSaveComplete(conn=${conn},id=${id})`, { "event.action": "onSaveComplete", "logrite.conn": conn, "logrite.id": id });
        if (!fieldsHaveValues(conn)) {
            conn.foldersData([]);
            return;
        }
        processData(conn, id);
        utility.notify((utility.resx.Connectors || utility.resx.PersonaBar).txt_Saved, true);
		logger.warn(`<<< Exiting onSaveComplete(conn=${conn},id=${id})`, { "event.action": "onSaveComplete", "logrite.conn": conn, "logrite.id": id });
    };


    var authenticate = function (conn, e) {
		logger.error(`>>> Entering authenticate(conn=${conn},e=${e})`, { "event.action": "authenticate", "logrite.conn": conn, "logrite.e": e });
        e.preventDefault();
        $.each(conn.configurations, function (i, v) {
            v.value.commit();
        });
        saveConnection(conn, onSaveComplete.bind(this, conn));
		logger.warn(`<<< Exiting authenticate(conn=${conn},e=${e})`, { "event.action": "authenticate", "logrite.conn": conn, "logrite.e": e });
    };

    //copy the save connection method to save sync.
    var onSaveConnection = false;
    var saveConnection = function (conn, success) {
		logger.error(`>>> Entering saveConnection(conn=${conn},success=${success})`, { "event.action": "saveConnection", "logrite.conn": conn, "logrite.success": success });
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

		logger.error(`saveConnection(conn=${conn},success=${success}): value → ${value}`, { "event.action": "saveConnection", "logrite.conn": conn, "logrite.success": success });
        }

        if (!postData.configurations[0].value && !postData.configurations[1].value) {
			logger.warn(`<<< Exiting saveConnection(conn=${conn},success=${success})`, { "event.action": "saveConnection", "logrite.conn": conn, "logrite.success": success });
            return conn.onDelete(conn);
        }

        utility.sf.call("POST", "SaveConnection", postData, function (d) {
            if (d && d.Success) {
                success(d.Id);
            } else {
                var message = d.responseJSON.Message;
                utility.notifyError(message || "Failed...");
            }
            onSaveConnection = false;
        }, saveFailed.bind(this, conn), null, null, true, false);
    };

    var saveFailed = function (conn, d) {
		logger.error(`>>> Entering saveFailed(conn=${conn},d=${d})`, { "event.action": "saveFailed", "logrite.conn": conn, "logrite.d": d });
        var message = d.responseJSON.Message;
        utility.notifyError(message || "Failed...");
        onSaveConnection = false;
        conn.cancel();
		logger.warn(`<<< Exiting saveFailed(conn=${conn},d=${d})`, { "event.action": "saveFailed", "logrite.conn": conn, "logrite.d": d });
    };

    var getActionButtons = function () {
		logger.error('>>> Entering getActionButtons()', { "event.action": "getActionButtons" });
        return [
            {
                className: "primarybtn",
                text: "Save",
                action: function (conn, e) {
					logger.error(`>>> Entering action(conn=${conn},e=${e})`, { "event.action": "action", "logrite.conn": conn, "logrite.e": e });
                    conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
					logger.warn(`<<< Exiting action(conn=${conn},e=${e})`, { "event.action": "action", "logrite.conn": conn, "logrite.e": e });
                }
            }
        ];
		logger.warn('<<< Exiting getActionButtons()', { "event.action": "getActionButtons" });
    };

    return {
        init: init,
        onSave: onSave,
        getActionButtons: getActionButtons
    };

});