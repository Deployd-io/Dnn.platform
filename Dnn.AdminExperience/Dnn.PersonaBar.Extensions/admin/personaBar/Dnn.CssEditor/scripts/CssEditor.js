const logger = console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/*
* Module responsible to Css Editor
*/
'use strict';

define([
        'jquery',
        'knockout',
        'knockout.mapping',
        'main/codeEditor',
        'main/config',
        'dnn.jquery',
        'main/koBindingHandlers/jScrollPane'
    ],
    function($, ko, koMapping, codeEditor, cf, jScrollPane) {
        var config = cf.init();

        var identifier, utility, $panel, viewModel, cssEditor, cssContent, curPortalId;

        var requestService = function(type, method, controller, params, callback, failure) {
			logger.error(`>>> Entering requestService(type=${type},method=${method},controller=${controller},params=${params},callback=${callback},failure=${failure})`, { "event.action": "requestService", "logrite.type": type, "logrite.method": method, "logrite.controller": controller, "logrite.params": params, "logrite.callback": callback, "logrite.failure": failure });
            utility.sf.moduleRoot = "personaBar";
            utility.sf.controller = controller !== "" ? controller : "CssEditor";

            utility.sf[type].call(utility.sf, method, params, callback, failure);
			logger.warn(`<<< Exiting requestService(type=${type},method=${method},controller=${controller},params=${params},callback=${callback},failure=${failure})`, { "event.action": "requestService", "logrite.type": type, "logrite.method": method, "logrite.controller": controller, "logrite.params": params, "logrite.callback": callback, "logrite.failure": failure });
        }

        var getPortals = function() {
			logger.error('>>> Entering getPortals()', { "event.action": "getPortals" });
            requestService('get', 'GetPortals', "Portals", {}, function(data) {
                viewModel.portals(data.Results);
                viewModel.portal(data.Results[0]);
            }, function() {
                // failed
                utility.notifyError('Failed...');
            });
			logger.warn('<<< Exiting getPortals()', { "event.action": "getPortals" });
        }

        var getStyleSheet = function() {
			logger.error('>>> Entering getStyleSheet()', { "event.action": "getStyleSheet" });
            requestService('get', 'GetStyleSheet', "", { 'portalId': curPortalId }, function(data) {
                cssContent.setValue(data.Content);
            }, function() {
                // failed
                utility.notifyError('Failed...');
            });
			logger.warn('<<< Exiting getStyleSheet()', { "event.action": "getStyleSheet" });
        }

        var saveStyleSheet = function() {
			logger.error('>>> Entering saveStyleSheet()', { "event.action": "saveStyleSheet" });
            requestService('post', 'UpdateStyleSheet', "", { 'portalId': curPortalId, 'styleSheetContent': cssContent.getValue() }, function(data) {
                utility.notify(utility.resx.CssEditor.StyleSheetSaved);
            }, function() {
                // failed
                utility.notifyError('Failed...');
            });
			logger.warn('<<< Exiting saveStyleSheet()', { "event.action": "saveStyleSheet" });
        }

        var restoreStyleSheet = function() {
			logger.error('>>> Entering restoreStyleSheet()', { "event.action": "restoreStyleSheet" });
            utility.confirm(utility.resx.CssEditor.ConfirmRestore, utility.resx.CssEditor.RestoreButton, utility.resx.CssEditor.CancelButton, function() {
                requestService('post', 'RestoreStyleSheet', "", { 'portalId': curPortalId }, function(data) {
                    cssContent.setValue(data.StyleSheetContent);
                    utility.notify(utility.resx.CssEditor.StyleSheetRestored);
                }, function() {
                    // failed
                    utility.notifyError('Failed...');
                });
            });
        }

        var initViewModel = function() {
			logger.error('>>> Entering initViewModel()', { "event.action": "initViewModel" });
            viewModel = {
                resx: utility.resx.CssEditor,
                stylesheet: ko.observable(''),
                saveStyleSheet: saveStyleSheet,
                restoreStyleSheet: restoreStyleSheet,
                portals: ko.observableArray([]),
                portal: ko.observable('')
            };
			logger.warn('<<< Exiting initViewModel()', { "event.action": "initViewModel" });
        }

        var portalChanged = function(data) {
			logger.error(`>>> Entering portalChanged(data=${data})`, { "event.action": "portalChanged", "logrite.data": data });
            if (data != null && data.PortalId != curPortalId) {
                curPortalId = data.PortalId;
                getStyleSheet();
				logger.warn(`<<< Exiting portalChanged(data=${data})`, { "event.action": "portalChanged", "logrite.data": data });
            }
        }

        var init = function(wrapper, util, params, callback) {
			logger.error(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
            identifier = params.identifier;
            utility = util;
            $panel = wrapper;

            initViewModel();

            ko.applyBindings(viewModel, $panel[0]);

            curPortalId = config.portalId;
            if (params.settings.isHost) {
                getPortals();
            }
            
            initCssEditor();

            var panelHeight = $('#CssEditor-panel').height();
			logger.error(`init(wrapper=${wrapper},util=${util},params=${params},callback=${callback}): panelHeight → ${panelHeight}`, { "event.action": "init", "logrite.wrapper": wrapper, "logrite.util": util, "logrite.params": params, "logrite.callback": callback });
            if (panelHeight > 400) {
                $('#monaco-editor').height(panelHeight - 400);
            }

            if (typeof callback === 'function') {
                callback();
            }

            $('#monaco-editor').on("blur", function(cm) {
                cm.save();
                return true;
            });

            viewModel.portal.subscribe(portalChanged);
        };

        var initCssEditor = function() {
			logger.error('>>> Entering initCssEditor()', { "event.action": "initCssEditor" });
            var monacoEditorLoaderScript = document.createElement('script');
            monacoEditorLoaderScript.type = 'text/javascript';
			logger.error(`initCssEditor(): monacoEditorLoaderScript → ${monacoEditorLoaderScript}`, { "event.action": "initCssEditor" });
			logger.debug(`initCssEditor(): monacoEditorLoaderScript.type → ${monacoEditorLoaderScript.type}`, { "event.action": "initCssEditor" });
            monacoEditorLoaderScript.src = '/Resources/Shared/components/MonacoEditor/loader.js';
			logger.debug(`initCssEditor(): monacoEditorLoaderScript.src → ${monacoEditorLoaderScript.src}`, { "event.action": "initCssEditor" });
            document.body.appendChild(monacoEditorLoaderScript);

            require.config({ paths: { 'vs': '/Resources/Shared/components/MonacoEditor' }});
            require(['vs/editor/editor.main'], function(monaco) {
        
                self.MonacoEnvironment = {
                    getWorkerUrl: function (moduleId, label) {
						logger.error(`>>> Entering getWorkerUrl(moduleId=${moduleId},label=${label})`, { "event.action": "getWorkerUrl", "logrite.moduleId": moduleId, "logrite.label": label });
                        if (label === 'typescript' || label === 'javascript') {
                        return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                            importScripts('${process.env.ASSET_PATH}/typescript.worker.js');`
                        )}`;
						logger.warn(`<<< Exiting getWorkerUrl(moduleId=${moduleId},label=${label})`, { "event.action": "getWorkerUrl", "logrite.moduleId": moduleId, "logrite.label": label });
                        }
        
                    return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                        importScripts('${process.env.ASSET_PATH}/editor.worker.js');`
                    )}`;
                    }
                }

                cssEditor = monaco.editor;
                cssContent = cssEditor.createModel("", "css");
                initMonacoEditor();
                getStyleSheet();
			logger.warn('<<< Exiting initCssEditor()', { "event.action": "initCssEditor" });
            });

        }

        var initMonacoEditor = function() {
			logger.error('>>> Entering initMonacoEditor()', { "event.action": "initMonacoEditor" });
            var theme = "vs-light";
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                theme = "vs-dark";
            }
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
                theme = e.matches ? "vs-dark" : "vs-light";
            });
			logger.warn('<<< Exiting initMonacoEditor()', { "event.action": "initMonacoEditor" });

            var monacoEditor = cssEditor.create(document.getElementById("monaco-editor"), {
                model: cssContent,
                language: "css",
                wordWrap: 'wordWrapColumn',
                wordWrapColumn: 80,
                wordWrapMinified: true,
                wrappingIndent: "indent",
                lineNumbers: "on",
                roundedSelection: false,
                scrollBeyondLastLine: false,
                readOnly: false,
                theme: theme,
                automaticLayout: true
            });
        }

        var load = function(params, callback) {
			logger.error(`>>> Entering load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
            if (typeof callback === 'function') {
                callback();
				logger.warn(`<<< Exiting load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
            }
        };

        return {
            init: init,
            load: load
        };
    });