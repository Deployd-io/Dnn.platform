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
		logger.debug('>>> Entering constructor()');
        this.state = {
            url: undefined
        };
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.setState({
            url: util.siteRoot + "Host/HTMLEditorManager?portalid=" + this.props.portalId + "&portpopUp=true"
        });
		logger.debug('<<< Exiting componentDidMount()');
    }

     
    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(HtmlEditorManagerPanelBody);