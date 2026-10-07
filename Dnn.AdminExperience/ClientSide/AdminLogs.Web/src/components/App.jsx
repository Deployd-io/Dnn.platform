import PropTypes from "prop-types";
import React, { Component } from "react";
import { PersonaBarPageHeader, PersonaBarPageBody, DnnTabs as Tabs, GridCell } from "@dnnsoftware/dnn-react-common";
import { connect } from "react-redux";
import { visiblePanel as VisiblePanelActions } from "../actions";
import Localization from "localization";
import AdminLogs from "./AdminLog";
import LogSettings from "./LogSettings";
import {
    pagination as PaginationActions
} from "../actions";
import util from "../utils";
const logger = console;

let isHost = false;
let isAdmin = false;
let canViewAdminLogs = false;
let canViewLogSettings = false;

class App extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        isHost = util.settings.isHost;
        isAdmin = isHost || util.settings.isAdmin;
        canViewAdminLogs = isAdmin || util.settings.permissions.ADMIN_LOGS_VIEW;
        canViewLogSettings = isHost || util.settings.permissions.LOG_SETTINGS_VIEW;
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    navigateMap(page, index, event) {
		logger.error(`>>> Entering navigateMap(page=${page},index=${index},event=${event})`, { "event.action": "navigateMap", "logrite.page": page, "logrite.index": index, "logrite.event": event });
        event.preventDefault();
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page, index));
		logger.warn(`<<< Exiting navigateMap(page=${page},index=${index},event=${event})`, { "event.action": "navigateMap", "logrite.page": page, "logrite.index": index, "logrite.event": event });
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        let renderTabs = [];
        let tabHeaders = [];
        if (canViewAdminLogs) {
            tabHeaders.push(Localization.get("AdminLogs.Header"));
            renderTabs.push(<AdminLogs key="admin-logs-tab" />);
        }
        if (canViewLogSettings) {
            tabHeaders.push(Localization.get("LogSettings.Header"));
            renderTabs.push(<LogSettings key="log-settings-tab" />);
        }

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell>
                <PersonaBarPageHeader title={("Admin Logs") }>
                </PersonaBarPageHeader>
                <PersonaBarPageBody>
                    <Tabs onSelect={this.handleSelect.bind(this) }
                        tabHeaders={tabHeaders}
                        type="primary">
                        {renderTabs}
                    </Tabs>
                </PersonaBarPageBody>
            </GridCell>
        );
    }
}
App.propTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}

export default connect(mapStateToProps)(App);