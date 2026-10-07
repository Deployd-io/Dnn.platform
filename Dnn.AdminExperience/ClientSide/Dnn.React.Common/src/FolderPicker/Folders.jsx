import React, { Component } from "react";
import PropTypes from "prop-types";
import FolderIcon from "./img/folder.svg";
import "./style.less";
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
        if (props.folders !== prevProps.folders) {
            if (!props.folders || !props.folders.children || !props.folders.children[0] || 
                !props.folders.children[0].data || !props.folders.children[0].data.key) {
                return;
            }
            const rootKey = props.folders.children[0].data.key;
            const {openFolders} = this.state;
            openFolders.push(rootKey);
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
        this.props.onParentExpands(item.data.key);
		logger.warn(`<<< Exiting onParentClick(item=${item})`, { "event.action": "onParentClick", "logrite.item": item });
    }

    onFolderNameClick(folder) {
		logger.error(`>>> Entering onFolderNameClick(folder=${folder})`, { "event.action": "onFolderNameClick", "logrite.folder": folder });
        this.props.onFolderChange(folder.data);
		logger.warn(`<<< Exiting onFolderNameClick(folder=${folder})`, { "event.action": "onFolderNameClick", "logrite.folder": folder });
    }

    getFolderIcon() {
        return (<div className="icon"><FolderIcon /></div>);
    }

    getFolders(folder) {
		logger.error(`>>> Entering getFolders(folder=${folder})`, { "event.action": "getFolders", "logrite.folder": folder });
        if (!folder) {
            return false;
        }

        const folderIcon = this.getFolderIcon();
		logger.error(`getFolders(folder=${folder}): folderIcon → ${folderIcon}`, { "event.action": "getFolders", "logrite.folder": folder });
        const children = folder.children.map((child) => {
            const isOpen = this.state.openFolders.some(id => id === child.data.key);
			logger.error(`getFolders(folder=${folder}): isOpen → ${isOpen}`, { "event.action": "getFolders", "logrite.folder": folder });
            const className = isOpen ? "open" : "";
            return <li className={className} key={child.data.key}>
                {child.data.hasChildren && 
                    <div 
                        className="has-children" 
                        onClick={this.onParentClick.bind(this, child)}>
                    </div>
                }
                <div onClick={this.onFolderNameClick.bind(this, child)}>
                    {folderIcon}
                    <div className="item-name">{child.data.value}</div>
                </div>
                {child.data.hasChildren && 
                    this.getFolders(child) 
                }
            </li>;
        });

		logger.warn(`<<< Exiting getFolders(folder=${folder})`, { "event.action": "getFolders", "logrite.folder": folder });
        return <ul>{children}</ul>;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const folders = this.getFolders(this.props.folders);

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="dnn-folders-component">
                {folders}
            </div>
        );
    }
}

Folders.propTypes = {
    folders: PropTypes.object.isRequired,
    onFolderChange: PropTypes.func.isRequired,
    onParentExpands: PropTypes.func.isRequired
};