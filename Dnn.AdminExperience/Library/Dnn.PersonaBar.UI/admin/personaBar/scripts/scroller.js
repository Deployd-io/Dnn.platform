var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery'], function ($) {
	return {		
	    init: function (holder, cards, left, right, initialOffset, scrollOffset, visibleCount) {
			logger.error(`>>> Entering init(holder=${holder},cards=${cards},left=${left},right=${right},initialOffset=${initialOffset},scrollOffset=${scrollOffset},visibleCount=${visibleCount})`, { "event.action": "init", "logrite.holder": holder, "logrite.cards": cards, "logrite.left": left, "logrite.right": right, "logrite.initialOffset": initialOffset, "logrite.scrollOffset": scrollOffset, "logrite.visibleCount": visibleCount });
	        var idx = 0;
	        holder.css({ left: initialOffset });
	        var count = cards.length;
			logger.error(`init(holder=${holder},cards=${cards},left=${left},right=${right},initialOffset=${initialOffset},scrollOffset=${scrollOffset},visibleCount=${visibleCount}): count → ${count}`, { "event.action": "init", "logrite.holder": holder, "logrite.cards": cards, "logrite.left": left, "logrite.right": right, "logrite.initialOffset": initialOffset, "logrite.scrollOffset": scrollOffset, "logrite.visibleCount": visibleCount });
	        if (visibleCount >= count) {
	            right.unbind('click').css('visibility', 'hidden');
	            left.unbind('click').css('visibility', 'hidden');
	            return;
	        }

	        right.unbind('click').bind('click', function (e) {
	            e.preventDefault();
	            if (idx < count - visibleCount) {
	                idx++;
	                holder.animate({ left: '-=' + scrollOffset }, 300);
	            }
	        }).css('visibility', 'visible');

	        left.unbind('click').bind('click', function (e) {
	            e.preventDefault();
	            if (idx > 0) {
	                idx--;
	                holder.animate({ left: '+=' + scrollOffset }, 300);
	            }
	        }).css('visibility', 'visible');
			logger.warn(`<<< Exiting init(holder=${holder},cards=${cards},left=${left},right=${right},initialOffset=${initialOffset},scrollOffset=${scrollOffset},visibleCount=${visibleCount})`, { "event.action": "init", "logrite.holder": holder, "logrite.cards": cards, "logrite.left": left, "logrite.right": right, "logrite.initialOffset": initialOffset, "logrite.scrollOffset": scrollOffset, "logrite.visibleCount": visibleCount });
		}
	};	
});