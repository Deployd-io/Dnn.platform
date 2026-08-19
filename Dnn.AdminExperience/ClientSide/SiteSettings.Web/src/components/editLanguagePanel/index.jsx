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
	logger.debug(`>>> Entering alreadyAChildTerm(childFolders=${childFolders},compareValue=${compareValue})`);
	logger.debug(`<<< Exiting alreadyAChildTerm(childFolders=${childFolders},compareValue=${compareValue})`);
    return childFolders.find((cf) => {
        return cf.NewValue === compareValue;
    });
}

function findParent(parentList, item) {
	logger.debug(`>>> Entering findParent(parentList=${parentList},item=${item})`);
    if (!parentList || !item) {
        return;
    }
    let compareValue = item.NewValue.substring(0, item.NewValue.lastIndexOf("/"));
    parentList.forEach(function (listItem) {
		logger.debug('>>> Entering forEach()');
        if (!listItem.ChildFolders) {
            listItem.ChildFolders = [];
        }
        if (listItem.NewValue === compareValue || (listItem.NewValue === "" && compareValue === "_") && !alreadyAChildTerm(listItem.ChildFolders, compareValue)) {
            listItem.ChildFolders.push(item);
        } else {
            findParent(listItem.ChildFolders, item);
			logger.debug('<<< Exiting forEach()');
        }
    });
	logger.debug(`<<< Exiting findParent(parentList=${parentList},item=${item})`);
}

function generateList(list) {
	logger.debug(`>>> Entering generateList(list=${list})`);
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
	logger.debug(`<<< Exiting generateList(list=${list})`);
    return _list;
}

class EditLanguagePanel extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            selectedMode: utilities.settings.isHost ? "System" : "Portal",
            highlightPendingTranslations: false
        };
		logger.debug('<<< Exiting constructor()');
    }
    refreshFileList() {
		logger.debug('>>> Entering refreshFileList()');
        this.props.dispatch(LanguageEditorActions.getRootResourcesFolder(this.props.portalId, this.state.selectedMode));
		logger.debug('<<< Exiting refreshFileList()');
    }
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.refreshFileList();
		logger.debug('<<< Exiting componentDidMount()');
    }

    backToSiteSettings() {
		logger.debug('>>> Entering backToSiteSettings()');
        this.props.dispatch(VisiblePanelActions.selectPanel(0));
		logger.debug('<<< Exiting backToSiteSettings()');
    }
    getChildFolders(folder, callback) {
		logger.debug(`>>> Entering getChildFolders(folder=${folder},callback=${callback})`);
        let _folder = folder !== "_" ? folder : "";
        this.props.dispatch(LanguageEditorActions.getSubRootResources(_folder, () => {
            if (callback) {
                callback();
				logger.debug(`<<< Exiting getChildFolders(folder=${folder},callback=${callback})`);
            }
        }));
    }
    getResxEntries(resxFile, callback) {
		logger.debug(`>>> Entering getResxEntries(resxFile=${resxFile},callback=${callback})`);
        const { props } = this;
        props.dispatch(LanguageEditorActions.getResxEntries({
            resourceFile: resxFile.replace("_/", ""),
            resourceFileActual: resxFile,
            mode: this.state.selectedMode,
            portalId: props.portalId,
            locale: props.languageBeingEdited.Code
        }, callback));
		logger.debug(`<<< Exiting getResxEntries(resxFile=${resxFile},callback=${callback})`);
    }
    onSelectMode(value) {
		logger.debug(`>>> Entering onSelectMode(value=${value})`);
        this.setState({
            selectedMode: value
        }, () => {
            if (this.props.resxBeingEdited !== "") {
                this.getResxEntries(this.props.resxBeingEdited);
				logger.debug(`<<< Exiting onSelectMode(value=${value})`);
            }
        });
    }
    onResxChange(updatedList) {
		logger.debug(`>>> Entering onResxChange(updatedList=${updatedList})`);
        this.props.dispatch(LanguageEditorActions.updateResxEntry(updatedList));
		logger.debug(`<<< Exiting onResxChange(updatedList=${updatedList})`);
    }
    onSaveTranslations() {
		logger.debug('>>> Entering onSaveTranslations()');
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
			logger.debug('<<< Exiting onSaveTranslations()');
        }));
    }
    onHighlightPendingTranslations() {
		logger.debug('>>> Entering onHighlightPendingTranslations()');
        this.setState({
            highlightPendingTranslations: !this.state.highlightPendingTranslations
        });
		logger.debug('<<< Exiting onHighlightPendingTranslations()');
    }
     
    render() {
		logger.debug('>>> Entering render()');
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
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
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
