import React, { Component } from "react";
import PropTypes from "prop-types";
import FolderPicker from "./FolderPicker";
import FilePicker from "./FilePicker";
import helper from "../helper";

import "./style.less";
const logger = console;
const KEY = {
    ENTER: 13,
    ESCAPE: 27
};

function findKey(thisObject, id) {
	logger.error(`>>> Entering findKey(thisObject=${thisObject},id=${id})`, { "event.action": "findKey", "logrite.thisObject": thisObject, "logrite.id": id });
    let p, tRet;
    for (p in thisObject) {
        if (p === "data") {
            if (thisObject[p].key === id) {
                return thisObject;
            }
        } else if (thisObject[p] instanceof Object) {
            if (Object.prototype.hasOwnProperty.call(thisObject, p)) {
                tRet = findKey(thisObject[p], id);
                if (tRet) { return tRet; }
            }
        }
    }
	logger.warn(`<<< Exiting findKey(thisObject=${thisObject},id=${id})`, { "event.action": "findKey", "logrite.thisObject": thisObject, "logrite.id": id });
    return false;
}

export default class Browse extends Component {

    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        let selectedFolder = this.props.selectedFolder;
        this.state = {
            showFolderPicker: false,
            showFilePicker: false,
            folders: null,
            files: null,
            selectedFolder,
            selectedFile: this.props.selectedFile
        };
        this.onKeyDown = this.onKeyDown.bind(this);
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    onKeyDown(event) {
        switch (event.keyCode) {
            case KEY.ENTER:
                return this.props.onSave(this.state.selectedFolder, this.state.selectedFile);
            case KEY.ESCAPE:
                return this.props.onCancel();
        }
    }

    onSave() {
		logger.error('>>> Entering onSave()', { "event.action": "onSave" });
		logger.warn('<<< Exiting onSave()', { "event.action": "onSave" });
        return this.props.onSave(this.state.selectedFolder, this.state.selectedFile);
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        if (this.props.portalId !== prevProps.portalId) {
            this.getFolders();
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("keyup", this.onKeyDown, false);
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.getFolders();
        document.addEventListener("keyup", this.onKeyDown, false);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    getServiceFramework() {
		logger.error('>>> Entering getServiceFramework()', { "event.action": "getServiceFramework" });
        let sf = this.props.utils.utilities.sf;
        sf.controller = "ItemListService";
		logger.debug(`getServiceFramework(): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework" });
        sf.moduleRoot = "InternalServices";
		logger.debug(`getServiceFramework(): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework" });
		logger.warn('<<< Exiting getServiceFramework()', { "event.action": "getServiceFramework" });
        return sf;
    }

    getFolders(searchText) {
		logger.error(`>>> Entering getFolders(searchText=${searchText})`, { "event.action": "getFolders", "logrite.searchText": searchText });
        const sf = this.getServiceFramework();
        let portalIdParams = this.props.portalId && this.props.portalId !== -1
            ? { portalId: this.props.portalId }
            : {};
                    
					logger.error(`getFolders(searchText=${searchText}): sf → ${sf}`, { "event.action": "getFolders", "logrite.searchText": searchText });
        if (!searchText) {
			logger.warn(`<<< Exiting getFolders(searchText=${searchText})`, { "event.action": "getFolders", "logrite.searchText": searchText });
            return sf.get("GetFolders", portalIdParams, this.setFolders.bind(this), this.handleError.bind(this));
        }
        sf.get("SearchFolders", { searchText }, this.setFolders.bind(this), this.handleError.bind(this));
    }

    getChildrenFolders(parentId) {
		logger.error(`>>> Entering getChildrenFolders(parentId=${parentId})`, { "event.action": "getChildrenFolders", "logrite.parentId": parentId });
        const sf = this.getServiceFramework();
        sf.get("GetFolderDescendants", { parentId }, this.addChildFolders.bind(this, parentId), this.handleError.bind(this));
		logger.error(`getChildrenFolders(parentId=${parentId}): sf → ${sf}`, { "event.action": "getChildrenFolders", "logrite.parentId": parentId });
    }

    getExtensions(fileFormats) {
		logger.error(`>>> Entering getExtensions(fileFormats=${fileFormats})`, { "event.action": "getExtensions", "logrite.fileFormats": fileFormats });
        let result = "";
        
		logger.error(`getExtensions(fileFormats=${fileFormats}): result → ${result}`, { "event.action": "getExtensions", "logrite.fileFormats": fileFormats });
        const extensions = {
            "text/html": "htm,html",
            "text/css": "css",
            "text/xml": "xml",
            "image/gif": "gif",
            "image/jpeg": "jpeg,jpg",
            "application/x-javascript": "js",
            "text/plain": "txt",
            "image/png": "png",
            "image/tiff": "tif,tiff",
            "image/x-icon": "ico",
            "image/x-ms-bmp": "bmp",
            "image/svg+xml": "svg",
            "image/webp": "webp",
            "application/msword": "doc",
            "application/pdf": "pdf",
            "application/rtf": "rtf",
            "application/vnd.ms-excel": "xls",
            "application.vnd.ms-powerpoint": "ppt",
            "application/x-shockwave-flash": "swf",
            "appliation/zip": "zip",
            "audio/mpeg": "mp3",
            "audio/ogg": "ogg",
            "video/3gpp": "3gpp,3gp",
            "video/mpeg": "mpeg,mpg",
            "video/quicktime": "mov",
            "video/x-flv": "flv",
            "video/x-ms-wmv": "wmv",
            "video/x-msvideo": "avi",
            "video/mp4": "m4v,mp4",
        };

        fileFormats.map(fileFormat => {
            if (extensions[fileFormat]) {
                result += extensions[fileFormat] + ",";
            }
            else {
                result += fileFormat.split("/")[1] + ",";
            }
        });
		logger.warn(`<<< Exiting getExtensions(fileFormats=${fileFormats})`, { "event.action": "getExtensions", "logrite.fileFormats": fileFormats });
        return result;
    }

    getFiles() {
		logger.error('>>> Entering getFiles()', { "event.action": "getFiles" });
        const sf = this.getServiceFramework();
        let parentId = this.state.selectedFolder ? this.state.selectedFolder.key : null;
		logger.error(`getFiles(): sf → ${sf}`, { "event.action": "getFiles" });
        const extensions = this.getExtensions(this.props.fileFormats);
        if (parentId) {
            sf.get("GetFiles", { parentId, filter: extensions }, this.setFiles.bind(this), this.handleError.bind(this));
        } else if (this.state.selectedFolder) {
            sf.get("SearchFolders", { searchText: this.state.selectedFolder.value }, this.setFolderId.bind(this), this.handleError.bind(this));
        } else {
            this.setFiles();
			logger.warn('<<< Exiting getFiles()', { "event.action": "getFiles" });
        }
    }

    setFolderId(result) {
		logger.error(`>>> Entering setFolderId(result=${result})`, { "event.action": "setFolderId", "logrite.result": result });
        const selectedFolder = result.Tree.children[0].data;
        const extensions = this.getExtensions(this.props.fileFormats);
		logger.error(`setFolderId(result=${result}): extensions → ${extensions}`, { "event.action": "setFolderId", "logrite.result": result });
        const sf = this.getServiceFramework();
        sf.get("GetFiles", { parentId: selectedFolder.key, filter: extensions }, this.setFiles.bind(this), this.handleError.bind(this));
		logger.error(`setFolderId(result=${result}): sf → ${sf}`, { "event.action": "setFolderId", "logrite.result": result });
    }

    setFiles(result) {
		logger.error(`>>> Entering setFiles(result=${result})`, { "event.action": "setFiles", "logrite.result": result });
        if (!result || !result.Tree || !result.Tree.children) {
            return;
        }
        this.setState({ files: result.Tree.children });
		logger.warn(`<<< Exiting setFiles(result=${result})`, { "event.action": "setFiles", "logrite.result": result });
    }

    setFolders(result) {
		logger.error(`>>> Entering setFolders(result=${result})`, { "event.action": "setFolders", "logrite.result": result });
        this.setState({ folders: result.Tree });
        if (this.state.selectedFile && !this.state.selectedFolder || this.state.selectedFolder && this.state.selectedFolder.value === "0") {
            const selectedFolder = result.Tree.children[0].data;
            this.setState({ selectedFolder });
			logger.warn(`<<< Exiting setFolders(result=${result})`, { "event.action": "setFolders", "logrite.result": result });
        }
    }

    handleError() {
	logger.error('>>> Entering handleError()', { "event.action": "handleError" });

	logger.warn('<<< Exiting handleError()', { "event.action": "handleError" });
    }

    onFolderClick(folder) {
		logger.error(`>>> Entering onFolderClick(folder=${folder})`, { "event.action": "onFolderClick", "logrite.folder": folder });
        this.setState({ selectedFolder: folder, files: null, selectedFile: null });
		logger.warn(`<<< Exiting onFolderClick(folder=${folder})`, { "event.action": "onFolderClick", "logrite.folder": folder });
    }

    onFileClick(file) {
		logger.error(`>>> Entering onFileClick(file=${file})`, { "event.action": "onFileClick", "logrite.file": file });
        const selectedFile = file ? file.data : null;
        this.setState({ selectedFile });
		logger.warn(`<<< Exiting onFileClick(file=${file})`, { "event.action": "onFileClick", "logrite.file": file });
    }

    addChildFolders(parentId, result) {
		logger.error(`>>> Entering addChildFolders(parentId=${parentId},result=${result})`, { "event.action": "addChildFolders", "logrite.parentId": parentId, "logrite.result": result });
        let folders = this.state.folders;
        let parent = findKey(folders, parentId);
		logger.error(`addChildFolders(parentId=${parentId},result=${result}): parent → ${parent}`, { "event.action": "addChildFolders", "logrite.parentId": parentId, "logrite.result": result });
		logger.debug(`addChildFolders(parentId=${parentId},result=${result}): parent.children → ${parent.children}`, { "event.action": "addChildFolders", "logrite.parentId": parentId, "logrite.result": result });
        let children = result.Items.map((item) => {
            return { data: item, children: [] };
        });
        parent.children = children;
        this.setState({ folders });
		logger.error(`addChildFolders(parentId=${parentId},result=${result}): children → ${children}`, { "event.action": "addChildFolders", "logrite.parentId": parentId, "logrite.result": result });
    }

    renderActions() {
		logger.error('>>> Entering renderActions()', { "event.action": "renderActions" });
        const {props} = this;

		logger.warn('<<< Exiting renderActions()', { "event.action": "renderActions" });
        return helper.renderActions(props.browseActionText, {
            "save": this.onSave.bind(this),
            "cancel": props.onCancel
        });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
         
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="file-upload-container">
            <h4>{this.props.folderText}</h4>
            <FolderPicker
                notSpecifiedText={this.props.notSpecifiedText}
                searchFoldersPlaceHolderText={this.props.searchFoldersPlaceHolderText}
                selectedFolder={this.state.selectedFolder}
                folders={this.state.folders}
                searchFolder={this.getFolders.bind(this)}
                onFolderClick={this.onFolderClick.bind(this) }
                getChildren={this.getChildrenFolders.bind(this) }
            />
            <h4>{this.props.fileText}</h4>
            <FilePicker
                notSpecifiedText={this.props.notSpecifiedText}
                searchFilesPlaceHolderText={this.props.searchFilesPlaceHolderText}
                selectedFile={this.state.selectedFile}
                files={this.state.files}
                onFileClick={this.onFileClick.bind(this) }
                getFiles={this.getFiles.bind(this) }                
            />
            <span>{this.renderActions()}</span>
        </div>;
    }
}


Browse.propTypes = {    
    utils: PropTypes.object.isRequired,
    selectedFile: PropTypes.object.isRequired,
    selectedFolder: PropTypes.object.isRequired,
    fileFormats: PropTypes.array,
    onSave: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,

    portalId: PropTypes.number,
    browseActionText: PropTypes.string,
    fileText: PropTypes.string,
    folderText: PropTypes.string,
    notSpecifiedText: PropTypes.string,
    searchFoldersPlaceHolderText: PropTypes.string,
    searchFilesPlaceHolderText: PropTypes.string
};

Browse.defaultProps = {
    fileFormats: []
};

