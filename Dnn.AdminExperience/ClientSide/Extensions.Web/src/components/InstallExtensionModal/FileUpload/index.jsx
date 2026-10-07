import React, { Component } from "react";
import PropTypes from "prop-types";
import UploadBar from "./UploadBar";
import AlreadyInstalled from "./AlreadyInstalled";
import Localization from "localization";
import LogDisplay from "./LogDisplay";
import { RadioButtons } from "@dnnsoftware/dnn-react-common";
import UploadIcon from "./img/upload.svg";
import "./style.less";
const logger = console;

export default class FileUpload extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        this.state = {
            text: Localization.get("InstallExtension_FileUploadDefault"),


            draggedOver: false,
            isDragging: false,

            fileName: "",

            uploading: false,
            uploadComplete: false,

            errorText: "",
            errorInPackage: false,

            noManifest: false
        };
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        window.addEventListener("dragover", this.prevent);
        window.addEventListener("drop", this.prevent);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    prevent(e) {
		logger.error(`>>> Entering prevent(e=${e})`, { "event.action": "prevent", "logrite.e": e });
        e.preventDefault();
		logger.warn(`<<< Exiting prevent(e=${e})`, { "event.action": "prevent", "logrite.e": e });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        window.removeEventListener("dragover", this.prevent);
        window.removeEventListener("drop", this.prevent);
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    onMouseEnter(text) {
		logger.error(`>>> Entering onMouseEnter(text=${text})`, { "event.action": "onMouseEnter", "logrite.text": text });
        this.setState({ text });
		logger.warn(`<<< Exiting onMouseEnter(text=${text})`, { "event.action": "onMouseEnter", "logrite.text": text });
    }

    onMouseLeave() {
		logger.error('>>> Entering onMouseLeave()', { "event.action": "onMouseLeave" });
        this.setState({ text: Localization.get("InstallExtension_FileUploadDefault") });
		logger.warn('<<< Exiting onMouseLeave()', { "event.action": "onMouseLeave" });
    }


    onFileUpload(e) {
		logger.error(`>>> Entering onFileUpload(e=${e})`, { "event.action": "onFileUpload", "logrite.e": e });
        this.uploadFile(e.target.files[0]);
		logger.warn(`<<< Exiting onFileUpload(e=${e})`, { "event.action": "onFileUpload", "logrite.e": e });
    }

    getErrorCount(_package) {
		logger.error(`>>> Entering getErrorCount(_package=${_package})`, { "event.action": "getErrorCount", "logrite._package": _package });
        let count = 0;
        _package.forEach(key => {
            if (key.Type === "Failure") {
                count++;
            }
        });
		logger.error(`getErrorCount(_package=${_package}): count → ${count}`, { "event.action": "getErrorCount", "logrite._package": _package });
        return count;
    }

    handleError(error) {
		logger.error(`>>> Entering handleError(error=${error})`, { "event.action": "handleError", "logrite.error": error });
        if (error && error.logs && !error.noManifest) {
            const errorCount = this.getErrorCount(error.logs);
			logger.error(`handleError(error=${error}): errorCount → ${errorCount}`, { "event.action": "handleError", "logrite.error": error });
            const errorText =
                typeof error === "string"
                    ? error
                    : errorCount > 0
                        ? (Localization.get("InstallExtension_UploadFailed") + errorCount + " " + Localization.get("Errors"))
                        : Localization.get("InstallExtension_UploadFailedUnknown");

            this.setState({ uploading: true, errorText, errorInPackage: true, noManifest: false }, () => {
            });
        } else {
            const errorText = Localization.get("InstallExtension_UploadFailedUnknown");
            this.setState({ uploading: true, errorText, errorInPackage: true, noManifest: false });
			logger.error(`handleError(error=${error}): errorText → ${errorText}`, { "event.action": "handleError", "logrite.error": error });
        }
    }

    uploadFile(file) {
		logger.error(`>>> Entering uploadFile(file=${file})`, { "event.action": "uploadFile", "logrite.file": file });
        this.setState({ fileName: file.name, errorInPackage: false });
        this.postFile(file);
		logger.warn(`<<< Exiting uploadFile(file=${file})`, { "event.action": "uploadFile", "logrite.file": file });
    }

    postFile(file) {
		logger.error(`>>> Entering postFile(file=${file})`, { "event.action": "postFile", "logrite.file": file });
        const { props } = this;
        props.parsePackage(file, this.uploadComplete.bind(this), this.handleError.bind(this));

        this.setState({ uploading: true, uploadComplete: false });
		logger.warn(`<<< Exiting postFile(file=${file})`, { "event.action": "postFile", "logrite.file": file });
    }

    cancelRepair() {
		logger.error('>>> Entering cancelRepair()', { "event.action": "cancelRepair" });
        this.props.clearParsedInstallationPackage();
        this.setState({
            uploading: false,
            errorInPackage: false,
            errorText: "",
            noManifest: false
        });
		logger.warn('<<< Exiting cancelRepair()', { "event.action": "cancelRepair" });
    }

    repairInstall() {
		logger.error('>>> Entering repairInstall()', { "event.action": "repairInstall" });
        const { props } = this;
        props.repairInstall();
		logger.warn('<<< Exiting repairInstall()', { "event.action": "repairInstall" });
    }

    uploadComplete(data) {
		logger.error(`>>> Entering uploadComplete(data=${data})`, { "event.action": "uploadComplete", "logrite.data": data });
        if (data.noManifest && !this.props.alreadyInstalled) {
            this.setState({
                uploading: false,
                noManifest: true
            });
        } else {
            setTimeout(() => {
                this.setState({ uploadComplete: true }, () => {
                    if (data.alreadyInstalled) {
                        this.setState({
                            uploading: false,
                            noManifest: false
                        });
                    }
                });
            }, 1000);
			logger.warn(`<<< Exiting uploadComplete(data=${data})`, { "event.action": "uploadComplete", "logrite.data": data });
        }
    }

    onViewLog() {
		logger.error('>>> Entering onViewLog()', { "event.action": "onViewLog" });
        this.props.toggleViewLog(true);
		logger.warn('<<< Exiting onViewLog()', { "event.action": "onViewLog" });
    }

    onDragOver() {
		logger.error('>>> Entering onDragOver()', { "event.action": "onDragOver" });
        this.setState({ draggedOver: true, text: Localization.get("InstallExtension_FileUploadDragOver") });
		logger.warn('<<< Exiting onDragOver()', { "event.action": "onDragOver" });
    }

    onDragLeave() {
		logger.error('>>> Entering onDragLeave()', { "event.action": "onDragLeave" });
        this.setState({ draggedOver: false, text: Localization.get("InstallExtension_FileUploadDefault") });
		logger.warn('<<< Exiting onDragLeave()', { "event.action": "onDragLeave" });
    }

    onDrop(e) {
		logger.error(`>>> Entering onDrop(e=${e})`, { "event.action": "onDrop", "logrite.e": e });
        e.preventDefault();
        this.uploadFile(e.dataTransfer.files[0]);
        this.onDragLeave();
		logger.warn(`<<< Exiting onDrop(e=${e})`, { "event.action": "onDrop", "logrite.e": e });
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const buttonsStyle = { width: 67 };
        let className = "overlay" + (this.state.draggedOver ? " hover" : "");


		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className={"dnn-package-upload" + (this.state.uploading ? " uploading" : "") + (this.props.alreadyInstalled ? " already-installed" : "") + (this.props.viewingLog ? " viewing-log" : "")}>

            {((!this.state.uploading || this.state.uploadComplete) && !this.props.viewingLog) && <div className="dropzone-container">
                <div
                    id="dropzoneId"
                    className={className}
                    onDragOver={this.onDragOver.bind(this)}
                    onDragLeave={this.onDragLeave.bind(this)}
                    onDrop={this.onDrop.bind(this)}>
                    <div className="buttons" style={buttonsStyle}>
                        <div
                            className="button upload"
                            onMouseEnter={this.onMouseEnter.bind(this, Localization.get("InstallExtension_UploadAFile"))}
                            onMouseLeave={this.onMouseLeave.bind(this)}>
                            <div><UploadIcon /></div>
                            <input type="file" onChange={this.onFileUpload.bind(this)} aria-label="File" />
                        </div>
                    </div>
                    <span>{this.state.text}</span>
                </div>
            </div>
            }
            {this.props.viewingLog &&
                <LogDisplay logs={this.props.parsedInstallationPackage && this.props.parsedInstallationPackage.logs} />}
            {(this.state.uploading && !this.props.viewingLog) &&
                <UploadBar
                    uploadComplete={this.state.uploadComplete}
                    errorText={this.state.errorText}
                    errorInPackage={this.state.errorInPackage}
                    fileName={this.state.fileName}
                    onViewLog={this.onViewLog.bind(this)}
                    onTryAgain={this.cancelRepair.bind(this)}
                    viewLogText={Localization.get("ViewErrorLog")}
                    tryAgainText={Localization.get("TryAgain")}
                    uploadCompleteText={Localization.get("InstallExtension_UploadComplete")}
                    uploadingText={Localization.get("InstallExtension_Uploading")} />
            }
            {
                this.state.noManifest &&
                <div className="no-valid-manifest" style={{ width: "calc(100% + 26px)", height: "calc(100% + 26px)" }}>
                    <p>{Localization.get("InvalidDNNManifest")}</p>
                    <RadioButtons options={[
                        {
                            label: Localization.get("CatalogSkin"),
                            value: "Skin"
                        }, {
                            label: Localization.get("Container"),
                            value: "Container"
                        }
                    ]}
                    buttonGroup="selectedLegacyType"
                    onChange={this.props.onSelectLegacyType.bind(this)}
                    value={this.props.selectedLegacyType} />
                </div>
            }
            {this.props.alreadyInstalled &&
                <AlreadyInstalled
                    fileName={this.state.fileName}
                    cancelRepair={this.cancelRepair.bind(this)}
                    repairInstall={this.repairInstall.bind(this)} />
            }
        </div>;
    }
}


FileUpload.propTypes = {
    cancelInstall: PropTypes.func,
    parsedInstallationPackage: PropTypes.object,
    viewingLog: PropTypes.bool,
    toggleViewLog: PropTypes.func,
    clearParsedInstallationPackage: PropTypes.func,
    onSelectLegacyType: PropTypes.func,
    selectedLegacyType: PropTypes.string,
    alreadyInstalled: PropTypes.bool,
    //---OPTIONAL PROPS---
    buttons: PropTypes.array
};