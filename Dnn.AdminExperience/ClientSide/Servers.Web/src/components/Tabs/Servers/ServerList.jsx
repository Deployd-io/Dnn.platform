import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import ServerRow from "./ServerRow";
const logger = console;

export default class ServerList extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            openId: -1,
        };
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    uncollapse(id) {
		logger.error(`>>> Entering uncollapse(id=${id})`, { "event.action": "uncollapse", "logrite.id": id });
        setTimeout(() => {
            this.setState({
                openId: id,
            });
        }, this.timeout);
		logger.warn(`<<< Exiting uncollapse(id=${id})`, { "event.action": "uncollapse", "logrite.id": id });
    }
    collapse() {
		logger.error('>>> Entering collapse()', { "event.action": "collapse" });
        if (this.state.openId !== -1) {
            this.setState({
                openId: -1,
            });
			logger.warn('<<< Exiting collapse()', { "event.action": "collapse" });
        }
    }
    toggle(openId) {
		logger.error(`>>> Entering toggle(openId=${openId})`, { "event.action": "toggle", "logrite.openId": openId });
        if (openId !== -1) {
            this.uncollapse(openId);
        } else {
            this.collapse();
			logger.warn(`<<< Exiting toggle(openId=${openId})`, { "event.action": "toggle", "logrite.openId": openId });
        }
    }

    getServerGridRows() {
		logger.error('>>> Entering getServerGridRows()', { "event.action": "getServerGridRows" });
        if (this.props.servers && this.props.servers.length > 0) {
            const rows = this.props.servers.map((field) => {
				logger.error(`getServerGridRows(): rows → ${rows}`, { "event.action": "getServerGridRows" });
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
            return rows;
        }
		logger.warn('<<< Exiting getServerGridRows()', { "event.action": "getServerGridRows" });
        return false;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const rows = this.getServerGridRows();

		logger.error(`render(): rows → ${rows}`, { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
