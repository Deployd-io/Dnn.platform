import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    languages as LanguagesActions
} from "../../../actions";
import LanguageVerifierGrid from "./languageVerifierGrid";
import { PersonaBarPageBody } from "@dnnsoftware/dnn-react-common";
import resx from "../../../resources";
import styles from "./style.module.less";
const logger = console;

class LanguageVerifierPanelBody extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const { props } = this;
        if (prevProps.selectedPage !== props.selectedPage && props.selectedPage === 2) {
            if (props.verificationResults) {
                this.setState({
                    verificationResults: props.verificationResults
                });
                return;
            }
            props.dispatch(LanguagesActions.verifyLanguageResourceFiles((data) => {
                this.setState({
                    verificationResults: Object.assign({}, data.Results)
                });
            }));
        }
		logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
    }

    renderedResults() {
		logger.error('>>> Entering renderedResults()', { "event.action": "renderedResults" });
        if (this.props.verificationResults) {
			logger.warn('<<< Exiting renderedResults()', { "event.action": "renderedResults" });
            return this.props.verificationResults.map((item, i) => {
                return (
                    <LanguageVerifierGrid
                        language={item.Language}
                        isDefault={item.IsSystemDefault}
                        icon={item.Icon}
                        missingFiles={item.MissingFiles}
                        filesWithDuplicateEntries={item.FilesWithDuplicateEntries}
                        filesWithMissingEntries={item.FilesWithMissingEntries}
                        filesWithObsoleteEntries={item.FilesWithObsoleteEntries}
                        oldFiles={item.OldFiles}
                        malformedFiles={item.MalformedFiles}
                        key={i}>
                    </LanguageVerifierGrid>
                );
            });
        }
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.languageVerifier}>                
                <PersonaBarPageBody backToLinkProps={{
                    text: resx.get("BackToLanguages"),
                    onClick: this.props.closeLanguageVerifier.bind(this)
                }}>
                    {this.renderedResults()}
                </PersonaBarPageBody>
            </div>
        );
    }
}

LanguageVerifierPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    portalId: PropTypes.number,
    closeLanguageVerifier: PropTypes.func,
    verificationResults: PropTypes.array,
    selectedPage: PropTypes.number
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        tabIndex: state.pagination.tabIndex,
        selectedPage: state.visiblePanel.selectedPage,
        verificationResults: state.languages.verificationResults
    };
}

export default connect(mapStateToProps)(LanguageVerifierPanelBody);