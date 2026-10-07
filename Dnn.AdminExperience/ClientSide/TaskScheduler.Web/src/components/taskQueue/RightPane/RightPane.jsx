import React, {Component } from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";
import resx from "../../../resources";
const logger = console;

class RightPane extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    onEnter(key, value) {
		logger.error(`>>> Entering onEnter(key=${key},value=${value})`, { "event.action": "onEnter", "logrite.key": key, "logrite.value": value });
        const {props} = this;
        props.onEnter(key, value, props.index);
		logger.warn(`<<< Exiting onEnter(key=${key},value=${value})`, { "event.action": "onEnter", "logrite.key": key, "logrite.value": value });
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.taskStatusItemRightPane}>
                <div className="taskDetailRight">
                    <div className="taskDetailRight-idWrapper">
                        <div className="taskDetailRight-id">{resx.get("ScheduleID.Header")}</div>
                        <div className="taskDetailRight-id-value">{props.scheduleId}</div>
                    </div>
                    <div className="taskDetailRight-common">
                        <div className="taskDetailRight-common-title">{resx.get("Servers.Header")}</div>
                        <div className="taskDetailRight-common-value">{props.servers}</div>
                    </div>
                    <div className="taskDetailRight-common">
                        <div className="taskDetailRight-common-title">{resx.get("ObjectDependencies.Header")}</div>
                        <div className="taskDetailRight-common-value">{props.objectDependencies}</div>
                    </div>
                    <div className="taskDetailRight-common">
                        <div className="taskDetailRight-common-title">{resx.get("TriggeredBy.Header")}</div>
                        <div className="taskDetailRight-common-value">{props.scheduleSource}</div>
                    </div>
                    <div className="taskDetailRight-common">
                        <div className="taskDetailRight-common-title">{resx.get("Thread.Header")}</div>
                        <div className="taskDetailRight-common-value">{props.threadId}</div>
                    </div>
                </div>
            </div>
        );
    }
}

RightPane.propTypes = {
    scheduleId: PropTypes.number,
    servers: PropTypes.string,
    objectDependencies: PropTypes.string,
    threadId: PropTypes.number,
    scheduleSource: PropTypes.string,
    onEnter: PropTypes.func,
    index: PropTypes.number
};

export default RightPane;