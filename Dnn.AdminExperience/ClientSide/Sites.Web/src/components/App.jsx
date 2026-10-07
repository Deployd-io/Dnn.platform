import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import PortalList from "./PortalList";
import {
    CommonVisiblePanelActions,
    CommonExportPortalActions
} from "dnn-sites-common-actions";
import { PersonaBarPage } from "@dnnsoftware/dnn-react-common";
import { ExportPortal } from "dnn-sites-common-components";
import CreatePortal from "./CreatePortal";
const logger = console;

class App extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            editMode: false,
            portalBeingExported: {}
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    onAddNewSite(event) {
		logger.error(`>>> Entering onAddNewSite(event=${event})`, { "event.action": "onAddNewSite", "logrite.event": event });
        event.preventDefault();
        this.navigateMap(1);
		logger.warn(`<<< Exiting onAddNewSite(event=${event})`, { "event.action": "onAddNewSite", "logrite.event": event });
    }

    onEditSite() {}

    onExportPortal(portalBeingExported) {
		logger.error(`>>> Entering onExportPortal(portalBeingExported=${portalBeingExported})`, { "event.action": "onExportPortal", "logrite.portalBeingExported": portalBeingExported });
        const { props } = this;
        props.dispatch(
            CommonExportPortalActions.setPortalBeingExported(
                portalBeingExported,
                this.navigateMap.bind(this, 2)
            )
        );
		logger.warn(`<<< Exiting onExportPortal(portalBeingExported=${portalBeingExported})`, { "event.action": "onExportPortal", "logrite.portalBeingExported": portalBeingExported });
    }

    navigateMap(page) {
		logger.error(`>>> Entering navigateMap(page=${page})`, { "event.action": "navigateMap", "logrite.page": page });
        const { props } = this;
        props.dispatch(CommonVisiblePanelActions.selectPanel(page));
		logger.warn(`<<< Exiting navigateMap(page=${page})`, { "event.action": "navigateMap", "logrite.page": page });
    }

    cancelExport(event) {
		logger.error(`>>> Entering cancelExport(event=${event})`, { "event.action": "cancelExport", "logrite.event": event });
        if (event !== undefined) event.preventDefault();
        this.setState({
            portalBeingExported: {}
        });
        this.navigateMap(0);
		logger.warn(`<<< Exiting cancelExport(event=${event})`, { "event.action": "cancelExport", "logrite.event": event });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="sites-Root">
                <PersonaBarPage
                    isOpen={props.selectedPage === 0 || props.selectedPage === 2}
                >
                    <PortalList
                        onAddNewSite={this.onAddNewSite.bind(this)}
                        onExportPortal={this.onExportPortal.bind(this)}
                    />
                </PersonaBarPage>
                {props.selectedPage === 1 && (
                    <PersonaBarPage isOpen={props.selectedPage === 1}>
                        <CreatePortal onCancel={this.navigateMap.bind(this, 0)} />
                    </PersonaBarPage>
                )}
                {props.selectedPage === 2 && (
                    <PersonaBarPage isOpen={props.selectedPage === 2}>
                        <ExportPortal onCancel={this.cancelExport.bind(this)} />
                    </PersonaBarPage>
                )}
            </div>
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
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex,
        portalBeingExported: state.exportPortal.portalBeingExported
    };
}

export default connect(mapStateToProps)(App);
