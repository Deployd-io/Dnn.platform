import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridSystem, Label, GridCell } from "@dnnsoftware/dnn-react-common";
import InfoBlock from "../../common/InfoBlock";
import BackupGrid from "./BackupGrid";
import FilesGrid from "./FilesGrid";
import Localization from "../../../localization";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import DatabaseTabActions from "../../../actions/databaseTab";
import utils from "../../../utils";

import "../tabs.less";
const logger = console;

const defaultPlaceHolder = "...";

class Database extends Component {
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.props.onRetrieveDatabaseServerInfo();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    UNSAFE_componentWillReceiveProps(newProps) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.notifyError(newProps.errorMessage);
			logger.warn(`<<< Exiting UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <GridCell>
            <GridCell className="dnn-servers-info-panel">
                <GridSystem>
                    <GridCell>
                        <InfoBlock label={Localization.get("DbInfo_ProductVersion")}
                            tooltip={Localization.get("DbInfo_ProductVersion.Help")}
                            text={props.databaseInfo.productVersion || defaultPlaceHolder} />

                        <InfoBlock label={Localization.get("DbInfo_ServicePack")}
                            tooltip={Localization.get("DbInfo_ServicePack.Help")}
                            text={props.databaseInfo.servicePack || defaultPlaceHolder} />

                        <InfoBlock label={Localization.get("DbInfo_ProductEdition")}
                            tooltip={Localization.get("DbInfo_ProductEdition.Help")}
                            text={props.databaseInfo.productEdition || defaultPlaceHolder} />
                    </GridCell>
                    <GridCell>
                        <InfoBlock label={Localization.get("DbInfo_SoftwarePlatform")}
                            tooltip={Localization.get("DbInfo_SoftwarePlatform.Help")}
                            text={props.databaseInfo.softwarePlatform || defaultPlaceHolder} />
                    </GridCell>
                </GridSystem>
            </GridCell>
            <GridCell className="dnn-servers-grid-panel">
                <Label className="header-title" label={Localization.get("plBackups")} />
                <BackupGrid backups={props.databaseInfo.backups} />
            </GridCell>
            <GridCell className="dnn-servers-grid-panel" style={{ paddingBottom: 55 }}>
                <Label className="header-title" label={Localization.get("plFiles")} />
                <FilesGrid files={props.databaseInfo.files} />
            </GridCell>
        </GridCell>;
    }
}

Database.propTypes = {
    databaseInfo: PropTypes.object.isRequired,
    errorMessage: PropTypes.string,
    onRetrieveDatabaseServerInfo: PropTypes.func.isRequired
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        databaseInfo: state.databaseTab.databaseServerInfo,
        errorMessage: state.databaseTab.errorMessage
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return {
        ...bindActionCreators({
            onRetrieveDatabaseServerInfo: DatabaseTabActions.loadDatabaseServerInfo
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(Database);