import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    importExport as ImportExportActions
} from "actions";
import { Line } from "rc-progress";
import "./style.less";
const logger = console;

class ProgressBar extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });

        this.timeout = "";
        this.started = false;
		logger.debug(`constructor(): this.timeout → ${this.timeout}`, { "event.action": "constructor" });
        this.state = {
            percent: 0
        };
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.started = true;
        this.increase();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        clearTimeout(this.timeout);
        this.setState({ percent: 0 });
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    componentDidUpdate() {
		logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
        let { props } = this;
        if (props.visible !== undefined) {

            if (!props.visible && this.started) {
                this.started = false;
                this.setState({ percent: 0 });
                clearTimeout(this.timeout);
            }

            if (props.visible && !this.started) {
                this.started = true;
                this.setState({ percent: 0 });
                this.increase();
				logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
            }
        }
    }

    increase() {
		logger.error('>>> Entering increase()', { "event.action": "increase" });
        if (this.state.percent > 95) {
            //clearTimeout(this.timeout);
            this.setState({ percent: 0 });
        }
        else {
            if (this.props.loaded) {
                this.setState({
                    percent: (100)
                }, () => {
                    this.timeout = setTimeout(() => {
                        this.props.dispatch(ImportExportActions.packageVerified(true));
                    }, 100);
                });
            }
            else {
                this.setState({
                    percent: (this.state.percent + 5)
                });
            }
        }
        this.timeout = setTimeout(() => {
            this.increase();
        }, 200);
		logger.warn('<<< Exiting increase()', { "event.action": "increase" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        let visibility = this.props.visible ? "block" : "none";

		logger.error(`render(): visibility → ${visibility}`, { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div id="progressContainer" className={this.props.className} style={{ display: visibility }}>
                <Line
                    percent={this.state.percent}
                    strokeWidth={5}
                    trailWidth={5}
                    strokeLinecap={"square"}
                    trailColor={"#eff0f0"}
                    strokeColor={"#9fdbf0"} />
            </div>
        );
    }
}

ProgressBar.propTypes = {
    dispatch: PropTypes.func.isRequired,
    visible: PropTypes.bool,
    className: PropTypes.string,
    loaded: PropTypes.bool
};

function mapStateToProps() {
	logger.error('>>> Entering mapStateToProps()', { "event.action": "mapStateToProps" });
	logger.warn('<<< Exiting mapStateToProps()', { "event.action": "mapStateToProps" });
    return {};
}

export default connect(mapStateToProps)(ProgressBar);