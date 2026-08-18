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
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
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
		logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
    }

    renderedResults() {
		logger.debug('>>> Entering renderedResults()');
        if (this.props.verificationResults) {
			logger.debug('<<< Exiting renderedResults()');
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
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.tabIndex,
        selectedPage: state.visiblePanel.selectedPage,
        verificationResults: state.languages.verificationResults
    };
}

export default connect(mapStateToProps)(LanguageVerifierPanelBody);