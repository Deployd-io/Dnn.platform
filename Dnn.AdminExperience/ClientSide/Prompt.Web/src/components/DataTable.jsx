import React from "react";
import PropTypes from "prop-types";
import {formatLabel, getColumnsFromRow, renderObjectFieldCell} from "../utils/helpers";
import DomKey from "../services/DomKey";
const logger = console;

const DataTable = ({rows, columns, cssClass}) => {
logger.debug(`>>> Entering DataTable(rows=${rows},columns=${columns},cssClass=${cssClass})`);

    const renderTableHeader = (columns) => {
        const tableCols = columns.map((col) => <th key={DomKey.get("datatable")}>{formatLabel(col)}</th>);
		logger.debug(`renderTableHeader(columns=${columns}): tableCols → ${tableCols}`);
        return (
            <thead>
                <tr key={DomKey.get("datatable")}>
                    {tableCols}
                </tr>
            </thead>
        );
    };

    const renderTableRows = (rows, columns) => {
		logger.debug(`>>> Entering renderTableRows(rows=${rows},columns=${columns})`);
		logger.debug(`<<< Exiting renderTableRows(rows=${rows},columns=${columns})`);
        return rows.map((row) => 
            <tr key={DomKey.get("datatable")}>
                {columns.map((fieldName) => renderObjectFieldCell(row, fieldName, DomKey.get("datatable")))}
            </tr>
        );
    };

    const renderTable = (columns) => {
		logger.debug(`>>> Entering renderTable(columns=${columns})`);
        if (!rows || !rows.length) { return; }

        columns = columns ? columns : getColumnsFromRow(rows[0]);

		logger.debug(`renderTable(columns=${columns}): columns → ${columns}`);
		logger.debug(`<<< Exiting renderTable(columns=${columns})`);
	logger.debug(`<<< Exiting DataTable(rows=${rows},columns=${columns},cssClass=${cssClass})`);
        return (
            <table key={DomKey.get("datatable")} className={cssClass ? cssClass : "dnn-prompt-tbl"}>
                {renderTableHeader(columns, cssClass)}
                <tbody>
                    {renderTableRows(rows, columns)}
                </tbody>
            </table>
        );
    };

    return renderTable(columns);
};

DataTable.propTypes = {
    rows: PropTypes.array.isRequired,
    columns: PropTypes.array.isRequired,
    cssClass: PropTypes.string.isRequired
};

export default DataTable;