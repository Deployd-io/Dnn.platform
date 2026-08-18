const logger = console;
﻿(function ($, window) {
	logger.debug('>>> Entering function()');
    $.fn.journalComments = function (options) {
        $.fn.journalComments.defaultOptions = {
            maxLength: 2000
        };
        var opts = $.extend({}, $.fn.journalComments.defaultOptions, options),
            $wrap = this,
            $id = $(this).attr('id'),
            $maxLength = opts.maxLength,
            $textarea = $('#' + $id + '-txt'),
            $placeHolder = $('#' + $id + ' .editorPlaceholder'),
            $button = $('#' + $id + ' .cmtbtn a'),
            $delete = $('#' + $id + ' .miniclose');
			logger.debug(`function(): opts → ${opts}`);
			logger.debug(`function(options=${options}): opts → ${opts}`);

        $placeHolder.click(function () {
			logger.debug('>>> Entering click()');
            $placeHolder.hide();
            $button.addClass('disabled').show();
            $textarea.show().animate({
                height: '+=45'
            }, 400, function () {

                $textarea.focus();
                $textarea.on('keypress', isDirtyHandler);
            });
        });
        $delete.on('click', deleteComment);

		logger.debug('<<< Exiting function()');
        $textarea.on('paste', function (e) {
            setTimeout(function () {
                $textarea.val($textarea.val());
                if ($textarea.val().length > $maxLength) {
                    var txt = $textarea.val().substring(0, $maxLength);
                    $textarea.val(txt);
					logger.debug(`setTimeout(txt=${txt}): txt → ${txt}`);
					logger.debug(`<<< Exiting setTimeout(txt=${txt})`);
                }
            }, 100);
            isDirtyHandler(e);

        });
        function deleteComment() {
			logger.debug('>>> Entering deleteComment()');
            var data = {};
            data.JournalId = $id.replace('jcmt-', '');
			logger.debug(`deleteComment(): data → ${data}`);
            data.CommentId = $(this).parent().attr('id').replace('cmt-', '');
            Post('CommentDelete', data);
			logger.debug('<<< Exiting deleteComment()');
            $(this).parent().fadeOut(function () {
				logger.debug(`>>> Entering fadeOut(this=${this})`);
                var p = $(this).parent();
                var id = $(this).attr('id');
                $(this).animate({
                    height: '0'
                }, 400, function () {
                    p.remove('#' + id);
                });
				logger.debug(`fadeOut(this=${this}): p → ${p}`);

            });
			logger.debug(`fadeOut(this=${this}): id → ${id}`);
        }
        var isDirtyHandler = function (event) {
			logger.debug(`>>> Entering isDirtyHandler(event=${event})`);
            $button.removeClass('disabled');
            $textarea.off('keypress', isDirtyHandler);
			logger.debug(`<<< Exiting isDirtyHandler(event=${event})`);
        };

        $button.click(function (event) {
            event.preventDefault();
            var jid = $id.replace('jcmt-', '');
			logger.debug(`click(): jid → ${jid}`);
            var data = {};
			logger.debug(`click(): data → ${data}`);
            data.JournalId = jid;
            data.Comment = encodeURIComponent($textarea.val());
			logger.debug(`click(): data.JournalId → ${data.JournalId}`);
            data.mentions = $textarea.data("mentions");
            var tmpValue = $textarea.val();
            while (/<(?:.|\n)*?>/gm.test(tmpValue)) {
                tmpValue = tmpValue.replace(/<(?:.|\n)*?>/gm, '');
            }
            while (/\s+/g.test(tmpValue)) {
                tmpValue = tmpValue.replace(/\s+/g, '');
            }
            while (/&nbsp;/g.test(tmpValue)) {
                tmpValue = tmpValue.replace(/&nbsp;/g, '');
            }
            if (tmpValue === '') {
                return false;
            }
            if (!data.Comment || data.Comment === '%3Cbr%3E') {
                return false;
            }
            Post('CommentSave', data, commentComplete, jid);
        });
		logger.debug(`click(): tmpValue → ${tmpValue}`);
        function commentComplete(data, journalId) {
            $textarea.animate({
                height: '0'
            }, 400, function () {
                $button.addClass('disabled').hide();
                $textarea.val('').hide();
                $placeHolder.show();

            });
            var li = $(data);
            li.insertBefore('#' + $id + ' .cmteditarea');
            $(li).find('.miniclose').on('click', deleteComment);
            bindConfirm();
			logger.debug(`<<< Exiting commentComplete(data=${data},journalId=${journalId})`);
        }
        function Post(method, data, callback, journalId) {
            var sf = opts.servicesFramework;
            
            $.ajax({
                type: "POST",
                url: sf.getServiceRoot('Journal') + 'Services/' + method,
                beforeSend: sf.setModuleHeaders,
                data: data,
                success: function (data) {
					logger.debug(`>>> Entering success(data=${data})`);
                    if (typeof (callback) != "undefined") {
                        callback(data, journalId);

					logger.debug(`<<< Exiting success(data=${data})`);
                    }
                },
                error: function (xhr, status, error) {
					logger.debug(`>>> Entering error(xhr=${xhr},status=${status},error=${error})`);
                    alert(error);
					logger.debug(`<<< Exiting error(xhr=${xhr},status=${status},error=${error})`);
                }
            });
			logger.debug(`<<< Exiting Post(method=${method},data=${data},callback=${callback},journalId=${journalId})`);
        };
    }
} (jQuery, window));