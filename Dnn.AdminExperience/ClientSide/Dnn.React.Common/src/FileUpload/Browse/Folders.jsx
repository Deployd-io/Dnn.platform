import React, { Component } from "react";
import PropTypes from "prop-types";
import FolderIcon from "./img/folder.svg";
const logger = console;

export default class Folders extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            openFolders: []
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const { props } = this;
        if (!props.folders || !props.folders.children || !props.folders.children[0] || !props.folders.children[0].data || !props.folders.children[0].data.key) {
            return;
        }
        const rootKey = props.folders.children[0].data.key;
        let {openFolders} = this.state;
        openFolders.push(rootKey);
        if (this.state.openFolders !== openFolders) {
            this.setState({ openFolders });
        }
		logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
    }

    toggleFolder(key) {
		logger.error(`>>> Entering toggleFolder(key=${key})`, { "event.action": "toggleFolder", "logrite.key": key });
        let {openFolders} = this.state;
        if (openFolders.some(id => id === key)) {
            openFolders = openFolders.filter(id => id !== key);
        } else {
            openFolders.push(key);
        }
        this.setState({ openFolders });
		logger.warn(`<<< Exiting toggleFolder(key=${key})`, { "event.action": "toggleFolder", "logrite.key": key });
    }

    onParentClick(item) {
		logger.error(`>>> Entering onParentClick(item=${item})`, { "event.action": "onParentClick", "logrite.item": item });
        if (!item.data.hasChildren) {
            return;
        }
        this.toggleFolder(item.data.key);
        if (item.children && item.children.length) {
            return;
        }
        this.props.getChildren(item.data.key);
		logger.warn(`<<< Exiting onParentClick(item=${item})`, { "event.action": "onParentClick", "logrite.item": item });
    }

    onFolderNameClick(folder) {
		logger.error(`>>> Entering onFolderNameClick(folder=${folder})`, { "event.action": "onFolderNameClick", "logrite.folder": folder });
        this.props.onFolderClick(folder.data);
		logger.warn(`<<< Exiting onFolderNameClick(folder=${folder})`, { "event.action": "onFolderNameClick", "logrite.folder": folder });
    }

    getFolders(folder) {
		logger.error(`>>> Entering getFolders(folder=${folder})`, { "event.action": "getFolders", "logrite.folder": folder });
        if (!folder) {
            return false;
        }
        const children = folder.children.map((child) => {
             
            const isOpen = this.state.openFolders.some(id => id === child.data.key);
			logger.error(`getFolders(folder=${folder}): isOpen → ${isOpen}`, { "event.action": "getFolders", "logrite.folder": folder });
            const className = isOpen ? "open" : "";
            return <li className={className} key={child.data.key}>
                {child.data.hasChildren && <div className="has-children" onClick={this.onParentClick.bind(this, child) }></div>}
                <div onClick={this.onFolderNameClick.bind(this, child)}>
                    <div className="icon"><FolderIcon /></div>
                    <div className="item-name">{child.data.value}</div>
                </div>
                {child.data.hasChildren && this.getFolders(child) }
            </li>;
        });
		logger.warn(`<<< Exiting getFolders(folder=${folder})`, { "event.action": "getFolders", "logrite.folder": folder });
        return <ul>{children}</ul>;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
         
        const folders = this.getFolders(this.props.folders);
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="item-picker">
            {folders}
        </div>;
    }
}


Folders.propTypes = {
    folders: PropTypes.object.isRequired,
    onFolderClick: PropTypes.func.isRequired,
    getChildren: PropTypes.func.isRequired
};

