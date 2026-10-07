import React, { Component } from "react";
import PropTypes from "prop-types";
import { ErrorStateIcon } from "../SvgIcons";
import "./style.less";
const logger = console;

export default class ContentLoadWrapper extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            percent: 0
        };
        this.timeout = 50;
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

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        let { props } = this;
        if (props.loadComplete !== prevProps.loadComplete) {
            if (props.loadComplete) {
                clearTimeout(this.setTimeout);
                if (this._isMounted) {
                    this.setState({ percent: 100 }, () => {
                    });
                }
                setTimeout(() => {
                    if (typeof props.onCompleteCallback === "function") {
                        props.onCompleteCallback();
                    }
                }, 300);
            }
        }
        if (props.loadError !== prevProps.loadError) {
            if (props.loadError) {
                clearTimeout(this.setTimeout);
                if (this._isMounted) {
                    this.setState({ percent: 100 }, () => {
                    });
                }
                setTimeout(() => {
                    if (typeof props.onErrorCallback === "function") {
                        props.onErrorCallback();
                    }
                }, 300);
				logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
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
    onTryAgain() {
		logger.error('>>> Entering onTryAgain()', { "event.action": "onTryAgain" });
        this.setState({
            percent: 0
        }, () => {
            setTimeout(this.increase.bind(this), 100);
        });
        if (typeof this.props.onTryAgain === "function") {
            this.props.onTryAgain();
			logger.warn('<<< Exiting onTryAgain()', { "event.action": "onTryAgain" });
        }
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        let {percent} = this.state;
        const className = "dnn-content-load-wrapper" + (this.props.loadComplete ? " complete" : "") + (this.props.loadError ? " upload-error" : "");
        if (typeof this.props.loadComplete === "undefined" || this.props.loadComplete) {
            return this.props.children;
        }

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className={className}>
            {this.props.svgSkeleton}
            {this.props.loadError &&
                <div className="try-load-again">
                    <div>
                        <div className="upload-icon">ErrorStateIcon</div>
                        <p>{this.props.failedToLoadText}</p>
                        <span onClick={this.onTryAgain.bind(this)}>[{this.props.tryAgainText}]</span>
                    </div>
                </div>}
            <div className="upload-bar-container">
                <div className="upload-bar-box">
                    <div className="upload-bar" style={{ width: percent + "%" }} />
                </div>
            </div>
        </div>;
    }
}

ContentLoadWrapper.propTypes = {
    loadComplete: PropTypes.bool.isRequired,
    failedToLoadText: PropTypes.string,
    children: PropTypes.node,
    svgSkeleton: PropTypes.node,
    tryAgainText: PropTypes.string,
    loadError: PropTypes.bool,
    onTryAgain: PropTypes.func,
    onCompleteCallback: PropTypes.func,
    onErrorCallback: PropTypes.func
};

ContentLoadWrapper.defaultProps = {
    failedToLoadText: "Failed To Load",
    tryAgainText: "Retry"
};