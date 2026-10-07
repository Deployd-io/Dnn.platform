var logger = (typeof logger !== 'undefined' && logger) || console;
;
var journalItem = {};
journalItem.JournalType = 'status';
journalItem.Title = '';
journalItem.Summary = '';
journalItem.Body = '';
journalItem.ItemData = null;
journalItem.Security = 'E';
var cancelRequest = false;
var photoTool = null;

function attachPhoto(fileId, path, isImage) {
	logger.error(`>>> Entering attachPhoto(fileId=${fileId},path=${path},isImage=${isImage})`, { "event.action": "attachPhoto", "logrite.fileId": fileId, "logrite.path": path, "logrite.isImage": isImage });
    $('#tbar-attach-Area').hide();
    $(".filePreviewArea").append("<div id='attachClose' class='miniclose'></div>");
    $("#attachClose").show().click(function () {
        journalItem.ItemData = null;
        journalItem.JournalType = 'status';
        $('.filePreviewArea').empty();
        $('#tbar-attach-Area').show();
    });
  
    if (photoTool.hasClass('selected') && isImage) {
        journalItem.JournalType = 'photo';
    } else {
        journalItem.JournalType = 'file';
    }
    journalItem.ItemData = {};
    journalItem.ItemData.ImageUrl = path;

    if (isImage) {
        $(".filePreviewArea").show().append($("<img src='" + path + "' />").css("width", "120px").hide().fadeIn());
        journalItem.ItemData.Url = 'fileid=' + fileId;
    } else {
        $(".filePreviewArea").show().append($("<img src='" + path + "' />").hide().fadeIn());
        journalItem.ItemData.Url = 'fileid=' + fileId;
    }
    $("#btnShare").removeClass('disabled');
	logger.warn(`<<< Exiting attachPhoto(fileId=${fileId},path=${path},isImage=${isImage})`, { "event.action": "attachPhoto", "logrite.fileId": fileId, "logrite.path": path, "logrite.isImage": isImage });
    $("#btnShare").off('keypress', 'isDirtyHandler');

}

function bindConfirm() {
	logger.error('>>> Entering bindConfirm()', { "event.action": "bindConfirm" });
    var options = journalOptions;
    $(".journalrow .minidel, .journalrow .miniclose").each(function () {
        if ($(this).data("confirmBinded")) {
            return;
        }

        var $this = $(this);
        var oThis = this;

        $this.data("confirmBinded", true);
        var clickFuncs = [];
        if (typeof $this.attr("onclick") != "undefined" && $this.attr("onclick").length > 0) {
            var clickFunc = $this.attr("onclick").substr(0, $this.attr("onclick").indexOf("("));
            $this.attr("onclick", "");
            clickFuncs.push(eval(clickFunc));
        } else {
            if (typeof $._data($this[0], "events") != "undefined") {
                for (var i = 0; i < $._data($this[0], "events").click.length; i++) {
                    var handler = $._data($this[0], "events").click[i].handler;
                    var functionName = handler.name || handler.toString().match(/^function\s*([^\s(]+)/)[1]; 
                    if (typeof functionName != "undefined" && functionName.length > 0) {
                        clickFuncs.push(handler);
                        break;
                    }
                }
            }
            $this.off("click");
        }

        $this.dnnConfirm({
            text: options.confirmText,
            yesText: options.yesText,
            noText: options.noText,
            title: options.title,
            isButton: true,
            callbackTrue: function () {
				logger.error('>>> Entering callbackTrue()', { "event.action": "callbackTrue" });
                for (var i = 0; i < clickFuncs.length; i++)
                    clickFuncs[i].call(oThis, oThis);
					logger.warn('<<< Exiting callbackTrue()', { "event.action": "callbackTrue" });
            }
        });
    });
	logger.warn('<<< Exiting bindConfirm()', { "event.action": "bindConfirm" });
}

function buildLikes(data, journalId) {
	logger.error(`>>> Entering buildLikes(data=${data},journalId=${journalId})`, { "event.action": "buildLikes", "logrite.data": data, "logrite.journalId": journalId });
    var currLike = $('#like-' + journalId).text();
    if (currLike == resxLike) {
		logger.error(`buildLikes(data=${data},journalId=${journalId}): currLike → ${currLike}`, { "event.action": "buildLikes", "logrite.data": data, "logrite.journalId": journalId });
        $('#like-' + journalId).fadeOut(function () {
            $(this).text(resxUnLike).fadeIn();
        });
    } else {
        $('#like-' + journalId).fadeOut(function () {
            $(this).text(resxLike).fadeIn();
        });
    }
    $('#jid-' + journalId + ' .likes').fadeOut(function () {
        $(this).empty().append(data.LikeList).fadeIn();
    });
	logger.warn(`<<< Exiting buildLikes(data=${data},journalId=${journalId})`, { "event.action": "buildLikes", "logrite.data": data, "logrite.journalId": journalId });
};

function getItems(sf) {
	logger.error(`>>> Entering getItems(sf=${sf})`, { "event.action": "getItems", "logrite.sf": sf });
    var rows = $(".journalrow").get();
        
		logger.error(`getItems(sf=${sf}): rows → ${rows}`, { "event.action": "getItems", "logrite.sf": sf });
    data = {};
    data.ProfileId = pid;
    data.GroupId = gid;
    data.RowIndex = rows.length + 1;
	logger.warn(`<<< Exiting getItems(sf=${sf})`, { "event.action": "getItems", "logrite.sf": sf });
    data.MaxRows = pagesize;

    $.ajax({
        type: "POST",
        url: sf.getServiceRoot('Journal') + 'Services/GetListForProfile',
        beforeSend: sf.setModuleHeaders,
        data: data,
        success: function (data) {
			logger.error(`>>> Entering success(data=${data})`, { "event.action": "success", "logrite.data": data });
            if (data.length > 0) {
                $("#journalItems").append(data);
                pluginInit();
                var newRows = $(".journalrow").get();
				logger.error(`success(data=${data}): newRows → ${newRows}`, { "event.action": "success", "logrite.data": data });
                var diff = (newRows.length - rows.length);
				logger.error(`success(data=${data}): diff → ${diff}`, { "event.action": "success", "logrite.data": data });
                if (diff < pagesize) {
                    $("#getMore").hide();
                }
            }
            if (typeof (callback) != "undefined") {
                callback(data);
				logger.warn(`<<< Exiting success(data=${data})`, { "event.action": "success", "logrite.data": data });
            }
        },
        error: function (xhr, status, error) {
			logger.error(`>>> Entering error(xhr=${xhr},status=${status},error=${error})`, { "event.action": "error", "logrite.xhr": xhr, "logrite.status": status, "logrite.error": error });
            alert(error);
			logger.warn(`<<< Exiting error(xhr=${xhr},status=${status},error=${error})`, { "event.action": "error", "logrite.xhr": xhr, "logrite.status": status, "logrite.error": error });
        }
    });
}

function IsImage(value) {
	logger.error(`>>> Entering IsImage(value=${value})`, { "event.action": "IsImage", "logrite.value": value });
    if (value != null) {
        var imageTypesArray = journalOptions.imageTypes.toLowerCase().split(",");
		logger.error(`IsImage(value=${value}): imageTypesArray → ${imageTypesArray}`, { "event.action": "IsImage", "logrite.value": value });
        return $.inArray(value.toLowerCase(), imageTypesArray) >= 0;
    } else {
		logger.warn(`<<< Exiting IsImage(value=${value})`, { "event.action": "IsImage", "logrite.value": value });
        return false;
    }
}

function journalDelete(obj) {
	logger.error(`>>> Entering journalDelete(obj=${obj})`, { "event.action": "journalDelete", "logrite.obj": obj });
    var p = obj.parentNode;
    var jid = p.id.replace('jid-', '');
	logger.error(`journalDelete(obj=${obj}): jid → ${jid}`, { "event.action": "journalDelete", "logrite.obj": obj });
    var data = {};
	logger.error(`journalDelete(obj=${obj}): data → ${data}`, { "event.action": "journalDelete", "logrite.obj": obj });
    data.JournalId = jid;
	logger.debug(`journalDelete(obj=${obj}): data.JournalId → ${data.JournalId}`, { "event.action": "journalDelete", "logrite.obj": obj });
    journalPost('SoftDelete', data, journalRemove, jid);
	logger.warn(`<<< Exiting journalDelete(obj=${obj})`, { "event.action": "journalDelete", "logrite.obj": obj });
};

function journalPost(method, data, callback, journalId) {
	logger.error(`>>> Entering journalPost(method=${method},data=${data},callback=${callback},journalId=${journalId})`, { "event.action": "journalPost", "logrite.method": method, "logrite.data": data, "logrite.callback": callback, "logrite.journalId": journalId });
    var sf = journalOptions.servicesFramework;
    $.ajax({
        type: "POST",
        url: sf.getServiceRoot('Journal') + "Services/" + method,
        beforeSend: sf.setModuleHeaders,
        data: data,
        success: function (data) {
            if (typeof (callback) != "undefined") {
                callback(data,journalId);
            }
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
	logger.warn(`<<< Exiting journalPost(method=${method},data=${data},callback=${callback},journalId=${journalId})`, { "event.action": "journalPost", "logrite.method": method, "logrite.data": data, "logrite.callback": callback, "logrite.journalId": journalId });
};

function journalRemove(data, jid) {
	logger.error(`>>> Entering journalRemove(data=${data},jid=${jid})`, { "event.action": "journalRemove", "logrite.data": data, "logrite.jid": jid });
    $('#jid-' + jid).slideUp(function () {
        $(this).remove();
    });
	logger.warn(`<<< Exiting journalRemove(data=${data},jid=${jid})`, { "event.action": "journalRemove", "logrite.data": data, "logrite.jid": jid });
};

function pluginInit() {
	logger.error('>>> Entering pluginInit()', { "event.action": "pluginInit" });
    var sf = journalOptions.servicesFramework;
    
    $('.jcmt').each(function () {
        if($(this).data("journalCommentsBinded")) {
            return;
        }
        $(this).data("journalCommentsBinded", true);
            
        $(this).journalComments(commentOpts);
    });
        
    var rows = $(".journalrow");
	logger.error(`pluginInit(): rows → ${rows}`, { "event.action": "pluginInit" });
    if (rows.length == pagesize) {
        $("#getMore").show();
    }
        
    $('a[id^="cmtbtn-"]').each(function () {
        if($(this).data("clickBinded")) {
            return;
        }
        $(this).data("clickBinded", true);

        $(this).click(function(e) {

            e.preventDefault();
            var jid = $(this).attr('id').replace('cmtbtn-', '');
            var cmtarea = $("#jcmt-" + jid + " .cmteditarea");
            var cmtbtn = $("#jcmt-" + jid + " .cmtbtn");
            var cmtbtnlink = $("#jcmt-" + jid + " .cmtbtn a");
            if (cmtarea.css('display') == 'none') {
                cmtarea.show();
                cmtbtnlink.addClass('disabled');
                cmtbtn.show();
                $("#jcmt-" + jid + "-txt").focus();

            } else {

                var cmtedit = $("#jcmt-" + jid + " .cmteditor");
                var plh = $("#jcmt-" + jid + " .editorPlaceholder");
                cmtedit.animate({
                    height: '0'
                }, 400, function() {
                    cmtbtn.hide();
                    cmtbtnlink.addClass('disabled').hide();
                    cmtedit.text('').hide();
                    cmtarea.hide();
                    plh.show();
                });
            }
        });
    });

    $('a[id^="like-"]').each(function () {
        if($(this).data("clickBinded")) {
            return;
        }
        $(this).data("clickBinded", true);

        $(this).click(function(e) {
            e.preventDefault();
            var jid = $(this).attr('id').replace('like-', '');
            var data = { };
            data.JournalId = jid;
			logger.debug(`pluginInit(): data.JournalId → ${data.JournalId}`, { "event.action": "pluginInit" });
            journalPost('Like', data, buildLikes, jid);
        });
    });

    bindConfirm();

    if (!$("#getMore").data("clickBinded")) {
        $("#getMore").click(function(e) {
            getItems(sf);
            e.preventDefault();
        });
        $("#getMore").data("clickBinded", true);
    }
    $('#journalContent, .cmteditor').mentionsInput({servicesFramework: sf});
	logger.warn('<<< Exiting pluginInit()', { "event.action": "pluginInit" });
}

(function ($, window) {
    "use strict";
    function resetJournalItem() {
		logger.error('>>> Entering resetJournalItem()', { "event.action": "resetJournalItem" });
        journalItem = {};
        journalItem.JournalType = 'status';
        journalItem.Title = '';
        journalItem.Summary = '';
        journalItem.Body = '';
        journalItem.ItemData = null;
        journalItem.Security = 'E';
        journalItem.Tags = [];
        journalItem.Users = [];
		logger.warn('<<< Exiting resetJournalItem()', { "event.action": "resetJournalItem" });
    }


    
    $.fn.journalTools = function (options) {
		logger.error(`>>> Entering journalTools(options=${options})`, { "event.action": "journalTools", "logrite.options": options });
        $.fn.journalTools.defaultOptions = {
            placeHolder: '#journalPlaceholder',
            shareButton: '#btnShare',
            closeButton: '#journalClose',
            optionsArea: '#journalOptionArea',
            content: '#journalContent',
            photoTool: '#tbar-photo',
            attachTool: '#tbar-attach',
            permTool: '#tbar-perm',
            securityMenu: '.securityMenu',
            photoText: '#tbar-photoText',
            fileText: '#tbar-fileText',
            maxLength: 250,
            fileManagerSelector: '#userFileManager'
        };
        var opts = $.extend({}, $.fn.journalTools.defaultOptions, options),
            $wrap = this,
            $placeHolder = $wrap.find(opts.placeHolder),
            $shareButton = $wrap.find(opts.shareButton),
            $closeButton = $wrap.find(opts.closeButton),
            $optionsArea = $wrap.find(opts.optionsArea),
            $content = $wrap.find(opts.content),
            $photoTool = $wrap.find(opts.photoTool),
            $attachTool = $wrap.find(opts.attachTool),
            $permTool = $wrap.find(opts.permTool),
            $photoArea = $wrap.find(opts.photoTool + '-Area'),
            $attachArea = $wrap.find(opts.attachTool + '-Area'),
            $photoText = $wrap.find(opts.photoText),
            $fileText = $wrap.find(opts.fileText),
            $contentShow = false,
            $maxLength = opts.maxLength,
            $securityMenu = $wrap.find(opts.securityMenu),
            $userFileManager = $wrap.find(opts.fileManagerSelector);
			logger.error(`journalTools(options=${options}): opts → ${opts}`, { "event.action": "journalTools", "logrite.options": options });
        
        photoTool = $photoTool;

        var isDirtyHandler = function (event) {
			logger.error(`>>> Entering isDirtyHandler(event=${event})`, { "event.action": "isDirtyHandler", "logrite.event": event });
            $shareButton.removeClass('disabled');
            $shareButton.off('keypress', isDirtyHandler);
			logger.warn(`<<< Exiting isDirtyHandler(event=${event})`, { "event.action": "isDirtyHandler", "logrite.event": event });
        };
        $permTool.click(function () {
            $securityMenu.toggle();
        });
        $securityMenu.find('.handle').click(function () {
            $securityMenu.toggle();
        });
        $(opts.securityMenu + ' input').click(function () {
            journalItem.Security = $(this).val();
        });

        $photoTool.click(function () {
            photoToolClick();
        });
        $attachTool.click(function () {
            attachToolClick();
        });

        $placeHolder.click(function () {
            showContent();
        });

        $closeButton.click(function () {
            closeEditor();
        });
        $content.on('keypress', function (event) {
            if (event.keyCode == 8 || event.keyCode == 46) {
                return;
            }
            if ($content.val().length >= $maxLength && $maxLength > 0) {
                return false;
            }
        });
        $content.on('paste', function (e) {
            setTimeout(function () {
                $content.val($content.val());
                if ($content.val().length>=$maxLength && $maxLength > 0) {
                    var txt = $content.val().substring(0,$maxLength);
                    $content.val(txt);
                }
            }, 100);

        });
        function closeEditor() {
			logger.error('>>> Entering closeEditor()', { "event.action": "closeEditor" });
            $closeButton.hide();
            $contentShow = false;
            $securityMenu.hide();
            $shareButton.addClass('disabled').hide();
            $photoArea.hide();
            $attachArea.hide();
            $optionsArea.hide();
            $('#tbar span').removeClass('selected');
            
            var linkArea = $('#linkArea');
			logger.error(`closeEditor(): linkArea → ${linkArea}`, { "event.action": "closeEditor" });
            $('#linkArea #imagePreviewer').hide();
            linkArea.find('#linkInfo b').text('');
            linkArea.find('#linkInfo p').text('');
            $content.data('linkedUp', false);
            linkArea.hide();
            linkArea.data('url','');

            $attachTool.off('click');
            $photoTool.off('click');
            $photoTool.click(function () {
                photoToolClick();
            });
            $attachTool.click(function () {
                attachToolClick();
            });
            $(".filePreviewArea").empty();
            $content.off('keypress', isDirtyHandler);
            $content.animate({
                height: '0'
            }, 400, function () {
                $content.val('').hide();
                $placeHolder.show();
                resetJournalItem();
            });
			logger.warn('<<< Exiting closeEditor()', { "event.action": "closeEditor" });
        }
        var showContent = function () {
			logger.error('>>> Entering showContent()', { "event.action": "showContent" });
            $placeHolder.hide();
            $shareButton.addClass('disabled').show();
            $content.show().animate({
                height: '+=65'
            }, 400, function () {
                $contentShow = true;
                $content.focus();
                $content.on('keypress', isDirtyHandler);
                $closeButton.show();


            });
			logger.warn('<<< Exiting showContent()', { "event.action": "showContent" });
        };
        var photoToolClick = function() {
			logger.error('>>> Entering photoToolClick()', { "event.action": "photoToolClick" });
            $('#tbar-attach-Area').show();
            if ($photoTool.hasClass('selected')) {
                $photoTool.removeClass('selected');
                $optionsArea.hide();
                $attachArea.hide();
                $attachTool.off('click');
                 $('.filePreviewArea').empty();
                resetJournalItem();
                $attachTool.click(function() {
                    attachToolClick();
                });
            } else {
                if ($contentShow == false) {
                    showContent();
                }
                $userFileManager.userFileManager.setFileExtensions(journalOptions.imageTypes);
                $photoTool.addClass('selected');
                $attachTool.off('click');
                $attachTool.on('click', clickDisable);
                $optionsArea.show();
                $attachArea.show();
                $fileText.hide();
                $photoText.show();
				logger.warn('<<< Exiting photoToolClick()', { "event.action": "photoToolClick" });
            }
        };
        var attachToolClick = function () {
			logger.error('>>> Entering attachToolClick()', { "event.action": "attachToolClick" });
            $('#tbar-attach-Area').show();
            if ($attachTool.hasClass('selected')) {
                $attachTool.removeClass('selected');
                $photoTool.off('click');
                $photoTool.click(function () {
                    photoToolClick();
                });
                $attachArea.hide();
                $optionsArea.hide();
                resetJournalItem();
                 journalItem.ItemData = null;
            } else {
                if ($contentShow == false) {
                    showContent();
                };
                $userFileManager.userFileManager.setFileExtensions('');
                $attachTool.addClass('selected');
                $photoTool.off('click');
                $photoTool.on('click', clickDisable);
                $attachArea.show();
                $optionsArea.show();
                $fileText.show();
                $photoText.hide();
				logger.warn('<<< Exiting attachToolClick()', { "event.action": "attachToolClick" });
            }
        };
        var clickDisable = function (e) {
			logger.error(`>>> Entering clickDisable(e=${e})`, { "event.action": "clickDisable", "logrite.e": e });
            e.preventDefault();
            //return false;
			logger.warn(`<<< Exiting clickDisable(e=${e})`, { "event.action": "clickDisable", "logrite.e": e });
        };
        $shareButton.click(function (e) {
            cancelRequest = true;
            e.preventDefault();
            var data = {};
			logger.debug(`journalTools(options=${options}): data.profileId → ${data.profileId}`, { "event.action": "journalTools", "logrite.options": options });
			logger.debug(`journalTools(options=${options}): data.groupId → ${data.groupId}`, { "event.action": "journalTools", "logrite.options": options });
			logger.debug(`journalTools(options=${options}): data.journalType → ${data.journalType}`, { "event.action": "journalTools", "logrite.options": options });
			logger.debug(`journalTools(options=${options}): data.securitySet → ${data.securitySet}`, { "event.action": "journalTools", "logrite.options": options });
			logger.debug(`journalTools(options=${options}): data.itemData → ${data.itemData}`, { "event.action": "journalTools", "logrite.options": options });
            data.text = encodeURIComponent($content.val());
           
            data.profileId = pid;
            data.groupId = gid;

            //for private group, overwrite existing security set so that the post only visible to group members
            if (gid > 0 && !ispublicgroup) {
                journalItem.Security = 'R';
            }

            data.journalType = journalItem.JournalType;
            data.securitySet = journalItem.Security;
            var ItemData = journalItem.ItemData;
            if (ItemData != null) {
                data.itemData = JSON.stringify(ItemData);
            } else {
                data.itemData = '';
            }
            if ((!data.text && !data.itemData) || data.text === '%3Cbr%3E') {
                return false;
            }
            //Check for a value
            var tmpValue = $content.val();
            while (/<(?:.|\n)*?>/gm.test(tmpValue)) {
                tmpValue = tmpValue.replace(/<(?:.|\n)*?>/gm, '');
            }
            while (/\s+/g.test(tmpValue)) {
                tmpValue = tmpValue.replace(/\s+/g, '');
            }
            while (/&nbsp;/g.test(tmpValue)) {
                tmpValue = tmpValue.replace(/&nbsp;/g, '');
            }

            if (tmpValue === '' && data.itemData === '') {
                return false;
            }
            data.mentions = $content.data("mentions");
            $.ajax({
                type: "POST",
                url: opts.servicesFramework.getServiceRoot('Journal') + "Services/Create",
                data: data,
                beforeSend: opts.servicesFramework.setModuleHeaders,
                success: function (data) {

                    var req = {};
					logger.debug(`success(data=${data}): req.GroupId → ${req.GroupId}`, { "event.action": "success", "logrite.data": data });
					logger.debug(`success(data=${data}): req.RowIndex → ${req.RowIndex}`, { "event.action": "success", "logrite.data": data });
					logger.debug(`success(data=${data}): req.MaxRows → ${req.MaxRows}`, { "event.action": "success", "logrite.data": data });
                    req.ProfileId = pid;
					logger.debug(`success(data=${data}): req.ProfileId → ${req.ProfileId}`, { "event.action": "success", "logrite.data": data });
                    req.GroupId = gid;
                    req.RowIndex = 0;
                    req.MaxRows = 1;
                    closeEditor();
                    getNewItem(req, insertNewItem);

                },
                error: function (xhr, status, error) {
                    alert(error);
                }
            });

        });
        function getNewItem(data, callback) {
			logger.error(`>>> Entering getNewItem(data=${data},callback=${callback})`, { "event.action": "getNewItem", "logrite.data": data, "logrite.callback": callback });
            $.ajax({
                type: "POST",
                url: opts.servicesFramework.getServiceRoot('Journal') + "Services/GetListForProfile",
                beforeSend: opts.servicesFramework.setModuleHeaders,
                data: data,
                success: function (data) {

                    if (typeof (callback) != "undefined") {
                        callback(data);
                    }

                },
                error: function (xhr, status, error) {
                    alert(error);
                }
            });
			logger.warn(`<<< Exiting getNewItem(data=${data},callback=${callback})`, { "event.action": "getNewItem", "logrite.data": data, "logrite.callback": callback });
        }
        function insertNewItem(html) {
			logger.error(`>>> Entering insertNewItem(html=${html})`, { "event.action": "insertNewItem", "logrite.html": html });
            $(html).hide().prependTo("#journalItems").fadeIn();
            pluginInit();
			logger.warn(`<<< Exiting insertNewItem(html=${html})`, { "event.action": "insertNewItem", "logrite.html": html });
            //.slideDown();

		logger.warn(`<<< Exiting journalTools(options=${options})`, { "event.action": "journalTools", "logrite.options": options });
        }

    };
    $.fn.previewify = function (options) {
		logger.error(`>>> Entering previewify(options=${options})`, { "event.action": "previewify", "logrite.options": options });
        var images = [];
        var currImage = 0;
        var opts = $.extend({}, $.fn.previewify.defaultOptions, options),
            $wrap = this,
            $textArea = $wrap.find(opts.textareaSelector),
            $clearLink = $wrap.find(opts.clearPreviewSelector),
            $previewArea = $wrap.find(opts.previewSelector);
			logger.error(`previewify(options=${options}): opts → ${opts}`, { "event.action": "previewify", "logrite.options": options });
        // updates the preview area
        function updatePageValues(url) {
		logger.error(`>>> Entering updatePageValues(url=${url})`, { "event.action": "updatePageValues", "logrite.url": url });

            if ($previewArea.data('url') === url) {
                return;
            }
            cancelRequest = false;
            $textArea.data('linkedUp', true);
            $previewArea.data('url', url);
            $clearLink.show();

            var data = { Url: url };
            $.ajax({
                type: "POST",
                url: opts.servicesFramework.getServiceRoot('Journal') + 'Services/PreviewURL',
                beforeSend: opts.servicesFramework.setModuleHeaders,
                data: data,
                success: function (data) {
                    buildLinkPreview(data);
                },
                error: function (xhr, status, error) {
                    alert(error);
                }
            });
			logger.warn(`<<< Exiting updatePageValues(url=${url})`, { "event.action": "updatePageValues", "logrite.url": url });
        }
        function prevImg() {
			logger.error('>>> Entering prevImg()', { "event.action": "prevImg" });
            currImage = currImage - 1;
            if (currImage < 0) {
                currImage = images.length - 1;
            };
            if (currImage > images.length - 1) {
                currImage = 0;
            };
            $(opts.previewSelector + ' #imgCount').text(currImage + 1 + ' of ' + images.length);
            $previewArea.find('#image img').fadeOut(function () {
                $(this).hide().attr('src', '').attr('src', images[currImage].URL).fadeIn();
            });
            journalItem.ItemData.ImageUrl = images[currImage].URL;
			logger.warn('<<< Exiting prevImg()', { "event.action": "prevImg" });
        };
        function nextImg() {
			logger.error('>>> Entering nextImg()', { "event.action": "nextImg" });
            currImage = currImage + 1;
            if (currImage < 0) {
                currImage = images.length - 1;
            };
            if (currImage > images.length - 1) {
                currImage = 0;
            };
            $(opts.previewSelector + ' #imgCount').text(currImage + 1 + ' of ' + images.length);
            $previewArea.find('#image img').fadeOut(function () {
                $(this).hide().attr('src', '').attr('src', images[currImage].URL).fadeIn();
            });
            journalItem.ItemData.ImageUrl = images[currImage].URL;
			logger.warn('<<< Exiting nextImg()', { "event.action": "nextImg" });
        };
        function buildLinkPreview(link) {
			logger.error(`>>> Entering buildLinkPreview(link=${link})`, { "event.action": "buildLinkPreview", "logrite.link": link });
            //$previewArea.empty();
            if (link.URL == null) {
                return;
            }
            if (cancelRequest) {
                removePageValues();
                cancelRequest = false;
                return;
            }
            journalItem.ItemData = {};
            if (link.Images.length > 0) {
                images = link.Images;
                journalItem.ItemData.ImageUrl = link.Images[0].URL;
                $previewArea.find('#image img').attr('src', link.Images[0].URL);
                $(opts.previewSelector + ' #imgPrev').on('click', prevImg);
                $(opts.previewSelector + ' #imgNext').on('click', nextImg);
                $(opts.previewSelector + ' #imgCount').text(currImage + 1 + ' of ' + images.length);
                $(opts.previewSelector + ' #imagePreviewer').show();
            } else {
                $(opts.previewSelector + ' #imagePreviewer').hide();
            }
            if (link.Description == null) {
                link.Description = '';
            };
            link.Description = link.Description.replace('&amp;', '&');
            link.Description = decodeURIComponent(link.Description);
			logger.debug(`buildLinkPreview(link=${link}): link.Description → ${link.Description}`, { "event.action": "buildLinkPreview", "logrite.link": link });
            $previewArea.find('#linkClose').show();
            $previewArea.find('#linkInfo b').text(link.Title);
            $previewArea.find('#linkInfo p').text(link.Description);
            $previewArea.show();
            $previewArea.parent().show();
            journalItem.JournalType = 'link';

            journalItem.ItemData.Title = link.Title;
            journalItem.ItemData.Description = escape(link.Description);
			logger.warn(`<<< Exiting buildLinkPreview(link=${link})`, { "event.action": "buildLinkPreview", "logrite.link": link });
            journalItem.ItemData.URL = link.URL;

        }
        // resets the preview area (but keeps the associated url in a data field for future use)
        function removePageValues() {
			logger.error('>>> Entering removePageValues()', { "event.action": "removePageValues" });
            $(opts.previewSelector + ' #imagePreviewer').hide();
            images = [];
            currImage = 0;
            $previewArea.find('#linkInfo b').text('');
            $previewArea.find('#linkInfo p').text('');
            $textArea.data('linkedUp', false);
            $previewArea.hide();
            $clearLink.hide();
            journalItem.JournalType = 'status';
            journalItem.ItemData = null;
            $previewArea.data('url','');
			logger.warn('<<< Exiting removePageValues()', { "event.action": "removePageValues" });
        }

        // returns true if link already found, false if not
        function isLinkedUp() {
			logger.error('>>> Entering isLinkedUp()', { "event.action": "isLinkedUp" });
			logger.warn('<<< Exiting isLinkedUp()', { "event.action": "isLinkedUp" });
            return $textArea.data('linkedUp');
        }

        // parses the first url out of a block of text
        function findUrl(enteredText) {
			logger.error(`>>> Entering findUrl(enteredText=${enteredText})`, { "event.action": "findUrl", "logrite.enteredText": enteredText });
            if (typeof (enteredText) == 'undefined') {
                enteredText = $textArea.val();
            }
            var replacePattern1, replacePattern2, replacePattern3, url;
			logger.error(`findUrl(enteredText=${enteredText}): enteredText → ${enteredText}`, { "event.action": "findUrl", "logrite.enteredText": enteredText });
            //URLs starting with "www."
            replacePattern1 = /(^|[^\/])(www\.[\S]+(\b|$))/gim;
			logger.error(`findUrl(enteredText=${enteredText}): replacePattern1 → ${replacePattern1}`, { "event.action": "findUrl", "logrite.enteredText": enteredText });
			logger.debug(`findUrl(enteredText=${enteredText}): replacePattern1 → ${replacePattern1}`, { "event.action": "findUrl", "logrite.enteredText": enteredText });
            url = replacePattern1.exec(enteredText);
            if (!url) {
                //URLs starting with http://, https://, or ftp://
                replacePattern2 = /(\b(https?|ftp):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/gim;
                url = replacePattern2.exec(enteredText);
            }
            if (url) {
                url = (url.toString().split(',')[0]);
                updatePageValues(url);
				logger.warn(`<<< Exiting findUrl(enteredText=${enteredText})`, { "event.action": "findUrl", "logrite.enteredText": enteredText });
            }
        }

        // set default state, link not found.
        $textArea.data('linkedUp', false);

        // key event
        $textArea.keyup(function (e) {

            var code = e.keyCode || e.which;
            if (code !== 13 && code !== 32) { // space, enter key
                return;
            }
            if (!isLinkedUp()) {
                findUrl($(this).val());
            }
        });

        // paste event
        $textArea.on('paste', function (e) {

            setTimeout(function () {
                $textArea.val($textArea.val());
                if (!isLinkedUp()) {
                    findUrl();
                }
            }, 100);

        });

        // removes the preview
        $clearLink.click(function (e) {
            e.preventDefault();
            removePageValues();
		logger.warn(`<<< Exiting previewify(options=${options})`, { "event.action": "previewify", "logrite.options": options });
        });

    };

    $.fn.previewify.defaultOptions = {
        textareaSelector: 'textarea',
        previewSelector: 'div',
        clearPreviewSelector: 'a'
    };



} (jQuery, window));
