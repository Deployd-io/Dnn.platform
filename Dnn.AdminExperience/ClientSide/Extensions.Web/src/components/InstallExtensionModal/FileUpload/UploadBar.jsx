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
        this.delta = 1.00;
        this.setTimeout = null;
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        setTimeout(this.increase.bind(this), 100);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        this._isMounted = false;
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    UNSAFE_componentWillReceiveProps(props) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.props": props });
        if (props.uploadComplete) {
            clearTimeout(this.setTimeout);
            if (this._isMounted) {
                this.setState({ percent: 100 }, () => {
                });
				logger.warn(`<<< Exiting UNSAFE_componentWillReceiveProps(props=${props})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.props": props });
            }
        }
    }

    increase() {
		logger.error('>>> Entering increase()', { "event.action": "increase" });
        let {percent} = this.state;
        percent++;
        this.timeout *= this.delta;
        this.delta *= 1.00;
        if (percent <= 100) {
            if (this._isMounted) {
                this.setState({ percent });
            }
        }
        if (percent < 95) {
            this.setTimeout = setTimeout(this.increase.bind(this), this.timeout);
			logger.warn('<<< Exiting increase()', { "event.action": "increase" });
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;

        let percent = props.errorText ? 0 : this.state.percent;
        let text = props.uploadComplete ? props.uploadCompleteText : props.uploadingText;
        text = props.errorText ? props.errorText : text;
        let UploadIcon =
            props.errorText
                ? ErrorIcon
                : props.uploadComplete
                    ? Checkmark
                    : Upload;
        const className = "file-upload-container dnn-upload-bar" + (props.uploadComplete ? " complete" : "") + (props.errorText ? " upload-error" : "");

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className={className}>
            <div className="upload-bar-container">
                <div className="upload-file-name">{this.props.fileName || "myImage.jpg"}</div>
                <div className="upload-icon"><UploadIcon /></div>
                <h4>{text}</h4>
                {props.errorInPackage &&
                    <p className="view-log-or-try-again">
                        <span onClick={props.onViewLog.bind(this)}>{props.viewLogText} </span>
                        or
                        <span onClick={props.onTryAgain.bind(this)}>{props.tryAgainText}</span>
                    </p>}
                <div className="upload-percent">{percent + "%"}</div>
                <div className="upload-bar-box">
                    <div className="upload-bar" style={{ width: percent + "%" }} />
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
    errorInPackage: PropTypes.bool
};

UploadBar.defaultProps = {
    uploadCompleteText: "Upload Complete",
    uploadingText: "Uploading..."
};