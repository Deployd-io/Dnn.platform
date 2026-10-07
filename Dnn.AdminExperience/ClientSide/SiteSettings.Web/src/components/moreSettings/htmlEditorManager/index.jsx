import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import resx from "../../../resources";
import { BackToLink } from "@dnnsoftware/dnn-react-common";
import util from "utils";
import styles from "./style.module.less";
const logger = console;

class HtmlEditorManagerPanelBody extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            url: undefined
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.setState({
            url: util.siteRoot + "Host/HTMLEditorManager?portalid=" + this.props.portalId + "&portpopUp=true"
        });
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="dnn-persona-bar-page-body">
                <div className={styles.htmlEditorManager}>
                    <BackToLink onClick={this.props.closeHtmlEditorManager} text={resx.get("BackToSiteBehavior") } />
                    <iframe className="htmlEditorIframe" src={this.state.url} frameBorder="0" />
                </div>
            </div>
        );
    }
}

HtmlEditorManagerPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    portalId: PropTypes.number,
    closeHtmlEditorManager: PropTypes.func
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(HtmlEditorManagerPanelBody);