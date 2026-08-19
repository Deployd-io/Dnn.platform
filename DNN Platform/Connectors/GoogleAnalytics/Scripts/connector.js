const logger = console;
﻿"use strict";
define(["jquery", "knockout", "templatePath/scripts/config", "templatePath/scripts/PersonaBarDialog"], function ($, ko, cf) {
	logger.debug('>>> Entering define()');
    var helper,
        bindViewModel,
        rootFolder,
        utility,
        appId,
        appSecret,
        container;

    var wasDeactivated = false;

    var init = function (koObject, connectionHelper, pluginFolder, util) {
	logger.debug(`>>> Entering init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util})`);

        helper = connectionHelper;
        bindViewModel = koObject;
        rootFolder = pluginFolder;
        utility = util;
        koObject.container = ko.observable("");

        if (typeof dnn === "undefined") {
            window.Sys = !window.Sys ? window.top.Sys : window.Sys;
			logger.debug(`define(): window.Sys → ${window.Sys}`);
			logger.debug(`define(): window.String → ${window.String}`);
            window.dnn = !window.dnn ? window.top.dnn : window.dnn;
			logger.debug(`define(): window.dnn → ${window.dnn}`);
            window.String = !window.String ? window.top.String : window.String;
            window.String.format = !window.String.format ? window.top.String.format : window.String.format;
        }

	logger.debug(`init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util}): window.Sys → ${window.Sys}`);
	logger.debug(`<<< Exiting init(koObject=${koObject},connectionHelper=${connectionHelper},pluginFolder=${pluginFolder},util=${util})`);
    };

    var onSave = function (conn) {
	logger.debug(`>>> Entering onSave(conn=${conn})`);

        // Convert boolean to string as the API requires a dictionary of string values
        conn.configurations[2].value(conn.configurations[2].value() || 'false');
        conn.configurations[3].value(conn.configurations[3].value() || 'false');
        conn.configurations[4].value(conn.configurations[4].value() || 'false');
		logger.debug(`<<< Exiting onSave(conn=${conn})`);
    };

    var onSaveComplete = function (conn, id) {
	logger.debug(`>>> Entering onSaveComplete(conn=${conn},id=${id})`);

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
			logger.debug(`<<< Exiting onSaveComplete(conn=${conn},id=${id})`);
        }
    };

    var activateDeleteButton = function (conn) {
	logger.debug(`>>> Entering activateDeleteButton(conn=${conn})`);

        if (conn.buttons().length === 1) {

            conn.buttons.push({
                className: "secondarybtn",
                text: bindViewModel.resx.btnDelete,
                action: function (conn, e) {
				logger.debug(`>>> Entering action(conn=${conn},e=${e})`);

                    // Set the isDeactivating flag to true to override the default save behaviour
                    // Temporary workaround until delete functionality on connectors is improved
                    conn.configurations[6].value("true");
                    wasDeactivated = true;
                    conn.save(conn, e, onSaveComplete.bind(this, conn, conn.id));
					logger.debug(`<<< Exiting action(conn=${conn},e=${e})`);
                }
            });

        }

	logger.debug(`<<< Exiting activateDeleteButton(conn=${conn})`);
    };

    var getActionButtons = function () {
	logger.debug('>>> Entering getActionButtons()');


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
                        conn.configurations[6].value("true");
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
		logger.debug('<<< Exiting getActionButtons()');
    };

    return {
        init: init,
        onSave: onSave,
        getActionButtons: getActionButtons
    };

});