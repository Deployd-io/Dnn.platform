import React  from "react";
import utilities from "./utilities";
const logger = console;

export function formatString() {
	logger.debug('>>> Entering formatString()');
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/[{\[](\d+)[\]}]/gi, function (value, index) {
        let argsIndex = parseInt(index) + 1;
		logger.debug(`<<< Exiting function(argsIndex=${argsIndex})`);
	logger.debug('<<< Exiting formatString()');
        return methodsArgs[argsIndex];
    });
}
export function sort(items, column, order) {
    order = order === undefined ? "asc" : order;
	logger.debug(`>>> Entering sort(order=${order})`);
    items = items.sort(function (a, b) {
        if (a[column] > b[column]) //sort string descending
            return order === "asc" ? 1 : -1;
        if (a[column] < b[column])
            return order === "asc" ? -1 : 1;
        return 0;//default return value (no sorting)
    });
    return items;
}

export function formatLabel(input) {
	logger.debug(`>>> Entering formatLabel(input=${input})`);
    if (typeof input === "string") {
        // format camelcase and remove Is from labels
        let output = input.replace("$", "").replace(/^(Is)(.+)/i, "$2");
        output = output.match(/[A-Z][a-z]+/g).join(" "); // rudimentary but should handle normal Camelcase
		logger.debug(`formatLabel(input=${input}): output → ${output}`);
	logger.debug(`<<< Exiting formatLabel(input=${input})`);
        return output;
    }
    return "";
}

export function getColumnsFromRow(row) {
	logger.debug(`>>> Entering getColumnsFromRow(row=${row})`);
    const columns = [];
    for (let key in row) {
        if (key.indexOf("__") !== 0) {
            columns.push(key);
        }
    }
	logger.debug(`<<< Exiting getColumnsFromRow(row=${row})`);
    return columns;
}

export function renderObject(data, fieldOrder) {
logger.debug(`>>> Entering renderObject(data=${data},fieldOrder=${fieldOrder})`);

    const columns = !fieldOrder || fieldOrder.length === 0 ? getColumnsFromRow(data) : fieldOrder;
    const rows = columns.map((fldName, index) => {
        const lbl = formatLabel(fldName);
        const fldVal = data[fldName] ? data[fldName] : "";
        const cmd = data["__" + fldName] ? data["__" + fldName] : null;

        if (cmd) {
            return <tr key={index}><td className="dnn-prompt-lbl">{lbl}</td><td>:</td><td><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, "&quot;")}>{fldVal}</a></td></tr>;
        } else {
            return <tr key={index}><td className="dnn-prompt-lbl">{lbl}</td><td>:</td><td>{fldVal}</td></tr>;
        }

    });
	logger.debug(`<<< Exiting renderObject(data=${data},fieldOrder=${fieldOrder})`);
    return <table className="dnn-prompt-tbl"><tbody>{rows}</tbody></table>;
}

export function stripWhiteSpaces(html = "") {
    const re = /\>[^\w^\<]+\</igm;
    return html.replace(re,"><");
}

export const util = {
    utilities
};