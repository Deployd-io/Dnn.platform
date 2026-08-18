const logger = console;
﻿jQuery(document).ready(function($) {
	logger.debug(`>>> Entering ready(document=${document})`);
    $('.dnnTooltip').dnnTooltip();
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function() {
		logger.debug('>>> Entering add_endRequest()');
        $('.dnnTooltip').dnnTooltip();
	logger.debug(`<<< Exiting ready(document=${document})`);
    });
});