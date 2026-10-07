import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import { Dropdown, Button, Pager } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import ApiTokenRow from "./ApiTokenRow";
import ApiTokenDetails from "./ApiTokenDetails";
import CreateApiToken from "./CreateApiToken";
import "./style.less";
const logger = console;

let scopeOptions = [];
let filterOptions = [];
let timespanOptions = [];
let isHost = false;

class ApiTokensPanelBody extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            portalList: [],
            currentPortal: "",
            currentPortalId: -2,
            currentScope: -2,
            apiTokens: [],
            apiTokenKeys: [],
            apiTokenKeysFiltered: [],
            filterByApiTokenKey: "",
            currentFilter: 0,
            pageIndex: 0,
            pageSize: 10,
            totalCount: 0,
            showNewApiToken: false,
        };
        isHost = util.settings.isHost;
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const { props } = this;
        if (isHost) {
            props.dispatch(SecurityActions.getPortalList(util.settings.isHost, (dataPortal) => {
                let portalList = Object.assign([], dataPortal.Results);
				logger.error(`UNSAFE_componentWillMount(): portalList → ${portalList}`, { "event.action": "UNSAFE_componentWillMount" });
                let currentPortalId = portalList[0].PortalId;
                let currentPortal = portalList[0].PortalName;
                this.setState({
                    portalList,
                    currentPortalId,
                    currentPortal
                });
            }));
        }
        props.dispatch(SecurityActions.getApiTokenKeys((data) => {
            this.setState({
                apiTokenKeys: data
            }, () => {
                this.resetApiFilterList();
            });
        }));
        if (props.apiTokenSettings) {
            this.setState({
                apiTokenSettings: props.apiTokenSettings
            });
        } else {
            props.dispatch(SecurityActions.getApiTokenSettings((data) => {
                let apiTokenSettings = Object.assign({}, data.Results.ApiTokenSettings);
                this.setState({
                    apiTokenSettings
                });
            }));
        }

        scopeOptions = [];
        scopeOptions.push({ "value": -2, "label": resx.get("All") });
        scopeOptions.push({ "value": 0, "label": resx.get("User") });
        if (util.settings.isAdmin) {
            scopeOptions.push({ "value": 1, "label": resx.get("Portal") });
        }
        if (util.settings.isHost) {
            scopeOptions.push({ "value": 2, "label": resx.get("Host") });
        }

        filterOptions = [];
        filterOptions.push({ "value": 0, "label": resx.get("All") });
        filterOptions.push({ "value": 1, "label": resx.get("Active") });
        filterOptions.push({ "value": 2, "label": resx.get("Revoked") });
        filterOptions.push({ "value": 3, "label": resx.get("Expired") });
        filterOptions.push({ "value": 4, "label": resx.get("RevokedOrExpired") });

        this.getData();
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }

    getData() {
		logger.error('>>> Entering getData()', { "event.action": "getData" });
        const crtPid = this.state.currentPortalId;
        this.props.dispatch(SecurityActions.getApiTokens(crtPid < 0 ? -2 : crtPid, this.state.currentFilter, this.state.filterByApiTokenKey, this.state.currentScope, this.state.pageIndex, this.state.pageSize, (data) => {
			logger.warn('<<< Exiting getData()', { "event.action": "getData" });
            this.setState({
                apiTokens: data.Data,
                totalCount: data.TotalCount
            });
        }));
    }

    getTimespanOptions() {
		logger.error('>>> Entering getTimespanOptions()', { "event.action": "getTimespanOptions" });
        if (timespanOptions.length === 0) {
            timespanOptions.push({ "value": 0, "label": resx.get("Days30") });
            timespanOptions.push({ "value": 1, "label": resx.get("Days60") });
            timespanOptions.push({ "value": 2, "label": resx.get("Days90") });
            timespanOptions.push({ "value": 3, "label": resx.get("Days180") });
            timespanOptions.push({ "value": 4, "label": resx.get("Years1") });
            timespanOptions.push({ "value": 5, "label": resx.get("Years2") });
            if (!isHost) {
                const max = this.state.apiTokenSettings.MaximumSiteTimespan;
                timespanOptions = timespanOptions.filter((item) => {
                    return item.value <= max;
                });
            }
        }
		logger.warn('<<< Exiting getTimespanOptions()', { "event.action": "getTimespanOptions" });
        return timespanOptions;
    }

    resetApiFilterList() {
		logger.error('>>> Entering resetApiFilterList()', { "event.action": "resetApiFilterList" });
        let newOptions = [];
        newOptions.push({ "value": "", "label": resx.get("All") });
        const f = this.state.currentScope === -2 ? this.state.apiTokenKeys : this.state.apiTokenKeys.filter((item) => {
            return item.Scope === this.state.currentScope;
        });
        f.forEach((item) => {
            if (newOptions.filter((option) => { return option.value === item.Key; }).length === 0)
                newOptions.push({ "value": item.Key, "label": item.Name });
        });
        this.setState({
            apiTokenKeysFiltered: newOptions,
            filterByApiTokenKey: ""
        }, () => {
            this.getData();
        });
		logger.warn('<<< Exiting resetApiFilterList()', { "event.action": "resetApiFilterList" });
    }

    onPageChange(currentPage, pageSize) {
		logger.error(`>>> Entering onPageChange(currentPage=${currentPage},pageSize=${pageSize})`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
        this.setState({
            pageIndex: currentPage,
            pageSize: pageSize
        }, () => {
            this.getData();
        });
		logger.warn(`<<< Exiting onPageChange(currentPage=${currentPage},pageSize=${pageSize})`, { "event.action": "onPageChange", "logrite.currentPage": currentPage, "logrite.pageSize": pageSize });
    }

    renderHeader() {
		logger.error('>>> Entering renderHeader()', { "event.action": "renderHeader" });
        const tableFields = [
            { name: "TokenName", width: 17 },
            { name: "Portal", width: 18 },
            { name: "Scope", width: 8 },
            { name: "CreatedOn", width: 10 },
            { name: "CreatedBy", width: 20 },
            { name: "ExpiresOn", width: 10 },
            { name: "ApiTokenStatus", width: 12 }
        ];

        let tableHeaders = tableFields.map((field, index) => {
            let className = "logHeader";
            return <div key={index} className={className} style={{ width: field.width.toString() + "%" }}>
                <span>{field.name !== "" && resx.get(field.name + ".Header")}&nbsp;</span>
            </div>;
        });

		logger.error(`renderHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderHeader" });
		logger.warn('<<< Exiting renderHeader()', { "event.action": "renderHeader" });
        return <div className="logHeader-wrapper">{tableHeaders}</div>;
    }

    renderList() {
        return this.state.apiTokens.map((item, index) => {
		logger.error('>>> Entering renderList()', { "event.action": "renderList" });
            return <ApiTokenRow
                key={index}
                apiToken={item}
                onClose={() => { this.getData(); }}
                scopes={scopeOptions}>
                <ApiTokenDetails
                    apiToken={item}
                    apiTokenKeys={this.state.apiTokenKeys}
                />
            </ApiTokenRow>;
		logger.warn('<<< Exiting renderList()', { "event.action": "renderList" });
        });
    }

    renderPager() {
		logger.error('>>> Entering renderPager()', { "event.action": "renderPager" });
        const { state } = this;
		logger.warn('<<< Exiting renderPager()', { "event.action": "renderPager" });
        return (
            <div className="logContainer">
                <div className="logPager">
                    <Pager
                        showStartEndButtons={false}
                        showPageSizeOptions={true}
                        showPageInfo={false}
                        numericCounters={4}
                        pageSize={state.pageSize}
                        totalRecords={state.totalCount}
                        onPageChanged={this.onPageChange.bind(this)}
                        pageSizeDropDownWithoutBorder={true}
                        pageSizeOptionText={resx.get("PageSizeOptions")}
                        summaryText={resx.get("PageSummary")}
                        culture={util.utilities.getCulture()}
                    />
                </div>
            </div>
        );
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { state } = this;
        if (state.apiTokenSettings && !state.apiTokenSettings.ApiTokensEnabled) {
            return (<div style={{ float: "left", width: "100%" }}>
                <div className="logContainer">
                    <div className="warningBox">
                        <div className="warningText">{resx.get("ApiTokensDisabled.Help")}</div>
                    </div>
                </div>
            </div>
            );
        }
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div style={{ float: "left", width: "100%" }}>
                <div className="tokenCommandBox">
                    <Button
                        type="danger"
                        onClick={() => {
                            util.utilities.confirm(resx.get("DeleteExpired.Confirm"), resx.get("Yes"), resx.get("No"), () => {
                                this.props.dispatch(SecurityActions.deleteExpiredAndRevokedApiTokens(() => {
                                    this.getData();
                                }));
                            });
                        }}>
                        {resx.get("DeleteExpired")}
                    </Button>
                    <Button
                        type="primary"
                        onClick={() => {
                            this.setState({
                                showAddApiToken: true
                            });
                        }}>
                        {resx.get("Add")}
                    </Button>
                </div>
                <div className="logContainer">
                    <div className="toolbar">
                        {isHost && state.portalList.length > 0 &&
                            <div className="security-filter-container">
                                <Dropdown
                                    value={state.currentPortalId}
                                    style={{ width: "100%" }}
                                    options={state.portalList.map((item) => {
                                        return { label: item.PortalName, value: item.PortalId };
                                    })}
                                    withBorder={false}
                                    onSelect={(value) => {
                                        let currentPortal = state.portalList.filter((item) => {
                                            return item.PortalId === value.value;
                                        })[0].PortalName;
                                        this.setState({
                                            currentPortalId: value.value,
                                            currentPortal
                                        }, () => {
                                            this.getData();
                                        });
                                    }}
                                />
                            </div>
                        }
                        {scopeOptions.length > 2 &&
                            <div className="security-filter-container">
                                <Dropdown
                                    value={state.currentScope}
                                    style={{ width: "100%" }}
                                    options={scopeOptions}
                                    withBorder={false}
                                    onSelect={(value) => {
                                        this.setState({
                                            currentScope: value.value
                                        }, () => {
                                            this.resetApiFilterList();
                                        });
                                    }}
                                />
                            </div>
                        }
                        <div className="security-filter-container">
                            <Dropdown
                                value={state.currentFilter}
                                style={{ width: "100%" }}
                                options={filterOptions}
                                withBorder={false}
                                onSelect={(value) => {
                                    this.setState({
                                        currentFilter: value.value
                                    }, () => {
                                        this.getData();
                                    });
                                }}
                            />
                        </div>
                        <div className="security-filter-container">
                            <Dropdown
                                value={state.filterByApiTokenKey}
                                style={{ width: "100%" }}
                                options={this.state.apiTokenKeysFiltered}
                                withBorder={false}
                                onSelect={(value) => {
                                    this.setState({
                                        filterByApiTokenKey: value.value
                                    }, () => {
                                        this.getData();
                                    });
                                }}
                            />
                        </div>
                    </div>
                    <div className="logContainerBox">
                        {this.state.showAddApiToken && <CreateApiToken
                            apiTokenKeys={this.state.apiTokenKeys}
                            scopeOptions={scopeOptions.slice(1)}
                            timespanOptions={this.getTimespanOptions()}
                            onCancel={() => {
                                this.setState({
                                    showAddApiToken: false
                                });
                            }}
                            onCreateApiToken={(name, scope, tokenTimespan, apiKeys) => {
                                this.setState({
                                    showAddApiToken: false
                                });
                                this.props.dispatch(SecurityActions.createApiToken(name, scope, tokenTimespan, apiKeys, (data) => {
                                    prompt(resx.get("ApiTokenCreated"), data);
                                    this.getData();
                                }));
                            }}
                        />}
                        {this.renderHeader()}
                        {this.renderList()}
                    </div>
                </div>
                {this.renderPager()}
            </div>
        );
    }
}

ApiTokensPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    apiTokens: PropTypes.array,
    cultureCode: PropTypes.string,
    apiTokenSettings: PropTypes.object,
    apiTokenSettingsClientModified: PropTypes.bool,
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        tabIndex: state.pagination.tabIndex,
        apiTokens: state.security.apiTokens,
        apiTokenSettings: state.security.apiTokenSettings,
        apiTokenSettingsClientModified: state.security.apiTokenSettingsClientModified,
    };
}

export default connect(mapStateToProps)(ApiTokensPanelBody);
