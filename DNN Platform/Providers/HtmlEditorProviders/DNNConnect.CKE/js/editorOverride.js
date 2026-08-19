const logger = console;
﻿(function ($) {
	logger.debug('>>> Entering function()');
    if (CKEDITOR) {
        CKEDITOR.on('dialogDefinition', function(evt) {
            var dialog = evt.data;
            if (dialog.name === 'image') {
                // Get dialog definition.
                var def = evt.data.definition;
                var originOkFunc = def.onOk;

                def.onOk = function () {
                    originOkFunc.apply(this, arguments);
                    var $element = $(this.imageElement);
                    $element.attr('title', $element.attr('alt'));
					logger.debug('<<< Exiting function()');
                };

			logger.debug(`<<< Exiting on(dialog=${dialog})`);
            }
        });
    }
})(jQuery);