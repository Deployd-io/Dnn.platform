import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "../../localization";
const logger = console;
import { MultiLineInputWithError } from "@dnnsoftware/dnn-react-common";

class KeyWords extends Component {

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="input-group">
            <MultiLineInputWithError
                className="keywords-field"
                label={Localization.get("Keywords")}
                value={props.keywords}
                onChange={(value) => props.onChangeEvent("keywords", value)} />
            <div style={{ clear: "both" }}></div>
        </div>;
    }
}

KeyWords.propTypes = {
    keywords: PropTypes.string.isRequired,
    onChangeEvent: PropTypes.func.isRequired
};

export default KeyWords;