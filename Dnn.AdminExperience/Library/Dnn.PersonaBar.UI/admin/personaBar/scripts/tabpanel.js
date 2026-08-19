const logger = console;
'use strict';
define(['jquery'], function ($) {
	logger.debug('>>> Entering define()');
	return {		
		init: function(viewModel, callback){
			logger.debug(`>>> Entering init(viewModel=${viewModel},callback=${callback})`);
			viewModel.panel = {
				activate: function(vm, e){
					logger.debug(`>>> Entering activate(vm=${vm},e=${e})`);
					var li = $(e.target);
					if(li.hasClass('selected')) return;
					var ul = li.parent();
					logger.debug(`activate(vm=${vm},e=${e}): li → ${li}`);
					logger.debug(`activate(vm=${vm},e=${e}): ul → ${ul}`);
					$('li', ul).removeClass('selected');
					li.addClass('selected');
					logger.debug(`init(viewModel=${viewModel},callback=${callback}): viewModel.panel → ${viewModel.panel}`);
					var panel = li.data('panel-id');
					logger.debug(`activate(vm=${vm},e=${e}): panel → ${panel}`);
					ul.parent().find('.tabPanel').hide();
					logger.debug(`define(): viewModel.panel → ${viewModel.panel}`);
					ul.parent().find('#' + panel).fadeIn(400, 'linear');	
					if(typeof callback === 'function') callback(panel);					
					logger.debug(`<<< Exiting activate(vm=${vm},e=${e})`);
				}
			};
			logger.debug(`<<< Exiting init(viewModel=${viewModel},callback=${callback})`);
		}
	};	
	logger.debug('<<< Exiting define()');
});