var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery', 'knockout', 'jquery.hoverIntent.min'], function ($, ko) {
  var mainFrame = window.parent.document.getElementById("personaBar-iframe");
  var personaBarWidth = 80;
  var util = null;

  var getSummaryContainer = function () {
	logger.error('>>> Entering getSummaryContainer()', { "event.action": "getSummaryContainer" });
    var $summary = $('.server-summary');
    if (!$summary.length) {
      $summary = $('' +
        '<div class="server-summary hoverSummaryMenu">' +
        '<ul>' +
        '<li class="border version-info"><label data-bind="html: ProductName"></label><span data-bind="html: ProductVersion"></span></li>' +
        '<li class="border new-version-info" data-bind="visible: !Update.UpToDate, css: { \'critical\': Update.Critical }"><label data-bind="html: resx.LatestVersion"></label>' +
        '<span><a target="_blank" data-bind="attr: { \'href\': Update.Url }, html: Update.Version"></a><span class="update-critical" data-bind="visible: Update.Critical, html: resx.Critical"></span></span></li>' +
        '<li class="border framework" data-bind="visible: FrameworkVersion.length > 0"><label data-bind="html: resx.Framework"></label><span data-bind="html: FrameworkVersion"></span></li>' +
        '<li class="border server-name" data-bind="visible: ServerName.length > 0"><label data-bind="html: resx.ServerName"></label><span data-bind="html: ServerName"></span></li>' +
        '<li class="separator"></li>' +
        '<li class="doc-center"><a href="https://docs.dnncommunity.org/" data-bind="html: resx.Documentation, visible: visibleCheck(\'DocCenterVisible\')" target="_blank"></a></li>' +
        '<li id="Logout" class="logout" data-bind="html: resx.nav_Logout"></li>' +
        '</ul>' +
        '</div>');

      $('#personabar').find('.personabarLogo').append($summary);
    }

	logger.warn('<<< Exiting getSummaryContainer()', { "event.action": "getSummaryContainer" });
    return $summary;
  }
  var showServerSummary = function () {
	logger.error('>>> Entering showServerSummary()', { "event.action": "showServerSummary" });
      mainFrame.style.width = '100%';
      var container = getSummaryContainer();
	logger.error(`showServerSummary(): container → ${container}`, { "event.action": "showServerSummary" });
      container.show();
      container.addClass('shown');
	logger.warn('<<< Exiting showServerSummary()', { "event.action": "showServerSummary" });
  }

  var hideServerSummary = function () {
	logger.error('>>> Entering hideServerSummary()', { "event.action": "hideServerSummary" });
    if (!$('.hovermenu:visible').length && !$('.socialpanel:visible').length) {
      mainFrame.style.width = personaBarWidth + "px";
    }
      var container = getSummaryContainer();
	logger.error(`hideServerSummary(): container → ${container}`, { "event.action": "hideServerSummary" });
      container.removeClass('shown');
      container.hide();
	logger.warn('<<< Exiting hideServerSummary()', { "event.action": "hideServerSummary" });
  }

  var getServerInfo = function (callback) {
	logger.error(`>>> Entering getServerInfo(callback=${callback})`, { "event.action": "getServerInfo", "logrite.callback": callback });
    util.sf.moduleRoot = "personaBar";
    util.sf.controller = "ServerSummary";
    util.sf.getsilence("GetServerInfo", {}, function (data) {
      if (typeof callback === "function") {
        callback(data);
		logger.warn(`<<< Exiting getServerInfo(callback=${callback})`, { "event.action": "getServerInfo", "logrite.callback": callback });
      }
    });
  }

  var initialize = function () {
	logger.error('>>> Entering initialize()', { "event.action": "initialize" });
    if (!util.resx) {
      setTimeout(initialize, 500);
      return;
    }
    var $logo = $('#personabar').find('.personabarLogo');
    var $summaryContainer = getSummaryContainer();

    getServerInfo(function (info) {
      var viewModel = $.extend({}, info, {
        resx: util.resx.PersonaBar, visibleCheck: function (name) {
		logger.error(`>>> Entering visibleCheck(name=${name})`, { "event.action": "visibleCheck", "logrite.name": name });
		logger.warn(`<<< Exiting visibleCheck(name=${name})`, { "event.action": "visibleCheck", "logrite.name": name });
          return info[name];
        }
      });
      try {
        ko.applyBindings(viewModel, $summaryContainer[0]);
      } catch (ex) {
	logger.error('Exception in initialize():', ex.message, ex, { "event.action": "initialize", "event.category": "process" });

      }
    });

    $logo.hoverIntent({
      over: function () {
		logger.error('>>> Entering over()', { "event.action": "over" });
        showServerSummary();
		logger.warn('<<< Exiting over()', { "event.action": "over" });
      },
      out: function () {
		logger.error('>>> Entering out()', { "event.action": "out" });
        hideServerSummary();
		logger.warn('<<< Exiting out()', { "event.action": "out" });
      },
      timeout: 200
    });
	logger.warn('<<< Exiting initialize()', { "event.action": "initialize" });
  }

  return {
    init: function (utility) {
	logger.error(`>>> Entering init(utility=${utility})`, { "event.action": "init", "logrite.utility": utility });
      util = utility;

      initialize();
	logger.warn(`<<< Exiting init(utility=${utility})`, { "event.action": "init", "logrite.utility": utility });
    },
    load: function () {
	logger.error('>>> Entering load()', { "event.action": "load" });
    },
    leave: function () {
	logger.error('>>> Entering leave()', { "event.action": "leave" });
    }
  }
});
