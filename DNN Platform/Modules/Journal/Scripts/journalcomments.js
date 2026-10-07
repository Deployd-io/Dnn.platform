var logger = (typeof logger !== 'undefined' && logger) || console;
(function ($, window) {
    $.fn.journalComments = function (options) {
		logger.error(`>>> Entering journalComments(options=${options})`, { "event.action": "journalComments", "logrite.options": options });
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
			logger.error(`journalComments(options=${options}): opts → ${opts}`, { "event.action": "journalComments", "logrite.options": options });

        $placeHolder.click(function () {
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

        $textarea.on('paste', function (e) {
            setTimeout(function () {
                $textarea.val($textarea.val());
                if ($textarea.val().length > $maxLength) {
                    var txt = $textarea.val().substring(0, $maxLength);
                    $textarea.val(txt);
                }
            }, 100);
            isDirtyHandler(e);

        });
        function deleteComment() {
			logger.error('>>> Entering deleteComment()', { "event.action": "deleteComment" });
            var data = {};
            data.JournalId = $id.replace('jcmt-', '');
			logger.error(`deleteComment(): data → ${data}`, { "event.action": "deleteComment" });
            data.CommentId = $(this).parent().attr('id').replace('cmt-', '');
            Post('CommentDelete', data);
            $(this).parent().fadeOut(function () {
                var p = $(this).parent();
                var id = $(this).attr('id');
                $(this).animate({
                    height: '0'
                }, 400, function () {
                    p.remove('#' + id);
                });

            });
			logger.warn('<<< Exiting deleteComment()', { "event.action": "deleteComment" });
        }
        var isDirtyHandler = function (event) {
			logger.error(`>>> Entering isDirtyHandler(event=${event})`, { "event.action": "isDirtyHandler", "logrite.event": event });
            $button.removeClass('disabled');
            $textarea.off('keypress', isDirtyHandler);
			logger.warn(`<<< Exiting isDirtyHandler(event=${event})`, { "event.action": "isDirtyHandler", "logrite.event": event });
        };

        $button.click(function (event) {
            event.preventDefault();
            var jid = $id.replace('jcmt-', '');
            var data = {};
            data.JournalId = jid;
			logger.debug(`journalComments(options=${options}): data.JournalId → ${data.JournalId}`, { "event.action": "journalComments", "logrite.options": options });
            data.Comment = encodeURIComponent($textarea.val());
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
        function commentComplete(data, journalId) {
			logger.error(`>>> Entering commentComplete(data=${data},journalId=${journalId})`, { "event.action": "commentComplete", "logrite.data": data, "logrite.journalId": journalId });
            $textarea.animate({
                height: '0'
            }, 400, function () {
                $button.addClass('disabled').hide();
                $textarea.val('').hide();
                $placeHolder.show();

            });
            var li = $(data);
			logger.error(`commentComplete(data=${data},journalId=${journalId}): li → ${li}`, { "event.action": "commentComplete", "logrite.data": data, "logrite.journalId": journalId });
            li.insertBefore('#' + $id + ' .cmteditarea');
            $(li).find('.miniclose').on('click', deleteComment);
            bindConfirm();
			logger.warn(`<<< Exiting commentComplete(data=${data},journalId=${journalId})`, { "event.action": "commentComplete", "logrite.data": data, "logrite.journalId": journalId });
        }
        function Post(method, data, callback, journalId) {
			logger.error(`>>> Entering Post(method=${method},data=${data},callback=${callback},journalId=${journalId})`, { "event.action": "Post", "logrite.method": method, "logrite.data": data, "logrite.callback": callback, "logrite.journalId": journalId });
            var sf = opts.servicesFramework;
            
            $.ajax({
                type: "POST",
                url: sf.getServiceRoot('Journal') + 'Services/' + method,
                beforeSend: sf.setModuleHeaders,
                data: data,
                success: function (data) {
					logger.error(`>>> Entering success(data=${data})`, { "event.action": "success", "logrite.data": data });
                    if (typeof (callback) != "undefined") {
                        callback(data, journalId);

					logger.warn(`<<< Exiting success(data=${data})`, { "event.action": "success", "logrite.data": data });
                    }
                },
                error: function (xhr, status, error) {
					logger.error(`>>> Entering error(xhr=${xhr},status=${status},error=${error})`, { "event.action": "error", "logrite.xhr": xhr, "logrite.status": status, "logrite.error": error });
                    alert(error);
					logger.warn(`<<< Exiting error(xhr=${xhr},status=${status},error=${error})`, { "event.action": "error", "logrite.xhr": xhr, "logrite.status": status, "logrite.error": error });
                }
            });
			logger.warn(`<<< Exiting Post(method=${method},data=${data},callback=${callback},journalId=${journalId})`, { "event.action": "Post", "logrite.method": method, "logrite.data": data, "logrite.callback": callback, "logrite.journalId": journalId });
        };
		logger.warn(`<<< Exiting journalComments(options=${options})`, { "event.action": "journalComments", "logrite.options": options });
    }
} (jQuery, window));