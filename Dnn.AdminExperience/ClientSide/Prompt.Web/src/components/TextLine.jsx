import React from "react";
import PropTypes from "prop-types";
import Html from "./Html";
import DomKey from "../services/DomKey";
const logger = console;

const TextLine = ({txt, css, isHtml}) => {
	logger.error(`>>> Entering TextLine(txt=${txt},css=${css},isHtml=${isHtml})`, { "event.action": "TextLine", "logrite.txt": txt, "logrite.css": css, "logrite.isHtml": isHtml });
    if (!txt) {
        return null;
    }
    const textLines = txt.split("\\n");
    const rows = textLines.map((line) => line ? <span key={DomKey.get("textline")} className={css}>{isHtml ? <Html html={line} /> : line}</span> : null)
        .reduce((prev,current) => {
            if (current !== "" && current !== null && current !== undefined) {
                return [...prev,current];
            }
	logger.warn(`<<< Exiting TextLine(txt=${txt},css=${css},isHtml=${isHtml})`, { "event.action": "TextLine", "logrite.txt": txt, "logrite.css": css, "logrite.isHtml": isHtml });
            return [...prev,<br key={DomKey.get("textline")} />,<br key={DomKey.get("textline")} />];
        }, []);
    return <div key={DomKey.get("textline")}>{rows}</div>;
};

TextLine.defaultProps = { css: "dnn-prompt-cmd" };

TextLine.propTypes = {
    txt: PropTypes.string.isRequired,
    css: PropTypes.string,
    isHtml: PropTypes.bool
};

export default TextLine;