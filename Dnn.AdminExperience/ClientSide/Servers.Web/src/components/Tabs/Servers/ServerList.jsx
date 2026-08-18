import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import ServerRow from "./ServerRow";
const logger = console;

export default class ServerList extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            openId: -1,
        };
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    uncollapse(id) {
		logger.debug(`>>> Entering uncollapse(id=${id})`);
        setTimeout(() => {
            this.setState({
                openId: id,
            });
        }, this.timeout);
		logger.debug(`<<< Exiting uncollapse(id=${id})`);
    }
    collapse() {
		logger.debug('>>> Entering collapse()');
        if (this.state.openId !== -1) {
            this.setState({
                openId: -1,
            });
			logger.debug('<<< Exiting collapse()');
        }
    }
    toggle(openId) {
		logger.debug(`>>> Entering toggle(openId=${openId})`);
        if (openId !== -1) {
            this.uncollapse(openId);
        } else {
            this.collapse();
			logger.debug(`<<< Exiting toggle(openId=${openId})`);
        }
    }

    getServerGridRows() {
		logger.debug('>>> Entering getServerGridRows()');
        if (this.props.servers && this.props.servers.length > 0) {
            const rows = this.props.servers.map((field) => {
				logger.debug(`getServerGridRows(): rows → ${rows}`);
                return (
                    <ServerRow
                        server={field}
                        key={field.serverId}
                        inEdit={this.state.openId === field.serverId}
                        openCollapse={this.toggle.bind(this, field.serverId)}
                        collapse={this.collapse.bind(this, field.serverId)}
                        deleteServer={this.props.deleteServer.bind(this)}
                    />
                );
            });
		logger.debug('<<< Exiting getServerGridRows()');
            return rows;
        }
        return false;
    }

    render() {
		logger.debug('>>> Entering render()');
        const rows = this.getServerGridRows();

		logger.debug(`render(): rows → ${rows}`);
		logger.debug('<<< Exiting render()');
        return (
            <div className="grid">
                <div className="row header">
                    <GridCell columnSize={30}>{Localization.get("Name")}</GridCell>
                    <GridCell columnSize={40}>{Localization.get("Url")}</GridCell>
                    <GridCell columnSize={20}>
                        {Localization.get("LastActivityDate")}
                    </GridCell>
                    <GridCell columnSize={10} className="right">
                        {Localization.get("Actions")}
                    </GridCell>
                </div>
                {rows}
            </div>
        );
    }
}

ServerList.propTypes = {
    servers: PropTypes.array,
    deleteServer: PropTypes.func,
};
