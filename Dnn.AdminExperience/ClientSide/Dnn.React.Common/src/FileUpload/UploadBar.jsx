import React, { Component } from "react";
import PropTypes from "prop-types";

import Upload from "./img/upload.svg";
import Checkmark from "./img/checkmark.svg";
import ErrorIcon from "./img/x.svg";
const logger = console;

export default class UploadBar extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            percent: 0
        };
        this.timeout = 20;
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
		logger.debug(`constructor(): this.timeout → ${this.timeout}`, { "event.action": "constructor" });
		logger.debug(`constructor(): this.setTimeout → ${this.setTimeout}`, { "event.action": "constructor" });
        this.delta = 1.01;
        this.setTimeout = null;
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        setTimeout(this.increase.bind(this), 100);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }
    
    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const { props } = this;
        if (props.uploadComplete && props.uploadComplete !== prevProps.uploadComplete) {
            clearTimeout(this.setTimeout);
            this.setState({percent: 100});
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    increase() {
		logger.error('>>> Entering increase()', { "event.action": "increase" });
        let {percent} = this.state;
        percent++;
        this.timeout *= this.delta;
        this.delta *= 1.001;
        this.setState({percent});
        if (percent < 99) {
            this.setTimeout = setTimeout(this.increase.bind(this), this.timeout);
			logger.warn('<<< Exiting increase()', { "event.action": "increase" });
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
         
        let percent = this.props.errorText ? 0 : this.state.percent;
        let text = this.props.uploadComplete ? this.props.uploadCompleteText : this.props.uploadingText;
        text = this.props.errorText ? this.props.errorText : text;
        let Svg = this.props.uploadComplete ? Checkmark : Upload;
        Svg = this.props.errorText ? ErrorIcon : Svg;
        const className = "file-upload-container dnn-upload-bar" + (!this.props.errorText && this.props.uploadComplete ? " complete" : "") + (this.props.errorText ? " upload-error" : "");

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className={className}>
            <div className="upload-bar-container">
                <div className="upload-file-name">{this.props.fileName || this.props.uploadDefaultText}</div>
                <div className="upload-icon"><Svg /></div>
                <h4>{text}</h4>
                <div className="upload-percent">{percent + "%"}</div>
                <div className="upload-bar-box">
                    <div className="upload-bar" style={{width: percent + "%"}}/>
                </div>
            </div>
        </div>;
    }
}

UploadBar.propTypes = {
    errorText: PropTypes.string.isRequired, 
    fileName: PropTypes.string.isRequired,
    uploadComplete: PropTypes.bool.isRequired,

    uploadCompleteText: PropTypes.string,
    uploadingText: PropTypes.string,
    uploadDefaultText: PropTypes.string
};