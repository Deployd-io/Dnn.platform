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
		logger.debug('>>> Entering constructor()');

        this.timeout = "";
        this.started = false;
		logger.debug(`constructor(): this.timeout → ${this.timeout}`);
        this.state = {
            percent: 0
        };
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.started = true;
        this.increase();
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        clearTimeout(this.timeout);
        this.setState({ percent: 0 });
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    componentDidUpdate() {
		logger.debug('>>> Entering componentDidUpdate()');
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
				logger.debug('<<< Exiting componentDidUpdate()');
            }
        }
    }

    increase() {
		logger.debug('>>> Entering increase()');
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
		logger.debug('<<< Exiting increase()');
    }

    render() {
		logger.debug('>>> Entering render()');
        let visibility = this.props.visible ? "block" : "none";

		logger.debug(`render(): visibility → ${visibility}`);
		logger.debug('<<< Exiting render()');
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
	logger.debug('>>> Entering mapStateToProps()');
	logger.debug('<<< Exiting mapStateToProps()');
    return {};
}

export default connect(mapStateToProps)(ProgressBar);