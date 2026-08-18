import React, { Component } from "react";
import PropTypes from "prop-types";
import Upload from "./img/upload.svg";
import Checkmark from "./img/checkmark.svg";
import ErrorIcon from "./img/x.svg";
const logger = console;

export default class UploadBar extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            percent: 0
        };
        this.timeout = 20;
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug(`constructor(): this.timeout → ${this.timeout}`);
		logger.debug(`constructor(): this.setTimeout → ${this.setTimeout}`);
        this.delta = 1.00;
        this.setTimeout = null;
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        setTimeout(this.increase.bind(this), 100);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        this._isMounted = false;
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    UNSAFE_componentWillReceiveProps(props) {
		logger.debug(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`);
        if (props.uploadComplete) {
            clearTimeout(this.setTimeout);
            if (this._isMounted) {
                this.setState({ percent: 100 }, () => {
                });
				logger.debug(`<<< Exiting UNSAFE_componentWillReceiveProps(props=${props})`);
            }
        }
    }

    increase() {
		logger.debug('>>> Entering increase()');
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
			logger.debug('<<< Exiting increase()');
        }
    }

    render() {
		logger.debug('>>> Entering render()');
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

		logger.debug('<<< Exiting render()');
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