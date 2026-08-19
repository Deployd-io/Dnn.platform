const logger = console;
﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function ($, ko) {
	logger.debug('>>> Entering define()');
    'use strict';
	logger.debug('<<< Exiting define()');

    var changeSort = function (e) {
		logger.debug(`>>> Entering changeSort(e=${e})`);
        var $element = $(e.currentTarget);
        var $arrow = $element.find('.sort');
        var viewModel = ko.dataFor($element.parent().parent().parent()[0]);
        var column = ko.dataFor($element[0]);

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
			logger.debug(`<<< Exiting changeSort(e=${e})`);
        }
    }

    var init = function(element, valueAccessor, allBindings, viewModel, bindingContext) {
		logger.debug(`>>> Entering init(element=${element},valueAccessor=${valueAccessor},allBindings=${allBindings},viewModel=${viewModel},bindingContext=${bindingContext})`);
        var $element = $(element);
		logger.debug(`<<< Exiting init(element=${element},valueAccessor=${valueAccessor},allBindings=${allBindings},viewModel=${viewModel},bindingContext=${bindingContext})`);

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