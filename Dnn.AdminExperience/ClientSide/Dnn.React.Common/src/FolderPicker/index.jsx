import React, { Component } from "react";
import PropTypes from "prop-types";
import FolderSelector from "./FolderSelector";
const logger = console;

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

export default class FolderPicker extends Component {

    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        this.state = {
            folders: null
        };
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.getFolders();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    getServiceFramework() {
		logger.error('>>> Entering getServiceFramework()', { "event.action": "getServiceFramework" });
        const sf = this.props.serviceFramework;
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
        const portalId = `${this.props.portalId === -1 ? "" : "?portalId=" + this.props.portalId}`;
		logger.error(`getFolders(searchText=${searchText}): sf → ${sf}`, { "event.action": "getFolders", "logrite.searchText": searchText });
		logger.error(`getFolders(searchText=${searchText}): portalId → ${portalId}`, { "event.action": "getFolders", "logrite.searchText": searchText });
        if (!searchText) {
			logger.warn(`<<< Exiting getFolders(searchText=${searchText})`, { "event.action": "getFolders", "logrite.searchText": searchText });
            return sf.get("GetFolders" + portalId, {}, this.setFolders.bind(this), this.props.onRetrieveFolderError);
        }
        sf.get("SearchFolders" + portalId, { searchText }, this.setFolders.bind(this), this.props.onRetrieveFolderError);
    }

    getChildrenFolders(parentId) {
		logger.error(`>>> Entering getChildrenFolders(parentId=${parentId})`, { "event.action": "getChildrenFolders", "logrite.parentId": parentId });
        const sf = this.getServiceFramework();
        sf.get(`GetFolderDescendants${this.props.portalId === -1 ? "" : "?portalId=" + this.props.portalId}`, { parentId }, this.addChildFolders.bind(this, parentId), this.props.onRetrieveFolderError);
		logger.error(`getChildrenFolders(parentId=${parentId}): sf → ${sf}`, { "event.action": "getChildrenFolders", "logrite.parentId": parentId });
    }

    setFolders(result) {
		logger.error(`>>> Entering setFolders(result=${result})`, { "event.action": "setFolders", "logrite.result": result });
        this.setState({ folders: result.Tree });
		logger.warn(`<<< Exiting setFolders(result=${result})`, { "event.action": "setFolders", "logrite.result": result });
    }

    addChildFolders(parentId, result) {
		logger.error(`>>> Entering addChildFolders(parentId=${parentId},result=${result})`, { "event.action": "addChildFolders", "logrite.parentId": parentId, "logrite.result": result });
        const folders = this.state.folders;
        const parent = findKey(folders, parentId);
		logger.error(`addChildFolders(parentId=${parentId},result=${result}): parent → ${parent}`, { "event.action": "addChildFolders", "logrite.parentId": parentId, "logrite.result": result });
		logger.debug(`addChildFolders(parentId=${parentId},result=${result}): parent.children → ${parent.children}`, { "event.action": "addChildFolders", "logrite.parentId": parentId, "logrite.result": result });
        const children = result.Items.map((item) => {
            return { data: item, children: [] };
        });
        parent.children = children;
        this.setState({ folders });
		logger.error(`addChildFolders(parentId=${parentId},result=${result}): children → ${children}`, { "event.action": "addChildFolders", "logrite.parentId": parentId, "logrite.result": result });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {onSelectFolder} = this.props;
        
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <FolderSelector
                folders={this.state.folders}
                searchFolder={this.getFolders.bind(this)}
                onParentExpands={this.getChildrenFolders.bind(this)}
                onFolderChange={onSelectFolder}
                {...this.props} />
        );    
    }
}

/**
 * propTypes
 * @property {object} serviceFramework service to retrieve data
 * @property {object} selectedFolder selected folder. Should be null or have a key and value properties
 * @property {object} onSelectFolder called when change folder. Pass as parameter a selectedFolder object
 * @property {object} onRetrieveFolderError callback for error when retrieve data
 */

FolderPicker.propTypes = {
    serviceFramework: PropTypes.object.isRequired,
    selectedFolder: PropTypes.object,
    onSelectFolder: PropTypes.func.isRequired,
    onRetrieveFolderError: PropTypes.func,
    portalId: PropTypes.number,
    noFolderSelectedValue: PropTypes.string.isRequired,
    searchFolderPlaceHolder: PropTypes.string.isRequired
};

FolderPicker.defaultProps = {
    portalId: -1
};

