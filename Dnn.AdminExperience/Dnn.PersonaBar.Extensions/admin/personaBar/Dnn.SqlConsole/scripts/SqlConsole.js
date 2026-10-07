const logger = console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/*
* Module responsible to Sql Console
*/
define(['jquery',
  'knockout',
  'knockout.mapping',
  './sort',
  './exportData',
  './clipboard.min',
  './html2canvas',
  './FileSaver.min',
  'dnn.jquery',
  'main/koBindingHandlers/jScrollPane'],
  function ($, ko, koMapping, sort, exportData, Clipboard) {
    'use strict';

    var utility, resx, $panel, viewModel, sqlConsole, sqlContent, jsPdf;

    var pagesCount = 7;

    var requestService = function (type, method, params, callback, error) {
      utility.sf.moduleRoot = "personaBar";
      utility.sf.controller = "SqlConsole";

      utility.sf[type].call(utility.sf, method, params, callback, error);
	logger.warn(`<<< Exiting requestService(type=${type},method=${method},params=${params},callback=${callback},error=${error})`, { "event.action": "requestService", "logrite.type": type, "logrite.method": method, "logrite.params": params, "logrite.callback": callback, "logrite.error": error });
    }

    var getSavedQueries = function (selectedId) {
	logger.error(`>>> Entering getSavedQueries(selectedId=${selectedId})`, { "event.action": "getSavedQueries", "logrite.selectedId": selectedId });
      requestService('get', 'GetSavedQueries', {}, function (data) {
        viewModel.connections(data.connections);
        viewModel.connection(data.connections[0]);

        var newQuery = {
          id: -1,
          name: resx.NewQuery,
          connection: viewModel.connection(),
          query: ''
        };

        viewModel.savedQueries.removeAll();
        viewModel.savedQueries.push(newQuery);
        for (var i = 0; i < data.queries.length; i++) {
          viewModel.savedQueries.push(data.queries[i]);
        }

        viewModel.selectedQuery(selectedId);

        if (!viewModel.name()) {
          viewModel.name(newQuery.name);
		logger.warn(`<<< Exiting getSavedQueries(selectedId=${selectedId})`, { "event.action": "getSavedQueries", "logrite.selectedId": selectedId });
        }
      });
    }

    var initUpload = function () {
	logger.error('>>> Entering initUpload()', { "event.action": "initUpload" });
      var $uploadContainer = $panel.find('.fileupload-wrapper');
      var $uploadControl = $uploadContainer.find('input');

      if (typeof FileReader === "undefined") {
        $uploadContainer.hide();
        return;
      }

      $uploadControl.on('change', function (e) {
        var file = $uploadControl[0].files[0];
        var textType = /text.*/;
        if (file.type.match(textType) || file.name.toLowerCase().split('.').pop() === 'sql') {
          var reader = new FileReader();

          reader.onload = function (e) {
			logger.error(`>>> Entering onload(e=${e})`, { "event.action": "onload", "logrite.e": e });
            viewModel.query(reader.result);
			logger.warn(`<<< Exiting onload(e=${e})`, { "event.action": "onload", "logrite.e": e });
          }

          reader.readAsText(file);
        } else {
          utility.notifyError('File not supported');
        }

        $uploadControl.val('');
      });
	logger.warn('<<< Exiting initUpload()', { "event.action": "initUpload" });
    }

    var changeTab = function (data) {
      var index = data.index;

      $panel.find('.tables-container > div').hide();
      $panel.find('.tables-container > div#result' + index).show();
      var $tableContainer = $panel.find('.tables-container > div#result' + index).find('> div.table-container');
      if ($tableContainer.data('jsp')) {
        $tableContainer.data('jsp').destroy();
        $tableContainer = $panel.find('.tables-container > div#result' + index).find('> div.table-container');
      }
      $tableContainer.jScrollPane();

      $panel.find('.result-tabs ul li').removeClass('selected').eq(index).addClass('selected');
	logger.warn(`<<< Exiting changeTab(data=${data})`, { "event.action": "changeTab", "logrite.data": data });
    }

    var scrollPaneInitialised = function (table, e) {
      var $table = $panel.find('.tables-container > div#result' + table.index).find('table');
      if (!$table.attr('width') || $table.attr('width') === '0') {
        $table.attr('width', $table.width());
      }
	logger.warn(`<<< Exiting scrollPaneInitialised(table=${table},e=${e})`, { "event.action": "scrollPaneInitialised", "logrite.table": table, "logrite.e": e });
    }

    var savedQueryChanged = function (val) {
	logger.error(`>>> Entering savedQueryChanged(val=${val})`, { "event.action": "savedQueryChanged", "logrite.val": val });
      var id = viewModel.selectedQuery();
      var query;
	logger.error(`savedQueryChanged(val=${val}): id → ${id}`, { "event.action": "savedQueryChanged", "logrite.val": val });
      var savedQueries = viewModel.savedQueries();
	logger.error(`savedQueryChanged(val=${val}): savedQueries → ${savedQueries}`, { "event.action": "savedQueryChanged", "logrite.val": val });
      for (var i = 0; i < savedQueries.length; i++) {
        if (savedQueries[i].id === id) {
          query = savedQueries[i];
          break;
		logger.debug(`savedQueryChanged(val=${val}): query → ${query}`, { "event.action": "savedQueryChanged", "logrite.val": val });
        }
      }

      if (!query || !query.name) {
        return;
      }

      viewModel.query(query.query || '');
      viewModel.name(query.name);
      viewModel.connection(query.connection);
      viewModel.id(query.id || -1);
	logger.warn(`<<< Exiting savedQueryChanged(val=${val})`, { "event.action": "savedQueryChanged", "logrite.val": val });
    }

    var startSaveQuery = function () {
	logger.error('>>> Entering startSaveQuery()', { "event.action": "startSaveQuery" });
      viewModel.query(sqlContent.getValue());

      if (!viewModel.query()) {
        return;
      }

      viewModel.saving(true);

      if (viewModel.id() <= 0) {
        viewModel.name('');
      }

      $('#query-name').focus();
	logger.warn('<<< Exiting startSaveQuery()', { "event.action": "startSaveQuery" });
    }

    var saveQuery = function () {
	logger.error('>>> Entering saveQuery()', { "event.action": "saveQuery" });
      viewModel.query(sqlContent.getValue());

      if (!viewModel.query()) {
        return;
      }

      if (!viewModel.name()) {
        utility.notify(resx.EmptyName);
        return;
      }

      var query = {
        id: viewModel.id(),
        name: viewModel.name(),
        query: viewModel.query(),
        connection: viewModel.connection()
      }

      viewModel.loading(true);

      requestService('post', 'SaveQuery', query, function (data) {
        getSavedQueries(data.id);
        viewModel.saving(false);
        viewModel.loading(false);
      });
	logger.warn('<<< Exiting saveQuery()', { "event.action": "saveQuery" });
    }

    var cancelSave = function () {
	logger.error('>>> Entering cancelSave()', { "event.action": "cancelSave" });
      viewModel.saving(false);
	logger.warn('<<< Exiting cancelSave()', { "event.action": "cancelSave" });
    }

    var deleteQuery = function () {
	logger.error('>>> Entering deleteQuery()', { "event.action": "deleteQuery" });
      var confirmText = resx.DeleteConfirm;
      var deleteText = resx.Delete;
      var cancelText = resx.Cancel;

      utility.confirm(confirmText, deleteText, cancelText, function () {
        viewModel.loading(true);
        requestService('post', 'DeleteQuery', { id: viewModel.id() }, function () {
          getSavedQueries();
          viewModel.loading(false);
        });
      });
    }

    var clone = function (data) {
      var cloneData = {};
      for (var name in data) {
        if (data.hasOwnProperty(name)) {
          cloneData[name] = data[name];
        }
      }

      return cloneData;
    }

    var encode = function (item) {
      for (var name in item) {
        if (item.hasOwnProperty(name) && item[name]) {
          item[name] = $('<div/>').text(item[name]).html();
        }
      }

      return item;
    };

    var matchKeyword = function (data, keyword) {
      if (!keyword) {
        return true;
      }

      var keywords = keyword.toLowerCase().split(' ');

      var allEmptyKey = true;
      for (var i = 0; i < keywords.length; i++) {
        var key = keywords[i].trim();
        if (key) {
          allEmptyKey = false;
          break;
        }
      }

      if (allEmptyKey) {
        return true;
      }

      var matched = false;
      for (var name in data) {
        if (data.hasOwnProperty(name) && data[name]) {
          for (var i = 0; i < keywords.length; i++) {
            var key = keywords[i].trim();
            if (key) {
              var regexVal = key.replace(/[|\\{}()[\]^$+*?.]/g, '\\$&');
              var regex = new RegExp('(' + regexVal + ')', 'gi');
              if (regex.test(data[name].toString())) {
                data[name] = data[name].toString().replace(regex, '<span class="highlight">$1</span>');
                matched = true;
              }
            }
          }
        }
      }

      return matched;
    }

    var normalizeType = function (value) {
	logger.error(`>>> Entering normalizeType(value=${value})`, { "event.action": "normalizeType", "logrite.value": value });
      if (value === "" || value === null || typeof value === "undefined") {
        return "";
      }
      var type = typeof value;
      switch (type) {
        case 'boolean':
          return value;
        case 'number':
          return value;
        case 'string':
          var lcValue = value.toLowerCase();
		logger.error(`normalizeType(value=${value}): lcValue → ${lcValue}`, { "event.action": "normalizeType", "logrite.value": value });
          if (isFinite(lcValue)) {
            return parseFloat(lcValue);
          } else if (lcValue == "true") {
            return true;
          } else if (lcValue == "false") {
            return false;
          }
          return lcValue;
        default:
		logger.warn(`<<< Exiting normalizeType(value=${value})`, { "event.action": "normalizeType", "logrite.value": value });
          return value;

      }
    };

    var createDataTable = function (data) {
	logger.error(`>>> Entering createDataTable(data=${data})`, { "event.action": "createDataTable", "logrite.data": data });
      var table = { header: [], rows: ko.observableArray(data) };
      //try to find headers
      var row = data[0];
	logger.error(`createDataTable(data=${data}): table → ${table}`, { "event.action": "createDataTable", "logrite.data": data });
      for (var name in row) {
        if (row.hasOwnProperty(name)) {
          table.header.push(name);
        }
      }

      table.sortColumn = ko.observable(table.header[0]);
      table.sortType = ko.observable(0);

      table.keywords = ko.observable('');
      table.keywords.subscribe(function () {
        table.currentPage(1);
      });
      table.keywords.extend({ rateLimit: 200 });

      //try to get page info
      table.pageSizes = ko.observableArray([
        { name: resx.AllEntries, value: 0 },
        { name: resx.PageSize.replace('{0}', 10), value: 10 },
        { name: resx.PageSize.replace('{0}', 25), value: 25 },
        { name: resx.PageSize.replace('{0}', 50), value: 50 },
        { name: resx.PageSize.replace('{0}', 100), value: 100 }
      ]);

      table.pageSize = ko.observable(10);
      table.showPageSizesList = ko.observable(false);

      table.pageSizesLabel = ko.computed(function () {
        var sizes = table.pageSizes();
        for (var i = 0; i < sizes.length; i++) {
          if (sizes[i].value === table.pageSize()) {
            return sizes[i].name;
          }
        }

        return '';
      });

      table.changePageSize = function () {
		logger.error('>>> Entering changePageSize()', { "event.action": "changePageSize" });
        table.pageSize(this.value);
        table.currentPage(1);

        var $currentTable = $('.tables-container > div:visible').find('div.table-container');
        $currentTable.data('jsp').destroy();

        $currentTable = $('.tables-container > div:visible').find('div.table-container');
        $currentTable.jScrollPane();

        table.showPageSizesList(false);
		logger.warn('<<< Exiting changePageSize()', { "event.action": "changePageSize" });
      }

      table.currentPage = ko.observable(1);
      table.currentPage.subscribe(function () {
        setTimeout(function () {
          var $currentTable = $('.tables-container > div:visible').find('div.table-container');
          if ($currentTable.data('jsp')) {
            $currentTable.data('jsp').destroy();
          }
        }, 0);
        setTimeout(function () {
          var $currentTable = $('.tables-container > div:visible').find('div.table-container');

          if (!$currentTable.data('jsp')) {
            $currentTable.jScrollPane();
          }
        }, 10);

      });

      table.startIndex = ko.computed(function () {
        return (table.currentPage() - 1) * table.pageSize();
      });

      table.filterData = ko.computed(function () {
        var filterData = ko.observableArray();
        var source = table.rows();
        for (var i = 0; i < source.length; i++) {
          var keywords = table.keywords();
          var item = encode(clone(source[i]));
          if (matchKeyword(item, keywords)) {
            filterData.push(item);
          }
        }

        var sortType = table.sortType();
        if (sortType !== 0) {
          filterData.sort(function (left, right) {
            var sortColumn = table.sortColumn();
            var leftData = normalizeType(left[sortColumn]);
            var rightData = normalizeType(right[sortColumn]);

            if (leftData === rightData) {
              return 0;
            } else if (leftData === "" || leftData < rightData) {
              return sortType === 1 ? -1 : 1;
            } else if (leftData > rightData || rightData === "") {
              return sortType === 1 ? 1 : -1;
            }
          });
        }

        return filterData();
      });

      table.endIndex = ko.computed(function () {
        var endIndex = table.startIndex() + table.pageSize();

        if (table.pageSize() === 0 || endIndex > table.filterData().length) {
          endIndex = table.filterData().length;
        }
        return endIndex;
      });

      table.totalPages = ko.computed(function () {
        if (table.pageSize() === 0) {
          return 0;
        }
        var totalPages = parseInt(table.filterData().length / table.pageSize());
        if (table.filterData().length % table.pageSize() !== 0) {
          totalPages++;
        }
        return totalPages;
      });

      table.statistics = ko.computed(function () {
        var args = [table.startIndex() + 1, table.endIndex(), table.filterData().length];
        return viewModel.resx.PageInfo.replace(/\{(\d+)\}/gi, function (i) { var index = parseInt(arguments[1]); return args[index]; });
      });

      table.pagesNumber = ko.computed(function () {
        var pages = [];
        var totalPages = table.totalPages();
        if (totalPages <= pagesCount) {
          for (var i = 1; i <= totalPages; i++) {
            pages.push(i);
          }
        } else {
          var currentPage = table.currentPage();
          var split = parseInt(pagesCount / 2);
          var start = currentPage > split ? currentPage - split : 1;
          var end = totalPages - currentPage < split ? totalPages : start + pagesCount - 1;

          if (end - start !== pagesCount - 1) {
            start = end - pagesCount + 1;
          }

          for (var i = start; i <= end; i++) {
            pages.push(i);
          }
        }

        return pages;
      });

      table.prev = function (e) {
		logger.error(`>>> Entering prev(e=${e})`, { "event.action": "prev", "logrite.e": e });
        if (table.currentPage() > 1) {
          table.currentPage(table.currentPage() - 1);
		logger.warn(`<<< Exiting prev(e=${e})`, { "event.action": "prev", "logrite.e": e });
        }
      }

      table.next = function (e) {
		logger.error(`>>> Entering next(e=${e})`, { "event.action": "next", "logrite.e": e });
        if (table.currentPage() < table.totalPages()) {
          table.currentPage(table.currentPage() + 1);
		logger.warn(`<<< Exiting next(e=${e})`, { "event.action": "next", "logrite.e": e });
        }
      }

      table.changePage = function (page, e) {
		logger.error(`>>> Entering changePage(page=${page},e=${e})`, { "event.action": "changePage", "logrite.page": page, "logrite.e": e });
        table.currentPage(page);
		logger.warn(`<<< Exiting changePage(page=${page},e=${e})`, { "event.action": "changePage", "logrite.page": page, "logrite.e": e });
      };

      table.pageData = ko.computed(function () {
        var data = [];
        var filterData = table.filterData();
        for (var i = table.startIndex(); i < table.endIndex(); i++) {
          data.push(filterData[i]);
        }
        return data;
      });

      table.hasData = ko.computed(function () {
        return table.pageData().length > 0 || table.keywords().length > 0;
      });

      var exportFile = function (name, data) {
		logger.error(`>>> Entering exportFile(name=${name},data=${data})`, { "event.action": "exportFile", "logrite.name": name, "logrite.data": data });
        var blob = new Blob([data], {
          type: "application/vnd.ms-excel;charset=utf-8"
        });
        saveAs(blob, name);
		logger.error(`exportFile(name=${name},data=${data}): blob → ${blob}`, { "event.action": "exportFile", "logrite.name": name, "logrite.data": data });
      }

      var exportExcel = function () {
		logger.error('>>> Entering exportExcel()', { "event.action": "exportExcel" });
        viewModel.loading(true);
        var pageSize = table.pageSize();
		logger.error(`exportExcel(): pageSize → ${pageSize}`, { "event.action": "exportExcel" });
        var currentPage = table.currentPage();
		logger.error(`exportExcel(): currentPage → ${currentPage}`, { "event.action": "exportExcel" });
        table.currentPage(1);
        table.pageSize(table.filterData().length);

        exportFile(table.title + '.xls', exportData.excel($('#result' + table.index + ' table')[0]));

        table.pageSize(pageSize);
        table.currentPage(currentPage);
        table.showExportList(false);
        viewModel.loading(false);
		logger.warn('<<< Exiting exportExcel()', { "event.action": "exportExcel" });
      }

      var exportCsv = function () {
		logger.error('>>> Entering exportCsv()', { "event.action": "exportCsv" });
        viewModel.loading(true);
        var pageSize = table.pageSize();
		logger.error(`exportCsv(): pageSize → ${pageSize}`, { "event.action": "exportCsv" });
        var currentPage = table.currentPage();
		logger.error(`exportCsv(): currentPage → ${currentPage}`, { "event.action": "exportCsv" });
        table.currentPage(1);
        table.pageSize(table.filterData().length);

        exportFile(table.title + '.csv', exportData.csv($('#result' + table.index + ' table')[0]));

        table.pageSize(pageSize);
        table.currentPage(currentPage);
        table.showExportList(false);
        viewModel.loading(false);
		logger.warn('<<< Exiting exportCsv()', { "event.action": "exportCsv" });
      }

      var exportPdf = function () {
		logger.error('>>> Entering exportPdf()', { "event.action": "exportPdf" });
        viewModel.loading(true);
        var pageSize = table.pageSize();
		logger.error(`exportPdf(): pageSize → ${pageSize}`, { "event.action": "exportPdf" });
        var currentPage = table.currentPage();
		logger.error(`exportPdf(): currentPage → ${currentPage}`, { "event.action": "exportPdf" });
        table.currentPage(1);
        table.pageSize(table.filterData().length);
        var selector = '#result' + table.index + ' .table-container';
		logger.error(`exportPdf(): selector → ${selector}`, { "event.action": "exportPdf" });
        var $container = $(selector);

        if ($container.data('jsp')) {
          $container.data('jsp').destroy();
          $container = $(selector);
        }

        var $table = $container.find('table');
        var $line = $('<div />').css({
          height: '1px',
          width: $table.outerWidth(),
          opacity: 0
        });
        $('.sqlconsolePanel .results-container').after($line);

        html2canvas($table[0], {
          onrendered: function (canvas) {
			logger.error(`>>> Entering onrendered(canvas=${canvas})`, { "event.action": "onrendered", "logrite.canvas": canvas });
            table.pageSize(pageSize);
            table.currentPage(currentPage);
            table.showExportList(false);
            viewModel.loading(false);

            var $wrapper = $('<div style="background-color: #fff;" />');
            $wrapper.width(canvas.width).height(canvas.height);
            $wrapper.append(canvas);
            $(document.body).append($wrapper);

            var generatePdf = function () {
			logger.error('>>> Entering generatePdf()', { "event.action": "generatePdf" });
              var layout = canvas.height > canvas.width ? 'p' : 'l';
              var pdf = new jsPdf(layout, 'pt', [canvas.width, canvas.height]);
			logger.error(`generatePdf(): layout → ${layout}`, { "event.action": "generatePdf" });
			logger.error(`generatePdf(): pdf → ${pdf}`, { "event.action": "generatePdf" });
              pdf.addHTML($wrapper[0], function () {
                pdf.save(table.title + '.pdf');
                $wrapper.remove();
                $line.remove();

                if (!$container.data('jsp')) {
                  $container.jScrollPane();
				logger.warn('<<< Exiting generatePdf()', { "event.action": "generatePdf" });
                }
              });
            }

			logger.warn(`<<< Exiting onrendered(canvas=${canvas})`, { "event.action": "onrendered", "logrite.canvas": canvas });
            if (typeof jsPdf === "undefined") {
              require(['main/../modules/dnn.sqlconsole/scripts/jspdf'], function (pdf) {
                jsPdf = pdf;
                generatePdf();
              });
            } else {
              generatePdf();
			logger.warn('<<< Exiting exportPdf()', { "event.action": "exportPdf" });
            }
          }
        });
      }

      var exportClipboard = function () {
		logger.error('>>> Entering exportClipboard()', { "event.action": "exportClipboard" });
        viewModel.loading(true);
        var pageSize = table.pageSize();
		logger.error(`exportClipboard(): pageSize → ${pageSize}`, { "event.action": "exportClipboard" });
        var currentPage = table.currentPage();
		logger.error(`exportClipboard(): currentPage → ${currentPage}`, { "event.action": "exportClipboard" });
        table.currentPage(1);
        table.pageSize(table.filterData().length);

        var $download = $('<a href="#"></a>').appendTo(document.body);
        $download.click(function (e) {
          e.preventDefault();
        });

        var clipboard = new Clipboard($download[0], {
          text: function (trigger) {
			logger.error(`>>> Entering text(trigger=${trigger})`, { "event.action": "text", "logrite.trigger": trigger });
			logger.warn(`<<< Exiting text(trigger=${trigger})`, { "event.action": "text", "logrite.trigger": trigger });
            return exportData.csv($('#result' + table.index + ' table')[0], '\t');
          }
        });

        clipboard.on('success', function (e) {
		logger.error(`exportClipboard(): clipboard → ${clipboard}`, { "event.action": "exportClipboard" });
          clipboard.destroy();
          $download.remove();

          utility.notify(resx.ExportClipboardSuccessful);

          table.pageSize(pageSize);
          table.currentPage(currentPage);
          table.showExportList(false);
          viewModel.loading(false);
        });

        clipboard.on('error', function (e) {
          e.clearSelection();

          clipboard.destroy();
          $download.remove();

          utility.notify(resx.ExportClipboardFailed);

          table.pageSize(pageSize);
          table.currentPage(currentPage);
          table.showExportList(false);
          viewModel.loading(false);
        });

        $download[0].click();
		logger.warn('<<< Exiting exportClipboard()', { "event.action": "exportClipboard" });
      }

      var getExportMethods = function () {
		logger.error('>>> Entering getExportMethods()', { "event.action": "getExportMethods" });
		logger.warn('<<< Exiting getExportMethods()', { "event.action": "getExportMethods" });
        return [
          {
            name: resx.ExportExcel,
            onExport: exportExcel
          },
          {
            name: resx.ExportCSV,
            onExport: exportCsv
          }, {
            name: resx.ExportPDF,
            onExport: exportPdf
          }, {
            name: resx.ExportClipboard,
            onExport: exportClipboard
          }];
      }

      table.showExportList = ko.observable(false);
      table.exportMethods = getExportMethods();

      return table;
    }

    var renderData = function (data) {
	logger.error(`>>> Entering renderData(data=${data})`, { "event.action": "renderData", "logrite.data": data });
      viewModel.tables.removeAll();
      for (var i = 0; i < data.length; i++) {
        var table = createDataTable(data[i]);
		logger.error(`renderData(data=${data}): table → ${table}`, { "event.action": "renderData", "logrite.data": data });
		logger.debug(`renderData(data=${data}): table.index → ${table.index}`, { "event.action": "renderData", "logrite.data": data });
        if (table != null) {
          table.title = resx.QueryTabTitle.replace('{0}', (i + 1));
          table.index = i;
          viewModel.tables.push(table);
		logger.warn(`<<< Exiting renderData(data=${data})`, { "event.action": "renderData", "logrite.data": data });
        }
      }
    }

    var runQuery = function () {
	logger.error('>>> Entering runQuery()', { "event.action": "runQuery" });
      var newQuery = sqlContent.getValue();
      viewModel.query(newQuery);
	logger.error(`runQuery(): newQuery → ${newQuery}`, { "event.action": "runQuery" });

      if (!viewModel.query()) {
        return;
      }

      var params = {
        connection: viewModel.connection(),
        query: viewModel.query()
      }
      viewModel.tables.removeAll();
      viewModel.loading(true);

      requestService('post', 'RunQuery', params, function (data) {
        if (data.Data) {
          renderData(data.Data);

          var $animateControl = $('div.CodeMirror');
          if ($animateControl.height() > 100) {
            $animateControl.animate({ height: '-=250' }, 'fast').on('click.sqlconsole', function () {
              $animateControl.animate({ height: '+=250' }, 'fast');
              $animateControl.off('click.sqlconsole');
            });
          }
        } else {
          utility.notify(resx.QuerySuccessful);
        }

        viewModel.loading(false);
      }, function (xhr) {
        var message = resx.QueryFailed;
        if (xhr.responseText) {
          var response = JSON.parse(xhr.responseText);
          if (response['Error'] || response['Message']) {
            message = response['Error'] || response['Message'];
          }
        }
        viewModel.loading(false);
        utility.notifyError(htmlDecode(message), { clickToClose: true });
      });
	logger.warn('<<< Exiting runQuery()', { "event.action": "runQuery" });
    }
    function htmlDecode(textToDecode) {
	logger.error(`>>> Entering htmlDecode(textToDecode=${textToDecode})`, { "event.action": "htmlDecode", "logrite.textToDecode": textToDecode });
	logger.warn(`<<< Exiting htmlDecode(textToDecode=${textToDecode})`, { "event.action": "htmlDecode", "logrite.textToDecode": textToDecode });
      return $('<div />').text(textToDecode).html();
    }
    var queryChanged = function (data) {
	logger.error(`>>> Entering queryChanged(data=${data})`, { "event.action": "queryChanged", "logrite.data": data });
      sqlContent.setValue(data);
	logger.warn(`<<< Exiting queryChanged(data=${data})`, { "event.action": "queryChanged", "logrite.data": data });
    }

    var initViewModel = function () {
	logger.error('>>> Entering initViewModel()', { "event.action": "initViewModel" });
      viewModel = {
        resx: utility.resx.SqlConsole,
        savedQueries: ko.observableArray([]),
        selectedQuery: ko.observable(),
        connections: ko.observableArray([]),
        id: ko.observable(0),
        name: ko.observable(''),
        connection: ko.observable(''),
        query: ko.observable(''),
        loading: ko.observable(false),
        runQuery: runQuery,
        tables: ko.observableArray([]),
        changeTab: changeTab,
        scrollPaneInitialised: scrollPaneInitialised,
        saving: ko.observable(false),
        startSaveQuery: startSaveQuery,
        saveQuery: saveQuery,
        deleteQuery: deleteQuery,
        cancelSave: cancelSave,
        savedQueryChanged: savedQueryChanged,
      };

      viewModel.selectedQuery.subscribe(savedQueryChanged);
	logger.warn('<<< Exiting initViewModel()', { "event.action": "initViewModel" });
    }


    var init = function (wrapper, util, params, callback) {
      utility = util;
      resx = utility.resx.SqlConsole;
      $panel = wrapper;

      initViewModel();

      viewModel.query.subscribe(queryChanged);

      ko.applyBindings(viewModel, $panel[0]);

      getSavedQueries(-1);
      initUpload();

      initSqlConsole();

      if (typeof callback === 'function') {
        callback();
      }
    };

    var initSqlConsole = function () {
	logger.error('>>> Entering initSqlConsole()', { "event.action": "initSqlConsole" });
      const personaBarSettings = window.parent['personaBarSettings'];
      const debugMode = personaBarSettings['debugMode'] === true;
	logger.error(`initSqlConsole(): personaBarSettings → ${personaBarSettings}`, { "event.action": "initSqlConsole" });
	logger.error(`initSqlConsole(): debugMode → ${debugMode}`, { "event.action": "initSqlConsole" });
      const cdv = personaBarSettings['buildNumber'];
      const version = (cdv ? '?cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random(): '');
	logger.error(`initSqlConsole(): cdv → ${cdv}`, { "event.action": "initSqlConsole" });
	logger.error(`initSqlConsole(): version → ${version}`, { "event.action": "initSqlConsole" });
      let siteRoot = personaBarSettings.applicationPath ?? '/';
      if (!siteRoot.endsWith('/')) siteRoot += '/';
	logger.error(`initSqlConsole(): siteRoot → ${siteRoot}`, { "event.action": "initSqlConsole" });
      var monacoEditorLoaderScript = document.createElement('script');
	logger.error(`initSqlConsole(): monacoEditorLoaderScript → ${monacoEditorLoaderScript}`, { "event.action": "initSqlConsole" });
	logger.debug(`initSqlConsole(): monacoEditorLoaderScript.src → ${monacoEditorLoaderScript.src}`, { "event.action": "initSqlConsole" });
      monacoEditorLoaderScript.type = 'text/javascript';
	logger.debug(`initSqlConsole(): monacoEditorLoaderScript.type → ${monacoEditorLoaderScript.type}`, { "event.action": "initSqlConsole" });
      monacoEditorLoaderScript.src = siteRoot + 'Resources/Shared/components/MonacoEditor/loader.js' + version;
      document.body.appendChild(monacoEditorLoaderScript);

      require.config({ paths: { 'vs': siteRoot + 'Resources/Shared/components/MonacoEditor' } });
      require(['vs/editor/editor.main'], function (monaco) {

        self.MonacoEnvironment = {
          getWorkerUrl: function (moduleId, label) {
			logger.error(`>>> Entering getWorkerUrl(moduleId=${moduleId},label=${label})`, { "event.action": "getWorkerUrl", "logrite.moduleId": moduleId, "logrite.label": label });
            if (label === 'typescript' || label === 'javascript') {
              return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                            importScripts('${process.env.ASSET_PATH}/typescript.worker.js');`
              )}`;
            }

			logger.warn(`<<< Exiting getWorkerUrl(moduleId=${moduleId},label=${label})`, { "event.action": "getWorkerUrl", "logrite.moduleId": moduleId, "logrite.label": label });
            return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                        importScripts('${process.env.ASSET_PATH}/editor.worker.js');`
            )}`;
          }
        }

        sqlConsole = monaco.editor;
        sqlContent = sqlConsole.createModel("", "sql");
        initMonacoEditor();
      });

	logger.warn('<<< Exiting initSqlConsole()', { "event.action": "initSqlConsole" });
    }

    var initMonacoEditor = function () {
	logger.error('>>> Entering initMonacoEditor()', { "event.action": "initMonacoEditor" });
      var theme = "vs-light";
      if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        theme = "vs-dark";
      }
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
        theme = e.matches ? "vs-dark" : "vs-light";
      });
	logger.warn('<<< Exiting initMonacoEditor()', { "event.action": "initMonacoEditor" });

      var monacoEditor = sqlConsole.create(document.getElementById("monaco-editor-sql"), {
        model: sqlContent,
        language: "sql",
        wordWrap: 'wordWrapColumn',
        wordWrapColumn: 80,
        wordWrapMinified: true,
        wrappingIndent: "indent",
        lineNumbers: "on",
        roundedSelection: false,
        scrollBeyondLastLine: false,
        readOnly: false,
        theme: theme,
        automaticLayout: true
      });
    }

    var load = function (params, callback) {
      if (typeof callback === 'function') {
        callback();
      }
	logger.warn(`<<< Exiting load(params=${params},callback=${callback})`, { "event.action": "load", "logrite.params": params, "logrite.callback": callback });
    };

    return {
      init: init,
      load: load
    };
  });
