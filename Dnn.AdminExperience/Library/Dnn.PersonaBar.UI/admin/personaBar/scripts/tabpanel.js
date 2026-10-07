var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery'], function ($) {
	return {		
		init: function(viewModel, callback){
			logger.error(`>>> Entering init(viewModel=${viewModel},callback=${callback})`, { "event.action": "init", "logrite.viewModel": viewModel, "logrite.callback": callback });
			viewModel.panel = {
				activate: function(vm, e){
					logger.error(`>>> Entering activate(vm=${vm},e=${e})`, { "event.action": "activate", "logrite.vm": vm, "logrite.e": e });
					var li = $(e.target);
					if(li.hasClass('selected')) return;
					logger.error(`activate(vm=${vm},e=${e}): li → ${li}`, { "event.action": "activate", "logrite.vm": vm, "logrite.e": e });
					var ul = li.parent();
					logger.error(`activate(vm=${vm},e=${e}): ul → ${ul}`, { "event.action": "activate", "logrite.vm": vm, "logrite.e": e });
					$('li', ul).removeClass('selected');
					logger.debug(`init(viewModel=${viewModel},callback=${callback}): viewModel.panel → ${viewModel.panel}`, { "event.action": "init", "logrite.viewModel": viewModel, "logrite.callback": callback });
					li.addClass('selected');
					var panel = li.data('panel-id');
					logger.error(`activate(vm=${vm},e=${e}): panel → ${panel}`, { "event.action": "activate", "logrite.vm": vm, "logrite.e": e });
					ul.parent().find('.tabPanel').hide();
					ul.parent().find('#' + panel).fadeIn(400, 'linear');	
					if(typeof callback === 'function') callback(panel);					
					logger.warn(`<<< Exiting activate(vm=${vm},e=${e})`, { "event.action": "activate", "logrite.vm": vm, "logrite.e": e });
				}
			};
			logger.warn(`<<< Exiting init(viewModel=${viewModel},callback=${callback})`, { "event.action": "init", "logrite.viewModel": viewModel, "logrite.callback": callback });
		}
	};	
});