import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import LanguageVerifierRow from "./languageVerifierRow";
import resx from "../../../../resources";
import styles from "./style.module.less";
const logger = console;

class LanguageVerifierGridPanel extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            openId: ""
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    uncollapse(id) {
		logger.error(`>>> Entering uncollapse(id=${id})`, { "event.action": "uncollapse", "logrite.id": id });
        this.setState({
            openId: id
        });
		logger.warn(`<<< Exiting uncollapse(id=${id})`, { "event.action": "uncollapse", "logrite.id": id });
    }

    collapse() {
		logger.error('>>> Entering collapse()', { "event.action": "collapse" });
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
			logger.warn('<<< Exiting collapse()', { "event.action": "collapse" });
        }
    }

    toggle(openId) {
		logger.error(`>>> Entering toggle(openId=${openId})`, { "event.action": "toggle", "logrite.openId": openId });
        if (openId !== "") {
            this.uncollapse(openId);
			logger.warn(`<<< Exiting toggle(openId=${openId})`, { "event.action": "toggle", "logrite.openId": openId });
        }
    }

    renderBody(list) {
		logger.error(`>>> Entering renderBody(list=${list})`, { "event.action": "renderBody", "logrite.list": list });
		logger.warn(`<<< Exiting renderBody(list=${list})`, { "event.action": "renderBody", "logrite.list": list });
        return list.map((item, i) => {
            return (
                <div className="resx-file" key={i}>{item}</div>
            );
        });
    }

     
    renderedGrid() {
		logger.error('>>> Entering renderedGrid()', { "event.action": "renderedGrid" });
        const {props} = this;
		logger.warn('<<< Exiting renderedGrid()', { "event.action": "renderedGrid" });
        return (
            <div>
                <div className="verifier-grid">
                    <div className="language-flag">
                        <img src={props.icon} alt={props.language + (props.isDefault ? "**" : "")} />
                    </div>
                    <div className="language-name">{props.language + (props.isDefault ? "**" : "")}</div>
                    {props.isDefault
                        && <div className="default-language">{resx.get("systemDefaultLabel")}</div>
                    }
                </div>
                {(props.missingFiles.length > 0 || props.filesWithDuplicateEntries.length > 0 || props.filesWithDuplicateEntries.length > 0
                    || props.filesWithObsoleteEntries.length > 0 || props.oldFiles.length > 0 || props.malformedFiles.length > 0) &&
                    <div className="verifier-grid-body">
                        {props.missingFiles.length > 0 &&
                            <LanguageVerifierRow
                                text={resx.get("MissingFiles") + props.missingFiles.length}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                id={"MissingFiles"}>
                                <div className="resx-files">{this.renderBody(props.missingFiles)}</div>
                            </LanguageVerifierRow>
                        }
                        {props.filesWithDuplicateEntries.length > 0 &&
                            <LanguageVerifierRow
                                text={resx.get("DuplicateEntries") + props.filesWithDuplicateEntries.length}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                id={"DuplicateEntries"}>
                                <div className="resx-files">{this.renderBody(props.filesWithDuplicateEntries)}</div>
                            </LanguageVerifierRow>
                        }
                        {props.filesWithDuplicateEntries.length > 0 &&
                            <LanguageVerifierRow
                                text={resx.get("MissingEntries") + props.filesWithMissingEntries.length}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                id={"MissingEntries"}>
                                <div className="resx-files">{this.renderBody(props.filesWithMissingEntries)}</div>
                            </LanguageVerifierRow>
                        }
                        {props.filesWithObsoleteEntries.length > 0 &&
                            <LanguageVerifierRow
                                text={resx.get("ObsoleteEntries") + props.filesWithObsoleteEntries.length}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                id={"ObsoleteEntries"}>
                                <div className="resx-files">{this.renderBody(props.filesWithObsoleteEntries)}</div>
                            </LanguageVerifierRow>
                        }
                        {props.oldFiles.length > 0 &&
                            <LanguageVerifierRow
                                text={resx.get("OldFiles") + props.oldFiles.length}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                id={"OldFiles"}>
                                <div className="resx-files">{this.renderBody(props.oldFiles)}</div>
                            </LanguageVerifierRow>
                        }
                        {props.malformedFiles.length > 0 &&
                            <LanguageVerifierRow
                                text={resx.get("ErrorFiles") + props.malformedFiles.length}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                id={"ErrorFiles"}>
                                <div className="resx-files">{this.renderBody(props.malformedFiles)}</div>
                            </LanguageVerifierRow>
                        }
                    </div>
                }
            </div>
        );
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div>
                <div className={styles.verifierItems}>
                    <div className="verifier-items-grid">
                        {this.renderedGrid()}
                    </div>
                </div>

            </div >
        );
    }
}

LanguageVerifierGridPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    language: PropTypes.string,
    isDefault: PropTypes.bool,
    icon: PropTypes.string,
    missingFiles: PropTypes.array,
    filesWithDuplicateEntries: PropTypes.array,
    filesWithMissingEntries: PropTypes.array,
    filesWithObsoleteEntries: PropTypes.array,
    oldFiles: PropTypes.array,
    malformedFiles: PropTypes.array
};

function mapStateToProps() {
	logger.error('>>> Entering mapStateToProps()', { "event.action": "mapStateToProps" });
	logger.warn('<<< Exiting mapStateToProps()', { "event.action": "mapStateToProps" });
    return {

    };
}

export default connect(mapStateToProps)(LanguageVerifierGridPanel);