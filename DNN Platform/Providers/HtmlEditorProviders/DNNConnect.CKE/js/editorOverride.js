var logger = (typeof logger !== 'undefined' && logger) || console;
(function ($) {
    if (CKEDITOR) {
        CKEDITOR.on('dialogDefinition', function(evt) {
            var dialog = evt.data;
            if (dialog.name === 'image') {
                // Get dialog definition.
                var def = evt.data.definition;
                var originOkFunc = def.onOk;

                def.onOk = function () {
					logger.error('>>> Entering onOk()', { "event.action": "onOk" });
                    originOkFunc.apply(this, arguments);
                    var $element = $(this.imageElement);
                    $element.attr('title', $element.attr('alt'));
					logger.warn('<<< Exiting onOk()', { "event.action": "onOk" });
                };

            }
        });
    }
})(jQuery);