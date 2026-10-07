import React, { Component } from "react";
import PropTypes from "prop-types";
import {
    Button,
    GridSystem,
    Label,
    GridCell,
} from "@dnnsoftware/dnn-react-common";
import InfoBlock from "../../../common/InfoBlock";
import Localization from "../../../../localization";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import ServersTabActions from "../../../../actions/serversTab";
import utils from "../../../../utils";

import "../../tabs.less";
import "./style.less";
import ServerList from "../ServerList";
const logger = console;

class Servers extends Component {
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.props.onRetrieveServers();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    UNSAFE_componentWillReceiveProps(newProps) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
        if (
            this.props.errorMessage !== newProps.errorMessage &&
      newProps.errorMessage
        ) {
            utils.notifyError(newProps.errorMessage);
        }
    }

    deleteServer(serverId) {
		logger.error(`>>> Entering deleteServer(serverId=${serverId})`, { "event.action": "deleteServer", "logrite.serverId": serverId });
        this.props.deleteServer(serverId);
		logger.warn(`<<< Exiting deleteServer(serverId=${serverId})`, { "event.action": "deleteServer", "logrite.serverId": serverId });
    }

    deleteNonActiveServers() {
		logger.error('>>> Entering deleteNonActiveServers()', { "event.action": "deleteNonActiveServers" });
        utils.confirm(
            Localization.get("DeleteNonActiveServers.Confirm"),
            Localization.get("Confirm"),
            Localization.get("Cancel"),
            function () {
                this.props.deleteNonActiveServers();
            }.bind(this)
        );
    }

    hasInactiveServers() {
		logger.error('>>> Entering hasInactiveServers()', { "event.action": "hasInactiveServers" });
        let res = false;
        this.props.servers.forEach((s) => {
            res = res || !s.isActive;
        });
		logger.warn('<<< Exiting hasInactiveServers()', { "event.action": "hasInactiveServers" });
        return res;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;
        let serverName = "";
        props.servers.forEach((element) => {
            if (element.isCurrent) {
                serverName = element.serverName;
            }
        });

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell>
                <GridCell className="dnn-servers-info-panel">
                    <GridSystem>
                        <GridCell>
                            <InfoBlock
                                label={Localization.get("Servers_CurrentServerName")}
                                tooltip={Localization.get("Servers_CurrentServerName.Help")}
                                text={serverName}
                            />
                        </GridCell>
                        <GridCell>
                            {this.hasInactiveServers() ? (
                                <div className="warningBox">
                                    <div className="warningText">
                                        {Localization.get("DeleteNonActiveServers.Warning")}
                                    </div>
                                    <div className="warningButton">
                                        <Button
                                            type="danger"
                                            onClick={this.deleteNonActiveServers.bind(this)}
                                        >
                                            {Localization.get("DeleteNonActiveServers")}
                                        </Button>
                                    </div>
                                </div>
                            ) : null}
                        </GridCell>
                    </GridSystem>
                </GridCell>
                <GridCell className="dnn-servers-grid-panel">
                    <Label
                        className="header-title"
                        label={Localization.get("plWebServers")}
                    />
                    <ServerList
                        servers={props.servers}
                        deleteServer={this.deleteServer.bind(this)}
                    />
                </GridCell>
            </GridCell>
        );
    }
}

Servers.propTypes = {
    errorMessage: PropTypes.string,
    onRetrieveServers: PropTypes.func.isRequired,
    deleteServer: PropTypes.func.isRequired,
    deleteNonActiveServers: PropTypes.func.isRequired,
    servers: PropTypes.array,
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        servers: state.serversTab.servers,
        errorMessage: state.serversTab.errorMessage,
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return {
        ...bindActionCreators(
            {
                onRetrieveServers: ServersTabActions.loadServers,
                deleteServer: ServersTabActions.deleteServer,
                deleteNonActiveServers: ServersTabActions.deleteNonActiveServers,
            },
            dispatch
        ),
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(Servers);
