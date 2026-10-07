import React, {Component} from "react";
import PropTypes from "prop-types";
import {pageActions as PageActions} from "../../../actions";
import utils from "../../../utils";
const logger = console;

const queryString = {"HideCancel": "true", "HideDelete": "true", "NoRedirectOnUpdate": "true"};

const iFrameStyle = { 
    width: "100%", 
    height: "450px", 
    marginTop: "20px" 
};

class ModuleEdit extends Component {
    
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.onIFrameLoad = this.onIFrameLoad.bind(this);
        this.closeOnEndRequest = false;
		logger.debug(`constructor(): this.closeOnEndRequest → ${this.closeOnEndRequest}`, { "event.action": "constructor" });
        this.state = {
            userMode: utils.getUserMode().toLowerCase(),
            editUrl: ""
        };
    }

    onIFrameLoad() {
		logger.error('>>> Entering onIFrameLoad()', { "event.action": "onIFrameLoad" });
        const iframe = this.iframeRef;
        const location = iframe.contentWindow.location.href;
        if (location.toLowerCase().indexOf("popup") === -1) {
            if (this.closeOnEndRequest) {
                this.props.onUpdatedModuleSettings();
            } else {
                this.redirectUrl(location);
            }
        } else {
            const pageRequestManager = iframe.contentWindow.Sys.WebForms.PageRequestManager.getInstance();
			logger.error(`onIFrameLoad(): pageRequestManager → ${pageRequestManager}`, { "event.action": "onIFrameLoad" });
            pageRequestManager.add_beginRequest(this.beginRequestHandler.bind(this));
            pageRequestManager.add_endRequest(this.endRequestHandler.bind(this));
			logger.warn('<<< Exiting onIFrameLoad()', { "event.action": "onIFrameLoad" });
        }
    }

    endRequestHandler() {
		logger.error('>>> Entering endRequestHandler()', { "event.action": "endRequestHandler" });
        if (this.closeOnEndRequest) {
            this.props.onUpdatedModuleSettings();
			logger.warn('<<< Exiting endRequestHandler()', { "event.action": "endRequestHandler" });
        }
    }

    beginRequestHandler(pageRequestManager, beginRequestEventArgs) {
		logger.error(`>>> Entering beginRequestHandler(pageRequestManager=${pageRequestManager},beginRequestEventArgs=${beginRequestEventArgs})`, { "event.action": "beginRequestHandler", "logrite.pageRequestManager": pageRequestManager, "logrite.beginRequestEventArgs": beginRequestEventArgs });
        const postBackElement = beginRequestEventArgs._postBackElement;
        this.closeOnEndRequest = postBackElement.id === ("dnn_ctr" + this.props.module.id + "_ModuleSettings_cmdUpdate");
        if (!this.closeOnEndRequest) {
			logger.warn(`<<< Exiting beginRequestHandler(pageRequestManager=${pageRequestManager},beginRequestEventArgs=${beginRequestEventArgs})`, { "event.action": "beginRequestHandler", "logrite.pageRequestManager": pageRequestManager, "logrite.beginRequestEventArgs": beginRequestEventArgs });
            this.closeOnEndRequest = postBackElement.id.indexOf("dnn_ctr" + this.props.module.id) > -1
                && postBackElement.id.match(/Close$|Cancel$|Save$/) !== null;
        }
    }

    checkUrlType() {
		logger.error('>>> Entering checkUrlType()', { "event.action": "checkUrlType" });
        const {props} = this;
        let editUrl = "";
        switch (props.editType) {
            case "content":
                editUrl = props.module.editContentUrl;
				logger.error(`checkUrlType(): editUrl → ${editUrl}`, { "event.action": "checkUrlType" });
                break;
            case "settings":
                editUrl = props.module.editSettingUrl;
                break;
        }

        if (editUrl !== "") {
            if (editUrl.toLowerCase().indexOf("popup") === -1) {
                this.redirectUrl(editUrl);
            } else {
                editUrl = utils.url.appendQueryString(editUrl, queryString);
                this.setState({editUrl: editUrl});
            }
            
			logger.warn('<<< Exiting checkUrlType()', { "event.action": "checkUrlType" });
        }
    }

    redirectUrl(url) {
		logger.error(`>>> Entering redirectUrl(url=${url})`, { "event.action": "redirectUrl", "logrite.url": url });
        utils.getUtilities().closePersonaBar(function () {
            window.parent.location = url;
        });
		logger.warn(`<<< Exiting redirectUrl(url=${url})`, { "event.action": "redirectUrl", "logrite.url": url });
    }

    addEventListener() {
		logger.error('>>> Entering addEventListener()', { "event.action": "addEventListener" });
        const iframe = this.iframeRef;
        if (iframe) {
            iframe.addEventListener("load", this.onIFrameLoad);
			logger.warn('<<< Exiting addEventListener()', { "event.action": "addEventListener" });
        }
    }

    removeEventListener() {
		logger.error('>>> Entering removeEventListener()', { "event.action": "removeEventListener" });
        const iframe = this.iframeRef;
        if (iframe) {
            iframe.removeEventListener("load", this.onIFrameLoad);
			logger.warn('<<< Exiting removeEventListener()', { "event.action": "removeEventListener" });
        }
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;

        PageActions.viewPage(props.selectedPage.tabId, null, () => {
            this.setState({
                userMode: "edit"
            }, () => {
                this.checkUrlType();
                this.addEventListener();
            });
        });
        this.addEventListener();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        this.removeEventListener();
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {state} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (state.userMode === "edit" && 
            <iframe ref={node => this.iframeRef = node} src={state.editUrl} style={iFrameStyle} frameBorder={0}></iframe>
        );
    }
}

ModuleEdit.propTypes = {
    module: PropTypes.object.isRequired,
    editType: PropTypes.string.isRequired,
    onUpdatedModuleSettings: PropTypes.func.isRequired,
    selectedPage: PropTypes.object,
};

export default ModuleEdit;