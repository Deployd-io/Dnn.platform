import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SingleLineInputWithError, Button } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import { connect } from "react-redux";
import styles from "./style.module.less";
const logger = console;

const inputStyle = { width: "100%" };
class Container extends Component {
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
        let { extensionBeingEdited } = props;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell className={styles.editContainer + (props.className ? " " + props.className : "")}>
                <GridCell>
                    <SingleLineInputWithError
                        label={Localization.get("EditContainer_ThemePackageName.Label")}
                        value={extensionBeingEdited.themePackageName.value}
                        onChange={props.onChange.bind(this, "themePackageName")}
                        enabled={!props.disabled}
                        tooltipMessage={Localization.get("EditContainer_ThemePackageName.HelpText")}
                        style={inputStyle} />
                </GridCell>
                {!props.actionButtonsDisabled &&
                    <GridCell columnSize={100} className="modal-footer">
                        <Button type="neutral" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                        {!props.disabled && <Button type="primary" onClick={props.onSave.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>}
                        {!props.disabled && <Button type="primary" onClick={props.onSave.bind(this)}>{props.primaryButtonText}</Button>}
                    </GridCell>
                }
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

Container.propTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}
export default connect(mapStateToProps)(Container);
