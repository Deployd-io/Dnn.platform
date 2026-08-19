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
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }

    renderedTaskStatusList() {
		logger.debug('>>> Entering renderedTaskStatusList()');
        const { props } = this;
		logger.debug('<<< Exiting renderedTaskStatusList()');
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
		logger.debug('>>> Entering renderedTaskProcessingList()');
        const { props } = this;
        if (props.taskProcessingList) {
			logger.debug('<<< Exiting renderedTaskProcessingList()');
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
		logger.debug('>>> Entering render()');
        const { props } = this;
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        schedulingEnabled: state.task.schedulingEnabled,
        taskStatusList: state.task.taskStatusList,
        taskProcessingList: state.task.taskProcessingList,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(TaskQueuePanelBody);