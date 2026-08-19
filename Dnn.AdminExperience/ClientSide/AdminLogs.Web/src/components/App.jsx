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
		logger.debug('>>> Entering constructor()');
        isHost = util.settings.isHost;
        isAdmin = isHost || util.settings.isAdmin;
        canViewAdminLogs = isAdmin || util.settings.permissions.ADMIN_LOGS_VIEW;
        canViewLogSettings = isHost || util.settings.permissions.LOG_SETTINGS_VIEW;
		logger.debug(`constructor(): canViewAdminLogs → ${canViewAdminLogs}`);
		logger.debug('<<< Exiting constructor()');
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    navigateMap(page, index, event) {
		logger.debug(`>>> Entering navigateMap(page=${page},index=${index},event=${event})`);
        event.preventDefault();
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page, index));
		logger.debug(`<<< Exiting navigateMap(page=${page},index=${index},event=${event})`);
    }
    render() {
		logger.debug('>>> Entering render()');
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

		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex
    };
}

export default connect(mapStateToProps)(App);