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
		logger.debug('>>> Entering constructor()');
        this.onIFrameLoad = this.onIFrameLoad.bind(this);
        this.closeOnEndRequest = false;
		logger.debug('<<< Exiting constructor()');
        this.state = {
            userMode: utils.getUserMode().toLowerCase(),
            editUrl: ""
        };
    }

    onIFrameLoad() {
		logger.debug('>>> Entering onIFrameLoad()');
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
			logger.debug(`onIFrameLoad(): pageRequestManager → ${pageRequestManager}`);
            pageRequestManager.add_beginRequest(this.beginRequestHandler.bind(this));
            pageRequestManager.add_endRequest(this.endRequestHandler.bind(this));
			logger.debug('<<< Exiting onIFrameLoad()');
        }
    }

    endRequestHandler() {
		logger.debug('>>> Entering endRequestHandler()');
        if (this.closeOnEndRequest) {
            this.props.onUpdatedModuleSettings();
			logger.debug('<<< Exiting endRequestHandler()');
        }
    }

    beginRequestHandler(pageRequestManager, beginRequestEventArgs) {
        const postBackElement = beginRequestEventArgs._postBackElement;
        this.closeOnEndRequest = postBackElement.id === ("dnn_ctr" + this.props.module.id + "_ModuleSettings_cmdUpdate");
        if (!this.closeOnEndRequest) {
            this.closeOnEndRequest = postBackElement.id.indexOf("dnn_ctr" + this.props.module.id) > -1
                && postBackElement.id.match(/Close$|Cancel$|Save$/) !== null;
        }
		logger.debug(`<<< Exiting beginRequestHandler(pageRequestManager=${pageRequestManager},beginRequestEventArgs=${beginRequestEventArgs})`);
    }

    checkUrlType() {
		logger.debug('>>> Entering checkUrlType()');
        const {props} = this;
        let editUrl = "";
        switch (props.editType) {
            case "content":
                editUrl = props.module.editContentUrl;
				logger.debug(`checkUrlType(): editUrl → ${editUrl}`);
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
            
			logger.debug('<<< Exiting checkUrlType()');
        }
    }

    redirectUrl(url) {
		logger.debug(`>>> Entering redirectUrl(url=${url})`);
        utils.getUtilities().closePersonaBar(function () {
			logger.debug('>>> Entering closePersonaBar()');
			logger.debug('<<< Exiting closePersonaBar()');
            window.parent.location = url;
        });
		logger.debug(`<<< Exiting redirectUrl(url=${url})`);
    }

    addEventListener() {
		logger.debug('>>> Entering addEventListener()');
        const iframe = this.iframeRef;
        if (iframe) {
            iframe.addEventListener("load", this.onIFrameLoad);
        }
		logger.debug('<<< Exiting addEventListener()');
    }

    removeEventListener() {
		logger.debug('>>> Entering removeEventListener()');
        const iframe = this.iframeRef;
        if (iframe) {
            iframe.removeEventListener("load", this.onIFrameLoad);
			logger.debug('<<< Exiting removeEventListener()');
        }
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
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
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        this.removeEventListener();
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    render() {
		logger.debug('>>> Entering render()');
        const {state} = this;

		logger.debug('<<< Exiting render()');
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