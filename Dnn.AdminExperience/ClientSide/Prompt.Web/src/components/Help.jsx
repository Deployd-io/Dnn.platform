import React from "react";
import PropTypes from "prop-types";
import Html from "./Html";
import Localization from "../localization/Localization";
import DataTable from "../components/DataTable";
import TextLine from "../components/TextLine";
import { stripWhiteSpaces } from "../utils/helpers";
import DomKey from "../services/DomKey";
const logger = console;

const Help = ({ style, isError, error, name, description, options, IsPaging, resultHtml }) => {
logger.debug(`>>> Entering Help(style=${style},isError=${isError},error=${error},name=${name},description=${description},options=${options},IsPaging=${IsPaging},resultHtml=${resultHtml})`);

    IsPaging(false);

    const css = style ? style : isError ? "dnn-prompt-error" : "dnn-prompt-ok";
	logger.debug(`Help(style=${style},isError=${isError},error=${error},name=${name},description=${description},options=${options},IsPaging=${IsPaging},resultHtml=${resultHtml}): css → ${css}`);
    if (isError) {
        return <TextLine txt={error} css={css}/>;
    }

    const headingName = <h3 className="mono">{name}</h3>;
    const anchorName = <a name={name}></a>;
    const paragraphDescription = <p className="lead">{description}</p>;
    const fields = ["$Flag", "Type", "Required", "Default", "Description"];
	logger.debug(`Help(style=${style},isError=${isError},error=${error},name=${name},description=${description},options=${options},IsPaging=${IsPaging},resultHtml=${resultHtml}): fields → ${fields}`);
    const out = (<section key={DomKey.get("help")} className="dnn-prompt-inline-help">
        {anchorName}
        {headingName}
        {paragraphDescription}
        {options && options.length > 0 && <h4>{Localization.get("Help_Options")}</h4>}
        {options && options.length > 0 && <div><DataTable rows={options} columns={fields} cssClass="table" /></div>}
        {resultHtml && <div><Html html={stripWhiteSpaces(resultHtml)}/></div>}
    </section>);

	logger.debug(`Help(style=${style},isError=${isError},error=${error},name=${name},description=${description},options=${options},IsPaging=${IsPaging},resultHtml=${resultHtml}): out → ${out}`);
	logger.debug(`<<< Exiting Help(style=${style},isError=${isError},error=${error},name=${name},description=${description},options=${options},IsPaging=${IsPaging},resultHtml=${resultHtml})`);
    return out;
};

Help.propTypes = {
    IsPaging: PropTypes.func.isRequired,
    style: PropTypes.string.isRequired,
    isError: PropTypes.bool.isRequired,
    error: PropTypes.string,
    name: PropTypes.string.isRequired,
    description: PropTypes.string,
    options: PropTypes.array,
    resultHtml: PropTypes.string
};

Help.defaultProps = {
    isError: false
};

export default Help;