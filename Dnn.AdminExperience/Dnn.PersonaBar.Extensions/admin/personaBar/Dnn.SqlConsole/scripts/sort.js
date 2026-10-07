var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var changeSort = function (e) {
		logger.error(`>>> Entering changeSort(e=${e})`, { "event.action": "changeSort", "logrite.e": e });
        var $element = $(e.currentTarget);
        var $arrow = $element.find('.sort');
        var viewModel = ko.dataFor($element.parent().parent().parent()[0]);
		logger.error(`changeSort(e=${e}): viewModel → ${viewModel}`, { "event.action": "changeSort", "logrite.e": e });
        var column = ko.dataFor($element[0]);
		logger.error(`changeSort(e=${e}): column → ${column}`, { "event.action": "changeSort", "logrite.e": e });

        $arrow.removeClass('asc desc');

        if (viewModel.sortColumn() !== column) {
            viewModel.sortType(0);

            $element.removeClass('current');
        }

        viewModel.sortColumn(column);

        var sortType = viewModel.sortType();
        switch (sortType) {
            case 0:
                viewModel.sortType(1);
				logger.error(`changeSort(e=${e}): sortType → ${sortType}`, { "event.action": "changeSort", "logrite.e": e });
                break;
            case 1:
                viewModel.sortType(2);
                break;
            case 2:
                viewModel.sortType(0);
                break;
            default:
                viewModel.sortType(1);
                break;
        }

        sortType = viewModel.sortType();

        viewModel.currentPage(1);
        $element.addClass('current');

        if (sortType === 1 || sortType === 2) {
            $arrow.addClass(sortType === 1 ? 'asc' : 'desc');
			logger.warn(`<<< Exiting changeSort(e=${e})`, { "event.action": "changeSort", "logrite.e": e });
        }
    }

    var init = function(element, valueAccessor, allBindings, viewModel, bindingContext) {
		logger.error(`>>> Entering init(element=${element},valueAccessor=${valueAccessor},allBindings=${allBindings},viewModel=${viewModel},bindingContext=${bindingContext})`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor, "logrite.allBindings": allBindings, "logrite.viewModel": viewModel, "logrite.bindingContext": bindingContext });
        var $element = $(element);

		logger.warn(`<<< Exiting init(element=${element},valueAccessor=${valueAccessor},allBindings=${allBindings},viewModel=${viewModel},bindingContext=${bindingContext})`, { "event.action": "init", "logrite.element": element, "logrite.valueAccessor": valueAccessor, "logrite.allBindings": allBindings, "logrite.viewModel": viewModel, "logrite.bindingContext": bindingContext });
        $element.addClass('table-sortable')
            .append('<span class="sort"></span>')
            .click(changeSort);
    }

    var update = function(element, valueAccessor, allBindings, viewModel, bindingContext) {}


    ko.bindingHandlers.sort = {
        init: init,
        update: update
    };
});