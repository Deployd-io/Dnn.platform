import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Collapsible, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import resx from "../../../resources";
import {
    task as TaskActions
} from "../../../actions";
import util from "../../../utils";
import CheckmarkIcon from "./../../svg/checkmark.svg";
import HistoryIcon from "./../../svg/history.svg";
const logger = console;

class SchedulerRow extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            panelOpened: -1
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    toggle(index) {
		logger.error(`>>> Entering toggle(index=${index})`, { "event.action": "toggle", "logrite.index": index });
        const { props } = this;
        if (props.settingsClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(TaskActions.cancelSettingsClientModified());
                this.toggleInternal(index);
            });
        }
        else {
            this.toggleInternal(index);
			logger.warn(`<<< Exiting toggle(index=${index})`, { "event.action": "toggle", "logrite.index": index });
        }
    }

    toggleInternal(index) {
		logger.error(`>>> Entering toggleInternal(index=${index})`, { "event.action": "toggleInternal", "logrite.index": index });
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            if (this.state.panelOpened !== index) {
                this.setState({
                    panelOpened: index
                });
                this.props.OpenCollapse(this.props.id, index);
            }
            else {
                this.props.Collapse();
            }
        }
        else {
            this.setState({
                panelOpened: index
            });
            this.props.OpenCollapse(this.props.id, index);
			logger.warn(`<<< Exiting toggleInternal(index=${index})`, { "event.action": "toggleInternal", "logrite.index": index });
        }
    }


    getEnabledDisplay() {
		logger.error('>>> Entering getEnabledDisplay()', { "event.action": "getEnabledDisplay" });
        if (this.props.id !== "add") {
            if (this.props.enabled) {
                return <div className="checkMarkIcon"><CheckmarkIcon /></div>;
            }
            else return <span>&nbsp; </span>;
        }
        else return <span>-</span>;
    }


    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={"collapsible-component-scheduler" + (opened ? " row-opened" : "")}>
                <div className={"collapsible-header-scheduler " + !opened} >
                    <div className={"row"}>
                        <div title={props.name} className="schedule-item item-row-name">
                            {props.name}&nbsp; </div>
                        <div className="schedule-item item-row-frequency">
                            {props.frequency}</div>
                        <div className="schedule-item item-row-retryTimeLapse">
                            {props.retryTimeLapse}</div>
                        <div className="schedule-item item-row-nextStart">
                            {props.nextStart}&nbsp; </div>
                        <div className="schedule-item item-row-enabled">
                            {this.getEnabledDisplay()}</div>
                        {props.id !== "add" &&
                            <div className="schedule-item item-row-historyButton">
                                <div className={opened && props.panelIndex === 1 ? "history-icon-active" : "history-icon"} title={resx.get("ControlTitle_history")} onClick={this.toggle.bind(this, 1)}>
                                    <HistoryIcon />
                                </div>
                            </div>
                        }
                        <div className="schedule-item item-row-editButton">
                            <div className={opened && props.panelIndex === 0 ? "edit-icon-active" : "edit-icon"} title={resx.get("ControlTitle_edit")} onClick={this.toggle.bind(this, 0)}>
                                <SvgIcons.EditIcon />
                            </div>
                        </div>
                    </div>
                </div>
                <Collapsible autoScroll={true} isOpened={opened} className="collapsible-body-scheduler">{opened && props.children}</Collapsible>
            </div>
        );
    }
}

SchedulerRow.propTypes = {
    scheduleId: PropTypes.number,
    name: PropTypes.string,
    frequency: PropTypes.string,
    retryTimeLapse: PropTypes.string,
    enabled: PropTypes.bool,
    nextStart: PropTypes.string,
    disabled: PropTypes.bool,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    panelIndex: PropTypes.number,
    settingsClientModified: PropTypes.bool,
    dispatch: PropTypes.func,
    children: PropTypes.node
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        scheduleItemDetail: state.task.scheduleItemDetail,
        settingsClientModified: state.task.settingsClientModified
    };
}

SchedulerRow.defaultProps = {
    collapsed: true,
};

export default connect(mapStateToProps)(SchedulerRow);