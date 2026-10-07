import React, { Component } from "react";
import PropTypes from "prop-types";
import TextArea from "../MultiLineInputWithError";
import Input from "../SingleLineInputWithError";
import { EditIcon } from "../SvgIcons";
import "./style.less";
const logger = console;

class EditableField extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            editMode: false,
            value: props.value,
            error: false
        };
        this.uniqueId = "editableField-" + (Date.now() * Math.random());
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }
    
    toggleEditMode() {
		logger.error('>>> Entering toggleEditMode()', { "event.action": "toggleEditMode" });
        this.setState({
            editMode: !this.state.editMode
        });
        if (!this.state.editMode) {
            this.editableInputRef.focus();
			logger.warn('<<< Exiting toggleEditMode()', { "event.action": "toggleEditMode" });
        }
    }

    getEditableLabelClass() {
		logger.error('>>> Entering getEditableLabelClass()', { "event.action": "getEditableLabelClass" });
        const { props, state } = this;

        let editableLabelClass = state.editMode ? " in-edit" : "";
		logger.error(`getEditableLabelClass(): editableLabelClass → ${editableLabelClass}`, { "event.action": "getEditableLabelClass" });
        let inputTypeClass = props.inputType === "textArea" ? " textArea" : " singleLine";
		logger.error(`getEditableLabelClass(): inputTypeClass → ${inputTypeClass}`, { "event.action": "getEditableLabelClass" });

		logger.warn('<<< Exiting getEditableLabelClass()', { "event.action": "getEditableLabelClass" });
        return "dnn-editable-field" + editableLabelClass + inputTypeClass;
    }

    onFocus() {
        window.dnn.stopEscapeFromClosingPB = true;
        if (typeof this.props.onFocus === "function") {
            this.props.onFocus();
        }
    }
    getInputFromType() {
		logger.error('>>> Entering getInputFromType()', { "event.action": "getInputFromType" });
        const { props, state } = this;
        if (props.inputType && props.inputType === "textArea") {
            return <TextArea
                value={state.value}
                onKeyDown={this.onKeyDown.bind(this)}
                onChange={this.onKeyUp.bind(this)}
                onBlur={this.onBlur.bind(this)}
                onFocus={this.onFocus.bind(this)}
                inputRef={node => this.editableInputRef = node}
                enabled={state.editMode}
                error={state.error}
                errorMessage={props.errorMessage}
            />;
        } else {
			logger.warn('<<< Exiting getInputFromType()', { "event.action": "getInputFromType" });
            return <Input
                value={state.value}
                onKeyDown={this.onKeyDown.bind(this)}
                onChange={this.onKeyUp.bind(this)}
                onBlur={this.onBlur.bind(this)}
                onFocus={this.onFocus.bind(this)}
                inputRef={node => this.editableInputRef = node}
                enabled={state.editMode}
                error={state.error}
                errorMessage={props.errorMessage}
            />;
        }
    }

    onKeyDown(event) {
        const { props, state } = this;
        if (typeof this.props.onKeyDown === "function") {
            this.props.onKeyDown(event);
        }
        switch (event.keyCode) {
            case 13:
                event.preventDefault();
                if (props.enableCallback) {
                    props.onEnter(state.value, () => {
                        this.setState({
                            editMode: false,
                            error: false
                        });
                        this.editableInputRef.blur();
                    }, () => {
                        this.setState({
                            error: true
                        });
                    });
                }
                else {
                    props.onEnter(state.value);
                    this.setState({
                        editMode: false,
                        error: false
                    });
                    this.editableInputRef.blur();
                }
                //enter
                break;
            case 27:
                event.preventDefault();
                //escape
                this.setState({
                    value: props.value,
                    editMode: false,
                    error: false
                });
                if (typeof this.props.onEscape === "function") {
                    this.props.onEscape(event);
                }
                setTimeout(() => {
                    this.editableInputRef.blur();
                }, 250);
                break;
            default:
                break;
        }
    }
    onBlur() {
        window.dnn.stopEscapeFromClosingPB = false;
        if (typeof this.props.onBlur === "function") {
            this.props.onBlur();
        }
    }
    onKeyUp(event) {
        const value = event.target.value;
        this.setState({
            value
        });
        if (typeof this.props.onKeyUp === "function") {
            this.props.onKeyUp(event);
        }
    }

     
    getEditButton() {
        return <div className="edit-button" onClick={this.toggleEditMode.bind(this)}><EditIcon /></div>;
    }

    getUrl(text) {
		logger.error(`>>> Entering getUrl(text=${text})`, { "event.action": "getUrl", "logrite.text": text });
        if (text.indexOf("http://") !== 0 && text.indexOf("https://") !== 0) {
            return "http://" + text;
        }
		logger.warn(`<<< Exiting getUrl(text=${text})`, { "event.action": "getUrl", "logrite.text": text });
        return text;
    }

    getEditableValue() {
		logger.error('>>> Entering getEditableValue()', { "event.action": "getEditableValue" });
        const { props, state } = this;
        if (!props.isUrl) {
            return <span className="editable-value">{state.value}</span>;
        }
		logger.warn('<<< Exiting getEditableValue()', { "event.action": "getEditableValue" });
        return <span className="editable-value"><a href={this.getUrl(state.value)} target="_blank" rel="noopener noreferrer">{state.value}</a></span>;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props, refs } = this;
        const editableLabelClass = this.getEditableLabelClass();
        const input = this.getInputFromType();
        const editableValue = this.getEditableValue();
        const editButton = this.getEditButton();

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={editableLabelClass}>
                <label className="editable-label" htmlFor={refs.input}>{props.label}</label>
                {props.editable && editButton}
                {editableValue}
                <div className="editable-input">
                    {input}
                    <span className="help-text">{(props.helpText || "Press ENTER when done, or ESC to cancel")}</span>
                </div>
            </div>
        );
    }
}

EditableField.propTypes = {
    label: PropTypes.string,
    value: PropTypes.string,
    inputType: PropTypes.string,
    onEnter: PropTypes.func,
    editable: PropTypes.bool,
    helpText: PropTypes.string,
    isUrl: PropTypes.bool.isRequired,
    onKeyUp: PropTypes.func,
    onKeyDown: PropTypes.func,
    onBlur: PropTypes.func,
    onFocus: PropTypes.func,
    onEscape: PropTypes.func,
    errorMessage: PropTypes.string,
    enableCallback: PropTypes.bool
};

EditableField.defaultProps = {
    editable: true,
    isUrl: false,
    enableCallback: false
};

export default EditableField;