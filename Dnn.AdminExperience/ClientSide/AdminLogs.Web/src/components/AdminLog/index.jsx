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
		logger.debug('>>> Entering constructor()');
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
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug(`constructor(): canEdit → ${canEdit}`);
        isHost = util.settings.isHost;
		logger.debug('<<< Exiting constructor()');
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        const {props} = this;
        props.dispatch(LogActions.getPortalList(util.settings.isHost, (dataPortal) => {
            let portalList = Object.assign([], dataPortal.Results);
			logger.debug(`UNSAFE_componentWillMount(): portalList → ${portalList}`);
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
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }
    getLogTypes() {
		logger.debug('>>> Entering getLogTypes()');
        const {props} = this;
        props.dispatch(LogActions.getLogTypeList((dataLog) => {
            let logTypeList = Object.assign([], dataLog.Results);
			logger.debug(`getLogTypes(): logTypeList → ${logTypeList}`);
            let currentLogType = logTypeList[0].LogTypeFriendlyName;
            let currentLogTypeKey = logTypeList[0].LogTypeKey;
            this.setState({
                logTypeList,
                currentLogType,
                currentLogTypeKey
            }, () => {
                props.dispatch(LogActions.getLogList(this.getNextPage()));
            });
			logger.debug('<<< Exiting getLogTypes()');
        }));
    }
    getNextPage() {
		logger.debug('>>> Entering getNextPage()');
        const {state} = this;
        if (state.currentPortalId === -1 || state.currentPortalId === "-1") {
            return {
                pageIndex: state.pageIndex || 0,
                pageSize: state.pageSize,
                logType: state.currentLogTypeKey
            };
        }
        else {
			logger.debug('<<< Exiting getNextPage()');
            return {
                portalId: state.currentPortalId,
                pageIndex: state.pageIndex || 0,
                pageSize: state.pageSize,
                logType: state.currentLogTypeKey
            };
        }
    }

    onSelectAll() {
		logger.debug('>>> Entering onSelectAll()');
        const {props} = this;
        if (props.excludedRowIds.length == 0) {
            props.dispatch(LogActions.deselectAll());
        }
        else {
            props.dispatch(LogActions.selectAll());
			logger.debug('<<< Exiting onSelectAll()');
        }
    }

    onClearLog() {
		logger.debug('>>> Entering onClearLog()');
        const {props} = this;
        util.utilities.confirm(Localization.get("ClearLog"), Localization.get("yes"), Localization.get("no"), () => {
            let getNextPageParam = this.getNextPage();
			logger.debug(`onClearLog(): getNextPageParam → ${getNextPageParam}`);

            props.dispatch(LogActions.clearLog(() => {
                props.dispatch(LogActions.getLogList(getNextPageParam));
				logger.debug('<<< Exiting onClearLog()');
            }));
        });
    }

    onDeleteLogItems() {
		logger.debug('>>> Entering onDeleteLogItems()');
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
			logger.debug('<<< Exiting onDeleteLogItems()');
        }
    }

    toggleEmailPanel() {
		logger.debug('>>> Entering toggleEmailPanel()');
        this.setState({
            emailPanelOpen: !this.state.emailPanelOpen
        });
		logger.debug('<<< Exiting toggleEmailPanel()');
    }

    onSelectPortal(option) {
		logger.debug(`>>> Entering onSelectPortal(option=${option})`);
        const {props, state} = this;
        if (option.value !== state.currentPortalId) {
            this.setState({
                currentPortal: option.label,
                currentPortalId: option.value,
                pageIndex: 0
            }, () => {
                props.dispatch(LogActions.getLogList(this.getNextPage()));
            });
			logger.debug(`<<< Exiting onSelectPortal(option=${option})`);
        }
    }

    onSelectLogType(option) {
		logger.debug(`>>> Entering onSelectLogType(option=${option})`);
        const {props, state} = this;
        if (option.value !== state.currentLogTypeKey) {
            this.setState({
                currentLogType: option.label,
                currentLogTypeKey: option.value,
                pageIndex: 0
            }, () => {
                props.dispatch(LogActions.getLogList(this.getNextPage()));
            });
			logger.debug(`<<< Exiting onSelectLogType(option=${option})`);
        }
    }

    renderLogListHeader() {
		logger.debug('>>> Entering renderLogListHeader()');
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

		logger.debug(`renderLogListHeader(): tableHeaders → ${tableHeaders}`);
        const isDeselectState = props.excludedRowIds.length == 0 && props.excludedRowIds.length || !props.excludedRowIds.length == 0 && props.selectedRowIds.length;
        const checkboxClassName = "checkbox" + (isDeselectState ? " deselect-state" : "");
        tableHeaders.unshift(<div key={"selector" + "999999"} className="logHeader logHeader-Checkbox" data-index="0">
            <div className={checkboxClassName}>
                <Checkbox value={props.excludedRowIds.length === 0 && props.selectedRowIds.length > 0 || isDeselectState} onChange={this.onSelectAll.bind(this) } />
            </div>
        </div>);

		logger.debug('<<< Exiting renderLogListHeader()');
        return <div className="logHeader-wrapper">{tableHeaders}</div>;
    }


    renderedLogList() {
		logger.debug('>>> Entering renderedLogList()');
        const {props} = this;
		logger.debug('<<< Exiting renderedLogList()');
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
		logger.debug('>>> Entering isLastPage()');
        const {props, state} = this;
        let total = Math.ceil(props.totalCount / state.pageSize);
		logger.debug(`isLastPage(): total → ${total}`);
        let current = state.pageIndex * state.pageSize / state.pageSize + 1;
		logger.debug('<<< Exiting isLastPage()');
        return total === current ? true : false;
    }

    onPageChange(currentPage, pageSize) {
		logger.debug(`>>> Entering onPageChange(currentPage=${currentPage},pageSize=${pageSize})`);
        let {state, props} = this;
        if (pageSize !== undefined && state.pageSize !== pageSize) {
            state.pageSize = pageSize;
        }
        state.pageIndex = currentPage;
        this.setState({
            state
        }, () => {
            let getNextPageParam = this.getNextPage();
			logger.debug(`onPageChange(currentPage=${currentPage},pageSize=${pageSize}): getNextPageParam → ${getNextPageParam}`);
            props.dispatch(LogActions.getLogList(getNextPageParam));
        });
		logger.debug(`onPageChange(currentPage=${currentPage},pageSize=${pageSize}): state.pageSize → ${state.pageSize}`);
		logger.debug(`<<< Exiting onPageChange(currentPage=${currentPage},pageSize=${pageSize})`);
    }

    renderPager() {
		logger.debug('>>> Entering renderPager()');
        const {props, state} = this;
		logger.debug('<<< Exiting renderPager()');
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
		logger.debug('>>> Entering renderedLogLegend()');
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

		logger.debug(`renderedLogLegend(): legend → ${legend}`);
		logger.debug('<<< Exiting renderedLogLegend()');
        return <div className="logLegend-wrapper">{legend}</div>;
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
        let portalOptions = createPortalOptions(state.portalList);
        let logTypeOptions = createLogTypeOptions(state.logTypeList);
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
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
