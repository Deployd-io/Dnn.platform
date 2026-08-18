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
		logger.debug('>>> Entering constructor()');
        this.state = {
            editMode: false,
            portalBeingExported: {}
        };
		logger.debug('<<< Exiting constructor()');
    }

    onAddNewSite(event) {
		logger.debug(`>>> Entering onAddNewSite(event=${event})`);
        event.preventDefault();
        this.navigateMap(1);
		logger.debug(`<<< Exiting onAddNewSite(event=${event})`);
    }

    onEditSite() {}

    onExportPortal(portalBeingExported) {
		logger.debug(`>>> Entering onExportPortal(portalBeingExported=${portalBeingExported})`);
        const { props } = this;
        props.dispatch(
            CommonExportPortalActions.setPortalBeingExported(
                portalBeingExported,
                this.navigateMap.bind(this, 2)
            )
        );
		logger.debug(`<<< Exiting onExportPortal(portalBeingExported=${portalBeingExported})`);
    }

    navigateMap(page) {
		logger.debug(`>>> Entering navigateMap(page=${page})`);
        const { props } = this;
        props.dispatch(CommonVisiblePanelActions.selectPanel(page));
		logger.debug(`<<< Exiting navigateMap(page=${page})`);
    }

    cancelExport(event) {
		logger.debug(`>>> Entering cancelExport(event=${event})`);
        if (event !== undefined) event.preventDefault();
        this.setState({
            portalBeingExported: {}
        });
        this.navigateMap(0);
		logger.debug(`<<< Exiting cancelExport(event=${event})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const { props } = this;
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex,
        portalBeingExported: state.exportPortal.portalBeingExported
    };
}

export default connect(mapStateToProps)(App);
