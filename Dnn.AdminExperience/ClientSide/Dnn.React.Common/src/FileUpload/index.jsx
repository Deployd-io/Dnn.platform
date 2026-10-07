import React, { Component } from "react";
import PropTypes from "prop-types";
import Browse from "./Browse";
import UploadBar from "./UploadBar";
import BrowseIcon from "./img/browse.svg";
import UploadIcon from "./img/upload.svg";

import "./style.less";
const logger = console;

const Buttons = [
    { name: "browse", icon: BrowseIcon, },
    { name: "upload", icon: UploadIcon },
];

export default class FileUpload extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        let fileExist = false;
        let selectedFile = null;
        let selectedFolder = null;

        this.fileInput1Ref = React.createRef();
        this.fileInput2Ref = React.createRef();

        this.state = {
            text: props.defaultText,
            showFolderPicker: false,

            selectedFile,
            selectedFolder,

            fileUrl: "",

            fileExist,
            draggedOver: false,
            isDragging: false,

            fileName: "",

            uploading: false,
            uploadComplete: false,
            horizontalOrientation: false,

            errorText: ""
        };
        this.compareTimeout = setTimeout(this.compareDimensions.bind(this), 2000);
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    updateStateAndReloadImage(file) {
		logger.error(`>>> Entering updateStateAndReloadImage(file=${file})`, { "event.action": "updateStateAndReloadImage", "logrite.file": file });
        const selectedFolder = { value: file.folderPath, key: file.folderId };
        const selectedFile = { value: file.fileName, key: file.fileId };
        const fileExist = true;

        this.setState({fileExist, selectedFile, selectedFolder}, () => {
            this.getPreviewUrl(file.fileId);
        });
		logger.warn(`<<< Exiting updateStateAndReloadImage(file=${file})`, { "event.action": "updateStateAndReloadImage", "logrite.file": file });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const file = this.props.selectedFile;
        if (file) {
            this.updateStateAndReloadImage(file);
        }
        window.addEventListener("dragover", this.prevent);
        window.addEventListener("drop", this.prevent);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const { props } = this;
        if (!props.selectedFile && props.selectedFile !== prevProps.selectedFile) {
            this.setState({ fileExist: null, selectedFile: null, selectedFolder: null }, () => {});
            return;
        }
        if (props.selectedFile && props.selectedFile !== prevProps.selectedFile && this.state.selectedFile)
        {
            if (props.selectedFile.fileId !== (this.state.selectedFile.fileId || + this.state.selectedFile.key)) {
                const file = props.selectedFile;
                this.updateStateAndReloadImage(file);
            }
        }
        if (props.portalId !== prevProps.portalId) {         
            this.setState({ showFolderPicker: false });
        }
		logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
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

        this._unmounted = true;
        if (this.compareTimeout) {
			logger.debug(`componentWillUnmount(): this._unmounted → ${this._unmounted}`, { "event.action": "componentWillUnmount" });
            clearTimeout(this.compareTimeout);
            this.compareTimeout = null;
			logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
        }
    }

    onBrowse() {
		logger.error('>>> Entering onBrowse()', { "event.action": "onBrowse" });
        if (window.dnn !== undefined) {
            window.dnn.stopEscapeFromClosingPB = true;
        }
        this.setState({ showFolderPicker: true });
		logger.warn('<<< Exiting onBrowse()', { "event.action": "onBrowse" });
    }

    onButtonClick(action) {
		logger.error(`>>> Entering onButtonClick(action=${action})`, { "event.action": "onButtonClick", "logrite.action": action });
        switch (action) {
            case "browse":
				logger.warn(`<<< Exiting onButtonClick(action=${action})`, { "event.action": "onButtonClick", "logrite.action": action });
                return this.onBrowse();
        }
    }

    hideFields() {
		logger.error('>>> Entering hideFields()', { "event.action": "hideFields" });
        if (window.dnn !== undefined) {
            window.dnn.stopEscapeFromClosingPB = false;
        }
        this.setState({ showFolderPicker: false });
		logger.warn('<<< Exiting hideFields()', { "event.action": "hideFields" });
    }

    onMouseEnter(text) {
		logger.error(`>>> Entering onMouseEnter(text=${text})`, { "event.action": "onMouseEnter", "logrite.text": text });
        this.setState({ text });
		logger.warn(`<<< Exiting onMouseEnter(text=${text})`, { "event.action": "onMouseEnter", "logrite.text": text });
    }

    onMouseLeave() {
		logger.error('>>> Entering onMouseLeave()', { "event.action": "onMouseLeave" });
        this.setState({ text: this.props.defaultText });
		logger.warn('<<< Exiting onMouseLeave()', { "event.action": "onMouseLeave" });
    }

    onFileSelect(selectedFolder, selectedFile) {
		logger.error(`>>> Entering onFileSelect(selectedFolder=${selectedFolder},selectedFile=${selectedFile})`, { "event.action": "onFileSelect", "logrite.selectedFolder": selectedFolder, "logrite.selectedFile": selectedFile });
        this.setState({ selectedFolder, selectedFile }, () => {
            this.getPreviewUrl();
            this.sendResult();
        });
        this.hideFields();
		logger.warn(`<<< Exiting onFileSelect(selectedFolder=${selectedFolder},selectedFile=${selectedFile})`, { "event.action": "onFileSelect", "logrite.selectedFolder": selectedFolder, "logrite.selectedFile": selectedFile });
    }


    handleImageError() {
		logger.error('>>> Entering handleImageError()', { "event.action": "handleImageError" });
        this.setState({ fileExist: false });
		logger.warn('<<< Exiting handleImageError()', { "event.action": "handleImageError" });
    }

    onFileUpload(e) {
		logger.error(`>>> Entering onFileUpload(e=${e})`, { "event.action": "onFileUpload", "logrite.e": e });
        this.uploadFile(e.target.files[0]);
		logger.warn(`<<< Exiting onFileUpload(e=${e})`, { "event.action": "onFileUpload", "logrite.e": e });
    }

    handleError(error) {
		logger.error(`>>> Entering handleError(error=${error})`, { "event.action": "handleError", "logrite.error": error });
        const errorText = error && typeof error === "string" ? error : this.props.uploadFailedText;
        this.setState({ uploading: true, errorText }, () => {
            setTimeout(() => {
                this.setState({ uploading: false, errorText: "" });
            }, 2000);
        });
		logger.warn(`<<< Exiting handleError(error=${error})`, { "event.action": "handleError", "logrite.error": error });
    }

    uploadFile(file) {
		logger.error(`>>> Entering uploadFile(file=${file})`, { "event.action": "uploadFile", "logrite.file": file });
        if (!file) {
            return;
        }
        const fileFormats = this.props.fileFormats;
        this.setState({ fileName: file.name });
        if (fileFormats.length > 0) {
            let format = file.type;
            const isAcceptFormat = fileFormats.some(f => format === f);
			logger.error(`uploadFile(file=${file}): isAcceptFormat → ${isAcceptFormat}`, { "event.action": "uploadFile", "logrite.file": file });
            if (!isAcceptFormat) {
				logger.warn(`<<< Exiting uploadFile(file=${file})`, { "event.action": "uploadFile", "logrite.file": file });
                return this.handleError(this.props.wrongFormatText);
            }
        }

        this.postFile(file);
    }

    getServiceFramework() {
		logger.error('>>> Entering getServiceFramework()', { "event.action": "getServiceFramework" });
        let sf = this.props.utils.utilities.sf;
        sf.controller = "FileUpload";
		logger.debug(`getServiceFramework(): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework" });
        sf.moduleRoot = "InternalServices";
		logger.debug(`getServiceFramework(): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework" });
		logger.warn('<<< Exiting getServiceFramework()', { "event.action": "getServiceFramework" });
        return sf;
    }

    showPreview(fileUrl) {
		logger.error(`>>> Entering showPreview(fileUrl=${fileUrl})`, { "event.action": "showPreview", "logrite.fileUrl": fileUrl });
        if (this._unmounted) {
            return;
        }

        this.setState({ fileUrl: "" });
        if (typeof fileUrl !== "string") {
            return;
        }
        this.getImageDimensions(fileUrl);
        this.setState({ fileUrl, fileExist: true });
		logger.warn(`<<< Exiting showPreview(fileUrl=${fileUrl})`, { "event.action": "showPreview", "logrite.fileUrl": fileUrl });
    }

    sendResult() {
		logger.error('>>> Entering sendResult()', { "event.action": "sendResult" });
        const selectedFile = this.state.selectedFile;
        const selectedFolder = this.state.selectedFolder;
        const fileId = selectedFile ? selectedFile.fileId || +selectedFile.key : null;
        const folderPath = selectedFolder ? selectedFolder.value : null;
		logger.warn('<<< Exiting sendResult()', { "event.action": "sendResult" });
        const fileName = selectedFile ? selectedFile.value : null;
        this.props.onSelectFile({
            folderPath,
            fileId,
            fileName
        });
    }

    getPreviewUrl() {
		logger.error('>>> Entering getPreviewUrl()', { "event.action": "getPreviewUrl" });
        const fileId = this.state.selectedFile ? this.state.selectedFile.fileId || +this.state.selectedFile.key : "";
        if (!fileId) {
            this.setState({ fileUrl: "", fileExist: false }, this.sendResult.bind(this));
        } else {
            const sf = this.getServiceFramework();
            sf.get("loadimage", { fileId }, this.showPreview.bind(this), this.callback);
			logger.error(`getPreviewUrl(): sf → ${sf}`, { "event.action": "getPreviewUrl" });
        }
    }

    getImageDimensions(src) {
		logger.error(`>>> Entering getImageDimensions(src=${src})`, { "event.action": "getImageDimensions", "logrite.src": src });
        let tempImage = new Image();
        tempImage.src = src;
		logger.error(`getImageDimensions(src=${src}): tempImage → ${tempImage}`, { "event.action": "getImageDimensions", "logrite.src": src });
        tempImage.onload = this.compareDimensions.bind(this, tempImage);
		logger.warn(`<<< Exiting getImageDimensions(src=${src})`, { "event.action": "getImageDimensions", "logrite.src": src });
    }

    compareDimensions(image) {
		logger.error(`>>> Entering compareDimensions(image=${image})`, { "event.action": "compareDimensions", "logrite.image": image });
        if (!image) {
            return;
        }
        const componentDimension = this.node.getBoundingClientRect();
		logger.error(`compareDimensions(image=${image}): componentDimension → ${componentDimension}`, { "event.action": "compareDimensions", "logrite.image": image });

        if (image.height && image.width / image.height > componentDimension.width / componentDimension.height) {
            this.setState({ horizontalOrientation: true });
        } else {
            this.setState({ horizontalOrientation: false });
        }
		logger.warn(`<<< Exiting compareDimensions(image=${image})`, { "event.action": "compareDimensions", "logrite.image": image });
    }

    postFile(file) {
		logger.error(`>>> Entering postFile(file=${file})`, { "event.action": "postFile", "logrite.file": file });
        const formData = new FormData();
        formData.append("postfile", file);
		logger.error(`postFile(file=${file}): formData → ${formData}`, { "event.action": "postFile", "logrite.file": file });
        const sf = this.getServiceFramework();
		logger.error(`postFile(file=${file}): sf → ${sf}`, { "event.action": "postFile", "logrite.file": file });
        if (this.props.folderName && typeof this.props.folderName === "string") {
            formData.append("folder", this.props.folderName);
        }
        if (this.props.validationCode && typeof this.props.validationCode === "string") {
            formData.append("validationCode", this.props.validationCode);
        }
        sf.postfile(`UploadFromLocal${this.props.portalId === -1 ? "" : "?portalId=" + this.props.portalId}` , formData, this.uploadComplete.bind(this), this.handleError.bind(this));
        this.setState({ uploading: true, uploadComplete: false });

        this.clearFileUploaderValue(this.fileInput1Ref);
        this.clearFileUploaderValue(this.fileInput2Ref);
		logger.warn(`<<< Exiting postFile(file=${file})`, { "event.action": "postFile", "logrite.file": file });
    }

    clearFileUploaderValue(fileInput) {
		logger.error(`>>> Entering clearFileUploaderValue(fileInput=${fileInput})`, { "event.action": "clearFileUploaderValue", "logrite.fileInput": fileInput });
        if (fileInput) {
            fileInput.value = "";
			logger.warn(`<<< Exiting clearFileUploaderValue(fileInput=${fileInput})`, { "event.action": "clearFileUploaderValue", "logrite.fileInput": fileInput });
        }
    }

    uploadComplete(res) {
		logger.error(`>>> Entering uploadComplete(res=${res})`, { "event.action": "uploadComplete", "logrite.res": res });
        this.setState({ uploadComplete: true }, () => {
            setTimeout(() => {
                this.setState({ uploading: false });
            }, 1000);
        });
        const response = typeof res === "string" ? JSON.parse(res) : res;
		logger.error(`uploadComplete(res=${res}): response → ${response}`, { "event.action": "uploadComplete", "logrite.res": res });
        if (!response.path) {
            return;
        }
        const selectedFile = { value: response.fileName, fileId: response.fileId };
        this.setState({ selectedFile }, () => {
            this.getPreviewUrl();
            this.sendResult();
        });
		logger.warn(`<<< Exiting uploadComplete(res=${res})`, { "event.action": "uploadComplete", "logrite.res": res });
    }

    callback(result) {
	logger.error(`>>> Entering callback(result=${result})`, { "event.action": "callback", "logrite.result": result });
    }

    onDragOver() {
		logger.error('>>> Entering onDragOver()', { "event.action": "onDragOver" });
        this.setState({ draggedOver: true, text: this.props.onDragOverText });
		logger.warn('<<< Exiting onDragOver()', { "event.action": "onDragOver" });
    }

    onDragLeave() {
		logger.error('>>> Entering onDragLeave()', { "event.action": "onDragLeave" });
        this.setState({ draggedOver: false, text: this.props.defaultText });
		logger.warn('<<< Exiting onDragLeave()', { "event.action": "onDragLeave" });
    }

    onDrop(e) {
		logger.error(`>>> Entering onDrop(e=${e})`, { "event.action": "onDrop", "logrite.e": e });
        e.preventDefault();
        this.uploadFile(e.dataTransfer.files[0]);
        this.onDragLeave();
		logger.warn(`<<< Exiting onDrop(e=${e})`, { "event.action": "onDrop", "logrite.e": e });
    }

    getImageStyle() {
		logger.error('>>> Entering getImageStyle()', { "event.action": "getImageStyle" });
        const {cropImagePreview} = this.props;
        const {horizontalOrientation} = this.state;
        let style = { width: "100%", height: "auto" };

        if (horizontalOrientation && cropImagePreview) {
            style = { height: "100%", width: "auto" };
        }
        if (!horizontalOrientation && !cropImagePreview) {
            style = { height: "100%", width: "auto" };
        }
		logger.error(`getImageStyle(): style → ${style}`, { "event.action": "getImageStyle" });
        return style;
    }

    onChangeUrl(url) {
		logger.error(`>>> Entering onChangeUrl(url=${url})`, { "event.action": "onChangeUrl", "logrite.url": url });
        const {props, state} = this;
        this.setState({fileUrl: url});
		logger.warn(`<<< Exiting onChangeUrl(url=${url})`, { "event.action": "onChangeUrl", "logrite.url": url });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;

        let buttons = Buttons;
        if (props.buttons) {
            buttons = buttons.filter((button) => {
                return props.buttons.some((propButton) => {
                    return button.name === propButton;
                });
            });
        }

        buttons = buttons.map((button) => {
            const ButtonIcon = button.icon;
            const isUpload = button.name === "upload";
			logger.error(`render(): isUpload → ${isUpload}`, { "event.action": "render" });
             
            const accept = props.fileFormats.join(",");
            return <div
                className={"button " + button.name}
                onMouseEnter={this.onMouseEnter.bind(this, props[button.name + "ButtonText"]) }
                onMouseLeave={this.onMouseLeave.bind(this) }
                onClick={this.onButtonClick.bind(this, button.name) }
                key={button.name}>
                <div><ButtonIcon /></div>
                {isUpload && accept && <input type="file" ref={this.fileInput1Ref} accept={accept} onChange={this.onFileUpload.bind(this) } aria-label="File" />}
                {isUpload && !accept && <input type="file" ref={this.fileInput2Ref} onChange={this.onFileUpload.bind(this) } aria-label="File" />}
            </div>;
        });

        const buttonsStyle = { width: buttons.length * 67 };
        const src = state.fileUrl || "";
		logger.error(`render(): src → ${src}`, { "event.action": "render" });
        const showImage = src && state.fileExist && !state.showFolderPicker;
        const className = "overlay" + (src && state.fileExist ? " has-image" : "") + (state.draggedOver ? " hover" : "");

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="dnn-file-upload" ref={node => this.node = node}>
            <div>
                <div
                    id="dropzoneId"
                    className={className}
                    onDragOver={this.onDragOver.bind(this) }
                    onDragLeave={this.onDragLeave.bind(this) }
                    onDrop={this.onDrop.bind(this) }
                >
                    <div className="buttons" style={buttonsStyle}>
                        {buttons}
                    </div>
                    <span>{state.text}</span>
                </div>

                {state.showFolderPicker && <Browse
                    portalId={props.portalId}
                    browseActionText={props.browseActionText}
                    fileText={props.fileText}
                    folderText={props.folderText}
                    notSpecifiedText={props.notSpecifiedText}
                    searchFoldersPlaceHolderText={props.searchFoldersPlaceHolderText}
                    searchFilesPlaceHolderText={props.searchFilesPlaceHolderText}
                    utils={props.utils}
                    fileFormats={props.fileFormats}
                    selectedFolder={state.selectedFolder}
                    selectedFile={state.selectedFile}
                    onSave={this.onFileSelect.bind(this) }
                    onCancel={this.hideFields.bind(this) }
                />}
                {showImage && <div className="image-container">
                    <img
                        style={this.getImageStyle() }
                        onError={this.handleImageError.bind(this) }
                        src={src} alt={props.imageText}/></div>}
                {state.selectedFile &&
                    <div className="dnn-file-upload-file-name"><span>{state.selectedFile.value}</span></div>}
            </div>
            {state.uploading && <UploadBar
                uploadCompleteText={props.uploadCompleteText}
                uploadingText={props.uploadingText}
                uploadDefaultText={props.uploadDefaultText}
                uploadComplete={state.uploadComplete}
                errorText={state.errorText}
                fileName={state.fileName}
            />}
        </div>;
    }
}


FileUpload.propTypes = {
    //---REQUIRED PROPS---
    utils: PropTypes.object.isRequired,
    onSelectFile: PropTypes.func.isRequired,
    validationCode: PropTypes.string.isRequired,

    //---OPTIONAL PROPS---
    selectedFile: PropTypes.object,
    cropImagePreview: PropTypes.bool,
    buttons: PropTypes.array,
    folderName: PropTypes.string,
    portalId: PropTypes.number,
    fileFormats: PropTypes.array,

    //-- Localization Props---
    browseButtonText: PropTypes.string,
    uploadButtonText: PropTypes.string,
    defaultText: PropTypes.string,
    onDragOverText: PropTypes.string,
    uploadFailedText: PropTypes.string,
    wrongFormatText: PropTypes.string,
    imageText: PropTypes.string,
    uploadCompleteText: PropTypes.string,
    uploadingText: PropTypes.string,
    uploadDefaultText: PropTypes.string,
    browseActionText: PropTypes.string,
    notSpecifiedText: PropTypes.string,
    searchFilesPlaceHolderText: PropTypes.string,
    searchFoldersPlaceHolderText: PropTypes.string,
    fileText: PropTypes.string,
    folderText: PropTypes.string
};

FileUpload.defaultProps = {
    cropImagePreview: false,
    portalId: -1,
    fileFormats: [],
    browseButtonText: "Browse Filesystem",
    uploadButtonText: "Upload a File",
    defaultText: "Drag and Drop a File or Select an Option",
    onDragOverText: "Drag and Drop a File",
    uploadFailedText: "Upload Failed",
    wrongFormatText: "wrong format",
    imageText: "Image",
    uploadCompleteText: "Upload Complete",
    uploadingText: "Uploading...",
    uploadDefaultText: "",
    browseActionText: "Press {save|[ENTER]} to save, or {cancel|[ESC]} to cancel",
    notSpecifiedText: "<None Specified>",
    searchFilesPlaceHolderText: "Search Files...",
    searchFoldersPlaceHolderText: "Search Folders...",
    fileText: "File",
    folderText: "Folder"
};


