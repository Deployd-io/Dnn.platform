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
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            modePanelOpen: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;
        const persistedSettings = util.utilities.persistent.load();
		logger.error(`componentDidMount(): persistedSettings → ${persistedSettings}`, { "event.action": "componentDidMount" });

        props.dispatch(TaskActions.getSchedulerSettings());
        props.dispatch(TaskActions.getTaskStatusList());
        this.taskListTimeout = setInterval(() => {
            if (persistedSettings.expandPersonaBar && persistedSettings.activeIdentifier === "Dnn.TaskScheduler") {
                props.dispatch(TaskActions.getTaskStatusList());
            }
        }, 5000);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        clearInterval(this.taskListTimeout);
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    onClick() {
		logger.error('>>> Entering onClick()', { "event.action": "onClick" });
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
				logger.warn('<<< Exiting onClick()', { "event.action": "onClick" });
            }));
        }
    }

    getButtonDisplay() {
		logger.error('>>> Entering getButtonDisplay()', { "event.action": "getButtonDisplay" });
        if (this.isStopped() || this.props.schedulerMode === "2") {
            return <TextOverflowWrapper text={resx.get("StartSchedule") } maxWidth={100}/>;
        }
        else {
			logger.warn('<<< Exiting getButtonDisplay()', { "event.action": "getButtonDisplay" });
            return <TextOverflowWrapper text={resx.get("StopSchedule") } maxWidth={100}/>;
        }
    }

    isStopped() {
		logger.error('>>> Entering isStopped()', { "event.action": "isStopped" });
        const {props} = this;
        if (props.status === "STOPPED") {
            return true;
        }
        else {
			logger.warn('<<< Exiting isStopped()', { "event.action": "isStopped" });
            return false;
        }
    }

    getschedulerModeDisplay() {
		logger.error('>>> Entering getschedulerModeDisplay()', { "event.action": "getschedulerModeDisplay" });
        const {props} = this;
        if (props.schedulerMode) {
            return props.schedulerModeOptions[parseInt(props.schedulerMode)].Key;
        }
		logger.warn('<<< Exiting getschedulerModeDisplay()', { "event.action": "getschedulerModeDisplay" });
        return "";
    }

    toggleModePanel() {
		logger.error('>>> Entering toggleModePanel()', { "event.action": "toggleModePanel" });
        if (this.props.status !== "SHUTTING_DOWN") {
            this.setState({
                modePanelOpen: !this.state.modePanelOpen
            });
        }
        else {
            if (this.state.modePanelOpen) {
				logger.warn('<<< Exiting toggleModePanel()', { "event.action": "toggleModePanel" });
                this.setState({
                    modePanelOpen: !this.state.modePanelOpen
                });
            }
        }
    }

    getModeOptions() {
		logger.error('>>> Entering getModeOptions()', { "event.action": "getModeOptions" });
        const {props} = this;
        if (props.schedulerModeOptions) {
            let options = props.schedulerModeOptions.map((mode) => {
                return { value: mode.Value, label: mode.Key };
            });
			logger.error(`getModeOptions(): options → ${options}`, { "event.action": "getModeOptions" });
            return options;
        }
        else return [{ value: "", label: "" }];
    }

    renderButton() {
		logger.error('>>> Entering renderButton()', { "event.action": "renderButton" });
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
			logger.warn('<<< Exiting renderButton()', { "event.action": "renderButton" });
            return (<Button
                type="primary" disabled={true}>
                {this.getButtonDisplay()}
            </Button>);
        }
    }


    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
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