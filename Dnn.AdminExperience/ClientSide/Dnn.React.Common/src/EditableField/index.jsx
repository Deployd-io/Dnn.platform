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
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            editMode: false,
            value: props.value,
            error: false
        };
        this.uniqueId = "editableField-" + (Date.now() * Math.random());
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }
    
    toggleEditMode() {
		logger.debug('>>> Entering toggleEditMode()');
        this.setState({
            editMode: !this.state.editMode
        });
        if (!this.state.editMode) {
            this.editableInputRef.focus();
			logger.debug('<<< Exiting toggleEditMode()');
        }
    }

    getEditableLabelClass() {
		logger.debug('>>> Entering getEditableLabelClass()');
        const { props, state } = this;

        let editableLabelClass = state.editMode ? " in-edit" : "";
		logger.debug(`getEditableLabelClass(): editableLabelClass → ${editableLabelClass}`);
        let inputTypeClass = props.inputType === "textArea" ? " textArea" : " singleLine";
		logger.debug(`getEditableLabelClass(): inputTypeClass → ${inputTypeClass}`);

		logger.debug('<<< Exiting getEditableLabelClass()');
        return "dnn-editable-field" + editableLabelClass + inputTypeClass;
    }

    onFocus() {
        window.dnn.stopEscapeFromClosingPB = true;
        if (typeof this.props.onFocus === "function") {
            this.props.onFocus();
        }
    }
    getInputFromType() {
		logger.debug('>>> Entering getInputFromType()');
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
			logger.debug('<<< Exiting getInputFromType()');
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
		logger.debug(`>>> Entering getUrl(text=${text})`);
        if (text.indexOf("http://") !== 0 && text.indexOf("https://") !== 0) {
		logger.debug(`<<< Exiting getUrl(text=${text})`);
            return "http://" + text;
        }
        return text;
    }

    getEditableValue() {
		logger.debug('>>> Entering getEditableValue()');
        const { props, state } = this;
        if (!props.isUrl) {
		logger.debug('<<< Exiting getEditableValue()');
            return <span className="editable-value">{state.value}</span>;
        }
        return <span className="editable-value"><a href={this.getUrl(state.value)} target="_blank" rel="noopener noreferrer">{state.value}</a></span>;
    }

    render() {
		logger.debug('>>> Entering render()');
        const { props, refs } = this;
        const editableLabelClass = this.getEditableLabelClass();
        const input = this.getInputFromType();
        const editableValue = this.getEditableValue();
        const editButton = this.getEditButton();

		logger.debug('<<< Exiting render()');
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