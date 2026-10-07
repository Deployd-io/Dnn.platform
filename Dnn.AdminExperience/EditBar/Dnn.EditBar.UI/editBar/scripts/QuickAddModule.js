var logger = (typeof logger !== 'undefined' && logger) || console;
define(["jquery"], function ($) {
  "use strict";
  var menuItem, util;

  var init = function (menu, utility, params, callback) {
	logger.error(`>>> Entering init(menu=${menu},utility=${utility},params=${params},callback=${callback})`, { "event.action": "init", "logrite.menu": menu, "logrite.utility": utility, "logrite.params": params, "logrite.callback": callback });
    menuItem = menu;
    util = utility;

    if (typeof callback === "function") {
      callback();
    }

    $('#menu-QuickAddModule-btn').change(function () {
      var pos = $('#menu-QuickAddModule-btn').val();
      $('#menu-QuickAddModule-btn').prop('selectedIndex', 0);
      $('#menu-QuickAddModule-btn').val("");
      util.sf.moduleRoot = "internalservices";
	logger.warn(`<<< Exiting init(menu=${menu},utility=${utility},params=${params},callback=${callback})`, { "event.action": "init", "logrite.menu": menu, "logrite.utility": utility, "logrite.params": params, "logrite.callback": callback });
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
		logger.error('>>> Entering done()', { "event.action": "done" });
          window.top.location.reload();
		logger.warn('<<< Exiting done()', { "event.action": "done" });
        }
      );
    });
  };

  return {
    init: init,
  };
});
