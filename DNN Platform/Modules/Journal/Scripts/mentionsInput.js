var logger = (typeof logger !== 'undefined' && logger) || console;
(function($) {
	$.fn.mentionsInput = function(options) {
		logger.error(`>>> Entering mentionsInput(options=${options})`, { "event.action": "mentionsInput", "logrite.options": options });
		if (this.length == 0)
			return this;
		options = $.extend({}, $.fn.mentionsInput.defaultOptions, options);
		var serviceFramework = options.servicesFramework;
		logger.error(`mentionsInput(options=${options}): options → ${options}`, { "event.action": "mentionsInput", "logrite.options": options });
	    var getBaseServicePath = function() {
			logger.error('>>> Entering getBaseServicePath()', { "event.action": "getBaseServicePath" });
			logger.warn('<<< Exiting getBaseServicePath()', { "event.action": "getBaseServicePath" });
            return serviceFramework.getServiceRoot('Journal') + 'Services/';
	    };

	    return this.each(function() {
	        var $this = $(this);
	        $this.data("mentions", []);
	        var getPosition = function () {
				logger.error('>>> Entering getPosition()', { "event.action": "getPosition" });
			    var element = $this.get(0);
			    if ('selectionStart' in element) {
					logger.error(`getPosition(): element → ${element}`, { "event.action": "getPosition" });
				    // Standard-compliant browsers
				    return element.selectionStart;
			    } else if (document.selection) {
				    // IE
				    element.focus();
				    var sel = document.selection.createRange();
					logger.error(`getPosition(): sel → ${sel}`, { "event.action": "getPosition" });
				    var selLen = document.selection.createRange().text.length;
					logger.error(`getPosition(): selLen → ${selLen}`, { "event.action": "getPosition" });
				    sel.moveStart('character', -element.value.length);
				    return sel.text.length - selLen;
			    } else {
					logger.warn('<<< Exiting getPosition()', { "event.action": "getPosition" });
				    return -1;
			    }
		    };

		    var getContentByCursor = function() {
				logger.error('>>> Entering getContentByCursor()', { "event.action": "getContentByCursor" });
			    var position = getPosition();
			    if (position > 0) {
				    if ($this.val().length > position) {
					    for (var i = position; i < $this.val().length; i++) {
						    if (/\s/.test($this.val()[i])) {
							    position = i;
							    break;
						    } else if (i == $this.val().length - 1) {
							    position = i + 1;
						    }
					    }
				    }
				    var content = $this.val().substr(0, position);
					logger.error(`getContentByCursor(): content → ${content}`, { "event.action": "getContentByCursor" });
				    var result = new RegExp(options.identityChar + "([\\S]+?)$").exec(content);
					logger.error(`getContentByCursor(): result → ${result}`, { "event.action": "getContentByCursor" });
				    if (result) {
					    return result[1];
				    }
			    }

				logger.warn('<<< Exiting getContentByCursor()', { "event.action": "getContentByCursor" });
			    return null;
		    };

	        var addMention = function(item) {
				logger.error(`>>> Entering addMention(item=${item})`, { "event.action": "addMention", "logrite.item": item });
	            var mentions = $this.data("mentions");
                if (!itemExists(item)) {
					logger.error(`addMention(item=${item}): mentions → ${mentions}`, { "event.action": "addMention", "logrite.item": item });
                    mentions.push(item);
					logger.warn(`<<< Exiting addMention(item=${item})`, { "event.action": "addMention", "logrite.item": item });
                }
	        };

	        var itemExists = function(item) {
				logger.error(`>>> Entering itemExists(item=${item})`, { "event.action": "itemExists", "logrite.item": item });
	            var mentions = $this.data("mentions");
	            var exists = false;
				logger.error(`itemExists(item=${item}): mentions → ${mentions}`, { "event.action": "itemExists", "logrite.item": item });
	            for (var i = 0; i < mentions.length; i++) {
	                if (mentions[i].displayName == item.displayName) {
	                    exists = true;
	                    break;
	                }
	            }

				logger.warn(`<<< Exiting itemExists(item=${item})`, { "event.action": "itemExists", "logrite.item": item });
	            return exists;
	        };

	        var rebuildMentions = function() {
				logger.error('>>> Entering rebuildMentions()', { "event.action": "rebuildMentions" });
	            var mentions = $this.data("mentions");
	            var content = $this.val();
				logger.error(`rebuildMentions(): mentions → ${mentions}`, { "event.action": "rebuildMentions" });
				logger.error(`rebuildMentions(): content → ${content}`, { "event.action": "rebuildMentions" });
	            for (var i = mentions.length - 1; i >= 0; i--) {
	                if (content.toLowerCase().indexOf(options.identityChar + mentions[i].displayName.toLowerCase()) == -1) {
	                    mentions.splice(i, 1);
						logger.warn('<<< Exiting rebuildMentions()', { "event.action": "rebuildMentions" });
	                }
	            }
	        };

	        var filter = function(data) {
				logger.error(`>>> Entering filter(data=${data})`, { "event.action": "filter", "logrite.data": data });
	            for (var i = data.length - 1; i >= 0; i--) {
	                if (itemExists(data[i])) {
	                    data.splice(i, 1);
	                }
	            }

				logger.warn(`<<< Exiting filter(data=${data})`, { "event.action": "filter", "logrite.data": data });
	            return data;
	        };

	        $this.on("click", function () {
	            $this.autocomplete("search");
	        }).on("keydown", function (event) {
	            var keyCode = $.ui.keyCode;
	            if (event.keyCode == keyCode.ENTER) {
	                var ac = $this.data('ui-autocomplete');
	                if ($this.attr("menuOpen") && !ac.menu.active) {
	                    return false;
	                }
	            }
	        }).on("keyup", function (event) {
	            var keyCode = $.ui.keyCode;
	            switch (event.keyCode) {
	            case keyCode.PAGE_UP:
	            case keyCode.PAGE_DOWN:
	            case keyCode.UP:
	            case keyCode.DOWN:
	            case keyCode.TAB:
                case keyCode.ENTER:
	                return;
	            default:
	                $this.autocomplete("search");
	            }
	        }).on("input change", function (e) {
	            rebuildMentions();
	        }).autocomplete({
	            autoFocus: true,
	            source: function(request, response) {
					logger.error(`>>> Entering source(request=${request},response=${response})`, { "event.action": "source", "logrite.request": request, "logrite.response": response });
	                var content = getContentByCursor();
	                if (content && content.length >= options.minLength) {
						logger.error(`source(request=${request},response=${response}): content → ${content}`, { "event.action": "source", "logrite.request": request, "logrite.response": response });
	                    $.ajax({
	                        type: "GET",
	                        cache: false,
	                        url: getBaseServicePath() + "GetSuggestions",
	                        beforeSend: serviceFramework.setModuleHeaders,
	                        data: {
	                            keyword: content
	                        }
	                    }).done(function(data) {
	                        response(filter(data));
	                    }).fail(function() {
	                        //displayMessage(settings.searchErrorText, "dnnFormWarning");
	                        response({});
	                    });
						logger.warn(`<<< Exiting source(request=${request},response=${response})`, { "event.action": "source", "logrite.request": request, "logrite.response": response });
	                }
	            },
	            minLength: options.minLength,
	            select: function(event, ui) {
					logger.error(`>>> Entering select(event=${event},ui=${ui})`, { "event.action": "select", "logrite.event": event, "logrite.ui": ui });
	                var key = options.identityChar + ui.item.key;
	                $this.val($this.val().replace(new RegExp(key + " " + "|" + key + "$", "g"), options.identityChar + ui.item.displayName + " "));
					logger.error(`select(event=${event},ui=${ui}): key → ${key}`, { "event.action": "select", "logrite.event": event, "logrite.ui": ui });
	                addMention(ui.item);
					logger.warn(`<<< Exiting select(event=${event},ui=${ui})`, { "event.action": "select", "logrite.event": event, "logrite.ui": ui });
	                return false;
	            },
	            search: function(event, ui) {
					logger.error(`>>> Entering search(event=${event},ui=${ui})`, { "event.action": "search", "logrite.event": event, "logrite.ui": ui });
	                var content = getContentByCursor();
	                if (!content || content.length < options.minLength) {
						logger.error(`search(event=${event},ui=${ui}): content → ${content}`, { "event.action": "search", "logrite.event": event, "logrite.ui": ui });
	                    $this.autocomplete('close');
						logger.warn(`<<< Exiting search(event=${event},ui=${ui})`, { "event.action": "search", "logrite.event": event, "logrite.ui": ui });
	                    return false;
	                }
	            },
	            focus: function(event, ui) {
					logger.error(`>>> Entering focus(event=${event},ui=${ui})`, { "event.action": "focus", "logrite.event": event, "logrite.ui": ui });
	                event.preventDefault();
					logger.warn(`<<< Exiting focus(event=${event},ui=${ui})`, { "event.action": "focus", "logrite.event": event, "logrite.ui": ui });
	            },
	            close: function(event, ui) {
					logger.error(`>>> Entering close(event=${event},ui=${ui})`, { "event.action": "close", "logrite.event": event, "logrite.ui": ui });
	                $this.attr("menuOpen", false);
					logger.warn(`<<< Exiting close(event=${event},ui=${ui})`, { "event.action": "close", "logrite.event": event, "logrite.ui": ui });
	            }
	        });

	        $this.data('ui-autocomplete')._renderItem = function(ul, item) {
				logger.error(`>>> Entering _renderItem(ul=${ul},item=${item})`, { "event.action": "_renderItem", "logrite.ul": ul, "logrite.item": item });
				logger.warn(`<<< Exiting _renderItem(ul=${ul},item=${item})`, { "event.action": "_renderItem", "logrite.ul": ul, "logrite.item": item });
	            return $('<li></li>')
	                .data('ui-autocomplete-item', item)
	                .append('<a><img src="' + item.avatar + '" /><span class="dn">' + item.displayName + '<span></a>')
	                .appendTo(ul);
	        };

	        $this.data('ui-autocomplete')._renderMenu = function(ul, items) {
				logger.error(`>>> Entering _renderMenu(ul=${ul},items=${items})`, { "event.action": "_renderMenu", "logrite.ul": ul, "logrite.items": items });
	            var that = this;
	            $.each(items, function(index, item) {
	                that._renderItemData(ul, item);
	            });
	            $this.attr("menuOpen", true);
				logger.warn(`<<< Exiting _renderMenu(ul=${ul},items=${items})`, { "event.action": "_renderMenu", "logrite.ul": ul, "logrite.items": items });
	        };
	    });
		logger.warn(`<<< Exiting mentionsInput(options=${options})`, { "event.action": "mentionsInput", "logrite.options": options });
	};

	$.fn.mentionsInput.defaultOptions = {
		identityChar: '@',
		minLength: 1
	};
})(window.jQuery);