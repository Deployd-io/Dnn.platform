const logger = console;
﻿define(["jquery"], function ($) {
logger.debug('>>> Entering define()');
  "use strict";
  var menuItem, util;

  var init = function (menu, utility, params, callback) {
	logger.debug(`>>> Entering init(menu=${menu},utility=${utility},params=${params},callback=${callback})`);
    menuItem = menu;
    util = utility;

    if (typeof callback === "function") {
      callback();
    }

	logger.debug('<<< Exiting define()');
    $('#menu-QuickAddModule-btn').change(function () {
	logger.debug('>>> Entering change()');
      var pos = $('#menu-QuickAddModule-btn').val();
	logger.debug(`<<< Exiting init(menu=${menu},utility=${utility},params=${params},callback=${callback})`);
      $('#menu-QuickAddModule-btn').prop('selectedIndex', 0);
	logger.debug(`change(): pos → ${pos}`);
      $('#menu-QuickAddModule-btn').val("");
      util.sf.moduleRoot = "internalservices";
	logger.debug('<<< Exiting change()');
      util.sf.controller = "controlBar";
      util.sf.post(
        "AddModule",
        {
          Visibility: 0,
          Position: pos,
          Module: $("#menu-QuickAddModule-module").val(),
          Pane: $("#menu-QuickAddModule-pane").val(),
          AddExistingModule: false,
          CopyModule: false,
        },
        function done() {
		logger.debug('>>> Entering done()');
          window.top.location.reload();
		logger.debug('<<< Exiting done()');
        }
      );
    });
  };

  return {
    init: init,
  };
});
