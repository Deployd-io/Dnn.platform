var logger = (typeof logger !== 'undefined' && logger) || console;
; if (typeof window.dnn === "undefined" || window.dnn === null) { window.dnn = {}; }; //var dnn = dnn || {};

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

(function ($, window, document, undefined) {
    "use strict";

    var $element = function (element, props) {
		logger.error(`>>> Entering $element(element=${element},props=${props})`, { "event.action": "$element", "logrite.element": element, "logrite.props": props });
        var $e = $(document.createElement(element));
        props && $e.attr(props);
		logger.warn(`<<< Exiting $element(element=${element},props=${props})`, { "event.action": "$element", "logrite.element": element, "logrite.props": props });
        return $e;
    };

    var suportAjaxUpload = (function () {
        var support;
        return function() {
            if (typeof support === "undefined") {
                var xhr = new XMLHttpRequest;
                support = !!(xhr && ('upload' in xhr) && ('onprogress' in xhr.upload));
            }
            return support;
        };
    })();

    var FileUploadPanel = this.FileUploadPanel = function (options) {
		logger.error(`>>> Entering FileUploadPanel(options=${options})`, { "event.action": "FileUploadPanel", "logrite.options": options });
        this.options = options;
        this.init();
		logger.debug(`FileUploadPanel(options=${options}): this.options → ${this.options}`, { "event.action": "FileUploadPanel", "logrite.options": options });
    };

    FileUploadPanel.prototype = {

        constructor: FileUploadPanel,

        init: function () {
			logger.error('>>> Entering init()', { "event.action": "init" });
            this.options = $.extend({}, FileUploadPanel.defaults(), this.options);

            this.$this = $(this);

            this._uploadMethods = new dnn.Enum([{ key: 0, value: "undefined" }, { key: 1, value: "local" }, { key: 2, value: "web" }]);
            this._uploadMethod = this._uploadMethods.local;

			logger.debug(`init(): this._uploadMethod → ${this._uploadMethod}`, { "event.action": "init" });
            this._initPanel();
			logger.warn('<<< Exiting init()', { "event.action": "init" });
        },

        setUploadPath: function (path) {
            this.options.folderPath = path;
            var folder = path == '' ? this.options.folderPicker.initialState.selectedItem.value
                : path.split('/').slice(-2)[0];
            this.$this[0].$element.find("div.fu-folder-picker-container > span").text(folder);
			logger.warn(`<<< Exiting setUploadPath(path=${path})`, { "event.action": "setUploadPath", "logrite.path": path });
        },

        addFiles: function (files) {
			logger.error(`>>> Entering addFiles(files=${files})`, { "event.action": "addFiles", "logrite.files": files });
            this._$inputFileControl.fileupload('add', { files: files });
			logger.warn(`<<< Exiting addFiles(files=${files})`, { "event.action": "addFiles", "logrite.files": files });
        },

        _selectUpload: function (uploadMethod, eventObject) {
			logger.error(`>>> Entering _selectUpload(uploadMethod=${uploadMethod},eventObject=${eventObject})`, { "event.action": "_selectUpload", "logrite.uploadMethod": uploadMethod, "logrite.eventObject": eventObject });
            var isLocal = uploadMethod === this._uploadMethods.local;
            this.$element.find(".fu-dialog-content-fileupload-local").toggle(isLocal);
            this.$element.find(".fu-dialog-content-fileupload-web").toggle(!isLocal);
            this._uploadMethod = uploadMethod;

			logger.debug(`_selectUpload(uploadMethod=${uploadMethod},eventObject=${eventObject}): this._uploadMethod → ${this._uploadMethod}`, { "event.action": "_selectUpload", "logrite.uploadMethod": uploadMethod, "logrite.eventObject": eventObject });
            var clickedElement = eventObject.currentTarget;
            this._$buttonGroup.find('a').each(function(i, element) {
                if (element !== clickedElement) {
                    $(element).removeClass("active");
                }
            });
            $(clickedElement).addClass('active');
			logger.warn(`<<< Exiting _selectUpload(uploadMethod=${uploadMethod},eventObject=${eventObject})`, { "event.action": "_selectUpload", "logrite.uploadMethod": uploadMethod, "logrite.eventObject": eventObject });
        },

        _createLayout: function () {
			logger.error('>>> Entering _createLayout()', { "event.action": "_createLayout" });
            var checkBoxId = dnn.uid("fu_");
            this._folderPicker = new dnn.DropDownList(null, this.options.folderPicker);
			logger.error(`_createLayout(): checkBoxId → ${checkBoxId}`, { "event.action": "_createLayout" });
            dnn[this._folderPicker.id()] = this._folderPicker;

            var dialog = $element('div', { tabindex: '-1', 'class': 'fu-container', role: 'dialog' }).append(
                $element('div', { 'class': 'fu-dialog-content' }).append(
                    $element("div", { 'class': 'fu-dialog-content-header' }).append(
                        $element("div", { 'class': 'dnnLeft' }).append(
                            $element("ul", { 'class': 'dnnButtonGroup' }).append(
                                $element("li").append(
                                    $element("a", { href: "javascript:void(0);", 'class': 'upload-file active' }).text(this.options.resources.uploadFileMethod).on("click", $.proxy(this._selectUpload, this, this._uploadMethods.local))
                                )
                            ),
                            this._$decompressOption = $element("span").append(
                                $element("input", { type: 'checkbox', id: checkBoxId }),
                                $element("label", { 'for': checkBoxId, 'class': 'fu-decompress-label' }).text(this.options.resources.decompressLabel)
                            )
                        ),
                        $element("div", { 'class': 'fu-folder-picker-container dnnRight' }).append(
                            $element("label").text(this.options.resources.uploadToFolderLabel),
                            !(this.options.folderPicker.disabled) ? this._folderPicker.$element.addClass("dnnLeftComboBox") : $element("span").append(this.options.folderPicker.initialState.selectedItem.value)
                        )
                    ),
                    $element("div", { 'class': 'fu-dialog-content-fileupload-local' }).append(
                        $element("div", { 'class': 'fu-dialog-drag-and-drop-area' }).append(
                            $element("div", { 'class': 'fu-dialog-drag-and-drop-area-message' })
                        )
                    ),
                    $element("div", { style: 'display: none', 'class': 'fu-fileupload-statuses-container' }).append(
                        $element("ul", { 'class': 'fu-fileupload-statuses' })
                    )
                )
            );

			logger.error(`_createLayout(): dialog → ${dialog}`, { "event.action": "_createLayout" });
			logger.warn('<<< Exiting _createLayout()', { "event.action": "_createLayout" });
            return dialog;
        },

        _getFileExtension: function(fileName) {
            var parts = fileName.split(".");
            if (parts.length === 1 || (parts[0] === "" && parts.length === 2)) {
                // If a.length is one, it's a visible file with no extension;
                // If a[0] === "" and a.length === 2 it's a hidden file with no extension ie. .htaccess;
                return "";
            }
			logger.warn(`<<< Exiting _getFileExtension(fileName=${fileName})`, { "event.action": "_getFileExtension", "logrite.fileName": fileName });
            return parts.pop();
        },

        _isValidExtension: function (fileName, acceptableExtensions) {
			logger.error(`>>> Entering _isValidExtension(fileName=${fileName},acceptableExtensions=${acceptableExtensions})`, { "event.action": "_isValidExtension", "logrite.fileName": fileName, "logrite.acceptableExtensions": acceptableExtensions });
            if (typeof acceptableExtensions === "undefined" || acceptableExtensions === null || acceptableExtensions.length === 0) {
                return true;
            }
            for (var i = 0; i < acceptableExtensions.length; i++) {
                if (fileName.toLowerCase().endsWith(acceptableExtensions[i].toLowerCase())) {
					logger.warn(`<<< Exiting _isValidExtension(fileName=${fileName},acceptableExtensions=${acceptableExtensions})`, { "event.action": "_isValidExtension", "logrite.fileName": fileName, "logrite.acceptableExtensions": acceptableExtensions });
                    return true;
                }
            }
            return false;
        },

        _createFileUploadStatusElement: function (status) {
            status.overwrite = false;
            status.path = this._selectedPath();
            var fileName = status.fileName;

            if (fileName.indexOf('http://') == 0 || fileName.indexOf('https://') == 0) {
                var segments = fileName.split('?')[0].split('/');
                fileName = segments[segments.length - 1];                
            }

            var path = status.path;
            var cancelUpload = status.data ? function () {
                var xhr = status.data.jqXHR;
                if (xhr && xhr.readyState !== 4) {
                    xhr.abort();
                }
            } : function() {};
            var $status = $element("li").append(
                $element("a", { href: "javascript:void(0);", "class": "fu-fileupload-thumbnail" }).append(
                    $element("div").append(
                        $element("img", { "class": "pt", src: "/Images/dnnanim.gif" })
                    )
                ),
                $element("div", { "class": "fu-fileupload-filename-container" }).append(
                    $element("span", { "class": "fu-fileupload-filename", title: fileName }).text(path + fileName)
                ),
                $element("div", { "class": "fu-fileupload-progressbar-container" }).append(
                    $element("div", { "class": "fu-fileupload-progressbar ui-progressbar" }).append(
                        $element("div", { "class": "ui-progressbar-value" }).width(0)
                    ),
                    $element("a", { href: "javascript:void(0);", "class": "uploading" }).on("click", cancelUpload)
                )
            )
            .data("status", status);
			logger.warn(`<<< Exiting _createFileUploadStatusElement(status=${status})`, { "event.action": "_createFileUploadStatusElement", "logrite.status": status });
            return $status;
        },

        _showFileUploadStatus: function ($fileUploadStatus, state) {
		logger.error(`>>> Entering _showFileUploadStatus($fileUploadStatus=${$fileUploadStatus},state=${state})`, { "event.action": "_showFileUploadStatus", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.state": state });

            $fileUploadStatus.find(".fu-fileupload-filename-container .fu-file-already-exists-prompt").remove();

            var $prompt = $element("div", { "class": "fu-file-already-exists-prompt" });
            var $statusMessage = $element("span", { "class": "fu-status-message" });
            $prompt.append($statusMessage);
            $fileUploadStatus.find(".fu-fileupload-filename-container").append($prompt);

            var message;
            // file already exists scenario: show Keep/Replace prompt
            if (state.alreadyExists) {

                this._showThumbnail($fileUploadStatus, state);
                var self = this;
                var $keepButton = $element("a", { href: "javascript:void(0);", "class": "fu-file-already-exists-prompt-button-keep" })
                    .text(this.options.resources.keepButtonText)
                    .click(function () {
                        $fileUploadStatus.removeClass().addClass(self.options.statusCancelledCss);
                        self._showFileUploadStatus($fileUploadStatus, { message: self.options.resources.uploadStopped });
                    });

                var $replaceButton = $element("a", { href: "javascript:void(0);", "class": "fu-file-already-exists-prompt-button-replace" })
                    .text(this.options.resources.replaceButtonText)
                    .on('click', function () {
                        var status = $fileUploadStatus.data("status");
                        status.overwrite = true;
						logger.debug(`_showFileUploadStatus($fileUploadStatus=${$fileUploadStatus},state=${state}): status.overwrite → ${status.overwrite}`, { "event.action": "_showFileUploadStatus", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.state": state });
                        self._submitResource(status);
                        $prompt.remove();
                    });

                $prompt.append($keepButton, $replaceButton);
                message = this.options.resources.fileAlreadyExists;
            }
            else {
                if (state.message[0] == '{') {

                } else {
                    message = state.message;
                }
            }
            $statusMessage.text(message);
			logger.debug(`_showFileUploadStatus($fileUploadStatus=${$fileUploadStatus},state=${state}): message → ${message}`, { "event.action": "_showFileUploadStatus", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.state": state });

            $fileUploadStatus.find(".fu-fileupload-progressbar-container").attr("title", message);
			logger.warn(`<<< Exiting _showFileUploadStatus($fileUploadStatus=${$fileUploadStatus},state=${state})`, { "event.action": "_showFileUploadStatus", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.state": state });
        },

        _showPrompt: function(prompt) {
            prompt = JSON.parse(prompt);
            if (prompt.invalidFiles) {
                var title = this.options.resources.unzipFilePromptTitle;
                var body = prompt.invalidFiles.length > 0 ?
                            this.options.resources.unzipFileFailedPromptBody
                            : this.options.resources.unzipFileSuccessPromptBody;
                body = body.replace('[COUNT]', prompt.invalidFiles.length)
                            .replace('[TOTAL]', prompt.totalCount)
                            .replace('[TOTAL]', prompt.totalCount) //replace twice
                            .replace('[FILELIST]', this._generateFileList(prompt.invalidFiles));
                $.dnnAlert({
                    title: title,
                    text: body,
                    maxHeight: 400
                });
            }
        },

        _generateFileList: function(files) {
			logger.error(`>>> Entering _generateFileList(files=${files})`, { "event.action": "_generateFileList", "logrite.files": files });
            var list = '<ul>';
            for (var i = 0; i < files.length; i++) {
                list += '<li>' + files[i] + '</li>';
            }
            list += '</ul>';
			logger.error(`_generateFileList(files=${files}): list → ${list}`, { "event.action": "_generateFileList", "logrite.files": files });
			logger.warn(`<<< Exiting _generateFileList(files=${files})`, { "event.action": "_generateFileList", "logrite.files": files });
            return list;
        },

        _add: function (e, data) {
            if (!this._$fileUploadStatusesContainer.is(':visible')) {
                this._$fileUploadStatusesContainer.show().jScrollbar("update");
                this.$element.trigger("show-statuses-container");
            }

            var count = data.originalFiles.length;
            if (!(typeof this.options.maxFiles === "undefined" || this.options.maxFiles === 0) && this.options.maxFiles < count) {
                if (data.originalFiles[0] === data.files[0]) {
                    alert(this.options.resources.tooManyFiles);
                }
                return;
            }

            if (data.originalFiles.slice(-1)[0] === data.files[0]) {
                // last file in the list
                for (var i = 0; i < count; i++) {
                    if (!this._isValidExtension(data.originalFiles[i].name, this.options.extensions)) {
                        $.dnnAlert({ title: this.options.resources.errorDialogTitle || "Error", text: this.options.resources.invalidFileExtensions });
                        break;
                    }
                }
            }

            if (!this._isValidExtension(data.files[0].name, this.options.extensions)) {
                return;
            }

            var message;

            // Empty file upload does not be supported in IE10
            this._enableBrowserDetection();
            if (data.files[0].size == 0 && $.browser.msie && $.browser.version == "10.0") {
                message = this.options.resources.emptyFileUpload;
            }

            if (typeof message === "undefined" && this.options.maxFileSize && data.files[0].size > this.options.maxFileSize) {
                message = this.options.resources.fileIsTooLarge;
            }

            var statusData = { fileName: data.files[0].name, data: data };
            var $fileUploadStatus = this._getInitializedStatusElement(statusData);

            if (message) {
                this._showError($fileUploadStatus, message);
                return;
            }

            setTimeout(function () { data.submit(); }, 25);
			logger.warn(`<<< Exiting _add(e=${e},data=${data})`, { "event.action": "_add", "logrite.e": e, "logrite.data": data });
        },

        _enableBrowserDetection: function () {
			logger.error('>>> Entering _enableBrowserDetection()', { "event.action": "_enableBrowserDetection" });
            (function ($) {
                if (typeof $.uaMatch === "undefined") {
                    $.uaMatch = function (ua) {
						logger.error(`>>> Entering uaMatch(ua=${ua})`, { "event.action": "uaMatch", "logrite.ua": ua });
                        ua = ua.toLowerCase();

                        var match = /(chrome)[ \/]([\w.]+)/.exec(ua) ||
                            /(webkit)[ \/]([\w.]+)/.exec(ua) ||
                            /(opera)(?:.*version|)[ \/]([\w.]+)/.exec(ua) ||
                            /(msie) ([\w.]+)/.exec(ua) ||
                            ua.indexOf("compatible") < 0 && /(mozilla)(?:.*? rv:([\w.]+)|)/.exec(ua) ||
                            [];
							logger.error(`uaMatch(ua=${ua}): ua → ${ua}`, { "event.action": "uaMatch", "logrite.ua": ua });

						logger.warn(`<<< Exiting uaMatch(ua=${ua})`, { "event.action": "uaMatch", "logrite.ua": ua });
                        return {
                            browser: match[1] || "",
                            version: match[2] || "0"
                        };
                    };
                }
                
                // Don't clobber any existing jQuery.browser in case it's different
                if (typeof $.browser === "undefined") {
                    var matched = jQuery.uaMatch(navigator.userAgent);
                    var browser = {};
					logger.debug(`_enableBrowserDetection(): browser.version → ${browser.version}`, { "event.action": "_enableBrowserDetection" });
					logger.debug(`_enableBrowserDetection(): browser.webkit → ${browser.webkit}`, { "event.action": "_enableBrowserDetection" });
					logger.debug(`_enableBrowserDetection(): browser.safari → ${browser.safari}`, { "event.action": "_enableBrowserDetection" });

                    if (matched.browser) {
                        browser[matched.browser] = true;
                        browser.version = matched.version;
                    }

                    // Chrome is Webkit, but Webkit is also Safari.
                    if (browser.chrome) {
                        browser.webkit = true;
                    } else if (browser.webkit) {
                        browser.safari = true;
                    }

                    $.browser = browser;
                }
            })(jQuery);
			logger.warn('<<< Exiting _enableBrowserDetection()', { "event.action": "_enableBrowserDetection" });
        },

        _getInitializedStatusElement: function(data) {
            var $fileUploadStatus = this._getFileUploadStatusElement(data.fileName);
            if (!$fileUploadStatus.length) {
                $fileUploadStatus = this._createFileUploadStatusElement(data);
                this._$fileUploadStatuses.prepend($fileUploadStatus);
                this._$fileUploadStatusesContainer.show().jScrollbar("update");
            }
            this._setProgress($fileUploadStatus, 0);
			logger.warn(`<<< Exiting _getInitializedStatusElement(data=${data})`, { "event.action": "_getInitializedStatusElement", "logrite.data": data });
            return $fileUploadStatus;
        },

        _submitResource: function(status) {
			logger.error(`>>> Entering _submitResource(status=${status})`, { "event.action": "_submitResource", "logrite.status": status });
            this._submitFile(status);
			logger.warn(`<<< Exiting _submitResource(status=${status})`, { "event.action": "_submitResource", "logrite.status": status });
        },

        _submitFile: function(status) {
			logger.error(`>>> Entering _submitFile(status=${status})`, { "event.action": "_submitFile", "logrite.status": status });
            var data = status.data;
            data.uploadedBytes = 0;
			logger.debug(`_submitFile(status=${status}): data.uploadedBytes → ${data.uploadedBytes}`, { "event.action": "_submitFile", "logrite.status": status });
			logger.error(`_submitFile(status=${status}): data → ${data}`, { "event.action": "_submitFile", "logrite.status": status });
            data.data = null;
			logger.debug(`_submitFile(status=${status}): data.data → ${data.data}`, { "event.action": "_submitFile", "logrite.status": status });
            data.submit();
        },

        _submit: function (e, data) {
			logger.error(`>>> Entering _submit(e=${e},data=${data})`, { "event.action": "_submit", "logrite.e": e, "logrite.data": data });
            var statusData = this._getFileUploadStatusElement(data.files[0].name).data("status");
            data.formData = {
                folder: this._selectedPath(),
                filter: this.options.extensions.join(","),
                extract: this._extract(),
                overwrite: statusData.overwrite,
                validationCode: this.options.validationCode
            };
            $.extend(data.formData, this.options.parameters);
			logger.error(`_submit(e=${e},data=${data}): statusData → ${statusData}`, { "event.action": "_submit", "logrite.e": e, "logrite.data": data });
			logger.warn(`<<< Exiting _submit(e=${e},data=${data})`, { "event.action": "_submit", "logrite.e": e, "logrite.data": data });
            return true;
        },

        _progress: function(e, data) {
			logger.error(`>>> Entering _progress(e=${e},data=${data})`, { "event.action": "_progress", "logrite.e": e, "logrite.data": data });
            var $fileUploadStatus = this._getFileUploadStatusElement(data.files[0].name);
            if (data.formData.extract) {
                if ($fileUploadStatus.find('.fu-dialog-fileupload-extracting').length === 0) {
                    $fileUploadStatus.find('.fu-fileupload-filename').append(
                        $element("span", { "class": "fu-dialog-fileupload-extracting" }).text(" - " + this.options.resources.decompressingFile));
                }
                return;
            }
            var progress = parseInt(data.loaded / data.total * 100, 10);
			logger.error(`_progress(e=${e},data=${data}): progress → ${progress}`, { "event.action": "_progress", "logrite.e": e, "logrite.data": data });
            if (progress < 100) {
                this._setProgress($fileUploadStatus, progress);
				logger.warn(`<<< Exiting _progress(e=${e},data=${data})`, { "event.action": "_progress", "logrite.e": e, "logrite.data": data });
            }
        },

        _done: function (e, data) {
			logger.error(`>>> Entering _done(e=${e},data=${data})`, { "event.action": "_done", "logrite.e": e, "logrite.data": data });
            this._processResponse(data.files[0].name, data.result);
            
            this.$element.trigger($.Event("onfileuploadcomplete"), [data.result]);
			logger.warn(`<<< Exiting _done(e=${e},data=${data})`, { "event.action": "_done", "logrite.e": e, "logrite.data": data });
        },

        _processResponse: function (fileName, response) {
            var $fileUploadStatus = this._getFileUploadStatusElement(fileName);
            var result = this._getFileUploadResult(response);
            if (result.message) {
                if (result.alreadyExists) {
                    this._showFileUploadStatus($fileUploadStatus, result);
                }
                else {
                    this._showError($fileUploadStatus, result.message);
                }
                return;
            }

            if (result.prompt) {
                this._showPrompt(result.prompt);
            }
            this._showThumbnail($fileUploadStatus, result);
            $fileUploadStatus.data("status").overwrite = false;
            this._setProgress($fileUploadStatus, 100);
            this._showFileUploadStatus($fileUploadStatus, { message: this.options.resources.fileUploaded });
            $fileUploadStatus.removeClass().addClass(this.options.statusUploadedCss);
			logger.warn(`<<< Exiting _processResponse(fileName=${fileName},response=${response})`, { "event.action": "_processResponse", "logrite.fileName": fileName, "logrite.response": response });
        },

        _showThumbnail: function ($fileUploadStatus, result) {
			logger.error(`>>> Entering _showThumbnail($fileUploadStatus=${$fileUploadStatus},result=${result})`, { "event.action": "_showThumbnail", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.result": result });
            var $img = $($fileUploadStatus[0].firstChild.firstChild.firstChild);
            $img.removeClass().addClass(result.orientation === 1 ? "pt" : "ls");
            var path = result.path;
            if (this._isValidExtension(result.fileName, [".bmp", ".gif", ".png", ".jpg", ".jpeg"])) {
                if (result.fileId) {
                    var thumbnailImage = dnn.getVar("sf_siteRoot", "/") + 'DnnImageHandler.ashx?mode=securefile&fileId=' + result.fileId + '&MaxWidth=74&MaxHeight=42';
					logger.error(`_showThumbnail($fileUploadStatus=${$fileUploadStatus},result=${result}): thumbnailImage → ${thumbnailImage}`, { "event.action": "_showThumbnail", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.result": result });
                    path = thumbnailImage;
                }
                $img.prop("src", path);
            }
            else {
                $img.prop("src", result.fileIconUrl);
            }
            var $link = $($fileUploadStatus[0].firstChild);
            path ? $link.attr({ target: path, href: path }).removeClass("fu-fileupload-thumbnail-inactive") :
                $link.attr("href", "javascript:void(0);").removeAttr("target").addClass("fu-fileupload-thumbnail-inactive");
				logger.warn(`<<< Exiting _showThumbnail($fileUploadStatus=${$fileUploadStatus},result=${result})`, { "event.action": "_showThumbnail", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.result": result });
        },

        _showError: function ($fileUploadStatus, errorMessage) {
			logger.error(`>>> Entering _showError($fileUploadStatus=${$fileUploadStatus},errorMessage=${errorMessage})`, { "event.action": "_showError", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.errorMessage": errorMessage });
            this._showFileUploadStatus($fileUploadStatus, { message: errorMessage });          
            $fileUploadStatus.removeClass().addClass(this.options.statusErrorCss);
            var $img = $($fileUploadStatus[0].firstChild.firstChild.firstChild);
            $img.removeClass().addClass("pt");
            $img.prop("src", "/Images/no-content.png");
            var $link = $($fileUploadStatus[0].firstChild);
            $link.attr("href", "javascript:void(0);").removeAttr("target").addClass("fu-fileupload-thumbnail-inactive");
			logger.warn(`<<< Exiting _showError($fileUploadStatus=${$fileUploadStatus},errorMessage=${errorMessage})`, { "event.action": "_showError", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.errorMessage": errorMessage });
        },

        _fail: function (e, data) {
			logger.error(`>>> Entering _fail(e=${e},data=${data})`, { "event.action": "_fail", "logrite.e": e, "logrite.data": data });
            var $fileUploadStatus = this._getFileUploadStatusElement(data.files[0].name);
            var message;
            if (data.errorThrown === "abort") {
                message = this.options.resources.fileUploadCancelled;
				logger.warn(`<<< Exiting _fail(e=${e},data=${data})`, { "event.action": "_fail", "logrite.e": e, "logrite.data": data });
            }
            else if (data.errorThrown === "Unauthorized") {
                message = "Unauthorized (401)";
            }
            else {
                message = this.options.resources.fileUploadFailed;
            }
            this._showError($fileUploadStatus, message);
        },

        _dragover: function (e, data) {
			logger.error(`>>> Entering _dragover(e=${e},data=${data})`, { "event.action": "_dragover", "logrite.e": e, "logrite.data": data });
            this._$dragAndDropArea.addClass("dragover");
			logger.warn(`<<< Exiting _dragover(e=${e},data=${data})`, { "event.action": "_dragover", "logrite.e": e, "logrite.data": data });
        },

        _drop: function (e, data) {
			logger.error(`>>> Entering _drop(e=${e},data=${data})`, { "event.action": "_drop", "logrite.e": e, "logrite.data": data });
            this._$dragAndDropArea.removeClass("dragover");
			logger.warn(`<<< Exiting _drop(e=${e},data=${data})`, { "event.action": "_drop", "logrite.e": e, "logrite.data": data });
        },

        _initFileUploadFromLocal: function () {
			logger.error('>>> Entering _initFileUploadFromLocal()', { "event.action": "_initFileUploadFromLocal" });
            this._$inputFileControl.fileupload({
                url: this._uploadFromLocalUrl(),
                beforeSend: $.dnnSF(this.options.moduleId).setModuleHeaders,
                dropZone: this._$dragAndDropArea,
                pasteZone: null,
                sequentialUpload: true,
                progressInterval: 20,
                autoUpload: false
            })
            .on("fileuploadadd", $.proxy(this._add, this))
            .on("fileuploadsubmit", $.proxy(this._submit, this))
            .on("fileuploadprogress", $.proxy(this._progress, this))
            .on("fileuploaddone", $.proxy(this._done, this))
            .on("fileuploadfail", $.proxy(this._fail, this))
            .on("fileuploaddragover", $.proxy(this._dragover, this))
            .on("fileuploaddrop", $.proxy(this._drop, this));
			logger.warn('<<< Exiting _initFileUploadFromLocal()', { "event.action": "_initFileUploadFromLocal" });
        },
    
        _extract: function() {
			logger.error('>>> Entering _extract()', { "event.action": "_extract" });
			logger.warn('<<< Exiting _extract()', { "event.action": "_extract" });
            return this._$extract.is(':checked');
        },

        _selectedPath: function () {
			logger.error('>>> Entering _selectedPath()', { "event.action": "_selectedPath" });
            var selectedPathArray = this._folderPicker.selectedPath();
            if (selectedPathArray.length === 0 && this.options.folderPath) {
				logger.error(`_selectedPath(): selectedPathArray → ${selectedPathArray}`, { "event.action": "_selectedPath" });
                return this.options.folderPath;
            }
            var selectedPath = "";
			logger.error(`_selectedPath(): selectedPath → ${selectedPath}`, { "event.action": "_selectedPath" });
            if (selectedPathArray.length > 1) {
                for (var i = 1, size = selectedPathArray.length; i < size; i++) {
                    selectedPath += selectedPathArray[i].name + "/";
					logger.warn('<<< Exiting _selectedPath()', { "event.action": "_selectedPath" });
                }
            } 
            return selectedPath;
        },

        _getFileUploadResult: function(response) {
			logger.error(`>>> Entering _getFileUploadResult(response=${response})`, { "event.action": "_getFileUploadResult", "logrite.response": response });
            var result;
            try {
                if (!suportAjaxUpload()) {
                    result = JSON.parse($("pre", response).html());
                }
                else {
                    result = JSON.parse(response);
                }
            }
            catch (e) {
				logger.warn(`<<< Exiting _getFileUploadResult(response=${response})`, { "event.action": "_getFileUploadResult", "logrite.response": response });
				logger.error(`Exception in _getFileUploadResult(response=${response}): ${e.message}`, e, { "event.action": "_getFileUploadResult", "event.category": "process", "logrite.response": response });
                return null;
            }
            return result;
        },

        _getFileUploadStatusElement: function (fileName) {
			logger.error(`>>> Entering _getFileUploadStatusElement(fileName=${fileName})`, { "event.action": "_getFileUploadStatusElement", "logrite.fileName": fileName });
            var path = this._selectedPath();
			logger.error(`_getFileUploadStatusElement(fileName=${fileName}): path → ${path}`, { "event.action": "_getFileUploadStatusElement", "logrite.fileName": fileName });
            return this._$fileUploadStatuses.children().filter(function(index) {
                return $(this).data("status").fileName == fileName && $(this).data("status").path == path;
            });
        },

        _setProgress: function ($fileUploadStatus, percent) {
		logger.error(`>>> Entering _setProgress($fileUploadStatus=${$fileUploadStatus},percent=${percent})`, { "event.action": "_setProgress", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.percent": percent });

            $fileUploadStatus.find(".fu-fileupload-progressbar > div").css('width', percent + '%');

            if (percent === 0) {
                $fileUploadStatus.find('.fu-fileupload-progressbar').addClass('indeterminate-progress');
            }

            if (percent === 100) {
                $fileUploadStatus.find('.fu-fileupload-progressbar').removeClass('indeterminate-progress');
                $fileUploadStatus.find('.fu-dialog-fileupload-extracting').remove();
				logger.warn(`<<< Exiting _setProgress($fileUploadStatus=${$fileUploadStatus},percent=${percent})`, { "event.action": "_setProgress", "logrite.$fileUploadStatus": $fileUploadStatus, "logrite.percent": percent });
            }
        },

        _initPanel: function () {
			logger.error('>>> Entering _initPanel()', { "event.action": "_initPanel" });
            this.$element = this._createLayout();

            if (this._isValidExtension(".zip", this.options.extensions)) {
                this._$decompressOption.show();
            }
            else {
                this._$decompressOption.hide();
            }

            var isMultiple = typeof this.options.maxFiles === "undefined" || this.options.maxFiles === 0 || this.options.maxFiles > 1;
			logger.error(`_initPanel(): isMultiple → ${isMultiple}`, { "event.action": "_initPanel" });

            this._$buttonGroup = this.$element.find(".fu-dialog-content-header ul.dnnButtonGroup");
            this._$fileUploadStatusesContainer = this.$element.find('.fu-fileupload-statuses-container').hide().jScrollbar();
            this._$fileUploadStatuses = this._$fileUploadStatusesContainer.find('.fu-fileupload-statuses').empty();
            this._$dragAndDropArea = this.$element.find('.fu-dialog-drag-and-drop-area');
            this._$inputFileControl = $element("input", { type: 'file', name: 'postfile', multiple: isMultiple, "data-text": this.options.resources.dragAndDropAreaTitle });
            this._$inputFileControl.attr('aria-label', 'File');
            this._$extract = this.$element.find("." + "fu-dialog-content-header").find("input");

            this._$inputFileControl.appendTo(this._$dragAndDropArea.find('.fu-dialog-drag-and-drop-area-message')).dnnFileInput(
                {
                    buttonClass: 'normalClass',
                    showSelectedFileNameAsButtonText: false
                });

            this._initFileUploadFromLocal();
			logger.warn('<<< Exiting _initPanel()', { "event.action": "_initPanel" });
        },

        _uploadFromLocalUrl: function() {
			logger.error('>>> Entering _uploadFromLocalUrl()', { "event.action": "_uploadFromLocalUrl" });
            var serviceUrl = $.dnnSF(this.options.moduleId).getServiceRoot(this.options.serviceRoot);
            var url = serviceUrl + this.options.fileUploadMethod;
			logger.error(`_uploadFromLocalUrl(): serviceUrl → ${serviceUrl}`, { "event.action": "_uploadFromLocalUrl" });
            var webResource = new dnn.WebResourceUrl(url);
			logger.error(`_uploadFromLocalUrl(): webResource → ${webResource}`, { "event.action": "_uploadFromLocalUrl" });
            if (!suportAjaxUpload()) {
                var antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
                webResource.parameters().set("__RequestVerificationToken", antiForgeryToken);
            }
            return webResource.toPathAndQuery();
        }

    };

    FileUploadPanel._defaults = {
        statusErrorCss: "fu-status-error",
        statusUploadedCss: "fu-status-uploaded",
        statusCancelledCss: "fu-status-cancelled",
        serviceRoot: "InternalServices",
        fileUploadMethod: "FileUpload/UploadFromLocal",
        maxFiles: 1,
        extensions: [".png", ".jpeg", ".jpg", ".bmp"]
    };

    FileUploadPanel.defaults = function (settings) {
		logger.error(`>>> Entering defaults(settings=${settings})`, { "event.action": "defaults", "logrite.settings": settings });
        if (typeof settings !== "undefined") {
            $.extend(FileUploadPanel._defaults, settings);
        }
		logger.warn(`<<< Exiting defaults(settings=${settings})`, { "event.action": "defaults", "logrite.settings": settings });
        return FileUploadPanel._defaults;
    };

    //var FileUploadDialog = this.FileUploadDialog = dnn.singletonify(FileUpload);

    var FileUploadDialog = this.FileUploadDialog = function (options) {
		logger.error(`>>> Entering FileUploadDialog(options=${options})`, { "event.action": "FileUploadDialog", "logrite.options": options });
        this.options = options;
        this.init();
		logger.debug(`FileUploadDialog(options=${options}): this.options → ${this.options}`, { "event.action": "FileUploadDialog", "logrite.options": options });
    };

    var fuDialogWidth;
    var setFileUploadDialogWidth = function () {
		logger.error('>>> Entering setFileUploadDialogWidth()', { "event.action": "setFileUploadDialogWidth" });
        var fuScreenWidth = $(window).width();
        switch (true) {
            case (fuScreenWidth < 400):
                fuDialogWidth = 350;
				logger.error(`setFileUploadDialogWidth(): fuScreenWidth → ${fuScreenWidth}`, { "event.action": "setFileUploadDialogWidth" });
                break;
            case (fuScreenWidth < 480):
                fuDialogWidth = 400;
                break;
            case (fuScreenWidth < 600):
                fuDialogWidth = 500;
                break;
            case (fuScreenWidth < 800):
                fuDialogWidth = 780;
                break;
            default:
                fuDialogWidth = 780;
				logger.warn('<<< Exiting setFileUploadDialogWidth()', { "event.action": "setFileUploadDialogWidth" });
                break;
        }
    };

    $(window).resize(function () {
        setFileUploadDialogWidth();
    });
    setFileUploadDialogWidth();
    
    FileUploadDialog.prototype = {
        constructor: FileUploadDialog,

        init: function () {
            this.options = $.extend({}, FileUploadDialog.defaults(), this.options);
            this.$this = $(this);
            if (this.options.showOnStartup) {
                this.show();
            }
        },

        _onCloseDialog: function () {
			logger.error('>>> Entering _onCloseDialog()', { "event.action": "_onCloseDialog" });
            this._isShown = false;
            this.$this.trigger($.Event("onfileuploadclose"), [this]);
			logger.debug(`_onCloseDialog(): this._isShown → ${this._isShown}`, { "event.action": "_onCloseDialog" });
        },

        show: function(options) {
			logger.error(`>>> Entering show(options=${options})`, { "event.action": "show", "logrite.options": options });
            if (this._isShown) {
                return;
            }
            this._isShown = true;

			logger.debug(`show(options=${options}): this._isShown → ${this._isShown}`, { "event.action": "show", "logrite.options": options });
            this.options = $.extend(this.options, options);

			logger.debug(`show(options=${options}): options → ${options}`, { "event.action": "show", "logrite.options": options });
            this._panel = new FileUploadPanel(this.options);

            var self = this;
            var $panel = this._panel.$element;
            $panel.dialog({
                modal: true,
                autoOpen: true,
                classes: {
                    "ui-dialog": "dnnFormPopup " + this.options.dialogCss
                },
                title: this.options.resources.title,
                resizable: false,
                width: fuDialogWidth,
                height: this.options.height,
                close: $.proxy(function() {
                    $panel.empty().remove();
                    self._onCloseDialog();
					logger.warn(`<<< Exiting show(options=${options})`, { "event.action": "show", "logrite.options": options });
                }, this),
                buttons: [{
                        text: this.options.resources.closeButtonText,
                        click: function() {
                            logger.error('>>> Entering click()', { "event.action": "click" });
                            logger.warn('<<< Exiting click()', { "event.action": "click" });
                            $(this).dialog("close");
                        },
                        "class": "dnnSecondaryAction"
                    }
                ]
            });
        }
    };

    FileUploadDialog._defaults = {
        dialogCss: "fu-dialog",
        width: fuDialogWidth,
        height: 630
    };

    FileUploadDialog.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(FileUploadDialog._defaults, settings);
        }
        return FileUploadDialog._defaults;
    };

}).apply(dnn, [jQuery, window, document]);


dnn.createFileUpload = function (options) {
	logger.error(`>>> Entering createFileUpload(options=${options})`, { "event.action": "createFileUpload", "logrite.options": options });
    $(document).ready(function () {
        var instance;
        if (options.parentClientId) {
            instance = new dnn.FileUploadPanel(options);
            var $parent = $("#" + options.parentClientId);
            if ($parent.length !== 0) {
                $parent.append(instance.$element);
            }
        }
        else {
            instance = new dnn.FileUploadDialog(options);
        }
        if (options.clientId) {
			logger.debug(`createFileUpload(options=${options}): instance → ${instance}`, { "event.action": "createFileUpload", "logrite.options": options });
            dnn[options.clientId] = instance;
			logger.warn(`<<< Exiting createFileUpload(options=${options})`, { "event.action": "createFileUpload", "logrite.options": options });
        }
    });
};

