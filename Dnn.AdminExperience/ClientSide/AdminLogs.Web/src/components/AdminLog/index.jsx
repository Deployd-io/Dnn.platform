import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import { log as LogActions } from "../../actions";
import LogItemRow from "./LogItemRow";
import EmailPanel from "./EmailPanel";
import Html from "../Html";
import { Checkbox, Dropdown, Pager, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import Localization from "localization";
const logger = console;
import {
    createPortalOptions,
    createLogTypeOptions
} from "../../reducerHelpers";


let pageSizeOptions = [];
let canEdit = false;
let isHost = false;
/*eslint-disable eqeqeq*/
class AdminLogPanelBody extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            logList: [],
            portalList: [],
            logTypeList: [],
            allRowIds: [],
            emailPanelOpen: false,
            currentPortal: "",
            currentPortalId: "-2",
            currentLogType: "",
            currentLogTypeKey: "",
            pageIndex: 0,
            pageSize: 10,
            selectedRowIds: [],
            excludedRowIds: [],
            totalCount: 0
        };
        canEdit = util.settings.isHost || util.settings.permissions.ADMIN_LOGS_EDIT;
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
        isHost = util.settings.isHost;
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const {props} = this;
        props.dispatch(LogActions.getPortalList(util.settings.isHost, (dataPortal) => {
            let portalList = Object.assign([], dataPortal.Results);
			logger.error(`UNSAFE_componentWillMount(): portalList → ${portalList}`, { "event.action": "UNSAFE_componentWillMount" });
            let currentPortalId = portalList[0].PortalId;
            let currentPortal = portalList[0].PortalName;
            this.setState({
                portalList,
                currentPortalId,
                currentPortal
            }, () => {
                this.getLogTypes();
            });
        }));

        pageSizeOptions = [];
        pageSizeOptions.push({ "value": 10, "label": "10 entries per page" });
        pageSizeOptions.push({ "value": 25, "label": "25 entries per page" });
        pageSizeOptions.push({ "value": 50, "label": "50 entries per page" });
        pageSizeOptions.push({ "value": 100, "label": "100 entries per page" });
        pageSizeOptions.push({ "value": 250, "label": "250 entries per page" });
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }
    getLogTypes() {
		logger.error('>>> Entering getLogTypes()', { "event.action": "getLogTypes" });
        const {props} = this;
        props.dispatch(LogActions.getLogTypeList((dataLog) => {
            let logTypeList = Object.assign([], dataLog.Results);
			logger.error(`getLogTypes(): logTypeList → ${logTypeList}`, { "event.action": "getLogTypes" });
            let currentLogType = logTypeList[0].LogTypeFriendlyName;
            let currentLogTypeKey = logTypeList[0].LogTypeKey;
            this.setState({
                logTypeList,
                currentLogType,
                currentLogTypeKey
            }, () => {
                props.dispatch(LogActions.getLogList(this.getNextPage()));
            });
			logger.warn('<<< Exiting getLogTypes()', { "event.action": "getLogTypes" });
        }));
    }
    getNextPage() {
		logger.error('>>> Entering getNextPage()', { "event.action": "getNextPage" });
        const {state} = this;
        if (state.currentPortalId === -1 || state.currentPortalId === "-1") {
            return {
                pageIndex: state.pageIndex || 0,
                pageSize: state.pageSize,
                logType: state.currentLogTypeKey
            };
        }
        else {
			logger.warn('<<< Exiting getNextPage()', { "event.action": "getNextPage" });
            return {
                portalId: state.currentPortalId,
                pageIndex: state.pageIndex || 0,
                pageSize: state.pageSize,
                logType: state.currentLogTypeKey
            };
        }
    }

    onSelectAll() {
		logger.error('>>> Entering onSelectAll()', { "event.action": "onSelectAll" });
        const {props} = this;
        if (props.excludedRowIds.length == 0) {
            props.dispatch(LogActions.deselectAll());
        }
        else {
            props.dispatch(LogActions.selectAll());
			logger.warn('<<< Exiting onSelectAll()', { "event.action": "onSelectAll" });
        }
    }

    onClearLog() {
		logger.error('>>> Entering onClearLog()', { "event.action": "onClearLog" });
        const {props} = this;
        util.utilities.confirm(Localization.get("ClearLog"), Localization.get("yes"), Localization.get("no"), () => {
            let getNextPageParam = this.getNextPage();
			logger.error(`onClearLog(): getNextPageParam → ${getNextPageParam}`, { "event.action": "onClearLog" });

            props.dispatch(LogActions.clearLog(() => {
                props.dispatch(LogActions.getLogList(getNextPageParam));
				logger.warn('<<< Exiting onClearLog()', { "event.action": "onClearLog" });
            }));
        });
    }

    onDeleteLogItems() {
		logger.error('>>> Entering onDeleteLogItems()', { "event.action": "onDeleteLogItems" });
        const {props, state} = this;
        if (props.selectedRowIds.length > 0) {
            util.utilities.confirm(Localization.get("LogDeleteWarning"), Localization.get("yes"), Localization.get("no"), () => {
                if (props.excludedRowIds.length == 0 && this.isLastPage()) {
                    this.setState({
                        pageIndex: state.pageIndex > 0 ? state.pageIndex - 1 : 0
                    }, () => {
                        let getNextPageParam = this.getNextPage();
                        props.dispatch(LogActions.deleteLogItems(props.selectedRowIds, () => {
                            props.dispatch(LogActions.getLogList(getNextPageParam));
                        }));
                    });
                }
                else {
                    let getNextPageParam = this.getNextPage();
                    props.dispatch(LogActions.deleteLogItems(props.selectedRowIds, () => {
                        props.dispatch(LogActions.getLogList(getNextPageParam));
                    }));
                }
            });
        }
        else {
            util.utilities.notifyError(Localization.get("SelectException"));
			logger.warn('<<< Exiting onDeleteLogItems()', { "event.action": "onDeleteLogItems" });
        }
    }

    toggleEmailPanel() {
		logger.error('>>> Entering toggleEmailPanel()', { "event.action": "toggleEmailPanel" });
        this.setState({
            emailPanelOpen: !this.state.emailPanelOpen
        });
		logger.warn('<<< Exiting toggleEmailPanel()', { "event.action": "toggleEmailPanel" });
    }

    onSelectPortal(option) {
		logger.error(`>>> Entering onSelectPortal(option=${option})`, { "event.action": "onSelectPortal", "logrite.option": option });
        const {props, state} = this;
        if (option.value !== state.currentPortalId) {
            this.setState({
                currentPortal: option.label,
                currentPortalId: option.value,
                pageIndex: 0
            }, () => {
                props.dispatch(LogActions.getLogList(this.getNextPage()));
            });
			logger.warn(`<<< Exiting onSelectPortal(option=${option})`, { "event.action": "onSelectPortal", "logrite.option": option });
        }
    }

    onSelectLogType(option) {
		logger.error(`>>> Entering onSelectLogType(option=${option})`, { "event.action": "onSelectLogType", "logrite.option": option });
        const {props, state} = this;
        if (option.value !== state.currentLogTypeKey) {
            this.setState({
                currentLogType: option.label,
                currentLogTypeKey: option.value,
                pageIndex: 0
            }, () => {
                props.dispatch(LogActions.getLogList(this.getNextPage()));
            });
			logger.warn(`<<< Exiting onSelectLogType(option=${option})`, { "event.action": "onSelectLogType", "logrite.option": option });
        }
    }

    renderLogListHeader() {
		logger.error('>>> Entering renderLogListHeader()', { "event.action": "renderLogListHeader" });
        const {props} = this;
        if (!props.excludedRowIds) {
            return;
        }

        const tableFields = [
            { "name": "", "id": "LogTypeCSSClass" },
            { "name": Localization.get("Date"), "id": "LogCreateDate" },
            { "name": Localization.get("Type"), "id": "LogTypeFriendlyName" },
            { "name": Localization.get("Username"), "id": "LogUserName" },
            { "name": Localization.get("Portal"), "id": "LogPortalName" },
            { "name": Localization.get("Summary"), "id": "Summary" }
        ];

        let tableHeaders = tableFields.map((field) => {
            let className = "logHeader logHeader-" + field.id;
            return <div key={field.id} className={className}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });

		logger.error(`renderLogListHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderLogListHeader" });
        const isDeselectState = props.excludedRowIds.length == 0 && props.excludedRowIds.length || !props.excludedRowIds.length == 0 && props.selectedRowIds.length;
        const checkboxClassName = "checkbox" + (isDeselectState ? " deselect-state" : "");
        tableHeaders.unshift(<div key={"selector" + "999999"} className="logHeader logHeader-Checkbox" data-index="0">
            <div className={checkboxClassName}>
                <Checkbox value={props.excludedRowIds.length === 0 && props.selectedRowIds.length > 0 || isDeselectState} onChange={this.onSelectAll.bind(this) } />
            </div>
        </div>);

		logger.warn('<<< Exiting renderLogListHeader()', { "event.action": "renderLogListHeader" });
        return <div className="logHeader-wrapper">{tableHeaders}</div>;
    }


    renderedLogList() {
		logger.error('>>> Entering renderedLogList()', { "event.action": "renderedLogList" });
        const {props} = this;
		logger.warn('<<< Exiting renderedLogList()', { "event.action": "renderedLogList" });
        return props.logList.map((term, index) => {
            return (
                <LogItemRow
                    cssClass={term.LogTypeCSSClass}
                    logId={term.LogGUID}
                    allRowIds={this.props.logList.map((row) => row.LogGUID) }
                    typeName={term.LogTypeFriendlyName}
                    createDate={term.LogCreateDate}
                    userName={term.LogUserName}
                    portalName={term.LogPortalName}
                    summary={term.Summary}
                    index={index}
                    key={"logTerm-" + index}
                    closeOnClick={true}>
                    <div className="log-detail"><Html html={term.LogProperties} /></div>
                </LogItemRow>
            );
        });
    }

    isLastPage() {
		logger.error('>>> Entering isLastPage()', { "event.action": "isLastPage" });
        const {props, state} = this;
        let total = Math.ceil(props.totalCount / state.pageSize);
		logger.error(`isLastPage(): total → ${total}`, { "event.action": "isLastPage" });
        let current = state.pageIndex * state.pageSize / state.pageSize + 1;
		logger.warn('<<< Exiting isLastPage()', { "event.action": "isLastPage" });
        return total === current ? true : false;
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
            let getNextPageParam = this.getNextPage();
			logger.error(`onPageChange(currentPage=${currentPage},pageSize=${pageSize}): getNextPageParam → ${getNextPageParam}`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
            props.dispatch(LogActions.getLogList(getNextPageParam));
        });
		logger.warn(`<<< Exiting onPageChange(currentPage=${currentPage},pageSize=${pageSize})`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
    }

    renderPager() {
		logger.error('>>> Entering renderPager()', { "event.action": "renderPager" });
        const {props, state} = this;
		logger.warn('<<< Exiting renderPager()', { "event.action": "renderPager" });
        return (
            <div className="logPager">
                <Pager
                    showStartEndButtons={false}
                    showPageSizeOptions={true}
                    showPageInfo={false}
                    numericCounters={4}
                    pageSize={state.pageSize}
                    totalRecords={props.totalCount}
                    onPageChanged={this.onPageChange.bind(this) }
                    pageSizeDropDownWithoutBorder={true}
                    pageSizeOptionText={"{0} results per page"}
                    summaryText={"Showing {0}-{1} of {2} results"}
                    culture={util.utilities.getCulture() }
                />
            </div>
        );
    }

    renderedLogLegend() {
		logger.error('>>> Entering renderedLogLegend()', { "event.action": "renderedLogLegend" });
        const legendItems = [
            { "name": Localization.get("ExceptionCode"), "id": "Exception" },
            { "name": Localization.get("ItemCreatedCode"), "id": "ItemCreated" },
            { "name": Localization.get("ItemUpdatedCode"), "id": "ItemUpdated" },
            { "name": Localization.get("ItemDeletedCode"), "id": "ItemDeleted" },
            { "name": Localization.get("SuccessCode"), "id": "OperationSuccess" },
            { "name": Localization.get("FailureCode"), "id": "OperationFailure" },
            { "name": Localization.get("AdminOpCode"), "id": "GeneralAdminOperation" },
            { "name": Localization.get("AdminAlertCode"), "id": "AdminAlert" },
            { "name": Localization.get("HostAlertCode"), "id": "HostAlert" },
            { "name": Localization.get("SecurityException"), "id": "SecurityException" }
        ];
        let legend = legendItems.map((item) => {
            return <div key={item.id} className="logLegend-item">
                <div className={item.id}>
                    <span></span>
                </div>
                <div>
                    <span>{item.name}</span>
                </div>
            </div>;
        });

		logger.error(`renderedLogLegend(): legend → ${legend}`, { "event.action": "renderedLogLegend" });
		logger.warn('<<< Exiting renderedLogLegend()', { "event.action": "renderedLogLegend" });
        return <div className="logLegend-wrapper">{legend}</div>;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
        let portalOptions = createPortalOptions(state.portalList);
        let logTypeOptions = createLogTypeOptions(state.logTypeList);
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div style={{ margin: "0 20px", float: "left" }}>
                <div className="toolbar">
                    {state.portalList.length > 0 &&
                        <div className="adminlogs-filter-container">
                            <Dropdown
                                value={state.currentPortalId}
                                style={{ width: "100%" }}
                                options={portalOptions}
                                withBorder={false}
                                onSelect={this.onSelectPortal.bind(this) }
                            />
                        </div>
                    }
                    {state.logTypeList.length > 0 &&
                        <div className="adminlogs-filter-container">
                            <Dropdown
                                value={state.currentLogTypeKey}
                                style={{ width: "100%" }}
                                options={logTypeOptions}
                                withBorder={false}
                                onSelect={this.onSelectLogType.bind(this) }
                            />
                        </div>
                    }
                    {(canEdit || util.settings.isAdmin) &&
                        <div className="toolbar-button toolbar-button-actions" style={{ width: "15%", paddingRight: 0, textAlign: "right" }}>
                            <div onClick={this.toggleEmailPanel.bind(this) }>
                                <TextOverflowWrapper
                                    text={Localization.get("btnEmail") }
                                    maxWidth={100}
                                    enabled={canEdit || util.settings.isAdmin}
                                />
                            </div>
                            <div className="collapsible-content">
                                <EmailPanel
                                    className="adminlogs-email-panel"
                                    isOpened={state.emailPanelOpen}
                                    logIds={props.selectedRowIds}
                                    onCloseEmailPanel={this.toggleEmailPanel.bind(this) }>
                                </EmailPanel>
                            </div>
                        </div>
                    }
                    {isHost && <div className="toolbar-button toolbar-button-actions" onClick={this.onDeleteLogItems.bind(this) } style={{ width: "18%" }}>
                        <TextOverflowWrapper
                            text={Localization.get("btnDelete") }
                            maxWidth={115}
                        />
                    </div>
                    }
                    {isHost && <div className="toolbar-button toolbar-button-actions" onClick={this.onClearLog.bind(this) } style={{ borderLeft: "none", width: "15%" }}>
                        <TextOverflowWrapper
                            text={Localization.get("btnClear") }
                            maxWidth={90}
                        />
                    </div>
                    }
                </div>
                <div className="logContainer">
                    <div className="logContainerBox">
                        {this.renderLogListHeader() }
                        {this.renderedLogList() }
                    </div>
                </div>
                {this.renderPager() }
                {this.renderedLogLegend() }
            </div>
        );
    }
}

AdminLogPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    logList: PropTypes.array,
    logTypeList: PropTypes.array,
    portalList: PropTypes.array,
    selectedRowIds: PropTypes.array.isRequired,
    allRowIds: PropTypes.array.isRequired,
    excludedRowIds: PropTypes.array.isRequired,
    totalCount: PropTypes.number
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        logList: state.log.logList,
        logTypeList: state.log.logTypeList,
        portalList: state.log.portalList,
        tabIndex: state.pagination.tabIndex,
        selectedRowIds: state.log.selectedRowIds,
        excludedRowIds: state.log.excludedRowIds,
        totalCount: state.log.totalCount
    };
}

export default connect(mapStateToProps)(AdminLogPanelBody);
