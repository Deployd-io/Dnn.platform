import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "../../localization";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import LogFileRow from "./LogFileRow";
import LogsTabActions from "../../actions/logsTab";
import utils from "../../utils";

import "./tabs.less";
const logger = console;

class Logs extends Component {
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        if (this.props.logs.length > 0) {
            return;
        }
        this.props.onRetrieveLogsServerInfo();
		logger.debug('<<< Exiting componentDidMount()');
    }

    UNSAFE_componentWillReceiveProps(newProps) {
		logger.debug(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`);
        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.notifyError(newProps.errorMessage);
			logger.debug(`<<< Exiting UNSAFE_componentWillReceiveProps(newProps=${newProps})`);
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const { props } = this;

		logger.debug('<<< Exiting render()');
        return (
            <div className="dnn-servers-info-panel-big logsTab">
                <div className="logContainer">
                    <div className="logContainerBox">
                        <div className="logHeader-wrapper">
                            <div className="logHeader logHeader-type">
                                <span>{Localization.get("Logs_Type.Header")}</span>
                            </div>
                            <div className="logHeader logHeader-filename">
                                <span>{Localization.get("Logs_Name.Header")}</span>
                            </div>
                            <div className="logHeader logHeader-date">
                                <span>{Localization.get("Logs_Date.Header")}</span>
                            </div>
                            <div className="logHeader logHeader-size">
                                <span>{Localization.get("Logs_Size.Header")}</span>
                            </div>
                        </div>
                        {props.logs.map &&
              props.logs.map(l => (
                  <LogFileRow
                      key={l.name}
                      fileName={l.name}
                      lastWriteTimeUtc={l.lastWriteTimeUtc}
                      size={l.size}
                      typeName={
                          l.upgradeLog
                              ? Localization.get("Logs_UpgradeLog")
                              : Localization.get("Logs_ServerLog")
                      }
                      onOpen={() => {
                          if (this.props.selectedLog !== l.name) {
                              this.props.onSelectedLog(l);
                          }
                      }}
                  >
                      <div className="log-file-display">{props.logData}</div>
                  </LogFileRow>
              ))}
                    </div>
                </div>
            </div>
        );
    }
}

Logs.propTypes = {
    logs: PropTypes.arrayOf(PropTypes.object),
    errorMessage: PropTypes.string,
    selectedLog: PropTypes.string,
    onRetrieveLogsServerInfo: PropTypes.func.isRequired,
    onSelectedLog: PropTypes.func,
    logData: PropTypes.func
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        logs: state.logsTab.logs,
        errorMessage: state.logsTab.errorMessage,
        selectedLog: state.logsTab.selectedLog,
        logData: state.logsTab.logData
    };
}

function mapDispatchToProps(dispatch) {
	logger.debug(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`);
	logger.debug(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`);
    return {
        ...bindActionCreators({
            onRetrieveLogsServerInfo: LogsTabActions.loadLogsServerInfo,
            onSelectedLog: LogsTabActions.loadSelectedLog
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(Logs);
