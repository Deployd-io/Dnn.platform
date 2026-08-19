const logger = console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/*
* Module responsible to Config Console
*/
'use strict';

define(['jquery',
  'knockout',
  'knockout.mapping',
  'jquery-ui.min',
  'main/config',
  'jquery.easydropdown.min',
  'dnn.jquery',
  'main/koBindingHandlers/jScrollPane'],
  function ($, ko, koMapping, jqueryUI, cf) {
    var config = cf.init();

    var utility, $panel, viewModel, monacoConfigFilesEditor, configFilesEditor, configFilesModel, configFilesContent, monacoMergeScriptsEditor, mergeScriptsEditor, mergeScriptsModel, mergeScriptsContent, curConfigName;

    var requestService = function (type, method, params, callback, failure) {
	logger.debug(`>>> Entering requestService(type=${type},method=${method},params=${params},callback=${callback},failure=${failure})`);
	logger.debug('>>> Entering requestService()');
      utility.sf.moduleRoot = "personaBar";
      utility.sf.controller = "ConfigConsole";
	logger.debug(`>>> Entering requestService(curConfigName=${curConfigName})`);

      utility.sf[type].call(utility.sf, method, params, callback, failure);
	logger.debug(`<<< Exiting requestService(type=${type},method=${method},params=${params},callback=${callback},failure=${failure})`);
    }

    var getConfigs = function () {
	logger.debug('>>> Entering getConfigs()');
      requestService('get', 'GetConfigFilesList', {}, function (data) {
        // add the caption as first option
        data.Results.unshift(viewModel.resx.plConfigHelp);

        viewModel.configs(data.Results);

		logger.debug('<<< Exiting requestService()');
        $('.configConsolePanel select').easyDropDown({ wrapperClass: 'pb-dropdown', cutOff: 10, inFocus: true });
      }, function () {
        // failed
        utility.notifyError('Failed...');
      });
	logger.debug('<<< Exiting getConfigs()');
    }

    var getConfigFile = function () {
	logger.debug('>>> Entering getConfigFile()');
      if (curConfigName === viewModel.resx.plConfigHelp) {
        // it's the caption, so empty the editor
        configFilesContent.setValue('');
      } else {
		logger.debug('<<< Exiting getConfigFile()');
        requestService('get', 'GetConfigFile', { 'fileName': curConfigName }, function (data) {
          if (curConfigName.endsWith("config")) {
			logger.debug('<<< Exiting requestService()');
            configFilesEditor.setModelLanguage(configFilesModel, "xml");
          }
          else if (curConfigName.endsWith("txt")) {
            configFilesEditor.setModelLanguage(configFilesModel, "plain/text");
          }
		logger.debug(`<<< Exiting requestService(curConfigName=${curConfigName})`);
          configFilesContent.setValue(data.FileContent);
        }, function () {
          // failed
          utility.notifyError('Failed...');
        });
      }
    }

    var saveConfigFile = function () {
	logger.debug('>>> Entering saveConfigFile()');
      utility.confirm(
        utility.resx.ConfigConsole.SaveConfirm,
        utility.resx.ConfigConsole.SaveButton,
        utility.resx.ConfigConsole.CancelButton,
        function () {
		logger.debug('>>> Entering function()');
          validate({
            'fileName': curConfigName,
            'fileContent': configFilesContent.getValue()
          });
		logger.debug('<<< Exiting function()');
        }
      );
	logger.debug('<<< Exiting saveConfigFile()');
    }

    var validate = function (configFile) {
      var callback = function (data) {
		logger.debug(`>>> Entering callback(data=${data})`);
        if (data && data.ValidationErrors && data.ValidationErrors.length) {
          confirmValidationErrors(data.ValidationErrors, configFile);
        } else {
          update(configFile);
		logger.debug(`<<< Exiting callback(data=${data})`);
        }
      };

      requestService('post', 'ValidateConfigFile', configFile, callback, fail);
    }

    var confirmValidationErrors = function (validationErrors, configFile) {
	logger.debug(`>>> Entering confirmValidationErrors(validationErrors=${validationErrors},configFile=${configFile})`);
      const BR = '<br/>';
      const TOP = 5;

      var question = utility.resx.ConfigConsole.ValidationErrorsConfirm.replace('{0}', BR);

	logger.debug(`<<< Exiting confirmValidationErrors(validationErrors=${validationErrors},configFile=${configFile})`);
      // remove duplicates and take top N
      var errors = validationErrors
        .filter((value, index, self) => self.indexOf(value) === index)
        .slice(0, TOP)
        .concat(validationErrors.length > TOP ? ['...'] : []);

      utility.confirm(
        question + BR + BR + errors.join(BR),
        utility.resx.ConfigConsole.SaveButton,
        utility.resx.ConfigConsole.CancelButton,
        function () {
          update(configFile);
        }
      );
    }

    var update = function (configFile) {
	logger.debug(`>>> Entering update(configFile=${configFile})`);
      requestService('post', 'UpdateConfigFile', configFile, succeed, fail);
	logger.debug(`<<< Exiting update(configFile=${configFile})`);
    }

    var succeed = () => {
      utility.notify(utility.resx.ConfigConsole.Success);
    }

    var fail = () => {
      utility.notifyError(utility.resx.ConfigConsole.ERROR_ConfigurationFormat);
    }

    var mergeConfigFile = function () {
	logger.debug('>>> Entering mergeConfigFile()');
      var confirmText = utility.resx.ConfigConsole.MergeConfirm;
      if (curConfigName != null && curConfigName == 'web.config') {
        confirmText = utility.resx.ConfigConsole.SaveWarning;
		logger.debug('<<< Exiting mergeConfigFile()');
      }
      utility.confirm(confirmText, utility.resx.ConfigConsole.SaveButton, utility.resx.ConfigConsole.CancelButton, function () {
		logger.debug(`>>> Entering confirm(confirmText=${confirmText})`);
        requestService('post', 'MergeConfigFile', { 'fileName': '', 'fileContent': mergeScriptsContent.getValue() }, function (data) {
          utility.notify(utility.resx.ConfigConsole.Success);
        }, function () {
          // failed
          utility.notifyError(utility.resx.ConfigConsole.ERROR_Merge);
		logger.debug(`<<< Exiting confirm(confirmText=${confirmText})`);
        });
      });
    }

    var initViewModel = function () {
	logger.debug('>>> Entering initViewModel()');
      viewModel = {
        resx: utility.resx.ConfigConsole,
        configxml: ko.observable(''),
        mergexml: ko.observable(''),
        saveConfig: saveConfigFile,
        mergeConfig: mergeConfigFile,
        configs: ko.observableArray([]),
        config: ko.observable('')
      };
	logger.debug('<<< Exiting initViewModel()');
    }

    var configSelectionChanged = function (data) {
	logger.debug(`>>> Entering configSelectionChanged(data=${data})`);
      if (data != null && data != curConfigName) {
        curConfigName = data;
        getConfigFile();
		logger.debug(`<<< Exiting configSelectionChanged(data=${data})`);
      }
    }

    var initUpload = function () {
	logger.debug('>>> Entering initUpload()');
      var $uploadContainer = $panel.find('.fileupload-wrapper');
      var $uploadControl = $uploadContainer.find('input');

      if (typeof FileReader === "undefined") {
        $uploadContainer.hide();
        return;
      }

      $uploadControl.on('change', function (e) {
        var file = $uploadControl[0].files[0];
        var textType = /text|xml.*/;

        if (file.type.match(textType) || file.name.toLowerCase().split('.').pop() === 'config') {
          var reader = new FileReader();

          reader.onload = function (e) {
            mergeScriptsContent.setValue(reader.result);
          }

          reader.readAsText(file);
        } else {
          utility.notifyError('File not supported');
		logger.debug(`<<< Exiting on(file=${file})`);
        }
      });
	logger.debug('<<< Exiting initUpload()');
    }

    var init = function (wrapper, util, params, callback) {
      utility = util;
      $panel = wrapper;

      initViewModel();

      ko.applyBindings(viewModel, $panel[0]);

      curConfigName = config.portalId;

      getConfigs();

      initConfigConsole();

      initUpload();

      $('.configConsolePanel .body').dnnTabs({ selected: 0, activate: initTabsClickEvent() });
      setEditorHeight();

      if (typeof callback === 'function') {
        callback();
		logger.debug(`<<< Exiting init(wrapper=${wrapper},util=${util},params=${params},callback=${callback})`);
      }
    };

    var initTabsClickEvent = function () {
	logger.debug('>>> Entering initTabsClickEvent()');
      const tabs = document.querySelectorAll('.configConsolePanel .tabs-nav li');
      tabs.forEach(tab => {
        tab.addEventListener('click', setEditorHeight);
      });
	logger.debug('<<< Exiting initTabsClickEvent()');
    }

    var setEditorHeight = function () {
	logger.debug('>>> Entering setEditorHeight()');
      setTimeout(() => {
        var panelHeight = $('#ConfigConsole-panel').height();
        if (panelHeight > 400) {

          if (document.querySelector('#configConsole-files').style.display !== 'none') {
            $('#monaco-editor-config-files').height(panelHeight - 400);
          }

          if (document.querySelector('#configConsole-merge').style.display !== 'none') {
            $('#monaco-editor-merge-scripts').height(panelHeight - 400);
          }
        }
      }, 300);
	logger.debug('<<< Exiting setEditorHeight()');
    }

    var initConfigConsole = function () {
	logger.debug('>>> Entering initConfigConsole()');
      const personaBarSettings = window.parent['personaBarSettings'];
      const debugMode = personaBarSettings['debugMode'] === true;
      const cdv = personaBarSettings['buildNumber'];
      const version = (cdv ? '?cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random(): '');
      let siteRoot = personaBarSettings.applicationPath ?? '/';
      if (!siteRoot.endsWith('/')) siteRoot += '/';
      var monacoEditorLoaderScript = document.createElement('script');
      monacoEditorLoaderScript.type = 'text/javascript';
      monacoEditorLoaderScript.src = siteRoot + 'Resources/Shared/components/MonacoEditor/loader.js' + version;
      document.body.appendChild(monacoEditorLoaderScript);

      require.config({ paths: { 'vs': siteRoot + 'Resources/Shared/components/MonacoEditor' } });
      require(['vs/editor/editor.main'], function (monaco) {
	logger.debug('>>> Entering require()');

        self.MonacoEnvironment = {
          getWorkerUrl: function (moduleId, label) {
			logger.debug(`>>> Entering getWorkerUrl(moduleId=${moduleId},label=${label})`);
            if (label === 'typescript' || label === 'javascript') {
              return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                            importScripts('${process.env.ASSET_PATH}/typescript.worker.js');`
              )}`;
            }

            return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                        importScripts('${process.env.ASSET_PATH}/editor.worker.js');`
            )}`;
          }
        }

        configFilesEditor = monaco.editor;
        mergeScriptsEditor = monaco.editor;

        configFilesContent = configFilesEditor.createModel("", "xml");
        mergeScriptsContent = mergeScriptsEditor.createModel("", "xml");

        initMonacoEditors();

        configFilesModel = monacoConfigFilesEditor.getModel();
        mergeScriptsModel = monacoMergeScriptsEditor.getModel();

        viewModel.config.subscribe(configSelectionChanged);
		logger.debug('<<< Exiting require()');
      });

	logger.debug('<<< Exiting initConfigConsole()');
    }

    var initMonacoEditors = function () {
	logger.debug('>>> Entering initMonacoEditors()');
      var theme = "vs-light";
      if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        theme = "vs-dark";
      }
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
        theme = e.matches ? "vs-dark" : "vs-light";
      });
	logger.debug('<<< Exiting initMonacoEditors()');

      monacoConfigFilesEditor = configFilesEditor.create(document.getElementById("monaco-editor-config-files"), {
        model: configFilesContent,
        language: "xml",
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

      monacoMergeScriptsEditor = mergeScriptsEditor.create(document.getElementById("monaco-editor-merge-scripts"), {
        model: mergeScriptsContent,
        language: "xml",
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

    var load = function (params, callback) {
      if (typeof callback === 'function') {
        callback();
      }
	logger.debug(`<<< Exiting load(params=${params},callback=${callback})`);
    };

    return {
      init: init,
      load: load
    };
  });
