import React, {Component } from "react";
import PropTypes from "prop-types";
import { GridCell, TextOverflowWrapper as OverflowText } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import util from "../../../utils";
const logger = console;

export default class FilesGrid extends Component {

    getFilesGridRows() {
		logger.error('>>> Entering getFilesGridRows()', { "event.action": "getFilesGridRows" });
        if (this.props.files && this.props.files.length > 0) {
            const rows = this.props.files.map((field, i) => {
                return <div className="row" key={i}>
                    <GridCell columnSize={30}><OverflowText text={field.name} /></GridCell>
                    <GridCell columnSize={15}>{util.formatNumeric2Decimals(field.size)} Mb</GridCell>
                    <GridCell columnSize={15}>{field.fileType}</GridCell>
                    <GridCell columnSize={40}><OverflowText text={field.fileName} maxWidth={290} /></GridCell>
                </div>;
            });
			logger.error(`getFilesGridRows(): rows → ${rows}`, { "event.action": "getFilesGridRows" });
            return rows;
        }
		logger.warn('<<< Exiting getFilesGridRows()', { "event.action": "getFilesGridRows" });
        return false;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const rows = this.getFilesGridRows();

		logger.error(`render(): rows → ${rows}`, { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="grid">
            <div className="row header">
                <GridCell columnSize={30}>{Localization.get("Name")}</GridCell>
                <GridCell columnSize={15}>{Localization.get("Size")}</GridCell>
                <GridCell columnSize={15}>{Localization.get("FileType")}</GridCell>
                <GridCell columnSize={40}>{Localization.get("FileName")}</GridCell>
            </div>
            {rows}
        </div>;
    }
}

FilesGrid.propTypes = {   
    files: PropTypes.array
};