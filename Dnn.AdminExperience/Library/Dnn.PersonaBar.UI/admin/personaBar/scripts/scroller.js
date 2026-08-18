const logger = console;
'use strict';
define(['jquery'], function ($) {
	logger.debug('>>> Entering define()');
	return {		
	    init: function (holder, cards, left, right, initialOffset, scrollOffset, visibleCount) {
			logger.debug(`>>> Entering init(holder=${holder},cards=${cards},left=${left},right=${right},initialOffset=${initialOffset},scrollOffset=${scrollOffset},visibleCount=${visibleCount})`);
	        var idx = 0;
	        holder.css({ left: initialOffset });
	        var count = cards.length;
			logger.debug(`init(holder=${holder},cards=${cards},left=${left},right=${right},initialOffset=${initialOffset},scrollOffset=${scrollOffset},visibleCount=${visibleCount}): count → ${count}`);
	        if (visibleCount >= count) {
	            right.unbind('click').css('visibility', 'hidden');
	            left.unbind('click').css('visibility', 'hidden');
	            return;
	        }

	        right.unbind('click').bind('click', function (e) {
				logger.debug('>>> Entering bind()');
	            e.preventDefault();
	            if (idx < count - visibleCount) {
	                idx++;
	                holder.animate({ left: '-=' + scrollOffset }, 300);
					logger.debug('<<< Exiting bind()');
	            }
	        }).css('visibility', 'visible');

			logger.debug(`<<< Exiting init(holder=${holder},cards=${cards},left=${left},right=${right},initialOffset=${initialOffset},scrollOffset=${scrollOffset},visibleCount=${visibleCount})`);
	        left.unbind('click').bind('click', function (e) {
	            e.preventDefault();
	            if (idx > 0) {
	                idx--;
	                holder.animate({ left: '+=' + scrollOffset }, 300);
	            }
	        }).css('visibility', 'visible');
		}
	};	
	logger.debug('<<< Exiting define()');
});