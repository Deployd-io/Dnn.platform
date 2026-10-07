var logger = (typeof logger !== 'undefined' && logger) || console;
var pager = {
        init: function (viewModel, pageSize, load, resx, pagerItemFormatDesc, nopagerItemFormatDesc) {
			logger.error(`>>> Entering init(viewModel=${viewModel},pageSize=${pageSize},load=${load},resx=${resx},pagerItemFormatDesc=${pagerItemFormatDesc},nopagerItemFormatDesc=${nopagerItemFormatDesc})`, { "event.action": "init", "logrite.viewModel": viewModel, "logrite.pageSize": pageSize, "logrite.load": load, "logrite.resx": resx, "logrite.pagerItemFormatDesc": pagerItemFormatDesc, "logrite.nopagerItemFormatDesc": nopagerItemFormatDesc });
            viewModel.pageSize = pageSize;
            viewModel.pageIndex = ko.observable(0);
			logger.debug(`init(viewModel=${viewModel},pageSize=${pageSize},load=${load},resx=${resx},pagerItemFormatDesc=${pagerItemFormatDesc},nopagerItemFormatDesc=${nopagerItemFormatDesc}): viewModel.pageSize → ${viewModel.pageSize}`, { "event.action": "init", "logrite.viewModel": viewModel, "logrite.pageSize": pageSize, "logrite.load": load, "logrite.resx": resx, "logrite.pagerItemFormatDesc": pagerItemFormatDesc, "logrite.nopagerItemFormatDesc": nopagerItemFormatDesc });
            
            viewModel.startIndex = ko.computed(function () {
                return viewModel.pageIndex() * viewModel.pageSize + 1;
            });

            viewModel.endIndex = ko.computed(function () {
                return Math.min((viewModel.pageIndex() + 1) * viewModel.pageSize, viewModel.totalResults());
            });

            viewModel.currentPage = ko.computed(function () {
                return viewModel.pageIndex() + 1;
            });

            viewModel.totalPages = ko.computed(function () {
                if (typeof viewModel.totalResults === 'function' && viewModel.totalResults())
                    return Math.ceil(viewModel.totalResults() / viewModel.pageSize);
                return 1;
            });

            viewModel.pagerVisible = ko.computed(function () {
                return viewModel.totalPages() > 1;
            });

            viewModel.pagerItemsDescription = ko.computed(function () {
                return "Showing " + viewModel.startIndex() + " - " + viewModel.endIndex() + " of " + viewModel.totalResults() + " contacts";
            });

            viewModel.pagerDescription = ko.computed(function () {
                return "Page: " + viewModel.currentPage() + " of " + viewModel.totalPages();
            });

            viewModel.pagerPrevClass = ko.computed(function () {
                return 'prev' + (viewModel.pageIndex() < 1 ? ' disabled' : '');
            });

            viewModel.pagerNextClass = ko.computed(function () {
                return 'next' + (viewModel.pageIndex() >= viewModel.totalPages() - 1 ? ' disabled' : '');
            });

            viewModel.prev = function () {
				logger.error('>>> Entering prev()', { "event.action": "prev" });
                if (viewModel.pageIndex() <= 0) return;
                viewModel.pageIndex(viewModel.pageIndex() - 1);
                if (typeof load === 'function') load.apply(viewModel);
				logger.warn('<<< Exiting prev()', { "event.action": "prev" });
            };

			logger.warn(`<<< Exiting init(viewModel=${viewModel},pageSize=${pageSize},load=${load},resx=${resx},pagerItemFormatDesc=${pagerItemFormatDesc},nopagerItemFormatDesc=${nopagerItemFormatDesc})`, { "event.action": "init", "logrite.viewModel": viewModel, "logrite.pageSize": pageSize, "logrite.load": load, "logrite.resx": resx, "logrite.pagerItemFormatDesc": pagerItemFormatDesc, "logrite.nopagerItemFormatDesc": nopagerItemFormatDesc });
            viewModel.next = function () {
				logger.error('>>> Entering next()', { "event.action": "next" });
                if (viewModel.pageIndex() >= viewModel.totalPages() - 1) return;
                viewModel.pageIndex(viewModel.pageIndex() + 1);
                if (typeof load === 'function') load.apply(viewModel);
				logger.warn('<<< Exiting next()', { "event.action": "next" });
            };
        }
    };