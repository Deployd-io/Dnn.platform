const logger = console;
'use strict';
define([
        'jquery',
        'knockout',
        '../../../../../Resources/Shared/components/DropDownList/dnn.DropDownList',
        '../../../../../Resources/Shared/scripts/dnn.DataStructures.js',
        '../../../../../Resources/Shared/scripts/TreeView/dnn.TreeView.js',
        '../../../../../Resources/Shared/scripts/TreeView/dnn.DynamicTreeView.js',
        '../../../../../Resources/Shared/scripts/dnn.jquery.extensions.js',
        '../../../../../Resources/Shared/scripts/dnn.extensions.js',
        '../../../../../js/dnn.servicesframework.js',
        '../../../../../Resources/Shared/scripts/jquery/dnn.jScrollbar.js',
        'css!../../../../../Resources/Shared/components/DropDownList/dnn.DropDownList.css'
],

function ($, ko) {
	logger.debug(`>>> Entering function(ko=${ko})`);
    var createFolderPicker = function (element, koOptions) {
        if (dnn[element.id]) {
            return;
        }
		logger.debug(`>>> Entering createFolderPicker(element=${element},koOptions=${koOptions})`);

        var selectFolderCallback = koOptions.selectFolderCallback;
        var koElement = koOptions.koElement;
        var id = "#{0}".replace(/\{0\}/g, element.id);
		logger.debug(`createFolderPicker(element=${element},koOptions=${koOptions}): id → ${id}`);
        var folderPicker;

        var selectFolderProxyCallback = function () {
			logger.debug('>>> Entering selectFolderProxyCallback()');
            selectFolderCallback.call(koElement, this.selectedItem());
			logger.debug('<<< Exiting selectFolderProxyCallback()');
        };

        var rootNodeId = koOptions.options.services.rootNodeId;
        if (typeof rootNodeId === "function") {
            koOptions.options.services.rootNodeId = rootNodeId();
        }
        var params = koOptions.options.services.parameters;
        if (typeof params === "function") {
            koOptions.options.services.parameters = params();
        }

        var options = {
            disabled: false,
            initialState: {
                selectedItem: koOptions.selectedFolder
            },
            services: {
                moduleId: '',
                serviceRoot: 'InternalServices',
                getTreeMethod: 'ItemListService/GetFolders',
                sortTreeMethod: 'ItemListService/SortFolders',
                getNodeDescendantsMethod: 'ItemListService/GetFolderDescendants',
                searchTreeMethod: 'ItemListService/SearchFolders',
                getTreeWithNodeMethod: 'ItemListService/GetTreePathForFolder',
                rootId: 'Root',
                parameters: {}
            },
            onSelectionChangedBackScript: selectFolderProxyCallback
        };

        $.extend(true, options, koOptions.options);

        dnn.createDropDownList(id, options, {});

        $(function () {
            folderPicker = dnn[element.id];
        });

        koElement.subscribe(function (folder) {
            if (folderPicker) {
				logger.debug('>>> Entering subscribe()');
                folderPicker.selectedItem({ key: folder.FolderID, value: folder.FolderName });
				logger.debug('<<< Exiting subscribe()');
            }
        });
		logger.debug(`<<< Exiting createFolderPicker(element=${element},koOptions=${koOptions})`);
    };

    var paramsChanged = function(param1, param2) {
		logger.debug(`>>> Entering paramsChanged(param1=${param1},param2=${param2})`);
        for (var a in param1) {
            if (param1.hasOwnProperty(a) && param2[a] !== param1[a]) {
                return true;
            }
        }

		logger.debug(`<<< Exiting paramsChanged(param1=${param1},param2=${param2})`);
        return false;
    }

    ko.bindingHandlers.folderPicker = {
        init: function (element, valueAccessor) {
			logger.debug(`>>> Entering init(element=${element},valueAccessor=${valueAccessor})`);
            var koOptions = valueAccessor();
            if (typeof koOptions.enabled === "undefined" || koOptions.enabled === true) {
				logger.debug(`<<< Exiting function(ko=${ko})`);
                createFolderPicker(element, koOptions);
				logger.debug(`<<< Exiting init(element=${element},valueAccessor=${valueAccessor})`);
            }
        },
        update: function (element, valueAccessor) {
			logger.debug(`>>> Entering update(element=${element},valueAccessor=${valueAccessor})`);
            var koOptions = valueAccessor();
            if (koOptions.enabled === true) {
				logger.debug(`update(element=${element},valueAccessor=${valueAccessor}): koOptions → ${koOptions}`);
                createFolderPicker(element, koOptions);
            }

            var folderPicker = dnn[element.id];
            if (folderPicker) {

                var params = koOptions.options.services.parameters;
                if (typeof params === "function") {
                    params = params();
                }

                var originParams = folderPicker.options.services.parameters;
                if (paramsChanged(originParams, params)) {
                    folderPicker.options.services.parameters = params;

                    if (folderPicker._treeView) {
                        folderPicker._treeView.controller.parameters = params;

                        folderPicker._treeView._$searchInput.val("");
                        folderPicker._treeView._$clearButton.hide();
                        folderPicker._treeView._search();
						logger.debug(`<<< Exiting update(element=${element},valueAccessor=${valueAccessor})`);
                    }
                }
            }
        }
    };
});
