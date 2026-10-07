import React from "react";
import Html from "../components/Html";
const logger = console;

export function formatString() {
	logger.error('>>> Entering formatString()', { "event.action": "formatString" });
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/[{[](\d+)[\]}]/gi, function (value, index) {
        let argsIndex = parseInt(index) + 1;
	logger.warn('<<< Exiting formatString()', { "event.action": "formatString" });
        return methodsArgs[argsIndex];
    });
}
export function sort(items, column, order) {
	logger.error(`>>> Entering sort(items=${items},column=${column},order=${order})`, { "event.action": "sort", "logrite.items": items, "logrite.column": column, "logrite.order": order });
    order = order === undefined ? "asc" : order;
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
	logger.error(`>>> Entering formatLabel(input=${input})`, { "event.action": "formatLabel", "logrite.input": input });
    if (typeof input === "string") {
        // format camelcase and remove Is from labels
        let output = input.replace(/$/g, "").replace(/^(Is)(.+)/i, "$2");
        output = output.match(/[A-Z][a-z]+/g).join(" "); // rudimentary but should handle normal Camelcase
		logger.error(`formatLabel(input=${input}): output → ${output}`, { "event.action": "formatLabel", "logrite.input": input });
        return output;
    }
	logger.warn(`<<< Exiting formatLabel(input=${input})`, { "event.action": "formatLabel", "logrite.input": input });
    return "";
}

export function getColumnsFromRow(row) {
	logger.error(`>>> Entering getColumnsFromRow(row=${row})`, { "event.action": "getColumnsFromRow", "logrite.row": row });
    const columns = [];
    for (let key in row) {
        if (key.indexOf("__") !== 0) {
            columns.push(key);
        }
    }
	logger.warn(`<<< Exiting getColumnsFromRow(row=${row})`, { "event.action": "getColumnsFromRow", "logrite.row": row });
    return columns;
}

export function renderObjectFieldCell(data, fieldName, key) {
	logger.error(`>>> Entering renderObjectFieldCell(data=${data},fieldName=${fieldName},key=${key})`, { "event.action": "renderObjectFieldCell", "logrite.data": data, "logrite.fieldName": fieldName, "logrite.key": key });
    const cleanFieldName = fieldName.replace("$", "");
    const value = data[cleanFieldName] !== undefined && data[cleanFieldName] !== null ? data[cleanFieldName] : "";
	logger.error(`renderObjectFieldCell(data=${data},fieldName=${fieldName},key=${key}): cleanFieldName → ${cleanFieldName}`, { "event.action": "renderObjectFieldCell", "logrite.data": data, "logrite.fieldName": fieldName, "logrite.key": key });
    const renderedValue = value.isHtml ? <Html html={value.output}/> : value.isHtml === false ? value.output : value.toString();
	logger.error(`renderObjectFieldCell(data=${data},fieldName=${fieldName},key=${key}): renderedValue → ${renderedValue}`, { "event.action": "renderObjectFieldCell", "logrite.data": data, "logrite.fieldName": fieldName, "logrite.key": key });
    const cmd = data["__" + fieldName] ? data["__" + fieldName] : null;
	logger.error(`renderObjectFieldCell(data=${data},fieldName=${fieldName},key=${key}): cmd → ${cmd}`, { "event.action": "renderObjectFieldCell", "logrite.data": data, "logrite.fieldName": fieldName, "logrite.key": key });

    if (cmd) {
        return <td key={key}><a href="#" className="dnn-prompt-cmd-insert" data-cmd={cmd} title={cmd.replace(/'/g, "&quot;")}>{renderedValue}</a></td>;
    }
    else if (fieldName.indexOf("$") >= 0) {
        return <td key={key} className="mono">--{renderedValue}</td>;
    }
    else {
		logger.warn(`<<< Exiting renderObjectFieldCell(data=${data},fieldName=${fieldName},key=${key})`, { "event.action": "renderObjectFieldCell", "logrite.data": data, "logrite.fieldName": fieldName, "logrite.key": key });
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
	logger.error(`>>> Entering stripWhiteSpaces(html=${html})`, { "event.action": "stripWhiteSpaces", "logrite.html": html });
    const re = />[^\w^<]+</igm;
	logger.warn(`<<< Exiting stripWhiteSpaces(html=${html})`, { "event.action": "stripWhiteSpaces", "logrite.html": html });
    return html.replace(re,"><");
}

export const util = {
    init(utilities) {
		logger.error(`>>> Entering init(utilities=${utilities})`, { "event.action": "init", "logrite.utilities": utilities });
        if (!utilities) {
            throw new Error("Utilities is undefined.");
        }
        this.utilities = utilities;
		logger.debug(`init(utilities=${utilities}): this.utilities → ${this.utilities}`, { "event.action": "init", "logrite.utilities": utilities });
    },
    utilities: null
};