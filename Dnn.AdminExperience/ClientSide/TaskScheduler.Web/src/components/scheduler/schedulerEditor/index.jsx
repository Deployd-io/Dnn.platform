import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { 
    Label,
    Button,
    Switch,
    Dropdown,
    DatePicker,
    SingleLineInputWithError
} from "@dnnsoftware/dnn-react-common";
import History from "../../history";
import {
    task as TaskActions
} from "../../../actions";
import util from "../../../utils";
import resx from "../../../resources";
const logger = console;

const re = /^[1-9][0-9]?[0-9]?[0-9]?[0-9]?[0-9]?$/;
let retainHistoryNumOptions = [];
let timeLapseMeasurementOptions = [];
let attachToEventOptions = [];
let catchUpEnabledOptions = [];

class SchedulerEditor extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });

        this.state = {
            error: {
                name: true,
                frequency: true,
                retry: false
            },
            triedToSubmit: false
        };
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const { props } = this;
        if (props.scheduleId) {
            props.dispatch(TaskActions.getGetScheduleItem({
                scheduleId: props.scheduleId
            }));
        }

        retainHistoryNumOptions = [];
        retainHistoryNumOptions.push({ "value": "0", "label": resx.get("None") });
        retainHistoryNumOptions.push({ "value": "1", "label": "1" });
        retainHistoryNumOptions.push({ "value": "5", "label": "5" });
        retainHistoryNumOptions.push({ "value": "10", "label": "10" });
        retainHistoryNumOptions.push({ "value": "25", "label": "25" });
        retainHistoryNumOptions.push({ "value": "50", "label": "50" });
        retainHistoryNumOptions.push({ "value": "60", "label": "60" });
        retainHistoryNumOptions.push({ "value": "100", "label": "100" });
        retainHistoryNumOptions.push({ "value": "250", "label": "250" });
        retainHistoryNumOptions.push({ "value": "500", "label": "500" });
        retainHistoryNumOptions.push({ "value": "-1", "label": resx.get("All") });

        if (timeLapseMeasurementOptions.length === 0) {
            timeLapseMeasurementOptions.push({ "value": "s", "label": resx.get("Seconds") });
            timeLapseMeasurementOptions.push({ "value": "m", "label": resx.get("Minutes") });
            timeLapseMeasurementOptions.push({ "value": "h", "label": resx.get("Hours") });
            timeLapseMeasurementOptions.push({ "value": "d", "label": resx.get("Days") });
            timeLapseMeasurementOptions.push({ "value": "w", "label": resx.get("Weeks") });
            timeLapseMeasurementOptions.push({ "value": "mo", "label": resx.get("Months") });
            timeLapseMeasurementOptions.push({ "value": "y", "label": resx.get("Years") });
        }

        if (attachToEventOptions.length === 0) {
            attachToEventOptions.push({ "value": "", "label": resx.get("None") });
            attachToEventOptions.push({ "value": "APPLICATION_START", "label": "APPLICATION_START" });
        }

        if (catchUpEnabledOptions.length === 0) {
            catchUpEnabledOptions.push({ "value": "false", "label": resx.get("Disabled") });
            catchUpEnabledOptions.push({ "value": "true", "label": resx.get("Enabled.Label") });
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }

        this.setState({
            triedToSubmit: false,
            error: {}
        });
    }

    runSchedule() {
		logger.error('>>> Entering runSchedule()', { "event.action": "runSchedule" });
        const { props } = this;
        let { scheduleItemDetail } = props;
        props.dispatch(TaskActions.runScheduleItem(scheduleItemDetail, () => {
            util.utilities.notify(resx.get("RunNow"));
        }, () => {
            util.utilities.notify(resx.get("RunNowError"));
			logger.warn('<<< Exiting runSchedule()', { "event.action": "runSchedule" });
        }));
    }

    isEmptyDate(date) {
		logger.error(`>>> Entering isEmptyDate(date=${date})`, { "event.action": "isEmptyDate", "logrite.date": date });
		logger.warn(`<<< Exiting isEmptyDate(date=${date})`, { "event.action": "isEmptyDate", "logrite.date": date });
        return !date || new Date(date).getFullYear() < 1970;
    }

    getDefaultIfNull(obj, key, defaultValue) {
		logger.error(`>>> Entering getDefaultIfNull(obj=${obj},key=${key},defaultValue=${defaultValue})`, { "event.action": "getDefaultIfNull", "logrite.obj": obj, "logrite.key": key, "logrite.defaultValue": defaultValue });
        if (obj && key && obj[key]) {
            return obj[key].toString();
        }
		logger.warn(`<<< Exiting getDefaultIfNull(obj=${obj},key=${key},defaultValue=${defaultValue})`, { "event.action": "getDefaultIfNull", "logrite.obj": obj, "logrite.key": key, "logrite.defaultValue": defaultValue });
        return defaultValue;
    }

    getValue(selectKey) {
		logger.error(`>>> Entering getValue(selectKey=${selectKey})`, { "event.action": "getValue", "logrite.selectKey": selectKey });
        const { props } = this;
        switch (selectKey) {
            case "FriendlyName":
                return this.getDefaultIfNull(props.scheduleItemDetail, "FriendlyName", "");
            case "TypeFullName":
                return this.getDefaultIfNull(props.scheduleItemDetail, "TypeFullName", "");
            case "RetainHistoryNum":
                return this.getDefaultIfNull(props.scheduleItemDetail, "RetainHistoryNum", "0");
            case "Servers":
                return this.getDefaultIfNull(props.scheduleItemDetail, "Servers", "");
            case "ObjectDependencies":
                return this.getDefaultIfNull(props.scheduleItemDetail, "ObjectDependencies", "");
            case "ScheduleStartDate":
                if (!this.isEmptyDate(this.getDefaultIfNull(props.scheduleItemDetail, "ScheduleStartDate", null))) {
                    return new Date(props.scheduleItemDetail.ScheduleStartDate);
                }
                else {
                    return null;
                }
            case "TimeLapse":
                return this.getDefaultIfNull(props.scheduleItemDetail, "TimeLapse", "");
            case "TimeLapseMeasurement":
                return this.getDefaultIfNull(props.scheduleItemDetail, "TimeLapseMeasurement", "s");
            case "RetryTimeLapse":
                return this.getDefaultIfNull(props.scheduleItemDetail, "RetryTimeLapse", "");
            case "RetryTimeLapseMeasurement":
                return this.getDefaultIfNull(props.scheduleItemDetail, "RetryTimeLapseMeasurement", "s");
            case "AttachToEvent":
                return this.getDefaultIfNull(props.scheduleItemDetail, "AttachToEvent", "");
            case "CatchUpEnabled":
                return this.getDefaultIfNull(props.scheduleItemDetail, "CatchUpEnabled", "false");
            case "Enabled":
                return this.getDefaultIfNull(props.scheduleItemDetail, "Enabled", "false");
            default:
                break;
        }
		logger.warn(`<<< Exiting getValue(selectKey=${selectKey})`, { "event.action": "getValue", "logrite.selectKey": selectKey });
    }

    onSettingChange(key, event) {
		logger.error(`>>> Entering onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        let { state, props } = this;
        let scheduleItemDetail = Object.assign({}, props.scheduleItemDetail);
		logger.error(`onSettingChange(key=${key},event=${event}): scheduleItemDetail → ${scheduleItemDetail}`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });

        if (key === "ScheduleStartDate") {
            scheduleItemDetail[key] = event;
			logger.warn(`<<< Exiting onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        }
        else if (key === "CatchUpEnabled") {
            scheduleItemDetail[key] = event.value === "true" ? true : false;
        }
        else if (key === "RetainHistoryNum" || key === "TimeLapseMeasurement"
            || key === "RetryTimeLapseMeasurement" || key === "AttachToEvent") {
            scheduleItemDetail[key] = event.value;
        }
        else {
            scheduleItemDetail[key] = typeof (event) === "object" ? event.target.value : event;
        }
        if (scheduleItemDetail[key] === "" && key === "TypeFullName") {
            state.error["name"] = true;
        }
        else if (scheduleItemDetail[key] !== "" && key === "TypeFullName") {
            state.error["name"] = false;
        }
        if ((scheduleItemDetail[key] === "" || !re.test(scheduleItemDetail[key])) && key === "TimeLapse") {
            state.error["frequency"] = true;
        }
        else if (scheduleItemDetail[key] !== "" && re.test(scheduleItemDetail[key]) && key === "TimeLapse") {
            state.error["frequency"] = false;
        }
        if (scheduleItemDetail[key] === "" && key === "RetryTimeLapse") {
            state.error["retry"] = false;
        }
        else if (key === "RetryTimeLapse") {
            if (!re.test(scheduleItemDetail[key])) {
                state.error["retry"] = true;
            }
            else {
                state.error["retry"] = false;
            }
        }
        this.setState({
            scheduleItemDetail: scheduleItemDetail,
            triedToSubmit: false,
            error: state.error
        });

        props.dispatch(TaskActions.settingsClientModified(scheduleItemDetail));
    }

    onUpdateItem(event) {
		logger.error(`>>> Entering onUpdateItem(event=${event})`, { "event.action": "onUpdateItem", "logrite.event": event });
        event.preventDefault();
        const { props, state } = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.name || state.error.frequency || state.error.retry) {
            return;
        }

        this.handleRecommendedServers(() => props.onUpdate(props.scheduleItemDetail));
		logger.warn(`<<< Exiting onUpdateItem(event=${event})`, { "event.action": "onUpdateItem", "logrite.event": event });
    }

    compareIgnoreCase(prev, next) {
		logger.error(`>>> Entering compareIgnoreCase(prev=${prev},next=${next})`, { "event.action": "compareIgnoreCase", "logrite.prev": prev, "logrite.next": next });
		logger.warn(`<<< Exiting compareIgnoreCase(prev=${prev},next=${next})`, { "event.action": "compareIgnoreCase", "logrite.prev": prev, "logrite.next": next });
        return prev.toLowerCase().localeCompare(next.toLowerCase());
    }

    uniqueIgnoreCase(value, index, self) {
		logger.error(`>>> Entering uniqueIgnoreCase(value=${value},index=${index},self=${self})`, { "event.action": "uniqueIgnoreCase", "logrite.value": value, "logrite.index": index });
        const lower = self.map(x => (x || "").toLowerCase());
		logger.error(`uniqueIgnoreCase(value=${value},index=${index},self=${self}): lower → ${lower}`, { "event.action": "uniqueIgnoreCase", "logrite.value": value, "logrite.index": index });
        return lower.indexOf(value.toLowerCase()) === index;
    }

    normalizeServers(commaSeparatedServers) {
		logger.error(`>>> Entering normalizeServers(commaSeparatedServers=${commaSeparatedServers})`, { "event.action": "normalizeServers", "logrite.commaSeparatedServers": commaSeparatedServers });
		logger.warn(`<<< Exiting normalizeServers(commaSeparatedServers=${commaSeparatedServers})`, { "event.action": "normalizeServers", "logrite.commaSeparatedServers": commaSeparatedServers });
        return (commaSeparatedServers || "")
            .split(",")
            .map(x => x.trim())
            .filter(x => !!x)
            .filter(this.uniqueIgnoreCase)
            .sort(this.compareIgnoreCase)
            .join(", ");
    }

    onServersBlur() {
		logger.error('>>> Entering onServersBlur()', { "event.action": "onServersBlur" });
        const { scheduleItemDetail } = this.props;
        this.onSettingChange("Servers", {
            target: {
                value: this.normalizeServers(scheduleItemDetail.Servers)
            }
        });
		logger.warn('<<< Exiting onServersBlur()', { "event.action": "onServersBlur" });
    }

    handleRecommendedServers(callback) {
		logger.error(`>>> Entering handleRecommendedServers(callback=${callback})`, { "event.action": "handleRecommendedServers", "logrite.callback": callback });
        const { scheduleItemDetail } = this.props;
        const recommendedServers = scheduleItemDetail.RecommendedServers || [];

        const proceed = () => {
			logger.error('>>> Entering proceed()', { "event.action": "proceed" });
            if (callback && typeof(callback) === "function") {
                callback();
            }
			logger.warn('<<< Exiting proceed()', { "event.action": "proceed" });
        };

        if (scheduleItemDetail.Enabled && recommendedServers.length > 0) {
            const servers = this.normalizeServers(scheduleItemDetail.Servers);
			logger.error(`handleRecommendedServers(callback=${callback}): servers → ${servers}`, { "event.action": "handleRecommendedServers", "logrite.callback": callback });
            const recommended = this.normalizeServers(recommendedServers.join(", "));
			logger.error(`handleRecommendedServers(callback=${callback}): recommended → ${recommended}`, { "event.action": "handleRecommendedServers", "logrite.callback": callback });

            if (this.compareIgnoreCase(servers, recommended) !== 0) {
                const message = util.formatString(resx.get("RecommendServers"), recommended);
				logger.error(`handleRecommendedServers(callback=${callback}): message → ${message}`, { "event.action": "handleRecommendedServers", "logrite.callback": callback });
                const yes = resx.get("Yes");
				logger.error(`handleRecommendedServers(callback=${callback}): yes → ${yes}`, { "event.action": "handleRecommendedServers", "logrite.callback": callback });
                const no = resx.get("No");
				logger.error(`handleRecommendedServers(callback=${callback}): no → ${no}`, { "event.action": "handleRecommendedServers", "logrite.callback": callback });
                util.utilities.confirm(message, yes, no, () => proceed());
            } else {
                proceed();
            }
        } else {
            proceed();
			logger.warn(`<<< Exiting handleRecommendedServers(callback=${callback})`, { "event.action": "handleRecommendedServers", "logrite.callback": callback });
        }
    }

    onCancel() {
		logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
        const { props } = this;
        if (props.settingsClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(TaskActions.cancelSettingsClientModified());
                props.Collapse();
            });
        }
        else {
            props.Collapse();
			logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
        }
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props, state } = this;
        if (props.panelIndex === 1) {
            return <History pageSize={5} scheduleId={props.scheduleId} title={resx.get("HistoryModalTitle")} />;
        }

        if ((props.scheduleItemDetail !== undefined && props.scheduleItemDetail.ScheduleID === props.scheduleId) || props.id === "add") {
            const columnOne = <div className="container" key="columnOne">
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ width: 100 + "%", float: "left" }}
                        label={resx.get("plFriendlyName")}
                        error={false}
                        value={this.getValue("FriendlyName") || ""}
                        onChange={this.onSettingChange.bind(this, "FriendlyName")}
                    />
                </div>
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ width: 100 + "%", float: "left" }}
                        label={resx.get("plType") + " *"}
                        error={state.error.name && state.triedToSubmit}
                        errorMessage={resx.get("TypeRequired")}
                        value={this.getValue("TypeFullName") || ""}
                        onChange={this.onSettingChange.bind(this, "TypeFullName")}
                    />
                </div>
                <div className="editor-row divider">
                    <Label label={resx.get("plRetainHistoryNum")} style={{ margin: "0 0 5px 0" }} />
                    <Dropdown
                        options={retainHistoryNumOptions}
                        value={this.getValue("RetainHistoryNum")}
                        onSelect={this.onSettingChange.bind(this, "RetainHistoryNum")}
                    />
                </div>
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ width: 100 + "%", float: "left" }}
                        label={resx.get("Servers")}
                        error={false}
                        value={this.getValue("Servers") || ""}
                        onBlur={this.onServersBlur.bind(this)}
                        onChange={this.onSettingChange.bind(this, "Servers")}
                    />
                </div>
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ width: 100 + "%", float: "left" }}
                        label={resx.get("plObjectDependencies")}
                        error={false}
                        value={this.getValue("ObjectDependencies") || ""}
                        onChange={this.onSettingChange.bind(this, "ObjectDependencies")}
                    />
                </div>
            </div>;
            const columnTwo = <div className="container right-column" key="columnTwo">
                <div className="editor-row divider">
                    <Label label={resx.get("plScheduleStartDate")} style={{ margin: "0 0 5px 0" }} />
                    <DatePicker
                        date={this.getValue("ScheduleStartDate")}
                        updateDate={this.onSettingChange.bind(this, "ScheduleStartDate")}
                        isDateRange={false}
                        hasTimePicker={true}
                        showClearDateButton={true}
                    />
                </div>
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ float: "left", width: "47.5%", whiteSpace: "pre" }}
                        label={resx.get("plTimeLapse") + " *"}
                        error={state.error.frequency && state.triedToSubmit}
                        errorMessage={resx.get("TimeLapseRequired.ErrorMessage")}
                        value={this.getValue("TimeLapse") || ""}
                        onChange={this.onSettingChange.bind(this, "TimeLapse")}
                    />
                    <div className="text-section">&nbsp; </div>
                    <Dropdown
                        style={{ width: 46 + "%", float: "right", margin: "25px 0 0 0" }}
                        options={timeLapseMeasurementOptions}
                        value={this.getValue("TimeLapseMeasurement")}
                        onSelect={this.onSettingChange.bind(this, "TimeLapseMeasurement")}
                    />
                </div>
                <div className="editor-row divider">
                    <SingleLineInputWithError
                        withLabel={true}
                        style={{ width: "47.5%", float: "left", whiteSpace: "pre" }}
                        label={resx.get("plRetryTimeLapse")}
                        error={state.error.retry && state.triedToSubmit}
                        errorMessage={resx.get("RetryTimeLapseValidator.ErrorMessage")}
                        value={this.getValue("RetryTimeLapse") === "-1" ? "" : this.getValue("RetryTimeLapse")}
                        onChange={this.onSettingChange.bind(this, "RetryTimeLapse")}
                    />
                    <div className="text-section">&nbsp; </div>
                    <Dropdown
                        style={{ width: 46 + "%", float: "right", margin: "25px 0 0 0" }}
                        options={timeLapseMeasurementOptions}
                        value={this.getValue("RetryTimeLapseMeasurement")}
                        onSelect={this.onSettingChange.bind(this, "RetryTimeLapseMeasurement")}
                    />
                </div>
                <div className="editor-row divider">
                    <Label label={resx.get("plAttachToEvent")} style={{ margin: "0 0 5px 0" }} />
                    <Dropdown
                        options={attachToEventOptions}
                        value={this.getValue("AttachToEvent")}
                        onSelect={this.onSettingChange.bind(this, "AttachToEvent")}
                    />
                </div>
                <div className="editor-row divider">
                    <Label label={resx.get("plCatchUpEnabled")} style={{ margin: "0 0 5px 0" }} />
                    <Dropdown
                        options={catchUpEnabledOptions}
                        value={this.getValue("CatchUpEnabled")}
                        onSelect={this.onSettingChange.bind(this, "CatchUpEnabled")}
                    />
                </div>
                <div className="editor-row divider">
                    <Label label={resx.get("plEnabled")} style={{ width: "47.5%" }} />
                    <div className="right">
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={this.getValue("Enabled") === "true" ? true : false}
                            onChange={this.onSettingChange.bind(this, "Enabled")}
                        />
                    </div>
                </div>
            </div>;

            return ( 
                <div className="scheduler-setting-editor">
                    <div className="scheduler-item-container">
                        <div className="scheduler-item-column">
                            {columnOne}
                        </div>
                        <div className="scheduler-item-column">
                            {columnTwo}
                        </div>
                    </div>
                    <div className="buttons-box">
                        {props.scheduleId !== undefined && <Button type="danger" onClick={props.onDelete.bind(this, props.scheduleId)}>{resx.get("cmdDelete")}</Button>}
                        <Button
                            type="neutral"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        {props.scheduleId !== undefined && props.scheduleItemDetail.Enabled && props.enabled &&
                            <Button type="secondary" onClick={this.runSchedule.bind(this)}>{resx.get("cmdRun")}</Button>
                        }
                        <Button
                            disabled={!props.settingsClientModified}
                            type="primary"
                            onClick={this.onUpdateItem.bind(this)}>
                            {props.scheduleId !== undefined ? resx.get("Update") : resx.get("cmdSave")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

SchedulerEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    scheduleItemDetail: PropTypes.object,
    enabled: PropTypes.bool,
    serverList: PropTypes.array,
    scheduleId: PropTypes.number,
    Collapse: PropTypes.func,
    onDelete: PropTypes.func,
    onUpdate: PropTypes.func,
    id: PropTypes.string,
    settingsClientModified: PropTypes.bool,
    panelIndex: PropTypes.number
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        scheduleItemDetail: state.task.scheduleItemDetail,
        settingsClientModified: state.task.settingsClientModified
    };
}

export default connect(mapStateToProps)(SchedulerEditor);
