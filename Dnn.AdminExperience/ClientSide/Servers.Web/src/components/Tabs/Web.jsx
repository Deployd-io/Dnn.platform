import React, {Component } from "react";
import PropTypes from "prop-types";
import { GridSystem } from "@dnnsoftware/dnn-react-common";
import InfoBlock from "../common/InfoBlock";
import Localization from "../../localization";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import WebTabActions from "../../actions/webTab";
import utils from "../../utils";

import "./tabs.less";
const logger = console;

const defaultPlaceHolder = "...";

class Web extends Component {
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.props.onRetrieveWebServerInfo();
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
        return <div className="dnn-servers-info-panel">
            <GridSystem>
                <div>
                    <InfoBlock label={Localization.get("ServerInfo_OSVersion")} 
                        tooltip={Localization.get("ServerInfo_OSVersion.Help")} 
                        text={props.webServerInfo.osVersion || defaultPlaceHolder} />   

                    <InfoBlock label={Localization.get("ServerInfo_IISVersion")} 
                        tooltip={Localization.get("ServerInfo_IISVersion.Help")} 
                        text={props.webServerInfo.iisVersion || defaultPlaceHolder} /> 

                    <InfoBlock label={Localization.get("ServerInfo_Framework")}
                        tooltip={Localization.get("ServerInfo_Framework.Help")}
                        text={props.webServerInfo.framework || defaultPlaceHolder} />

                    <InfoBlock label={Localization.get("ServerInfo_Framework_Product")}
                        tooltip={Localization.get("ServerInfo_Framework_Product.Help")}
                        text={props.webServerInfo.netFrameworkVersion || defaultPlaceHolder} />

                    <div className="tooltipAdjustment">
                        <InfoBlock label={Localization.get("ServerInfo_Identity")} 
                            tooltip={Localization.get("ServerInfo_Identity.Help")} 
                            text={props.webServerInfo.identity || defaultPlaceHolder} />
                    </div> 
                </div>
                <div>
                    <InfoBlock label={Localization.get("ServerInfo_HostName")} 
                        tooltip={Localization.get("ServerInfo_HostName.Help")} 
                        text={props.webServerInfo.hostName || defaultPlaceHolder} /> 
                        
                    <InfoBlock label={Localization.get("ServerInfo_PhysicalPath")} 
                        tooltip={Localization.get("ServerInfo_PhysicalPath.Help")} 
                        text={props.webServerInfo.physicalPath || defaultPlaceHolder} /> 

                    <InfoBlock label={Localization.get("ServerInfo_Url")} 
                        tooltip={Localization.get("ServerInfo_Url.Help")} 
                        text={props.webServerInfo.url || defaultPlaceHolder} /> 

                    <InfoBlock label={Localization.get("ServerInfo_RelativePath")} 
                        tooltip={Localization.get("ServerInfo_RelativePath.Help")} 
                        text={props.webServerInfo.relativePath || defaultPlaceHolder} /> 

                    <InfoBlock label={Localization.get("ServerInfo_ServerTime")} 
                        tooltip={Localization.get("ServerInfo_ServerTime.Help")} 
                        text={props.webServerInfo.serverTime || defaultPlaceHolder} />
                </div>
            </GridSystem>
        </div>;
    }
}

Web.propTypes = {   
    webServerInfo: PropTypes.object.isRequired,
    errorMessage: PropTypes.string,
    onRetrieveWebServerInfo: PropTypes.func.isRequired
};

function mapStateToProps(state) {    
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        webServerInfo: state.webTab.webServerInfo,
        errorMessage: state.webTab.errorMessage
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return {
        ...bindActionCreators ({
            onRetrieveWebServerInfo: WebTabActions.loadWebServerInfo     
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(Web);
