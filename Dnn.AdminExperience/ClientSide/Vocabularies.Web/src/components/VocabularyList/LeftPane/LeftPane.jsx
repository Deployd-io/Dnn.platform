import React, {Component } from "react";
import PropTypes from "prop-types";
import { EditableField as EditableWithLabel} from "@dnnsoftware/dnn-react-common";
import LocalizedResources from "../../../resources";
import util from "utils";
import styles from "./style.module.less";
const logger = console;


class LeftPane extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }
    onEnter(key, value) {
		logger.debug(`>>> Entering onEnter(key=${key},value=${value})`);
        const {props} = this;
        props.onEnter(key, value, props.index);
		logger.debug(`<<< Exiting onEnter(key=${key},value=${value})`);
    }

    canEdit() {
		logger.debug('>>> Entering canEdit()');
        const {props} = this;
		logger.debug('<<< Exiting canEdit()');
        return util.isHost() || (props.scopeType === "Portal" && util.canEdit());
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
		logger.debug('<<< Exiting render()');
        return (
            <div className={styles.vocabulariesLeftPane}>
                <EditableWithLabel
                    label={LocalizedResources.get("Description") }
                    value={props.description}
                    onEnter={this.onEnter.bind(this, "Description") }
                    inputType="textArea"
                    editable={this.canEdit()}
                    helpText={LocalizedResources.get("EditFieldHelper")}
                />
                <EditableWithLabel
                    label={LocalizedResources.get("Type") }
                    value={LocalizedResources.get(props.type)}
                    onEnter={this.onEnter.bind(this, "Type") }
                    editable={false}
                />
                <EditableWithLabel
                    label={LocalizedResources.get("Scope") }
                    value={LocalizedResources.get(props.scopeType)}
                    onEnter={this.onEnter.bind(this, "ScopeType") }
                    editable={false}
                />
            </div>
        );
    }
}

LeftPane.propTypes = {
    description: PropTypes.string,
    type: PropTypes.string,
    scope: PropTypes.string,
    onEnter: PropTypes.func,
    index: PropTypes.number,
    scopeType: PropTypes.string
};

export default LeftPane;