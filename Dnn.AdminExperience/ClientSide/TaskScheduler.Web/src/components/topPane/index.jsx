import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    task as TaskActions
} from "../../actions";
import styles from "./style.module.less";
import { Button, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
import ModePanel from "./modePanel";
import util from "../../utils";
import resx from "../../resources";
import EditIcon from "./../svg/edit.svg";
const logger = console;

class TopPane extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            modePanelOpen: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        const {props} = this;
        const persistedSettings = util.utilities.persistent.load();
		logger.debug(`componentDidMount(): persistedSettings → ${persistedSettings}`);

        props.dispatch(TaskActions.getSchedulerSettings());
        props.dispatch(TaskActions.getTaskStatusList());
        this.taskListTimeout = setInterval(() => {
            if (persistedSettings.expandPersonaBar && persistedSettings.activeIdentifier === "Dnn.TaskScheduler") {
                props.dispatch(TaskActions.getTaskStatusList());
            }
        }, 5000);
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        clearInterval(this.taskListTimeout);
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    onClick() {
		logger.debug('>>> Entering onClick()');
        const {props} = this;
        if (this.isStopped()) {
            props.dispatch(TaskActions.startSchedule(() => {
                util.utilities.notify(resx.get("SchedulerStartSuccess"));
                props.dispatch(TaskActions.getTaskStatusList());
            }, () => {
                util.utilities.notify(resx.get("SchedulerStartError"));
            }));
        }
        else {
            props.dispatch(TaskActions.stopSchedule(() => {
                util.utilities.notify(resx.get("SchedulerStopSuccess"));
                props.dispatch(TaskActions.getTaskStatusList());
            }, () => {
                util.utilities.notify(resx.get("SchedulerStopError"));
				logger.debug('<<< Exiting onClick()');
            }));
        }
    }

    getButtonDisplay() {
		logger.debug('>>> Entering getButtonDisplay()');
        if (this.isStopped() || this.props.schedulerMode === "2") {
            return <TextOverflowWrapper text={resx.get("StartSchedule") } maxWidth={100}/>;
        }
        else {
			logger.debug('<<< Exiting getButtonDisplay()');
            return <TextOverflowWrapper text={resx.get("StopSchedule") } maxWidth={100}/>;
        }
    }

    isStopped() {
		logger.debug('>>> Entering isStopped()');
        const {props} = this;
        if (props.status === "STOPPED") {
            return true;
        }
        else {
			logger.debug('<<< Exiting isStopped()');
            return false;
        }
    }

    getschedulerModeDisplay() {
		logger.debug('>>> Entering getschedulerModeDisplay()');
        const {props} = this;
        if (props.schedulerMode) {
		logger.debug('<<< Exiting getschedulerModeDisplay()');
            return props.schedulerModeOptions[parseInt(props.schedulerMode)].Key;
        }
        return "";
    }

    toggleModePanel() {
		logger.debug('>>> Entering toggleModePanel()');
        if (this.props.status !== "SHUTTING_DOWN") {
            this.setState({
                modePanelOpen: !this.state.modePanelOpen
            });
        }
        else {
            if (this.state.modePanelOpen) {
				logger.debug('<<< Exiting toggleModePanel()');
                this.setState({
                    modePanelOpen: !this.state.modePanelOpen
                });
            }
        }
    }

    getModeOptions() {
		logger.debug('>>> Entering getModeOptions()');
        const {props} = this;
        if (props.schedulerModeOptions) {
            let options = props.schedulerModeOptions.map((mode) => {
                return { value: mode.Value, label: mode.Key };
            });
			logger.debug(`getModeOptions(): options → ${options}`);
            return options;
        }
        else return [{ value: "", label: "" }];
		logger.debug('<<< Exiting getModeOptions()');
    }

    renderButton() {
		logger.debug('>>> Entering renderButton()');
        const {props} = this;
        if (props.status !== "SHUTTING_DOWN") {
            if (props.schedulingEnabled === "True") {
                if (props.schedulerMode === "2") {
                    return (<Button
                        type="primary" disabled={true}>
                        {this.getButtonDisplay()}
                    </Button>);
                }
                else {
                    return (<Button
                        className={props.status !== "STOPPED" ? "topPane-button-start" : ""}
                        type={props.status !== "STOPPED" ? "secondary" : "primary"}
                        onClick={this.onClick.bind(this)}>
                        {this.getButtonDisplay()}
                    </Button>);
                }
            }
            else {
                return (<Button
                    type="primary" disabled={true}>
                    {this.getButtonDisplay()}
                </Button>);
            }
        }
        else {
			logger.debug('<<< Exiting renderButton()');
            return (<Button
                type="primary" disabled={true}>
                {this.getButtonDisplay()}
            </Button>);
        }
    }


    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
		logger.debug('<<< Exiting render()');
        return (
            <div className={styles.topPaneWrapper}>
                <div className="topPane-time">
                    <div className="topPane-time-name">{resx.get("ServerTime")}</div>
                    <div className="topPane-time-value">{props.serverTime}</div>
                </div>
                <div className="topPane">
                    <div className="topPane-left">{resx.get("lblStatusLabel")}</div>
                    <div className="topPane-middle">
                        <div className={props.status === "STOPPED" ? "topPane-middle-name-stopped" : "topPane-middle-name"}>{resx.get(props.status)}</div>
                        <div className="topPane-middle-common">
                            <div className="topPane-middle-common-title" style={{maxWidth: "60%"}}>{resx.get("plSchedulerMode")}</div>
                            <div>
                                <div className={props.status === "SHUTTING_DOWN" ? "editIconDisabled" : "editIcon"} onClick={this.toggleModePanel.bind(this)}><EditIcon /></div>
                                <div className="collapsible-content">
                                    {props.schedulerDelay &&
                                        <ModePanel
                                            className="topPane-middle-content-body"
                                            isOpened={state.modePanelOpen}
                                            onClose={this.toggleModePanel.bind(this)}
                                            schedulerDelay={props.schedulerDelay}
                                            schedulerModeOptions={this.getModeOptions()}
                                            schedulerMode={props.schedulerMode}>
                                        </ModePanel>
                                    }
                                </div>
                            </div>
                            <div className="topPane-middle-common-value">{this.getschedulerModeDisplay()}</div>
                        </div>

                        <div className="topPane-middle-common">
                            <div className="topPane-middle-common-title">{resx.get("lblStartDelay")}</div>
                            <div className="topPane-middle-common-value">{props.schedulerDelay}</div>
                        </div>
                    </div>
                    <div className="topPane-right">
                        <div className="topPane-right-common">
                            <div className="topPane-right-common-title">{resx.get("lblMaxThreadsLabel")}</div>
                            <div className="topPane-right-common-value">{props.maxThreads}</div>
                        </div>
                        <div className="topPane-right-common">
                            <div className="topPane-right-common-title">{resx.get("lblActiveThreadsLabel")}</div>
                            <div className="topPane-right-common-value">{props.activeThreads}</div>
                        </div>
                        <div className="topPane-right-common">
                            <div className="topPane-right-common-title">{resx.get("lblFreeThreadsLabel")}</div>
                            <div className="topPane-right-common-value">{props.freeThreads}</div>
                        </div>
                    </div>
                    <div className="topPane-button">
                        {this.renderButton()}
                    </div>
                </div>
            </div>
        );
    }
}

TopPane.propTypes = {
    dispatch: PropTypes.func.isRequired,
    status: PropTypes.string,
    maxThreads: PropTypes.string,
    activeThreads: PropTypes.string,
    freeThreads: PropTypes.string,
    schedulerMode: PropTypes.string,
    schedulerDelay: PropTypes.number,
    schedulerModeOptions: PropTypes.array,
    schedulingEnabled: PropTypes.string,
    serverTime: PropTypes.string
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        schedulingEnabled: state.task.schedulingEnabled,
        status: state.task.status,
        freeThreads: state.task.freeThreads,
        activeThreads: state.task.activeThreads,
        maxThreads: state.task.maxThreads,
        schedulerMode: state.task.schedulerMode,
        schedulerDelay: state.task.schedulerDelay,
        schedulerModeOptions: state.task.schedulerModeOptions,
        serverTime: state.task.serverTime
    };
}


export default connect(mapStateToProps)(TopPane);