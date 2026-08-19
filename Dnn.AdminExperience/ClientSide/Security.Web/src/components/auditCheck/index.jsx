 
import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import { ContentLoadWrapper, SvgIcons } from "@dnnsoftware/dnn-react-common";
import resx from "../../resources";
import Html from "../Html";
import styles from "./style.module.less";
const logger = console;

class AuditCheckPanelBody extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');

        this.state = {
            checking: false
        };
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        const {props} = this;
        if (props.auditCheckResults) {
            return;
        }
        props.dispatch(SecurityActions.getAuditCheckResults());
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }

    renderHeader() {
		logger.debug('>>> Entering renderHeader()');
        const tableFields = [
            { "name": resx.get("SecurityCheck"), "id": "SecurityCheck" },
            { "name": resx.get("Result"), "id": "Result" },
            { "name": resx.get("Notes"), "id": "Notes" }
        ];
        let tableHeaders = tableFields.map((field, i) => {
            let className = "auditCheckHeader auditCheckHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

		logger.debug(`renderHeader(): tableHeaders → ${tableHeaders}`);
		logger.debug('<<< Exiting renderHeader()');
        return <div className="auditCheckHeader-wrapper">{tableHeaders}</div>;
    }

    getResultDisplay(result) {
		logger.debug(`>>> Entering getResultDisplay(result=${result})`);
        const severity = result.Severity;
        const successText = result.SuccessText;
        const failureText = result.FailureText;
        const reason = result.Reason;
        
        switch (severity) {
            case 0:
                return (
                    <div className="label-result-severity">
                        <div className="label-result-severity-pass">
                            {resx.get("Pass")}
                        </div>
                        <div>
                            {successText}
                        </div>
                    </div>
                );
            case 1:
                return (
                    <div className="label-result-severity">
                        <div className="label-result-severity-alert">
                            {resx.get("Alert")}
                        </div>
                        <div>
                            {failureText}
                        </div>
                    </div>
                );
            case 2:
                return (
                    <div className="label-result-severity">
                        <div className="label-result-severity-fail">
                            {resx.get("Fail")}
                        </div>
                        <div><Html html={failureText} /></div>
                    </div>
                );
            default:
				logger.debug(`<<< Exiting getResultDisplay(result=${result})`);
                return (
                    <div className="label-result-severity">
                        <div className="label-result-severity-unverified" onClick={this.onAuditCheck.bind(this, result)}>
                            {resx.get("Check")}
                        </div>
                        <div>
                            {reason}
                        </div>
                    </div>
                );
        }
    }

    onAuditCheck(result) {
		logger.debug(`>>> Entering onAuditCheck(result=${result})`);
        const {state, props} = this;
        if (state.checking) {
            return;
        }

        this.setState({checking: true}, () => {
            props.dispatch(SecurityActions.getAuditCheckResult(result.CheckName, () => {
                this.setState({checking: false});
            }));
        });
		logger.debug(`<<< Exiting onAuditCheck(result=${result})`);
    }

     
    getNotesDisplay(notes) {
		logger.debug(`>>> Entering getNotesDisplay(notes=${notes})`);
        if (notes && notes.length > 0) {
            return <div className="log-detail"><Html html={notes } /></div>;
        }
        else {
			logger.debug(`<<< Exiting getNotesDisplay(notes=${notes})`);
            return "N/A";
        }
    }

    renderedList() {
		logger.debug('>>> Entering renderedList()');
        const {props} = this;
		logger.debug('<<< Exiting renderedList()');
        return props.auditCheckResults.map((term, i) => {
            return (
                <div className="auditCheckItem" key={i}>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.CheckNameText}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-result">
                        <div className="label-wrapper">
                            <span>{this.getResultDisplay(term)}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-notes">
                        <div className="label-wrapper">
                            <span>{this.getNotesDisplay(term.Notes)}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        let contentShouldShow = (props.auditCheckResults && props.auditCheckResults.length > 0) ? true : false;
		logger.debug('<<< Exiting render()');
        return (
            <ContentLoadWrapper loadComplete={contentShouldShow}
                svgSkeleton={<div><SvgIcons.TableEmptyState /></div>}>
                <div className={styles.auditCheckResults}>
                    <div className="auditcheck-topbar">
                        {resx.get("AuditExplanation")}
                    </div>
                    <div className="auditCheckItems">
                        {contentShouldShow && this.renderHeader()}
                        {contentShouldShow && this.renderedList()}
                    </div>
                </div>
            </ContentLoadWrapper>
        );

    }
}

AuditCheckPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    auditCheckResults: PropTypes.object
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.tabIndex,
        auditCheckResults: state.security.auditCheckResults
    };
}

export default connect(mapStateToProps)(AuditCheckPanelBody);
