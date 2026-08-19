import React, { Component } from "react";
import PropTypes from "prop-types";
import FolderSelector from "./FolderSelector";
const logger = console;

function findKey(thisObject, id) {
	logger.debug(`>>> Entering findKey(thisObject=${thisObject},id=${id})`);
     
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
	logger.debug(`<<< Exiting findKey(thisObject=${thisObject},id=${id})`);
    return false;
     
}

export default class FolderPicker extends Component {

    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);

        this.state = {
            folders: null
        };
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.getFolders();
		logger.debug('<<< Exiting componentDidMount()');
    }

    getServiceFramework() {
		logger.debug('>>> Entering getServiceFramework()');
        const sf = this.props.serviceFramework;
        sf.controller = "ItemListService";
		logger.debug(`getServiceFramework(): sf.controller → ${sf.controller}`);
        sf.moduleRoot = "InternalServices";
		logger.debug(`getServiceFramework(): sf.moduleRoot → ${sf.moduleRoot}`);
		logger.debug('<<< Exiting getServiceFramework()');
        return sf;
    }

    getFolders(searchText) {
		logger.debug(`>>> Entering getFolders(searchText=${searchText})`);
        const sf = this.getServiceFramework();
        const portalId = `${this.props.portalId === -1 ? "" : "?portalId=" + this.props.portalId}`;
		logger.debug(`getFolders(searchText=${searchText}): sf → ${sf}`);
		logger.debug(`getFolders(searchText=${searchText}): portalId → ${portalId}`);
        if (!searchText) {
			logger.debug(`<<< Exiting getFolders(searchText=${searchText})`);
            return sf.get("GetFolders" + portalId, {}, this.setFolders.bind(this), this.props.onRetrieveFolderError);
        }
        sf.get("SearchFolders" + portalId, { searchText }, this.setFolders.bind(this), this.props.onRetrieveFolderError);
    }

    getChildrenFolders(parentId) {
		logger.debug(`>>> Entering getChildrenFolders(parentId=${parentId})`);
        const sf = this.getServiceFramework();
        sf.get(`GetFolderDescendants${this.props.portalId === -1 ? "" : "?portalId=" + this.props.portalId}`, { parentId }, this.addChildFolders.bind(this, parentId), this.props.onRetrieveFolderError);
		logger.debug(`getChildrenFolders(parentId=${parentId}): sf → ${sf}`);
		logger.debug(`<<< Exiting getChildrenFolders(parentId=${parentId})`);
    }

    setFolders(result) {
		logger.debug(`>>> Entering setFolders(result=${result})`);
        this.setState({ folders: result.Tree });
		logger.debug(`<<< Exiting setFolders(result=${result})`);
    }

    addChildFolders(parentId, result) {
		logger.debug(`>>> Entering addChildFolders(parentId=${parentId},result=${result})`);
        const folders = this.state.folders;
        const parent = findKey(folders, parentId);
		logger.debug(`addChildFolders(parentId=${parentId},result=${result}): parent → ${parent}`);
        const children = result.Items.map((item) => {
            return { data: item, children: [] };
        });
        parent.children = children;
        this.setState({ folders });
		logger.debug(`addChildFolders(parentId=${parentId},result=${result}): children → ${children}`);
		logger.debug(`<<< Exiting addChildFolders(parentId=${parentId},result=${result})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const {onSelectFolder} = this.props;
        
		logger.debug('<<< Exiting render()');
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

