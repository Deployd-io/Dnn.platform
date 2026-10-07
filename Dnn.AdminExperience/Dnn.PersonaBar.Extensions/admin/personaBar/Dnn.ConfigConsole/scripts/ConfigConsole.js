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
	logger.error(`>>> Entering requestService(type=${type},method=${method},params=${params},callback=${callback},failure=${failure})`, { "event.action": "requestService", "logrite.type": type, "logrite.method": method, "logrite.params": params, "logrite.callback": callback, "logrite.failure": failure });
      utility.sf.moduleRoot = "personaBar";
      utility.sf.controller = "ConfigConsole";

      utility.sf[type].call(utility.sf, method, params, callback, failure);
	logger.warn(`<<< Exiting requestService(type=${type},method=${method},params=${params},callback=${callback},failure=${failure})`, { "event.action": "requestService", "logrite.type": type, "logrite.method": method, "logrite.params": params, "logrite.callback": callback, "logrite.failure": failure });
    }

    var getConfigs = function () {
	logger.error('>>> Entering getConfigs()', { "event.action": "getConfigs" });
      requestService('get', 'GetConfigFilesList', {}, function (data) {
        // add the caption as first option
        data.Results.unshift(viewModel.resx.plConfigHelp);

        viewModel.configs(data.Results);

        $('.configConsolePanel select').easyDropDown({ wrapperClass: 'pb-dropdown', cutOff: 10, inFocus: true });
      }, function () {
        // failed
        utility.notifyError('Failed...');
      });
	logger.warn('<<< Exiting getConfigs()', { "event.action": "getConfigs" });
    }

    var getConfigFile = function () {
	logger.error('>>> Entering getConfigFile()', { "event.action": "getConfigFile" });
      if (curConfigName === viewModel.resx.plConfigHelp) {
        // it's the caption, so empty the editor
        configFilesContent.setValue('');
      } else {
        requestService('get', 'GetConfigFile', { 'fileName': curConfigName }, function (data) {
          if (curConfigName.endsWith("config")) {
            configFilesEditor.setModelLanguage(configFilesModel, "xml");
          }
          else if (curConfigName.endsWith("txt")) {
            configFilesEditor.setModelLanguage(configFilesModel, "plain/text");
          }
          configFilesContent.setValue(data.FileContent);
        }, function () {
          // failed
          utility.notifyError('Failed...');
        });
		logger.warn('<<< Exiting getConfigFile()', { "event.action": "getConfigFile" });
      }
    }

    var saveConfigFile = function () {
	logger.error('>>> Entering saveConfigFile()', { "event.action": "saveConfigFile" });
      utility.confirm(
        utility.resx.ConfigConsole.SaveConfirm,
        utility.resx.ConfigConsole.SaveButton,
        utility.resx.ConfigConsole.CancelButton,
        function () {
          validate({
            'fileName': curConfigName,
            'fileContent': configFilesContent.getValue()
          });
        }
      );
	logger.warn('<<< Exiting saveConfigFile()', { "event.action": "saveConfigFile" });
    }

    var validate = function (configFile) {
      var callback = function (data) {
		logger.error(`>>> Entering callback(data=${data})`, { "event.action": "callback", "logrite.data": data });
        if (data && data.ValidationErrors && data.ValidationErrors.length) {
          confirmValidationErrors(data.ValidationErrors, configFile);
        } else {
          update(configFile);
		logger.warn(`<<< Exiting callback(data=${data})`, { "event.action": "callback", "logrite.data": data });
        }
      };

      requestService('post', 'ValidateConfigFile', configFile, callback, fail);
    }

    var confirmValidationErrors = function (validationErrors, configFile) {
	logger.error(`>>> Entering confirmValidationErrors(validationErrors=${validationErrors},configFile=${configFile})`, { "event.action": "confirmValidationErrors", "logrite.validationErrors": validationErrors, "logrite.configFile": configFile });
      const BR = '<br/>';
      const TOP = 5;
	logger.error(`confirmValidationErrors(validationErrors=${validationErrors},configFile=${configFile}): BR → ${BR}`, { "event.action": "confirmValidationErrors", "logrite.validationErrors": validationErrors, "logrite.configFile": configFile });

      var question = utility.resx.ConfigConsole.ValidationErrorsConfirm.replace('{0}', BR);
	logger.error(`confirmValidationErrors(validationErrors=${validationErrors},configFile=${configFile}): question → ${question}`, { "event.action": "confirmValidationErrors", "logrite.validationErrors": validationErrors, "logrite.configFile": configFile });

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
		logger.warn(`<<< Exiting confirmValidationErrors(validationErrors=${validationErrors},configFile=${configFile})`, { "event.action": "confirmValidationErrors", "logrite.validationErrors": validationErrors, "logrite.configFile": configFile });
        }
      );
    }

    var update = function (configFile) {
	logger.error(`>>> Entering update(configFile=${configFile})`, { "event.action": "update", "logrite.configFile": configFile });
      requestService('post', 'UpdateConfigFile', configFile, succeed, fail);
	logger.warn(`<<< Exiting update(configFile=${configFile})`, { "event.action": "update", "logrite.configFile": configFile });
    }

    var succeed = () => {
      utility.notify(utility.resx.ConfigConsole.Success);
    }

    var fail = () => {
      utility.notifyError(utility.resx.ConfigConsole.ERROR_ConfigurationFormat);
    }

    var mergeConfigFile = function () {
	logger.error('>>> Entering mergeConfigFile()', { "event.action": "mergeConfigFile" });
      var confirmText = utility.resx.ConfigConsole.MergeConfirm;
      if (curConfigName != null && curConfigName == 'web.config') {
        confirmText = utility.resx.ConfigConsole.SaveWarning;
      }
      utility.confirm(confirmText, utility.resx.ConfigConsole.SaveButton, utility.resx.ConfigConsole.CancelButton, function () {
        requestService('post', 'MergeConfigFile', { 'fileName': '', 'fileContent': mergeScriptsContent.getValue() }, function (data) {
          utility.notify(utility.resx.ConfigConsole.Success);
        }, function () {
          // failed
          utility.notifyError(utility.resx.ConfigConsole.ERROR_Merge);
        });
      });
    }

    var initViewModel = function () {
	logger.error('>>> Entering initViewModel()', { "event.action": "initViewModel" });
      viewModel = {
        resx: utility.resx.ConfigConsole,
        configxml: ko.observable(''),
        mergexml: ko.observable(''),
        saveConfig: saveConfigFile,
        mergeConfig: mergeConfigFile,
        configs: ko.observableArray([]),
        config: ko.observable('')
      };
	logger.warn('<<< Exiting initViewModel()', { "event.action": "initViewModel" });
    }

    var configSelectionChanged = function (data) {
	logger.error(`>>> Entering configSelectionChanged(data=${data})`, { "event.action": "configSelectionChanged", "logrite.data": data });
      if (data != null && data != curConfigName) {
        curConfigName = data;
        getConfigFile();
		logger.warn(`<<< Exiting configSelectionChanged(data=${data})`, { "event.action": "configSelectionChanged", "logrite.data": data });
      }
    }

    var initUpload = function () {
	logger.error('>>> Entering initUpload()', { "event.action": "initUpload" });
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
			logger.error(`>>> Entering onload(e=${e})`, { "event.action": "onload", "logrite.e": e });
            mergeScriptsContent.setValue(reader.result);
			logger.warn(`<<< Exiting onload(e=${e})`, { "event.action": "onload", "logrite.e": e });
          }

          reader.readAsText(file);
        } else {
          utility.notifyError('File not supported');
        }
      });
	logger.warn('<<< Exiting initUpload()', { "event.action": "initUpload" });
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
      }
    };

    var initTabsClickEvent = function () {
	logger.error('>>> Entering initTabsClickEvent()', { "event.action": "initTabsClickEvent" });
      const tabs = document.querySelectorAll('.configConsolePanel .tabs-nav li');
      tabs.forEach(tab => {
		logger.error(`initTabsClickEvent(): tabs → ${tabs}`, { "event.action": "initTabsClickEvent" });
        tab.addEventListener('click', setEditorHeight);
      });
	logger.warn('<<< Exiting initTabsClickEvent()', { "event.action": "initTabsClickEvent" });
    }

    var setEditorHeight = function () {
	logger.error('>>> Entering setEditorHeight()', { "event.action": "setEditorHeight" });
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
	logger.warn('<<< Exiting setEditorHeight()', { "event.action": "setEditorHeight" });
    }

    var initConfigConsole = function () {
	logger.error('>>> Entering initConfigConsole()', { "event.action": "initConfigConsole" });
      const personaBarSettings = window.parent['personaBarSettings'];
      const debugMode = personaBarSettings['debugMode'] === true;
	logger.error(`initConfigConsole(): personaBarSettings → ${personaBarSettings}`, { "event.action": "initConfigConsole" });
	logger.error(`initConfigConsole(): debugMode → ${debugMode}`, { "event.action": "initConfigConsole" });
      const cdv = personaBarSettings['buildNumber'];
      const version = (cdv ? '?cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random(): '');
	logger.error(`initConfigConsole(): cdv → ${cdv}`, { "event.action": "initConfigConsole" });
	logger.error(`initConfigConsole(): version → ${version}`, { "event.action": "initConfigConsole" });
      let siteRoot = personaBarSettings.applicationPath ?? '/';
      if (!siteRoot.endsWith('/')) siteRoot += '/';
	logger.error(`initConfigConsole(): siteRoot → ${siteRoot}`, { "event.action": "initConfigConsole" });
      var monacoEditorLoaderScript = document.createElement('script');
	logger.error(`initConfigConsole(): monacoEditorLoaderScript → ${monacoEditorLoaderScript}`, { "event.action": "initConfigConsole" });
	logger.debug(`initConfigConsole(): monacoEditorLoaderScript.src → ${monacoEditorLoaderScript.src}`, { "event.action": "initConfigConsole" });
      monacoEditorLoaderScript.type = 'text/javascript';
	logger.debug(`initConfigConsole(): monacoEditorLoaderScript.type → ${monacoEditorLoaderScript.type}`, { "event.action": "initConfigConsole" });
      monacoEditorLoaderScript.src = siteRoot + 'Resources/Shared/components/MonacoEditor/loader.js' + version;
      document.body.appendChild(monacoEditorLoaderScript);

      require.config({ paths: { 'vs': siteRoot + 'Resources/Shared/components/MonacoEditor' } });
      require(['vs/editor/editor.main'], function (monaco) {

        self.MonacoEnvironment = {
          getWorkerUrl: function (moduleId, label) {
			logger.error(`>>> Entering getWorkerUrl(moduleId=${moduleId},label=${label})`, { "event.action": "getWorkerUrl", "logrite.moduleId": moduleId, "logrite.label": label });
            if (label === 'typescript' || label === 'javascript') {
              return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                            importScripts('${process.env.ASSET_PATH}/typescript.worker.js');`
              )}`;
            }

			logger.warn(`<<< Exiting getWorkerUrl(moduleId=${moduleId},label=${label})`, { "event.action": "getWorkerUrl", "logrite.moduleId": moduleId, "logrite.label": label });
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
      });

	logger.warn('<<< Exiting initConfigConsole()', { "event.action": "initConfigConsole" });
    }

    var initMonacoEditors = function () {
	logger.error('>>> Entering initMonacoEditors()', { "event.action": "initMonacoEditors" });
      var theme = "vs-light";
      if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        theme = "vs-dark";
      }
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
        theme = e.matches ? "vs-dark" : "vs-light";
      });
	logger.warn('<<< Exiting initMonacoEditors()', { "event.action": "initMonacoEditors" });

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
	logger.warn(`<<< Exiting load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
    };

    return {
      init: init,
      load: load
    };
  });
