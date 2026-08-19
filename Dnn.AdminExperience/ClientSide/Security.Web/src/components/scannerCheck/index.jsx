import React, {Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import { SearchBox, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

class ScannerCheckPanelBody extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }

    renderFileHeader() {
		logger.debug('>>> Entering renderFileHeader()');
        const fileTableFields = [
            { "name": resx.get("FileName"), "id": "FileName" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        let tableHeaders = fileTableFields.map((field, i) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

		logger.debug(`renderFileHeader(): tableHeaders → ${tableHeaders}`);
		logger.debug('<<< Exiting renderFileHeader()');
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedFileList() {
		logger.debug('>>> Entering renderedFileList()');
        const {props} = this;
		logger.debug('<<< Exiting renderedFileList()');
        return props.searchResults.FoundInFiles.map((term, i) => {
            return (
                <div className="scannerCheckItem" key={i}>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.FileName}&nbsp;</span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastWriteTime}&nbsp;</span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderDatabaseHeader() {
		logger.debug('>>> Entering renderDatabaseHeader()');
        const fileTableFields = [
            { "name": resx.get("DatabaseInstance"), "id": "DatabaseInstance" },
            { "name": resx.get("DatabaseValue"), "id": "DatabaseValue" }
        ];
        let tableHeaders = fileTableFields.map((field, i) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

		logger.debug(`renderDatabaseHeader(): tableHeaders → ${tableHeaders}`);
		logger.debug('<<< Exiting renderDatabaseHeader()');
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedDatabaseList() {
		logger.debug('>>> Entering renderedDatabaseList()');
        const {props} = this;
		logger.debug('<<< Exiting renderedDatabaseList()');
        return props.searchResults.FoundInDatabase.map((term, i) => {
            return (
                <div className="scannerCheckItem" key={i}>
                    <div className="label-columnname">
                        <div className="label-wrapper">
                            <span>{term.ColumnName}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-columnvalue">
                        <div className="label-wrapper">
                            <span>{term.ColumnValue}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderRiskFilesHeader() {
		logger.debug('>>> Entering renderRiskFilesHeader()');
        const fileTableFields = [
            { "name": resx.get("FileName"), "id": "FileName" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        let tableHeaders = fileTableFields.map((field, i) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

		logger.debug(`renderRiskFilesHeader(): tableHeaders → ${tableHeaders}`);
		logger.debug('<<< Exiting renderRiskFilesHeader()');
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderRiskFilesList(risk) {
		logger.debug(`>>> Entering renderRiskFilesList(risk=${risk})`);
        const {props} = this;
        let list = props.modifiedFiles.HighRiskFiles;
        if (risk === "low") {
            list = props.modifiedFiles.LowRiskFiles;
        }
		logger.debug(`<<< Exiting renderRiskFilesList(risk=${risk})`);
        return list.map((term, i) => {
            return (
                <div className="scannerCheckItem" key={i}>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.FilePath}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastWriteTime}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderPortalSettingsHeader() {
		logger.debug('>>> Entering renderPortalSettingsHeader()');
        const fileTableFields = [
            { "name": resx.get("PortalId"), "id": "PortalId" },
            { "name": resx.get("SettingName"), "id": "SettingName" },
            { "name": resx.get("SettingValue"), "id": "SettingValue" },
            { "name": resx.get("UserId"), "id": "UserId" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        let tableHeaders = fileTableFields.map((field, i) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

		logger.debug(`renderPortalSettingsHeader(): tableHeaders → ${tableHeaders}`);
		logger.debug('<<< Exiting renderPortalSettingsHeader()');
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedPortalSettingsList() {
		logger.debug('>>> Entering renderedPortalSettingsList()');
        const {props} = this;
		logger.debug('<<< Exiting renderedPortalSettingsList()');
        return props.modifiedSettings.PortalSettings.map((term, i) => {
            return (
                <div className="scannerCheckItem" key={i}>
                    <div className="label-id">
                        <div className="label-wrapper">
                            <span>{term.PortalId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.SettingName}&nbsp;</span>
                        </div>
                    </div>
                    <div className="label-value">
                        <div className="label-wrapper">
                            <span>{term.SettingValue}&nbsp;</span>
                        </div>
                    </div>
                    <div className="label-user">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedByUserId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedOnDate}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderHostSettingsHeader() {
		logger.debug('>>> Entering renderHostSettingsHeader()');
        const fileTableFields = [
            { "name": resx.get("SettingName"), "id": "SettingName" },
            { "name": resx.get("SettingValue"), "id": "SettingValue" },
            { "name": resx.get("UserId"), "id": "UserId" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        let tableHeaders = fileTableFields.map((field, i) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

		logger.debug(`renderHostSettingsHeader(): tableHeaders → ${tableHeaders}`);
		logger.debug('<<< Exiting renderHostSettingsHeader()');
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedHostSettingsList() {
		logger.debug('>>> Entering renderedHostSettingsList()');
        const {props} = this;
		logger.debug('<<< Exiting renderedHostSettingsList()');
        return props.modifiedSettings.PortalSettings.map((term, i) => {
            return (
                <div className="scannerCheckItem" key={i}>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.SettingName}&nbsp;</span>
                        </div>
                    </div>
                    <div className="label-value">
                        <div className="label-wrapper">
                            <span>{term.SettingValue}&nbsp;</span>
                        </div>
                    </div>
                    <div className="label-user">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedByUserId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedOnDate}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderTabSettingsHeader() {
		logger.debug('>>> Entering renderTabSettingsHeader()');
        const fileTableFields = [
            { "name": resx.get("TabId"), "id": "TabId" },
            { "name": resx.get("PortalId"), "id": "PortalId" },
            { "name": resx.get("SettingName"), "id": "SettingName" },
            { "name": resx.get("SettingValue"), "id": "SettingValue" },
            { "name": resx.get("UserId"), "id": "UserId" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        let tableHeaders = fileTableFields.map((field, i) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

		logger.debug(`renderTabSettingsHeader(): tableHeaders → ${tableHeaders}`);
		logger.debug('<<< Exiting renderTabSettingsHeader()');
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedTabSettingsList() {
		logger.debug('>>> Entering renderedTabSettingsList()');
        const {props} = this;
		logger.debug('<<< Exiting renderedTabSettingsList()');
        return props.modifiedSettings.TabSettings.map((term, i) => {
            return (
                <div className="scannerCheckItem" key={i}>
                    <div className="label-tab">
                        <div className="label-wrapper">
                            <span>{term.TabId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-id">
                        <div className="label-wrapper">
                            <span>{term.PortalId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.SettingName}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-value">
                        <div className="label-wrapper">
                            <span>{term.SettingValue}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-user">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedByUserId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedOnDate}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    renderModuleSettingsHeader() {
		logger.debug('>>> Entering renderModuleSettingsHeader()');
        const fileTableFields = [
            { "name": resx.get("Type"), "id": "Type" },
            { "name": resx.get("ModuleId"), "id": "ModuleId" },
            { "name": resx.get("PortalId"), "id": "PortalId" },
            { "name": resx.get("SettingName"), "id": "SettingName" },
            { "name": resx.get("SettingValue"), "id": "SettingValue" },
            { "name": resx.get("UserId"), "id": "UserId" },
            { "name": resx.get("LastWriteTime"), "id": "LastWriteTime" }
        ];
        let tableHeaders = fileTableFields.map((field, i) => {
            let className = "scannerCheckHeader scannerCheckHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

		logger.debug(`renderModuleSettingsHeader(): tableHeaders → ${tableHeaders}`);
		logger.debug('<<< Exiting renderModuleSettingsHeader()');
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedModuleSettingsList() {
		logger.debug('>>> Entering renderedModuleSettingsList()');
        const {props} = this;
		logger.debug('<<< Exiting renderedModuleSettingsList()');
        return props.modifiedSettings.ModuleSettings.map((term, i) => {
            return (
                <div className="scannerCheckItem" key={i}>
                    <div className="label-type">
                        <div className="label-wrapper">
                            <span>{term.Type}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-module">
                        <div className="label-wrapper">
                            <span>{term.ModuleId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-id">
                        <div className="label-wrapper">
                            <span>{term.PortalId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-name">
                        <div className="label-wrapper">
                            <span>{term.SettingName}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-value">
                        <div className="label-wrapper">
                            <span>{term.SettingValue}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-user">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedByUserId}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-date">
                        <div className="label-wrapper">
                            <span>{term.LastModifiedOnDate}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

    onKeywordChanged(keyword) {
		logger.debug(`>>> Entering onKeywordChanged(keyword=${keyword})`);
        const {props} = this;

        if (keyword === "" && props.scannerCheckKeyword !== "") {
            keyword = props.scannerCheckKeyword;
        }

        if (keyword === props.scannerCheckKeyword) {
            props.dispatch(SecurityActions.updatefileSystemAndDatabaseActiveTab("search"));
            return;
        }

        if (keyword && keyword !== "") {
            props.dispatch(SecurityActions.searchFileSystemAndDatabase({ term: keyword }, () => {
            }));
        }
        else {
            props.dispatch(SecurityActions.clearFileSystemAndDatabaseSearch());
        }
        props.dispatch(SecurityActions.updatefileSystemAndDatabaseSearchKeyword(keyword));
		logger.debug(`<<< Exiting onKeywordChanged(keyword=${keyword})`);
    }

    getModifiedSettings() {
		logger.debug('>>> Entering getModifiedSettings()');
        const {props} = this;
        if (props.scannerCheckActiveTab === "settings") {
            return;
        }

        if (props.modifiedSettings) {
            props.dispatch(SecurityActions.updatefileSystemAndDatabaseActiveTab("settings"));
            return;
        }
        props.dispatch(SecurityActions.getLastModifiedSettings(() => {
        }));
		logger.debug('<<< Exiting getModifiedSettings()');
    }

    getModifiedFiles() {
		logger.debug('>>> Entering getModifiedFiles()');
        const {props} = this;
        if (props.scannerCheckActiveTab === "files") {
            return;
        }

        if (props.modifiedFiles) {
            props.dispatch(SecurityActions.updatefileSystemAndDatabaseActiveTab("files"));
            return;
        }
        props.dispatch(SecurityActions.getLastModifiedFiles(() => {
        }));
		logger.debug('<<< Exiting getModifiedFiles()');
    }

    showSearchResults() {
		logger.debug('>>> Entering showSearchResults()');
        const {props} = this;
        if (props.searchResults && props.scannerCheckActiveTab === "search") {
		logger.debug('<<< Exiting showSearchResults()');
            return true;
        }
        return false;
    }

    showModifiedSettings() {
		logger.debug('>>> Entering showModifiedSettings()');
        const {props} = this;
        if (props.modifiedSettings && props.scannerCheckActiveTab === "settings") {
		logger.debug('<<< Exiting showModifiedSettings()');
            return true;
        }
        return false;
    }

    showModifiedFiles() {
		logger.debug('>>> Entering showModifiedFiles()');
        const {props} = this;
        if (props.modifiedFiles && props.scannerCheckActiveTab === "files") {
		logger.debug('<<< Exiting showModifiedFiles()');
            return true;
        }
        return false;
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
		logger.debug('<<< Exiting render()');
        return (
            <div className={styles.scannerCheckResults}>
                <div className="scannercheck-topbar">
                    <div className={props.scannerCheckActiveTab === "search" ? "search-filter-active" : "search-filter"}>
                        <SearchBox
                            placeholder={resx.get("ScannerChecks") }
                            initialValue={props.scannerCheckKeyword && props.scannerCheckKeyword.length > 0 ? props.scannerCheckKeyword : null}
                            onSearch={this.onKeywordChanged.bind(this) }
                            maxLength={50} />
                        <div className="clear"></div>
                    </div>
                    <div
                        className={this.showModifiedSettings() ? "settings-filter-active" : "settings-filter"}
                        onClick={this.getModifiedSettings.bind(this) }>
                        <TextOverflowWrapper text={resx.get("ModifiedSettings") } maxWidth={150}/>
                    </div>
                    <div
                        className={this.showModifiedFiles() ? "files-filter-active" : "files-filter"}
                        onClick={this.getModifiedFiles.bind(this) }>
                        <TextOverflowWrapper text={resx.get("ModifiedFiles") } maxWidth={150}/>
                    </div>
                </div>
                {this.showSearchResults() && props.searchResults.FoundInFiles &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("SearchFileSystemResult").replace("{0}", props.searchResults.FoundInFiles.length) }
                        </div>
                        {props.searchResults.FoundInFiles.length > 0 &&
                            <div className="scannerCheckItems">
                                { this.renderFileHeader() }
                                { this.renderedFileList() }
                            </div>
                        }
                    </div>
                }
                {this.showSearchResults() && props.searchResults.FoundInDatabase &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("SearchDatabaseResult").replace("{0}", props.searchResults.FoundInDatabase.length) }
                        </div>
                        {props.searchResults.FoundInDatabase.length > 0 &&
                            <div className="scannerCheckItems">
                                { this.renderDatabaseHeader() }
                                { this.renderedDatabaseList() }
                            </div>
                        }
                    </div>
                }
                {this.showModifiedFiles() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("HighRiskFiles") }
                        </div>
                        <div className="scannerCheckItems-riskFiles">
                            { this.renderRiskFilesHeader() }
                            { this.renderRiskFilesList("high") }
                        </div>
                    </div>
                }
                {this.showModifiedFiles() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("LowRiskFiles") }
                        </div>
                        <div className="scannerCheckItems-riskFiles">
                            { this.renderRiskFilesHeader() }
                            { this.renderRiskFilesList("low") }
                        </div>
                    </div>
                }
                {this.showModifiedSettings() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("PortalSettings") }
                        </div>
                        <div className="scannerCheckItems-portalSettings">
                            { this.renderPortalSettingsHeader() }
                            { this.renderedPortalSettingsList() }
                        </div>
                    </div>
                }
                {this.showModifiedSettings() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("HostSettings") }
                        </div>
                        <div className="scannerCheckItems-hostSettings">
                            { this.renderHostSettingsHeader() }
                            { this.renderedHostSettingsList() }
                        </div>
                    </div>
                }
                {this.showModifiedSettings() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("TabSettings") }
                        </div>
                        <div className="scannerCheckItems-tabSettings">
                            { this.renderTabSettingsHeader() }
                            { this.renderedTabSettingsList() }
                        </div>
                    </div>
                }
                {this.showModifiedSettings() &&
                    <div>
                        <div className="scannerCheckItems-title">
                            {resx.get("ModuleSettings") }
                        </div>
                        <div className="scannerCheckItems-moduleSettings">
                            { this.renderModuleSettingsHeader() }
                            { this.renderedModuleSettingsList() }
                        </div>
                    </div>
                }
            </div>
        );
    }
}

ScannerCheckPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    searchResults: PropTypes.object,
    modifiedSettings: PropTypes.object,
    modifiedFiles: PropTypes.object,
    scannerCheckKeyword: PropTypes.string,
    scannerCheckActiveTab: PropTypes.string
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.tabIndex,
        searchResults: state.security.searchResults,
        modifiedSettings: state.security.modifiedSettings,
        modifiedFiles: state.security.modifiedFiles,
        scannerCheckKeyword: state.security.scannerCheckKeyword,
        scannerCheckActiveTab: state.security.scannerCheckActiveTab
    };
}

export default connect(mapStateToProps)(ScannerCheckPanelBody);