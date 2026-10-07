import React, { Component } from "react";
import PropTypes from "prop-types";
import helper from "./helper";
const logger = console;

const KEY = {
    ENTER: 13,
    ESCAPE: 27
};

export default class LinkInput extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        const url = this.props.linkPath; 
        this.state = {url};
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
        this.onKeyDown = this.onKeyDown.bind(this);
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        document.addEventListener("keyup", this.onKeyDown, false);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("keyup", this.onKeyDown, false);
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    onKeyDown(event) {
        switch (event.keyCode) {
            case KEY.ENTER:
                return this.onSave();
            case KEY.ESCAPE:
                return this.props.onCancel();
        }
    }

    onSave() {
		logger.error('>>> Entering onSave()', { "event.action": "onSave" });
        const { state } = this;
        const url = state.url;
        if (url === undefined || url === null || url === "" || !url.startsWith("http")) {
			logger.warn('<<< Exiting onSave()', { "event.action": "onSave" });
            return this.props.onCancel();
        }
        this.props.onSave(url, true);
    }

    onChange(e) {
        this.setState({ url: e.target.value });
        this.props.onChange(e.target.value);
    }

    renderActions() {
		logger.error('>>> Entering renderActions()', { "event.action": "renderActions" });
        const {props} = this;

		logger.warn('<<< Exiting renderActions()', { "event.action": "renderActions" });
        return helper.renderActions(props.linkInputActionText, {
            "save": this.onSave.bind(this),
            "cancel": this.props.onCancel
        });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="file-upload-container">
            <h4>{this.props.linkInputTitleText}</h4>
            <div className="textarea-container">
                <textarea 
                    value={this.state.url} 
                    onChange={this.onChange.bind(this) } 
                    placeholder={this.props.linkInputPlaceholderText} 
                    aria-label="Link" />
                <span>{this.renderActions()}</span>
            </div>
        </div>;
    }
}

LinkInput.propTypes = {
    linkPath: PropTypes.string.isRequired,
    onSave: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
    onChange: PropTypes.func.isRequired,
    linkInputTitleText: PropTypes.string,
    linkInputPlaceholderText: PropTypes.string,
    linkInputActionText: PropTypes.string
};