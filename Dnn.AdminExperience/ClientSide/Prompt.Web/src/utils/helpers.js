import React from "react";
import Html from "../components/Html";
const logger = console;

export function formatString() {
	logger.debug('>>> Entering formatString()');
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/[{[](\d+)[\]}]/gi, function (value, index) {
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
        let output = input.replace(/$/g, "").replace(/^(Is)(.+)/i, "$2");
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

export function renderObjectFieldCell(data, fieldName, key) {
	logger.debug(`>>> Entering renderObjectFieldCell(data=${data},fieldName=${fieldName},key=${key})`);
    const cleanFieldName = fieldName.replace("$", "");
    const value = data[cleanFieldName] !== undefined && data[cleanFieldName] !== null ? data[cleanFieldName] : "";
	logger.debug(`renderObjectFieldCell(data=${data},fieldName=${fieldName},key=${key}): cleanFieldName → ${cleanFieldName}`);
    const renderedValue = value.isHtml ? <Html html={value.output}/> : value.isHtml === false ? value.output : value.toString();
	logger.debug(`renderObjectFieldCell(data=${data},fieldName=${fieldName},key=${key}): renderedValue → ${renderedValue}`);
    const cmd = data["__" + fieldName] ? data["__" + fieldName] : null;
	logger.debug(`renderObjectFieldCell(data=${data},fieldName=${fieldName},key=${key}): cmd → ${cmd}`);

    if (cmd) {
        return <td key={key}><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, "&quot;")}>{renderedValue}</a></td>;
    }
    else if (fieldName.indexOf("$") >= 0) {
        return <td key={key} className="mono">--{renderedValue}</td>;
    }
    else {
		logger.debug(`<<< Exiting renderObjectFieldCell(data=${data},fieldName=${fieldName},key=${key})`);
        return <td key={key}>{renderedValue}</td>;
    }
}

export function renderObject(data, fieldOrder) {

    const columns = !fieldOrder || fieldOrder.length === 0 ? getColumnsFromRow(data) : fieldOrder;
    const rows = columns.map((fieldName, index) => {
        return <tr key={index}>
            <td className="dnn-prompt-lbl">{formatLabel(fieldName)}</td>
            <td>:</td>
            {renderObjectFieldCell(data, fieldName)}
        </tr>;
    });
    return <table className="dnn-prompt-tbl"><tbody>{rows}</tbody></table>;
}

export function stripWhiteSpaces(html = "") {
	logger.debug(`>>> Entering stripWhiteSpaces(html=${html})`);
    const re = />[^\w^<]+</igm;
	logger.debug(`<<< Exiting stripWhiteSpaces(html=${html})`);
    return html.replace(re,"><");
}

export const util = {
    init(utilities) {
		logger.debug(`>>> Entering init(utilities=${utilities})`);
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;
		logger.debug(`init(utilities=${utilities}): this.utilities → ${this.utilities}`);
		logger.debug(`<<< Exiting init(utilities=${utilities})`);
    },
    utilities: null
};