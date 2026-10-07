import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteInfo as SiteInfoActions
} from "../../actions";
import { 
    InputGroup, 
    SingleLineInputWithError, 
    MultiLineInputWithError, 
    GridSystem, 
    Dropdown, 
    FileUpload, 
    Label, 
    Button } from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

let canEdit = false;
class BasicSettingsPanelBody extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this._mounted = false;
		logger.debug(`constructor(): this._mounted → ${this._mounted}`, { "event.action": "constructor" });
        this.state = {
            basicSettings: undefined,
            error: {
                title: false
            },
            triedToSubmit: false
        };
        canEdit = util.settings.isHost || util.settings.isAdmin || util.settings.permissions.SITE_INFO_EDIT;
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }

    getPortalSettings() {
		logger.error('>>> Entering getPortalSettings()', { "event.action": "getPortalSettings" });
        const {props} = this;
        props.dispatch(SiteInfoActions.getPortalSettings(props.portalId, props.cultureCode, (data) => {
            if (this._mounted) {
				logger.warn('<<< Exiting getPortalSettings()', { "event.action": "getPortalSettings" });
                this.setState({
                    basicSettings: Object.assign({}, data.Settings)
                });
            }
        }));
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this._mounted = true;
        this.getPortalSettings();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentDidUpdate() {
		logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
        let { props} = this;
        if (props.basicSettings) {
            let portalIdChanged = props.portalId !== undefined && props.basicSettings.PortalId !== props.portalId;
            let cultureCodeChanged = props.cultureCode !== undefined && props.basicSettings.CultureCode !== props.cultureCode;

            if (portalIdChanged || cultureCodeChanged) {
                this.getPortalSettings();
				logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
            }
        }
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        this._mounted = false;
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    onSettingChange(key, event) {
		logger.error(`>>> Entering onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        let {state, props} = this;
        let basicSettings = Object.assign({}, state.basicSettings);
		logger.error(`onSettingChange(key=${key},event=${event}): basicSettings → ${basicSettings}`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });

        if (key === "LogoFile" || key === "FavIcon") {
            basicSettings[key] = event;
			logger.warn(`<<< Exiting onSettingChange(key=${key},event=${event})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event });
        }
        else if (key === "TimeZone" || key === "IconSets") {
            basicSettings[key] = event.value;
        }
        else {
            basicSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }

        if (basicSettings[key] === "" && key === "PortalName") {
            state.error["title"] = true;
        }
        else if (basicSettings[key] !== "" && key === "PortalName") {
            state.error["title"] = false;
        }

        this.setState({
            basicSettings: basicSettings,
            error: state.error,
            triedToSubmit: false
        });

        props.dispatch(SiteInfoActions.portalSettingsClientModified(basicSettings));
    }

    getTimeZoneOptions() {
		logger.error('>>> Entering getTimeZoneOptions()', { "event.action": "getTimeZoneOptions" });
        let options = [];
        if (this.props.timeZones !== undefined) {
            options = this.props.timeZones.map((item) => {
                return { label: item.DisplayName, value: item.Id };
            });
        }
		logger.warn('<<< Exiting getTimeZoneOptions()', { "event.action": "getTimeZoneOptions" });
        return options;
    }

    getIconSetOptions() {
		logger.error('>>> Entering getIconSetOptions()', { "event.action": "getIconSetOptions" });
        let options = [];
        if (this.props.iconSets !== undefined) {
            options = this.props.iconSets.map((item) => {
                return { label: item, value: item };
            });
        }
		logger.warn('<<< Exiting getIconSetOptions()', { "event.action": "getIconSetOptions" });
        return options;
    }

    onUpdate(event) {
		logger.error(`>>> Entering onUpdate(event=${event})`, { "event.action": "onUpdate", "logrite.event": event });
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });

        if (state.error.title) {
            return;
        }

        props.dispatch(SiteInfoActions.updatePortalSettings(state.basicSettings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
		logger.warn(`<<< Exiting onUpdate(event=${event})`, { "event.action": "onUpdate", "logrite.event": event });
    }

    onCancel() {
		logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {            
            props.dispatch(SiteInfoActions.getPortalSettings(props.portalId, props.cultureCode, (data) => {
				logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
                let basicSettings = Object.assign({}, data.Settings);
                this.setState({
                    basicSettings
                });
            }));
        });
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
        if (state.basicSettings) {
            const columnOne = <div key="column-one" className="left-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plDescription.Help") }
                        label={resx.get("plDescription") }
                    />
                    <MultiLineInputWithError
                        value={state.basicSettings.Description}
                        onChange={this.onSettingChange.bind(this, "Description") }
                        enabled={canEdit}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plTimeZone.Help") }
                        label={resx.get("plTimeZone") }
                    />
                    <Dropdown
                        options={this.getTimeZoneOptions() }
                        value={state.basicSettings.TimeZone}
                        onSelect={this.onSettingChange.bind(this, "TimeZone") }
                        enabled={canEdit}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plFooterText.Help") }
                        label={resx.get("plFooterText") }
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={false}
                        value={state.basicSettings.FooterText}
                        onChange={this.onSettingChange.bind(this, "FooterText") }
                        style={{ width: "100%" }}
                        enabled={canEdit}
                    />
                </InputGroup>
            </div>;
            const columnTwo = <div key="column-two" className="right-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plKeyWords.Help") }
                        label={resx.get("plKeyWords") }
                    />
                    <MultiLineInputWithError
                        value={state.basicSettings.KeyWords}
                        onChange={this.onSettingChange.bind(this, "KeyWords") }
                        enabled={canEdit}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plGUID.Help") }
                        label={resx.get("plGUID") }
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={false}
                        value={state.basicSettings.GUID}
                        style={{ width: "100%" }}
                        enabled={false}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plHomeDirectory.Help") }
                        label={resx.get("plHomeDirectory") }
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={false}
                        value={state.basicSettings.HomeDirectory}
                        style={{ width: "100%" }}
                        enabled={false}
                    />
                </InputGroup>
            </div>;
            const columnThree = <div key="column-three" className="left-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plLogo.Help") }
                        label={resx.get("plLogo") }
                    />
                    <FileUpload
                        utils={util}
                        portalId={props.portalId}
                        selectedFile={state.basicSettings.LogoFile}
                        folderName={state.basicSettings.LogoFile ? state.basicSettings.LogoFile.FolderName : null}
                        validationCode={state.basicSettings.ValidationCode}
                        onSelectFile={this.onSettingChange.bind(this, "LogoFile") }
                        fileFormats={["image/png", "image/jpg", "image/jpeg", "image/bmp", "image/gif", "image/jpeg", "image/svg+xml"]}
                        browseButtonText={resx.get("BrowseButton")}
                        uploadButtonText={resx.get("UploadButton")}
                        linkButtonText={resx.get("LinkButton")}
                        defaultText={resx.get("DragDefault")}
                        onDragOverText={resx.get("DragOver")}
                        uploadFailedText={resx.get("UploadFailed")}
                        wrongFormatText={resx.get("WrongFormat")}
                        imageText={resx.get("DefaultImageTitle")}
                        linkInputTitleText={resx.get("LinkInputTitle")}
                        linkInputPlaceholderText={resx.get("LinkInputPlaceholder")}
                        linkInputActionText={resx.get("LinkInputAction")}
                        uploadCompleteText={resx.get("UploadComplete")}
                        uploadingText={resx.get("Uploading")}
                        uploadDefaultText={resx.get("UploadDefault")}
                        browseActionText={resx.get("BrowseAction")}
                        notSpecifiedText={resx.get("NotSpecified")}
                        searchFilesPlaceHolderText={resx.get("SearchFilesPlaceHolder")}
                        searchFoldersPlaceHolderText={resx.get("SearchFoldersPlaceHolder")}
                        fileText={resx.get("File")}
                        folderText={resx.get("Folder")}
                    />
                </InputGroup>
            </div>;
            const columnFour = <div key="column-four" className="right-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plFavIcon.Help") }
                        label={resx.get("plFavIcon") }
                    />
                    <FileUpload
                        utils={util}
                        portalId={props.portalId}
                        selectedFile={state.basicSettings.FavIcon}
                        folderName={state.basicSettings.FavIcon ? state.basicSettings.FavIcon.FolderName : null}
                        validationCode={state.basicSettings.ValidationCode}
                        onSelectFile={this.onSettingChange.bind(this, "FavIcon") }
                        fileFormats={["image/x-icon", "image/ico"]}
                        browseButtonText={resx.get("BrowseButton")}
                        uploadButtonText={resx.get("UploadButton")}
                        linkButtonText={resx.get("LinkButton")}
                        defaultText={resx.get("DragDefault")}
                        onDragOverText={resx.get("DragOver")}
                        uploadFailedText={resx.get("UploadFailed")}
                        wrongFormatText={resx.get("WrongFormat")}
                        imageText={resx.get("DefaultImageTitle")}
                        linkInputTitleText={resx.get("LinkInputTitle")}
                        linkInputPlaceholderText={resx.get("LinkInputPlaceholder")}
                        linkInputActionText={resx.get("LinkInputAction")}
                        uploadCompleteText={resx.get("UploadComplete")}
                        uploadingText={resx.get("Uploading")}
                        uploadDefaultText={resx.get("UploadDefault")}
                        browseActionText={resx.get("BrowseAction")}
                        notSpecifiedText={resx.get("NotSpecified")}
                        searchFilesPlaceHolderText={resx.get("SearchFilesPlaceHolder")}
                        searchFoldersPlaceHolderText={resx.get("SearchFoldersPlaceHolder")}
                        fileText={resx.get("File")}
                        folderText={resx.get("Folder")}
                    />
                </InputGroup>
            </div>;

            return (
                <div className={styles.basicSettings}>
                    <InputGroup>
                        <Label
                            tooltipMessage={resx.get("plPortalName.Help") }
                            label={resx.get("plPortalName") }
                        />
                        <SingleLineInputWithError
                            inputStyle={{ margin: "0" }}
                            withLabel={false}
                            error={this.state.error.title && this.state.triedToSubmit}
                            errorMessage={resx.get("valPortalName.ErrorMessage") }
                            value={state.basicSettings.PortalName}
                            onChange={this.onSettingChange.bind(this, "PortalName") }
                            style={{ width: "100%" }}
                            enabled={canEdit}
                        />
                    </InputGroup>
                    <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                    <InputGroup>
                        <Label
                            className={"sectionLabel"}
                            label={resx.get("plLogoIcon") }
                        />
                    </InputGroup>
                    {canEdit && <GridSystem numberOfColumns={2}>{[columnThree, columnFour]}</GridSystem>}
                    <InputGroup style={{ paddingTop: "10px" }}>
                        <Label
                            tooltipMessage={resx.get("plIconSet.Help") }
                            label={resx.get("plIconSet") }
                        />
                        <Dropdown
                            options={this.getIconSetOptions() }
                            value={state.basicSettings.IconSet}
                            onSelect={this.onSettingChange.bind(this, "IconSets") }
                            enabled={canEdit}
                        />
                    </InputGroup>
                    {canEdit && <div className="buttons-box">
                        <Button
                            disabled={!this.props.clientModified}
                            type="neutral"
                            onClick={this.onCancel.bind(this) }>
                            {resx.get("Cancel") }
                        </Button>
                        <Button
                            disabled={!this.props.clientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this) }>
                            {resx.get("Save") }
                        </Button>
                    </div>}
                </div>
            );
        }
        else {
			logger.warn('<<< Exiting render()', { "event.action": "render" });
            return <div />;
        }
    }
}

BasicSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    basicSettings: PropTypes.object,
    timeZones: PropTypes.array,
    iconSets: PropTypes.array,
    clientModified: PropTypes.bool,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        tabIndex: state.pagination.tabIndex,
        basicSettings: state.siteInfo.settings,
        timeZones: state.siteInfo.timeZones,
        iconSets: state.siteInfo.iconSets,
        clientModified: state.siteInfo.clientModified,
    };
}

export default connect(mapStateToProps)(BasicSettingsPanelBody);