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
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    renderFileHeader() {
		logger.error('>>> Entering renderFileHeader()', { "event.action": "renderFileHeader" });
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

		logger.error(`renderFileHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderFileHeader" });
		logger.warn('<<< Exiting renderFileHeader()', { "event.action": "renderFileHeader" });
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedFileList() {
		logger.error('>>> Entering renderedFileList()', { "event.action": "renderedFileList" });
        const {props} = this;
		logger.warn('<<< Exiting renderedFileList()', { "event.action": "renderedFileList" });
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
		logger.error('>>> Entering renderDatabaseHeader()', { "event.action": "renderDatabaseHeader" });
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

		logger.error(`renderDatabaseHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderDatabaseHeader" });
		logger.warn('<<< Exiting renderDatabaseHeader()', { "event.action": "renderDatabaseHeader" });
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedDatabaseList() {
		logger.error('>>> Entering renderedDatabaseList()', { "event.action": "renderedDatabaseList" });
        const {props} = this;
		logger.warn('<<< Exiting renderedDatabaseList()', { "event.action": "renderedDatabaseList" });
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
		logger.error('>>> Entering renderRiskFilesHeader()', { "event.action": "renderRiskFilesHeader" });
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

		logger.error(`renderRiskFilesHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderRiskFilesHeader" });
		logger.warn('<<< Exiting renderRiskFilesHeader()', { "event.action": "renderRiskFilesHeader" });
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderRiskFilesList(risk) {
		logger.error(`>>> Entering renderRiskFilesList(risk=${risk})`, { "event.action": "renderRiskFilesList", "logrite.risk": risk });
        const {props} = this;
        let list = props.modifiedFiles.HighRiskFiles;
        if (risk === "low") {
            list = props.modifiedFiles.LowRiskFiles;
        }
		logger.warn(`<<< Exiting renderRiskFilesList(risk=${risk})`, { "event.action": "renderRiskFilesList", "logrite.risk": risk });
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
		logger.error('>>> Entering renderPortalSettingsHeader()', { "event.action": "renderPortalSettingsHeader" });
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

		logger.error(`renderPortalSettingsHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderPortalSettingsHeader" });
		logger.warn('<<< Exiting renderPortalSettingsHeader()', { "event.action": "renderPortalSettingsHeader" });
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedPortalSettingsList() {
		logger.error('>>> Entering renderedPortalSettingsList()', { "event.action": "renderedPortalSettingsList" });
        const {props} = this;
		logger.warn('<<< Exiting renderedPortalSettingsList()', { "event.action": "renderedPortalSettingsList" });
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
		logger.error('>>> Entering renderHostSettingsHeader()', { "event.action": "renderHostSettingsHeader" });
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

		logger.error(`renderHostSettingsHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderHostSettingsHeader" });
		logger.warn('<<< Exiting renderHostSettingsHeader()', { "event.action": "renderHostSettingsHeader" });
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedHostSettingsList() {
		logger.error('>>> Entering renderedHostSettingsList()', { "event.action": "renderedHostSettingsList" });
        const {props} = this;
		logger.warn('<<< Exiting renderedHostSettingsList()', { "event.action": "renderedHostSettingsList" });
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
		logger.error('>>> Entering renderTabSettingsHeader()', { "event.action": "renderTabSettingsHeader" });
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

		logger.error(`renderTabSettingsHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderTabSettingsHeader" });
		logger.warn('<<< Exiting renderTabSettingsHeader()', { "event.action": "renderTabSettingsHeader" });
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedTabSettingsList() {
		logger.error('>>> Entering renderedTabSettingsList()', { "event.action": "renderedTabSettingsList" });
        const {props} = this;
		logger.warn('<<< Exiting renderedTabSettingsList()', { "event.action": "renderedTabSettingsList" });
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
		logger.error('>>> Entering renderModuleSettingsHeader()', { "event.action": "renderModuleSettingsHeader" });
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

		logger.error(`renderModuleSettingsHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderModuleSettingsHeader" });
		logger.warn('<<< Exiting renderModuleSettingsHeader()', { "event.action": "renderModuleSettingsHeader" });
        return <div className="scannerCheckHeader-wrapper">{tableHeaders}</div>;
    }

    renderedModuleSettingsList() {
		logger.error('>>> Entering renderedModuleSettingsList()', { "event.action": "renderedModuleSettingsList" });
        const {props} = this;
		logger.warn('<<< Exiting renderedModuleSettingsList()', { "event.action": "renderedModuleSettingsList" });
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
		logger.error(`>>> Entering onKeywordChanged(keyword=${keyword})`, { "event.action": "onKeywordChanged", "logrite.keyword": keyword });
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
		logger.warn(`<<< Exiting onKeywordChanged(keyword=${keyword})`, { "event.action": "onKeywordChanged", "logrite.keyword": keyword });
    }

    getModifiedSettings() {
		logger.error('>>> Entering getModifiedSettings()', { "event.action": "getModifiedSettings" });
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
		logger.warn('<<< Exiting getModifiedSettings()', { "event.action": "getModifiedSettings" });
    }

    getModifiedFiles() {
		logger.error('>>> Entering getModifiedFiles()', { "event.action": "getModifiedFiles" });
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
		logger.warn('<<< Exiting getModifiedFiles()', { "event.action": "getModifiedFiles" });
    }

    showSearchResults() {
		logger.error('>>> Entering showSearchResults()', { "event.action": "showSearchResults" });
        const {props} = this;
        if (props.searchResults && props.scannerCheckActiveTab === "search") {
            return true;
        }
		logger.warn('<<< Exiting showSearchResults()', { "event.action": "showSearchResults" });
        return false;
    }

    showModifiedSettings() {
		logger.error('>>> Entering showModifiedSettings()', { "event.action": "showModifiedSettings" });
        const {props} = this;
        if (props.modifiedSettings && props.scannerCheckActiveTab === "settings") {
            return true;
        }
		logger.warn('<<< Exiting showModifiedSettings()', { "event.action": "showModifiedSettings" });
        return false;
    }

    showModifiedFiles() {
		logger.error('>>> Entering showModifiedFiles()', { "event.action": "showModifiedFiles" });
        const {props} = this;
        if (props.modifiedFiles && props.scannerCheckActiveTab === "files") {
            return true;
        }
		logger.warn('<<< Exiting showModifiedFiles()', { "event.action": "showModifiedFiles" });
        return false;
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
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