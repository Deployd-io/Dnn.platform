import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Collapsible, Button, Select, SingleLineInputWithError } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import {
    task as TaskActions
} from "../../../actions";
import util from "../../../utils";
import resx from "../../../resources";
const logger = console;

const re = /^([0-9]|[1-9][0-9]|[1-9][0-9][0-9]|[1-9][0-4][0-4][0])$/;

class ModePanel extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            updateRequest: {
                SchedulerMode: "",
                SchedulerdelayAtAppStart: ""
            },
            error: {
                schedulerDelay: false
            },
            triedToSubmit: false,
            clientModified: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        let {props, state} = this;
        if (props.schedulerDelay === "" || !re.test(props.schedulerDelay)) {
            state.error["schedulerDelay"] = true;
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }
        this.setState({
            triedToSubmit: false,
            error: state.error,
            updateRequest: {
                SchedulerMode: props.schedulerMode,
                SchedulerdelayAtAppStart: props.schedulerDelay
            }
        });        
    }    

    onSave(event) {
		logger.error(`>>> Entering onSave(event=${event})`, { "event.action": "onSave", "logrite.event": event });
        event.preventDefault();
        const {props, state} = this;

        this.setState({
            triedToSubmit: true
        });

        if (state.error.schedulerDelay) {
            return;
        }

        props.dispatch(TaskActions.updateSchedulerSettings(state.updateRequest, () => {
            util.utilities.notify(resx.get("SchedulerUpdateSuccess"));
        }, () => {
            util.utilities.notify(resx.get("SchedulerUpdateError"));
        }));

        this.setState({
            clientModified: false
        });

        props.onClose();
		logger.warn(`<<< Exiting onSave(event=${event})`, { "event.action": "onSave", "logrite.event": event });
    }

    onValueChange(key, event) {
        let {state} = this;
        const value = event.target.value;
        const { updateRequest } = this.state;
        updateRequest[key] = value;

        if (!re.test(updateRequest[key]) && key === "SchedulerdelayAtAppStart") {
            state.error["schedulerDelay"] = true;
        }
        else if (re.test(updateRequest[key]) && key === "SchedulerdelayAtAppStart") {
            state.error["schedulerDelay"] = false;
        }

        this.setState({
            triedToSubmit: false,
            updateRequest,
            error: state.error,
            clientModified: true
        });
    }

    onClose() {
		logger.error('>>> Entering onClose()', { "event.action": "onClose" });
        const {props, state} = this;
        state.error["schedulerDelay"] = false;
        this.setState({
            error: state.error,
            clientModified: false
        });
        props.onClose();
		logger.warn('<<< Exiting onClose()', { "event.action": "onClose" });
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={props.isOpened ? "collapsible-content-mode" : "collapsible-content-mode-hidden"}>
                <Collapsible
                    className={props.className}
                    isOpened={props.isOpened}>
                    <div className="modepanel-content-outer-wrapper">
                        <div className="modepanel-content-wrapper" style={{ height: "calc(100% - 100px)" }}>
                            <div className="">
                                <div className="editor-row divider">
                                    <label>{resx.get("plSchedulerMode")}</label>
                                    <Select
                                        onChange={this.onValueChange.bind(this, "SchedulerMode")}
                                        options={props.schedulerModeOptions}
                                        value={state.updateRequest.SchedulerMode} />
                                </div>
                                <div className="editor-row divider">
                                    <label>{resx.get("plScheduleAppStartDelay")}</label>
                                    <SingleLineInputWithError
                                        inputStyle={{ margin: "0" }}
                                        withLabel={false}
                                        error={this.state.error.schedulerDelay && this.state.triedToSubmit}
                                        errorMessage={resx.get("ScheduleAppStartDelayValidation")}
                                        value={state.updateRequest.SchedulerdelayAtAppStart}
                                        onChange={this.onValueChange.bind(this, "SchedulerdelayAtAppStart")}
                                    />
                                </div>
                                <div className="action-buttons">
                                    <Button type="secondary" onClick={this.onClose.bind(this)}>{resx.get("Cancel")}</Button>
                                    <Button
                                        type="primary"
                                        disabled={!state.clientModified}
                                        onClick={this.onSave.bind(this)}>{resx.get("Update")}
                                    </Button>
                                </div>
                            </div>
                        </div>
                    </div>
                </Collapsible>
            </div>
        );
    }
}

ModePanel.propTypes = {
    label: PropTypes.string,
    className: PropTypes.string,
    collapsibleWidth: PropTypes.number,
    collapsibleHeight: PropTypes.number,
    scrollAreaStyle: PropTypes.object,
    children: PropTypes.node,
    isOpened: PropTypes.bool,
    onClose: PropTypes.func.isRequired,
    schedulerMode: PropTypes.string.isRequired,
    schedulerDelay: PropTypes.number.isRequired,
    schedulerModeOptions: PropTypes.array.isRequired,
    clientModified: PropTypes.bool,
    dispatch: PropTypes.func
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        schedulerDelay: state.task.schedulerDelay,
        schedulerMode: state.task.schedulerMode,
        clientModified: state.task.settingsClientModified
    };
}

export default connect(mapStateToProps)(ModePanel);