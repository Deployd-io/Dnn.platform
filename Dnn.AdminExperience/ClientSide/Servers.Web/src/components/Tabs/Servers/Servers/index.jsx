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
		logger.debug('>>> Entering componentDidMount()');
        this.props.onRetrieveServers();
		logger.debug('<<< Exiting componentDidMount()');
    }

    UNSAFE_componentWillReceiveProps(newProps) {
		logger.debug(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`);
        if (
            this.props.errorMessage !== newProps.errorMessage &&
      newProps.errorMessage
        ) {
            utils.notifyError(newProps.errorMessage);
        }
    }

    deleteServer(serverId) {
		logger.debug(`>>> Entering deleteServer(serverId=${serverId})`);
        this.props.deleteServer(serverId);
		logger.debug(`<<< Exiting deleteServer(serverId=${serverId})`);
    }

    deleteNonActiveServers() {
		logger.debug('>>> Entering deleteNonActiveServers()');
        utils.confirm(
            Localization.get("DeleteNonActiveServers.Confirm"),
            Localization.get("Confirm"),
            Localization.get("Cancel"),
            function () {
				logger.debug('>>> Entering function()');
				logger.debug('<<< Exiting function()');
                this.props.deleteNonActiveServers();
            }.bind(this)
        );
    }

    hasInactiveServers() {
		logger.debug('>>> Entering hasInactiveServers()');
        let res = false;
        this.props.servers.forEach((s) => {
            res = res || !s.isActive;
        });
		logger.debug('<<< Exiting hasInactiveServers()');
        return res;
    }

    render() {
		logger.debug('>>> Entering render()');
        const { props } = this;
        let serverName = "";
        props.servers.forEach((element) => {
            if (element.isCurrent) {
                serverName = element.serverName;
            }
        });

		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        servers: state.serversTab.servers,
        errorMessage: state.serversTab.errorMessage,
    };
}

function mapDispatchToProps(dispatch) {
	logger.debug(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`);
	logger.debug(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`);
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
