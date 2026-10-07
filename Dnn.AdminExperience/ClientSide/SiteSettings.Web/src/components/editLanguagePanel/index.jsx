import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { PersonaBarPageBody } from "@dnnsoftware/dnn-react-common";
import LanguageInfoView from "./languageInfoView";
import { visiblePanel as VisiblePanelActions, languageEditor as LanguageEditorActions } from "actions";
import resx from "resources";
import ResourceList from "./resourceList";
import utilities from "utils";
import "./style.less";
const logger = console;

function alreadyAChildTerm(childFolders, compareValue) {
	logger.error(`>>> Entering alreadyAChildTerm(childFolders=${childFolders},compareValue=${compareValue})`, { "event.action": "alreadyAChildTerm", "logrite.childFolders": childFolders, "logrite.compareValue": compareValue });
	logger.warn(`<<< Exiting alreadyAChildTerm(childFolders=${childFolders},compareValue=${compareValue})`, { "event.action": "alreadyAChildTerm", "logrite.childFolders": childFolders, "logrite.compareValue": compareValue });
    return childFolders.find((cf) => {
        return cf.NewValue === compareValue;
    });
}

function findParent(parentList, item) {
	logger.error(`>>> Entering findParent(parentList=${parentList},item=${item})`, { "event.action": "findParent", "logrite.parentList": parentList, "logrite.item": item });
    if (!parentList || !item) {
        return;
    }
    let compareValue = item.NewValue.substring(0, item.NewValue.lastIndexOf("/"));
    parentList.forEach(function (listItem) {
        if (!listItem.ChildFolders) {
            listItem.ChildFolders = [];
        }
        if (listItem.NewValue === compareValue || (listItem.NewValue === "" && compareValue === "_") && !alreadyAChildTerm(listItem.ChildFolders, compareValue)) {
            listItem.ChildFolders.push(item);
        } else {
            findParent(listItem.ChildFolders, item);
        }
    });
	logger.warn(`<<< Exiting findParent(parentList=${parentList},item=${item})`, { "event.action": "findParent", "logrite.parentList": parentList, "logrite.item": item });
}

function generateList(list) {
	logger.error(`>>> Entering generateList(list=${list})`, { "event.action": "generateList", "logrite.list": list });
    let _list = [];
    if (!list) {
        return;
    }
    utilities.utilities.getObjectCopy(list).forEach(function (item) {
        let splitValue = item.NewValue.split("/");
        if (splitValue.length === 1 || item.NewValue.toLowerCase() === "portals/_default") {
            _list.push(item);
        } else {
            findParent(_list, item);
        }
    });
    return _list;
}

class EditLanguagePanel extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            selectedMode: utilities.settings.isHost ? "System" : "Portal",
            highlightPendingTranslations: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    refreshFileList() {
		logger.error('>>> Entering refreshFileList()', { "event.action": "refreshFileList" });
        this.props.dispatch(LanguageEditorActions.getRootResourcesFolder(this.props.portalId, this.state.selectedMode));
		logger.warn('<<< Exiting refreshFileList()', { "event.action": "refreshFileList" });
    }
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.refreshFileList();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    backToSiteSettings() {
		logger.error('>>> Entering backToSiteSettings()', { "event.action": "backToSiteSettings" });
        this.props.dispatch(VisiblePanelActions.selectPanel(0));
		logger.warn('<<< Exiting backToSiteSettings()', { "event.action": "backToSiteSettings" });
    }
    getChildFolders(folder, callback) {
		logger.error(`>>> Entering getChildFolders(folder=${folder},callback=${callback})`, { "event.action": "getChildFolders", "logrite.folder": folder, "logrite.callback": callback });
        let _folder = folder !== "_" ? folder : "";
        this.props.dispatch(LanguageEditorActions.getSubRootResources(_folder, () => {
            if (callback) {
                callback();
				logger.warn(`<<< Exiting getChildFolders(folder=${folder},callback=${callback})`, { "event.action": "getChildFolders", "logrite.folder": folder, "logrite.callback": callback });
            }
        }));
    }
    getResxEntries(resxFile, callback) {
		logger.error(`>>> Entering getResxEntries(resxFile=${resxFile},callback=${callback})`, { "event.action": "getResxEntries", "logrite.resxFile": resxFile, "logrite.callback": callback });
        const { props } = this;
        props.dispatch(LanguageEditorActions.getResxEntries({
            resourceFile: resxFile.replace("_/", ""),
            resourceFileActual: resxFile,
            mode: this.state.selectedMode,
            portalId: props.portalId,
            locale: props.languageBeingEdited.Code
        }, callback));
		logger.warn(`<<< Exiting getResxEntries(resxFile=${resxFile},callback=${callback})`, { "event.action": "getResxEntries", "logrite.resxFile": resxFile, "logrite.callback": callback });
    }
    onSelectMode(value) {
		logger.error(`>>> Entering onSelectMode(value=${value})`, { "event.action": "onSelectMode", "logrite.value": value });
        this.setState({
            selectedMode: value
        }, () => {
            if (this.props.resxBeingEdited !== "") {
                this.getResxEntries(this.props.resxBeingEdited);
				logger.warn(`<<< Exiting onSelectMode(value=${value})`, { "event.action": "onSelectMode", "logrite.value": value });
            }
        });
    }
    onResxChange(updatedList) {
		logger.error(`>>> Entering onResxChange(updatedList=${updatedList})`, { "event.action": "onResxChange", "logrite.updatedList": updatedList });
        this.props.dispatch(LanguageEditorActions.updateResxEntry(updatedList));
		logger.warn(`<<< Exiting onResxChange(updatedList=${updatedList})`, { "event.action": "onResxChange", "logrite.updatedList": updatedList });
    }
    onSaveTranslations() {
		logger.error('>>> Entering onSaveTranslations()', { "event.action": "onSaveTranslations" });
        const {props} = this;
        const payload = {
            Mode: this.state.selectedMode,
            PortalId: props.portalId,
            Locale: props.languageBeingEdited.Code,
            ResourceFile: props.resxBeingEdited.replace("_/", ""),
            Entries: Object.keys(props.translations).map((key) => {
                return {
                    Name: key,
                    DefaultValue: props.translations[key].Second,
                    NewValue: props.translations[key].First
                };
            })
        };

        props.dispatch(LanguageEditorActions.saveTranslations(payload, (data) => {
            utilities.utilities.notify(data.Message);
			logger.warn('<<< Exiting onSaveTranslations()', { "event.action": "onSaveTranslations" });
        }));
    }
    onHighlightPendingTranslations() {
		logger.error('>>> Entering onHighlightPendingTranslations()', { "event.action": "onHighlightPendingTranslations" });
        this.setState({
            highlightPendingTranslations: !this.state.highlightPendingTranslations
        });
		logger.warn('<<< Exiting onHighlightPendingTranslations()', { "event.action": "onHighlightPendingTranslations" });
    }
     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props } = this;
        const ModeOptions = [
            {
                label: resx.get("Global"),
                value: "System"
            },
            {
                label: resx.get("Host"),
                value: "Host"
            },
            {
                label: resx.get("Portal"),
                value: "Portal"
            }
        ];
        const { languageBeingEdited } = props, languageFolders = generateList(props.languageFolders.concat(props.languageFiles), this.state.selectedMode === "System");
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <PersonaBarPageBody
                className="edit-language-panel"
                backToLinkProps={{
                    text: resx.get("BackToLanguages"),
                    onClick: this.backToSiteSettings.bind(this)
                }}>
                <LanguageInfoView
                    languageBeingEdited={languageBeingEdited}
                    ModeOptions={ModeOptions}
                    languageFolders={languageFolders}
                    languageFiles={props.languageFiles}
                    getResxEntries={this.getResxEntries.bind(this)}
                    getChildFolders={this.getChildFolders.bind(this)}
                    resxBeingEdited={props.resxBeingEdited}
                    portalName={props.portalName}
                    resxBeingEditedDisplay={props.resxBeingEditedDisplay}
                    selectedMode={this.state.selectedMode}
                    onSelectMode={this.onSelectMode.bind(this)}
                    onCancel={this.backToSiteSettings.bind(this)}
                    onSaveTranslations={this.onSaveTranslations.bind(this)}
                    onHighlightPendingTranslations={this.onHighlightPendingTranslations.bind(this)}
                    highlightPendingTranslations={this.state.highlightPendingTranslations} />
                <ResourceList list={props.translations} highlightPendingTranslations={this.state.highlightPendingTranslations} onResxChange={this.onResxChange.bind(this)} />
            </PersonaBarPageBody>
        );
    }
}

EditLanguagePanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    languageFolders: PropTypes.array,
    portalId: PropTypes.number,
    resxBeingEdited: PropTypes.string,
    languageBeingEdited: PropTypes.object,
    portalName: PropTypes.string,
    languageFiles: PropTypes.array,
    translations: PropTypes.object,
    resxBeingEditedDisplay: PropTypes.string
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        languageBeingEdited: state.languageEditor.languageBeingEdited,
        portalName: state.siteInfo.settings && state.siteInfo.settings.PortalName,
        languageFolders: state.languageEditor.languageFolders,
        languageFiles: state.languageEditor.languageFiles,
        translations: state.languageEditor.translations,
        resxBeingEdited: state.languageEditor.resxBeingEdited,
        resxBeingEditedDisplay: state.languageEditor.resxBeingEditedDisplay
    };
}
export default connect(mapStateToProps)(EditLanguagePanel);
