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
			logger.debug(`>>> Entering requestService(type=${type},method=${method},controller=${controller},params=${params},callback=${callback},failure=${failure})`);
			logger.debug('>>> Entering requestService()');
            utility.sf.moduleRoot = "personaBar";
            utility.sf.controller = controller !== "" ? controller : "CssEditor";
			logger.debug(`>>> Entering requestService(curPortalId=${curPortalId})`);

            utility.sf[type].call(utility.sf, method, params, callback, failure);
			logger.debug('<<< Exiting requestService()');
			logger.debug(`<<< Exiting requestService(type=${type},method=${method},controller=${controller},params=${params},callback=${callback},failure=${failure})`);
        }

        var getPortals = function() {
			logger.debug('>>> Entering getPortals()');
            requestService('get', 'GetPortals', "Portals", {}, function(data) {
                viewModel.portals(data.Results);
                viewModel.portal(data.Results[0]);
            }, function() {
                // failed
                utility.notifyError('Failed...');
            });
			logger.debug('<<< Exiting getPortals()');
        }

        var getStyleSheet = function() {
			logger.debug('>>> Entering getStyleSheet()');
            requestService('get', 'GetStyleSheet', "", { 'portalId': curPortalId }, function(data) {
				logger.debug(`<<< Exiting requestService(curPortalId=${curPortalId})`);
                cssContent.setValue(data.Content);
            }, function() {
                // failed
                utility.notifyError('Failed...');
            });
			logger.debug(`<<< Exiting requestService(curPortalId=${curPortalId})`);
			logger.debug('<<< Exiting getStyleSheet()');
        }

        var saveStyleSheet = function() {
			logger.debug('>>> Entering saveStyleSheet()');
            requestService('post', 'UpdateStyleSheet', "", { 'portalId': curPortalId, 'styleSheetContent': cssContent.getValue() }, function(data) {
				logger.debug(`>>> Entering function(curPortalId=${curPortalId})`);
				logger.debug(`<<< Exiting function(curPortalId=${curPortalId})`);
                utility.notify(utility.resx.CssEditor.StyleSheetSaved);
            }, function() {
                // failed
                utility.notifyError('Failed...');
            });
			logger.debug('<<< Exiting saveStyleSheet()');
        }

        var restoreStyleSheet = function() {
			logger.debug('>>> Entering restoreStyleSheet()');
            utility.confirm(utility.resx.CssEditor.ConfirmRestore, utility.resx.CssEditor.RestoreButton, utility.resx.CssEditor.CancelButton, function() {
				logger.debug('>>> Entering confirm()');
                requestService('post', 'RestoreStyleSheet', "", { 'portalId': curPortalId }, function(data) {
                    cssContent.setValue(data.StyleSheetContent);
                    utility.notify(utility.resx.CssEditor.StyleSheetRestored);
                }, function() {
                    // failed
                    utility.notifyError('Failed...');
				logger.debug('<<< Exiting confirm()');
                });
            });
			logger.debug('<<< Exiting restoreStyleSheet()');
        }

        var initViewModel = function() {
			logger.debug('>>> Entering initViewModel()');
            viewModel = {
                resx: utility.resx.CssEditor,
                stylesheet: ko.observable(''),
                saveStyleSheet: saveStyleSheet,
                restoreStyleSheet: restoreStyleSheet,
                portals: ko.observableArray([]),
                portal: ko.observable('')
            };
			logger.debug('<<< Exiting initViewModel()');
        }

        var portalChanged = function(data) {
			logger.debug(`>>> Entering portalChanged(data=${data})`);
            if (data != null && data.PortalId != curPortalId) {
                curPortalId = data.PortalId;
                getStyleSheet();
				logger.debug(`portalChanged(data=${data}): curPortalId → ${curPortalId}`);
            }
			logger.debug(`<<< Exiting portalChanged(data=${data})`);
        }

        var init = function(wrapper, util, params, callback) {
			logger.debug(`>>> Entering init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
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
			logger.debug(`init(wrapper=${wrapper},util=${util},params=${params},callback=${callback}): panelHeight → ${panelHeight}`);
            if (panelHeight > 400) {
                $('#monaco-editor').height(panelHeight - 400);
            }

            if (typeof callback === 'function') {
                callback();
            }

            $('#monaco-editor').on("blur", function(cm) {
				logger.debug('>>> Entering on()');
				logger.debug('<<< Exiting on()');
                cm.save();
                return true;
            });

            viewModel.portal.subscribe(portalChanged);
        };

        var initCssEditor = function() {
			logger.debug('>>> Entering initCssEditor()');
            var monacoEditorLoaderScript = document.createElement('script');
            monacoEditorLoaderScript.type = 'text/javascript';
			logger.debug(`initCssEditor(): monacoEditorLoaderScript → ${monacoEditorLoaderScript}`);
			logger.debug(`initCssEditor(): monacoEditorLoaderScript.type → ${monacoEditorLoaderScript.type}`);
            monacoEditorLoaderScript.src = '/Resources/Shared/components/MonacoEditor/loader.js';
			logger.debug(`initCssEditor(): monacoEditorLoaderScript.src → ${monacoEditorLoaderScript.src}`);
            document.body.appendChild(monacoEditorLoaderScript);

            require.config({ paths: { 'vs': '/Resources/Shared/components/MonacoEditor' }});
            require(['vs/editor/editor.main'], function(monaco) {
			logger.debug('>>> Entering require()');
        
                self.MonacoEnvironment = {
                    getWorkerUrl: function (moduleId, label) {
						logger.debug(`>>> Entering getWorkerUrl(moduleId=${moduleId},label=${label})`);
                        if (label === 'typescript' || label === 'javascript') {
                        return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                            importScripts('${process.env.ASSET_PATH}/typescript.worker.js');`
                        )}`;
						logger.debug(`<<< Exiting getWorkerUrl(moduleId=${moduleId},label=${label})`);
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
				logger.debug('<<< Exiting require()');
			logger.debug('<<< Exiting initCssEditor()');
            });

        }

        var initMonacoEditor = function() {
			logger.debug('>>> Entering initMonacoEditor()');
            var theme = "vs-light";
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                theme = "vs-dark";
            }
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
                theme = e.matches ? "vs-dark" : "vs-light";
            });
			logger.debug('<<< Exiting initMonacoEditor()');

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
			logger.debug(`>>> Entering load(params=${params},callback=${callback})`);
            if (typeof callback === 'function') {
                callback();
				logger.debug(`<<< Exiting load(params=${params},callback=${callback})`);
            }
        };

        return {
            init: init,
            load: load
        };
    });