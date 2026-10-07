import React, {Component } from "react";
import PropTypes from "prop-types";
import { GridSystem } from "@dnnsoftware/dnn-react-common";
import InfoBlock from "../common/InfoBlock";
import Localization from "../../localization";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import ApplicationTabActions from "../../actions/applicationTab";
import utils from "../../utils";

import "./tabs.less";
const logger = console;

const defaultPlaceHolder = "...";

class Application extends Component {

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        if (!this.props.isApplicationInfoLoaded) {
            this.props.onRetrieveApplicationInfo();
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }        
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
                    <InfoBlock label={Localization.get("plProduct")} 
                        tooltip={Localization.get("plProduct.Help")} 
                        text={props.applicationInfo.product || defaultPlaceHolder} />   

                    <InfoBlock label={Localization.get("plVersion")} 
                        tooltip={Localization.get("plVersion.Help")} 
                        text={props.applicationInfo.version || defaultPlaceHolder} /> 

                    {utils.isHostUser() && 
                        <div className="tooltipAdjustment">
                            <InfoBlock label={Localization.get("plGUID")} 
                                tooltip={Localization.get("plGUID.Help")} 
                                text={props.applicationInfo.guid || defaultPlaceHolder} /> 
                        </div>
                    }
                    
                    <InfoBlock label={Localization.get("HostInfo_HtmlEditorProvider")} 
                        tooltip={Localization.get("HostInfo_HtmlEditorProvider.Help")} 
                        text={props.applicationInfo.htmlEditorProvider || defaultPlaceHolder} />

                    <InfoBlock label={Localization.get("plDataProvider")} 
                        tooltip={Localization.get("plDataProvider.Help")} 
                        text={props.applicationInfo.dataProvider || defaultPlaceHolder} /> 

                    <InfoBlock label={Localization.get("HostInfo_CachingProvider")} 
                        tooltip={Localization.get("HostInfo_CachingProvider.Help")} 
                        text={props.applicationInfo.cachingProvider || defaultPlaceHolder} /> 

                    <InfoBlock label={Localization.get("HostInfo_LoggingProvider")} 
                        tooltip={Localization.get("HostInfo_LoggingProvider.Help")} 
                        text={props.applicationInfo.loggingProvider || defaultPlaceHolder} /> 
                </div>
                <div>
                    <InfoBlock label={Localization.get("HostInfo_FriendlyUrlProvider")} 
                        tooltip={Localization.get("HostInfo_FriendlyUrlProvider.Help")} 
                        text={props.applicationInfo.friendlyUrlProvider || defaultPlaceHolder} /> 

                    <InfoBlock label={Localization.get("HostInfo_FriendlyUrlEnabled")} 
                        tooltip={Localization.get("HostInfo_FriendlyUrlEnabled.Help")} 
                        text={props.applicationInfo.friendlyUrlsEnabled || defaultPlaceHolder} /> 

                    <InfoBlock label={Localization.get("HostInfo_FriendlyUrlType")} 
                        tooltip={Localization.get("HostInfo_FriendlyUrlType.Help")} 
                        text={props.applicationInfo.friendlyUrlType || defaultPlaceHolder} /> 

                    <div className="tooltipAdjustment">
                        <InfoBlock label={Localization.get("HostInfo_SchedulerMode")} 
                            tooltip={Localization.get("HostInfo_SchedulerMode.Help")} 
                            text={props.applicationInfo.schedulerMode || defaultPlaceHolder} />
                    </div>

                    <InfoBlock label={Localization.get("HostInfo_WebFarmEnabled")} 
                        tooltip={Localization.get("HostInfo_WebFarmEnabled.Help")} 
                        text={props.applicationInfo.webFarmEnabled || defaultPlaceHolder} />

                    <InfoBlock label={Localization.get("HostInfo_Permissions")} 
                        tooltip={Localization.get("HostInfo_Permissions.Help")} 
                        text={props.applicationInfo.casPermissions || defaultPlaceHolder} />
                </div>
            </GridSystem>
        </div>;
    }
}

Application.propTypes = {   
    applicationInfo: PropTypes.object.isRequired,
    isApplicationInfoLoaded: PropTypes.bool.isRequired,
    errorMessage: PropTypes.string,
    onRetrieveApplicationInfo: PropTypes.func.isRequired
};

function mapStateToProps(state) {    
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        applicationInfo: state.applicationTab.applicationInfo,
        isApplicationInfoLoaded: state.applicationTab.isApplicationInfoLoaded,
        errorMessage: state.applicationTab.errorMessage
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return {
        ...bindActionCreators ({
            onRetrieveApplicationInfo: ApplicationTabActions.loadApplicationInfo     
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(Application);