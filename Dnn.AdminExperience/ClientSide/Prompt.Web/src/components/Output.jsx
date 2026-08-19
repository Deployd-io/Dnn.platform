import React, {Component} from "react";
import PropTypes from "prop-types";
import "../components/Prompt.less";
import DataTable from "../components/DataTable";
import Command from "../components/Command";
import TextLine from "../components/TextLine";
import Help from "../components/Help";
import { renderObject } from "../utils/helpers";
import DomKey from "../services/DomKey";
const logger = console;

export class Output extends Component {

    renderResults() {
		logger.debug('>>> Entering renderResults()');
        const {props} = this;
        const {fieldOrder} = props;

        props.IsPaging(false);

        if (props.isHelp) {
            if (props.commandList !== null && props.commandList.length > 0) {
                return <Command key={DomKey.get("output")} {...props} commandList={props.commandList} IsPaging={props.IsPaging}/>;
            } else {
                return <Help key={DomKey.get("output")} {...props} IsPaging={props.IsPaging} style={props.style} isError={props.isError}
                    name={props.name}/>;
            }
        }

        if (props.reload) {
            if (props.output !== null && props.output !== "" && props.output.toLowerCase().indexOf("http") >= 0) {
                window.top.location.href = props.output;
            } else {
                location.reload(true);
            }
        }
        else if (props.data) {
            const result = [];
            if (props.paging !== null && props.paging !== undefined) {
                props.IsPaging(props.paging.pageNo < props.paging.totalPages);
            }
            result.push(this.renderData(props.data, fieldOrder));
            result.push(<TextLine key={DomKey.get("output")} txt={props.output} css="dnn-prompt-ok"/>);

            return result;
        }
        else if (props.output) {
            const style = props.isHtml ? "" : props.isError ? "dnn-prompt-error" : `dnn-prompt-${props.style === "cmd" ? "cmd" : "ok"}`;
			logger.debug('<<< Exiting renderResults()');
            return <TextLine key={DomKey.get("output")} txt={props.output} css={style} isHtml={props.isHtml}/>;
        }
    }

    renderData(data, fieldOrder) {
		logger.debug(`>>> Entering renderData(data=${data},fieldOrder=${fieldOrder})`);
        if (data.length > 1) {
            return <DataTable rows={data} columns={fieldOrder} cssClass=""/>;
        } else if (data.length === 1) {
		logger.debug(`<<< Exiting renderData(data=${data},fieldOrder=${fieldOrder})`);
            return renderObject(data[0], fieldOrder);
        }
        return <br />;
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        this.output = !props.clearOutput && this.output ? this.output : [];
        const out = this.renderResults();
		logger.debug(`render(): this.output → ${this.output}`);
		logger.debug(`render(): out → ${out}`);
        this.output.push(out);
		logger.debug('<<< Exiting render()');
        return (
            <div className="dnn-prompt-output">
                {!props.clearOutput && this.output}
            </div>
        );
    }
}

Output.propTypes = {
    output: PropTypes.string,
    data: PropTypes.array,
    paging: PropTypes.object,
    isHtml: PropTypes.bool,
    reload: PropTypes.bool,
    isError: PropTypes.bool,
    clearOutput: PropTypes.bool,
    fieldOrder: PropTypes.array,
    commandList: PropTypes.array,
    style: PropTypes.string,
    isHelp: PropTypes.bool,
    name: PropTypes.string,
    description: PropTypes.string,
    options: PropTypes.array,
    resultHtml: PropTypes.string,
    error: PropTypes.string,
    IsPaging: PropTypes.func.isRequired
};

export default Output;