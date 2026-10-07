var logger = (typeof logger !== 'undefined' && logger) || console;
define([], function () {
    "use strict";
    var template = { excel: '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>' };
    var csvDelimiter = ",";
    var csvNewLine = "\r\n";
    var format = function (s, c) {
		logger.error(`>>> Entering format(s=${s},c=${c})`, { "event.action": "format", "logrite.s": s, "logrite.c": c });
        return s.replace(new RegExp("{(\\w+)}", "g"), function (m, p) {
		logger.warn(`<<< Exiting format(s=${s},c=${c})`, { "event.action": "format", "logrite.s": s, "logrite.c": c });
            return c[p];
        });
    };

    var fixCsvField = function (value) {
		logger.error(`>>> Entering fixCsvField(value=${value})`, { "event.action": "fixCsvField", "logrite.value": value });
        var fixedValue = value;
        var addQuotes = (value.indexOf(csvDelimiter) !== -1) || (value.indexOf('\r') !== -1) || (value.indexOf('\n') !== -1);
		logger.error(`fixCsvField(value=${value}): addQuotes → ${addQuotes}`, { "event.action": "fixCsvField", "logrite.value": value });
        var replaceDoubleQuotes = (value.indexOf('"') !== -1);
		logger.error(`fixCsvField(value=${value}): replaceDoubleQuotes → ${replaceDoubleQuotes}`, { "event.action": "fixCsvField", "logrite.value": value });

        if (replaceDoubleQuotes) {
            fixedValue = fixedValue.replace(/"/g, '""');
        }
        if (addQuotes || replaceDoubleQuotes) {
            fixedValue = '"' + fixedValue + '"';
        }
		logger.error(`fixCsvField(value=${value}): fixedValue → ${fixedValue}`, { "event.action": "fixCsvField", "logrite.value": value });
        return fixedValue;
    };

    var tableToCsv = function (table) {
		logger.error(`>>> Entering tableToCsv(table=${table})`, { "event.action": "tableToCsv", "logrite.table": table });
        var data = "";
        var i, j, row, col;
        for (i = 0; i < table.rows.length; i++) {
            row = table.rows[i];
            for (j = 0; j < row.cells.length; j++) {
                col = row.cells[j];
                data = data + (j ? csvDelimiter : '') + fixCsvField(col.textContent.trim());
            }
            data = data + csvNewLine;
        }
		logger.error(`tableToCsv(table=${table}): data → ${data}`, { "event.action": "tableToCsv", "logrite.table": table });
        return data;
    };

    return {
        excel: function (table, name) {
			logger.error(`>>> Entering excel(table=${table},name=${name})`, { "event.action": "excel", "logrite.table": table, "logrite.name": name });
            var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML };
			logger.error(`excel(table=${table},name=${name}): ctx → ${ctx}`, { "event.action": "excel", "logrite.table": table, "logrite.name": name });
            return format(template.excel, ctx);
        },
        csv: function (table, delimiter, newLine) {
			logger.error(`>>> Entering csv(table=${table},delimiter=${delimiter},newLine=${newLine})`, { "event.action": "csv", "logrite.table": table, "logrite.delimiter": delimiter, "logrite.newLine": newLine });
            if (delimiter !== undefined && delimiter) {
                csvDelimiter = delimiter;
            }
            if (newLine !== undefined && newLine) {
                csvNewLine = newLine;
				logger.warn(`<<< Exiting csv(table=${table},delimiter=${delimiter},newLine=${newLine})`, { "event.action": "csv", "logrite.table": table, "logrite.delimiter": delimiter, "logrite.newLine": newLine });
            }
            return tableToCsv(table);
        }
    };
}());