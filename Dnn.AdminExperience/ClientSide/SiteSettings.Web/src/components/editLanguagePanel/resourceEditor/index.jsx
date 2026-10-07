import React, { Component } from "react";
import PropTypes from "prop-types";
import debounce from "lodash/debounce";
import FullEditor from "./fullEditor";
import "./style.less";
const logger = console;
import { SingleLineInput, MultiLineInput, Modal, SvgIcons } from "@dnnsoftware/dnn-react-common";

class ResourceEditor extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            inFullMode: false,
            content: props.value ? props.value : ""
        };
        this.debouncedOnChange = debounce(this.changeContent, 500);
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        if (this.props.value !== prevProps.value) {
            this.setState({content: this.props.value});
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    onChange(e) {
        this.setState({content: e.target.value});
        this.debouncedOnChange();
    }

    changeContent() {
		logger.error('>>> Entering changeContent()', { "event.action": "changeContent" });
        const { props } = this;

        if (props.enabled) {
            props.onChange(this.state.content);
			logger.warn('<<< Exiting changeContent()', { "event.action": "changeContent" });
        }
    }

    renderMulti() {
        const { props, state } = this;

        let lines = props.value.length / 30;
        if (props.value.length % 30 !== 0) {
            lines ++;
        }

        let height = lines * 18 + 16;
        if (height > 100) {
            height = 100;
        }

        return (<MultiLineInput 
            className={props.className} 
            value={state.content} 
            enabled={props.enabled}
            style={{height: height + "px"}}
            onChange={this.onChange.bind(this)} />);
    }

    renderSingle() {
		logger.error('>>> Entering renderSingle()', { "event.action": "renderSingle" });
        const { props, state } = this;

		logger.warn('<<< Exiting renderSingle()', { "event.action": "renderSingle" });
        return (<SingleLineInput 
            className={props.className} 
            value={state.content} 
            enabled={props.enabled}
            onChange={this.onChange.bind(this)} />);
    }

    onEnterFullMode() {
		logger.error('>>> Entering onEnterFullMode()', { "event.action": "onEnterFullMode" });
        this.setState({
            inFullMode: true
        });

        window.dnn.stopEscapeFromClosingPB = true;
		logger.warn('<<< Exiting onEnterFullMode()', { "event.action": "onEnterFullMode" });
    }

    onExitFullMode() {
		logger.error('>>> Entering onExitFullMode()', { "event.action": "onExitFullMode" });
        this.setState({
            inFullMode: false
        });

        window.dnn.stopEscapeFromClosingPB = false;
		logger.warn('<<< Exiting onExitFullMode()', { "event.action": "onExitFullMode" });
    }

    onFullEditorChange(value) {
		logger.error(`>>> Entering onFullEditorChange(value=${value})`, { "event.action": "onFullEditorChange", "logrite.value": value });
        const { props } = this;

        props.onChange(value);

        this.onExitFullMode();
		logger.warn(`<<< Exiting onFullEditorChange(value=${value})`, { "event.action": "onFullEditorChange", "logrite.value": value });
    }

    onFullEditorCancel() {
		logger.error('>>> Entering onFullEditorCancel()', { "event.action": "onFullEditorCancel" });
        this.onExitFullMode();
		logger.warn('<<< Exiting onFullEditorCancel()', { "event.action": "onFullEditorCancel" });
    }
    
     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props, state } = this;

        const renderMulti = props.value && props.value.length > 30;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (<div className="dnn-language-resource-editor">
            {renderMulti ? this.renderMulti() : this.renderSingle()}
            {props.enabled && 
            <div className="edit-svg" onClick={this.onEnterFullMode.bind(this)}>
                <SvgIcons.EditIcon />
            </div>
            }
            {props.enabled && 
            <Modal 
                isOpen={state.inFullMode}
                onRequestClose={this.onExitFullMode.bind(this)}
                shouldCloseOnOverlayClick={false}
                modalHeight={390}>
                <FullEditor 
                    value={props.value} 
                    onChange={this.onFullEditorChange.bind(this)}
                    onCancel={this.onFullEditorCancel.bind(this)}
                />
            </Modal>
            }
        </div>);
    }
}

ResourceEditor.propTypes = {
    value: PropTypes.string,
    enabled: PropTypes.bool,
    onChange: PropTypes.func,
    className: PropTypes.string
};

ResourceEditor.defaultProps = {
    enabled: true
};

export default ResourceEditor;
