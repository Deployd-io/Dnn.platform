import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import TaskStatusItemRow from "./taskStatusItemRow";
import "./style.less";
import resx from "../../resources";
import NoDataIcon from "./../svg/nodata.svg";
const logger = console;

class TaskQueuePanelBody extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    renderedTaskStatusList() {
		logger.error('>>> Entering renderedTaskStatusList()', { "event.action": "renderedTaskStatusList" });
        const { props } = this;
		logger.warn('<<< Exiting renderedTaskStatusList()', { "event.action": "renderedTaskStatusList" });
        return props.taskStatusList.map((term, index) => {
            return (
                <TaskStatusItemRow
                    scheduleId={term.ScheduleID}
                    friendlyName={term.FriendlyName}
                    overdue={term.Overdue}
                    remainingTime={term.RemainingTime}
                    nextStart={term.NextStart}
                    objectDependencies={term.ObjectDependencies}
                    scheduleSource={term.ScheduleSource}
                    threadId={term.ThreadID}
                    servers={term.Servers}
                    key={"taskStatusItem-" + index}
                    closeOnClick={true}>
                </TaskStatusItemRow>
            );
        });
    }

    renderedTaskProcessingList() {
		logger.error('>>> Entering renderedTaskProcessingList()', { "event.action": "renderedTaskProcessingList" });
        const { props } = this;
        if (props.taskProcessingList) {
			logger.warn('<<< Exiting renderedTaskProcessingList()', { "event.action": "renderedTaskProcessingList" });
            return props.taskProcessingList.map((term, index) => {
                return (
                    <TaskStatusItemRow
                        scheduleId={term.ScheduleID}
                        friendlyName={term.TypeFullName}
                        elapsedTime={term.ElapsedTime}
                        startDate={term.StartDate}
                        objectDependencies={term.ObjectDependencies}
                        scheduleSource={term.ScheduleSource}
                        threadId={term.ThreadID}
                        servers={term.Servers}
                        key={"taskStatusItem-" + index}
                        closeOnClick={true}>
                    </TaskStatusItemRow>
                );
            });
        }
    }

    /*eslint-disable eqeqeq*/
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div>
                {props.taskStatusList && props.taskStatusList.length == 0 && props.taskProcessingList && props.taskProcessingList.length == 0 &&
                    <div className="noData">
                        <div className="noTasks">{props.schedulingEnabled === "True" ? resx.get("NoTasks") : resx.get("DisabledMessage")}</div>
                        <div className="noTasksMessage">{resx.get("NoTasksMessage")}</div>
                        <div><NoDataIcon /></div>
                    </div>
                }
                {this.renderedTaskProcessingList()}
                {this.renderedTaskStatusList()}
            </div>
        );
    }
}

TaskQueuePanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    schedulingEnabled: PropTypes.string,
    taskStatusList: PropTypes.array,
    taskProcessingList: PropTypes.array
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        schedulingEnabled: state.task.schedulingEnabled,
        taskStatusList: state.task.taskStatusList,
        taskProcessingList: state.task.taskProcessingList,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(TaskQueuePanelBody);