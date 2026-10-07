import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Button, PersonaBarPageHeader, PersonaBarPage } from "@dnnsoftware/dnn-react-common";
import Body from "./Body/Body";
import localization from "../localization";
import { bindActionCreators } from "redux";
import ServerActions from "../actions/server";
import utils from "../utils";
const logger = console;

const restartAppButtonStyle = {
    "marginRight": "10px"
};

class App extends Component { 

    UNSAFE_componentWillReceiveProps(newProps) {       
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
        if (this.props.infoMessage !== newProps.infoMessage && newProps.infoMessage) {
            utils.notify(newProps.infoMessage);
        }

        if (newProps.reloadPage) {
            location.reload();
            return;
        }
        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.notifyError(newProps.errorMessage);
        }
		logger.warn(`<<< Exiting UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
        const buttonVisible = utils.isHostUser();
        if (this.props.reloadPage) {
            window.top.location.reload();
            return null;
        }
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="servers-app personaBar-mainContainer">
                <PersonaBarPage isOpen={true}>
                    <PersonaBarPageHeader title="Servers">
                        {buttonVisible && 
                            <Button type="danger" size="large" 
                                onClick={props.onClearCacheClicked}>{localization.get("clearCacheButtonLabel")}</Button>
                        }
                        {buttonVisible && 
                            <Button type="secondary" size="large" 
                                onClick={props.onRestartApplicationClicked} 
                                style={restartAppButtonStyle}>{localization.get("restartApplicationButtonLabel")}</Button>
                        }                        
                    </PersonaBarPageHeader>
                    <Body />
                </PersonaBarPage>                
            </div>
        );
    }
}

App.propTypes = {
    dispatch: PropTypes.func,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number,
    onRestartApplicationClicked: PropTypes.func.isRequired,
    reloadPage: PropTypes.bool.isRequired,
    errorMessage: PropTypes.string,
    infoMessage: PropTypes.string,
    onClearCacheClicked: PropTypes.func
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        selectedPage: state.visiblePanel.selectedPage,
        selectedPageVisibleIndex: state.visiblePanel.selectedPageVisibleIndex,
        reloadPage: state.server.reloadPage,
        errorMessage: state.server.errorMessage,
        infoMessage: state.server.infoMessage
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return {
        ...bindActionCreators ({
            onRestartApplicationClicked: ServerActions.restartApplication,
            onClearCacheClicked: ServerActions.clearCache
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(App);