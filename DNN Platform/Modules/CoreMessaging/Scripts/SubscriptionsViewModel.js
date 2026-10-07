var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

(function ($) {
    window.SubscriptionsViewModel = function (ko, settings, root) {
		logger.error(`>>> Entering SubscriptionsViewModel(ko=${ko},settings=${settings},root=${root})`, { "event.action": "SubscriptionsViewModel", "logrite.ko": ko, "logrite.settings": settings, "logrite.root": root });
        var self = this;

        this.isLoading = ko.observable(false);
        this.results = ko.observableArray([]);
                
        this.localizer = function () {
			logger.error('>>> Entering localizer()', { "event.action": "localizer" });
			logger.warn('<<< Exiting localizer()', { "event.action": "localizer" });
	        return root.localizationController;
        };
        
        this.search = function () {
			logger.error('>>> Entering search()', { "event.action": "search" });
            var startSearch = function () {
				logger.error('>>> Entering startSearch()', { "event.action": "startSearch" });
                self.isLoading(true);
				logger.warn('<<< Exiting startSearch()', { "event.action": "startSearch" });
            };

            var finishSearch = function () {
				logger.error('>>> Entering finishSearch()', { "event.action": "finishSearch" });
                self.isLoading(false);
				logger.warn('<<< Exiting finishSearch()', { "event.action": "finishSearch" });
            };

            var success = function (model) {
				logger.error(`>>> Entering success(model=${model})`, { "event.action": "success", "logrite.model": model });
                self.results(model.Results);
                self.totalCount(model.TotalResults || 0);

                finishSearch();
				logger.warn(`<<< Exiting success(model=${model})`, { "event.action": "success", "logrite.model": model });
            };

            var failure = function (xhr, status) {                
				logger.error(`>>> Entering failure(xhr=${xhr},status=${status})`, { "event.action": "failure", "logrite.xhr": xhr, "logrite.status": status });
                if ((status || new String()).length > 0) {
                    $.dnnAlert({
                        title: localizer.getString('SearchErrorTitle'),
                        text: localizer.getString('SearchError') + ': ' + (status || 'Unknown error')
                    });
                }

                self.results([]);
                self.totalResults(0);

                finishSearch();
				logger.warn(`<<< Exiting failure(xhr=${xhr},status=${status})`, { "event.action": "failure", "logrite.xhr": xhr, "logrite.status": status });
            };

            startSearch();

            var searchParameters = {
                pageIndex: self.currentPage(),
                pageSize: self.pageSize(),
                sortExpression: self.sortedByColumnName() + ' ' + self.sortedByOrder()
            };

            root.requestService('Subscriptions/GetSubscriptions', 'get', searchParameters, success, failure, self.loading);
			logger.warn('<<< Exiting search()', { "event.action": "search" });
        };
	          
        this.sortedByColumnName = ko.observable('');
        this.sortedByOrder = ko.observable('');
        this.currentPage = ko.observable(10);
        
        this.sortColumn = function (columnName) {
			logger.error(`>>> Entering sortColumn(columnName=${columnName})`, { "event.action": "sortColumn", "logrite.columnName": columnName });
            var sort = '';
            if (self.sortedByColumnName() == columnName) {
				logger.error(`sortColumn(columnName=${columnName}): sort → ${sort}`, { "event.action": "sortColumn", "logrite.columnName": columnName });
                if (self.sortedByOrder() == 'asc') sort = 'desc';
                if (self.sortedByOrder() == '') sort = 'asc';
            } else {
                sort = 'asc';
                self.sortedByColumnName(columnName);
            }

            self.sortedByOrder(sort);
            self.currentPage(0);
            self.search();
			logger.warn(`<<< Exiting sortColumn(columnName=${columnName})`, { "event.action": "sortColumn", "logrite.columnName": columnName });
        };

        this.sortByDescription = function () {
			logger.error('>>> Entering sortByDescription()', { "event.action": "sortByDescription" });
            self.sortColumn('Description');
			logger.warn('<<< Exiting sortByDescription()', { "event.action": "sortByDescription" });
        };

        this.sortBySubscriptionType = function () {
			logger.error('>>> Entering sortBySubscriptionType()', { "event.action": "sortBySubscriptionType" });
            self.sortColumn('SubscriptionType');
			logger.warn('<<< Exiting sortBySubscriptionType()', { "event.action": "sortBySubscriptionType" });
        };

        this.sortCss = function (columnName) {
			logger.error(`>>> Entering sortCss(columnName=${columnName})`, { "event.action": "sortCss", "logrite.columnName": columnName });
            if (self.sortedByColumnName() == columnName) {
                if (self.sortedByOrder() == 'asc') return 'sortAsc';

                if (self.sortedByOrder() == 'desc') return 'sortDesc';
            }
            
			logger.warn(`<<< Exiting sortCss(columnName=${columnName})`, { "event.action": "sortCss", "logrite.columnName": columnName });
            return '';
        };

        this.sortCssDescription = ko.computed(function () {
            return self.sortCss('Description');
        });
        
        this.sortCssSubscriptionType = ko.computed(function () {
            return self.sortCss('SubscriptionType');
        });
        
        self.totalCount = ko.observable(0);

        self.pageSize = ko.observable('10');

        self.currentPage = ko.observable(0);

        self.pageSlide = ko.observable(2);
        
        self.lastPage = ko.computed(function () {
            return Math.ceil(self.totalCount() / self.pageSize());
        });
        
        this.pages = ko.computed(function () {
            var pageCount = self.lastPage();
            var pageFrom = Math.max(1, self.currentPage() - self.pageSlide());
            var pageTo = Math.min(pageCount, self.currentPage() + self.pageSlide());
            pageFrom = Math.max(1, Math.min(pageTo - 2 * self.pageSlide(), pageFrom));
            pageTo = Math.min(pageCount, Math.max(pageFrom + 2 * self.pageSlide(), pageTo));

            var result = [];
            for (var i = pageFrom; i <= pageTo; i++) {
                result.push(i);
            }

            return result;
        });
        
        self.changePage = function (page) {
			logger.error(`>>> Entering changePage(page=${page})`, { "event.action": "changePage", "logrite.page": page });
            self.currentPage(page);
            self.search();
			logger.warn(`<<< Exiting changePage(page=${page})`, { "event.action": "changePage", "logrite.page": page });
        };
        
        self.totalItemsText = ko.computed(function () {
            if (self.totalCount() == 1) {
                return self.localizer().getString('OneItem');
            } else if (self.totalCount() < self.pageSize()) {
                return self.localizer().getString('Items')
                    .replace('[ITEMS]', self.totalCount());
            } else {
                return self.localizer().getString('ItemsOnPage')
                    .replace('[ITEMS]', self.totalCount())
                    .replace('[PAGES]', Math.ceil(self.totalCount() / self.pageSize()));
            }
        });
        
        // Wait for other components to finish registration
        $(document).ready(function () {
            if (root.pageControl != null) {
                root.pageControl.page.subscribe(
                    function () {
                        self.search();
                    });
            }

            // Do an initial search.
            self.search();
        });
		logger.warn(`<<< Exiting SubscriptionsViewModel(ko=${ko},settings=${settings},root=${root})`, { "event.action": "SubscriptionsViewModel", "logrite.ko": ko, "logrite.settings": settings, "logrite.root": root });
    };
})(window.jQuery);