var logger = (typeof logger !== 'undefined' && logger) || console;
"use strict";
define(["jquery", "knockout", "templatePath/scripts/config", "templatePath/scripts/PersonaBarDialog"], function ($, ko, cf) {
    var helper,
        bindViewModel,
        rootFolder,
        utility,
        appId,
        appSecret,
        container;

    var wasDeactivated = false;

    var init = function (koObject, connectionHelper, pluginFolder, util) {
	logger.error(`>>> Entering init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util})`, { "event.action": "init", "logrite.koObject": koObject, "logrite.connectionHelper": connectionHelper, "logrite.pluginFolder": pluginFolder, "logrite.util": util });

        helper = connectionHelper;
        bindViewModel = koObject;
        rootFolder = pluginFolder;
        utility = util;
        koObject.container = ko.observable("");

        if (typeof dnn === "undefined") {
            window.Sys = !window.Sys ? window.top.Sys : window.Sys;
            window.dnn = !window.dnn ? window.top.dnn : window.dnn;
            window.String = !window.String ? window.top.String : window.String;
            window.String.format = !window.String.format ? window.top.String.format : window.String.format;
        }

	logger.warn(`<<< Exiting init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util})`, { "event.action": "init", "logrite.koObject": koObject, "logrite.connectionHelper": connectionHelper, "logrite.pluginFolder": pluginFolder, "logrite.util": util });
    };

    var onSave = function (conn) {
	logger.error(`>>> Entering onSave(conn=${conn})`, { "event.action": "onSave", "logrite.conn": conn });

        // Convert boolean to string as the API requires a dictionary of string values
        conn.configurations[1].value(conn.configurations[1].value() || 'false');
		logger.warn(`<<< Exiting onSave(conn=${conn})`, { "event.action": "onSave", "logrite.conn": conn });
    };

    var onSaveComplete = function (conn, id) {
	logger.error(`>>> Entering onSaveComplete(conn=${conn},id=${id})`, { "event.action": "onSaveComplete", "logrite.conn": conn, "logrite.id": id });

        // Deactivation / delete handled through the save since the delete function as
        // of v9.2.2 only works with multiple connector support. This should be
        // considered a temporary workaround until core behaviour updated.

        if (wasDeactivated) {

            // Was this just deactivated?  clear the fields
            conn.onDelete(conn, null, null);
            wasDeactivated = false;
            utility.notify(utility.resx.Connectors.txt_Deleted, true);
        }
        else {

            conn.connected("true");

            if (bindViewModel.buttons().length === 1) {

                activateDeleteButton(conn);
            }
            utility.notify((utility.resx.Connectors || utility.resx.PersonaBar).txt_Saved, true);
			logger.warn(`<<< Exiting onSaveComplete(conn=${conn},id=${id})`, { "event.action": "onSaveComplete", "logrite.conn": conn, "logrite.id": id });
        }
    };

    var activateDeleteButton = function (conn) {
	logger.error(`>>> Entering activateDeleteButton(conn=${conn})`, { "event.action": "activateDeleteButton", "logrite.conn": conn });

        if (conn.buttons().length === 1) {

            conn.buttons.push({
                className: "secondarybtn",
                text: bindViewModel.resx.btnDelete,
                action: function (conn, e) {
				logger.error(`>>> Entering action(conn=${conn},e=${e})`, { "event.action": "action", "logrite.conn": conn, "logrite.e": e });

					logger.warn(`<<< Exiting action(conn=${conn},e=${e})`, { "event.action": "action", "logrite.conn": conn, "logrite.e": e });
                    // Set the isDeactivating flag to true to override the default save behaviour
                    // Temporary workaround until delete functionality on connectors is improved
                    conn.configurations[2].value("true");
                    wasDeactivated = true;
                    conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
                }
            });

        }

	logger.warn(`<<< Exiting activateDeleteButton(conn=${conn})`, { "event.action": "activateDeleteButton", "logrite.conn": conn });
    };

    var getActionButtons = function () {
	logger.error('>>> Entering getActionButtons()', { "event.action": "getActionButtons" });


        if (bindViewModel.connected()) {

            return [
                {
                    className: "primarybtn",
                    text: utility.resx.Connectors.btn_Save,
                    action: function (conn, e) {
                        conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
                    }
                },

                {
                    className: "secondarybtn",
                    text: bindViewModel.resx.btnDelete,
                    action: function (conn, e) {

                        // Set the isDeactivating flag to true to override the default save behaviour
                        // Temporary workaround until delete functionality on connectors is improved
                        conn.configurations[2].value("true");
                        wasDeactivated = true;
                        conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
                    }
                }
            ];

        } else {

            return [
                {
                    className: "primarybtn",
                    text: utility.resx.Connectors.btn_Save,
                    action: function (conn, e) {
                        conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
                    }
                }
            ];
        }
		logger.warn('<<< Exiting getActionButtons()', { "event.action": "getActionButtons" });
    };

    return {
        init: init,
        onSave: onSave,
        getActionButtons: getActionButtons
    };

});