import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    task as TaskActions
} from "../../actions";
import TaskHistoryItemRow from "./taskHistoryItemRow";
import "./style.less";
import { Pager } from "@dnnsoftware/dnn-react-common";
import resx from "../../resources";
import HistoryIcon from "./../svg/history.svg";
const logger = console;

let pageSizeOptions = [];
let tableFields = [];

class HistoryPanelBody extends Component {
    constructor(props) {
        super();
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            taskHistoryList: [],
            scheduleId: -1,
            pageIndex: 0,
            pageSize: props.pageSize ? props.pageSize : 10,
            totalCount: 0
        };
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const {props, state} = this;
        props.dispatch(TaskActions.getScheduleItemHistory({ scheduleId: props.scheduleId, pageIndex: state.pageIndex, pageSize: state.pageSize }));

        tableFields = [];
        tableFields.push({ "name": resx.get("DescriptionColumn"), "id": "LogNotes" });
        tableFields.push({ "name": resx.get("RanOnServerColumn"), "id": "Server" });
        tableFields.push({ "name": resx.get("DurationColumn"), "id": "ElapsedTime" });
        tableFields.push({ "name": resx.get("SucceededColumn"), "id": "Succeeded" });
        tableFields.push({ "name": resx.get("StartEndColumn"), "id": "StartEnd" });

        pageSizeOptions = [];
        pageSizeOptions.push({ "value": "10", "label": "10 entries per page" });
        pageSizeOptions.push({ "value": "25", "label": "25 entries per page" });
        pageSizeOptions.push({ "value": "50", "label": "50 entries per page" });
        pageSizeOptions.push({ "value": "100", "label": "100 entries per page" });
        pageSizeOptions.push({ "value": "250", "label": "250 entries per page" });
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }

    onEnter(key) {
		logger.error(`>>> Entering onEnter(key=${key})`, { "event.action": "onEnter", "logrite.key": key });
        const { state } = this;
        alert("You pressed enter! My value is: " + state[key]);
		logger.warn(`<<< Exiting onEnter(key=${key})`, { "event.action": "onEnter", "logrite.key": key });
    }

    onPageChange(currentPage, pageSize) {
		logger.error(`>>> Entering onPageChange(currentPage=${currentPage},pageSize=${pageSize})`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
        let {state, props} = this;
        if (pageSize !== undefined && state.pageSize !== pageSize) {
			logger.debug(`onPageChange(currentPage=${currentPage},pageSize=${pageSize}): state.pageIndex → ${state.pageIndex}`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
            state.pageSize = pageSize;
			logger.debug(`onPageChange(currentPage=${currentPage},pageSize=${pageSize}): state.pageSize → ${state.pageSize}`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
        }
        state.pageIndex = currentPage;
        this.setState({
            state
        }, () => {
            props.dispatch(TaskActions.getScheduleItemHistory({ scheduleId: props.scheduleId, pageIndex: state.pageIndex, pageSize: state.pageSize }));
        });
		logger.warn(`<<< Exiting onPageChange(currentPage=${currentPage},pageSize=${pageSize})`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
    }

    renderedHistoryListHeader() {
		logger.error('>>> Entering renderedHistoryListHeader()', { "event.action": "renderedHistoryListHeader" });
        let tableHeaders = tableFields.map((field, index) => {
            let className = "historyHeader historyHeader-" + field.id;
            return <div className={className} key={"historyHeader-" + index}>
                <span>{field.name}</span>
            </div>;
        });

		logger.error(`renderedHistoryListHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderedHistoryListHeader" });
		logger.warn('<<< Exiting renderedHistoryListHeader()', { "event.action": "renderedHistoryListHeader" });
        return <div className="historyHeader-wrapper">{tableHeaders}</div>;
    }

    renderPager() {
		logger.error('>>> Entering renderPager()', { "event.action": "renderPager" });
        const {props, state} = this;
		logger.warn('<<< Exiting renderPager()', { "event.action": "renderPager" });
        return (
            <div className="taskHistoryList-pager">
                <Pager
                    showStartEndButtons={true}
                    showPageSizeOptions={true}
                    showPageInfo={false}
                    numericCounters={4}
                    pageSize={state.pageSize}
                    totalRecords={props.totalCount || 0}
                    onPageChanged={this.onPageChange.bind(this)}
                    pageSizeDropDownWithoutBorder={true}
                    pageSizeOptionText={resx.get("pageSizeOption")}
                    summaryText={resx.get("pagerSummary")}
                />
            </div>
        );
    }


    renderedHistoryList() {
		logger.error('>>> Entering renderedHistoryList()', { "event.action": "renderedHistoryList" });
        const {props} = this;
        if (props.taskHistoryList) {
			logger.warn('<<< Exiting renderedHistoryList()', { "event.action": "renderedHistoryList" });
            return props.taskHistoryList.map((term, index) => {
                return (
                    <TaskHistoryItemRow
                        friendlyName={term.FriendlyName}
                        logNotes={term.LogNotes}
                        server={term.Server}
                        elapsedTime={term.ElapsedTime}
                        succeeded={term.Succeeded}
                        startDate={term.StartDate}
                        endDate={term.EndDate}
                        nextStart={term.NextStart}
                        key={"taskHistoryItem-" + index}
                        closeOnClick={true}>
                    </TaskHistoryItemRow>
                );
            });
        }
    }


    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div>
                <div className="historyIcon"><HistoryIcon /></div>
                <div className="taskHistoryList-title">{props.title}</div>
                <div className="taskHistoryList-grid">
                    {this.renderedHistoryListHeader()}
                    {this.renderedHistoryList()}
                </div>
                {this.renderPager()}
            </div>
        );
    }
}

HistoryPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    taskHistoryList: PropTypes.array,
    scheduleId: PropTypes.number,
    title: PropTypes.string,
    totalCount: PropTypes.number,
    pageSize: PropTypes.number
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        taskHistoryList: state.task.taskHistoryList,
        tabIndex: state.pagination.tabIndex,
        totalCount: state.task.totalHistoryCount
    };
}

export default connect(mapStateToProps)(HistoryPanelBody);